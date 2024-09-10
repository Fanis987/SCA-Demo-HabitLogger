using Microsoft.Data.Sqlite;
using System.Text.RegularExpressions;

namespace HabitLogger
{
    internal class InputHandler
    {
        //Support For multiple habits
        private bool _habitMenuDisplayed = true;
        private string _activeHabit = "";

        //A copy of connection string for SQL operations
        private string _connectionStringCopy = "";

        internal void SetConnectionStringCopy(string connectionString)
        {
            _connectionStringCopy = connectionString;
        }

        //====Menu Selector==============================================================================================
        /// <summary>
        /// Displays a menu based on the current state of the program.
        /// </summary>
        internal void DisplayMenu()
        {
            if (_habitMenuDisplayed)
            {
                PrintHabitOptions();
                TryGetChoiceInput(out int input, 5);
                HandleHabitMenuInput(input);
            }
            else
            {
                PrintLogOptions();
                TryGetChoiceInput(out int input, 6);
                HandleLogMenuInput(input);
            }
        }

        //====Habit Menu==============================================================================================
        /// <summary>
        /// Displays the habit menu options for the user to choose from.
        /// </summary>
        /// <param name="input"></param>
        private void PrintHabitOptions()
        {
            Console.WriteLine("\nPress the number of the option you want");
            Console.WriteLine("0 : Exit Program");
            Console.WriteLine("1 : View active habits");
            Console.WriteLine("2 : Set active habit");
            Console.WriteLine("3 : Insert a new habit");
            Console.WriteLine("4 : Delete a habit");
            Console.WriteLine("5 : Access logs of active habit\n");
            Console.WriteLine((_activeHabit == "") ? "No active habit set\n" : $"Active Habit: {_activeHabit}\n");
        }

        /// <summary>
        /// Handles user's input and calls the appropriate method.
        /// </summary>
        /// <param name="input"></param>
        private void HandleHabitMenuInput(int input)
        {
            if (_connectionStringCopy == "")
            {
                Console.WriteLine("Connection string was not properly set. Exiting program");
                Environment.Exit(0);
            }

            switch (input)
            {
                case 0:
                    Console.WriteLine("Exiting program...");
                    Environment.Exit(0);
                    break;
                case 1:
                    //View active habits
                    Console.WriteLine("Active Habits:");
                    List<string> habits = GetActiveHabits();
                    foreach (var habit in habits)
                    {
                        Console.WriteLine(habit);
                    }
                    break;
                case 2:
                    //Set active habit
                    SetActiveHabit();
                    break;
                case 3:
                    //Insert a new habit
                    InsertHabit();
                    break;
                case 4:
                    //Delete a habit
                    DeleteHabit();
                    break;
                case 5:
                    //Switch to log menu
                    SwitchToLogMenu();
                    break;
                default:
                    Console.WriteLine("Invalid option, please try again");
                    break;
            }
        }

        /// <summary>
        /// Gets all active habits from the database's table names
        /// </summary>
        /// <returns>A list of the active habit names</returns>
        private List<string> GetActiveHabits()
        {
            List<string> habits = new();
            try
            {
                using (var connection = new SqliteConnection(_connectionStringCopy))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT name FROM sqlite_schema
                                            WHERE type = 'table'
                                            ORDER BY name;";
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        habits.Add(reader.GetString(0));
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured while trying to insert a habit in the database:");
                Console.WriteLine(ex.Message);
            }
            return habits;
        }

        /// <summary>
        /// Sets the active habit tot he user's selection, if the latter is valid.
        /// </summary>
        private void SetActiveHabit()
        {
            //get all active habits
            List<string> habits = GetActiveHabits();

            //get input from user
            Console.WriteLine("Please insert the name of the habit you want to set as active or 'e' to return to menu:");
            string? habitName = Console.ReadLine();

            //habit name check
            if (!IsValidWord(habitName)) return;

            //check if habit exists
            if (!habits.Contains(habitName))
            {
                Console.WriteLine("Habit requested does not exist. Please check the active habits list or insert a new one.");
                Console.WriteLine("Returning to menu\n");
                return;
            }

            //set active habit
            _activeHabit = habitName;
            Console.WriteLine($"Active habit set to: {_activeHabit}");
        }

        /// <summary>
        /// Creates a new habit table in the database based on user's input.
        /// </summary>
        private void InsertHabit()
        {
            //get input from user
            Console.WriteLine("Please insert the name of the habit you want to add or 'e' to return to menu:");
            string? habitName = Console.ReadLine();

            //habit name check
            if (!IsValidWord(habitName)) return;

            //Create table in database with the name of the habit
            try
            {
                using (var connection = new SqliteConnection(_connectionStringCopy))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = $@" CREATE TABLE IF NOT EXISTS [{habitName}] (
                                            Id INTEGER PRIMARY KEY,
                                            Date DATE NOT NULL
                                            )";
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured while trying to insert a habit in the database:");
                Console.WriteLine(ex.Message);
                return;
            }
            Console.WriteLine($"Habit '{habitName}' was created successfully\n");
        }

