using System;
using System.Collections.Generic;
using System.Linq;

namespace Triangulation {

    /// <summary>
    /// Builds a Delaunay triangulation for a set of points in 2D environment
    /// </summary>
    class Triangulator {

        /// <summary>
        /// A structure of a search result
        /// </summary>
        protected class SearchResult {
            public enum Type {
                TRIANGLE, EDGE, VERTEX
            }

            /// <summary>
            /// Type of the found point
            /// </summary>
            public Type type;

            /// <summary>
            /// An index of the edge which is affected by the point
            /// </summary>
            public int edgeIndex;

            /// <summary>
            /// A triangle which is affected by the point
            /// </summary>
            public Triangle triangle;

            public SearchResult(Type type, Triangle triangle, int edgeIndex) {
                this.type = type;
                this.triangle = triangle;
                this.edgeIndex = edgeIndex;
            }

            public SearchResult(Type type, Triangle triangle)
            : this(type, triangle, 0) {
            }
        }




        /// <summary>
        /// A placeholder triangle, which is not a pert of the triangulation.
        /// It is used to keep a place of the deleted adjacent triangle in other triangles.
        /// </summary>
        protected static Triangle PLACEHOLDER_TRIANGLE = new Triangle(new Vertex2D(0, 0), new Vertex2D(0, 1), new Vertex2D(0, 1));




        #region Fields

        /// <summary>
        /// A list of triangles in the triangulation
        /// </summary>
        protected List<Triangle> triangles;

        /// <summary>
        /// A list of vertices which are related to the super structure.
        /// These vertices have to be deleted from the resulting triangulation.
        /// </summary>
        protected List<Vertex2D> superStructureVertices;

        /// <summary>
        /// A set of points to triangulate
        /// </summary>
        protected List<Vertex2D> points;

        /// <summary>
        /// A list of triangles which have to be checked with Delauney criteria
        /// </summary>
        protected Queue<Triangle> suspiciousTriangles;

        #endregion




        /// <summary>
        /// Constructs a triangular for a given set of points
        /// </summary>
        /// <param name="points">A set of points to triangulate</param>
        public Triangulator(List<Vertex2D> points) {
            this.points = points;
            this.suspiciousTriangles = new Queue<Triangle>();
        }




        /// <summary>
        /// Performs triangulation of the set of points
        /// </summary>
        /// <returns>Triangulation</returns>
        public List<Triangle> Triangulate() {
            triangles = new List<Triangle>();
            
            // A super triangle is the first triangle in the triangulation
            AddSuperStructure();

            // Here we can re-order points to make it easier to find
            // corect triangles during insertion of new points into
            // the triangulation

            // Insert points one by one
            for (int i = 0; i < points.Count; i++) {
                InsertPoint(points[i]);
            }

            // Remove super structure from the triagulation
            RemoveSuperStructure();

            triangles.TrimExcess();

            return triangles;
        }




        #region Super structure

        /// <summary>
        /// Adds a super structure to the triangulation 
        /// </summary>
        protected void AddSuperStructure() {
            double minX = points[0].X;
            double minY = points[0].Y;
            double maxX = minX;
            double maxY = minY;

            for (int i = 1; i < points.Count; i++) {
                if (points[i].X < minX) {
                    minX = points[i].X;
                } else if (points[i].X > maxX) {
                    maxX = points[i].X;
                }

                if (points[i].Y < minY) {
                    minY = points[i].Y;
                } else if (points[i].Y > maxY) {
                    maxY = points[i].Y;
                }
            }

            double dx = maxX - minX;
            double dy = maxY - minY;

            double centerX = minX + dx * 0.5;
            double centerY = minY + dy * 0.5;

            // Edge of the super cube
            double edge = Math.Max(dx, dy);
            edge += 0.5d * edge;

            // Super triangle around super cube
            Vertex2D left = new Vertex2D(centerX - 1.5d * edge, centerY - 0.5d * edge);
            Vertex2D right = new Vertex2D(centerX + 1.5d * edge, centerY - 0.5d * edge);
            Vertex2D top = new Vertex2D(centerX, centerY + edge);

            superStructureVertices = new List<Vertex2D>(3);
            superStructureVertices.Add(left);
            superStructureVertices.Add(top);
            superStructureVertices.Add(right);

            triangles.Add(new Triangle(left, top, right, null, null, null));
        }

