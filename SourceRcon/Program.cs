using System;
using System.Net;
using System.Threading;


namespace SourceRcon
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
            string ipaddress, password, command;
            int port;

            bool interactive;

            if (args.Length > 0)
            {
                if (args.Length == 4)
                {
                    interactive = false;
                    ipaddress = args[0];
                    port = int.Parse(args[1]);
                    password = args[2];
                    command = args[3];
                }

                else
                {
                    Console.WriteLine("To use in interactive mode, use no parameters.");
                    Console.WriteLine("Else use parameters in the form: ip port password command");
                    Console.WriteLine("Enclose the command in \" marks if it is more than one word");
                    Console.WriteLine("E.g. sourcercon 192.168.0.5 27015 testpass \"say Testing!\"");
                    return;
                }
            }
            else
            {
                interactive = true;
                Console.WriteLine("Enter IP Address:");
                ipaddress = Console.ReadLine();
                Console.WriteLine("Enter port:");
                port = int.Parse(Console.ReadLine());
                Console.WriteLine("Enter password:");
                password = Console.ReadLine();
                command = null;
            }

			SourceRcon Sr = new SourceRcon();
			Sr.Errors += new StringOutput(ErrorOutput);
			Sr.ServerOutput += new StringOutput(ConsoleOutput);

            if (Sr.Connect(new IPEndPoint(IPAddress.Parse(ipaddress), port), password))
			{
				while(!Sr.Connected)
				{
					Thread.Sleep(10);
				}
                if(interactive)
                {
                    Console.WriteLine("Ready for commands:");
				    while(true)
				    {
				    	Sr.ServerCommand(Console.ReadLine());
				    }
                }
                else
                {
                    Sr.ServerCommand(command);
                    Thread.Sleep(1000);
                    return;
                }
			}
			else
			{
				Console.WriteLine("No connection!");
				Thread.Sleep(1000);
			}
		}

		static void ErrorOutput(string input)
		{
			Console.WriteLine("Error: {0}", input);
		}

		static void ConsoleOutput(string input)
		{
			Console.WriteLine("Console: {0}", input);
		}

	}
}
