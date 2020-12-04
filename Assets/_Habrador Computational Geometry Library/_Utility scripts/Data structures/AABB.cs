using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Axis-Aligned-Bounding-Box, which is a rectangle in 2d space aligned along the x and y axis
    public struct AABB2
    {
        public float minX;
        public float maxX;
        public float minY;
        public float maxY;


        //We know the min and max values
        public AABB2(float minX, float maxX, float minY, float maxY)
        {
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
        }


        //We have a list with points and want to find the min and max values
        public AABB2(List<MyVector2> points)
        {
            MyVector2 p1 = points[0];

            float minX = p1.x;
            float maxX = p1.x;
            float minY = p1.y;
            float maxY = p1.y;

            for (int i = 1; i < points.Count; i++)
            {
                MyVector2 p = points[i];

                if (p.x < minX)
                {
                    minX = p.x;
                }
                else if (p.x > maxX)
                {
                    maxX = p.x;
                }

                if (p.y < minY)
                {
                    minY = p.y;
                }
                else if (p.y > maxY)
                {
                    maxY = p.y;
                }
            }

            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
        }
    }



    //Axis-Aligned-Bounding-Box, which is a box in 3d space aligned along the x and y axis
    public struct AABB3
    {
        //top is y-axis, front is z-axis, and right is x-axis
        public MyVector3 topFR;
        public MyVector3 topFL;
        public MyVector3 topBR;
        public MyVector3 topBL;

        public MyVector3 bottomFR;
        public MyVector3 bottomFL;
        public MyVector3 bottomBR;
        public MyVector3 bottomBL;


        //Bounds is a Unity data structure
        //You can get it from either mesh (local space)
        //or mesh renderer world space
        public AABB3(Bounds bounds)
        {
            Vector3 halfSize = bounds.extents;

            //Top (y axis)
            Vector3 top = bounds.center + Vector3.up * halfSize.y;

            //z axis
            Vector3 topF = top + Vector3.forward * halfSize.z;
            Vector3 topB = top - Vector3.forward * halfSize.z;

            //x axis
            Vector3 topFR_u = topF + Vector3.right * halfSize.x;
            Vector3 topFL_u = topF + Vector3.left * halfSize.x;
            Vector3 topBR_u = topB + Vector3.right * halfSize.x;
            Vector3 topBL_u = topB + Vector3.left * halfSize.x;


            //Bottom
            Vector3 bottom = bounds.center - Vector3.up * halfSize.y;

            Vector3 bottomF = bottom + Vector3.forward * halfSize.z;
            Vector3 bottomB = bottom - Vector3.forward * halfSize.z;

            Vector3 bottomFR_u = bottomF + Vector3.right * halfSize.x;
            Vector3 bottomFL_u = bottomF + Vector3.left * halfSize.x;
            Vector3 bottomBR_u = bottomB + Vector3.right * halfSize.x;
            Vector3 bottomBL_u = bottomB + Vector3.left * halfSize.x;


            this.topFR = topFR_u.ToMyVector3();
            this.topFL = topFL_u.ToMyVector3();
            this.topBR = topBR_u.ToMyVector3();
            this.topBL = topBL_u.ToMyVector3();

            this.bottomFR = bottomFR_u.ToMyVector3();
            this.bottomFL = bottomFL_u.ToMyVector3();
            this.bottomBR = bottomBR_u.ToMyVector3();
            this.bottomBL = bottomBL_u.ToMyVector3();
        }



        //Its common that we want to display this box for debugging, so return a list with edges that form the box
        public List<Edge3> GetEdges()
        {
            List<Edge3> edges = new List<Edge3>()
            {
                new Edge3(topFR, topFL),
                new Edge3(topFL, topBL),
                new Edge3(topBL, topBR),
                new Edge3(topBR, topFR),

                new Edge3(bottomFR, bottomFL),
                new Edge3(bottomFL, bottomBL),
                new Edge3(bottomBL, bottomBR),
                new Edge3(bottomBR, bottomFR),

                new Edge3(topFR, bottomFR),
                new Edge3(topFL, bottomFL),
                new Edge3(topBL, bottomBL),
                new Edge3(topBR, bottomBR),
            };

            return edges;
        }



        //Get all corners of the box
        public HashSet<MyVector3> GetCorners()
        {
            HashSet<MyVector3> corners = new HashSet<MyVector3>()
            {
                topFR,
                topFL,
                topBR,
                topBL,

                bottomFR,
                bottomFL,
                bottomBR,
                bottomBL,
            };

            return corners;
        }
    }
}
