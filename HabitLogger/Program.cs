//MAIN PROGRAM
using HabitLogger;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

//Get the connection string from a json file
var configBuilder = new ConfigurationBuilder();

configBuilder.AddJsonFile("appSettingsExtra.json",optional:false,reloadOnChange:false);

var config = configBuilder.Build();

var connectionString = config["connectionString"];
if(string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Connection string not found in appSettingsExtra.json");
    Console.WriteLine("Exiting...");
    Environment.Exit(1);
}

//Make an instance of the input handler
InputHandler handler = new ();
handler.SetConnectionStringCopy(connectionString);

//Create a default table in the database for "drinking water"
try
{
    using (var connection = new SqliteConnection(connectionString))
    {
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS DrinkingWater ( 
                        Id INTEGER PRIMARY KEY,
                        Date DA NOT NULL
                        )";
        command.ExecuteNonQuery();
        //closes automatically
    }
}
catch (Exception ex)
{
    Console.WriteLine("An error occured while trying to create a table in the database:");
    Console.WriteLine(ex.Message);
}

//Main loop
while (true)
{
    handler.DisplayMenu();
}