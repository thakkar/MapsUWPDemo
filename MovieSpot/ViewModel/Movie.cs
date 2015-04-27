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
using System.ComponentModel;

namespace MovieSpot.ViewModel
{
    public class Movie : INotifyPropertyChanged
    {
     
        private string title;
        public string Title
        {
            get
            {
                return title;
            }
            set {
                if (value != title)
                {
                    title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }

        private string releaseYear;
        public string ReleaseYear
        {
            get
            {
                return releaseYear;
            }
            set
            {
                if (value != releaseYear)
                {
                    releaseYear = value;
                    NotifyPropertyChanged("ReleaseYear");
                }
            }
        }

        private string streetName;
        public string StreetName
        {
            get
            {
                return streetName;
            }
            set
            {
                if (value != streetName)
                {
                    streetName = value;
                    NotifyPropertyChanged("StreetName");
                }
            }
        }

        private string funFacts;
        public string FunFacts
        {
            get
            {
                return funFacts;
            }
            set
            {
                if (value != funFacts)
                {
                    funFacts = value;
                    NotifyPropertyChanged("Funfacts");
                }
            }
        }

        private string productionCompany;
        public string ProductionCompany
        {
            get
            {
                return productionCompany;
            }
            set
            {
                if (value != productionCompany)
                {
                    productionCompany = value;
                    NotifyPropertyChanged("ProductionCompany");
                }
            }
        }

        private string distributor;
        public string Distributor
        {
            get
            {
                return distributor;
            }
            set
            {
                if (value != distributor)
                {
                    distributor = value;
                    NotifyPropertyChanged("Distributor");
                }
            }
        }

        private string director;
        public string Director
        {
            get
            {
                return director;
            }
            set
            {
                if (value != director)
                {
                    director = value;
                    NotifyPropertyChanged("Director");
                }
            }
        }                

        private string writer;
        public string Writer
        {
            get
            {
                return writer;
            }
            set
            {
                if (value != writer)
                {
                    writer = value;
                    NotifyPropertyChanged("Writer");
                }
            }
        }

        private string actor1;
        public string Actor1
        {
            get
            {
                return actor1;
            }
            set
            {
                if (value != actor1)
                {
                    actor1 = value;
                    NotifyPropertyChanged("Actor1");
                }
            }
        }

        private string actor2;
        public string Actor2
        {
            get
            {
                return actor2;
            }
            set
            {
                if (value != actor2)
                {
                    actor2 = value;
                    NotifyPropertyChanged("Actor2");
                }
            }
        }

        private string actor;
        public string Actor
        {
            get
            {
                return actor;
            }
            set
            {
                if (value != actor)
                {
                    actor = value;
                    NotifyPropertyChanged("Actor");
                }
            }
        }        

        private double latitude;
        public double Latitude
        {
            get
            {
                return latitude;
            }
            set
            {
                if (value != latitude)
                {
                    latitude = value;
                    NotifyPropertyChanged("Latitude");
                }
            }
        }        

        private double longitude;
        public double Longitude
        {
            get
            {
                return longitude;
            }
            set
            {
                if (value != longitude)
                {
                    longitude = value;
                    NotifyPropertyChanged("Longitude");
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
