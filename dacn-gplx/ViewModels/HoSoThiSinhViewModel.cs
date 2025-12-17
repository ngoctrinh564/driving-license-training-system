using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace dacn_gplx.ViewModels
{
    public class HoSoThiSinhViewModel
    {
        public int HosoId { get; set; }
        public int HocvienId { get; set; }

        public string? Tenhoso { get; set; }
        public DateOnly? Ngaydangky { get; set; }
        public string? Trangthai { get; set; }
        public string? Ghichu { get; set; }

        public int? HangId { get; set; }
        public string? TenHang { get; set; }

        public int? KhamsuckhoeId { get; set; }

        public bool ChoPhepChinhSua { get; set; }
        public bool ChoPhepXoa { get; set; }

        // ====== THÔNG TIN GIẤY KHÁM SỨC KHỎE ======
        public string? Hieuluc { get; set; }
        public DateOnly? Thoihan { get; set; }
        public string? Khammat { get; set; }
        public string? KhamMatTrai { get; set; }
        public string? KhamMatPhai { get; set; }

        public string? Huyetap { get; set; }
        public decimal? Chieucao { get; set; }
        public decimal? Cannang { get; set; }

        // ====== ẢNH GKSK ======
        // Ảnh đã tồn tại (dùng cho hiển thị + map Id để xóa)
        public List<string> AnhGkskUrls { get; set; } = new();
        public List<int> AnhGkskIds { get; set; } = new();

        // File upload mới
        public IFormFile[]? AnhGkskFiles { get; set; }

        // Id ảnh cần xóa trong Edit
        public List<int>? AnhGkskDeleteIds { get; set; }
    }
}
