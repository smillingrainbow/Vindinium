using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;

namespace vindinium
{
    class Bot
    {
        private ServerStuff serverStuff;

        // Variables qui détermine le nb de bière et de mines sur la cartes
        private int nbBiereDispo;
        private int nbMinesDispo;
        private int nbBiereTotal;
        private int nbMinesTotal;

        // Liste des positions des différentes bières/mines/ennemis
        private static List<Pos> bieresDispo = new List<Pos>();
        private static List<Pos> minesDispo = new List<Pos>(); 
        private static List<Pos> ennemisDispo = new List<Pos>();

        /**
         * Constructeur
         **/
        public Bot(ServerStuff serveurStuff)
        {
            this.serverStuff = serveurStuff;
        }

        /**
         * Démarre le jeu
         **/
        public void run()
        {

            Console.Out.WriteLine("Poly_morphisme démarre");
            bool Continu = false;
            bool ContinuFuir = false;
            serverStuff.createGame();

            //initMinesTotal();
            //initBiereTotal();
            // Ouvre une page web pour suivre le jeu
            // Asynchrone
            if (serverStuff.errored == false)
            {
                new Thread(delegate()
                {
                    System.Diagnostics.Process.Start(serverStuff.viewURL);
                }).Start();
            }

            while (serverStuff.finished == false && serverStuff.errored == false)
            {
                // la variable deplacement correspond au deplacement de notre hero durant notre tour
                string deplacement = Direction.Stay;
                // suite deplacement est la liste des déplacement d'un hero vers son objectif
                List<string> suitedeplacement = new List<string>();

                // si il n'y a pas d'ennemis autour de notre hero
                if (EnnemisACoté() == null)
                {
                    Console.Out.WriteLine("personne à coté");
                    ContinuFuir = false;
                    if (Continu == false)
                    {
                        // si la vie du hero est supérieure ou égale à 40, on va chercher la mine la plus proche
                        if (serverStuff.myHero.life >= 40)
                        {
                            InitMines();
                            suitedeplacement = AStar(serverStuff.myHero.pos, minesDispo[0]);
                            deplacement = suitedeplacement[0];
                            suitedeplacement.RemoveAt(0);
                        }
                        // si la vie est inférieure à 40, on va se bourrer la gueule
                        else
                        {
                            initBieres();
                            suitedeplacement = AStar(serverStuff.myHero.pos, bieresDispo[0]);
                            deplacement = suitedeplacement[0];
                            suitedeplacement.RemoveAt(0);
                        }
                        Continu = true;
                    }
                    else
                    {
                        if (suitedeplacement.Count != 0)
                        {
                            deplacement = suitedeplacement[0];
                            suitedeplacement.RemoveAt(0);
                        }
                      
                    }
                    if (suitedeplacement.Count == 0)
                        Continu = false;
                }
                else
                {
                    // si la vie du héro est supérieur ou égale à 80, on va taper l'adversaire
                    if (serverStuff.myHero.life >= 80)
                    {
                        suitedeplacement = AStar(serverStuff.myHero.pos, EnnemisACoté());
                        deplacement = suitedeplacement[0];
                        suitedeplacement.RemoveAt(0);
                        ContinuFuir = false;
                    }
                    // sinon, on fuit vers la bière la plus proche
                    else
                    {
                        if (!ContinuFuir)
                        {
                            initBieres();
                            suitedeplacement = AStar(serverStuff.myHero.pos, bieresDispo[0]);
                            deplacement = suitedeplacement[0];
                            suitedeplacement.RemoveAt(0);
                            ContinuFuir = true;
                        }
                        else
                        {
                            if (suitedeplacement.Count != 0)
                            {
                                deplacement = suitedeplacement[0];
                                suitedeplacement.RemoveAt(0);
                            }
                        }
                    }
                    if (suitedeplacement.Count == 0)
                        ContinuFuir = false;
                }

               //  on bouge le héro
 
                //Console.Out.WriteLine(deplacement);
                serverStuff.moveHero(deplacement);
            

                if (serverStuff.errored)
                {
                    Console.Out.WriteLine("error: " + serverStuff.errorText);
                }
            }
            Console.Out.WriteLine("Poly_morphisme a fini");
        }


