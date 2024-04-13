using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using Infrastructure_and_Maintaince_and_monitoring_system.Models;
using System.IO;
using System.Data;
using System.Configuration;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Controllers
{
    public class SurveyController : Controller
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public ActionResult Index()
        {
            List<AssetModel> assets = new List<AssetModel>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT AssetID, AssetName FROM Tbl_Asset where Status=1";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    AssetModel asset = new AssetModel
                    {
                        AssetID = (int)reader["AssetID"],
                        AssetName = reader["AssetName"].ToString()
                    };
                    assets.Add(asset);
                }
                connection.Close();
            }
            if  (assets.Count  ==  0)
            {
              View(new List<AssetModel>());
            }

            List<RoomModel> rooms = new List<RoomModel>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT RoomID, RoomNo FROM Tbl_Room where Status=1";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    RoomModel room = new RoomModel
                    {
                        RoomID = (int)reader["RoomID"],
                        RoomNo = reader["RoomNo"].ToString()
                    };
                    rooms.Add(room);
                }
                connection.Close();
            }

            ViewBag.Rooms = new SelectList(rooms, "RoomID", "RoomNo");
            return View(assets);
        }

        [HttpPost]
        public ActionResult Index(List<AssetModel> assets,int selectedRoom)
        {
            List<RoomAssetModel> roomAssets = new List<RoomAssetModel>();
            foreach (var asset in assets)
            {
                RoomAssetModel roomAsset = new RoomAssetModel
                {
                    AssetID = asset.AssetID,
                    RoomID = selectedRoom,
                    Working = asset.Working,
                    NonWorking = asset.NonWorking
                };

                roomAssets.Add(roomAsset);
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO Tbl_Room_Asset (AssetID, RoomID, Working, NonWorking, Status) VALUES (@AssetID, @RoomID, @Working, @NonWorking, @Status)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.Add("@AssetID", System.Data.SqlDbType.Int);
                command.Parameters.Add("@RoomID", System.Data.SqlDbType.Int);
                command.Parameters.Add("@Working", System.Data.SqlDbType.Int);
                command.Parameters.Add("@NonWorking", System.Data.SqlDbType.Int);
                command.Parameters.Add("@Status", System.Data.SqlDbType.Bit);
                connection.Open();
                foreach (var roomAsset in roomAssets)
                {
                    command.Parameters["@AssetID"].Value = roomAsset.AssetID;
                    command.Parameters["@RoomID"].Value = roomAsset.RoomID;
                    command.Parameters["@Working"].Value = roomAsset.Working;
                    command.Parameters["@NonWorking"].Value = roomAsset.NonWorking;
                    command.Parameters["@Status"].Value = roomAsset.Status;
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }

            return RedirectToAction("Index","RoomAsset");
        }
    }
}