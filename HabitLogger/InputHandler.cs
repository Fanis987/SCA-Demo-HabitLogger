using Microsoft.Data.Sqlite;
using System.Net.NetworkInformation;

namespace HabitLogger
{
    internal class InputHandler
    {
        private static string _connectionStringCopy = "";

        internal static void SetConnectionStringCopy(string connectionString)
        {
            _connectionStringCopy = connectionString;
        }

        internal static bool TryGetChoiceInput(out int input)
        {
            PrintOptions();
            string? inputStr = Console.ReadLine();
            if(int.TryParse(inputStr, out input))
            {
                if(!(input >= 0 && input <= 4))
                {
                    Console.WriteLine("Input must be between 0-4, please try again");
                    return false;
                }
                return true;
            }
            else
            {
                Console.WriteLine("Invalid input, please try again");
                return false;
            }
        }

        internal static void PrintOptions()
        {
            Console.WriteLine("\nPress the number of the option you want");
            Console.WriteLine("0 : Exit Program");
            Console.WriteLine("1 : View habit logs");
            Console.WriteLine("2 :Insert habit log");
            Console.WriteLine("3 :Update habit log");
            Console.WriteLine("4 :Delete habit log\n");
        }

        internal static void HandleInput(int input)
        {
            switch(input) 
            {
                case 0:
                    Console.WriteLine("Exiting program");
                    Environment.Exit(0);
                    break;
                case 1:
                    Console.WriteLine("Viewing habit logs:");
                    PrintAllLogs();
                    break;
                case 2:
                    Console.WriteLine("Adding a new log to habit:");
                    InsertLog();
                    break;
                case 3:
                    Console.WriteLine("Updating a habit log:");
                    break;
                case 4:
                    Console.WriteLine("Deleting a habit log:");
                    break;
                default:
                    Console.WriteLine("Invalid option, please try again");
                    break;
            }
        }


        

        private static void PrintAllLogs()
        {
            using (var connection = new SqliteConnection(_connectionStringCopy))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM DrinkingWater";
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine($"Id: {reader.GetInt32(0)} Date: {reader.GetDateTime(1)}");
                }
                connection.Close();
            }
        }

        private static void InsertLog()
        {
            Console.WriteLine("Please insert date:");
            string? inputDateStr = Console.ReadLine();

            using (var connection = new SqliteConnection(_connectionStringCopy))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"INSERT INTO DrinkingWater (Date)
                                        VALUES ($date)";
                command.Parameters.AddWithValue("$date", inputDateStr);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine($"Id: {reader.GetInt32(0)} Date: {reader.GetDateTime(1)}");
                }
                connection.Close();
            }
        }

    }
}
