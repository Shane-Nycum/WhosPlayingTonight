﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

        }

        private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

        }
    }
}
