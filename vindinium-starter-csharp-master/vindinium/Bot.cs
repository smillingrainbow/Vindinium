using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace vindinium
{
	class Bot
	{
		private ServerStuff serverStuff;

		/**
		 * Constructeur
		 **/
		public Bot (ServerStuff serveurStuff)
		{
			this.serverStuff = serveurStuff;
		}


		/**
		 * Démarre le jeu
		 **/
		public void run(){

			Console.Out.WriteLine("Poly_morphisme démarre");

			serverStuff.createGame();

			// Ouvre une page web pour suivre le jeu
			// Asynchrone
			if (serverStuff.errored == false)
			{
				new Thread(delegate()
					{
						System.Diagnostics.Process.Start(serverStuff.viewURL);
					}).Start();
			}
			Random random = new Random ();
			while (serverStuff.finished == false && serverStuff.errored == false)
			{
				switch(random.Next(0, 6))
				{
				case 0:
					serverStuff.moveHero(Direction.East);
					break;
				case 1:
					serverStuff.moveHero(Direction.North);
					break;
				case 2:
					serverStuff.moveHero(Direction.South);
					break;
				case 3:
					serverStuff.moveHero(Direction.Stay);
					break;
				case 4:
					serverStuff.moveHero(Direction.West);
					break;
				}

				Console.Out.WriteLine("completed turn " + serverStuff.currentTurn);
			}

				if (serverStuff.errored)
			{
					Console.Out.WriteLine("error: " + serverStuff.errorText);
			}

			Console.Out.WriteLine("Poly_morphisme a fini");
		}


	}
}

