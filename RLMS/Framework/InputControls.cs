using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Squared.Task;
using Squared.Util.Event;
using Squared.Game.Input;

namespace RLMS.Framework {
    public class InputControls : InputControlCollection {
        public struct ActiveControllerInfo {
            public bool IsKeyboard;
            public int ConnectedGamepads;
            public PlayerIndex? ActiveController;
        }

        public readonly EventBus GameEventBus;

        public static float GraceTime = 0.25f;
        public static float RepeatRate = 0.25f;
        public static float DeadZone = 0.45f;

        public InputControl Left;
        public InputControl Up;
        public InputControl Right;
        public InputControl Down;

        public Vector2? MouseLocation = null;
        public Vector2 LeftStick;
        public Vector2 RightStick;
        public Vector2 DPad;

        public InputControl JoypadA;
        public InputControl JoypadX;
        public InputControl JoypadB;
        public InputControl JoypadY;

        public PlayerIndex? ActiveController;

        protected bool _UseKeyboard = false;
        protected int _GamePadsConnected = 0;

        public InputControls (EventBus gameEventBus)
            : base() {
            GameEventBus = gameEventBus;
        }

        public InputControl Accept {
            get {
                return JoypadA;
            }
        }

        public InputControl Cancel {
            get {
                return JoypadB;
            }
        }

        protected override float GetRepeatRate () {
            return RepeatRate;
        }

        public int ConnectedGamepadCount {
            get {
                return _GamePadsConnected;
            }
        }

        public bool Available {
            get {
                return ((_GamePadsConnected > 0) && (ActiveController.HasValue)) ||
                    (_UseKeyboard);
            }
        }

        public bool IsKeyboard {
            get {
                return _UseKeyboard;
            }
        }

        protected void BroadcastControllerChange () {
            var info = new ActiveControllerInfo {
                ActiveController = ActiveController,
                IsKeyboard = _UseKeyboard,
                ConnectedGamepads = _GamePadsConnected
            };

            EventBus.Broadcast(this, "ActiveControllerChanged", info);
            GameEventBus.Broadcast(this, "ActiveControllerChanged", info);
        }

