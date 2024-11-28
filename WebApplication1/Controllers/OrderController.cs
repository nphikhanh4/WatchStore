using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using WebApplication1.Models;
namespace admin4.Controllers
{
    public class OrderController : Controller
    {
        private WatchStore_WebEntities db = new WatchStore_WebEntities();

        // GET: Order
        // GET: Order
        public ActionResult Index(int page = 1, int entriesPerPage = 10, string status = "All", string search = "")
        {
            // Start with all orders as a queryable collection
            var orders = db.Orders.AsQueryable();

            // Apply the search filter if the search term is provided
            //if (!string.IsNullOrEmpty(search))
            //{
            //    orders = orders.Where(o => o.Name.Contains(search));
            //}

            // Filter based on status
            if (status != "All") // Adjusted to check for "All"
            {
                orders = orders.Where(o => o.Status == status);
            }

            // Set ViewBag values to keep track of the selected status and search term
            ViewBag.Status = status;
            ViewBag.SearchResults = search;

            // Ensure there's sorting applied before pagination
            orders = orders.OrderBy(o => o.OrderDate); // Change this to the field you want to sort by

            // Get the count of filtered entries
            var totalEntries = orders.Count();
            var totalPages = (int)Math.Ceiling((double)totalEntries / entriesPerPage);
            var skipRecords = (page - 1) * entriesPerPage;

            // Apply pagination
            var paginatedOrders = orders.Skip(skipRecords).Take(entriesPerPage).ToList();

            // Calculate range of items being displayed
            int startEntry = skipRecords + 1;
            int endEntry = Math.Min(skipRecords + entriesPerPage, totalEntries);

            // Set ViewBag values for pagination
            ViewBag.TotalEntries = totalEntries;
            ViewBag.Page = page;
            ViewBag.NoOfPages = totalPages;
            ViewBag.StartEntry = startEntry;
            ViewBag.EndEntry = endEntry;
            ViewBag.EntriesPerPage = entriesPerPage;

            return View(paginatedOrders);
        }




        // POST: Order/Approve/5
        [HttpPost]
        public ActionResult Approve(int id)
        {
            var order = db.Orders.Find(id);
            if (order != null)
            {
                order.Status = "Approved"; // Thay đổi trạng thái thành Approved
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // POST: Order/Reject/5
        [HttpPost]
        public ActionResult Reject(int id)
        {
            var order = db.Orders.Find(id);
            if (order != null)
            {
                order.Status = "Rejected"; // Thay đổi trạng thái thành Rejected
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Invoice(int id)
        {
            // Truy vấn đơn hàng cùng với các OrderItems và Products liên quan
            var order = db.Orders
                .Include(o => o.OrderItems.Select(oi => oi.Product)) // Include các OrderItems và Products
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound("Không tìm thấy đơn hàng với ID: " + id);
            }

            return View(order);
        }


    }
}