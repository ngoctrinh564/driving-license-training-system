using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dacn_gplx.Models;
using dacn_gplx.ViewModels;

namespace dacn_gplx.Controllers
{
    [Authorize]
    public class HoSoThiSinhController : Controller
    {
        private readonly QuanLyGplxContext _context;
        private readonly IWebHostEnvironment _env;

        private readonly string[] AllowExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024;
        private const int MaxImageCount = 10;

        public HoSoThiSinhController(QuanLyGplxContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ==================================================================
        // HÀM PHỤ: LƯU ẢNH
        // ==================================================================
        private async Task SaveImagesAsync(int kskId, IFormFile[]? files)
        {
            if (files == null || files.Length == 0) return;

            string folder = Path.Combine(_env.WebRootPath, "images", "health");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            int currentCount = await _context.AnhGksks.CountAsync(a => a.KhamsuckhoeId == kskId);

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                string ext = Path.GetExtension(file.FileName).ToLower();
                if (!AllowExtensions.Contains(ext)) continue;

                if (file.Length > MaxFileSize) continue;

                if (currentCount >= MaxImageCount) break;

                string fileName = Guid.NewGuid() + ext;
                string path = Path.Combine(folder, fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);

                _context.AnhGksks.Add(new AnhGksk
                {
                    KhamsuckhoeId = kskId,
                    Urlanh = $"/images/health/{fileName}"
                });

                currentCount++;
            }

            await _context.SaveChangesAsync();
        }


        private void DeletePhysicalFile(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            string path = Path.Combine(_env.WebRootPath, url.TrimStart('/'));

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }


        // ==================================================================
        // 1) INDEX (DUY NHẤT)
        // ==================================================================
        public async Task<IActionResult> Index(string? status)
        {
            var username = User.Identity?.Name;
            var hocVien = await _context.Users
                .Include(u => u.HocViens)
                .Where(u => u.Username == username)
                .Select(u => u.HocViens.FirstOrDefault())
                .FirstOrDefaultAsync();

            if (hocVien == null)
                return RedirectToAction("Index", "Home");

            var today = DateOnly.FromDateTime(DateTime.Today);

            var query = _context.HoSoThiSinhs
                .Include(h => h.Hang)
                .Where(h => h.HocvienId == hocVien.HocvienId);

            if (!string.IsNullOrEmpty(status) && status != "All")
                query = query.Where(h => h.Trangthai == status);

            var list = await query
                .OrderByDescending(h => h.HosoId)
                .Select(h => new HoSoThiSinhViewModel
                {
                    HosoId = h.HosoId,
                    Tenhoso = h.Tenhoso,
                    Trangthai = h.Trangthai,
                    Ngaydangky = h.Ngaydangky,
                    TenHang = h.Hang.Tenhang,

                    ChoPhepChinhSua =
                        h.Trangthai == "Đang xử lý" &&
                        (h.Ngaydangky == null || h.Ngaydangky.Value.AddDays(7) >= today),

                    ChoPhepXoa =
                        h.Trangthai == "Từ chối" ||
                        (h.Trangthai == "Đang xử lý" &&
                         (h.Ngaydangky == null || h.Ngaydangky.Value.AddDays(7) >= today))
                })
                .ToListAsync();

            ViewBag.StatusFilter = status ?? "All";

            return View(list);
        }



        // ==================================================================
        // 2) CREATE - GET
        // ==================================================================
        public async Task<IActionResult> Create()
        {
            ViewBag.HangGPLX = await _context.HangGplxes.ToListAsync();
            return View();
        }


