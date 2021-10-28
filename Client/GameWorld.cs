﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using NotAGame;
using System.Linq;
using NotAGame.Component;
using System.Threading;
using NotAGame.Command_Pattern;
using System;

namespace SpaceRTS
{
    public class GameWorld : Game
    {
        #region Singleton

        private static GameWorld instance;

        public static GameWorld Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameWorld();
                }
                return instance;
            }
        }

        #endregion Singleton

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Lobby lobby;
        private Random rnd = new Random();
        private Color yourColor;
        private List<int> playersId = new List<int>();
        private string name = "NoName";
        private List<string> names = new List<string>() { "Jeff", "John", "Joe", "Jack", "Jim", "Peter", "Paul", "Ticky", "Tennis", "Egg Salad", "Dingus", "Fred", "Mango", "Cupcake", "Snowball", "Dragonborn" };
        private Player player;
        private string som;
        public List<GameObject> opponents = new List<GameObject>();
        private int OpponentGOBJCounter = 0;
        public int playerID;
        public int totalPoints = 0;
        public int changeInTotalPoints = 0;
        public Vector2 changeInPosition;
        public int positionWait = 0;
        public bool playerMoveFrame = false;
        public bool playerHaltFrame = false;

        private List<GameObject> gameObjects = new List<GameObject>();

        public List<GameObject> GameObjects
        {
            get
            {
                return gameObjects;
            }

            set
            {
                gameObjects = value;
            }
        }

        private GameObject playerGo;
        private List<Player> players = new List<Player>();

        public static bool changeGame = false;
        private MiniGamesManager gameManager;
        public static Texture2D Emil;

        private Client client = new Client();
        private string serverMessage;
        private List<string[]> playerInfomationList = new List<string[]>();
        private int plInfoListCountIsTheSame = 0;
        private Color color;
        private Thread sendThread;
        private Thread reciveThread;

        private readonly object recivelock = new object();
        private readonly object sendlock = new object();
        private string comparePrevServerMsg;
        private List<string> chatstring;

        public float DeltaTime { get; set; }

        public GameWorld()

        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
        }

        protected override void Initialize()
        {
            gameManager = new MiniGamesManager();
            lobby = new Lobby();

            // #region GameObjects - Player, texts, add to gameObjects

            #region Component

            playerGo = new GameObject();
            player = new Player();
            playerGo.AddComponent(player);
            playerGo.AddComponent(new SpriteRenderer());
            gameObjects.Add(playerGo);

            yourColor = new Color(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            Random r = new Random();
            int index = r.Next(names.Count);
            name = names[index];

            #region Tekst

            GameObject goText = new GameObject();
            SpriteRenderer cpSprite = new SpriteRenderer();
            Text CpText = new Text();
            goText.AddComponent(cpSprite);
            goText.AddComponent(CpText);
            CpText.SetText("Arial96", "MonoParty!", 620, 5, 1f, 0.12f, Color.MonoGameOrange);
            cpSprite.hasShadow = true;
            cpSprite.Color2 = Color.Black;
            //cpSprite.Layerdepth = 0.5f;
            gameObjects.Add(goText);
            //--------------
            goText = new GameObject();
            cpSprite = new SpriteRenderer();
            CpText = new Text();
            goText.AddComponent(cpSprite);
            goText.AddComponent(CpText);
            CpText.SetText("Arial24", "More text here!", 100, 190, 1f, -0.05f, Color.Black);
            gameObjects.Add(goText);
            //--------------
            goText = new GameObject();
            cpSprite = new SpriteRenderer();
            CpText = new Text();
            goText.AddComponent(cpSprite);
            goText.AddComponent(CpText);
            CpText.SetText("Hands", "Outline test.", 100, 250, 0.5f, 0, Color.White);
            cpSprite.hasOutline = true;
            cpSprite.Color2 = Color.Black;
            gameObjects.Add(goText);
            //--------------
            goText = new GameObject();
            cpSprite = new SpriteRenderer();
            CpText = new Text();
            goText.AddComponent(cpSprite);
            goText.AddComponent(CpText);
            CpText.SetText("Hands", "Shadow test.", 100, 330, 0.5f, 0, Color.White);
            cpSprite.hasShadow = true;
            gameObjects.Add(goText);
            //--------------
            goText = new GameObject();
            cpSprite = new SpriteRenderer();
            CpText = new Text();
            goText.AddComponent(cpSprite);
            goText.AddComponent(CpText);
            CpText.SetText("Hands", ":D", 125, 425, 0.5f, 0, Color.Green);
            cpSprite.hasOutline = false;
            cpSprite.hasShadow = true;
            cpSprite.Spin = true;
            gameObjects.Add(goText);
            //--------------
            goText = new GameObject();
            cpSprite = new SpriteRenderer();
            CpText = new Text();
            goText.AddComponent(cpSprite);
            goText.AddComponent(CpText);
            CpText.SetText("Hands", "Shadow test.", 100, 330, 0.5f, 0, Color.White);
            cpSprite.hasShadow = true;
            gameObjects.Add(goText);

            #endregion Tekst

            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Awake();
            }
            foreach (GameObject opponent in opponents)
            {
                opponent.Awake();
            }

            #endregion Component

            #region Server Thread

            sendThread = new Thread(() => client.SendData());
            reciveThread = new Thread(() => ReceiveThread());
            sendThread.IsBackground = true;
            reciveThread.IsBackground = true;
            sendThread.Start();
            reciveThread.Start();

            #endregion Server Thread

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Emil = Content.Load<Texture2D>("Emil");
            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Start();
            }
            foreach (GameObject opponent in opponents)
            {
                opponent.Start();
            }
        }

        protected override void Update(GameTime gameTime)
        {
            positionWait++;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                Exit();
            }
            DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            InputHandler.Instance.Excute(player);
            gameManager.ChangeGame();

            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Update(gameTime);
            }

            foreach (GameObject opponent in opponents)
            {
                opponent.Update(gameTime);
            }

            #region Client/Server

            #region Server Beskeder

            if (playerGo.transform.Position != changeInPosition) //If player pos. is new, send position to server.
            {
                if (positionWait > 5) //Sleeptimer
                {
                    client.cq.Enqueue("PO" + playerGo.transform.Position);
                    positionWait = 0;
                }
                //client.cq.Enqueue("PO" + playerGo.transform.Position);
                changeInPosition = playerGo.transform.Position;
            }
            if (serverMessage != null && serverMessage != comparePrevServerMsg) //if not empty or same
            {
                string serverMsg = serverMessage;
                #region Create Opponent GameObjects Equal to total opponents (virker med dig selv, men ikke med flere spillere endnu)
                Debug.Write("  <");
                if (serverMsg.StartsWith("PO"))
                {
                    Debug.Write("(PO)");
                    serverMsg = serverMsg.Remove(0, 2);
                    string[] serverMsgArray = serverMsg.Split("_");
                    string ID = serverMsgArray[1].ToString();
                    string Position = serverMsgArray[0].ToString();

                    if (playerInfomationList.Count == 0)
                    {
                        Debug.Write("(ID " + serverMsgArray[1] + " == 0)");
                        playerInfomationList.Add(new string[] { ID, Position });
                        Debug.Write("(NOW PLI: " + playerInfomationList.Count + " Opponents: " + opponents.Count + ")");
                        CreateOpponentObj(ID);
                    }
                    else
                    {
                        Debug.Write("(PIL NOT == 0)");
                        bool foundID = false;
                        for (int i = 0; i < playerInfomationList.Count; i++)
                        {
                            if (playerInfomationList[i][0].Contains(ID)) //Is the player whose servermessage belong to in the PIL list?
                            {
                                foundID = true;
                                Debug.Write("(FOUND ID, do nothing.)");
                            }
                        }
                        if (foundID == false) //if servermessage's ID is not on the PIL list, add it to the list and create new opponent object.
                        {
                            Debug.Write("(DIDN'T FIND ID: " + serverMsgArray[1] + ")");
                            playerInfomationList.Add(new string[] { ID, Position });
                            CreateOpponentObj(serverMsgArray[1]);
                            Debug.Write("NOW PLI: " + playerInfomationList.Count + " Opponents: " + opponents.Count + ")");
                        }
                        else
                        {
                            Debug.Write("(I already know player ID " + ID + "!)");
                        }
                    }


                    UpdatePos(Convert.ToInt32(ID), Position);

                    comparePrevServerMsg = "PO" + serverMsg;
                }

                if (serverMsg.StartsWith("ID"))
                {
                    Debug.Write("(ID)");
                    //Din ID
                    serverMsg = serverMsg.Remove(0, 2);
                    playerID = Convert.ToInt32(serverMsg);
                    foreach (var item in playerInfomationList)
                    {
                        string ID = serverMsg[1].ToString();
                        bool foundID = false;

                        if (!item.Contains(serverMsg[1].ToString()))
                        {
                            Debug.Write("(no ID: " + serverMsg[1] + " , adding)");
                            playerInfomationList.Add(new string[] { ID, new Vector2().ToString() });
                            CreateOpponentObj(serverMsg[1].ToString());
                        }
                        else
                            Debug.Write("(I already know player ID " + ID + "!)");
                    }
                    comparePrevServerMsg = "ID" + serverMsg;
                }
                //Modtagende beskeder.
                if (serverMsg.StartsWith("ME"))
                {
                    serverMsg = serverMsg.Remove(0, 2);
                    chatstring.Add(serverMsg);
                }

                //Send Dine Totale Points til serveren.
                if (totalPoints != changeInTotalPoints)
                {
                    client.cq.Enqueue("TP" + totalPoints);
                    changeInTotalPoints = totalPoints;
                }

                //Modtag Alles Totale Points
                if (serverMsg.StartsWith("TP"))
                {
                    serverMsg = serverMsg.Remove(0, 2);
                }
                //Send Dine Minigame points til serveren.
                if (serverMsg.StartsWith("MP"))
                {
                    serverMsg = serverMsg.Remove(0, 2);
                }
                //Modtage alles Minigame points til serveren.
                if (serverMsg.StartsWith("MP"))
                {
                    serverMsg = serverMsg.Remove(0, 2);
                }

                //Send Done
                if (serverMsg.StartsWith("DO"))
                {
                    serverMsg = serverMsg.Remove(0, 2);
                }
                //Modtage Done
                if (serverMsg.StartsWith("DO"))
                {
                    serverMsg = serverMsg.Remove(0, 2);
                }
                //Send Fail
                if (serverMsg.StartsWith("FA"))
                {
                    serverMsg = serverMsg.Remove(0, 2);
                }
                //Modtage Fails
                if (serverMsg.StartsWith("FA"))
                {
                    serverMsg = serverMsg.Remove(0, 2);
                }
                //Send Username
                if (serverMsg.StartsWith("US"))
                {
                    serverMsg = serverMsg.Remove(0, 2);
                }
                //Modtage Username
                if (serverMsg.StartsWith("US"))
                {
                    serverMsg = serverMsg.Remove(0, 2);
                }
                //Send Color
                if (serverMsg.StartsWith("CO"))
                {
                    serverMsg = serverMsg.Remove(0, 2);
                }
                //Modtage Color
                if (serverMsg.StartsWith("CO"))
                {
                    serverMsg = serverMsg.Remove(0, 2);
                }

                if (opponents.Count < playerInfomationList.Count)//er opponents mindre end antallet af array strenge? tilføj ny opponent.
                {
                    while (opponents.Count < playerInfomationList.Count)
                    {
                        rnd = new Random();
                        //Color randomColor = new Color(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                        GameObject oppObj = new GameObject();
                        SpriteRenderer oppSpr = new SpriteRenderer();
                        Opponent oppOpp = new Opponent();
                        //oppSpr.Color = randomColor;
                        oppObj.AddComponent(oppSpr);
                        oppObj.AddComponent(oppOpp);
                        //Adding opponents and playerId at the same time should help us keep track of who is who, because their positions in the lists are the same...
                        opponents.Add(oppObj);
                        playersId.Add(Convert.ToInt32(playerInfomationList[playerInfomationList.Count - 1][0]));
                        oppSpr.Font = Content.Load<SpriteFont>("Fonts/Arial24");
                        oppSpr.hasLabel = true;
                        oppSpr.Text = playerInfomationList[playerInfomationList.Count - 1][0] + " ";
                        oppObj.Awake();
                        oppObj.Start();
                    }
                }
                if (opponents.Count > playerInfomationList.Count)//er opponents mindre end antallet af array strenge? tilføj ny opponent.
                {
                    Debug.WriteLine("Oops, somebody somehow disconnected?");
                }
                foreach (int id in playersId)
                {
                    //UpdatePos(id);
                    UpdateColor(id);
                    UpdateName(id);
                }

                #endregion Create Opponent GameObjects Equal to total opponents (virker med dig selv, men ikke med flere spillere endnu)


                plInfoListCountIsTheSame = playerInfomationList.Count;
                Debug.Write("> PLInfo: " + plInfoListCountIsTheSame + ", opponents: " + opponents.Count + " " + comparePrevServerMsg);
                Debug.WriteLine("");
                //serverMessageIsTheSame = serverMessage;
                //if (serverMessage != serverMessageIsTheSame)
                //{
                    
                //}
            }
            else
            {
                comparePrevServerMsg = serverMessage;
            }

            #endregion Server Beskeder

            #endregion Client/Server

            // position + message + totalPoints +  minigamePoints + done + failed username + color;
            //                  position,                                                        message,     totalPoints, minigamePoints + done + failed username + color;
            //client.cq.Enqueue(playerGo.transform.ReturnPosition(playerGo).ToString() + "@" + "messageTest" + "@" + "1" + "@" + "9" + "@" + "false" + "@" + "false" + "@" + name + "@" + yourColor);
            serverMessage = null;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            
            GraphicsDevice.Clear(Color.DarkGray);
            _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Draw(_spriteBatch);
            }
            foreach (GameObject opponent in opponents)
            {
                opponent.Draw(_spriteBatch);
            }

            //foreach (string message in chatstring)
            //{
            //    _spriteBatch.DrawString(, message, new Vector2(100, 100), Color.Black);
            //}
            gameManager.DrawNextGame(_spriteBatch);

            base.Draw(gameTime);
            _spriteBatch.End();
        }

        public void UnloadGame(GameObject go)
        {
            if (go.Tag == "Tiles")
            {
                gameObjects.Remove(go);
            }
        }

        #region Thread Method

        public void ReceiveThread()
        {
            while (true)
            {
                serverMessage = client.ReceiveData();
                Debug.Write("~" + serverMessage + "~. ");
            }
        }


        #endregion Thread Method

        public void UpdatePos(int id, string value)
        {
            foreach (var obj in opponents)
            {
                if (id == obj.Id)
                {
                    foreach (var InfoID in playerInfomationList)
                    {
                        if (id == Convert.ToInt32(InfoID[0]))
                        {
                            InfoID[1] = value;//update position in ListString
                            string replacer = InfoID[1].ToString();
                            string cleanString = replacer.Replace("{X:", "");
                            cleanString = cleanString.Replace("Y:", "");
                            cleanString = cleanString.Replace("}", "");
                            cleanString = cleanString.Replace(".", ",");
                            string[] xyVals = cleanString.Split(' ');
                            float XPos = float.Parse(xyVals[0]);
                            float YPos = float.Parse(xyVals[1]);
                            obj.transform.Position = new Vector2(XPos, YPos); //update position in opponentslist.
                        }
                    }
                }
            }

            //Debug.WriteLine("Upd. Pos. OppList id " + id + ": X" + XPos + ", Y" + YPos + " " + opponents[id].transform.Position + ", InfoVal: " + playerInfomationList[id][1]);
            //Debug.Write("[Obj:" + id + ", id:" + playerInfomationList[id][0] + "]");
        }

        public void UpdateColor(int id)
        {
            string som = playerInfomationList[id][8].ToString();
            string cleanString = som.Replace("{R:", "");
            cleanString = cleanString.Replace("G:", "");
            cleanString = cleanString.Replace("B:", "");
            cleanString = cleanString.Replace("A:255}", "");
            cleanString = cleanString.Replace(".", ",");
            string[] xyVals = cleanString.Split(' ');
            int R = Convert.ToInt32(xyVals[0]);
            int G = Convert.ToInt32(xyVals[1]);
            int B = Convert.ToInt32(xyVals[2]);
            //string client0Message = som + " anyway, X: " + XPos + ", og Y: " + YPos;
            //Debug.WriteLine(client0Message);
            SpriteRenderer srr = (SpriteRenderer)opponents[id].GetComponent("SpriteRenderer");
            Color newColor = new Color(R, G, B);
            srr.Color = newColor;
            //srr.Color = new Color(R, G, B);
            Debug.WriteLine(srr.Color);
        }

        public void UpdateName(int id)
        {
            string som = playerInfomationList[id][8].ToString();
            SpriteRenderer srr = (SpriteRenderer)opponents[id].GetComponent("SpriteRenderer");
            srr.Text = playerInfomationList[id][0] + " " + playerInfomationList[id][7];
            //srr.Color = new Color(R, G, B);
            Debug.WriteLine(srr.Color);
        }

        public void CreateOpponentObj(string ID)
        {
            int theID = Convert.ToInt32(ID);
            rnd = new Random();
            //Color randomColor = new Color(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            GameObject oppObj = new GameObject();
            oppObj.Id = theID;
            SpriteRenderer oppSpr = new SpriteRenderer();
            Opponent oppOpp = new Opponent();
            //oppSpr.Color = randomColor;
            oppObj.AddComponent(oppSpr);
            oppObj.AddComponent(oppOpp);
            //Adding opponents and playerId at the same time should help us keep track of who is who, because their positions in the lists are the same...
            opponents.Add(oppObj);
            //playersId.Add(Convert.ToInt32(playerInfomationList[playerInfomationList.Count - 1][0]));
            //oppSpr.Font = Content.Load<SpriteFont>("Fonts/Arial24");
            //oppSpr.hasLabel = true;
            //oppSpr.Text = playerInfomationList[playerInfomationList.Count - 1][0] + " ";
            oppObj.Awake();
            oppObj.Start();

            Debug.Write("(Creating... " + opponents.Count + " Opponent Objs 1 new.");
        }
    }
}