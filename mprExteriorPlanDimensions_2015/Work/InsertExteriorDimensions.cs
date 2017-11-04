using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using mprExteriorPlanDimensions.Body;
using mprExteriorPlanDimensions.Body.AdvancedClasses;
using mprExteriorPlanDimensions.Body.Enumerators;
using mprExteriorPlanDimensions.Body.SelectionFilters;
using mprExteriorPlanDimensions.Configurations;
using ModPlusAPI;
using ModPlusAPI.Windows;

namespace mprExteriorPlanDimensions.Work
{
    public class InsertExteriorDimensions
    {
        private readonly ExteriorConfiguration _exteriorConfiguration;
        private readonly UIApplication _uiApplication;
        private readonly List<AdvancedWall> _advancedWalls;
        private readonly List<AdvancedGrid> _advancedGrids;
        private double _cutPlanZ;
        public InsertExteriorDimensions(ExteriorConfiguration configuration, UIApplication uiApplication)
        {
            _exteriorConfiguration = configuration;
            _uiApplication = uiApplication;
            _advancedGrids = new List<AdvancedGrid>();
            _advancedWalls = new List<AdvancedWall>();
        }
        /// <summary>Проставить внешние размеры</summary>
        public void DoWork()
        {
            Document doc = _uiApplication.ActiveUIDocument.Document;
            _cutPlanZ = GeometryHelpers.GetViewPlanCutPlaneElevation((ViewPlan)doc.ActiveView, doc);
            // select
            var selectedElements = SelectElements();
            if (selectedElements == null) return;
            // get list of advanced elements
            foreach (Element element in selectedElements)
            {
                switch (element)
                {
                    case Wall wall:
                        var advancedWall = new AdvancedWall(wall);
                        if (advancedWall.IsDefinded) _advancedWalls.Add(advancedWall);
                        break;
                    case Grid grid:
                        var advancedGrid = new AdvancedGrid(grid);
                        if (advancedGrid.IsDefinded) _advancedGrids.Add(advancedGrid);
                        break;
                }
            }
            if (!_advancedWalls.Any())
            {
                MessageBox.Show("Не удалось создать рабочий список стен!", MessageBoxIcon.Close);
                return;
            }
            // Фильтрую стены по толщине
            AdvancedHelpers.FilterByWallWidth(_advancedWalls);
            if (!_advancedWalls.Any())
            {
                MessageBox.Show("В указанном наборе нет стен, подходящих по минимально допустимой толщине!", MessageBoxIcon.Close);
                return;
            }
            // Фильтрую стены, оставляя которые пересекаются секущим диапазоном
            AdvancedHelpers.FilterByCutPlan(_advancedWalls, _uiApplication.ActiveUIDocument.Document);
            // Вдруг после этого не осталось стен!
            if (!_advancedWalls.Any())
            {
                MessageBox.Show(
                    "В указанном наборе нет стен, пересекаемых секущей плоскостью секущего диапазоном вида!", MessageBoxIcon.Close);
                return;
            }
            //ExportGeometryToXml.ExportAdvancedWallsFaces(_advancedWalls, "selected walls");
            //ExportGeometryToXml.ExportAdvancedWallsEdges(_advancedWalls, "selected by edges");
            // Поиск крайних групп стен
            // ReSharper disable RedundantAssignment
            //var rightExtreme = new List<AdvancedWall>();
            //var leftExtreme = new List<AdvancedWall>();
            //var topextreme = new List<AdvancedWall>();
            //var bottomExtreme = new List<AdvancedWall>();
            // ReSharper restore RedundantAssignment

            AdvancedHelpers.FindExtremes(_advancedWalls, doc, out var leftExtreme, out var rightExtreme, out var topextreme, out var bottomExtreme);
            //ExportGeometryToXml.ExportAdvancedWallsFaces(rightExtreme, "right extreme");
            //ExportGeometryToXml.ExportAdvancedWallsFaces(leftExtreme, "left extreme");
            //ExportGeometryToXml.ExportAdvancedWallsFaces(topextreme, "top extreme");
            //ExportGeometryToXml.ExportAdvancedWallsFaces(bottomExtreme, "bottom extreme");

            // create dimensions
            List<Dimension> createdDimensions = new List<Dimension>();
            if (_exteriorConfiguration.RightDimensions)
                createdDimensions.AddRange(CreateSideDimensions(rightExtreme, _advancedWalls, ExtremeWallVariant.Right));
            if (_exteriorConfiguration.LeftDimensions)
                createdDimensions.AddRange(CreateSideDimensions(leftExtreme, _advancedWalls, ExtremeWallVariant.Left));
            if (_exteriorConfiguration.TopDimensions)
                createdDimensions.AddRange(CreateSideDimensions(topextreme, _advancedWalls, ExtremeWallVariant.Top));
            if (_exteriorConfiguration.BottomDimensions)
                createdDimensions.AddRange(CreateSideDimensions(bottomExtreme, _advancedWalls, ExtremeWallVariant.Bottom));
        }

