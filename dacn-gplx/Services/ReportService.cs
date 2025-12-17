using dacn_gplx.Models;
using dacn_gplx.ViewModels.Reports;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace dacn_gplx.Services
{
    public class ReportService
    {
        private readonly QuanLyGplxContext _context;

        private readonly string _logoPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot/images/logo/logo.png"
        );

        public ReportService(QuanLyGplxContext context)
        {
            _context = context;
        }

        private static DateOnly? ToDateOnly(DateTime? dt)
            => dt.HasValue ? DateOnly.FromDateTime(dt.Value) : null;

        private static string FormatRange(DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue && !toDate.HasValue)
                return "Tất cả";

            if (fromDate.HasValue && toDate.HasValue)
                return $"{fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}";

            if (fromDate.HasValue)
                return $"Từ {fromDate:dd/MM/yyyy}";

            return $"Đến {toDate:dd/MM/yyyy}";
        }

        private static string TabTitle(string tab) => tab switch
        {
            "users" => "BÁO CÁO NGƯỜI DÙNG",
            "courses" => "BÁO CÁO KHÓA HỌC",
            "exams" => "BÁO CÁO KỲ THI",
            _ => "BÁO CÁO TỔNG QUAN"
        };

        // ===================== DATA =====================
        public async Task<DashboardReportVM> GetDashboardAsync(DateTime? fromDate, DateTime? toDate)
        {
            var fromDO = ToDateOnly(fromDate);
            var toDO = ToDateOnly(toDate);

            // ===================== 1) REVENUE =====================
            var phieuQuery = _context.PhieuThanhToans.AsQueryable()
                .Where(x => x.Ngaylap.HasValue);

            if (fromDO.HasValue) phieuQuery = phieuQuery.Where(x => x.Ngaylap!.Value >= fromDO.Value);
            if (toDO.HasValue) phieuQuery = phieuQuery.Where(x => x.Ngaylap!.Value <= toDO.Value);

            var tongDoanhThu = await phieuQuery.SumAsync(x => x.Tongtien);

            var revenueRaw = await phieuQuery
                .GroupBy(x => new { Y = x.Ngaylap!.Value.Year, M = x.Ngaylap!.Value.Month })
                .Select(g => new
                {
                    g.Key.Y,
                    g.Key.M,
                    TongTien = g.Sum(x => x.Tongtien)
                })
                .OrderBy(x => x.Y).ThenBy(x => x.M)
                .ToListAsync();

            var revenueByMonth = revenueRaw
                .Select(x => new ChartPointVM
                {
                    Label = $"{x.M}/{x.Y}",
                    Value = (decimal)x.TongTien
                })
                .ToList();

            int soThangCoDoanhThu = revenueByMonth.Count;
            decimal doanhThuTbThang = (decimal)(soThangCoDoanhThu > 0 ? tongDoanhThu / soThangCoDoanhThu : 0);

            // ===================== 2) USERS NEW =====================
            var hoSoQuery = _context.HoSoThiSinhs.AsQueryable()
                .Where(x => x.Ngaydangky.HasValue);

            if (fromDO.HasValue) hoSoQuery = hoSoQuery.Where(x => x.Ngaydangky!.Value >= fromDO.Value);
            if (toDO.HasValue) hoSoQuery = hoSoQuery.Where(x => x.Ngaydangky!.Value <= toDO.Value);

            var soNguoiMoi = await hoSoQuery.Select(x => x.HocvienId).Distinct().CountAsync();

            var newUsersRaw = await hoSoQuery
                .GroupBy(x => new { Y = x.Ngaydangky!.Value.Year, M = x.Ngaydangky!.Value.Month })
                .Select(g => new
                {
                    g.Key.Y,
                    g.Key.M,
                    Count = g.Select(x => x.HocvienId).Distinct().Count()
                })
                .OrderBy(x => x.Y).ThenBy(x => x.M)
                .ToListAsync();

            var newUsersByMonth = newUsersRaw
                .Select(x => new ChartPointVM { Label = $"{x.M}/{x.Y}", Value = x.Count })
                .ToList();

            // ===================== 3) COURSES =====================
            var khoaHocQuery = _context.KhoaHocs.AsQueryable()
                .Where(x => x.Ngaybatdau.HasValue);

            if (fromDO.HasValue) khoaHocQuery = khoaHocQuery.Where(x => x.Ngaybatdau!.Value >= fromDO.Value);
            if (toDO.HasValue) khoaHocQuery = khoaHocQuery.Where(x => x.Ngaybatdau!.Value <= toDO.Value);

            var soKhoaHocMoi = await khoaHocQuery.CountAsync();

            var newCoursesRaw = await khoaHocQuery
                .GroupBy(x => new { Y = x.Ngaybatdau!.Value.Year, M = x.Ngaybatdau!.Value.Month })
                .Select(g => new
                {
                    g.Key.Y,
                    g.Key.M,
                    Count = g.Count()
                })
                .OrderBy(x => x.Y).ThenBy(x => x.M)
                .ToListAsync();

            var newCoursesByMonth = newCoursesRaw
                .Select(x => new ChartPointVM { Label = $"{x.M}/{x.Y}", Value = x.Count })
                .ToList();

            // phân bố khóa học theo Hạng
            var coursesByHang = await _context.KhoaHocs
                .Include(k => k.Hang)
                .Where(x => x.Ngaybatdau.HasValue)
                .Where(x => x.Hang != null && !string.IsNullOrEmpty(x.Hang.Tenhang))
                .ToListAsync();

            if (fromDO.HasValue) coursesByHang = coursesByHang.Where(x => x.Ngaybatdau!.Value >= fromDO.Value).ToList();
            if (toDO.HasValue) coursesByHang = coursesByHang.Where(x => x.Ngaybatdau!.Value <= toDO.Value).ToList();

            var coursesByHangPie = coursesByHang
                .GroupBy(x => x.Hang!.Tenhang)
                .Select(g => new PieSliceVM { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .ToList();

            // ===================== 4) EXAMS =====================
            var lichThiQuery = _context.LichThis.AsQueryable();

            if (fromDate.HasValue) lichThiQuery = lichThiQuery.Where(x => x.Thoigianthi >= fromDate.Value);
            if (toDate.HasValue) lichThiQuery = lichThiQuery.Where(x => x.Thoigianthi <= toDate.Value);

            var soKyThiMoi = await lichThiQuery.Select(x => x.KythiId).Distinct().CountAsync();

            var newExamsRaw = await lichThiQuery
                .Where(x => x.Thoigianthi.HasValue)
                .GroupBy(x => new { Y = x.Thoigianthi!.Value.Year, M = x.Thoigianthi!.Value.Month })
                .Select(g => new
                {
                    g.Key.Y,
                    g.Key.M,
                    Count = g.Select(x => x.KythiId).Distinct().Count()
                })
                .OrderBy(x => x.Y).ThenBy(x => x.M)
                .ToListAsync();

            var newExamsByMonth = newExamsRaw
                .Select(x => new ChartPointVM { Label = $"{x.M}/{x.Y}", Value = x.Count })
                .ToList();

            var examsByType = await _context.LichThis
                .Include(l => l.Kythi)
                .ToListAsync();

            if (fromDate.HasValue) examsByType = examsByType.Where(x => x.Thoigianthi >= fromDate.Value).ToList();
            if (toDate.HasValue) examsByType = examsByType.Where(x => x.Thoigianthi <= toDate.Value).ToList();

            var examsByTypePie = examsByType
                .GroupBy(x => string.IsNullOrEmpty(x.Kythi?.Loaikythi) ? "Khác" : x.Kythi!.Loaikythi!)
                .Select(g => new PieSliceVM { Label = g.Key, Value = g.Select(x => x.KythiId).Distinct().Count() })
                .OrderByDescending(x => x.Value)
                .ToList();

            var soHang = await _context.HangGplxes.CountAsync();

            return new DashboardReportVM
            {
                FromDate = fromDate,
                ToDate = toDate,

                TongDoanhThu = (decimal)tongDoanhThu,
                SoThangCoDoanhThu = soThangCoDoanhThu,
                SoHangGplx = soHang,
                DoanhThuTrungBinhThang = doanhThuTbThang,

                SoNguoiMoi = soNguoiMoi,
                SoKhoaHocMoi = soKhoaHocMoi,
                SoKyThiMoi = soKyThiMoi,

                RevenueByMonth = revenueByMonth,

                NewUsersByMonth = newUsersByMonth,

                NewCoursesByMonth = newCoursesByMonth,
                CoursesByHang = coursesByHangPie,

                NewExamsByMonth = newExamsByMonth,
                ExamsByType = examsByTypePie
            };
        }

        // ===================== PDF =====================
        public byte[] GenerateDashboardPdf(
            DashboardReportVM model,
            DateTime? fromDate,
            DateTime? toDate,
            string tab)
        {
            var rangeText = FormatRange(fromDate, toDate);
            var title = TabTitle(tab);

            var hasLogo = File.Exists(_logoPath);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // ===== HEADER =====
                    page.Header().PaddingBottom(10).Row(row =>
                    {
                        // ===== LEFT: TITLE + TIME =====
                        row.RelativeItem().Column(col =>
                        {
                            col.Item()
                                .Text(title)
                                .FontSize(18)
                                .Bold()
                                .FontColor(Colors.Black);

                            col.Item()
                                .PaddingTop(2)
                                .Text($"Thời gian: {rangeText}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken2);
                        });

                        // ===== RIGHT: LOGO =====
                        row.ConstantItem(80)
                            .AlignRight()
                            .AlignMiddle()
                            .Element(e =>
                            {
                                if (hasLogo)
                                {
                                    e.Image(_logoPath)
                                     .FitArea(); // ✅ tự scale, không méo
                                }
                                else
                                {
                                    e.Border(1)
                                     .BorderColor(Colors.Grey.Lighten2)
                                     .AlignCenter()
                                     .AlignMiddle()
                                     .Text("LOGO")
                                     .FontSize(10)
                                     .FontColor(Colors.Grey.Darken2);
                                }
                            });
                    });

                    page.Content().Column(col =>
                    {
                        col.Spacing(12);

                        // ===== SUMMARY "CARDS" =====
                        col.Item().Element(x => BuildSummaryCards(x, model, tab));

                        col.Item().LineHorizontal(1);

                        // ===== TAB CONTENT =====
                        switch (tab)
                        {
                            case "users":
                                BuildUsersSection(col, model);
                                break;

                            case "courses":
                                BuildCoursesSection(col, model);
                                break;

                            case "exams":
                                BuildExamsSection(col, model);
                                break;

                            default:
                                BuildOverviewSection(col, model);
                                break;
                        }
                    });

                    page.Footer().AlignCenter()
                        .Text($"Xuất lúc {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken2);
                });
            });

            return document.GeneratePdf();
        }

        // ===== UI Helpers =====

        private void BuildSummaryCards(IContainer container, DashboardReportVM model, string tab)
        {
            container.Row(row =>
            {
                row.Spacing(10);

                if (tab == "overview" || tab == "courses")
                    row.RelativeItem().Element(e => Card(e, "Tổng doanh thu", $"{model.TongDoanhThu:N0} VND"));

                if (tab == "overview" || tab == "users")
                    row.RelativeItem().Element(e => Card(e, "Người mới", model.SoNguoiMoi.ToString()));

                if (tab == "overview" || tab == "courses")
                    row.RelativeItem().Element(e => Card(e, "Khóa học mới", model.SoKhoaHocMoi.ToString()));

                if (tab == "overview" || tab == "exams")
                    row.RelativeItem().Element(e => Card(e, "Kỳ thi mới", model.SoKyThiMoi.ToString()));
            });
        }

        private static void Card(IContainer container, string title, string value)
        {
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.Grey.Lighten5)
                .Padding(8)
                .Column(col =>
                {
                    col.Item().Text(title).FontSize(9).FontColor(Colors.Grey.Darken2);
                    col.Item().Text(value).FontSize(14).Bold();
                });
        }

        private static void BuildOverviewSection(ColumnDescriptor col, DashboardReportVM model)
        {
            col.Item().Text("Doanh thu theo tháng").Bold().FontSize(13);

            col.Item().Element(c => SimpleBarChart(c, model.RevenueByMonth, maxBars: 10));

            col.Item().Text("Bảng chi tiết doanh thu").Bold().FontSize(12);

            col.Item().Element(c => BuildTable(c, "Tháng", "Doanh thu (VND)", model.RevenueByMonth, isMoney: true));
        }

        private static void BuildUsersSection(ColumnDescriptor col, DashboardReportVM model)
        {
            col.Item().Text("Người dùng mới theo tháng").Bold().FontSize(13);

            col.Item().Element(c => SimpleBarChart(c, model.NewUsersByMonth, maxBars: 10));

            col.Item().Text("Bảng chi tiết người dùng mới").Bold().FontSize(12);
            col.Item().Element(c => BuildTable(c, "Tháng", "Người mới", model.NewUsersByMonth, isMoney: false));
        }

        private static void BuildCoursesSection(ColumnDescriptor col, DashboardReportVM model)
        {
            col.Item().Text("Khóa học mới theo tháng").Bold().FontSize(13);

            col.Item().Element(c => SimpleBarChart(c, model.NewCoursesByMonth, maxBars: 10));

            col.Item().Text("Bảng chi tiết khóa học mới").Bold().FontSize(12);
            col.Item().Element(c => BuildTable(c, "Tháng", "Số khóa học", model.NewCoursesByMonth, isMoney: false));

            col.Item().Text("Phân bố khóa học theo hạng").Bold().FontSize(13);

            col.Item().Element(c => SimplePieLikeList(c, model.CoursesByHang));
        }

        private static void BuildExamsSection(ColumnDescriptor col, DashboardReportVM model)
        {
            col.Item().Text("Kỳ thi mới theo tháng").Bold().FontSize(13);

            col.Item().Element(c => SimpleBarChart(c, model.NewExamsByMonth, maxBars: 10));

            col.Item().Text("Bảng chi tiết kỳ thi mới").Bold().FontSize(12);
            col.Item().Element(c => BuildTable(c, "Tháng", "Số kỳ thi", model.NewExamsByMonth, isMoney: false));

            col.Item().Text("Phân bố kỳ thi theo loại").Bold().FontSize(13);
            col.Item().Element(c => SimplePieLikeList(c, model.ExamsByType));
        }

        // ===== Chart / Table blocks (QuestPDF only) =====

        private static void BuildTable(
            IContainer container,
            string col1,
            string col2,
            List<ChartPointVM> data,
            bool isMoney)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.RelativeColumn();
                });

                table.Header(h =>
                {
                    h.Cell().Element(CellHeader).Text(col1);
                    h.Cell().Element(CellHeader).AlignRight().Text(col2);
                });

                foreach (var item in data.Take(12)) // tối đa 12 tháng
                {
                    table.Cell().Element(CellBody).Text(item.Label);
                    table.Cell().Element(CellBody).AlignRight().Text(isMoney ? item.Value.ToString("N0") : item.Value.ToString("N0"));
                }
            });

            static IContainer CellHeader(IContainer c) =>
                c.Background(Colors.Grey.Lighten3).PaddingVertical(6).PaddingHorizontal(6).DefaultTextStyle(x => x.SemiBold());

            static IContainer CellBody(IContainer c) =>
                c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(6).PaddingHorizontal(6);
        }

        private static void SimpleBarChart(IContainer container, List<ChartPointVM> data, int maxBars)
        {
            var list = data?.Take(maxBars).ToList() ?? new List<ChartPointVM>();
            if (!list.Any())
            {
                container.Text("Không có dữ liệu").FontColor(Colors.Grey.Darken1);
                return;
            }

            var max = list.Max(x => x.Value);
            if (max <= 0) max = 1;

            container.Column(col =>
            {
                col.Spacing(6);

                foreach (var item in list)
                {
                    var ratio = (float)(item.Value / max);

                    // ✅ clamp ratio về [0..1]
                    ratio = Math.Clamp(ratio, 0f, 1f);

                    // ✅ QuestPDF không cho RelativeItem(0)
                    const float epsilon = 0.0001f;
                    var fill = Math.Max(ratio, epsilon);
                    var rest = Math.Max(1f - ratio, epsilon);

                    col.Item().Row(row =>
                    {
                        row.ConstantItem(70).Text(item.Label).FontSize(9);

                        row.RelativeItem().Height(12).Element(track =>
                        {
                            track.Row(r =>
                            {
                                r.RelativeItem(fill).Height(12)
                                    .Background(Colors.Blue.Lighten1);

                                r.RelativeItem(rest).Height(12)
                                    .Background(Colors.Grey.Lighten4);
                            });
                        });

                        row.ConstantItem(70).AlignRight()
                            .Text(item.Value.ToString("N0"))
                            .FontSize(9);
                    });
                }
            });
        }

        private static void SimplePieLikeList(IContainer container, List<PieSliceVM> slices)
        {
            var list = slices ?? new List<PieSliceVM>();
            if (!list.Any())
            {
                container.Text("Không có dữ liệu").FontColor(Colors.Grey.Darken1);
                return;
            }

            var total = list.Sum(x => x.Value);
            if (total <= 0) total = 1;

            container.Column(col =>
            {
                col.Spacing(6);

                foreach (var s in list.Take(10))
                {
                    var percent = s.Value / total * 100m;

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text(s.Label);
                        row.ConstantItem(80).AlignRight().Text($"{s.Value} ({percent:0.#}%)");
                    });
                }
            });
        }
    }
}
