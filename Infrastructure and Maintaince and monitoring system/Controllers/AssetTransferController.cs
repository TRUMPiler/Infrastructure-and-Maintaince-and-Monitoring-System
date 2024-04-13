using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.Mvc;
using Infrastructure_and_Maintaince_and_monitoring_system.Models;

public class AssetTransferController : Controller
{
    private string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

    public ActionResult Index()
    {
        List<AssetTransferModel> transfers = GetTransfers();
        return View(transfers);
    }

    public ActionResult Add()
    {
        ViewBag.RoomList = GetRoomList();
        ViewBag.AssetList = GetAssetList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Add(AssetTransferModel model)
    {
        if (ModelState.IsValid)
        {
            model.TransferDate = DateTime.ParseExact(DateTime.Now.ToString("yyyy-MM-dd"), "yyyy-MM-dd", CultureInfo.InvariantCulture);
            AddTransfer(model);
            return RedirectToAction("Index");
        }

        ViewBag.RoomList = GetRoomList();
        ViewBag.AssetList = GetAssetList();
        return View(model);
    }

    public ActionResult Update(int id)
    {
        AssetTransferModel model = GetTransferById(id);
        if (model == null)
        {
            return HttpNotFound();
        }
        ViewBag.RoomList = GetRoomList();
        ViewBag.AssetList = GetAssetList();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Update(AssetTransferModel model)
    {
        if (ModelState.IsValid)
        {
            UpdateTransfer(model);
            return RedirectToAction("Index");
        }
        ViewBag.RoomList = GetRoomList();
        ViewBag.AssetList = GetAssetList();
        return View(model);
    }


    public ActionResult Delete(int id)
    {
        DeleteTransfer(id);
        return RedirectToAction("Index");
    }

    private List<AssetTransferModel> GetTransfers()
    {
        List<AssetTransferModel> transfers = new List<AssetTransferModel>();
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "SELECT * FROM Tbl_Asset_Transfer";
            SqlCommand command = new SqlCommand(query, connection);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                AssetTransferModel transfer = new AssetTransferModel
                {
                    ATid = Convert.ToInt32(reader["ATid"]),
                    From_RoomID = Convert.ToInt32(reader["From_RoomID"]),
                    To_RoomID = Convert.ToInt32(reader["To_RoomID"]),
                    AssetID = Convert.ToInt32(reader["AssetID"]),
                    Quantity = Convert.ToInt32(reader["Quantity"]),
                    TransferDate = Convert.ToDateTime(reader["TransferDate"])
                };
                transfers.Add(transfer);
            }
        }
        return transfers;
    }

    private void AddTransfer(AssetTransferModel model)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "INSERT INTO Tbl_Asset_Transfer (TransferDate, From_RoomID, To_RoomID, AssetID, Quantity) VALUES (@TransferDate, @FromRoomId, @ToRoomId, @AssetId, @Quantity)";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.Add("@TransferDate", model.TransferDate);
            command.Parameters.AddWithValue("@FromRoomId", model.From_RoomID);
            command.Parameters.AddWithValue("@ToRoomId", model.To_RoomID);
            command.Parameters.AddWithValue("@AssetId", model.AssetID);
            command.Parameters.AddWithValue("@Quantity", model.Quantity);
            connection.Open();
            command.ExecuteNonQuery();
        }
    }

    private void UpdateTransfer(AssetTransferModel model)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "UPDATE Tbl_Asset_Transfer SET From_RoomID = @FromRoomId, To_RoomID = @ToRoomId, AssetID = @AssetId, Quantity = @Quantity WHERE ATid = @TransferId";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@FromRoomId", SqlDbType.Int).Value=model.From_RoomID;
            command.Parameters.AddWithValue("@ToRoomId", SqlDbType.Int).Value = model.To_RoomID;
            command.Parameters.AddWithValue("@AssetId", SqlDbType.Int).Value = model.AssetID;
            command.Parameters.AddWithValue("@Quantity", SqlDbType.Int).Value = model.Quantity;
            command.Parameters.AddWithValue("@TransferId", SqlDbType.Int).Value = model.ATid;


            connection.Open();
            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                // Handle update failure
                throw new Exception("Update failed: no rows affected.");
            }
        }
    } 
    private void DeleteTransfer(int id)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "DELETE FROM Tbl_Asset_Transfer WHERE ATid = @TransferId";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TransferId", id);
            connection.Open();
            command.ExecuteNonQuery();
        }
    }

    private List<SelectListItem> GetRoomList()
    {
        List<SelectListItem> roomList = new List<SelectListItem>();
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "SELECT RoomID, CONCAT(Wing, ' ', RoomNo) AS RoomInfo FROM Tbl_Room";
            SqlCommand command = new SqlCommand(query, connection);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                roomList.Add(new SelectListItem
                {
                    Value = reader["RoomID"].ToString(),
                    Text = reader["RoomInfo"].ToString()
                });
            }
        }
        return roomList;
    }

    private List<SelectListItem> GetAssetList()
    {
        List<SelectListItem> assetList = new List<SelectListItem>();
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "SELECT AssetID, AssetName FROM Tbl_Asset";
            SqlCommand command = new SqlCommand(query, connection);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                assetList.Add(new SelectListItem
                {
                    Value = reader["AssetID"].ToString(),
                    Text = reader["AssetName"].ToString()
                });
            }
        }
        return assetList;
    }

    private AssetTransferModel GetTransferById(int id)
    {
        AssetTransferModel transfer = null;
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "SELECT * FROM Tbl_Asset_Transfer WHERE ATid = @TransferId";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TransferId", id);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                transfer = new AssetTransferModel
                {
                    ATid = Convert.ToInt32(reader["ATid"]),
                    TransferDate = Convert.ToDateTime(reader["TransferDate"]),
                    From_RoomID = Convert.ToInt32(reader["From_RoomID"]),
                    To_RoomID = Convert.ToInt32(reader["To_RoomID"]),
                    AssetID = Convert.ToInt32(reader["AssetID"]),
                    Quantity = Convert.ToInt32(reader["Quantity"])
                };
            }
        }
        return transfer;
    }
}
