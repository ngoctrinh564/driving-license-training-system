using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class ChiTietGplx
{
    public int HangId { get; set; }

    public int GplxId { get; set; }

    public DateOnly? NgayCapCtgp { get; set; }

    public virtual GiayPhepLaiXe Gplx { get; set; } = null!;

    public virtual HangGplx Hang { get; set; } = null!;
}
