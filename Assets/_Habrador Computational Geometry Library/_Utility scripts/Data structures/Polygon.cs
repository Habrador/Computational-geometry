using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Polygon in 2d space
    public struct Polygon2
    {
        public List<MyVector2> vertices;


        public Polygon2(List<MyVector2> vertices)
        {
            this.vertices = vertices;
        }
    }


    //Polygon in 3d space
    public struct Polygon3
    {
        public List<MyVector3> vertices;


        public Polygon3(List<MyVector3> vertices)
        {
            this.vertices = vertices;
        }
    }
}
