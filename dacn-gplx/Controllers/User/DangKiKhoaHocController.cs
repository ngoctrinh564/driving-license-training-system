using dacn_gplx.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace dacn_gplx.Controllers.User
{
    public class DangKiKhoaHocController : Controller
    {
        private readonly QuanLyGplxContext _context;

        public DangKiKhoaHocController(QuanLyGplxContext context)
        {
            _context = context;
        }

        // ============================================
        // DANH SÁCH KHÓA HỌC + TÌM KIẾM + LỌC HẠNG
        // ============================================
        public async Task<IActionResult> Index(int? hangId, string? search)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            List<int> hosoIds = new();

            if (userId != null)
            {
                var hv = await _context.HocViens
                    .Include(h => h.HoSoThiSinhs)
                    .FirstOrDefaultAsync(h => h.UserId == userId);

                if (hv != null)
                    hosoIds = hv.HoSoThiSinhs.Select(h => h.HosoId).ToList();
            }

            ViewBag.HosoIds = hosoIds;

            var list = _context.KhoaHocs
                .Include(k => k.Hang)
                .Where(k => k.Ngaybatdau.HasValue && k.Ngaybatdau.Value > DateOnly.FromDateTime(DateTime.Today))
                .AsQueryable();

            if (hangId != null)
                list = list.Where(k => k.HangId == hangId);

            if (!string.IsNullOrEmpty(search))
                list = list.Where(k => k.TenKhoaHoc.Contains(search));

            var khoaHocList = await list.ToListAsync();

            List<int> dangKyIds = new();
            Dictionary<int, bool> khoaHocHoanThanhDict = new();
            Dictionary<int, bool> khoaHocKhongDuocDangKyDict = new();

            if (hosoIds.Any())
            {
                // Lấy tất cả ChiTiet kết quả học tập kèm KetQuaHocTap
                var chiTietList = await _context.ChiTietKetQuaHocTaps
                    .Include(c => c.KetQuaHocTap)
                    .Include(c => c.Khoahoc)
                    .Where(c => c.KetQuaHocTap != null
                                && c.KetQuaHocTap.HosoId.HasValue
                                && hosoIds.Contains(c.KetQuaHocTap.HosoId.Value))
                    .ToListAsync();

                // Danh sách khóa học đã đăng ký
                dangKyIds = chiTietList
                    .Where(c => c.KetQuaHocTap != null && c.KhoahocId.HasValue)
                    .Select(c => c.KhoahocId.Value)
                    .Distinct()
                    .ToList();

                // Lấy tất cả HangId mà học viên đã hoàn thành
                var hangDaHoanThanh = chiTietList
                    .Where(c => c.LythuyetKq == true &&
                                c.SahinhKq == true &&
                                c.DuongtruongKq == true &&
                                c.MophongKq == true &&
                                c.Khoahoc?.HangId != null)
                    .Select(c => c.Khoahoc.HangId.Value)
                    .Distinct()
                    .ToList();

                // Kiểm tra từng khóa học
                foreach (var kh in khoaHocList)
                {
                    // Hoàn thành khóa học này
                    var chiTietCuaKh = chiTietList.Where(c => c.KhoahocId == kh.KhoahocId).ToList();
                    bool hoanThanh = chiTietCuaKh.Any() && chiTietCuaKh.All(c =>
                        c.LythuyetKq == true &&
                        c.SahinhKq == true &&
                        c.DuongtruongKq == true &&
                        c.MophongKq == true
                    );
                    khoaHocHoanThanhDict[kh.KhoahocId] = hoanThanh;

                    // Khóa học không được đăng ký nếu học viên đã hoàn thành khóa cùng hạng
                    khoaHocKhongDuocDangKyDict[kh.KhoahocId] = hangDaHoanThanh.Contains(kh.HangId ?? 0);
                }
            }

            ViewBag.DaDangKy = dangKyIds;
            ViewBag.KhoaHocHoanThanhDict = khoaHocHoanThanhDict;
            ViewBag.KhoaHocKhongDuocDangKyDict = khoaHocKhongDuocDangKyDict;
            ViewBag.HangList = await _context.HangGplxes.ToListAsync();

            return View("~/Views/User/KhoaHoc/Index.cshtml", khoaHocList);
        }

        // ============================================
        // XEM CHI TIẾT KHÓA HỌC
        // ============================================
        public async Task<IActionResult> Details(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            List<int> hosoIds = new();

            if (userId != null)
            {
                var hv = await _context.HocViens
                    .Include(h => h.HoSoThiSinhs)
                    .FirstOrDefaultAsync(h => h.UserId == userId);

                if (hv != null)
                    hosoIds = hv.HoSoThiSinhs.Select(h => h.HosoId).ToList();
            }

            ViewBag.HosoIds = hosoIds;

            var khoaHoc = await _context.KhoaHocs
                .Include(k => k.Hang)
                .Include(k => k.ChiTietKetQuaHocTaps)
                    .ThenInclude(c => c.KetQuaHocTap)
                .FirstOrDefaultAsync(k => k.KhoahocId == id);

            if (khoaHoc == null)
                return NotFound();

            bool daDangKy = false;
            ChiTietKetQuaHocTap ketQuaHocVien = null;

            if (hosoIds.Any())
            {
                ketQuaHocVien = khoaHoc.ChiTietKetQuaHocTaps
                    .FirstOrDefault(c => c.KetQuaHocTap != null &&
                                         c.KetQuaHocTap.HosoId.HasValue &&
                                         hosoIds.Contains(c.KetQuaHocTap.HosoId.Value));

                daDangKy = ketQuaHocVien != null;
            }

            bool khoaHocHoanThanhCungHang = false;

            if (hosoIds.Any() && khoaHoc.HangId.HasValue)
            {
                khoaHocHoanThanhCungHang = await _context.ChiTietKetQuaHocTaps
                    .Include(c => c.KetQuaHocTap)
                    .Where(c => c.KetQuaHocTap != null
                                && c.KetQuaHocTap.HosoId.HasValue
                                && hosoIds.Contains(c.KetQuaHocTap.HosoId.Value)
                                && c.Khoahoc != null
                                && c.Khoahoc.HangId == khoaHoc.HangId
                                && c.LythuyetKq == true
                                && c.SahinhKq == true
                                && c.DuongtruongKq == true
                                && c.MophongKq == true)
                    .AnyAsync();
            }

            ViewBag.DaDangKy = daDangKy;
            ViewBag.KetQuaHocVien = ketQuaHocVien;
            ViewBag.KhoaHocHoanThanhCungHang = khoaHocHoanThanhCungHang;

            return View("~/Views/User/KhoaHoc/Details.cshtml", khoaHoc);
        }

        // ============================================
        // DANH SÁCH KẾT QUẢ HỌC TẬP
        // ============================================
        public async Task<IActionResult> KetQuaHocTap()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var hv = await _context.HocViens
                .Include(h => h.HoSoThiSinhs)
                .FirstOrDefaultAsync(h => h.UserId == userId);

            if (hv == null || !hv.HoSoThiSinhs.Any())
            {
                TempData["KQWarning"] = "Bạn chưa có kết quả học tập nào.";
                return View("~/Views/User/KhoaHoc/KetQuaHocTap.cshtml", new List<KetQuaHocTap>());
            }

            var hosoIds = hv.HoSoThiSinhs.Select(h => h.HosoId).ToList();

            var ketQuaList = await _context.KetQuaHocTaps
                .Include(k => k.Hoso)
                    .ThenInclude(h => h.Hocvien)
                .Include(k => k.Ketquahoctap)
                    .ThenInclude(c => c.Khoahoc)
                .Where(k => k.HosoId.HasValue && hosoIds.Contains(k.HosoId.Value))
                .ToListAsync();

            return View("~/Views/User/KhoaHoc/KetQuaHocTap.cshtml", ketQuaList);
        }

        // GET: DangKiKhoaHoc/ChiTietKetQua
        public async Task<IActionResult> ChiTietKetQua(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            // Lấy danh sách hồ sơ của học viên
            var hv = await _context.HocViens
                .Include(h => h.HoSoThiSinhs)
                .FirstOrDefaultAsync(h => h.UserId == userId);

            if (hv == null || !hv.HoSoThiSinhs.Any())
                return RedirectToAction("Index");

            var hosoIds = hv.HoSoThiSinhs.Select(h => h.HosoId).ToList();

            // Lấy chi tiết kết quả học tập theo ID của ChiTietKetQuaHocTap
            var chiTiet = await _context.ChiTietKetQuaHocTaps
                .Include(c => c.KetQuaHocTap)
                    .ThenInclude(k => k.Hoso)
                        .ThenInclude(h => h.Hocvien)
                .Include(c => c.Khoahoc)
                    .ThenInclude(k => k.Hang)
                .FirstOrDefaultAsync(c => c.KetquahoctapId == id &&
                                          c.KetQuaHocTap != null &&
                                          c.KetQuaHocTap.HosoId.HasValue &&
                                          hosoIds.Contains(c.KetQuaHocTap.HosoId.Value));

            if (chiTiet == null)
                return NotFound();

            return View("~/Views/User/KhoaHoc/ChiTietKetQua.cshtml", chiTiet);
        }

        // ============================================
        // ĐĂNG KÝ KHÓA HỌC
        // ============================================
        public async Task<IActionResult> DangKy(int id, bool? confirm)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            // Lấy học viên + hồ sơ
            var hv = await _context.HocViens
                .Include(h => h.HoSoThiSinhs)
                .FirstOrDefaultAsync(h => h.UserId == userId);

            if (hv == null || !hv.HoSoThiSinhs.Any())
            {
                TempData["Warning"] = "Bạn chưa có hồ sơ để đăng ký khóa học!";
                return RedirectToAction("Index");
            }

            // Lấy khóa học
            var khoaHoc = await _context.KhoaHocs
                .Include(k => k.Hang)
                .FirstOrDefaultAsync(k => k.KhoahocId == id);

            if (khoaHoc == null)
                return NotFound();

            // ============================================================
            // 1️⃣ TỰ ĐỘNG CHỌN ĐÚNG HỒ SƠ CÙNG HẠNG VỚI KHÓA HỌC
            // ============================================================
            var hoSo = hv.HoSoThiSinhs
                .Where(h => h.HangId == khoaHoc.HangId)
                .OrderByDescending(h => h.HosoId)
                .FirstOrDefault();

            if (hoSo == null)
            {
                TempData["Warning"] =
                    $"Bạn chưa có hồ sơ hạng {khoaHoc.Hang.Tenhang} để đăng ký khóa học này!";
                return RedirectToAction("Index");
            }

            int hosoId = hoSo.HosoId;

            // Kiểm tra đã đăng ký khóa học này chưa
            bool alreadyRegistered = await _context.ChiTietKetQuaHocTaps
                .AnyAsync(c => c.KhoahocId == id &&
                               c.KetQuaHocTap != null &&
                               c.KetQuaHocTap.HosoId == hosoId);

            if (alreadyRegistered)
            {
                TempData["Warning"] = "Bạn đã đăng ký khóa học này rồi.";
                return RedirectToAction("Index");
            }

            // ==================================================================
            // 1️⃣ KIỂM TRA TRÙNG HẠNG -> XÁC ĐỊNH ĐANG HỌC HAY HỌC XONG
            // ==================================================================
            var khoaCungHang = await _context.ChiTietKetQuaHocTaps
                .Include(c => c.KetQuaHocTap)
                .Include(c => c.Khoahoc)
                .Where(c => c.KetQuaHocTap.HosoId == hosoId &&
                            c.Khoahoc.HangId == khoaHoc.HangId)
                .ToListAsync();

            bool hasInProgress = khoaCungHang.Any(c =>
                c.LythuyetKq == false ||
                c.SahinhKq == false ||
                c.DuongtruongKq == false ||
                c.MophongKq == false
            );

            bool hasCompleted = khoaCungHang.Any(c =>
                c.LythuyetKq == true &&
                c.SahinhKq == true &&
                c.DuongtruongKq == true &&
                c.MophongKq == true
            );

            // ⇨ ❌ ĐANG HỌC DỞ → KHÔNG CHO ĐĂNG KÝ
            if (hasInProgress)
            {
                TempData["Warning"] =
                    $"Bạn đang học một khóa học hạng {khoaHoc.Hang.Tenhang}. Bạn phải hoàn thành trước khi đăng ký thêm.";
                return RedirectToAction("Index");
            }

            // ⇨ ⚠️ ĐÃ HỌC XONG → YÊU CẦU XÁC NHẬN
            if (hasCompleted && confirm != true)
            {
                TempData["ConfirmMessage"] =
                    $"Bạn đã hoàn thành một khóa học hạng {khoaHoc.Hang.Tenhang}. Bạn có muốn đăng ký thêm không?";

                TempData["ConfirmUrl"] = Url.Action("DangKy", new { id, confirm = true });

                return RedirectToAction("Index");
            }

            // ==================================================================
            // 2️⃣ RÀNG BUỘC: Học viên chỉ được đăng ký đúng hạng hồ sơ
            // ==================================================================
            if (khoaHoc.HangId != hoSo.HangId)
            {
                var tenHoSoHang = await _context.HangGplxes
                    .Where(h => h.HangId == hoSo.HangId)
                    .Select(h => h.Tenhang)
                    .FirstOrDefaultAsync();

                TempData["Warning"] =
                    $"Hồ sơ của bạn là hạng {tenHoSoHang}, không thể đăng ký khóa học hạng {khoaHoc.Hang.Tenhang}.";

                return RedirectToAction("Index");
            }

            // ==================================================================
            // 3️⃣ TIẾN HÀNH ĐĂNG KÝ
            // ==================================================================
            var chiTiet = new ChiTietKetQuaHocTap
            {
                KhoahocId = id,
                LythuyetKq = false,
                SahinhKq = false,
                DuongtruongKq = false,
                MophongKq = false
            };

            _context.ChiTietKetQuaHocTaps.Add(chiTiet);
            await _context.SaveChangesAsync();

            int newId = chiTiet.KetquahoctapId;

            var ketQua = new KetQuaHocTap
            {
                KetquahoctapId = newId,
                HosoId = hosoId,
                Sobuoihoc = 0,
                Sobuoivang = 0
            };

            _context.KetQuaHocTaps.Add(ketQua);
            await _context.SaveChangesAsync();
            
            // ==================================================================
            // 4️⃣ TẠO PHIẾU THANH TOÁN TỰ ĐỘNG
            // ==================================================================

            // Lấy học phí từ bảng HangGPLX
            decimal hocPhi = khoaHoc.Hang.Hocphi ?? 0;

            // Tạo phiếu thanh toán
            var phieu = new PhieuThanhToan
            {
                Tenphieu = $"Học phí khóa học {khoaHoc.TenKhoaHoc}",
                Ngaylap = DateOnly.FromDateTime(DateTime.Today),
                Tongtien = hocPhi,
                Ngaynop = null,
                AnhMinhChung = null,
                Trangthai = "Chưa Thanh Toán"
            };

            _context.PhieuThanhToans.Add(phieu);
            await _context.SaveChangesAsync();

            // ❗❗ RẤT QUAN TRỌNG: THÊM LIÊN KẾT VÀO CHI TIẾT PHIẾU THANH TOÁN
            var chiTietPhieu = new ChiTietPhieuThanhToan
            {
                PhieuId = phieu.PhieuId,   // ID phiếu vừa tạo
                HosoId = hosoId            // Hồ sơ của học viên
            };

            _context.ChiTietPhieuThanhToans.Add(chiTietPhieu);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Bạn đã đăng ký khóa học. Vui lòng thanh toán để hoàn tất!";

            return RedirectToAction("Detail", "PhieuThanhToan", new { id = phieu.PhieuId });
        }
    }
}
