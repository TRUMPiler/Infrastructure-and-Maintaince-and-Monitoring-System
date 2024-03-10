using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.Mvc;

namespace Infrastructure_and_Maintaince_and_monitoring_system
{
    public class Class1
    {
        // Adjust your connection string as needed
        private string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=YourDatabaseName;Integrated Security=True";

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                string extension = Path.GetExtension(file.FileName).ToLower();
                if (extension == ".csv")
                {
                    try
                    {
                        DataTable dt = ProcessCSV(file);
                        InsertData(dt);
                        ViewBag.Message = "CSV data imported successfully.";
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = "Error: " + ex.Message;
                    }
                }
                else
                {
                    ViewBag.Message = "Please upload a valid CSV file.";
                }
            }
            else
            {
                ViewBag.Message = "Please select a CSV file.";
            }
            return View("Index");
        }

        private DataTable ProcessCSV(HttpPostedFileBase file)
        {
            // ... (Same as before, reads the CSV into a DataTable)
        }

        private void InsertData(DataTable dt)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                foreach (DataRow row in dt.Rows)
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO YourDatabaseTable (column1, column2, ...) VALUES (@column1, @column2, ...)", con))
                    {
                        // Add parameters with values from the CSV row
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            cmd.Parameters.AddWithValue("@" + dt.Columns[i].ColumnName, row[i]);
                        }

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}