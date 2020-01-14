namespace mprExteriorPlanDimensions.Body
{
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Configurations;

    public static class SettingsFile
    {
        private static string _settingsFile;

        /// <summary>Инициализация файла настроек - создание нового в случае отсутствия</summary>
        public static void InitSettingsFile()
        {
            var configDirectory = Path.Combine(ModPlusAPI.Constants.UserDataDirectory, "DimConfigurations");
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
            var configurations = new ObservableCollection<ExteriorConfiguration>();
            if (string.IsNullOrEmpty(_settingsFile))
                InitSettingsFile();
            var settingsFile = XElement.Load(_settingsFile);
            var configurationsXElement = settingsFile.Element(Constants.XElementName_ExteriorConfigurations);
            if (configurationsXElement != null)
            {
                if (configurationsXElement.Elements(Constants.XElementName_ExteriorConfiguration).Any())
                {
                    foreach (var xElement in configurationsXElement.Elements(Constants.XElementName_ExteriorConfiguration))
                        configurations.Add(ExteriorConfiguration.GetExteriorConfigurationFromXElement(xElement));
                }
            }

            return configurations;
        }

        /// <summary>Сохранить список Конфигураций наружных размеров в файл (всегда происходит перезапись)</summary>
        /// <param name="configurations"></param>
        public static void SaveExteriorConfigurations(ObservableCollection<ExteriorConfiguration> configurations)
        {
            if (string.IsNullOrEmpty(_settingsFile))
                InitSettingsFile();
            var settingsFile = XElement.Load(_settingsFile);

            var exteriorConfigurationsXElement =
                settingsFile.Element(Constants.XElementName_ExteriorConfigurations);
            exteriorConfigurationsXElement?.Remove();
            exteriorConfigurationsXElement = new XElement(Constants.XElementName_ExteriorConfigurations);

            foreach (var configuration in configurations)
                exteriorConfigurationsXElement.Add(configuration.GetXElementFromExteriorConfigurationInstance());

            settingsFile.Add(exteriorConfigurationsXElement);

            settingsFile.Save(_settingsFile);
        }

        #endregion
    }
}