        // Retourne l'ennemis à coté si il existe
        public Pos EnnemisACoté()
        {
            for(int i = 1 ; i < 5 ; i++){
                //Console.Out.WriteLine(i);
                //Console.Out.WriteLine(Math.Abs(serverStuff.myHero.pos.x - serverStuff.heroes[i - 1].pos.x + serverStuff.myHero.pos.y - serverStuff.heroes[i - 1].pos.y));
                if ((i != serverStuff.myHero.id) && ((Math.Abs(serverStuff.myHero.pos.x - serverStuff.heroes[i-1].pos.x + serverStuff.myHero.pos.y - serverStuff.heroes[i-1].pos.y)) <= 2))
                {
                    return serverStuff.heroes[i].pos;
                }
             }  
             return null;
        }


        public List<string> AStar(Pos posHero, Pos posDestination)
        {
            List<Pos> listeOuverte = new List<Pos>();
            List<Pos> listeFerme = new List<Pos>();
            Pos posEnCours = new Pos();
            int index;
            bool continu = true;
            int[][][] tableParent = new int[serverStuff.board.Length][][];
            int[][][] tableDistance = new int[serverStuff.board.Length][][];

            for (int i = 0; i < serverStuff.board.Length; i++)
            {
                tableDistance[i] = new int[serverStuff.board.Length][];
                tableParent[i] = new int[serverStuff.board.Length][];

                for (int j = 0; j < serverStuff.board.Length; j++)
                {
                    tableDistance[i][j] = new int[2];
                    tableParent[i][j] = new int[2];

                    for (int k = 0; k < 2; k++)
                    {
                        tableDistance[i][j][k] = 0;
                    }
                }
            }
                    
            List<int> distanceEstimeNoeudDestination = new List<int>();
            List<int> distanceReelleNoeudDestination = new List<int>();
            List<int> SommeDistanceEstimeReelle = new List<int>();

            listeOuverte.Add(posHero);
            tableDistance[posHero.x][posHero.y][0] = 0;
            tableDistance[posHero.x][posHero.y][1] = (int)getDistanceReelle(posHero,posDestination);


            while (continu == true)
            {
                index = retournerPlusPetitNoeud(listeOuverte, tableDistance);
                listeFerme.Add(listeOuverte[index]);
                listeOuverte.RemoveAt(index);
               
               Console.Out.WriteLine("Position Hero  initial : x= "+ posHero.x + "e et y = "+ posHero.y);
               Console.Out.WriteLine("Position Destination initial : x= "+ posDestination.x + "e et y = "+ posDestination.y);
               Console.Out.WriteLine("-----------------------");
                //Console.Out.WriteLine("Taille jeux :" +serverStuff.board.Length);
                //Console.Out.WriteLine("Taille Tableau :" + listeFerme[(listeFerme.Count) - 1].x + "Et " + listeFerme[(listeFerme.Count) - 1].y);
                // On regarde si on à des arbre
                for (int X = -1; X < 2; X++)
                {
                    for (int Y = -1; Y < 2; Y++)
                    {
                        if ((X + Y < 2) && (Y + X != 0) && (X + Y > -2))
                        {
                            // On vérifie qu'il existe un arbre à côté de notre héro
                            if (((listeFerme[(listeFerme.Count) - 1].x + X  < serverStuff.board.Length) && (listeFerme[(listeFerme.Count) - 1].x + X  >= 0) &&
                                (listeFerme[(listeFerme.Count) - 1].y + Y < serverStuff.board.Length) && (listeFerme[(listeFerme.Count) - 1].y + Y >= 0)))
                            {
                                 if(serverStuff.board[listeFerme[(listeFerme.Count) - 1].x + X][listeFerme[(listeFerme.Count) - 1].y + Y] != Tile.IMPASSABLE_WOOD)
                                {
                                    Pos posIntermediaire = new Pos();

                                    posIntermediaire.x = listeFerme[(listeFerme.Count) - 1].x + X;
                                    posIntermediaire.y = listeFerme[(listeFerme.Count) - 1].y + Y;

                                     Console.Out.WriteLine("Traitement de la position : x = "+ posIntermediaire.x + " et y = " + posIntermediaire.y);
                                    // Si il n'existe pas dans la liste, on va le faire
                                    if (!existeDansListe(posIntermediaire, listeOuverte, listeFerme))
                                    {
                                       // Console.Out.WriteLine("Je RENTRE DEDANS");
                                        listeOuverte.Add(posIntermediaire);
                                        // On va incrémenter la valeur réelle de notre distance par rapport à celle de notre héro
                                        tableDistance[posIntermediaire.x][posIntermediaire.y][0] = tableDistance[listeFerme[(listeFerme.Count) - 1].x][listeFerme[(listeFerme.Count) - 1].y][0] + 1;
                                        // On rajOute 1 à la valeur de distance concernant notre objet par rapport à la destination
                                        tableDistance[posIntermediaire.x][posIntermediaire.y][1] = (int)getDistanceReelle(posIntermediaire, posDestination);

                                        // On initialise la table parent entrant les coordonées de notre héro en x et y
                                        tableParent[posIntermediaire.x][posIntermediaire.y][0] = listeFerme[(listeFerme.Count) - 1].x;
                                        tableParent[posIntermediaire.x][posIntermediaire.y][1] = listeFerme[(listeFerme.Count) - 1].y;
                                    }
                                    else
                                    {
                                        // Si, on rencontre un arbre, on vérifie s'il la distance est meilleur en passant par un point précis, dans ce cas on réinitialise la table distance
                                        if (tableDistance[posIntermediaire.x][posIntermediaire.y][0] >= tableDistance[listeFerme[(listeFerme.Count) - 1].x][listeFerme[(listeFerme.Count) - 1].y][0])
                                        {
                                            tableDistance[posIntermediaire.x][posIntermediaire.y][0] = tableDistance[listeFerme[(listeFerme.Count) - 1].x][listeFerme[(listeFerme.Count) - 1].y][0];
                                            tableParent[posIntermediaire.x][posIntermediaire.x][0] = listeFerme[(listeFerme.Count) - 1].x;
                                            tableParent[posIntermediaire.x][posIntermediaire.y][0] = listeFerme[(listeFerme.Count) - 1].y;
                                        }
                                    }
                                }
                            }
                            if (((listeFerme[(listeFerme.Count) - 1].x == posDestination.x) && (listeFerme[(listeFerme.Count) - 1].y == posDestination.y)) || (listeOuverte.Count == 0))
                                continu = false;
                        }
                  
                    }
                
                }

            }

          
            //afficheTable(tableParent,0);
            //afficheTable(tableParent, 1);

            listeFerme.Clear();
            listeOuverte.Clear();
            bool finish = false;
            
            List<string> deplacement = new List<string>();
            Pos posEnfant = posDestination;
            posEnfant.x = posDestination.x;
            posEnfant.y = posDestination.y;
            Pos posParent = new Pos();
            Console.Out.WriteLine("En hero x :" + posHero.x);
            Console.Out.WriteLine("En hero y :" + posHero.y);
            Console.Out.WriteLine("-------------");
            Console.Out.WriteLine("En destination x :" + posDestination.x);
            Console.Out.WriteLine("En destination y :" + posDestination.y);
            Console.Out.WriteLine("-------------");
            Console.Out.WriteLine("En x :" + posEnfant.x);
            Console.Out.WriteLine("En y :" + posEnfant.y);
            Console.Out.WriteLine("-------------");
            Console.Out.WriteLine("En x :" + tableParent[posEnfant.x][posEnfant.y][0]);
            Console.Out.WriteLine("En y :" + tableParent[posEnfant.x][posEnfant.y][1]);
            // On va remplir la liste de déplacement
            while (finish == false)
            {
                posParent.x = tableParent[posEnfant.x][posEnfant.y][0];
                posParent.y = tableParent[posEnfant.x][posEnfant.y][1];

                if (posParent.y > posEnfant.y)
                { // parent au dessus de l'enfant --> déplacement vers le bas
                    deplacement.Add(Direction.East);
                   // Console.Out.WriteLine("East");
                }
                else if (posParent.y < posEnfant.y)
                { // parent en dessous de l'enfant --> déplacement vers la haut
                    deplacement.Add(Direction.West);
                   // Console.Out.WriteLine("West");
                }
                else if (posParent.x < posEnfant.x)
                { // parent à gauche de l'enfant --> déplacement vers la droite
                    deplacement.Add(Direction.South);
                    //Console.Out.WriteLine("Direction.South");
                }
                else if (posParent.x > posEnfant.x)
                { // parent à droite de l'enfant --> déplacement vers la gauche

                    deplacement.Add(Direction.North);
                    //Console.Out.WriteLine("North");    
      
                }

                if (posParent.x == posHero.x && posParent.y == posHero.y)
                {
                    finish = true;

                }
                else
                {
                    posEnfant.x = posParent.x;
                    posEnfant.y = posParent.y;
                }
            }
            deplacement.Reverse();
            int count = deplacement.Count;
            for(int i =0; i<count ; i++)
            Console.Out.WriteLine("Déplacement à faire en "+ deplacement[i]);
            return deplacement;
            
        }

