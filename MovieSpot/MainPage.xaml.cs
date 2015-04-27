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


using MovieSpot.UserControls;
using MovieSpot.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;

namespace MovieSpot
{
    public sealed partial class MainPage : Page
    {
        #region private variables     
        RandomAccessStreamReference movieIconStreamReference, differentMovieSameLocationIconStreamReference;
        List<RandomAccessStreamReference> numberIconRefrences;
        double previousZoomLevel = 1;                
        ClusteringExtension.ClusterGenerator clusterGenerator;                
        bool isViewRouteEnabled = false;       
        bool LocationAccessDenied = false;
        Geopoint sfcenterPoint = new Geopoint(new BasicGeoposition
        {
            // Center point of San Francisco
            Latitude = 37.783333,
            Longitude = -122.416667,
            Altitude = 1,

        }, AltitudeReferenceSystem.Surface);
        #endregion
        public MainPage()
        {
            this.InitializeComponent();
            MapService.ServiceToken = "TODO: INSERT TOKEN";            
            this.movieIconStreamReference = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/MovieSpotIcon.png"));
            this.differentMovieSameLocationIconStreamReference = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/DifferentMovieSameLocationIcon.png"));
            this.InitalizeNumberIconReferences();
            this.Loaded += mainPage_Loaded;
        }
        private async void movieMap_Loaded(object sender, RoutedEventArgs e)
        {
            //movieMap.Center = sfcenterPoint;
            //movieMap.ZoomLevel = 12;
            await updateViewAsync(sfcenterPoint, 12, 45, 60);            
        }                 
        private async void mainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await MovieManager.GetInstance.GetMoviesDataAsync();
            movieMap.MapElements.Clear();            
            itineraryListView.ItemsSource = MovieManager.GetInstance.SelectedItinerary;            
            //loadAllMapIcons();

            #region DEMO Make Icons Better 
            
            var converter = new Converter.ItemConverter();
            clusterGenerator = new ClusteringExtension.ClusterGenerator(converter, 20);
            clusterGenerator.GenerateClusteringData(MovieManager.GetInstance.AllMovies);
            refreshMapIcons();
            
            #endregion
        }
        private void movieMap_ZoomLevelChanged(MapControl sender, object args)
        {
            if (previousZoomLevel != Math.Floor(movieMap.ZoomLevel) && !isViewRouteEnabled)
            {
                previousZoomLevel = Math.Floor(movieMap.ZoomLevel);                          
                refreshMapIcons();                
            }
        }

        #region DEMO 1 Load MapIcons
        private void loadAllMapIcons()
        {
            foreach (Movie movie in MovieManager.GetInstance.AllMovies)
            {
                MapIcon movieSpotIcon = new MapIcon();
                movieSpotIcon.Title = movie.Title;
                movieSpotIcon.Location = new Geopoint(new BasicGeoposition
                {
                    Latitude = movie.Latitude,
                    Longitude = movie.Longitude
                });
                movieSpotIcon.NormalizedAnchorPoint = new Point(1, 1);
                movieSpotIcon.CollisionBehaviorDesired = 
                    MapElementCollisionBehavior.RemainVisible;
                movieSpotIcon.Image = movieIconStreamReference;
                movieMap.MapElements.Add(movieSpotIcon);
            }
        }
        #endregion

        #region DEMO 2 XAML info Pane
        private async void movieMap_MapElementClick(MapControl sender, 
            MapElementClickEventArgs args)
        {
            MapIcon topMost = args.MapElements[0] as MapIcon;

            if (topMost.Title == "Lots of Movie Spots")
            {
                if (movieMap.ZoomLevel < 16)
                {
                    //await movieMap.TrySetViewAsync(args.Location, 16);
                     await movieMap.TrySetSceneAsync(
                    MapScene.CreateFromLocationAndRadius(args.Location, 500, 45, 50));
                }
                else
                {
                    await movieMap.TrySetViewAsync(args.Location, movieMap.ZoomLevel + 2);
                }
            }
            else
            {

                MovieInfoPanel infoPanel = new MovieInfoPanel
                {
                    ClickedLocation = topMost.Location
                };
                infoPanel.Closed += infoPanel_Closed;
                movieMap.Children.Add(infoPanel);

                MapControl.SetLocation(infoPanel, topMost.Location);
            }
        }
        private void infoPanel_Closed(object sender, EventArgs args)
        {
            movieMap.Children.Remove(sender as MovieInfoPanel);
        }
        #endregion

