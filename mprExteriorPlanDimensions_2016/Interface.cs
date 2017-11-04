using System;
using System.Collections.Generic;
using ModPlusAPI.Interfaces;

namespace mprExteriorPlanDimensions
{
    public class Interface : IModPlusFunctionInterface
    {
        public SupportedProduct SupportedProduct => SupportedProduct.Revit;
        public string Name => "mprExteriorPlanDimensions";
        public string AvailProductExternalVersion => "2016";
        public string FullClassName => "mprExteriorPlanDimensions.Commands.ExteriorPlanDimensionsCommand";
        public string AppFullClassName => string.Empty;
        public Guid AddInId => Guid.Empty;
        public string LName => "Наружные размеры на плане";
        public string Description => "Простановка наружных размеров на плане этажа";
        public string Author => "Пекшев Александр aka Modis";
        public string Price => "0";
        public bool CanAddToRibbon => true;
        public string FullDescription => "Функция позволяет в один клик проставить наружные размеры на плане этажа. Функция обрабатывает только горизонтальные и вертикальные стены и оси. Простановка наружных размеров происходит согласно текущей рабочей конфигурации, в которой указываются стороны простановки размеров и настройки размерных цепочек";
        public string ToolTipHelpImage => "";
        public List<string> SubFunctionsNames => new List<string> { "mprExteriorPlanDimensionsSettings" };
        public List<string> SubFunctionsLames => new List<string> { "Наружные размеры. Настройки" };
        public List<string> SubDescriptions => new List<string> { "Настройки рабочих конфигураций для наружных размеров на плане этажа" };
        public List<string> SubFullDescriptions => new List<string> { "" };
        public List<string> SubHelpImages => new List<string> { "" };
        public List<string> SubClassNames => new List<string> { "mprExteriorPlanDimensions.Commands.ExteriorPlanDimensionsSettingsCommand" };
    }
}
