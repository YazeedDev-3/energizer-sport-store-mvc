using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_web2.Data;
using project_web2.Models;

namespace project_web2.Controllers
{
    public class useraccountsController : BaseController
    {
        private readonly project_web2Context _context;
        private readonly IConfiguration _config;

        public useraccountsController(project_web2Context context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // login
        public IActionResult Login()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("Name"))
                return View();
            else
            {
                string na = HttpContext.Request.Cookies["Name"].ToString();
                string ro = HttpContext.Request.Cookies["Role"].ToString();
                HttpContext.Session.SetString("Name", na);
                HttpContext.Session.SetString("Role", ro);
                if (ro == "customer")
                    return RedirectToAction("customerHome", "useraccounts");
                else
                    return RedirectToAction("adminhome", "useraccounts");
            }
        }

        [HttpPost, ActionName("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string na, string pa, string auto, string? returnUrl = null)
        {
            var ur = await _context.Users.FirstOrDefaultAsync(u => u.Username == na);
            bool passwordValid = false;
            if (ur != null && !string.IsNullOrEmpty(ur.PasswordHash))
            {
                try { passwordValid = BCrypt.Net.BCrypt.Verify(pa, ur.PasswordHash); }
                catch (Exception) { passwordValid = false; }
            }
            if (!passwordValid)
            {
                ViewData["Message"] = "wrong user name password";
                return View();
            }

            HttpContext.Session.SetString("userid", Convert.ToString(ur.Id));
            HttpContext.Session.SetString("Name", ur.Username);
            HttpContext.Session.SetString("Role", ur.Role);
            if (auto == "on")
            {
                HttpContext.Response.Cookies.Append("Name", ur.Username);
                HttpContext.Response.Cookies.Append("Role", ur.Role);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (ur.Role == "customer")
                return RedirectToAction("customerHome", "useraccounts");
            else if (ur.Role == "admin")
                return RedirectToAction("adminhome", "useraccounts");
            else
                return View();
        }

        // logout
        public IActionResult logout()
        {
            HttpContext.Response.Cookies.Delete("Name");
            HttpContext.Response.Cookies.Delete("Role");
            HttpContext.Session.Clear();
            return RedirectToAction("login", "useraccounts");
        }

        // registration
        public IActionResult registration()
        {
            if (IsAdmin() || IsCustomer())
                return RedirectToAction("login", "useraccounts");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> registration(string Username, string pass,
            [Bind("FullName,Email,Gender,City")] CustomerProfiles prof)
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(pass) ||
                string.IsNullOrEmpty(prof.FullName) || string.IsNullOrEmpty(prof.Email) ||
                string.IsNullOrEmpty(prof.City))
            {
                ViewData["message"] = "Please fill in all required fields correctly.";
                return View();
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == Username);
            if (existingUser != null)
            {
                ViewData["message"] = "Username already exists";
                return View();
            }

            var acc = new Users
            {
                Username = Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(pass),
                Role = "customer"
            };
            _context.Users.Add(acc);
            await _context.SaveChangesAsync();

            prof.UserId = acc.Id;
            _context.CustomerProfiles.Add(prof);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("userid", Convert.ToString(acc.Id));
            HttpContext.Session.SetString("Name", acc.Username);
            HttpContext.Session.SetString("Role", acc.Role);

            return RedirectToAction("customerhome", "useraccounts");
        }

        // customerHome
        public async Task<IActionResult> customerHome()
        {
            if (IsCustomer())
            {
                ViewData["name"] = GetSessionUser();
                var discount = await _context.Products
                    .Where(b => b.Discount > 0)
                    .ToListAsync();
                return View(discount);
            }
            else
                return RedirectToAction("login", "useraccounts");
        }

        // adminhome
        public async Task<IActionResult> adminhome()
        {
            if (!IsAdmin())
                return RedirectToAction("login", "useraccounts");

            ViewData["name"] = GetSessionUser();
            ViewData["totalProducts"] = await _context.Products.CountAsync();
            ViewData["totalOrders"]   = await _context.Orders.CountAsync();
            ViewData["totalUsers"]    = await _context.Users.CountAsync();
            ViewData["totalRevenue"]  = await _context.Orders.SumAsync(o => (decimal?)o.Total) ?? 0m;
            ViewData["d1"] = await _context.Products.CountAsync(i => i.CategoryId == 1);
            ViewData["d2"] = await _context.Products.CountAsync(i => i.CategoryId == 2);
            ViewData["d3"] = await _context.Products.CountAsync(i => i.CategoryId == 3);
            ViewData["d4"] = await _context.Products.CountAsync(i => i.CategoryId == 4);
            ViewData["d5"] = await _context.Products.CountAsync(i => i.CategoryId == 5);
            return View();
        }

        // email
        public IActionResult email()
        {
            if (!IsAdmin())
                return RedirectToAction("login", "useraccounts");
            return View();
        }

        [HttpPost, ActionName("email")]
        [ValidateAntiForgeryToken]
        public IActionResult email(string address, string subject, string body)
        {
            if (!IsAdmin())
                return RedirectToAction("login", "useraccounts");
            string senderAddress = _config["Email:SenderAddress"];
            string senderPassword = _config["Email:Password"];

            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            var mail = new MailMessage();
            mail.From = new MailAddress(senderAddress);
            mail.To.Add(address);
            mail.Subject = subject;
            mail.IsBodyHtml = true;
            mail.Body = body;
            SmtpServer.Port = 587;
            SmtpServer.UseDefaultCredentials = false;
            SmtpServer.Credentials = new System.Net.NetworkCredential(senderAddress, senderPassword);
            SmtpServer.EnableSsl = true;
            SmtpServer.Send(mail);
            ViewData["Message"] = "Email sent.";
            return View();
        }

        // Users_search
        public IActionResult Users_search()
        {
            if (!IsAdmin())
                return RedirectToAction("login", "useraccounts");
            return View(new Users());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Users_search(string name)
        {
            if (!IsAdmin())
                return RedirectToAction("login", "useraccounts");
            var uuss = await _context.Users.FirstOrDefaultAsync(u => u.Username == name);
            return View(uuss);
        }

        // AddAdmin
        public IActionResult AddAdmin()
        {
            if (!IsAdmin())
                return RedirectToAction("login", "useraccounts");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAdmin(string name, string pass, string confirmPass)
        {
            if (!IsAdmin())
                return RedirectToAction("login", "useraccounts");

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pass))
                return View();

            if (pass != confirmPass)
            {
                ViewData["Message"] = "Passwords do not match.";
                return View();
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == name);
            if (existingUser != null)
            {
                ViewData["Message"] = "Username already exists.";
                return View();
            }

            var acc = new Users
            {
                Username = name,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(pass),
                Role = "admin"
            };
            _context.Users.Add(acc);
            await _context.SaveChangesAsync();

            ViewData["Message"] = "Admin added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: useraccounts
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
                return RedirectToAction("login", "useraccounts");
            var users = await _context.Users.ToListAsync();
            var profiles = await _context.CustomerProfiles.ToListAsync();
            ViewBag.Emails = profiles.ToDictionary(p => p.UserId, p => p.Email);
            return View(users);
        }

        // GET: useraccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        // GET: useraccounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: useraccounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Username,PasswordHash,Role")] Users user)
        {
            if (ModelState.IsValid)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: useraccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: useraccounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,PasswordHash,Role")] Users user)
        {
            if (id != user.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(e => e.Id == user.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: useraccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: useraccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
                _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