        /// <summary>
        /// Helper method that checks if the provided word is a valid habit name.
        /// </summary>
        /// <param name="word">The input from user.</param>
        /// <returns>A boolean describing wheter the input is valid</returns>
        private bool IsValidWord(string? word)
        {
            if (string.IsNullOrEmpty(word))
            {
                Console.WriteLine("Habit name cannot be empty. Returning to menu\n");
                return false;
            }

            if (word == "e")
            {
                Console.WriteLine("Returning to menu\n");
                return false;
            }

            // Check if the word is between 3 and 20 characters using regex
            if (!Regex.IsMatch(word, @"^[a-zA-Z0-9_]{3,20}$"))
            {
                Console.WriteLine("Habit name must be between 3 and 20 characters and can only contain letters, numbers, and underscores. ");
                Console.WriteLine("Returning to menu\n");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Deletes a habit from the database, based on user's input.
        /// </summary>
        private void DeleteHabit()
        {
            //get input from user
            Console.WriteLine("Please insert the name of the habit you want to delete or 'e' to return to menu:");
            string? habitName = Console.ReadLine();

            //habit name check
            if (!IsValidWord(habitName)) return;

            //existance check
            List<string> habits = GetActiveHabits();
            if (!habits.Contains(habitName))
            {
                Console.WriteLine($"Requested habit '{habitName}' does not exist. Please check the active habits list.");
                Console.WriteLine("Returning to menu\n");
                return;
            }

            //Delete table in database with the name of the habit
            try
            {
                using (var connection = new SqliteConnection(_connectionStringCopy))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = $@" DROP TABLE IF EXISTS [{habitName}]";
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured while trying to delete a habit from the database:");
                Console.WriteLine(ex.Message);
                return;
            }
            Console.WriteLine($"Habit '{habitName}' was deleted successfully\n");
        }

        /// <summary>
        /// Switch to the log menu
        /// </summary>
        private void SwitchToLogMenu()
        {
            //selection check
            if (_activeHabit == "")
            {
                Console.WriteLine("No active habit set. Please set an active habit first.");
                return;
            }

            _habitMenuDisplayed = false;
        }

        //====Log Menu==============================================================================================
        /// <summary>
        /// Prints the main options for the user to choose from
        /// </summary>
        private void PrintLogOptions()
        {
            Console.WriteLine("\nPress the number of the option you want");
            Console.WriteLine("0 : Exit Program");
            Console.WriteLine("1 : View habit logs");
            Console.WriteLine("2 : Insert habit log");
            Console.WriteLine("3 : Update habit log");
            Console.WriteLine("4 : Delete habit log");
            Console.WriteLine("5 : Delete all habit logs");
            Console.WriteLine("6 : Return to habit selection menu\n");

        }

        /// <summary>
        /// Handles the user input and calls the appropriate method.
        /// </summary>
        /// <param name="input"> User's input number</param>
        private void HandleLogMenuInput(int input)
        {
            if (_connectionStringCopy == "")
            {
                Console.WriteLine("Connection string was not properly set. Exiting program");
                Environment.Exit(0);
            }

            switch (input)
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
                case 6:
                    _habitMenuDisplayed = true;
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
        private bool TryGetChoiceInput(out int input, int optionsNum)
        {
            string? inputStr = Console.ReadLine();
            if (int.TryParse(inputStr, out input))
            {
                if (!(input >= 0 && input <= optionsNum))
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
        private void PrintAllLogs()
        {
            try
            {
                using (var connection = new SqliteConnection(_connectionStringCopy))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM [{_activeHabit}]";
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
        private void InsertLog()
        {
            //Get date from user or return to menu
            var date = GetValidDateFromInput();
            if (date == DateTime.MinValue)
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
                    command.CommandText = $@"INSERT INTO [{_activeHabit}] (Date)
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
        private DateTime GetValidDateFromInput()
        {
            DateTime date = DateTime.MinValue;
            string? inputDateStr = "";
            Console.WriteLine("Please insert date (format:dd-mm-yy) or 'e' to return to the previous menu:");
            while (true)//loop until valid date is entered
            {
                inputDateStr = Console.ReadLine();

                if (inputDateStr == "e")//exit check
                {
                    return DateTime.MinValue;
                }

                if (!DateTime.TryParse(inputDateStr, out date))
                {
                    Console.WriteLine("Invalid date format. Please insert date again (format:dd-mm-yy):");
                    continue;
                }

                if (date > DateTime.Now)
                {
                    Console.WriteLine("Date cannot be in the future. Please insert date again (format:dd-mm-yy):");
                    continue;
                }

                if (date < new DateTime(2024, 1, 1))
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
        private void UpdateLog()
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
                    command.CommandText = $@"UPDATE [{_activeHabit}]
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
        private void DeleteLog()
        {
            //Chose id to delete
            int delId;
            if (!TryGetValidInput(out delId)) return;

            //check if user is sure
            Console.WriteLine($"Are you sure you want to delete the log with id: {delId}? (y/n)");
            if (!IsConfirmedRequest()) return;

            //Insert log in database
            try
            {
                using (var connection = new SqliteConnection(_connectionStringCopy))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    //delete all rows from the table
                    command.CommandText = $@"DELETE FROM [{_activeHabit}]
                                            WHERE Id = ($id)";
                    command.Parameters.AddWithValue("$id", delId);
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
        /// <returns>Whether the input is valid</returns>
        private bool TryGetValidInput(out int id)
        {
            string inputStr;
            while (true)
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
        private void DeleteAllLogs()
        {
            //check if user is sure
            Console.WriteLine("Are you sure you want to delete all logs? (y/n)");
            if (!IsConfirmedRequest()) return;

            //Insert log in database
            try
            {
                using (var connection = new SqliteConnection(_connectionStringCopy))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    //delete all rows from the table
                    command.CommandText = $@"DELETE FROM [{_activeHabit}]";
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

        /// <summary>
        /// Checks if the user is sure about the request.
        /// </summary>
        /// <returns> Confirmation from the user</returns>
        private bool IsConfirmedRequest()
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