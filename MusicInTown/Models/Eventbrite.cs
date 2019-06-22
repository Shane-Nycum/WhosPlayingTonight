using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MusicInTown.Models
{
    /// <summary>
    /// Provides functionality for getting a list of music-related
    /// events from Eventbrite's API
    /// </summary>
    public class Eventbrite
    {
        const int categoryIDMusic = 103;
        string oAuthToken = "SS7XVNOG3R7FR6BW7OP3";
        int nextPage = 1;
        int pageCount;

        /// <summary>
        /// Returns the next page of events in the given geographic area, starting with page 1.
        /// The list is sorted starting with the soonest event, going forward in time.
        /// </summary>
        /// <param name="location"> City name or zip code of the location to search </param>
        /// <param name="proximity"> Proximity to the given location to search </param>
        /// <returns> List of Events</returns>
        public async Task<List<Event>> GetNextEventsList(string location, int proximity = 25)
        {
            // Test to see if all pages have been retrieved
            if (pageCount != 0 && nextPage > pageCount)
            {
                throw new NoMoreEventsPagesException();
            }

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
            nextPage++;

            // return the List of Events
            return ConvertToEventList(responseBodyDeserialized);
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

                    EventsList.Add(new Event
                    {
                        Name = item.name.text,
                        Description = item.description.text,
                        StartTime = item.start.local,
                        Venue = item.venue.name

                    });
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
