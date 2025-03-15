using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

class TestConnection
{
    public static void Run() // Đổi từ Main() thành Run()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        string connectionString = config.GetConnectionString("TourismDB");

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                Console.WriteLine("✅ Kết nối thành công đến SQL Server!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi kết nối: " + ex.Message);
            }
        }
    }
}
