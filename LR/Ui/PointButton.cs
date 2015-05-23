using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ui {
    class PointButton : Label {

        public delegate void OnPositionChangedHandler(PointButton sender, Point newPosition);

        public event OnPositionChangedHandler OnPositionChanged;

        protected Color baseColor;

        protected Color hoverColor;

        protected Color activeColor;

        protected Boolean isHovered;

        protected Boolean isActive;

        protected Point mouseOffset;

        public Point Location {
            get {
                Point location = base.Location;
                Point newLocation = new Point(location.X + Size.Width / 2, location.Y + Size.Height / 2);
                return newLocation;
            }
            set {
                Point newLocation = new Point(value.X - Size.Width / 2, value.Y - Size.Height / 2);
                base.Location = newLocation;
            }
        }

        public PointButton(Int32 size, Color color, Color hoverColor, Color activeColor) {
            this.Size = new Size(size, size);
            this.BackColor = this.baseColor = color;
            this.hoverColor = hoverColor;
            this.activeColor = activeColor;
            isHovered = false;
            isActive = false;
        }

        protected void ApplyStateColor() {
            if (isActive) {
                BackColor = activeColor;
            } else if (isHovered) {
                BackColor = hoverColor;
            } else {
                BackColor = baseColor;
            }
        }

        protected override void OnMouseHover(EventArgs e) {
            isHovered = true;
            ApplyStateColor();
            base.OnMouseHover(e);
        }

        protected override void OnMouseLeave(EventArgs e) {
            isHovered = false;
            ApplyStateColor();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            isActive = true;
            ApplyStateColor();
            mouseOffset = e.Location;
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if (isActive) {
                Int32 x = this.Location.X + e.Location.X - mouseOffset.X;
                Int32 y = this.Location.Y + e.Location.Y - mouseOffset.Y;
                this.Location = new Point(x, y);
                FireOnPositionChanged();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            isActive = false;
            ApplyStateColor();
            base.OnMouseUp(e);
        }

        protected void FireOnPositionChanged() {
            if (OnPositionChanged != null) {
                OnPositionChanged.Invoke(this, this.Location);
            }
        }

    }
}
