using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using UnityEditor;

public class InterpolationController : MonoBehaviour
{
    public Transform transPointA;
    public Transform transPointB;
    public Transform transHandleA;
    public Transform transHandleB;

    private int seed = 0;

    //t slider for experiments
    [Range(0f, 1f)]
    public float tSliderValue = 0f;



    private void OnDrawGizmos()
    {
        //3d
        MyVector3 posA = transPointA.position.ToMyVector3();
        MyVector3 posB = transPointB.position.ToMyVector3();

        MyVector3 handleA = transHandleA.position.ToMyVector3();
        MyVector3 handleB = transHandleB.position.ToMyVector3();


        //Interpolate between cooldinates

        //Bezier curves

        //BezierLinearTest(posA, posB);

        //BezierQuadraticTest(posA, posB, handleA);

        //BezierQuadraticEqualStepsTest(posA, posB, handleA);

        //BezierCubicTest(posA, posB, handleA, handleB);

        BezierCubicEqualStepsTest(posA, posB, handleA, handleB);

        //CatmullRomTest(posA, handleA, handleB, posB);


        //Interpolation between values
        //OtherInterpolations(posA, posB);
    }



    private void BezierLinearTest(MyVector3 posA, MyVector3 posB)
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

            MyVector3 interpolatedValue = BezierLinear.GetPosition(posA, posB, t);

            interpolatedValues.Add(interpolatedValue.ToVector3());

