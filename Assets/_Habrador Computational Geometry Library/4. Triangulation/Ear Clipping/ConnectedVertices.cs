using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public class ConnectedVertices
    {
        public List<MyVector2> vertices;

        //the vertex in the list with the maximum x-value
        public MyVector2 maxX_Vert;

        //The position in the list where the maxX vert is 
        public int maxX_ListPos;


        public ConnectedVertices(List<MyVector2> vertices)
        {
            this.vertices = vertices;

            CalculateMaxXValue();
        }


        //Find the vertex with the maximum x-value
        private void CalculateMaxXValue()
        {
            this.maxX_Vert = vertices[0];

            this.maxX_ListPos = 0;

            for (int i = 1; i < vertices.Count; i++)
            {
                MyVector2 v = vertices[i];

                if (v.x > maxX_Vert.x)
                {
                    this.maxX_Vert = v;

                    this.maxX_ListPos = i;
                }
            }
        }
    }
}
