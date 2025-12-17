using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class KetQuaHocTap
{
    public int KetquahoctapId { get; set; }

    public int? HosoId { get; set; }

    public string? Nhanxet { get; set; }

    public int? Sobuoihoc { get; set; }

    public int? Sobuoivang { get; set; }

    public string? Sokmhoanthanh { get; set; }

    public virtual HoSoThiSinh? Hoso { get; set; }

    public virtual ChiTietKetQuaHocTap? Ketquahoctap { get; set; }
}
