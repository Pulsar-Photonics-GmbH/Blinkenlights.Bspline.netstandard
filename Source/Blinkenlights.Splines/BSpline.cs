/*
The MIT License(MIT)

Copyright(c) 2015 Thibaut Séguy<thibaut.seguy@gmail.com>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

(Translated to C# by Paul Grebenc, 2016.)
(Converted to a netstandard library by Pulsar Photonics GmbH, 2023.)

*/

using System;

namespace Blinkenlights.Splines
{
    public class BSpline
    {
        public static double[] Interpolate(double t, int degree, double[][] points, double[] knots, double[] weights, double[] result)
        {
            var n = points.Length; // points count
            var d = points[0].Length; // point dimensionality

            if (degree < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(degree), "order must be at least 1 (linear)");
            }
            if (degree > n-1)
            {
                throw new ArgumentOutOfRangeException(nameof(degree), "order must be less than or equal to point count - 1");
            }

            if (weights == null)
            {
                // build weight vector
                weights = new double[n];
                for (var i = 0; i < n; i++)
                {
                    weights[i] = 1;
                }
            }

            if (knots == null)
            {
                // build knot vector of length [n + degree + 1]
                knots = new double[n + degree + 1];
                for (var i = 0; i < n + degree + 1; i++)
                {
                    knots[i] = i;
                }
            }
            else
            {
                if (knots.Length != n + degree + 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(knots), "bad knot vector length");
                }
            }

            var domain = new int[] { degree, knots.Length - 1 - degree };

            // remap t to the domain where the spline is defined
            var low = knots[domain[0]];
            var high = knots[domain[1]];
            t = t * (high - low) + low;

            if (t < low || t > high)
            {
                throw new InvalidOperationException("out of bounds");
            }

            int s;
            for (s = domain[0]; s < domain[1]; s++)
            {
                if (t >= knots[s] && t <= knots[s + 1])
                {
                    break;
                }
            }

            // convert points to homogeneous coordinates
            var v = new double[n, d + 1];
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < d; j++)
                {
                    v[i, j] = points[i][j] * weights[i];
                }
                v[i, d] = weights[i];
            }

            // l (level) goes from 1 to the curve order
            for (var l = 1; l <= degree+1; l++)
            {
                // build level l of the pyramid
                for (var i = s; i > s - degree - 1 + l; i--)
                {
                    var alpha = (t - knots[i]) / (knots[i + degree + 1 - l] - knots[i]);

                    // interpolate each component
                    for (var j = 0; j < d + 1; j++)
                    {
                        v[i, j] = (1 - alpha) * v[i - 1, j] + alpha * v[i, j];
                    }
                }
            }

            // convert back to cartesian and return
            if (result == null)
            {
                result = new double[d];
            }
            for (var i = 0; i < d; i++)
            {
                result[i] = v[s, i] / v[s, d];
            }

            return result;
        }
    }
}
