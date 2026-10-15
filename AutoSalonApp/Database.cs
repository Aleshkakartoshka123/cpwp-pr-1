using Npgsql;

namespace AutoSalonApp;

public class Database
{
    public string ConnString = "Host=localhost;Port=5433;Username=admin;Password=admin123;Database=autosalon";

    public NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(ConnString);
    }
}