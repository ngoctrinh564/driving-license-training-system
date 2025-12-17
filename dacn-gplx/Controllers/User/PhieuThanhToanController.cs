using dacn_gplx.Models;
using dacn_gplx.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
public class PhieuThanhToanController : Controller
{
    private readonly QuanLyGplxContext _context;

    public PhieuThanhToanController(QuanLyGplxContext context)
    {
        _context = context;
    }

    // ============================
    // 1. DANH SÁCH PHIẾU THANH TOÁN CỦA USER
    // ============================
    public async Task<IActionResult> Index()
    {
        // Lấy UserId từ Session (tuỳ bạn dùng Cookies/Claims thì sửa lại)
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login", "Auth");

        // Tìm học viên theo UserId
        var hocvien = await _context.HocViens
            .FirstOrDefaultAsync(h => h.UserId == userId);

        if (hocvien == null)
            return Content("Không tìm thấy học viên!");

        int hocvienId = hocvien.HocvienId;

        // Lấy tất cả hồ sơ của học viên
        var hoSoIds = await _context.HoSoThiSinhs
            .Where(h => h.HocvienId == hocvienId)
            .Select(h => h.HosoId)
            .ToListAsync();

        // Lấy tất cả phiếu thanh toán qua bảng chi tiết
        var phieuList = await _context.ChiTietPhieuThanhToans
            .Where(ct => hoSoIds.Contains(ct.HosoId))
            .Select(ct => ct.Phieu)
            .Distinct()
            .ToListAsync();

        return View("~/Views/User/PhieuThanhToan/Index.cshtml", phieuList);
    }

    // ============================
    // 2. XEM CHI TIẾT PHIẾU THANH TOÁN (ĐÃ SỬA CHUẨN ERD)
    // ============================
    public async Task<IActionResult> Detail(int id)
    {
        // Lấy phiếu + chi tiết
        var phieu = await _context.PhieuThanhToans
            .Include(p => p.ChiTietPhieuThanhToans)
            .FirstOrDefaultAsync(p => p.PhieuId == id);

        if (phieu == null)
            return NotFound();

        // Nếu thiếu chi tiết
        var chiTiet = phieu.ChiTietPhieuThanhToans.FirstOrDefault();
        if (chiTiet == null)
        {
            return View("~/Views/User/PhieuThanhToan/Details.cshtml",
                new ChiTietPhieuVM
                {
                    PhieuId = phieu.PhieuId,
                    Tenphieu = phieu.Tenphieu,
                    Ngaylap = phieu.Ngaylap,
                    Tongtien = phieu.Tongtien,
                    Ngaynop = phieu.Ngaynop,
                    Trangthai = phieu.Trangthai,
                    AnhMinhChung = phieu.AnhMinhChung
                });
        }

        int hosoId = chiTiet.HosoId;

        // ============================
        // LẤY CHÍNH XÁC KHÓA HỌC CỦA LẦN ĐĂNG KÝ NÀY
        // ============================

        // 1 hồ sơ → 1 kết quả học tập
        var ketQua = await _context.KetQuaHocTaps
            .FirstOrDefaultAsync(k => k.HosoId == hosoId);

        if (ketQua == null)
        {
            return View("~/Views/User/PhieuThanhToan/Details.cshtml",
                new ChiTietPhieuVM
                {
                    PhieuId = phieu.PhieuId,
                    Tenphieu = phieu.Tenphieu,
                    Ngaylap = phieu.Ngaylap,
                    Tongtien = phieu.Tongtien,
                    Ngaynop = phieu.Ngaynop,
                    Trangthai = phieu.Trangthai,
                    AnhMinhChung = phieu.AnhMinhChung
                });
        }

        int ketQuaId = ketQua.KetquahoctapId;

        // Lấy đúng khóa học của kết quả (mỗi kết quả ứng với 1 khóa mới)
        var chiTietKQ = await _context.ChiTietKetQuaHocTaps
            .Include(c => c.Khoahoc)
                .ThenInclude(k => k.Hang)
            .Where(c => c.KetquahoctapId == ketQuaId)
            .OrderByDescending(c => c.KetquahoctapId)   // ✔️ ĐÚNG
            .FirstOrDefaultAsync();

        var khoaHoc = chiTietKQ?.Khoahoc;


        // Gửi model ra view
        var vm = new ChiTietPhieuVM
        {
            PhieuId = phieu.PhieuId,
            Tenphieu = phieu.Tenphieu,
            Ngaylap = phieu.Ngaylap,
            Tongtien = phieu.Tongtien,
            Ngaynop = phieu.Ngaynop,
            Trangthai = phieu.Trangthai,
            AnhMinhChung = phieu.AnhMinhChung,

            // Thông tin khóa học
            KhoaHocId = khoaHoc?.KhoahocId,
            TenKhoaHoc = khoaHoc?.TenKhoaHoc,
            HangId = khoaHoc?.HangId,
            TenHang = khoaHoc?.Hang?.Tenhang,
            NgayBatDau = khoaHoc?.Ngaybatdau,
            NgayKetThuc = khoaHoc?.Ngayketthuc,
            DiaDiem = khoaHoc?.Diadiem
        };

        return View("~/Views/User/PhieuThanhToan/Details.cshtml", vm);
    }



    // ============================
    // 3. THANH TOÁN PHIẾU (Cập nhật ngày nộp)
    // ============================
    [HttpPost]
    public async Task<IActionResult> ThanhToan(int id)
    {
        var phieu = await _context.PhieuThanhToans.FindAsync(id);
        if (phieu == null)
            return NotFound();

        // Nếu chưa có ảnh thì không cho xác nhận
        if (string.IsNullOrEmpty(phieu.AnhMinhChung))
        {
            TempData["Error"] = "Bạn phải tải ảnh thanh toán trước!";
            return RedirectToAction("Detail", new { id });
        }

        // Chuyển trạng thái sang CHỜ XÁC THỰC
        phieu.Trangthai = "Chờ Xác Thực";  // Bạn nhớ thêm cột này trong DB nếu chưa có
        phieu.Ngaynop = DateOnly.FromDateTime(DateTime.Now);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã gửi xác nhận thanh toán. Vui lòng chờ admin duyệt!";
        return RedirectToAction("Detail", new { id });
    }

    // ============================
    // 4. UPLOAD MINH CHỨNG THANH TOÁN
    // ============================
    [HttpPost]
    public async Task<IActionResult> UploadMinhChung(int PhieuId, IFormFile MinhChungThanhToan)
    {
        var phieu = await _context.PhieuThanhToans.FindAsync(PhieuId);
        if (phieu == null)
            return NotFound();

        if (MinhChungThanhToan != null && MinhChungThanhToan.Length > 0)
        {
            // Tạo folder nếu chưa có
            string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // Tạo tên file duy nhất
            string fileExt = Path.GetExtension(MinhChungThanhToan.FileName);
            string fileName = $"phieu_{PhieuId}_{Guid.NewGuid()}{fileExt}";
            string filePath = Path.Combine(folder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await MinhChungThanhToan.CopyToAsync(stream);
            }

            // Lưu đường dẫn tương đối vào DB
            phieu.AnhMinhChung = "/uploads/" + fileName;


            await _context.SaveChangesAsync();

            TempData["Success"] = "Upload minh chứng thành công!";
            return RedirectToAction("Detail", new { id = PhieuId });
        }

        TempData["Error"] = "Vui lòng chọn file!";
        return RedirectToAction("Detail", new { id = PhieuId });
    }
}
