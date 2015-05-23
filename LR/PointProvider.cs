using System;
using System.Collections.Generic;
using Triangulation;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace LR {
    /// <summary>
    /// Provides a list of points for triangulation
    /// </summary>
    class PointProvider {

        #region Fields

        /// <summary>
        /// Minimal possible X coordinate value
        /// </summary>
        public double MinX { get; protected set; }

        /// <summary>
        /// Maximal possible X coordinate value
        /// </summary>
        public double MaxX { get; protected set; }

        /// <summary>
        /// Minimal possible Y coordinate value
        /// </summary>
        public double MinY { get; protected set; }

        /// <summary>
        /// Maximal possible Y coordinate value
        /// </summary>
        public double MaxY { get; protected set; }

        /// <summary>
        /// A name of the file to store a current list of points
        /// </summary>
        protected String filename;

        /// <summary>
        /// A randomizer object to generate random points
        /// </summary>
        protected Random randomizer;

        /// <summary>
        /// An index of the point which will be returned next
        /// </summary>
        protected int nextPointIndex;

        /// <summary>
        /// A list of generated points
        /// </summary>
        protected List<Vertex2D> points;

        #endregion




        /// <summary>
        /// Constructs a point provider
        /// </summary>
        /// <param name="filename">A name of the file to store a list of points</param>
        /// <param name="minX">A minimal possible X coordinate value</param>
        /// <param name="maxX">A maximal possible X coordinate value</param>
        /// <param name="minY">A minimal possible Y coordinate value</param>
        /// <param name="maxY">A maximal possible Y coordinate value</param>
        public PointProvider(String filename, double minX, double maxX, double minY, double maxY) {
            this.filename = filename;
            nextPointIndex = 0;
            points = new List<Vertex2D>();

            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;

            randomizer = new Random();
            ReadFile();
        }




        /// <summary>
        /// Returns next point. This may be either a point read from the file, or a newly generated random point
        /// </summary>
        /// <returns>A point</returns>
        public Vertex2D GetNextPoint() {
            Vertex2D point;
            if (nextPointIndex >= points.Count) {
                point = GenerateRandomPoint();
                points.Add(point);
            } else {
                point = points[nextPointIndex];
            }
            nextPointIndex++;
            return point;
        }

        /// <summary>
        /// Generates a new random point
        /// </summary>
        /// <returns>A random point</returns>
        protected Vertex2D GenerateRandomPoint() {
            double x = randomizer.NextDouble() * (MaxX - MinX) + MinX;
            double y = randomizer.NextDouble() * (MaxY - MinY) + MinY;
            return new Vertex2D(x, y);
        }

        /// <summary>
        /// Resets a history of generated points
        /// </summary>
        public void Reset() {
            points.Clear();
        }




        #region File operations

        /// <summary>
        /// Tries to read a previously saved list of points from a file
        /// </summary>
        protected void ReadFile() {
            try {
                using (Stream stream = File.Open(filename, FileMode.Open)) {
                    BinaryFormatter bin = new BinaryFormatter();
                    points = (List<Vertex2D>)bin.Deserialize(stream);
                }
            } catch (IOException) {}
        }

        /// <summary>
        /// Tries to save the current list of points into a file
        /// Throws IOException
        /// </summary>
        public void SavePoints() {
            using (Stream stream = File.Open(filename, FileMode.Create)) {
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, points);
            }
        }

        #endregion

    }
}
