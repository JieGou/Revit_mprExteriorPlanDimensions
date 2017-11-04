using System;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using mprExteriorPlanDimensions.Body;
using mprExteriorPlanDimensions.Configurations;
using mprExteriorPlanDimensions.View;
using mprExteriorPlanDimensions.Work;
using ModPlusAPI;
using ModPlusAPI.Windows;

namespace mprExteriorPlanDimensions.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExteriorPlanDimensionsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Statistic.SendCommandStarting(new Interface());

                Document doc = commandData.Application.ActiveUIDocument.Document;
                // Проверяем, что находимся на нужном виде
                var view = doc.ActiveView;
                var isViewPlan = view is ViewPlan;
                if (isViewPlan)
                {
                    var configuration = GetExteriorConfiguration();
                    if (configuration != null)
                    {
                        InsertExteriorDimensions insertExteriorDimensions =
                            new InsertExteriorDimensions(configuration, commandData.Application);
                        insertExteriorDimensions.DoWork();
                    }
                    else
                    {
                        if (MessageBox.ShowYesNo(
                            "Отсутствуют рабочие конфигурации" + Environment.NewLine +
                            "Открыть настройку рабочих конфигураций?", MessageBoxIcon.Question))
                        {
                            SettingsWindow settings = new SettingsWindow();
                            settings.ShowDialog();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Нужно перейти на план этажа!", MessageBoxIcon.Alert);
                }

                return Result.Succeeded;
            }
            catch (OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
                return Result.Failed;
            }
        }
        private ExteriorConfiguration GetExteriorConfiguration()
        {
            try
            {
                var defConfig = Guid.TryParse(UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings,
                    "mprExteriorPlanDimensions",
                    "DefaultExteriorConfiguration"), out Guid g)
                    ? g
                    : Guid.Empty;
                if (defConfig == Guid.Empty) return null;

                var exteriorConfigurations = SettingsFile.LoadExteriorConfigurations();
                
                foreach (ExteriorConfiguration configuration in exteriorConfigurations)
                {
                    if (configuration.Id.Equals(defConfig))
                    {
                        return configuration;
                    }
                }
                if (exteriorConfigurations.Any())
                    return exteriorConfigurations[0];
                return null;
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
                return null;
            }
        }
    }
}
