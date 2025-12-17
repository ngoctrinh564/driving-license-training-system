using System;

namespace dacn_gplx.ViewModels
{
    public class HoSoIndexViewModel
    {
        public int HosoId { get; set; }
        public string? TenHoSo { get; set; }
        public DateOnly? NgayDangKy { get; set; }
        public string? TrangThai { get; set; }

        // Học viên
        public int HocVienId { get; set; }
        public string? HoTen { get; set; }
        public string? SoCmndCccd { get; set; }

        // Hạng GPLX
        public string? TenHang { get; set; }
    }
}
