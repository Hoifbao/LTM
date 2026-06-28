using System;
using System.Drawing;
using System.Windows.Forms;

namespace LabAdmin.Client
{
    partial class frmClientMain
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
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;

            // Các hằng số đại diện cho các vị trí viền và góc của Form
            const int HTLEFT = 10;
            const int HTRIGHT = 11;
            const int HTTOP = 12;
            const int HTTOPLEFT = 13;
            const int HTTOPRIGHT = 14;
            const int HTBOTTOM = 15;
            const int HTBOTTOMLEFT = 16;
            const int HTBOTTOMRIGHT = 17;

            base.WndProc(ref m);

            // Bắt sự kiện khi chuột di chuyển trên Form
            if (m.Msg == WM_NCHITTEST)
            {
                int resizeAreaSize = 10; // Độ rộng của vùng có thể kéo (tính bằng pixel)
                Point cursor = this.PointToClient(Cursor.Position);

                // Kiểm tra xem chuột đang ở góc hay cạnh nào để đổi con trỏ chuột tương ứng
                if (cursor.X <= resizeAreaSize && cursor.Y <= resizeAreaSize)
                    m.Result = (IntPtr)HTTOPLEFT;
                else if (cursor.X >= this.ClientSize.Width - resizeAreaSize && cursor.Y <= resizeAreaSize)
                    m.Result = (IntPtr)HTTOPRIGHT;
                else if (cursor.X <= resizeAreaSize && cursor.Y >= this.ClientSize.Height - resizeAreaSize)
                    m.Result = (IntPtr)HTBOTTOMLEFT;
                else if (cursor.X >= this.ClientSize.Width - resizeAreaSize && cursor.Y >= this.ClientSize.Height - resizeAreaSize)
                    m.Result = (IntPtr)HTBOTTOMRIGHT;
                else if (cursor.X <= resizeAreaSize)
                    m.Result = (IntPtr)HTLEFT;
                else if (cursor.X >= this.ClientSize.Width - resizeAreaSize)
                    m.Result = (IntPtr)HTRIGHT;
                else if (cursor.Y <= resizeAreaSize)
                    m.Result = (IntPtr)HTTOP;
                else if (cursor.Y >= this.ClientSize.Height - resizeAreaSize)
                    m.Result = (IntPtr)HTBOTTOM;
            }
        }
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmClientMain));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlCard1 = new System.Windows.Forms.Panel();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.label5 = new System.Windows.Forms.Label();
            this.lblClientIPAddress = new System.Windows.Forms.Label();
            this.lblClientName = new System.Windows.Forms.Label();
            this.pnlCard2 = new System.Windows.Forms.Panel();
            this.rtbLogsClient = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.panel1.SuspendLayout();
            this.pnlCard1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.pnlCard2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(40)))));
            this.panel1.Controls.Add(this.btnRefresh);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Name = "panel1";
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // btnRefresh
            // 
            resources.ApplyResources(this.btnRefresh, "btnRefresh");
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnRefresh.FlatAppearance.BorderSize = 0;
            this.btnRefresh.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
            this.btnRefresh.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label1.Name = "label1";
            // 
            // pnlCard1
            // 
            resources.ApplyResources(this.pnlCard1, "pnlCard1");
            this.pnlCard1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(40)))));
            this.pnlCard1.Controls.Add(this.pictureBox3);
            this.pnlCard1.Controls.Add(this.label5);
            this.pnlCard1.Controls.Add(this.lblClientIPAddress);
            this.pnlCard1.Controls.Add(this.lblClientName);
            this.pnlCard1.Name = "pnlCard1";
            // 
            // pictureBox3
            // 
            resources.ApplyResources(this.pictureBox3, "pictureBox3");
            this.pictureBox3.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.TabStop = false;
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.label5.Name = "label5";
            // 
            // lblClientIPAddress
            // 
            resources.ApplyResources(this.lblClientIPAddress, "lblClientIPAddress");
            this.lblClientIPAddress.Name = "lblClientIPAddress";
            // 
            // lblClientName
            // 
            resources.ApplyResources(this.lblClientName, "lblClientName");
            this.lblClientName.Name = "lblClientName";
            // 
            // pnlCard2
            // 
            resources.ApplyResources(this.pnlCard2, "pnlCard2");
            this.pnlCard2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(40)))));
            this.pnlCard2.Controls.Add(this.rtbLogsClient);
            this.pnlCard2.Controls.Add(this.label2);
            this.pnlCard2.Name = "pnlCard2";
            // 
            // rtbLogsClient
            // 
            resources.ApplyResources(this.rtbLogsClient, "rtbLogsClient");
            this.rtbLogsClient.BackColor = System.Drawing.Color.White;
            this.rtbLogsClient.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbLogsClient.ForeColor = System.Drawing.SystemColors.InfoText;
            this.rtbLogsClient.Name = "rtbLogsClient";
            this.rtbLogsClient.ReadOnly = true;
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.ForeColor = System.Drawing.SystemColors.Highlight;
            this.label2.Name = "label2";
            // 
            // btnSubmit
            // 
            resources.ApplyResources(this.btnSubmit, "btnSubmit");
            this.btnSubmit.BackColor = System.Drawing.Color.DimGray;
            this.btnSubmit.FlatAppearance.BorderSize = 0;
            this.btnSubmit.ForeColor = System.Drawing.Color.Black;
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.UseVisualStyleBackColor = false;
            // 
            // checkBox1
            // 
            resources.ApplyResources(this.checkBox1, "checkBox1");
            this.checkBox1.FlatAppearance.BorderSize = 0;
            this.checkBox1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Lime;
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // notifyIcon1
            // 
            resources.ApplyResources(this.notifyIcon1, "notifyIcon1");
            // 
            // frmClientMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(24)))), ((int)(((byte)(33)))));
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.btnSubmit);
            this.Controls.Add(this.pnlCard2);
            this.Controls.Add(this.pnlCard1);
            this.Controls.Add(this.panel1);
            this.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.Name = "frmClientMain";
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.frmClientMain_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.pnlCard1.ResumeLayout(false);
            this.pnlCard1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.pnlCard2.ResumeLayout(false);
            this.pnlCard2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Panel pnlCard1;
        private System.Windows.Forms.Panel pnlCard2;
        private System.Windows.Forms.Label lblClientIPAddress;
        private System.Windows.Forms.Label lblClientName;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.RichTextBox rtbLogsClient;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label label5;
    }
}

