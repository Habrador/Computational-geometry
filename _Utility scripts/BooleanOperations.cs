namespace Habrador_Computational_Geometry
{
    //Different boolean operations
    //Intersection - Remove everything except where both A and B intersect
    //Difference - Remove from A where B intersect with A. Remove everything from B
    //ExclusiveOr - Remove from A and B where A and B intersect
    //Union - Combine A and B into one. Keep everything from A and B
    public enum BooleanOperation { Intersection, Difference, ExclusiveOr, Union }
}
