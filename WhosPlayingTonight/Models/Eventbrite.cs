using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Configuration;

namespace WhosPlayingTonight.Models
{
    /// <summary>
    /// Provides functionality for getting a list of music-related
    /// events from Eventbrite's API
    /// </summary>
    public class Eventbrite
    {
        /// Eventbrite category ID for music events 
        private const int categoryIDMusic = 103;
        /// OAuth Token for Eventbrite API. Stored in an XML file outside source control
        private readonly string oAuthToken = ConfigurationManager.AppSettings["eventbriteSecret"];
        /// The next events page to get from Eventbrite API
        private int nextPage = 1;
        /// Total number of event pages available for the current search
        private int pageCount;
        /// Location for the current search
        private string currentLocation;
        /// Proximity for the current search
        private int currentProximity;

        /// <summary>
        /// Queries Eventbrite API for music events in a given proximity of the given location.
        /// Returns the first page of results.
        /// </summary>
        /// <param name="location"> Location to search </param>
        /// <param name="proximity"> Proximity in miles to the given location to search </param>
        /// <returns> First page of Eventbrite music events </returns>
        public async Task<List<Event>> GetNewEventsPage (string location, int proximity = 25)
        {
            nextPage = 1;
            currentLocation = location;
            currentProximity = proximity;
            var eventsList = await GetEventsList(location, proximity);
            nextPage++;
            return eventsList;
        }

        /// <summary>
        /// Returns the next page of events (which is tracked by nextPage) for the given location / proximity
        /// </summary>
        /// <param name="location"> Location to search </param>
        /// <param name="proximity">Proximity in miles to the given location to search </param>
        /// <returns> List of Events </returns>
        private async Task<List<Event>> GetEventsList (string location, int proximity)
        {
            // Make the api request
            string urlString = $"https://www.eventbriteapi.com/v3/events/search/?categories={categoryIDMusic}" +
                $"&location.address={location}&location.within={proximity}mi&expand=venue&sort_by=date&page={nextPage}";
            string responseBodyJson;
            using (WebClient client = new WebClient())
            {
                client.Headers.Add($"Authorization: Bearer {oAuthToken}");
                responseBodyJson = await client.DownloadStringTaskAsync(urlString);
            }


            // Parse the response body with the list of events, putting them into a List of Events
            dynamic responseBodyDeserialized = JsonConvert.DeserializeObject(responseBodyJson);

            // set the page count equal to the page count returned by Eventbrite for this search
            // increment the nextPage counter
            pageCount = responseBodyDeserialized.pagination.page_count;
            return ConvertToEventList(responseBodyDeserialized);
        }


        /// <summary>
        /// Returns the next page of events in the given geographic area
        /// The list is sorted starting with the soonest event, going forward in time.
        /// </summary>
        /// <param name="location"> City name or zip code of the location to search </param>
        /// <param name="proximity"> Proximity to the given location to search </param>
        /// <returns> List of Events</returns>
        public async Task<List<Event>> GetNextEventsPage()
        {
            // Test to see if all pages have been retrieved
            if (pageCount != 0 && nextPage > pageCount)
            {
                throw new NoMoreEventsPagesException();
            }
            var eventsList = await GetEventsList(currentLocation, currentProximity);
            nextPage++;
            return eventsList;
        }



        /// <summary>
        /// Converts the deserialized response body into a List of type Event
        /// </summary>
        /// <param name="responseBodyDeserialized"> Deserialized response body containing the list of events</param>
        /// <returns> List of type Event </returns>
        private List<Event> ConvertToEventList(dynamic responseBodyDeserialized)
        {
            List<Event> EventsList = new List<Event>();
            foreach (dynamic item in responseBodyDeserialized.events)
            {
                try
                {
                    if (item.name.text != null) {
                        Event newEvent = new Event() { Name = item.name.text };
                        // Add description
                        if (item.description.text != null)
                        {
                            newEvent.Description = item.description.text;
                        } else
                        {
                            newEvent.Description = "(event description unavailable)";
                        }
                        // Add start time
                        if (item.start.local != null)
                        {
                            newEvent.StartTime = item.start.local;
                        } else
                        {
                            newEvent.Description = "(event date/time information unavailable)";
                        }
                        // Add venue
                        if (item.venue.name != null)
                        {
                            if (item.venue.address.city != null && item.venue.address.region != null)
                            {
                                newEvent.Venue = item.venue.name + " - " + item.venue.address.city + ", " + item.venue.address.region;
                            } else
                            {
                                newEvent.Venue = item.venue.name;
                            }

                        } else
                        {
                            newEvent.Venue = "(event venue information unavailable)";
                        }

                        EventsList.Add(newEvent);
                    }
                }
                catch (Exception) { }
            }
            return EventsList;
        }

        /// <summary>
        /// Custom exception type for when there are no more events pages to query
        /// </summary>
        public class NoMoreEventsPagesException : Exception
        {
            /// <summary>
            /// Constructor for when no param is given
            /// </summary>
            public NoMoreEventsPagesException() : base("No more events pages to query")
            {
            }

            /// <summary>
            /// Constructor for when a param with a message is given
            /// </summary>
            /// <param name="message">
            /// Message to include with the exception
            /// </param>
            public NoMoreEventsPagesException(string message)
                : base(message)
            {
            }
        }

    }
}
