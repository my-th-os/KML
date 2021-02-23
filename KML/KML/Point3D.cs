using System;

namespace KML
{
    /// <summary>
    /// Point3D as local replacement for Point3D from System.Windows.Media.Media3D
    /// </summary>
    public class Point3D : IEquatable<Point3D>
    {
        /// <summary>
        /// X coordinate
        /// </summary>
        public double X;

        /// <summary>
        /// Y ccordinate
        /// </summary>
        public double Y;

        /// <summary>
        /// Z coordinate
        /// </summary>
        public double Z;

        /// <summary>
        /// Creates a Point3D instance
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="z">Z coordinate</param>
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Check Point3D equality
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True if equal</returns>
        public override bool Equals(object obj)
        {
            return obj is Point3D && this.Equals(obj as Point3D);
        }

        /// <summary>
        /// Check Point3D equality
        /// </summary>
        /// <param name="p">Another Point3D</param>
        /// <returns>True if equal</returns>
        public bool Equals(Point3D p)
        {
            return p.X == X && p.Y == Y && p.Z == Z;
        }

        /// <summary>
        /// Hash code according to equality
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode();
        }

        /// <summary>
        /// String representation
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return "(" + X + ", " + Y + ", " + Z + ")";
        }
    }
}
