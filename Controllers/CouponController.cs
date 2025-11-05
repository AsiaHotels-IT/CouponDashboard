using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace CouponDashboard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CouponController : ControllerBase
    {
        private readonly string connectionString = "Server=10.10.0.42;Database=CouponDbV2;User Id=sa;Password=Wutt@1976;TrustServerCertificate=True;";

        [HttpGet("summary")]
        public IActionResult GetCouponSummary()
        {
            var result = new List<object>();
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
                    SELECT 
                        sp.Branch,
                        COALESCE(SUM(r.TotalAmount), 0) AS TotalSales
                    FROM SalesPerson sp
                    LEFT JOIN Receipts r ON sp.ID = r.SalesPersonId AND r.Status != 'cancelled'
                    GROUP BY sp.Branch
                    ORDER BY SUM(r.TotalAmount) DESC
                ", conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new {
                                Branch = reader["Branch"]?.ToString(),
                                TotalSales = reader["TotalSales"] != DBNull.Value ? Convert.ToDouble(reader["TotalSales"]) : 0
                            });
                        }
                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, detail = ex.ToString() });
            }
        }

        [HttpGet("topsales")]
        public IActionResult GetTopSales()
        {
            var result = new List<object>();
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
                    SELECT sp.ID, sp.Name AS SalesPerson, ISNULL(SUM(r.TotalAmount),0) AS TotalSales
                    FROM SalesPerson sp
                    LEFT JOIN Receipts r ON r.SalesPersonId = sp.ID AND r.Status != 'cancelled'
                    GROUP BY sp.ID, sp.Name
                    ORDER BY ISNULL(SUM(r.TotalAmount),0) DESC
                ", conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new {
                                ID = reader["ID"] != DBNull.Value ? Convert.ToInt32(reader["ID"]) : 0,
                                SalesPerson = reader["SalesPerson"]?.ToString(),
                                TotalSales = reader["TotalSales"] != DBNull.Value ? Convert.ToDouble(reader["TotalSales"]) : 0
                            });
                        }
                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, detail = ex.ToString() });
            }
        }
    }
}