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


using MovieSpot.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;

namespace MovieSpot.UserControls
{
    public sealed partial class MovieInfoPanel : UserControl
    {
        private ItineraryPoint currentClickedPoint;
        public MovieInfoPanel()
        {
            this.InitializeComponent();
            currentClickedPoint = new ItineraryPoint();           
        }

        private void movieInfoPanel_Loaded(object sender, RoutedEventArgs e)
        {
            initializeControlChildren();
        }        
        private void initializeControlChildren()
        {
            if (MovieManager.GetInstance.SelectedItinerary.Contains(currentClickedPoint))
            {
                addedToTourText.Opacity = 1;
                addToTour.Opacity = 0;
                removeFromTourButton.Opacity = 0;
            }

            moviesShotHere.ItemsSource = currentClickedPoint.ItineraryMoviesAtPoint;
            if (currentClickedPoint.ItineraryMoviesAtPoint.Count() > 1)
            {
                movieCountTextBlock.Opacity = 1;
                movieCountTextBlock.Text = "Total movies shot here: " + currentClickedPoint.ItineraryMoviesAtPoint.Count();
            }

             loadStreetsidePanorama(currentClickedPoint.ItineraryGeopoint);
        }        
        
        private async void loadStreetsidePanorama(Geopoint location)
        {
            StreetsidePanorama movieLocationPanorama = await StreetsidePanorama.FindNearbyAsync(location);

            if (movieLocationPanorama == null)
            {
                streetSideMap.Visibility = Visibility.Collapsed;
                return;
            }

            streetSideMap.Visibility = Visibility.Visible;

            StreetsideExperience movieLocationStreetside = new StreetsideExperience(movieLocationPanorama)
            {
                ExitButtonVisible = false,
                ZoomButtonsVisible = false,
                OverviewMapVisible = false,
            };

            streetSideMap.CustomExperience = movieLocationStreetside;
        }    
           
        private async void addToTour_Click(object sender, RoutedEventArgs e)
        {
            currentClickedPoint.FormattedAddress = await reverseGeocodeAsync(currentClickedPoint.ItineraryGeopoint);

            MovieManager.GetInstance.SelectedItinerary.Add(currentClickedPoint);
            addToTour.Opacity = 0;
            removeFromTourButton.Opacity = 1;
            addedToTourText.Opacity = 1;
        }       
        private async Task<string> reverseGeocodeAsync(Geopoint pointToReverseGeocode)
        {
            // Reverse geocode the specified geographic location.
            MapLocationFinderResult result =
                await MapLocationFinder.FindLocationsAtAsync(pointToReverseGeocode);

            // If the query returns results, display the name of the town
            // contained in the address of the first result.
            if (result.Status == MapLocationFinderStatus.Success)
            {
                MapAddress address = result.Locations.First().Address;
                return address.FormattedAddress;
            }
            else
                return null;
        }
    
        #region non demo code
        public delegate void InfoPanelClosedHandler(object sender, EventArgs args);
        public event InfoPanelClosedHandler Closed;
        public Geopoint ClickedLocation
        {
            set
            {
                currentClickedPoint.ItineraryGeopoint = value;
                currentClickedPoint.ItineraryMoviesAtPoint = new ObservableCollection<Movie>(MovieManager.GetInstance.GetMovies(currentClickedPoint.ItineraryGeopoint.Position.Latitude,
                    currentClickedPoint.ItineraryGeopoint.Position.Longitude, 0.0001));                
            }
        }
        private void closeInfoButton_Click(object sender, RoutedEventArgs e)
        {
            Closed(this, new EventArgs());
        }

        private void removeFromTourButton_Click(object sender, RoutedEventArgs e)
        {
            MovieManager.GetInstance.SelectedItinerary.Remove(currentClickedPoint);
            addToTour.Opacity = 1;
            removeFromTourButton.Opacity = 0;
            addedToTourText.Opacity = 0;
        }

        #endregion

    }
}
