namespace WorkoutCreator.Controls
{
    partial class TimeTextbox
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.timeDisplayTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // timeDisplayTextBox
            // 
            this.timeDisplayTextBox.Location = new System.Drawing.Point(0, 0);
            this.timeDisplayTextBox.Name = "timeDisplayTextBox";
            this.timeDisplayTextBox.Size = new System.Drawing.Size(41, 20);
            this.timeDisplayTextBox.TabIndex = 0;
            this.timeDisplayTextBox.Text = "00:00";
            this.timeDisplayTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.timeDisplayTextBox_KeyDown);
            this.timeDisplayTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.timeDisplayTextBox_KeyUp);
            // 
            // TimeTextbox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.timeDisplayTextBox);
            this.Name = "TimeTextbox";
            this.Size = new System.Drawing.Size(43, 22);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox timeDisplayTextBox;
    }
}
