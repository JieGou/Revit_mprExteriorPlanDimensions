using System;

namespace mprExteriorPlanDimensions.Configurations
{
    /// <summary>Базовый класс Конфигурации</summary>
    public class BaseConfiguration : BaseNotify
    {
        #region Fields
        /// <summary>Идентификатор</summary>
        public Guid Id { get; set; }

        private string _name;
        /// <summary>Название конфигурации</summary>
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }
        #endregion
    }
}
