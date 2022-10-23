using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Habrador_Computational_Geometry
{
    //Heap data structure (fast way to find the smallest value in an array)
    //Based on "A* Pathfinding (E04: heap optimization)" https://www.youtube.com/watch?v=3Dw5d7PlcTM
    //A heap is like a tree, where the smallest value is at the top
    //The two values below are both bigger

    //T has to implement interface IHeapItem
    public class Heap<T> where T : IHeapItem<T>
    {
        private T[] items;

        private int currentItemCount;


        //The heap is using the array, so we need to define how many items we can possibly have in the array
        public Heap(int maxHeapSize)
        {
            this.items = new T[maxHeapSize];
        }
        

        //Add a new item to the heap
        public void Add(T item)
        {
            item.HeapIndex = currentItemCount;

            //Add it to the end of the heap
            items[currentItemCount] = item;

            SortUp(item);

            currentItemCount += 1;
        }


        //Remove the best value from the heap (the one at the top of the heap)
        public T RemoveFirst()
        {
            T firstItem = items[0];

            currentItemCount -= 1;

            //To restore the heap, take the item at the end of the heap, and sort it downwards
            items[0] = items[currentItemCount];

            items[0].HeapIndex = 0;

            SortDown(items[0]);

            return firstItem;
        }


        //Is an item in the heap?
        //Make sure the item has a heapIndex!
        public bool Contains(T item)
        {
            return Equals(items[item.HeapIndex], item);
        }


        //How many items do we have in the heap?
        public int Count
        {
            get { return currentItemCount; }
        }


        public void UpdateItem(T item)
        {
            SortUp(item);
            SortDown(item);
        }


        //Sort and item downwards in the heap
        private void SortDown(T item)
        {
            while (true)
            {
                //Each item in the heap has a maximum of two children below it
                int childIndexL = item.HeapIndex * 2 + 1;
                int childIndexR = item.HeapIndex * 2 + 2;

                int swapIndex = 0;

                //Do we have a child to the left?
                if (childIndexL < currentItemCount)
                {
                    swapIndex = childIndexL;

                    //Do we have a child to the right?
                    if (childIndexR < currentItemCount)
                    {
                        //We know we have two children, so which one should move up?
                        if (items[childIndexL].CompareTo(items[childIndexR]) < 0)
                        {
                            //This means that the item to the left was bigger than the item to the right
                            //So we can't move left item up one level because then it would be bigger than its right child
                            swapIndex = childIndexR;
                        }
                    }

                    //BUT we also have to check that the item we want to move down is bigger 
                    if (item.CompareTo(items[swapIndex]) < 0)
                    {
                        Swap(item, items[swapIndex]);
                    }
                    //The item is where it should be
                    else
                    {
                        return;
                    }
                }
                //There are no children to the left
                else
                {
                    return;
                }
            }
        }


        //Sort an item upwards in the heap
        //When we add a new item, we add it to the bottom, but it should maybe be higher up
        private void SortUp(T item)
        {
            int parentIndex = (item.HeapIndex - 1) / 2;

            while (true)
            {
                T parentItem = items[parentIndex];

                //If the parent is bigger than the child, then swap
                //CompareTo returns: 
                // 1 if child smaller than parent (meaning child has higher priority)
                // 0 if same
                // 1 if bigger
                //Remember that we should invert CompareTo in our implementation of CompareTo in the node itself 
                if (item.CompareTo(parentItem) > 0)
                {
                    Swap(item, parentItem);
                }
                else
                {
                    break;
                }

                parentIndex = (item.HeapIndex - 1) / 2;
            }
        }


        //Swap tw items in the heap
        private void Swap(T itemA, T itemB)
        {
            items[itemA.HeapIndex] = itemB;
            items[itemB.HeapIndex] = itemA;

            int itemAIndex = itemA.HeapIndex;

            itemA.HeapIndex = itemB.HeapIndex;
            itemB.HeapIndex = itemAIndex;
        }
    }
}
