using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class GiayPhepLaiXe
{
    public int GplxId { get; set; }

    public DateOnly? Ngaycap { get; set; }

    public DateOnly? Ngayhethan { get; set; }

    public string? Trangthai { get; set; }

    public int? HosoId { get; set; }

    public virtual ICollection<ChiTietGplx> ChiTietGplxes { get; set; } = new List<ChiTietGplx>();

    public virtual HoSoThiSinh? Hoso { get; set; }

    public virtual ICollection<YeuCauNangHang> YeuCauNangHangs { get; set; } = new List<YeuCauNangHang>();
}
