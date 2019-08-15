using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WhosPlayingTonight.Models;
using WhosPlayingTonight.ViewModels;

namespace WhosPlayingTonight.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ShellView()
        {
            
            InitializeComponent();
            videoMediaTimeline.Source = new Uri(System.IO.Directory.GetCurrentDirectory().ToString() + @"\Media\concert_video.mp4");
            videoMediaElement.Play();
            PopulateSearchRangeComboBox();

        }

        /// <summary>
        /// Gets the next page of Events when the user is scrolled to the bottom of the Events list
        /// </summary>
        private async void EventsListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var listBox = (ListBox)sender;
            var scrollViewer = GetDescendantByType(listBox, typeof(ScrollViewer)) as ScrollViewer;
            if (listBox.Items.Count > 0 && scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                // Get the viewmodel instance
                var viewModelInstance = DataContext as ShellViewModel;
                await viewModelInstance.GetNextEventsPage();
            }
            
        }
        /// <summary>
        /// Returns descendant of the given Visual.
        /// Used in this program to access the ScrollViewer property of a ListBox.
        /// </summary>
        /// <remarks> Solution found at https://stackoverflow.com/questions/10293236/accessing-the-scrollviewer-of-a-listbox-from-c-sharp </remarks>
        private Visual GetDescendantByType(Visual element, Type type)
        {
            if (element == null)
            {
                return null;
            }
            if (element.GetType() == type)
            {
                return element;
            }
            Visual foundElement = null;
            if (element is FrameworkElement)
            {
                (element as FrameworkElement).ApplyTemplate();
            }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType(visual, type);
                if (foundElement != null)
                {
                    break;
                }
            }
            return foundElement;
        }
        /// <summary>
        /// Populates the SearchRange combobox with values available to select.
        /// </summary>
        private void PopulateSearchRangeComboBox()
        {
            for (int i = 5; i <= 100; i += 5)
            {
                SearchRange.Items.Add(i);
            }
            int defaultSearchRangeSelection = 4;
            SearchRange.SelectedIndex = defaultSearchRangeSelection;
           
        }
        /// <summary>
        /// Searches Eventbrite
        /// </summary>
        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            var scrollViewer = GetDescendantByType(EventsListBox, typeof(ScrollViewer)) as ScrollViewer;
            scrollViewer.ScrollToTop();
            var viewModelInstance = DataContext as ShellViewModel;
            await viewModelInstance.GetNewEventsPage(Location.Text, (int)SearchRange.SelectedValue);
            if (viewModelInstance.EventsList.Count > 0)
            {
                StopPlayback.Visibility = Visibility.Visible;
                PlaySpotifyPreview.Visibility = Visibility.Visible;
            } else
            {
                StopPlayback.Visibility = Visibility.Hidden;
                PlaySpotifyPreview.Visibility = Visibility.Hidden;
            }

        }
        /// <summary>
        /// Plays Spotify preview for the selected artist
        /// </summary>
        private void PlaySpotifyPreview_Click(object sender, RoutedEventArgs e)
        {
            Event selectedEvent = (Event)EventsListBox.SelectedItem;
            var viewModelInstance = DataContext as ShellViewModel;
            viewModelInstance.PlayPreview(selectedEvent.PreviewUrl);
        }
        /// <summary>
        /// Enables play / stop buttons if a Spotify preview is found for the selected artist
        /// Makes TextBlock visible that displays an "unavailable" message if the Spotify preview is not found
        /// </summary>
        private async void EventsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EventsListBox.SelectedItem != null)
            {
                var viewModelInstance = DataContext as ShellViewModel;
                Event selectedEvent = (Event)EventsListBox.SelectedItem;
                string previewUrl = await viewModelInstance.GetSpotifyPreviewUrl(selectedEvent);
                selectedEvent.PreviewUrl = previewUrl;
                if (selectedEvent.PreviewUrl != null && selectedEvent.PreviewUrl != "" && 
                    selectedEvent.PreviewUrl != "(Spotify preview not available)")
                {
                    PlaySpotifyPreview.IsEnabled = true;
                    SpotifyPreviewAvailable.Visibility = Visibility.Hidden;
                } else
                {
                    PlaySpotifyPreview.IsEnabled = false;
                    SpotifyPreviewAvailable.Visibility = Visibility.Visible;
                }
            } else
            {
                PlaySpotifyPreview.IsEnabled = false;
                SpotifyPreviewAvailable.Visibility = Visibility.Visible;
            }
            
        }
    }
}
