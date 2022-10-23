using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Habrador_Computational_Geometry
{
    //Interface for each item in the heap
    public interface IHeapItem<T> : IComparable<T>
    {
        int HeapIndex
        {
            get;
            set;
        }
    }
}
