using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Text;


namespace WpfApp
{
    public partial class MainWindow : Window
    {
        private const string ConnectionString = "Data Source=localhost; Application Name=Sales; Initial Catalog=Sales; User ID=sa;Password=sa;";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void GetAllOrders_Click(object sender, RoutedEventArgs e)
        {
            string query = "SELECT * FROM Orders";
            ExecuteQuery(query,"OrderID","UserID", "ProductID", "TransactionID", "OrderDate");
        }

        private void GetPopularProduct_Click(object sender, RoutedEventArgs e)
        {
            string query = @"
                        SELECT TOP 1 Products.ProductName, COUNT(Orders.ProductId) AS Count
                        FROM Orders
                        JOIN Users ON Orders.UserId = Users.UserId
                        JOIN Products ON Orders.ProductId = Products.ProductId
                        WHERE DATEDIFF(YEAR, Users.DateOfBirth, GETDATE()) - 
                        CASE 
                        WHEN MONTH(Users.DateOfBirth) > MONTH(GETDATE()) OR 
                        (MONTH(Users.DateOfBirth) = MONTH(GETDATE()) AND DAY(Users.DateOfBirth) > DAY(GETDATE())) 
                        THEN 1 ELSE 0 END
                        BETWEEN 20 AND 30
                        GROUP BY Products.ProductName
                        ORDER BY Count DESC";

            ExecuteQuery(query, "ProductName", "Count");  
        }

        private void ExecuteQueryWithParameters(string query, int startAge, int endAge)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@startAge", startAge);
                    command.Parameters.AddWithValue("@endAge", endAge);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);
                        Output.Text = dataTable.Rows.Count > 0 ? dataTable.Rows[0]["ProductName"].ToString() : "No results.";
                    }
                }
            }
        }

        private void GetTopAgeGroup_Click(object sender, RoutedEventArgs e)
        {
            string query = @"
                            SELECT TOP 1 
                            DATEDIFF(YEAR, Users.DateOfBirth, GETDATE()) - 
                            CASE 
                            WHEN MONTH(Users.DateOfBirth) > MONTH(GETDATE()) OR 
                            (MONTH(Users.DateOfBirth) = MONTH(GETDATE()) AND DAY(Users.DateOfBirth) > DAY(GETDATE())) 
                            THEN 1 ELSE 0 END AS Age,
                            COUNT(Orders.TransactionID) AS TransactionCount
                            FROM Orders
                            JOIN Users ON Orders.UserId = Users.UserId
                            GROUP BY 
                            DATEDIFF(YEAR, Users.DateOfBirth, GETDATE()) - 
                            CASE 
                            WHEN MONTH(Users.DateOfBirth) > MONTH(GETDATE()) OR 
                            (MONTH(Users.DateOfBirth) = MONTH(GETDATE()) AND DAY(Users.DateOfBirth) > DAY(GETDATE())) 
                            THEN 1 ELSE 0 END
                            ORDER BY TransactionCount DESC";

            ExecuteQuery(query, "Age", "TransactionCount");
        }

        private void GetTopRevenueProduct_Click(object sender, RoutedEventArgs e)
        {
            string query = @"
                  SELECT TOP 1 Products.ProductName, SUM(Products.Price) AS TotalRevenue
                  FROM Orders
                  JOIN Products ON Orders.ProductId = Products.ProductId
                  GROUP BY Products.ProductName
                  ORDER BY TotalRevenue DESC";

            ExecuteQuery(query, "ProductName", "TotalRevenue"); ;
        }

        private void ExecuteQuery(string query, params string[] columns)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        if (dataTable.Rows.Count > 0)
                        {
                            StringBuilder result = new StringBuilder();
                            foreach (DataRow row in dataTable.Rows)
                            {
                                foreach (string column in columns)
                                {
                                    if (dataTable.Columns.Contains(column))
                                    {
                                        result.Append($"{column}: {row[column].ToString()}  ");
                                    }
                                }
                                result.AppendLine(); 
                            }
                            Output.Text = result.ToString();
                        }
                        else
                        {
                            Output.Text = "No results found.";
                        }
                    }
                }
            }
        }

    }
}