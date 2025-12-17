using System;
using System.Collections.Generic;
using System.Linq;

namespace dacn_gplx.Models
{
    public class Message
    {
        public int UserId { get; set; }      // ID học viên
        public string Sender { get; set; }   // "User" hoặc "Admin"
        public string Content { get; set; }  // Nội dung
        public DateTime Time { get; set; }   // Thời gian gửi
        public bool IsRead { get; set; }   // 👈 THÊM DÒNG NÀY
    }

    public static class ChatStore
    {
        // ===============================
        // LƯU TOÀN BỘ CHAT
        // Key = UserId
        // ===============================
        public static Dictionary<int, List<Message>> Chats
            = new Dictionary<int, List<Message>>();

        // ===============================
        // THÊM TIN NHẮN
        // ===============================
        public static void AddMessage(int userId, string sender, string content)
        {
            if (!Chats.ContainsKey(userId))
                Chats[userId] = new List<Message>();

            Chats[userId].Add(new Message
            {
                UserId = userId,
                Sender = sender,
                Content = content,
                Time = DateTime.Now,

                // ✅ LOGIC ĐÚNG
                IsRead = sender == "Admin"
            });
        }


        // ===============================
        // LẤY DANH SÁCH TIN NHẮN
        // ===============================
        public static List<Message> GetMessages(int userId)
        {
            return Chats.ContainsKey(userId)
                ? Chats[userId]
                : new List<Message>();
        }

        // ===============================
        // ĐẾM TIN NHẮN CHƯA ĐỌC (USER → ADMIN)
        // ===============================
        public static int GetUnreadCount(int userId)
        {
            if (!Chats.ContainsKey(userId))
                return 0;

            return Chats[userId]
                .Count(m => m.Sender == "User" && !m.IsRead);
        }


        // ===============================
        // ĐÁNH DẤU ADMIN ĐÃ XEM CHAT
        // ===============================
        public static void MarkAdminRead(int userId)
        {
            if (!Chats.ContainsKey(userId)) return;

            foreach (var m in Chats[userId])
            {
                if (m.Sender == "User")
                    m.IsRead = true;
            }
        }

        // ===============================
        // ĐÁNH DẤU USER ĐÃ ĐỌC TIN NHẮN (ADMIN → USER)
        // ===============================
        public static void MarkUserRead(int userId)
        {
            if (!Chats.ContainsKey(userId))
                return;

            foreach (var msg in Chats[userId])
            {
                if (msg.Sender == "Admin")
                {
                    msg.IsRead = true;
                }
            }
        }

        // ===============================
        // ĐẾM TIN NHẮN CHƯA ĐỌC (ADMIN → USER)
        // ===============================
        public static int GetUnreadForUser(int userId)
        {
            if (!Chats.ContainsKey(userId))
                return 0;

            return Chats[userId]
                .Count(m => m.Sender == "Admin" && !m.IsRead);
        }

        // ===============================
        // TỔNG TIN NHẮN USER → ADMIN CHƯA ĐỌC
        // ===============================
        public static int GetTotalUnreadForAdmin()
        {
            return Chats.Sum(kv =>
                kv.Value.Count(m => m.Sender == "User" && !m.IsRead)
            );
        }


    }
}
