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
        // =========================================================
        // 1. KHAI BÁO BIẾN TOÀN CỤC
        // =========================================================

        // Tách riêng 2 luồng (Thread) cho UDP và TCP để không làm treo giao diện Form
        private Thread udpThread;
        private Thread tcpThread;

        // Socket dùng để duy trì kết nối TCP liên tục với Server
        private Socket tcpSocket;

        // Form khóa màn hình (Khai báo ở đây để dễ dàng gọi lệnh Mở/Khóa)
        private FrmScreenLocker frmLocker;

        // IP của Server. Mặc định là localhost, sẽ được cập nhật tự động khi Server bắn UDP quét mạng
        private string serverIpAddress = "127.0.0.1";

        public frmClientMain()
        {
            InitializeComponent();
        }

        // =========================================================
        // 2. SỰ KIỆN FORM LOAD (KHỞI ĐỘNG CLIENT)
        // =========================================================
        private void frmClientMain_Load(object sender, EventArgs e)
        {
            // --- CẤU HÌNH HIỂN THỊ FORM ---
            // Đã chỉnh lại để Form hiện lên cho nhóm trưởng dễ test. 
            // Khi nào nộp đồ án, chỉ cần đổi Opacity = 0, ShowInTaskbar = false là tàng hình trở lại.
            this.Opacity = 1.0;
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            this.Show();

            // Bật biểu tượng nhỏ dưới góc phải màn hình (System Tray)
            if (notifyIcon1 != null)
            {
                notifyIcon1.Visible = true;
            }

            // Ghi Registry để phần mềm tự khởi động cùng Windows
            // SetStartup();

            // Khởi chạy luồng UDP (Lắng nghe lệnh quét mạng từ Server)
            udpThread = new Thread(ListenUdp);
            udpThread.IsBackground = true; // IsBackground = true giúp Thread tự hủy khi tắt ứng dụng
            udpThread.Start();

            // Khởi chạy luồng TCP (Kết nối chính thức để nhận lệnh)
            tcpThread = new Thread(ConnectTcp);
            tcpThread.IsBackground = true;
            tcpThread.Start();
        }

        // =========================================================
        // 3. HÀM TỰ KHỞI ĐỘNG CÙNG WINDOWS
        // =========================================================
        private void SetStartup()
        {
            try
            {
                // Can thiệp vào Registry của Windows để add file .exe hiện tại vào thư mục Run
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                rk.SetValue("LabAdminClient", Application.ExecutablePath);
            }
            catch
            {
                // Bỏ qua lỗi nếu máy tính sinh viên không cấp quyền ghi Registry
            }
        }

        // =========================================================
        // 4. LUỒNG UDP: LẮNG NGHE ĐIỂM DANH (CỔNG 8888)
        // =========================================================
        private void ListenUdp()
        {
            // Khởi tạo Socket UDP
            Socket sckUdp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sckUdp.Bind(new IPEndPoint(IPAddress.Any, 8888)); // Client mở cửa port 8888 chờ Server

            byte[] buffer = new byte[1024];
            EndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                try
                {
                    // Hứng dữ liệu từ Server bắn tới
                    int size = sckUdp.ReceiveFrom(buffer, ref remoteEp);
                    string data = Encoding.UTF8.GetString(buffer, 0, size);

                    // Nếu gói tin nhận được đúng là mã lệnh Quét Mạng
                    if (data == NetworkProtocol.CMD_SCAN)
                    {
                        // TRỌNG TÂM: Trích xuất IP của Server từ địa chỉ người gửi
                        serverIpAddress = ((IPEndPoint)remoteEp).Address.ToString();

                        // Lấy tên máy tính hiện tại của Client
                        string machineName = Environment.MachineName;

                        // Đóng gói dữ liệu phản hồi: Tên mã | Tên máy
                        string replyData = NetworkProtocol.REP_SCAN_ACK + NetworkProtocol.DELIMITER + machineName;
                        byte[] sendData = Encoding.UTF8.GetBytes(replyData);

                        // Mở một Socket UDP tạm để bắn trả lại IP Server ở cổng 8090
                        Socket replySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        replySocket.SendTo(sendData, new IPEndPoint(IPAddress.Parse(serverIpAddress), 8090));
                        replySocket.Close();
                    }
                }
                catch { } // Bỏ qua lỗi mạng lặt vặt để vòng lặp không bị chết
            }
        }

        // =========================================================
        // 5. LUỒNG TCP: KẾT NỐI VÀ DUY TRÌ BỀN VỮNG (CỔNG 9090)
        // =========================================================
        private void ConnectTcp()
        {
            while (true)
            {
                try
                {
                    // Nếu chưa kết nối hoặc kết nối bị rớt
                    if (tcpSocket == null || !tcpSocket.Connected)
                    {
                        tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        tcpSocket.Connect(new IPEndPoint(IPAddress.Parse(serverIpAddress), 9090));

                        // Nếu Connect thành công, đẩy việc nhận lệnh sang một Thread mới
                        Thread receiveThread = new Thread(ReceiveCommand);
                        receiveThread.IsBackground = true;
                        receiveThread.Start();
                    }
                }
                catch
                {
                    // Server chưa mở máy hoặc rớt mạng -> Ngủ 5 giây rồi thử kết nối lại (Auto Reconnect)
                    Thread.Sleep(5000);
                }
            }
        }

        // =========================================================
        // 6. XỬ LÝ LỆNH TỪ SERVER (TÌM PHÂN MẢNH LỆNH)
        // =========================================================
        private void ReceiveCommand()
        {
            // Cấp phát bộ đệm 1MB, đủ an toàn cho việc nhận chuỗi lệnh
            byte[] buffer = new byte[1024 * 1024];

            while (true)
            {
                try
                {
                    // Chặn tại đây cho đến khi có dữ liệu đổ về
                    int size = tcpSocket.Receive(buffer);
                    if (size == 0) // size = 0 nghĩa là Server đã ngắt kết nối
                    {
                        tcpSocket.Close();
                        break;
                    }

                    // Giải mã gói tin thành chuỗi
                    string commandStr = Encoding.UTF8.GetString(buffer, 0, size);

                    // Cắt chuỗi dựa vào dấu phân cách (Delimiter: '|')
                    string[] parts = commandStr.Split(NetworkProtocol.DELIMITER);
                    string cmd = parts[0]; // Phần tử đầu tiên luôn là Mã lệnh

                    // Điều hướng xử lý dựa trên Mã lệnh
                    switch (cmd)
                    {
                        case NetworkProtocol.CMD_LOCK:
                            // Dùng Invoke để đẩy việc cập nhật giao diện (UI) về luồng chính, tránh lỗi Cross-thread
                            this.Invoke(new Action(() => {
                                if (frmLocker == null || frmLocker.IsDisposed)
                                    frmLocker = new FrmScreenLocker();
                                frmLocker.Show();
                            }));
                            break;

                        case NetworkProtocol.CMD_UNLOCK:
                            this.Invoke(new Action(() => {
                                if (frmLocker != null && !frmLocker.IsDisposed)
                                    frmLocker.Hide();
                            }));
                            break;

                        case NetworkProtocol.CMD_SHUTDOWN:
                            // Gọi CMD của hệ thống để ép tắt máy ngay lập tức (thời gian = 0)
                            Process.Start("shutdown", "-s -t 0");
                            break;

                        case NetworkProtocol.CMD_MSG:
                            // Nếu gói tin có chứa nội dung thông báo đi kèm
                            if (parts.Length > 1)
                            {
                                string msgContent = parts[1];
                                this.Invoke(new Action(() => {
                                    // Lấy giờ hiện tại để khung thông báo trông chuyên nghiệp hơn
                                    string time = DateTime.Now.ToString("HH:mm:ss");

                                    // Đổ text vào khung Thông báo từ máy chủ (rtbLogsClient)
                                    rtbLogsClient.AppendText($"[{time}] Giáo viên: {msgContent}\n");

                                    // Tự động cuộn xuống dòng mới nhất
                                    rtbLogsClient.ScrollToCaret();
                                }));
                            }
                            break;

                        case NetworkProtocol.CMD_PULL:
                            // Chạy hàm Thu bài (Nén Zip)
                            HandlePullCommand();
                            break;

                        case NetworkProtocol.CMD_CAPTURE:
                            // Chạy hàm Chụp màn hình
                            HandleCaptureCommand();
                            break;
                    }
                }
                catch
                {
                    // Lỗi mạng đứt ngang -> Đóng socket, thoát vòng lặp để Thread ConnectTCP bên trên tự động kết nối lại
                    tcpSocket.Close();
                    break;
                }
            }
        }

        // =========================================================
        // 7. XỬ LÝ DỮ LIỆU NẶNG: NÉN ZIP VÀ GỬI BÀI
        // =========================================================
        private void HandlePullCommand()
        {
            try
            {
                // Thư mục sinh viên làm bài (Cần đảm bảo thư mục này luôn tồn tại trên máy trạm)
                string sourceDir = @"D:\BaiLam";
                // Thư mục tạm để chứa file zip trước khi gửi
                string tempDir = @"D:\Temp";
                string zipFilePath = Path.Combine(tempDir, "NopBai.zip");

                // Tạo thư mục tạm nếu chưa có
                if (!Directory.Exists(tempDir))
                    Directory.CreateDirectory(tempDir);

                // Xóa file bài cũ (nếu có) để không bị đè dữ liệu
                if (File.Exists(zipFilePath))
                    File.Delete(zipFilePath);

                if (Directory.Exists(sourceDir))
                {
                    // Nén toàn bộ thư mục D:\BaiLam thành 1 file .zip duy nhất
                    ZipFile.CreateFromDirectory(sourceDir, zipFilePath);

                    // Đọc file .zip thành mảng Byte và bắn thẳng lên Server qua TCP
                    byte[] fileBytes = File.ReadAllBytes(zipFilePath);
                    tcpSocket.Send(fileBytes);
                }
            }
            catch { }
        }

        // =========================================================
        // 8. XỬ LÝ DỮ LIỆU NẶNG: CHỤP ẢNH MÀN HÌNH (SPY)
        // =========================================================
        private void HandleCaptureCommand()
        {
            try
            {
                // Lấy kích thước màn hình thực tế của Client
                Rectangle bounds = Screen.PrimaryScreen.Bounds;

                // Khởi tạo một bức ảnh trống (Bitmap) với kích thước vừa lấy
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    // Dùng Graphics để "vẽ" lại những gì đang hiển thị trên màn hình vào Bitmap
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                    }

                    // Chuyển ảnh thành mảng Byte chuẩn JPEG để giảm dung lượng mạng
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Jpeg);
                        byte[] imageBytes = ms.ToArray();

                        // Bắn mảng Byte ảnh lên Server
                        tcpSocket.Send(imageBytes);
                    }
                }
            }
            catch { }
        }
    }
}