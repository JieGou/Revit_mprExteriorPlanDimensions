namespace mprExteriorPlanDimensions.Body.AdvancedClasses
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;
    using Enumerators;

    public class AdvancedWall
    {
        #region Public Fields and Properties
        public readonly Wall Wall;
        
        /// <summary>False - не удалось определить значения для элемента</summary>
        public bool IsDefined = true;
        
        /// <summary>Ориентация элемента</summary>
        public ElementOrientation Orientation;
        
        /// <summary>Тип кривой, лежащий в основе</summary>
        public ElementCurveType CurveType;
        
        /// <summary>LocationCurve's Start Point</summary>
        public XYZ StartPoint;
        
        /// <summary>LocationCurve's End Point</summary>
        public XYZ EndPoint;
        
        /// <summary>LocationCurve's Middle Point</summary>
        public XYZ MidPoint;
        
        /// <summary>LocationCurve's Curve</summary>
        public Curve LocationCurveCurve;
        
        /// <summary>Wall's solids</summary>
        public List<Solid> Solids;
        
        /// <summary>Wall's edges</summary>
        public List<Edge> Edges;
        
        /// <summary>Wall's advanced faces</summary>
        public List<AdvancedPlanarFace> AdvancedPlanarFaces;
        #endregion

        #region Constructor
        
        public AdvancedWall(Wall wall)
        {
            Wall = wall;
            Solids = new List<Solid>();
            Edges = new List<Edge>();
            AdvancedPlanarFaces = new List<AdvancedPlanarFace>();
            DefineAdvancedWallFields();
        }

        #endregion

        #region Private Methods

        private void DefineAdvancedWallFields()
        {
            // get location curve
            var locationCurve = Wall.Location as LocationCurve;
            if (locationCurve == null)
            {
                IsDefined = false;
                return;
            }
            // Get curve from location curve
            LocationCurveCurve = locationCurve.Curve;
            // get curve type            
            if (locationCurve.Curve is Line) CurveType = ElementCurveType.Line;
            else if (locationCurve.Curve is Arc) CurveType = ElementCurveType.Arc;
            else
            {
                IsDefined = false;
                return;
            }
            // get ends points
            StartPoint = locationCurve.Curve.GetEndPoint(0);
            EndPoint = locationCurve.Curve.GetEndPoint(1);
            MidPoint = 0.5 * (StartPoint + EndPoint);
            // get solids
            GetSolids();
            // get faces and edges
            GetFacesAndEdges();
            //if (Edges.Count == 0 || Faces.Count == 0)
            if (Edges.Count == 0 || AdvancedPlanarFaces.Count == 0)
            {
                IsDefined = false;
                return;
            }
            // get orientation
            Orientation = GeometryHelpers.GetElementOrientation(locationCurve.Curve);
            if (Orientation == ElementOrientation.CloseToHorizontal ||
                Orientation == ElementOrientation.CloseToVertical ||
                Orientation == ElementOrientation.Undefined)
                IsDefined = false;

            
        }
        private void GetSolids()
        {
            Options options = new Options
            {
                ComputeReferences = true // Обязательно ставить true, так как по Reference строятся размеры
            };
            GeometryElement geometry = Wall.get_Geometry(options);
            IEnumerable<GeometryObject> geometryObjects = geometry;
            foreach (GeometryObject geometryObject in geometryObjects)
            {
                Solid solid = geometryObject as Solid;
                if (solid != null)
                {
                    Solids.Add(solid);
                }
            }
        }
        private void GetFacesAndEdges()
        {
            foreach (Solid solid in Solids)
            {
                foreach (Edge edge in solid.Edges)
                {
                    Edges.Add(edge);
                }
                foreach (Face face in solid.Faces)
                {
                    PlanarFace planarFace = face as PlanarFace;
                    if (planarFace != null)
                    {
                        AdvancedPlanarFace advancedPlanarFace = new AdvancedPlanarFace(Wall.Id.IntegerValue, planarFace);
                        if(advancedPlanarFace.IsDefined)
                            AdvancedPlanarFaces.Add(advancedPlanarFace);
                    }
                }
            }
        }
        #endregion

        #region Public Methods
        
        /// <summary>Получение максимального из значений Z по граням стены</summary>
        /// <returns></returns>
        public double GetMaxZ()
        {
            List<double> zList = new List<double>();
            
            foreach (var face in AdvancedPlanarFaces)
            {
                foreach (var edge in face.Edges)
                {
                    var pt1 = edge.AsCurve().GetEndPoint(0);
                    var pt2 = edge.AsCurve().GetEndPoint(1);
                    if (!zList.Contains(pt1.Z)) zList.Add(pt1.Z);
                    if (!zList.Contains(pt2.Z)) zList.Add(pt2.Z);
                }
            }
            return zList.Max();
        }
        
        /// <summary>Получение минимального из значений Z по граням стены</summary>
        /// <returns></returns>
        public double GetMinZ()
        {
            List<double> zList = new List<double>();
            
            foreach (var face in AdvancedPlanarFaces)
            {
                foreach (var edge in face.Edges)
                {
                    var pt1 = edge.AsCurve().GetEndPoint(0);
                    var pt2 = edge.AsCurve().GetEndPoint(1);
                    if (!zList.Contains(pt1.Z)) zList.Add(pt1.Z);
                    if (!zList.Contains(pt2.Z)) zList.Add(pt2.Z);
                }
            }
            return zList.Min();
        }

        public double GetMinX()
        {
            List<double> xList = new List<double>();
            
            foreach (var face in AdvancedPlanarFaces)
            {
                xList.Add(face.MinX);
            }
            return xList.Min();
        }

        public double GetMaxX()
        {
            List<double> xList = new List<double>();
            
            foreach (var face in AdvancedPlanarFaces)
            {
                xList.Add(face.MaxX);
            }
            return xList.Max();
        }

        public double GetMinY()
        {
            List<double> yList = new List<double>();
            
            foreach (var face in AdvancedPlanarFaces)
            {
                yList.Add(face.MinY);
            }
            return yList.Min();
        }
        
        public double GetMaxY()
        {
            List<double> yList = new List<double>();
            
            foreach (var face in AdvancedPlanarFaces)
            {
                yList.Add(face.MaxY);
            }
            return yList.Max();
        }
        
        #endregion
    }
}
