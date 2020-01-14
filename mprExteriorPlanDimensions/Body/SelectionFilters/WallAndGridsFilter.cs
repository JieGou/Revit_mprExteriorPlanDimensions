namespace mprExteriorPlanDimensions.Body.SelectionFilters
{
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI.Selection;

    /// <summary>
    /// Walls and Grids selection filter
    /// </summary>
    internal class WallAndGridsFilter: ISelectionFilter
    {
        /// <inheritdoc />
        public bool AllowElement(Element elem)
        {
            return elem is Wall || elem is Grid;
        }

        /// <inheritdoc />
        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
