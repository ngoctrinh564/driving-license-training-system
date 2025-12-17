using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace dacn_gplx.Models;

public partial class QuanLyGplxContext : DbContext
{
    public QuanLyGplxContext()
    {
    }

    public QuanLyGplxContext(DbContextOptions<QuanLyGplxContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AnhGksk> AnhGksks { get; set; }

    public virtual DbSet<BaiThi> BaiThis { get; set; }

    public virtual DbSet<CanBoGiamSat> CanBoGiamSats { get; set; }

    public virtual DbSet<ChiTietDangKyThi> ChiTietDangKyThis { get; set; }

    public virtual DbSet<ChiTietGplx> ChiTietGplxes { get; set; }

    public virtual DbSet<ChiTietKetQuaHocTap> ChiTietKetQuaHocTaps { get; set; }

    public virtual DbSet<ChiTietKetQuaThi> ChiTietKetQuaThis { get; set; }

    public virtual DbSet<ChiTietPhanCongGiamSat> ChiTietPhanCongGiamSats { get; set; }

    public virtual DbSet<ChiTietPhieuThanhToan> ChiTietPhieuThanhToans { get; set; }

    public virtual DbSet<GiayPhepLaiXe> GiayPhepLaiXes { get; set; }

    public virtual DbSet<HangGplx> HangGplxes { get; set; }

    public virtual DbSet<HoSoThiSinh> HoSoThiSinhs { get; set; }

    public virtual DbSet<HocVien> HocViens { get; set; }

    public virtual DbSet<KetQuaHocTap> KetQuaHocTaps { get; set; }

    public virtual DbSet<KhoaHoc> KhoaHocs { get; set; }

    public virtual DbSet<KyThi> KyThis { get; set; }

    public virtual DbSet<LichThi> LichThis { get; set; }

    public virtual DbSet<PhieuKhamSucKhoe> PhieuKhamSucKhoes { get; set; }

    public virtual DbSet<PhieuThanhToan> PhieuThanhToans { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<YeuCauNangHang> YeuCauNangHangs { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=haiit;Database=QuanLyGPLX;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnhGksk>(entity =>
        {
            entity.HasKey(e => e.AnhId).HasName("PK__ANH_GKSK__1D6DFBE39B2FEC64");

            entity.ToTable("ANH_GKSK");

            entity.Property(e => e.AnhId).HasColumnName("ANH_ID");
            entity.Property(e => e.KhamsuckhoeId).HasColumnName("KHAMSUCKHOE_ID");
            entity.Property(e => e.Urlanh)
                .HasMaxLength(300)
                .HasColumnName("URLANH");

            entity.HasOne(d => d.Khamsuckhoe).WithMany(p => p.AnhGksks)
                .HasForeignKey(d => d.KhamsuckhoeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ANH_GKSK__KHAMSU__02FC7413");
        });

        modelBuilder.Entity<BaiThi>(entity =>
        {
            entity.HasKey(e => e.BaithiId).HasName("PK__BAI_THI__BAC1B24D71A35CBA");

            entity.ToTable("BAI_THI");

            entity.Property(e => e.BaithiId).HasColumnName("BAITHI_ID");
            entity.Property(e => e.KythiId).HasColumnName("KYTHI_ID");
            entity.Property(e => e.Loaibaithi)
                .HasMaxLength(50)
                .HasColumnName("LOAIBAITHI");
            entity.Property(e => e.Mota)
                .HasMaxLength(255)
                .HasColumnName("MOTA");
            entity.Property(e => e.Tenbaithi)
                .HasMaxLength(100)
                .HasColumnName("TENBAITHI");

            entity.HasOne(d => d.Kythi).WithMany(p => p.BaiThis)
                .HasForeignKey(d => d.KythiId)
                .HasConstraintName("FK__BAI_THI__KYTHI_I__693CA210");
        });

        modelBuilder.Entity<CanBoGiamSat>(entity =>
        {
            entity.HasKey(e => e.CanboId).HasName("PK__CAN_BO_G__B60AD93518F59A11");

            entity.ToTable("CAN_BO_GIAM_SAT");

            entity.Property(e => e.CanboId).HasColumnName("CANBO_ID");
            entity.Property(e => e.Diachi)
                .HasMaxLength(255)
                .HasColumnName("DIACHI");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Gioitinh)
                .HasMaxLength(10)
                .HasColumnName("GIOITINH");
            entity.Property(e => e.Hoten)
                .HasMaxLength(100)
                .HasColumnName("HOTEN");
            entity.Property(e => e.Ngaysinh).HasColumnName("NGAYSINH");
            entity.Property(e => e.Sdt)
                .HasMaxLength(15)
                .HasColumnName("SDT");
        });

        modelBuilder.Entity<ChiTietDangKyThi>(entity =>
        {
            entity.HasKey(e => new { e.KythiId, e.HosoId })
                .HasName("PK_CHI_TIET_DANG_KY_THI");

            entity.ToTable("CHI_TIET_DANG_KY_THI");

            entity.Property(e => e.KythiId).HasColumnName("KYTHI_ID");
            entity.Property(e => e.HosoId).HasColumnName("HOSO_ID");
            entity.Property(e => e.Thoigiandangky)
                .HasColumnType("datetime")
                .HasColumnName("THOIGIANDANGKY");

            entity.HasOne(d => d.Hoso)
                .WithMany(p => p.ChiTietDangKyThis)
                .HasForeignKey(d => d.HosoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTDK_HOSO");

            entity.HasOne(d => d.Kythi)
                .WithMany(p => p.ChiTietDangKyThis)
                .HasForeignKey(d => d.KythiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTDK_KYTHI");
        });

        modelBuilder.Entity<ChiTietGplx>(entity =>
        {
            entity.HasKey(e => new { e.HangId, e.GplxId }).HasName("PK__CHI_TIET__E89385C1B484890E");

            entity.ToTable("CHI_TIET_GPLX");

            entity.Property(e => e.HangId).HasColumnName("HANG_ID");
            entity.Property(e => e.GplxId).HasColumnName("GPLX_ID");
            entity.Property(e => e.NgayCapCtgp).HasColumnName("NGAY_CAP_CTGP");

            entity.HasOne(d => d.Gplx).WithMany(p => p.ChiTietGplxes)
                .HasForeignKey(d => d.GplxId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHI_TIET___GPLX___5165187F");

            entity.HasOne(d => d.Hang).WithMany(p => p.ChiTietGplxes)
                .HasForeignKey(d => d.HangId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHI_TIET___HANG___5070F446");
        });

        modelBuilder.Entity<ChiTietKetQuaHocTap>(entity =>
        {
            entity.HasKey(e => e.KetquahoctapId).HasName("PK__CHI_TIET__F5F234781CC17C89");

            entity.ToTable("CHI_TIET_KET_QUA_HOC_TAP");

            entity.Property(e => e.KetquahoctapId).HasColumnName("KETQUAHOCTAP_ID");
            entity.Property(e => e.DuongtruongKq).HasColumnName("DUONGTRUONG_KQ");
            entity.Property(e => e.KhoahocId).HasColumnName("KHOAHOC_ID");
            entity.Property(e => e.LythuyetKq).HasColumnName("LYTHUYET_KQ");
            entity.Property(e => e.MophongKq).HasColumnName("MOPHONG_KQ");
            entity.Property(e => e.SahinhKq).HasColumnName("SAHINH_KQ");

            entity.HasOne(d => d.Khoahoc).WithMany(p => p.ChiTietKetQuaHocTaps)
                .HasForeignKey(d => d.KhoahocId)
                .HasConstraintName("FK__CHI_TIET___KHOAH__59FA5E80");
        });

        modelBuilder.Entity<ChiTietKetQuaThi>(entity =>
        {
            entity.HasKey(e => new { e.BaithiId, e.HosoId }).HasName("PK__CHI_TIET__015F3F7A701F06FE");

            entity.ToTable("CHI_TIET_KET_QUA_THI");

            entity.Property(e => e.BaithiId).HasColumnName("BAITHI_ID");
            entity.Property(e => e.HosoId).HasColumnName("HOSO_ID");
            entity.Property(e => e.KetQuaDatDuoc)
                .HasMaxLength(50)
                .HasColumnName("KET_QUA_DAT_DUOC");
            entity.Property(e => e.TongDiem).HasColumnName("TONG_DIEM");

            entity.HasOne(d => d.Baithi).WithMany(p => p.ChiTietKetQuaThis)
                .HasForeignKey(d => d.BaithiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHI_TIET___BAITH__71D1E811");

            entity.HasOne(d => d.Hoso).WithMany(p => p.ChiTietKetQuaThis)
                .HasForeignKey(d => d.HosoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHI_TIET___HOSO___72C60C4A");
        });

        modelBuilder.Entity<ChiTietPhanCongGiamSat>(entity =>
        {
            entity.HasKey(e => new { e.BaithiId, e.CanboId }).HasName("PK__CHI_TIET__F1A11FDEC83C4DAC");

            entity.ToTable("CHI_TIET_PHAN_CONG_GIAM_SAT");

            entity.Property(e => e.BaithiId).HasColumnName("BAITHI_ID");
            entity.Property(e => e.CanboId).HasColumnName("CANBO_ID");
            entity.Property(e => e.Ghichu)
                .HasMaxLength(255)
                .HasColumnName("GHICHU");
            entity.Property(e => e.Phongthi)
                .HasMaxLength(100)
                .HasColumnName("PHONGTHI");
            entity.Property(e => e.Thoigianbatdau)
                .HasColumnType("datetime")
                .HasColumnName("THOIGIANBATDAU");
            entity.Property(e => e.Thoigianketthuc)
                .HasColumnType("datetime")
                .HasColumnName("THOIGIANKETTHUC");

            entity.HasOne(d => d.Baithi).WithMany(p => p.ChiTietPhanCongGiamSats)
                .HasForeignKey(d => d.BaithiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHI_TIET___BAITH__6E01572D");

            entity.HasOne(d => d.Canbo).WithMany(p => p.ChiTietPhanCongGiamSats)
                .HasForeignKey(d => d.CanboId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHI_TIET___CANBO__6EF57B66");
        });

        modelBuilder.Entity<ChiTietPhieuThanhToan>(entity =>
        {
            entity.HasKey(e => new { e.HosoId, e.PhieuId }).HasName("PK__CHI_TIET__02BFB4DB719BC7DA");

            entity.ToTable("CHI_TIET_PHIEU_THANH_TOAN");

            entity.Property(e => e.HosoId).HasColumnName("HOSO_ID");
            entity.Property(e => e.PhieuId).HasColumnName("PHIEU_ID");
            entity.Property(e => e.Ghichu)
                .HasMaxLength(255)
                .HasColumnName("GHICHU");
            entity.Property(e => e.Loaiphi)
                .HasMaxLength(100)
                .HasColumnName("LOAIPHI");

            entity.HasOne(d => d.Hoso).WithMany(p => p.ChiTietPhieuThanhToans)
                .HasForeignKey(d => d.HosoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHI_TIET___HOSO___49C3F6B7");

            entity.HasOne(d => d.Phieu).WithMany(p => p.ChiTietPhieuThanhToans)
                .HasForeignKey(d => d.PhieuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHI_TIET___PHIEU__4AB81AF0");
        });

        modelBuilder.Entity<GiayPhepLaiXe>(entity =>
        {
            entity.HasKey(e => e.GplxId).HasName("PK__GIAY_PHE__3C39E1892E5B8DB3");

            entity.ToTable("GIAY_PHEP_LAI_XE");

            entity.Property(e => e.GplxId).HasColumnName("GPLX_ID");
            entity.Property(e => e.HosoId).HasColumnName("HOSO_ID");
            entity.Property(e => e.Ngaycap).HasColumnName("NGAYCAP");
            entity.Property(e => e.Ngayhethan).HasColumnName("NGAYHETHAN");
            entity.Property(e => e.Trangthai)
                .HasMaxLength(50)
                .HasColumnName("TRANGTHAI");

            entity.HasOne(d => d.Hoso).WithMany(p => p.GiayPhepLaiXes)
                .HasForeignKey(d => d.HosoId)
                .HasConstraintName("FK__GIAY_PHEP__HOSO___4D94879B");
        });

        modelBuilder.Entity<HangGplx>(entity =>
        {
            entity.HasKey(e => e.HangId).HasName("PK__HANG_GPL__6B501BD989B5FC30");

            entity.ToTable("HANG_GPLX");

            entity.Property(e => e.HangId).HasColumnName("HANG_ID");
            entity.Property(e => e.Hocphi)
    .HasColumnType("decimal(18,2)")
    .HasColumnName("HOCPHI");
            entity.Property(e => e.Loaiphuongtien)
                .HasMaxLength(100)
                .HasColumnName("LOAIPHUONGTIEN");
            entity.Property(e => e.Mota)
                .HasMaxLength(255)
                .HasColumnName("MOTA");
            entity.Property(e => e.Tenhang)
                .HasMaxLength(20)
                .HasColumnName("TENHANG");
            entity.Property(e => e.Thoihanlythuyet).HasColumnName("THOIHANLYTHUYET");
            entity.Property(e => e.Thoihanthuchanh).HasColumnName("THOIHANTHUCHANH");
        });

        modelBuilder.Entity<HoSoThiSinh>(entity =>
        {
            entity.HasKey(e => e.HosoId).HasName("PK__HO_SO_TH__B9E8D37A178F863E");

            entity.ToTable("HO_SO_THI_SINH");

            entity.Property(e => e.HosoId).HasColumnName("HOSO_ID");
            entity.Property(e => e.Ghichu)
                .HasMaxLength(255)
                .HasColumnName("GHICHU");
            entity.Property(e => e.HangId).HasColumnName("HANG_ID");
            entity.Property(e => e.HocvienId).HasColumnName("HOCVIEN_ID");
            entity.Property(e => e.KhamsuckhoeId).HasColumnName("KHAMSUCKHOE_ID");
            entity.Property(e => e.Ngaydangky).HasColumnName("NGAYDANGKY");
            entity.Property(e => e.Tenhoso)
                .HasMaxLength(100)
                .HasColumnName("TENHOSO");
            entity.Property(e => e.Trangthai)
                .HasMaxLength(50)
                .HasColumnName("TRANGTHAI");

            entity.HasOne(d => d.Hang).WithMany(p => p.HoSoThiSinhs)
                .HasForeignKey(d => d.HangId)
                .HasConstraintName("FK__HO_SO_THI__HANG___44FF419A");

            entity.HasOne(d => d.Hocvien).WithMany(p => p.HoSoThiSinhs)
                .HasForeignKey(d => d.HocvienId)
                .HasConstraintName("FK__HO_SO_THI__HOCVI__4316F928");

            entity.HasOne(d => d.Khamsuckhoe).WithMany(p => p.HoSoThiSinhs)
                .HasForeignKey(d => d.KhamsuckhoeId)
                .HasConstraintName("FK__HO_SO_THI__KHAMS__440B1D61");
        });

        modelBuilder.Entity<HocVien>(entity =>
        {
            entity.HasKey(e => e.HocvienId).HasName("PK__HOC_VIEN__851EDF58B40EBBCF");

            entity.ToTable("HOC_VIEN");

            entity.Property(e => e.HocvienId).HasColumnName("HOCVIEN_ID");
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.Gioitinh)
                .HasMaxLength(10)
                .HasColumnName("GIOITINH");
            entity.Property(e => e.Hoten)
                .HasMaxLength(100)
                .HasColumnName("HOTEN");
            entity.Property(e => e.Mail).HasMaxLength(50);
            entity.Property(e => e.Namsinh).HasColumnName("NAMSINH");
            entity.Property(e => e.Sdt)
                .HasMaxLength(15)
                .HasColumnName("SDT");
            entity.Property(e => e.SoCmndCccd)
                .HasMaxLength(20)
                .HasColumnName("SO_CMND_CCCD");
            entity.Property(e => e.UserId).HasColumnName("USER_ID");

            entity.HasOne(d => d.User).WithMany(p => p.HocViens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__HOC_VIEN__USER_I__3C69FB99");
        });

        modelBuilder.Entity<KetQuaHocTap>(entity =>
        {
            entity.HasKey(e => e.KetquahoctapId).HasName("PK__KET_QUA___F5F2347827C4F0F9");

            entity.ToTable("KET_QUA_HOC_TAP");

            entity.Property(e => e.KetquahoctapId)
                .ValueGeneratedNever()
                .HasColumnName("KETQUAHOCTAP_ID");
            entity.Property(e => e.HosoId).HasColumnName("HOSO_ID");
            entity.Property(e => e.Nhanxet)
                .HasMaxLength(255)
                .HasColumnName("NHANXET");
            entity.Property(e => e.Sobuoihoc).HasColumnName("SOBUOIHOC");
            entity.Property(e => e.Sobuoivang).HasColumnName("SOBUOIVANG");
            entity.Property(e => e.Sokmhoanthanh)
                .HasMaxLength(100)
                .HasColumnName("SOKMHOANTHANH");

            entity.HasOne(d => d.Hoso).WithMany(p => p.KetQuaHocTaps)
                .HasForeignKey(d => d.HosoId)
                .HasConstraintName("FK__KET_QUA_H__HOSO___5DCAEF64");

            entity.HasOne(d => d.Ketquahoctap).WithOne(p => p.KetQuaHocTap)
                .HasForeignKey<KetQuaHocTap>(d => d.KetquahoctapId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KET_QUA_H__KETQU__5CD6CB2B");
        });

        modelBuilder.Entity<KhoaHoc>(entity =>
        {
            entity.HasKey(e => e.KhoahocId).HasName("PK__KHOA_HOC__9E83DE94855B14F0");

            entity.ToTable("KHOA_HOC", tb => tb.HasTrigger("TRG_AUTO_TENKHOAHOC"));  

            entity.Property(e => e.KhoahocId).HasColumnName("KHOAHOC_ID");
            entity.Property(e => e.Diadiem)
                .HasMaxLength(255);

            entity.Property(e => e.TenKhoaHoc)
                .HasMaxLength(200);

            entity.Property(e => e.HangId).HasColumnName("HANG_ID");
            entity.Property(e => e.Ngaybatdau).HasColumnName("NGAYBATDAU");
            entity.Property(e => e.Ngayketthuc).HasColumnName("NGAYKETTHUC");
            entity.Property(e => e.Trangthai)
                .HasMaxLength(50)
                .HasColumnName("TRANGTHAI");

            entity.HasOne(d => d.Hang).WithMany(p => p.KhoaHocs)
                .HasForeignKey(d => d.HangId)
                .HasConstraintName("FK__KHOA_HOC__HANG_I__571DF1D5");
        });


        modelBuilder.Entity<KyThi>(entity =>
        {
            entity.HasKey(e => e.KythiId).HasName("PK__KY_THI__6C9B16EAFB107C4E");

            entity.ToTable("KY_THI");

            entity.Property(e => e.KythiId).HasColumnName("KYTHI_ID");
            entity.Property(e => e.Loaikythi)
                .HasMaxLength(50)
                .HasColumnName("LOAIKYTHI");
            entity.Property(e => e.Tenkythi)
                .HasMaxLength(100)
                .HasColumnName("TENKYTHI");
        });

        modelBuilder.Entity<LichThi>(entity =>
        {
            entity.HasKey(e => e.LichthiId)
                .HasName("PK_LICH_THI");

            entity.ToTable("LICH_THI");

            entity.Property(e => e.LichthiId).HasColumnName("LICHTHI_ID");
            entity.Property(e => e.Diadiem).HasMaxLength(255).HasColumnName("DIADIEM");
            entity.Property(e => e.Thoigianthi).HasColumnType("datetime").HasColumnName("THOIGIANTHI");
            entity.Property(e => e.KythiId).HasColumnName("KYTHI_ID");

            entity.HasOne(d => d.Kythi)
                .WithMany(p => p.LichThis)
                .HasForeignKey(d => d.KythiId)
                .HasConstraintName("FK_LICHTHI_KYTHI");
        });

        modelBuilder.Entity<PhieuKhamSucKhoe>(entity =>
        {
            entity.HasKey(e => e.KhamsuckhoeId).HasName("PK__PHIEU_KH__6FA22F23447C3B50");

            entity.ToTable("PHIEU_KHAM_SUC_KHOE");

            entity.Property(e => e.KhamsuckhoeId).HasColumnName("KHAMSUCKHOE_ID");
            entity.Property(e => e.Cannang)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("CANNANG");
            entity.Property(e => e.Chieucao)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("CHIEUCAO");
            entity.Property(e => e.Hieuluc)
                .HasMaxLength(50)
                .HasColumnName("HIEULUC");
            entity.Property(e => e.Huyetap)
                .HasMaxLength(50)
                .HasColumnName("HUYETAP");
            entity.Property(e => e.Khammat)
                .HasMaxLength(50)
                .HasColumnName("KHAMMAT");
            entity.Property(e => e.Thoihan).HasColumnName("THOIHAN");
            entity.Property(e => e.UrlAnh).HasMaxLength(500);
        });

        modelBuilder.Entity<PhieuThanhToan>(entity =>
        {
            entity.HasKey(e => e.PhieuId).HasName("PK__PHIEU_TH__B5767A1AD28F852F");

            entity.ToTable("PHIEU_THANH_TOAN");

            entity.Property(e => e.PhieuId).HasColumnName("PHIEU_ID");
            entity.Property(e => e.Ngaylap).HasColumnName("NGAYLAP");
            entity.Property(e => e.Ngaynop).HasColumnName("NGAYNOP");
            entity.Property(e => e.Tenphieu)
                .HasMaxLength(100)
                .HasColumnName("TENPHIEU");
            entity.Property(e => e.Tongtien)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("TONGTIEN");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__ROLE__5AC4D22252BA32C2");

            entity.ToTable("ROLE");

            entity.Property(e => e.RoleId).HasColumnName("ROLE_ID");
            entity.Property(e => e.Mota)
                .HasMaxLength(255)
                .HasColumnName("MOTA");
            entity.Property(e => e.Rolename)
                .HasMaxLength(100)
                .HasColumnName("ROLENAME");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__USER__F3BEEBFFAEF9399F");

            entity.ToTable("USER");

            entity.Property(e => e.UserId).HasColumnName("USER_ID");
            entity.Property(e => e.Isactive).HasColumnName("ISACTIVE");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("PASSWORD");
            entity.Property(e => e.RoleId).HasColumnName("ROLE_ID");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("USERNAME");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__USER__ROLE_ID__398D8EEE");
        });

        modelBuilder.Entity<YeuCauNangHang>(entity =>
        {
            entity.HasKey(e => e.YeucauId).HasName("PK__YEU_CAU___1EBF362A3CA6A16B");

            entity.ToTable("YEU_CAU_NANG_HANG");

            entity.Property(e => e.YeucauId).HasColumnName("YEUCAU_ID");
            entity.Property(e => e.Dieukien)
                .HasMaxLength(255)
                .HasColumnName("DIEUKIEN");
            entity.Property(e => e.GplxId).HasColumnName("GPLX_ID");
            entity.Property(e => e.Noidung)
                .HasMaxLength(255)
                .HasColumnName("NOIDUNG");

            entity.HasOne(d => d.Gplx).WithMany(p => p.YeuCauNangHangs)
                .HasForeignKey(d => d.GplxId)
                .HasConstraintName("FK__YEU_CAU_N__GPLX___5441852A");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
