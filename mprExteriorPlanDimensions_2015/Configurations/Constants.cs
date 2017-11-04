using System.Collections.Generic;

namespace mprExteriorPlanDimensions.Configurations
{
    public class Constants
    {
        // ReSharper disable InconsistentNaming
        public const string XElementName_Root = "Configurations";
        public const string XElementName_Chain = "Chain";
        public const string XElementName_ExteriorConfiguration = "ExteriorConfiguration";
        public const string XElementName_ExteriorConfigurations = "ExteriorConfigurations";
        // ReSharper restore InconsistentNaming

        static Constants()
        {
            ElementOffsets = new List<int>();
            for (var i = 1; i <= 50; i++)
            {
                ElementOffsets.Add(1 * i);
            }
        }

        /// <summary>Допустимые величины отступа в мм, кратно 100</summary>
        public static List<int> ElementOffsets;
        
    }
}
