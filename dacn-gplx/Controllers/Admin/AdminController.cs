using dacn_gplx.Models;
using dacn_gplx.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace dacn_gplx.Controllers.Admin
{
    public class AdminController : Controller
    {
        private readonly QuanLyGplxContext _context;

        public AdminController(QuanLyGplxContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var now = DateTime.Now;

            // ===== THÁNG HIỆN TẠI =====
            var startOfMonth = new DateOnly(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            var vm = new DashboardViewModel();

            // ===== KPI =====
            vm.TongHocVien = _context.HocViens.Count();
            vm.TongHoSo = _context.HoSoThiSinhs.Count();

            vm.KyThiSapToi = _context.LichThis
                .Count(x => x.Thoigianthi >= now);

            vm.DoanhThuThang = _context.PhieuThanhToans
                .Where(x =>
                    x.Trangthai == "Đã Thanh Toán" &&
                    x.Ngaylap != null &&
                    x.Ngaylap >= startOfMonth &&
                    x.Ngaylap < endOfMonth)
                .Sum(x => (decimal?)x.Tongtien) ?? 0;

            // =================================================
            // HỒ SƠ THEO THÁNG (CLIENT-SIDE – CHẮC CHẮN CHẠY)
            // =================================================
            var hoSoDates = _context.HoSoThiSinhs
                .Select(x => x.Ngaydangky)
                .ToList();

            var hoSoTheoThang = hoSoDates
                .Where(d => d != null)
                .Select(d => d!.Value)
                .GroupBy(d => new { d.Year, d.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    SoLuong = g.Count()
                })
                .ToList();

            foreach (var item in hoSoTheoThang)
            {
                vm.Thang.Add($"Tháng {item.Thang}/{item.Nam}");
                vm.SoHoSoTheoThang.Add(item.SoLuong);
            }

            // =================================================
            // DOANH THU THEO THÁNG (CLIENT-SIDE)
            // =================================================
            var phieuData = _context.PhieuThanhToans
                .Where(x => x.Trangthai == "Đã Thanh Toán" && x.Ngaylap != null)
                .Select(x => new
                {
                    Ngay = x.Ngaylap!.Value,
                    x.Tongtien
                })
                .ToList();

            var doanhThuTheoThang = phieuData
                .GroupBy(x => new { x.Ngay.Year, x.Ngay.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => g.Sum(x => (decimal?)x.Tongtien) ?? 0)
                .ToList();

            vm.DoanhThuTheoThang = doanhThuTheoThang;

            // GẮN VIEW ĐÚNG CẤU TRÚC
            return View("~/Views/Admin/Index.cshtml", vm);
        }
    }
}
