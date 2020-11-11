using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A collection of classes to make the methods more general
namespace Habrador_Computational_Geometry
{
    //Base class for all curves
    public abstract class _Curve
    {
        //All child classes need to have these methods
        public abstract MyVector3 GetPosition(float t);

        public abstract float CalculateDerivative(float t);
    }
}
