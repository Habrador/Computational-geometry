using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

//Attach to every mesh you want to cut
//A bottleneck is converting from mesh to half-edge data structure, so better to cache it
public class CutMesh : MonoBehaviour
{
    //This data should be in the same space as the mesh 
    public HalfEdgeData3 halfEdge3DataStructure;
}
