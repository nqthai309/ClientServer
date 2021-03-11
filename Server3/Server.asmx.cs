using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Services;
using Server3.Entities;
using Newtonsoft.Json;
using Server3.Class;
using Microsoft.EntityFrameworkCore;

namespace Server3
{
    /// <summary>
    /// Summary description for Server
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Server : System.Web.Services.WebService
    {
        static Model1 context = new Model1();
        public string EncodingPassword(string password)
        {
            MD5 mh = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(password.Trim());
            byte[] hash = mh.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        [WebMethod]
        public string FE_FindUserByUser_id(string user_id)
        {
            var user = from u in context.users
                       where u.user_id.ToString() == user_id
                       select
                       new { u.user_id, u.username, u.password, u.roles.role_name, u.full_name, u.phone, u.email, u.address, u.point, u.discount.discount_value };
            return JsonConvert.SerializeObject(user);
        }

        [WebMethod]
        public string FE_FindUserByUsername_Password(string userName, string passWord)
        {
            passWord = EncodingPassword(passWord);
            var user = from u in context.users where u.username == userName && u.password == passWord
                       select
                       new { u.user_id, u.username, u.password, u.roles.role_name, u.full_name, u.phone, u.email, u.address, u.point, u.discount.discount_value };
            return JsonConvert.SerializeObject(user);
        }

        

        [WebMethod]
        public string FE_LoginCheck(string userName, string passWord)
        {
            string passWord2 = EncodingPassword(passWord);
            var user = context.users.Where(u => u.username == userName && u.password == passWord2).SingleOrDefault();
            if (user != null)
            {
                return "1";
            }
            else
            {
                return "-1";
            }
        }


        [WebMethod]
        public void FE_Register(string json)
        {
            var user = JsonConvert.DeserializeObject<users>(json);
            var newUser = new users();
            newUser.username = user.username;
            newUser.password = EncodingPassword(user.password);
            newUser.role_id = user.role_id;
            newUser.full_name = user.full_name;
            newUser.phone = user.phone;
            newUser.email = user.email;
            newUser.address = user.address;
            newUser.point = user.point;
            newUser.discount_id = user.discount_id;
            context.users.Add(newUser);
            context.SaveChanges();
        }

        [WebMethod]
        public void FE_UpdateUserInfo(string user_id, string json)
        {
            var user = context.users.SingleOrDefault(u => u.user_id.ToString() == user_id);
            var newUser = JsonConvert.DeserializeObject<users>(json);
            user.phone = newUser.phone;
            user.address = newUser.address;
            user.email = newUser.email;
            user.full_name = newUser.full_name;
            context.SaveChanges();
        }
        //string json2 = JsonConvert.SerializeObject(user, Formatting.Indented, new JsonSerializerSettings
        //{
        //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        //});

        [WebMethod]
        public string FE_isUsernameCheck(string userName)
        {
            var user = context.users.Where(u => u.username == userName).SingleOrDefault();
            if(user != null)
            {
                return "true";
            }
            else
            {
                return "false";
            }
        }

        [WebMethod]
        public void FE_UpdateUserPassword(string user_id, string newPassword)
        {
            var user = context.users.SingleOrDefault(u => u.user_id.ToString() == user_id);
            user.password = EncodingPassword(newPassword);
            context.SaveChanges();
        }

        [WebMethod]
        public string FE_GetHotel_OrderByComment()
        {
            List<String> maxPrice = new List<string>();
            List<String> minPrice = new List<string>();
            List<String> totalComment = new List<string>();
            List<ItemHotel> itemHotels = new List<ItemHotel>();

            var hotelList = from ht in context.hotels
                            select 
                            new { ht.hotel_id, ht.image_url, ht.detail_header_image_url, ht.hotel_name, ht.star, ht.description, ht.locations.location_name, ht.location_details };
            
            foreach (var it in hotelList.ToList())
            {
                maxPrice.Add(context.hotel_rooms.Where(hr => hr.hotel_id == it.hotel_id).Max(hr => hr.sell_price).ToString());
                minPrice.Add(context.hotel_rooms.Where(hr => hr.hotel_id == it.hotel_id).Min(hr => hr.sell_price).ToString());
                totalComment.Add(context.comments.Count(c => c.hotel_id.ToString() == it.hotel_id.ToString()).ToString());

            }

            for(int i=0; i < hotelList.Count(); i++)
            {
                var it = new ItemHotel();
                it.HotelID = hotelList.ToList()[i].hotel_id;
                it.Image_url = hotelList.ToList()[i].image_url;
                it.Detail_header_image_url = hotelList.ToList()[i].detail_header_image_url;
                it.HotelName = hotelList.ToList()[i].hotel_name;
                it.MaxPrice = double.Parse(maxPrice[i]);
                it.MinPrice = double.Parse(minPrice[i]);
                it.Star = (int)hotelList.ToList()[i].star;
                it.TotalComment = int.Parse(totalComment[i]);
                it.Description = hotelList.ToList()[i].description;
                it.AddressDetail = hotelList.ToList()[i].location_details;
                it.Address = hotelList.ToList()[i].location_name;
                itemHotels.Add(it);

            }
            return JsonConvert.SerializeObject(itemHotels);
        }

        [WebMethod]
        public string FE_GetListLocation()
        {
            var locationList = from loc in context.locations
                               select new { loc.location_id, loc.image_url, loc.location_name };
            return JsonConvert.SerializeObject(locationList);
        }

        [WebMethod]
        public string FE_GetBanner()
        {
            var banner = context.banner.ToList();
            return JsonConvert.SerializeObject(banner);
        }
        
        [WebMethod]
        public string FE_GetService()
        {
            var services = from sv in context.services
                           select new {sv.service_id, sv.service_name, sv.service_image };
            return JsonConvert.SerializeObject(services);
        }

        [WebMethod]
        public string FE_GetConvenient()
        {
            var convenient = from cv in context.convenient
                             select new {cv.convenient_id, cv.convenient_name, cv.convenient_image };
            return JsonConvert.SerializeObject(convenient);
        }

        [WebMethod]
        public string FE_Search_HotelList(string location_name, string hotel_name, string star)
        {
            List<String> maxPrice = new List<string>();
            List<String> minPrice = new List<string>();
            List<String> totalComment = new List<string>();
            List<ItemHotel> itemHotels = new List<ItemHotel>();
            string star2 = "";
            if(star == null)
            {
                star2 = "";
            }
            else
            {
                string[] data = star.Split(' ');
                star2 = data[0];
            }
            
            var hotelList = from ht in context.hotels
                         where ht.star.ToString().Contains(star2) && (ht.locations.location_name.Contains(location_name)) && ht.hotel_name.Contains(hotel_name)
                         select 
                         new { ht.hotel_id, ht.image_url, ht.detail_header_image_url, ht.hotel_name, ht.star, ht.description, ht.locations.location_name, ht.location_details };
            


            foreach (var it in hotelList.ToList())
            {
                maxPrice.Add(context.hotel_rooms.Where(hr => hr.hotel_id == it.hotel_id).Max(hr => hr.sell_price).ToString());
                minPrice.Add(context.hotel_rooms.Where(hr => hr.hotel_id == it.hotel_id).Min(hr => hr.sell_price).ToString());
                totalComment.Add(context.comments.Count(c => c.hotel_id.ToString() == it.hotel_id.ToString()).ToString());

            }

            for (int i = 0; i < hotelList.Count(); i++)
            {
                var it = new ItemHotel();
                it.HotelID = hotelList.ToList()[i].hotel_id;
                it.Image_url = hotelList.ToList()[i].image_url;
                it.Detail_header_image_url = hotelList.ToList()[i].detail_header_image_url;
                it.HotelName = hotelList.ToList()[i].hotel_name;
                it.MaxPrice = double.Parse(maxPrice[i]);
                it.MinPrice = double.Parse(minPrice[i]);
                it.Star = (int)hotelList.ToList()[i].star;
                it.TotalComment = int.Parse(totalComment[i]);
                it.Description = hotelList.ToList()[i].description;
                it.AddressDetail = hotelList.ToList()[i].location_details;
                
                it.Address = hotelList.ToList()[i].location_name;
                itemHotels.Add(it);

            }
            return JsonConvert.SerializeObject(itemHotels);
        }

        [WebMethod]
        public string FE_Search_Hotel_Service_Convenient(string sv, string conv)
        {
            List<String> hotelIDs_sv = new List<string>();
            List<String> hotelIDs_conv = new List<string>();
            List<String> result = new List<string>();
            List<String> duplicates = new List<string>();

            string[] sv_split = sv.Split(',');
            string[] conv_split = conv.Split(',');
            foreach (var item in sv_split)
            {
                var hotel_id = from htsv in context.hotel_service
                               where htsv.service_id.ToString() == item
                               select new { htsv.hotel_id };
                foreach(var it in hotel_id)
                {
                    hotelIDs_sv.Add(it.ToString());
                }
            }

            foreach(var item in conv_split)
            {
                var hotel_id = from htcv in context.hotel_convenient
                               where htcv.convenient_id.ToString() == item
                               select new { htcv.hotel_id };
                foreach(var it in hotel_id)
                {
                    hotelIDs_conv.Add(it.ToString());
                }
            }
            hotelIDs_sv = hotelIDs_sv.Distinct().ToList();
            hotelIDs_conv = hotelIDs_conv.Distinct().ToList();
            
            if(hotelIDs_sv.Count() == 0)
            {
                duplicates = hotelIDs_conv.ToList();
            }
            else if(hotelIDs_conv.Count() == 0)
            {
                duplicates = hotelIDs_sv.ToList();
            }
            else
            {
                duplicates = hotelIDs_sv.Intersect(hotelIDs_conv).ToList();
            }    
            
            foreach(var it in duplicates)
            {
                char[] data = it.ToCharArray();
                result.Add(data[data.Length - 3].ToString());
            }

            return JsonConvert.SerializeObject(result);
        }

        [WebMethod]
        public string FE_GetListRoom(int hotel_id)
        {
            var rooms = from htr in context.hotel_rooms
                        where htr.hotel_id == hotel_id
                        select new { htr.hotel_rooms_id, htr.hotel_id, htr.detail_header_image_url_1, htr.detail_header_image_url_2, htr.detail_header_image_url_3,
                        htr.detail_header_image_url_4, htr.acreage, htr.floors, htr.limit_people, htr.bed_count, htr.price, htr.sell_price};
            return JsonConvert.SerializeObject(rooms);
        }

        [WebMethod]
        public string FE_GetComment(int hotel_id)
        {
            var comList = from com in context.comments
                          where com.hotel_id == hotel_id
                          select new { com.comment_id, com.hotel_id, com.users.username, com.content, com.time_comment };

            return JsonConvert.SerializeObject(comList);
        }
        [WebMethod]
        public string FE_Search_Room(int hotel_id, int limitPeople, int selectPrice)
        {
            var rooms = from htr in context.hotel_rooms
                        where htr.limit_people == limitPeople && htr.hotel_id == hotel_id
                        select new {
                            htr.hotel_rooms_id,
                            htr.hotel_id,
                            htr.detail_header_image_url_1,
                            htr.detail_header_image_url_2,
                            htr.detail_header_image_url_3,
                            htr.detail_header_image_url_4,
                            htr.acreage,
                            htr.floors,
                            htr.limit_people,
                            htr.bed_count,
                            htr.price,
                            htr.sell_price
                        };
            if(selectPrice == 1)
            {
                var result = rooms.OrderBy(r => r.sell_price).ToList();
                return JsonConvert.SerializeObject(result);
            }
            else
            {
                var result = rooms.OrderByDescending(r => r.sell_price).ToList();
                return JsonConvert.SerializeObject(result);
            }
        }

        [WebMethod]
        public string FE_Search_Room_Conv(string conv, int hotel_id)
        {
            List<String> room_IDs = new List<string>();
            List<String> result = new List<string>();
            string[] data = conv.Split(',');
            foreach(var it in data)
            {
                var rooms = from htc in context.hotel_convenient
                            where htc.convenient_id.ToString() == it && htc.hotel_id == hotel_id
                            select new { htc.hotel_rooms_id };
                foreach(var it2 in rooms)
                {
                    room_IDs.Add(it2.ToString());
                }
            }
            room_IDs = room_IDs.Distinct().ToList();
            foreach(var it in room_IDs)
            {
                char[] data2 = it.ToCharArray();
                result.Add(data2[data2.Length - 3].ToString());
            }
            return JsonConvert.SerializeObject(result);

        }

        [WebMethod]
        public string FE_Get_Conv_By_hotelID_roomID(int hotel_id, int hotel_rooms_id)
        {
            var listConv = from cv in context.hotel_convenient
                           where cv.hotel_id == hotel_id && cv.hotel_rooms_id == hotel_rooms_id
                           select new { cv.hotel_id, cv.hotel_rooms_id, cv.convenient_id, cv.convenient.convenient_name, cv.convenient.convenient_image };

            return JsonConvert.SerializeObject(listConv);

        }

        [WebMethod]
        public string FE_Get_Service_by_HotelID(int hotel_id)
        {
            var listService = from sv in context.hotel_service
                              where sv.hotel_id == hotel_id
                              select new { sv.hotel_id, sv.service_id, sv.unit_price, sv.services.service_image, sv.services.service_name };
            return JsonConvert.SerializeObject(listService);
        }

        [WebMethod]
        public string FE_GetHistoryOrderOfRoom(int hotel_id, int hotel_rooms_id)
        {
            var list = from history in context.booking_hotel_detail
                       where history.hotel_id == hotel_id && history.hotel_rooms_id == hotel_rooms_id
                       select new { history.from_date, history.to_date };
            return JsonConvert.SerializeObject(list);
        }

        [WebMethod]
        public string FE_Get_hotline(int hotel_id)
        {
            var hotline = context.hotels.Where(ht => ht.hotel_id == hotel_id).Select(ht => ht.hotel_hotline).SingleOrDefault();
            return hotline;
        }
        [WebMethod]
        public string FE_Payment(string list, int user_id, float total_booking_price, string payment_status)
        {
            List<CartItem> cartItems = JsonConvert.DeserializeObject<List<CartItem>>(list);
            var newItemBooking = new bookings();
            newItemBooking.user_id = user_id;
            newItemBooking.time_booking = DateTime.Now;
            newItemBooking.total_booking_price = total_booking_price;
            newItemBooking.payment_status = payment_status;
            context.bookings.Add(newItemBooking);
            context.SaveChanges();

            foreach(var it in cartItems)
            {
                var newBookingHotelDetail = new booking_hotel_detail();
                newBookingHotelDetail.booking_id = newItemBooking.booking_id;
                newBookingHotelDetail.hotel_id = it.Product.Hotel_id;
                newBookingHotelDetail.hotel_rooms_id = it.Product.Hotel_rooms_id;
                newBookingHotelDetail.from_date = Convert.ToDateTime(it.Product.From_date);
                newBookingHotelDetail.to_date = Convert.ToDateTime(it.Product.To_date);
                newBookingHotelDetail.total_price = it.Product.TotalPrice;
                newBookingHotelDetail.service_list = it.Product.Service_list;
                newBookingHotelDetail.total_services_price = it.Product.TotalPriceService;
                context.booking_hotel_detail.Add(newBookingHotelDetail);
                context.SaveChanges();

            }
            return "1";
        }

        [WebMethod]
        public string FE_SendComment(string idAccount, int hotel_id, string content)
        {
            var user_id = int.Parse(idAccount);
            var cmt_element = context.comments.Where(cmt => cmt.hotel_id == hotel_id && cmt.user_id == user_id).ToList();
            if (cmt_element == null)
            {
                return "-1";
            }
            else
            {
                var newCmt = new comments();
                newCmt.content = content;
                newCmt.hotel_id = hotel_id;
                newCmt.user_id = user_id;
                newCmt.time_comment = DateTime.Now;
                context.comments.Add(newCmt);
                context.SaveChanges();
                return "1";
            }
        }

        [WebMethod]
        public string BE_FindUserByUsername_Password(string userName, string passWord)
        {
            passWord = EncodingPassword(passWord);
            var user = from u in context.users
                       where u.username == userName && u.password == passWord && u.role_id == 2
                       select
                       new { u.user_id, u.username, u.password, u.roles.role_name, u.full_name, u.phone, u.email, u.address, u.point, u.discount.discount_value };
            return JsonConvert.SerializeObject(user);
        }


        //--------------------THAI----------------------------//




        //--------------------THANG--------------------------//
        //code tai day//
        //--------------------THANG-------------------------//




        //-------------------TUNG-------------------------//
        //code tai day//
        //------------------TUNG-------------------------//
    }
}
