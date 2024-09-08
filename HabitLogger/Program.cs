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


//Create a connection to the database
try
{
    using (var connection = new SqliteConnection(connectionString) )
    {
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "";
        command.ExecuteNonQuery();
        connection.Close();
    }
}
catch (Exception ex)
{
    Console.WriteLine("An error occured while connecting to the database:");
    Console.WriteLine(ex.Message);
}

//create a table in the database
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

while (true)
{
    int input;
    if (InputHandler.TryGetInput(out input))
    {
        InputHandler.HandleInput(input);
    };
    
}



