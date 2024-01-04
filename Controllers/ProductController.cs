using Jewelly.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.EnterpriseServices.CompensatingResourceManager;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.Xml;
using System.Web;
using System.Web.Mvc;
using static System.Collections.Specialized.BitVector32;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace Jewelly.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        JwelleyEntities db = new JwelleyEntities();
        public ActionResult Cart()
        {
            if (Session["username"] != null)
            {
                if (TempData["result"] != null)
                {
                    ViewBag.SuccessMg = TempData["result"];
                }
                if (TempData["error"] != null)
                {
                    ViewBag.ErrorMg = TempData["error"];
                }
                int user = (int)Session["userID"];
                var shoppingCarts = db.ShoppingCarts.Where(s => s.User_id == user).ToList();
                return View(shoppingCarts);
            }
            else
            {
                RedirectToAction("Jewelry", "Product");
            }
            return View();
        }

        public ActionResult Jewelry(decimal? MinPrice, decimal? MaxPrice, int? Brandtype, int? jewelry, int? Gold, int? Categorytype, int? stoneq, string prices)
        {
            var model = new Join().SelectProduct(MinPrice, MaxPrice, Brandtype, Gold, jewelry, Categorytype, stoneq, prices).ToList();
            List<BrandMst> brand = db.BrandMsts.ToList();
            List<JewelTypeMst> jewe = db.JewelTypeMsts.ToList();
            List<CatMst> gold = db.CatMsts.ToList();
            List<GoldKrtMst> gold_t = db.GoldKrtMsts.ToList();
            List<StoneQltyMst> stone = db.StoneQltyMsts.ToList();
            dynamic models1 = new ExpandoObject();
            models1.Brand = brand;
            models1.Producter = model;
            models1.Jewe = jewe;
            models1.Cate = gold;
            models1.GoldType = gold_t;
            models1.Stone = stone;
            return View(models1);

        }

        public ActionResult Details(int id)
        {
            var model = new JoinDetails().SelectDetails(id).ToList();
            dynamic models = new ExpandoObject();
            models.Details = model;
            return View(models);
        }




        [HttpGet]
        public ActionResult AddtoCart(int Id, string Img, string Path, string Name, decimal? Price, string Prod, int quantity = 1)
        {
            if (Session["Username"] != null)
            {
                int user = (int)Session["userID"];
                Cocart cocart = new Cocart();
                cocart.Add(Id, user, Img, Path, Name, Price, Prod, quantity);
                return RedirectToAction("Details/" + Id, "Product");
            }
            else
            {
                return RedirectToAction("Payment", "Product");
            }


        }
        public ActionResult ShowOrder()
        {
            if (Session["Username"] == null)
            {
                return Content("You are not logged in, Please log in!");

            }
            int username = (int)Session["userID"];
            var users = db.ShoppingCarts.Where(s => s.User_id == username).ToList();
            var money = db.ShoppingCarts.Where(s => s.User_id == username).Sum(s => s.Total);
            dynamic model = new ExpandoObject();
            model.Details = users;
            model.Money = money;
            return View(model);
        }

        public PartialViewResult BagCartTotal()
        {
            var total = 0;
            if (Session["username"] != null)
            {

                var user = (int)Session["userID"];
                var cart = db.ShoppingCarts.Where(row => row.User_id == user);
                var check = db.ShoppingCarts.Where(row => row.Quantity == 0);
                if (cart.Count() > 0 && check != null)
                {
                    total = (int)cart.Sum(s => s.Quantity);
                }
                else if (cart == null && check == null)
                {
                    total = 0;

                }

            }

            ViewBag.QuantityCart = total;
            return PartialView("BagCartTotal");
        }
        public void Update_Quantity_Shopping(int id, int quantity, int itemid)
        {
            var user = (int)Session["userID"];
            var exc = db.ShoppingCarts.Where(c => c.User_id == user && c.item_id == itemid).FirstOrDefault();
            if (exc != null)
            {
                exc.Quantity = quantity;
                db.SaveChanges();
            }
        }


        public ActionResult Delete(int id)
        {
            if (Session["Username"] != null)
            {
                int username = (int)Session["userID"];

                var cart = db.ShoppingCarts.Where(s => s.ID == id && s.User_id == username).FirstOrDefault();
                if (cart != null)
                {
                    db.ShoppingCarts.Remove(cart);
                    db.SaveChanges();

                    TempData["result"] = "Delete Product in cart successfully!";
                    return RedirectToAction("ShowOrder", "Product");
                }
                var item = db.ShoppingCarts.Where(s => s.User_id == username).ToList();
                if (item == null)
                {
                    return RedirectToAction("ShowOrder", "Product");

                }
                else
                {
                    return RedirectToAction("Cart", "Product");

                }


            }
            else
            {
                TempData["error"] = "You need to log in, please log in to continue!";
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult CheckOut()
        {
            //if (TempData["result"] != null)
            //{
            //    ViewBag.SuccessMg = TempData["result"];
            //}
            //if (TempData["error"] != null)
            //{
            //    ViewBag.ErrorMg = TempData["error"];
            //}
            if (Session["Username"] != null)
            {
                var total_item = 0;
                var user = (int)Session["userID"];
                var shopping = db.ShoppingCarts.Where(s => s.User_id == user).ToList();
                var money = db.ShoppingCarts.Where(s => s.User_id == user).Sum(s => s.Total);

                dynamic model = new ExpandoObject();
                model.Shop = shopping;
                model.Money = money;
                return View(model);
            }
            else
            {
                TempData["error"] = "You need to log in, please log in to continue!";
                return RedirectToAction("Login", "Account");
            }
        }
        [HttpPost]
        public ActionResult CheckedOut(FormCollection form)
        { 
            int id = (int)Session["userID"];
            var cart = db.ShoppingCarts.Where(s => s.User_id == id).ToList();
            if(cart != null)
            {
                CartList cartList = new CartList();
                cartList.ShipCode = form["payment"];
                if (cartList.ShipCode == "Visa")
                {
                    Payment payments = new Payment();
                    payments.numbercard = form["numbercard"];
                    payments.cgv = int.Parse(form["cgv"]);
                    payments.expiration_date = DateTime.Parse(form["dateend"]);
                    payments.type = "Visa";
                    db.Payments.Add(payments);
                }
                else
                {
                    cartList.payment_ID = null;
                }
                cartList.payment_ID = 1;
                cartList.userID = null;
                cartList.userName = "an1203";
                cartList.ShipCode = "Visa";
                cartList.OrderDate = DateTime.Now.ToString();
                cartList.MRP = decimal.Parse(form["mrp"]);
                cartList.ShipName = form["name"];
                cartList.Email_ID = form["email"];
                cartList.Phone = form["phone"];
                cartList.ShipAddress = form["address"];
                cartList.ShipCity = form["city"];
                cartList.ShipCountry = form["country"];
                cartList.Status = "Pending";
                cartList.Note = form["note"];
                cartList.Product_Name = form["product"];

                db.CartLists.Add(cartList);
                foreach (var item in cart)
                {
                    Orderdetail details = new Orderdetail();
                    details.ID = cartList.ID;
                    details.Style_Code = item.item_id;
                    details.Quantity = (int?)item.Quantity;
                    details.UnitPrice = item.Price;
                    db.Orderdetails.Add(details);
                }
                db.SaveChanges();
                return RedirectToAction("ShoppingSuccess", "Product");
            }
                return RedirectToAction("ShoppingSuccess", "Product");

        }

        public ActionResult Payment()
        {
            return View();
        }
    }
}

       