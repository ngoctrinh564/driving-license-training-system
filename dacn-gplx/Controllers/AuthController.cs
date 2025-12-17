using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Collections.Generic;

// Alias Models
using UserEntity = dacn_gplx.Models.User;
using HocVienEntity = dacn_gplx.Models.HocVien;

using dacn_gplx.Models;
using dacn_gplx.Services;
using dacn_gplx.ViewModels;

namespace dacn_gplx.Controllers
{
    public class AuthController : Controller
    {
        private readonly QuanLyGplxContext _context;
        private readonly EmailService _emailService;

        public AuthController(QuanLyGplxContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ============================
        // LOGIN - GET
        // ============================
        public IActionResult Login()
        {
            return View();
        }

        // ============================ 
        // LOGIN - POST
        // ============================
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.HocViens)
                .FirstOrDefaultAsync(u =>
                    u.Isactive &&
                    (
                        u.Username == model.LoginId ||
                        u.HocViens.Any(h => h.Mail == model.LoginId)
                    )
                );

            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim("UserId", user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("RoleId", user.RoleId.ToString()),
                new Claim("RoleName", user.Role.Rolename ?? "")
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserRole", user.Role.Rolename);

            if (user.RoleId == 1)
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "Home");
        }

        // ============================
        // LOGOUT
        // ============================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ============================
        // REGISTER - GET
        // ============================
        public IActionResult Register()
        {
            return View();
        }

        // ============================
        // REGISTER - POST
        // ============================
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Username duy nhất
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("", "Tên đăng nhập đã tồn tại");
                return View(model);
            }

            // =========================
            // TẠO USER
            // =========================
            var user = new UserEntity
            {
                Username = model.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                RoleId = 2,
                Isactive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // =========================
            // TẠO HỒ SƠ HỌC VIÊN
            // =========================
            var hocVien = new HocVienEntity
            {
                Hoten = model.Hoten,
                SoCmndCccd = model.SoCmndCccd,
                Namsinh = model.Namsinh,
                Sdt = model.Sdt,
                Mail = model.Mail,
                UserId = user.UserId
            };

            _context.HocViens.Add(hocVien);
            await _context.SaveChangesAsync();

            // =========================
            // TỰ ĐỘNG ĐĂNG NHẬP
            // =========================
            var claims = new List<Claim>
            {
                new Claim("UserId", user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("RoleId", user.RoleId.ToString()),
                new Claim("RoleName", "User")
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserRole", "User");

            // =========================
            // ÉP VÀO TRANG THÔNG TIN CÁ NHÂN
            // =========================
            return RedirectToAction("Index", "Profile", new { edit = true });
        }

        // ================================================
        // FORGOT PASSWORD (3 bước) - GIỮ NGUYÊN
        // ================================================
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Otp))
            {
                ModelState.Remove("NewPassword");
                ModelState.Remove("ConfirmPassword");

                string sessionOtp = HttpContext.Session.GetString("OTP");
                string sessionUserId = HttpContext.Session.GetString("OTPUserId");

                ViewBag.ShowOtpForm = true;
                ViewBag.ShowPasswordForm = false;

                if (model.Otp != sessionOtp)
                {
                    ModelState.AddModelError("", "OTP không hợp lệ");
                    return View(model);
                }

                ViewBag.ShowPasswordForm = true;
                return View(model);
            }

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                string sessionUserId = HttpContext.Session.GetString("OTPUserId");

                ViewBag.ShowOtpForm = true;
                ViewBag.ShowPasswordForm = true;

                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError("", "Mật khẩu mới không khớp");
                    return View(model);
                }

                int userId = int.Parse(sessionUserId);
                var user = await _context.Users.FindAsync(userId);

                user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                await _context.SaveChangesAsync();

                HttpContext.Session.Remove("OTP");
                HttpContext.Session.Remove("OTPUserId");

                ViewBag.Message = "Mật khẩu đã đổi thành công!";
                ModelState.Clear();

                return View(new ForgotPasswordViewModel());
            }

            if (string.IsNullOrEmpty(model.Username))
            {
                ModelState.AddModelError("", "Hãy nhập username hoặc email");
                return View(model);
            }

            var userObj = await _context.Users
                .Include(u => u.HocViens)
                .FirstOrDefaultAsync(u =>
                    u.Isactive &&
                    (
                        u.Username == model.Username ||
                        u.HocViens.Any(h => h.Mail == model.Username)
                    )
                );

            if (userObj == null)
            {
                ModelState.AddModelError("", "Không tìm thấy tài khoản");
                return View(model);
            }

            var hocVien = userObj.HocViens.FirstOrDefault();

            if (hocVien == null || string.IsNullOrEmpty(hocVien.Mail))
            {
                ModelState.AddModelError("", "Tài khoản không có email");
                return View(model);
            }

            string otp = new Random().Next(100000, 999999).ToString();

            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("OTPUserId", userObj.UserId.ToString());

            _emailService.SendEmail(
                hocVien.Mail,
                "GPLX - Mã OTP xác thực",
                $"Mã OTP của bạn là: <b>{otp}</b>");

            ModelState.Clear();

            ViewBag.Message = "OTP đã được gửi đến email!";
            ViewBag.ShowOtpForm = true;
            ViewBag.ShowPasswordForm = false;

            return View(model);
        }
    }
}
