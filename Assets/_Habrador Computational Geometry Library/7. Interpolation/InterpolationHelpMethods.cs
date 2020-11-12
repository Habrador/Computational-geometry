using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //Help methods related to interpolation
    public static class InterpolationHelpMethods
    {
        //
        // Split a curve into sections with some resolution
        //

        //Steps is the number of sections we are going to split the curve in
        //So the number of interpolated values are steps + 1
        //tEnd is where we want to stop measuring if we dont want to split the entire curve, so tEnd is maximum of 1
        public static List<MyVector3> SplitCurve(_Curve curve, int steps, float tEnd)
        {
            //Store the interpolated values so we later can display them
            List<MyVector3> interpolatedPositions = new List<MyVector3>();

            //Loop between 0 and tStop in steps. If tStop is 1 we loop through the entire curve
            //1 step is minimum, so if steps is 5 then the line will be cut in 5 sections
            float stepSize = tEnd / (float)steps;

            float t = 0f;

            //+1 becuase wa also have to include the first point
            for (int i = 0; i < steps + 1; i++)
            {
                //Debug.Log(t);

                MyVector3 interpolatedValue = curve.GetPosition(t);

                interpolatedPositions.Add(interpolatedValue);

                t += stepSize;
            }

            return interpolatedPositions;
        }
    
    

        //
        // Calculate length of curve
        //

        //Get the length of the curve with a naive method where we divide the
        //curve into straight lines and then measure the length of each line
        //tEnd is 1 if we want to get the length of the entire curve
        public static float GetLength_Naive(_Curve curve, int steps, float tEnd)
        {
            //Split the ruve into positions with some steps resolution
            List<MyVector3> CurvePoints = SplitCurve(curve, steps, tEnd);

            //Calculate the length by measuring the length of each step
            float length = 0f;

            for (int i = 1; i < CurvePoints.Count; i++)
            {
                float thisStepLength = MyVector3.Distance(CurvePoints[i - 1], CurvePoints[i]);

                length += thisStepLength;
            }

            return length;
        }



        //Get the length by using Simpson's Rule (not related to the television show)
        //https://www.youtube.com/watch?v=J_a4PXI_nLY
        //The basic idea is that we cut the curve into sections and each section is approximated by a polynom 
        public static float GetLength_SimpsonsRule(_Curve curve, float tStart, float tEnd)
        {
            //Divide the curve into sections
            
            //How many sections?
            int n = 10;

            //The width of each section
            float delta = (tEnd - tStart) / (float)n;


            //The main loop to calculate the length

            //Everything multiplied by 1
            float derivativeStart = curve.GetDerivative(tStart);
            float derivativeEnd = curve.GetDerivative(tEnd);

            float endPoints = derivativeStart + derivativeEnd;


            //Everything multiplied by 4
            float x4 = 0f;
            for (int i = 1; i < n; i += 2)
            {
                float t = tStart + delta * i;

                x4 += curve.GetDerivative(t);
            }


            //Everything multiplied by 2
            float x2 = 0f;
            for (int i = 2; i < n; i += 2)
            {
                float t = tStart + delta * i;

                x2 += curve.GetDerivative(t);
            }


            //The final length
            float length = (delta / 3f) * (endPoints + 4f * x4 + 2f * x2);


            return length;
        }
    


        //
        // Divide the curve into constant steps
        //

        //The problem with the t-value and Bezier curves is that the t-value is NOT percentage along the curve
        //So sometimes we need to divide the curve into equal steps, for example if we generate a mesh along the curve
        //So we have a distance along the curve and we want to find the t-value that generates that distance

        //Alternative 1
        //Use Newton–Raphsons method to find which t value we need to travel distance d on the curve
        //d is measured from the start of the curve in [m] so is not the same as paramete t which is [0, 1]
        //https://en.wikipedia.org/wiki/Newton%27s_method
        //https://www.youtube.com/watch?v=-mad4YPAn2U
        //TODO: We can use the lookup table from the lookup-method in the newton-raphson method!
        public static float Find_t_FromDistance_Iterative(_Curve curve, float d, float totalLength)
        {
            //Need a start value to make the method start
            //Should obviously be between 0 and 1
            //We can say that a good starting point is the percentage of distance traveled
            //If this start value is not working you can use the Bisection Method to find a start value
            //https://en.wikipedia.org/wiki/Bisection_method
            float tGood = d / totalLength;
            
            //Need an error so we know when to stop the iteration
            float error = 0.001f;

            //We also need to avoid infinite loops
            int iterations = 0;

            while (true)
            {
                //The derivative and the length can be calculated in different ways

                //The derivative at point t
                float derivative = curve.GetDerivative(tGood);

                //The length of the curve to point t from the start
                //float lengthTo_t = GetLengthNaive_CubicBezier(pA, pB, hA, hB, steps: 20, tEnd: t);
                float lengthTo_t = GetLength_SimpsonsRule(curve, tStart: 0f, tEnd: tGood);


                //Calculate a better t with Newton's method: x_n+1 = x_n + (f(x_n) / f'(x_n))
                //Our f(x) = lengthTo_t - d = 0. We want them to be equal because we want to find the t value 
                //that generates a distance(t) which is the same as the d we want. So when f(x) is close to zero we are happy
                //When we take the derivative of f(x), d disappears which is why we are not subtracting it in the bottom
                float tNext = tGood - ((lengthTo_t - d) / derivative);


                //Have we reached the desired accuracy?
                float diff = tNext - tGood;

                tGood = tNext;

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


            
            return tGood;
        }



        //Alternative 2
        //Create a lookup-table with distances along the curve, then interpolate these distances
        //This is faster but less accurate than using the iterative Newton–Raphsons method
        //But the difference from far away is barely noticeable
        //https://medium.com/@Acegikmo/the-ever-so-lovely-b%C3%A9zier-curve-eb27514da3bf
        public static float Find_t_FromDistance_Lookup(_Curve curve, float d, List<float> accumulatedDistances)
        {
            //Step 1. Find accumulated distances along the curve by using the bad t-value
            //This value can be pre-calculated
            if (accumulatedDistances == null)
            {
                accumulatedDistances = GetAccumulatedDistances(curve, steps: 100);
            }

            if (accumulatedDistances == null || accumulatedDistances.Count == 0)
            {
                throw new System.Exception("Cant interpolate to split bezier into equal steps");
            }



            //Step 2. Iterate through the table, to find at what index we reach the desired distance
            //It will most likely not end up exactly at an index, so we need to interpolate by using the 
            //index to the left and the index to the right

            //First we need special cases for end-points to avoid unnecessary calculations
            if (d <= accumulatedDistances[0])
            {
                return 0f;
            }
            else if (d >= accumulatedDistances[accumulatedDistances.Count - 1])
            {
                return 1f;
            }


            //Find the index to the left
            int indexLeft = 0;

            for (int i = 0; i < accumulatedDistances.Count - 1; i++)
            {
                if (accumulatedDistances[i + 1] > d)
                {
                    indexLeft = i;

                    break;
                }
            }

            //Step 3. Interpolate to get the t-value
            //Each distance also has a t-value we used to generate that distance
            
            //Each t in the list is increasing each step by: 
            float stepSize = 1f / (float)(accumulatedDistances.Count - 1);

            //With this step size we can calculate the t-value of the index
            float tValueL = indexLeft * stepSize;
            //The index right is just:
            float tValueR = (indexLeft + 1) * stepSize;

            //To interpolate we need a percentage between the left index and the right index in the distances array
            float percentage = (d - accumulatedDistances[indexLeft]) / (accumulatedDistances[indexLeft + 1] - accumulatedDistances[indexLeft]);

            float tGood = _Interpolation.Lerp(tValueL, tValueR, percentage);



            return tGood;
        }



        //
        // Get actual percentage along curve
        //

        //Parameter t is not always percentage along the curve
        //Sometimes we need to calculate the actual percentage if t had been percentage along the curve
        //From https://www.youtube.com/watch?v=o9RK6O2kOKo
        public static float FindPercentageAlongCurve(_Curve curve, float tBad, List<float> accumulatedDistances)
        {
            //Step 1. Find accumulated distances along the curve by using the bad t-value
            //This value can be pre-calculated
            if (accumulatedDistances == null)
            {
                accumulatedDistances = GetAccumulatedDistances(curve);
            }

            //The length of the entire curve
            float totalDistance = accumulatedDistances[accumulatedDistances.Count - 1];


            //Step 2. Find the positions in the distances list tBad is closest to and interpolate between them
            if (accumulatedDistances == null || accumulatedDistances.Count == 0)
            {
                Debug.Log("Cant interpolate to find exact percentage along curve");
                
                return 0f;
            }

            //If we have just one value, just return it
            if (accumulatedDistances.Count == 1)
            {
                return accumulatedDistances[0] / totalDistance;
            }


            //Convert the t-value to an array position
            //t-value can be seen as percentage, so we get percentage along the list of values
            //If we have 5 values in the list, we have 4 buckets, so if t is 0.65, we get 0.65*4 = 2.6
            float arrayPosBetween = tBad * (float)(accumulatedDistances.Count - 1);
            
            //Round up and down to get the actual array positions
            int arrayPosL = Mathf.FloorToInt(arrayPosBetween); //2 if we follow the example above 
            int arrayPosR = Mathf.FloorToInt(arrayPosBetween + 1f); //2.6 + 1 = 3.6 -> 3 

            //If we reached too high return the last value
            if (arrayPosR >= accumulatedDistances.Count)
            {
                return accumulatedDistances[accumulatedDistances.Count - 1] / totalDistance;
            }
            //Too low
            else if (arrayPosR < 0f)
            {
                return accumulatedDistances[0] / totalDistance;
            }

            //Interpolate by lerping
            float percentage = arrayPosBetween - arrayPosL; //2.6 - 2 = 0.6 if we follow the example above 

            //(1f - t) * a + t * b; so if percentage is 0.6 we should get more of the one to the right
            float interpolatedDistance = _Interpolation.Lerp(accumulatedDistances[arrayPosL], accumulatedDistances[arrayPosR], percentage);

            //This is the actual t-value that we should have used to get to this distance
            //So if tBad is 0.8 it doesnt mean that we have travelled 80 percent along the curve
            //If tBad is 0.8 and tActual is 0.7, it means that we have actually travelled 70 percent along the curve
            float tActual = interpolatedDistance / totalDistance;

            //Debug.Log("t-bad: " + tBad + " t-actual: " + tActual);

            

            return tActual;
        }



        //
        // Calculate the accumulated total distances along the curve by walking along it with constant t-steps
        //
        public static List<float> GetAccumulatedDistances(_Curve curve, int steps = 20)
        {
            //Step 1. Find positions on the curve by using the inaccurate t-value
            List<MyVector3> positionsOnCurve = SplitCurve(curve, steps, tEnd: 1f);


            //Step 2. Calculate the cumulative distances along the curve for each position along the curve 
            //we just calculated
            List<float> accumulatedDistances = new List<float>();

            float totalDistance = 0f;

            accumulatedDistances.Add(totalDistance);

            for (int i = 1; i < positionsOnCurve.Count; i++)
            {
                totalDistance += MyVector3.Distance(positionsOnCurve[i], positionsOnCurve[i - 1]);

                accumulatedDistances.Add(totalDistance);
            }


            return accumulatedDistances;
        }



        //
        // Estimate the derivative at point t
        //
        //https://www.youtube.com/watch?v=pHMzNW8Agq4
        public static float EstimateDerivative(_Curve curve, float t)
        {
            //We can estimate the derivative by taking a step in each direction of the point we are interested in
            //Should be around this number
            float derivativeStepSize = 0.0001f;

            MyVector3 valueMinus = curve.GetPosition(t - derivativeStepSize);
            MyVector3 valuePlus = curve.GetPosition(t + derivativeStepSize);

            //Have to multiply by two because we are taking a step in each direction
            MyVector3 derivativeVector = (valuePlus - valueMinus) * (1f / (derivativeStepSize * 2f));


            float derivative = MyVector3.Magnitude(derivativeVector);


            return derivative;
        }
    }
}
