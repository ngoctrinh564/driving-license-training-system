using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class HocVien
{
    public int HocvienId { get; set; }

    public string? Hoten { get; set; }

    public string? SoCmndCccd { get; set; }

    public DateOnly? Namsinh { get; set; }

    public string? Gioitinh { get; set; }

    public string? Sdt { get; set; }

    public string? Mail { get; set; }

    public int? UserId { get; set; }

    public string? AvatarUrl { get; set; }

    public virtual ICollection<HoSoThiSinh> HoSoThiSinhs { get; set; } = new List<HoSoThiSinh>();

    public virtual User? User { get; set; }
}
