using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Based on "Procedural Geometry - An Improvised Live Course" https://www.youtube.com/watch?v=6xs0Saff940

    [CreateAssetMenu(menuName = "My Assets/Mesh Profile")]
    public class MeshProfile : ScriptableObject
    {
        public Vertex[] vertices;

        //So we know how the vertices are connected
        //Sometimes we have multiple vertices at the same coordinate to get a sharp corner and those shouldnt be connected with a mesh
        //When adding these you should step around the profile clockwise to make it easier to make a mesh
        public MyVector2Int[] lineIndices;
    }


    [System.Serializable]
    public class Vertex
    {
        public MyVector2 point;
        public MyVector2 normal;
        //The u in uv (v is in the extrusion direction)
        public float u;
    }
}
