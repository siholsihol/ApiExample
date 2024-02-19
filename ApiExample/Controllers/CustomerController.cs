using ApiExample.Models;
using ApiHelper;
using Microsoft.AspNetCore.Mvc;
using SQLHelper;

namespace ApiExample.Controllers
{
    public class CustomerController : BaseApiController
    {
        // GET: api/customer
        [HttpGet]
        public IActionResult GetCustomers()
        {
            var queryHelper = new QueryCls();

            try
            {
                var query = "SELECT * FROM Customers (NOLOCK)";

                var result = queryHelper.SqlExecObjectQuery<Customer>(query);

                return Ok(Result<List<Customer>>.Success(result));
            }
            catch (Exception ex)
            {
                return BadRequest(Result.Fail(ex.Message));
            }
        }

        // GET: api/customer/1
        [HttpGet("{customerId}")]
        public IActionResult GetCustomer(string customerId)
        {
            var queryHelper = new QueryCls();

            try
            {
                var query = "SELECT * FROM Customers (NOLOCK) ";
                query += $"WHERE CustomerID = '{customerId}'";

                var result = queryHelper.SqlExecObjectQuery<Customer>(query).FirstOrDefault();

                return Ok(Result<Customer>.Success(result));
            }
            catch (Exception ex)
            {
                return BadRequest(Result.Fail(ex.Message));
            }
        }

        // PUT: api/category
        [HttpPut]
        public IActionResult PutCustomer(Customer customer)
        {
            var queryHelper = new QueryCls();

            try
            {
                var connection = queryHelper.GetConnection();

                var query = "SELECT * FROM Customers (NOLOCK) ";
                query += $"WHERE CustomerID = '{customer.CustomerID}'";

                var result = queryHelper.SqlExecObjectQuery<Customer>(query, connection, false).FirstOrDefault();

                if (result == null)
                    return NotFound(Result.Fail());

                query = "UPDATE Customers ";
                query += $"SET CompanyName = '{customer.CompanyName}' ";
                query += $"WHERE CustomerID = '{customer.CustomerID}'";

                queryHelper.SqlExecNonQuery(query, connection, true);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(Result.Fail(ex.Message));
            }
        }

        // POST: api/category
        [HttpPost]
        public IActionResult PostCustomer(Customer customer)
        {
            var queryHelper = new QueryCls();

            try
            {
                var connection = queryHelper.GetConnection();

                var query = "SELECT * FROM Customers (NOLOCK) ";
                query += $"WHERE CustomerID = '{customer.CustomerID}'";

                var result = queryHelper.SqlExecObjectQuery<Customer>(query, connection, false).FirstOrDefault();

                if (result != null)
                    return BadRequest(Result.Fail());

                query = "INSERT INTO Customers (CustomerID, CompanyName) ";
                query += $"VALUES ('{customer.CustomerID}', '{customer.CompanyName}')";

                queryHelper.SqlExecNonQuery(query, connection, true);

                return CreatedAtAction(nameof(GetCustomer), new { customerId = customer.CustomerID }, customer);
            }
            catch (Exception ex)
            {
                return BadRequest(Result.Fail(ex.Message));
            }
        }

        // DELETE: api/category
        [HttpDelete]
        public IActionResult DeleteCustomer(Customer customer)
        {
            var queryHelper = new QueryCls();

            try
            {
                var connection = queryHelper.GetConnection();

                var query = "SELECT * FROM Customers (NOLOCK) ";
                query += $"WHERE CustomerID = '{customer.CustomerID}'";

                var result = queryHelper.SqlExecObjectQuery<Customer>(query, connection, false).FirstOrDefault();

                if (result == null)
                    return NotFound(Result.Fail());

                query = "DELETE Customers ";
                query += $"WHERE CustomerID = '{customer.CustomerID}'";

                queryHelper.SqlExecNonQuery(query, connection, true);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(Result.Fail(ex.Message));
            }
        }
    }
}
