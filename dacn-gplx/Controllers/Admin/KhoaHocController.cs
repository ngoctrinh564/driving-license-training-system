using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using dacn_gplx.Models;

namespace dacn_gplx.Controllers.Admin
{
    public class KhoaHocController : Controller
    {
        private readonly QuanLyGplxContext _context;

        public KhoaHocController(QuanLyGplxContext context)
        {
            _context = context;
        }

        // =================== INDEX ===================
        public async Task<IActionResult> Index(int? hangId, string trangThai, string tuKhoa)
        {
            var query = _context.KhoaHocs
                .Include(k => k.Hang)
                .AsQueryable();

            if (hangId.HasValue)
                query = query.Where(x => x.HangId == hangId.Value);

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(x => x.Trangthai == trangThai);

            if (!string.IsNullOrEmpty(tuKhoa))
                query = query.Where(x => x.TenKhoaHoc != null &&
                                         x.TenKhoaHoc.Contains(tuKhoa));

            var list = await query
                .OrderByDescending(x => x.KhoahocId)
                .ToListAsync();

            // Hạng GPLX
            ViewBag.HangList = await _context.HangGplxes
                .Select(h => new SelectListItem
                {
                    Value = h.HangId.ToString(),
                    Text = h.Tenhang
                }).ToListAsync();

            // Trạng thái
            ViewBag.TrangThaiList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Sắp khai giảng", Text = "Sắp khai giảng" },
                new SelectListItem { Value = "Đang học", Text = "Đang học" },
                new SelectListItem { Value = "Đã kết thúc", Text = "Đã kết thúc" }
            };

            return View("~/Views/Admin/KhoaHoc/Index.cshtml", list);
        }



        // =================== CREATE (GET) ===================
        public async Task<IActionResult> Create()
        {
            ViewBag.HangList = await _context.HangGplxes
                .Select(h => new SelectListItem
                {
                    Value = h.HangId.ToString(),
                    Text = h.Tenhang
                }).ToListAsync();

            // Dropdown địa điểm
            ViewBag.DiaDiemList = new List<string>
            {
                "Trung tâm số 1",
                "Trung tâm số 2",
                "Trung tâm số 3"
            };

            return View("~/Views/Admin/KhoaHoc/Create.cshtml");
        }



        // =================== CREATE (POST) ===================
        [HttpPost]
        public async Task<IActionResult> Create(KhoaHoc model)
        {
            // ===== VALIDATE NGÀY =====
            if (model.Ngaybatdau == null || model.Ngayketthuc == null)
            {
                ModelState.AddModelError("", "Ngày bắt đầu và kết thúc không được để trống.");
            }
            else
            {
                DateOnly today = DateOnly.FromDateTime(DateTime.Now);

                var start = model.Ngaybatdau.Value;
                var end = model.Ngayketthuc.Value;

                if (start <= today)
                    ModelState.AddModelError("Ngaybatdau", "Ngày bắt đầu phải lớn hơn ngày hiện tại (không được trùng).");

                if (end <= start)
                    ModelState.AddModelError("Ngayketthuc", "Ngày kết thúc phải lớn hơn ngày bắt đầu.");
            }


            if (!ModelState.IsValid)
            {
                // Load lại dropdown
                ViewBag.HangList = await _context.HangGplxes
                    .Select(h => new SelectListItem
                    {
                        Value = h.HangId.ToString(),
                        Text = h.Tenhang
                    }).ToListAsync();

                ViewBag.DiaDiemList = new List<string>
                {
                    "Trung tâm số 1",
                    "Trung tâm số 2",
                    "Trung tâm số 3"
                };

                return View("~/Views/Admin/KhoaHoc/Create.cshtml", model);
            }


            // ===== XỬ LÝ LƯU =====
            model.TenKhoaHoc = null;          // Trigger SQL tự sinh
            model.Trangthai = "Sắp khai giảng"; // Trạng thái mặc định

            _context.KhoaHocs.Add(model);

            // KHẮC PHỤC LỖI EF + TRIGGER SQL
            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        // =========================
        // TRANG CHI TIẾT KHOÁ HỌC
        // =========================
        public async Task<IActionResult> Details(int id)
        {
            var kh = await _context.KhoaHocs
                .Include(k => k.Hang)
                .FirstOrDefaultAsync(k => k.KhoahocId == id);

            if (kh == null)
                return NotFound();

            return View("~/Views/Admin/KhoaHoc/Details.cshtml", kh);
        }

        // ====================== EDIT - GET ============================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _context.KhoaHocs
                .Include(k => k.Hang)
                .FirstOrDefaultAsync(k => k.KhoahocId == id);

            if (model == null) return NotFound();

            if (model.Trangthai == "Đã kết thúc")
            {
                TempData["Error"] = "Khóa học đã kết thúc, không thể chỉnh sửa.";
                return RedirectToAction("Details", new { id = id });
            }

            ViewBag.HangList = await _context.HangGplxes
                .Select(h => new SelectListItem
                {
                    Value = h.HangId.ToString(),
                    Text = h.Tenhang
                }).ToListAsync();

            ViewBag.DiaDiemList = new List<string>
    {
        "Trung tâm số 1",
        "Trung tâm số 2",
        "Trung tâm số 3"
    };

            return View("~/Views/Admin/KhoaHoc/Edit.cshtml", model);
        }


        // ====================== EDIT - POST ============================
        [HttpPost]
        public async Task<IActionResult> Edit(KhoaHoc model)
        {
            var old = await _context.KhoaHocs.FindAsync(model.KhoahocId);
            if (old == null) return NotFound();

            if (old.Trangthai == "Đã kết thúc")
            {
                TempData["Error"] = "Khóa học đã kết thúc, không thể chỉnh sửa.";
                return RedirectToAction("Details", new { id = model.KhoahocId });
            }

            DateTime today = DateTime.Now.Date;

            if (model.Ngaybatdau <= DateOnly.FromDateTime(today))
                ModelState.AddModelError("Ngaybatdau", "Ngày bắt đầu phải lớn hơn ngày hiện tại.");

            if (model.Ngayketthuc <= model.Ngaybatdau)
                ModelState.AddModelError("Ngayketthuc", "Ngày kết thúc phải lớn hơn ngày bắt đầu.");

            if (!ModelState.IsValid)
            {
                ViewBag.HangList = _context.HangGplxes
                    .Select(h => new SelectListItem { Value = h.HangId.ToString(), Text = h.Tenhang })
                    .ToList();

                ViewBag.DiaDiemList = new List<string>
        {
            "Trung tâm số 1", "Trung tâm số 2", "Trung tâm số 3"
        };

                return View("~/Views/Admin/KhoaHoc/Edit.cshtml", model);
            }

            old.HangId = model.HangId;
            old.Ngaybatdau = model.Ngaybatdau;
            old.Ngayketthuc = model.Ngayketthuc;
            old.Diadiem = model.Diadiem;

            if (old.Ngayketthuc < DateOnly.FromDateTime(today))
                old.Trangthai = "Đã kết thúc";
            else if (old.Ngaybatdau > DateOnly.FromDateTime(today))
                old.Trangthai = "Sắp khai giảng";
            else
                old.Trangthai = "Đang học";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật khóa học thành công!";
            return RedirectToAction("Details", new { id = model.KhoahocId });
        }


    }
}
