using System;
using System.Collections.Generic;

namespace Triangulation {

    /// <summary>
    /// This class represents a triangle in the Delaunay triangulation.
    /// 
    /// It is a part of the vertices-triangles data structure.
    /// A triangle has:
    /// * 3 vertices which are referenced in a clockwise order
    /// * 3 adjacent triangles
    /// 
    /// Indexes of vertices and triangles depend on each other.
    /// Index of the vertex equals to the index of the oposite triangle, and vise versa.
    /// </summary>
    class Triangle {

        #region Constants 

        /// <summary>
        /// Indicates that a point is outside the triangle
        /// </summary>
        public const int POSITION_OUTSIDE = -3;

        /// <summary>
        /// Indicates that a point is one of the triangle's vertices
        /// </summary>
        public const int POSITION_VERTEX = -2;

        /// <summary>
        /// Indicates that a point as inside the triangle
        /// </summary>
        public const int POSITION_INSIDE = -1;

        /// <summary>
        /// Indicates that a needed entity is not found
        /// </summary>
        public const int INDEX_NOT_FOUND = -1;

        #endregion




        #region Fields

        /// <summary>
        /// Vertices of the triangle aranged in a clockwise order
        /// </summary>
        public Vertex2D[] Vertices { get; protected set; }

        /// <summary>
        /// Adjacent triangles aranged in a clockwise order
        /// </summary>
        public Triangle[] Triangles { get; protected set; }

        /// <summary>
        /// A center of the triangle's circumcircle
        /// </summary>
        public Vertex2D CircumcircleCenter { get; protected set; }

        /// <summary>
        /// A radius of the tricngle's circumcircle
        /// </summary>
        public double CircumcircleRadius { get; protected set; }

        #endregion




        #region Constructors

        /// <summary>
        /// Conscructs a triangle. All vertices has to be aranged in clockwise order.
        /// </summary>
        /// <param name="v1">First vertex of the triangle</param>
        /// <param name="v2">Second vertex of the triangle</param>
        /// <param name="v3">Third vertex of the triangle</param>
        /// <param name="t1">An adjacent triangle which is oposite to the first vertex</param>
        /// <param name="t2">An adjacent triangle which is oposite to the second vertex</param>
        /// <param name="t3">An adjacent triangle which is oposite to the third vertex</param>
        public Triangle(Vertex2D v1, Vertex2D v2, Vertex2D v3, Triangle t1, Triangle t2, Triangle t3) {
            Vertices = new Vertex2D[3] { v1, v2, v3 };
            Triangles = new Triangle[3] { t1, t2, t3 };
            CalcCircumcircle();
        }

        /// <summary>
        /// Conscructs a triangle. All vertices has to be aranged in clockwise order.
        /// </summary>
        /// <param name="v1">First vertex of the triangle</param>
        /// <param name="v2">Second vertex of the triangle</param>
        /// <param name="v3">Third vertex of the triangle</param>
        public Triangle(Vertex2D v1, Vertex2D v2, Vertex2D v3)
        :this(v1, v2, v3, null, null, null) {
        }

        #endregion




        #region Initialization

        /// <summary>
        /// Calculates center and redius of the triangle's circumcircle
        /// </summary>
        protected void CalcCircumcircle() {
            double Bx = Vertices[1].X - Vertices[0].X;
            double By = Vertices[1].Y - Vertices[0].Y;
            double Cx = Vertices[2].X - Vertices[0].X;
            double Cy = Vertices[2].Y - Vertices[0].Y;
            double D = 2 * (Bx * Cy - By * Cx);

            double B2Sum = Bx * Bx + By * By;
            double C2Sum = Cx * Cx + Cy * Cy;

            double Ox = (Cy * B2Sum - By * C2Sum) / D;
            double Oy = (Bx * C2Sum - Cx * B2Sum) / D;

            CircumcircleCenter = new Vertex2D(Ox + Vertices[0].X, Oy + Vertices[0].Y);
            CircumcircleRadius = CircumcircleCenter.CalcDistanceTo(Vertices[0]);
        }

        #endregion




        #region Accessors

        /// <summary>
        /// Returns a vertex which is oposite to the given adjacent triangle
        /// </summary>
        /// <param name="adjacentTriangle">An oposite adjacent triangle</param>
        /// <returns>An oposite vertex of null</returns>
        public Vertex2D GetOpositeVertex(Triangle adjacentTriangle) {
            int triangleIndex = FindTriangleIndex(adjacentTriangle);
            if (triangleIndex != INDEX_NOT_FOUND) {
                return Vertices[triangleIndex];
            } else {
                return null;
            }
        }

