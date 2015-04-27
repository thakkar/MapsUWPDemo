//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using Windows.Devices.Geolocation;

using ClusteringExtension.DataModel;

namespace ClusteringExtension
{
    public enum BoxLengthType 
    {
        Pixels,
        Distance
    }

    public sealed class ClusterGenerator
    {
        private Dictionary<Object, BasicGeoposition> cachedConvertedObjects;
        private Dictionary<string, IList<Object>> keyToItems;
        private List<ClusterList> zoomLevelToClusters;
        private ClusterList validItemsToCluster;
        private int numZoomLevels;
        private IGpsValueConverter gpsConverter;


        public double MinHitBoxSizeInMiles { get; set; } // This determines what the smallest bounding box size is (in miles)

        // These seem like pretty terrible names, played around with AllItemPerClusterZoomLevel
        // This tells you the maximum zoom level where zooming out (zoomlevel--) makes no difference in clusters 
        public int LeastClustersZoomLevel { get; private set; } 
        // The opposite of the above; this is the zoom level which has the most clusters (ie: all clusters are single items
        public int MostClustersZoomLevel { get; private set; } 
        public ClusterGenerator(IGpsValueConverter converter, int NumZoomLevels, double lengthValue, BoxLengthType hitBoxType)
        {
            this.cachedConvertedObjects = new Dictionary<Object, BasicGeoposition>();
            this.keyToItems = new Dictionary<string, IList<Object>>();
            this.zoomLevelToClusters = new List<ClusterList>();
            this.validItemsToCluster = new ClusterList();
            this.gpsConverter = converter;
            this.numZoomLevels = NumZoomLevels;

            if (hitBoxType == BoxLengthType.Distance)
            {
                this.MinHitBoxSizeInMiles = lengthValue;
            }
            else if (hitBoxType == BoxLengthType.Pixels)
            {
                this.MinHitBoxSizeInMiles = GeospatialHelperStatic.ConvertPixelsToMiles(lengthValue, NumZoomLevels);
            }

            this.LeastClustersZoomLevel = 1;
            this.MostClustersZoomLevel = 1;
            
        }

        public ClusterGenerator(IGpsValueConverter converter, int NumZoomLevels)
        {
            this.cachedConvertedObjects = new Dictionary<Object, BasicGeoposition>();
            this.keyToItems = new Dictionary<string, IList<Object>>();
            this.zoomLevelToClusters = new List<ClusterList>();
            this.validItemsToCluster = new ClusterList();
            this.gpsConverter = converter;
            this.numZoomLevels = NumZoomLevels;

            // This is a 100 pixels for the minimum hitbox length
            this.MinHitBoxSizeInMiles = GeospatialHelperStatic.ConvertPixelsToMiles(100, NumZoomLevels);            

            this.LeastClustersZoomLevel = 1;
            this.MostClustersZoomLevel = 1;

        }

        // Perf? Do it by pictures first and then by zoom level?
        // The assumption is that the d
        public void GenerateClusteringData(IEnumerable items)
        {
            this.cachedConvertedObjects.Clear();
            this.keyToItems.Clear();
            this.zoomLevelToClusters.Clear();

            this.LeastClustersZoomLevel = 1;
            this.MostClustersZoomLevel = 1;


            CreateValidItemsAsClusters(items);
                        
            // 1 is space view
            // 20 is ground view            
            for (int i = 1; i <= this.numZoomLevels; i++)
            {
                // List of Clusters for this zoom level
                ClusterList clusters = new ClusterList();
                foreach (var item in items)
                {
                    BasicGeoposition itemCoord = ConvertObjToGPS(item);

                    if (!GeospatialHelperStatic.IsValidGPS(itemCoord))
                    {
                        continue;
                    }

                    bool addedToCluster = false;

                    for (int j = 0; j < clusters.Count; j++)
                    {
                        if (IsWithinBoundary(itemCoord, clusters[j].Location, i))
                        {
                            clusters[j].Count += 1;                            
                            clusters[j].Objects.Add(item);

                            string id = String.Format("ZL{0}_C{1}", i, j);
                           this.keyToItems[id].Add(item);

                            addedToCluster = true;
                            break;
                        }
                    }
                    if (addedToCluster == false)
                    {
                        string id = String.Format("ZL{0}_C{1}", i, clusters.Count);
                        List<Object> singleItem = new List<Object>() { item };

                        clusters.Add(new Cluster()
                        {
                            ClusterId = id,
                            Objects = singleItem,
                            Count = 1,
                            Location = itemCoord
                        });                       
                        List<Object> otherSingleItem = new List<Object>() { item };
                        this.keyToItems.Add(id, otherSingleItem);
                    }
                }
                if (this.zoomLevelToClusters.Count > 1)
                {                    
                    int minClusters = this.zoomLevelToClusters[this.LeastClustersZoomLevel-1].Count;
                    int maxClusters = this.zoomLevelToClusters[this.MostClustersZoomLevel-1].Count;

                    // eg: 1 1 1 3 5 5 7 7 7, this will do the 1s. See definition for what these two are
                    if (clusters.Count <= minClusters) 
                    {
                        this.LeastClustersZoomLevel = i;
                    }
                    if (clusters.Count > maxClusters) 
                    {
                        this.MostClustersZoomLevel = i;
                    }
                }
                
                this.zoomLevelToClusters.Add(clusters);
            }
            RecalculateCenters();
        }


