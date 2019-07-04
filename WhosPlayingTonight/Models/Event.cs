using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPlayingTonight.Models
{
    /// <summary>
    /// Class that holds information on an event
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Name of the event
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description of the event
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Venue where the event is taking place
        /// </summary>
        public string Venue { get; set; }
        /// <summary>
        /// Start time of the event
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// Url where a clip of the artist's music can be found
        /// </summary>
        public string PreviewUrl { get; set; }

    }
}
