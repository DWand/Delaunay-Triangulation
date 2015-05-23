using System;
using System.Collections.Generic;

namespace Triangulation {
    /// <summary>
    /// Compares two vertices by their polar angle
    /// </summary>
    class Vertex2DPolarAngleComparer : IComparer<Vertex2D> {

        /// <summary>
        /// Coordinates of a center of a coordinate system
        /// </summary>
        public Vertex2D CenterPoint { get; protected set; }

        /// <summary>
        /// Creates a vertex comparer for the specified coordinate system
        /// </summary>
        /// <param name="centerPoint">Coordinates of a center of the coordinate system</param>
        public Vertex2DPolarAngleComparer(Vertex2D centerPoint) {
            CenterPoint = centerPoint;
        }

        /// <summary>
        /// Compares two vertices by their polar angles
        /// </summary>
        /// <param name="fst">A first vertex to compare</param>
        /// <param name="snd">A second vertex to compare</param>
        /// <returns>Positive number: fst > snd; negative number: fst < snd; zero: fst == snd</returns>
        public int Compare(Vertex2D fst, Vertex2D snd) {
            double fstAngle = Math.Atan2(fst.Y - CenterPoint.Y, fst.X - CenterPoint.X);
            double sndAngle = Math.Atan2(snd.Y - CenterPoint.Y, snd.X - CenterPoint.X);
            return fstAngle.CompareTo(sndAngle);
        }

    }
}