        // Vérifie si un élément de la carte à été traité par l'algorithme A*
        public bool existeDansListe(Pos point, List<Pos> listeOuverte, List<Pos> listeFerme)
        {
            bool found = false;
            int i = 0;
            int j =0;

            // On vérifie dans la liste ouverte (position à traiter)
            while ( (i < listeOuverte.Count) && (found == false) ) 
            {
                if ( ((listeOuverte[i].x == point.x) && (listeOuverte[i].y == point.y)) 
                    )
                    found = true;
                i++;
            }
            // On vérifie dans la liste fermée (position traitée)
            while ((found == false) && ((j < listeFerme.Count)))
            {
                if((listeFerme[j].x == point.x) && (listeFerme[j].y == point.y))
                    found = true;
              j++;
            }
            Console.Out.WriteLine(" Resultat : "+ found);
            return found;
        }
        

        // On retourne le plus petit noeud dde la liste, c'est à dire le point dont la distance euclidienne est la plus petite par rapport à l'objectif 
        public int retournerPlusPetitNoeud(List<Pos> listeOuverte, int[][][] tableDistance)
        {
            int index = 0;
            int minF = Int32.MaxValue;

            for (int i = 0; i < listeOuverte.Count; i++)
            {
                if(tableDistance[listeOuverte[i].x][listeOuverte[i].y][0] + tableDistance[listeOuverte[i].x][listeOuverte[i].y][1]  < minF)
                {
                    minF = tableDistance[listeOuverte[i].x][listeOuverte[i].y][0] + tableDistance[listeOuverte[i].x][listeOuverte[i].y][1];
                    index = i;
                    //Console.Out.WriteLine("L'index : "+ i +" de valeur ["+ listeOuverte[i].x + " , "+listeOuverte[i].y +"]");
                }
            }
            return index;       
        }

