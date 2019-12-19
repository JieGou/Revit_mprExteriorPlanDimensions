namespace mprExteriorPlanDimensions.View
{
    using System.Windows;
    using System.Windows.Input;
    using Configurations;

    public partial class ExteriorConfigurationWin
    {
        private const string LangItem = "mprExteriorPlanDimensions";
        public ExteriorConfiguration CurrentExteriorConfiguration;

        public ExteriorConfigurationWin(ExteriorConfiguration exteriorConfiguration = null)
        {
            InitializeComponent();
            CurrentExteriorConfiguration = exteriorConfiguration ?? new ExteriorConfiguration();
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
                ModPlusAPI.Windows.MessageBox.Show(ModPlusAPI.Language.GetItem(LangItem, "msg1"));
                return;
            }

            if (!CurrentExteriorConfiguration.BottomDimensions &&
                !CurrentExteriorConfiguration.LeftDimensions &&
                !CurrentExteriorConfiguration.RightDimensions &&
                !CurrentExteriorConfiguration.TopDimensions)
            {
                ModPlusAPI.Windows.MessageBox.Show(ModPlusAPI.Language.GetItem(LangItem, "msg2"));
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
            if (selectedChainIndex != -1)
                CurrentExteriorConfiguration.Chains.RemoveAt(selectedChainIndex);
        }

        private void ExteriorConfigurationWin_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
