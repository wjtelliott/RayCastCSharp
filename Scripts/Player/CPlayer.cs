using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SimpleRayCast.Scripts.Player {
    public class CPlayer {

        private double _x;
        private double _y;
        private double _angle;

        private Keys _forward;
        private Keys _back;
        private Keys _left;
        private Keys _right;
        private Keys _lookLeft;
        private Keys _lookRight;
        private Keys _useScope;

        private double tempFiringSpeed = 120;
        private bool shoot = false;

        struct MouseRel {
            public int X;
            public int Y;
            public int XNow;
            public int YNow;

            public int GetDistance() {
                return this.XNow - this.X;
            }

            public void ResetVals(int x, int y) {
                this.X = this.XNow = x;
                this.Y = this.YNow = y;
            }
        }

        private bool _holdMouse;
        private MouseRel _mouseRel;

        public double[] Position {
            get { return new double[] { this._x, this._y }; }
            private set { this._x = value[0]; this._y = value[1]; }
        }
        public int[] MapPosition {
            get {
                return new int[] { (int)(this._x), (int)(this._y) };
            }
            private set {
                // We will just set this based on our double POS prop
                this.Position = value.Select(intVal => Convert.ToDouble(intVal)).ToArray();
            }
        }
        
        public double X { get { return this._x; } }
        public double Y { get { return this._y; } }
        public double Angle { get { return this._angle; } }

        private bool _isScoped = false;
        public bool IsScoped { get { return this._isScoped; } }

        public RayCastGame Game;
        public CPlayer(RayCastGame Game) {
            this.Game = Game;
            this.MapPosition = this.Game.Map.PlayerSpawn;
            this._angle = CSettings.PLAYER_ANGLE;

            this._forward = Keys.W;
            this._back = Keys.S;
            this._left = Keys.A;
            this._right = Keys.D;

            this._lookLeft = Keys.Left;
            this._lookRight = Keys.Right;

            this._holdMouse = false;

            this._mouseRel = new MouseRel();

            this._useScope = Keys.Z;
        }

        private bool WallExists(int x, int y) {
            return
                this.Game.Map.WorldMap.ContainsKey(
                    this.Game.Map.ConvertCoordsToDictionary(x, y)
                );
        }

        private void CheckWallCollision(double deltaX, double deltaY) {
            if (!this.WallExists((int)(this._x + deltaX * CSettings.PLAYER_SCALE), (int)this._y))
                this._x += deltaX;
            if (!this.WallExists((int)(this._x), (int)(this._y + deltaY * CSettings.PLAYER_SCALE)))
                this._y += deltaY;
        }

        public void Movement() {
            double sinAngle = Math.Sin(this._angle);
            double cosAngle = Math.Cos(this._angle);

            double deltaX = 0, deltaY = 0;

            double speed = CSettings.PLAYER_SPEED * this.Game.DeltaTime;
            double speedSin = speed * sinAngle;
            double speedCos = speed * cosAngle;

            if (this.Game.IsKeyHeld(this._forward)) {
                deltaX += speedCos;
                deltaY += speedSin;
            }
            if (this.Game.IsKeyHeld(this._back)) {
                deltaX += -speedCos;
                deltaY += -speedSin;
            }
            if (this.Game.IsKeyHeld(this._left)) {
                deltaX += speedSin;
                deltaY += -speedCos;
            }
            if (this.Game.IsKeyHeld(this._right)) {
                deltaX += -speedSin;
                deltaY += speedCos;
            }

            this.CheckWallCollision(deltaX, deltaY);

            if (this.Game.IsKeyHeld(this._lookLeft)) {
                this._angle -= CSettings.PLAYER_ROT_SPEED * this.Game.DeltaTime;
            }
            if (this.Game.IsKeyHeld(this._lookRight)) {
                this._angle += CSettings.PLAYER_ROT_SPEED * this.Game.DeltaTime;
            }
            this._angle %= Math.PI* 2;
        }

        public void Update() {
            this.Movement();
            this.MouseMovement();
            this.ShootWeapon();
            this._isScoped = this.Game.IsKeyHeld(this._useScope);

            if (this.Game.IsKeyPressed(Keys.O)) this.Game.Console.RunCommand("ng");
        }

        void ShootWeapon() {
            int maxShoot = 120;

            if (this.tempFiringSpeed < maxShoot / 2) this.shoot = false;
            this.tempFiringSpeed = Math.Max(0, this.tempFiringSpeed - this.Game.DeltaTime);

            if (this.tempFiringSpeed > 0 || !this.Game.IsLeftMouseHeld()) return;
            this.shoot = true;
            this.tempFiringSpeed = maxShoot;
        }

        public void MouseMovement() {

            if (this.Game.IsKeyPressed(Keys.P)) {
                this._holdMouse = !this._holdMouse;
            }

            this._mouseRel.XNow = this.Game.GetCurrentMouseState().X;
            this._mouseRel.YNow = this.Game.GetCurrentMouseState().Y;

            int dist = this._mouseRel.GetDistance();
            this._angle += dist * this.Game.DeltaTime * CSettings.PLAYER_MOUSE_SENSITIVITY;

            this._mouseRel.X = this._mouseRel.XNow;
            this._mouseRel.Y = this._mouseRel.YNow;

            if (this._holdMouse) {
                // lock mouse to about 1/3 screen size
                int topBound = (int)CSettings.HEIGHT / 3;
                int bottomBound = (int)CSettings.HEIGHT * 2 / 3;
                int leftBound = (int)CSettings.WIDTH / 3;
                int rightBound = (int)CSettings.WIDTH * 2 / 3;

                if (this._mouseRel.YNow < topBound ||
                    this._mouseRel.YNow > bottomBound ||
                    this._mouseRel.XNow > rightBound ||
                    this._mouseRel.XNow < leftBound) {

                    // recenter & reset vals
                    int centerX = CSettings.WIDTH / 2;
                    int centerY = CSettings.HEIGHT / 2;
                    this.Game.SetMousePos(centerX, centerY);
                    this._mouseRel.ResetVals(centerX, centerY);
                }
            }
        }

        public void DrawViewmodel(SpriteBatch spriteBatch) {
            //weapon stuff will go here, for now its a magic number

            //check if we are scoped in
            if (this.IsScoped) {
                Texture2D scopeTexture = this.Game.TextureHandler.GetTexture("viewmodels/scope_overlay");
                Rectangle scopeDest = new Rectangle(
                    0,
                    0,
                    (int)CSettings.WIDTH,
                    (int)CSettings.HEIGHT
                );
                spriteBatch.Draw(scopeTexture, scopeDest, Color.White);
                return;
            }

            if (this.shoot) {
                Texture2D muzzle = this.Game.TextureHandler.GetTexture("viewmodels/muzzle_flash_1");
                Rectangle muzzleDest = new Rectangle(
                    // eventually we will have weapons that will mark their 'origins' and such for muzzle flashes as props
                    (int)(CSettings.HALF_WIDTH - muzzle.Width / 4),
                    (int)(CSettings.HEIGHT - muzzle.Height - 210),
                    muzzle.Width,
                    muzzle.Height
                );
                spriteBatch.Draw(
                    muzzle,
                    muzzleDest,
                    Color.White
                );
            }

            Texture2D viewmodelTexture = this.Game.TextureHandler.GetTexture("viewmodels/viewmodel1");
            Rectangle destRect = new Rectangle(
                (int)(CSettings.HALF_WIDTH - viewmodelTexture.Width / 3),
                (int)(CSettings.HEIGHT - viewmodelTexture.Height + 140),
                viewmodelTexture.Width,
                viewmodelTexture.Height
            );

            spriteBatch.Draw(
                viewmodelTexture,
                destRect,
                Color.White
            );
        }
    }
}
