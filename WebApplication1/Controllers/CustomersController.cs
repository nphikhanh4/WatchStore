using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
namespace admin4.Controllers
{
    public class CustomersController : Controller
    {
        private WatchStore_WebEntities db = new WatchStore_WebEntities();

        // GET: Customers
        public ActionResult Index(string search = "", string SortColumn = "CustomerID", string IconClass = "fa-sort-asc", int page = 1, int entriesPerPage = 10, string gender = "Gioi tinh")
        {
            // Initialize the queryable for customers
            var or = db.Customers.AsQueryable();

            // Apply gender filter if a valid value is selected
            if (!string.IsNullOrWhiteSpace(gender) && gender.Trim() != "Gioi tinh")
            {
                or = or.Where(p => p.Gender == gender.Trim());
            }

            // Store the selected gender in ViewBag
            ViewBag.Gender = gender;

            // Filter customers based on search input
            if (!string.IsNullOrEmpty(search))
            {
                or = or.Where(c => c.FullName.Contains(search));
            }

            // Store the search term in ViewBag to keep it in the search field
            ViewBag.SearchTerm = search;

            // Sorting logic
            or = SortCustomers(or, SortColumn, IconClass);

            // Pagination logic
            int totalEntries = or.Count();
            int totalPages = (int)Math.Ceiling((double)totalEntries / entriesPerPage);

            // Calculate records to skip for pagination
            var paginatedCustomers = or.Skip((page - 1) * entriesPerPage).Take(entriesPerPage).ToList();

            // Set ViewBag values for pagination and sorting
            ViewBag.TotalEntries = totalEntries;
            ViewBag.Page = page;
            ViewBag.NoOfPages = totalPages;
            ViewBag.StartEntry = (page - 1) * entriesPerPage + 1;
            ViewBag.EndEntry = Math.Min(page * entriesPerPage, totalEntries);
            ViewBag.SortColumn = SortColumn;
            ViewBag.IconClass = IconClass;

            // Pass the paginated customer list to the view
            return View(paginatedCustomers);
        }

        // Helper method for sorting customers
        private IQueryable<Customer> SortCustomers(IQueryable<Customer> customers, string sortColumn, string iconClass)
        {
            switch (sortColumn)
            {
                case "FullName":
                    return iconClass == "fa-sort-asc" ? customers.OrderBy(c => c.FullName) : customers.OrderByDescending(c => c.FullName);
                case "Email":
                    return iconClass == "fa-sort-asc" ? customers.OrderBy(c => c.Email) : customers.OrderByDescending(c => c.Email);
                // Add more cases as needed for other columns
                default:
                    return iconClass == "fa-sort-asc" ? customers.OrderBy(c => c.CustomerID) : customers.OrderByDescending(c => c.CustomerID);
            }
        }


        // GET: Customers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }
        // GET: Customers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CustomerID,FullName,Email,Password,Images,Phone,Gender,Address")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Customers.Add(customer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(customer);
        }

        // GET: Customers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CustomerID,FullName,Email,Password,Images,Phone,Gender,Address")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        // GET: Customers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Customer customer = db.Customers.Find(id);
            db.Customers.Remove(customer);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
