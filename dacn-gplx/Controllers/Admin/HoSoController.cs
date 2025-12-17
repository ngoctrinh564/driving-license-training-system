using dacn_gplx.Models;
using dacn_gplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dacn_gplx.Controllers.Admin
{
    public class HoSoController : Controller
    {
        private readonly QuanLyGplxContext _context;

        public HoSoController(QuanLyGplxContext context)
        {
            _context = context;
        }

        // ============================
        // 1) DANH SÁCH + LỌC
        // ============================
        // GET: HoSo/Index
        public async Task<IActionResult> Index(string? cccd, string? ten, string? hang, string? trangthai)
        {
            var query = _context.HoSoThiSinhs
                .Include(h => h.Hocvien)
                .Include(h => h.Hang)
                .AsQueryable();

            // 1. Lọc CCCD
            if (!string.IsNullOrWhiteSpace(cccd))
            {
                query = query.Where(h =>
                    h.Hocvien != null &&
                    h.Hocvien.SoCmndCccd != null &&
                    h.Hocvien.SoCmndCccd.Contains(cccd)
                );
            }

            // 2. Lọc theo tên học viên
            if (!string.IsNullOrWhiteSpace(ten))
            {
                query = query.Where(h =>
                    h.Hocvien != null &&
                    h.Hocvien.Hoten != null &&
                    h.Hocvien.Hoten.Contains(ten)
                );
            }

            // 3. Lọc theo hạng GPLX
            if (!string.IsNullOrWhiteSpace(hang))
            {
                query = query.Where(h =>
                    h.Hang != null &&
                    h.Hang.Tenhang == hang
                );
            }

            // 4. Lọc theo trạng thái
            if (!string.IsNullOrWhiteSpace(trangthai))
            {
                query = query.Where(h => h.Trangthai == trangthai);
            }

            // 5. Dựng ViewModel cho Index
            var model = await query
                .Select(h => new HoSoIndexViewModel
                {
                    HosoId = h.HosoId,
                    TenHoSo = h.Tenhoso,
                    NgayDangKy = h.Ngaydangky,
                    TrangThai = h.Trangthai,

                    HocVienId = h.Hocvien != null ? h.Hocvien.HocvienId : 0,
                    HoTen = h.Hocvien != null ? h.Hocvien.Hoten : string.Empty,
                    SoCmndCccd = h.Hocvien != null ? h.Hocvien.SoCmndCccd : string.Empty,

                    TenHang = h.Hang != null ? h.Hang.Tenhang : string.Empty
                })
                .OrderByDescending(h => h.HosoId)
                .ToListAsync();

            // 6. Gửi lại giá trị filter cho View
            ViewBag.CCCD = cccd;
            ViewBag.Ten = ten;
            ViewBag.Hang = hang;
            ViewBag.TrangThai = trangthai;

            return View("~/Views/Admin/HoSo/Index.cshtml", model);
        }

        // ============================
        // 2) XEM CHI TIẾT HỒ SƠ
        // ============================
        // GET: HoSo/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var hoso = await _context.HoSoThiSinhs
                .Include(h => h.Hocvien)
                .Include(h => h.Hang)
                .Include(h => h.Khamsuckhoe)
                    .ThenInclude(pk => pk.AnhGksks)   // ⬅ THÊM DÒNG NÀY
                .FirstOrDefaultAsync(h => h.HosoId == id);

            if (hoso == null)
                return NotFound();

            return View("~/Views/Admin/HoSo/Details.cshtml", hoso);
        }


        // ============================
        // 3) DUYỆT HỒ SƠ
        // ============================
        // GET: HoSo/Approve/5
        public async Task<IActionResult> Approve(int id)
        {
            var hoso = await _context.HoSoThiSinhs.FindAsync(id);
            if (hoso == null)
                return NotFound();

            hoso.Trangthai = "Đã duyệt";
            await _context.SaveChangesAsync();

            // Sau khi duyệt xong quay lại danh sách
            return RedirectToAction(nameof(Index));
        }

        // ============================
        // 4) TỪ CHỐI HỒ SƠ
        // ============================
        // GET: HoSo/Reject/5
        public async Task<IActionResult> Reject(int id)
        {
            var hoso = await _context.HoSoThiSinhs.FindAsync(id);
            if (hoso == null)
                return NotFound();

            hoso.Trangthai = "Từ chối";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
