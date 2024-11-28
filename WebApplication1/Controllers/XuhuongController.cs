using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using WebApplication1.Models;
using System.Net;
using System.Net.Mail;


// chính

namespace WebApplication1.Controllers
{
    public class XuhuongController : Controller
    {
        private int _savedProductId;
        public double AverageRating { get; set; }

        public int GetId1()
        {
            return _savedProductId; // Trả về giá trị ID đã lưu
        }

        public class YourViewModel
        {
            public List<Product> Product { get; set; } // Danh sách sản phẩm
            public List<Customer> Customer { get; set; } // Danh sách sản phẩm
            public List<Detail> Details { get; set; } // Danh sách chi tiết sản phẩm
            public List<Images_Default> Images_Default { get; set; } // Hình ảnh mặc định
            public List<Images_Reality> Images_Realities { get; set; } // Hình ảnh thực tế
            public List<Images_Certification> images_Certifications { get; set; }
            public List<Feedback> feedbacks { get; set; } // Danh sách thương hiệu
            public List<Brand> Brand { get; set; } // Danh sách thương hiệu
            public List<Video> Videos { get; set; }
            public int ProductId { get; set; } // ID của sản phẩm
            public string BrandName { get; set; } // Tên thương hiệu
            public string ImageUrl { get; set; } // Tên sản phẩm
            public string ProductName { get; set; } // Tên sản phẩm
            public string PageID { get; set; }
        }

        // GET: Xuhuong

