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


        //Interpolate between coordinates in 3d 

        //BezierLinearTest(posA, posB);

        //BezierQuadraticTest(posA, posB, handleA);

        //BezierQuadraticEqualStepsTest(posA, posB, handleA);

        //BezierCubicTest(posA, posB, handleA, handleB);

        BezierCubicEqualStepsTest(posA, posB, handleA, handleB);

        //CatmullRomTest(posA, posB, handleA, handleB);


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


        DisplayInterpolation.DisplayCurve(interpolatedValues, useRandomColor: true);
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
        DisplayInterpolation.DisplayCurve(interpolatedValues, useRandomColor: true);

        //Display the start and end values and the handle points
        DisplayInterpolation.DisplayHandle(handle.ToVector3(), posA.ToVector3());
        DisplayInterpolation.DisplayHandle(handle.ToVector3(), posB.ToVector3());



        //Display other related data
        //Get the forwrd dir of the point at t and display it
        MyVector3 forwardDir = BezierQuadratic.GetTangent(posA, posB, handle, tSliderValue);

        MyVector3 slidePos = BezierQuadratic.GetPosition(posA, posB, handle, tSliderValue);

        TestAlgorithmsHelpMethods.DisplayArrow(slidePos.ToVector3(), (slidePos + forwardDir * 2f).ToVector3(), 0.2f, Color.blue);

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(slidePos.ToVector3(), 0.15f);
    }



    private void BezierQuadraticEqualStepsTest(MyVector3 posA, MyVector3 posB, MyVector3 handle)
    {
        //Create a curve which is the data structure used in the following calculations
        BezierQuadratic bezierQuadratic = new BezierQuadratic(posA, posB, handle);


        //Step 1. Calculate the length of the entire curve
        //This is needed to so we know how long we should walk each step
        float lengthNaive = InterpolationHelpMethods.GetLength_Naive(bezierQuadratic, steps: 20, tEnd: 1f);

        float lengthExact = InterpolationHelpMethods.GetLength_SimpsonsRule(bezierQuadratic, tStart: 0f, tEnd: 1f);

        //Debug.Log("Naive length: " + lengthNaive + " Exact length: " + lengthExact);


        //Step 2. Convert the t's to be percentage along the curve
        //Save the accurate t at each position on the curve
        List<float> accurateTs = new List<float>();

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
            //float accurateT = InterpolationHelpMethods.Find_t_FromDistance_Iterative(bezierQuadratic, distanceTravelled, length);
            //Method 2
            float accurateT = InterpolationHelpMethods.Find_t_FromDistance_Lookup(bezierQuadratic, distanceTravelled, accumulatedDistances: null);

            accurateTs.Add(accurateT);

            //Test that the derivative calculations are working
            //float dEst = InterpolationHelpMethods.EstimateDerivative(bezierQuadratic, t);
            //float dAct = bezierQuadratic.GetDerivative(t);

            //Debug.Log("Estimated derivative: " + dEst + " Actual derivative: " + dAct);



            //Debug.Log("Distance " + distanceTravelled);

            //Move on to next iteration
            distanceTravelled += lengthStepSize;

            t += stepSize;
        }


        //Get the data we want from the curve

        //Store the interpolated values so we later can display them
        List<Vector3> actualPositions = new List<Vector3>();
        //
        List<Vector3> tangents = new List<Vector3>();
        //Orientation, which includes the tangent and position
        List<InterpolationTransform> orientations = new List<InterpolationTransform>();

        for (int i = 0; i < accurateTs.Count; i++)
        {
            float accurateT = accurateTs[i];
            
            MyVector3 actualPos = bezierQuadratic.GetPosition(accurateT);

            actualPositions.Add(actualPos.ToVector3());


            MyVector3 tangent = bezierQuadratic.GetTangent(accurateT);

            tangents.Add(tangent.ToVector3());


            //Orientation, which includes both position and tangent
            InterpolationTransform orientation = InterpolationTransform.GetTransform(bezierQuadratic, accurateT);

            orientations.Add(orientation);
        }



        //Display

        //Unity doesnt have a built-in method to display an accurate Qudratic bezier, so we have to create our own
        //DisplayInterpolation.DisplayBezierQuadratic(bezierQuadratic, Color.black);
        DisplayInterpolation.DisplayCurve(bezierQuadratic, Color.black);

        //DisplayInterpolation.DisplayCurve(actualPositions, useRandomColor: true);
        DisplayInterpolation.DisplayCurve(actualPositions, Color.gray);

        //Display the start and end values and the handle points
        DisplayInterpolation.DisplayHandle(handle.ToVector3(), posA.ToVector3());
        DisplayInterpolation.DisplayHandle(handle.ToVector3(), posB.ToVector3());


        //Stuff on the curve
        //DisplayInterpolation.DisplayDirections(actualPositions, tangents, 1f, Color.red);

        DisplayInterpolation.DisplayOrientations(orientations, 1f);
    }



    private void BezierCubicTest(MyVector3 posA, MyVector3 posB, MyVector3 handleA, MyVector3 handleB)
    {
        //Store the interpolated values so we later can display them
        List<Vector3> interpolatedValues = new List<Vector3>();

        //Loop between 0 and 1 in steps, where 1 step is minimum
        //So if steps is 5 then the line will be cut in 5 sections
        int steps = 20;

        float t_stepSize = 1f / (float)steps;

        float t = 0f;

        //+1 becuase wa also have to include the first point
        for (int i = 0; i < steps + 1; i++)
        {
            //Debug.Log(t);

            MyVector3 interpolatedPos = BezierCubic.GetPosition(posA, posB, handleA, handleB, t);

            interpolatedValues.Add(interpolatedPos.ToVector3());

            t += t_stepSize;
        }


        //The curve
        DisplayInterpolation.DisplayCurve(interpolatedValues, useRandomColor: true);

        //The start and end values and the handle points
        DisplayInterpolation.DisplayHandle(handleA.ToVector3(), posA.ToVector3());
        DisplayInterpolation.DisplayHandle(handleB.ToVector3(), posB.ToVector3());
    }



    private void BezierCubicEqualStepsTest(MyVector3 posA, MyVector3 posB, MyVector3 handleA, MyVector3 handleB)
    {
        //Create a curve which is the data structure used in the following calculations
        BezierCubic bezierCubic = new BezierCubic(posA, posB, handleA, handleB);


        //Step 1. Calculate the length of the entire curve
        //This is needed so we know for how long we should walk each step
        float lengthNaive = InterpolationHelpMethods.GetLength_Naive(bezierCubic, steps: 20, tEnd: 1f);

        float lengthExact = InterpolationHelpMethods.GetLength_SimpsonsRule(bezierCubic, tStart: 0f, tEnd: 1f);

        //Debug.Log("Naive length: " + lengthNaive + " Exact length: " + lengthExact);


        //Step 2. Convert the t's to be percentage along the curve
        //Save the accurate t at each position on the curve
        List<float> accurateTs = new List<float>();

        //The number of sections we want to divide the curve into
        int steps = 6;

        //Important not to confuse this with the step size we use to iterate t
        //This step size is distance in m
        float curveLength = lengthNaive;

        float curveLength_stepSize = curveLength / (float)steps;

        float t_stepSize = 1f / (float)steps;

        float t = 0f;

        float distanceTravelled = 0f;

        for (int i = 0; i < steps + 1; i++)
        {
            //MyVector3 inaccuratePos = bezierCubic.GetPosition(t);

            //Calculate the t needed to get to this distance along the curve
            //Method 1
            //float accurateT = InterpolationHelpMethods.Find_t_FromDistance_Iterative(bezierCubic, distanceTravelled, length);
            //Method 2
            float accurateT = InterpolationHelpMethods.Find_t_FromDistance_Lookup(bezierCubic, distanceTravelled, accumulatedDistances: null);

            accurateTs.Add(accurateT);

            //Debug.Log(accurateT);


            //Test that the derivative calculations are working
            //float dEst = InterpolationHelpMethods.EstimateDerivative(bezierCubic, t);
            //float dAct = bezierCubic.ExactDerivative(t);

            //Debug.Log("Estimated derivative: " + dEst + " Actual derivative: " + dAct);

            //Debug.Log("Distance " + distanceTravelled);


            //Move on to next iteration
            distanceTravelled += curveLength_stepSize;

            t += t_stepSize;
        }


        //Step3. Use the new t's to get information from the curve

        //The interpolated positions
        List<Vector3> actualPositions = new List<Vector3>();
        //Save the tangent at each position on the curve
        List<Vector3> tangents = new List<Vector3>();
        //Save the orientation, which includes the tangent
        List<InterpolationTransform> orientations = new List<InterpolationTransform>();

        for (int i = 0; i < accurateTs.Count; i++)
        {
            float accurateT = accurateTs[i];

            //Position on the curve
            MyVector3 actualPos = bezierCubic.GetPosition(accurateT);

            actualPositions.Add(actualPos.ToVector3());

            //Tangent at each position
            MyVector3 tangentDir = BezierCubic.GetTangent(posA, posB, handleA, handleB, accurateT);

            tangents.Add(tangentDir.ToVector3());

            //Orientation, which includes both position and tangent
            InterpolationTransform orientation = InterpolationTransform.GetTransform(bezierCubic, accurateT);

            orientations.Add(orientation);
        }


        //The orientation at each position by using "Rotation Minimising Frame"
        List<InterpolationTransform> orientationsFrame = InterpolationTransform.GetTransforms(bezierCubic, accurateTs);


        //Display stuff

        //The curve which is split into steps
        //DisplayInterpolation.DisplayCurve(actualPositions, useRandomColor: true);
        DisplayInterpolation.DisplayCurve(actualPositions, Color.gray);

        //The start and end values and the handle points
        DisplayInterpolation.DisplayHandle(handleA.ToVector3(), posA.ToVector3());
        DisplayInterpolation.DisplayHandle(handleB.ToVector3(), posB.ToVector3());

        //The actual Bezier cubic for reference
        DisplayInterpolation.DisplayCurve(bezierCubic, Color.black);
        //Handles.DrawBezier(posA.ToVector3(), posB.ToVector3(), handleA.ToVector3(), handleB.ToVector3(), Color.black, EditorGUIUtility.whiteTexture, 1f);

        //The tangents
        //DisplayInterpolation.DisplayDirections(actualPositions, tangents, 1f, Color.red);

        //The orientation
        //DisplayInterpolation.DisplayOrientations(orientations, 1f);
        DisplayInterpolation.DisplayOrientations(orientationsFrame, 1f);

        //Extrude mesh along the curve
        InterpolationTransform testTrans = orientationsFrame[1];

        MyVector3 pos = testTrans.LocalToWorld(MyVector3.Up * 2f);

        Gizmos.DrawSphere(pos.ToVector3(), 0.1f);
    }



    private void CatmullRomTest(MyVector3 posA, MyVector3 posB, MyVector3 handleA, MyVector3 handleB)
    {
        CatmullRom catmullRomCurve = new CatmullRom(posA, posB, handleA, handleB);
    
        //Store the interpolated values so we later can display them
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> tangents = new List<Vector3>();

        //Loop between 0 and 1 in steps, where 1 step is minimum
        //So if steps is 5 then the line will be cut in 5 sections
        int steps = 5;

        float stepSize = 1f / (float)steps;

        float t = 0f;

        //+1 becuase wa also have to include the first point
        for (int i = 0; i < steps + 1; i++)
        {
            //Debug.Log(t);

            MyVector3 interpolatedPos = CatmullRom.GetPosition(posA, posB, handleA, handleB, t);

            positions.Add(interpolatedPos.ToVector3());

            MyVector3 interpolatedTangent = CatmullRom.GetTangent(posA, posB, handleA, handleB, t);

            tangents.Add(interpolatedTangent.ToVector3());

            t += stepSize;
        }


        //Display
        //DisplayInterpolation.DisplayCurve(positions, useRandomColor: true);
        DisplayInterpolation.DisplayCurve(positions, Color.black);

        //The actual curve for comparison
        DisplayInterpolation.DisplayCurve(catmullRomCurve, Color.gray);

        //The control points
        //The start and end values and the handle points
        DisplayInterpolation.DisplayHandle(handleA.ToVector3(), posA.ToVector3());
        DisplayInterpolation.DisplayHandle(handleB.ToVector3(), posB.ToVector3());

        //Other stuff
        DisplayInterpolation.DisplayDirections(positions, tangents, 1f, Color.blue);
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


        DisplayInterpolation.DisplayCurve(interpolatedValues, useRandomColor: true);
    }
}
