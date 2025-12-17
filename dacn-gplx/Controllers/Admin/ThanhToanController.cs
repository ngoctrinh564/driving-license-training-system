using dacn_gplx.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace dacn_gplx.Controllers.Admin
{
    public class ThanhToanController : Controller
    {
        private readonly QuanLyGplxContext _context;

        public ThanhToanController(QuanLyGplxContext context)
        {
            _context = context;
        }

        // ============================================
        // ⭐ 1) DANH SÁCH PHIẾU THANH TOÁN
        // ============================================
        public async Task<IActionResult> Index(string keyword, DateOnly? fromDate, DateOnly? toDate)
        {
            var query = _context.PhieuThanhToans.AsQueryable();

            // Tìm theo mã phiếu hoặc tên phiếu
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p =>
                    p.Tenphieu.Contains(keyword) ||
                    p.PhieuId.ToString().Contains(keyword));
            }

            // Lọc khoảng ngày lập phiếu
            if (fromDate.HasValue)
            {
                query = query.Where(p => p.Ngaylap >= fromDate);
            }
            if (toDate.HasValue)
            {
                query = query.Where(p => p.Ngaylap <= toDate);
            }

            var data = await query
                .OrderByDescending(p => p.Ngaylap)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View("~/Views/Admin/ThanhToan/Index.cshtml", data);
        }


        // ============================================
        // ⭐ 2) CHI TIẾT PHIẾU THANH TOÁN
        // ============================================
        public async Task<IActionResult> Details(int id)
        {
            // Lấy Phiếu + Chi tiết + Hồ sơ học viên
            var phieu = await _context.PhieuThanhToans
                .Include(p => p.ChiTietPhieuThanhToans)
                    .ThenInclude(ct => ct.Hoso)
                .FirstOrDefaultAsync(p => p.PhieuId == id);

            if (phieu == null)
                return NotFound();

            return View("~/Views/Admin/ThanhToan/Details.cshtml", phieu);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTrangThai(int PhieuId, string Trangthai)
        {
            var phieu = await _context.PhieuThanhToans.FindAsync(PhieuId);
            if (phieu == null)
                return NotFound();

            // Cập nhật trạng thái
            phieu.Trangthai = Trangthai;

            // Nếu trạng thái là "Đã Xác Nhận", có thể cập nhật Ngày nộp
            if (Trangthai == "Đã Xác Nhận" && phieu.Ngaynop == null)
            {
                phieu.Ngaynop = DateOnly.FromDateTime(DateTime.Now);
            }

            await _context.SaveChangesAsync();


            return RedirectToAction("Index");
        }

    }
}
