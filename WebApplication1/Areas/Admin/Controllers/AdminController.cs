using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Index()
        {// Khởi tạo ngữ cảnh cơ sở dữ liệu
            WatchStore_WebEntities db = new WatchStore_WebEntities();

            // Tổng số hóa đơn trong năm 2024
            var totalOrders = db.Orders
                .Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Year == 2024)
                .Count();

            // Tổng doanh thu trong năm 2024
            var totalRevenue = db.Orders
                .Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Year == 2024)
                .Join(db.OrderItems,
                    o => o.OrderID,
                    oi => oi.OrderID,
                    (o, oi) => new { oi.Quantity, oi.UnitPrice })
                .Sum(x => x.Quantity * x.UnitPrice);

            // Sản phẩm bán chạy nhất trong năm 2024
            var bestSellingProduct = db.Orders
                .Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Year == 2024)
                .Join(db.OrderItems,
                    o => o.OrderID,
                    oi => oi.OrderID,
                    (o, oi) => new { oi.ProductID, oi.Quantity })
                .GroupBy(x => x.ProductID)
                .Select(g => new
                {
                    ProductID = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Join(db.Products,
                    g => g.ProductID,
                    p => p.ProductID,
                    (g, p) => new { p.ProductID, p.ProductName, g.TotalQuantity })
                .FirstOrDefault();

            // Tổng số khách hàng đã mua hàng trong năm 2024
            var totalCustomers = db.Orders
                .Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Year == 2024)
                .Select(o => o.CustomerID)
                .Distinct()
                .Count();

            // Truyền dữ liệu sang ViewBag để hiển thị trong view
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.BestSellingProduct = bestSellingProduct;
            ViewBag.TotalCustomers = totalCustomers;


            var bestSellingProducts = db.Orders
           .Where(o => o.OrderDate.Value.Year == 2024)
           .Join(db.OrderItems,
                 o => o.OrderID,
                 oi => oi.OrderID,
                 (o, oi) => new { o, oi })
           .Join(db.Products,
                 combined => combined.oi.ProductID,
                 p => p.ProductID,
                 (combined, p) => new { p, combined.oi.Quantity })
           .GroupBy(x => new { x.p.ProductID, x.p.ProductName })
           .Select(g => new
           {
               ProductID = g.Key.ProductID,
               ProductName = g.Key.ProductName,
               TotalQuantity = g.Sum(x => x.Quantity)
           })
           .OrderByDescending(x => x.TotalQuantity)
           .Take(5)
           .ToList();


            return View();
        }
    }
}