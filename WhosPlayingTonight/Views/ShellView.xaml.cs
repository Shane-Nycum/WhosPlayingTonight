using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WhosPlayingTonight.Models;
using WhosPlayingTonight.ViewModels;

namespace WhosPlayingTonight.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        public ShellView()
        {
            
            InitializeComponent();
            videoMediaTimeline.Source = new Uri(System.IO.Directory.GetCurrentDirectory().ToString() + @"\Media\concert_video.mp4");
            videoMediaElement.Play();
            populateSearchRangeComboBox();

        }

        public async void EventsListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
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

        public static Visual GetDescendantByType(Visual element, Type type)
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

        private void populateSearchRangeComboBox()
        {
            for (int i = 5; i <= 100; i += 5)
            {
                SearchRange.Items.Add(i);
            }
            int defaultSearchRangeSelection = 4;
            SearchRange.SelectedIndex = defaultSearchRangeSelection;
           
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            var scrollViewer = GetDescendantByType(EventsListBox, typeof(ScrollViewer)) as ScrollViewer;
            scrollViewer.ScrollToTop();
            var viewModelInstance = DataContext as ShellViewModel;
            await viewModelInstance.GetNewEventsPage(Location.Text, (int)SearchRange.SelectedValue);
        }

        private void PlaySpotifyPreview_Click(object sender, RoutedEventArgs e)
        {
            Event selectedEvent = (Event)EventsListBox.SelectedItem;
            var viewModelInstance = DataContext as ShellViewModel;
            viewModelInstance.PlayPreview(selectedEvent.PreviewUrl);
        }

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
                    PlaySpotifyPreview.Visibility = Visibility.Visible;
                } else
                {
                    PlaySpotifyPreview.Visibility = Visibility.Hidden;
                }
            } else
            {
                PlaySpotifyPreview.Visibility = Visibility.Hidden;
            }
            
        }
    }
}
