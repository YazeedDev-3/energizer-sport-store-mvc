using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_web2.Data;
using project_web2.Models;

namespace project_web2.Controllers
{
    public class itemsController : BaseController
    {
        private readonly project_web2Context _context;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public itemsController(project_web2Context context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: items
        public async Task<IActionResult> Index()
        {
            ViewData["role"] = HttpContext.Session.GetString("Role");
            return View(await _context.Products.ToListAsync());
        }

        // GET: items/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewData["role"] = HttpContext.Session.GetString("Role");
            if (id == null) return NotFound();
            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // GET: items/Create
        public IActionResult Create()
        {
            if (!IsAdmin())
                return RedirectToAction("login", "useraccounts");
            return View();
        }

        // POST: items/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile? file, [Bind("Id,Name,Description,Price,Discount,CategoryId,Stock")] Products product)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(product);

                product.CreatedAt = DateTime.Now;

                if (file != null && file.Length > 0)
                {
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!AllowedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError("", "Only image files are allowed (jpg, jpeg, png, gif, webp).");
                        return View(product);
                    }
                    string filename = Path.GetFileName(file.FileName);
                    string folder = Path.Combine(_env.WebRootPath, "images");
                    Directory.CreateDirectory(folder);
                    using var stream = new FileStream(Path.Combine(folder, filename), FileMode.Create);
                    await file.CopyToAsync(stream);
                    product.ImageFile = filename;
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Unexpected error: " + ex.Message);
                return View(product);
            }
        }

        // GET: items/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin())
                return RedirectToAction("login", "useraccounts");
            if (id == null) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: items/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IFormFile? file, [Bind("Id,Name,Description,Price,Discount,CategoryId,Stock,ImageFile")] Products product)
        {
            if (!ModelState.IsValid)
                return View(product);

            if (file != null && file.Length > 0)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("", "Only image files are allowed (jpg, jpeg, png, gif, webp).");
                    return View(product);
                }
                try
                {
                    string filename = Path.GetFileName(file.FileName);
                    string folder = Path.Combine(_env.WebRootPath, "images");
                    Directory.CreateDirectory(folder);
                    using var stream = new FileStream(Path.Combine(folder, filename), FileMode.Create);
                    await file.CopyToAsync(stream);
                    product.ImageFile = filename;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Image upload failed: " + ex.Message);
                    return View(product);
                }
            }
            try
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to save product: " + ex.Message);
                return View(product);
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: items/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin())
                return RedirectToAction("login", "useraccounts");
            if (id == null) return NotFound();
            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
                _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Category
        public async Task<IActionResult> Category()
        {
            if (!IsAdmin())
                return RedirectToAction("login", "useraccounts");
            return View(await _context.Products.OrderBy(m => m.CategoryId).ToListAsync());
        }

    }
}
