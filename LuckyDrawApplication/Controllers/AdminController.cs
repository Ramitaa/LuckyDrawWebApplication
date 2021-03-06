﻿using LuckyDrawApplication.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LuckyDrawApplication.Controllers
{
    public class AdminController : Controller
    {
        private string response_message = "";
        private List<int> staff_prizes = new List<int>() { 4888, 3888, 5888 };
        private int orderNo = 0;

        public ActionResult Index()
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            ViewBag.Name = a_user.Name;
            ViewBag.PurchasersList = GetPurchasersCount(luckydrawevent.EventID);
            ViewBag.WinnersList = GetWinnersCount(luckydrawevent.EventID);

            return View();
        }

        [HttpGet]
        public ActionResult CreateUserAndDraw()
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            var list = new List<SelectListItem>();
            for (var i = 1; i < 41; i++)
                list.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });

            ViewBag.FloorUnitList = list;
            ViewBag.ProjectList = GetProjectList(luckydrawevent.EventID);
            ViewBag.SalesLocation = luckydrawevent.EventLocation;
            ViewBag.Name = a_user.Name;

            DateTime dateTime = DateTime.UtcNow.Date;

            ViewBag.Date = dateTime.ToString("dd | MM | yyyy").ToString();
            ViewBag.Time = DateTime.Now.ToShortTimeString().ToString();

            return View();
        }

        [HttpPost]
        public ActionResult CreateUserAndDraw(Models.User user)
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            if (user != null)
            {
                if (DuplicateUserExists(user))
                {
                    return Json(new
                    {
                        success = false,
                        draw = -1,
                        message = "This unit has already been purchased by another buyer!"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    Tuple<int, int> results = CreateNewUser(user, luckydrawevent.EventID);

                    return Json(new
                    {
                        success = true,
                        draw = results,
                        urllink = Url.Action("LuckyDrawAnimation", "Home", new { id = results.Item1 }, "https"),
                        message = user.Name.ToUpper() + " has been registered successfully!"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                response_message = "User is null!";

                return Json(new
                {
                    success = false,
                    draw = -1,
                    message = user.Name.ToUpper() + " cannot be registered! Error: " + response_message
                }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public ActionResult StaffLuckyDraw()
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            if (luckydrawevent == null)
                return RedirectToAction("Index", "Login");

            ViewBag.Name = a_user.Name;

            return View();
        }

        [HttpPost]
        public ActionResult PostStaffLuckyDraw()
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            if (luckydrawevent == null)
                return RedirectToAction("Index", "Login");

            ViewBag.Name = a_user.Name;

            string staff_name = StaffLuckyDraw(luckydrawevent.EventID);

            if (staff_name == null || staff_name == "")
            {
                return Json(new
                {
                    success = false,
                    message = "No sales agent to be picked as winner!"
                }, JsonRequestBehavior.AllowGet); ;
            }
            else
            {
                int prize_a = 0;

                if (orderNo < staff_prizes.Count)
                {
                    prize_a = staff_prizes[orderNo];
                }

                Debug.WriteLine("Order No: " + orderNo);

                orderNo = orderNo +  1;

                return Json(new
                {
                    success = true,
                    message = staff_name,
                    urllink = Url.Action("StaffLuckyDrawAnimation", "Admin", new { name = staff_name.ToUpper(), prize = prize_a }, "https"),
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult StaffLuckyDrawAnimation(string name, int prize)
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            ViewBag.Name = a_user.Name;
            ViewBag.WinnerName = name;
            ViewBag.WinnerPrize = prize;

            return View();
        }

        [HttpGet]
        public ActionResult LuckyDrawAnimation(int id)
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");


            Models.User user = GetUser(id);

            ViewBag.Name = a_user.Name;
            ViewBag.WinnerName = user.Name;
            ViewBag.WinnerPrize = user.PrizeWon;

            return View();
        }


        public ActionResult Users()
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            List<User> userList = GetUserList(luckydrawevent.EventID);

            ViewBag.Name = a_user.Name;

            return View(userList);
        }

        public ActionResult Winners()
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            List<User> winnerList = GetWinnerList(luckydrawevent.EventID);

            ViewBag.Name = a_user.Name;

            return View(winnerList);
        }

        public ActionResult ViewUser(int id)
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            Models.User user = GetUser(id);

            ViewBag.Name = a_user.Name;

            return View(user);
        }

        [HttpGet]
        public ActionResult ModifyUser(int id)
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            Models.User user = GetUser(id);
            string[] tokens = user.Unit.Split('-');
            ViewBag.Block = tokens[0];
            ViewBag.Level = tokens[1];
            ViewBag.Unit = tokens[2];

            var list = new List<SelectListItem>();
            for (var i = 1; i < 41; i++)
                list.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });

            ViewBag.FloorUnitList = list;
            ViewBag.ProjectList = GetProjectList(luckydrawevent.EventID);
            ViewBag.SalesLocation = luckydrawevent.EventLocation;
            ViewBag.Name = a_user.Name;

            return View(user);
        }

        [HttpPost]
        public ActionResult ModifyUser(Models.User user)
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            if (user != null)
            {
                if (DuplicateUserExistsForModification(user))
                {
                    return Json(new
                    {
                        success = false,
                        message = "This unit has already been purchased by another buyer!"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ModifyExistingUser(user, luckydrawevent.EventID);
                    return Json(new
                    {
                        success = true,
                        url = Url.Action("Users", "Admin"),
                        message = user.Name.ToUpper() + " has been successfully modified!"
                    }, JsonRequestBehavior.AllowGet);
                }   
            }
            else
            {
                response_message = "User is null!";

                return Json(new
                {
                    success = false,
                    draw = -1,
                    message = user.Name.ToUpper() + " cannot be modified! Error: " + response_message
                }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult ExportToExcelPurchasers()
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            var gv = new GridView();
            gv.DataSource = ToDataTable<User>(GetUserList(luckydrawevent.EventID));
            gv.DataBind();

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=DemoExcel.xls");
            Response.ContentType = "application/ms-excel";

            Response.Charset = "";
            StringWriter objStringWriter = new StringWriter();
            HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);

            gv.RenderControl(objHtmlTextWriter);

            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();

            return View("Index", "Admin");
        }

        public ActionResult ExportToExcelWinners()
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            var gv = new GridView();
            gv.DataSource = ToDataTable<User>(GetWinnerList(luckydrawevent.EventID));
            gv.DataBind();

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=DemoExcel.xls");
            Response.ContentType = "application/ms-excel";

            Response.Charset = "";
            StringWriter objStringWriter = new StringWriter();
            HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);

            gv.RenderControl(objHtmlTextWriter);

            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();

            return View("Index", "Admin");
        }


        //Generic method to convert List to DataTable
        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        // Register new user;
        [NonAction]
        public Tuple<int, int> CreateNewUser(Models.User user, int eventCode)
        {
            int last_inserted_id = 0;
            response_message = "";

            try
            {
                MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
                cn.Open();

                MySqlCommand cmd = cn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = String.Format("INSERT INTO user(Name, ICNumber, EmailAddress, ContactNumber, EventID, ProjectID, Unit, SalesConsultant, PrizeWon) VALUES (@name, @ic, @email, @contact, @eventID, @projectID, @unit, @salesconsultant, @prizewon); SELECT LAST_INSERT_ID() AS id;");
                cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = user.Name.ToUpper();
                cmd.Parameters.Add("@ic", MySqlDbType.VarChar).Value = user.ICNumber;
                cmd.Parameters.Add("@email", MySqlDbType.VarChar).Value = user.EmailAddress.ToLower();
                cmd.Parameters.Add("@contact", MySqlDbType.VarChar).Value = user.ContactNumber;
                cmd.Parameters.Add("@eventID", MySqlDbType.Int32).Value = eventCode;
                cmd.Parameters.Add("@projectID", MySqlDbType.Int32).Value = user.ProjectID;
                cmd.Parameters.Add("@unit", MySqlDbType.VarChar).Value = user.Unit.ToUpper();
                cmd.Parameters.Add("@salesconsultant", MySqlDbType.VarChar).Value = user.SalesConsultant.ToUpper();
                cmd.Parameters.Add("@prizewon", MySqlDbType.Int16).Value = 0;

                MySqlDataReader rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    last_inserted_id = Convert.ToInt32(rd["id"]);
                }
                rd.Close();
                cmd.Dispose();
                cn.Close();
            }

            catch (Exception e)
            {
                response_message = e.Message;
            }

            try
            {
                MySqlConnection cn1 = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
                cn1.Open();

                MySqlCommand cmd1 = cn1.CreateCommand();
                cmd1.CommandType = CommandType.Text;
                cmd1.CommandText = String.Format("UPDATE project SET NoOfProject= NoOfProject + 1 WHERE ProjectID = @projectID");
                cmd1.Parameters.Add("@projectID", MySqlDbType.Int32).Value = user.ProjectID;
              
                MySqlDataReader rd1 = cmd1.ExecuteReader();
                rd1.Close();
                cmd1.Dispose();
                cn1.Close();
            }

            catch (Exception e)
            {
                response_message = e.Message;
            }

            return CheckForLuckyDraw(user, last_inserted_id);
        }

        [NonAction]
        public static Tuple<int, int> CheckForLuckyDraw(Models.User user, int last_inserted_id)
        {
            string prizeCategory = "";
            int prizeCode = 0;
            int prizeAmount = 0;

            MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT project.PrizeCategory AS prizeCategory, luckydraw.Prize AS prize FROM project INNER JOIN luckydraw ON project.ProjectID = luckydraw.ProjectID WHERE luckydraw.ProjectID = @projectID AND luckydraw.OrderNo = project.NoOfProject");
            cmd.Parameters.Add("@projectID", MySqlDbType.Int16).Value = user.ProjectID;
            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                prizeCategory = rd["prizeCategory"].ToString();
                prizeCode = Convert.ToInt32(rd["prize"]);
            }

            rd.Close();
            cmd.Dispose();
            cn.Close();

            Debug.WriteLine("Prize Category: " + prizeCategory + ", PrizeCode: " + prizeCode);

            if (prizeCode != 0)
            {
                string[] prizes = prizeCategory.Split(',');
                prizeAmount = Convert.ToInt32(prizes[prizeCode - 1]);
                UpdateDatabaseWhenLuckyDrawIsWon(last_inserted_id, prizeCode);
                return new Tuple<int, int>(last_inserted_id, prizeAmount);
            }

            else
            {
                return new Tuple<int, int>(last_inserted_id, 0);
            }
        }

        [NonAction]
        public static void UpdateDatabaseWhenLuckyDrawIsWon(int userID, int prizeCode)
        {
            Debug.WriteLine("Updating Database with PrizeCode: " + prizeCode);

            MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("UPDATE user SET PrizeWon= @PrizeWon WHERE PurchaserID = @PurchaserID");
            cmd.Parameters.Add("@PurchaserID", MySqlDbType.Int16).Value = userID;
            cmd.Parameters.Add("@PrizeWon", MySqlDbType.VarChar).Value = prizeCode;
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            cn.Close();

        }

        // Check if ic, project and unit clashes;
        [NonAction]
        public static Boolean DuplicateUserExists(Models.User user)
        {
            int count = 0;

            MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
            cn.Open();
            
            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT COUNT(PurchaserID) AS userExists FROM user WHERE ProjectID = @project AND Unit = @unit");
            cmd.Parameters.Add("@project", MySqlDbType.Int32).Value = user.ProjectID;
            cmd.Parameters.Add("@unit", MySqlDbType.VarChar).Value = user.Unit.ToUpper();

            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                count = Convert.ToInt32(rd["userExists"].ToString());
            }

            rd.Close();
            cmd.Dispose();
            cn.Close();

            return count > 0;

        }

        // Get users list
        [NonAction]
        public static List<Models.User> GetUserList(int eventID)
        {
            List<Models.User> UserList = new List<Models.User>();
            MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT user.*, project.ProjectName, project.PrizeCategory, event.EventLocation FROM user INNER JOIN project on project.ProjectID = user.ProjectID INNER JOIN event ON event.EventID = user.EventID WHERE user.EventID = @eventID");
            cmd.Parameters.Add("@eventID", MySqlDbType.Int32).Value = eventID;
            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                Models.User user = new Models.User();
                user.PurchaserID = Convert.ToInt32(rd["PurchaserID"].ToString());
                user.Name = rd["Name"].ToString();
                user.ICNumber = rd["ICNumber"].ToString();
                user.EmailAddress = rd["EmailAddress"].ToString();
                user.ContactNumber = rd["ContactNumber"].ToString();
                user.EventID = Convert.ToInt32(rd["EventID"].ToString());
                user.ProjectID = Convert.ToInt32(rd["ProjectID"].ToString());
                user.ProjectName = rd["ProjectName"].ToString();
                user.SalesLocation = rd["EventLocation"].ToString();
                user.Unit = rd["Unit"].ToString();

                if (Convert.ToInt32(rd["PrizeWon"]) > 0)
                {
                    string[] prizes = rd["PrizeCategory"].ToString().Split(',');
                    user.PrizeWon = Convert.ToInt32(prizes[Convert.ToInt32(rd["PrizeWon"]) - 1]);
                }
                else
                {
                    user.PrizeWon = 0;
                }

                user.SalesConsultant = rd["SalesConsultant"].ToString();
                user.DateTime = rd["DateTime"].ToString();
                UserList.Add(user);
            }

            rd.Close();
            cmd.Dispose();
            cn.Close();

            return UserList;
        }

        // Get users list
        [NonAction]
        public static List<Models.User> GetWinnerList(int eventID)
        {
            List<Models.User> UserList = new List<Models.User>();
            MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT user.*, project.ProjectName, project.PrizeCategory, event.EventLocation FROM user INNER JOIN project on project.ProjectID = user.ProjectID INNER JOIN event ON event.EventID = user.EventID WHERE PrizeWon > 0 AND user.EventID = @eventID");
            cmd.Parameters.Add("@eventID", MySqlDbType.Int32).Value = eventID;
            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                Models.User user = new Models.User();
                user.PurchaserID = Convert.ToInt32(rd["PurchaserID"].ToString());
                user.Name = rd["Name"].ToString();
                user.ICNumber = rd["ICNumber"].ToString();
                user.EmailAddress = rd["EmailAddress"].ToString();
                user.ContactNumber = rd["ContactNumber"].ToString();
                user.EventID = Convert.ToInt32(rd["EventID"].ToString());
                user.ProjectID = Convert.ToInt32(rd["ProjectID"].ToString());
                user.ProjectName = rd["ProjectName"].ToString();
                user.SalesLocation = rd["EventLocation"].ToString();
                user.Unit = rd["Unit"].ToString();
                user.SalesConsultant = rd["SalesConsultant"].ToString();

                if (Convert.ToInt32(rd["PrizeWon"]) > 0)
                {
                    string[] prizes = rd["PrizeCategory"].ToString().Split(',');
                    user.PrizeWon = Convert.ToInt32(prizes[Convert.ToInt32(rd["PrizeWon"]) - 1]);
                }
                else
                {
                    user.PrizeWon = 0;
                }

                user.DateTime = rd["DateTime"].ToString();
                UserList.Add(user);
            }

            rd.Close();
            cmd.Dispose();
            cn.Close();

            return UserList;
        }

        // Get users list
        [NonAction]
        public static Models.User GetUser(int userID)
        {
            Models.User user = new Models.User();
            MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT project.ProjectName, project.PrizeCategory, event.EventLocation, user.* FROM user INNER JOIN project on project.ProjectID = user.ProjectID INNER JOIN event ON event.EventID = user.EventID WHERE PurchaserID = @id");
            cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = userID;
            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                user.PurchaserID = Convert.ToInt32(rd["PurchaserID"].ToString());
                user.Name = rd["Name"].ToString();
                user.ICNumber = rd["ICNumber"].ToString();
                user.EmailAddress = rd["EmailAddress"].ToString();
                user.ContactNumber = rd["ContactNumber"].ToString();
                user.EventID = Convert.ToInt32(rd["EventID"].ToString());
                user.ProjectID = Convert.ToInt32(rd["ProjectID"].ToString());
                user.ProjectName = rd["ProjectName"].ToString();
                user.SalesLocation = rd["EventLocation"].ToString();
                user.Unit = rd["Unit"].ToString();
                user.SalesConsultant = rd["SalesConsultant"].ToString();

                if (Convert.ToInt32(rd["PrizeWon"]) > 0)
                {
                    string[] prizes = rd["PrizeCategory"].ToString().Split(',');
                    user.PrizeWon = Convert.ToInt32(prizes[Convert.ToInt32(rd["PrizeWon"]) - 1]);
                }
                else
                {
                    user.PrizeWon = 0;
                }

                user.DateTime = rd["DateTime"].ToString();
            }

            rd.Close();
            cmd.Dispose();
            cn.Close();

            return user;
        }

        // Modify existing user;
        [NonAction]
        public void ModifyExistingUser(Models.User user, int eventCode)
        {
            try
            {
                MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
                cn.Open();

                MySqlCommand cmd = cn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = String.Format("UPDATE user SET Name = @name, ICNumber = @ic, EmailAddress= @email, ContactNumber = @contact, EventID = @eventID, ProjectID = @projectID, Unit = @unit, SalesConsultant = @salesconsultant WHERE PurchaserID = @id");
                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = user.PurchaserID;
                cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = user.Name.ToUpper();
                cmd.Parameters.Add("@ic", MySqlDbType.VarChar).Value = user.ICNumber;
                cmd.Parameters.Add("@email", MySqlDbType.VarChar).Value = user.EmailAddress.ToLower();
                cmd.Parameters.Add("@contact", MySqlDbType.VarChar).Value = user.ContactNumber;
                cmd.Parameters.Add("@eventID", MySqlDbType.Int32).Value = eventCode;
                cmd.Parameters.Add("@projectID", MySqlDbType.Int32).Value = user.ProjectID;
                cmd.Parameters.Add("@unit", MySqlDbType.VarChar).Value = user.Unit.ToUpper();
                cmd.Parameters.Add("@salesconsultant", MySqlDbType.VarChar).Value = user.SalesConsultant.ToUpper();

                MySqlDataReader rd = cmd.ExecuteReader();
                rd.Close();
                cmd.Dispose();
                cn.Close();
            }

            catch (Exception e)
            {
                response_message = e.Message;
            }
        }

        // Check if project and unit clashes;
        [NonAction]
        public static Boolean DuplicateUserExistsForModification(Models.User user)
        {
            Debug.WriteLine("Checking for duplicate" + user.PurchaserID + " projectID: " + user.ProjectID + "unit: " + user.Unit.ToUpper());
            int count = 0;

            MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT COUNT(PurchaserID) AS userExists FROM user WHERE ProjectID = @projectID AND Unit = @unit AND PurchaserID != @id");
            cmd.Parameters.Add("@id", MySqlDbType.VarChar).Value = user.PurchaserID;
            cmd.Parameters.Add("@projectID", MySqlDbType.Int32).Value = user.ProjectID;
            cmd.Parameters.Add("@unit", MySqlDbType.VarChar).Value = user.Unit.ToUpper();

            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                count = Convert.ToInt32(rd["userExists"].ToString());
            }

            rd.Close();
            cmd.Dispose();
            cn.Close();

            return count > 0;

        }

        [NonAction]
        public static List<SelectListItem> GetProjectList(int eventID)
        {
            List<SelectListItem> Projects = new List<SelectListItem>();

            MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT * FROM project WHERE EventID = @eventID");
            cmd.Parameters.Add("@eventID", MySqlDbType.Int32).Value = eventID;
            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                Projects.Add(new SelectListItem() { Text = rd["ProjectName"].ToString(), Value = Convert.ToInt32(rd["ProjectID"]).ToString()});
            }
            rd.Close();
            cmd.Dispose();

            cn.Close();

            return Projects;
        }

        [NonAction]
        public static List<Project> GetPurchasersCount(int eventID)
        {
            List<Project> projectList = new List<Project>();

            MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT * FROM project WHERE EventID = @eventID;");
            cmd.Parameters.Add("@eventID", MySqlDbType.Int32).Value = eventID;
            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                Models.Project project = new Models.Project();
                project.ProjectID = rd.GetInt16("ProjectID");
                project.ProjectName = rd["ProjectName"].ToString();
                project.EventID = eventID;
                project.NoOfProjects = rd.GetInt32("NoOfProject");
                projectList.Add(project);
            }

            rd.Close();
            cmd.Dispose();
            cn.Close();

            return projectList;
        }

        [NonAction]
        public static List<Project> GetWinnersCount(int eventID)
        {
            List<Project> projectList = new List<Project>();

            MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT project.ProjectID, project.ProjectName, COUNT(DISTINCT(user.PrizeWon)) AS PrizesWon FROM `project` INNER JOIN `user` ON project.ProjectID = user.ProjectID WHERE user.PrizeWon != 0 AND user.EventID = @eventID GROUP BY project.ProjectID");
            cmd.Parameters.Add("@eventID", MySqlDbType.Int32).Value = eventID;
            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                Models.Project project = new Models.Project();
                project.ProjectID = rd.GetInt16("ProjectID");
                project.ProjectName = rd["ProjectName"].ToString();
                project.EventID = eventID;
                project.NoOfProjects = rd.GetInt32("PrizesWon");
                projectList.Add(project);
            }

            rd.Close();
            cmd.Dispose();
            cn.Close();

            return projectList;
        }

        [NonAction]
        public string StaffLuckyDraw(int eventID)
        {
            Models.User user = new Models.User();

            MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT * FROM user WHERE user.EventID = @eventID AND user.StaffWon = 0 ORDER BY RAND() LIMIT 1");
            cmd.Parameters.Add("@eventID", MySqlDbType.Int32).Value = eventID;
            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                user.PurchaserID = Convert.ToInt32(rd["PurchaserID"].ToString());
                user.Name = rd["Name"].ToString();
                user.ICNumber = rd["ICNumber"].ToString();
                user.EmailAddress = rd["EmailAddress"].ToString();
                user.ContactNumber = rd["ContactNumber"].ToString();
                user.EventID = Convert.ToInt32(rd["EventID"].ToString());
                user.ProjectID = Convert.ToInt32(rd["ProjectID"].ToString());
                user.Unit = rd["Unit"].ToString();
                user.SalesConsultant = rd["SalesConsultant"].ToString();
            }

            rd.Close();
            cmd.Dispose();
            cn.Close();

            UpdateDatabaseAfterStaffWon(user);

            return user.SalesConsultant;
        }

        // Modify existing user;
        [NonAction]
        public void UpdateDatabaseAfterStaffWon(Models.User user)
        {
            try
            {
                MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
                cn.Open();

                MySqlCommand cmd = cn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = String.Format("UPDATE user SET StaffWon = @staffWon WHERE PurchaserID = @id");
                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = user.PurchaserID;               
                cmd.Parameters.Add("@staffWon", MySqlDbType.Int32).Value = 1;

                MySqlDataReader rd = cmd.ExecuteReader();
                rd.Close();
                cmd.Dispose();
                cn.Close();
            }

            catch (Exception e)
            {
                response_message = e.Message;
            }
        }

    }
}