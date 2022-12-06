using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SimpleRayCast.Scripts.Maps {
    public class RayCastMap {


        const int _ = 0;
        private int[][] _miniMap;
        private Dictionary<string, int> _worldMap;
        private Keys _displayKey;
        private bool _isDrawingMap;

        private string _name;

        private int[] _playerSpawn;
        public int[] PlayerSpawn { get { return _playerSpawn; } }

        public string Name { get { return _name; } }

        public Dictionary<string, int> WorldMap { get { return this._worldMap; } }
        public int[][] MiniMap { get { return _miniMap; } }

        public bool IsDrawingMap { get { return this._isDrawingMap; } }

        // We can keep this as a field
        public RayCastGame Game;
        public RayCastMap(RayCastGame game) {
            this.Game = game;

            this._displayKey = Keys.M;

            this._playerSpawn = new int[] { 5, 5 };

            if (this._miniMap == null) {
                this._miniMap = new int[][] {
                    new int[] { 1, 1, 1, 1, 1, 1, 1, 5, 1, 1, 1, 1, 1 },
                    new int[] { 1, _, _, _, 3, _, _, _, _, _, 1, _, 1 },
                    new int[] { 1, _, _, _, _, 4, 1, _, 1, _, _, _, 1 },
                    new int[] { 1, _, _, _, _, _, 6, _, 1, _, _, _, 1 },
                    new int[] { 1, _, _, _, _, _, _, _, 1, _, _, _, 1 },
                    new int[] { 1, 1, 1, _, _, _, _, _, 1, _, 1, 1, 1 },
                    new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
                };

                this._worldMap = new Dictionary<string, int>();
            }

            this._isDrawingMap = false;

            this._name = "unnamed";

            this.InitWorldMap();
        }
        
        private void InitWorldMap() {
            Console.Write("Initializing World => MAP={0}, SIZE={1}...", this._name, this._miniMap.Length * this._miniMap[0].Length);

            for (int y = 0; y < this._miniMap.Length; y++)
                for (int x = 0; x < this._miniMap[y].Length; x++)
                    if (this._miniMap[y][x] != 0)
                        this._worldMap.Add(
                            this.ConvertCoordsToDictionary(x, y),
                            this._miniMap[y][x]
                        );
            Console.WriteLine(" Done, FINAL_SIZE={0}", this._worldMap.Count());
        }

        public string ConvertCoordsToDictionary(int[] coords) {
            // this assumes [0] = x
            return this.ConvertCoordsToDictionary(coords[0], coords[1]);
        }
        public string ConvertCoordsToDictionary(int x, int y) {
            return string.Format("X: {0}, Y: {1}", x, y);
        }

        public int[] ConvertDictCoordsToInt(string coords) {
            Regex rx = new Regex(@"^(?:X\:\s)(\d*)(?:\,\sY\:\s)(\d*)$");

            MatchCollection matches = rx.Matches(coords);
            
            // Groups[1] will be X, Groups[2] will be Y in this regex. Groups[0] will be the whole string
            if (matches.Count > 0) {
                return new int[] {
                    Convert.ToInt32(matches[0].Groups[1].Value),
                    Convert.ToInt32(matches[0].Groups[2].Value)
                };
            }

            return new int[] { 0, 0 };
        }

        public void DrawPlayerCenterFov_Debug(SpriteBatch spriteBatch, Texture2D texture, int magnitude, int offsetX, int offsetY) {
            var origin = new Vector2(0f, 0.5f);
            var scale = new Vector2(100f, 2f);
            spriteBatch.Draw(
                texture,
                new Vector2((float)(this.Game.Player.X * magnitude + offsetX), (float)(this.Game.Player.Y * magnitude + offsetY)),
                null,
                Color.DarkGoldenrod,
                (float)this.Game.Player.Angle,
                origin,
                scale,
                SpriteEffects.None,
                0f
            );
        }

        public void Update() {
            if (this.Game.IsKeyPressed(this._displayKey)) {
                this._isDrawingMap = !this._isDrawingMap;
            }
        }

        public void DrawMapOverlay(SpriteBatch spriteBatch) {

            if (!this._isDrawingMap) return;

            

            int offsetMagnitude = 100;

            // get offset so we can draw the player in the middle
            int offsetX = (int)(CSettings.HALF_WIDTH - this.Game.Player.X * offsetMagnitude);
            int offsetY = (int)(CSettings.HALF_HEIGHT - this.Game.Player.Y * offsetMagnitude);

            Texture2D text = new Texture2D(this.Game.GraphicsDevice, 1, 1);
            text.SetData(new[] { Color.White });

            // we should clear the screen and draw a background!!!
            spriteBatch.Draw(
                text,
                new Rectangle(0, 0, CSettings.WIDTH, CSettings.HEIGHT),
                Color.CornflowerBlue
            );


            // Draw our tiles
            foreach (KeyValuePair<string, int> valuePair in this._worldMap) {
                int[] i = ConvertDictCoordsToInt(valuePair.Key);
                spriteBatch.Draw(
                    text,
                    new Rectangle(
                        i[0] * offsetMagnitude + offsetX,
                        i[1] * offsetMagnitude + offsetY,
                        100,
                        100),
                    Color.White
                );
            }

            // draw player

            int playerSize = 16;

            spriteBatch.Draw(
                text,
                new Rectangle(
                    (int)(this.Game.Player.X * offsetMagnitude + offsetX),
                    (int)(this.Game.Player.Y * offsetMagnitude + offsetY),
                    playerSize,
                    playerSize),
                null,
                Color.Yellow,
                (float)this.Game.Player.Angle,
                new Vector2(0.5f, 0.5f),
                SpriteEffects.None,
                0f
            );

            this.DrawPlayerCenterFov_Debug(spriteBatch, text, offsetMagnitude, offsetX, offsetY);
        }
    }
}
