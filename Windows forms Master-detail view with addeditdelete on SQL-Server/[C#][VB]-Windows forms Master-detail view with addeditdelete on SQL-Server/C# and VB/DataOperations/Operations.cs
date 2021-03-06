﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataOperations
{

    public class Operations
    {
        private string ConnectionString = "Data Source=KARENS-PC;Initial Catalog=MasterDetailSimple;Integrated Security=True";
        public bool HasErrors { get; set; }
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// This is the master data
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        private BindingSource _Master = new BindingSource();
        public BindingSource Master
        {
            get
            {
                return _Master;
            }
            set
            {
                _Master = value;
            }
        }
        public Operations()
        {
            GetStateInformation();
        }
        /// <summary>
        /// This is the details to the Master 
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        private BindingSource _Details = new BindingSource();
        public BindingSource Details
        {
            get
            {
                return _Details;
            }
            set
            {
                _Details = value;
            }
        }
        /// <summary>
        /// This will contain order details for the current order of a customer
        /// </summary>
        /// <returns></returns>
        private BindingSource _OrderDetails = new BindingSource();
        public BindingSource OrderDetails
        {
            get
            {
                return _OrderDetails;
            }
            set
            {
                _OrderDetails = value;
            }
        }
        public List<StateItems> StateInformation { get; set; }
        /// <summary>
        /// For a forum question only
        /// </summary>
        /// <param name="CompanyName"></param>
        /// <param name="NewIdentifier"></param>
        public void DemoInsert(string CompanyName, ref int NewIdentifier)
        {
            using (SqlConnection cn = new SqlConnection { ConnectionString = ConnectionString })
            {
                using (SqlCommand cmd = new SqlCommand { Connection = cn })
                {
                    cmd.CommandText = "INSERT INTO [Customer] (CompanyName) VALUES (@CompanyName); SELECT CAST(scope_identity() AS int);";
                    cmd.Parameters.AddWithValue("@CompanyName", CompanyName);
                    cn.Open();
                    NewIdentifier = (int)cmd.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Read state table data into a list
        /// </summary>
        /// <remarks>
        /// First row is virual
        /// </remarks>
        private void GetStateInformation()
        {
            StateInformation = new List<StateItems>();
            StateInformation.Add(new StateItems { Identifier = -1, Name = "Select one" });
            using (SqlConnection cn = new SqlConnection { ConnectionString = ConnectionString })
            {
                using (SqlCommand cmd = new SqlCommand { Connection = cn, CommandText = "SELECT id,StateName,StateAbbrev FROM StateLookup" })
                {
                    cn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        StateInformation.Add(new StateItems { Identifier = reader.GetInt32(0), Name = reader.GetString(1), Abbreviation = reader.GetString(2) });
                    }
                }
            }
        }
        /// <summary>
        /// Initial data loaded was generated by Red Gate SQL Data Generator
        /// so the invoice numbers will be different than the ones created
        /// here with the database sequence I have provided here.
        /// </summary>
        public void LoadData()
        {

            DataSet ds = new DataSet();

            using (SqlConnection cn = new SqlConnection { ConnectionString = ConnectionString })
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT id,FirstName,LastName,Address,City,State,ZipCode FROM Customer", cn);
                try
                {
                    da.Fill(ds, "Customer");
                    DataTable dt = ds.Tables["Customer"];

                    da = new SqlDataAdapter("SELECT id,CustomerId,OrderDate,Invoice FROM Orders", cn);
                    da.Fill(ds, "Orders");
                    ds.SetRelation("Customer", "Orders", "Id", "CustomerId");

                    da = new SqlDataAdapter("SELECT id,OrderId ,ProductName,UnitPrice,Quantity FROM OrderDetails", cn);
                    da.Fill(ds, "OrderDetails");
                    ds.SetRelation("Orders", "OrderDetails", "Id", "OrderId");

                    Master.DataSource = ds;
                    Master.DataMember = ds.Tables[0].TableName;

                    Details.DataSource = Master;
                    Details.DataMember = ds.Relations[0].RelationName;

                    OrderDetails.DataSource = Details;
                    OrderDetails.DataMember = ds.Relations[1].RelationName;
                    
                    //DataTable CustomerTable = ds.Tables["Customer"];
                    //DataTable OrderTable = ds.Tables["Orders"];

                }
                catch (Exception ex)
                {
                    HasErrors = true;
                    ExceptionMessage = ex.Message;
                }
            }
        }

        /// <summary>
        /// Add new order for a customer
        /// </summary>
        /// <param name="CustomerId">Identifies the customer for this order</param>
        /// <param name="OrderDate"></param>
        /// <param name="Invoice"></param>
        /// <param name="NewPrimaryKeyValue">new primary key for new order row</param>
        /// <remarks>
        /// Here I'm using NewPrimaryKeyValue as success of the operations
        /// while in AddCustomer I use a function returning a bool. I simply
        /// wanted to show two variations on how one might write this code.
        /// </remarks>
        public void AddOrder(int CustomerId, DateTime OrderDate, ref string Invoice, ref int NewPrimaryKeyValue)
        {
            using (SqlConnection cn = new SqlConnection { ConnectionString = ConnectionString })
            {
                using (SqlCommand cmd = new SqlCommand { Connection = cn })
                {

                    cmd.CommandText = "INSERT INTO Orders (CustomerId,OrderDate,Invoice) VALUES (@CustomerId,@OrderDate,@Invoice)";

                    try
                    {
                        cn.Open();

                        cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
                        cmd.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                        Invoice = GenerateInvoice(cn);
                        cmd.Parameters.AddWithValue("@Invoice", Invoice);

                        int result = cmd.ExecuteNonQuery();
                        if (result == 1)
                        {
                            cmd.CommandText = "Select @@Identity";
                            NewPrimaryKeyValue = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                    catch (Exception ex)
                    {
                        HasErrors = true;
                        ExceptionMessage = ex.Message;
                        NewPrimaryKeyValue = -1;
                    }

                }
            }
        }
        /// <summary>
        /// We only permit order date changed as the invoice is a generated value that in my
        /// cases a business rule to not permit the value to change.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="OrderDate"></param>
        /// <returns></returns>
        public bool UpdateOrder(int id, DateTime OrderDate)
        {
            bool success = false;

            string sql = "UPDATE Orders  SET OrderDate = @OrderDate WHERE id = @Id";

            using (SqlConnection cn = new SqlConnection { ConnectionString = ConnectionString })
            {
                using (SqlCommand cmd = new SqlCommand { Connection = cn, CommandText = sql })
                {

                    cmd.Parameters.AddWithValue("@OrderDate", OrderDate);
                    cmd.Parameters.AddWithValue("@id", id);

                    try
                    {
                        cn.Open();
                        success = cmd.ExecuteNonQuery() == 1;
                    }
                    catch (Exception ex)
                    {
                        HasErrors = true;
                        ExceptionMessage = ex.Message;
                    }
                }
            }

            return success;

        }
        /// <summary>
        /// Remove a single order
        /// </summary>
        /// <param name="OrderId">Primary key to identify a valid order record</param>
        public bool RemoveSingleOrder(int OrderId)
        {
            bool success = false;

            string sql = "DELETE FROM [Orders] WHERE id = @id";

            using (SqlConnection cn = new SqlConnection { ConnectionString = ConnectionString })
            {
                using (SqlCommand cmd = new SqlCommand { Connection = cn, CommandText = sql })
                {

                    cmd.Parameters.AddWithValue("id", OrderId);

                    try
                    {
                        cn.Open();
                        success = cmd.ExecuteNonQuery() == 1;
                    }
                    catch (Exception ex)
                    {
                        HasErrors = true;
                        ExceptionMessage = ex.Message;
                    }
                }
            }

            return success;

        }
        /// <summary>
        /// Add a new customer
        /// </summary>
        /// <param name="FirstName"></param>
        /// <param name="LastName"></param>
        /// <param name="Address"></param>
        /// <param name="City"></param>
        /// <param name="State"></param>
        /// <param name="ZipCode"></param>
        /// <param name="NewPrimaryKeyValue">new primary key for newly added customer row</param>
        /// <returns></returns>
        /// <remarks>
        /// See comments in AddOrder as I did this method different than AddOrder
        /// to show variations.
        /// </remarks>
        public bool AddCustomer(string FirstName, string LastName, string Address, string City, string State, string ZipCode, ref int NewPrimaryKeyValue)
        {
            bool Success = false;
            using (SqlConnection cn = new SqlConnection { ConnectionString = ConnectionString })
            {
                using (SqlCommand cmd = new SqlCommand { Connection = cn })
                {
                    cmd.CommandText = "INSERT INTO Customer (FirstName,LastName,[Address],City,[State],ZipCode) VALUES (@FirstName,@LastName,@Address,@City,@State,@ZipCode)";
                    try
                    {
                        cmd.Parameters.AddWithValue("@FirstName", FirstName);
                        cmd.Parameters.AddWithValue("@LastName", LastName);
                        cmd.Parameters.AddWithValue("@Address", Address);
                        cmd.Parameters.AddWithValue("@City", City);
                        cmd.Parameters.AddWithValue("@State", State);
                        cmd.Parameters.AddWithValue("@ZipCode", ZipCode);
                        cn.Open();

                        int result = cmd.ExecuteNonQuery();
                        if (result == 1)
                        {
                            cmd.CommandText = "Select @@Identity";
                            NewPrimaryKeyValue = Convert.ToInt32(cmd.ExecuteScalar());
                            Success = true;
                        }

                    }
                    catch (Exception ex)
                    {
                        HasErrors = true;
                        ExceptionMessage = ex.Message;
                        NewPrimaryKeyValue = -1;
                        Success = false;
                    }
                }
            }

            return Success;

        }
        /// <summary>
        /// Remove customer (master) by primary key along with tha orders (childern).
        /// To be safe a transation is used.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RemoveCustomerAndOrders(int id)
        {

            string sql = "DELETE FROM Orders WHERE CustomerId =  @CustomerId";
            bool success = false;

            using (SqlConnection cn = new SqlConnection { ConnectionString = ConnectionString })
            {
                cn.Open();

                SqlTransaction trans = cn.BeginTransaction("DeleteOps");

                using (SqlCommand cmd = new SqlCommand { Connection = cn, CommandText = sql, Transaction = trans })
                {

                    cmd.Parameters.AddWithValue("@CustomerId", id);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.CommandText = "DELETE FROM Customer WHERE  id = @id";
                        cmd.ExecuteNonQuery();
                        trans.Commit();
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        HasErrors = true;
                        ExceptionMessage = ex.Message;
                        try
                        {
                            trans.Rollback();
                        }
                        catch (Exception transEx)
                        {
                            HasErrors = true;
                            ExceptionMessage = transEx.Message;
                        }
                    }
                }
            }

            return success;

        }
        /// <summary>
        /// Update a customer
        /// </summary>
        /// <param name="CustomerRow"></param>
        /// <returns></returns>
        public bool UpdateCustomer(DataRow CustomerRow)
        {

            bool success = false;

            string sql = "UPDATE Customer  SET FirstName = @FirstName,LastName = @LastName,[Address] = @Address,City = @City,[State] = @State,ZipCode = @ZipCode WHERE id = @Id";

            using (SqlConnection cn = new SqlConnection { ConnectionString = ConnectionString })
            {
                using (SqlCommand cmd = new SqlCommand { Connection = cn, CommandText = sql })
                {

                    cmd.Parameters.AddWithValue("@FirstName", CustomerRow.Field<string>("FirstName"));
                    cmd.Parameters.AddWithValue("@LastName", CustomerRow.Field<string>("LastName"));
                    cmd.Parameters.AddWithValue("Address", CustomerRow.Field<string>("Address"));
                    cmd.Parameters.AddWithValue("@City", CustomerRow.Field<string>("City"));
                    cmd.Parameters.AddWithValue("@State", CustomerRow.Field<string>("State"));
                    cmd.Parameters.AddWithValue("@ZipCode", CustomerRow.Field<string>("ZipCode"));
                    cmd.Parameters.AddWithValue("@id", CustomerRow.Field<int>("id"));

                    try
                    {
                        cn.Open();
                        success = cmd.ExecuteNonQuery() == 1;
                    }
                    catch (Exception ex)
                    {
                        HasErrors = true;
                        ExceptionMessage = ex.Message;
                    }
                }
            }

            return success;

        }

        /// <summary>
        /// Calls a database sequence in the database to get 
        /// a unique invoice number
        /// 
        /// Requires a open connection as per above.
        /// </summary>
        /// <param name="cn"></param>
        /// <returns></returns>
        public string GenerateInvoice(SqlConnection cn)
        {

            string result = "";
            string sql = "SELECT CONVERT(VARCHAR(4), GETDATE(), 12) + RIGHT('0000' + CAST( NEXT VALUE FOR dbo.GetInvoiceNumber AS VARCHAR(3)),4)";

            using (SqlCommand cmd = new SqlCommand { Connection = cn, CommandText = sql })
            {
                result = cmd.ExecuteScalar().ToString();
            }

            return result;

        }
    }
}
