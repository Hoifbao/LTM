using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LabAdmin.Client
{
    public partial class FrmScreenLocker : Form
    {
        public FrmScreenLocker()
        {
            InitializeComponent();
            // Đăng ký sự kiện quản lý đóng Form
            this.FormClosing += FrmScreenLocker_FormClosing;
        }

        // ----- XỬ LÝ CHẶN ĐÓNG FORM BẰNG ALT + F4 -----
        private void FrmScreenLocker_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Nếu tác vụ đóng Form xuất phát từ phía người dùng (ấn Alt + F4, bấm Close...)
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // Hủy lệnh đóng Form, ép màn hình tiếp tục khóa
            }
        }

        //private void FrmScreenLocker_Load(object sender, EventArgs e)
        //{

        //}
        private void FrmScreenLocker_Load(object sender, EventArgs e) 
        { this.FormBorderStyle = FormBorderStyle.None; 
            this.WindowState = FormWindowState.Maximized; 
            this.TopMost = true; 
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}