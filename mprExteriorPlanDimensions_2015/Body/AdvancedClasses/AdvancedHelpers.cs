namespace mprExteriorPlanDimensions.Body.AdvancedClasses
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;
    using Enumerators;
    using ModPlusAPI;

    public static class AdvancedHelpers
    {

        /// <summary>Фильтр стен по пересечению секущей плоскостью секущего диапазона</summary>
        /// <param name="advancedWalls"></param>
        /// <param name="doc"></param>
        public static void FilterByCutPlan(List<AdvancedWall> advancedWalls, Document doc)
        {
            var checkedZ = GeometryHelpers.GetViewPlanCutPlaneElevation((ViewPlan)doc.ActiveView, doc);
            for (int i = advancedWalls.Count - 1; i >= 0; i--)
            {
                if (checkedZ < advancedWalls[i].GetMinZ() || checkedZ > advancedWalls[i].GetMaxZ())
                {
                    advancedWalls.RemoveAt(i);
                }
            }
        }
       
        /// <summary>Фильтр стен по допустимой толщине, указанной в настройках</summary>
        /// <param name="advancedWalls"></param>
        public static void FilterByWallWidth(List<AdvancedWall> advancedWalls)
        {
            var minWallWidth = int.TryParse(
                UserConfigFile.GetValue(
                    UserConfigFile.ConfigFileZone.Settings, "mprExteriorPlanDimensions", "MinWallWidth"), out var m)
                ? m
                : 50;
            for (var i = advancedWalls.Count - 1; i >= 0; i--)
            {
                // Пропускаю витражи, т.к. их толщина мала, но работать с ними нужно
                if (advancedWalls[i].Wall.CurtainGrid != null)
                {
                    continue;
                }

                if (advancedWalls[i].Wall.Width * 304.8 < minWallWidth)
                {
                    advancedWalls.RemoveAt(i);
                }
            }
        }
        
        public static void FindExtremes(
            List<AdvancedWall> walls,
            Document doc,
            out List<AdvancedWall> leftExtreme,
            out List<AdvancedWall> rightExtreme,
            out List<AdvancedWall> topExtreme,
            out List<AdvancedWall> bottomExtreme
        )
        {
            rightExtreme = new List<AdvancedWall>();
            leftExtreme = new List<AdvancedWall>();
            topExtreme = new List<AdvancedWall>();
            bottomExtreme = new List<AdvancedWall>();
            // Нахожу "внешние" стены
            var outerWalls = GetOuterWalls(walls);
            if (!outerWalls.Any())
            {
                return;
            }

            //ExportGeometryToXml.ExportAdvancedWallsFaces(outerWalls, "outer walls");

            for (var i = 0; i < outerWalls.Count; i++)
            {
                AdvancedWall checkedWall = outerWalls[i];
                if (checkedWall.Orientation == ElementOrientation.Vertical)
                {

                    var leftPerpendicularLine = Line.CreateBound(
                        new XYZ(checkedWall.MidPoint.X - 1000000, checkedWall.MidPoint.Y, 0.0),
                        new XYZ(checkedWall.MidPoint.X, checkedWall.MidPoint.Y, 0.0));
                    var rightPerpendicularLine = Line.CreateBound(
                        new XYZ(checkedWall.MidPoint.X, checkedWall.MidPoint.Y, 0.0),
                        new XYZ(checkedWall.MidPoint.X + 1000000, checkedWall.MidPoint.Y, 0.0));
                    var hasPerpendicularLeftIntersect = false;
                    var hasPerpendicularRightIntersect = false;
                    for (int j = 0; j < outerWalls.Count; j++)
                    {
                        if (i == j)
                        {
                            continue;
                        }

                        var checkedCurve = Line.CreateBound(
                            new XYZ(outerWalls[j].StartPoint.X, outerWalls[j].StartPoint.Y, 0.0),
                            new XYZ(outerWalls[j].EndPoint.X, outerWalls[j].EndPoint.Y, 0.0));
                        if (leftPerpendicularLine.IntersectTo(checkedCurve))
                        {
                            hasPerpendicularLeftIntersect = true;
                        }

                        if (rightPerpendicularLine.IntersectTo(checkedCurve))
                        {
                            hasPerpendicularRightIntersect = true;
                        }
                    }

                    if (hasPerpendicularRightIntersect && !hasPerpendicularLeftIntersect)
                    {
                        leftExtreme.Add(checkedWall);
                    }

                    if (hasPerpendicularLeftIntersect && !hasPerpendicularRightIntersect)
                    {
                        rightExtreme.Add(checkedWall);
                    }
                }

                if (checkedWall.Orientation == ElementOrientation.Horizontal)
                {
                    var topPerpendicularLine = Line.CreateBound(
                        new XYZ(checkedWall.MidPoint.X, checkedWall.MidPoint.Y + 1000000, 0.0),
                        new XYZ(checkedWall.MidPoint.X, checkedWall.MidPoint.Y, 0.0));
                    var bottomPerpendicularLine = Line.CreateBound(
                        new XYZ(checkedWall.MidPoint.X, checkedWall.MidPoint.Y, 0.0),
                        new XYZ(checkedWall.MidPoint.X, checkedWall.MidPoint.Y - 1000000, 0.0));
                    var hasPerpendicularTopIntersect = false;
                    var hasPerpendicularBottomIntersect = false;
                    for (int j = 0; j < outerWalls.Count; j++)
                    {
                        if (i == j)
                        {
                            continue;
                        }

                        var checkedCurve = Line.CreateBound(
                            new XYZ(outerWalls[j].StartPoint.X, outerWalls[j].StartPoint.Y, 0.0),
                            new XYZ(outerWalls[j].EndPoint.X, outerWalls[j].EndPoint.Y, 0.0));
                        if (topPerpendicularLine.IntersectTo(checkedCurve))
                        {
                            hasPerpendicularTopIntersect = true;
                        }

                        if (bottomPerpendicularLine.IntersectTo(checkedCurve))
                        {
                            hasPerpendicularBottomIntersect = true;
                        }
                    }

                    if (hasPerpendicularBottomIntersect && !hasPerpendicularTopIntersect)
                    {
                        topExtreme.Add(checkedWall);
                    }

                    if (hasPerpendicularTopIntersect && !hasPerpendicularBottomIntersect)
                    {
                        bottomExtreme.Add(checkedWall);
                    }
                }
            }

            foreach (AdvancedWall checkedAdvancedWall in outerWalls)
            {
                if (checkedAdvancedWall.Orientation == ElementOrientation.Vertical)
                {
                    foreach (AdvancedWall wall in topExtreme)
                    {
                        if (checkedAdvancedWall.IsAdjoinToByLocationCurveEnds(wall))
                        {
                            topExtreme.Add(checkedAdvancedWall);
                            break;
                        }
                    }

                    foreach (AdvancedWall wall in bottomExtreme)
                    {
                        if (checkedAdvancedWall.IsAdjoinToByLocationCurveEnds(wall))
                        {
                            bottomExtreme.Add(checkedAdvancedWall);
                            break;
                        }
                    }
                }
                else if (checkedAdvancedWall.Orientation == ElementOrientation.Horizontal)
                {
                    foreach (AdvancedWall wall in leftExtreme)
                    {
                        if (checkedAdvancedWall.IsAdjoinToByLocationCurveEnds(wall))
                        {
                            leftExtreme.Add(checkedAdvancedWall);
                            break;
                        }
                    }

                    foreach (AdvancedWall wall in rightExtreme)
                    {
                        if (checkedAdvancedWall.IsAdjoinToByLocationCurveEnds(wall))
                        {
                            rightExtreme.Add(checkedAdvancedWall);
                            break;
                        }
                    }
                }
            }
        }
       
        /// <summary>Поиск из стен только "внешних".
        /// Принцип поиска: из каждой стены (из середины) откладывается перпендикуляр. Если с одной стороны
        /// есть пересечения, а с другой нет, значит стена "внешняя"</summary>
        /// <param name="walls">Изначальный набор стен</param>
        /// <returns></returns>
        public static List<AdvancedWall> GetOuterWalls(IReadOnlyList<AdvancedWall> walls)
        {
            var outerWalls = new List<AdvancedWall>();

            for (int i = 0; i < walls.Count; i++)
            {
                var checkedWall = walls[i];
                if (checkedWall.Orientation == ElementOrientation.Vertical)
                {
                    var leftLine = Line.CreateBound(
                        new XYZ(checkedWall.MidPoint.X - 1000000, checkedWall.MidPoint.Y, 0.0),
                        new XYZ(checkedWall.MidPoint.X, checkedWall.MidPoint.Y, 0.0));
                    var rightLine = Line.CreateBound(
                        new XYZ(checkedWall.MidPoint.X, checkedWall.MidPoint.Y, 0.0),
                        new XYZ(checkedWall.MidPoint.X + 1000000, checkedWall.MidPoint.Y, 0.0));
                    var hasLeftIntersect = false;
                    var hasRightIntersect = false;
                    for (int j = 0; j < walls.Count; j++)
                    {
                        if (i == j)
                        {
                            continue;
                        }

                        if (walls[j] == null)
                        {
                            continue;
                        }

                        var checkedCurve = Line.CreateBound(
                            new XYZ(walls[j].StartPoint.X, walls[j].StartPoint.Y, 0.0),
                            new XYZ(walls[j].EndPoint.X, walls[j].EndPoint.Y, 0.0));
                        if (leftLine.IntersectTo(checkedCurve))
                        {
                            hasLeftIntersect = true;
                        }

                        if (rightLine.IntersectTo(checkedCurve))
                        {
                            hasRightIntersect = true;
                        }
                    }

                    if (hasLeftIntersect ^ hasRightIntersect)
                    {
                        outerWalls.Add(checkedWall);
                    }
                }

                if (checkedWall.Orientation == ElementOrientation.Horizontal)
                {
                    var topLine = Line.CreateBound(
                        new XYZ(checkedWall.MidPoint.X, checkedWall.MidPoint.Y + 1000000, 0.0),
                        new XYZ(checkedWall.MidPoint.X, checkedWall.MidPoint.Y, 0.0));
                    var bottomLine = Line.CreateBound(
                        new XYZ(checkedWall.MidPoint.X, checkedWall.MidPoint.Y, 0.0),
                        new XYZ(checkedWall.MidPoint.X, checkedWall.MidPoint.Y - 1000000, 0.0));
                    var hasTopIntersect = false;
                    var hasBottomIntersect = false;
                    for (int j = 0; j < walls.Count; j++)
                    {
                        if (i == j)
                        {
                            continue;
                        }

                        var checkedCurve = Line.CreateBound(
                            new XYZ(walls[j].StartPoint.X, walls[j].StartPoint.Y, 0.0),
                            new XYZ(walls[j].EndPoint.X, walls[j].EndPoint.Y, 0.0));
                        if (topLine.IntersectTo(checkedCurve))
                        {
                            hasTopIntersect = true;
                        }

                        if (bottomLine.IntersectTo(checkedCurve))
                        {
                            hasBottomIntersect = true;
                        }
                    }

                    if (hasTopIntersect ^ hasBottomIntersect)
                    {
                        outerWalls.Add(checkedWall);
                    }
                }
            }

            var ids = outerWalls.Select(wall => wall.Wall.Id.IntegerValue).ToList();
            // Этот вариант возьмет не все стены, поэтому делаю дополнительный цикл
            // Теперь ищу стены через LocationCurve.get_ElementsAtJoin
            var tempWalls = new List<AdvancedWall>();
            foreach (var checkedAdvancedWall in walls)
            {
                if (!ids.Contains(checkedAdvancedWall.Wall.Id.IntegerValue))
                {
                    var onFirstEnd = false;
                    var onSecondEnd = false;
                    foreach (AdvancedWall outerWall in outerWalls)
                    {
                        if (checkedAdvancedWall.IsAdjoinToByLocationCurveEnds(outerWall, 0) &&
                            outerWall.IsAdjoinToByLocationCurveEnds(checkedAdvancedWall))
                        {
                            onFirstEnd = true;
                        }

                        if (checkedAdvancedWall.IsAdjoinToByLocationCurveEnds(outerWall, 1) &&
                            outerWall.IsAdjoinToByLocationCurveEnds(checkedAdvancedWall))
                        {
                            onSecondEnd = true;
                        }
                    }

                    if (onFirstEnd && onSecondEnd)
                    {
                        tempWalls.Add(checkedAdvancedWall);
                    }
                }
            }

            outerWalls.AddRange(tempWalls);
            return outerWalls;
        }

        /// <summary>Проверка наличия стены в списке по ID</summary>
        public static bool HasWallInListById(IEnumerable<AdvancedWall> walls, AdvancedWall advancedWall)
        {
            foreach (var wall in walls)
            {
                if (wall.Wall.Id.IntegerValue.Equals(advancedWall.Wall.Id.IntegerValue))
                {
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>Проверка что текущая стена имеет на конце соединение с проверяемой стеной</summary>
        /// <param name="wall">Current wall</param>
        /// <param name="checkedWall">Checked wall</param>
        public static bool IsAdjoinToByLocationCurveEnds(this AdvancedWall wall, AdvancedWall checkedWall)
        {
            var joinIds = new List<int>();
            ElementArray elementsAtJoinAtStart = ((LocationCurve)wall.Wall.Location).get_ElementsAtJoin(0);
            ElementArray elementsAtJoinAtEnd = ((LocationCurve)wall.Wall.Location).get_ElementsAtJoin(1);
            if (!elementsAtJoinAtEnd.IsEmpty)
            {
                foreach (Element e in elementsAtJoinAtEnd)
                {
                    if (e is Wall && !wall.Wall.Id.IntegerValue.Equals(e.Id.IntegerValue))
                    {
                        joinIds.Add(e.Id.IntegerValue);
                    }
                }
            }

            if (!elementsAtJoinAtStart.IsEmpty)
            {
                foreach (Element e in elementsAtJoinAtStart)
                {
                    if (e is Wall && !wall.Wall.Id.IntegerValue.Equals(e.Id.IntegerValue))
                    {
                        joinIds.Add(e.Id.IntegerValue);
                    }
                }
            }

            return joinIds.Contains(checkedWall.Wall.Id.IntegerValue);
        }
        
        private static bool IsAdjoinToByLocationCurveEnds(this AdvancedWall wall, AdvancedWall checkedWall, int end)
        {
            var joinIds = new List<int>();
            ElementArray elementsAtJoin = ((LocationCurve)wall.Wall.Location).get_ElementsAtJoin(end);
            if (!elementsAtJoin.IsEmpty)
            {
                foreach (Element e in elementsAtJoin)
                {
                    if (e is Wall && !wall.Wall.Id.IntegerValue.Equals(e.Id.IntegerValue))
                    {
                        joinIds.Add(e.Id.IntegerValue);
                    }
                }
            }

            return joinIds.Contains(checkedWall.Wall.Id.IntegerValue);
        }

        /// <summary>Получение линии размера для цепочки в зависимости от внешнего направления</summary>
        /// <param name="doc"></param>
        /// <param name="sideWalls">Набор "боковых" стен</param>
        /// <param name="extremeWallVariant">Вариант направления</param>
        /// <param name="offset">Отступ от крайней стены</param>
        /// <returns></returns>
        public static Line GetDimensionLineForChain(Document doc,
            List<AdvancedWall> sideWalls, ExtremeWallVariant extremeWallVariant, double offset)
        {
            var cutPlanZ = GeometryHelpers.GetViewPlanCutPlaneElevation((ViewPlan)doc.ActiveView, doc);
            var points = new List<XYZ>();
            foreach (var wall in sideWalls)
            {
                points.Add(wall.EndPoint);
                points.Add(wall.StartPoint);
            }

            if (!points.Any())
            {
                return null;
            }

            switch (extremeWallVariant)
            {
                case ExtremeWallVariant.Right:
                    {
                        points.Sort((x, y) => x.X.CompareTo(y.X));
                        var maxX = points.Last().X;
                        points.Sort((x, y) => x.Y.CompareTo(y.Y));
                        var minY = points.First().Y;
                        var maxY = points.Last().Y;
                        return TryCreateBound(
                            new XYZ(maxX + offset, minY, cutPlanZ),
                            new XYZ(maxX + offset, maxY, cutPlanZ)
                        );
                    }

                case ExtremeWallVariant.Left:
                    {
                        points.Sort((x, y) => x.X.CompareTo(y.X));
                        var minX = points.First().X;
                        points.Sort((x, y) => x.Y.CompareTo(y.Y));
                        var minY = points.First().Y;
                        var maxY = points.Last().Y;
                        return TryCreateBound(
                            new XYZ(minX - offset, minY, cutPlanZ),
                            new XYZ(minX - offset, maxY, cutPlanZ));
                    }

                case ExtremeWallVariant.Top:
                    {
                        points.Sort((x, y) => x.X.CompareTo(y.X));
                        var minX = points.First().X;
                        var maxX = points.Last().X;
                        points.Sort((x, y) => x.Y.CompareTo(y.Y));
                        var maxY = points.Last().Y;
                        return TryCreateBound(
                            new XYZ(minX, maxY + offset, cutPlanZ),
                            new XYZ(maxX, maxY + offset, cutPlanZ)
                        );
                    }

                case ExtremeWallVariant.Bottom:
                    {
                        points.Sort((x, y) => x.X.CompareTo(y.X));
                        var minX = points.First().X;
                        var maxX = points.Last().X;
                        points.Sort((x, y) => x.Y.CompareTo(y.Y));
                        var minY = points.First().Y;
                        return TryCreateBound(
                            new XYZ(minX, minY - offset, cutPlanZ),
                            new XYZ(maxX, minY - offset, cutPlanZ)
                        );
                    }
            }

            return null;
        }
        
        public static Line TryCreateBound(XYZ pt1, XYZ pt2)
        {
            return pt1.IsAlmostEqualTo(pt2) ? null : Line.CreateBound(pt1, pt2);
        }
       
        /// <summary>Выбор стены из списка по Id</summary>
        /// <param name="walls"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static AdvancedWall GetAdvancedWallFromListById(IEnumerable<AdvancedWall> walls, int id)
        {
            foreach (AdvancedWall advancedWall in walls)
            {
                if (advancedWall.Wall.Id.IntegerValue.Equals(id))
                {
                    return advancedWall;
                }
            }

            return null;
        }

        /// <summary>Наибольшая толщина стены в списке</summary>
        /// <param name="walls"></param>
        /// <returns></returns>
        public static double GetMaxWallWidthFromList(IEnumerable<AdvancedWall> walls)
        {
            return walls.Select(w => w.Wall.Width).Max();
        }
    }
}