        // Fonction qui fait la distance euclidienne
        public double getDistanceReelle(Pos posCourante, Pos arrive)
        {
            return Math.Abs(arrive.x - posCourante.x)+ Math.Abs(arrive.y - posCourante.y);
        }

         // Fonction qui initialise une liste des mines triées  par rapport à la distance à notre héro
         public void InitMines(){
             minesDispo.Clear();
             nbMinesDispo = 0;
             Pos objetTrouve;
            // int k = 0;
             if (serverStuff.myHero.id == 1)
             {
                 for (int i = 0; i < serverStuff.board.Length; i++)
                 {
                     for (int j = 0; j < serverStuff.board.Length; j++)
                     {
                         if (serverStuff.board[i][j] == Tile.GOLD_MINE_NEUTRAL ||
                             serverStuff.board[i][j] == Tile.GOLD_MINE_2 ||
                             serverStuff.board[i][j] == Tile.GOLD_MINE_3 ||
                             serverStuff.board[i][j] == Tile.GOLD_MINE_4 )
                         {
                             objetTrouve = new Pos();
                             nbMinesDispo++;
                             objetTrouve.x = i;
                             objetTrouve.y = j;
                             minesDispo.Add(objetTrouve);
                          }
                     }
                 }
                 afficheListe(minesDispo, "mine");
             }

              if (serverStuff.myHero.id == 2)
             {
                 for (int i = 0; i < serverStuff.board.Length; i++)
                 {
                     for (int j = 0; j < serverStuff.board.Length; j++)
                     {
                         if (serverStuff.board[i][j] == Tile.GOLD_MINE_NEUTRAL ||
                             serverStuff.board[i][j] == Tile.GOLD_MINE_1 ||
                             serverStuff.board[i][j] == Tile.GOLD_MINE_3 ||
                             serverStuff.board[i][j] == Tile.GOLD_MINE_4 )
                         {
                             objetTrouve = new Pos();
                             this.nbMinesDispo++;
                             objetTrouve.x = i;
                             objetTrouve.y = j;
                             minesDispo.Add(objetTrouve);
                         }
                     }
                 }
             }

              if (serverStuff.myHero.id == 3)
             {
                 for (int i = 0; i < serverStuff.board.Length; i++)
                 {
                     for (int j = 0; j < serverStuff.board.Length; j++)
                     {
                         if (serverStuff.board[i][j] == Tile.GOLD_MINE_NEUTRAL ||
                             serverStuff.board[i][j] == Tile.GOLD_MINE_1 ||
                             serverStuff.board[i][j] == Tile.GOLD_MINE_2 ||
                             serverStuff.board[i][j] == Tile.GOLD_MINE_4 )
                         {
                             objetTrouve = new Pos();
                             this.nbMinesDispo++;
                             objetTrouve.x = i;
                             objetTrouve.y = j;
                             minesDispo.Add(objetTrouve);
                         }
                     }
                 }
             }

              if (serverStuff.myHero.id == 4)
             {
                 for (int i = 0; i < serverStuff.board.Length; i++)
                 {
                     for (int j = 0; j < serverStuff.board.Length; j++)
                     {
                         if (serverStuff.board[i][j] == Tile.GOLD_MINE_NEUTRAL ||
                             serverStuff.board[i][j] == Tile.GOLD_MINE_1 ||
                             serverStuff.board[i][j] == Tile.GOLD_MINE_2 ||
                             serverStuff.board[i][j] == Tile.GOLD_MINE_3 )
                         {
                             objetTrouve = new Pos();
                             this.nbMinesDispo++;
                             objetTrouve.x = i;
                             objetTrouve.y = j;
                             minesDispo.Add(objetTrouve);
                         }
                     }
                 }
             }
             //afficheListe(minesDispo, "mine 2");
              trieList(minesDispo, nbMinesDispo);
         }

