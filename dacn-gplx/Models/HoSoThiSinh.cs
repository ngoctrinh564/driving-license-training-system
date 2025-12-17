using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace dacn_gplx.Models;

public partial class HoSoThiSinh
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int HosoId { get; set; }

    public int? HocvienId { get; set; }

    public string? Tenhoso { get; set; }
    public DateOnly? Ngaydangky { get; set; }

    public string? Trangthai { get; set; }

    public string? Ghichu { get; set; }

    public int? KhamsuckhoeId { get; set; }

    public int? HangId { get; set; }

    public virtual ICollection<ChiTietDangKyThi> ChiTietDangKyThis { get; set; } = new List<ChiTietDangKyThi>();

    public virtual ICollection<ChiTietKetQuaThi> ChiTietKetQuaThis { get; set; } = new List<ChiTietKetQuaThi>();

    public virtual ICollection<ChiTietPhieuThanhToan> ChiTietPhieuThanhToans { get; set; } = new List<ChiTietPhieuThanhToan>();

    public virtual ICollection<GiayPhepLaiXe> GiayPhepLaiXes { get; set; } = new List<GiayPhepLaiXe>();

    public virtual HangGplx? Hang { get; set; }

    public virtual HocVien? Hocvien { get; set; }

    public virtual ICollection<KetQuaHocTap> KetQuaHocTaps { get; set; } = new List<KetQuaHocTap>();

    public virtual PhieuKhamSucKhoe? Khamsuckhoe { get; set; }
}
