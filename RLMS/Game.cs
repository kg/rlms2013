using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Squared.Game.Input;
using Squared.Render;
using Squared.Render.Convenience;
using Squared.Task;
using Squared.Util.Event;
using RLMS.Framework;

namespace RLMS {
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game : MultithreadedGame {
        public readonly EventBus EventBus = new EventBus();
        public readonly InputControls InputControls;
        new public readonly ThreadedComponents Components = new ThreadedComponents();
        public readonly ThreadedStateStack States;

        public DefaultMaterialSet Materials;
        public GraphicsDeviceManager Graphics;

        public Texture2D Cursor;
        public SpriteFont UIText;

        public Game () {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            States = new ThreadedStateStack(Scheduler);
            InputControls = new InputControls(EventBus);
        }

        protected TaskScheduler Scheduler {
            get {
                return Program.TaskScheduler;
            }
        }

        public int ViewportWidth {
            get {
                return GraphicsDevice.Viewport.Width;
            }
        }

        public int ViewportHeight {
            get {
                return GraphicsDevice.Viewport.Height;
            }
        }

        public bool Paused {
            get {
                return !IsActive;
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize () {
            // TODO: Add your initialization logic here

            base.Initialize();

            Scheduler.Start(MainTask(), TaskExecutionPolicy.RunAsBackgroundTask);

            if (!InputControls.Available)
                InputControls.PickInputDevice();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent () {
            // Create a new SpriteBatch, which can be used to draw textures.
            Materials = new DefaultMaterialSet(Services);

            // TODO: use this.Content to load your game content here

            Cursor = Content.Load<Texture2D>("cursor");
            UIText = Content.Load<SpriteFont>("UIText");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent () {
            // TODO: Unload any non ContentManager content here
        }

        public IEnumerator<object> MainTask () {
            string menuItem = null;

            while (true) {
                yield return Menu.ShowNew(
                    this, "Mode Select",
                    new[] { 
                        "Action",
                        "Exit" 
                    }
                ).Bind(() => menuItem);

                switch (menuItem) {
                    case "Action":
                        yield return States.Push(new States.Action(this));
                        yield break;

                    case "Exit":
                        this.Exit();
                        yield break;
                }
            }
        }

        protected override void Update (GameTime gameTime) {
            InputControls.Update(IsActive);

            Scheduler.Step();

            foreach (var component in Components)
                component.Update();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            base.Update(gameTime);
        }

        public override void Draw (GameTime gameTime, Frame frame) {
            var ir = new ImperativeRenderer(frame, Materials);

            ir.Clear(color: Color.CornflowerBlue);
            ir.Layer += 1;

            foreach (var component in Components)
                component.Draw(frame, ref ir);
        }
    }
}
