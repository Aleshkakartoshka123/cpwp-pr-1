using AutoSalonApp;

Database db = new Database();
Menu menu = new Menu();

// Проверка подключения
try
{
    var conn = db.GetConnection();
    conn.Open();
    Console.WriteLine("Подключение к БД успешно");
    conn.Close();
}
catch (Exception ex)
{
    Console.WriteLine("Ошибка подключения: " + ex.Message);
    return;
}

menu.Run();