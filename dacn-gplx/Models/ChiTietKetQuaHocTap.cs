using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class ChiTietKetQuaHocTap
{
    public int KetquahoctapId { get; set; }

    public int? KhoahocId { get; set; }

    public bool? LythuyetKq { get; set; }

    public bool? SahinhKq { get; set; }

    public bool? DuongtruongKq { get; set; }

    public bool? MophongKq { get; set; }

    public virtual KetQuaHocTap? KetQuaHocTap { get; set; }

    public virtual KhoaHoc? Khoahoc { get; set; }
}
