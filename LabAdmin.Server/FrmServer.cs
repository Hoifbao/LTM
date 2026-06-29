using LabAdmin.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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

        // IP -> Socket. Moi truy cap deu trong lock(clientList).
        private readonly Dictionary<string, Socket> clientList = new Dictionary<string, Socket>();
        // IP -> dang khoa? (de toggle nut Khoa/Mo)
        private readonly HashSet<string> lockedIPs = new HashSet<string>();
        // Moi IP tai dung 1 viewer
        private readonly Dictionary<string, FrmImageViewer> viewers = new Dictionary<string, FrmImageViewer>();

        private Thread udpThread;
        private Thread tcpThread;
        private TcpListener tcpListener;
        private Socket udpListenSocket;

        private volatile bool isRunning = true;
        private string SaveFolderPath = "";
        public frmServerMain()
        {
            InitializeComponent();
            // Khởi chạy thread lắng nghe phản hồi UDP
            udpThread = new Thread(ListenUDP) { IsBackground = true };
            udpThread.Start();
            tcpThread = new Thread(ListenTCP) { IsBackground = true };
            tcpThread.Start();

        }
        // =========================================================
        // DONG SERVER
        // =========================================================
        private void frmServerMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Hệ thống đang chạy. Bạn có chắc chắn muốn tắt Server và ngắt kết nối toàn bộ máy trạm?",
                "Cảnh báo an toàn", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.No) { e.Cancel = true; return; }

            isRunning = false;
            try { tcpListener?.Stop(); } catch { }
            try { udpListenSocket?.Close(); } catch { }
            lock (clientList)
            {
                foreach (var s in clientList.Values)
                {
                    try { s.Shutdown(SocketShutdown.Both); } catch { }
                    try { s.Close(); } catch { }
                }
                clientList.Clear();
            }
        }

        // =========================================================
        // GRID
        // =========================================================
        public void AddOrUpdateClient(string ip, string machineName, string status)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string, string, string>(AddOrUpdateClient), ip, machineName, status);
                return;
            }
            foreach (DataGridViewRow row in dgvClients.Rows)
            {
                if (row.IsNewRow) continue;
                if (row.Cells["IP"].Value?.ToString() == ip)
                {
                    row.Cells["colStatus"].Value = status;
                    if (!string.IsNullOrEmpty(machineName))
                        row.Cells["Column2"].Value = machineName;
                    UpdateTotalClients();
                    return;
                }
            }
            int idx = dgvClients.Rows.Add(ip, machineName, status);
            dgvClients.Rows[idx].Selected = true;
            UpdateTotalClients();
        }

        private void SetClientStatus(string ip, string status) => AddOrUpdateClient(ip, null, status);

        // =========================================================
        // QUET MANG
        // =========================================================
        private void btnScan_Click(object sender, EventArgs e)
        {
            // KHONG xoa grid o day - tranh mat dong bo voi clientList dang ket noi TCP
            // Chi can phat broadcast, cac may chua co TCP se tu xuat hien qua UDP reply
            try
            {
                using (Socket sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    sck.EnableBroadcast = true;
                    byte[] data = Encoding.UTF8.GetBytes(NetworkProtocol.CMD_SCAN);
                    sck.SendTo(data, new IPEndPoint(IPAddress.Broadcast, NetworkProtocol.UDP_SCAN_PORT));
                }
                LogMessage("Đã gửi lệnh Quét mạng (Broadcast). Đang chờ phản hồi...");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi quét mạng: " + ex.Message); }
        }


        // =========================================================
        // UDP: hung phan hoi diem danh
        // =========================================================
        private void ListenUDP()
        {
            try
            {
                udpListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                udpListenSocket.Bind(new IPEndPoint(IPAddress.Any, NetworkProtocol.UDP_REPLY_PORT));
                byte[] buffer = new byte[1024];
                EndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);

                while (isRunning)
                {
                    try
                    {
                        int size = udpListenSocket.ReceiveFrom(buffer, ref remoteEp);
                        string data = Encoding.UTF8.GetString(buffer, 0, size);
                        string[] parts = data.Split(NetworkProtocol.DELIMITER);
                        if (parts.Length >= 2 && parts[0] == NetworkProtocol.REP_SCAN_ACK)
                        {
                            string ip = ((IPEndPoint)remoteEp).Address.ToString();
                            AddOrUpdateClient(ip, parts[1], NetworkProtocol.STATUS_ONLINE);
                        }
                    }
                    catch (Exception ex) { if (isRunning) System.Diagnostics.Debug.WriteLine("UDP: " + ex.Message); }
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Mo UDP: " + ex.Message); }
        }


        // =========================================================
        // TCP: accept ket noi
        // =========================================================
        private void ListenTCP()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, NetworkProtocol.TCP_PORT);
                tcpListener.Start();
                while (isRunning)
                {
                    Socket client = tcpListener.AcceptSocket();
                    // Bat KeepAlive: phat hien mat ket noi sau ~20s thay vi bi dong im lang
                    client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    string ip = ((IPEndPoint)client.RemoteEndPoint).Address.ToString();

                    // Bo qua ket noi tu loopback khi dang debug tren cung may
                    if (ip == "127.0.0.1" || ip == "::1")
                    {
                        LogMessage($"⚠ Bỏ qua kết nối loopback ({ip}) - chạy server và client cùng máy.", Color.Gray);
                        try { client.Close(); } catch { }
                        continue;
                    }
                    lock (clientList)
                    {
                        if (clientList.TryGetValue(ip, out Socket old)) { try { old.Close(); } catch { } }
                        clientList[ip] = client;
                    }
                    // Ten may se duoc cap nhat khi nhan REP_SCAN_ACK tu client
                    AddOrUpdateClient(ip, ip, NetworkProtocol.STATUS_ONLINE);
                    Task.Run(() => ReceiveData(client, ip));
                }
            }
            catch (Exception ex) { if (isRunning) System.Diagnostics.Debug.WriteLine("TCP listener: " + ex.Message); }
        }

        // =========================================================
        // NHAN DU LIEU theo khung [type][length][payload]
        // =========================================================
        private void ReceiveData(Socket client, string ip)
        {
            try
            {
                while (isRunning)
                {
                    if (!NetworkProtocol.ReceiveFrame(client, out byte type, out byte[] payload)) break;

                    switch (type)
                    {
                        case NetworkProtocol.TYPE_ZIP: SaveZip(ip, payload); break;
                        case NetworkProtocol.TYPE_IMAGE: SaveAndShowImage(ip, payload); break;
                        case NetworkProtocol.TYPE_TEXT: HandleClientText(ip, payload); break;
                    }
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Nhan tu {ip}: {ex.Message}"); }
            finally
            {
                lock (clientList)
                {
                    if (clientList.TryGetValue(ip, out Socket s) && ReferenceEquals(s, client))
                        clientList.Remove(ip);
                }
                lock (lockedIPs) { lockedIPs.Remove(ip); }
                try { client.Close(); } catch { }
                SetClientStatus(ip, NetworkProtocol.STATUS_OFFLINE);
                LogMessage($"Máy {ip} đã ngắt kết nối.", Color.Gray);
            }
        }

        private void HandleClientText(string ip, byte[] payload)
        {
            string text = Encoding.UTF8.GetString(payload);
            string[] parts = text.Split(NetworkProtocol.DELIMITER);
            if (parts.Length == 0) return;

            if (parts[0] == NetworkProtocol.REP_SCAN_ACK && parts.Length >= 2)
            {
                // Client gui ten may ngay khi TCP ket noi -> cap nhat grid
                string machineName = parts[1];
                AddOrUpdateClient(ip, machineName, NetworkProtocol.STATUS_ONLINE);
                LogMessage($"🖥 {machineName} ({ip}) đã kết nối.", Color.Cyan);
            }
            else if (parts[0] == NetworkProtocol.REP_HELP && parts.Length >= 3)
            {
                string name = parts[1];
                bool on = parts[2] == "1";
                // Cap nhat bieu tuong tren grid de giang vien biet may nao giơ tay
                UpdateHelpStatus(ip, on);
                LogMessage(on ? $"✋ {name} ({ip}) GIƠ TAY xin hỗ trợ!" : $"✔ {name} ({ip}) đã hạ tay.",
                           on ? Color.Yellow : Color.Gray);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[{ip}] {text}");
            }
        }

        // Cap nhat trang thai gió tay tren cot "Trang Thai" cua grid
        private void UpdateHelpStatus(string ip, bool isRaisingHand)
        {
            if (this.InvokeRequired) { this.Invoke(new Action<string, bool>(UpdateHelpStatus), ip, isRaisingHand); return; }

            foreach (DataGridViewRow row in dgvClients.Rows)
            {
                if (row.IsNewRow) continue;
                if (row.Cells["IP"].Value?.ToString() != ip) continue;

                string currentStatus = row.Cells["colStatus"].Value?.ToString() ?? "";
                if (isRaisingHand)
                {
                    // Them bieu tuong giơ tay vao trang thai neu chua co
                    if (!currentStatus.Contains("✋"))
                        row.Cells["colStatus"].Value = "✋ " + currentStatus;
                }
                else
                {
                    // Xoa bieu tuong giơ tay
                    row.Cells["colStatus"].Value = currentStatus.Replace("✋ ", "").Replace("✋", "").Trim();
                }
                break;
            }
        }

        private void SaveZip(string ip, byte[] data)
        {
            try
            {
                string baseFolder = string.IsNullOrEmpty(SaveFolderPath)
                    ? Path.Combine(Application.StartupPath, "ThuBai") : SaveFolderPath;
                string folderPath = Path.Combine(baseFolder, ip);
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                string filePath = Path.Combine(folderPath, "BaiLam_" + DateTime.Now.ToString("HHmmss") + ".zip");
                File.WriteAllBytes(filePath, data);
                LogMessage($"Đã thu bài từ {ip} ({data.Length / 1024} KB).", Color.Lime);
            }
            catch (Exception ex) { LogMessage($"Lỗi lưu bài từ {ip}: {ex.Message}", Color.Red); }
        }

        private void SaveAndShowImage(string ip, byte[] data)
        {
            try
            {
                string folderPath = Path.Combine(Application.StartupPath, "AnhChup", ip);
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                string imgPath = Path.Combine(folderPath, "Screen_" + DateTime.Now.ToString("HHmmss") + ".jpg");
                File.WriteAllBytes(imgPath, data);

                this.Invoke(new Action(() =>
                {
                    if (!viewers.TryGetValue(ip, out FrmImageViewer viewer) || viewer.IsDisposed)
                    {
                        viewer = new FrmImageViewer(imgPath);
                        viewer.FormClosed += (s, e) => viewers.Remove(ip);
                        viewers[ip] = viewer;
                    }
                    else viewer.LoadNewImage(imgPath);

                    // Lay ten may thuc tu grid neu co, khong thi dung IP
                    string machineName = ip;
                    foreach (DataGridViewRow row in dgvClients.Rows)
                    {
                        if (!row.IsNewRow && row.Cells["IP"].Value?.ToString() == ip)
                        {
                            string mn = row.Cells["Column2"].Value?.ToString();
                            if (!string.IsNullOrEmpty(mn)) machineName = mn;
                            break;
                        }
                    }
                    viewer.UpdateClientInfo(machineName, ip);
                    viewer.Show();
                    viewer.BringToFront();
                }));
                LogMessage($"Đã nhận ảnh màn hình từ {ip}.");
            }
            catch (Exception ex) { LogMessage($"Lỗi xử lý ảnh từ {ip}: {ex.Message}", Color.Red); }
        }




        // =========================================================
        // GUI LENH (dong khung text)
        // =========================================================
        private bool SendCommand(string targetIP, string commandText, out string error)
        {
            error = null;
            Socket client;
            lock (clientList)
            {
                if (string.IsNullOrEmpty(targetIP) || !clientList.TryGetValue(targetIP, out client))
                { error = "Máy không còn trong danh sách kết nối."; return false; }
            }
            if (client == null || !client.Connected) { error = "Máy đã ngắt kết nối."; return false; }
            try { client.Send(NetworkProtocol.BuildTextFrame(commandText)); return true; }
            catch (Exception ex) { error = ex.Message; return false; }
        }

        private string GetSelectedIP()
        {
            if (dgvClients.SelectedRows.Count > 0)
                return dgvClients.SelectedRows[0].Cells["IP"].Value?.ToString();
            return null;
        }

        // Nut Khoa = toggle Khoa/Mo
        private void btnLock_Click(object sender, EventArgs e)
        {
            string targetIP = GetSelectedIP();
            if (string.IsNullOrEmpty(targetIP)) { MessageBox.Show("Vui lòng chọn một máy!"); return; }

            bool currentlyLocked;
            lock (lockedIPs) { currentlyLocked = lockedIPs.Contains(targetIP); }

            if (!currentlyLocked)
            {
                if (SendCommand(targetIP, NetworkProtocol.CMD_LOCK, out string err))
                {
                    lock (lockedIPs) { lockedIPs.Add(targetIP); }
                    SetClientStatus(targetIP, NetworkProtocol.STATUS_LOCKED);
                    LogMessage($"Đã gửi lệnh KHÓA tới {targetIP}.", Color.Orange);
                    UpdateLockButtonText(targetIP); // doi nut thanh "Mo khoa"
                }
                else LogMessage($"Lỗi gửi lệnh tới {targetIP}: {err}", Color.Red);
            }
            else
            {
                if (SendCommand(targetIP, NetworkProtocol.CMD_UNLOCK, out string err))
                {
                    lock (lockedIPs) { lockedIPs.Remove(targetIP); }
                    SetClientStatus(targetIP, NetworkProtocol.STATUS_ONLINE);
                    LogMessage($"Đã gửi lệnh MỞ KHÓA tới {targetIP}.", Color.Lime);
                    UpdateLockButtonText(targetIP); // doi nut ve "Khoa man hinh"
                }
                else LogMessage($"Lỗi gửi lệnh tới {targetIP}: {err}", Color.Red);
            }
        }

        // Cap nhat text + mau nut Khoa/Mo dua vao trang thai may dang chon
        private void UpdateLockButtonText(string ip = null)
        {
            if (ip == null) ip = GetSelectedIP();
            if (ip == null)
            {
                // Khong chon may nao -> nut ve mac dinh
                btnLock.Text = "Khóa màn hình";
                btnLock.BackColor = Color.FromArgb(200, 50, 70);
                return;
            }
            bool isLocked;
            lock (lockedIPs) { isLocked = lockedIPs.Contains(ip); }
            if (isLocked)
            {
                btnLock.Text = "Mở khóa";
                btnLock.BackColor = Color.FromArgb(255, 140, 0); // cam - dang khoa
            }
            else
            {
                btnLock.Text = "Khóa màn hình";
                btnLock.BackColor = Color.FromArgb(200, 50, 70); // do - binh thuong
            }
        }

        private void btnMsg_Click(object sender, EventArgs e)
        {
            string targetIP = GetSelectedIP();
            if (string.IsNullOrWhiteSpace(txtMessage.Text))
            { MessageBox.Show("Vui lòng nhập nội dung cần thông báo!", "Chú ý"); return; }
            if (string.IsNullOrEmpty(targetIP))
            { MessageBox.Show("Vui lòng chọn một máy đã kết nối!", "Chú ý"); return; }

            string payload = NetworkProtocol.CMD_MSG + NetworkProtocol.DELIMITER + txtMessage.Text;
            if (SendCommand(targetIP, payload, out string err))
            {
                LogMessage($"Đã gửi thông báo tới {targetIP}: {txtMessage.Text}");
                txtMessage.Clear();
            }
            else
            {
                LogMessage($"Lỗi gửi thông báo tới {targetIP}: {err}", Color.Red);
                MessageBox.Show("Lỗi khi gửi thông báo: " + err, "Lỗi");
            }
        }

        private void btnPull_Click(object sender, EventArgs e)
        {
            string targetIP = GetSelectedIP();
            if (string.IsNullOrEmpty(targetIP)) { MessageBox.Show("Vui lòng chọn một máy đã kết nối!"); return; }

            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Chọn thư mục để lưu bài thi của sinh viên:";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    SaveFolderPath = fbd.SelectedPath;
                    if (SendCommand(targetIP, NetworkProtocol.CMD_PULL, out string err))
                        LogMessage($"Lệnh THU BÀI đã phát tới {targetIP}. Lưu tại: {SaveFolderPath}");
                    else LogMessage($"Lỗi gửi lệnh thu bài tới {targetIP}: {err}", Color.Red);
                }
            }
        }

        private void btnCapture_Click(object sender, EventArgs e)
        {
            string targetIP = GetSelectedIP();
            if (string.IsNullOrEmpty(targetIP)) { MessageBox.Show("Vui lòng chọn một máy đang kết nối!"); return; }
            if (SendCommand(targetIP, NetworkProtocol.CMD_CAPTURE, out string err))
                LogMessage("Đã gửi lệnh chụp màn hình đến: " + targetIP);
            else LogMessage($"Lỗi gửi lệnh chụp tới {targetIP}: {err}", Color.Red);
        }


        // =========================================================
        // KEO FORM + UI
        // =========================================================
        private void pnlTopBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { isDragging = true; lastCursor = Cursor.Position; lastForm = this.Location; }
        }
        private void pnlTopBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
                this.Location = new Point(lastForm.X + (Cursor.Position.X - lastCursor.X),
                                          lastForm.Y + (Cursor.Position.Y - lastCursor.Y));
        }
        private void pnlTopBar_MouseUp(object sender, MouseEventArgs e) => isDragging = false;
        private void button1_Click(object sender, EventArgs e) => this.Close();

        private void UpdateTotalClients()
        {
            int count = 0;
            foreach (DataGridViewRow row in dgvClients.Rows)
            {
                if (row.IsNewRow) continue;
                string st = row.Cells["colStatus"].Value?.ToString();
                if (st == NetworkProtocol.STATUS_ONLINE || st == NetworkProtocol.STATUS_LOCKED) count++;
            }
            label7.Text = "Đang hoạt động: " + count;
        }

        private void dgvClients_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == dgvClients.Columns["colStatus"].Index && e.RowIndex >= 0)
            {
                e.PaintBackground(e.CellBounds, true);
                string status = e.Value?.ToString();
                if (string.IsNullOrEmpty(status)) { e.Handled = true; return; }

                bool active = status == NetworkProtocol.STATUS_ONLINE;
                bool locked = status == NetworkProtocol.STATUS_LOCKED;
                Color backColor, foreColor;
                if (active) { backColor = Color.FromArgb(220, 245, 225); foreColor = Color.FromArgb(34, 160, 80); }
                else if (locked) { backColor = Color.FromArgb(255, 244, 214); foreColor = Color.FromArgb(180, 120, 20); }
                else { backColor = Color.FromArgb(250, 230, 230); foreColor = Color.FromArgb(180, 50, 50); }

                Rectangle rect = new Rectangle(e.CellBounds.X + 15, e.CellBounds.Y + 8,
                    e.CellBounds.Width - 30, e.CellBounds.Height - 16);
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int r = 10;
                    path.AddArc(rect.X, rect.Y, r, r, 180, 90);
                    path.AddArc(rect.Right - r, rect.Y, r, r, 270, 90);
                    path.AddArc(rect.Right - r, rect.Bottom - r, r, r, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - r, r, r, 90, 90);
                    path.CloseFigure();
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (var b = new SolidBrush(backColor)) e.Graphics.FillPath(b, path);
                    using (var f = new Font("Segoe UI", 9, FontStyle.Bold))
                    using (var fb = new SolidBrush(foreColor))
                        e.Graphics.DrawString(status, f, fb, rect,
                            new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }
                e.Handled = true;
            }
        }

        private void FrmServer_Load(object sender, EventArgs e)
        {
            LogMessage("Hệ thống: Server Lab Admin đã khởi động.");
            LogMessage("Mạng: Đang lắng nghe kết nối từ sinh viên...");
        }

        // LOG
        private void LogMessage(string message) => LogMessage(message, Color.Lime);
        private void LogMessage(string message, Color color)
        {
            if (this.InvokeRequired) { this.Invoke(new Action<string, Color>(LogMessage), message, color); return; }
            rtbLogs.SelectionStart = rtbLogs.TextLength;
            rtbLogs.SelectionLength = 0;
            rtbLogs.SelectionColor = color;
            rtbLogs.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            rtbLogs.ScrollToCaret();
        }
        private void LogToConsole(string message) => LogMessage(message);


        private void dgvClients_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e) => UpdateTotalClients();
        private void dgvClients_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e) => UpdateTotalClients();
        private void dgvClients_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e) => UpdateTotalClients();

        // Khi chon may khac tren grid -> cap nhat text nut Khoa/Mo cho dung may do
        private void dgvClients_SelectionChanged(object sender, EventArgs e)
        {
            UpdateLockButtonText();
        }

        // Handler trong do Designer sinh
        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e) { }
        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e) { }
        private void dgvClients_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void Form2_Load(object sender, EventArgs e) { }
        private void txtMessage_TextChanged(object sender, EventArgs e) { }
        private void rtbLogs_SelectedIndexChanged(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void groupBox1_Enter(object sender, EventArgs e) { }
        private void dgvClients_CellContentClick_1(object sender, DataGridViewCellEventArgs e) { }
        private void panel9_Paint(object sender, PaintEventArgs e) { }
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e) { }
        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e) { }
        private void btnScan_Click_1(object sender, EventArgs e) { }
        private void pnlTopBar_Paint(object sender, PaintEventArgs e) { }

    }
}

