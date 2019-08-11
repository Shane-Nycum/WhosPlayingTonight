using Caliburn.Micro;
using WhosPlayingTonight.Models;
using System;
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
        private Thread _currentlyPlaying;
        private Spotify _spotify = new Spotify();
        private Eventbrite _eventbrite = new Eventbrite();
        private BindableCollection<Event> _eventsList = new BindableCollection<Event>();
        private Event _selectedEvent;
        private bool _isPlayingAudio;

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

        public ShellViewModel()
        {
            IsPlayingAudio = false;
        }
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

        public async Task<string> GetSpotifyPreviewUrl(Event evnt)
        {
            string previewUrl = await Spotify.GetPreviewUrl(evnt.Name);
            return previewUrl;
        }

        public async Task GetNextEventsPage()
        {
            List<Event> nextEventsPage;
            try
            {
                nextEventsPage = await Eventbrite.GetNextEventsPage();
            }
            catch
            {
                return;
            }

            EventsList.AddRange(nextEventsPage);
            NotifyOfPropertyChange(() => EventsList);
        }

        public async Task GetNewEventsPage(string location, int proximity = 25)
        {
            List<Event> eventsPage;
            try
            {
                eventsPage = await Eventbrite.GetNewEventsPage(location, proximity);
            }
            catch
            {
                return;
            }
            EventsList.Clear();
            EventsList.AddRange(eventsPage);
            NotifyOfPropertyChange(() => EventsList);
        }

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
