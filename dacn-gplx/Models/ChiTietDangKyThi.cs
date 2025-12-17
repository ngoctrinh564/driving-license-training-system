using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class ChiTietDangKyThi
{
    public int KythiId { get; set; }

    public int HosoId { get; set; }

    public DateTime? Thoigiandangky { get; set; }

    public virtual HoSoThiSinh Hoso { get; set; } = null!;

    public virtual KyThi Kythi { get; set; } = null!;
}
