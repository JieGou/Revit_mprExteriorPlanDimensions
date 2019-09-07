namespace mprExteriorPlanDimensions.Body.AdvancedClasses
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;

    /// <summary>
    /// Advanced PlanarFace
    /// </summary>
    public class AdvancedPlanarFace
    {
        #region Conctructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="wallId">Parent wall id</param>
        /// <param name="planarFace">Planar face</param>
        public AdvancedPlanarFace(int wallId, PlanarFace planarFace)
        {
            PlanarFace = planarFace;
            WallId = wallId;
            DefineAdvancedPlanarFace();
        }

        #endregion

        #region Properties

        public PlanarFace PlanarFace { get; }

        /// <summary>False - не удалось определить значения для элемента</summary>
        public bool IsDefined { get; set; } = true;

        /// <summary>Parent wall id</summary>
        public int WallId { get; }

        public double MinX { get; set; }

        public double MaxX { get; set; }

        public double MinY { get; set; }

        public double MaxY { get; set; }

        public double MinZ { get; set; }

        public double MaxZ { get; set; }

        /// <summary>PlanarFace's edges</summary>
        public List<Edge> Edges { get; set; }

        public bool IsHorizontal { get; set; }

        public bool IsVertical { get; set; }

        #endregion

        #region Private Methods

        private void DefineAdvancedPlanarFace()
        {
            MinX = PlanarFace.GetMinX();
            MaxX = PlanarFace.GetMaxX();
            MinY = PlanarFace.GetMinY();
            MaxY = PlanarFace.GetMaxY();
            MinZ = PlanarFace.GetMinZ();
            MaxZ = PlanarFace.GetMaxZ();

            Edges = GetEdges();
            if (!Edges.Any())
            {
                IsDefined = false;
                return;
            }

            IsHorizontal = PlanarFace.IsHorizontal();
            IsVertical = PlanarFace.IsVertical();
            if (!IsVertical && !IsHorizontal)
            {
                IsDefined = false;
            }
        }
        
        /// <summary>Get edges from PlanarFace</summary>
        /// <returns>List of edges</returns>
        private List<Edge> GetEdges()
        {
            var edges = new List<Edge>();
            var edgeArrayArray = PlanarFace.EdgeLoops;
            foreach (EdgeArray edgeArray in edgeArrayArray)
            {
                foreach (Edge edge in edgeArray)
                {
                    edges.Add(edge);
                }
            }

            return edges;
        }

        #endregion
    }
}