        #region DEMO 3 Make Icons Better
        private void refreshMapIcons()
        {
            if ((clusterGenerator == null) || (movieMap.ZoomLevel > 20)) return;
            movieMap.MapElements.Clear();
            foreach (var cluster in clusterGenerator.GetClustersByZoomLevel((int)movieMap.ZoomLevel))
            {
                if (cluster.Count > 1)
                {
                    if (movieMap.ZoomLevel >= 15)
                    {
                        #region ZoomedIn
                        IEnumerable<Movie> movies;
                        movies = MovieManager.GetInstance.GetMovies(cluster.Location.Latitude, cluster.Location.Longitude, 0.0001);
                        if (movies != null && movies.Count() > 1)
                        {
                            MapIcon differentMovieShotHereMapIcon = new MapIcon
                            {
                                Title = movies.Count().ToString() + " movies shot here",
                                NormalizedAnchorPoint = new Point(0.5, 0.5),
                                Location = new Geopoint(cluster.Location, AltitudeReferenceSystem.Surface),
                                CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible,
                                Image = differentMovieSameLocationIconStreamReference,
                            };
                            movieMap.MapElements.Add(differentMovieShotHereMapIcon);
                        }
                        else
                        {
                            MapIcon groupMapIcon = new MapIcon
                            {
                                Title = "Lots of Movie Spots",
                                NormalizedAnchorPoint = new Point(0.5, 0.5),
                                Location = new Geopoint(cluster.Location, AltitudeReferenceSystem.Surface),
                                CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible,                                
                            };

                            if (cluster.Count >= 9)
                                groupMapIcon.Image = numberIconRefrences.Last();
                            else
                                groupMapIcon.Image = numberIconRefrences[cluster.Count - 2];
                            movieMap.MapElements.Add(groupMapIcon);

                        }
                        #endregion
                    }
                    else
                    {
                        MapIcon groupMapIcon = new MapIcon
                        {
                            Title = "Lots of Movie Spots",
                            NormalizedAnchorPoint = new Point(0.5, 0.5),
                            Location = new Geopoint(cluster.Location, AltitudeReferenceSystem.Surface),
                            CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible,                            
                        };

                        if (cluster.Count >= 9)
                            groupMapIcon.Image = numberIconRefrences.Last();
                        else
                            groupMapIcon.Image = numberIconRefrences[cluster.Count - 2];
                        movieMap.MapElements.Add(groupMapIcon);
                    }
                }
                else
                {
                    Movie movie = (Movie)cluster.Objects.First();
                    MapIcon movieSpotIcon = new MapIcon();
                    movieSpotIcon.Title = movie.Title;
                    movieSpotIcon.Location = new Geopoint(new BasicGeoposition
                    {
                        Latitude = movie.Latitude,
                        Longitude = movie.Longitude,
                        Altitude = 0
                    }, AltitudeReferenceSystem.Surface);
                    movieSpotIcon.NormalizedAnchorPoint = new Point(1, 1);
                    movieSpotIcon.CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible;
                    movieSpotIcon.Image = movieIconStreamReference;
                    movieMap.MapElements.Add(movieSpotIcon);
                }
            }
        }
        #endregion        