        /// <summary>
        /// Removes the super structure from the triangulation
        /// </summary>
        protected void RemoveSuperStructure() {
            for (int i = 0; i < superStructureVertices.Count; i++) {
                RemoveSuperVertex(superStructureVertices[i]);
            }
            CheckSuspiciousTriangles();
        }

        /// <summary>
        /// Removes the specified vertex of the super structure from the triangulation
        /// </summary>
        /// <param name="superVertex">A super structure's vertex to remove</param>
        protected void RemoveSuperVertex(Vertex2D superVertex) {
            if (superStructureVertices.Contains(superVertex) == false) {
                return;
            }

            // Hurt triangles are triangles which are affected by deletion of their adjacent triangles
            HashSet<Triangle> hurtTriangles = new HashSet<Triangle>();

            // Free points are point which are no more a part of the triangulation
            // They form a hull of the triangulation in place of deleted triangles
            List<Vertex2D> freePoints = new List<Vertex2D>();

            // Firstly we have to delete all triangles which are adjacent with the given
            // super structure's vertex

            Triangle badTriangle;
            Triangle hurtTriangle;
            for (int i = 0; i < triangles.Count; i++) {
                if (triangles[i].HasVertex(superVertex)) {
                    badTriangle = triangles[i];

                    // Remove bad triangle from triangulation
                    triangles.RemoveAt(i);
                    i--;

                    // Bad triangle is no more hurt (if was) - it is deleted now
                    hurtTriangles.Remove(badTriangle);

                    // Mark adjacent triangles as hurt to cure them later
                    for (int ti = 0; ti < 3; ti++) {
                        if (badTriangle.Triangles[ti] != null) {
                            hurtTriangle = badTriangle.Triangles[ti];
                            hurtTriangles.Add(hurtTriangle);
                            hurtTriangle.ChangeAdjacentTriangle(badTriangle, PLACEHOLDER_TRIANGLE);
                        }
                    }

                    // All points of this triangle are now free to build convex hull of them
                    freePoints.AddRange(badTriangle.Vertices);
                }
            }

            // All adjacent triangles are removed.
            // Now we have to restore a convex hull in this part of the triangulation

            if (hurtTriangles.Count > 1) {

                freePoints = freePoints.Distinct().ToList();
                freePoints.Remove(superVertex); // this vertex is not a part of the triangulation any more

                // Turn points set into counterclockwise ordered sequence (relative to the superVertex).
                // Each 2 adjacent points in this sequence will form an edge of some of the hurt triangles.
                // The superVertex is outside of all hurt triangles, so these new edges match a direction
                // of edges inside hurt triangles.

                freePoints.Sort(new Vertex2DPolarAngleComparer(superVertex));

                // freePoints:
                // 
                //                                            @ [0] - begin
                //             [2] - next                 *.
                //              @                    * .
                //            * . *             *  .
                //  *      *    .   *      *    .
                //    * @      .      @      .
                //     [3]     .    .[1] - end
                //        .   .    .  .
                //         .  .  . .
                //          .. ..
                //           @ superVertex
                //
                // It is good when the "next" vertex is right relative to a ("begin", "end") vector
                // If the "next" vertex is left, we have to build a new triangle ("begin", "next", "end")

                // There are at least 3 points free in the list
                int beginIndex = 0;
                int endIndex = 1;
                int nextIndex = 2;
                Vertex2D begin, end, next;
                VectorHelper.PointSide nextPointSide;

                do {
                    begin = freePoints[beginIndex];
                    end = freePoints[endIndex];
                    next = freePoints[nextIndex];

                    nextPointSide = VectorHelper.DetermineSide(begin, end, next);
                    if (nextPointSide == VectorHelper.PointSide.LEFT) {
                        // We have to build a part of a convex hull on top of these vertices.
                        FillConvexHullGap(begin, end, next, hurtTriangles);

                        // Check next point
                        // Don't move the "begin" point
                        endIndex = nextIndex;
                        nextIndex++;
                    } else {
                        // These 3 points are a part of a convex hull
                        // Just move further
                        beginIndex = endIndex;
                        endIndex = nextIndex;
                        nextIndex++;
                    }

                } while (nextIndex < freePoints.Count);
            }
            
            // Fix all remaining hurt triangles
            foreach (Triangle triangle in hurtTriangles) {
                for (int i = 0; i < 3; i++) {
                    if (triangle.Triangles[i] == PLACEHOLDER_TRIANGLE) {
                        triangle.Triangles[i] = null;
                    }
                }
            }
        }