        /// <summary>Выбор пользователем элементов (стены и оси)</summary>
        /// <returns></returns>
        private IList<Element> SelectElements()
        {
            try
            {
                var selection = _uiApplication.ActiveUIDocument.Selection;
                while (true)
                {
                    var result = selection.PickElementsByRectangle(new WallAndGridsFilter(), "Выберите стены и оси");
                    if (result.Count <= 1)
                        MessageBox.Show("Нужно выбрать больше одного элемента!", MessageBoxIcon.Alert);
                    else return result;
                }
            }
            catch
            {
                return null;
            }
        }

        private List<Dimension> CreateSideDimensions(List<AdvancedWall> sideAdvancedWalls, List<AdvancedWall> allWalls, ExtremeWallVariant extremeWallVariant)
        {
            var createdDimensions = new List<Dimension>();
            var doc = _uiApplication.ActiveUIDocument.Document;

            // Сумма длин отступов от крайней стены
            int chainOffsetSumm = 0;
            // Делаю цикл по цепочкам
            foreach (ExteriorDimensionChain chain in _exteriorConfiguration.Chains)
            {
                // Суммирую отступы
                chainOffsetSumm += chain.ElementOffset;
                // Получаю линию для построения размера с учетом масштаба
                Line chainDimensionLine = AdvancedHelpers.GetDimensionLineForChain(
                    doc, sideAdvancedWalls, extremeWallVariant,
                    chainOffsetSumm * 0.00328084 * doc.ActiveView.Scale);

                if (chainDimensionLine == null)
                {
                    MessageBox.Show("Не удалось создать размерную линию!", MessageBoxIcon.Close);
                    continue;
                }


                // Крайние оси
                if (chain.ExtremeGrids)
                    createdDimensions.Add(CreateDimensionByExtremeGrids(doc, chainDimensionLine, extremeWallVariant));
                else if (chain.Overall)
                    createdDimensions.Add(CreateDimensionByOverallWalls(doc, chainDimensionLine, sideAdvancedWalls, extremeWallVariant));
                else // Так как вариант "крайние оси" или "габарит" перекрывает все остальные
                {
                    ReferenceArray referenceArray = new ReferenceArray();
                    // Собираю референсы для стен в зависимости от настроек цепочки
                    if (chain.Walls)
                        GetWallsReferences(sideAdvancedWalls, allWalls, extremeWallVariant, chain, ref referenceArray);
                    // from grids
                    if (chain.Grids)
                        GetGridsReferences(extremeWallVariant, ref referenceArray);

                    if (!referenceArray.IsEmpty)
                        using (var transaction = new Transaction(doc, "mprExteriorPlanDimensions"))
                        {
                            transaction.Start();
                            var dimension = doc.Create.NewDimension(doc.ActiveView, chainDimensionLine, referenceArray);
                            if (dimension != null)
                                createdDimensions.Add(dimension);
                            transaction.Commit();
                        }
                }
            }
            return createdDimensions;
        }

