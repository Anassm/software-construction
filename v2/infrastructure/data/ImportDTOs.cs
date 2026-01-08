namespace v2.Infrastructure.Data
{
    public class UserJson
    {
        public string id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string role { get; set; }
        public string created_at { get; set; }
        public int birth_year { get; set; }
        public bool active { get; set; }
    }

    public class VehicleJson
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public string license_plate { get; set; }
        public string make { get; set; }
        public string model { get; set; }
        public string color { get; set; }
        public int year { get; set; }
        public string created_at { get; set; }
    }

    public class ParkingLotJson
    {
        public string id { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public string address { get; set; }
        public int capacity { get; set; }
        public int reserved { get; set; }
        public double tariff { get; set; }
        public double daytariff { get; set; }
        public string created_at { get; set; }
        public Coordinates coordinates { get; set; }
    }

    public class Coordinates { public double lat { get; set; } public double lng { get; set; } }

    public class ReservationJson
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public string parking_lot_id { get; set; }
        public string vehicle_id { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string status { get; set; }
        public string created_at { get; set; }
        public double cost { get; set; }
    }

    public class PaymentJson
    {
        public string transaction { get; set; }
        public double amount { get; set; }
        public string initiator { get; set; }
        public string created_at { get; set; }
        public string completed { get; set; }
        public string hash { get; set; }
        public PaymentTransactionData t_data { get; set; }
    }

    public class PaymentTransactionData
    {
        public double amount { get; set; }
        public string date { get; set; }
        public string method { get; set; }
        public string issuer { get; set; }
        public string bank { get; set; }
    }

    public class SessionJson
    {
        public string id { get; set; }
        public string parking_lot_id { get; set; }
        public string licenseplate { get; set; }
        public string started { get; set; }
        public string stopped { get; set; }
        public string user { get; set; }
        public int duration_minutes { get; set; }
        public double cost { get; set; }
        public string payment_status { get; set; }
    }
}
