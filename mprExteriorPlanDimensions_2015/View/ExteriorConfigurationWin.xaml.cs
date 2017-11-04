using System.Windows;
using System.Windows.Input;
using mprExteriorPlanDimensions.Configurations;
using ModPlusAPI.Windows.Helpers;
using MessageBox = ModPlusAPI.Windows.MessageBox;

namespace mprExteriorPlanDimensions.View
{
    public partial class ExteriorConfigurationWin
    {
        public ExteriorConfiguration CurrentExteriorConfiguration;
        public ExteriorConfigurationWin(ExteriorConfiguration exteriorConfiguration = null)
        {
            InitializeComponent();
            this.OnWindowStartUp();
            CurrentExteriorConfiguration = exteriorConfiguration ?? new ExteriorConfiguration();
        }

        private void ExteriorConfiguration_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ExteriorConfiguration_OnLoaded(object sender, RoutedEventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
            DataContext = CurrentExteriorConfiguration;
        }

        private void BtAccept_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentExteriorConfiguration.Name))
            {
                MessageBox.Show("Нужно указать название конфигурации!");
                return;
            }
            if (!CurrentExteriorConfiguration.BottomDimensions &&
                !CurrentExteriorConfiguration.LeftDimensions &&
                !CurrentExteriorConfiguration.RightDimensions &&
                !CurrentExteriorConfiguration.TopDimensions)
            {
                MessageBox.Show("Нужно указать хотя бы одну сторону установки размеров!");
                return;
            }
            
            DialogResult = true;
        }

        private void BtAddRow_OnClick(object sender, RoutedEventArgs e)
        {
            CurrentExteriorConfiguration.Chains.Add(new ExteriorDimensionChain());
        }

        private void BtDeleteSelectedChain_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedChainIndex = DgChains.SelectedIndex;
            if(selectedChainIndex != -1) CurrentExteriorConfiguration.Chains.RemoveAt(selectedChainIndex);
        }

        private void ExteriorConfigurationWin_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }
    }
}
