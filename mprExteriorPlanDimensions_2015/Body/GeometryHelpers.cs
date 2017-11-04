using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Autodesk.Revit.DB;
using mprExteriorPlanDimensions.Body.AdvancedClasses;
using mprExteriorPlanDimensions.Body.Enumerators;
using Point = Autodesk.Revit.DB.Point;

namespace mprExteriorPlanDimensions.Body
{
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
        /// <summary>Проверка, что Face расположен в плоскости Z (т.е. все Z одинаковые)</summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public static bool IsInZplane(this PlanarFace face)
        {
            var tolerance = 0.0001;
            List<XYZ> points = new List<XYZ>();
            EdgeArrayArray edgeArrayArray = face.EdgeLoops;
            foreach (EdgeArray edgeArray in edgeArrayArray)
            {
                foreach (Edge edge in edgeArray)
                {
                    foreach (XYZ xyz in edge.Tessellate())
                    {
                        points.Add(xyz);
                    }
                }
            }
            var allZsame = true;
            for (var i = 0; i < points.Count - 1; i++)
            {
                XYZ point = points[i];
                XYZ nextPoint = points[i + 1];
                if (Math.Abs(point.Z - nextPoint.Z) < tolerance) continue;
                allZsame = false;
                break;
            }
            return allZsame;
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

        public static bool IsHorizontal(this Curve curve)
        {
            var tolerance = 0.0001;
            if (curve is Line line)
            {
                if (Math.Abs(line.GetEndPoint(0).Y - line.GetEndPoint(1).Y) < tolerance)
                    return true;
                return false;
            }
            return false;
        }

        public static bool IsVertical(this Curve curve)
        {
            var tolerance = 0.0001;
            if (curve is Line line)
            {
                if (Math.Abs(line.GetEndPoint(0).X - line.GetEndPoint(1).X) < tolerance)
                    return true;
                return false;
            }
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
        /// <summary>Get curves that has Reference from solid</summary>
        /// <param name="solid"></param>
        /// <param name="curves"></param>
        public static void GetCurvesFromSolid(Solid solid, ref List<Curve> curves)
        {
            foreach (Edge edge in solid.Edges)
            {
                if (edge.AsCurve().Reference != null)
                    curves.Add(edge.AsCurve());
            }
            foreach (var f in solid.Faces)
            {
                if (f is Face face)
                    GetCurvesFromFace(face, ref curves);
            }
        }
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

        public static void GetCurvesFromFace(Face face, ref List<Curve> curves)
        {
            EdgeArrayArray edgeArrayArray = face.EdgeLoops;
            foreach (EdgeArray edgeArray in edgeArrayArray)
            {
                foreach (Edge edge in edgeArray)
                {
                    if (edge.AsCurve().Reference != null)
                        curves.Add(edge.AsCurve());
                }
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


        public static Reference GetSpecialFamilyReference(FamilyInstance inst, SpecialReferenceType refType)
        {
            Reference indexRef = null;

            int idx = (int)refType;

            if (inst != null)
            {
                Document dbDoc = inst.Document;

                Options geomOptions = new Options
                {
                    ComputeReferences = true,
                    View = dbDoc.ActiveView,
                    IncludeNonVisibleObjects = true
                };

                GeometryElement gElement = inst.get_Geometry(geomOptions);
                GeometryInstance gInst = gElement.First() as GeometryInstance;

                String sampleStableRef = null;

                if (gInst != null)
                {
                    GeometryElement gSymbol = gInst.GetSymbolGeometry();

                    if (gSymbol != null)
                    {
                        foreach (GeometryObject geomObj in gSymbol)
                        {
                            if (geomObj is Solid)
                            {
                                Solid solid = geomObj as Solid;

                                if (solid.Faces.Size > 0)
                                {
                                    Face face = solid.Faces.get_Item(0);
                                    sampleStableRef = face.Reference.ConvertToStableRepresentation(dbDoc);
                                    break;
                                }
                            }
                            else if (geomObj is Curve)
                            {
                                Curve curve = geomObj as Curve;

                                sampleStableRef = curve.Reference.ConvertToStableRepresentation(dbDoc);
                                break;
                            }
                            else if (geomObj is Point)
                            {
                                Point point = geomObj as Point;

                                sampleStableRef = point.Reference.ConvertToStableRepresentation(dbDoc);
                                break;
                            }
                        }
                    }

                    if (sampleStableRef != null)
                    {
                        String[] refTokens = sampleStableRef.Split(':');

                        String customStableRef = refTokens[0] + ":" + refTokens[1] + ":" + refTokens[2] + ":" + refTokens[3] + ":" + idx;

                        indexRef = Reference.ParseFromStableRepresentation(dbDoc, customStableRef);

                        GeometryObject geoObj = inst.GetGeometryObjectFromReference(indexRef);

                        if (geoObj != null)
                        {
                            String finalToken = "";

                            if (geoObj is Edge)
                            {
                                finalToken = ":LINEAR";
                            }

                            if (geoObj is Face)
                            {
                                finalToken = ":SURFACE";
                            }

                            customStableRef += finalToken;

                            indexRef = Reference.ParseFromStableRepresentation(dbDoc, customStableRef);
                        }
                        else
                        {
                            indexRef = null;
                        }
                    }
                }
                else
                {
                    //throw new Exception("No Symbol Geometry found...");
                    return null;
                }
            }
            return indexRef;
        }
        /// <summary>Проверка, что текущая линия пересекается с проверяемой путем поднятия текущей до уровня проверяемой по Z</summary>
        /// <param name="line"></param>
        /// <param name="checkedLine"></param>
        /// <returns></returns>
        public static bool IntersectToByMovingZ(this Curve line, Line checkedLine)
        {
            var lineToCheck = AdvancedHelpers.TryCreateBound(
                new XYZ(line.GetEndPoint(0).X, line.GetEndPoint(0).Y, checkedLine.Origin.Z),
                new XYZ(line.GetEndPoint(1).X, line.GetEndPoint(1).Y, checkedLine.Origin.Z));
            if (lineToCheck != null)
                if (lineToCheck.Intersect(checkedLine) == SetComparisonResult.Overlap)
                {
                    return true;
                }
            return false;
        }
        public static bool IntersectToByMovingZ(this Curve line, Line checkedLine, out XYZ intersectPoint)
        {
            var lineToCheck = AdvancedHelpers.TryCreateBound(
                new XYZ(line.GetEndPoint(0).X, line.GetEndPoint(0).Y, checkedLine.Origin.Z),
                new XYZ(line.GetEndPoint(1).X, line.GetEndPoint(1).Y, checkedLine.Origin.Z));
            if (lineToCheck != null)
                if (lineToCheck.Intersect(checkedLine, out IntersectionResultArray intersectionResult) == SetComparisonResult.Overlap)
                {
                    intersectPoint = intersectionResult.get_Item(0).XYZPoint;
                    return true;
                }
            intersectPoint = null;
            return false;
        }

        public static bool IntersectTo(this Line line, Line checkedLine)
        {
            if (line.Intersect(checkedLine) == SetComparisonResult.Overlap) return true;
            return false;
        }

        public static bool IntersectToByMovingZ(this PlanarFace planarFace, Line checkedLine)
        {
            EdgeArrayArray edgeArrayArray = planarFace.EdgeLoops;
            foreach (EdgeArray edgeArray in edgeArrayArray)
            {
                foreach (Edge edge in edgeArray)
                {
                    if (edge.AsCurve() is Line line)
                    {
                        var lineToCheck = AdvancedHelpers.TryCreateBound(
                            new XYZ(line.GetEndPoint(0).X, line.GetEndPoint(0).Y, checkedLine.Origin.Z),
                            new XYZ(line.GetEndPoint(1).X, line.GetEndPoint(1).Y, checkedLine.Origin.Z));
                        if (lineToCheck != null)
                            if (lineToCheck.Intersect(checkedLine) == SetComparisonResult.Overlap)
                            {
                                return true;
                            }
                    }
                }
            }
            return false;
        }
        /// <summary>Проверка, что хоть одна линия в списке пересекается с проверяемой</summary>
        /// <param name="lines"></param>
        /// <param name="checkedLine"></param>
        /// <returns></returns>
        public static bool HasIntesectionByMovingZ(List<Line> lines, Line checkedLine)
        {
            foreach (var line in lines)
            {
                if (line.IntersectToByMovingZ(checkedLine)) return true;
            }
            return false;
        }
        /// <summary>Получение средней линии из списка линий. Обрабатывает только горизонтальные и вертикальные линии</summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static Line GetMiddleLine(List<Line> lines)
        {
            if (!lines.Any()) return null;
            var tolerance = 0.0001;
            var allX = new List<double>();
            var allY = new List<double>();
            foreach (var line in lines)
            {
                var p1 = line.GetEndPoint(0);
                var p2 = line.GetEndPoint(1);
                if (!allX.Contains(p1.X)) allX.Add(p1.X);
                if (!allX.Contains(p2.X)) allX.Add(p2.X);
                if (!allY.Contains(p1.Y)) allY.Add(p1.Y);
                if (!allY.Contains(p2.Y)) allY.Add(p2.Y);
            }
            // так как я точно передаю либо все горизонтальные, либо все вертикальные
            if (lines[0].IsVertical())
            {
                var middleX = 0.5 * (allX.Max() + allX.Min());
                foreach (var line in lines)
                {
                    if (Math.Abs(line.GetEndPoint(0).X - middleX) < tolerance)
                        return line;
                }
            }
            else if (lines[0].IsHorizontal())
            {
                var middleY = 0.5 * (allY.Max() + allY.Min());
                foreach (var line in lines)
                {
                    if (Math.Abs(line.GetEndPoint(0).Y - middleY) < tolerance)
                        return line;
                }
            }
            return null;
        }

        public static Face GetInstanceFaceFromSymbolRef(Reference symbolRef, Document dbDoc)
        {
            Face instFace = null;

            Options gOptions = new Options
            {
                ComputeReferences = true,
                DetailLevel = ViewDetailLevel.Undefined,
                IncludeNonVisibleObjects = true
            };

            Element elem = dbDoc.GetElement(symbolRef.ElementId);
            string stableRefSymbol = symbolRef.ConvertToStableRepresentation(dbDoc);
            string[] tokenList = stableRefSymbol.Split(':');
            string stableRefInst = tokenList[3] + ":" + tokenList[4] + ":" + tokenList[5];

            GeometryElement geomElem = elem.get_Geometry(gOptions);
            foreach (GeometryObject geomElemObj in geomElem)
            {
                GeometryInstance geomInst = geomElemObj as GeometryInstance;
                if (geomInst != null)
                {
                    GeometryElement gInstGeom = geomInst.GetInstanceGeometry();
                    foreach (GeometryObject gGeomObject in gInstGeom)
                    {
                        Solid solid = gGeomObject as Solid;
                        if (solid != null)
                        {
                            //foreach (Edge edge in solid.Edges)
                            //{
                            //    string stableRef = edge.Reference.ConvertToStableRepresentation(dbDoc);

                            //    if (stableRef == stableRefInst)
                            //    {
                            //        instFace = edge;
                            //        break;
                            //    }
                            //}
                            foreach (Face face in solid.Faces)
                            {
                                string stableRef = face.Reference.ConvertToStableRepresentation(dbDoc);
                                if (stableRef == stableRefInst)
                                {
                                    instFace = face;
                                    break;
                                }
                            }
                        }

                        if (instFace != null)
                        {
                            // already found, exit early
                            break;
                        }
                    }
                }
                if (instFace != null)
                {
                    // already found, exit early
                    break;
                }
            }
            return instFace;
        }

        public static XYZ GetCenterPoint(this Line line)
        {
            var p1 = line.GetEndPoint(0);
            var p2 = line.GetEndPoint(1);
            return (p1 + p2) / 2;
        }
    }
}
