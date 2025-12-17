using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class HangGplx
{
    public int HangId { get; set; }

    public string? Tenhang { get; set; }

    public string? Mota { get; set; }

    public string? Loaiphuongtien { get; set; }

    public int? Thoihanlythuyet { get; set; }

    public int? Thoihanthuchanh { get; set; }

    public decimal? Hocphi { get; set; }

    public virtual ICollection<ChiTietGplx> ChiTietGplxes { get; set; } = new List<ChiTietGplx>();

    public virtual ICollection<HoSoThiSinh> HoSoThiSinhs { get; set; } = new List<HoSoThiSinh>();

    public virtual ICollection<KhoaHoc> KhoaHocs { get; set; } = new List<KhoaHoc>();
}
