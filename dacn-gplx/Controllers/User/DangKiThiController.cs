using dacn_gplx.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace dacn_gplx.Controllers.User
{
    public class DangKiThiController : Controller
    {
        private readonly QuanLyGplxContext _context;

        public DangKiThiController(QuanLyGplxContext context)
        {
            _context = context;
        }

        private static readonly Dictionary<string, int> TongKmHang = new()
        {
            // 🚲 XE MÁY
            { "A1", 100 },
            { "A",  120 },

            // 🚗 Ô TÔ CON
            { "B1", 810 },
            { "B",  900 },

            // 🚚 Ô TÔ TẢI
            { "C1", 1200 },
            { "C",  1500 },

            // 🚌 Ô TÔ KHÁCH
            { "D1", 1800 },
            { "D2", 2000 },
            { "D",  2200 },

            // 🚛 KÉO RƠ-MOÓC / ĐẦU KÉO
            { "BE",  1200 },
            { "C1E", 1400 },
            { "CE",  2400 },
            { "D1E", 1600 },
            { "D2E", 1800 },
            { "DE",  2600 }
        };

        // ================================
        // 1) DANH SÁCH KỲ THI
        // ================================
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var hv = await _context.HocViens
                .Include(h => h.HoSoThiSinhs)
                .FirstOrDefaultAsync(h => h.UserId == userId);

            if (hv == null) return View(new List<KyThi>());

            var hosoIds = hv.HoSoThiSinhs.Select(h => h.HosoId).ToList();

            // ❗ Lấy danh sách Hạng mà HV có khóa học đã thanh toán
            var hangDaThanhToan = await (
                    from ct in _context.ChiTietPhieuThanhToans
                    join p in _context.PhieuThanhToans on ct.PhieuId equals p.PhieuId
                    join hs in _context.HoSoThiSinhs on ct.HosoId equals hs.HosoId
                    where hosoIds.Contains(hs.HosoId)
                          && p.Trangthai == "Đã Thanh Toán"
                    select hs.HangId
                ).Distinct().ToListAsync();

            // ❗ Chỉ hiện kỳ thi có hạng mà HV đã có khóa học đã thanh toán
            var exams = await _context.KyThis
            .Include(k => k.LichThis)
            .Include(k => k.ChiTietDangKyThis) // ⭐ BẮT BUỘC
            .Where(k =>
                k.LichThis.Any(l => l.Thoigianthi > DateTime.Now) &&
                hangDaThanhToan.Any(h =>
                    k.Tenkythi.Trim().EndsWith(
                        _context.HangGplxes.First(x => x.HangId == h).Tenhang.Trim()
                    )
                )
            )
            .ToListAsync();
            // ===== ĐẾM SỐ HỌC VIÊN ĐĂNG KÝ CHO TỪNG KỲ THI =====
            foreach (var exam in exams)
            {
                int soDangKy = exam.ChiTietDangKyThis
                .Select(c => c.HosoId)
                .Distinct()
                .Count();

                ViewData[$"SoDangKy_{exam.KythiId}"] = soDangKy;
            }
            ViewBag.HosoIds = hosoIds;

            return View("~/Views/User/Exam/Index.cshtml", exams);
        }


        // ================================
        // 2) CHI TIẾT KỲ THI
        // ================================
        public async Task<IActionResult> Details(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            var exam = await _context.KyThis
                .Include(k => k.LichThis)
                .Include(k => k.ChiTietDangKyThis) // ⭐ BẮT BUỘC
                .FirstOrDefaultAsync(k => k.KythiId == id);

            if (exam == null) return NotFound();

            // ===== ĐẾM TỔNG SỐ HỌC VIÊN ĐÃ ĐĂNG KÝ =====
            int soDangKy = exam.ChiTietDangKyThis
                .Select(c => c.HosoId)
                .Distinct()
                .Count();

            ViewBag.SoDangKyHienThi = soDangKy;

            // ===== HỒ SƠ CỦA USER (để check nút đăng ký) =====
            if (userId != null)
            {
                var hv = await _context.HocViens
                    .Include(h => h.HoSoThiSinhs)
                    .FirstOrDefaultAsync(h => h.UserId == userId);

                ViewBag.HosoIds = hv?.HoSoThiSinhs.Select(h => h.HosoId).ToList()
                                    ?? new List<int>();
            }

            return View("~/Views/User/Exam/Details.cshtml", exam);
        }
        // ================================
        // 3) ĐĂNG KÝ KỲ THI
        // ================================
        [HttpPost]
        public async Task<IActionResult> Register(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            // ===== LẤY THÔNG TIN HỌC VIÊN =====
            var hocVien = await _context.HocViens
                .Include(h => h.HoSoThiSinhs)
                .FirstOrDefaultAsync(h => h.UserId == userId);

            if (hocVien == null || !hocVien.HoSoThiSinhs.Any())
            {
                TempData["KiThiWarning"] = "Bạn chưa có hồ sơ để đăng ký kỳ thi.";
                return RedirectToAction("Index");
            }

            // ===== LẤY THÔNG TIN KỲ THI =====
            var exam = await _context.KyThis
                .Include(k => k.ChiTietDangKyThis)
                .FirstOrDefaultAsync(k => k.KythiId == id);

            if (exam == null)
            {
                TempData["KiThiWarning"] = "Không tìm thấy kỳ thi.";
                return RedirectToAction("Index");
            }

            // ===== XÁC ĐỊNH HẠNG CỦA KỲ THI (dựa vào TenKyThi) =====

            // Lấy phần hạng ở cuối tên kỳ thi (VD: "Kỳ thi - A1" → "A1")
            string hangFromName = exam.Tenkythi?
                .Split('-')
                .Last()
                .Trim()
                ?? "";

            var hangThi = await _context.HangGplxes
                .FirstOrDefaultAsync(h => h.Tenhang == hangFromName);

            if (hangThi == null)
            {
                TempData["KiThiWarning"] = "Không xác định được hạng của kỳ thi này.";
                return RedirectToAction("Index", new { id });
            }


            // ===== CHỌN HỒ SƠ ĐÚNG HẠNG & ĐÃ DUYỆT =====
            var hoSo = hocVien.HoSoThiSinhs
                .Where(h => h.Trangthai == "Đã duyệt" && h.HangId == hangThi.HangId)
                .OrderByDescending(h => h.Ngaydangky)
                .FirstOrDefault();

            if (hoSo == null)
            {
                TempData["KiThiWarning"] =
                    $"Bạn chưa có hồ sơ hạng {hangThi.Tenhang} đã được duyệt để đăng ký kỳ thi này.";
                return RedirectToAction("Index");
            }

            int hosoId = hoSo.HosoId;

            // ===== KIỂM TRA KẾT QUẢ HỌC TẬP =====
            var daDatKetQua = await (from ct in _context.ChiTietKetQuaHocTaps
                                     join kq in _context.KetQuaHocTaps on ct.KetquahoctapId equals kq.KetquahoctapId
                                     where kq.HosoId == hosoId
                                           && ct.LythuyetKq == true
                                           && ct.SahinhKq == true
                                           && ct.DuongtruongKq == true
                                           && ct.MophongKq == true
                                     select ct).AnyAsync();

            if (!daDatKetQua)
            {
                TempData["KiThiWarning"] =
                    "Bạn chưa đạt đủ 4 nội dung của khóa học để đăng ký thi.";
                return RedirectToAction("Index", new { id });
            }

            // ===== KIỂM TRA ĐỦ KM TỐI THIỂU =====
            var ketQuaHocTap = await _context.KetQuaHocTaps
                .FirstOrDefaultAsync(k => k.HosoId == hosoId);

            if (ketQuaHocTap == null)
            {
                TempData["KiThiWarning"] = "Chưa có kết quả học tập để đăng ký thi.";
                return RedirectToAction("Index");
            }

            int kmDaHoc = int.TryParse(ketQuaHocTap.Sokmhoanthanh, out int km) ? km : 0;

            int kmCan = TongKmHang.ContainsKey(hangThi.Tenhang)
                ? TongKmHang[hangThi.Tenhang]
                : 0;

            if (kmDaHoc < kmCan)
            {
                TempData["KiThiWarning"] =
                    $"Bạn chưa đủ số KM tối thiểu của hạng {hangThi.Tenhang} " +
                    $"({kmDaHoc}/{kmCan} KM).";
                return RedirectToAction("Index");
            }

            // ===== KHÔNG CHO ĐĂNG KÝ HAI KỲ THI CÙNG HẠNG =====
            var cacKyThiDaDangKy = await _context.ChiTietDangKyThis
                .Include(c => c.Kythi)
                .Where(c => c.HosoId == hosoId)
                .ToListAsync();

            bool daDangKyHangNay = cacKyThiDaDangKy
                .Any(c => c.Kythi.Tenkythi.Trim()
                    .EndsWith(hangThi.Tenhang.Trim(), StringComparison.OrdinalIgnoreCase));

            if (daDangKyHangNay)
            {
                TempData["KiThiWarning"] =
                    $"Bạn đã đăng ký một kỳ thi khác thuộc hạng {hangThi.Tenhang}. Không thể đăng ký thêm.";
                return RedirectToAction("Index");
            }

            // ===== KIỂM TRA TRÙNG ĐĂNG KÝ =====
            if (exam.ChiTietDangKyThis.Any(x => x.HosoId == hosoId))
            {
                TempData["KiThiWarning"] = "Bạn đã đăng ký kỳ thi này.";
                return RedirectToAction("Index", new { id });
            }

            // ===== TIẾN HÀNH ĐĂNG KÝ =====
            _context.ChiTietDangKyThis.Add(new ChiTietDangKyThi
            {
                KythiId = id,
                HosoId = hosoId,
                Thoigiandangky = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["ExamSuccess"] = "Đăng ký kỳ thi thành công!";

            return RedirectToAction("Details", new { id });
        }


        // ================================
        // 4) DANH SÁCH KẾT QUẢ THI
        // ================================
        public async Task<IActionResult> DanhSachKQThi()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var hocVien = await _context.HocViens
                .Include(h => h.HoSoThiSinhs)
                .FirstOrDefaultAsync(h => h.UserId == userId);

            if (hocVien == null || !hocVien.HoSoThiSinhs.Any())
            {
                return View("~/Views/User/Exam/DanhSachKQThi.cshtml",
                    new List<ChiTietKetQuaThi>());
            }

            var hosoIds = hocVien.HoSoThiSinhs.Select(h => h.HosoId).ToList();

            var results = await _context.ChiTietKetQuaThis
                .Include(r => r.Baithi)
                .Include(r => r.Hoso)
                .Where(r => hosoIds.Contains(r.HosoId))
                .OrderByDescending(r => r.BaithiId)
                .ToListAsync();

            return View("~/Views/User/Exam/DanhSachKQThi.cshtml", results);
        }

        // ================================
        // 5) CHI TIẾT KẾT QUẢ THI
        // ================================
        public async Task<IActionResult> ChiTietKQThi(int baithiId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var hocVien = await _context.HocViens
                .Include(h => h.HoSoThiSinhs)
                .FirstOrDefaultAsync(h => h.UserId == userId);

            if (hocVien == null)
            {
                TempData["Warning"] = "Không tìm thấy hồ sơ.";
                return RedirectToAction("Index");
            }

            var hosoIds = hocVien.HoSoThiSinhs.Select(h => h.HosoId).ToList();

            var result = await _context.ChiTietKetQuaThis
                .Include(r => r.Baithi)
                .Include(r => r.Hoso)
                .FirstOrDefaultAsync(r => r.BaithiId == baithiId &&
                                          hosoIds.Contains(r.HosoId));

            if (result == null)
            {
                TempData["Warning"] = "Không tìm thấy kết quả.";
                return RedirectToAction("Index");
            }

            return View("~/Views/User/Exam/ChiTietKQThi.cshtml", result);
        }
    }
}
