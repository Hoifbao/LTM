using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            this.DoubleBuffered = true; // Bật chống chớp giật màn hình
        }
        public FrmImageViewer(string imagePath) : this() // Lệnh :this() để gọi hàm bên trên
        {
            try
            {
                if (!string.IsNullOrEmpty(imagePath))
                {
                    picViewer.Image = Image.FromFile(imagePath);
                    // Ép kích thước PictureBox bằng ảnh thật để bật thanh cuộn
                    picViewer.Size = picViewer.Image.Size;
                }
                this.Text = "Xem ảnh chụp màn hình Client";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải ảnh: " + ex.Message);
            }

            // Bật tính năng cuộn chuột cho Panel
            if (pnlContainer != null)
            {
                pnlContainer.MouseWheel += PnlContainer_MouseWheel;
            }
        }
        // ----- 4. HÀM NẠP ẢNH MỚI KHI FORM ĐANG ẨN -----
        public void LoadNewImage(string newImagePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(newImagePath))
                {
                    if (picViewer.Image != null) picViewer.Image.Dispose();
                    picViewer.Image = Image.FromFile(newImagePath);
                    picViewer.Size = picViewer.Image.Size;
                    zoomRatio = 1.0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nạp ảnh: " + ex.Message);
            }
        }

        // ----- 5. NÚT ĐÓNG BÊN PHẢI (BUTTON1) -----
        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide(); // Giấu Form đi để mở cho lẹ vào lần sau
        }

        // ----- 6. DI CHUYỂN FORM BẰNG THANH HEADER -----
        private void pnlHeader_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDraggingForm = true;
                dragStartPoint = e.Location;
            }
        }

        private void pnlHeader_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingForm)
            {
                this.Location = new Point(
                    (this.Location.X - dragStartPoint.X) + e.X,
                    (this.Location.Y - dragStartPoint.Y) + e.Y
                );
                this.Update();
            }
        }

        private void pnlHeader_MouseUp(object sender, MouseEventArgs e)
        {
            isDraggingForm = false;
        }

        // ----- 7. ZOOM BẰNG LĂN CHUỘT -----
        private void PnlContainer_MouseWheel(object sender, MouseEventArgs e)
        {
            if (picViewer.Image == null) return;

            double zoomFactor = (e.Delta > 0) ? 1.1 : 0.9;
            if ((zoomRatio > 5.0 && e.Delta > 0) || (zoomRatio < 0.2 && e.Delta < 0)) return;

            zoomRatio *= zoomFactor;
            picViewer.Width = (int)(picViewer.Image.Width * zoomRatio);
            picViewer.Height = (int)(picViewer.Image.Height * zoomRatio);
        }

        // ----- 8. CẦM KÉO ẢNH BÊN TRONG (PANNING) -----
        private void picViewer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDraggingImage = true;
                startPoint = e.Location;
                picViewer.Cursor = Cursors.NoMove2D;
            }
        }
        public void UpdateClientInfo(string pcName, string ipAddress)
        {
            lblPCName.Text = pcName;

            // Lấy giờ hiện tại của hệ thống lúc nhận được ảnh
            string currentTime = DateTime.Now.ToString("HH:mm:ss");

            lblIPTime.Text = $"IP: {ipAddress} • {currentTime}";
        }
        private void picViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingImage && pnlContainer != null)
            {
                int deltaX = e.X - startPoint.X;
                int deltaY = e.Y - startPoint.Y;

                pnlContainer.AutoScrollPosition = new Point(
                    -pnlContainer.AutoScrollPosition.X - deltaX,
                    -pnlContainer.AutoScrollPosition.Y - deltaY
                );
            }
        }

        private void picViewer_MouseUp(object sender, MouseEventArgs e)
        {
            isDraggingImage = false;
            picViewer.Cursor = Cursors.Default;
        }

        private void picViewer_MouseEnter(object sender, EventArgs e)
        {
            picViewer.Focus();
        }

        // =======================================================
        // CÁC HÀM SỰ KIỆN TRỐNG (GIỮ LẠI ĐỂ DESIGNER KHÔNG BÁO LỖI)
        // =======================================================
        private void FrmImageViewer_Load(object sender, EventArgs e) { }
        private void picScreen_Click(object sender, EventArgs e) { }
        private void pnlHeader_Paint(object sender, PaintEventArgs e) { }

        private void btnSaveImage_Click(object sender, EventArgs e)
        {
            if (picViewer.Image == null)
            {
                MessageBox.Show("Không có ảnh để lưu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                // Gợi ý tên file mặc định là tên máy trạm và giờ chụp
                string defaultName = lblPCName.Text + "_" + DateTime.Now.ToString("HHmmss");

                sfd.FileName = defaultName;
                sfd.Title = "Lưu ảnh chụp màn hình sinh viên";
                sfd.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        picViewer.Image.Save(sfd.FileName);
                        MessageBox.Show("Đã lưu ảnh thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi lưu ảnh: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void lblPCName_Click(object sender, EventArgs e)
        {

        }

        private void lblIPTime_Click(object sender, EventArgs e)
        {

        }
    }
}
