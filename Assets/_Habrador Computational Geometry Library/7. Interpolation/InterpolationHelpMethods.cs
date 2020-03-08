using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Help methods related to interpolation
    public static class InterpolationHelpMethods
    {
        //
        // Calculate length of curve
        //

        //Get the length of the curve with a naive method where we divide the
        //curve into straight lines and then measure the length of each line
        //tEnd is 1 if we want to get the length of the entire curve
        public static float GetLengthNaive_CubicBezier(MyVector3 posA, MyVector3 posB, MyVector3 handleA, MyVector3 handleB, int steps, float tEnd)
        {
            if (steps < 1)
            {
                Debug.Log("Can't calculate the length because too few steps");

                return 0f;
            }
        
            //Store the interpolated values so we later can display them
            List<MyVector3> interpolatedValues = new List<MyVector3>();

            //Loop between 0 and tStop in steps. If tStop is 1 we loop through the entire curve
            //1 step is minimum, so if steps is 5 then the line will be cut in 5 sections
            float stepSize = tEnd / (float)steps;

            float t = 0f;

            //+1 becuase wa also have to include the first point
            for (int i = 0; i < steps + 1; i++)
            {
                //Debug.Log(t);

                MyVector3 interpolatedValue = _Interpolation.BezierCubic(posA, posB, handleA, handleB, t);

                interpolatedValues.Add(interpolatedValue);

                t += stepSize;
            }


            //Calculate the length by measuring the length of each step
            float length = 0f;

            for (int i = 1; i < interpolatedValues.Count; i++)
            {
                float thisStepLength = MyVector3.Distance(interpolatedValues[i - 1], interpolatedValues[i]);

                length += thisStepLength;
            }

            return length;
        }



        //Get the length by using Simpson's Rule (not related to the television show)
        //https://www.youtube.com/watch?v=J_a4PXI_nLY
        //The basic idea is that we cut the curve into sections and each section is approximated by a polynom 
        public static float GetLengthSimpsonsRule_CubicBezier(MyVector3 posA, MyVector3 posB, MyVector3 handleA, MyVector3 handleB, float tStart, float tEnd)
        {
            //Divide the curve into sections
            
            //How many sections?
            int n = 10;

            //The width of each section
            float delta = (tEnd - tStart) / (float)n;


            //The main loop to calculate the length

            //Everything multiplied by 1
            float derivativeStart = MyVector3.Magnitude(Derivative_CubicBezier(posA, posB, handleA, handleB, tStart));
            float derivativeEnd = MyVector3.Magnitude(Derivative_CubicBezier(posA, posB, handleA, handleB, tEnd));

            float endPoints = derivativeStart + derivativeEnd;


            //Everything multiplied by 4
            float x4 = 0f;
            for (int i = 1; i < n; i += 2)
            {
                float t = tStart + delta * i;

                x4 += MyVector3.Magnitude(Derivative_CubicBezier(posA, posB, handleA, handleB, t));
            }


            //Everything multiplied by 2
            float x2 = 0f;
            for (int i = 2; i < n; i += 2)
            {
                float t = tStart + delta * i;

                x2 += MyVector3.Magnitude(Derivative_CubicBezier(posA, posB, handleA, handleB, t));
            }


            //The final length
            float length = (delta / 3f) * (endPoints + 4f * x4 + 2f * x2);


            return length;
        }



        //
        // Calculate the derivative at a point on a curve
        //

        //Alternative 1. Estimate the derivative at point t
        //https://www.youtube.com/watch?v=jvYZNp5myXg
        //https://www.alanzucconi.com/2017/04/10/robotic-arms/
        public static MyVector3 EstimateDerivative_CubicBezier(MyVector3 posA, MyVector3 posB, MyVector3 handleA, MyVector3 handleB, float t)
        {
            //We can estimate the derivative by taking a step in each direction of the point we are interested in
            //Should be around this number
            float derivativeStepSize = 0.0001f;

            MyVector3 valueMinus = _Interpolation.BezierCubic(posA, posB, handleA, handleB, t - derivativeStepSize);
            MyVector3 valuePlus = _Interpolation.BezierCubic(posA, posB, handleA, handleB, t + derivativeStepSize);

            //Have to multiply by two because we are taking a step in each direction
            MyVector3 derivativeVector = (valuePlus - valueMinus) * (1f / (derivativeStepSize * 2f));

            return derivativeVector;
        }



        //Alternative 2. Actual derivative at point t
        public static MyVector3 Derivative_CubicBezier(MyVector3 posA, MyVector3 posB, MyVector3 handleA, MyVector3 handleB, float t)
        {
            MyVector3 A = posA;
            MyVector3 B = handleA;
            MyVector3 C = handleB;
            MyVector3 D = posB;

            MyVector3 derivativeVector = t * t * (-3f * (A - 3f * (B - C) - D));

            derivativeVector += t * (6f * (A - 2f * B + C));

            derivativeVector += -3f * (A - B);

            return derivativeVector;
        }



        //
        // Divide the curve into equal steps
        //

        //Use Newton–Raphsons method to find which t value we need to travel distance d on the curve
        //d is measured from the start of the curve in [m] so is not the same as paramete t which is [0, 1]
        //https://en.wikipedia.org/wiki/Newton%27s_method
        //https://www.youtube.com/watch?v=-mad4YPAn2U
        public static float FindTValueToTravelDistance_CubicBezier(MyVector3 posA, MyVector3 posB, MyVector3 handleA, MyVector3 handleB, float d, float totalLength)
        {
            //Need a start value to make the method start
            //Should obviously be between 0 and 1
            //We can say that a good starting point is the percentage of distance traveled
            //If this start value is not working you can use the Bisection Method to find a start value
            //https://en.wikipedia.org/wiki/Bisection_method
            float t = d / totalLength;
            
            //Need an error so we know when to stop the iteration
            float error = 0.001f;

            //We also need to avoid infinite loops
            int iterations = 0;

            while (true)
            {
                //The derivative and the length can be calculated in different ways
            
                //The derivative vector at point t
                MyVector3 derivativeVec = EstimateDerivative_CubicBezier(posA, posB, handleA, handleB, t);
                //MyVector3 derivativeVec = DerivativeCubicBezier(posA, posB, handleA, handleB, t);

                //The length of the curve to point t from the start
                //float lengthTo_t = GetLengthNaiveCubicBezier(posA, posB, handleA, handleB, steps: 20, tEnd: t);
                float lengthTo_t = GetLengthSimpsonsRule_CubicBezier(posA, posB, handleA, handleB, tStart: 0f, tEnd: t);


                //Calculate a better t with Newton's method: x_n+1 = x_n + (f(x_n) / f'(x_n))
                //Our f(x) = lengthTo_t - d = 0. We want them to be equal because we want to find the t value 
                //that generates a distance(t) which is the same as the d we want. So when f(x) is close to zero we are happy
                //When we take the derivative of f(x), d disappears which is why we are not subtracting it in the bottom
                float tNext = t - ((lengthTo_t - d) / MyVector3.Magnitude(derivativeVec));


                //Have we reached the desired accuracy?
                float diff = tNext - t;

                t = tNext;

                //Have we found a t to get a distance which matches the distance we want?  
                if (diff < error && diff > -error)
                {
                    //Debug.Log("d: " + d + " t: " + t + " Distance: " + GetLengthSimpsonsCubicBezier(posA, posB, handleA, handleB, tStart: 0f, tEnd: tNext));

                    break;
                }


                //Safety so we don't get stuck in an infinite loop
                iterations += 1;

                if (iterations > 1000)
                {
                    Debug.Log("Couldnt find a t value within the iteration limit");

                    break;
                }
            }


            return t;
        }

    }
}
