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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Markeli.Storyboards;

namespace CardioMonitor.Ui.View.Devices
{
    /// <summary>
    /// Interaction logic for DeviceConfigCreationView.xaml
    /// </summary>
    public partial class DeviceConfigCreationView : IStoryboardPageView
    {
        public DeviceConfigCreationView()
        {
            InitializeComponent();
        }

        public IStoryboardPageViewModel ViewModel
        {
            get => DataContext as IStoryboardPageViewModel;
            set => DataContext = value;
        }
    }
}
