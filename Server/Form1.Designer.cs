namespace Server
{
    partial class MainForm
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
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.logListBox = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // logListBox
            // 
            this.logListBox.FormattingEnabled = true;
            this.logListBox.Location = new System.Drawing.Point(12, 13);
            this.logListBox.Name = "logListBox";
            this.logListBox.Size = new System.Drawing.Size(1147, 771);
            this.logListBox.TabIndex = 3;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1171, 792);
            this.Controls.Add(this.logListBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Asteria Server";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox logListBox;
    }
}

