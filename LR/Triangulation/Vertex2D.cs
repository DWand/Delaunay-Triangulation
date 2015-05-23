using System;

namespace Triangulation {
    /// <summary>
    /// Represents a point in 2D environment
    /// </summary>
    [Serializable()]
    class Vertex2D {

        /// <summary>
        /// X coordinate of the vertex
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y coordinate of the vertex
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Constructs a 2D vertex with (x,y) coordinates
        /// </summary>
        /// <param name="x">X coordinate of the vertex</param>
        /// <param name="y">Y coordinate of the vertex</param>
        public Vertex2D(double x, double y) {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Constructs a 2D vertex with (0,0) coordinates
        /// </summary>
        public Vertex2D()
        : this(0d, 0d) {
        }

        /// <summary>
        /// Constructs a copy of the given vertex
        /// </summary>
        /// <param name="prototype"></param>
        public Vertex2D(Vertex2D prototype)
        : this (prototype.X, prototype.Y) {
        }

        /// <summary>
        /// Calculates distance between this vertex and the given one
        /// </summary>
        /// <param name="otherVertex">The vertex to calculate distance to</param>
        /// <returns>A distance between two vertices</returns>
        public double CalcDistanceTo(Vertex2D otherVertex) {
            return Vertex2D.CalcDistanceBetween(this, otherVertex);
        }

        /// <summary>
        /// Calculates distance between two given vertices
        /// </summary>
        /// <param name="fst">A first vertex</param>
        /// <param name="snd">A second vertex</param>
        /// <returns>A distance between two vertices</returns>
        public static double CalcDistanceBetween(Vertex2D fst, Vertex2D snd) {
            double x = fst.X - snd.X;
            double y = fst.Y - snd.Y;
            return Math.Sqrt(x * x + y * y);
        }

        /// <summary>
        /// Compares two points
        /// </summary>
        /// <param name="obj">Another point</param>
        /// <returns>Whether the given is equal for this</returns>
        public override bool Equals(object obj) {
            if (!(obj is Vertex2D)) {
                return false;
            }

            Vertex2D o = obj as Vertex2D;

            if (this.X != o.X || this.Y != o.Y) {
                double diff = Math.Abs(this.X - o.X) + Math.Abs(this.Y - o.Y);
                return diff < double.Epsilon;
            } else {
                return true;
            }
        }

    }
}
