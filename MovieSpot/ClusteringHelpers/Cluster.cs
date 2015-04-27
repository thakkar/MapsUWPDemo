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
using System.Collections.Generic;
using Windows.Devices.Geolocation;

namespace ClusteringExtension.DataModel
{
    public sealed class Cluster
    {
        public Cluster()
        {
            this.Objects = new List<Object>();
        }

        public Cluster(BasicGeoposition location)
        {
            this.Location = location;
        }

        // ID of the Cluster
        public string ClusterId { get; set; }

        // GPS location
        public BasicGeoposition Location { get; set; }

        // List of ISV's Objects
        public IList<Object> Objects { get; set; }

        // # of ISV's Objects
        public int Count { get; set; }
    }
}
