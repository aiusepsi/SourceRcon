using System;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using SourceRconLib;


namespace SourceRcon
{
	/// <summary>
	/// Program class.
    /// </summary>
	class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
        /// 
		[STAThread]
		static int Main(string[] args)
		{
            // Grab English strings for the interface.
            ILanguage lang = new English();

            string password, command;
            IPAddress ip;
            int port;

            bool interactive;

            // Parse command-line to tell if we're doing a one-shot command or if we're running interactive.
            if (args.Length > 0)
            {
                // There are arguments, so not interactive.
                interactive = false;

                // Four arguments indicates this is a one-shot command.
                if (args.Length == 4)
                {
                    try
                    {
                        ip = IPAddress.Parse(args[0]);
                        port = int.Parse(args[1]);
                    }
                    catch (FormatException fe)
                    {
                        Console.WriteLine(lang["invalidparams"], fe.Message);
                        return -1;
                    }
                    catch (OverflowException oe)
                    {
                        Console.WriteLine(lang["invalidparams"], oe.Message);
                        return -1;
                    }
                    password = args[2];
                    command = args[3];
                }

                else
                {
                    // Print out the usage instructions.
                    Console.WriteLine(lang["usage_instructions"]);
                    return 1;
                }
            }
            else
            {
                interactive = true;

                #if DEBUG
                Console.WriteLine("Use quick debug? Press y, else anything else!");
                if (Console.ReadKey().KeyChar == 'y')
                {
                    Console.ReadLine();
                    Console.WriteLine();
                    ip = IPAddress.Parse("192.168.1.50");
                    port = 27015;
                    password = "blah";
                }
                else
                {
                #endif


                // Walk the user through entering parameters, & prevent them from entering anything invalid:

                do
                {
                    Console.WriteLine(lang["enterip"]);
                }
                while (!IPAddress.TryParse(Console.ReadLine(), out ip));

                do
                {
                    do
                    {
                        Console.WriteLine(lang["enterport"]);
                    }
                    while (!int.TryParse(Console.ReadLine(), out port));
                }
                while (port < IPEndPoint.MinPort && port > IPEndPoint.MaxPort);

                // Valve's problem to stop people breaking the server with malformed passwords, not mine!
                Console.WriteLine(lang["enterpassword"]);
                password = Console.ReadLine();

                #if DEBUG
                }
                #endif

                command = null;
            }

			Rcon Sr = new Rcon();

            // Wire up our event handlers to receive errors from the server:
            Sr.Errors += (MessageCode, Message) => Console.WriteLine(lang["error"], MessageCode.ToString());

            bool IsConnected = false;

            // Now, we'll actually try to connect!
            try
            {
                IsConnected = Sr.ConnectBlocking(new IPEndPoint(ip, (int)port), password);
            }
            catch(ArgumentOutOfRangeException e)
            {
                Console.WriteLine(lang["invalidparams"], e.Message);
                return -1;
            }
            
            if (IsConnected)
            {
                if (interactive)
                {
                    Console.WriteLine(lang["commandready"]);
                    // Just pull lines from the input and send them off.
                    while (true)
                    {
                        // Sr.ServerCommand(Console.ReadLine());
                        Console.WriteLine(Sr.ServerCommandBlocking(Console.ReadLine()));
                    }
                }
                else
                {
                    // Fire a one-shot command.
                    Console.WriteLine(Sr.ServerCommandBlocking(command));
                }
            }
            else
            {
                // Not connected, so complain about it.
                Console.WriteLine(lang["noconn"]);
            }

            return 0;
		}

	}
}
