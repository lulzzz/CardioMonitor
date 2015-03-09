using System;
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

namespace CardioMonitor.Settings
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsView()
        {
            _viewModel = new SettingsViewModel();
            DataContext = _viewModel;

            InitializeComponent();
            _viewModel.SelectedAppTheme =
                _viewModel.AppThemes.FirstOrDefault(x => x.Name == Settings.Instance.SeletedAppThemeName);
            _viewModel.SelectedAccentColor =
                _viewModel.AccentColors.FirstOrDefault(x => x.Name == Settings.Instance.SelectedAcentColorName);
        }
    }
}
