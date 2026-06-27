using LabAdmin.Shared;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace LabAdmin.Client
{
    public partial class frmClientMain : Form
    {
        // Tách thread riêng cho mạng để form không bị lag/treo
        private Thread udpThread;
        private Thread tcpThread;
        private Socket tcpSocket;

        // Form khóa màn hình
        private FrmScreenLocker lockerForm;

        // Lưu IP Server động (Lấy được khi Server phát UDP quét mạng)
        private string serverIP = "127.0.0.1";

        public frmClientMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Cài đặt cho ứng dụng tự khởi động cùng Windows (Ghi Registry)
            SetStartup();

            // Khởi chạy thread lắng nghe mạng UDP cổng 8888
            udpThread = new Thread(ListenUDP);
            udpThread.IsBackground = true; // Set background để tự kill khi tắt form
            udpThread.Start();

            // Khởi chạy thread kết nối TCP tới Server
            tcpThread = new Thread(ConnectTCP);
            tcpThread.IsBackground = true;
            tcpThread.Start();
        }

        // ----- XỬ LÝ KHỞI ĐỘNG CÙNG WINDOWS -----
        private void SetStartup()
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                rk.SetValue("LabAdminClient", Application.ExecutablePath);
            }
            catch
            {
                // Bỏ qua nếu không có quyền ghi Registry
            }
        }

        // ----- XỬ LÝ MẠNG UDP (ĐIỂM DANH) -----
        private void ListenUDP()
        {
            Socket sckUdp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            // Client hứng gói tin broadcast ở port 8888
            sckUdp.Bind(new IPEndPoint(IPAddress.Any, 8888));
            byte[] buffer = new byte[1024];
            EndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                try
                {
                    int size = sckUdp.ReceiveFrom(buffer, ref remoteEp);
                    string data = Encoding.UTF8.GetString(buffer, 0, size);

                    // Nếu nhận được lệnh Quét từ Server
                    if (data == NetworkProtocol.CMD_SCAN)
                    {
                        // Cập nhật IP của Server dựa trên người gửi gói tin
                        serverIP = ((IPEndPoint)remoteEp).Address.ToString();

                        // Phản hồi điểm danh về Server qua port 8889
                        string machineName = Environment.MachineName;
                        string replyData = NetworkProtocol.REP_SCAN_ACK + NetworkProtocol.DELIMITER + machineName;
                        byte[] sendData = Encoding.UTF8.GetBytes(replyData);

                        Socket replySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        replySocket.SendTo(sendData, new IPEndPoint(IPAddress.Parse(serverIP), 8889));
                        replySocket.Close();
                    }
                }
                catch { }
            }
        }

        // ----- XỬ LÝ KẾT NỐI TCP -----
        private void ConnectTCP()
        {
            while (true)
            {
                try
                {
                    if (tcpSocket == null || !tcpSocket.Connected)
                    {
                        tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        // Hàm Connect() tới Server port 9090
                        tcpSocket.Connect(new IPEndPoint(IPAddress.Parse(serverIP), 9090));

                        // Kết nối thành công thì bắt đầu vòng lặp Receive vô tận
                        Thread receiveThread = new Thread(ReceiveCommand);
                        receiveThread.IsBackground = true;
                        receiveThread.Start();
                    }
                }
                catch
                {
                    // Nếu Server chưa mở, dùng Thread.Sleep(5000) để kết nối lại liên tục mà không văng lỗi
                    Thread.Sleep(5000);
                }
            }
        }

        // ----- XỬ LÝ LỆNH TỪ SERVER (NHÁNH 2) -----
        private void ReceiveCommand()
        {
            byte[] buffer = new byte[1024 * 1024]; // Cấp buffer an toàn
            while (true)
            {
                try
                {
                    int size = tcpSocket.Receive(buffer);
                    if (size == 0)
                    {
                        tcpSocket.Close();
                        break; // Mất kết nối
                    }

                    string commandStr = Encoding.UTF8.GetString(buffer, 0, size);
                    // Cắt chuỗi lấy lệnh
                    string[] parts = commandStr.Split(NetworkProtocol.DELIMITER);
                    string cmd = parts[0];

                    // Xử lý chuỗi nhận được bằng switch/case
                    switch (cmd)
                    {
                        case NetworkProtocol.CMD_LOCK:
                            // Dùng Invoke để fix lỗi cross-thread khi gọi giao diện Form
                            this.Invoke(new Action(() => {
                                if (lockerForm == null || lockerForm.IsDisposed)
                                {
                                    lockerForm = new FrmScreenLocker();
                                }
                                lockerForm.Show();
                            }));
                            break;

                        case NetworkProtocol.CMD_UNLOCK:
                            this.Invoke(new Action(() => {
                                if (lockerForm != null && !lockerForm.IsDisposed)
                                {
                                    lockerForm.Hide();
                                }
                            }));
                            break;

                        case NetworkProtocol.CMD_SHUTDOWN:
                            // Gọi hàm Process.Start tắt máy tính
                            Process.Start("shutdown", "-s -t 0");
                            break;

                        //case NetworkProtocol.CMD_MSG:
                        //    // Cắt chuỗi lấy nội dung phía sau dấu |
                        //    if (parts.Length > 1)
                        //    {
                        //        string msgContent = parts[1];
                        //        MessageBox.Show(msgContent, "Thông báo từ Giáo viên", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //    }
                        //    break;
                        case NetworkProtocol.CMD_MSG:
                            if (parts.Length > 1)
                            {
                                string msgContent = parts[1];
                                this.Invoke(new Action(() => {
                                    MessageBox.Show(this, msgContent, "Thông báo...", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }));
                            }
                            break;
                        case NetworkProtocol.CMD_PULL:
                            HandlePullCommand();
                            break;

                        case NetworkProtocol.CMD_CAPTURE:
                            HandleCaptureCommand();
                            break;
                    }
                }
                catch
                {
                    tcpSocket.Close();
                    break;
                }
            }
        }
        private void ToggleTaskManager(bool disable)

        {
            try
            {
                RegistryKey objRegistryKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System");
                if (disable)
                    objRegistryKey.SetValue("DisableTaskMgr", 1);
                else
                    objRegistryKey.DeleteValue("DisableTaskMgr");

                objRegistryKey.Close();

            }
            catch { }


        }
        // ----- XỬ LÝ DỮ LIỆU NẶNG (NHÁNH 3) -----
        private void HandlePullCommand()
        {
            try
            {
                string sourceDir = @"D:\BaiLam";
                string tempDir = @"D:\Temp";
                string zipFilePath = Path.Combine(tempDir, "NopBai.zip");

                if (!Directory.Exists(tempDir))
                {
                    Directory.CreateDirectory(tempDir);
                }

                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }
                // Dùng thư viện ZipFile.CreateFromDirectory() nén thư mục định sẵn
                if (Directory.Exists(sourceDir))
                {
                    ZipFile.CreateFromDirectory(sourceDir, zipFilePath);

                    // Đọc file NopBai.zip thành mảng Byte và Send() lên Server
                    byte[] fileBytes = File.ReadAllBytes(zipFilePath);
                    tcpSocket.Send(fileBytes);
                }
            }
            catch { }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void lblIPAddress_Click(object sender, EventArgs e)
        {

        }

        private void btnStatus_Click(object sender, EventArgs e)
        {

        }
    }
}