        /// <summary>
        /// Returns an adjacent triangle which is oposite to the given vertex
        /// </summary>
        /// <param name="vertex">An oposite vertex of the triangle</param>
        /// <returns>An oposite adjacent triangle or null</returns>
        public Triangle GetOpositeTriangle(Vertex2D vertex) {
            int vertexIndex = FindVertexIndex(vertex);
            if (vertexIndex != INDEX_NOT_FOUND) {
                return Triangles[vertexIndex];
            } else {
                return null;
            }
        }

        /// <summary>
        /// Returns an index of the given edge
        /// </summary>
        /// <param name="begin">A begin vertex of the edge</param>
        /// <param name="end">An end vertex of the edge</param>
        /// <returns>An index of the edge or -1 in case the edge is not found</returns>
        public int GetEdgeIndex(Vertex2D begin, Vertex2D end) {
            var beginIndex = FindVertexIndex(begin);
            if (beginIndex == INDEX_NOT_FOUND) {
                return INDEX_NOT_FOUND;
            }
            if (Vertices[(beginIndex + 1) % 3] != end) {
                return INDEX_NOT_FOUND;
            }
            return (beginIndex + 2) % 3;
        }

        #endregion




        #region Checkers

        /// <summary>
        /// Determines a position of the given point
        /// </summary>
        /// <param name="point">A point to check</param>
        /// <returns>
        /// A position of the point:
        ///     -3 outside,
        ///     -2 one of the vertices,
        ///     -1 inside,
        ///     0-3 an index of the point which is oposite to the crossed edge
        /// </returns>
        public int DeterminePointPosition(Vertex2D point) {
            VectorHelper.PointSide side;

            for (int i = 0; i < 3; i++) {
                if (Vertices[i] == point) {
                    return POSITION_VERTEX;
                }
            }

            side = VectorHelper.DetermineSide(Vertices[0], Vertices[1], point);
            if (side == VectorHelper.PointSide.LEFT) {
                return POSITION_OUTSIDE;
            } else if (side == VectorHelper.PointSide.CENTER) {
                return 2;
            }

            side = VectorHelper.DetermineSide(Vertices[1], Vertices[2], point);
            if (side == VectorHelper.PointSide.LEFT) {
                return POSITION_OUTSIDE;
            } else if (side == VectorHelper.PointSide.CENTER) {
                return 0;
            }

            side = VectorHelper.DetermineSide(Vertices[2], Vertices[0], point);
            if (side == VectorHelper.PointSide.LEFT) {
                return POSITION_OUTSIDE;
            } else if (side == VectorHelper.PointSide.CENTER) {
                return 1;
            }

            return POSITION_INSIDE;
        }

        /// <summary>
        /// Checks whether the given point is inside of the triangle's circumcircle
        /// </summary>
        /// <param name="point">A point to check</param>
        /// <returns>Whether the point is inside of the circumcircle</returns>
        public Boolean ContainsPointInCircumcircle(Vertex2D point) {
            double distance = CircumcircleCenter.CalcDistanceTo(point);
            return CircumcircleRadius >= distance;
        }

        /// <summary>
        /// Checks whether the triangle has a specified vertex
        /// </summary>
        /// <param name="vertex">A vertex to check</param>
        /// <returns>Whether the triangle has the vertex</returns>
        public Boolean HasVertex(Vertex2D vertex) {
            return FindVertexIndex(vertex) != INDEX_NOT_FOUND;
        }

        /// <summary>
        /// Checks whether the triangle has a specified edge
        /// </summary>
        /// <param name="begin">A begin point of the edge</param>
        /// <param name="end">An end point of the edge</param>
        /// <returns>Whether the triangle has the edge</returns>
        public Boolean HasEdge(Vertex2D begin, Vertex2D end) {
            int vertexIndex = FindVertexIndex(begin);
            if (vertexIndex == INDEX_NOT_FOUND) {
                return false;
            }
            return Vertices[(vertexIndex + 1) % 3] == end;
        }

        /// <summary>
        /// Checks whether the triangle has a specified adjacent triangle
        /// </summary>
        /// <param name="triangle">A triangle to check</param>
        /// <returns>Whether the triangle has the specified adjacent triangle</returns>
        public Boolean HasAdjacentTriangle(Triangle triangle) {
            int triangleIndex = FindTriangleIndex(triangle);
            return triangleIndex != INDEX_NOT_FOUND;
        }

        #endregion




        #region Transformations

