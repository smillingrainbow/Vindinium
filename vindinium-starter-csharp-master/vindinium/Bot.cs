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

        // Liste des positions des différentes bières/mines/ennemis
        private List<Pos> bieresDispo = new List<Pos>();
        private List<Pos> minesDispo  = new List<Pos>(); 
        private List<Pos> ennemisDispo = new List<Pos>();

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

            // Ouvre une page web pour suivre le jeu
            // Asynchrone
            if (serverStuff.errored == false)
            {
                new Thread(delegate()
                {
                    System.Diagnostics.Process.Start(serverStuff.viewURL);
                }).Start();
            }

            // La variable deplacement correspond au deplacement de notre hero durant notre tour
            string deplacement = Direction.Stay;
            // Suite Deplacement est la liste des déplacement d'un hero vers son objectif
            List<string> SuiteDeplacement = new List<string>();

            // Si il n'y a pas d'ennemis autour de notre hero
            if(EnnemisACoté() == null){
                ContinuFuir = false;
                if(Continu == false){
                    // Si la vie du hero est supérieure ou égale à 40, on va chercher la mine la plus proche
                    if (serverStuff.myHero.life >= 40)
                  {
                        initMines();
                        SuiteDeplacement = AStar(serverStuff.myHero.pos, minesDispo[0]);
                        deplacement = SuiteDeplacement[0];
                    }
                    // Si la vie est inférieure à 40, on va se bourrer la gueule
                    else
                   {
                        initBieres();
                        SuiteDeplacement = AStar(serverStuff.myHero.pos, bieresDispo[0]);
                        deplacement = SuiteDeplacement[0];
                    }
               }else{
                       SuiteDeplacement.RemoveAt(0);
                       deplacement = SuiteDeplacement[0];   
               }
            }
           else{
                // Si la vie du héro est supérieur ou égale à 80, on va taper l'adversaire
                if(serverStuff.myHero.life >= 80){
                        SuiteDeplacement = AStar(serverStuff.myHero.pos, EnnemisACoté());
                        deplacement = SuiteDeplacement[0];
                }
                // Sinon, on fuit vers la bière la plus proche
                else{
                    if(!ContinuFuir){
                        initBieres();
                        SuiteDeplacement = AStar(serverStuff.myHero.pos, bieresDispo[0]);
                        deplacement = SuiteDeplacement[0];
                    }
                    else
                    {
                        SuiteDeplacement.RemoveAt(0);
                        deplacement = SuiteDeplacement[0]; 
                    }
                }
               
            }
           
            // On bouge le héro
            Continu = true;
            ContinuFuir = true;
            serverStuff.moveHero(deplacement);

            if (serverStuff.errored)
            {
                Console.Out.WriteLine("error: " + serverStuff.errorText);
            }

            Console.Out.WriteLine("Poly_morphisme a fini");
        }


        // Retourne l'ennemis à coté si il existe
        public Pos EnnemisACoté()
        {
            Console.Out.WriteLine("ID : "+serverStuff.myHero.id);
            for(int i = 1 ; i < 5 ; i++){
                Console.Out.WriteLine(i);
                Console.Out.WriteLine(Math.Abs(serverStuff.myHero.pos.x - serverStuff.heroes[i - 1].pos.x + serverStuff.myHero.pos.y - serverStuff.heroes[i - 1].pos.y));
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
            int[][][] tableParent = new int[100][][];
            int[][][] tableDistance = new int[100][][] ;
           
            for (int i = 0; i < serverStuff.board.Length; i++)
            {
                tableDistance[i] = new int[100][];
                tableParent[i] = new int[100][];

                for (int j = 0; j < serverStuff.board.Length +2 ; j++)
                {
                    tableDistance[i][j] = new int[2];
                    tableParent[i][j] = new int[2];

                    for (int k = 0; k < 2; k++)
                    {
                        tableDistance[i][j][k] = 9999;
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
                Console.Out.WriteLine("Taille jeux :" +serverStuff.board.Length);
                Console.Out.WriteLine("Taille Tableau :" + listeFerme[(listeFerme.Count) - 1].x + "Et " + listeFerme[(listeFerme.Count) - 1].y);
                // On regarde si on à des arbre
                for (int X = -1; X < 2; X++)
                {
                    for (int Y = -1; Y < 2; Y++)
                    {
                        if ((X + Y < 2) && (Y + X != 0) && (X + Y > -2))
                        {

                            // On vérifie qu'il existe un arbre à côté de notre héro
                            if ((listeFerme[(listeFerme.Count) - 1].x + X + 1 != serverStuff.board.Length) || (listeFerme[(listeFerme.Count) - 1].x + X - 1 != 0) ||
                                (listeFerme[(listeFerme.Count) - 1].y + Y + 1 != serverStuff.board.Length) || (listeFerme[(listeFerme.Count) - 1].y + Y - 1 != 0) ||
                                (serverStuff.board[listeFerme[(listeFerme.Count) - 1].x + X][listeFerme[(listeFerme.Count) - 1].y + Y] != Tile.IMPASSABLE_WOOD))
                            {
                                Pos posIntermediaire = new Pos();
                                posIntermediaire.x = listeFerme[(listeFerme.Count) - 1].x + X;
                                posIntermediaire.y = listeFerme[(listeFerme.Count) - 1].y + Y;

                                // Si il n'existe pas dans la liste, on va le faire
                                if (!existeDansListe(posIntermediaire, listeOuverte, listeFerme))
                                {
                                    listeOuverte.Add(posIntermediaire);
                                    // On va incrémenter la valeur réelle de notre distance par rapport à celle de notre héro
                                    tableDistance[posIntermediaire.x][posIntermediaire.y][0] = tableDistance[listeFerme[(listeFerme.Count) - 1].x][listeFerme[(listeFerme.Count) - 1].y][0] + 1;
                                    // On rajoute 1 à la valeur de distance concernant notre objet par rapport à la destination
                                    tableDistance[posIntermediaire.x][posIntermediaire.y][1] = (int)getDistanceReelle(posIntermediaire, posDestination);

                                    // On initialise la table parent entrant les coordonées de notre héro en x et y
                                    tableParent[posIntermediaire.x][posIntermediaire.y][0] = listeFerme[(listeFerme.Count) - 1].x;
                                    tableParent[posIntermediaire.x][posIntermediaire.y][0] = listeFerme[(listeFerme.Count) - 1].y;
                                }
                                else
                                {
                                    // Si, on rencontre un arbre, on vérifie s'il la distance est meilleur en passant par un point précis, dans ce cas on réinitialise la table distance
                                    if (tableDistance[posIntermediaire.x][posIntermediaire.y][0] >= tableDistance[listeFerme[(listeFerme.Count) - 1].x][listeFerme[(listeFerme.Count) - 1].y][0])
                                    {
                                        tableDistance[posIntermediaire.x][posIntermediaire.y][0] = tableDistance[listeFerme[(listeFerme.Count) - 1].x][listeFerme[(listeFerme.Count) - 1].y][0];
                                        tableParent[posIntermediaire.x][posIntermediaire.y][0] = listeFerme[(listeFerme.Count) - 1].x;
                                        tableParent[posIntermediaire.x][posIntermediaire.y][0] = listeFerme[(listeFerme.Count) - 1].y;
                                    }
                                }
                            }
                        }
                    }
                }

                if ( (listeFerme[(listeFerme.Count) - 1].x == posDestination.x) && (listeFerme[(listeFerme.Count) - 1].y == posDestination.y))
                    continu = false;
            }
            Console.Out.WriteLine("c'est passé YOUHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHh");

            bool finish = false;
            List<string> deplacement = new List<string>();
            Pos posEnfant = posDestination; 
            Pos posParent = new Pos();

            // On va remplir la liste de déplacement
            while (finish == false)
            {
                posParent.x = tableParent[posEnfant.x][posEnfant.y][0];
                posParent.y = tableParent[posEnfant.x][posEnfant.y][1];

                if (posParent.x == posEnfant.x && posParent.y > posEnfant.y)
                { // parent au dessus de l'enfant --> déplacement vers le bas
                    deplacement.Add("Direction.South");
                }
                else if (posParent.x == posEnfant.x && posParent.y < posEnfant.y)
                { // parent en dessous de l'enfant --> déplacement vers la haut
                    deplacement.Add("Direction.North");
                }
                else if (posParent.x < posEnfant.x && posParent.y == posEnfant.y)
                { // parent à gauche de l'enfant --> déplacement vers la droite
                    deplacement.Add("Direction.East");
                }
                else{ // parent à droite de l'enfant --> déplacement vers la gauche
                    deplacement.Add("Direction.West");
                }

                if (posParent.x == posHero.x && posParent.y == posHero.y)
                {
                    finish = true;
                }
                else
                {
                    posParent.x = posEnfant.x;
                    posParent.y = posEnfant.y;
                }
                //finish = true;
            }
            deplacement.Reverse();
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
            while ((i < listeOuverte.Count) && (found == false) && ((j < listeFerme.Count)))
            {
                if((listeFerme[j].x == point.x) && (listeFerme[j].y == point.y))
                    found = true;
              j++;
            }
            return found;
        }
        

        // On retourne le plus petit noeud dde la liste, c'est à dire le point dont la distance euclidienne est la plus petite par rapport à l'objectif 
        public int retournerPlusPetitNoeud(List<Pos> listeOuverte, int[][][] tableDistance)
        {
            int index = 0;
            int minF = Int32.MaxValue;

            for (int i = 0; i < listeOuverte.Count; i++)
            {
                if(tableDistance[listeOuverte[i].x][listeOuverte[i].y][0] + tableDistance[listeOuverte[i].x][listeOuverte[i].y][1]  < minF){
                    minF = tableDistance[listeOuverte[i].x][listeOuverte[i].y][0] + tableDistance[listeOuverte[i].x][listeOuverte[i].y][1];
                    index = i;
                }
            }
            return index;
        }

        // Fonction qui fait la distance euclidienne
        public double getDistanceReelle(Pos posCourante, Pos arrive)
        {
            return arrive.x - posCourante.x + arrive.y - posCourante.y;
        }

         // Fonction qui initialise une liste des mines triées  par rapport à la distance à notre héro
         public void initMines(){
             this.minesDispo.Clear();
             this.nbMinesDispo = 0;
             Pos objetTrouve = new Pos();

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
                             this.nbMinesDispo++;
                             objetTrouve.x = i;
                             objetTrouve.y = j;
                             minesDispo.Add(objetTrouve);
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
                         if (serverStuff.board[i][j] == Tile.GOLD_MINE_NEUTRAL ||
                             serverStuff.board[i][j] == Tile.GOLD_MINE_1 ||
                             serverStuff.board[i][j] == Tile.GOLD_MINE_3 ||
                             serverStuff.board[i][j] == Tile.GOLD_MINE_4 )
                         {
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
                             this.nbMinesDispo++;
                             objetTrouve.x = i;
                             objetTrouve.y = j;
                             minesDispo.Add(objetTrouve);
                         }
                     }
                 }
             }
              trieList(minesDispo, nbMinesDispo);
         }

         // Fonction qui initialise une liste des bières triées  par rapport à la distance à notre héro
         public void initBieres()
         {
             this.nbBiereDispo = 0;
             this.bieresDispo.Clear();
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
             this.nbBiereDispo = 0;
             this.bieresDispo.Clear();
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
                          (Math.Abs(liste[j].x - serverStuff.myHero.pos.x) + (Math.Abs(liste[j].y - -serverStuff.myHero.pos.y)))
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
    }
}
