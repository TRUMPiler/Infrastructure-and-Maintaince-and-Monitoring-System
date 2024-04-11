
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Web.Mvc;
using Infrastructure_and_Maintaince_and_monitoring_system.Models;

public class AssetController : Controller
{
    private string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

    // GET: Asset
    public ActionResult Asset()
    {
        List<Asset> assets = new List<Asset>();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "SELECT * FROM Tbl_Asset WHERE Status = 1";
            SqlCommand command = new SqlCommand(query, connection);

            connection.Open();
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Asset asset = new Asset
                {
                    AssetID = Convert.ToInt32(reader["AssetID"]),
                    AssetName = reader["AssetName"].ToString(),
                    Status = Convert.ToBoolean(reader["Status"])
                };

                assets.Add(asset);
            }
        }

        return View(assets);
    }

    // GET: Asset/Create
    public ActionResult Create()
    {
        return View();
    }

    // POST: Asset/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(Asset asset)
    {
        if (ModelState.IsValid)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Tbl_Asset (AssetName, Status) VALUES (@AssetName, @Status)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@AssetName", asset.AssetName);
                command.Parameters.AddWithValue("@Status", asset.Status);

                connection.Open();
                command.ExecuteNonQuery();
            }

            return RedirectToAction("Asset");
        }

        return View(asset);
    }

    // GET: Asset/Update/5
    public ActionResult Update(int? id)
    {
        if (id == null)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        Asset asset = null;

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "SELECT * FROM Tbl_Asset WHERE AssetID = @AssetID";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AssetID", id);

            connection.Open();
            SqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                asset = new Asset
                {
                    AssetID = Convert.ToInt32(reader["AssetID"]),
                    AssetName = reader["AssetName"].ToString(),
                    Status = Convert.ToBoolean(reader["Status"])
                };
            }
        }

        if (asset == null)
        {
            return HttpNotFound();
        }

        return View(asset);
    }

    // POST: Asset/Update/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Update(Asset asset)
    {
        if (ModelState.IsValid)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "UPDATE Tbl_Asset SET AssetName = @AssetName, Status = @Status WHERE AssetID = @AssetID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@AssetID", asset.AssetID);
                command.Parameters.AddWithValue("@AssetName", asset.AssetName);
                command.Parameters.AddWithValue("@Status", asset.Status);

                connection.Open();
                command.ExecuteNonQuery();
            }

            return RedirectToAction("Asset");
        }

        return View(asset);
    }

    // POST: Asset/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "UPDATE Tbl_Asset SET Status = 0 WHERE AssetID = @AssetID";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AssetID", id);

            connection.Open();
            command.ExecuteNonQuery();
        }

        return RedirectToAction("Asset");
    }
}