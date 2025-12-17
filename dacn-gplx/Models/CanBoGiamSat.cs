using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class CanBoGiamSat
{
    public int CanboId { get; set; }

    public string? Hoten { get; set; }

    public DateOnly? Ngaysinh { get; set; }

    public string? Gioitinh { get; set; }

    public string? Diachi { get; set; }

    public string? Email { get; set; }

    public string? Sdt { get; set; }

    public virtual ICollection<ChiTietPhanCongGiamSat> ChiTietPhanCongGiamSats { get; set; } = new List<ChiTietPhanCongGiamSat>();
}