        /// <summary>Создание размеров для крайних осей</summary>
        /// <param name="doc"></param>
        /// <param name="chainDimensionLine"></param>
        /// <param name="extremeWallVariant"></param>
        private Dimension CreateDimensionByExtremeGrids(Document doc, Line chainDimensionLine, ExtremeWallVariant extremeWallVariant)
        {
            if (!_advancedGrids.Any()) return null;
            Dimension returnedDimension = null;
            ReferenceArray referenceArray = new ReferenceArray();
            Options opt = new Options
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = true,
                View = _uiApplication.ActiveUIDocument.Document.ActiveView
            };
            // Нужно получить референсы крайних осей в зависимости от направления
            if (extremeWallVariant == ExtremeWallVariant.Left || extremeWallVariant == ExtremeWallVariant.Right)
            {
                // Беру горизонтальные оси
                List<AdvancedGrid> verticalGrids = _advancedGrids.Where(g => g.Orientation == ElementOrientation.Horizontal).ToList();
                // Сортирую по Y
                verticalGrids.Sort((g1, g2) => g1.StartPoint.Y.CompareTo(g2.StartPoint.Y));
                var grids = new List<AdvancedGrid> { verticalGrids.First(), verticalGrids.Last() };
                foreach (AdvancedGrid grid in grids)
                {
                    foreach (GeometryObject o in grid.Grid.get_Geometry(opt))
                    {
                        var line = o as Line;
                        if (line != null)
                            referenceArray.Append(line.Reference);
                    }
                }
            }
            else // Иначе верх/низ
            {
                List<AdvancedGrid> horizontalGrids = _advancedGrids.Where(g => g.Orientation == ElementOrientation.Vertical).ToList();
                horizontalGrids.Sort((g1, g2) => g1.StartPoint.X.CompareTo(g2.StartPoint.X));
                var grids = new List<AdvancedGrid> { horizontalGrids.First(), horizontalGrids.Last() };
                foreach (AdvancedGrid grid in grids)
                {
                    foreach (GeometryObject o in grid.Grid.get_Geometry(opt))
                    {
                        var line = o as Line;
                        if (line != null)
                            referenceArray.Append(line.Reference);
                    }
                }
            }
            if (!referenceArray.IsEmpty)
                using (var transaction = new Transaction(doc, "mprExteriorPlanDimensions"))
                {
                    transaction.Start();
                    returnedDimension = doc.Create.NewDimension(doc.ActiveView, chainDimensionLine, referenceArray);
                    transaction.Commit();
                }
            return returnedDimension;
        }

        private void GetGridsReferences(ExtremeWallVariant extremeWallVariant, ref ReferenceArray referenceArray)
        {
            List<AdvancedGrid> verticalGrids = _advancedGrids.Where(g => g.Orientation == ElementOrientation.Vertical).ToList();
            List<AdvancedGrid> horizontalGrids = _advancedGrids.Where(g => g.Orientation == ElementOrientation.Horizontal).ToList();
            Options opt = new Options
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = true,
                View = _uiApplication.ActiveUIDocument.Document.ActiveView
            };
            // Так как оси не нужно проверять на совпадение, то сразу добавляю их в массив
            if (extremeWallVariant == ExtremeWallVariant.Right || extremeWallVariant == ExtremeWallVariant.Left)
                foreach (var grid in horizontalGrids)
                {
                    foreach (GeometryObject o in grid.Grid.get_Geometry(opt))
                    {
                        var line = o as Line;
                        if (line != null)
                            referenceArray.Append(line.Reference);
                    }
                }

