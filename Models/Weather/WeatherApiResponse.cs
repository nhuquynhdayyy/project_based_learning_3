// Trong thư mục Models hoặc một thư mục riêng cho DTOs
namespace TourismWeb.Models.Weather
{
    public class WeatherApiResponse
    {
        public Coord Coord { get; set; }
        public List<WeatherInfo> Weather { get; set; }
        public Main Main { get; set; }
        public Wind Wind { get; set; }
        public Clouds Clouds { get; set; }
        public Sys Sys { get; set; }
        public string Name { get; set; } // Tên thành phố
        public int Cod { get; set; } // Mã trạng thái
    }

    public class Coord
    {
        public double Lon { get; set; }
        public double Lat { get; set; }
    }

    public class WeatherInfo
    {
        public int Id { get; set; }
        public string Main { get; set; } // Ví dụ: "Clouds", "Rain"
        public string Description { get; set; } // Mô tả chi tiết, ví dụ: "mây rải rác"
        public string Icon { get; set; } // Mã icon, ví dụ: "04d"
    }

    public class Main
    {
        public float Temp { get; set; } // Nhiệt độ (Kelvin mặc định, có thể đổi sang Celsius)
        public float Feels_like { get; set; }
        public float Temp_min { get; set; }
        public float Temp_max { get; set; }
        public int Pressure { get; set; }
        public int Humidity { get; set; } // Độ ẩm (%)
    }

    public class Wind
    {
        public float Speed { get; set; } // Tốc độ gió (m/s)
        public int Deg { get; set; }
    }

    public class Clouds
    {
        public int All { get; set; } // Phần trăm mây (%)
    }

    public class Sys
    {
        public string Country { get; set; } // Mã quốc gia
        public long Sunrise { get; set; } // Unix timestamp
        public long Sunset { get; set; } // Unix timestamp
    }

    // ViewModel để truyền dữ liệu đã xử lý sang View
    public class WeatherViewModel
    {
        public string CityName { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public double TemperatureCelsius { get; set; }
        public int Humidity { get; set; }
        public double WindSpeed { get; set; } // m/s
        public string ErrorMessage { get; set; }
        // Có thể thêm các ngày dự báo nếu API hỗ trợ và bạn muốn hiển thị
    }
}