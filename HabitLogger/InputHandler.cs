﻿using Microsoft.Data.Sqlite;

namespace HabitLogger
{
    internal class InputHandler
    {
        //A copy of connection string for SQL operations
        private static string _connectionStringCopy = "";

        internal static void SetConnectionStringCopy(string connectionString)
        {
            _connectionStringCopy = connectionString;
        }

        /// <summary>
        /// Prints the main options for the user to choose from
        /// </summary>
        internal static void PrintOptions()
        {
            Console.WriteLine("\nPress the number of the option you want");
            Console.WriteLine("0 : Exit Program");
            Console.WriteLine("1 : View habit logs");
            Console.WriteLine("2 : Insert habit log");
            Console.WriteLine("3 : Update habit log");
            Console.WriteLine("4 : Delete habit log");
            Console.WriteLine("5 : Delete all habit logs\n");
        }

        /// <summary>
        /// Handles the user input and calls the appropriate method.
        /// </summary>
        /// <param name="input"> User's input number</param>
        internal static void HandleInput(int input)
        {
            if(_connectionStringCopy == "")
            {
                Console.WriteLine("Connection string was not properly set. Exiting program");
                Environment.Exit(0);
            }

            switch(input) 
            {
                case 0:
                    Console.WriteLine("Exiting program...");
                    Environment.Exit(0);
                    break;
                case 1:
                    PrintAllLogs();
                    break;
                case 2:
                    InsertLog();
                    break;
                case 3:
                    UpdateLog();
                    break;
                case 4:
                    DeleteLog();
                    break;
                case 5:
                    DeleteAllLogs();
                    break;
                default:
                    Console.WriteLine("Invalid option, please try again");
                    break;
            }
        }

        /// <summary>
        /// Helper method to get valid user input for the main menu
        /// </summary>
        /// <param name="input">the</param>
        /// <returns> Whether a valid input was obtained</returns>
        internal static bool TryGetChoiceInput(out int input, int optionsNum)
        {
            PrintOptions();
            string? inputStr = Console.ReadLine();
            if(int.TryParse(inputStr, out input))
            {
                if(!(input >= 0 && input <= optionsNum))
                {
                    Console.WriteLine($"Input must be between 0-{optionsNum}, please try again");
                    return false;
                }
                return true;
            }
            else
            {
                Console.WriteLine("Invalid input, please try again with a number");
                return false;
            }
        }

        /// <summary>
        /// Prints all logs in the database
        /// </summary>
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

        /// <summary>
        /// Inserts a log in the database
        /// </summary>
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

                    Console.WriteLine("Adding a new log to habit:");
                    command.ExecuteNonQuery();
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

        /// <summary>
        /// Helper method to get a valid date from the user.
        /// </summary>
        /// <returns> The DateTime from user's input.</returns>
        private static DateTime GetValidDateFromInput()
        {
            DateTime date = DateTime.MinValue;
            string? inputDateStr = "";
            Console.WriteLine("Please insert date (format:dd-mm-yy) or 'e' to return to the previous menu:");
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

        /// <summary>
        ///  Updates a log in the database.
        /// </summary>
        private static void UpdateLog()
        {
            //Chose id to update
            int delId;
            if (!TryGetValidInput(out delId)) return;

            //Get date from user or return to menu
            var date = GetValidDateFromInput();
            if (date == DateTime.MinValue)
            {
                Console.WriteLine("Returning to menu\n");
                return;
            }

            //Try update log in database
            try
            {
                using (var connection = new SqliteConnection(_connectionStringCopy))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"UPDATE DrinkingWater
                                            SET Date = ($date)
                                            WHERE Id = ($id)";
                    command.Parameters.AddWithValue("$id", delId);
                    command.Parameters.AddWithValue("$date", date);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        Console.WriteLine("No log with that id found. No changes were made\n");
                        return;
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured while trying to update a log in the database:");
                Console.WriteLine(ex.Message);
                return;
            }
            Console.WriteLine("Desired log updated successfully\n");
        }

        /// <summary>
        ///  Deletes a log in the database.
        /// </summary>
        private static void DeleteLog()
        {
            //Chose id to delete
            int delId;
            if(!TryGetValidInput(out delId)) return;

            //check if user is sure
            Console.WriteLine($"Are you sure you want to delete the log with id: {delId }? (y/n)");
            if (!IsConfirmedRequest()) return;

            //Insert log in database
            try
            {
                using (var connection = new SqliteConnection(_connectionStringCopy))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    //delete all rows from the table
                    command.CommandText = @"DELETE FROM DrinkingWater
                                            WHERE Id = ($id)";
                    command.Parameters.AddWithValue("$id", delId);
                    int rowsAffected = command.ExecuteNonQuery(); 
                    if(rowsAffected == 0)
                    {
                        Console.WriteLine("No log with that id found. No changes were made\n");
                        return;
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured while trying to delete a log in the database:");
                Console.WriteLine(ex.Message);
                return;
            }
            Console.WriteLine("Desired log deleted successfully\n");
        }

        /// <summary>
        /// Checks if the input is a valid integer and returns it.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static bool TryGetValidInput(out int id)
        {
            string inputStr;
            while(true)
            {
                Console.WriteLine("Please insert the id of the log you want to delete or 'e' to return to the previous menu:");
                inputStr = Console.ReadLine();
                if (inputStr == "e")
                {
                    id = -1;
                    return false;
                }
                if (!int.TryParse(inputStr, out id))
                {
                    Console.WriteLine("Invalid input. Please insert a number:");
                    continue;
                }
                return true;
            }
        }

        /// <summary>
        /// Deletes all logs in the database.
        /// </summary>
        private static void DeleteAllLogs()
        {
            //check if user is sure
            Console.WriteLine("Are you sure you want to delete all logs? (y/n)");
            if(!IsConfirmedRequest()) return;
            
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

        private static bool IsConfirmedRequest()
        {
            string? checkInput = Console.ReadLine();
            if (checkInput != "y")
            {
                Console.WriteLine("Returning to menu...\n");
                return false;
            }
            return true;
        }
    }
}