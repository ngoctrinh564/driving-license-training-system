using dacn_gplx.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using UserEntity = dacn_gplx.Models.User;

namespace dacn_gplx.Controllers.Admin
{
    [Route("Admin/[controller]/[action]")]
    public class UserController : Controller
    {
        private readonly QuanLyGplxContext _context;

        public UserController(QuanLyGplxContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => (u.Username ?? "").Contains(search));

            ViewBag.Search = search;
            return View("~/Views/Admin/User/Index.cshtml", query.ToList());
        }

        public IActionResult Details(int id)
        {
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserId == id);

            if (user == null)
                return NotFound();

            var hocVien = _context.HocViens
                .AsNoTracking()
                .FirstOrDefault(hv => hv.UserId == id);

            ViewBag.HocVien = hocVien;

            // ✅ SỬA CHỖ NÀY: trả về đúng view path
            return View("~/Views/Admin/User/Details.cshtml", user);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(_context.Roles.ToList(), "RoleId", "Rolename");
            return View("~/Views/Admin/User/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserEntity model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(_context.Roles.ToList(), "RoleId", "Rolename");
                return View("~/Views/Admin/User/Create.cshtml", model);
            }

            if (!string.IsNullOrEmpty(model.Password))
                model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

            model.Isactive = true;

            _context.Users.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserId == id);

            if (user == null) return NotFound();

            ViewBag.Roles = new SelectList(_context.Roles.ToList(), "RoleId", "Rolename", user.RoleId);
            return View("~/Views/Admin/User/Edit.cshtml", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(UserEntity model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(_context.Roles.ToList(), "RoleId", "Rolename", model.RoleId);
                return View("~/Views/Admin/User/Edit.cshtml", model);
            }

            var existingUser = _context.Users.AsNoTracking()
                .FirstOrDefault(u => u.UserId == model.UserId);

            if (existingUser == null) return NotFound();

            model.Password = existingUser.Password;

            var currentUsername = User.Identity.Name;

            if (existingUser.Username.Equals(currentUsername, StringComparison.OrdinalIgnoreCase))
            {
                model.RoleId = existingUser.RoleId;
                model.Isactive = existingUser.Isactive;
            }

            _context.Users.Update(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
