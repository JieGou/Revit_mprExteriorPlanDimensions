using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace mprExteriorPlanDimensions.Body.AdvancedClasses
{
    public class AdvancedPlanarFace
    {
        #region Public Fields

        public PlanarFace PlanarFace;
        /// <summary>False - не удалось определить значения для элемента</summary>
        public bool IsDefinded = true;
        /// <summary>Parent wall id</summary>
        public int WallId;

        public double MinX;
        public double MaxX;
        public double MinY;
        public double MaxY;
        public double MinZ;
        public double MaxZ;
        /// <summary>PlanarFace's edges</summary>
        public List<Edge> Edges;

        public bool IsHorizontal;
        public bool IsVertical;
        

        #endregion
        #region Conctructor

        public AdvancedPlanarFace(int wallId, PlanarFace planarFace)
        {
            PlanarFace = planarFace;
            WallId = wallId;
            DefineAdvancedPlanarFace();
        }
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
                IsDefinded = false;
                return;
            }
            IsHorizontal = PlanarFace.IsHorizontal();
            IsVertical = PlanarFace.IsVertical();
            if (!IsVertical && !IsHorizontal)
            {
                IsDefinded = false;
            }
        }
        /// <summary>Get edges from PlanarFace</summary>
        /// <returns>List of edges</returns>
        private List<Edge> GetEdges()
        {
            List<Edge> edges = new List<Edge>();
            EdgeArrayArray edgeArrayArray = PlanarFace.EdgeLoops;
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

        #region Public Methods


        #endregion
    }
}
