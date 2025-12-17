using dacn_gplx.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace dacn_gplx.Controllers.Admin
{
    public class ExamController : Controller
    {
        private readonly QuanLyGplxContext _context;

        public ExamController(QuanLyGplxContext context)
        {
            _context = context;
        }

        private List<SelectListItem> GetLoaiKyThiList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Sát hạch", Text = "Sát hạch" },
                new SelectListItem { Value = "Tốt nghiệp", Text = "Tốt nghiệp" }
            };
        }

        // ============================
        // 1) INDEX + TÌM KIẾM
        // ============================
        public async Task<IActionResult> Index(string search)
        {
            var query = _context.KyThis
                .Include(k => k.ChiTietDangKyThis) // ⭐ Thêm
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(k => (k.Tenkythi ?? "").Contains(search)
                                       || (k.Loaikythi ?? "").Contains(search));
            }

            var exams = await query.ToListAsync();

            // Tính số HV đăng ký cho từng kỳ thi
            foreach (var exam in exams)
            {
                int soDangKy = exam.ChiTietDangKyThis
                                  .Select(c => c.HosoId)
                                  .Distinct()
                                  .Count();

                ViewData[$"SoDangKy_{exam.KythiId}"] = soDangKy;
            }

            ViewBag.Search = search;
            return View("~/Views/Admin/Exam/Index.cshtml", exams);
        }


        // ============================
        // 2) XEM CHI TIẾT
        // ============================
        public async Task<IActionResult> Details(int id)
        {
            var kyThi = await _context.KyThis
                .Include(k => k.LichThis)
                .Include(k => k.ChiTietDangKyThis) // ⭐ thêm
                .FirstOrDefaultAsync(k => k.KythiId == id);

            if (kyThi == null) return NotFound();

            int soDangKy = kyThi.ChiTietDangKyThis
                .Select(c => c.HosoId)
                .Distinct()
                .Count();

            ViewBag.SoDangKy = soDangKy;

            return View("~/Views/Admin/Exam/Details.cshtml", kyThi);
        }


        // ============================
        // 3) THÊM KÌ THI - GET
        // ============================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.LoaiKyThiList = GetLoaiKyThiList();
            ViewBag.HangGplxList = await _context.HangGplxes.ToListAsync(); // ⭐ BẮT BUỘC

            return View("~/Views/Admin/Exam/Create.cshtml");
        }


        // ============================
        // 3) THÊM KÌ THI - POST
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KyThi model, string HangGplx)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.LoaiKyThiList = GetLoaiKyThiList();
                ViewBag.HangGplxList = await _context.HangGplxes.ToListAsync(); // ⭐ BẮT BUỘC
                return View("~/Views/Admin/Exam/Create.cshtml", model);
            }

            // Ghép tên kỳ thi + hạng GPLX
            if (!string.IsNullOrWhiteSpace(HangGplx))
            {
                model.Tenkythi = $"{model.Tenkythi.Trim()} - {HangGplx}";
            }

            model.TrangThai = false;

            _context.KyThis.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // ============================
        // 4) SỬA KÌ THI - GET
        // ============================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kyThi = await _context.KyThis.FindAsync(id);
            if (kyThi == null)
            {
                return NotFound();
            }

            // Gán danh sách dropdown và dữ liệu kì thi
            ViewBag.LoaiKyThiList = GetLoaiKyThiList();
            return View("~/Views/Admin/Exam/Edit.cshtml", kyThi);
        }

        // ============================
        // 4) SỬA KÌ THI - POST
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("KythiId, Tenkythi, Loaikythi, Trangthai")] KyThi model)
        {
            if (id != model.KythiId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.KyThis.Any(e => e.KythiId == model.KythiId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.LoaiKyThiList = GetLoaiKyThiList();
            return View("~/Views/Admin/Exam/Edit.cshtml", model);
        }


        // ============================
        // 5) THÊM LỊCH THI - GET
        // ============================
        // Lấy ID Kì Thi để biết lịch thi này thuộc về kì thi nào
        [HttpGet]
        public IActionResult CreateSchedule(int examId)
        {
            ViewBag.KythiId = examId;
            return View("~/Views/Admin/Exam/CreateSchedule.cshtml");
        }

        // ============================
        // 5) THÊM LỊCH THI - POST
        // ============================
        // ============================
        // 5) THÊM LỊCH THI - POST
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSchedule(LichThi model)
        {
            if (ModelState.IsValid)
            {
                // Lấy ngày hiện tại (chỉ lấy phần ngày)
                var today = DateTime.Today;

                // KIỂM TRA NẾU NGÀY THI NHỎ HƠN HOẶC BẰNG NGÀY HÔM NAY
                if (model.Thoigianthi.HasValue && model.Thoigianthi.Value.Date <= today)
                {
                    ModelState.AddModelError("Thoigianthi", "Thời gian thi phải sau ngày hôm nay (tức là từ ngày mai trở đi).");
                }

                // Lấy kỳ thi để kiểm tra số lượng lịch thi
                var exam = await _context.KyThis
                    .Include(k => k.LichThis)
                    .FirstOrDefaultAsync(k => k.KythiId == model.KythiId);

                if (exam == null)
                {
                    ModelState.AddModelError("", "Kỳ thi không tồn tại.");
                }
                else if (exam.LichThis.Count >= 6) // <--- Giới hạn 6 lịch thi
                {
                    ModelState.AddModelError("", "Kỳ thi đã đủ 6 lịch thi. Không thể thêm lịch mới.");
                }

                if (ModelState.IsValid)
                {
                    _context.LichThis.Add(model);
                    await _context.SaveChangesAsync();

                    // Chuyển hướng trở lại trang Chi tiết Kì Thi sau khi thêm thành công
                    return RedirectToAction("Details", new { id = model.KythiId });
                }
            }

            // Nếu model không hợp lệ (bao gồm cả lỗi kiểm tra ngày và số lượng lịch), trả về form với dữ liệu đã nhập
            ViewBag.KythiId = model.KythiId;
            return View("~/Views/Admin/Exam/CreateSchedule.cshtml", model);
        }

    }
}