        /// <summary>
        /// Builds a new triangle which is a part of the convex hull
        /// </summary>
        /// <param name="begin">First vertex in a counterclockwise order relative to the superVertex</param>
        /// <param name="end">Second vertex in a counterclockwise order relative to the superVertex</param>
        /// <param name="next">Third vertex in a counterclockwise order relative to the superVertex. It has to be on the left side of a (begin, end) vector</param>
        /// <param name="hurtTriangles">A set of hurt triangles</param>
        protected void FillConvexHullGap(Vertex2D begin, Vertex2D end, Vertex2D next, HashSet<Triangle> hurtTriangles) {

            //          [1] next
            //           @
            // super     * *   hurt triangle is here
            // triangle  *   *
            // was       *     @ [2] end
            // here      *   *
            //           * *   hurt triangle is here
            //           @
            //          [0] begin

            Triangle newTriangle = new Triangle(begin, next, end, PLACEHOLDER_TRIANGLE, PLACEHOLDER_TRIANGLE, PLACEHOLDER_TRIANGLE);

            int edgeIndex;
            foreach (Triangle hurtTriangle in hurtTriangles) {

                // Check [0] edge
                edgeIndex = hurtTriangle.GetEdgeIndex(next, end);
                if (edgeIndex != Triangle.INDEX_NOT_FOUND) {
                    hurtTriangle.Triangles[edgeIndex] = newTriangle;
                    newTriangle.Triangles[0] = hurtTriangle;
                }

                // Check [1] edge
                edgeIndex = hurtTriangle.GetEdgeIndex(end, begin);
                if (edgeIndex != Triangle.INDEX_NOT_FOUND) {
                    hurtTriangle.Triangles[edgeIndex] = newTriangle;
                    newTriangle.Triangles[1] = hurtTriangle;
                }
                
            }

            // Remove totally cured triangles
            hurtTriangles.RemoveWhere(triangle => !triangle.HasAdjacentTriangle(PLACEHOLDER_TRIANGLE));

            // A new triangle is hurt because it has one PLACEHOLDER_TRIANGLE
            // which may be resolved either as a new triangle or as null
            hurtTriangles.Add(newTriangle);

            triangles.Add(newTriangle);

            RegisterSuspiciousTriangle(newTriangle);
        }

        #endregion




        #region Insert point

        /// <summary>
        /// Inserts a point into the triangulation
        /// </summary>
        /// <param name="point">A point to insert</param>
        protected void InsertPoint(Vertex2D point) {
            SearchResult search = FindEnclosingTriangle(point);

            if (search == null) {
                return;
            }

            if (search.type == SearchResult.Type.TRIANGLE) {
                InsertTrianglePoint(point, search.triangle);
            } else if (search.type == SearchResult.Type.EDGE) {
                InsertEdgePoint(point, search.triangle, search.edgeIndex);
            }

            CheckSuspiciousTriangles();
        }

