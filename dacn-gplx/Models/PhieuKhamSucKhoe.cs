using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class PhieuKhamSucKhoe
{
    public int KhamsuckhoeId { get; set; }

    public string? Hieuluc { get; set; }

    public DateOnly? Thoihan { get; set; }

    public string? Khammat { get; set; }

    public string? Huyetap { get; set; }

    public decimal? Chieucao { get; set; }

    public decimal? Cannang { get; set; }

    public string? UrlAnh { get; set; }

    public virtual ICollection<AnhGksk> AnhGksks { get; set; } = new List<AnhGksk>();

    public virtual ICollection<HoSoThiSinh> HoSoThiSinhs { get; set; } = new List<HoSoThiSinh>();
}
