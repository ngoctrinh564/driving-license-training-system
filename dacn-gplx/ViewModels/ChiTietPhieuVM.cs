using System.ComponentModel.DataAnnotations;

namespace dacn_gplx.ViewModels
{
    public class ChiTietPhieuVM
    {
        public int PhieuId { get; set; }
        public string? Tenphieu { get; set; }
        public DateOnly? Ngaylap { get; set; }
        public decimal? Tongtien { get; set; }
        public DateOnly? Ngaynop { get; set; }

        // ===== THÔNG TIN KHÓA HỌC =====
        public int? KhoaHocId { get; set; }
        public string? TenKhoaHoc { get; set; }
        public int? HangId { get; set; }
        public string? TenHang { get; set; }
        public DateOnly? NgayBatDau { get; set; }
        public DateOnly? NgayKetThuc { get; set; }
        public string? DiaDiem { get; set; }
        public string? AnhMinhChung { get; set; }
        public string? Trangthai { get; set; }

        // Học phí = Tổng tiền của phiếu thanh toán → KHÔNG cần thêm property mới
        public decimal? HocPhi => Tongtien;

        // ===== UPLOAD MINH CHỨNG THANH TOÁN =====
        [Display(Name = "Minh chứng thanh toán")]
        public IFormFile? MinhChungThanhToan { get; set; }

        // Đường dẫn ảnh đã upload, dùng để hiển thị trong Details
        public string? AnhMinhChungUrl { get; set; }
    }
}
