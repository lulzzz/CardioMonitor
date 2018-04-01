﻿using System.Windows.Controls;
using CardioMonitor.Ui.ViewModel.Sessions;

namespace CardioMonitor.Ui.View.Sessions
{
    /// <summary>
    /// Interaction logic for SessionsView.xaml
    /// </summary>
    public partial class SessionsView : UserControl
    {
        private SessionsViewModel _viewModel;

        public SessionsView()
        {
            DataContext = _viewModel;

            InitializeComponent();
        }
    }
}