         // Fonction qui initialise une liste des bières triées  par rapport à la distance à notre héro
         public void initBieres()
         {
             nbBiereDispo = 0;
             bieresDispo.Clear();
             Pos objetTrouve;

             for (int i = 0; i < serverStuff.board.Length; i++)
             {
                 for (int j = 0; j < serverStuff.board.Length; j++)
                 {
                     if (serverStuff.board[i][j] == Tile.TAVERN)
                     {
                         objetTrouve = new Pos();
                         nbBiereDispo++;
                         objetTrouve.x = i;
                         objetTrouve.y = i;
                         bieresDispo.Add(objetTrouve);
                     }
                 }
             }
             trieList(bieresDispo, nbBiereDispo);
         }

         // Fonction qui initialise une liste des ennemis triés par rapport à la distance à notre héro
         public void initEnnemis()
         {
              Pos objetTrouve = new Pos();

             if (serverStuff.myHero.id == 1)
             {
                 for (int i = 0; i < serverStuff.board.Length; i++)
                 {
                     for (int j = 0; j < serverStuff.board.Length; j++)
                     {
                         if (serverStuff.board[i][j] == Tile.HERO_2 ||
                             serverStuff.board[i][j] == Tile.HERO_3 ||
                             serverStuff.board[i][j] == Tile.HERO_4)
                         {
                             objetTrouve = new Pos();
                             objetTrouve.x = i;
                             objetTrouve.y = j;
                             ennemisDispo.Add(objetTrouve);
                         }
                     }
                 }
             }

             if (serverStuff.myHero.id == 2)
             {
                 for (int i = 0; i < serverStuff.board.Length; i++)
                 {
                     for (int j = 0; j < serverStuff.board.Length; j++)
                     {
                         if (serverStuff.board[i][j] == Tile.HERO_1 ||
                             serverStuff.board[i][j] == Tile.HERO_3 ||
                             serverStuff.board[i][j] == Tile.HERO_4)
                         {
                             objetTrouve = new Pos();
                             objetTrouve.x = i;
                             objetTrouve.y = j;
                             ennemisDispo.Add(objetTrouve);
                         }
                     }
                 }
             }

             if (serverStuff.myHero.id == 3)
             {
                 for (int i = 0; i < serverStuff.board.Length; i++)
                 {
                     for (int j = 0; j < serverStuff.board.Length; j++)
                     {
                         if (serverStuff.board[i][j] == Tile.HERO_1 ||
                             serverStuff.board[i][j] == Tile.HERO_2 ||
                             serverStuff.board[i][j] == Tile.HERO_4)
                         {
                             objetTrouve = new Pos();
                             objetTrouve.x = i;
                             objetTrouve.y = j;
                             ennemisDispo.Add(objetTrouve);
                         }
                     }
                 }
             }

             if (serverStuff.myHero.id == 4)
             {
                 for (int i = 0; i < serverStuff.board.Length; i++)
                 {
                     for (int j = 0; j < serverStuff.board.Length; j++)
                     {
                         if (serverStuff.board[i][j] == Tile.HERO_1 ||
                             serverStuff.board[i][j] == Tile.HERO_2 ||
                             serverStuff.board[i][j] == Tile.HERO_3)
                         {
                             objetTrouve = new Pos();
                             objetTrouve.x = i;
                             objetTrouve.y = j;
                             minesDispo.Add(objetTrouve);
                         }
                     }
                 }
             }
             trieList(ennemisDispo, 3);
         }

