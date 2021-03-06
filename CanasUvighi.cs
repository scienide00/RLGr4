﻿//
//  CanasUvighi.cs
//
//  Author:
//       scienide <alexandar921@abv.bg>
//
//  Copyright (c) 2015 scienide
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace RLG
{
    #region Using statements

    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Storage;

    using RLG.Contracts;
    using RLG.Entities;
    using RLG.Enumerations;
    using RLG.Framework;
    using RLG.Utilities;

    #endregion

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class CanasUvighi : Game
    {
        private const int ScreenWidth = 800;
        private const int ScreenHeight = 500;
        private const int MinTurnCost = 100;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // Custom defined fields
        private bool expectCommand = false;
        private VisualEngine visualEngine;
        private SpriteFont asciiGraphicsFont;
        private SpriteFont logFont;
        private IMessageLog messageLog;
        private KeyboardBuffer keyboardBuffer;
        private ActorQueueHelper queueHelper;
        private IActor actor;

        // Temporary
        private IMap testMap;

        public CanasUvighi()
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.Content.RootDirectory = "Content";
            this.graphics.IsFullScreen = false;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            this.spriteBatch = new SpriteBatch(GraphicsDevice);

            this.graphics.PreferredBackBufferWidth = ScreenWidth;
            this.graphics.PreferredBackBufferHeight = ScreenHeight;
            this.graphics.ApplyChanges();
            this.IsMouseVisible = true;

            this.keyboardBuffer = new KeyboardBuffer();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Font for ASCII graphics
            this.asciiGraphicsFont = this.Content.Load<SpriteFont>("Fonts/BPmono40Bold");
            this.logFont = this.Content.Load<SpriteFont>("Fonts/Consolas12");

            #region Temporary code

            // Create a map to use for (test) playground.
            this.testMap = new MapContainer(
                                   MapUtilities.GenerateRandomMap(20, 20, VisualMode.ASCII));
            this.testMap.LoadTileNeighboors();

            // The Actor queue (for the turn system) specific to this map.
            this.queueHelper = new ActorQueueHelper(this.testMap);

            // Initialize VisualEngine for the game
            this.visualEngine = new VisualEngine(
                VisualMode.ASCII,
                32,
                16, 11,
                this.testMap,
                null,
                this.asciiGraphicsFont);
            this.visualEngine.DeltaTileDrawCoordinates = new Point(4, -6);
            this.visualEngine.ASCIIScale = 0.7f;

            // Initialize MessageLog.
            Rectangle logRect = new Rectangle(
                                    0,
                                    this.visualEngine.MapDrawboxTileSize.Y * this.visualEngine.TileSize,
                                    ScreenWidth - 30,
                                    (ScreenHeight - 30) - (this.visualEngine.MapDrawboxTileSize.Y * this.visualEngine.TileSize));            
            this.messageLog = new MessageLog(logRect, this.logFont);

            // Create an actor (unit, character, etc.) for the player.
            this.actor = new Actor(
                "SCiENiDE",
                "@",
                new PropertyBag<int>(),
                this.testMap, 
                Flags.IsPlayerControl,
                85,
                Species.Human);
            
            //// TODO: Improve actor spawn/add to actorQueue.
            // "Spawn" the actor on the map, and add him to the map actor list.
            this.actor.Position = new Point();
            this.queueHelper.AddActor(this.actor);
           
            #endregion
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Update the keyboard buffer to get pressed keys
            this.keyboardBuffer.Update();

            // Get the first waiting (FIFO) key
            Keys key = this.keyboardBuffer.Dequeue();

            #region Actor-independent input

            // Process the key
            switch (key)
            {
                case Keys.Escape:
                    this.Exit();
                    break;

                case Keys.Q:
                    //Actor npc = new Actor("Minotaur", "M", 85);
                    break;

                default:
                    this.keyboardBuffer.Enqueue(key);
                    break;
            }

            #endregion

            if (!this.expectCommand)
            {
                this.queueHelper.ActorQueue.AccumulateEnergy();
            }

            for (int x = 0; x < this.queueHelper.ActorQueue.Count; x++)
            {
                IActor current = this.queueHelper.ActorQueue[x];
                if (current.Properties["energy"] >= MinTurnCost)
                {
                    if (current.Flags.HasFlag(Flags.IsPlayerControl))
                    {
                        this.expectCommand = true;

                        #region Actor-dependent input

                        switch (this.keyboardBuffer.Dequeue())
                        {                            
                            case Keys.NumPad8:
                            case Keys.K:
                            case Keys.Up:
                                {
                                    current.Properties["energy"] -= current.Move(CardinalDirection.North);
                                    this.expectCommand = false;
                                    this.OnMove(current);
                                    break;
                                }

                            case Keys.NumPad2:
                            case Keys.J:
                            case Keys.Down:
                                {
                                    current.Properties["energy"] -= current.Move(CardinalDirection.South);
                                    this.expectCommand = false;
                                    this.OnMove(current);
                                    break;
                                }

                            case Keys.NumPad4:
                            case Keys.H:
                            case Keys.Left:
                                {
                                    current.Properties["energy"] -= current.Move(CardinalDirection.West);
                                    this.expectCommand = false;
                                    this.OnMove(current);
                                    break;
                                }

                            case Keys.NumPad6:
                            case Keys.L:
                            case Keys.Right:
                                {
                                    current.Properties["energy"] -= current.Move(CardinalDirection.East);
                                    this.expectCommand = false;
                                    this.OnMove(current);
                                    break;
                                }

                            case Keys.NumPad7:
                            case Keys.Y:
                                {
                                    current.Properties["energy"] -= current.Move(CardinalDirection.NorthWest);
                                    this.expectCommand = false;
                                    this.OnMove(current);
                                    break;
                                }

                            case Keys.NumPad9:
                            case Keys.U:
                                {
                                    current.Properties["energy"] -= current.Move(CardinalDirection.NorthEast);
                                    this.expectCommand = false;
                                    this.OnMove(current);
                                    break;
                                }

                            case Keys.NumPad1:
                            case Keys.B:
                                {
                                    current.Properties["energy"] -= current.Move(CardinalDirection.SouthWest);
                                    this.expectCommand = false;
                                    this.OnMove(current);
                                    break;
                                }

                            case Keys.NumPad3:
                            case Keys.N:
                                {
                                    current.Properties["energy"] -= current.Move(CardinalDirection.SouthEast);
                                    this.expectCommand = false;
                                    this.OnMove(current);
                                    break;
                                }

                            default:
                                break;
                        }

                        #endregion
                    }
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            this.graphics.GraphicsDevice.Clear(Color.Black);

            this.visualEngine.DrawGame(this.spriteBatch, this.actor.Position);

            // this.visualEngine.DrawGrid(this.GraphicsDevice, this.spriteBatch);
            this.messageLog.DrawLog(this.spriteBatch);

            base.Draw(gameTime);
        }

        private void OnMove(IActor actor)
        {
            // this.messageLog.SendMessage(string.Format("player: [{0},{1}]", actor.Position.X, actor.Position.Y));
            /*var path = actor.GetPathTo(actor.CurrentMap[new Point(12, 19)]);
            this.visualEngine.HighlightPath(path);
            if (path == null)
            {
                this.messageLog.SendMessage("no path exists to given coordinates.");
            }*/
        }
    }
}