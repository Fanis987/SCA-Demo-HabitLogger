using Microsoft.Data.Sqlite;
using System.Net.NetworkInformation;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                if(!(input >= 0 && input <= 5))
                {
                    Console.WriteLine("Input must be between 0-5, please try again");
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
            Console.WriteLine("4 :Delete habit log");
            Console.WriteLine("5 :Delete all habit logs\n");
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
                    UpdateLog();
                    break;
                case 4:
                    Console.WriteLine("Deleting a habit log:");
                    DeleteLog();
                    break;
                case 5:
                    Console.WriteLine("Delete all logs");
                    DeleteAllLogs();
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
                return;
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
                return;
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

        //Update Log
        private static void UpdateLog()
        {

        }

        //Delete Log
        private static void DeleteLog()
        {

        }

        //Delete all logs
        private static void DeleteAllLogs()
        {
            //check if user is sure
            Console.WriteLine("Are you sure you want to delete all logs? (y/n)");
            string? checkInput = Console.ReadLine();
            if(checkInput != "y")
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
                    //delete all rows from the table
                    command.CommandText = @"DELETE FROM DrinkingWater";
                    var reader = command.ExecuteReader();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured while trying to delete all logs in the database:");
                Console.WriteLine(ex.Message);
                return;
            }
            Console.WriteLine("All logs deleted successfully\n");

        }
    }
}
