using System;
using System.Windows.Forms;

namespace LabAdmin.Client
{
    public partial class FrmScreenLocker : Form
    {
        // =========================================================
        // 1. KHỞI TẠO FORM VÀ ĐĂNG KÝ SỰ KIỆN
        // =========================================================
        public FrmScreenLocker()
        {
            InitializeComponent();

            // Lắng nghe sự kiện trước khi Form bị đóng để can thiệp chặn lại
            this.FormClosing += FrmScreenLocker_FormClosing;
        }

        // =========================================================
        // 2. CHẶN PHÍM TẮT ALT + F4
        // =========================================================
        private void FrmScreenLocker_FormClosing(object sender, FormClosingEventArgs e)
        {
            // CloseReason.UserClosing: Là khi sinh viên cố tình bấm Alt+F4 hoặc dùng Task Manager end task
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Hủy bỏ lệnh đóng Form, ép Form phải mở tiếp
                e.Cancel = true;
            }
        }

        // =========================================================
        // 3. CẤU HÌNH HIỂN THỊ CHIẾM QUYỀN TOÀN MÀN HÌNH
        // =========================================================
        private void FrmScreenLocker_Load(object sender, EventArgs e)
        {
            // Tắt viền cửa sổ, tắt 3 nút thu phóng/đóng mặc định của Windows
            this.FormBorderStyle = FormBorderStyle.None;

            // Phóng to tràn viền, che luôn thanh Taskbar bên dưới
            this.WindowState = FormWindowState.Maximized;

            // Thuộc tính quan trọng nhất: Ép Form này luôn nằm đè lên trên tất cả các ứng dụng khác
            this.TopMost = true;
        }
    }
}