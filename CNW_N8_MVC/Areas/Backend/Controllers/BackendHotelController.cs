using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Dynamic;

using CNW_N8_MVC.Models;
using CNW_N8_MVC.Class;
using Newtonsoft.Json;

namespace CNW_N8_MVC.Areas.Backend.Controllers
{
    public class BackendHotelController : BaseController
    {
        static ServiceReference1.WebService1SoapClient client = new ServiceReference1.WebService1SoapClient();
        private Model1 context = new Model1();
        static int id_Old;
        static Server.ServerSoapClient server = new Server.ServerSoapClient();
        public static List<ItemLocation> locList;
        // GET: BackendHotel
        public ActionResult AddHotelRoom(int? id)
        {
            return View();
        }
        public ActionResult List()
        {
            var hotels = JsonConvert.DeserializeObject<List<ItemHotel>>(server.GetListHotel_BE());
            ViewData["hotels"] = hotels;
            return View();
        }

        public ActionResult Add()
        {
            dynamic model = new ExpandoObject();
            locList = JsonConvert.DeserializeObject<List<ItemLocation>>(server.FE_GetListLocation());
            model.itemLocations = locList;
            return View(model);
        }

        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return RedirectToAction("List", "BackendHotel", new { area = "Backend" });
            }
            else
            {
                int a;
                bool check = int.TryParse(id.ToString(), out a);
                if (check == true)
                {
                    var model = context.hotels.Find(a);
                    if (model == null)
                    {
                        return RedirectToAction("List", "BackendHotel", new { area = "Backend" });
                    }
                    else
                    {
                        id_Old = a;
                        var listLocations = context.locations.Where(l => l.location_name != null).ToList();
                        ViewData["listLocations"] = listLocations;
                        return View(model);
                    }
                }
                else
                {
                    return RedirectToAction("List", "BackendHotel", new { area = "Backend" });
                }
            }

        }
        public int checkAddHotel(string location_id, string hotel_name, string description, string more_imformation, string price, string sell_price)
        {
            if (location_id == "" || hotel_name == "")
            {
                return -1;
            }
            return 1;
        }
        [HttpPost]
        public ActionResult AddHotel(hotels hotel)
        {
            hotel.image_url = "/Content/img/Group 68.png";
            hotel.detail_header_image_url = "/Content/img/hotel-detail.jpg";
            hotel.star = 0;
            hotel.booking_count = 0;
            server.AddHotel_BE(JsonConvert.SerializeObject(hotel));
            context.hotels.Add(hotel);
            context.SaveChanges();
            return RedirectToAction("List", "BackendHotel", new { area = "Backend" });
        }

        [HttpPost]
        public ActionResult EditHotel(hotels hotel)
        {
            hotel.image_url = "/Content/img/Group 68.png";
            hotel.detail_header_image_url = "/Content/img/hotel-detail.jpg";
            client.EditHotel_BE(id_Old, JsonConvert.SerializeObject(hotel));
            context.hotels.Remove(context.hotels.Find(id_Old));
            context.hotels.Add(hotel);
            context.SaveChanges();
            return RedirectToAction("List", "BackendHotel", new { area = "Backend" });
        }


        public ActionResult DeleteHotel(string id)
        {
            if (id == null)
            {
                return RedirectToAction("List", "BackendHotel", new { area = "Backend" });
            }
            else
            {
                int a;
                bool check = int.TryParse(id.ToString(), out a);
                if (check == true)
                {
                    var result = context.hotels.Find(a);
                    if (result == null)
                    {
                        return RedirectToAction("List", "BackendHotel", new { area = "Backend" });
                    }
                    else
                    {
                        client.DeleteHotel_BE(int.Parse(id));
                        //context.hotels.Remove(result);
                        //context.SaveChanges();
                        return RedirectToAction("List", "BackendHotel", new { area = "Backend" });

                    }
                }
                else
                {
                    return RedirectToAction("List", "BackendHotel", new { area = "Backend" });
                }
            }

        }

        public int checkEditHotel(string location_id, string hotel_name, string description, string more_imformation, string price, string sell_price)
        {
            if (location_id == "" || hotel_name == "" || description == "" || more_imformation == "" || price == "" || sell_price == "")
            {
                return -1;
            }
            else
            {
                var result = context.hotels.Where(h => h.hotel_name == hotel_name).FirstOrDefault();
                var hotel_old = context.hotels.Find(id_Old);

                if (result == null || hotel_old.hotel_name == hotel_name)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}