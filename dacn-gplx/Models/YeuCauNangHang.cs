using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class YeuCauNangHang
{
    public int YeucauId { get; set; }

    public string? Noidung { get; set; }

    public string? Dieukien { get; set; }

    public int? GplxId { get; set; }

    public virtual GiayPhepLaiXe? Gplx { get; set; }
}
