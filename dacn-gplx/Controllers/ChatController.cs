using Microsoft.AspNetCore.Mvc;
using dacn_gplx.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace dacn_gplx.Controllers
{
    public class ChatController : Controller
    {
        private readonly QuanLyGplxContext _context;

        public ChatController(QuanLyGplxContext context)
        {
            _context = context;
        }

        // =================================================
        // USER: LOAD KHUNG CHAT NỔI (PARTIAL)
        // =================================================
        public IActionResult Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Content("");

            var messages = ChatStore.GetMessages(userId.Value);
            return PartialView("Index", messages);
        }


        // =================================================
        // USER: GỬI TIN NHẮN
        // =================================================
        [HttpPost]
        public IActionResult SendMessage(string content)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || string.IsNullOrWhiteSpace(content))
                return Json(new { success = false });

            ChatStore.AddMessage(userId.Value, "User", content);
            return Json(new { success = true });
        }

        // =================================================
        // ADMIN: TRANG QUẢN LÝ CHAT (DANH SÁCH USER)
        // =================================================
        public IActionResult Admin()
        {
            if (!IsAdmin())
                return Unauthorized();

            var userIds = ChatStore.Chats.Keys.ToList();
            return View(userIds); // Views/Chat/Admin.cshtml
        }

        // =================================================
        // ADMIN: LOAD CHAT 1 USER (PARTIAL)
        // =================================================
        public IActionResult AdminChat(int? userId)
        {
            if (!IsAdmin())
                return Unauthorized();

            ViewBag.UserId = userId;

            var chatUserIds = ChatStore.Chats.Keys.ToList();

            var hocViens = _context.HocViens
                .Where(hv => hv.UserId != null && chatUserIds.Contains(hv.UserId.Value))
                .Select(hv => new
                {
                    UserId = hv.UserId!.Value,
                    HoTen = hv.Hoten,
                    Avatar = hv.AvatarUrl
                })
                .ToList();

            ViewBag.UserList = hocViens;

            var messages = new List<Message>();

            if (userId != null)
            {
                // ✅ ĐÁNH DẤU ADMIN ĐÃ ĐỌC TIN USER
                ChatStore.MarkAdminRead(userId.Value);

                messages = ChatStore.GetMessages(userId.Value);

                ViewBag.CurrentUserName = hocViens
                    .Where(h => h.UserId == userId)
                    .Select(h => h.HoTen)
                    .FirstOrDefault();
            }

            return View("AdminChat", messages);
        }


        // =================================================
        // ADMIN: GỬI TIN NHẮN CHO USER
        // =================================================
        [HttpPost]
        public IActionResult AdminSendMessage(int userId, string content)
        {
            if (!IsAdmin() || string.IsNullOrWhiteSpace(content))
                return Json(new { success = false });

            ChatStore.AddMessage(userId, "Admin", content);
            return Json(new { success = true });
        }

        // =================================================
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        // =================================================
        // USER: ĐÁNH DẤU ĐÃ ĐỌC TIN NHẮN TỪ ADMIN
        // =================================================
        [HttpPost]
        public IActionResult MarkUserRead()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                ChatStore.MarkUserRead(userId.Value);
            }
            return Ok();
        }

        // =================================================
        // USER: LẤY SỐ TIN CHƯA ĐỌC
        // =================================================
        public IActionResult GetUserUnreadCount()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(0);

            int count = ChatStore.GetUnreadForUser(userId.Value);
            return Json(count);
        }

        // =================================================
        // ADMIN: LẤY TỔNG SỐ TIN CHƯA ĐỌC
        // =================================================
        public IActionResult GetAdminUnreadCount()
        {
            if (!IsAdmin())
                return Json(0);

            int count = ChatStore.GetTotalUnreadForAdmin();
            return Json(count);
        }

    }
}
