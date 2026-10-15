using Npgsql;

namespace AutoSalonApp;

public class Menu
{
    Database db = new Database();

    public void Run()
    {
        while (true)
        {
            Console.WriteLine("\n=== Автосалон ===");
            Console.WriteLine("1. Автомобили");
            Console.WriteLine("2. Поставка");
            Console.WriteLine("3. Продажа");
            Console.WriteLine("4. Отчет");
            Console.WriteLine("0. Выход");
            Console.Write("Выбор: ");
            string c = Console.ReadLine();

            if (c == "1") CarsMenu();
            else if (c == "2") Operation("in");
            else if (c == "3") Operation("out");
            else if (c == "4") Report();
            else if (c == "0") break;
            else Console.WriteLine("Неверный ввод");
        }
    }

    void CarsMenu()
    {
        Console.WriteLine("\n--- Автомобили ---");
        Console.WriteLine("1. Добавить");
        Console.WriteLine("2. Удалить");
        Console.WriteLine("3. Посмотреть все");
        Console.WriteLine("4. Поиск");
        Console.Write("Выбор: ");
        string c = Console.ReadLine();

        if (c == "1") AddCar();
        else if (c == "2") DeleteCar();
        else if (c == "3") ListCars();
        else if (c == "4") SearchCar();
    }

    void AddCar()
    {
        Console.Write("Марка: ");
        string brand = Console.ReadLine();
        Console.Write("Модель: ");
        string model = Console.ReadLine();
        Console.Write("Год: ");
        int year = int.Parse(Console.ReadLine());
        Console.Write("Цвет: ");
        string color = Console.ReadLine();
        Console.Write("VIN: ");
        string vin = Console.ReadLine();
        Console.Write("Цена: ");
        decimal price = decimal.Parse(Console.ReadLine());
        Console.Write("Количество: ");
        int qty = int.Parse(Console.ReadLine());

        var conn = db.GetConnection();
        conn.Open();

        string sql = "INSERT INTO cars (brand, model, year, color, vin, price, quantity) " +
                     "VALUES (@b, @m, @y, @c, @v, @p, @q)";
        var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("b", brand);
        cmd.Parameters.AddWithValue("m", model);
        cmd.Parameters.AddWithValue("y", year);
        cmd.Parameters.AddWithValue("c", color);
        cmd.Parameters.AddWithValue("v", vin);
        cmd.Parameters.AddWithValue("p", price);
        cmd.Parameters.AddWithValue("q", qty);
        cmd.ExecuteNonQuery();

        conn.Close();
        Console.WriteLine("Автомобиль добавлен");
    }

    void DeleteCar()
    {
        Console.Write("ID: ");
        int id = int.Parse(Console.ReadLine());

        var conn = db.GetConnection();
        conn.Open();

        var cmd = new NpgsqlCommand("DELETE FROM cars WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        int n = cmd.ExecuteNonQuery();

        conn.Close();

        if (n > 0) Console.WriteLine("Удалено");
        else Console.WriteLine("Не найдено");
    }

    void ListCars()
    {
        var conn = db.GetConnection();
        conn.Open();

        var cmd = new NpgsqlCommand("SELECT id, brand, model, year, color, quantity, price FROM cars ORDER BY id", conn);
        var reader = cmd.ExecuteReader();

        Console.WriteLine("\nID | Марка | Модель | Год | Цвет | Кол-во | Цена");
        while (reader.Read())
        {
            Console.WriteLine(reader.GetInt32(0) + " | " +
                              reader.GetString(1) + " | " +
                              reader.GetString(2) + " | " +
                              reader.GetInt32(3) + " | " +
                              reader.GetString(4) + " | " +
                              reader.GetInt32(5) + " | " +
                              reader.GetDecimal(6));
        }

        conn.Close();
    }

    void SearchCar()
    {
        Console.Write("Введите название: ");
        string q = Console.ReadLine();

        var conn = db.GetConnection();
        conn.Open();

        var cmd = new NpgsqlCommand("SELECT id, brand, model, quantity FROM cars WHERE brand LIKE @q OR model LIKE @q", conn);
        cmd.Parameters.AddWithValue("q", "%" + q + "%");
        var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            Console.WriteLine(reader.GetInt32(0) + " | " +
                              reader.GetString(1) + " " +
                              reader.GetString(2) + " | остаток: " +
                              reader.GetInt32(3));
        }

        conn.Close();
    }

