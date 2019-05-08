using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //From "Fully Dynamic Constrained Delaunay Triangulations" by Kallmann and others
    public static class DynamicConstrainedDelaunay
    {
        //Add constraint
        public static HalfEdgeData AddConstraint(HalfEdgeData triangleData, Edge constraintToAdd, List<Edge> allConstraints)
        {
            if (constraintToAdd == null || triangleData == null)
            {
                return triangleData;
            }



            return triangleData;
        }



        //Remove constraint
        public static HalfEdgeData RemoveConstraint(HalfEdgeData triangleData, Edge constraintToRemove, List<Edge> allConstraints)
        {
            if (constraintToRemove == null || triangleData == null)
            {
                return triangleData;
            }



            return triangleData;
        }


    }
}
