using Microsoft.AspNet.Identity;
using neMag.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

public class CartOrderAuthorize : AuthorizeAttribute
{
    public override void OnAuthorization(AuthorizationContext filterContext)
    {
        // If they are authorized, handle accordingly
        if (this.AuthorizeCore(filterContext.HttpContext))
        {
            base.OnAuthorization(filterContext);
        }
        else
        {
            // Otherwise redirect to your specific authorized area
            filterContext.Result = new RedirectResult("~/Products/Index");
        }
    }
}

namespace neMag.Controllers
{
    public class CosController : Controller
    {
        private Models.ApplicationDbContext db = new Models.ApplicationDbContext();
        // consts for possible status of an order
        private const string CART = "Cart";
        private const string SENT = "Sent";
        private const string DONE = "Done";


        [Authorize(Roles = "RestrictedUser,User,Admin")]
        public ActionResult Index()
        {
            var uid = User.Identity.GetUserId();
            UpdateCartValue(); // might be unneccessary
            var cart = GetCart(); 
            var cartElems = cart.OrderContents.Select(oc => oc.Product); // some handy c# stuff
            var orderCons = cart.OrderContents;
            ViewBag.CartElems = cartElems;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            return View(cart);
        }

        [Authorize(Roles = "Collaborator,Admin")]
        public ActionResult OrdersFromMe()
        {
            IEnumerable<int> orderids = (from order in db.Orders
                                       where order.Status == SENT
                                       select order.OrderId).ToList();
            string uid = User.Identity.GetUserId();
            IEnumerable<int> productids = (from pr in db.Products
                                           where pr.UserId == uid
                                           select pr.ProductId).ToList();
            List<OrderContent> Contents = new List<OrderContent>();
            
            foreach (var x in orderids)
            {
                IEnumerable<OrderContent> ocs = (from oc in db.OrderContents
                                                where oc.Order.OrderId == x
                                                select oc);
                foreach (var y in ocs)
                {
                    if(productids.Contains(y.Product.ProductId))
                    {
                        Contents.Add(y);
                    }
                }
            }
            ViewBag.Contents = Contents;
            return View();
        }

