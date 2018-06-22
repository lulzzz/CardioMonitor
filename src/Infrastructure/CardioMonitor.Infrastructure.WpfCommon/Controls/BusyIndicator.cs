using System.Windows;
using System.Windows.Controls;

namespace CardioMonitor.Infrastructure.WpfCommon.Controls
{
    public class BusyIndicator : ContentControl
    {

        static BusyIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(BusyIndicator), 
                new FrameworkPropertyMetadata("BusyIndicatorStylekey"));

        }

        /// <summary>
        /// Identifies the IsBusy dependency property.
        /// </summary>
        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
            nameof(IsBusy),
            typeof(bool),
            typeof(BusyIndicator),
            new PropertyMetadata(false, OnIsBusyChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the busy indicator should show.
        /// </summary>
        public bool IsBusy
        {
            get => (bool)GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }

        /// <summary>
        /// IsBusyProperty property changed handler.
        /// </summary>
        /// <param name="d">BusyIndicator that changed its IsBusy.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnIsBusyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BusyIndicator)d).OnIsBusyChanged(e);
        }

        /// <summary>
        /// IsBusyProperty property changed handler.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnIsBusyChanged(DependencyPropertyChangedEventArgs e)
        {/*
            var isBusy = (bool)e.NewValue;
            ProgressGrid.Visibility = isBusy
                ? Visibility.Visible
                : Visibility.Collapsed;
            Ring.IsActive = isBusy;*/
        }


        /// <summary>
        /// Identifies the IsBusy dependency property.
        /// </summary>
        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            nameof(Status),
            typeof(string),
            typeof(BusyIndicator),
            new PropertyMetadata(null, OnStatusChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the busy indicator should show.
        /// </summary>
        public string Status
        {
            get => (string)GetValue(StatusProperty);
            set => SetValue(ContentProperty, value);
        }

        /// <summary>
        /// IsBusyProperty property changed handler.
        /// </summary>
        /// <param name="d">BusyIndicator that changed its IsBusy.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BusyIndicator)d).OnStatusChanged(e);
        }

        /// <summary>
        /// IsBusyProperty property changed handler.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnStatusChanged(DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
