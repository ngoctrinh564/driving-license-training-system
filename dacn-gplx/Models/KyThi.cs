using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class KyThi
{
    public int KythiId { get; set; }

    public string? Tenkythi { get; set; }

    public string? Loaikythi { get; set; }
    public bool? TrangThai { get; set; }
    public virtual ICollection<BaiThi> BaiThis { get; set; } = new List<BaiThi>();
    public virtual ICollection<LichThi> LichThis { get; set; } = new List<LichThi>();

    public virtual ICollection<ChiTietDangKyThi> ChiTietDangKyThis { get; set; } = new List<ChiTietDangKyThi>();
}
