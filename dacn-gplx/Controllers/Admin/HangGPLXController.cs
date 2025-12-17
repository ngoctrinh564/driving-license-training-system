using dacn_gplx.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace dacn_gplx.Controllers.Admin
{
    public class HangGplxController : Controller
    {
        private readonly QuanLyGplxContext _context;

    public HangGplxController(QuanLyGplxContext context)
        {
            _context = context;
        }

        // ============================
        // 1) INDEX + TÌM KIẾM
        // ============================
        public IActionResult Index(string search)
        {
            var query = _context.HangGplxes.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(h =>
                    (h.Tenhang ?? "").Contains(search) ||
                    (h.Loaiphuongtien ?? "").Contains(search) 
                );
            }

            ViewBag.Search = search;

            return View("~/Views/Admin/HangGplx/Index.cshtml", query.ToList());
        }

        // ============================
        // 2) XEM CHI TIẾT
        // ============================
        public IActionResult Details(int id)
        {
            var hang = _context.HangGplxes
                .FirstOrDefault(h => h.HangId == id);

            if (hang == null) return NotFound();

            return View("~/Views/Admin/HangGplx/Details.cshtml", hang);
        }

        // ============================
        // 3) THÊM HẠNG - GET
        // ============================
        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Admin/HangGplx/Create.cshtml");
        }

        // ============================
        // 3) THÊM HẠNG - POST
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(HangGplx model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/HangGplx/Create.cshtml", model);

            // Kiểm tra tên hạng không trùng
            bool exists = _context.HangGplxes.Any(h => h.Tenhang == model.Tenhang);
            if (exists)
            {
                ModelState.AddModelError("Tenhang", "Tên hạng này đã tồn tại.");
                return View("~/Views/Admin/HangGplx/Create.cshtml", model);
            }

            _context.HangGplxes.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ============================
        // 4) SỬA HẠNG - GET
        // ============================
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var hang = _context.HangGplxes
                .FirstOrDefault(h => h.HangId == id);

            if (hang == null) return NotFound();

            return View("~/Views/Admin/HangGplx/Edit.cshtml", hang);
        }

        // ============================
        // 4) SỬA HẠNG - POST
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(HangGplx model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/HangGplx/Edit.cshtml", model);

            var existing = _context.HangGplxes.AsNoTracking()
                .FirstOrDefault(h => h.HangId == model.HangId);

            if (existing == null) return NotFound();

            // Giữ nguyên tên hạng, không cho sửa
            model.Tenhang = existing.Tenhang;

            _context.HangGplxes.Update(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
