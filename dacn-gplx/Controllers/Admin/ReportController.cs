using dacn_gplx.Services;
using Microsoft.AspNetCore.Mvc;

namespace dacn_gplx.Controllers.Admin
{
    [Route("Admin/Report")]
    public class ReportController : Controller
    {
        private readonly ReportService _service;

        public ReportController(ReportService service)
        {
            _service = service;
        }

        // Trang khung
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {
            var model = await _service.GetDashboardAsync(fromDate, toDate);
            return View("~/Views/Admin/Report/Index.cshtml", model);
        }

        [HttpGet("Tab/{tab}")]
        public async Task<IActionResult> Tab(string tab, DateTime? fromDate, DateTime? toDate)
        {
            var model = await _service.GetDashboardAsync(fromDate, toDate);

            return tab switch
            {
                "users" => PartialView("~/Views/Admin/Report/_TabUsers.cshtml", model),
                "courses" => PartialView("~/Views/Admin/Report/_TabCourses.cshtml", model),
                "exams" => PartialView("~/Views/Admin/Report/_TabExams.cshtml", model),
                _ => PartialView("~/Views/Admin/Report/_TabOverview.cshtml", model),
            };
        }

        [HttpGet("ExportPdf")]
        public async Task<IActionResult> ExportPdf(DateTime? fromDate, DateTime? toDate, string tab)
        {
            var model = await _service.GetDashboardAsync(fromDate, toDate);
            var pdf = _service.GenerateDashboardPdf(model, fromDate, toDate, tab);

            return File(pdf, "application/pdf",
                $"BaoCaoThongKe_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
    }
}
