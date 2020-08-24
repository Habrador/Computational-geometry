using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //2d space
    public class VoronoiEdge2
    {
        //These are the voronoi vertices
        public MyVector2 p1;
        public MyVector2 p2;

        //All positions within a vornoi cell is closer to this position than any other position in the diagram
        public MyVector2 sitePos;
        public Color32 color;

        public VoronoiEdge2(MyVector2 p1, MyVector2 p2, MyVector2 sitePos, Color32 color)
        {
            this.p1 = p1;
            this.p2 = p2;

            this.sitePos = sitePos;
            this.color = color;
        }
    }



    //2d space
    public class VoronoiCell2
    {
        //All positions within a vornoi cell is closer to this position than any other position in the diagram
        public MyVector2 sitePos;

        public List<VoronoiEdge2> edges = new List<VoronoiEdge2>();

        public Color32 color;

        public VoronoiCell2(MyVector2 sitePos, Color32 color)
        {
            this.sitePos = sitePos;
            this.color = color;
        }
    }
}