            if (extremeWallVariant == ExtremeWallVariant.Bottom || extremeWallVariant == ExtremeWallVariant.Top)
                foreach (var grid in verticalGrids)
                {
                    foreach (GeometryObject o in grid.Grid.get_Geometry(opt))
                    {
                        var line = o as Line;
                        if (line != null)
                            referenceArray.Append(line.Reference);
                    }
                }
        }

        /// <summary>Получение референсов в группе стен по условиям</summary>
        /// <param name="sideWalls">Группа "боковых" стен</param>
        /// <param name="allWalls">Все стены</param>
        /// <param name="extremeWallVariant">Вариант направления простановки размеров - лево/право/верх/низ</param>
        /// <param name="chain">Настройки цепочки</param>
        /// <param name="referenceArray">Массив референсов для заполнения</param>
        private void GetWallsReferences(
            List<AdvancedWall> sideWalls,
            List<AdvancedWall> allWalls,
            ExtremeWallVariant extremeWallVariant,
            ExteriorDimensionChain chain, ref ReferenceArray referenceArray)
        {
            List<AdvancedPlanarFace> faces = new List<AdvancedPlanarFace>();
            List<AdvancedWall> verticalWalls = sideWalls.Where(w => w.Orientation == ElementOrientation.Vertical).ToList();
            List<AdvancedWall> horizontalWalls = sideWalls.Where(w => w.Orientation == ElementOrientation.Horizontal).ToList();
            // Если вдруг нет стен
            if (!verticalWalls.Any() && !horizontalWalls.Any()) return;
            // Варианты "проемы" и "пересекающиеся стены" могут быть только в случае включения в цепочку стен!

            // Если не стоит вариант "пересекающиеся стены", то нужно искать нужные референсы
            if (!chain.IntersectingWalls)
            {
                if (extremeWallVariant == ExtremeWallVariant.Right || extremeWallVariant == ExtremeWallVariant.Left)
                {
                    foreach (var wall in sideWalls)
                    {
                        List<AdvancedPlanarFace> horizontalTempFaces = new List<AdvancedPlanarFace>();
                        foreach (var face in wall.AdvancedPlanarFaces)
                        {
                            if (face.IsHorizontal)
                                horizontalTempFaces.Add(face);
                        }
                        horizontalTempFaces.Sort((f1, f2) => f1.PlanarFace.Origin.Y.CompareTo(f2.PlanarFace.Origin.Y));
                        faces.Add(horizontalTempFaces.First());
                        faces.Add(horizontalTempFaces.Last());
                    }
                }
                if (extremeWallVariant == ExtremeWallVariant.Top || extremeWallVariant == ExtremeWallVariant.Bottom)
                {
                    foreach (var wall in sideWalls)
                    {
                        List<AdvancedPlanarFace> verticalTempFaces = new List<AdvancedPlanarFace>();
                        foreach (var face in wall.AdvancedPlanarFaces)
                        {
                            if (face.IsVertical)
                                verticalTempFaces.Add(face);
                        }
                        verticalTempFaces.Sort((f1, f2) => f1.PlanarFace.Origin.X.CompareTo(f2.PlanarFace.Origin.X));
                        faces.Add(verticalTempFaces.First());
                        faces.Add(verticalTempFaces.Last());
                    }
                }
                //ExportGeometryToXml.ExportAdvancedFaces(faces, "wall faces");
            }
            else // Иначе достаточно найти все нужные референсы стен, лежащих в перпендикулярном направлении
            {
                if (extremeWallVariant == ExtremeWallVariant.Right || extremeWallVariant == ExtremeWallVariant.Left)
                {
                    foreach (AdvancedWall wall in horizontalWalls)
                    {
                        var wallNeededFaces = new List<AdvancedPlanarFace>();
                        foreach (var face in wall.AdvancedPlanarFaces)
                        {
                            if (face.IsHorizontal) wallNeededFaces.Add(face);
                        }
                        // Теперь сортирую по Y и беру первый и последний
                        wallNeededFaces.Sort((f1, f2) => f1.PlanarFace.Origin.Y.CompareTo(f2.PlanarFace.Origin.Y));
                        faces.Add(wallNeededFaces.First());
                        faces.Add(wallNeededFaces.Last());
                    }
                    var iw = FindIntersectionWalls(sideWalls, allWalls)
                        .Where(w => w.Orientation == ElementOrientation.Horizontal).ToList();
                    foreach (AdvancedWall wall in iw)
                    {
                        var wallNeededFaces = new List<AdvancedPlanarFace>();
                        foreach (var face in wall.AdvancedPlanarFaces)
                        {
                            if (face.IsHorizontal) wallNeededFaces.Add(face);
                        }
                        // Теперь сортирую по Y и беру первый и последний
                        wallNeededFaces.Sort((f1, f2) => f1.PlanarFace.Origin.Y.CompareTo(f2.PlanarFace.Origin.Y));
                        faces.Add(wallNeededFaces.First());
                        faces.Add(wallNeededFaces.Last());
                    }
                }
                if (extremeWallVariant == ExtremeWallVariant.Bottom || extremeWallVariant == ExtremeWallVariant.Top)
                {
                    foreach (AdvancedWall wall in verticalWalls)
                    {
                        var wallNeededFaces = new List<AdvancedPlanarFace>();
                        foreach (var face in wall.AdvancedPlanarFaces)
                        {
                            if (face.IsVertical)
                                wallNeededFaces.Add(face);
                        }
                        // Теперь сортирую по X и беру первый и последний
                        wallNeededFaces.Sort((f1, f2) => f1.PlanarFace.Origin.X.CompareTo(f2.PlanarFace.Origin.X));
                        faces.Add(wallNeededFaces.First());
                        faces.Add(wallNeededFaces.Last());
                    }
                    var iw = FindIntersectionWalls(sideWalls, allWalls)
                        .Where(w => w.Orientation == ElementOrientation.Vertical).ToList();
                    foreach (AdvancedWall wall in iw)
                    {
                        var wallNeededFaces = new List<AdvancedPlanarFace>();
                        foreach (var face in wall.AdvancedPlanarFaces)
                        {
                            if (face.IsVertical) wallNeededFaces.Add(face);
                        }
                        // Теперь сортирую по Y и беру первый и последний
                        wallNeededFaces.Sort((f1, f2) => f1.PlanarFace.Origin.Y.CompareTo(f2.PlanarFace.Origin.Y));
                        faces.Add(wallNeededFaces.First());
                        faces.Add(wallNeededFaces.Last());
                    }
                }
            }
            // если проемы
            if (chain.Openings)
            {
                if (extremeWallVariant == ExtremeWallVariant.Right || extremeWallVariant == ExtremeWallVariant.Left)
                    foreach (var wall in verticalWalls)
                        foreach (var face in wall.AdvancedPlanarFaces)
                            if (face.IsHorizontal)
                            {
                                faces.Add(face);
                                //referenceArray.Append(face.Reference);
                            }
                if (extremeWallVariant == ExtremeWallVariant.Bottom || extremeWallVariant == ExtremeWallVariant.Top)
                    foreach (var wall in horizontalWalls)
                        foreach (var face in wall.AdvancedPlanarFaces)
                            if (face.IsVertical)
                            {
                                //referenceArray.Append(face.Reference);
                                faces.Add(face);
                            }
            }
            // filtered
            var filteredFaces = FilterFaces(extremeWallVariant, sideWalls, faces);
            foreach (var face in filteredFaces)
            {
                referenceArray.Append(face.PlanarFace.Reference);
            }
        }

        private Dimension CreateDimensionByOverallWalls(
            Document doc, Line chainDimensionLine,
            List<AdvancedWall> sideWalls,
            ExtremeWallVariant extremeWallVariant)
        {
            Dimension returnedDimension = null;
            List<AdvancedWall> verticalWalls = sideWalls.Where(w => w.Orientation == ElementOrientation.Vertical).ToList();
            List<AdvancedWall> horizontalWalls = sideWalls.Where(w => w.Orientation == ElementOrientation.Horizontal).ToList();
            // Если вдруг нет стен
            if (!verticalWalls.Any() && !horizontalWalls.Any()) return null;
            ReferenceArray referenceArray = new ReferenceArray();
            // То же самое, что и получить из стен, только взять крайние референсы
            if (extremeWallVariant == ExtremeWallVariant.Right || extremeWallVariant == ExtremeWallVariant.Left)
            {
                var faces = new List<AdvancedPlanarFace>();
                // для каждой вертикальной стены нахожу соприкасающиеся горизонтальные стены
                foreach (AdvancedWall verticalWall in verticalWalls)
                {
                    // Добавляю в список все фейсы самой стены
                    foreach (var face in verticalWall.AdvancedPlanarFaces)
                    {
                        if (face.IsHorizontal) faces.Add(face);
                    }
                    ElementArray adjoinElements1 = ((LocationCurve)verticalWall.Wall.Location).get_ElementsAtJoin(0);
                    ElementArray adjoinElements2 = ((LocationCurve)verticalWall.Wall.Location).get_ElementsAtJoin(1);
                    var adjoinWalls = new List<AdvancedWall>();
                    foreach (Element element in adjoinElements1)
                    {
                        var w = AdvancedHelpers.GetAdvancedWallFromListById(horizontalWalls, element.Id.IntegerValue);
                        if (w != null) adjoinWalls.Add(w);
                    }
                    foreach (Element element in adjoinElements2)
                    {
                        var w = AdvancedHelpers.GetAdvancedWallFromListById(horizontalWalls, element.Id.IntegerValue);
                        if (w != null) adjoinWalls.Add(w);
                    }
                    // добавляю все фейсы соприкасающихся стен
                    foreach (var wall in adjoinWalls)
                    {
                        foreach (var face in wall.AdvancedPlanarFaces)
                        {
                            if (face.IsHorizontal) faces.Add(face);
                        }
                    }
                }
                faces.Sort((f1, f2) => f1.MinY.CompareTo(f2.MinY));
                referenceArray.Append(faces.First().PlanarFace.Reference);
                referenceArray.Append(faces.Last().PlanarFace.Reference);
            }
            if (extremeWallVariant == ExtremeWallVariant.Top || extremeWallVariant == ExtremeWallVariant.Bottom)
            {
                var faces = new List<AdvancedPlanarFace>();
                // для каждой вертикальной стены нахожу соприкасающиеся горизонтальные стены
                foreach (AdvancedWall horizontalWall in horizontalWalls)
                {
                    // Добавляю в список все фейсы самой стены
                    foreach (var face in horizontalWall.AdvancedPlanarFaces)
                    {
                        if (face.IsVertical) faces.Add(face);
                    }
                    ElementArray adjoinElements1 = ((LocationCurve)horizontalWall.Wall.Location).get_ElementsAtJoin(0);
                    ElementArray adjoinElements2 = ((LocationCurve)horizontalWall.Wall.Location).get_ElementsAtJoin(1);
                    var adjoinWalls = new List<AdvancedWall>();
                    foreach (Element element in adjoinElements1)
                    {
                        var w = AdvancedHelpers.GetAdvancedWallFromListById(verticalWalls, element.Id.IntegerValue);
                        if (w != null) adjoinWalls.Add(w);
                    }
                    foreach (Element element in adjoinElements2)
                    {
                        var w = AdvancedHelpers.GetAdvancedWallFromListById(verticalWalls, element.Id.IntegerValue);
                        if (w != null) adjoinWalls.Add(w);
                    }
                    // добавляю все фейсы соприкасающихся стен
                    foreach (var wall in adjoinWalls)
                    {
                        foreach (var face in wall.AdvancedPlanarFaces)
                        {
                            if (face.IsVertical) faces.Add(face);
                        }
                    }
                }
                faces.Sort((f1, f2) => f1.MinX.CompareTo(f2.MinX));
                referenceArray.Append(faces.First().PlanarFace.Reference);
                referenceArray.Append(faces.Last().PlanarFace.Reference);
            }
            if (!referenceArray.IsEmpty)
                using (var transaction = new Transaction(doc, "mprExteriorPlanDimensions"))
                {
                    transaction.Start();
                    returnedDimension = doc.Create.NewDimension(doc.ActiveView, chainDimensionLine, referenceArray);
                    transaction.Commit();
                }
            return returnedDimension;
        }

        /// <summary>Фильтрация face'ов по условиям</summary>
        /// <returns></returns>
        private List<AdvancedPlanarFace> FilterFaces(ExtremeWallVariant extremeWallVariant,
            List<AdvancedWall> sideWalls, List<AdvancedPlanarFace> selectedFaces)
        {
            //ExportGeometryToXml.ExportAdvancedFaces(selectedFaces, extremeWallVariant + "_ before filter");
            var tolerance = 0.0001;
            // Нужно два списка так как разные фильтрации
            List<AdvancedPlanarFace> faces = new List<AdvancedPlanarFace>();

            // Удаляю фейсы, не пересекаемые секущей плоскостью
            foreach (var face in selectedFaces)
            {
                if (face.MinZ <= _cutPlanZ && face.MaxZ >= _cutPlanZ)
                    faces.Add(face);
            }
            //ExportGeometryToXml.ExportAdvancedFaces(faces, extremeWallVariant + "_ after remove by cut");

            // Нужно удалить фейсы, которые совпадают по направлению
            List<AdvancedPlanarFace> returnedFaces = new List<AdvancedPlanarFace>();

            bool hasFaces;
            do
            {
                hasFaces = faces.Any(f => f != null);
                for (int i = 0; i < faces.Count; i++)
                {
                    var face = faces[i];
                    if (face != null)
                    {
                        returnedFaces.Add(face);
                        for (var j = 0; j < faces.Count; j++)
                        {
                            if (i == j) continue;
                            if (faces[j] == null) continue;

                            if (extremeWallVariant == ExtremeWallVariant.Left ||
                                extremeWallVariant == ExtremeWallVariant.Right)
                            {
                                // same Y
                                if (Math.Abs(face.PlanarFace.Origin.Y - faces[j].PlanarFace.Origin.Y) < tolerance)
                                    faces[j] = null;
                            }
                            else if (extremeWallVariant == ExtremeWallVariant.Top ||
                                     extremeWallVariant == ExtremeWallVariant.Bottom)
                            {
                                // same X
                                if (Math.Abs(face.PlanarFace.Origin.X - faces[j].PlanarFace.Origin.X) < tolerance)
                                    faces[j] = null;
                            }
                        }
                        faces[i] = null;
                    }
                }

            } while (hasFaces);

            //ExportGeometryToXml.ExportAdvancedFaces(returnedFaces, extremeWallVariant + "_ after remove colins");

            // Удаление по глубине проецирования
            // Глубину проецирования беру по наибольшей толщине стен в списке
            var depth = AdvancedHelpers.GetMaxWallWidthFromList(sideWalls) * 2;

            if (extremeWallVariant == ExtremeWallVariant.Bottom)
            {

                foreach (AdvancedWall wall in sideWalls.Where(w => w.Orientation == ElementOrientation.Horizontal))
                {
                    for (int i = returnedFaces.Count - 1; i >= 0; i--)
                    {
                        var face = returnedFaces[i];
                        if (face.MinX > wall.GetMinX() - wall.Wall.Width &&
                            face.MinX < wall.GetMaxX() + wall.Wall.Width)
                        {
                            if (face.MinY > wall.GetMinY() + depth)
                                returnedFaces.RemoveAt(i);
                        }
                    }
                }
            }
            if (extremeWallVariant == ExtremeWallVariant.Top)
            {

                foreach (AdvancedWall wall in sideWalls.Where(w => w.Orientation == ElementOrientation.Horizontal))
                {
                    for (int i = returnedFaces.Count - 1; i >= 0; i--)
                    {
                        var face = returnedFaces[i];
                        if (face.MinX > wall.GetMinX() - wall.Wall.Width &&
                            face.MinX < wall.GetMaxX() + wall.Wall.Width)
                        {
                            if (face.MaxY < wall.GetMaxY() - depth)
                                returnedFaces.RemoveAt(i);
                        }
                    }
                }
            }
            if (extremeWallVariant == ExtremeWallVariant.Left)
            {

                foreach (AdvancedWall wall in sideWalls.Where(w => w.Orientation == ElementOrientation.Vertical))
                {
                    for (int i = returnedFaces.Count - 1; i >= 0; i--)
                    {
                        var face = returnedFaces[i];
                        if (face.MinY > wall.GetMinY() - wall.Wall.Width &&
                            face.MinY < wall.GetMaxY() + wall.Wall.Width)
                        {
                            if (face.MinX > wall.GetMinX() + depth)
                                returnedFaces.RemoveAt(i);
                        }
                    }
                }
            }
            if (extremeWallVariant == ExtremeWallVariant.Right)
            {
                foreach (AdvancedWall wall in sideWalls.Where(w => w.Orientation == ElementOrientation.Vertical))
                {
                    for (int i = returnedFaces.Count - 1; i >= 0; i--)
                    {
                        var face = returnedFaces[i];
                        if (face.MinY > wall.GetMinY() - wall.Wall.Width &&
                            face.MinY < wall.GetMaxY() + wall.Wall.Width)
                        {
                            if (face.MaxX < wall.GetMaxX() - depth)
                                returnedFaces.RemoveAt(i);
                        }
                    }
                }
            }

            //ExportGeometryToXml.ExportAdvancedFaces(returnedFaces, extremeWallVariant + "_ after remove by depth");

            // Фильтрация ближайших граней: если расстояние между гранями меньше заданного в настройка,
            // то удалять из этой пары ту грань, которая указана в настройках (по длине грани)
            var minWidthSetting = int.TryParse(UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings,
                "mprExteriorPlanDimensions",
                "ExteriorFaceMinWidthBetween"), out int m)
                ? m
                : 100;
            var minWidthBetween = minWidthSetting * 0.00328084;
            // Вариант удаления: 0 - наменьший, 1 - наибольший
            var removeVariant = int.TryParse(UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings,
                "mprExteriorPlanDimensions",
                "ExteriorMinWidthFaceRemove"), out m)
                ? m
                : 0;
            if (extremeWallVariant == ExtremeWallVariant.Bottom || extremeWallVariant == ExtremeWallVariant.Top)
            {
                // Сначала нужно отсортировать
                returnedFaces.Sort((f1, f2) => f1.MinX.CompareTo(f2.MinX));
                var wasRemoved = false;
                do
                {
                    for (var i = 0; i < returnedFaces.Count - 1; i++)
                    {
                        var face1 = returnedFaces[i];
                        var face2 = returnedFaces[i + 1];
                        var distance = Math.Abs(face1.MinX - face2.MinX);
                        if (distance < minWidthBetween)
                        {
                            wasRemoved = true;
                            var face1Lenght = Math.Abs(face1.MaxY - face1.MinY);
                            var face2Lenght = Math.Abs(face2.MaxY - face2.MinY);
                            if (removeVariant == 0)
                            {
                                if (face1Lenght < face2Lenght) returnedFaces.RemoveAt(i);
                                else returnedFaces.RemoveAt(i + 1);
                            }
                            else
                            {
                                if (face1Lenght > face2Lenght) returnedFaces.RemoveAt(i);
                                else returnedFaces.RemoveAt(i + 1);
                            }
                            break;
                        }
                        wasRemoved = false;
                    }

                } while (wasRemoved);
            }
            if (extremeWallVariant == ExtremeWallVariant.Left || extremeWallVariant == ExtremeWallVariant.Right)
            {
                // Сначала нужно отсортировать
                returnedFaces.Sort((f1, f2) => f1.MinY.CompareTo(f2.MinY));
                var wasRemoved = false;
                do
                {
                    for (var i = 0; i < returnedFaces.Count - 1; i++)
                    {
                        var face1 = returnedFaces[i];
                        var face2 = returnedFaces[i + 1];
                        var distance = Math.Abs(face1.MinY - face2.MinY);
                        if (distance < minWidthBetween)
                        {
                            wasRemoved = true;
                            var face1Lenght = Math.Abs(face1.MaxX - face1.MinX);
                            var face2Lenght = Math.Abs(face2.MaxX - face2.MinX);
                            if (removeVariant == 0)
                            {
                                if (face1Lenght < face2Lenght) returnedFaces.RemoveAt(i);
                                else returnedFaces.RemoveAt(i + 1);
                            }
                            else
                            {
                                if (face1Lenght > face2Lenght) returnedFaces.RemoveAt(i);
                                else returnedFaces.RemoveAt(i + 1);
                            }
                            break;
                        }
                        wasRemoved = false;
                    }

                } while (wasRemoved);
            }

            return returnedFaces;
        }

        private List<AdvancedWall> FindIntersectionWalls(List<AdvancedWall> sideWalls, List<AdvancedWall> allWalls)
        {
            var intersectionWalls = new List<AdvancedWall>();

            foreach (AdvancedWall wall in allWalls)
            {
                foreach (AdvancedWall sideWall in sideWalls)
                {
                    if (wall.IsAdjoinToByLocationCurveEnds(sideWall) &&
                        !AdvancedHelpers.HasWallInListById(sideWalls, wall))
                    {
                        intersectionWalls.Add(wall);
                    }
                }
            }

            return intersectionWalls;
        }
    }
}
