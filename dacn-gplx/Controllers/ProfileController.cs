using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;

using dacn_gplx.Models;
using dacn_gplx.ViewModels;
using dacn_gplx.Services;

namespace dacn_gplx.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly QuanLyGplxContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly FaceValidator _faceValidator;

        public ProfileController(
            QuanLyGplxContext context,
            IWebHostEnvironment env,
            FaceValidator faceValidator)
        {
            _context = context;
            _env = env;
            _faceValidator = faceValidator;
        }

        // ============================
        // GET: /Profile
        // ============================
        [HttpGet]
        public async Task<IActionResult> Index(bool edit = false)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Auth");

            var user = await _context.Users
                .Include(u => u.HocViens)
                .ThenInclude(h => h.HoSoThiSinhs)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var hocVien = user.HocViens.FirstOrDefault();
            if (hocVien == null)
            {
                TempData["ErrorMessage"] = "Tài khoản chưa có thông tin học viên.";
                return RedirectToAction("Index", "Home");
            }

            // Lấy giấy khám sức khoẻ từ hồ sơ
            int? skkId = hocVien.HoSoThiSinhs
                                .OrderByDescending(h => h.HosoId)
                                .Select(h => h.KhamsuckhoeId)
                                .FirstOrDefault();

            var vm = new ProfileViewModel
            {
                HocvienId = hocVien.HocvienId,
                Hoten = hocVien.Hoten,
                SoCmndCccd = hocVien.SoCmndCccd,
                Namsinh = hocVien.Namsinh,
                Gioitinh = hocVien.Gioitinh,
                Sdt = hocVien.Sdt,
                Mail = hocVien.Mail,
                AvatarUrl = hocVien.AvatarUrl,

                // ⭐ Gán tình trạng giấy SKK
                KhamsuckhoeId = skkId
            };

            ViewBag.EditMode = edit;
            return View(vm);
        }

        // ============================
        // POST: /Profile/Update
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProfileViewModel model, IFormFile? avatarFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.EditMode = true;
                return View("Index", model);
            }

            var hocVien = await _context.HocViens
                .FirstOrDefaultAsync(h => h.HocvienId == model.HocvienId);

            if (hocVien == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin học viên.";
                return RedirectToAction("Index");
            }

            // ============================
            // Cập nhật thông tin cơ bản
            // ============================
            hocVien.Hoten = model.Hoten;
            hocVien.SoCmndCccd = model.SoCmndCccd;
            hocVien.Namsinh = model.Namsinh;
            hocVien.Gioitinh = model.Gioitinh;
            hocVien.Sdt = model.Sdt;
            hocVien.Mail = model.Mail;

            // ============================
            // Upload + kiểm tra ảnh thẻ
            // ============================
            if (avatarFile != null && avatarFile.Length > 0)
            {
                // --- Lưu file tạm ---
                var tempFolder = Path.Combine(_env.WebRootPath, "temp");
                if (!Directory.Exists(tempFolder))
                    Directory.CreateDirectory(tempFolder);

                var tempFileName = "temp_" + Guid.NewGuid() + Path.GetExtension(avatarFile.FileName);
                var tempPath = Path.Combine(tempFolder, tempFileName);

                using (var stream = new FileStream(tempPath, FileMode.Create))
                    await avatarFile.CopyToAsync(stream);

                // --- Kiểm tra ảnh mặt ---
                if (!_faceValidator.ValidateFace(tempPath, out string msg))
                {
                    System.IO.File.Delete(tempPath);
                    TempData["ErrorMessage"] = msg;
                    ViewBag.EditMode = true;
                    return View("Index", model);
                }

                // --- Lưu ảnh chính ---
                var avatarFolder = Path.Combine(_env.WebRootPath, "images", "avatar");
                if (!Directory.Exists(avatarFolder))
                    Directory.CreateDirectory(avatarFolder);

                var fileName = $"avatar_{hocVien.HocvienId}_{Guid.NewGuid()}{Path.GetExtension(avatarFile.FileName)}";
                var finalPath = Path.Combine(avatarFolder, fileName);

                System.IO.File.Move(tempPath, finalPath);

                hocVien.AvatarUrl = $"/images/avatar/{fileName}";
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
            return RedirectToAction("Index", new { edit = false });
        }
    }
}
