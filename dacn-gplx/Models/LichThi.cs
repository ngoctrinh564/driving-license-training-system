using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class LichThi
{
    public int LichthiId { get; set; }

    public DateTime? Thoigianthi { get; set; }

    public string? Diadiem { get; set; }

    public int? KythiId { get; set; }

    public virtual KyThi? Kythi { get; set; }
}
