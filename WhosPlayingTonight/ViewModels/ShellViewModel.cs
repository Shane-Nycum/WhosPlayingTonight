using Caliburn.Micro;
using WhosPlayingTonight.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Net;
using NAudio.Wave;

namespace WhosPlayingTonight.ViewModels
{
    /// <summary>
    /// ViewModel class. Inherits from PropertyChangedBase, 
    /// which provides a lambda-based NotifyOfPropertyChange method
    /// </summary>
    public class ShellViewModel : PropertyChangedBase
    {
        /// Backing field for CurrentlyPlaying property
        private Thread _currentlyPlaying;
        /// Backing field for Spotify property
        private Spotify _spotify = new Spotify();
        /// Backing field for Eventbrite property
        private Eventbrite _eventbrite = new Eventbrite();
        /// Backing field for EventsList property
        private BindableCollection<Event> _eventsList = new BindableCollection<Event>();
        /// Backing field for SelectedEvent property
        private Event _selectedEvent;
        /// Backing field for IsPlayingAudio property
        private bool _isPlayingAudio;

        /// <summary>
        /// Thread that plays the Spotify artist preview
        /// </summary>
        public Thread CurrentlyPlaying
        {
            get
            {
                return _currentlyPlaying;
            }
            set
            {
                _currentlyPlaying = value;
                NotifyOfPropertyChange(() => CurrentlyPlaying);
            }
        }
        /// <summary>
        /// Instance of Spotify class, which is used to get artist preview clips
        /// </summary>
        private Spotify Spotify
        {
            get
            {
                return _spotify;
            }
            set
            {
                _spotify = value;
                NotifyOfPropertyChange(() => Spotify);
            }
        }
        /// <summary>
        /// Instance of Eventbrite class, which is used to get the list of music events
        /// </summary>
        private Eventbrite Eventbrite
        {
            get
            {
                return _eventbrite;
            }
            set
            {
                _eventbrite = value;
                NotifyOfPropertyChange(() => Eventbrite);
            }
        }
        /// <summary>
        /// List of Events returned by the user's search to display on the UI
        /// </summary>
        public BindableCollection<Event> EventsList
        {
            get
            {
                return _eventsList;
            }
            set
            {
                _eventsList = value;
                NotifyOfPropertyChange(() => EventsList);
            }
        }
        /// <summary>
        /// The Event currently selected by the user on the UI
        /// </summary>
        public Event SelectedEvent
        {
            get
            {
                return _selectedEvent;
            }
            set
            {
                _selectedEvent = value;
                NotifyOfPropertyChange(() => SelectedEvent);
            }
        }
        /// <summary>
        /// True if audio is currently being played
        /// </summary>
        public bool IsPlayingAudio
        {
            get
            {
                return _isPlayingAudio;
            }
            set
            {
                _isPlayingAudio = value;
                NotifyOfPropertyChange(() => IsPlayingAudio);
            }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public ShellViewModel()
        {
            IsPlayingAudio = false;
        }
        /// <summary>
        /// Plays the audio clip at the given URL
        /// </summary>
        /// <param name="previewUrl"> URL of the mp3 file to play</param>
        public void PlayPreview(string previewUrl)
        {
            if (CurrentlyPlaying != null)
            {
                if (CurrentlyPlaying.IsAlive)
                {
                    StopPlayback();
                }
            }
            CurrentlyPlaying = StreamFromUrl(previewUrl);

        }

        /// <summary>
        /// Returns a Spotify URL at which an mp3 preview for the artist of the given Event is found
        /// </summary>
        /// <param name="evnt"> Event for which to get a Spotify preview URL </param>
        /// <returns> Spotify preview URL </returns>
        public async Task<string> GetSpotifyPreviewUrl(Event evnt)
        {
            string previewUrl = await Spotify.GetPreviewUrl(evnt.Name);
            return previewUrl;
        }

        /// <summary>
        /// Get the next page of events for the current search and appends it to EventsList
        /// </summary>
        /// <returns> Task which is getting the events page asynchronously </returns>
        public async Task GetNextEventsPage()
        {
            List<Event> nextEventsPage;
            try
            {
                nextEventsPage = await Eventbrite.GetNextEventsPage();
            }
            catch (Eventbrite.NoMoreEventsPagesException)
            {
                return;
            }

            EventsList.AddRange(nextEventsPage);
            NotifyOfPropertyChange(() => EventsList);
        }

        /// <summary>
        /// Clears EventsList and populates it with a new page of events for the given location / proximity
        /// </summary>
        /// <param name="location"> Location to search </param>
        /// <param name="proximity"> Proximity in miles to the given location to search </param>
        /// <returns> Task which is getting the events page asynchronously </returns>
        public async Task GetNewEventsPage(string location, int proximity = 25)
        {
            List<Event> eventsPage;
            try
            {
                eventsPage = await Eventbrite.GetNewEventsPage(location, proximity);
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("No results found for your search. Check your internet location and city name / zip code");
                return;
            }
            EventsList.Clear();
            EventsList.AddRange(eventsPage);
            NotifyOfPropertyChange(() => EventsList);
        }

        /// <summary>
        /// Stops playback of audio that is currently playing
        /// </summary>
        public void StopPlayback()
        {
            CurrentlyPlaying.Abort();
            IsPlayingAudio = false;
            NotifyOfPropertyChange(() => CurrentlyPlaying);
        }
        /// <summary>
        /// Streams an mp3 from the given URL in a new thread
        /// </summary>
        /// <param name="url"> URL where the mp3 file is located </param>
        /// <returns> Thread that's streaming the mp3 </returns>
        /// <remarks>
        /// Uses NAudio library. 
        /// Solution found at https://stackoverflow.com/questions/184683/play-audio-from-a-stream-using-c-sharp.
        /// </remarks>
        public Thread StreamFromUrl(string url)
        {
            IsPlayingAudio = true;
            var playerThread = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                try
                {
                    using (Stream ms = new MemoryStream())
                    {
                        using (Stream stream = WebRequest.Create(url)
                            .GetResponse().GetResponseStream())
                        {
                            byte[] buffer = new byte[32768];
                            int read;
                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                ms.Write(buffer, 0, read);
                            }
                        }

                        ms.Position = 0;
                        using (WaveStream blockAlignedStream =
                            new BlockAlignReductionStream(
                                WaveFormatConversionStream.CreatePcmStream(
                                    new Mp3FileReader(ms))))
                        {
                            using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                            {
                                waveOut.Init(blockAlignedStream);
                                waveOut.Play();
                                while (waveOut.PlaybackState == PlaybackState.Playing)
                                {
                                    System.Threading.Thread.Sleep(100);
                                }
                            }
                        }
                    }
                }
                catch { }
                IsPlayingAudio = false;

            });
            playerThread.Start();
            return playerThread;

        }


    }
}
