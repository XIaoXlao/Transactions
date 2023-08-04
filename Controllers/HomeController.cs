using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Transactions.Models;

namespace Transactions.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
       
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult Index(IFormFile file)
        {
            bool uploadStatus = false;
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please select a file.");
            }

            // Check file size
            if (file.Length > 1048576) // 1MB in bytes
            {
                return BadRequest("File size exceeds the allowed limit (1MB).");
            }

            // Check the file extension to allow only CSV or XML formats
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (fileExtension != ".csv" && fileExtension != ".xml")
            {
                return BadRequest("Only CSV or XML files are allowed.");
            }

            // Process the file here (you can save it to a location or read its content)
            if (fileExtension == ".csv")
                uploadStatus = ProcessCsvRecords(file);
            else if (fileExtension == ".xml")
                uploadStatus = ProcessXmlRecords(file);
            else
            {
                return BadRequest("Unknown format.");
            }

            if (uploadStatus)
                return Ok("File uploaded successfully.");
            else
                return BadRequest("File uploaded failed.");
        }

        private bool ProcessCsvRecords(IFormFile file)
        {
            try
            {
                // Read the content of the CSV file
                using (var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
                {
                    DataTable dtRecords = new DataTable();
                    //creating columns  
                    dtRecords.Columns.Add("TransNo");
                    dtRecords.Columns.Add("Amount");
                    dtRecords.Columns.Add("CurrencyCode");
                    dtRecords.Columns.Add("TransDate");
                    dtRecords.Columns.Add("Status");

                    using (var csvParser = new TextFieldParser(reader))
                    {
                        csvParser.SetDelimiters(new string[] { "," });

                        while (!csvParser.EndOfData)
                        {
                            string[] fields = csvParser.ReadFields();

                            // Validate each field for null or empty
                            foreach (var field in fields)
                            {
                                if (string.IsNullOrEmpty(field))
                                {
                                    return false;
                                }
                            }

                            dtRecords.Rows.Add(fields);
                        }
                    }

                    InsertRecords(dtRecords);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during XML reading
                Console.WriteLine("Error while reading CSV file: " + ex.Message);
                return false;
            }            
        }

        public bool ProcessXmlRecords(IFormFile file)
        {
            DataTable dtRecords = new DataTable();

            try
            {
                // Load the XML file using XDocument
                XDocument doc = XDocument.Load(new StreamReader(file.OpenReadStream(), Encoding.UTF8));

                // Use LINQ to XML to query the data
                var Transactions = doc.Descendants("Transactions");

                // Create columns for the DataTable
                dtRecords.Columns.Add("TransNo");
                dtRecords.Columns.Add("Amount");
                dtRecords.Columns.Add("CurrencyCode");
                dtRecords.Columns.Add("TransDate");
                dtRecords.Columns.Add("Status");

                var transactions = doc.Descendants("Transaction");
                foreach (var transaction in transactions)
                {
                    var transactionObj = new Models.Transactions
                    {
                        TransNo = transaction.Attribute("id")?.Value,
                        TransactionDate = DateTime.Parse(transaction.Element("TransactionDate")?.Value),
                        Amount = decimal.Parse(transaction.Element("PaymentDetails")?.Element("Amount")?.Value ?? "0"),
                        CurrencyCode = transaction.Element("PaymentDetails")?.Element("CurrencyCode")?.Value,
                        Status = transaction.Element("Status")?.Value
                    };

                    // Validate all fields for null or empty
                    if (!string.IsNullOrEmpty(transactionObj.TransNo) &&
                        transactionObj.TransactionDate != default &&
                        transactionObj.Amount != 0 &&
                        !string.IsNullOrEmpty(transactionObj.CurrencyCode) &&
                        !string.IsNullOrEmpty(transactionObj.Status))
                    {
                        dtRecords.Rows.Add(transactionObj.TransNo, transactionObj.Amount,
                        transactionObj.CurrencyCode, transactionObj.TransactionDate, transactionObj.Status);
                    }
                    else
                        return false;
                }

                InsertRecords(dtRecords);

                return true;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during XML reading
                Console.WriteLine("Error while reading XML file: " + ex.Message);
                return false;
            }
        }

        private void InsertRecords(DataTable csvdt)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            //creating object of SqlBulkCopy    
            SqlBulkCopy objbulk = new SqlBulkCopy(con);
            //assigning Destination table name    
            objbulk.DestinationTableName = "Transactions";
            //inserting Datatable Records to DataBase    
            con.Open();
            objbulk.WriteToServer(csvdt);
            con.Close();
        }

        
    }    
}
