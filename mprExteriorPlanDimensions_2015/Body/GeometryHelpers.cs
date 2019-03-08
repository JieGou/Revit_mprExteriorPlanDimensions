namespace mprExteriorPlanDimensions.Body
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Autodesk.Revit.DB;
    using AdvancedClasses;
    using Enumerators;
    using Point = Autodesk.Revit.DB.Point;

    public static class GeometryHelpers
    {
        /// <summary>Определение ориентации по ординатам X и Y (т.е. в плане)</summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static ElementOrientation GetElementOrientation(Curve curve)
        {
            var tolerance = 0.0001;
            var startPoint = curve.GetEndPoint(0);
            var endPoint = curve.GetEndPoint(1);
            // Если точки совпадают
            if (startPoint.IsAlmostEqualTo(endPoint)) return ElementOrientation.Undefined;
            // Если X равны и Y не равны
            if (Math.Abs(startPoint.X - endPoint.X) < tolerance &&
                Math.Abs(startPoint.Y - endPoint.Y) > tolerance)
                return ElementOrientation.Vertical;
            // Если Y равны и X не равны
            if (Math.Abs(startPoint.X - endPoint.X) > tolerance &&
                Math.Abs(startPoint.Y - endPoint.Y) < tolerance)
                return ElementOrientation.Horizontal;
            // Наклонные
            return Math.Abs(endPoint.X - startPoint.X) >= Math.Abs(endPoint.Y - startPoint.Y)
                ? ElementOrientation.CloseToHorizontal
                : ElementOrientation.CloseToVertical;
        }

        public static double GetViewPlanCutPlaneElevation(ViewPlan viewPlan, Document doc)
        {
            PlanViewRange planViewRange = viewPlan.GetViewRange();
            return planViewRange.GetOffset(PlanViewPlane.CutPlane) + viewPlan.GenLevel.Elevation;
        }
        
        public static bool IsHorizontal(this PlanarFace face)
        {
            var tolerance = 0.0001;

            if (Math.Abs(face.GetMinY() - face.GetMaxY()) < tolerance) return true;
            return false;
        }

        public static bool IsVertical(this PlanarFace face)
        {
            var tolerance = 0.0001;
            if (Math.Abs(face.GetMinX() - face.GetMaxX()) < tolerance) return true;
            return false;
        }
        
        #region Faces extensions

        public static double GetMinX(this PlanarFace face)
        {
            var points = new List<XYZ>();
            EdgeArrayArray edgeArrayArray = face.EdgeLoops;
            foreach (EdgeArray array in edgeArrayArray)
            {
                foreach (Edge edge in array)
                {
                    points.Add(edge.AsCurve().GetEndPoint(0));
                    points.Add(edge.AsCurve().GetEndPoint(1));
                }
            }
            points.Sort((p1, p2) => p1.X.CompareTo(p2.X));
            return points.First().X;
        }
        public static double GetMaxX(this PlanarFace face)
        {
            var points = new List<XYZ>();
            EdgeArrayArray edgeArrayArray = face.EdgeLoops;
            foreach (EdgeArray array in edgeArrayArray)
            {
                foreach (Edge edge in array)
                {
                    points.Add(edge.AsCurve().GetEndPoint(0));
                    points.Add(edge.AsCurve().GetEndPoint(1));
                }
            }
            points.Sort((p1, p2) => p1.X.CompareTo(p2.X));
            return points.Last().X;
        }
        public static double GetMinY(this PlanarFace face)
        {
            var points = new List<XYZ>();
            EdgeArrayArray edgeArrayArray = face.EdgeLoops;
            foreach (EdgeArray array in edgeArrayArray)
            {
                foreach (Edge edge in array)
                {
                    points.Add(edge.AsCurve().GetEndPoint(0));
                    points.Add(edge.AsCurve().GetEndPoint(1));
                }
            }
            points.Sort((p1, p2) => p1.Y.CompareTo(p2.Y));
            return points.First().Y;
        }
        public static double GetMaxY(this PlanarFace face)
        {
            var points = new List<XYZ>();
            EdgeArrayArray edgeArrayArray = face.EdgeLoops;
            foreach (EdgeArray array in edgeArrayArray)
            {
                foreach (Edge edge in array)
                {
                    points.Add(edge.AsCurve().GetEndPoint(0));
                    points.Add(edge.AsCurve().GetEndPoint(1));
                }
            }
            points.Sort((p1, p2) => p1.Y.CompareTo(p2.Y));
            return points.Last().Y;
        }
        public static double GetMinZ(this PlanarFace face)
        {
            var points = new List<XYZ>();
            EdgeArrayArray edgeArrayArray = face.EdgeLoops;
            foreach (EdgeArray array in edgeArrayArray)
            {
                foreach (Edge edge in array)
                {
                    points.Add(edge.AsCurve().GetEndPoint(0));
                    points.Add(edge.AsCurve().GetEndPoint(1));
                }
            }
            points.Sort((p1, p2) => p1.Z.CompareTo(p2.Z));
            return points.First().Z;
        }
        public static double GetMaxZ(this PlanarFace face)
        {
            var points = new List<XYZ>();
            EdgeArrayArray edgeArrayArray = face.EdgeLoops;
            foreach (EdgeArray array in edgeArrayArray)
            {
                foreach (Edge edge in array)
                {
                    points.Add(edge.AsCurve().GetEndPoint(0));
                    points.Add(edge.AsCurve().GetEndPoint(1));
                }
            }
            points.Sort((p1, p2) => p1.Z.CompareTo(p2.Z));
            return points.Last().Z;
        }

        #endregion

        public static void GetGeometryFromGeometryElement(
            GeometryElement geometryElement,
            ref List<Face> faces,
            ref List<Curve> curves,
            ref List<Solid> solids
            )
        {
            foreach (GeometryObject geometryObject in geometryElement)
            {

                Face face = geometryObject as Face;
                if (face != null)
                {
                    faces.Add(face);
                    continue;
                }
                Curve curve = geometryObject as Curve;
                if (curve != null)
                {
                    curves.Add(curve);
                    continue;
                }
                Solid solid = geometryObject as Solid;
                if (solid != null)
                {
                    solids.Add(solid);
                    continue;
                }
                GeometryInstance geometryInstance = geometryObject as GeometryInstance;
                if (geometryInstance != null)
                {
                    //GeometryElement geometrySymbol = geometryInstance.GetSymbolGeometry();
                    //if(geometrySymbol != null)
                    //    GetGeometryFromGeometryElement(geometrySymbol, ref faces, ref curves, ref solids);
                    GeometryElement instanceGeometry = geometryInstance.GetInstanceGeometry();
                    if (instanceGeometry != null)
                        GetGeometryFromGeometryElement(instanceGeometry, ref faces, ref curves, ref solids);
                }
            }
        }
        
        #region Get curves from ...
       
        /// <summary>Get only lines form solid</summary>
        /// <param name="solid"></param>
        /// <param name="lines"></param>
        public static void GetLinesFromSolid(Solid solid, ref List<Line> lines)
        {
            foreach (Edge edge in solid.Edges)
            {
                var curve = edge.AsCurve();
                if (curve.Reference != null && curve is Line line)
                    lines.Add(line);
            }
            foreach (var f in solid.Faces)
            {
                if (f is Face face)
                    GetLinesFromFace(face, ref lines);
            }
        }
        
        /// <summary>Получение только линий из фейсов</summary>
        /// <param name="face"></param>
        /// <param name="lines"></param>
        public static void GetLinesFromFace(Face face, ref List<Line> lines)
        {
            EdgeArrayArray edgeArrayArray = face.EdgeLoops;
            foreach (EdgeArray edgeArray in edgeArrayArray)
            {
                foreach (Edge edge in edgeArray)
                {
                    var curve = edge.AsCurve();
                    if (curve.Reference != null && curve is Line line)
                        lines.Add(line);
                }
            }
        }
        
        #endregion

        /// <summary>Получение (рекурсивно) только линий из элемента геометрии</summary>
        /// <param name="geometryElement"></param>
        /// <param name="lines"></param>
        public static void GetLinesFromGeometryElement(
            GeometryElement geometryElement, ref List<Line> lines)
        {
            foreach (GeometryObject geometryObject in geometryElement)
            {
                if (geometryObject is Line line)
                {
                    lines.Add(line);
                    continue;
                }
                if (geometryObject is Face face)
                {
                    GetLinesFromFace(face, ref lines);
                    continue;
                }
                if (geometryObject is Solid solid)
                {
                    GetLinesFromSolid(solid, ref lines);
                    continue;
                }
                if (geometryObject is GeometryInstance geometryInstance)
                {
                    //GeometryElement geometrySymbol = geometryInstance.GetSymbolGeometry();
                    //if (geometrySymbol != null)
                    //    GetLinesFromGeometryElement(geometrySymbol, ref lines);
                    GeometryElement instanceGeometry = geometryInstance.GetInstanceGeometry();
                    if (instanceGeometry != null)
                        GetLinesFromGeometryElement(instanceGeometry, ref lines);
                }
            }
        }
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum SpecialReferenceType
        {
            Left = 0,
            CenterLR = 1,
            Right = 2,
            Front = 3,
            CenterFB = 4,
            Back = 5,
            Bottom = 6,
            CenterElevation = 7,
            Top = 8
        }
        
        public static bool IntersectTo(this Line line, Line checkedLine)
        {
            if (line.Intersect(checkedLine) == SetComparisonResult.Overlap) return true;
            return false;
        }
    }
}
