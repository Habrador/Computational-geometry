using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Generate a Transform (position and orientation) suitable for curves like Bezier in 3d space
    public class InterpolationTransform
    {
        public MyVector3 position;
        
        public MyQuaternion orientation;

        public enum GenerateOrientationAlternative
        {
            UpRef,
            FrenetNormal,
            RotationMinimisingFrame
        }

        public InterpolationTransform(MyVector3 position, MyQuaternion orientation)
        {
            this.position = position;
            this.orientation = orientation;
        }



        //
        // Transform (position and orientation) at point t
        //

        //The position and the tangent are easy to find, what's difficult to find is the normal because a line doesn't have a single normal

        //To get the normal in 2d, we can just flip two coordinates in the forward vector and set one to negative
        //MyVector3 normal = new MyVector3(-forwardDir.z, 0f, forwardDir.x);

        //In 3d there are multiple alternatives:

        //Alternative 1. Use ref vector to know which direction is up
        public static InterpolationTransform GetTransform_RefUp(_Curve curve, float t)
        {
            //Position on the curve at point t
            MyVector3 pos = curve.GetPosition(t);

            //Forward direction (tangent) on the curve at point t
            MyVector3 forwardDir = curve.GetTangent(t);

            //A simple way to get the other directions is to use LookRotation with just forward dir as parameter
            //Then the up direction will always be the world up direction, and it calculates the right direction 
            //This idea is not working for all possible curve orientations
            //MyQuaternion orientation = new MyQuaternion(forwardDir);

            //Your own reference up vector
            //This could be an interpolation between the start and end up-vector if you know them (which you do if you put a Unity transform at the start and end). If you use Unity's transform, you can also say that z-scale if the offset of the handle which might simplify things
            //MyVector3 upRef = Vector3.Lerp(startUp, endUp, t).normalized;
            MyQuaternion orientation = InterpolationTransform.GetOrientation_UpRef(forwardDir, Vector3.up.ToMyVector3());


            InterpolationTransform trans = new InterpolationTransform(pos, orientation);

            return trans;
        }


        //Alternative 2. Frenet normal. Use the tagent we have and a tangent next to it
        public static InterpolationTransform GetTransform_FrenetNormal(_Curve curve, float t)
        {
            //Position on the curve at point t
            MyVector3 pos = curve.GetPosition(t);

            //Forward direction (tangent) on the curve at point t
            MyVector3 forwardDir = curve.GetTangent(t);

            MyVector3 secondDerivativeVec = curve.GetSecondDerivativeVec(t);

            MyQuaternion orientation = InterpolationTransform.GetOrientation_FrenetNormal(forwardDir, secondDerivativeVec);


            InterpolationTransform trans = new InterpolationTransform(pos, orientation);

            return trans;
        }


        //Alternative 3. Rotation Minimising Frame
        public static InterpolationTransform GetTransform_RotationMinimisingFrame(MyVector3 position, MyVector3 tangent, InterpolationTransform previousTransform)
        {
            //The first point needs to be initalized with an orientation
            if (previousTransform == null)
            {
                //Just use one of the other algorithms available to generate a transform at a single position
                MyQuaternion orientation = InterpolationTransform.GetOrientation_UpRef(tangent, Vector3.up.ToMyVector3());

                InterpolationTransform transform = new InterpolationTransform(position, orientation);

                return transform;
            }
            //For all other points on the curve
            else
            {
                //To calculate the orientation for this point, we need data from the previous point on the curve
                MyQuaternion orientation = InterpolationTransform.GetOrientation_RotationFrame(position, tangent, previousTransform);

                InterpolationTransform transform = new InterpolationTransform(position, orientation);

                return transform;
            }
        }


        //Get all transforms at all positions
        //Some generate-transform-algorithms require more than one point, such as the "Rotation Minimising Frame"
        public static List<InterpolationTransform> GetTransforms(_Curve curve, List<float> tValues, GenerateOrientationAlternative orientationAlternative)
        {
            List<InterpolationTransform> orientations = new List<InterpolationTransform>();

            for (int i = 0; i < tValues.Count; i++)
            {
                float t = tValues[i];

                //The position and tangent
                MyVector3 position = curve.GetPosition(t);
                MyVector3 tangent = curve.GetTangent(t);

                //Generate an orientation with one of the possible alternatives
                if (orientationAlternative == GenerateOrientationAlternative.RotationMinimisingFrame)
                {
                    InterpolationTransform previousTransform = (i == 0) ? null : orientations[i - 1];

                    InterpolationTransform transform = InterpolationTransform.GetTransform_RotationMinimisingFrame(position, tangent, previousTransform);

                    orientations.Add(transform);
                }
                else if (orientationAlternative == GenerateOrientationAlternative.FrenetNormal)
                {
                    InterpolationTransform transform = InterpolationTransform.GetTransform_FrenetNormal(curve, t);

                    orientations.Add(transform);
                }
                else if (orientationAlternative == GenerateOrientationAlternative.UpRef)
                {
                    InterpolationTransform transform = InterpolationTransform.GetTransform_RefUp(curve, t);

                    orientations.Add(transform);
                }
            }

            return orientations;
        }


        //Method that can take curve sections and returns the orientation for each point on the entire curve 



        //
        // Orientation at point t
        //

        //You can read about these methods here:
        //https://pomax.github.io/bezierinfo/#pointvectors3d
        //Game Programming Gems 2: The Parallel Transport Frame (p. 215)

        //"Fixed Up"
        //Just pick an "up" reference vector
        //From "Unite 2015 - A coder's guide to spline-based procedural geometry" https://www.youtube.com/watch?v=o9RK6O2kOKo
        //Is not going to work if we have loops, but should work if you make "2d" roads like in cities skylines so no roller coasters
        public static MyQuaternion GetOrientation_UpRef(MyVector3 tangent, MyVector3 upRef)
        {
            tangent = MyVector3.Normalize(tangent);
            
            MyVector3 biNormal = MyVector3.Normalize(MyVector3.Cross(upRef, tangent));

            MyVector3 normal = MyVector3.Normalize(MyVector3.Cross(tangent, biNormal));

            MyQuaternion orientation = new MyQuaternion(tangent, normal);

            return orientation;
        }

        //"Frenet Normal" (also known as Frenet Frame)
        //Works in many cases (but sometimes the frame may flip because of changes in the second derivative) 
        public static MyQuaternion GetOrientation_FrenetNormal(MyVector3 tangent, MyVector3 secondDerivativeVec)
        {
            MyVector3 a = MyVector3.Normalize(tangent);

            //What a next point's tangent would be if the curve stopped changing at our point and just had the same derivative and second derivative from that point on
            MyVector3 b = MyVector3.Normalize(a + secondDerivativeVec);

            //A vector that we use as the "axis of rotation" for turning the tangent a quarter circle to get the normal
            MyVector3 r = MyVector3.Normalize(MyVector3.Cross(a, b));

            //The normal vector should be perpendicular to the plane that the tangent and the axis of rotation lie in
            MyVector3 normal = MyVector3.Normalize(MyVector3.Cross(r, a));

            MyQuaternion orientation = new MyQuaternion(tangent, normal);

            return orientation;
        }

        //"Rotation Minimising Frame" (also known as "Parallel Transport Frame" or "Bishop Frame")
        //Gets its stability by incrementally rotating a coordinate system (= frame) as it is translate along the curve
        //Has to be computed for the entire curve because we need the previous frame (previousTransform) belonging to a point before this point
        //Is initalized by using "Fixed Up" or "Frenet Normal"
        public static MyQuaternion GetOrientation_RotationFrame(MyVector3 position, MyVector3 tangent, InterpolationTransform previousTransform)
        {
            /*
            //This version is from https://pomax.github.io/bezierinfo/#pointvectors3d
            //Reflect the known frame onto the next point, by treating the plane through the curve at the point exactly between the next and previous points as a "mirror"
            MyVector3 v1 = position - previousTransform.position;

            float c1 = MyVector3.Dot(v1, v1);

            MyVector3 riL = previousTransform.Right - v1 * (2f / c1) * MyVector3.Dot(v1, previousTransform.Right);

            MyVector3 tiL = previousTransform.Forward - v1 * (2f / c1) * MyVector3.Dot(v1, previousTransform.Forward);

            //This gives the next point a tangent vector that's essentially pointing in the opposite direction of what it should be, and a normal that's slightly off-kilter
            //reflect the vectors of our "mirrored frame" a second time, but this time using the plane through the "next point" itself as "mirror".
            MyVector3 v2 = tangent - tiL;

            float c2 = MyVector3.Dot(v2, v2);

            //Now we can calculate the normal and right vector belonging to this orientation
            MyVector3 right = riL - v2 * (2f / c2) * MyVector3.Dot(v2, riL);

            //The source has right x tangent, but then every second normal is flipped
            MyVector3 normal = MyVector3.Cross(tangent, right);
            
            MyQuaternion orientation = new MyQuaternion(tangent, normal); 
            */

            
            //This version is from Game Programming Gems 2: The Parallel Transport Frame
            //They generate the same result and this one is easier to understand

            //The two tangents
            MyVector3 T1 = previousTransform.Forward;
            MyVector3 T2 = tangent;

            //You move T1 to the new position, so A is a vector going from the new position
            MyVector3 A = MyVector3.Cross(T1, T2);

            //This is the angle between T1 and T2
            float alpha = Mathf.Acos(MyVector3.Dot(T1, T2) / (MyVector3.Magnitude(T1) * MyVector3.Magnitude(T2)));

            //Now rotate the previous frame around axis A with angle alpha
            MyQuaternion F1 = previousTransform.orientation;

            MyQuaternion F2 = MyQuaternion.RotateQuaternion(F1, alpha * Mathf.Rad2Deg, A);

            MyQuaternion orientation = F2;
            

            return orientation;
        }



        //
        // Get directions from orientation
        //

        public MyVector3 Forward => orientation.Forward;
        public MyVector3 Right   => orientation.Right;
        public MyVector3 Up      => orientation.Up;



        //
        // Transform between coordinate systems
        //

        //Transform a position from local to world
        //If input is MyVector3.Right * 2f then we should get a point in world space on the curve 
        //at this position moved along the local x-axis 2m
        public MyVector3 LocalToWorld_Pos(MyVector3 localPos)
        {
            MyVector3 worldPos = position + MyQuaternion.RotateVector(orientation, localPos);

            return worldPos;
        }

        //Transform a direction from local to world
        public MyVector3 LocalToWorld_Dir(MyVector3 localDir)
        {
            MyVector3 worldDir = MyQuaternion.RotateVector(orientation, localDir);

            return worldDir;
        }
    }
}
