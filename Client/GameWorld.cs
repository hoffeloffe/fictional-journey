﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Windows.Forms;

using System.Diagnostics;
using NotAGame;
using System.Linq;
using System.Threading;

namespace SpaceRTS
{
    public class GameWorld : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Map map;
        private Client client = new Client();
        private string serverMessage;
        private List<List<string>> playerInfomationList = new List<List<string>>();
        private string playerPosition;
        private Color color;
        private Thread sendThread;
        private Thread reciveThread;
        private readonly object recivelock = new object();
        private readonly object sendlock = new object();
        private string serverMessageIsTheSame;

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
            sendThread = new Thread(() => SendThread());

            reciveThread = new Thread(() => ReceiveThread());
            map = new Map();
            sendThread.IsBackground = true;
            reciveThread.IsBackground = true;
            sendThread.Start();
            reciveThread.Start();

            base.Initialize();
        }

        public void ReceiveThread()
        {
            while (true)
            {
                lock (recivelock)
                {
                    serverMessage = client.ReceiveData();
                }
            }
        }

        public void SendThread()
        {
            while (true)
            {
                lock (sendlock)
                {
                    client.SendData(new Vector2(2, 3).ToString());
                }
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            map.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                Exit();
            }

            string superservermessage;

            lock (recivelock)
            {
                superservermessage = serverMessage;
            }

            if (superservermessage != null && superservermessage != serverMessageIsTheSame)
            {
                string[] array = superservermessage.Split(',');

                for (int i = 0; i < array.Length; i++)
                {
                    if (i + 1 > playerInfomationList.Count)
                    {
                        playerInfomationList.Add(array[i].Split('_').ToList());
                    }
                    else
                        playerInfomationList[i] = array[i].Split('_').ToList();
                }

                string som = playerInfomationList[0][0].ToString();

                serverMessageIsTheSame = superservermessage;
            }

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            map.Draw(_spriteBatch);
            base.Draw(gameTime);
            _spriteBatch.End();
        }
    }
}