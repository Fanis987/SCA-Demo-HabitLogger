using System.Net.NetworkInformation;

namespace HabitLogger
{
    internal class InputHandler
    {

        internal static bool TryGetInput(out int input)
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
                    Console.WriteLine("Viewing habit logs");
                    break;
                case 2:
                    Console.WriteLine("Adding a new log to habit");
                    break;
                case 3:
                    Console.WriteLine("Updating a habit log");
                    break;
                case 4:
                    Console.WriteLine("Deleting a habit log");
                    break;
                default:
                    Console.WriteLine("Invalid option, please try again");
                    break;
            }
        }
    }
}
