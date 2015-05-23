using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Triangulation;
using Ui;
using System.IO;

namespace LR {
    public partial class LR : Form {

        #region Visualization

        private Brush regionBrush = new SolidBrush(Color.FromArgb(0x88, 0x88, 0x88, 0x88));
        private Brush activeRegionBrush = new SolidBrush(Color.FromArgb(0x88, 0x88, 0xff, 0x88));
        private Pen regionPen = new Pen(Color.DimGray, 1);
        private Pen circlePen = new Pen(Color.Red, 1);

        private Int32 POINT_SIZE = 6;
        private Color POINT_BASE_COLOR = Color.DimGray;
        private Color POINT_HOVER_COLOR = Color.Black;
        private Color POINT_ACTIVE_COLOR = Color.Red;

        #endregion


        #region Point generation

        private Int32 POINTS_COUNT = 25;
        private Int32 PADDING = 10;
        private const String FILENAME = "points";
        private PointProvider pointProvider;

        #endregion

        /// <summary>
        /// Points to triangulate
        /// </summary>
        private List<Vertex2D> points;

        /// <summary>
        /// Triangulation generator
        /// </summary>
        private Triangulator triangulator;

        /// <summary>
        /// A generated triangulation on form of simple arrays
        /// </summary>
        private List<Point[]> triangulation;

        /// <summary>
        /// A generated triangulation on form of triangles
        /// </summary>
        private List<Triangle> triangles;




        private Dictionary<PointButton, Int32> ctrl2pointMap; // maps points to controls
        private List<int> activeTriangles; // current "active" triangles




        public LR() {
            InitializeComponent();
            
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            
            pane.Paint += pane_Paint;
            pane.DoubleClick += pane_DoubleClick;
            pane.MouseDown += pane_MouseDown;
            pane.MouseUp += pane_MouseUp;
            pane.CreateGraphics().CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.GammaCorrected;

            pointProvider = new PointProvider(FILENAME, PADDING, ClientSize.Width - PADDING, PADDING, ClientSize.Height - PADDING);
            InitializePoints();

            GetTriangulation();
        }

        


        #region Initialization

        private PointButton CreateButton(int x, int y) {
            PointButton btn = new PointButton(
                POINT_SIZE,
                POINT_BASE_COLOR, POINT_HOVER_COLOR, POINT_ACTIVE_COLOR
            );
            btn.Location = new Point(x, y);
            btn.OnPositionChanged += OnPointPositionChanged;
            return btn;
        }

        private void OnPointPositionChanged(PointButton sender, Point newPosition) {
            int pointIndex;
            if (!ctrl2pointMap.TryGetValue(sender, out pointIndex)) {
                return;
            }

            Vertex2D point = points[pointIndex];
            point.X = newPosition.X;
            point.Y = newPosition.Y;
        }

        /// <summary>
        /// Initializes a set of points to triangulate
        /// </summary>
        private void InitializePoints() {
            pane.Controls.Clear();

            points = new List<Vertex2D>(POINTS_COUNT);
            ctrl2pointMap = new Dictionary<PointButton, Int32>(POINTS_COUNT);

            Vertex2D newPoint;
            PointButton newPointCtrl;
            for (int i = 0; i < POINTS_COUNT; i++) {
                newPoint = pointProvider.GetNextPoint();
                newPointCtrl = CreateButton((int)newPoint.X, (int)newPoint.Y);

                points.Add(newPoint);
                ctrl2pointMap.Add(newPointCtrl, i);
                pane.Controls.Add(newPointCtrl);
            }
        }

        /// <summary>
        /// Gets a triangulation for a set of points
        /// </summary>
        private void GetTriangulation() {
            triangulator = new Triangulator(points);
            triangles = triangulator.Triangulate();
            triangulation = new List<Point[]>(triangles.Count);
            for (int i = 0; i < triangles.Count; i++) {
                triangulation.Add(new Point[] {
                    new Point((int)triangles[i].Vertices[0].X, (int)triangles[i].Vertices[0].Y),
                    new Point((int)triangles[i].Vertices[1].X, (int)triangles[i].Vertices[1].Y),
                    new Point((int)triangles[i].Vertices[2].X, (int)triangles[i].Vertices[2].Y)
                });
            }
        }

        #endregion

       


        #region Visualization

        protected void pane_Paint(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;
            g.Clear(pane.BackColor);

            // Draw regular triangles
            Point[] region;
            for (int i = 0; i < triangulation.Count; i++) {
                region = triangulation[i];

                g.FillPolygon(regionBrush, region);
                g.DrawPolygon(regionPen, region);
            }

            // Draw "active" triangles
            if (activeTriangles != null) {
                Triangle triangle;
                Vertex2D center;
                double radius;
                for (int i = 0; i < activeTriangles.Count; i++) {
                    triangle = triangles[activeTriangles[i]];
                    region = triangulation[activeTriangles[i]];

                    center = triangle.CircumcircleCenter;
                    radius = triangle.CircumcircleRadius;
                    
                    g.FillPolygon(activeRegionBrush, region);
                    g.DrawPolygon(regionPen, region);

                    g.DrawEllipse(circlePen,
                        (float)(center.X - radius), (float)(center.Y - radius),
                        (float)radius * 2, (float)radius * 2);
                }
            }
            


            g.Flush();
        }

        #endregion




        #region User interaction

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            pane.Invalidate();
        }


        void pane_DoubleClick(object sender, EventArgs e) {
            // Update triangulation
            GetTriangulation();
            pane.Invalidate();
        }


        void pane_MouseUp(object sender, MouseEventArgs e) {
            // Clear "active" trangle
            activeTriangles = null;
            pane.Invalidate();
        }


        void pane_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == System.Windows.Forms.MouseButtons.Left) {

                // Show "active" triangle
                if (triangulation != null) {
                    activeTriangles = new List<int>();

                    Vertex2D curPosition = new Vertex2D(e.X, e.Y);
                    int pointPosition;
                    for (int i = 0; i < triangles.Count; i++) {
                        pointPosition = triangles[i].DeterminePointPosition(curPosition);
                        if (pointPosition != Triangle.POSITION_OUTSIDE) {
                            activeTriangles.Add(i);
                        }
                    }

                    pane.Invalidate();
                }
            } else if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                pointProvider.Reset();
                InitializePoints();
                GetTriangulation();
                pane.Invalidate();
            }
        }


        protected override void OnKeyDown(KeyEventArgs e) {
            if (e.KeyCode == Keys.S) {
                // Save current points to a file
                try {
                    pointProvider.SavePoints();
                    MessageBox.Show("Points are saved", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } catch (IOException exc) {
                    MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            } else if (e.KeyCode == Keys.R) {
                // Delete current set of point from a file
                try {
                    File.Delete(FILENAME);
                    MessageBox.Show("Points are deleted", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } catch (IOException exc) {
                    MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        #endregion

    }
}
