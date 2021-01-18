using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Help class for cut mesh with plane
    public class CutMeshHole
    {
        public HalfEdgeData3 holeMeshI;
        public HalfEdgeData3 holeMeshO;

        //We need a single edge to make it easier to identify which mesh should be merged with this hole
        public HalfEdge3 holeEdgeI;
        public HalfEdge3 holeEdgeO;


        public CutMeshHole(HalfEdgeData3 holeMeshI, HalfEdgeData3 holeMeshO, HalfEdge3 holeEdgeI, HalfEdge3 holeEdgeO)
        {
            this.holeMeshI = holeMeshI;
            this.holeMeshO = holeMeshO;

            this.holeEdgeI = holeEdgeI;
            this.holeEdgeO = holeEdgeO;
        }
    }
}
