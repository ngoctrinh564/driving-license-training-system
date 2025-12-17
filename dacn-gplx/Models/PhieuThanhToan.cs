using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class PhieuThanhToan
{
    public int PhieuId { get; set; }

    public string? Tenphieu { get; set; }

    public DateOnly? Ngaylap { get; set; }

    public decimal? Tongtien { get; set; }

    public DateOnly? Ngaynop { get; set; }
    // === Thêm mới ===
    public string? AnhMinhChung { get; set; }

    public string? Trangthai { get; set; }
    public virtual ICollection<ChiTietPhieuThanhToan> ChiTietPhieuThanhToans { get; set; } = new List<ChiTietPhieuThanhToan>();
}