        public ActionResult Xuhuong()
        {
            using (WatchStore_WebEntities db = new WatchStore_WebEntities())
            {
                // Lấy danh sách sản phẩm
                var products = db.Products.ToList();
                // Lấy danh sách thương hiệu
                var brands = db.Brands.ToList();
                var detail = db.Details.ToList();
                // Gộp dữ liệu vào ViewModel
                var viewModel = new YourViewModel
                {
                    Product = products,
                    Details = detail,
                    Brand = brands,
                };
                // Trả về View với ViewModel đã chuẩn bị
                return View(viewModel);
            }
        }
        [HttpPost]
        public ActionResult XuHuong(string imageName)
        {
            // Lưu tên ảnh vào Session
            Session["ImageName"] = imageName;

            Console.WriteLine("Saved Image Name: " + imageName);
            return Json(new { result = imageName }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BanHang(int page = 1, string filterType = "", string search = "")
        {

            using (WatchStore_WebEntities db = new WatchStore_WebEntities())
            {
                int pageSize = 8;
                var imageName = Session["ImageName"] as string;

                var query = db.Products.Where(p => p.Brand.BrandName == imageName);

                // Apply filters (same as your existing logic)
                switch (filterType)
                {
                    case "NoiBat":
                        query = query
                            .Join(db.OrderItems, p => p.ProductID, oi => oi.ProductID, (p, oi) => new { p, oi }) // Kết nối với bảng OrderItem
                            .GroupBy(x => x.p.ProductID) // Nhóm theo ProductID của sản phẩm
                            .Select(g => new
                            {
                                Product = g.FirstOrDefault().p, // Lấy sản phẩm đầu tiên trong nhóm
                                TotalSold = g.Sum(x => x.oi.Quantity) // Tính tổng số lượng bán ra từ OrderItem
                            })
                            .OrderByDescending(p => p.TotalSold) // Sắp xếp giảm dần theo tổng số lượng bán ra
                            .Select(p => p.Product); // Lấy lại các sản phẩm đã sắp xếp
                        break;

                    case "SaleLon":
                        query = query
                            .Where(p => p.Discount < 50)  // Lọc sản phẩm có Discount > 30
                            .OrderByDescending(p => p.Discount);  // Sắp xếp theo Discount giảm dần
                        break;

                    case "DanhGiaNhieu":
                        query = query
                            .OrderByDescending(p => p.AverageRating) // Sắp xếp giảm dần theo AverageRating
                            .Select(p => p); // Lấy lại các sản phẩm đã sắp xếp
                        break;

                    case "GiaGiamDan":
                        // Sắp xếp giá giảm dần
                        query = query.OrderByDescending(p => p.Price);
                        break;

                    case "GiaTangDan":
                        // Sắp xếp giá tăng dần
                        query = query.OrderBy(p => p.Price);
                                

                        break;

                    case "Duoi1Trieu":
                        // Lọc sản phẩm có giá dưới 1 triệu và sắp xếp theo giá giảm dần
                        query = query.Where(p => p.Price < 1000000).OrderByDescending(p => p.Price);
                        break;

                    case "Tu1Den2Trieu":
                        // Lọc sản phẩm có giá từ 1 triệu đến 2 triệu và sắp xếp theo giá giảm dần
                        query = query.Where(p => p.Price >= 1000000 && p.Price <= 2000000).OrderByDescending(p => p.Price);
                        break;

                    case "Tu2Den4Trieu":
                        // Lọc sản phẩm có giá từ 2 triệu đến 4 triệu và sắp xếp theo giá giảm dần
                        query = query.Where(p => p.Price >= 2000000 && p.Price <= 4000000).OrderByDescending(p => p.Price);
                        break;

                    case "Tren4Trieu":
                        // Lọc sản phẩm có giá trên 4 triệu và sắp xếp theo giá giảm dần
                        query = query.Where(p => p.Price > 4000000).OrderByDescending(p => p.Price);
                        break;

                    case "Nam":
                        query = query
                            .Join(db.Categories, p => p.CategoryID, c => c.CategoryID, (p, c) => new { p, c })  // Kết nối bảng Product với bảng Category
                            .Where(x => x.p.CategoryID == 1)
                            .Select(x => x.p);
                        query = query.AsEnumerable()
                                     .OrderBy(x => Guid.NewGuid())
                                     .AsQueryable();
                        break;

                    case "Nu":
                        query = query
                            .Join(db.Categories, p => p.CategoryID, c => c.CategoryID, (p, c) => new { p, c })  // Kết nối bảng Product với bảng Category
                            .Where(x => x.p.CategoryID == 2)
                            .Select(x => x.p);

                        query = query.AsEnumerable()
                                     .OrderBy(x => Guid.NewGuid())
                                     .AsQueryable();
                        break;

                    case "Unisex":
                        query = query
                            .Join(db.Categories, p => p.CategoryID, c => c.CategoryID, (p, c) => new { p, c })  // Kết nối bảng Product với bảng Category
                            .Where(x => x.p.CategoryID == 3)
                            .Select(x => x.p);
                        query = query.AsEnumerable()
                                     .OrderBy(x => Guid.NewGuid())
                                     .AsQueryable();
                        break;

                    case "TheThao":
                        query = query
                            .Join(db.Categories, p => p.CategoryID, c => c.CategoryID, (p, c) => new { p, c })
                            .Where(x => x.p.CategoryID == 4)
                            .Select(x => x.p);  // Lấy lại các sản phẩm sau khi lọc

                        // Sắp xếp ngẫu nhiên trong bộ nhớ (IEnumerable) sau khi đã lọc xong
                        query = query.AsEnumerable()  // Chuyển sang IEnumerable sau khi lọc
                                     .OrderBy(x => Guid.NewGuid())  // Sắp xếp ngẫu nhiên bằng Guid
                                     .AsQueryable();  // Chuyển lại thành IQueryable để trả về
                        break;

                    case "ThoiTrang":
                        // Lọc sản phẩm có CategoryName == "Nam"
                        query = query
                            .Join(db.Categories, p => p.CategoryID, c => c.CategoryID, (p, c) => new { p, c })  // Kết nối bảng Product với bảng Category
                            .Where(x => x.p.CategoryID == 5)  // Lọc theo CategoryName là "Nam"
                            .Select(x => x.p);  // Lấy lại các sản phẩm sau khi lọc

                        query = query.AsEnumerable()  // Chuyển sang IEnumerable sau khi lọc
                                     .OrderBy(x => Guid.NewGuid())  // Sắp xếp ngẫu nhiên bằng Guid
                                     .AsQueryable();  // Chuyển lại thành IQueryable để trả về
                        break;

                    case "Automatic":
                    case "Quartz":
                    case "Solar":
                    case "Cơ":
                    case "Mechanical":
                        query = query
                            .Join(db.Details, p => p.ProductID, c => c.ProductID, (p, c) => new { p, c })
                            .Where(x => x.c.Typeof == filterType)
                            .Select(x => x.p);
                        break;

                    case "Sapphire":
                    case "Mineral":
                    case "Hardlex":
                    case "Kính Khoáng":
                        query = query
                            .Join(db.Details, p => p.ProductID, c => c.ProductID, (p, c) => new { p, c })
                            .Where(x => x.c.GlassMaterial == filterType)
                            .Select(x => x.p);
                        break;

                    case "36mm":
                    case "37mm":
                    case "38mm":
                    case "39mm":
                    case "40mm":
                    case "41mm":
                    case "42mm":
                    case "43mm":
                    case "45mm":
                    case "50mm":
                        query = query
                            .Join(db.Details, p => p.ProductID, c => c.ProductID, (p, c) => new { p, c })
                            .Where(x => x.c.FaceSize == filterType)
                            .Select(x => x.p);
                        break;

                    case "Dây Cao Su":
                    case "Dây Kim Loại":
                    case "Dây Da":
                    case "Dây Nhựa":
                    case "Dây Thép":
                    case "Dây Bạc":
                    case "Dây Inox":
                        query = query
                            .Join(db.Details, p => p.ProductID, c => c.ProductID, (p, c) => new { p, c })
                            .Where(x => x.c.Material == filterType)
                            .Select(x => x.p);
                        break;

                    case "Nhật Bản":
                    case "Switzerland":
                    case "Đức":
                    case "Anh":
                    case "Ba Lan":
                    case "Hàn Quốc":
                    case "Pháp":
                    case "Trung Quốc":
                    case "Hoa Kỳ":
                    case "Thụy Sĩ":
                    case "Ý":
                    case "Tây Ban Nha":
                    case "Thái Lan":
                        query = query
                            .Join(db.Details, p => p.ProductID, c => c.ProductID, (p, c) => new { p, c })
                            .Where(x => x.c.Origin == filterType)
                            .Select(x => x.p);
                        break;

                    default:
                        // Sắp xếp mặc định nếu không có filterType
                        query = query.OrderBy(p => p.ProductID);
                        break;
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.ProductName.Contains(search));
                }

                int totalProducts = query.Count();
                int totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

                query = query.OrderByDescending(p => p.ProductID);

                var pagedProducts = query
                    .Skip((page - 1) * pageSize) 
                    .Take(pageSize)
                    .ToList();

                // Get the list of brands and details
                var brands = db.Brands.ToList();
                var details = db.Details.ToList();

                // Create the ViewModel to pass to the view
                var viewModel = new YourViewModel
                {
                    Product = pagedProducts,
                    Details = details,
                    Brand = brands,
                    PageID = imageName
                };

                ViewBag.TotalPages = totalPages;
                ViewBag.CurrentPage = page;
                ViewBag.FilterType = filterType;
                ViewBag.Search = search;

                return View(viewModel);
            }
        }

        public ActionResult partialBrand()
        {
            WatchStore_WebEntities db = new WatchStore_WebEntities();
            var brand = db.Brands.ToList();
            return PartialView(brand);
        }
 
        public ActionResult Chi_tiet(int id)
        {
            _savedProductId = id;
            using (WatchStore_WebEntities db = new WatchStore_WebEntities())
            {
                // Lấy danh sách sản phẩm
                var products = db.Products.ToList();

                var customer = db.Customers.ToList();
                // Lấy ID sản phẩm từ hàm GetId
                //int productId = GetId();
                int productId = id;


                // Lấy sản phẩm dựa trên ID
                var selectedProduct = db.Products.FirstOrDefault(p => p.ProductID == productId);

                // Kiểm tra xem sản phẩm có tồn tại không, nếu có thì lấy tên thương hiệu và tên sản phẩm
                string brandName = null;
                string productName = null;
                string ImageUrl = null;
                int? brandId = null; // Để lưu BrandID nếu có

                if (selectedProduct != null)
                {
                    brandName = selectedProduct.Brand?.BrandName; // Lấy tên thương hiệu
                    productName = selectedProduct.ProductName; // Lấy tên sản phẩm
                    ImageUrl = selectedProduct.ImageUrl;
                    brandId = selectedProduct.BrandID; // Lấy BrandID từ sản phẩm


                }

                // Lấy danh sách chi tiết sản phẩm dựa trên ProductID
                var productDetails = db.Details.Where(d => d.ProductID == productId).ToList();

                // Lấy danh sách hình ảnh mặc định dựa trên ProductID
                var imagesDefault = db.Images_Default.Where(i => i.ProductID == productId).ToList();

                // Lấy danh sách hình ảnh thực tế dựa trên ProductID
                var imagesReality = db.Images_Reality.Where(i => i.ProductID == productId).ToList();

                // Lấy danh sách chứng nhận (Images_Certification) theo BrandID
                var ImagesCertification = db.Images_Certification
                                            .Where(i => i.BrandID == brandId) // Lọc theo BrandID
                                            .ToList();

                var Feedback = db.Feedbacks.Where(i => i.ProductID == productId).ToList();

                var Video = db.Videos.Where(i => i.ProductID == productId).ToList();


                // Lấy danh sách thương hiệu
                var brands = db.Brands.ToList();

                // Gộp dữ liệu vào ViewModel
                var viewModel = new YourViewModel
                {
                    Product = products,
                    Customer = customer,
                    Details = productDetails, // Gán danh sách chi tiết sản phẩm vào ViewModel
                    Images_Default = imagesDefault, // Gán hình ảnh mặc định vào ViewModel
                    Images_Realities = imagesReality, // Gán hình ảnh thực tế vào ViewModel4
                    images_Certifications = ImagesCertification,

                    Videos = Video,
                    Brand = brands,
                    ProductId = productId, // Gán ID sản phẩm vào ViewModel
                    BrandName = brandName, // Gán tên thương hiệu vào ViewModel
                    ProductName = productName,// Gán tên sản phẩm vào ViewModel
                    ImageUrl = ImageUrl,
                    feedbacks = Feedback,
                };

                // Trả về View với ViewModel đã chuẩn bị
                return View(viewModel);
            }
        }

        // Kiệt
        public ActionResult HistoryOrder()
        {
            WatchStore_WebEntities db = new WatchStore_WebEntities();
            List<OrderItem> ordersItem = db.OrderItems.ToList(); // Or fetch an existing order
            return View(ordersItem);
        }

        public ActionResult Find(string searchQuery)
        {
            return RedirectToAction("BanHang", new { search = searchQuery });
        }

        // Khanh
        public ActionResult GioHang()
        {
            WatchStore_WebEntities db = new WatchStore_WebEntities();
            List<OrderItem> orderItems = db.OrderItems.ToList();
            var provinces = db.provinces.Select(p => new { p.province_id, p.province_name }).ToList();

            // Truyền danh sách tỉnh vào View dưới dạng SelectList
            ViewBag.Provinces = new SelectList(provinces, "province_id", "province_name");
            return View(orderItems);
        }
        [HttpGet]
        // Quận
        public ActionResult GetDistricts(int provinceId)
        {
            WatchStore_WebEntities db = new WatchStore_WebEntities();

            // Lấy danh sách quận/huyện tương ứng với provinceId
            var districts = db.districts
                              .Where(d => d.province_id == provinceId)
                              .Select(d => new { d.district_id, d.name_district })
                              .ToList();

            // Trả về một phần tử JSON để cập nhật dropdown
            return Json(districts, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        // xã 
        public ActionResult GetWards(int districtId)
        {
            WatchStore_WebEntities db = new WatchStore_WebEntities();

            // Lấy danh sách phường/xã tương ứng với districtId
            var wards = db.wards
                          .Where(w => w.district_id == districtId)
                          .Select(w => new { w.wards_id, w.wards_name })
                          .ToList();

            // Trả về một phần tử JSON để cập nhật dropdown phường/xã
            return Json(wards, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult IncreaseQuantity(int orderItemId)
        {
            System.Diagnostics.Debug.WriteLine($"Received OrderItemID: {orderItemId}");

            using (WatchStore_WebEntities db = new WatchStore_WebEntities())
            {
                var orderItem = db.OrderItems.FirstOrDefault(o => o.OrderItemID == orderItemId && o.Order.OrderStatus == 0);
                if (orderItem != null)
                {
                    orderItem.Quantity += 1;
                    orderItem.UnitPrice = orderItem.Quantity * orderItem.Product.Price * (100 - orderItem.Product.Discount) / 100;    
                    db.SaveChanges();

                    return Json(new { success = true, newQuantity = orderItem.Quantity, newPrice = orderItem.UnitPrice }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public JsonResult DecreaseQuantity(int orderItemId)
        {
            System.Diagnostics.Debug.WriteLine($"Received OrderItemID: {orderItemId}");

            using (WatchStore_WebEntities db = new WatchStore_WebEntities())
            {
                var orderItem = db.OrderItems.FirstOrDefault(o => o.OrderItemID == orderItemId && o.Order.OrderStatus == 0);
                if (orderItem != null)
                {
                    bool showConfirmation = false;

                    if (orderItem.Quantity == 1)
                    {
                        showConfirmation = true;
                    }
                    else
                    {
                        orderItem.Quantity -= 1; // Decrease quantity
                        orderItem.UnitPrice = orderItem.Quantity * orderItem.Product.Price * (100 - orderItem.Product.Discount) / 100;
                        db.SaveChanges();
                    }

                    return Json(new { success = true, newQuantity = orderItem.Quantity, newPrice = orderItem.UnitPrice, showConfirmation }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = false });
            }
        }
       
        public ActionResult AddOrderItem()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddOrderItem(int productId)
        {
            using (WatchStore_WebEntities db = new WatchStore_WebEntities())
            {
                string userEmail = Session["UserEmail"]?.ToString();
                if (string.IsNullOrEmpty(userEmail))
                    return RedirectToAction("Login", "Account");

                Random random = new Random();

                var product = db.Products.FirstOrDefault(p => p.ProductID == productId);
                var customer = db.Customers.FirstOrDefault(c => c.Email == userEmail);

                if (product == null || customer == null)
                    return RedirectToAction("GioHang");

                // Find an existing cart order (OrderStatus == 0) for the current customer
                var existingOrder = db.Orders
                    .FirstOrDefault(o => o.CustomerID == customer.CustomerID && o.OrderStatus == 0);

                if (existingOrder == null)
                {
                    // Create a new order if no existing cart order is found
                    existingOrder = new Order
                    {
                        OrderDate = DateTime.Now,
                        CustomerID = customer.CustomerID,
                        Discount_Code = random.Next(5, 15),
                        OrderStatus = 0,
                        Status = "Pending",
                    };
                    db.Orders.Add(existingOrder);
                    db.SaveChanges();
                }

                // Check if the product is already in the cart
                var existingOrderItem = db.OrderItems
                    .FirstOrDefault(oi => oi.OrderID == existingOrder.OrderID && oi.ProductID == productId);

                if (existingOrderItem != null)
                {
                    // Increase the quantity if the item already exists
                    existingOrderItem.Quantity += 1;
                }
                else
                {
                    // Add a new item to the cart if it doesn't exist
                    OrderItem newOrderItem = new OrderItem
                    {
                        OrderID = existingOrder.OrderID,
                        ProductID = productId,
                        Quantity = 1,
                        UnitPrice = product.Price * (100 - product.Discount) / 100,
                    };
                    db.OrderItems.Add(newOrderItem);
                }

                db.SaveChanges();
                return RedirectToAction("Xuhuong");
            }
        }

        public ActionResult DeleteOrderItem(int id)
        {
            using (WatchStore_WebEntities db = new WatchStore_WebEntities())
            {
                // Find the order by ID
                var orderToDelete = db.OrderItems.FirstOrDefault(o => o.OrderItemID == id && o.Order.OrderStatus == 0);

                if (orderToDelete != null)
                {
                    db.OrderItems.Remove(orderToDelete);
                    db.SaveChanges();
                }
                return RedirectToAction("GioHang");
            }
        }

        [HttpPost]
        public ActionResult Accept(int? id, FormCollection form)
        {
            decimal emailTotalPrice = 0;
            using (WatchStore_WebEntities db = new WatchStore_WebEntities())
            {
                string userEmail = Session["UserEmail"]?.ToString();
                if (string.IsNullOrEmpty(userEmail))
                    return RedirectToAction("Login", "Account");

                var orderSelected = db.Orders.FirstOrDefault(i => i.Customer.Email == userEmail && i.OrderStatus == 0);
                if (orderSelected == null)
                    return RedirectToAction("GioHang");


                string hoTen = form["HoTen"];
                if (!string.IsNullOrEmpty(hoTen))
                {
                    orderSelected.ReceiverName = hoTen; // Assign to ReceiverName field
                }

                // Retrieve form values
                string diaChi = form["DiaChi"]; // House number
                string idTinhThanh = form["Tinh"];  // Province
                string idQuanHuyen = form["QuanHuyen"]; // District
                string idPhuongXa = form["PhuongXa"];   // Ward
                string note = form["GhiChu"];
                string email = form["Email"];
                string voucher = form["MaGiamGia"];

                string nameTinh = db.provinces
                               .Where(p => p.province_id.ToString() == idTinhThanh)
                               .Select(p => p.province_name)
                               .FirstOrDefault();

                string nameHuyen = db.districts
                               .Where(p => p.district_id.ToString() == idQuanHuyen)
                               .Select(p => p.name_district)
                               .FirstOrDefault();

                string nameXa = db.wards
                               .Where(p => p.wards_id.ToString() == idPhuongXa)
                               .Select(p => p.wards_name)
                               .FirstOrDefault();

                // Combine dropdown values for ReceiverAddress
                if (!string.IsNullOrEmpty(nameTinh) && !string.IsNullOrEmpty(nameHuyen) && !string.IsNullOrEmpty(nameXa))
                {
                    orderSelected.ReceiverAddress = $"{nameTinh} - {nameHuyen} - {nameXa}";
                }

                // Assign other fields
                if (!string.IsNullOrEmpty(hoTen))
                {
                    orderSelected.ReceiverName = hoTen;
                }

                if (!string.IsNullOrEmpty(diaChi))
                {
                    orderSelected.HouseNumber = diaChi;
                }
                if (!string.IsNullOrEmpty(note))
                {
                    orderSelected.Note = note;
                }
                if (!string.IsNullOrEmpty(email))
                {
                    orderSelected.ReceiverEmail = email;
                }

                var orderItems = db.OrderItems
                                   .Where(oi => oi.Order.Customer.Email == userEmail && oi.Order.OrderStatus == 0)
                                   .ToList();

                var usedVoucher = db.Vouchers.FirstOrDefault(v => v.Code == voucher);
                if (usedVoucher != null)
                {
                    if (usedVoucher.Type == "P")
                    {
                        emailTotalPrice = (decimal)orderItems.Sum(oi => oi.UnitPrice) * (100 - usedVoucher.Value) / 100;
                        orderSelected.TotalPrice = orderItems.Sum(oi => oi.UnitPrice) * (100 - usedVoucher.Value) / 100;
                    }
                    else
                    {
                        emailTotalPrice = (decimal)orderItems.Sum(oi => oi.UnitPrice) - usedVoucher.Value;
                        orderSelected.TotalPrice = orderItems.Sum(oi => oi.UnitPrice) - usedVoucher.Value;
                    }

                    usedVoucher.RemainingUses--;
                }
                else
                {
                    emailTotalPrice = (decimal)orderItems.Sum(oi => oi.UnitPrice);
                    orderSelected.TotalPrice = orderItems.Sum(oi => oi.UnitPrice);

                }

                orderSelected.OrderStatus = 1;
                db.SaveChanges();

                try
                {
                    var smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("watchstore4conga@gmail.com", "wfxx gjdt ucie kzdk"),
                        EnableSsl = true, // Bật SSL để bảo mật
                    };

                    string mailBody = "Đã đặt hàng lúc " + DateTime.Now + ":\n";
                    foreach (var item in db.OrderItems)
                    {
                        if(item.OrderID == orderSelected.OrderID)
                        {
                            mailBody += item.Product.ProductName + " x " + item.Quantity + "\n";
                        }
                    }

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("watchstore4conga@gmail.com"), // Địa chỉ email gửi
                        Subject = "Lịch sử đặt hàng.",
                        Body = mailBody + "\nTổng giá trị: " + emailTotalPrice,
                        IsBodyHtml = true, 
                    };

                    mailMessage.To.Add(Session["UserEmail"]?.ToString());

                    // Gửi email
                    smtpClient.Send(mailMessage);
                    TempData["ErrorMessage"] = "Gửi email thành công tới: " + Session["UserEmail"]?.ToString();
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Gửi email thất bại tới: " + Session["UserEmail"]?.ToString() + $" { ex.Message}";
                }
            }
            return RedirectToAction("GioHang");
        }

        [HttpPost]
        public ActionResult SubmitFeedback(string ChiaSe, string HoTen, string SDT, int? Rating, HttpPostedFileBase ImageFile, int ProductId)
        {
            WatchStore_WebEntities db = new WatchStore_WebEntities();
            if (ModelState.IsValid)
            {
                // Kiểm tra xem Rating có được cung cấp không
                if (Rating == null)
                {
                    ModelState.AddModelError("Rating", "Bạn cần cung cấp đánh giá sao.");
                    return View();
                }

                string userEmail = Session["UserEmail"]?.ToString();
                if (string.IsNullOrEmpty(userEmail))
                    return RedirectToAction("Login", "Account");


                var customer = db.Customers.FirstOrDefault(c => c.Email == userEmail);

                if ( customer == null)
                    return RedirectToAction("Xuhuong");

                var existingFeedback = db.Feedbacks
                    .FirstOrDefault(o => o.CustomerID == customer.CustomerID );

              


                int feedbackCount = db.Feedbacks.Count(f => f.ProductID == ProductId);

                // Tạo tên file hình ảnh
                string imageFileName = $"{feedbackCount + 1}.jpg";

                // Lưu hình ảnh vào thư mục D:\Uploads nếu có file
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    // Chỉ định đường dẫn tuyệt đối tới thư mục trên ổ D
                    var filePath = Path.Combine(@"D:\Uploads\", imageFileName);
                    ImageFile.SaveAs(filePath);
                }


                // Lưu feedback vào database
                var feedback = new Feedback
                {
                    ProductID = ProductId,
                    CustomerID = customer.CustomerID,
                    FeedbackText = ChiaSe,
                    Rating = Rating.Value, // Sử dụng Rating.Value vì Rating là kiểu nullable
                    ImageFeedBack = imageFileName,
                    CreatedAt = DateTime.Now
                };

                db.Feedbacks.Add(feedback);
                db.SaveChanges();

                return RedirectToAction("Xuhuong");
            }

            return View();
        }

        [HttpPost]
        public JsonResult checkVoucher(string code, decimal subtotal)
        {
            WatchStore_WebEntities db = new WatchStore_WebEntities();   
            var vouchers = db.Vouchers.ToList();
            var selectedVoucher = vouchers.FirstOrDefault(v => v.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

            var voucher = vouchers.FirstOrDefault(v =>
                v.Code.Equals(code, StringComparison.OrdinalIgnoreCase) &&
                v.RemainingUses > 0 &&
                v.status == 1 &&
                DateTime.Now < v.ExpDate &&
                subtotal >= v.MinOrderValue
            );

            if (voucher != null)
            {
                return Json(new
                {
                    success = true,
                     code = voucher.Code,
                remaining = voucher.RemainingUses,
                discount = voucher.Value,
                vstatus = voucher.status,
                date = DateTime.Now,
                subtotal1 = subtotal,
                typeVoucher = voucher.Type.ToString(),
                });
            }
            else
            {
                if(selectedVoucher.RemainingUses == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Voucher đạt giới hạn sử dụng."
                    });
                }

               else if (selectedVoucher.status == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Voucher không còn được áp dụng."
                    });
                }

               else if (selectedVoucher.ExpDate < DateTime.Now)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Voucher quá hạn sử dụng."
                    });
                }

                else if (subtotal < selectedVoucher.MinOrderValue)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Voucher áp dụng cho đơn hàng trên " + selectedVoucher.MinOrderValue.ToString() + "₫",
                    });
                }
                return Json(new
                {
                    success = false,
                    message = "Lỗi gì ai biết đâu",
                });
            }
        }
    }
}
