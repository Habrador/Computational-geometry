using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Simple class that will remove unwanted triangles from a mesh
    //These unwanted triangles are nown as slivers
    public static class RemoveUnwantedTriangles
    {
        public static void Remove(HalfEdgeData3 meshData)
        {
            //In mesh generation context, the ratio of the shortest edge to the radius of the triangle's circumcircle is used to express the quality of a triangle
            //slivers are usually defined by having much smaller area/volume than its circumcircle

            // - Caps. Triangle where one angle is close to 180 degrees. Are difficult to remove. if the vertex is connected to three triangles, we can maybe just remove the vertex and build one big triangle. Is this case a flat terahedron?
            // - Needles. Triangle where the longest edge is much longer than the shortest one. You can detect these by by finding the ratio between the shortest and lonest side. If this ratio is below 0.01 then remove it. Same as saying that the smallest angle is close to 0 degrees? Can often be removed by collapsing the shortest edge
            //A triangle can be both a caps and a needle

            //Since there's no general algorithm, maybe better to test one of the mesh simplification algorithms, which should remove slivers first?
        }
    }
}