        // ==================================================================
        // 3) CREATE - POST
        // ==================================================================
        [HttpPost]
        public async Task<IActionResult> Create(HoSoThiSinhViewModel model)
        {
            var username = User.Identity?.Name;
            var hocVien = await _context.Users.Include(u => u.HocViens)
                                  .Where(u => u.Username == username)
                                  .Select(u => u.HocViens.FirstOrDefault())
                                  .FirstOrDefaultAsync();

            if (hocVien == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy học viên.";
                return RedirectToAction("Index");
            }

            // NGÀY KHÁM KHÔNG ĐƯỢC TƯƠNG LAI
            if (model.Thoihan > DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError("Thoihan", "Ngày khám không được ở tương lai.");
                ViewBag.HangGPLX = await _context.HangGplxes.ToListAsync();
                return View(model);
            }

            // VALIDATE MẮT
            Regex reEye = new("^[0-9]{1,2}/[0-9]{1,2}$");
            if (!reEye.IsMatch(model.KhamMatTrai ?? "") || !reEye.IsMatch(model.KhamMatPhai ?? ""))
            {
                ModelState.AddModelError("", "Dữ liệu khám mắt không hợp lệ.");
                ViewBag.HangGPLX = await _context.HangGplxes.ToListAsync();
                return View(model);
            }

            // VALIDATE HUYẾT ÁP
            Regex reBp = new("^[0-9]{2,3}/[0-9]{2,3}$");
            if (!reBp.IsMatch(model.Huyetap ?? ""))
            {
                ModelState.AddModelError("", "Huyết áp không hợp lệ.");
                ViewBag.HangGPLX = await _context.HangGplxes.ToListAsync();
                return View(model);
            }

            var hang = await _context.HangGplxes.FindAsync(model.HangId);
            string tenHoSo = $"Hồ sơ {hang.Tenhang} - {hocVien.Hoten}";

            var ngayHetHan = model.Thoihan?.AddMonths(12);

            var ksk = new PhieuKhamSucKhoe
            {
                Hieuluc = "12 tháng",
                Thoihan = ngayHetHan,
                Khammat = $"{model.KhamMatTrai}|{model.KhamMatPhai}",
                Huyetap = model.Huyetap,
                Chieucao = model.Chieucao,
                Cannang = model.Cannang
            };

            _context.PhieuKhamSucKhoes.Add(ksk);
            await _context.SaveChangesAsync();

            var hoso = new HoSoThiSinh
            {
                HocvienId = hocVien.HocvienId,
                Tenhoso = tenHoSo,
                Ghichu = model.Ghichu,
                Ngaydangky = DateOnly.FromDateTime(DateTime.Today),
                Trangthai = "Đang xử lý",
                HangId = model.HangId,
                KhamsuckhoeId = ksk.KhamsuckhoeId
            };

            _context.HoSoThiSinhs.Add(hoso);
            await _context.SaveChangesAsync();

            await SaveImagesAsync(ksk.KhamsuckhoeId, model.AnhGkskFiles);

            TempData["SuccessMessage"] = "Tạo hồ sơ thành công!";
            return RedirectToAction("Index");
        }



        // ==================================================================
        // 4) DETAILS
        // ==================================================================
        public async Task<IActionResult> Details(int id)
        {
            var username = User.Identity?.Name;
            var hocVien = await _context.Users.Include(u => u.HocViens)
                              .Where(u => u.Username == username)
                              .Select(u => u.HocViens.FirstOrDefault())
                              .FirstOrDefaultAsync();

            var hoso = await _context.HoSoThiSinhs
                .Include(h => h.Hang)
                .Include(h => h.Khamsuckhoe).ThenInclude(k => k.AnhGksks)
                .FirstOrDefaultAsync(h => h.HosoId == id);

            if (hoso == null || hoso.HocvienId != hocVien?.HocvienId)
                return Unauthorized();

            var vm = new HoSoThiSinhViewModel
            {
                HosoId = hoso.HosoId,
                Tenhoso = hoso.Tenhoso, 
                Ngaydangky = hoso.Ngaydangky,
                Trangthai = hoso.Trangthai,
                Ghichu = hoso.Ghichu,
                TenHang = hoso.Hang?.Tenhang,

                Hieuluc = hoso.Khamsuckhoe?.Hieuluc,
                Thoihan = hoso.Khamsuckhoe?.Thoihan,

                Huyetap = hoso.Khamsuckhoe?.Huyetap,
                Chieucao = hoso.Khamsuckhoe?.Chieucao,
                Cannang = hoso.Khamsuckhoe?.Cannang,

                AnhGkskUrls = hoso.Khamsuckhoe.AnhGksks.Select(a => a.Urlanh).ToList()
            };

            if (!string.IsNullOrWhiteSpace(hoso.Khamsuckhoe?.Khammat))
            {
                var p = hoso.Khamsuckhoe.Khammat.Split('|');
                vm.KhamMatTrai = p[0];
                vm.KhamMatPhai = p.Length > 1 ? p[1] : "";
            }

            return View(vm);
        }


        // ==================================================================
        // 5) EDIT - GET
        // ==================================================================
        public async Task<IActionResult> Edit(int id)
        {
            var username = User.Identity?.Name;
            var hocVien = await _context.Users.Include(u => u.HocViens)
                              .Where(u => u.Username == username)
                              .Select(u => u.HocViens.FirstOrDefault())
                              .FirstOrDefaultAsync();

            var hoso = await _context.HoSoThiSinhs
                .Include(h => h.Hang)
                .Include(h => h.Khamsuckhoe).ThenInclude(k => k.AnhGksks)
                .FirstOrDefaultAsync(h => h.HosoId == id);

            if (hoso == null || hoso.HocvienId != hocVien?.HocvienId)
                return Unauthorized();

            var today = DateOnly.FromDateTime(DateTime.Today);
            bool allowEdit = hoso.Trangthai == "Đang xử lý" &&
                            (hoso.Ngaydangky == null || hoso.Ngaydangky.Value.AddDays(7) >= today);

            if (!allowEdit)
            {
                TempData["ErrorMessage"] = "Hồ sơ đã quá hạn sửa.";
                return RedirectToAction("Index");
            }

            var vm = new HoSoThiSinhViewModel
            {
                HosoId = hoso.HosoId,
                Ghichu = hoso.Ghichu,
                HangId = hoso.HangId,

                // NGÀY KHÁM = NGÀY HẾT HẠN - 12 THÁNG
                Thoihan = hoso.Khamsuckhoe?.Thoihan?.AddMonths(-12),

                Huyetap = hoso.Khamsuckhoe?.Huyetap,
                Chieucao = hoso.Khamsuckhoe?.Chieucao,
                Cannang = hoso.Khamsuckhoe?.Cannang,

                AnhGkskUrls = hoso.Khamsuckhoe.AnhGksks.Select(a => a.Urlanh).ToList(),
                AnhGkskIds = hoso.Khamsuckhoe.AnhGksks.Select(a => a.AnhId).ToList()
            };

            if (!string.IsNullOrWhiteSpace(hoso.Khamsuckhoe?.Khammat))
            {
                var arr = hoso.Khamsuckhoe.Khammat.Split('|');
                vm.KhamMatTrai = arr[0];
                vm.KhamMatPhai = arr.Length > 1 ? arr[1] : "";
            }

            ViewBag.HangGPLX = await _context.HangGplxes.ToListAsync();

            return View(vm);
        }



