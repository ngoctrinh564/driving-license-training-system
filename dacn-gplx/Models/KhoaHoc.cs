using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class KhoaHoc
{
    public int KhoahocId { get; set; }

    public string? TenKhoaHoc { get; set; }

    public int? HangId { get; set; }

    public DateOnly? Ngaybatdau { get; set; }

    public DateOnly? Ngayketthuc { get; set; }

    public string? Diadiem { get; set; }

    public string? Trangthai { get; set; }

    public virtual ICollection<ChiTietKetQuaHocTap> ChiTietKetQuaHocTaps { get; set; } = new List<ChiTietKetQuaHocTap>();

    public virtual HangGplx? Hang { get; set; }
}
