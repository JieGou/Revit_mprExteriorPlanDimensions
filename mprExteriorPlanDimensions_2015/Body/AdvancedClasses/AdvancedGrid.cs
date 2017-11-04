using Autodesk.Revit.DB;
using mprExteriorPlanDimensions.Body.Enumerators;

namespace mprExteriorPlanDimensions.Body.AdvancedClasses
{
    public class AdvancedGrid
    {
        #region Public Fields
        public readonly Grid Grid;
        /// <summary>False - не удалось определить значения для элемента</summary>
        public bool IsDefinded = true;
        /// <summary>Ориентация элемента</summary>
        public ElementOrientation Orientation;
        /// <summary>Тип кривой, лежащий в основе</summary>
        public ElementCurveType CurveType;
        // Start Point
        public XYZ StartPoint;
        // End Point
        public XYZ EndPoint;
        #endregion

        #region Constructors

        public AdvancedGrid(Grid grid)
        {
            Grid = grid;
            DefineAdvancedGridFields();
        }

        #endregion

        #region Private Methods

        private void DefineAdvancedGridFields()
        {
            // get location curve
            var curve = Grid.Curve;
            if (curve == null)
            {
                IsDefinded = false;
                return;
            }
            // get curve type
            if (curve is Line) CurveType = ElementCurveType.Line;
            else if (curve is Arc) CurveType = ElementCurveType.Arc;
            else
            {
                IsDefinded = false;
                return;
            }
            // points
            StartPoint = curve.GetEndPoint(0);
            EndPoint = curve.GetEndPoint(1);
            // get orientation
            Orientation = GeometryHelpers.GetElementOrientation(curve);
            if (Orientation == ElementOrientation.CloseToHorizontal ||
                Orientation == ElementOrientation.CloseToVertical ||
                Orientation == ElementOrientation.Undefined)
                IsDefinded = false;
        }

        #endregion
    }
}
