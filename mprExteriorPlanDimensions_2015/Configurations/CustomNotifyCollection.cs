using System.Collections.ObjectModel;

namespace mprExteriorPlanDimensions.Configurations
{
    public class CustomNotifyCollection<T> : ObservableCollection<T>
    {
        protected override void RemoveItem(int index)
        {
            if(Count == 1)
                return;
            base.RemoveItem(index);
        }
    }
}