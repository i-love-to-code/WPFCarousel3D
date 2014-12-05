namespace SamNoble.Wpf.Controls.Carousel3D.Helpers
{
    using System;
    using System.Windows;
    using System.Windows.Media.Media3D;

    public static class MathHelper
    {
        //Create these here so we don't have to create them each time.
        private static readonly Vector3D UnitXAxis3D = new Vector3D(1d, 0d, 0d);
        private static readonly Vector3D UnitYAxis3D = new Vector3D(0d, 1d, 0d);
        private static readonly Vector3D UnitZAxis3D = new Vector3D(0d, 0d, 1d);

        /// <summary>
        /// Rotates a <paramref name="point"/> by the given angles for each axis.
        /// </summary>
        /// <param name="point">The point to rotate.</param>
        /// <param name="xRotation">The angle by which to rotate <paramref name="point"/> around the x-axis.</param>
        /// <param name="yRotation">The angle by which to rotate <paramref name="point"/> around the y-axis.</param>
        /// <param name="zRotation">The angle by which to rotate <paramref name="point"/> around the z-axis.</param>
        /// <returns>The original point in its rotated form.</returns>
        public static Point3D RotatePoint3D(Point3D point, double xRotation, double yRotation, double zRotation)
        {
            //Refer to http://www.genesis3d.com/~kdtop/Quaternions-UsingToRepresentRotation.htm for more
            //information on quaternions
            
            //Use quaternions to avoid Gimbal lock.
            var xQ = new Quaternion(UnitXAxis3D, xRotation);
            var yQ = new Quaternion(UnitYAxis3D, yRotation);
            var zQ = new Quaternion(UnitZAxis3D, zRotation);

            var pQ = new Quaternion(point.X, point.Y, point.Z, 0d);

            var xyzQ = xQ * yQ * zQ;
            var xyzQc = new Quaternion(xyzQ.X, xyzQ.Y, xyzQ.Z, xyzQ.W);
            xyzQc.Conjugate();

            //Now multiply everything together
            var q = xyzQ * pQ * xyzQc;

            //And extract our point.
            var rotatedPoint = new Point3D(q.X, q.Y, q.Z);

            return rotatedPoint;
        }

        /// <summary>
        /// Calculates the point on an ellipse at the given angle.
        /// </summary>        
        /// <param name="width">The width of the ellipse (length of major axis).</param>
        /// <param name="height">The height of the ellipse (length of minor axis).</param>
        /// <param name="origin">The location of the ellipse's centre.</param>
        /// <param name="thea">The angle (in radians) at which the point of interest lies.</param>
        /// <returns>A <see cref="Point"/> object detailing the coordinates of the point on the ellipse.</returns>
        public static Point GetPointOnEllipse(Rect ellipse, double theta)
        {
            // This code uses the rather expensive Math.Cos() and Math.Sin() methods.
            // If performance is a big issue you may want to consider using a look
            // up table of precomputed angles.
            var x = ellipse.X + (ellipse.Width * Math.Cos(theta));
            var y = ellipse.Y + (ellipse.Height * Math.Sin(theta));

            return new Point(x, y);
        }

        /// <summary>
        /// Uses the second Ramanujan approximation to calculate the circumference of an ellipse.
        /// </summary>
        /// <returns>The circumference of an ellipse with a width <param name="a"/> and height <param name="b" />.</returns>
        public static double CalculateEllipseCircumferenceRamanujan2(double a, double b)
        {
            var h = Math.Pow(a - b, 2) / Math.Pow(a + b, 2);

            // pi (a + b) [ 1 + 3 h / (10 + (4 - 3 h)^1/2 ) ]            
            var result = Math.PI * (a + b) * (1 + 3 * h / (10 + Math.Pow((4 - 3 * h), 0.5)));

            return result;
        }
    }
}