    void Operation(string type)
    {
        string title = type == "in" ? "Поставка" : "Продажа";
        Console.WriteLine("\n--- " + title + " ---");

        Console.Write("ID автомобиля: ");
        int id = int.Parse(Console.ReadLine());
        Console.Write("Количество: ");
        int qty = int.Parse(Console.ReadLine());
        Console.Write("Клиент: ");
        string client = Console.ReadLine();

        var conn = db.GetConnection();
        conn.Open();

        // Узнаем текущее количество и цену
        var getCmd = new NpgsqlCommand("SELECT quantity, price FROM cars WHERE id = @id", conn);
        getCmd.Parameters.AddWithValue("id", id);
        var reader = getCmd.ExecuteReader();

        if (!reader.Read())
        {
            Console.WriteLine("Автомобиль не найден");
            conn.Close();
            return;
        }

        int current = reader.GetInt32(0);
        decimal price = reader.GetDecimal(1);
        reader.Close();

        int newQty;
        if (type == "in") newQty = current + qty;
        else newQty = current - qty;

        if (newQty < 0)
        {
            Console.WriteLine("Недостаточно автомобилей");
            conn.Close();
            return;
        }

        // Обновляем количество
        var upCmd = new NpgsqlCommand("UPDATE cars SET quantity = @q WHERE id = @id", conn);
        upCmd.Parameters.AddWithValue("q", newQty);
        upCmd.Parameters.AddWithValue("id", id);
        upCmd.ExecuteNonQuery();

        // Записываем операцию
        decimal amount = price * qty;
        var opCmd = new NpgsqlCommand(
            "INSERT INTO operations (car_id, type, quantity, client_name, amount) " +
            "VALUES (@c, @t, @q, @cl, @a)", conn);
        opCmd.Parameters.AddWithValue("c", id);
        opCmd.Parameters.AddWithValue("t", type);
        opCmd.Parameters.AddWithValue("q", qty);
        opCmd.Parameters.AddWithValue("cl", client);
        opCmd.Parameters.AddWithValue("a", amount);
        opCmd.ExecuteNonQuery();

        conn.Close();
        Console.WriteLine(title + " оформлена. Остаток: " + newQty);
    }

    void Report()
    {
        Console.Write("Введите дату (например 2026-10-15): ");
        string dateStr = Console.ReadLine();

        var conn = db.GetConnection();
        conn.Open();

        var cmd = new NpgsqlCommand(
            "SELECT type, SUM(quantity), SUM(amount) FROM operations " +
            "WHERE date::date = @d GROUP BY type", conn);
        cmd.Parameters.AddWithValue("d", DateTime.Parse(dateStr));

        var reader = cmd.ExecuteReader();

        int inQty = 0, outQty = 0;
        decimal income = 0, expense = 0;

        while (reader.Read())
        {
            if (reader.GetString(0) == "in")
            {
                inQty = (int)reader.GetInt64(1);
                expense = reader.GetDecimal(2);
            }
            else
            {
                outQty = (int)reader.GetInt64(1);
                income = reader.GetDecimal(2);
            }
        }

        conn.Close();

        Console.WriteLine("\n--- Отчет за " + dateStr + " ---");
        Console.WriteLine("Поставлено: " + inQty + " (закупка: " + expense + ")");
        Console.WriteLine("Продано: " + outQty + " (выручка: " + income + ")");
        Console.WriteLine("Прибыль: " + (income - expense));
    }
}