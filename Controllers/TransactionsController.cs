using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Controllers
{
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public TransactionsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: Transactions/Details/5
        [HttpGet]
        [Route("api/GetTransactions")]
        public ActionResult<IEnumerable<Models.Transactions>> GetTransactions()
        {
            string dateFormat = "yyyyMMdd HH:mm:ss";
            var transactionsReport = new List<Models.TransactionReport>();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();                

                HttpContext.Request.Query.TryGetValue("searchby", out var parameterValue1);
                HttpContext.Request.Query.TryGetValue("searchValue", out var parameterValue2);
                HttpContext.Request.Query.TryGetValue("searchValue1", out var parameterValue3);
                HttpContext.Request.Query.TryGetValue("searchValue2", out var parameterValue4);

                string searchby = parameterValue1.ToString();
                string searchValue = parameterValue2.ToString();
                string searchValue1 = parameterValue3.ToString();
                string searchValue2 = parameterValue4.ToString();

                string query = "SELECT TransNo, Amount, CurrencyCode, Status FROM Transactions";

                if (!string.IsNullOrEmpty(searchby))
                    if (searchby.ToLower() == "currency")
                        query += " where currencycode = @searchValue";
                    else if (searchby.ToLower() == "transdate" && !string.IsNullOrEmpty(searchValue1) && !string.IsNullOrEmpty(searchValue2))
                        query += " where transdate between @startDate and @endDate";
                    else if (searchby.ToLower() == "status")
                        query += " where status = @searchValue";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@searchValue", searchValue));
                    if (!string.IsNullOrEmpty(searchValue1))
                        command.Parameters.Add(new SqlParameter("@startDate", Convert.ToDateTime(searchValue1).ToString(dateFormat)));
                    if (!string.IsNullOrEmpty(searchValue2))
                        command.Parameters.Add(new SqlParameter("@endDate", Convert.ToDateTime(searchValue2).ToString(dateFormat)));

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var transaction = new Models.TransactionReport
                            {
                                id = reader["TransNo"].ToString(),
                                payment = Convert.ToDecimal(reader["Amount"]).ToString("F2") + " " + reader["CurrencyCode"].ToString(),
                                Status = reader["Status"].ToString()
                            };

                            if (transaction.Status.ToLower() == "approved")
                                transaction.Status = "A";
                            else if (transaction.Status.ToLower() == "failed" || transaction.Status.ToLower() == "rejected")
                                transaction.Status = "R";
                            else if (transaction.Status.ToLower() == "finished" || transaction.Status.ToLower() == "done")
                                transaction.Status = "D";

                            transactionsReport.Add(transaction);
                        }
                    }
                }
            }

            return Ok(transactionsReport);
        }


    }
}
