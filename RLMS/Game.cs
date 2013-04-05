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
    public class Game : MultithreadedGame {
        public readonly EventBus EventBus = new EventBus();
        public readonly InputControls InputControls;
        new public readonly ThreadedComponents Components = new ThreadedComponents();
        public readonly ThreadedStateStack States;

        public DefaultMaterialSet Materials;
        public GraphicsDeviceManager Graphics;
        public ContentLoader ContentLoader;

        public Texture2D Cursor;
        public SpriteFont UIText;

        public Game () {
            Graphics = new GraphicsDeviceManager(this) {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720
            };
            Content.RootDirectory = "Content";

            States = new ThreadedStateStack(Scheduler);
            InputControls = new InputControls(EventBus);
            IsMouseVisible = true;
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

        protected override void Initialize () {
            base.Initialize();

            Scheduler.Start(MainTask(), TaskExecutionPolicy.RunAsBackgroundTask);

            if (!InputControls.Available)
                InputControls.PickInputDevice();
        }

        protected override void LoadContent () {
            Materials = new DefaultMaterialSet(Services);

            Cursor = Content.Load<Texture2D>("cursor");
            UIText = Content.Load<SpriteFont>("UIText");

            ContentLoader = new ContentLoader(Content, GraphicsDevice, RenderCoordinator.CreateResourceLock);
        }

        protected override void UnloadContent () {
        }

        public IEnumerator<object> MainTask () {
            string menuItem = null;

            while (true) {
                yield return Menu.ShowNew(
                    this, "Mode Select",
                    new[] {
                        "Narrative",
                        "Action",
                        "Exit" 
                    }
                ).Bind(() => menuItem);

                switch (menuItem) {
                    case "Narrative":
                        yield return States.Push(new States.NarrativeState(this, new States.Narrative.Scenes.Intro()));
                        break;

                    case "Action":
                        yield return States.Push(new States.ActionState(this));
                        break;

                    case "Exit":
                        this.Exit();
                        yield break;
                }
            }
        }

        protected override void Update (GameTime gameTime) {
            InputControls.Update(IsActive);

            Scheduler.Step();

            var currentState = States.Current;
            if (currentState != null)
                currentState.Update();

            foreach (var component in Components)
                component.Update();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            base.Update(gameTime);
        }

        public override void Draw (GameTime gameTime, Frame frame) {
            var ir = new ImperativeRenderer(
                frame, Materials, 
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.LinearClamp
            );

            ir.Clear(color: new Color(0, 0, 32));
            ir.Layer += 1;

            var currentState = States.Current;
            if (currentState != null)
                currentState.Draw(frame, ref ir);

            foreach (var component in Components)
                component.Draw(frame, ref ir);
        }
    }
}