        // ==================================================================
        // 6) EDIT - POST
        // ==================================================================
        [HttpPost]
        public async Task<IActionResult> Edit(HoSoThiSinhViewModel model)
        {
            var username = User.Identity?.Name;
            var hocVien = await _context.Users.Include(u => u.HocViens)
                              .Where(u => u.Username == username)
                              .Select(u => u.HocViens.FirstOrDefault())
                              .FirstOrDefaultAsync();

            var hoso = await _context.HoSoThiSinhs
                .Include(h => h.Khamsuckhoe).ThenInclude(k => k.AnhGksks)
                .FirstOrDefaultAsync(h => h.HosoId == model.HosoId);

            if (hoso == null || hoso.HocvienId != hocVien?.HocvienId)
                return Unauthorized();

            var today = DateOnly.FromDateTime(DateTime.Today);
            bool allowEdit =
                hoso.Trangthai == "Đang xử lý" &&
                (hoso.Ngaydangky == null || hoso.Ngaydangky.Value.AddDays(7) >= today);

            if (!allowEdit)
            {
                TempData["ErrorMessage"] = "Không thể chỉnh sửa.";
                return RedirectToAction("Index");
            }

            var hang = await _context.HangGplxes.FindAsync(model.HangId);
            hoso.Tenhoso = $"Hồ sơ {hang.Tenhang} - {hocVien.Hoten}";
            hoso.HangId = model.HangId;
            hoso.Ghichu = model.Ghichu;

            var ksk = hoso.Khamsuckhoe;

            // Ngày hết hạn = ngày khám + 12 tháng
            var ngayHetHan = model.Thoihan?.AddMonths(12);

            ksk.Thoihan = ngayHetHan;
            ksk.Hieuluc = "12 tháng";

            ksk.Khammat = $"{model.KhamMatTrai}|{model.KhamMatPhai}";
            ksk.Huyetap = model.Huyetap;
            ksk.Chieucao = model.Chieucao;
            ksk.Cannang = model.Cannang;

            // XÓA ẢNH
            if (model.AnhGkskDeleteIds != null)
            {
                var del = ksk.AnhGksks.Where(a => model.AnhGkskDeleteIds.Contains(a.AnhId)).ToList();

                foreach (var img in del)
                {
                    DeletePhysicalFile(img.Urlanh);
                    _context.AnhGksks.Remove(img);
                }
            }

            await _context.SaveChangesAsync();

            // THÊM ẢNH MỚI
            await SaveImagesAsync(ksk.KhamsuckhoeId, model.AnhGkskFiles);

            TempData["SuccessMessage"] = "Cập nhật thành công!";
            return RedirectToAction("Details", new { id = hoso.HosoId });
        }



        // ==================================================================
        // 7) DELETE
        // ==================================================================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var hoso = await _context.HoSoThiSinhs
                .Include(h => h.Khamsuckhoe).ThenInclude(k => k.AnhGksks)
                .FirstOrDefaultAsync(h => h.HosoId == id);

            if (hoso == null)
            {
                TempData["ErrorMessage"] = "Hồ sơ không tồn tại.";
                return RedirectToAction("Index");
            }

            var today = DateOnly.FromDateTime(DateTime.Today);

            bool allowDelete =
                hoso.Trangthai == "Từ chối" ||
                (hoso.Trangthai == "Đang xử lý" &&
                 (hoso.Ngaydangky == null || hoso.Ngaydangky.Value.AddDays(7) >= today));

            if (!allowDelete)
            {
                TempData["ErrorMessage"] = "Không thể xoá hồ sơ này.";
                return RedirectToAction("Index");
            }

            foreach (var img in hoso.Khamsuckhoe.AnhGksks)
                DeletePhysicalFile(img.Urlanh);

            _context.AnhGksks.RemoveRange(hoso.Khamsuckhoe.AnhGksks);
            _context.PhieuKhamSucKhoes.Remove(hoso.Khamsuckhoe);
            _context.HoSoThiSinhs.Remove(hoso);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa thành công!";
            return RedirectToAction("Index");
        }
    }
}
