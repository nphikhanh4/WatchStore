using System;
using System.Collections.Generic;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.DynamicData;
using System.Web.Mvc;
using WebApplication1.Models;
namespace admin4.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product

        public ActionResult Index(string search = "", string sortColumn = "Id", string iconClass = "fa-sort-asc", int page = 1, int entriesPerPage = 10, string status = "Category")
        {
            WatchStore_WebEntities db = new WatchStore_WebEntities();

            // Lấy sản phẩm ban đầu dưới dạng IQueryable để áp dụng các bộ lọc động
            var products1 = db.Products.AsQueryable();

            // Áp dụng bộ lọc danh mục nếu đã chọn
            if (!string.IsNullOrEmpty(status) && status != "Category")
            {
                products1 = products1.Where(p => p.Category.CategoryName == status);
            }

            // Áp dụng tìm kiếm nếu có
            if (!string.IsNullOrEmpty(search))
            {
                products1 = products1.Where(p => p.ProductName.Contains(search));
            }

            // Áp dụng sắp xếp
            switch (sortColumn)
            {
                case "Id":
                    products1 = iconClass == "fa-sort-asc" ? products1.OrderBy(p => p.ProductID) : products1.OrderByDescending(p => p.ProductID);
                    break;
                case "ProductName":
                    products1 = iconClass == "fa-sort-asc" ? products1.OrderBy(p => p.ProductName) : products1.OrderByDescending(p => p.ProductName);
                    break;
                case "UnitPrice":
                    products1 = iconClass == "fa-sort-asc" ? products1.OrderBy(p => p.Price) : products1.OrderByDescending(p => p.Price);
                    break;
            }

            // Tổng số sản phẩm sau khi áp dụng các bộ lọc
            int totalEntries = products1.Count();
            int totalPages = (int)Math.Ceiling((double)totalEntries / entriesPerPage);

            // Tính toán dữ liệu phân trang
            int skipRecords = (page - 1) * entriesPerPage;
            var paginatedProducts = products1.Skip(skipRecords).Take(entriesPerPage).ToList();

            // Thiết lập các giá trị ViewBag cho View
            ViewBag.Category = status;
            ViewBag.search = search;
            ViewBag.SortColumn = sortColumn;
            ViewBag.IconClass = iconClass;
            ViewBag.EntriesPerPage = entriesPerPage;
            ViewBag.TotalEntries = totalEntries;
            ViewBag.Page = page;
            ViewBag.NoOfPages = totalPages;

            return View(paginatedProducts);
        }

        public ActionResult Detail(int? id)
        {
            WatchStore_WebEntities db = new WatchStore_WebEntities();
            Product pro = db.Products.Where(row => row.ProductID == id).FirstOrDefault();
            return View(pro);
        }

        public ActionResult Create()
        {
            using (WatchStore_WebEntities db = new WatchStore_WebEntities())
            {
                // Retrieve the list of suppliers from the database
                var suppliers = db.Suppliers.ToList();
                ViewBag.Suppliers = suppliers;
                // Retrieve the list of suppliers from the database
                var brand = db.Brands.ToList();
                ViewBag.Brands = brand;
                // Retrieve the list of suppliers from the database
                var category = db.Categories.ToList();
                ViewBag.Categorie = category;
            }

            return View();
        }
        [HttpPost]
        public ActionResult Create(Product product, HttpPostedFileBase ImageUrl)
        {
            using (WatchStore_WebEntities db = new WatchStore_WebEntities())
            {
                // Check if the SupplierId is valid
                var supplierExists = db.Suppliers.Any(s => s.SupplierID == product.SupplierID);
                if (!supplierExists)
                {
                    // Add an error message if the Supplier does not exist
                    ModelState.AddModelError("", "The selected supplier does not exist.");
                    return View(product); // Return the same view with the error
                }

                // Check if the Brand is valid
                var brandExists = db.Brands.Any(b => b.BrandID == product.BrandID);
                if (!brandExists)
                {
                    // Add an error message if the Brand does not exist
                    ModelState.AddModelError("", "The selected Brand does not exist.");
                    return View(product); // Return the same view with the error
                }

                // Check if the Category is valid
                var categoryExists = db.Categories.Any(c => c.CategoryID == product.CategoryID);
                if (!categoryExists)
                {
                    // Add an error message if the Category does not exist
                    ModelState.AddModelError("", "The selected Category does not exist.");
                    return View(product); // Return the same view with the error
                }

                // Handle the image file if it was uploaded
                if (ImageUrl != null && ImageUrl.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(ImageUrl.FileName);
                    string filePath = Path.Combine(Server.MapPath("~/Content/imgdata/"), fileName);
                    ImageUrl.SaveAs(filePath);
                    product.ImageUrl = fileName; // Save relative path in the database
                }

                // Add the product
                db.Products.Add(product);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }


        public ActionResult Edit(int id)
        {
            using (WatchStore_WebEntities db = new WatchStore_WebEntities())
            {
                var product = db.Products.Find(id);
                if (product == null)
                {
                    return HttpNotFound();
                }

                // Populate the dropdowns
                ViewBag.Brands = db.Brands.ToList();
                ViewBag.Categorie = db.Categories.ToList();

                return View(product);
            }
        }


        [HttpPost]
        public ActionResult Edit(Product pro, HttpPostedFileBase ImageUrl)
        {
            using (WatchStore_WebEntities db = new WatchStore_WebEntities())
            {
                // Find the existing product in the database
                var product = db.Products.Find(pro.ProductID);
                if (product == null)
                {
                    return HttpNotFound();
                }

                // Check if the SupplierId is valid
                var supplierExists = db.Suppliers.Any(s => s.SupplierID == pro.SupplierID);
                if (!supplierExists)
                {
                    ModelState.AddModelError("", "The selected supplier does not exist.");
                    return View(pro);
                }

                // Check if the BrandId is valid
                var brandExists = db.Brands.Any(b => b.BrandID == pro.BrandID);
                if (!brandExists)
                {
                    ModelState.AddModelError("", "The selected brand does not exist.");
                    return View(pro);
                }

                // Check if the CategoryId is valid
                var categoryExists = db.Categories.Any(c => c.CategoryID == pro.CategoryID);
                if (!categoryExists)
                {
                    ModelState.AddModelError("", "The selected category does not exist.");
                    return View(pro);
                }

                // Handle the image file if it was uploaded
                if (ImageUrl != null && ImageUrl.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(ImageUrl.FileName);
                    string filePath = Path.Combine(Server.MapPath("~/Content/imgdata/"), fileName);
                    ImageUrl.SaveAs(filePath);
                    product.ImageUrl = fileName; // Save relative path in the database
                }

                // Update product details
                product.ProductName = pro.ProductName;
                product.Price = pro.Price;
                product.BrandID = pro.BrandID; // Update BrandID instead of Brand
                product.CategoryID = pro.CategoryID;
                product.SupplierID = pro.SupplierID;

                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }

        public ActionResult Delete(int id)
        {

            WatchStore_WebEntities db = new WatchStore_WebEntities();
            Product product = db.Products.Where(row => row.ProductID == id).FirstOrDefault();

            return View(product);
        }
        [HttpPost]
        public ActionResult Delete(int id, Product pro)
        {
            using (WatchStore_WebEntities db = new WatchStore_WebEntities())
            {
                // Find the product to delete
                Product product = db.Products.Where(p => p.ProductID == id).FirstOrDefault();

                if (product == null)
                {
                    return HttpNotFound();
                }

                // Find and delete related OrderItem
                var orderItems = db.OrderItems.Where(oi => oi.ProductID == id).ToList();
                foreach (var orderItem in orderItems)
                {
                    db.OrderItems.Remove(orderItem);
                }

                // Now delete the product
                db.Products.Remove(product);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }

    }
}