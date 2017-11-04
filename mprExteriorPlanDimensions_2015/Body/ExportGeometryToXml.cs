using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Autodesk.Revit.DB;
using mprExteriorPlanDimensions.Body.AdvancedClasses;

namespace mprExteriorPlanDimensions.Body
{
    /// <summary>Экспорт геометрии в xml для последующей отрисовки в AutoCAD</summary>
    public static class ExportGeometryToXml
    {
        private const string _folderName = "E:\\Test\\ExteriorPlanDimensions";

        public static void ExportSolidWithPlanarFaces(Solid solid, string header)
        {
            List<PlanarFace> faces = new List<PlanarFace>();
            foreach (var solidFace in solid.Faces)
            {
                if (solidFace is PlanarFace face)
                {
                    faces.Add(face);
                }
            }
            if (faces.Any())
                ExportFaces(faces, header);
        }
        public static void ExportSolidByFaces(Solid solid, string header)
        {
            List<Face> faces = new List<Face>();
            foreach (Face solidFace in solid.Faces)
            {
                    faces.Add(solidFace);
            }
            if(faces.Any())
                ExportFaces(faces, header);
        }
        public static void ExportSolidByEdgesAsLine(Solid solid, string header)
        {
            List<Line> edges = new List<Line>();
            foreach (Edge edge in solid.Edges)
            {
                if(edge.AsCurve() is Line line)
                    edges.Add(line);
            }
            if (edges.Any())
                ExportLines(edges, header);
        }
        public static void ExportAdvancedFaces(List<AdvancedPlanarFace> faces, string header)
        {
            List<Curve> wallCurves = new List<Curve>();

            foreach (AdvancedPlanarFace face in faces)
            {
                foreach (var edge in face.Edges)
                {
                    wallCurves.Add(edge.AsCurve());
                }
            }
            ExportCurves(wallCurves, header);
        }
        public static void ExportFaces(List<PlanarFace> faces, string header)
        {
            List<Curve> wallCurves = new List<Curve>();

            foreach (PlanarFace face in faces)
            {
                EdgeArrayArray edgeArrayArray = face.EdgeLoops;
                foreach (EdgeArray edgeArray in edgeArrayArray)
                {
                    foreach (Edge edge in edgeArray)
                    {
                        wallCurves.Add(edge.AsCurve());
                    }
                }
            }
            ExportCurves(wallCurves, header);
        }
        public static void ExportFaces(List<Face> faces, string header)
        {
            List<Curve> wallCurves = new List<Curve>();

            foreach (Face face in faces)
            {
                EdgeArrayArray edgeArrayArray = face.EdgeLoops;
                foreach (EdgeArray edgeArray in edgeArrayArray)
                {
                    foreach (Edge edge in edgeArray)
                    {
                        wallCurves.Add(edge.AsCurve());
                    }
                }
            }
            ExportCurves(wallCurves, header);
        }
        public static void ExportAdvancedWallsEdges(List<AdvancedWall> advancedWalls, string header)
        {
            List<Curve> wallCurves = new List<Curve>();
            foreach (AdvancedWall wall in advancedWalls)
            {
                foreach (Edge edge in wall.Edges)
                {
                    wallCurves.Add(edge.AsCurve());
                }
            }
            ExportCurves(wallCurves, header);
        }

