using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace KissCaller.SocketIo
{
	class Program
	{
		static void Main(string[] args)
		{
            log4net.Config.XmlConfigurator.Configure();

			Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=============================");
            Console.WriteLine("Socket.IO Proxy for KissCaller");
            Console.WriteLine();
			Console.WriteLine("To Exit  Console: 'q'");
			Console.WriteLine("To Clear Console: 'c'");
			Console.WriteLine("=============================");
			Console.WriteLine("");
			Console.ResetColor();

			SocketIoProxy tClient = new SocketIoProxy();
            if (args.Length > 0)
                tClient.Start(args[0]);
            else
                tClient.Start(ConfigurationManager.AppSettings["NodeUrl"]);

			bool run = true;
			while (run)
			{
				string line = Console.ReadLine();
				if (!string.IsNullOrWhiteSpace(line))
				{
					char key = line.FirstOrDefault();
					switch (key)
					{
						case 'c':
						case 'C':
							Console.Clear();
							break;

						case 'q':
						case 'Q':
							run = false;
							break;
						
						default:
							break;
					}
				}
			}
			tClient.Close();
		}
	}
}
