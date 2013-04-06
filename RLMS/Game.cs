using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using RLMS.States.Exploration;
using RLMS.States.Narrative;
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
                        "Exploration",
                        "Action",
                        "Exit" 
                    }
                ).Bind(() => menuItem);

                switch (menuItem) {
                    case "Narrative":
                        yield return NarrativeTest();
                        break;

                    case "Exploration":
                        yield return ExplorationTest();
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

        public IEnumerator<object> NarrativeTest () {
            var allSceneTypes = Scene.GetAllSceneTypes();

            string sceneName = null;

            yield return Menu.ShowNew(
                this, "Scene Select",
                (from t in allSceneTypes select t.Name)
            ).Bind(() => sceneName);

            var sceneType = allSceneTypes.FirstOrDefault((s) => s.Name == sceneName);

            if (sceneType != null)
                yield return PlayScene(sceneType);

            yield break;
        }

        public IEnumerator<object> ExplorationTest () {
            var allAreaTypes = Area.GetAllAreaTypes();

            string areaName = null;

            yield return Menu.ShowNew(
                this, "Area Select",
                (from t in allAreaTypes select t.Name)
            ).Bind(() => areaName);

            var areaType = allAreaTypes.FirstOrDefault((s) => s.Name == areaName);

            if (areaType != null)
                yield return VisitArea(areaType);
        }

        public IFuture PlayScene<T> ()
            where T : Scene {
            return PlayScene(typeof(T));
        }

        internal IFuture PlayScene (Type sceneType) {
            return PlayScene((Scene)Activator.CreateInstance(sceneType));
        }

        internal IFuture PlayScene (Scene scene) {
            return States.Push(new States.NarrativeState(this, scene));
        }

        public IFuture VisitArea<T> ()
            where T : Area {
            return VisitArea(typeof(T));
        }

        internal IFuture VisitArea (Type areaType) {
            return VisitArea((Area)Activator.CreateInstance(areaType));
        }

        internal IFuture VisitArea (Area area) {
            return States.Push(new States.ExplorationState(this, area));
        }

        protected override void Update (GameTime gameTime) {
            InputControls.Update(IsActive);

            Scheduler.Step();

            foreach (var state in States)
                state.Update();

            foreach (var component in Components)
                component.Update();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) {
                while (States.Count > 0)
                    States.Pop();
            }

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

            foreach (var state in States)
                state.Draw(frame, ref ir);

            foreach (var component in Components)
                component.Draw(frame, ref ir);
        }
    }
}
