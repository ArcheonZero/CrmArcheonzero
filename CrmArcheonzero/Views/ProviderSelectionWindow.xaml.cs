using System.Windows;
using System.Windows.Controls;
using CrmArcheonzero.Data;

namespace CrmArcheonzero.Views
{
    public partial class ProviderSelectionWindow : Window
    {
        public ProviderSelectionWindow()
        {
            InitializeComponent();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = ProviderCombo.SelectedItem as ComboBoxItem;
            var provider = selectedItem?.Tag as string;

            if (string.IsNullOrEmpty(provider))
            {
                MessageBox.Show("Выберите базу данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Устанавливаем провайдер в фабрике
            DbContextFactory.SetProvider(provider, GetConnectionString(provider));

            DialogResult = true;
            Close();
        }

        private string GetConnectionString(string provider)
        {
            return provider.ToLower() switch
            {
                "postgresql" => "Host=aws-0-eu-west-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.qnlvugqiokfjcerpvobx;Password=qqRWeKgP6Aoibruz;SSL Mode=Require;Trust Server Certificate=true;",
                "sqlite" => "Data Source=crm.db",
                "sqlserver" => "Server=(localdb)\\mssqllocaldb;Database=CrmDb;Trusted_Connection=True;",
                _ => "Data Source=crm.db"
            };
        }
    }
}