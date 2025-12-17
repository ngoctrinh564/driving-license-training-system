using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class BaiThi
{
    public int BaithiId { get; set; }

    public string? Tenbaithi { get; set; }

    public string? Mota { get; set; }

    public string? Loaibaithi { get; set; }

    public int? KythiId { get; set; }

    public virtual ICollection<ChiTietKetQuaThi> ChiTietKetQuaThis { get; set; } = new List<ChiTietKetQuaThi>();

    public virtual ICollection<ChiTietPhanCongGiamSat> ChiTietPhanCongGiamSats { get; set; } = new List<ChiTietPhanCongGiamSat>();

    public virtual KyThi? Kythi { get; set; }
}