        /// <summary>
        /// Inserts a point inside the specified triangle
        /// </summary>
        /// <param name="point">A point to insert</param>
        /// <param name="triangle">A triangle to insert the point into</param>
        protected void InsertTrianglePoint(Vertex2D point, Triangle triangle) {

            //        @ v1
            //        * *              triangle  
            //  t    ^ *  V             becomes
            //  O    ^  *   V                t2
            //  l   *    *    *
            //  d   *    *      *
            //  2  *  t2  *   t0  *       tOld0
            //     *       *        *
            //    ^        @ point    V
            //    ^     *       *       V
            //   *   *     t1        *    *
            //   @ * < < * * * * * * < < *  @
            //  v0          tOld1             v2
            
            Vertex2D v0 = triangle.Vertices[0];
            Vertex2D v1 = triangle.Vertices[1];
            Vertex2D v2 = triangle.Vertices[2];
            
            Triangle tOld0 = triangle.Triangles[0];
            Triangle tOld1 = triangle.Triangles[1];
            Triangle tOld2 = triangle.Triangles[2];

            Triangle t0 = new Triangle(v1, v2, point, null/*t1*/, triangle, tOld0);
            Triangle t1 = new Triangle(v2, v0, point, triangle,   t0,       tOld1);
            // t2 is triangle

            triangle.ChangeVertex(v2, point);

            triangle.Triangles[0] = t0;
            triangle.Triangles[1] = t1;

            t0.Triangles[0] = t1;

            if (tOld0 != null) {
                tOld0.ChangeAdjacentTriangle(triangle, t0);
            }
            if (tOld1 != null) {
                tOld1.ChangeAdjacentTriangle(triangle, t1);
            }

            triangles.Add(t0);
            triangles.Add(t1);

            RegisterSuspiciousTriangle(triangle);
            RegisterSuspiciousTriangle(t0);
            RegisterSuspiciousTriangle(t1);

            RegisterSuspiciousTriangle(tOld0);
            RegisterSuspiciousTriangle(tOld1);
            RegisterSuspiciousTriangle(tOld2);
        }

        /// <summary>
        /// Inserts a point in the middle of the edge
        /// </summary>
        /// <param name="point">A point to insert</param>
        /// <param name="triangle">A triangle which edge is affected by the point</param>
        /// <param name="edgeIndex">An index of the affected edge in the triangle</param>
        protected void InsertEdgePoint(Vertex2D point, Triangle triangle, int edgeIndex) {

            //              @ v0
            // tOld20     * % *     tOld30
            //          *   %   *
            //        * t0  %  t2 *
            //      *       % point *
            // v3 @ * * * * @ * * * * @ v2
            //      *       %       *
            //        * t1  %  t3 *
            //          *   %   *
            // tOld21     * % *     tOld31
            //              @ v1
            //
            // * - curent lines, % - new lines
            //
            // triangle is t0, t1 is adjacent to t0
            // t0 splits into t0 and t2
            // t1 splits into t1 and t3

            Triangle t0 = triangle;
            Triangle t1 = triangle.Triangles[edgeIndex];

            Vertex2D v0 = t0.Vertices[edgeIndex];
            Vertex2D v1 = t1.GetOpositeVertex(t0);

            int v0Index0 = edgeIndex;
            int v2Index0 = (edgeIndex + 1) % 3;
            int v3Index0 = (edgeIndex + 2) % 3;

            Vertex2D v2 = t0.Vertices[v2Index0];
            Vertex2D v3 = t0.Vertices[v3Index0];

            Triangle tOld20 = t0.GetOpositeTriangle(v2);
            Triangle tOld30 = t0.GetOpositeTriangle(v3);
            Triangle tOld21 = t1.GetOpositeTriangle(v2);
            Triangle tOld31 = t1.GetOpositeTriangle(v3);

            Triangle t2 = new Triangle(point, v0, v2, tOld30, null/*t3*/, t0);
            Triangle t3 = new Triangle(point, v2, v1, tOld31, t1, t2);
            t2.Triangles[1] = t3;

            t0.ChangeVertex(v2, point);
            t0.ChangeOpositeTriangle(v3, t2);

            t1.ChangeVertex(v2, point);
            t1.ChangeOpositeTriangle(v3, t3);

            if (tOld30 != null) {
                tOld30.ChangeAdjacentTriangle(t0, t2);
            }
            if (tOld31 != null) {
                tOld31.ChangeAdjacentTriangle(t1, t3);
            }

            triangles.Add(t2);
            triangles.Add(t3);

            RegisterSuspiciousTriangle(t0);
            RegisterSuspiciousTriangle(t1);
            RegisterSuspiciousTriangle(t2);
            RegisterSuspiciousTriangle(t3);

            RegisterSuspiciousTriangle(tOld20);
            RegisterSuspiciousTriangle(tOld30);
            RegisterSuspiciousTriangle(tOld21);
            RegisterSuspiciousTriangle(tOld31);
        }

