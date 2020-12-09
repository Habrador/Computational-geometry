using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public class Hole
    {
        public HalfEdgeData3 holeMeshI;
        public HalfEdgeData3 holeMeshO;

        //Needed to make it faster to indentify to which mesh this hole belongs
        public List<HalfEdge3> holeEdges;

        public Hole(HalfEdgeData3 holeMeshI, HalfEdgeData3 holeMeshO, List<HalfEdge3> holeEdges)
        {
            this.holeMeshI = holeMeshI;
            this.holeMeshO = holeMeshO;

            this.holeEdges = holeEdges;
        }
    }
}
