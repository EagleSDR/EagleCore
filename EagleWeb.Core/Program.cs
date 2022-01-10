using System;
using System.Threading;

namespace EagleWeb.Core
{
    class Program
    {
        private static EagleContext context;

        static void Main(string[] args)
        {
            //Create the context
            context = new EagleContext(@"C:\Users\Roman\Desktop\EagleSDR\");

            //Prompt to set up a new admin user if needed
            if (context.Auth.AccountCount == 0)
                PromptCreateAdminAccount();

            //Init
            context.Init();

            //Run
            context.Run();
        }

        static void PromptCreateAdminAccount()
        {
            //Configure console
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.BackgroundColor = ConsoleColor.Black;

            //Write info
            Console.WriteLine("********************************************************************************************");
            Console.WriteLine("");
            Console.WriteLine("WELCOME TO EAGLESDR. There are no accounts created. A new admin account will now be created.");
            Console.WriteLine("You'll be able to log into this account to manage the EagleSDR server. Complete the form.");
            Console.WriteLine("");
            Console.WriteLine("********************************************************************************************");
            Console.WriteLine("");
            string username = PromptField("USERNAME", false);
            string password = PromptField("PASSWORD", true);
            Console.Clear();

            //Add
            context.Auth.CreateUser(username, password, out Auth.EagleAccount account);
            account.IsAdmin = true;
        }

        static string PromptField(string name, bool censor)
        {
            Console.Write(name + " >");
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;

            string result = "";
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.KeyChar == '\b')
                    continue;
                if (key.KeyChar != '\n' && key.KeyChar != '\r')
                {
                    result += key.KeyChar;
                    Console.Write(censor ? '*' : key.KeyChar);
                }
            } while ((key.KeyChar != '\n' && key.KeyChar != '\r') || result.Length == 0);

            Console.ForegroundColor = color;
            Console.WriteLine();
            return result;
        }
    }
}
