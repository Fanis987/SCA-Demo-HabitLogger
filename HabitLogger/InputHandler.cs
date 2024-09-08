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
            try
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
            catch (Exception ex)
            {
                Console.WriteLine("An error occured while trying to view logs in the database:");
                Console.WriteLine(ex.Message);
            }
        }

        private static void InsertLog()
        {
            //Get date from user or return to menu
            var date = GetValidDateFromInput();
            if(date == DateTime.MinValue)
            {
                Console.WriteLine("Returning to menu\n");
                return;
            }

            //Insert log in database
            try
            {
                using (var connection = new SqliteConnection(_connectionStringCopy))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"INSERT INTO DrinkingWater (Date)
                                            VALUES ($date)";
                    command.Parameters.AddWithValue("$date", date);
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Console.WriteLine($"Id: {reader.GetInt32(0)} Date: {reader.GetDateTime(1)}");
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured while trying to insert a log in the database:");
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Log inserted successfully\n");
        }

        private static DateTime GetValidDateFromInput()
        {
            DateTime date = DateTime.MinValue;
            string? inputDateStr = "";
            Console.WriteLine("Please insert date (format:dd-mm-yy) or ) to return to the previous menu:");
            while (true)//loop until valid date is entered
            {
                inputDateStr = Console.ReadLine();
                
                if(inputDateStr == "0")//exit check
                {
                    return DateTime.MinValue;
                }

                if(!DateTime.TryParse(inputDateStr, out date))
                {
                    Console.WriteLine("Invalid date format. Please insert date again (format:dd-mm-yy):");
                    continue;
                }

                if(date > DateTime.Now)
                {
                    Console.WriteLine("Date cannot be in the future. Please insert date again (format:dd-mm-yy):");
                    continue;
                }

                if(date < new DateTime(2024,1,1))
                {
                    Console.WriteLine("Date cannot be before 2024. Please insert date again (format:dd-mm-yy):");
                    continue;
                }

                return date;
            }
        }
    }
}
