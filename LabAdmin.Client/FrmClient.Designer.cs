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
            this.lblClientIPAddress = new System.Windows.Forms.Label();
            this.lblClientName = new System.Windows.Forms.Label();
            this.pnlCard2 = new System.Windows.Forms.Panel();
            this.rtbLogsClient = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.label5 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.pnlCard1.SuspendLayout();
            this.pnlCard2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(40)))));
            this.panel1.Controls.Add(this.btnRefresh);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(796, 85);
            this.panel1.TabIndex = 0;
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnRefresh.FlatAppearance.BorderSize = 0;
            this.btnRefresh.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnRefresh.Location = new System.Drawing.Point(686, 19);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(50, 50);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "↺";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label1.Location = new System.Drawing.Point(42, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(414, 45);
            this.label1.TabIndex = 0;
            this.label1.Text = "Bảng điều khiển máy trạm";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // pnlCard1
            // 
            this.pnlCard1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(40)))));
            this.pnlCard1.Controls.Add(this.pictureBox3);
            this.pnlCard1.Controls.Add(this.label5);
            this.pnlCard1.Controls.Add(this.lblClientIPAddress);
            this.pnlCard1.Controls.Add(this.lblClientName);
            this.pnlCard1.Location = new System.Drawing.Point(31, 134);
            this.pnlCard1.Name = "pnlCard1";
            this.pnlCard1.Size = new System.Drawing.Size(726, 135);
            this.pnlCard1.TabIndex = 1;
            // 
            // lblClientIPAddress
            // 
            this.lblClientIPAddress.AutoSize = true;
            this.lblClientIPAddress.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblClientIPAddress.Location = new System.Drawing.Point(29, 79);
            this.lblClientIPAddress.Name = "lblClientIPAddress";
            this.lblClientIPAddress.Size = new System.Drawing.Size(207, 38);
            this.lblClientIPAddress.TabIndex = 1;
            this.lblClientIPAddress.Text = "Địa chỉ IP: ..........";
            this.lblClientIPAddress.Click += new System.EventHandler(this.lblIPAddress_Click);
            // 
            // lblClientName
            // 
            this.lblClientName.AutoSize = true;
            this.lblClientName.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblClientName.Location = new System.Drawing.Point(29, 18);
            this.lblClientName.Name = "lblClientName";
            this.lblClientName.Size = new System.Drawing.Size(123, 38);
            this.lblClientName.TabIndex = 0;
            this.lblClientName.Text = "PC: .......";
            // 
            // pnlCard2
            // 
            this.pnlCard2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(40)))));
            this.pnlCard2.Controls.Add(this.rtbLogsClient);
            this.pnlCard2.Controls.Add(this.label2);
            this.pnlCard2.Location = new System.Drawing.Point(31, 310);
            this.pnlCard2.Name = "pnlCard2";
            this.pnlCard2.Size = new System.Drawing.Size(726, 241);
            this.pnlCard2.TabIndex = 2;
            // 
            // rtbLogsClient
            // 
            this.rtbLogsClient.BackColor = System.Drawing.Color.White;
            this.rtbLogsClient.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbLogsClient.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbLogsClient.ForeColor = System.Drawing.SystemColors.InfoText;
            this.rtbLogsClient.Location = new System.Drawing.Point(36, 62);
            this.rtbLogsClient.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.rtbLogsClient.Name = "rtbLogsClient";
            this.rtbLogsClient.ReadOnly = true;
            this.rtbLogsClient.Size = new System.Drawing.Size(650, 161);
            this.rtbLogsClient.TabIndex = 22;
            this.rtbLogsClient.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.Highlight;
            this.label2.Location = new System.Drawing.Point(29, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(304, 38);
            this.label2.TabIndex = 2;
            this.label2.Text = "Thông báo từ máy chủ ";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // btnSubmit
            // 
            this.btnSubmit.BackColor = System.Drawing.Color.DimGray;
            this.btnSubmit.FlatAppearance.BorderSize = 0;
            this.btnSubmit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSubmit.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSubmit.ForeColor = System.Drawing.Color.Black;
            this.btnSubmit.Location = new System.Drawing.Point(67, 573);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(247, 38);
            this.btnSubmit.TabIndex = 3;
            this.btnSubmit.Text = "Chọn File Và Nộp Bài";
            this.btnSubmit.UseVisualStyleBackColor = false;
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.FlatAppearance.BorderSize = 0;
            this.checkBox1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Lime;
            this.checkBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBox1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox1.Location = new System.Drawing.Point(520, 578);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(182, 36);
            this.checkBox1.TabIndex = 5;
            this.checkBox1.Text = "Giơ tay hỗ trợ";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            // 
            // pictureBox3
            // 
            this.pictureBox3.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox3.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox3.Image")));
            this.pictureBox3.Location = new System.Drawing.Point(587, 11);
            this.pictureBox3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(25, 35);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox3.TabIndex = 9;
            this.pictureBox3.TabStop = false;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.label5.Location = new System.Drawing.Point(617, 18);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 21);
            this.label5.TabIndex = 8;
            this.label5.Text = "Đã kết nối";
            // 
            // frmClientMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(24)))), ((int)(((byte)(33)))));
            this.ClientSize = new System.Drawing.Size(796, 636);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.btnSubmit);
            this.Controls.Add(this.pnlCard2);
            this.Controls.Add(this.pnlCard1);
            this.Controls.Add(this.panel1);
            this.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "frmClientMain";
            this.Opacity = 0D;
            this.ShowInTaskbar = false;
            this.Text = "Client";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.pnlCard1.ResumeLayout(false);
            this.pnlCard1.PerformLayout();
            this.pnlCard2.ResumeLayout(false);
            this.pnlCard2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
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

