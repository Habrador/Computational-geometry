using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry.Marching_Squares
{
    //A square with control nodes in the corners and nodes on the edges halfway between the corners
    //We need all 8 to determine the position and size of the mesh as this square
    public class Square
    {
        //The nodes that determines the size of the mesh
        public ControlNode TL, TR, BL, BR;
        //The midpoint nodes that are halfway between the control nodes which are used when we generate the mesh
        //So L is between TL and BL
        public Node L, T, R, B;
        //The marching square configuration for this square (16 possibilities)
        public int configuration = 0;

        public Square(ControlNode TL, ControlNode TR, ControlNode BR, ControlNode BL)
        {
            this.TL = TL;
            this.TR = TR;
            this.BL = BL;
            this.BR = BR;

            this.L = BL.above;
            this.T = TL.right;
            this.R = BR.above;
            this.B = BL.right;

            if (TL.isActive)
            {
                configuration += 8;
            }
            if (TR.isActive)
            {
                configuration += 4;
            }
            if (BL.isActive)
            {
                configuration += 1;
            }
            if (BR.isActive)
            {
                configuration += 2;
            }
        }
    }
}
