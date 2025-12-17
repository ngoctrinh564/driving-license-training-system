namespace dacn_gplx.ViewModels
{
    public class DashboardViewModel
    {
        // KPI
        public int TongHocVien { get; set; }
        public int TongHoSo { get; set; }
        public int KyThiSapToi { get; set; }
        public decimal DoanhThuThang { get; set; }

        // Chart
        public List<string> Thang { get; set; } = new();
        public List<int> SoHoSoTheoThang { get; set; } = new();
        public List<decimal> DoanhThuTheoThang { get; set; } = new();
    }
}