        [HttpPut]
        public ActionResult AddToOrder(int id)
        {
            if(!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            string uid = User.Identity.GetUserId();
            Order cart = GetCart();
            Product product = db.Products.Find(id);
            IEnumerable<OrderContent> alreadyOrdered = (from oc in db.OrderContents
                                                 where oc.Product.ProductId == id && oc.Order.OrderId == cart.OrderId
                                                select oc).ToList();
            if (product.Accepted == false)
            {
                TempData["message"] = "Produsul nu poate fi comandat";
                return RedirectToAction("Index","Products");
            }
            if (alreadyOrdered.Count() == 1)
            {
                /**
                 * If the product already exists in the cart, there's no need for a new OrderContent.
                 * We can just search for the already existing OC and update its values.
                 * */
                OrderContent sameOrder = alreadyOrdered.First();
                if(TryUpdateModel(sameOrder))
                {
                    sameOrder.Quantity++;
                    sameOrder.Total += product.Price - product.Price * product.Discount;
                    db.SaveChanges(); //TODO: check if this is neccessary
                    UpdateCartValue();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Eroare neidentificată la adăugarea unui produs în coș";
                    return View("Index");
                }
            }
            else
            {
                OrderContent oc = new OrderContent();
                oc.Order = cart;
                oc.Product = product;
                oc.Quantity = 1;
                oc.Total = product.Price - product.Price * product.Discount;
                cart.OrderContents.Add(oc);
                db.OrderContents.Add(oc);
                db.SaveChanges();
                UpdateCartValue();
                TempData["message"] = "Produs adăugat în coș";
                return RedirectToAction("Index");
            }

        }

        [HttpDelete]
        [Authorize(Roles = "RestrictedUser,User,Admin")]
        public ActionResult Delete(int id)
        {
            OrderContent toDelete = db.OrderContents.Find(id);
            Order cart = toDelete.Order;
            cart.OrderContents.Remove(toDelete);
            db.OrderContents.Remove(toDelete);
            db.SaveChanges();
            UpdateCartValue();
            TempData["message"] = "Produsul a fost scos din coș";
            return RedirectToAction("Index");
        }

        [HttpDelete]
        [Authorize(Roles = "Collaborator,Admin")]
        public ActionResult EditDelete(int id,int page)
        {
            // page = 1 => redirect to show, page = 2 => redirect to OrdersFromMe
            OrderContent toDelete = db.OrderContents.Find(id);
            Order order = toDelete.Order;
            if (order.Status == SENT)
            {
                order.OrderContents.Remove(toDelete);
                db.OrderContents.Remove(toDelete);
                db.SaveChanges();
                TempData["message"] = "Produsul a fost scos din comanda";
            }
            else
            {
                ViewBag.message = "Comanda finalizata nu poate fi modificata";

            }
            if (page == 1)
            {
                return RedirectToAction("Show/"+order.OrderId);
            }
            else
            {
                return RedirectToAction("OrdersFromMe");
            }
        }



        [HttpPut]
        [Authorize(Roles = "RestrictedUser,User,Admin")]
        public ActionResult Increase(int id)
        {
            OrderContent oc = db.OrderContents.Find(id);
            if (TryUpdateModel(oc))
            {
                oc.Quantity++;
                oc.Total += oc.Product.Price - oc.Product.Price * oc.Product.Discount;
                db.SaveChanges();
                UpdateCartValue();
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Eroare la creșterea numărului de produse din coș";
                return RedirectToAction("Index");
            }
        }

        [HttpPut]
        [Authorize(Roles = "RestrictedUser,User,Admin")]
        public ActionResult Decrease(int id)
        {
            OrderContent oc = db.OrderContents.Find(id);
            if (TryUpdateModel(oc))
            {
                oc.Quantity--;
                oc.Total -= oc.Product.Price - oc.Product.Price * oc.Product.Discount;
                db.SaveChanges();
                UpdateCartValue();
                if(oc.Quantity == 0)
                {
                    Order cart = oc.Order;
                    cart.OrderContents.Remove(oc);
                    db.OrderContents.Remove(oc);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Eroare la scaderea numărului de produse din coș";
                return RedirectToAction("Index");
            }
        }
        [HttpPut]
        [Authorize(Roles = "Collaborator,Admin")]
        public ActionResult EditIncrease(int id,int page)
        {
            OrderContent oc = db.OrderContents.Find(id);
            string status = (from or in db.Orders
                          where or.OrderId == oc.Order.OrderId
                          select or.Status).First();
            if (status == SENT)
            {
                if (TryUpdateModel(oc))
                {
                    oc.Quantity++;
                    oc.Total += oc.Product.Price - oc.Product.Price * oc.Product.Discount;
                    db.SaveChanges();
                    UpdateCartValue();
                }
                else
                {
                    TempData["message"] = "Eroare la creșterea numărului de produse din coș";
                }

            }
            else
            {
                ViewBag.message = "Comanda finalizata nu poate fi modificata";
            }

            if (page == 1)
            {
                return RedirectToAction("Show/" + oc.Order.OrderId);
            }
            else
            {
                return RedirectToAction("OrdersFromMe");
            }
        }

        [HttpPut]
        [Authorize(Roles = "Collaborator,Admin")]
        public ActionResult EditDecrease(int id, int page)
        {
            OrderContent oc = db.OrderContents.Find(id);
            string status = (from or in db.Orders
                             where or.OrderId == oc.Order.OrderId
                             select or.Status).First();
            if (status == SENT)
            {
                if (TryUpdateModel(oc))
                {
                    oc.Quantity--;
                    oc.Total -= oc.Product.Price - oc.Product.Price * oc.Product.Discount;
                    db.SaveChanges();
                    UpdateCartValue();
                    if (oc.Quantity == 0)
                    {
                        Order cart = oc.Order;
                        cart.OrderContents.Remove(oc);
                        db.OrderContents.Remove(oc);
                        db.SaveChanges();
                    }
                }
                else
                {
                    TempData["message"] = "Eroare la scaderea numărului de produse din coș";
                }

            }
            else
            {
                ViewBag.message = "Comanda finalizata nu poate fi modificata";
            }

            
            if (page == 1)
            {
                return RedirectToAction("Show/" + oc.Order.OrderId);
            }
            else
            {
                return RedirectToAction("OrdersFromMe");
            }
        }
        /**
         * If it's not clear enough, this view serves as the final part of the ordering process.
         * The user inputs some extra details and places the order.
         * Might be subject to renaming in the future, it could get confusing to have a view with the
         * same name as the model.
         **/
        [Authorize(Roles = "RestrictedUser,User,Admin")]
        public ActionResult Order(int id)
        {
            Order cart = db.Orders.Find(id);
            return View(cart);
        }

        [HttpPut]
        [Authorize(Roles = "RestrictedUser,User,Admin")]
        public ActionResult Order(int id, Order order)
        {
            Order cart = db.Orders.Find(order.OrderId);
            try
            {
                if(ModelState.IsValid)
                {
                    if(TryUpdateModel(cart))
                    {
                        cart.Details = order.Details;
                        cart.Status = SENT;
                        cart.Date = DateTime.Now;
                        db.SaveChanges();
                        TempData["message"] = "Comanda a fost trimisă";
                        Order newCart = GetCart(); // create a new cart, might be unnecessary
                        return RedirectToAction("Index", "Products");
                    }
                    return View(cart);
                }
                return View(cart);
            }
            catch(Exception e)
            {
                return View(cart);
            }
        }

        // Admin-only history of all orders issued
        [Authorize(Roles = "Admin")]
        public ActionResult AllOrders()
        {
            ViewBag.Orders = db.Orders.ToList();
            ViewBag.CART = CART;
            ViewBag.SENT = SENT;
            ViewBag.DONE = DONE;
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Finish(int id)
        {
            Order order = db.Orders.Find(id);
            order.Status = DONE;
            db.SaveChanges();
            return RedirectToAction("AllOrders");
        }
        // secondFinish method is a copy of Finish that redirects to another page
        // is used only when calling from the Orders/1 page
        [Authorize(Roles = "Admin")]
        public ActionResult secondFinish(int id)
        {
            Order order = db.Orders.Find(id);
            order.Status = DONE;
            db.SaveChanges();
            return RedirectToAction("Orders/1");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Show(int id)
        {
            Order order = db.Orders.Find(id);
            ViewBag.SENT = SENT;
            return View(order);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Orders(int id)
        {

            var orderList = db.Orders.ToList();
            ViewBag.CART = CART;
            ViewBag.SENT = SENT;
            ViewBag.DONE = DONE;
            ViewBag.orderList = orderList;
            if (id == 1)
            {
                ViewBag.pagina = "processing";
            }
            if (id == 2)
            {
                ViewBag.pagina = "finished";
            }

            return View();
        }

        [NonAction]
        private void UpdateCartValue()
        {
            Order cart = GetCart();
            cart.TotalPrice = cart.OrderContents.Select(oc => oc.Total).Sum();
            db.SaveChanges();
        }

        [NonAction]
        private Order GetCart()
        {
            string uid = User.Identity.GetUserId();
            IEnumerable<Order> cart = (from order in db.Orders
                          where order.UserId == uid && order.Status == CART
                          select order).ToList(); // tolist because we dont want to work with raw linq
            if (cart.Count() == 1)
                return cart.First();
            else
            {   
                // if there's no existing cart, make one
                Order newCart = new Order();
                db.Orders.Add(newCart);
                newCart.UserId = uid;
                newCart.Status = CART;
                newCart.Date = DateTime.Now;
                newCart.OrderContents = new List<OrderContent>();
                db.SaveChanges();
                return newCart;
            }
        }

    }
}