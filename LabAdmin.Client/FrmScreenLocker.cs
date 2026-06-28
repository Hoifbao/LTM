using System;
using System.Drawing;
using System.Windows.Forms;

namespace LabAdmin.Client
{
    public partial class FrmScreenLocker : Form
    {
        // Mật khẩu mở khóa khẩn cấp (dùng khi mất kết nối mạng)
        // Giáo viên có thể đọc mật khẩu này trực tiếp trên server hoặc thay đổi tại đây
        private const string EMERGENCY_PASSWORD = "giaovien2025";

        // Ten may + IP nhan tu FrmClient de hien len man hinh khoa
        // VD: "DESKTOP-FI27QT2 · 192.168.1.94"
        public string ClientInfo { get; set; } = "";


        // =========================================================
        // 1. KHỞI TẠO FORM VÀ ĐĂNG KÝ SỰ KIỆN
        // =========================================================
        public FrmScreenLocker()
        {
            InitializeComponent();

            this.FormClosing += FrmScreenLocker_FormClosing;

            // Gắn sự kiện cho nút Mở và ô nhập mật khẩu
            btnRefresh.Click += BtnRefresh_Click;
            textBox1.KeyDown += TextBox1_KeyDown;

            // Ẩn ký tự mật khẩu
            textBox1.PasswordChar = '●';
        }

        // Chan dong form bang Alt+F4 / end task
        private void FrmScreenLocker_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
                e.Cancel = true;
        }

        // Chiem toan man hinh (Designer wire su kien nay)
        private void FrmScreenLocker_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;

            // Doi mau panel tu xam sang toi hon
            panel1.BackColor = Color.FromArgb(28, 30, 38);

            // Doi mau chu "May Tinh Da Bi Khoa" sang trang ro hon
            label1.ForeColor = Color.White;

            // Hien ten may + IP neu co (label3: "Vui long doi may chu...")
            if (!string.IsNullOrEmpty(ClientInfo))
                label3.Text = ClientInfo;
        }



        private void BtnRefresh_Click(object sender, EventArgs e) => TryUnlock();

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                TryUnlock();
            }
        }

        // Mo khoa bang mat khau khan cap (phia client)
        private void TryUnlock()
        {
            if (textBox1.Text == EMERGENCY_PASSWORD)
            {
                HideLocker();
            }
            else
            {
                MessageBox.Show("Mật khẩu không đúng!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Clear();
                textBox1.Focus();
            }
        }

        // Mo khoa do SERVER ra lenh CMD_UNLOCK (an toan cross-thread)
        public void ForceUnlock()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(ForceUnlock)); return; }
            HideLocker();
        }

        private void HideLocker()
        {
            this.FormClosing -= FrmScreenLocker_FormClosing;
            this.Hide();
            this.FormClosing += FrmScreenLocker_FormClosing;
            textBox1.Clear();
        }


        private void label2_Click(object sender, EventArgs e) { }

        private void btnRefresh_Click_1(object sender, EventArgs e)
        {

        }
    }
}
