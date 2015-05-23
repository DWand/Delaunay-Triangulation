namespace LR {
    partial class LR {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.pane = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // pane
            // 
            this.pane.BackColor = System.Drawing.Color.White;
            this.pane.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pane.Location = new System.Drawing.Point(0, 0);
            this.pane.Name = "pane";
            this.pane.Size = new System.Drawing.Size(604, 487);
            this.pane.TabIndex = 0;
            // 
            // LR
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 487);
            this.Controls.Add(this.pane);
            this.Name = "LR";
            this.Text = "Лабораторна робота";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pane;
    }
}

