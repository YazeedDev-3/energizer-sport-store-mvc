using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_web2.Data;
using project_web2.Models;

namespace project_web2.Controllers
{
    public class ordersController : BaseController
    {
        private readonly project_web2Context _context;

        public ordersController(project_web2Context context)
        {
            _context = context;
        }

        // CatalogueBuy
        public async Task<IActionResult> CatalogueBuy()
        {
            return View(await _context.Products.ToListAsync());
        }

        // itemsBuyDetail
        public async Task<IActionResult> itemsBuyDetail(int? id)
        {
            var product = await _context.Products.FindAsync(id);
            return View(product);
        }

        // cartadd
        List<buyitems> Bbks = new List<buyitems>();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> cartadd(int Id, int quantity)
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;
            var sessionString = HttpContext.Session.GetString("Cart");
            if (sessionString is not null)
                Bbks = JsonSerializer.Deserialize<List<buyitems>>(sessionString);

            var item = await _context.Products.FindAsync(Id);

            if (item == null)
            {
                ViewData["Error"] = "Item not found.";
                return View("itemsBuyDetail", item);
            }

            if (quantity > item.Stock)
            {
                ViewData["Error"] = "Requested quantity exceeds available stock.";
                return View("itemsBuyDetail", item);
            }

            Bbks.Add(new buyitems
            {
                ProductId = item.Id,
                name = item.Name,
                Price = item.Price,
                quant = quantity
            });

            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(Bbks));
            return RedirectToAction("CartBuy");
        }

        // CartBuy
        public IActionResult CartBuy()
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;
            var sessionString = HttpContext.Session.GetString("Cart");
            if (sessionString is not null)
                Bbks = JsonSerializer.Deserialize<List<buyitems>>(sessionString);
            return View(Bbks);
        }

        // Buy
        public async Task<IActionResult> Buy()
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;
            var sessionString = HttpContext.Session.GetString("Cart");
            if (sessionString is not null)
                Bbks = JsonSerializer.Deserialize<List<buyitems>>(sessionString);

            int userId = Convert.ToInt32(HttpContext.Session.GetString("userid"));
            Orders order = new Orders
            {
                UserId = userId,
                OrderDate = DateTime.Today,
                Total = 0
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            decimal tot = 0;
            foreach (var bk in Bbks.ToList())
            {
                OrderItems oline = new OrderItems
                {
                    OrderId = order.Id,
                    ProductId = bk.ProductId,
                    Quantity = bk.quant,
                    UnitPrice = bk.Price
                };
                _context.OrderItems.Add(oline);
                await _context.SaveChangesAsync();

                var product = await _context.Products.FindAsync(bk.ProductId);
                product.Stock -= bk.quant;
                _context.Update(product);
                await _context.SaveChangesAsync();

                tot += bk.quant * bk.Price;
            }
            order.Total = tot;
            _context.Update(order);
            await _context.SaveChangesAsync();

            Bbks = new List<buyitems>();
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(Bbks));
            return RedirectToAction("MyOrder");
        }

        // MyOrder
        public async Task<IActionResult> MyOrder()
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;
            int userId = Convert.ToInt32(HttpContext.Session.GetString("userid"));
            return View(await _context.Orders.Where(o => o.UserId == userId).ToListAsync());
        }

        // Orderline
        public async Task<IActionResult> Orderline(int? orid)
        {
            ViewBag.role = HttpContext.Session.GetString("Role");
            var items = await (
                from oi in _context.OrderItems
                join p in _context.Products on oi.ProductId equals p.Id
                where oi.OrderId == orid
                select new OrderItemView
                {
                    ItemName = p.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }
            ).ToListAsync();
            return View(items);
        }

        // Orderdetale
        public async Task<IActionResult> Orderdetale(string? custname)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == custname);
            if (user == null) return NotFound();
            var orders = await _context.Orders.Where(o => o.UserId == user.Id).ToListAsync();
            ViewBag.custname = custname;
            return View(orders);
        }

        // GET: orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders.FirstOrDefaultAsync(m => m.Id == id);
            if (order == null) return NotFound();
            return View(order);
        }

        // GET: orders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,OrderDate,Total")] Orders order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction("adminhome", "useraccounts");
            }
            return View(order);
        }

        // GET: orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        // POST: orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,OrderDate,Total")] Orders order)
        {
            if (id != order.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Orders.Any(e => e.Id == order.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction("adminhome", "useraccounts");
            }
            return View(order);
        }

        // GET: orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders.FirstOrDefaultAsync(m => m.Id == id);
            if (order == null) return NotFound();
            return View(order);
        }

        // POST: orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
                _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction("adminhome", "useraccounts");
        }
    }
}
