using System;

namespace LabAdmin.Shared
{
    public static class NetworkProtocol
    {
        // Ký tự phân cách dữ liệu (Delimiter)
        public const char DELIMITER = '|';

        // Các lệnh từ Server gửi xuống Client
        public const string CMD_SCAN = "CMD_SCAN";
        public const string CMD_LOCK = "CMD_LOCK";
        public const string CMD_UNLOCK = "CMD_UNLOCK";
        public const string CMD_SHUTDOWN = "CMD_SHUTDOWN";
        public const string CMD_RESTART = "CMD_RESTART";
        public const string CMD_MSG = "CMD_MSG";
        public const string CMD_PULL = "CMD_PULL";
        public const string CMD_CAPTURE = "CMD_CAPTURE";
        // Các lệnh từ Client phản hồi lên Server
        public const string REP_SCAN_ACK = "REP_SCAN_ACK";
    }
}