using System;
using System.Net.Sockets;
using System.Text;

namespace LabAdmin.Shared
{
    public static class NetworkProtocol
    {
        // ----- CONG MANG -----
        public const int UDP_SCAN_PORT = 8888;
        public const int UDP_REPLY_PORT = 8090;
        public const int TCP_PORT = 9090;

        public const char DELIMITER = '|';

        // ----- LOAI GOI -----
        public const byte TYPE_TEXT = 1;
        public const byte TYPE_IMAGE = 2;
        public const byte TYPE_ZIP = 3;
        public const int HEADER_SIZE = 5;

        // ----- LENH Server -> Client -----
        public const string CMD_SCAN = "CMD_SCAN";
        public const string CMD_LOCK = "CMD_LOCK";
        public const string CMD_UNLOCK = "CMD_UNLOCK";
        public const string CMD_SHUTDOWN = "CMD_SHUTDOWN";
        public const string CMD_RESTART = "CMD_RESTART";
        public const string CMD_MSG = "CMD_MSG";
        public const string CMD_PULL = "CMD_PULL";
        public const string CMD_CAPTURE = "CMD_CAPTURE";

        // ----- PHAN HOI Client -> Server -----
        public const string REP_SCAN_ACK = "REP_SCAN_ACK";
        public const string REP_HELP = "REP_HELP";

        // ----- TRANG THAI grid (chuan hoa) -----
        public const string STATUS_ONLINE = "Đang hoạt động";
        public const string STATUS_LOCKED = "Đã khóa";
        public const string STATUS_OFFLINE = "Mất kết nối";

        public static byte[] BuildFrame(byte type, byte[] payload)
        {
            if (payload == null) payload = new byte[0];
            int len = payload.Length;
            byte[] frame = new byte[HEADER_SIZE + len];
            frame[0] = type;
            frame[1] = (byte)(len >> 24);
            frame[2] = (byte)(len >> 16);
            frame[3] = (byte)(len >> 8);
            frame[4] = (byte)(len);
            Buffer.BlockCopy(payload, 0, frame, HEADER_SIZE, len);
            return frame;
        }

        public static byte[] BuildTextFrame(string text)
        {
            return BuildFrame(TYPE_TEXT, Encoding.UTF8.GetBytes(text ?? string.Empty));
        }

        public static bool ReceiveExact(Socket socket, byte[] buffer, int count)
        {
            int offset = 0;
            while (offset < count)
            {
                int read = socket.Receive(buffer, offset, count - offset, SocketFlags.None);
                if (read == 0) return false;
                offset += read;
            }
            return true;
        }

        public static bool ReceiveFrame(Socket socket, out byte type, out byte[] payload)
        {
            type = 0; payload = null;
            byte[] header = new byte[HEADER_SIZE];
            if (!ReceiveExact(socket, header, HEADER_SIZE)) return false;
            type = header[0];
            int length = (header[1] << 24) | (header[2] << 16) | (header[3] << 8) | header[4];
            const int MAX = 100 * 1024 * 1024;
            if (length < 0 || length > MAX) return false;
            payload = new byte[length];
            if (length > 0 && !ReceiveExact(socket, payload, length)) return false;
            return true;
        }
    }
}
