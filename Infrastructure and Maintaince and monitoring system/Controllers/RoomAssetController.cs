using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Infrastructure_and_Maintaince_and_monitoring_system.Models;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Web.Helpers;
using System.Configuration;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Controllers
{

    public class RoomAssetController : Controller
        {
            private string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            public ActionResult Index()
            {
                List<RoomAssetsModel> roomAssets = new List<RoomAssetsModel>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM Tbl_Room_Asset";
                    SqlCommand command = new SqlCommand(query, connection);

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        RoomAssetsModel roomAsset = new RoomAssetsModel
                        {
                            RAid = Convert.ToInt32(reader["RAid"]),
                            RoomID = Convert.ToInt32(reader["RoomID"]),
                            AssetID = Convert.ToInt32(reader["AssetID"]),
                            Working = Convert.ToInt32(reader["Working"]),
                            NonWorking = Convert.ToInt32(reader["NonWorking"])
                        };

                        roomAssets.Add(roomAsset);
                    }
                }

                return View(roomAssets);
            }

            public ActionResult Update(int id)
            {
                RoomAssetsModel roomAsset = GetRoomAssetById(id);

                if (roomAsset == null)
                {
                    return HttpNotFound();
                }

                ViewBag.RoomList = GetRoomList();
                ViewBag.AssetList = GetAssetList();

                return View(roomAsset);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Update(RoomAssetsModel model)
            {
                if (ModelState.IsValid)
                {
                    UpdateRoomAsset(model);
                    return RedirectToAction("Index");
                }

                ViewBag.RoomList = GetRoomList();
                ViewBag.AssetList = GetAssetList();

                return View(model);
            }

            [HttpDelete]
            public ActionResult Delete(int id)
            {
                DeleteRoomAsset(id);
                return RedirectToAction("Index");
            }

            private RoomAssetsModel GetRoomAssetById(int id)
            {
                RoomAssetsModel roomAsset = null;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT RoomID, AssetID, Working, NonWorking FROM Tbl_Room_Asset WHERE RAid = @RAid";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@RAid", id);

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        roomAsset = new RoomAssetsModel
                        {
                            RAid = id,
                            RoomID = Convert.ToInt32(reader["RoomID"]),
                            AssetID = Convert.ToInt32(reader["AssetID"]),
                            Working = Convert.ToInt32(reader["Working"]),
                            NonWorking = Convert.ToInt32(reader["NonWorking"])
                        };
                    }
                }

                return roomAsset;
            }

            private void UpdateRoomAsset(RoomAssetsModel model)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Tbl_Room_Asset SET RoomID = @RoomID, AssetID = @AssetID, Working = @Working, NonWorking = @NonWorking WHERE RAid = @RAid";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@RoomID", model.RoomID);
                    command.Parameters.AddWithValue("@AssetID", model.AssetID);
                    command.Parameters.AddWithValue("@Working", model.Working);
                    command.Parameters.AddWithValue("@NonWorking", model.NonWorking);
                    command.Parameters.AddWithValue("@RAid", model.RAid);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            private void DeleteRoomAsset(int id)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM Tbl_Room_Asset WHERE RAid = @RAid";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@RAid", id);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            private IEnumerable<SelectListItem> GetAssetList()
            {
                List<SelectListItem> assets = new List<SelectListItem>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT AssetID, AssetName FROM Tbl_Asset";
                    SqlCommand command = new SqlCommand(query, connection);

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int assetID = Convert.ToInt32(reader["AssetID"]);
                        string assetName = reader["AssetName"].ToString();
                        assets.Add(new SelectListItem { Value = assetID.ToString(), Text = assetName });
                    }
                }

                return assets;
            }

            private IEnumerable<SelectListItem> GetRoomList()
            {
                List<SelectListItem> rooms = new List<SelectListItem>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT RoomID, CONCAT(Wing, ' ', RoomNo) AS RoomName FROM Tbl_Room";
                    SqlCommand command = new SqlCommand(query, connection);

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int roomID = Convert.ToInt32(reader["RoomID"]);
                        string roomName = reader["RoomName"].ToString();
                        rooms.Add(new SelectListItem { Value = roomID.ToString(), Text = roomName });
                    }
                }

                return rooms;
            }
    }
}