        private void CreateValidItemsAsClusters(IEnumerable items)
        {                
            foreach (var item in items)
            {
                BasicGeoposition itemCoord = ConvertObjToGPS(item);

                if (!GeospatialHelperStatic.IsValidGPS(itemCoord))
                {
                    continue;
                }

                string id = String.Format("C{0}", this.validItemsToCluster.Count);
                List<Object> singleItem = new List<Object>() { item };

                this.validItemsToCluster.Add(new Cluster()
                {
                    ClusterId = id,
                    Objects = singleItem,
                    Count = 1,
                    Location = itemCoord
                });
                List<Object> otherSingleItem = new List<Object>() { item };
                this.keyToItems.Add(id, otherSingleItem);
            }
        }

        private void RecalculateCenters()
        {
            for (int i = 0; i < this.zoomLevelToClusters.Count; i++)
            {
                foreach (var cluster in this.zoomLevelToClusters[i])
                {
                    cluster.Location = CalculateCenter(cluster.Objects);
                }
            }
        }

        // Used to calculate the center so that its "more correct"
        private BasicGeoposition CalculateCenter(IEnumerable itemsInCluster)
        {
            BasicGeoposition center = new BasicGeoposition();
            center.Latitude = 0.0;
            center.Longitude = 0.0;
            int count = 0;
            foreach (var item in itemsInCluster)
            {
                BasicGeoposition itemCoord = ConvertObjToGPS(item);

                center.Latitude += itemCoord.Latitude;
                center.Longitude += itemCoord.Longitude;
                count++;
            }

            center.Latitude = center.Latitude / count;
            center.Longitude = center.Longitude / count;

            return center;
        }

        public void AddItemToClusters(Object item)
        {
            if (!GeospatialHelperStatic.IsValidGPS(ConvertObjToGPS(item)))
            {
                return;
            }

            for (int i = 1; i <= this.numZoomLevels; i++)
            {
                // List of Clusters for this zoom level
                ClusterList clusters = this.zoomLevelToClusters[i - 1];
                bool addedToCluster = false;
                BasicGeoposition itemCoord = ConvertObjToGPS(item);

                for (int j = 0; j < clusters.Count; j++)
                {
                    if (IsWithinBoundary(itemCoord, clusters[j].Location, i))
                    {
                        clusters[j].Count += 1;
                        clusters[j].Objects.Add(item);

                        string id = String.Format("ZL{0}_C{1}", i, j);
                        this.keyToItems[id].Add(item);

                        addedToCluster = true;
                        break;
                    }
                }
                if (addedToCluster == false)
                {
                    string id = String.Format("ZL{0}_C{1}", i, clusters.Count);
                    List<Object> singleItem = new List<Object>() { item };
                    clusters.Add(new Cluster()
                    {
                        ClusterId = id,
                        Objects = singleItem,
                        Count = 1,
                        Location = itemCoord
                    });
                    this.keyToItems.Add(id, singleItem);
                }
                this.zoomLevelToClusters[i - 1] = clusters;
            }
        }

