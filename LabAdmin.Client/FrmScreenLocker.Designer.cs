namespace LabAdmin.Client
{
    partial class FrmScreenLocker
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblLockMsg = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblLockMsg
            // 
            this.lblLockMsg.AutoSize = true;
            this.lblLockMsg.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLockMsg.ForeColor = System.Drawing.Color.Red;
            this.lblLockMsg.Location = new System.Drawing.Point(209, 161);
            this.lblLockMsg.Name = "lblLockMsg";
            this.lblLockMsg.Size = new System.Drawing.Size(249, 25);
            this.lblLockMsg.TabIndex = 0;
            this.lblLockMsg.Text = "MÁY TÍNH ĐANG BỊ KHÓA";
            // 
            // FrmScreenLocker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(711, 360);
            this.Controls.Add(this.lblLockMsg);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "FrmScreenLocker";
            this.Text = "FrmScreenLocker";
            this.TopMost = true;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FrmScreenLocker_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblLockMsg;
    }
}