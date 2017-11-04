using System;
using Autodesk.AutoCAD.Runtime;
using System.Xml.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace PlanDimensionsAutocadHelper
{
    public class CreateFromXml
    {
        [CommandMethod("PlanDimensions")]
        public void Create()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            XElement fileXElement = XElement.Load(ofd.FileName);
            Table table = new Table();
            var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;
            using (var tr = doc.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                // from root
                CreateLines(fileXElement, tr, btr);
                CreateArcs(fileXElement, tr, btr);
                CreatePoints(fileXElement, tr, btr);
                // all in one
                XElement lines = fileXElement.Element("Lines");
                if (lines != null)                
                    CreateLines(lines, tr, btr);
                XElement arcs = fileXElement.Element("Arcs");
                if (arcs != null)
                    CreateArcs(arcs, tr, btr);
                XElement points = fileXElement.Element("Points");
                if (points != null) CreatePoints(points, tr, btr);

                tr.Commit();
            }
        }
        private void CreateLines(XElement root, Transaction tr, BlockTableRecord btr)
        {
            foreach (XElement curveXelement in root.Elements("Line"))
            {
                XElement startPointXElement = curveXelement.Element("StartPoint");
                Point3d startPoint = new Point3d(
                    Convert.ToDouble(startPointXElement?.Attribute("X")?.Value),
                    Convert.ToDouble(startPointXElement?.Attribute("Y")?.Value),
                    Convert.ToDouble(startPointXElement?.Attribute("Z")?.Value));
                XElement endPointXElement = curveXelement.Element("EndPoint");
                Point3d endPoint = new Point3d(
                    Convert.ToDouble(endPointXElement?.Attribute("X")?.Value),
                    Convert.ToDouble(endPointXElement?.Attribute("Y")?.Value),
                    Convert.ToDouble(endPointXElement?.Attribute("Z")?.Value));
                using (Line line = new Line(startPoint, endPoint))
                {
                    btr.AppendEntity(line);
                    tr.AddNewlyCreatedDBObject(line, true);
                }
            }
        }
        private void CreateArcs(XElement root, Transaction tr, BlockTableRecord btr)
        {
            foreach (XElement curveXelement in root.Elements("Arc"))
            {
                double radius = Convert.ToDouble(curveXelement.Attribute("Radius").Value);

                XElement startPointXElement = curveXelement.Element("StartPoint");
                Point3d startPoint = new Point3d(
                    Convert.ToDouble(startPointXElement?.Attribute("X")?.Value),
                    Convert.ToDouble(startPointXElement?.Attribute("Y")?.Value),
                    Convert.ToDouble(startPointXElement?.Attribute("Z")?.Value));
                XElement endPointXElement = curveXelement.Element("EndPoint");
                Point3d endPoint = new Point3d(
                    Convert.ToDouble(endPointXElement?.Attribute("X")?.Value),
                    Convert.ToDouble(endPointXElement?.Attribute("Y")?.Value),
                    Convert.ToDouble(endPointXElement?.Attribute("Z")?.Value));
                XElement centerPointXElement = curveXelement.Element("Center");
                Point3d centerPoint = new Point3d(
                    Convert.ToDouble(centerPointXElement?.Attribute("X")?.Value),
                    Convert.ToDouble(centerPointXElement?.Attribute("Y")?.Value),
                    Convert.ToDouble(centerPointXElement?.Attribute("Z")?.Value)
                    );
                XElement pointOnArcXElement = curveXelement.Element("PointOnArc");
                Point3d pointOnArc = new Point3d(
                    Convert.ToDouble(pointOnArcXElement?.Attribute("X")?.Value),
                    Convert.ToDouble(pointOnArcXElement?.Attribute("Y")?.Value),
                    Convert.ToDouble(pointOnArcXElement?.Attribute("Z")?.Value)
                );
                // create a CircularArc3d
                CircularArc3d carc = new CircularArc3d(startPoint, pointOnArc, endPoint);

                // now convert the CircularArc3d to an Arc
                Point3d cpt = carc.Center;
                Vector3d normal = carc.Normal;
                Vector3d refVec = carc.ReferenceVector;
                Plane plan = new Plane(cpt, normal);
                double ang = refVec.AngleOnPlane(plan);
                using (Arc arc = new Arc(cpt, normal, carc.Radius, carc.StartAngle + ang, carc.EndAngle + ang))
                {
                    btr.AppendEntity(arc);
                    tr.AddNewlyCreatedDBObject(arc, true);
                }
                // dispose CircularArc3d
                carc.Dispose();
            }
        }
        private void CreatePoints(XElement root, Transaction tr, BlockTableRecord btr)
        {
            foreach (var xElement in root.Elements("Point"))
            {
                Point3d startPoint = new Point3d(
                    Convert.ToDouble(xElement?.Attribute("X")?.Value),
                    Convert.ToDouble(xElement?.Attribute("Y")?.Value),
                    Convert.ToDouble(xElement?.Attribute("Z")?.Value));
                DBPoint dbPoint = new DBPoint(startPoint);
                btr.AppendEntity(dbPoint);
                tr.AddNewlyCreatedDBObject(dbPoint, true);
            }
        }
    }
}
