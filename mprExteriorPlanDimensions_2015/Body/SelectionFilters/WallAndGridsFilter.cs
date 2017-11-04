using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace mprExteriorPlanDimensions.Body.SelectionFilters
{
    internal class WallAndGridsFilter: ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is Wall || elem is Grid;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