        // Removes item from the clusters, also will need to delete clusters if they are empty
        public void RemoveItemFromClusters(Object item)
        {
            if (!GeospatialHelperStatic.IsValidGPS(ConvertObjToGPS(item)))
            {
                return;
            }

            for (int i = 1; i <= this.numZoomLevels; i++)
            {
                // List of Clusters for this zoom level
                ClusterList clusters = this.zoomLevelToClusters[i-1];
                for (int j = 0; j < clusters.Count; j++)
                {
                    for (int k = 0; k < clusters[j].Objects.Count; k++)
                    {
                        if (item == clusters[j].Objects[k])
                        {
                            // Remove the entire cluster if there is only one element
                            if (clusters[j].Objects.Count == 1) 
                            {
                                clusters.RemoveAt(j);
                            } 
                            // Remove just that object from the list
                            else 
                            {
                                clusters[j].Objects.RemoveAt(k);
                            }

                            // only one object removed per zoom level and since we can't do multi-loop breaks
                            // have this assignment to break out of the 2nd loops
                            j = clusters.Count;
                            break;
                        }
                    }
                }
            }
        }

        // function that determines if one item is within the shape (in this case, square). 
        // The size of the square is determined by the zoom level
        private bool IsWithinBoundary(BasicGeoposition pointToTest, BasicGeoposition centerOfShape, int zoomLevel)
        {
            // Hitbox for this zoom level
            double distanceToDiff = this.MinHitBoxSizeInMiles * Math.Pow(2, this.numZoomLevels - zoomLevel);
            // 1 degree = 69.11 miles; we need to convert the hitbox(miles) to degrees
            // The / 2 is because we want the radius
            double latDiffRadius =  distanceToDiff / 69.11 / 2;
            double longDiffRadius = distanceToDiff / 69.11 * Math.Cos(centerOfShape.Latitude * Math.PI / 180) / 2;

            
            if (IsWithinLat(pointToTest.Latitude, centerOfShape.Latitude, latDiffRadius) &&
                IsWithinLong(pointToTest.Longitude, centerOfShape.Longitude, longDiffRadius))
            {
                return true;
            }

            return false;
        }

        // Checks to make sure a point is with the latitude range
        private bool IsWithinLat(double latToTest, double latCenter, double distance)
        {
            double min = Math.Max(-90.0, latCenter - distance);
            double max = Math.Min(90.0, latCenter + distance);

            return (latToTest >= min && latToTest <= max);
        }

        // Checks to make sure a point is with the longitude range
        private bool IsWithinLong(double longToTest, double longCenter, double distance)
        {
            // Need to check this + 360 for the overlap since -180 can go to 180;
            double anotherLongToTest = longToTest + 360.0;

            double min = Math.Max(-180.0, longCenter - distance);
            double max = Math.Min(540.0, longCenter + distance);

            return ((longToTest >= min && longToTest <= max) || (anotherLongToTest >= min && anotherLongToTest <= max));
        }

        public ClusterList GetClustersByZoomLevel(int zoomLevel)
        {
            if (!IsValidZoomLevel(zoomLevel))
            {
                throw new NotSupportedException();
            }

            return this.zoomLevelToClusters[zoomLevel - 1];
        }

        public ClusterList GetValidItemsAsClusters()
        {
            return this.validItemsToCluster;
        }

        // Returns a list of objects by clusterid
        public IList<Object> GetItemsById(string clusterId)
        {
            if (!this.keyToItems.ContainsKey(clusterId))
            {
                throw new KeyNotFoundException();
            }

            return this.keyToItems[clusterId];
        }

        // Uses the converter to convert the object to a geoposition
        private BasicGeoposition ConvertObjToGPS(Object item)
        {
            if (!this.cachedConvertedObjects.ContainsKey(item))
            {
                BasicGeoposition location = (BasicGeoposition)this.gpsConverter.Convert(item);
                this.cachedConvertedObjects.Add(item, location);   
            }
            return this.cachedConvertedObjects[item];
        }

        // Verifies if its a valid zoom level
        private bool IsValidZoomLevel(int zoomLevel)
        {
            return (zoomLevel > 0 & zoomLevel <= this.numZoomLevels);
        }
    }
}