        #region DEMO 4 Show Route on map, geolocator and get directions
        private async void showMyRoute_Click(object sender, RoutedEventArgs e)
        {
            if (MovieManager.GetInstance.SelectedItinerary == null || MovieManager.GetInstance.SelectedItinerary.Count <= 0) return;

            var wayPoints = from ItineraryPoint point in MovieManager.GetInstance.SelectedItinerary select point.ItineraryGeopoint;

            MapRouteFinderResult routeResult =
                await MapRouteFinder.GetDrivingRouteFromWaypointsAsync(wayPoints, MapRouteOptimization.Distance, MapRouteRestrictions.None);

            //Easier way to add route - 
            //movieMap.Routes.Add(new MapRouteView(routeResult.Route));

            if (routeResult.Status == MapRouteFinderStatus.Success)
            {
                movieMap.MapElements.Clear();
                isViewRouteEnabled = true;

                MapPolyline routeline = new MapPolyline
                {
                    Path = routeResult.Route.Path,
                    StrokeColor = Colors.Blue,
                    StrokeThickness = 3,
                };
                movieMap.MapElements.Add(routeline);

                foreach(ItineraryPoint point in MovieManager.GetInstance.SelectedItinerary)
                {                    
                    MapIcon movieSpotIcon = new MapIcon();
                    movieSpotIcon.Location = point.ItineraryGeopoint;
                    movieSpotIcon.NormalizedAnchorPoint = new Point(1, 1);
                    movieSpotIcon.CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible;
                    movieSpotIcon.Image = movieIconStreamReference;
                    movieMap.MapElements.Add(movieSpotIcon);
                }

                await updateViewAsync(sfcenterPoint, 12, 45, 60);
            }
        }
        private async void giveMeDirections_Click(object sender, RoutedEventArgs e)
        {
            GeolocationAccessStatus locationAccessStatus = await Geolocator.RequestAccessAsync();           

            if (locationAccessStatus == GeolocationAccessStatus.Allowed)
            {
                Geolocator geolocator = new Geolocator();               
                geolocator.DesiredAccuracy = PositionAccuracy.High;
                Geoposition userPosition = await geolocator.GetGeopositionAsync();

                string formattedString = String.Format("bingmaps:?rtp=pos.{0}_{1}~adr.555%20North%20Point%20St,%20San%20Francisco,%20CA%2094133;mode=d&amp;trfc=1",
                    userPosition.Coordinate.Point.Position.Latitude, userPosition.Coordinate.Point.Position.Longitude);

                await Launcher.LaunchUriAsync(new Uri(formattedString));
            }
            else
            {
                if (!LocationAccessDenied)
                {
                    ContentDialog cd = new ContentDialog
                    {
                        Title = "Oops, looks like you forgot to give us permission to your location",
                        Content = @"To help us service you better we would like to request your location access. We respect user's privacy and your data is safe with us
                    Could you revist settings page and grant us permissions please? 
                    ",
                        PrimaryButtonText = "Take me to settings",
                        SecondaryButtonText = "No Access for you!"
                    };
                    cd.PrimaryButtonClick += cd_PrimaryButtonClick;
                    cd.SecondaryButtonClick += cd_SecondaryButtonClick;

                    await cd.ShowAsync();
                }
                else
                    return;
            }
        }
        private void cd_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //user has denied you location access again, be a good citizen and stop bothering. 
            LocationAccessDenied = true;
        }
        private async void cd_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings://privacy/location"));
        }
        #endregion

        #region ASB filtering, Reset and other non demo code
        private void InitalizeNumberIconReferences()
        {
            this.numberIconRefrences = new List<RandomAccessStreamReference>();
            for (int i = 2; i <= 9; i++)
            {
                this.numberIconRefrences.Add(RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Numbers/" + i.ToString() + ".png")));
            }
        }
        private async void searchForMovieBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (string.IsNullOrEmpty(sender.Text as string))
                {
                    await resetToStartViewAsync();
                }
                else
                {
                    var matchingMovies = MovieManager.GetInstance.GetMovieTitles(sender.Text);
                    sender.ItemsSource = matchingMovies.ToList();
                }
            }
        }
        private async void searchForMovieBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                //User selected an item, take an action on it here
                await filterIconsOnMap(args.ChosenSuggestion as string);
            }
            else
            {
                await resetToStartViewAsync();
            }
        }
        private async void searchForMovieBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            string selectedString = (args.SelectedItem as string);
            await filterIconsOnMap(selectedString);
        }

        private async Task filterIconsOnMap(string selectedString)
        {
            movieMap.MapElements.Clear();

            var asbSelectedMovies = MovieManager.GetInstance.GetMovies(selectedString);

            var converter = new Converter.ItemConverter();
            clusterGenerator = new ClusteringExtension.ClusterGenerator(converter, 20);
            clusterGenerator.GenerateClusteringData(asbSelectedMovies);

            await updateViewAsync(sfcenterPoint, 14, 45, 60);

            refreshMapIcons();
        }

        private async void resetButton_Click(object sender, RoutedEventArgs e)
        {
            await resetToStartViewAsync();
        }

        private async Task resetToStartViewAsync()
        {
            var converter = new Converter.ItemConverter();
            isViewRouteEnabled = false;
            movieMap.Routes.Clear();
            movieMap.Children.Clear();
            movieMap.MapElements.Clear();
            MovieManager.GetInstance.SelectedItinerary.Clear();
            clusterGenerator = new ClusteringExtension.ClusterGenerator(converter, 20);
            clusterGenerator.GenerateClusteringData(MovieManager.GetInstance.AllMovies);
            await updateViewAsync(sfcenterPoint, 12, 25, 60);
            refreshMapIcons();
        }

        private async Task updateViewAsync(Geopoint point, double zoom, double headingIndegrees, double pitchindegrees)
        {
            double radius = (2200 - (110 * zoom));
            //await movieMap.TrySetViewAsync(point, zoom, 0, 0, MapAnimationKind.Linear);
            await movieMap.TrySetSceneAsync(MapScene.CreateFromLocationAndRadius(point, radius, headingIndegrees, pitchindegrees));
        }

        private void togglePaneButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width >= 640)
            {
                if (splitView.IsPaneOpen)
                {
                    splitView.DisplayMode = SplitViewDisplayMode.CompactOverlay;
                    splitView.IsPaneOpen = false;

                }
                else
                {
                    splitView.IsPaneOpen = true;
                    splitView.DisplayMode = SplitViewDisplayMode.Inline;
                }
            }
            else
            {
                splitView.IsPaneOpen = !splitView.IsPaneOpen;
            }


        }
        #endregion
    }
}
