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
using Windows.Devices.Geolocation;

namespace ClusteringExtension
{
    public interface IGpsValueConverter
    {
        BasicGeoposition Convert(object valueToConvert);
        object ConvertBack(BasicGeoposition gpsLocation);
    }

    public sealed class GeospatialHelperStatic
    {
        // This is how much distance 1 pixel is at the highest zoom level
        // Taken from here: http://msdn.microsoft.com/en-us/library/bb259689.aspx
        private static readonly double HighestZoomLevel_GroundResolutionInMeters = 78271.5170;
        private static readonly double MilesToMetersFactor = 1609.344;

        public static double ConvertMetersToMiles(double meters)
        {
            return (meters / MilesToMetersFactor);
        }

        public static double ConvertMilesToMeters(double miles)
        {
            return (miles * MilesToMetersFactor);
        }

        public static double ConvertPixelsToMiles(double pixels, int zoomLevel)
        {
            return (pixels * (HighestZoomLevel_GroundResolutionInMeters / Math.Pow(2, zoomLevel - 1))) / MilesToMetersFactor;
        }

        public static double ConvertMilesToPixels(double miles, int zoomLevel)
        {
            return (miles * MilesToMetersFactor) / (HighestZoomLevel_GroundResolutionInMeters / Math.Pow(2, zoomLevel - 1)); 
        }


        public static bool IsValidGPS(BasicGeoposition position)
        {
            return ((Math.Abs(position.Latitude) <= 90.0) && (Math.Abs(position.Longitude) <= 180.0));
        }

        // Takes in two gps coordinates and returns their lat/long/avg lat long diff. This is useful for calculating hitboxes in clustering
        // TODO: Add a geobounding box function call that will be the geobounding box
        public static double CalculateLengthByBoxInMiles(BasicGeoposition initialCorner, BasicGeoposition oppositeCorner, int currZoomLevel, int maxZoomLevel, CalculationType mode)
        {
            double latDiff = Math.Abs(initialCorner.Latitude - oppositeCorner.Latitude);
            double longDiff = Math.Abs(initialCorner.Longitude - oppositeCorner.Longitude);
            double latAvg = (initialCorner.Latitude + oppositeCorner.Latitude) / 2;

            // 1 degree lat = 69.11 miles
            double latLength = latDiff * 69.11;
            double longLength = longDiff * 69.11 / Math.Cos(latAvg * Math.PI / 180);

            // Have to take account for the zoom level
            double zoomLevelMultiplier = Math.Pow(2, maxZoomLevel - currZoomLevel);

            double sideLength = 0.0;

            // mode: 0 = min, 1 = max, 2 = average
            if (mode == CalculationType.Min)
            {
                sideLength = Math.Min(latLength, longLength) / zoomLevelMultiplier;
            }
            else if (mode == CalculationType.Max)
            {
                sideLength = Math.Max(latLength, longLength) / zoomLevelMultiplier;
            }
            else if (mode == CalculationType.Average)
            {
                sideLength = ((latLength + longLength) / 2) / zoomLevelMultiplier;
            }
            
            return sideLength;
        }
    }

    public enum CalculationType
    {
        Min,
        Max,
        Average
    }
}