            t += stepSize;
        }


        DisplayInterpolatedValues(interpolatedValues, useRandomColor: true);
    }



    private void BezierQuadraticTest(MyVector3 posA, MyVector3 posB, MyVector3 handle)
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

            MyVector3 interpolatedValue = BezierQuadratic.GetPosition(posA, posB, handle, t);

            interpolatedValues.Add(interpolatedValue.ToVector3());

            t += stepSize;
        }


        //Display the curve
        DisplayInterpolatedValues(interpolatedValues, useRandomColor: true);

        //Display the start and end values and the handle points
        DisplayHandle(handle.ToVector3(), posA.ToVector3());
        DisplayHandle(handle.ToVector3(), posB.ToVector3());



        //Display other related data
        //Get the forwrd dir of the point at t and display it
        MyVector3 forwardDir = BezierQuadratic.GetForwardDir(posA, posB, handle, tSliderValue);

        MyVector3 slidePos = BezierQuadratic.GetPosition(posA, posB, handle, tSliderValue);

        TestAlgorithmsHelpMethods.DisplayArrow(slidePos.ToVector3(), (slidePos + forwardDir * 2f).ToVector3(), 0.2f, Color.blue);

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(slidePos.ToVector3(), 0.15f);
    }



    private void BezierQuadraticEqualStepsTest(MyVector3 posA, MyVector3 posB, MyVector3 handle)
    {
        //Store the interpolated values so we later can display them
        List<Vector3> actualPositions = new List<Vector3>();

        //Create a curve which is the data structure used in the following calculations
        BezierQuadratic bezierQuadratic = new BezierQuadratic(posA, posB, handle);


        //Step 1. Calculate the length of the entire curve
        //This is needed to so we know how long we should walk each step
        float lengthNaive = InterpolationHelpMethods.GetLength_Naive(bezierQuadratic, steps: 20, tEnd: 1f);

        float lengthExact = InterpolationHelpMethods.GetLength_SimpsonsRule(bezierQuadratic, tStart: 0f, tEnd: 1f);

        Debug.Log("Naive length: " + lengthNaive + " Exact length: " + lengthExact);





        int steps = 5;

        //Important not to confuse this with the step size we use to iterate t
        //This step size is distance in m
        float length = lengthNaive;

        float lengthStepSize = length / (float)steps;

        float stepSize = 1f / (float)steps;

        float t = 0f;

        float distanceTravelled = 0f;

        for (int i = 0; i < steps + 1; i++)
        {
            //MyVector3 inaccuratePos = bezierCubic.GetInterpolatedValue(t);



            //Calculate t to get to this distance
            //Method 1
            //float actualT = InterpolationHelpMethods.Find_t_FromDistance_Iterative(bezierQuadratic, distanceTravelled, length);
            //Method 2
            float actualT = InterpolationHelpMethods.Find_t_FromDistance_Lookup(bezierQuadratic, distanceTravelled, accumulatedDistances: null);

            MyVector3 actualPos = bezierQuadratic.GetPosition(actualT);

            actualPositions.Add(actualPos.ToVector3());



            //Test that the derivative calculations are working
            float dEst = InterpolationHelpMethods.EstimateDerivative(bezierQuadratic, t);
            float dAct = bezierQuadratic.ExactDerivative(t);

            Debug.Log("Estimated derivative: " + dEst + " Actual derivative: " + dAct);



            //Debug.Log("Distance " + distanceTravelled);

            //Move on to next iteration
            distanceTravelled += lengthStepSize;

            t += stepSize;
        }

        //List<MyVector3> positionsOnCurve = InterpolationHelpMethods.SplitCurve(bezierQuadratic, 20, tEnd: 1f);

        //foreach (MyVector3 p in positionsOnCurve)
        //{
        //    Gizmos.DrawWireSphere(p.ToVector3(), 0.1f);
        //}

        DisplayInterpolatedValues(actualPositions, useRandomColor: true);

        //Display the start and end values and the handle points
        DisplayHandle(handle.ToVector3(), posA.ToVector3());
        DisplayHandle(handle.ToVector3(), posB.ToVector3());
    }



    private void BezierCubicTest(MyVector3 posA, MyVector3 posB, MyVector3 handleA, MyVector3 handleB)
    {
        //Store the interpolated values so we later can display them
        List<Vector3> interpolatedValues = new List<Vector3>();

        //Loop between 0 and 1 in steps, where 1 step is minimum
        //So if steps is 5 then the line will be cut in 5 sections
        int steps = 20;

        float stepSize = 1f / (float)steps;

        float t = 0f;

        //+1 becuase wa also have to include the first point
        for (int i = 0; i < steps + 1; i++)
        {
            //Debug.Log(t);

            MyVector3 interpolatedPos = BezierCubic.GetPosition(posA, posB, handleA, handleB, t);

            interpolatedValues.Add(interpolatedPos.ToVector3());

            t += stepSize;
        }


        //Display the curve
        DisplayInterpolatedValues(interpolatedValues, useRandomColor: true);

        //Display the start and end values and the handle points
        DisplayHandle(handleA.ToVector3(), posA.ToVector3());
        DisplayHandle(handleB.ToVector3(), posB.ToVector3());


        //Display other related data
        //Get the orientation of the point at t
        BezierCubic bezierCubic = new BezierCubic(posA, posB, handleA, handleB);

        InterpolationTransform trans = bezierCubic.GetTransform(tSliderValue);

        //Multiply the orientation with a direction vector to rotate the direction
        Vector3 forwardDir = trans.Forward.ToVector3();
        //- right vector because in this test files we are looking from above
        //so -right looks like up even though in the actual coordinate system it is -right
        Vector3 upDir = -trans.Right.ToVector3();

        Vector3 slidePos = BezierCubic.GetPosition(posA, posB, handleA, handleB, tSliderValue).ToVector3();

        TestAlgorithmsHelpMethods.DisplayArrow(slidePos, slidePos + forwardDir * 2f, 0.2f, Color.blue);
        TestAlgorithmsHelpMethods.DisplayArrow(slidePos, slidePos + upDir * 2f, 0.2f, Color.blue);

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(slidePos, 0.15f);
    }


    private void BezierCubicEqualStepsTest(MyVector3 posA, MyVector3 posB, MyVector3 handleA, MyVector3 handleB)
    {
        //Store the interpolated values so we later can display them
        List<Vector3> actualPositions = new List<Vector3>();

        //Create a curve which is the data structure used in the following calculations
        BezierCubic bezierCubic = new BezierCubic(posA, posB, handleA, handleB);


        //Step 1. Calculate the length of the entire curve
        //This is needed so we know for how long we should walk each step
        float lengthNaive = InterpolationHelpMethods.GetLength_Naive(bezierCubic, steps: 20, tEnd: 1f);

        float lengthExact = InterpolationHelpMethods.GetLength_SimpsonsRule(bezierCubic, tStart: 0f, tEnd: 1f);

        //Debug.Log("Naive length: " + lengthNaive + " Exact length: " + lengthExact);


        //If we want to display the tangent at each position on the curve
        List<Vector3> tangents = new List<Vector3>();


        int steps = 5;

        //Important not to confuse this with the step size we use to iterate t
        //This step size is distance in m
        float length = lengthNaive;

        float lengthStepSize = length / (float)steps;

        float stepSize = 1f / (float)steps;

        float t = 0f;

        float distanceTravelled = 0f;

        for (int i = 0; i < steps + 1; i++)
        {
            //MyVector3 inaccuratePos = bezierCubic.GetInterpolatedValue(t);



            //Calculate the t needed to get to this distance along the curve
            //Method 1
            //float actualT = InterpolationHelpMethods.Find_t_FromDistance_Iterative(bezierCubic, distanceTravelled, length);
            //Method 2
            float actualT = InterpolationHelpMethods.Find_t_FromDistance_Lookup(bezierCubic, distanceTravelled, accumulatedDistances: null);

            MyVector3 actualPos = bezierCubic.GetPosition(actualT);

            actualPositions.Add(actualPos.ToVector3());


            //Test that the derivative calculations are working
            //float dEst = InterpolationHelpMethods.EstimateDerivative(bezierCubic, t);
            //float dAct = bezierCubic.ExactDerivative(t);

            //Debug.Log("Estimated derivative: " + dEst + " Actual derivative: " + dAct);


            //Calculate the tangent at each position
            MyVector3 tangentDir = BezierCubic.GetForwardDir(posA, posB, handleA, handleB, actualT);

            tangents.Add(tangentDir.ToVector3());


            //Debug.Log("Distance " + distanceTravelled);


            //Move on to next iteration
            distanceTravelled += lengthStepSize;

            t += stepSize;
        }


        //Display stuff

        //List<MyVector3> positionsOnCurve = InterpolationHelpMethods.SplitCurve(bezierCubic, 20, tEnd: 1f);

        //foreach (MyVector3 p in positionsOnCurve)
        //{
        //    Gizmos.DrawWireSphere(p.ToVector3(), 0.1f);
        //}

        DisplayInterpolatedValues(actualPositions, useRandomColor: true);

        //Display the start and end values and the handle points
        DisplayHandle(handleA.ToVector3(), posA.ToVector3());
        DisplayHandle(handleB.ToVector3(), posB.ToVector3());


        //Display the actual Bezier cubic for reference
        Handles.DrawBezier(posA.ToVector3(), posB.ToVector3(), handleA.ToVector3(), handleB.ToVector3(), Color.blue, EditorGUIUtility.whiteTexture, 1f);


        //Display the tangents
        DisplayDirections(actualPositions, tangents, 1f, Color.red);
    }



    private void CatmullRomTest(MyVector3 a, MyVector3 b, MyVector3 c, MyVector3 d)
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
    private static List<Vector3> GetCatmullRomPoints(MyVector3 posA, MyVector3 posB, MyVector3 handleA, MyVector3 handleB)
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

            MyVector3 interpolatedValue = CatmullRom.GetPosition(posA, posB, handleA, handleB, t);

            interpolatedValues.Add(interpolatedValue.ToVector3());

            t += stepSize;
        }

        return interpolatedValues;
    }



    //Interpolation between values
    private void OtherInterpolations(MyVector3 a, MyVector3 b)
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

            //Ease out
            //float interpolatedValueX = _Interpolation.Sinerp(a.x, b.x, t);
            //float interpolatedValueZ = _Interpolation.Sinerp(a.z, b.z, t);


            //Ease in
            //float interpolatedValueX = _Interpolation.Coserp(a.x, b.x, t);
            //float interpolatedValueZ = _Interpolation.Coserp(a.z, b.z, t);


            //Exponential
            //float interpolatedValueX = _Interpolation.Eerp(a.x, b.x, t);
            //float interpolatedValueZ = _Interpolation.Eerp(a.z, b.z, t);


            //Smoothstep and Smootherstep
            float interpolatedValueX = _Interpolation.Smoothersteperp(a.x, b.x, t);
            float interpolatedValueZ = _Interpolation.Smoothersteperp(a.z, b.z, t);


            //Similar to bezier cubic
            //float handleA = 0f;
            //float handleB = 0.5f;
            //float interpolatedValueX = _Interpolation.CubicBezierErp(a.x, b.x, handleA, handleB, t);
            //float interpolatedValueZ = _Interpolation.CubicBezierErp(a.z, b.z, handleA, handleB, t);

            Vector3 interpolatedPos = new Vector3(interpolatedValueX, 0f, interpolatedValueZ);

            interpolatedValues.Add(interpolatedPos);

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
    


    //Display rays
    private void DisplayDirections(List<Vector3> startPos, List<Vector3> rayDir, float rayLength, Color color)
    {
        for (int i = 0; i < startPos.Count; i++)
        {
            Debug.DrawRay(startPos[i], rayDir[i] * rayLength, color);
        }
    }
}
