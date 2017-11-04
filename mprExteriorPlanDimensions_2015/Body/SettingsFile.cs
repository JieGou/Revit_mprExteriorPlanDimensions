using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using mprExteriorPlanDimensions.Configurations;

namespace mprExteriorPlanDimensions.Body
{
    public static class SettingsFile
    {
        private static string _settingsFile;
        /// <summary>Инициализация файлна настроек - создание нового в случае отсутсвия</summary>
        public static void InitSettingsFile()
        {
            var configDirectory = Path.Combine(ModPlusAPI.Constants.CurrentDirectory, "UserData", "DimConfigurations");
            if (!Directory.Exists(configDirectory))
                Directory.CreateDirectory(configDirectory);
            var file = Path.Combine(configDirectory, "ExteriorPlanDimensions.xml");
            if (!File.Exists(file))
            {
                var xElement = new XElement(Constants.XElementName_Root);
                // save
                xElement.Save(file);
            }
            _settingsFile = file;
        }

        #region Configurations
        /// <summary>Загрузка из файла настроек Конфигураций для наружных размеров</summary>
        /// <returns></returns>
        public static ObservableCollection<ExteriorConfiguration> LoadExteriorConfigurations()
        {
            ObservableCollection<ExteriorConfiguration> configurations = new ObservableCollection<ExteriorConfiguration>();
            if (string.IsNullOrEmpty(_settingsFile)) InitSettingsFile();
            XElement settingsFile = XElement.Load(_settingsFile);
            var configurationsXElement = settingsFile.Element(Constants.XElementName_ExteriorConfigurations);
            if (configurationsXElement != null)
                if (configurationsXElement.Elements(Constants.XElementName_ExteriorConfiguration).Any())
                {
                    foreach (var xElement in configurationsXElement.Elements(Constants.XElementName_ExteriorConfiguration))
                        configurations.Add(ExteriorConfiguration.GetExteriorConfigurationFromXElement(xElement));
                }
            return configurations;
        }
        /// <summary>Сохранить список Конфигураций наружных размеров в файл (всегда происходит перезапись)</summary>
        /// <param name="configurations"></param>
        public static void SaveExteriorConfigurations(ObservableCollection<ExteriorConfiguration> configurations)
        {
            if (string.IsNullOrEmpty(_settingsFile)) InitSettingsFile();
            XElement settingsFile = XElement.Load(_settingsFile);

            XElement exteriorConfigurationsXElement =
                settingsFile.Element(Constants.XElementName_ExteriorConfigurations);
            exteriorConfigurationsXElement?.Remove();
            exteriorConfigurationsXElement = new XElement(Constants.XElementName_ExteriorConfigurations);

            foreach (ExteriorConfiguration configuration in configurations)
                exteriorConfigurationsXElement.Add(configuration.GetXElementFromExteriorConfigurationInstance());

            settingsFile.Add(exteriorConfigurationsXElement);

            settingsFile.Save(_settingsFile);
        }

        #endregion
    }
}
