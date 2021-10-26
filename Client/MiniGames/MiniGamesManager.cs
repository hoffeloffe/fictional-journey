﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NotAGame.MiniGames;
using SpaceRTS;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotAGame
{
    class MiniGamesManager
    {
        #region Singleton
        private static MiniGamesManager instance;
        public static MiniGamesManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MiniGamesManager();
                }
                return instance;
            }
        }
        #endregion Singleton
        public int currentGame;
        MiniGameTest miniGame;
        private static GameSSP sSP;
        Lobby lobby;

        private bool noHoldDown = false;

        public MiniGamesManager()
        {
            lobby = new Lobby();
            miniGame = new MiniGameTest();
            sSP = new GameSSP();

            currentGame = 0;
        }

        public void Update()
        {
            KeyboardState key = Keyboard.GetState();

            if (key.IsKeyDown(Keys.M) && noHoldDown == false)
            {
                currentGame += 1;
                noHoldDown = true;
            }

            sSP.Update();

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            sSP.DrawText(spriteBatch);
        }

        public void DrawNextGame(SpriteBatch spriteBatch)
        {
            switch (currentGame)
            {
                case 1:
                    miniGame.DrawEmil(spriteBatch);
                    break;
                case 2:
                    lobby.LobbyMaker();
                    break;
            }
            if (currentGame > 5)
            {
                currentGame = 0;
                noHoldDown = false;
            }
           
        }

    }
}
