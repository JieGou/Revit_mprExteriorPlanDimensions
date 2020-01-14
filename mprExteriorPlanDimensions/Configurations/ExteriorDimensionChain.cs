namespace mprExteriorPlanDimensions.Configurations
{
    using System.Xml.Linq;
    using ModPlusAPI.Mvvm;

    /// <summary>Цепочка размеров для наружных стен</summary>
    public class ExteriorDimensionChain : VmBase
    {
        private string _displayName;
        private int _elementOffset;
        private bool _walls;
        private bool _intersectingWalls;
        private bool _openings;
        private System.Windows.Visibility _intersectingWallsAndOpeningsVisibility;
        private bool _grids;
        private bool _extremeGrids;
        private bool _overall;

        public ExteriorDimensionChain()
        {
            _walls = true;
            _openings = false;
            _intersectingWalls = false;
            _grids = false;
            _elementOffset = 8;
            _extremeGrids = false;
        }
        
        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        /// <summary>
        /// Отступ от предыдущего элемента (стены или другой цепочки размеров)
        /// </summary>
        public int ElementOffset
        {
            get => _elementOffset;
            set
            {
                _elementOffset = value;
                OnPropertyChanged(nameof(ElementOffset));
            }
        }

        /// <summary>Ставить размеры по стенам</summary>
        public bool Walls
        {
            get => _walls;
            set
            {
                _walls = value;
                OnPropertyChanged(nameof(Walls));
                if (value)
                {
                    IntersectingWallsAndOpeningsVisibility = System.Windows.Visibility.Visible;
                    if (Overall)
                    {
                        Overall = false;
                        OnPropertyChanged(nameof(Overall));
                    }

                    if (ExtremeGrids)
                    {
                        ExtremeGrids = false;
                        OnPropertyChanged(nameof(ExtremeGrids));
                    }
                }
                else
                {
                    if (IntersectingWalls)
                    {
                        IntersectingWalls = false;
                        OnPropertyChanged(nameof(IntersectingWalls));
                    }

                    if (Openings)
                    {
                        Openings = false;
                        OnPropertyChanged(nameof(Openings));
                    }

                    IntersectingWallsAndOpeningsVisibility = System.Windows.Visibility.Hidden;
                    if (!ExtremeGrids && !Overall)
                    {
                        ExtremeGrids = true;
                        OnPropertyChanged(nameof(ExtremeGrids));
                    }
                }
            }
        }

        /// <summary>Ставить размеры по пересекающимся стенам</summary>
        public bool IntersectingWalls
        {
            get => _intersectingWalls;
            set
            {
                _intersectingWalls = value;
                OnPropertyChanged(nameof(IntersectingWalls));
            }
        }

        /// <summary>Ставить размеры по проемам</summary>
        public bool Openings
        {
            get => _openings;
            set
            {
                _openings = value;
                OnPropertyChanged(nameof(Openings));
            }
        }

        public System.Windows.Visibility IntersectingWallsAndOpeningsVisibility
        {
            get => _intersectingWallsAndOpeningsVisibility; set
            {
                _intersectingWallsAndOpeningsVisibility = value;
                OnPropertyChanged(nameof(IntersectingWallsAndOpeningsVisibility));
            }
        }

        /// <summary>Ставить размеры по разбивочным осям</summary>
        public bool Grids
        {
            get => _grids;
            set
            {
                _grids = value;
                if (value)
                {
                    if (ExtremeGrids)
                    {
                        ExtremeGrids = false;
                        OnPropertyChanged(nameof(ExtremeGrids));
                    }

                    if (Overall)
                    {
                        Overall = false;
                        OnPropertyChanged(nameof(Overall));
                    }
                }

                OnPropertyChanged(nameof(Grids));
            }
        }

        public bool ExtremeGrids
        {
            get => _extremeGrids;
            set
            {
                _extremeGrids = value;
                OnPropertyChanged(nameof(ExtremeGrids));
                if (value)
                {
                    if (Walls)
                    {
                        Walls = false;
                        OnPropertyChanged(nameof(Walls));
                    }

                    if (Openings)
                    {
                        Openings = false;
                        OnPropertyChanged(nameof(Openings));
                    }

                    if (IntersectingWalls)
                    {
                        IntersectingWalls = false;
                        OnPropertyChanged(nameof(IntersectingWalls));
                    }

                    if (Grids)
                    {
                        Grids = false;
                        OnPropertyChanged(nameof(Grids));
                    }

                    if (Overall)
                    {
                        Overall = false;
                        OnPropertyChanged(nameof(Overall));
                    }
                }
                else
                {
                    if (!Walls && !Overall && !Grids)
                    {
                        Walls = true;
                        OnPropertyChanged(nameof(Walls));
                    }
                }
            }
        }

        public bool Overall
        {
            get => _overall;
            set
            {
                _overall = value;
                OnPropertyChanged(nameof(Overall));
                if (value)
                {
                    if (Walls)
                    {
                        Walls = false;
                        OnPropertyChanged(nameof(Walls));
                    }

                    if (Openings)
                    {
                        Openings = false;
                        OnPropertyChanged(nameof(Openings));
                    }

                    if (IntersectingWalls)
                    {
                        IntersectingWalls = false;
                        OnPropertyChanged(nameof(IntersectingWalls));
                    }

                    if (Grids)
                    {
                        Grids = false;
                        OnPropertyChanged(nameof(Grids));
                    }

                    if (ExtremeGrids)
                    {
                        ExtremeGrids = false;
                        OnPropertyChanged(nameof(ExtremeGrids));
                    }
                }
                else
                {
                    if (!Walls && !ExtremeGrids && !Grids)
                    {
                        Walls = true;
                        OnPropertyChanged(nameof(Walls));
                    }
                }
            }
        }
        
        /// <summary>Получение "цепочки" их xml-элемента</summary>
        /// <param name="xElement"></param>
        /// <returns></returns>
        public static ExteriorDimensionChain GetExteriorDimensionChainFromXElement(XElement xElement)
        {
            var edc = new ExteriorDimensionChain();

            // bools
            edc.Walls = !bool.TryParse(xElement.Attribute(nameof(edc.Walls))?.Value, out var b) || b; // true
            edc.Grids = bool.TryParse(xElement.Attribute(nameof(edc.Grids))?.Value, out b) && b; // false
            edc.Openings = bool.TryParse(xElement.Attribute(nameof(edc.Openings))?.Value, out b) && b; // false
            edc.IntersectingWalls = bool.TryParse(xElement.Attribute(nameof(edc.IntersectingWalls))?.Value, out b) && b; // false
            edc.ExtremeGrids = bool.TryParse(xElement.Attribute(nameof(edc.ExtremeGrids))?.Value, out b) && b; // false
            edc.Overall = bool.TryParse(xElement.Attribute(nameof(edc.Overall))?.Value, out b) && b; // false

            // ints
            edc.ElementOffset = int.TryParse(xElement.Attribute(nameof(edc.ElementOffset))?.Value, out var i) ? i : 8;

            return edc;
        }

        /// <summary>Получение XElement из экземпляра класса "цепочки"</summary>
        public XElement GetXElementFromDimensionChainInstance()
        {
            var xElement = new XElement(Constants.XElementName_Chain);
            xElement.SetAttributeValue(nameof(Walls), Walls);
            xElement.SetAttributeValue(nameof(IntersectingWalls), IntersectingWalls);
            xElement.SetAttributeValue(nameof(Openings), Openings);
            xElement.SetAttributeValue(nameof(Grids), Grids);
            xElement.SetAttributeValue(nameof(ExtremeGrids), ExtremeGrids);
            xElement.SetAttributeValue(nameof(ElementOffset), ElementOffset);
            xElement.SetAttributeValue(nameof(Overall), Overall);

            return xElement;
        }
    }
}
