using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class GenerateMesh
    {
        //Generate a flat chunk with some dimensions
        public static Mesh GenerateChunk(float width, int cells)
        {
            Mesh chunk = Chunk.GenerateChunk(width, cells);

            return chunk;
        }
    }
}
