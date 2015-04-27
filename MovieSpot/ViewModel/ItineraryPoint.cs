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
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.Devices.Geolocation;

namespace MovieSpot.ViewModel
{
    public class ItineraryPoint : INotifyPropertyChanged
    {
        private Geopoint itineraryGeopoint;
        public Geopoint ItineraryGeopoint
        {
            get
            {
                return itineraryGeopoint;
            }
            set
            {
                if (value != itineraryGeopoint)
                {
                    itineraryGeopoint = value;
                    NotifyPropertyChanged("ItineraryGeopoint");
                }
            }
        }

        private string formattedAddress;
        public string FormattedAddress
        {
            get
            {
                return formattedAddress;
            }
            set
            {
                if (value != formattedAddress)
                {
                    formattedAddress = value;
                    NotifyPropertyChanged("FormattedAddress");
                }
            }
        }

        private ObservableCollection<Movie> itineraryMoviesAtPoint;
        public ObservableCollection<Movie> ItineraryMoviesAtPoint
        {
            get
            {
                return itineraryMoviesAtPoint;
            }
            set
            {
                if (value != itineraryMoviesAtPoint)
                {
                    itineraryMoviesAtPoint = value;
                    NotifyPropertyChanged("ItineraryMoviesAtPoint");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
