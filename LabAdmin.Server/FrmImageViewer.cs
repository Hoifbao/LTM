using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LabAdmin.Server
{
    public partial class FrmImageViewer : Form
    {
        private double zoomRatio = 1.0;
        private Point startPoint;
        private bool isDraggingImage = false;
        private bool isDraggingForm = false;
        private Point dragStartPoint;

        public FrmImageViewer()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            if (pnlContainer != null) pnlContainer.MouseWheel += PnlContainer_MouseWheel;
            this.FormClosed += (s, e) => DisposeCurrentImage();

            // Label hint zoom - them vao cuoi form (duoi pnlContainer)
            var lblHint = new Label
            {
                Text = "  🔍 Cuộn để zoom  ·  Giữ chuột trái để kéo ảnh",
                Dock = DockStyle.Bottom,
                Height = 26,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(130, 130, 130),
                BackColor = Color.FromArgb(18, 18, 18)
            };
            this.Controls.Add(lblHint);
            lblHint.BringToFront();
        }


        public FrmImageViewer(string imagePath) : this()
        {
            this.Text = "Xem ảnh chụp màn hình Client";
            LoadNewImage(imagePath);
        }

        // Doc anh qua stream roi dong ngay -> KHONG giu lock file, cho phep ghi de lan sau
        private Image LoadImageNoLock(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var tmp = Image.FromStream(fs))
                return new Bitmap(tmp);
        }

        private void DisposeCurrentImage()
        {
            if (picViewer.Image != null)
            {
                var old = picViewer.Image;
                picViewer.Image = null;
                old.Dispose();
            }
        }

        public void LoadNewImage(string newImagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(newImagePath)) return;
                DisposeCurrentImage();
                picViewer.Image = LoadImageNoLock(newImagePath);
                picViewer.Size = picViewer.Image.Size;
                zoomRatio = 1.0;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi nạp ảnh: " + ex.Message); }
        }
       

        private void button1_Click(object sender, EventArgs e) => this.Hide();

        private void pnlHeader_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { isDraggingForm = true; dragStartPoint = e.Location; }
        }
        private void pnlHeader_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingForm)
            {
                this.Location = new Point((this.Location.X - dragStartPoint.X) + e.X,
                                          (this.Location.Y - dragStartPoint.Y) + e.Y);
                this.Update();
            }
        }

        private void pnlHeader_MouseUp(object sender, MouseEventArgs e) => isDraggingForm = false;

        private void PnlContainer_MouseWheel(object sender, MouseEventArgs e)
        {
            if (picViewer.Image == null) return;
            double zoomFactor = (e.Delta > 0) ? 1.1 : 0.9;
            if ((zoomRatio > 5.0 && e.Delta > 0) || (zoomRatio < 0.2 && e.Delta < 0)) return;
            zoomRatio *= zoomFactor;
            picViewer.Width = (int)(picViewer.Image.Width * zoomRatio);
            picViewer.Height = (int)(picViewer.Image.Height * zoomRatio);
        }

        private void picViewer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDraggingImage = true; startPoint = e.Location; picViewer.Cursor = Cursors.NoMove2D;
            }
        }
        public void UpdateClientInfo(string pcName, string ipAddress)
        {
            // Hien thi: DESKTOP-FI27QT2 • 192.168.1.94 • 19:49:01
            lblPCName.Text = $"{pcName} • {ipAddress} • {DateTime.Now:HH:mm:ss}";
            lblIPTime.Visible = false; // an di vi thong tin da gop vao lblPCName
        }
        private void picViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingImage && pnlContainer != null)
            {
                int dx = e.X - startPoint.X, dy = e.Y - startPoint.Y;
                pnlContainer.AutoScrollPosition = new Point(
                    -pnlContainer.AutoScrollPosition.X - dx, -pnlContainer.AutoScrollPosition.Y - dy);
            }
        }
        private void picViewer_MouseUp(object sender, MouseEventArgs e)
        {
            isDraggingImage = false; picViewer.Cursor = Cursors.Default;
        }
        private void picViewer_MouseEnter(object sender, EventArgs e) => picViewer.Focus();

        private void btnSaveImage_Click(object sender, EventArgs e)
        {
            if (picViewer.Image == null)
            {
                MessageBox.Show("Không có ảnh để lưu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                // Ten file mac dinh: DESKTOP-FI27QT2_194901.jpg
                sfd.FileName = lblPCName.Text.Split('•')[0].Trim() + "_" + DateTime.Now.ToString("HHmmss");
                sfd.Title = "Lưu ảnh chụp màn hình sinh viên";
                sfd.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        picViewer.Image.Save(sfd.FileName);
                        MessageBox.Show("Đã lưu ảnh thành công!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi lưu ảnh: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        private void FrmImageViewer_Load(object sender, EventArgs e) { }
        private void picScreen_Click(object sender, EventArgs e) { }
        private void pnlHeader_Paint(object sender, PaintEventArgs e) { }
        private void pictureBox4_Click(object sender, EventArgs e) { }
        private void lblPCName_Click(object sender, EventArgs e) { }
        private void lblIPTime_Click(object sender, EventArgs e) { }
    }
}