        protected override void OnUpdate () {
            _GamePadsConnected = 0;

            for (int i = 0; i < 3; i++) {
                PlayerIndex playerIndex;

                switch (i) {
                    default:
                        playerIndex = PlayerIndex.One;
                    break;
                    case 1:
                        playerIndex = PlayerIndex.Two;
                    break;
                    case 2:
                        playerIndex = PlayerIndex.Three;
                    break;
                    case 3:
                        playerIndex = PlayerIndex.Four;
                    break;
                }

                if (ActiveController.HasValue && (playerIndex != ActiveController.Value))
                    continue;

                var gs = GamePad.GetState(playerIndex, GamePadDeadZone.IndependentAxes);

                if (ActiveController.HasValue) {
                    if (gs.IsConnected) {
                        _GamePadsConnected += 1;
                    } else if (!_UseKeyboard) {
                        EventBus.Broadcast(this, "ControllerDisconnected", playerIndex);
                        ActiveController = null;
                        BroadcastControllerChange();
                    }
                } else {
                    if (gs.IsConnected)
                        _GamePadsConnected += 1;
                }
                
                if (gs.IsConnected) {
                    LeftStick = gs.ThumbSticks.Left;

                    if (LeftStick.X > 0)
                        Right.Value = LeftStick.X;
                    else if (LeftStick.X < 0)
                        Left.Value = -LeftStick.X;
                    if (LeftStick.Y > 0)
                        Up.Value = LeftStick.Y;
                    else if (LeftStick.Y < 0)
                        Down.Value = -LeftStick.Y;

                    RightStick = gs.ThumbSticks.Right;

                    DPad.X = DPad.Y = 0;

                    if (gs.IsButtonDown(Buttons.DPadLeft)) {
                        Left.State = true;
                        DPad.X -= 1;
                    }

                    if (gs.IsButtonDown(Buttons.DPadRight)) {
                        Right.State = true;
                        DPad.X += 1;
                    }

                    if (gs.IsButtonDown(Buttons.DPadUp)) {
                        Up.State = true;
                        DPad.Y -= 1;
                    }

                    if (gs.IsButtonDown(Buttons.DPadDown)) {
                        Down.State = true;
                        DPad.Y += 1;
                    }

                    if (gs.IsButtonDown(Buttons.A))
                        JoypadA.State = true;
                    if (gs.IsButtonDown(Buttons.B))
                        JoypadB.State = true;
                    if (gs.IsButtonDown(Buttons.X))
                        JoypadX.State = true;
                    if (gs.IsButtonDown(Buttons.Y))
                        JoypadY.State = true;
                    if (gs.IsButtonDown(Buttons.Back))
                        Cancel.State = true;

                    if (!ActiveController.HasValue) {
                        if (gs.IsButtonDown(Buttons.Start) ||
                            gs.IsButtonDown(Buttons.A) ||
                            gs.IsButtonDown(Buttons.B) ||
                            gs.IsButtonDown(Buttons.X) ||
                            gs.IsButtonDown(Buttons.Y)) {

                            ActiveController = playerIndex;
                            EventBus.Broadcast(this, "ControllerConnected", playerIndex);
                            BroadcastControllerChange();
                            break;
                        }
                    }
                }
            }

#if !XBOX
            var ks = new LocalizedKeyboardState(Keyboard.GetState());

            if (!ActiveController.HasValue) {
                if (ks.IsKeyDown(Keys.Enter, true) || ks.IsKeyDown(Keys.Space, true)) {
                    ActiveController = PlayerIndex.One;
                    _UseKeyboard = true;
                    EventBus.Broadcast(this, "ControllerConnected", PlayerIndex.One);
                    BroadcastControllerChange();
                }
            }

            if (ks.IsKeyDown(Keys.Left, true) || ks.IsKeyDown(Keys.A))
                Left.State = true;
            if (ks.IsKeyDown(Keys.Right, true) || ks.IsKeyDown(Keys.D))
                Right.State = true;
            if (ks.IsKeyDown(Keys.Up, true) || ks.IsKeyDown(Keys.W))
                Up.State = true;
            if (ks.IsKeyDown(Keys.Down, true) || ks.IsKeyDown(Keys.S))
                Down.State = true;

            if (ks.IsKeyDown(Keys.Space, true) || ks.IsKeyDown(Keys.Enter, true))
                Accept.State = true;
            if (ks.IsKeyDown(Keys.Escape, true))
                Cancel.State = true;

            var ms = Mouse.GetState();
            if (ms.LeftButton == ButtonState.Pressed)
                Accept.State = true;
            if (ms.RightButton == ButtonState.Pressed)
                Cancel.State = true;

            MouseLocation = new Vector2(ms.X, ms.Y);
#endif
        }

        public void PickInputDevice () {
            var gps = GamePad.GetState(PlayerIndex.One);

            if (gps.IsConnected)
                ActiveController = PlayerIndex.One;
            else
                _UseKeyboard = true;
        }
    }

    public static class InputExtensions {
        private class WaitForPressThunk {
            public readonly SignalFuture Future = new SignalFuture();
            public readonly InputEventSubscription Subscription;

            public WaitForPressThunk (InputControl control) {
                Subscription = control.AddListener(EventHandler);
            }

            public bool EventHandler (InputControl c, InputEvent e) {
                if (e.Type == InputEventType.Press) {
                    Subscription.Dispose();
                    Future.SetResult(NoneType.None, null);
                }

                return true;
            }
        }

        public static SignalFuture WaitForPress (this InputControl control) {
            return (new WaitForPressThunk(control)).Future;
        }
    }
}