        // Calcul le nombre total de bière sur la carte
         public void nbBieres()
         {
             nbBiereDispo = 0;
             bieresDispo.Clear();
             Pos objetTrouve = new Pos();

             for (int i = 0; i < serverStuff.board.Length; i++)
             {
                 for (int j = 0; j < serverStuff.board.Length; j++)
                 {
                     if (serverStuff.board[i][j] == Tile.TAVERN)
                     {
                         nbBiereDispo++;
                         objetTrouve.x = i;
                         objetTrouve.y = i;
                         bieresDispo.Add(objetTrouve);
                     }
                 }
             }
             trieList(bieresDispo, nbBiereDispo);
         }

        // Fonction qui permet de trier une liste selon la position par rapport au héro
         public void trieList(List<Pos> liste, int nombreObjet)
         {
             Pos objetTemp = new Pos();
             objetTemp.x = 9999;
             objetTemp.y = 9999;

             for (int i = 0; i < nombreObjet; i++)
             {
                 for (int j = 0; j < nombreObjet; j++)
                 {
                     if( (i != j) &&
                         ((Math.Abs(liste[i].x - serverStuff.myHero.pos.x) + (Math.Abs(liste[i].y - serverStuff.myHero.pos.y))) <
                          (Math.Abs(liste[j].x - serverStuff.myHero.pos.x) + (Math.Abs(liste[j].y - serverStuff.myHero.pos.y)))
                          )){
                         // objetTemp = liste[i];
                         objetTemp.x = liste[i].x;
                         objetTemp.y = liste[i].y;
                         // liste[i] = liste[j];
                         liste[i].x = liste[j].x;
                         liste[i].y = liste[j].y;
                         // liste[j] = objetTemp;
                         liste[j].x = objetTemp.x;
                         liste[j].y = objetTemp.y;
                     }
                 }
             }
         }

         public void afficheListe(List<Pos> liste, string nom)
         {
             for(int i = 0 ; i < liste.Count ; i ++)
             {
                 Console.Out.WriteLine("Pour la "+ nom + " numéro "+ i +"elle est à la position "+ liste[i].x +"et "+liste[i].y);
             }
         }

         public void afficheTable(int[][][] table,int k)
         {
             for (int i = 0; i < serverStuff.board.Count(); i++) {
                 for (int j = 0; j < serverStuff.board.Count(); j++)
                 {
                     Console.Out.Write(table[i][j][k] + " | ");
                 }
                 Console.Out.WriteLine(" |||| ");
             }
             Console.Out.WriteLine("\n");
         }
         //public void initMinesTotal()
         //{
         //     nbMinesTotal = 0;
         //     for (int i = 0; i < serverStuff.board.Length; i++)
         //     {
         //         for (int j = 0; j < serverStuff.board.Length; j++)
         //         {
         //             if (serverStuff.board[i][j] == Tile.GOLD_MINE_1 ||
         //                 serverStuff.board[i][j] == Tile.GOLD_MINE_2 ||
         //                 serverStuff.board[i][j] == Tile.GOLD_MINE_3 ||
         //                 serverStuff.board[i][j] == Tile.GOLD_MINE_4 ||
         //                 serverStuff.board[i][j] == Tile.GOLD_MINE_NEUTRAL)

         //                 nbMinesTotal++;
         //         }
         //     }

         //     for (int i = 0; i < nbMinesTotal+1; i++)
         //     {
         //         minesDispo[i] = new Pos();
         //     }
         //}

         //public void initBiereTotal()
         //{
         //    nbBiereTotal = 0;
         //    for (int i = 0; i < serverStuff.board.Length; i++)
         //    {
         //        for (int j = 0; j < serverStuff.board.Length; j++)
         //        {
         //            if (serverStuff.board[i][j] == Tile.TAVERN)
         //                nbBiereTotal++;
         //        }
         //    }

         //    for (int i = 0; i < nbMinesTotal; i++)
         //    {
         //        bieresDispo[i] = new Pos();
         //    }
         //}

    }
}
