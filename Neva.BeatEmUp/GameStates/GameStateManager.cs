﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Neva.BeatEmUp.GameStates
{
    public sealed class GameStateManager : DrawableGameComponent
    {
        #region Vars

        private readonly List<GameState> states;
        private GameState current, previous;

        #endregion

        #region Properties

        public SpriteBatch SpriteBatch
        {
            get;
            private set;
        }
        public GameState Current
        {
            get
            {
                return current;
            }
        }

        #endregion

        #region Ctor

        public GameStateManager(Game game) : 
            base(game)
        {
            states = new List<GameState>();
        }

        #endregion

        #region Methods

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        public void Push(GameState gameState)
        {
            if (states.Count != 0) previous = states[states.Count - 1];
            current = gameState;

            states.Add(gameState);
            gameState.Initialize(Game, this);
        }

        public void Pop()
        {
            if (states.Count == 0) return;
            current = previous;
            previous = states[states.Count - 1];
            states.RemoveAt(states.Count - 1);
        }

        public void Change(GameState gameState)
        {
            if (current != null)
            {
                current.OnDeactivate();
            }

            previous = current;
            current = gameState;

            if (current != null)
            {
                current.OnActivate();
            }

            if (states.Count == 0)
                states.Add(gameState);
            else
            {
                states[states.Count - 1] = gameState;
            }

            gameState.Initialize(Game, this);
        }

        public override void Update(GameTime gameTime)
        {
            if (current == null)
            {
                return;
            }

            for (int i = states.Count - 1; i >= 0; i--)
            {
                states[i].Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (current == null)
            {
                return;
            }

            SpriteBatch.Begin();

            current.Draw(SpriteBatch);

            SpriteBatch.End();
        }

        #endregion
    }
}