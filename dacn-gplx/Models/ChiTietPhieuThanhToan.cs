using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class ChiTietPhieuThanhToan
{
    public int HosoId { get; set; }

    public int PhieuId { get; set; }

    public string? Loaiphi { get; set; }

    public string? Ghichu { get; set; }

    public virtual HoSoThiSinh Hoso { get; set; } = null!;

    public virtual PhieuThanhToan Phieu { get; set; } = null!;
}
