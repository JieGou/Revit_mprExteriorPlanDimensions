namespace mprExteriorPlanDimensions.Configurations
{
    using System.Collections.Generic;

    public class Constants
    {
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1310 // Field names should not contain underscore
        //// ReSharper disable InconsistentNaming
        public const string XElementName_Root = "Configurations";
        public const string XElementName_Chain = "Chain";
        public const string XElementName_ExteriorConfiguration = "ExteriorConfiguration";
        public const string XElementName_ExteriorConfigurations = "ExteriorConfigurations";
        //// ReSharper restore InconsistentNaming
#pragma warning restore SA1310 // Field names should not contain underscore
#pragma warning restore SA1600 // Elements should be documented

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
