#pragma warning disable SA1600 // Elements should be documented
namespace mprExteriorPlanDimensions
{
    using System;
    using System.Collections.Generic;
    using ModPlusAPI.Interfaces;

    public class ModPlusConnector : IModPlusFunctionInterface
    {
        public SupportedProduct SupportedProduct => SupportedProduct.Revit;

        public string Name => "mprExteriorPlanDimensions";

#if R2015
        public string AvailProductExternalVersion => "2015";
#elif R2016
        public string AvailProductExternalVersion => "2016";
#elif R2017
        public string AvailProductExternalVersion => "2017";
#elif R2018
        public string AvailProductExternalVersion => "2018";
#elif R2019
        public string AvailProductExternalVersion => "2019";
#elif R2020
        public string AvailProductExternalVersion => "2020";
#endif

        public string FullClassName => "mprExteriorPlanDimensions.Commands.ExteriorPlanDimensionsCommand";

        public string AppFullClassName => string.Empty;

        public Guid AddInId => Guid.Empty;

        public string LName => "Наружные размеры на плане";

        public string Description => "Простановка наружных размеров на плане этажа";

        public string Author => "Пекшев Александр aka Modis";

        public string Price => "0";

        public bool CanAddToRibbon => true;

        public string FullDescription => "Плагин позволяет в один клик проставить наружные размеры на плане этажа. Плагин обрабатывает только горизонтальные и вертикальные стены и оси. Простановка наружных размеров происходит согласно текущей рабочей конфигурации, в которой указываются стороны простановки размеров и настройки размерных цепочек";

        public string ToolTipHelpImage => string.Empty;

        public List<string> SubFunctionsNames => new List<string> { "mprExteriorPlanDimensionsSettings" };

        public List<string> SubFunctionsLames => new List<string> { "Наружные размеры. Настройки" };

        public List<string> SubDescriptions => new List<string> { "Настройки рабочих конфигураций для наружных размеров на плане этажа" };

        public List<string> SubFullDescriptions => new List<string> { string.Empty };

        public List<string> SubHelpImages => new List<string> { string.Empty };

        public List<string> SubClassNames => new List<string> { "mprExteriorPlanDimensions.Commands.ExteriorPlanDimensionsSettingsCommand" };
    }
}
#pragma warning restore SA1600 // Elements should be documented