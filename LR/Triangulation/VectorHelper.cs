using System;

namespace Triangulation {
    /// <summary>
    /// Performs some operations with vectors
    /// </summary>
    class VectorHelper {

        /// <summary>
        /// A side of a point relative to a line
        /// </summary>
        public enum PointSide {
            LEFT, CENTER, RIGHT
        };

        /// <summary>
        /// Determines to which side of vector lies the point
        /// </summary>
        /// <param name="begin">Begin point of the vector</param>
        /// <param name="end">End point of the vector</param>
        /// <param name="point">A target point</param>
        /// <returns>Location of the point</returns>
        public static PointSide DetermineSide(Vertex2D begin, Vertex2D end, Vertex2D point) {
            double D = CalcCrossProduct(begin, point, begin, end);

            if (Math.Abs(D) < double.Epsilon) {
                return PointSide.CENTER;
            } else if (D < 0) {
                return PointSide.LEFT;
            } else {
                return PointSide.RIGHT;
            }
        }

        /// <summary>
        /// Calculates a exterior product of given vectors (cross product?)
        /// </summary>
        /// <param name="beg1">Begin point of the first vector</param>
        /// <param name="end1">End point of the first vector</param>
        /// <param name="beg2">Begin point of the second vector</param>
        /// <param name="end2">End point of the second vector</param>
        /// <returns>Exterior product of the given vectors (cross product?)</returns>
        public static double CalcCrossProduct(Vertex2D beg1, Vertex2D end1, Vertex2D beg2, Vertex2D end2) {
            double x1 = end1.X - beg1.X;
            double y1 = end1.Y - beg1.Y;
            double x2 = end2.X - beg2.X;
            double y2 = end2.Y - beg2.Y;
            return x1 * y2 - x2 * y1;
        }

    }
}
