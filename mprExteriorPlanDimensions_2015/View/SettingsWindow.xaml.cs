using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using mprExteriorPlanDimensions.Body;
using mprExteriorPlanDimensions.Configurations;
using ModPlusAPI;
using ModPlusAPI.Windows;
using ModPlusAPI.Windows.Helpers;
using MessageBox = System.Windows.MessageBox;

namespace mprExteriorPlanDimensions.View
{
    public partial class SettingsWindow 
    {
        public SettingsWindow()
        {
            InitializeComponent();
            this.OnWindowStartUp();
        }
        private ObservableCollection<ExteriorConfiguration> _exteriorConfigurations;
        private void SettingsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            GetExteriorConfigurations();
            // load settings
            var minWidthSetting = int.TryParse(UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings,
                "mprExteriorPlanDimensions",
                "ExteriorFaceMinWidthBetween"), out int m)
                ? m
                : 100;
            TbExteriorFaceMinWidthBetween.Text = minWidthSetting.ToString();
            CbExteriorMinWidthFaceRemove.SelectedIndex = int.TryParse(UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings,
                "mprExteriorPlanDimensions",
                "ExteriorMinWidthFaceRemove"), out m)
                ? m
                : 0;
            var minWallWidth = int.TryParse(UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings,
                "mprExteriorPlanDimensions",
                "MinWallWidth"), out m)
                ? m
                : 50;
            TbMinWallWidth.Text = minWallWidth.ToString();
        }
        // Загрузка Конфигураций для наружных стен из файла
        private void GetExteriorConfigurations()
        {
            try
            {
                var defConfig = Guid.TryParse(UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings,
                    "mprExteriorPlanDimensions",
                    "DefaultExteriorConfiguration"), out Guid g)
                    ? g
                    : Guid.Empty;
                if (defConfig == Guid.Empty)
                {
                    BtDeleteExteriorConfiguration.IsEnabled = false;
                    BtEditExteriorConfiguration.IsEnabled = false;
                    _exteriorConfigurations = new ObservableCollection<ExteriorConfiguration>();
                    return;
                }
                _exteriorConfigurations = SettingsFile.LoadExteriorConfigurations();
                CbExteriorConfigurations.ItemsSource = _exteriorConfigurations;
                var index = 0;
                for (var i = 0; i < _exteriorConfigurations.Count; i++)
                {
                    if (_exteriorConfigurations[i].Id.Equals(defConfig))
                    {
                        index = i;
                        break;
                    }
                }
                CbExteriorConfigurations.SelectedIndex = index;
                if (!_exteriorConfigurations.Any())
                {
                    BtDeleteExteriorConfiguration.IsEnabled = false;
                    BtEditExteriorConfiguration.IsEnabled = false;
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }
        private void CbExteriorConfigurations_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mprExteriorPlanDimensions",
                "DefaultExteriorConfiguration", ((ExteriorConfiguration) e.AddedItems[0]).Id.ToString(), true);
        }
        // Add new Exterior Configuration
        private void BtAddNewExteriorConfiguration_OnClick(object sender, RoutedEventArgs e)
        {
            Hide();
            try
            {
                ExteriorConfigurationWin win = new ExteriorConfigurationWin { TbWinTitle = { Text = "СОЗДАНИЕ КОНФИГУРАЦИИ ДЛЯ НАРУЖНЫХ РАЗМЕРОВ" } };
                var result = win.ShowDialog();
                if (result == true)
                {
                    _exteriorConfigurations.Add(win.CurrentExteriorConfiguration);
                    CbExteriorConfigurations.ItemsSource = _exteriorConfigurations;
                    CbExteriorConfigurations.SelectedIndex = CbExteriorConfigurations.Items.Count - 1;
                    SettingsFile.SaveExteriorConfigurations(_exteriorConfigurations);
                    BtDeleteExteriorConfiguration.IsEnabled = true;
                    BtEditExteriorConfiguration.IsEnabled = true;
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
            finally
            {
                ShowDialog();
            }
        }
        // Edit Exterior Configuration
        private void BtEditExteriorConfiguration_OnClick(object sender, RoutedEventArgs e)
        {
            Hide();
            try
            {
                if (CbExteriorConfigurations.SelectedIndex == -1) return;
                var selected = (ExteriorConfiguration)CbExteriorConfigurations.SelectedItem;
                var selectedIndex = CbExteriorConfigurations.SelectedIndex;

                ExteriorConfigurationWin win = new ExteriorConfigurationWin(selected) { TbWinTitle = { Text = "РЕДАКТИРОВАНИЕ КОНФИГУРАЦИИ ДЛЯ НАРУЖНЫХ РАЗМЕРОВ" } };
                var result = win.ShowDialog();
                if (result == true)
                {
                    _exteriorConfigurations.RemoveAt(selectedIndex);
                    _exteriorConfigurations.Insert(selectedIndex, win.CurrentExteriorConfiguration);
                    CbExteriorConfigurations.ItemsSource = _exteriorConfigurations;
                    CbExteriorConfigurations.SelectedIndex = selectedIndex;
                    SettingsFile.SaveExteriorConfigurations(_exteriorConfigurations);
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
            finally
            {
                ShowDialog();
            }
        }
        // delete configuration
        private void BtDeleteExteriorConfiguration_OnClick(object sender, RoutedEventArgs e)
        {
            if (CbExteriorConfigurations.SelectedIndex == -1) return;
            var selected = (ExteriorConfiguration)CbExteriorConfigurations.SelectedItem;
            var selectedIndex = CbExteriorConfigurations.SelectedIndex;
            if (MessageBox.Show(
                    "Конфигурация \"" + selected.Name + "\" будет удалена безвозратно!" + Environment.NewLine +
                    "Уверены?", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                _exteriorConfigurations.RemoveAt(selectedIndex);
                CbExteriorConfigurations.ItemsSource = _exteriorConfigurations;
                if (selectedIndex >= 1)
                    CbExteriorConfigurations.SelectedIndex = selectedIndex - 1;
                SettingsFile.SaveExteriorConfigurations(_exteriorConfigurations);
                if (!_exteriorConfigurations.Any())
                {
                    BtDeleteExteriorConfiguration.IsEnabled = false;
                    BtEditExteriorConfiguration.IsEnabled = false;
                }
            }
        }


        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        // only integers
        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;

            var newText = tb?.Text;
            if (string.IsNullOrEmpty(newText)) return;

            if (!int.TryParse(newText, out int _))
                newText = newText.Remove(newText.Length - 1);

            tb.Text = newText;
            tb.CaretIndex = newText.Length;
        }

        private void SettingsWindow_OnClosed(object sender, EventArgs e)
        {
            // save settings
            UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mprExteriorPlanDimensions", "ExteriorFaceMinWidthBetween",
                TbExteriorFaceMinWidthBetween.Text, false);
            UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mprExteriorPlanDimensions", "MinWallWidth",
                TbMinWallWidth.Text, false);
            UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mprExteriorPlanDimensions", "ExteriorMinWidthFaceRemove",
                CbExteriorMinWidthFaceRemove.SelectedIndex.ToString(), false);
            UserConfigFile.SaveConfigFile();
        }
    }
}
