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
        private const string LangItem = "mprExteriorPlanDimensions";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Statistic.SendCommandStarting(new ModPlusConnector());

                var doc = commandData.Application.ActiveUIDocument.Document;
                
                // Проверяем, что находимся на нужном виде
                var view = doc.ActiveView;
                var isViewPlan = view is ViewPlan;
                if (isViewPlan)
                {
                    var configuration = GetExteriorConfiguration();
                    if (configuration != null)
                    {
                        var insertExteriorDimensions =
                            new InsertExteriorDimensions(configuration, commandData.Application);
                        insertExteriorDimensions.DoWork();
                    }
                    else
                    {
                        if (MessageBox.ShowYesNo(Language.GetItem(LangItem, "msg5"), MessageBoxIcon.Question))
                        {
                            var settings = new SettingsWindow();
                            settings.ShowDialog();
                        }
                    }
                }
                else
                {
                    MessageBox.Show(Language.GetItem(LangItem, "msg6"), MessageBoxIcon.Alert);
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
                    "DefaultExteriorConfiguration"), out var g)
                    ? g
                    : Guid.Empty;
                if (defConfig == Guid.Empty) return null;

                var exteriorConfigurations = SettingsFile.LoadExteriorConfigurations();
                
                foreach (var configuration in exteriorConfigurations)
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