        /// <summary>
        /// Finds a triangle to insert a point into
        /// </summary>
        /// <param name="point">A point which has to be inserted</param>
        /// <returns>A result of the search or null in nothing is found</returns>
        protected SearchResult FindEnclosingTriangle(Vertex2D point) {
            
            // Just a simple linear search
            // Here we can implement another searching algorithm to increase speed
            
            int pos;
            for (int i = 0; i < triangles.Count; i++) {
                pos = triangles[i].DeterminePointPosition(point);
                if (pos == Triangle.POSITION_VERTEX) {
                    // The point is one the triangle's vertices
                    return new SearchResult(SearchResult.Type.VERTEX, triangles[i]);
                } else if (pos == Triangle.POSITION_INSIDE) {
                    // The point is inside of the triangle
                    return new SearchResult(SearchResult.Type.TRIANGLE, triangles[i]);
                } else if (pos != Triangle.POSITION_OUTSIDE) {
                    // The point is in the middle of the triangle's edge
                    return new SearchResult(SearchResult.Type.EDGE, triangles[i], pos);
                }
            }

            return null;
        }

        #endregion




        #region Delaunay criteria

        /// <summary>
        /// Registers a triangle which may break Delauney criteria and have to be checked
        /// </summary>
        /// <param name="triangle">A triangle to check over Delauney criteria</param>
        protected void RegisterSuspiciousTriangle(Triangle triangle) {
            if (triangle != null && !suspiciousTriangles.Contains(triangle)) {
                suspiciousTriangles.Enqueue(triangle);
            }
        }
        
        /// <summary>
        /// Checks registered triangles over Delauney criteria. If some triangles don't meet
        /// the criteria, this method performs flips to fix them.
        /// </summary>
        protected void CheckSuspiciousTriangles() {
            Triangle triangle;
            while (suspiciousTriangles.Count > 0) {
                triangle = suspiciousTriangles.Dequeue();
                ApplyDelauneyCriteria(triangle);
            }
        }

        /// <summary>
        /// Applies Delauney criteria to the given triangle. If the triangle doesn't meet
        /// the criteria, this method performs flip to fit it.
        /// </summary>
        /// <param name="triangle">A triangle to check</param>
        protected void ApplyDelauneyCriteria(Triangle triangle) {
            Vertex2D point;
            for (int i = 0; i < 3; i++) {
                if (triangle.Triangles[i] != null) {
                    point = triangle.Triangles[i].GetOpositeVertex(triangle);
                    if (triangle.ContainsPointInCircumcircle(point)) {
                        FlipTriangles(triangle, triangle.Triangles[i]);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Flips two triangles
        /// </summary>
        /// <param name="t1">A first triangle to flips</param>
        /// <param name="t2">A second triangle to flip</param>
        protected void FlipTriangles(Triangle t1, Triangle t2) {
            t1.FlipWith(t2);

            for (int i = 0; i < 3; i++) {
                // this includes t2 triangle
                RegisterSuspiciousTriangle(t1.Triangles[i]); 

                // this includes t1 triangle
                RegisterSuspiciousTriangle(t2.Triangles[i]);
            }
        }

        #endregion

    }
}
