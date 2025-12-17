using dacn_gplx.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace dacn_gplx.Controllers.Admin
{
    public class KetQuaController : Controller
    {
        private readonly QuanLyGplxContext _context;

        public KetQuaController(QuanLyGplxContext context)
        {
            _context = context;
        }
        // ===== QUY ĐỊNH TỔNG KM THEO HẠNG GPLX =====
        private static readonly Dictionary<string, int> TongKmTheoHang = new()
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


        // ============================
        // 1) KẾT QUẢ HỌC TẬP - INDEX + TÌM KIẾM
        // ============================
        public IActionResult HocTap(string search)
        {
            var query = _context.KetQuaHocTaps
                .Include(k => k.Hoso)
                    .ThenInclude(h => h.Hocvien)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(k =>
                    (k.Hoso.Hocvien.Hoten ?? "").Contains(search) ||
                    (k.Hoso.Tenhoso ?? "").Contains(search)
                );
            }

            ViewBag.Search = search;
            return View("~/Views/Admin/KetQua/HocTap.cshtml", query.ToList());
        }

        // ============================
        // 2) CHI TIẾT KẾT QUẢ HỌC TẬP - XEM CHI TIẾT
        // ============================
        public IActionResult ChiTietHocTap(int id)
        {
            var ketQua = _context.KetQuaHocTaps
                 .Include(k => k.Hoso)
                   .ThenInclude(h => h.Hocvien)
                   .FirstOrDefault(k => k.KetquahoctapId == id);

            if (ketQua == null) return NotFound();
            // Lấy danh sách chi tiết kết quả học tập kèm thông tin khóa học và học viên
            var chiTietList = _context.ChiTietKetQuaHocTaps
                .Include(c => c.Khoahoc) // ✅ Load tên khóa học
                .Include(c => c.KetQuaHocTap)
                    .ThenInclude(k => k.Hoso)
                        .ThenInclude(h => h.Hocvien) // ✅ Load tên học viên
                .Where(c => c.KetquahoctapId == id)
                .ToList();

            if (chiTietList.Count == 0) return NotFound();

            ViewBag.KetQua = ketQua;

            return View("~/Views/Admin/KetQua/ChiTietHocTap.cshtml", chiTietList);
        }

        // ============================
        // 3) KẾT QUẢ HỌC TẬP - SỬA KET_QUA_HOC_TAP
        // ============================
        public IActionResult SuaHocTap(int id)
        {
            var kq = _context.KetQuaHocTaps
                .Include(x => x.Hoso)
                    .ThenInclude(h => h.Hocvien)
                .Include(x => x.Hoso)
                    .ThenInclude(h => h.Hang)   // ✅ HẠNG NẰM Ở HỒ SƠ
                .FirstOrDefault(x => x.KetquahoctapId == id);

            if (kq == null)
                return NotFound();

            var chiTiet = _context.ChiTietKetQuaHocTaps
                .Include(x => x.Khoahoc)
                .Where(x => x.KetquahoctapId == id)
                .ToList();

            // ================= KM =================
            int tongKmDaHoc = int.TryParse(kq.Sokmhoanthanh, out int km) ? km : 0;
            string hang = kq.Hoso.Hang.Tenhang;   // ✅ LẤY ĐÚNG HẠNG


            ViewBag.ChiTietHocTap = chiTiet;

            return View("~/Views/Admin/KetQua/SuaHocTap.cshtml", kq);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuaHocTap(KetQuaHocTap model, List<ChiTietKetQuaHocTap> chiTietList)
        {
            // ================= LẤY KẾT QUẢ HỌC TẬP =================
            var kq = _context.KetQuaHocTaps
                .Include(x => x.Hoso)
                    .ThenInclude(h => h.Hang)
                .FirstOrDefault(x => x.KetquahoctapId == model.KetquahoctapId);

            if (kq == null)
                return NotFound();

            // ================= KM THEO HẠNG =================
            string hang = kq.Hoso.Hang.Tenhang;

            int tongKmDaHoc = int.TryParse(model.Sokmhoanthanh, out int km) ? km : 0;


            // ================= NẾU CÓ LỖI → TRẢ VIEW NGAY =================
            if (!ModelState.IsValid)
            {
                ViewBag.ChiTietHocTap = _context.ChiTietKetQuaHocTaps
                    .Include(x => x.Khoahoc)
                    .Where(x => x.KetquahoctapId == model.KetquahoctapId)
                    .ToList();

                return View("~/Views/Admin/KetQua/SuaHocTap.cshtml", model);
            }

            // ================= CẬP NHẬT KQ HỌC TẬP =================
            kq.Sobuoihoc = model.Sobuoihoc;
            kq.Sobuoivang = model.Sobuoivang;
            kq.Nhanxet = model.Nhanxet;
            kq.Sokmhoanthanh = model.Sokmhoanthanh;

            // ================= CẬP NHẬT CHI TIẾT =================
            var chiTietDb = _context.ChiTietKetQuaHocTaps
                .Where(x => x.KetquahoctapId == model.KetquahoctapId)
                .ToList();

            foreach (var ct in chiTietList)
            {
                var old = chiTietDb.FirstOrDefault(x => x.KhoahocId == ct.KhoahocId);
                if (old != null)
                {
                    old.LythuyetKq = ct.LythuyetKq;
                    old.SahinhKq = ct.SahinhKq;
                    old.DuongtruongKq = ct.DuongtruongKq;
                    old.MophongKq = ct.MophongKq;
                }
            }

            _context.SaveChanges();
            return RedirectToAction("SuaHocTap", new { id = model.KetquahoctapId });
        }
        // ============================
        // 5) KẾT QUẢ THI - INDEX + TÌM KIẾM
        // ============================
        public IActionResult Thi(string search)
        {
            var query = _context.ChiTietKetQuaThis
                .Include(c => c.Hoso)
                    .ThenInclude(h => h.Hocvien)
                .Include(c => c.Baithi)
                    .ThenInclude(b => b.Kythi)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    (c.Hoso!.Hocvien!.Hoten ?? "").Contains(search) ||
                    (c.Baithi!.Tenbaithi ?? "").Contains(search)
                );
            }

            ViewBag.Search = search;
            return View("~/Views/Admin/KetQua/Thi.cshtml", query.ToList());
        }

        // ============================
        // 6) KẾT QUẢ THI - XEM CHI TIẾT
        // ============================
        public IActionResult ChiTietThi(int baithiId, int hosoId)
        {
            var ketQuaThi = _context.ChiTietKetQuaThis
                .Include(c => c.Hoso)
                    .ThenInclude(h => h.Hocvien)
                .Include(c => c.Baithi)
                    .ThenInclude(b => b.Kythi)
                .FirstOrDefault(c => c.BaithiId == baithiId && c.HosoId == hosoId);

            if (ketQuaThi == null) return NotFound();

            return View("~/Views/Admin/KetQua/ChiTietThi.cshtml", ketQuaThi);
        }
    }
}
