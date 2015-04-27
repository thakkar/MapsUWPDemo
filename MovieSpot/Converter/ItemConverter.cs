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
using MovieSpot.ViewModel;

namespace MovieSpot.Converter
{
    public class ItemConverter : ClusteringExtension.IGpsValueConverter
    {
        public BasicGeoposition Convert(object valueToConvert)
        {
            var item = (Movie)valueToConvert;
            if (item == null)
            {
                throw new NotSupportedException();
            }

            return new BasicGeoposition { Latitude = item.Latitude, Longitude = item.Longitude, Altitude = 0 };
        }

        public object ConvertBack(BasicGeoposition gpsLocation)
        {
            throw new NotImplementedException();
        }
    }
}
