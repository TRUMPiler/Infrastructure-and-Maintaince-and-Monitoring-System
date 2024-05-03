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
            if (model.From_RoomID.Contains(model.To_RoomID))
            {
                string script = "<script>alert('From and to Room Cannot be Same');window.location='/AssetTransfer/Add'</script>";
                return Content(script, "text/html");
            }
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
            if (model.From_RoomID.Contains(model.To_RoomID))
            {
                string script = "<script>alert('From and to Room Cannot be Same');window.location='/AssetTransfer/Update/"+model.ATid+"'</script>";
                return Content(script, "text/html");
            }
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
            string query = "SELECT t.ATid, t.TransferDate, CAST(fr.Wing AS VARCHAR(50)) + CAST(fr.RoomNo AS VARCHAR(50)) AS FromRoom, CAST(tr.Wing AS VARCHAR(50)) + CAST(tr.RoomNo AS VARCHAR(50)) AS ToRoom,a.AssetName,t.Quantity FROM  Tbl_Asset_Transfer AS t JOIN Tbl_Room AS fr ON t.From_RoomID = fr.RoomID JOIN Tbl_Room AS tr ON t.To_RoomID = tr.RoomID JOIN Tbl_Asset AS a ON t.AssetID = a.AssetID;";
            SqlCommand command = new SqlCommand(query, connection);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                AssetTransferModel transfer = new AssetTransferModel
                {
                    ATid = Convert.ToInt32(reader["ATid"]),
                    From_RoomID = Convert.ToString(reader["FromRoom"]),
                    To_RoomID = Convert.ToString(reader["ToRoom"]),
                    AssetID = Convert.ToString(reader["AssetName"]),
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
            command.Parameters.AddWithValue("@FromRoomId", Convert.ToInt32(model.From_RoomID));
            command.Parameters.AddWithValue("@ToRoomId", Convert.ToInt32(model.To_RoomID));
            command.Parameters.AddWithValue("@AssetId", Convert.ToInt32(model.AssetID));
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
            command.Parameters.AddWithValue("@FromRoomId", SqlDbType.Int).Value= Convert.ToInt32(model.From_RoomID);
            command.Parameters.AddWithValue("@ToRoomId", SqlDbType.Int).Value = Convert.ToInt32(model.To_RoomID);
            command.Parameters.AddWithValue("@AssetId", SqlDbType.Int).Value = Convert.ToInt32(model.AssetID);
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
                    From_RoomID = Convert.ToString(reader["From_RoomID"]),
                    To_RoomID = Convert.ToString(reader["To_RoomID"]),
                    AssetID = Convert.ToString(reader["AssetID"]),
                    Quantity = Convert.ToInt32(reader["Quantity"])
                };
            }
        }
        return transfer;
    }
}
