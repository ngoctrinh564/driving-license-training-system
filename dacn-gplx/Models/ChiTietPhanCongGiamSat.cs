using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class ChiTietPhanCongGiamSat
{
    public int BaithiId { get; set; }

    public int CanboId { get; set; }

    public DateTime? Thoigianbatdau { get; set; }

    public DateTime? Thoigianketthuc { get; set; }

    public string? Phongthi { get; set; }

    public string? Ghichu { get; set; }

    public virtual BaiThi Baithi { get; set; } = null!;

    public virtual CanBoGiamSat Canbo { get; set; } = null!;
}
