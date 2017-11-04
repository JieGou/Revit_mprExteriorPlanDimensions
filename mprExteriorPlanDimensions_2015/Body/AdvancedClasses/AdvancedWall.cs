using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using mprExteriorPlanDimensions.Body.Enumerators;

namespace mprExteriorPlanDimensions.Body.AdvancedClasses
{
    public class AdvancedWall
    {
        #region Public Fields and Properties
        public readonly Wall Wall;
        /// <summary>False - не удалось определить значения для элемента</summary>
        public bool IsDefinded = true;
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
        /// <summary>Wall's faces</summary>
        //public List<PlanarFace> Faces;
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
                IsDefinded = false;
                return;
            }
            // Get curve from location curve
            LocationCurveCurve = locationCurve.Curve;
            // get curve type            
            if (locationCurve.Curve is Line) CurveType = ElementCurveType.Line;
            else if (locationCurve.Curve is Arc) CurveType = ElementCurveType.Arc;
            else
            {
                IsDefinded = false;
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
                IsDefinded = false;
                return;
            }
            // get orientation
            Orientation = GeometryHelpers.GetElementOrientation(locationCurve.Curve);
            if (Orientation == ElementOrientation.CloseToHorizontal ||
                Orientation == ElementOrientation.CloseToVertical ||
                Orientation == ElementOrientation.Undefined)
                IsDefinded = false;

            
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
                        if(advancedPlanarFace.IsDefinded)
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
        public SolidCurveIntersection IntersectToWall(AdvancedWall checkedAdvancedWall)
        {
            SolidCurveIntersection intersection;
            // Пропускаем проверку стены самой с собой
            if (Wall.Id.IntegerValue.Equals(checkedAdvancedWall.Wall.Id.IntegerValue)) return null;
            /* Получаю для проверяемой стены два отрезка
             * которые будут начинаться внизу стены. Учитывая, что LocationCurve не всегда лежит
             * в основании стены, нужно точки создавать на основе минимального значения Z
             * Проверку нужно проводит в двух направлениях: текущую с проверяемой и наоборот
             */
            // Получаю точки tessellate
            var tessellatePoints = checkedAdvancedWall.LocationCurveCurve.Tessellate();
            var checkedWallMinZ = checkedAdvancedWall.GetMinZ();
            // Количество пересечений
            //var intersectionsCount = 0;
            foreach (XYZ tessellatePoint in tessellatePoints)
            {
                // Проверяемый отрезок обязательно должен быть ограниченный (Bound)
                var line = Line.CreateBound(
                    new XYZ(tessellatePoint.X, tessellatePoint.Y, checkedWallMinZ),
                    new XYZ(tessellatePoint.X, tessellatePoint.Y, checkedWallMinZ + 1000000));
                foreach (Solid solid in Solids)
                {
                    try
                    {
                        intersection = solid.IntersectWithCurve(line, null);
                        // Если есть пересечение, то увеличиваем счетчик
                        if (intersection.SegmentCount > 0) //intersectionsCount++;
                            return intersection;
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            return null;
        }
        /// <summary>Сравнение что данные две стены лежат на одной прямой по указанной стороне</summary>
        /// <param name="checkedAdvancedWall">Сравниваемая стена</param>
        /// <param name="extremeWallVariant">Направление стороны</param>
        /// <returns></returns>
        public bool LiesInLineWithWallBySide(AdvancedWall checkedAdvancedWall, ExtremeWallVariant extremeWallVariant)
        {
            var tolerance = 0.0001;

            if (Orientation == ElementOrientation.Vertical &&
                checkedAdvancedWall.Orientation == ElementOrientation.Vertical)
            {
                if (extremeWallVariant == ExtremeWallVariant.Left)
                {
                    var minX = Math.Min(checkedAdvancedWall.StartPoint.X, checkedAdvancedWall.EndPoint.X);
                    if (Math.Abs(StartPoint.X - minX) < tolerance ||
                        Math.Abs(EndPoint.X - minX) < tolerance)
                    {
                        return true;
                    }
                }
                if (extremeWallVariant == ExtremeWallVariant.Right)
                {
                    var maxX = Math.Max(checkedAdvancedWall.StartPoint.X, checkedAdvancedWall.EndPoint.X);
                    if (Math.Abs(StartPoint.X - maxX) < tolerance ||
                        Math.Abs(EndPoint.X - maxX) < tolerance)
                    {
                        return true;
                    }
                }
            }
            if (Orientation == ElementOrientation.Horizontal &&
                checkedAdvancedWall.Orientation == ElementOrientation.Horizontal)
            {
                if (extremeWallVariant == ExtremeWallVariant.Top)
                {
                    var maxY = Math.Max(checkedAdvancedWall.StartPoint.Y, checkedAdvancedWall.EndPoint.Y);
                    if (Math.Abs(StartPoint.Y - maxY) < tolerance ||
                        Math.Abs(EndPoint.Y - maxY) < tolerance)
                    {
                        return true;
                    }
                }
                if (extremeWallVariant == ExtremeWallVariant.Bottom)
                {
                    var minY = Math.Min(checkedAdvancedWall.StartPoint.Y, checkedAdvancedWall.EndPoint.Y);
                    if (Math.Abs(StartPoint.Y - minY) < tolerance ||
                        Math.Abs(EndPoint.Y - minY) < tolerance)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        /// <summary>Является ли стена отдельно стоящей</summary>
        /// <returns></returns>
        public bool IsStandAlone(IList<Element> allWalls, Document doc)
        {
            var joinIds = new List<int>();

            var joind = JoinGeometryUtils.GetJoinedElements(doc, Wall);
            foreach (ElementId elementId in joind)
            {
                var el = doc.GetElement(elementId);
                if(el is Wall && !elementId.IntegerValue.Equals(Wall.Id.IntegerValue))
                    if(!joinIds.Contains(elementId.IntegerValue))
                        joinIds.Add(elementId.IntegerValue);
            }
            // Нужно найти количество элементов на концах стены
            ElementArray elementsAtJoinAtStart = ((LocationCurve)Wall.Location).get_ElementsAtJoin(0);
            ElementArray elementsAtJoinAtEnd = ((LocationCurve)Wall.Location).get_ElementsAtJoin(1);
            if (!elementsAtJoinAtEnd.IsEmpty)
            {
                foreach (Element e in elementsAtJoinAtEnd)
                    if (e is Wall && !Wall.Id.IntegerValue.Equals(e.Id.IntegerValue)) // Не забываем исклучить саму себя
                        if(!joinIds.Contains(e.Id.IntegerValue))
                            joinIds.Add(e.Id.IntegerValue);
            }
            if (!elementsAtJoinAtStart.IsEmpty)
            {
                foreach (Element e in elementsAtJoinAtStart)
                    if (e is Wall && !Wall.Id.IntegerValue.Equals(e.Id.IntegerValue))
                        if (!joinIds.Contains(e.Id.IntegerValue))
                            joinIds.Add(e.Id.IntegerValue);
            }
            // Ищу сопряжения стен, приходящих к проверяемой например в середине
            foreach (var wall in allWalls)
            {
                if(wall.Id.IntegerValue.Equals(Wall.Id.IntegerValue)) continue;
                elementsAtJoinAtStart = ((LocationCurve)wall.Location).get_ElementsAtJoin(0);
                elementsAtJoinAtEnd = ((LocationCurve)wall.Location).get_ElementsAtJoin(1);
                if (!elementsAtJoinAtEnd.IsEmpty)
                {
                    foreach (Element e in elementsAtJoinAtEnd)
                        if (e is Wall &&
                            Wall.Id.IntegerValue.Equals(e.Id.IntegerValue)) // Проверяем пересечение с искомой стеной
                            return false; // Если есть пересечение, то дальше итерация не нужна
                }
                if (!elementsAtJoinAtStart.IsEmpty)
                {
                    foreach (Element e in elementsAtJoinAtStart)
                        if (e is Wall && Wall.Id.IntegerValue.Equals(e.Id.IntegerValue))
                            return false;
                }
            }
            // если список пустой, то стена отдельно стоящая
            return !joinIds.Any();
        }
        #endregion
    }
}
