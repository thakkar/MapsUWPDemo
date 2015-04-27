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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace MovieSpot.ViewModel
{
    public class MovieManager 
    {
        public ObservableCollection<Movie> AllMovies
        {
            get; set;
        }        
        public ObservableCollection<ItineraryPoint> SelectedItinerary
        {
            get; set;
        }

        private static MovieManager instance;         
           
        private MovieManager() {
            AllMovies = new ObservableCollection<Movie>();
            SelectedItinerary = new ObservableCollection<ItineraryPoint>();           
        }
        
        public static MovieManager GetInstance
        {
            get
            {
                if(instance==null)
                {
                    instance = new MovieManager();                    
                }
                return instance;
            }
        }        
     
        public async Task GetMoviesDataAsync()
        {
            if (AllMovies.Count != 0) return;
            System.Diagnostics.Debug.WriteLine("getting data");
            Uri dataUri = new Uri("ms-appx:///DataSource/moviespots.json");

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            string jsonText = await FileIO.ReadTextAsync(file);
            JsonObject jsonObject = JsonObject.Parse(jsonText);
            JsonArray jsonArray = jsonObject["MovieSpots"].GetArray();            
            
            foreach (JsonValue value in jsonArray)
            {
                JsonObject movieJsonObject = value.GetObject();

                Movie movie = new Movie
                {
                    Title = movieJsonObject["Title"].GetString(),
                    ReleaseYear = movieJsonObject["Release Year"].GetString(),
                    StreetName = movieJsonObject["Locations"].GetString(),
                    FunFacts = movieJsonObject["Fun Facts"].GetString(),
                    ProductionCompany = movieJsonObject["Production Company"].GetString(),
                    Distributor = movieJsonObject["Distributor"].GetString(),
                    Director = movieJsonObject["Director"].GetString(),
                    Writer = movieJsonObject["Writer"].GetString(),
                    Actor1 = movieJsonObject["Actor 1"].GetString(),
                    Actor2 = movieJsonObject["Actor 2"].GetString(),
                    Actor = movieJsonObject["Actor "].GetString(),
                    Latitude = movieJsonObject["Latitude"].GetNumber(),
                    Longitude = movieJsonObject["Longitude"].GetNumber()
                };

                AllMovies.Add(movie);
            }
        }

        public ObservableCollection<Movie> GetMoviesByYear(string year)
        {
            return new ObservableCollection<Movie>(from movie in AllMovies where movie.ReleaseYear == year select movie);
         }

        public IEnumerable<Movie> GetMovies(string Title, double latitude, double longitude)
        {
            var matchingMovies = from movie in AllMovies
                   where (movie.Latitude == latitude && movie.Longitude==longitude && movie.Title== Title) 
                   select movie;

            return matchingMovies;
        }

        public IEnumerable<Movie> GetMovies(double latitude, double longitude)
        {
            var matchingMovies = from movie in AllMovies
                                 where (movie.Latitude == latitude && movie.Longitude == longitude)
                                 select movie;

            return matchingMovies;
        }

        public IEnumerable<Movie> GetMovies(double latitude, double longitude, double around)
        {
            var matchingMovies = from movie in AllMovies
                                 where (Math.Abs(movie.Latitude-latitude)<around && Math.Abs(movie.Longitude-longitude)<around)
                                 select movie;

            return matchingMovies;
        }

        public IEnumerable<Movie> GetMovies(string year, double latitude, double longitude, double around)
        {         
            var matchingMovies = from movie in AllMovies
                                 where (Math.Abs(movie.Latitude - latitude) < around && Math.Abs(movie.Longitude - longitude) < around && movie.ReleaseYear ==year)
                                 select movie;

            return matchingMovies;
        }

        public IEnumerable<string> GetMovieTitles(string query)
        {
            return AllMovies
                .Where(c => c.Title.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > -1)
                .OrderByDescending(c => c.Title.StartsWith(query, StringComparison.CurrentCultureIgnoreCase))
                .Select(c=> c.Title).Distinct();
        }

        public IEnumerable<Movie> GetMovies(string query)
        {
            return AllMovies
                .Where(c => c.Title.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > -1)
                .OrderByDescending(c => c.Title.StartsWith(query, StringComparison.CurrentCultureIgnoreCase)). Distinct();
        }
    }
}