        /// <summary>
        /// Performs a flip operation with the given adjacent triangle
        /// </summary>
        /// <param name="other">An adjacent triangle to flip with</param>
        public void FlipWith(Triangle other) {

            //   v2         tOldT3            v0
            //     @ * * * * * * * * * * > > @
            //     ^ *                     % *
            //     ^   *                 %   *
            //     *     *     this    %     *
            //   t *       *         %       * t
            //   O *         *     %         * O
            //   l *           * %           * l
            //   d *           % *           * d
            //   O *         %     *         * T
            //   3 *       %         *       * 2
            //     *     %     other   *     V
            //     *   %                 *   V
            //     * %                     * *
            //     @ * < < * * * * * * * * * @
            //   v1           tOldO2          v3
            //
            // * - current lines, % - new lines
            //
            // this:  from [v0, v3, v2] to [v0, v1, v2]
            // other: from [v1, v2, v3] to [v1, v0, v3]

            if (FindTriangleIndex(other) == INDEX_NOT_FOUND) {
                return;
            }

            Vertex2D v0 = this.GetOpositeVertex(other);
            Vertex2D v1 = other.GetOpositeVertex(this);
            
            int v0IndexThis = FindVertexIndex(v0);
            int v3IndexThis = (v0IndexThis + 1) % 3;
            int v2IndexThis = (v0IndexThis + 2) % 3;
            
            int v1IndexOther = other.FindVertexIndex(v1);
            int v2IndexOther = (v1IndexOther + 1) % 3;
            int v3IndexOther = (v1IndexOther + 2) % 3;

            Vertex2D v3 = Vertices[v3IndexThis];
            Vertex2D v2 = Vertices[v2IndexThis];

            Triangle tOldT3 = Triangles[v3IndexThis];
            Triangle tOldT2 = Triangles[v2IndexThis];
            Triangle tOldO3 = other.Triangles[v3IndexOther];
            Triangle tOldO2 = other.Triangles[v2IndexOther];

            this.Vertices = new Vertex2D[3] { v0, v1, v2 };
            this.Triangles = new Triangle[3] { tOldO3, tOldT3, other };
            this.CalcCircumcircle();

            other.Vertices = new Vertex2D[3] { v1, v0, v3 };
            other.Triangles = new Triangle[3] { tOldT2, tOldO2, this };
            other.CalcCircumcircle();

            if (tOldT2 != null) {
                tOldT2.ChangeAdjacentTriangle(this, other);
            }
            if (tOldO3 != null) {
                tOldO3.ChangeAdjacentTriangle(other, this);
            }
        }

        #endregion




        #region Change vertices and triangles

        /// <summary>
        /// Changes the specified vertex
        /// </summary>
        /// <param name="oldVertex">An old vertex which has to be replaced</param>
        /// <param name="newVertex">A new vertex which has to be stored</param>
        public void ChangeVertex(Vertex2D oldVertex, Vertex2D newVertex) {
            int vertexIndex = FindVertexIndex(oldVertex);
            if (vertexIndex != INDEX_NOT_FOUND) {
                Vertices[vertexIndex] = newVertex;
                CalcCircumcircle();
            }
        }

        /// <summary>
        /// Changes the specified adjacent triangle
        /// </summary>
        /// <param name="oldTriangle">An old triangle which has to be replaced</param>
        /// <param name="newTriangle">A new triangle which has to be stored</param>
        public void ChangeAdjacentTriangle(Triangle oldTriangle, Triangle newTriangle) {
            int triangleIndex = FindTriangleIndex(oldTriangle);
            if (triangleIndex != INDEX_NOT_FOUND) {
                Triangles[triangleIndex] = newTriangle;
            }
        }

        /// <summary>
        /// Changes the oposite triangle
        /// </summary>
        /// <param name="opositeVertex">An oposite vertex</param>
        /// <param name="newTriangle">A new triangle which has to be inserted</param>
        public void ChangeOpositeTriangle(Vertex2D opositeVertex, Triangle newTriangle) {
            int vertexIndex = FindVertexIndex(opositeVertex);
            if (vertexIndex != INDEX_NOT_FOUND) {
                Triangles[vertexIndex] = newTriangle;
            }
        }

        #endregion




        #region Find internal index

        /// <summary>
        /// Finds internal index of the given vertex
        /// </summary>
        /// <param name="value">Vertex to find</param>
        /// <returns>Index of the vertex or -1 if it is not found</returns>
        public int FindVertexIndex(Vertex2D value) {
            for (int i = 0; i < 3; i++) {
                if (Vertices[i] == value) {
                    return i;
                }
            }
            return INDEX_NOT_FOUND;
        }

        /// <summary>
        /// Finds internal index of the given triangle
        /// </summary>
        /// <param name="value">Triangle to find</param>
        /// <returns>Index of the triangle or -1 if it is not found</returns>
        public int FindTriangleIndex(Triangle value) {
            for (int i = 0; i < 3; i++) {
                if (Triangles[i] == value) {
                    return i;
                }
            }
            return INDEX_NOT_FOUND;
        }

        #endregion

    }
}
