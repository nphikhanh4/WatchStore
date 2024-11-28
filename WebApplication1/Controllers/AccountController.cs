using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        WatchStore_WebEntities db = new WatchStore_WebEntities();
        // GET: Account
       
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(register sign)
        {
            if (ModelState.IsValid)
            {
                if (db.Customers.Any(x => x.Email == sign.Email) || db.Admins.Any(x => x.Email == sign.Email))
                {
                    ViewBag.Message = " Email already registered";
                }
                else
                {
                    var newCustomer = new Customer
                    {
                        FullName = sign.FullName,
                        Email = sign.Email,
                        Password = sign.Password,
                        Phone = sign.Phone,
                        Gender = sign.Gender,
                        Address = sign.Address
                    };
                    db.Customers.Add(newCustomer);
                    db.SaveChanges();
                    return RedirectToAction("Login", "Account");

                }
            }

            return View();
        }
       
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(login l)
        {
            var query1 = db.Customers
                  .FirstOrDefault(m => m.Email == l.Email && m.Password == l.Password);
            var query2 = db.Admins
                           .FirstOrDefault(a => a.Email == l.Email && a.Password == l.Password);
            if (query1 != null)
            {
                Session["UserLoggedIn"] = query1.FullName; // Lưu tên người dùng vào session
                Session["userEmail"] = query1.Email;
                Session["userId"] = query1.CustomerID;
                return RedirectToAction("Xuhuong", "Xuhuong");
            }
            else if (query2 != null)
            {
                Session["AddminLoggedIn"] = query2.FullName; // Lưu tên admin vào session

                return RedirectToAction("Index", "Adimin");
            }
            else
            {
                ModelState.AddModelError("", "Tên người dùng hoặc mật khẩu không đúng.");
            }
            return View();
        }

        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Xuhuong", "Xuhuong");
        }

        public ActionResult ProfileUser()
        {
            List<Customer> customers = db.Customers.ToList();
            return View(customers);
        }
       
        public ActionResult EditProfileUser()
        {
            // Lấy email từ session để xác định người dùng hiện tại
            string userEmail = Session["userEmail"]?.ToString();
            if (userEmail == null) return RedirectToAction("Login", "Account");

            var customer = db.Customers.FirstOrDefault(c => c.Email == userEmail);
            if (customer == null) return HttpNotFound();

            return View(customer);
        }

        [HttpPost]
        public ActionResult EditProfileUser(Customer model)
        {
            if (ModelState.IsValid)
            {
                var customer = db.Customers.FirstOrDefault(c => c.CustomerID == model.CustomerID);
                if (customer != null)
                {
                    customer.FullName = model.FullName;
                    customer.Email = model.Email;
                    customer.Phone = model.Phone;
                    customer.Address = model.Address;
                    db.SaveChanges();  // Lưu thay đổi vào cơ sở dữ liệu
                    ViewBag.Message = "Profile updated successfully!";
                    return RedirectToAction("ProfileUser");
                }
            }
            return View(model);
        }

        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(login l)
        {
            var selectedCustomer = db.Customers.FirstOrDefault(m => m.Email == l.Email);
            if (selectedCustomer != null)
            {
                var random = new Random();
                var otpCode = random.Next(100000, 999999).ToString();

                try
                {
                    var smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587, // Cổng SMTP của Gmail
                        Credentials = new NetworkCredential("watchstore4conga@gmail.com", "wfxx gjdt ucie kzdk"),
                        EnableSsl = true, // Bật SSL để bảo mật
                    };

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("watchstore4conga@gmail.com"), // Địa chỉ email gửi
                        Subject = "OTP đổi mật khẩu.",
                        Body = "Mã OTP của bạn là: " + otpCode,
                        IsBodyHtml = true,
                    };
                    mailMessage.To.Add(selectedCustomer.Email);
                    smtpClient.Send(mailMessage);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Gửi email thất bại tới: " + selectedCustomer.Email + $" {ex.Message}";
                }

                var newOtp = new OTP
                {
                    CustomerID = selectedCustomer.CustomerID, 
                    OTPCode = otpCode, 
                    ExpiresAt = DateTime.Now.AddMinutes(10) // OTP hết hạn sau 10 phút
                };

                db.OTPs.Add(newOtp);
                db.SaveChanges();

                Session["selectedEmail"] = selectedCustomer.Email;
                return RedirectToAction("ConfirmOTP");
            }
            else
            {
                ModelState.AddModelError("", "Email không được tìm thấy!");
            }
            return View();
        }

        public ActionResult ConfirmOTP()
        {
            string selectedEmail = Session["selectedEmail"].ToString();
            if (selectedEmail == null)
                return RedirectToAction("ResetPassword");
            return View();
        }

        [HttpPost]
        public ActionResult ConfirmOTP(OTP o)
        {
            string selectedEmail = Session["selectedEmail"].ToString();
            if(selectedEmail == null)
                return RedirectToAction("ResetPassword");

            var selectedConfirmOTP = db.OTPs.FirstOrDefault(m => m.OTPCode == o.OTPCode && m.ExpiresAt > DateTime.Now && m.Customer.Email == selectedEmail);
            if (selectedConfirmOTP != null)
            {

                return RedirectToAction("ChangePassword");
            }
            else
            {
                ModelState.AddModelError("", "Mã OTP không đúng hoặc đã hết hạn");
            }
            return View();
        }

        public ActionResult ChangePassword()
        {
            string selectedEmail = Session["selectedEmail"].ToString();
            if (selectedEmail == null)
                return RedirectToAction("ResetPassword");
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(login l)
        {
            string selectedEmail = Session["selectedEmail"]?.ToString();
            var selectedCustomer = db.Customers.FirstOrDefault(c => c.Email == selectedEmail);

            if (string.IsNullOrEmpty(l.Password))
                ModelState.AddModelError("Password", "Mật khẩu không được để trống.");
            
            else if (string.IsNullOrEmpty(l.ConfirmPassword))
                ModelState.AddModelError("ConfirmPassword", "Xác nhận mật khẩu không được để trống.");

            else if (l.Password != l.ConfirmPassword)
                ModelState.AddModelError("ConfirmPassword", "Xác nhận mật khẩu không khớp.");

            else
            {
                if (selectedCustomer != null)
                {
                    selectedCustomer.Password = l.Password;
                    db.SaveChanges();

                    return RedirectToAction("Login");
                }

                else
                {
                    ModelState.AddModelError("", "Không tìm thấy khách hàng với email đã chọn.");
                    return View();
                }
            }
            return View();
        }
    }
}