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
        public FrmImageViewer(string imagePath)
        {
            InitializeComponent();
            try
            {
                // Nạp bức ảnh từ ổ đĩa cứng vào PictureBox
                picViewer.Image = Image.FromFile(imagePath);

                // Cập nhật tiêu đề cửa sổ cho ngầu
                this.Text = "Xem ảnh chụp màn hình Client";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải ảnh: " + ex.Message);
            }
        }

        private void FrmImageViewer_Load(object sender, EventArgs e)
        {

        }

        private void picScreen_Click(object sender, EventArgs e)
        {

        }
    }
}
