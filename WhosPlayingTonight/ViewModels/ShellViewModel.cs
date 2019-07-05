using Caliburn.Micro;
using WhosPlayingTonight.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WhosPlayingTonight.ViewModels
{
    public class ShellViewModel : Screen
    {
        private Thread _currentlyPlaying { get; set; }
        private Spotify _thisSpotify = new Spotify();
        private Eventbrite _thisEventbrite = new Eventbrite();
        private BindableCollection<Event> _eventsList = new BindableCollection<Event>();
        private Event _selectedEvent;

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
        private Spotify ThisSpotify
        {
            get
            {
                return _thisSpotify;
            }
            set
            {
                _thisSpotify = value;
                NotifyOfPropertyChange(() => ThisSpotify);
            }
        }
        private Eventbrite ThisEventbrite
        {
            get
            {
                return _thisEventbrite;
            }
            set
            {
                _thisEventbrite = value;
                NotifyOfPropertyChange(() => ThisEventbrite);
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


        public async Task PlayClip(string artistName)
        {
            try
            {
                string previewUrl = await ThisSpotify.GetPreviewUrl(artistName);
                if (CurrentlyPlaying != null)
                {
                    if (CurrentlyPlaying.IsAlive)
                    {
                        StopPlayback();
                    }
                }
                CurrentlyPlaying = ThisSpotify.StreamFromUrl(previewUrl);
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        public async Task GetNextEventsPage()
        {
            List<Event> nextEventsPage;
            try
            {
                nextEventsPage = await ThisEventbrite.GetNextEventsPage();
            } catch
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
                eventsPage = await ThisEventbrite.GetNewEventsPage(location, proximity);
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
            NotifyOfPropertyChange(() => CurrentlyPlaying);
        }

    }
}