        public static void ExportAdvancedWallsFaces(List<AdvancedWall> advancedWalls, string header)
        {
            List<Curve> wallCurves = new List<Curve>();
            foreach (AdvancedWall wall in advancedWalls)
            {
                foreach (AdvancedPlanarFace face in wall.AdvancedPlanarFaces)
                {
                    foreach (var edge in face.Edges)
                    {
                        wallCurves.Add(edge.AsCurve());
                    }
                }
            }
            ExportCurves(wallCurves, header);
        }
        public static void ExportCurves(List<Curve> curves, string header)
        {
#if DEBUG
            if (!Directory.Exists(_folderName)) return;
            XElement root = new XElement("Curves");
            XElement linesRootXElement = new XElement("Lines");
            XElement arcsRootXElement = new XElement("Arcs");
            foreach (Curve curve in curves)
            {
                Line line = curve as Line;
                if (line != null)
                {
                    var lineXel = GetXElementFromLine(line);
                    if(lineXel != null)
                        linesRootXElement.Add(lineXel);
                }
                Arc arc = curve as Arc;
                if (arc != null)
                {
                    var arcXel = GetXElementFromArc(arc);
                    if(arcXel != null)
                        arcsRootXElement.Add(arcXel);
                }
            }
            if (linesRootXElement.HasElements) root.Add(linesRootXElement);
            if (arcsRootXElement.HasElements) root.Add(arcsRootXElement);

            root.Save(Path.Combine(_folderName, DateTime.Now.Minute + "_" + DateTime.Now.Second + "_" + DateTime.Now.Millisecond + "_" + header + ".xml"));
#endif
        }
        public static void ExportLines(List<Line> lines, string header)
        {
#if DEBUG
            if (!Directory.Exists(_folderName)) return;

            XElement rootXElement = new XElement("Lines");
            foreach (Line line in lines)
            {
                rootXElement.Add(GetXElementFromLine(line));
            }
            rootXElement.Save(Path.Combine(_folderName, DateTime.Now.Minute + "_" + DateTime.Now.Second + "_" + DateTime.Now.Millisecond + "_" + header + ".xml"));
#endif
        }
        public static void ExportArcs(List<Arc> arcs, string header)
        {
#if DEBUG
            if (!Directory.Exists(_folderName)) return;

            XElement rootXElement = new XElement("Arcs");
            foreach (Arc arc in arcs)
            {
                rootXElement.Add(GetXElementFromArc(arc));
            }
            rootXElement.Save(Path.Combine(_folderName, DateTime.Now.Minute + "_" + DateTime.Now.Second + "_" + DateTime.Now.Millisecond + "_" + header + ".xml"));
#endif
        }
        public static void ExportPoints(List<XYZ> points, string header)
        {
#if DEBUG
            if (!Directory.Exists(_folderName)) return;

            XElement rootXElement = new XElement("Points");
            foreach (XYZ point in points)
            {
                rootXElement.Add(GetXElementFromPoint(point));
            }
            rootXElement.Save(Path.Combine(_folderName, DateTime.Now.Minute + "_" + DateTime.Now.Second + "_" + DateTime.Now.Millisecond + "_" + header + ".xml"));
#endif
        }
        private static XElement GetXElementFromLine(Line line)
        {
            try
            {
                XElement lineXElement = new XElement("Line");
                XElement startPointXElement = new XElement("StartPoint");
                startPointXElement.SetAttributeValue("X", line.GetEndPoint(0).X);
                startPointXElement.SetAttributeValue("Y", line.GetEndPoint(0).Y);
                startPointXElement.SetAttributeValue("Z", line.GetEndPoint(0).Z);
                lineXElement.Add(startPointXElement);
                XElement endPointXElement = new XElement("EndPoint");
                endPointXElement.SetAttributeValue("X", line.GetEndPoint(1).X);
                endPointXElement.SetAttributeValue("Y", line.GetEndPoint(1).Y);
                endPointXElement.SetAttributeValue("Z", line.GetEndPoint(1).Z);
                lineXElement.Add(endPointXElement);
                return lineXElement;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static XElement GetXElementFromArc(Arc arc)
        {
            try
            {
                XElement arcXElement = new XElement("Arc");
                XElement element = new XElement("StartPoint");
                element.SetAttributeValue("X", arc.GetEndPoint(0).X);
                element.SetAttributeValue("Y", arc.GetEndPoint(0).Y);
                element.SetAttributeValue("Z", arc.GetEndPoint(0).Z);
                arcXElement.Add(element);
                element = new XElement("EndPoint");
                element.SetAttributeValue("X", arc.GetEndPoint(1).X);
                element.SetAttributeValue("Y", arc.GetEndPoint(1).Y);
                element.SetAttributeValue("Z", arc.GetEndPoint(1).Z);
                arcXElement.Add(element);
                element = new XElement("PointOnArc");
                element.SetAttributeValue("X", arc.Tessellate()[1].X);
                element.SetAttributeValue("Y", arc.Tessellate()[1].Y);
                element.SetAttributeValue("Z", arc.Tessellate()[1].Z);
                arcXElement.Add(element);
                return arcXElement;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static XElement GetXElementFromPoint(XYZ point)
        {
            XElement pointXElement = new XElement("Point");
            pointXElement.SetAttributeValue("X", point.X);
            pointXElement.SetAttributeValue("Y", point.Y);
            pointXElement.SetAttributeValue("Z", point.Z);
            return pointXElement;
        }
    }
}
