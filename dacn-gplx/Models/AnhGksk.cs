using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class AnhGksk
{
    public int AnhId { get; set; }

    public int KhamsuckhoeId { get; set; }

    public string Urlanh { get; set; } = null!;

    public virtual PhieuKhamSucKhoe Khamsuckhoe { get; set; } = null!;
}
