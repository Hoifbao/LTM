using LabAdmin.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LabAdmin.Server
{
    public partial class frmServerMain : Form
    {
        private bool isDragging = false;
        private Point lastCursor;
        private Point lastForm;
        // Lưu danh sách các máy đã kết nối (Dùng IP làm key, Socket làm value để dễ tìm)
        private Dictionary<string, Socket> clientList = new Dictionary<string, Socket>();
        // Tách thread riêng cho mạng để form không bị lag/treo
        private Thread udpThread;
        private Thread tcpThread;
        private string SaveFolderPath = "";
        public frmServerMain()
        {
            InitializeComponent();
            // Khởi chạy thread lắng nghe phản hồi UDP
            udpThread = new Thread(ListenUDP);
            udpThread.IsBackground = true; // Set background để tự kill khi tắt form
            udpThread.Start();
            // Khởi chạy thread lắng nghe kết nối TCP
            tcpThread = new Thread(ListenTCP);
            tcpThread.IsBackground = true;
            tcpThread.Start();
        }
        private void frmServerMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("Hệ thống đang chạy. Bạn có chắc chắn muốn tắt Server và ngắt kết nối toàn bộ máy trạm?",
                                                  "Cảnh báo an toàn",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Warning);
            if (result == DialogResult.No)
            {
                e.Cancel = true; // Hủy lệnh đóng Form
            }
        }
        // Hàm update data lên grid (dùng Invoke để fix lỗi cross-thread khi gọi từ thread mạng)
        public void AddOrUpdateClient(string ip, string machineName, string status)
        {
            // Check nếu đang ở thread khác thì đẩy việc update UI về thread chính
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string, string, string>(AddOrUpdateClient), ip, machineName, status);
                return;
            }
            // Duyệt xem IP đã tồn tại trong grid chưa
            foreach (DataGridViewRow row in dgvClients.Rows)
            {
                if (row.Cells["IP"].Value.ToString() == ip)
                {
                    // Có rồi thì chỉ update status
                    row.Cells["Status"].Value = status;
                    return;
                }
            }
            // Chưa có thì add thêm dòng mới
            dgvClients.Rows.Add(ip, machineName, status);
        }
        private void btnScan_Click(object sender, EventArgs e)
        {
            try
            {
                // Clear grid trước khi quét lại
                dgvClients.Rows.Clear();
                // Dùng UDP để broadcast gọi các máy con
                Socket sckUdp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                sckUdp.EnableBroadcast = true;
                // Chuẩn bị gói tin lệnh CMD_SCAN
                byte[] sendData = Encoding.UTF8.GetBytes(NetworkProtocol.CMD_SCAN);
                // Bắn broadcast tới toàn mạng LAN ở port 8888
                IPEndPoint ep = new IPEndPoint(IPAddress.Broadcast, 8888);
                sckUdp.SendTo(sendData, ep);

                sckUdp.Close();
                MessageBox.Show("Đã phát lệnh điểm danh toàn phòng máy!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi phát sóng UDP: " + ex.Message);
            }
        }
        private void ListenUDP()
        {
            Socket sckUdp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            // Server hứng gói tin phản hồi ở port 8889
            sckUdp.Bind(new IPEndPoint(IPAddress.Any, 8889));
            byte[] buffer = new byte[1024];
            EndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {
                    int size = sckUdp.ReceiveFrom(buffer, ref remoteEp);
                    string data = Encoding.UTF8.GetString(buffer, 0, size);
                    // Cắt chuỗi để lấy data theo ký tự phân cách
                    string[] parts = data.Split(NetworkProtocol.DELIMITER);
                    // Nếu đúng mã phản hồi thì update lên grid
                    if (parts[0] == NetworkProtocol.REP_SCAN_ACK)
                    {
                        string machineName = parts[1];
                        string ip = ((IPEndPoint)remoteEp).Address.ToString();
                        AddOrUpdateClient(ip, machineName, "Online");
                    }
                }
                catch { }
            }
        }
        private void ListenTCP()
        {
            Socket sckTcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Bật cờ reuse để chống lỗi kẹt cổng 
            sckTcp.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            // TCP chạy cổng 9090 để không đụng UDP
            sckTcp.Bind(new IPEndPoint(IPAddress.Any, 9090));
            sckTcp.Listen(100);
            while (true)
            {
                try
                {
                    Socket clientSocket = sckTcp.Accept();
                    string ip = ((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString();

                    // Lưu client vào dictionary để quản lý
                    if (!clientList.ContainsKey(ip))
                    {
                        clientList.Add(ip, clientSocket);
                    }
                    else
                    {
                        clientList[ip] = clientSocket;
                    }
                    AddOrUpdateClient(ip, "Đã kết nối TCP", "Connected");
                    // Cấp riêng 1 thread cho mỗi client để hứng data liên tục
                    Thread receiveThread = new Thread(() => ReceiveData(clientSocket, ip));
                    receiveThread.IsBackground = true;
                    receiveThread.Start();
                }
                catch { }
            }
        }
        private void ReceiveData(Socket client, string ip)
        {
            // Cấp buffer 5MB để hứng đủ các file/ảnh có dung lượng lớn
            byte[] buffer = new byte[5 * 1024 * 1024];

            while (true)
            {
                try
                {
                    int size = client.Receive(buffer);
                    if (size == 0) break; // Client đứt kết nối thì out
                    // Nếu data lớn > 1000 byte thì xác định là file zip hoặc file ảnh
                    if (size > 1000)
                    {
                        // Check header (Magic Number): File ZIP luôn bắt đầu bằng 'PK' (0x50, 0x4B)
                        if (buffer[0] == 0x50 && buffer[1] == 0x4B)
                        {
                            // ----- XỬ LÝ NHẬN BÀI THI (FILE ZIP) -----
                            // Nếu giáo viên chưa chọn thư mục, mặc định lưu vào thư mục "ThuBai" của phần mềm
                            string baseFolder = string.IsNullOrEmpty(SaveFolderPath) ? System.IO.Path.Combine(Application.StartupPath, "ThuBai") : SaveFolderPath;
                            // Tạo thư mục riêng cho từng IP sinh viên
                            string folderPath = System.IO.Path.Combine(baseFolder, ip);
                            if (!System.IO.Directory.Exists(folderPath))
                            {
                                System.IO.Directory.CreateDirectory(folderPath);
                            }
                            // Gắn thời gian vào tên file để tránh ghi đè
                            string filePath = System.IO.Path.Combine(folderPath, "BaiLam_" + DateTime.Now.ToString("HHmmss") + ".zip");
                            // Cắt đúng mảng byte thực tế nhận được rồi lưu xuống ổ
                            byte[] fileData = new byte[size];
                            Array.Copy(buffer, fileData, size);
                            System.IO.File.WriteAllBytes(filePath, fileData);
                            // Dùng LogToConsole thay vì MessageBox để không bị vướng màn hình
                            LogToConsole($"Đã thu bài thành công từ máy: {ip}");
                        }
                        else
                        {
                            // ----- XỬ LÝ NHẬN ẢNH CHỤP MÀN HÌNH -----
                            string folderPath = System.IO.Path.Combine(Application.StartupPath, "AnhChup", ip);
                            if (!System.IO.Directory.Exists(folderPath))
                                System.IO.Directory.CreateDirectory(folderPath);
                            string imgPath = System.IO.Path.Combine(folderPath, "Screen_" + DateTime.Now.ToString("HHmmss") + ".jpg");
                            byte[] imageData = new byte[size];
                            Array.Copy(buffer, imageData, size);
                            System.IO.File.WriteAllBytes(imgPath, imageData);

                            // Nhận xong thì bật form FrmImageViewer lên để xem luôn
                            this.Invoke(new Action(() => {
                                FrmImageViewer viewer = new FrmImageViewer(imgPath);
                                viewer.Show();
                            }));
                        }
                    }
                    else
                    {
                        // Gói nhỏ thì decode ra chuỗi text bình thường
                        string textData = Encoding.UTF8.GetString(buffer, 0, size);
                    }
                }
                catch
                {
                    break; // Có lỗi (rớt mạng, client sập) thì break vòng lặp
                }
            }
        }
        // Lấy IP từ dòng đang được chọn trên DataGridView
        private string GetSelectedIP()
        {
            if (dgvClients.SelectedRows.Count > 0)
            {
                return dgvClients.SelectedRows[0].Cells["IP"].Value.ToString();
            }
            return null;
        }
        private void btnLock_Click(object sender, EventArgs e)
        {
            string targetIP = GetSelectedIP();
            // Check xem đã chọn máy và client còn sống trong list không
            if (targetIP != null && clientList.ContainsKey(targetIP))
            {
                try
                {
                    Socket client = clientList[targetIP];

                    // Đóng gói và bắn lệnh LOCK
                    byte[] data = Encoding.UTF8.GetBytes(NetworkProtocol.CMD_LOCK);
                    client.Send(data);
                    MessageBox.Show("Đã gửi lệnh KHÓA tới máy: " + targetIP, "Thành công");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi gửi lệnh: " + ex.Message, "Lỗi");
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một máy tính đã kết nối TCP trên bảng!", "Chú ý");
            }
        }
        private void btnMsg_Click(object sender, EventArgs e)
        {
            string targetIP = GetSelectedIP();
            // Bắt lỗi rỗng
            if (string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                MessageBox.Show("Vui lòng nhập nội dung cần thông báo!", "Chú ý");
                return;
            }
            if (targetIP != null && clientList.ContainsKey(targetIP))
            {
                try
                {
                    Socket client = clientList[targetIP];

                    // Nối lệnh và message để gửi qua
                    string payload = NetworkProtocol.CMD_MSG + NetworkProtocol.DELIMITER + txtMessage.Text;
                    byte[] data = Encoding.UTF8.GetBytes(payload);

                    client.Send(data);

                    MessageBox.Show("Đã gửi thông báo tới máy: " + targetIP, "Thành công");
                    txtMessage.Clear(); // Gửi xong thì clear text đi
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi gửi thông báo: " + ex.Message, "Lỗi");
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một máy tính đã kết nối TCP trên bảng!", "Chú ý");
            }
        }
        private void btnPull_Click(object sender, EventArgs e)
        {
            string targetIP = GetSelectedIP();
            if (targetIP != null && clientList.ContainsKey(targetIP))
            {
                // Ứng dụng Bài 21: Bật hộp thoại chọn thư mục lưu bài
                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    fbd.Description = "Chọn thư mục để lưu bài thi của sinh viên:";
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        // Lưu tạm đường dẫn mà giáo viên vừa chọn vào một biến toàn cục
                        // (Bạn cần khai báo biến string SaveFolderPath ở đầu class)
                        SaveFolderPath = fbd.SelectedPath;
                        // Gửi lệnh thu bài đi
                        Socket client = clientList[targetIP];
                        byte[] data = Encoding.UTF8.GetBytes(NetworkProtocol.CMD_PULL);
                        client.Send(data);

                        LogToConsole($"Đã phát lệnh THU BÀI tới máy {targetIP}. Sẽ lưu tại: {SaveFolderPath}");
                    }
                }
            }
        }
        // Hàm hỗ trợ ghi log có màu sắc
        private void LogToConsole(string message)
        {
            if (lstLogs.InvokeRequired)
            {
                lstLogs.Invoke(new Action<string>(LogToConsole), message);
                return;
            }
            lstLogs.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            lstLogs.TopIndex = lstLogs.Items.Count - 1; // Tự động cuộn xuống dòng mới nhất
        }
        private void btnCapture_Click(object sender, EventArgs e)
        {
            string targetIP = GetSelectedIP();
            if (targetIP != null && clientList.ContainsKey(targetIP))
            {
                try
                {
                    Socket client = clientList[targetIP];
                    // Bắn lệnh kích hoạt chụp lén
                    byte[] data = Encoding.UTF8.GetBytes(NetworkProtocol.CMD_CAPTURE);
                    client.Send(data);

                    MessageBox.Show("Đã gửi lệnh CHỤP MÀN HÌNH tới máy: " + targetIP, "Thành công");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi gửi lệnh chụp màn hình: " + ex.Message, "Lỗi");
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một máy tính đã kết nối TCP trên bảng!", "Chú ý");
            }
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e) { }
        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e) { }
        private void dgvClients_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void Form2_Load(object sender, EventArgs e) { }
        private void txtMessage_TextChanged(object sender, EventArgs e)
        {

        }

        private void lstLogs_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pnlTopBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastCursor = Cursor.Position;
                lastForm = this.Location;
            }
        }

        private void pnlTopBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                int xDiff = Cursor.Position.X - lastCursor.X;
                int yDiff = Cursor.Position.Y - lastCursor.Y;
                this.Location = new Point(lastForm.X + xDiff, lastForm.Y + yDiff);
            }
        }

        private void pnlTopBar_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void dgvClients_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgvClients_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == dgvClients.Columns["colStatus"].Index && e.RowIndex >= 0)
            {
                e.PaintBackground(e.CellBounds, true);

                string status = e.Value?.ToString();
                if (string.IsNullOrEmpty(status)) return;

                // Thiết lập màu sắc: Xanh lá cho "Đang hoạt động", Đỏ cho "Đã khóa"
                Color backColor = (status == "Đang hoạt động") ? Color.FromArgb(220, 245, 225) : Color.FromArgb(250, 230, 230);
                Color foreColor = (status == "Đang hoạt động") ? Color.FromArgb(34, 160, 80) : Color.FromArgb(180, 50, 50);

                // Tính toán kích thước nút bo góc
                Rectangle rect = new Rectangle(e.CellBounds.X + 15, e.CellBounds.Y + 8, e.CellBounds.Width - 30, e.CellBounds.Height - 16);

                using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int r = 10; // Độ bo góc
                    path.AddArc(rect.X, rect.Y, r, r, 180, 90);
                    path.AddArc(rect.Right - r, rect.Y, r, r, 270, 90);
                    path.AddArc(rect.Right - r, rect.Bottom - r, r, r, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - r, r, r, 90, 90);
                    path.CloseFigure();

                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(new SolidBrush(backColor), path);
                    e.Graphics.DrawString(status, new Font("Segoe UI", 9, FontStyle.Bold), new SolidBrush(foreColor), rect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }
                e.Handled = true;
            }
        }
    }
}