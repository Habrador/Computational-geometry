using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class InterpolationController : MonoBehaviour
{
    public Transform transPointA;
    public Transform transPointB;
    public Transform transHandleA;
    public Transform transHandleB;

    private int seed = 0;


    private void OnDrawGizmos()
    {
        //3d
        MyVector3 posA = transPointA.position.ToMyVector3();
        MyVector3 posB = transPointB.position.ToMyVector3();

        MyVector3 handleA = transHandleA.position.ToMyVector3();
        MyVector3 handleB = transHandleB.position.ToMyVector3();


        //Interpolate between cooldinates

        //Exponential (BROKEN)
        //Eerp(posA, posB);


        //Bezier curves

        //BezierLinear(posA, posB);

        //BezierQuadratic(posA, posB, handleA);

        //BezierCubic(posA, posB, handleA, handleB);

        //BezierCubicEqualSteps(posA, posB, handleA, handleB);

        CatmullRom(posA, posB, handleA, handleB);
    }



    private void BezierLinear(MyVector3 a, MyVector3 b)
    {
        //Store the interpolated values so we later can display them
        List<Vector3> interpolatedValues = new List<Vector3>();

        //Loop between 0 and 1 in steps, where 1 step is minimum
        //So if steps is 5 then the line will be cut in 5 sections
        int steps = 5;

        float stepSize = 1f / (float)steps;

        float t = 0f;

        //+1 becuase wa also have to include the first point
        for (int i = 0; i < steps + 1; i++)
        {
            //Debug.Log(t);

            MyVector3 interpolatedValue = _Interpolation.BezierLinear(a, b, t);

            interpolatedValues.Add(interpolatedValue.ToVector3());

            t += stepSize;
        }


        DisplayInterpolatedValues(interpolatedValues, useRandomColor: true);
    }



    private void BezierQuadratic(MyVector3 posA, MyVector3 posB, MyVector3 handle)
    {
        //Store the interpolated values so we later can display them
        List<Vector3> interpolatedValues = new List<Vector3>();

        //Loop between 0 and 1 in steps, where 1 step is minimum
        //So if steps is 5 then the line will be cut in 5 sections
        int steps = 10;

        float stepSize = 1f / (float)steps;

        float t = 0f;

        //+1 becuase wa also have to include the first point
        for (int i = 0; i < steps + 1; i++)
        {
            //Debug.Log(t);

            MyVector3 interpolatedValue = _Interpolation.BezierQuadratic(posA, posB, handle, t);

            interpolatedValues.Add(interpolatedValue.ToVector3());

            t += stepSize;
        }


        DisplayInterpolatedValues(interpolatedValues, useRandomColor: true);

        //Display the start and end values and the handle points
        DisplayHandle(handle.ToVector3(), posA.ToVector3());
        DisplayHandle(handle.ToVector3(), posB.ToVector3());
    }



    private void BezierCubic(MyVector3 posA, MyVector3 posB, MyVector3 handleA, MyVector3 handleB)
    {
        //Store the interpolated values so we later can display them
        List<Vector3> interpolatedValues = new List<Vector3>();

        //Loop between 0 and 1 in steps, where 1 step is minimum
        //So if steps is 5 then the line will be cut in 5 sections
        int steps = 5;

        float stepSize = 1f / (float)steps;

        float t = 0f;

        //+1 becuase wa also have to include the first point
        for (int i = 0; i < steps + 1; i++)
        {
            //Debug.Log(t);

            MyVector3 interpolatedPos = _Interpolation.BezierCubic(posA, posB, handleA, handleB, t);

            interpolatedValues.Add(interpolatedPos.ToVector3());

            t += stepSize;
        }



        DisplayInterpolatedValues(interpolatedValues, useRandomColor: true);

        //Display the start and end values and the handle points
        DisplayHandle(handleA.ToVector3(), posA.ToVector3());
        DisplayHandle(handleB.ToVector3(), posB.ToVector3());
    }


    private void BezierCubicEqualSteps(MyVector3 posA, MyVector3 posB, MyVector3 handleA, MyVector3 handleB)
    {
        //Store the interpolated values so we later can display them
        List<Vector3> actualPositions = new List<Vector3>();


        //Step 1. Calculate the length of the entire curve
        //This is needed to so we know how long we should walk each step
        //float length = InterpolationHelpMethods.GetLengthNaiveCubicBezier(posA, posB, handleA, handleB, steps: 20, tEnd: 1f);

        float length = InterpolationHelpMethods.GetLengthSimpsonsRule_CubicBezier(posA, posB, handleA, handleB, tStart: 0f, tEnd: 1f);

        //Debug.Log(length + " " + lengthOther);

      

        

        int steps = 5;

        //Important not to confuse this with the step size we use to iterate t
        //This step size is distance in m
        float lengthStepSize = length / (float)steps;

        float distanceTravelled = 0f;

        for (int i = 0; i < steps + 1; i++)
        {
            float actualT = InterpolationHelpMethods.FindTValueToTravelDistance_CubicBezier(posA, posB, handleA, handleB, distanceTravelled, length);

            //float dEst = MyVector3.Magnitude(InterpolationHelpMethods.EstimateDerivativeCubicBezier(posA, posB, handleA, handleB, t));
            //float dAct = MyVector3.Magnitude(InterpolationHelpMethods.DerivativeCubicBezier(posA, posB, handleA, handleB, t));

            //Debug.Log("Estimated derivative: " + dEst + " Actual derivative: " + dAct);

            //Debug.Log("Distance " + distanceTravelled);

            MyVector3 actualPos = _Interpolation.BezierCubic(posA, posB, handleA, handleB, actualT);

            actualPositions.Add(actualPos.ToVector3());


            distanceTravelled += lengthStepSize;
        }



        DisplayInterpolatedValues(actualPositions, useRandomColor: true);

        //Display the start and end values and the handle points
        DisplayHandle(handleA.ToVector3(), posA.ToVector3());
        DisplayHandle(handleB.ToVector3(), posB.ToVector3());
    }



    private void CatmullRom(MyVector3 a, MyVector3 b, MyVector3 c, MyVector3 d)
    {
        //Store the interpolated values so we later can display them
        List<Vector3> interpolatedValues = new List<Vector3>();

        //Make a connected shape by using all 4 points
        List<MyVector3> controlPoints = new List<MyVector3>() { a, b, c, d };

        //Loop through all points
        for (int i = 0; i < controlPoints.Count; i++)
        {
            MyVector3 p0 = controlPoints[MathUtility.ClampListIndex(i - 1, controlPoints.Count)];
            MyVector3 p1 = controlPoints[MathUtility.ClampListIndex(i + 0, controlPoints.Count)];
            MyVector3 p2 = controlPoints[MathUtility.ClampListIndex(i + 1, controlPoints.Count)];
            MyVector3 p3 = controlPoints[MathUtility.ClampListIndex(i + 2, controlPoints.Count)];

            interpolatedValues.AddRange(GetCatmullRomPoints(p0, p1, p2, p3));
        }



        DisplayInterpolatedValues(interpolatedValues, useRandomColor: true);

        Gizmos.color = Color.white;

        float radius = 0.1f;

        Gizmos.DrawWireSphere(a.ToVector3(), radius);
        Gizmos.DrawWireSphere(b.ToVector3(), radius);
        Gizmos.DrawWireSphere(c.ToVector3(), radius);
        Gizmos.DrawWireSphere(d.ToVector3(), radius);
    }

    //Get values between two points for CatmullRom
    private static List<Vector3> GetCatmullRomPoints(MyVector3 a, MyVector3 b, MyVector3 c, MyVector3 d)
    {
        //Store the interpolated values so we later can display them
        List<Vector3> interpolatedValues = new List<Vector3>();

        //Loop between 0 and 1 in steps, where 1 step is minimum
        //So if steps is 5 then the line will be cut in 5 sections
        int steps = 10;

        float stepSize = 1f / (float)steps;

        float t = 0f;

        //+1 becuase wa also have to include the first point
        for (int i = 0; i < steps + 1; i++)
        {
            //Debug.Log(t);

            MyVector3 interpolatedValue = _Interpolation.CatmullRom(a, b, c, d, t);

            interpolatedValues.Add(interpolatedValue.ToVector3());

            t += stepSize;
        }

        return interpolatedValues;
    }



    //Exponential interpolation
    private void Eerp(Vector3 a, Vector3 b)
    {
        //Store the interpolated values so we later can display them
        List<Vector3> interpolatedValues = new List<Vector3>();

        //Loop between 0 and 1 in steps, where 1 step is minimum
        //So if steps is 5 then the line will be cut in 5 sections
        int steps = 5;

        float stepSize = 1f / (float)steps;

        float t = 0f;

        //+1 becuase wa also have to include the first point
        for (int i = 0; i < steps + 1; i++)
        {
            //Debug.Log(t);

            //TODO will never go exactly between 0 and 1
            float exponential_t = 1f - Mathf.Exp(t - 1f);

            float interpolatedValueX = _Interpolation.Lerp(a.x, b.x, exponential_t);
            float interpolatedValueZ = _Interpolation.Lerp(a.z, b.z, exponential_t);

            Vector3 interpolatedValue = new Vector3(interpolatedValueX, 0f, interpolatedValueZ);

            interpolatedValues.Add(interpolatedValue);

            t += stepSize;
        }


        DisplayInterpolatedValues(interpolatedValues, useRandomColor: true);
    }



    //
    // Help methods
    //

    //Display interpolated values
    private void DisplayInterpolatedValues(List<Vector3> values, bool useRandomColor)
    {
        DisplayInterpolatedValues(values, useRandomColor: true, Color.white);
    }

    private void DisplayInterpolatedValues(List<Vector3> values, Color color)
    {
        DisplayInterpolatedValues(values, useRandomColor: false, color);
    }

    private void DisplayInterpolatedValues(List<Vector3> values, bool useRandomColor, Color color)
    {
        //Draw lines
        Random.InitState(seed);
    
        for (int i = 1; i < values.Count; i++)
        {
            if (!useRandomColor)
            {
                Gizmos.color = color;
            }
            else
            {
                Gizmos.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            }

            Gizmos.DrawLine(values[i - 1], values[i]);
        }


        //Draw points
        Gizmos.color = Color.black;

        for (int i = 0; i < values.Count; i++)
        {
            Gizmos.DrawWireSphere(values[i], 0.05f);
        }
    }



    //Display handle points
    private void DisplayHandle(Vector3 handlePos, Vector3 curvePos)
    {
        Gizmos.color = Color.white;

        Gizmos.DrawLine(handlePos, curvePos);

        Gizmos.DrawWireSphere(handlePos, 0.2f);
    }
    
}
