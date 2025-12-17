using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class ChiTietKetQuaThi
{
    public int BaithiId { get; set; }

    public int HosoId { get; set; }

    public string? KetQuaDatDuoc { get; set; }

    public double? TongDiem { get; set; }

    public virtual BaiThi Baithi { get; set; } = null!;

    public virtual HoSoThiSinh Hoso { get; set; } = null!;
}
