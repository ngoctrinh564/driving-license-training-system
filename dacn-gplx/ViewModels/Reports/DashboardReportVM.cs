using System;
using System.Collections.Generic;

namespace dacn_gplx.ViewModels.Reports
{
    public class DashboardReportVM
    {
        // Filter info (để hiển thị lại input)
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Summary cards
        public decimal TongDoanhThu { get; set; }
        public int SoThangCoDoanhThu { get; set; }
        public int SoHangGplx { get; set; }
        public decimal DoanhThuTrungBinhThang { get; set; }

        // Extra KPIs
        public int SoNguoiMoi { get; set; }        // theo HoSoThiSinh.Ngaydangky distinct HocvienId
        public int SoKhoaHocMoi { get; set; }      // theo KhoaHoc.Ngaybatdau
        public int SoKyThiMoi { get; set; }        // theo LichThi.Thoigianthi distinct KythiId

        // Charts - Overview (Revenue)
        public List<ChartPointVM> RevenueByMonth { get; set; } = new();

        // Charts - Users tab
        public List<ChartPointVM> NewUsersByMonth { get; set; } = new();

        // Charts - Courses tab
        public List<ChartPointVM> NewCoursesByMonth { get; set; } = new();
        public List<PieSliceVM> CoursesByHang { get; set; } = new();

        // Charts - Exams tab
        public List<ChartPointVM> NewExamsByMonth { get; set; } = new();
        public List<PieSliceVM> ExamsByType { get; set; } = new();
    }
}
