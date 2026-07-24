using System.Windows.Controls;

namespace CrmArcheonzero.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            DataContext = new ViewModels.SettingsViewModel();
        }
    }
}