using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SimpleRayCast.Scripts;
using SimpleRayCast.Scripts.Maps;
using SimpleRayCast.Scripts.Player;
using SimpleRayCast.Scripts.Render;

namespace SimpleRayCast {
    public class RayCastGame : Game {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private double _deltaTime;
        public double DeltaTime {
            get {
                return _deltaTime;
            }
        }
        public int FPS {
            get {
                // we don't want a divide by 0 error here.
                double delta = this._deltaTime == 0 ? 1 : this._deltaTime;
                return (int)(1000 / delta);
            }
        }

        private KeyboardState _kState;
        private KeyboardState _kStateNow;

        private MouseState _mState;
        private MouseState _mStateNow;

        public RayCastMap Map;
        public CPlayer Player;
        public RayCaster RayCast;
        public TextureHandler TextureHandler;
        public CConsole Console;

        public RayCastGame() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        public void NewGame() {
            this.Map = new RayCastMap(this);
            this.Player = new CPlayer(this);
            this.RayCast = new RayCaster(this);
        }

        protected override void Initialize() {

            // init resolution settings
            this._graphics.PreferredBackBufferWidth = CSettings.WIDTH;
            this._graphics.PreferredBackBufferHeight= CSettings.HEIGHT;

            // use fps cap?
            this.IsFixedTimeStep = false;

            // if so, target for ~60 fps
            this.TargetElapsedTime = TimeSpan.FromMilliseconds(16);
            this._graphics.ApplyChanges();

            this.Console = new CConsole(this);
            this.NewGame();

            this.TextureHandler = new TextureHandler(this, this.Content);

            // init this last
            base.Initialize();
        }

        protected override void LoadContent() {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            this.TextureHandler.InitializeTextures();
        }

        protected override void Update(GameTime gameTime) {

            this._kStateNow = Keyboard.GetState();
            this._mStateNow = Mouse.GetState();

            // Update our title for displaying current fps value
            this.Window.Title = string.Format("RayCast Game; FPS: {0}", this.FPS.ToString());

            this.Map.Update();

            this.RayCast.Update();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            this.Player.Update();

            this._deltaTime = gameTime.ElapsedGameTime.TotalMilliseconds;
            this._kState = this._kStateNow;
            this._mState = this._mStateNow;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            //GraphicsDevice.Clear(Color.CornflowerBlue);

            

            this._spriteBatch.Begin();

            if (this.Map.IsDrawingMap) {
                this.Map.DrawMapOverlay(this._spriteBatch);

                // we need to 'draw' this to avoid a mem leak in debug mode
                // todo: we should fix this and clamp the array size regardless of drawing
                this.RayCast.Draw(this._spriteBatch);
                this._spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

            // floor
            Texture2D floorTexture = new Texture2D(this.GraphicsDevice, 1, 1);
            floorTexture.SetData(new Color[] { Color.LightBlue });
            this._spriteBatch.Draw(floorTexture, new Rectangle(0, (int)CSettings.HALF_HEIGHT, CSettings.WIDTH, (int)CSettings.HALF_HEIGHT), Color.White);


            // sky
            this._spriteBatch.Draw(floorTexture, new Rectangle(0, 0, CSettings.WIDTH, (int)CSettings.HALF_HEIGHT), Color.DarkRed);
            
            // raycast, and map if overlay is being drawn
            this.RayCast.Draw(this._spriteBatch);
            this.Map.DrawMapOverlay(this._spriteBatch);

            this.Player.DrawViewmodel(this._spriteBatch);

            this._spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        public bool IsKeyHeld(Keys key) {
            return this._kStateNow.GetPressedKeys().Contains<Keys>(key);
        }

        public bool IsKeyPressed(Keys key) {
            return this._kStateNow.GetPressedKeys().Contains<Keys>(key) &&
                !this._kState.GetPressedKeys().Contains<Keys>(key);
        }

        public bool IsLeftMousePressed() {
            return this._mStateNow.LeftButton == ButtonState.Pressed &&
                !(this._mState.LeftButton == ButtonState.Pressed);
        }

        public bool IsLeftMouseHeld() {
            return this._mStateNow.LeftButton == ButtonState.Pressed;
        }
        public MouseState GetCurrentMouseState() {
            return this._mStateNow;
        }

        public void SetMousePos (int x, int y) {
            Mouse.SetPosition(x, y);
            return;
        }
    }
}