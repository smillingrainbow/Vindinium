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
        private int nbBiereDispo;
        private int nbMinesDispo;
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

            string deplacement = Direction.Stay;
            List<string> SuiteDeplacement = new List<String>();

            if(EnnemisACoté() == null){
                ContinuFuir = false;
               if(Continu == false){
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
                if(serverStuff.myHero.life >= 80){
                        SuiteDeplacement = AStar(serverStuff.myHero.pos, EnnemisACoté());
                        deplacement = SuiteDeplacement[0];
                }
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
           
               Continu = true;
               ContinuFuir = true;
              serverStuff.moveHero(deplacement);




            //Random random = new Random();
            //while (serverStuff.finished == false && serverStuff.errored == false)
            //{
            //    switch (random.Next(0, 6))
            //    {
            //        case 0:
            //            serverStuff.moveHero(Direction.East);
            //            break;
            //        case 1:
            //            serverStuff.moveHero(Direction.North);
            //            break;
            //        case 2:
            //            serverStuff.moveHero(Direction.South);
            //            break;
            //        case 3:
            //            serverStuff.moveHero(Direction.Stay);
            //            break;
            //        case 4:
            //            serverStuff.moveHero(Direction.West);
            //            break;
            //    }

            //    Console.Out.WriteLine("completed turn " + serverStuff.currentTurn);
            //}

            if (serverStuff.errored)
            {
                Console.Out.WriteLine("error: " + serverStuff.errorText);
            }

            Console.Out.WriteLine("Poly_morphisme a fini");
        }


        // Tu va retourner la position du méchant!

        public Pos EnnemisACoté()
        {
            
            Pos ennemis = new Pos();
            
                if (serverStuff.myHero.id == 1){
                   for (int i = -2; i < 3; i++)
                   {
                        for(int j = -2 ; j<3; j++){
                            if(((serverStuff.board[serverStuff.myHero.pos.x + i ][serverStuff.myHero.pos.y + j])== Tile.HERO_2) ||
                                ((serverStuff.board[serverStuff.myHero.pos.x + i ][serverStuff.myHero.pos.y + j])== Tile.HERO_3) ||
                                ((serverStuff.board[serverStuff.myHero.pos.x + i ][serverStuff.myHero.pos.y] + j)== Tile.HERO_4))
                                 // Ennemis = true;
                                ennemis.x = serverStuff.myHero.pos.x + i;
                                ennemis.y = serverStuff.myHero.pos.y + j;
                        }
                    }     
                }  

                if(serverStuff.myHero.id == 2){
                    for (int i = -2; i < 3; i++)
                   {
                        for(int j = -2 ; j<3; j++){
                            if(((serverStuff.board[serverStuff.myHero.pos.x + i ][serverStuff.myHero.pos.y + j])== Tile.HERO_1) ||
                                ((serverStuff.board[serverStuff.myHero.pos.x + i ][serverStuff.myHero.pos.y + j])== Tile.HERO_3) ||
                                ((serverStuff.board[serverStuff.myHero.pos.x + i ][serverStuff.myHero.pos.y] + j)== Tile.HERO_4))
                                  //Ennemis = true;
                            ennemis.x = serverStuff.myHero.pos.x + i;
                            ennemis.y = serverStuff.myHero.pos.y + j;
                        }
                    }     
                }  
 

                if(serverStuff.myHero.id == 3){
                   for (int i = -2; i < 3; i++)
                   {
                        for(int j = -2 ; j<3; j++){
                            if(((serverStuff.board[serverStuff.myHero.pos.x + i ][serverStuff.myHero.pos.y + j])== Tile.HERO_1) ||
                                ((serverStuff.board[serverStuff.myHero.pos.x + i ][serverStuff.myHero.pos.y + j])== Tile.HERO_2) ||
                                ((serverStuff.board[serverStuff.myHero.pos.x + i ][serverStuff.myHero.pos.y] + j)== Tile.HERO_4))
                                  //Ennemis = true;
                            ennemis.x = serverStuff.myHero.pos.x + i;
                            ennemis.y = serverStuff.myHero.pos.y + j;
                        }
                    }     
                }  


                if(serverStuff.myHero.id == 4){
                   for (int i = -2; i < 3; i++)
                   {
                        for(int j = -2 ; j<3; j++){
                            if(((serverStuff.board[serverStuff.myHero.pos.x + i ][serverStuff.myHero.pos.y + j])== Tile.HERO_1) ||
                                ((serverStuff.board[serverStuff.myHero.pos.x + i ][serverStuff.myHero.pos.y + j])== Tile.HERO_2) ||
                                ((serverStuff.board[serverStuff.myHero.pos.x + i ][serverStuff.myHero.pos.y] + j)== Tile.HERO_3))
                                 // Ennemis = true;
                            ennemis.x = serverStuff.myHero.pos.x + i;
                            ennemis.y = serverStuff.myHero.pos.y + j;
                        }
                    }     
                }  

            return ennemis;
        }


        public List<String> AStar(Pos posHero, Pos posDestination)
        {
            List<Pos> listeOuverte = new List<Pos>();
            List<Pos> listeFerme = new List<Pos>();
            Pos posEnCours = new Pos();
            int index;
            int[][][] tableParent = new int[serverStuff.board.Length][][];

            int[][][] tableDistance = new int[serverStuff.board.Length][][] ;
            for (int i = 0; i < serverStuff.board.Length; i++)
            {
                tableDistance[i] = new int[serverStuff.board.Length][];
                tableParent[i] = new int[serverStuff.board.Length][];
                for (int j = 0; i < serverStuff.board.Length; i++)
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
           

            while ((listeOuverte.Count != 0) && (listeFerme[(listeFerme.Count) - 1].x == posDestination.x) && (listeFerme[(listeFerme.Count) - 1].y == posDestination.y))
            {
                index = retournerPlusPetitNoeud(listeOuverte, tableDistance);
                listeFerme.Add(listeOuverte[index]);
                listeOuverte.RemoveAt(index);

                if (serverStuff.board[listeFerme[(listeFerme.Count) - 1].x + 1][listeFerme[(listeFerme.Count) - 1].y] != Tile.IMPASSABLE_WOOD)
                {
                    Pos posDroite = new Pos();
                    posDroite.x = listeFerme[(listeFerme.Count) - 1].x + 1;
                    posDroite.y = listeFerme[(listeFerme.Count) - 1].y;

                    if (!existeDansListe(posDroite, listeOuverte))
                    {
                        listeOuverte.Add(posDroite);
                        tableDistance[posDroite.x][posDroite.y][0] = tableDistance[listeFerme[(listeFerme.Count) - 1].x][listeFerme[(listeFerme.Count) - 1].y][0] + 1;
                        tableDistance[posDroite.x][posDroite.y][1] = (int)getDistanceReelle(posDroite, posDestination);
                        tableParent[posDroite.x][posDroite.y][0] = listeFerme[(listeFerme.Count) - 1].x;
                        tableParent[posDroite.x][posDroite.y][0] = listeFerme[(listeFerme.Count) - 1].y;
                    }
                    else
                    {
                        if (tableDistance[posDroite.x][posDroite.y][0] >= tableDistance[listeFerme[(listeFerme.Count) - 1].x][listeFerme[(listeFerme.Count) - 1].y][0])
                        {
                            tableDistance[posDroite.x][posDroite.y][0] = tableDistance[listeFerme[(listeFerme.Count) - 1].x][listeFerme[(listeFerme.Count) - 1].y][0];
                            tableParent[posDroite.x][posDroite.y][0] = listeFerme[(listeFerme.Count) - 1].x;
                            tableParent[posDroite.x][posDroite.y][0] = listeFerme[(listeFerme.Count) - 1].y;
                        }
                    }
                  
                }

                if (serverStuff.board[listeFerme[(listeFerme.Count) - 1].x - 1][listeFerme[(listeFerme.Count) - 1].y] != Tile.IMPASSABLE_WOOD)
                {
                    Pos posGauche = new Pos();
                    posGauche.x = listeFerme[(listeFerme.Count) - 1].x - 1;
                    posGauche.y = listeFerme[(listeFerme.Count) - 1].y;

                    if (!existeDansListe(posGauche, listeOuverte))
                    {
                        listeOuverte.Add(posGauche);
                        tableDistance[posGauche.x][posGauche.y][0] = tableDistance[listeFerme[(listeFerme.Count) - 1].x][listeFerme[(listeFerme.Count) - 1].y][0] + 1;
                        tableDistance[posGauche.x][posGauche.y][1] = (int)getDistanceReelle(posGauche, posDestination);
                        tableParent[posGauche.x][posGauche.y][0] = listeFerme[(listeFerme.Count) - 1].x;
                        tableParent[posGauche.x][posGauche.y][0] = listeFerme[(listeFerme.Count) - 1].y;
                    }
                    else
                    {
                        if (tableDistance[posGauche.x][posGauche.y][0] >= tableDistance[listeFerme[(listeFerme.Count) - 1].x][listeFerme[(listeFerme.Count) - 1].y][0])
                        {
                            tableDistance[posGauche.x][posGauche.y][0] = tableDistance[listeFerme[(listeFerme.Count) - 1].x][listeFerme[(listeFerme.Count) - 1].y][0];
                            tableParent[posGauche.x][posGauche.y][0] = listeFerme[(listeFerme.Count) - 1].x;
                            tableParent[posGauche.x][posGauche.y][0] = listeFerme[(listeFerme.Count) - 1].y;
                        }
                    }
                }

                if (serverStuff.board[listeFerme[(listeFerme.Count) - 1].x][listeFerme[(listeFerme.Count) - 1].y + 1] != Tile.IMPASSABLE_WOOD)
                {
                    Pos posHaut = new Pos();
                    posHaut.x = listeFerme[(listeFerme.Count) - 1].x;
                    posHaut.y = listeFerme[(listeFerme.Count) - 1].y + 1;

                    if (!existeDansListe(posHaut, listeOuverte))
                    {
                        listeOuverte.Add(posHaut);
                        tableDistance[posHaut.x][posHaut.y][0] = tableDistance[listeFerme[(listeFerme.Count) - 1].x][listeFerme[(listeFerme.Count) - 1].y][0] + 1;
                        tableDistance[posHaut.x][posHaut.y][1] = (int)getDistanceReelle(posHaut, posDestination);
                        tableParent[posHaut.x][posHaut.y][0] = listeFerme[(listeFerme.Count) - 1].x;
                        tableParent[posHaut.x][posHaut.y][0] = listeFerme[(listeFerme.Count) - 1].y;
                    }
                    else
                    {
                        if (tableDistance[posHaut.x][posHaut.y][0] >= tableDistance[listeFerme[(listeFerme.Count) - 1].x][listeFerme[(listeFerme.Count) - 1].y][0])
                        {
                            tableDistance[posHaut.x][posHaut.y][0] = tableDistance[listeFerme[(listeFerme.Count) - 1].x][listeFerme[(listeFerme.Count) - 1].y][0];
                            tableParent[posHaut.x][posHaut.y][0] = listeFerme[(listeFerme.Count) - 1].x;
                            tableParent[posHaut.x][posHaut.y][0] = listeFerme[(listeFerme.Count) - 1].y;
                        }
                    }

                }

                if (serverStuff.board[listeFerme[(listeFerme.Count) - 1].x][listeFerme[(listeFerme.Count) - 1].y - 1] != Tile.IMPASSABLE_WOOD)
                {
                    Pos posBas = new Pos();
                    posBas.x = listeFerme[(listeFerme.Count) - 1].x;
                    posBas.y = listeFerme[(listeFerme.Count) - 1].y - 1;

                    if (!existeDansListe(posBas, listeOuverte))
                    {
                        listeOuverte.Add(posBas);
                        tableDistance[posBas.x][posBas.y][0] = tableDistance[listeFerme[(listeFerme.Count) - 1].x][listeFerme[(listeFerme.Count) - 1].y][0] + 1;
                        tableDistance[posBas.x][posBas.y][1] = (int)getDistanceReelle(posBas, posDestination);
                        tableParent[posBas.x][posBas.y][0] = listeFerme[(listeFerme.Count) - 1].x;
                        tableParent[posBas.x][posBas.y][0] = listeFerme[(listeFerme.Count) - 1].y;
                    }
                    else
                    {
                        if (tableDistance[posBas.x][posBas.y][0] >= tableDistance[listeFerme[(listeFerme.Count) - 1].x][listeFerme[(listeFerme.Count) - 1].y][0])
                        {
                            tableDistance[posBas.x][posBas.y][0] = tableDistance[listeFerme[(listeFerme.Count) - 1].x][listeFerme[(listeFerme.Count) - 1].y][0];
                            tableParent[posBas.x][posBas.y][0] = listeFerme[(listeFerme.Count) - 1].x;
                            tableParent[posBas.x][posBas.y][0] = listeFerme[(listeFerme.Count) - 1].y;
                        }
                    }

                }           
            }

            bool finish = false;
            List<String> deplacement = new List<String>();
            Pos posEnfant = posDestination; 
            Pos posParent = new Pos();
            while (finish == false)
            {
                posParent.x = tableParent[posEnfant.x][posEnfant.y][0];
                posParent.y = tableParent[posEnfant.x][posEnfant.y][1];

                if (posParent.x == posEnfant.x && posParent.y > posEnfant.y)
                { // parent au dessus de l'enfant --> déplacement vers le bas
                    deplacement.Add(Direction.South);
                }
                else if (posParent.x == posEnfant.x && posParent.y < posEnfant.y)
                { // parent en dessous de l'enfant --> déplacement vers la haut
                    deplacement.Add(Direction.North);
                }
                else if (posParent.x < posEnfant.x && posParent.y == posEnfant.y)
                { // parent à gauche de l'enfant --> déplacement vers la droite
                    deplacement.Add(Direction.East);
                }
                else{ // parent à droite de l'enfant --> déplacement vers la gauche
                    deplacement.Add(Direction.West);
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

                
            }
            deplacement.Reverse();
            return deplacement;
            
        }

        public bool existeDansListe(Pos point, List<Pos> listeOuverte)
        {
            bool found = false;
            int i = 0;
            while ((i < listeOuverte.Count) && (found == false))
                if ((listeOuverte[i].x == point.x) && (listeOuverte[i].y == point.y))
                    found = true;
            return found;
        }


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

        public double getDistanceReelle(Pos posCourante, Pos arrive)
        {
            return arrive.x - posCourante.x + arrive.y - posCourante.y;
        }



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

         public void trieList(List<Pos> liste, int nombreObjet)
         {
             Pos objetTemp = new Pos();
             for (int i = 0; i < nombreObjet; i++)
             {
                 for (int j = 0; j < nombreObjet; i++)
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
    
        

    
    }
}
