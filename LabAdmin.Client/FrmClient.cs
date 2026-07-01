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
        // Cờ để đảm bảo chỉ có 1 thread ReceiveCommand chạy tại một thời điểm
        private readonly object sendLock = new object();   // tranh 2 nguon Send xen ke
        private volatile bool isRunning = true;            // co dung toan bo luong nen
        private volatile bool isReceiving = false;         // dang co 1 ReceiveCommand chay
        // Form khóa màn hình (Khai báo ở đây để dễ dàng gọi lệnh Mở/Khóa)
        private FrmScreenLocker frmLocker;

        // IP của Server. Mặc định là localhost, sẽ được cập nhật tự động khi Server bắn UDP quét mạng
      
        // [1 MÁY]: Để là "127.0.0.1"
        // [2 MÁY]: Để là IP của máy Server (ví dụ: "192.168.1.5")
        private string serverIpAddress = "127.0.0.1";

        public frmClientMain()
        {
            InitializeComponent();
        }

        // =========================================================
        // FORM LOAD
        // =========================================================
        private void frmClientMain_Load(object sender, EventArgs e)
        {
            // Che do test: hien form. Khi nop: Opacity=0, ShowInTaskbar=false
            this.Opacity = 1.0;
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            this.Show();

            // Hien ten may + IP noi bo len card
            try
            {
                lblClientName.Text = "PC: " + Environment.MachineName;
                lblClientIPAddress.Text = "Địa chỉ IP: " + GetLocalIPv4();
            }
            catch { }

            if (notifyIcon1 != null) notifyIcon1.Visible = true;

            // Wire cac nut/o (Designer khong wire san)
            if (btnSubmit != null) btnSubmit.Click += btnSubmit_Click;
            if (btnRefresh != null) btnRefresh.Click += btnRefresh_Click;

            // SetStartup();

            udpThread = new Thread(ListenUDP) { IsBackground = true };
            udpThread.Start();
            tcpThread = new Thread(ConnectTcp) { IsBackground = true };
            tcpThread.Start();

            this.FormClosing += FrmClientMain_FormClosing;

            LogClient("Client đã khởi động. Đang chờ kết nối tới máy chủ...");
        }
        
        private void FrmClientMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            isRunning = false;
            CloseTcpSocket();
        }

        private string GetLocalIPv4()
        {
            try
            {
                foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        return ip.ToString();
            }
            catch { }
            return "127.0.0.1";
        }

        // =========================================================
        // TU KHOI DONG CUNG WINDOWS
        // =========================================================
        private void SetStartup()
        {
            try
            {
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(
                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (rk != null) rk.SetValue("LabAdminClient", Application.ExecutablePath);
                }
            }
            catch (Exception ex) { Debug.WriteLine("SetStartup loi: " + ex.Message); }
        }

        // =========================================================
        // UDP: lang nghe diem danh
        // =========================================================
        private void ListenUDP()
        {
            try
            {
                using (UdpClient udpClient = new UdpClient())
                {
                    udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, NetworkProtocol.UDP_SCAN_PORT));

                    while (isRunning)
                    {
                        try
                        {
                            IPEndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);
                            byte[] data = udpClient.Receive(ref remoteEp);
                            string command = Encoding.UTF8.GetString(data);

                            if (command == NetworkProtocol.CMD_SCAN)
                            {
                                serverIpAddress = remoteEp.Address.ToString();
                                byte[] reply = Encoding.UTF8.GetBytes(
                                    NetworkProtocol.REP_SCAN_ACK + NetworkProtocol.DELIMITER + Environment.MachineName);
                                udpClient.Send(reply, reply.Length,
                                    new IPEndPoint(remoteEp.Address, NetworkProtocol.UDP_REPLY_PORT));
                            }
                        }
                        catch (Exception ex) { Debug.WriteLine("UDP receive loi: " + ex.Message); }
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine("Mo UDP loi: " + ex.Message); }
        }

        // =========================================================
        // TCP: ket noi + duy tri
        // =========================================================
        private void ConnectTcp()
        {
            while (isRunning)
            {
                try
                {
                    if (!isReceiving)
                    {
                        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        s.Connect(new IPEndPoint(IPAddress.Parse(serverIpAddress), NetworkProtocol.TCP_PORT));
                        tcpSocket = s;

                        isReceiving = true;
                        Thread receiveThread = new Thread(ReceiveCommand) { IsBackground = true };
                        receiveThread.Start();

                        // Gui ten may len server ngay khi TCP ket noi thanh cong
                        // Server se cap nhat cot "Ten May" tren grid chinh xac
                        string hello = NetworkProtocol.REP_SCAN_ACK
                                       + NetworkProtocol.DELIMITER
                                       + Environment.MachineName;
                        SendFrame(NetworkProtocol.TYPE_TEXT, System.Text.Encoding.UTF8.GetBytes(hello));

                        LogClient("Đã kết nối tới máy chủ " + serverIpAddress);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Chua ket noi duoc Server: " + ex.Message);
                    CloseTcpSocket();
                    isReceiving = false;
                }
                Thread.Sleep(5000);
            }
        }

        private void CloseTcpSocket()
        {
            try { tcpSocket?.Shutdown(SocketShutdown.Both); } catch { }
            try { tcpSocket?.Close(); } catch { }
            tcpSocket = null;
        }


        // =========================================================
        // NHAN LENH (doc theo khung)
        // =========================================================
        private void ReceiveCommand()
        {
            try
            {
                while (isRunning)
                {
                    if (!NetworkProtocol.ReceiveFrame(tcpSocket, out byte type, out byte[] payload))
                        break;
                    if (type != NetworkProtocol.TYPE_TEXT) continue; // server chi gui lenh text
                    string data = Encoding.UTF8.GetString(payload);
                    string[] parts = data.Split(NetworkProtocol.DELIMITER);
                    if (parts.Length == 0) continue;
                    string cmd = parts[0];
                    Debug.WriteLine("Lenh: " + cmd);
                    switch (cmd)
                    {
                        case NetworkProtocol.CMD_MSG:
                            string msg = parts.Length >= 2
                                ? string.Join(NetworkProtocol.DELIMITER.ToString(), parts, 1, parts.Length - 1)
                                : string.Empty;
                            LogClient("Thông báo từ GV: " + msg);
                            this.Invoke(new Action(() => MessageBox.Show(msg, "Thông báo từ Giảng Viên",
                                MessageBoxButtons.OK, MessageBoxIcon.Information)));
                            break;
                        case NetworkProtocol.CMD_LOCK:
                            this.Invoke(new Action(() =>
                            {
                                if (frmLocker == null || frmLocker.IsDisposed)
                                {
                                    frmLocker = new FrmScreenLocker();
                                    // Truyen ten may + IP de hien len man hinh khoa
                                    frmLocker.ClientInfo = $"{Environment.MachineName}  ·  {GetLocalIPv4()}";
                                }
                                frmLocker.Show();
                                frmLocker.BringToFront();
                            }));
                            LogClient("Máy đã bị KHÓA bởi giảng viên.");
                            break;

                        case NetworkProtocol.CMD_UNLOCK:
                            this.Invoke(new Action(() =>
                            {
                                if (frmLocker != null && !frmLocker.IsDisposed) frmLocker.ForceUnlock();
                            }));
                            LogClient("Máy đã được MỞ KHÓA.");
                            break;

                        case NetworkProtocol.CMD_CAPTURE:
                            HandleCaptureCommand();
                            break;

                        case NetworkProtocol.CMD_PULL:
                            HandlePullCommand();
                            break;

                        case NetworkProtocol.CMD_SHUTDOWN:
                            try { Process.Start("shutdown", "/s /t 0"); }
                            catch (Exception ex) { Debug.WriteLine("shutdown loi: " + ex.Message); }
                            break;

                        case NetworkProtocol.CMD_RESTART:
                            try { Process.Start("shutdown", "/r /t 0"); }
                            catch (Exception ex) { Debug.WriteLine("restart loi: " + ex.Message); }
                            break;
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine("Loi Receive: " + ex.Message); }
            finally
            {
                CloseTcpSocket();
                isReceiving = false;
                LogClient("Mất kết nối tới máy chủ. Sẽ tự kết nối lại...");
            }
        }

        // =========================================================
        // THU BAI (nen zip thu muc lam bai + gui)
        // =========================================================
        private void HandlePullCommand()
        {
            try
            {
                LogClient("DEBUG: Đang xử lý lệnh thu bài...");
                string sourceDir = @"D:\BaiLam";
                if (!Directory.Exists(sourceDir))
                {
                    LogClient("LỖI: Không tìm thấy thư mục D:\\BaiLam trên máy trạm!");
                    return;
                }

                string tempDir = Path.Combine(Path.GetTempPath(), "LabAdminTemp");
                if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);
                string zipFilePath = Path.Combine(tempDir, "NopBai.zip");
                if (File.Exists(zipFilePath)) File.Delete(zipFilePath);

                ZipFile.CreateFromDirectory(sourceDir, zipFilePath);
                SendFrame(NetworkProtocol.TYPE_ZIP, File.ReadAllBytes(zipFilePath));

                LogClient("Đã nộp bài thành công.");
            }
            catch (Exception ex) { LogClient("Lỗi thu bài: " + ex.Message); }
        }

        // =========================================================
        // CHUP MAN HINH
        // =========================================================
        private void HandleCaptureCommand()
        {
            try
            {
                Rectangle bounds = Screen.PrimaryScreen.Bounds;
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                        g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Jpeg);
                        SendFrame(NetworkProtocol.TYPE_IMAGE, ms.ToArray());
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine("HandleCapture loi: " + ex.Message); }
        }


        // Gui 1 khung [type][length][payload], khoa de khong xen ke
        private void SendFrame(byte type, byte[] data)
        {
            byte[] frame = NetworkProtocol.BuildFrame(type, data);
            lock (sendLock)
            {
                Socket s = tcpSocket;
                if (s != null && s.Connected) s.Send(frame);
            }
        }


        // =========================================================
        // NUT: NOP BAI THU CONG (chon file/thu muc -> zip -> gui)
        // =========================================================
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (tcpSocket == null || !tcpSocket.Connected)
            {
                MessageBox.Show("Chưa kết nối tới máy chủ, không thể nộp bài.", "Chú ý",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn file bài làm để nộp";
                ofd.Multiselect = true;
                if (ofd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    string tempDir = Path.Combine(Path.GetTempPath(), "LabAdminSubmit");
                    if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                    Directory.CreateDirectory(tempDir);

                    foreach (string f in ofd.FileNames)
                        File.Copy(f, Path.Combine(tempDir, Path.GetFileName(f)), true);

                    string zipPath = Path.Combine(Path.GetTempPath(), "NopBai_Manual.zip");
                    if (File.Exists(zipPath)) File.Delete(zipPath);
                    ZipFile.CreateFromDirectory(tempDir, zipPath);

                    SendFrame(NetworkProtocol.TYPE_ZIP, File.ReadAllBytes(zipPath));
                    LogClient($"Đã nộp {ofd.FileNames.Length} file lên máy chủ.");
                    MessageBox.Show("Đã nộp bài thành công!", "Thành công",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    LogClient("Lỗi nộp bài: " + ex.Message);
                    MessageBox.Show("Lỗi nộp bài: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



        // Nut refresh: hien lai ten may/IP + trang thai ket noi
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            lblClientName.Text = "PC: " + Environment.MachineName;
            lblClientIPAddress.Text = "Địa chỉ IP: " + GetLocalIPv4();
            bool connected = tcpSocket != null && tcpSocket.Connected;
            LogClient(connected ? "Trạng thái: Đã kết nối máy chủ." : "Trạng thái: Chưa kết nối.");
        }

        // Checkbox: gio tay ho tro -> bao len server
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            bool on = checkBox1.Checked;

            // Kiem tra ket noi TCP truoc khi gui
            if (tcpSocket == null || !tcpSocket.Connected)
            {
                LogClient("⚠ Chưa kết nối tới máy chủ, không thể gửi tín hiệu giơ tay.");
                // Dat lai checkbox ve trang thai cu de tranh nham
                checkBox1.CheckedChanged -= checkBox1_CheckedChanged;
                checkBox1.Checked = !on;
                checkBox1.CheckedChanged += checkBox1_CheckedChanged;
                return;
            }

            try
            {
                string payload = NetworkProtocol.REP_HELP + NetworkProtocol.DELIMITER
                                 + Environment.MachineName + NetworkProtocol.DELIMITER + (on ? "1" : "0");
                SendFrame(NetworkProtocol.TYPE_TEXT, Encoding.UTF8.GetBytes(payload));
                LogClient(on ? "✋ Đã giơ tay xin hỗ trợ." : "✔ Đã hạ tay.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Help loi: " + ex.Message);
                LogClient("⚠ Lỗi gửi tín hiệu giơ tay: " + ex.Message);
            }
        }


        // Ghi log ra rtbLogsClient (an toan cross-thread)
        private void LogClient(string message)
        {
            if (rtbLogsClient == null) return;
            if (rtbLogsClient.InvokeRequired)
            {
                rtbLogsClient.Invoke(new Action<string>(LogClient), message);
                return;
            }
            rtbLogsClient.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            rtbLogsClient.ScrollToCaret();
        }
        private void panel1_Paint(object sender, PaintEventArgs e) { }

        private void panel1_Paint_1(object sender, PaintEventArgs e)
        {

        }
    }
}
