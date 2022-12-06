using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SimpleRayCast.Scripts.Render {
    public class RayCaster {


        private double[][][] rayPoints;
        WallDrawInfo[] _wallsToDraw;
        public WallDrawInfo[] WallsToDraw { get { return this._wallsToDraw; } }

        public struct WallDrawInfo {
            public Rectangle rect;
            public double depth;
            public double offset;
            public Rectangle sourceRect;
            public int texture;
        }

        public RayCastGame Game;
        public RayCaster(RayCastGame game) {
            this.Game = game;
            this.rayPoints = new double[0][][];
            this._wallsToDraw = new WallDrawInfo[0];
        }

        private void RayCast(bool debug = false) {

            double playerX = this.Game.Player.X;
            double playerY = this.Game.Player.Y;
            int playerMapX = this.Game.Player.MapPosition[0];
            int playerMapY = this.Game.Player.MapPosition[1];

            // we want to avoid a divide by zero error here, add a small value
            double rayAngle = this.Game.Player.Angle - CSettings.PLAYER_HALF_FOV + 0.0001;


            // loop thru the number of rays that are being cast

            double startingFov = this.Game.Player.IsScoped ? CSettings.RENDER_NUM_RAYS / 4 : 0;
            double endingFov = this.Game.Player.IsScoped ? CSettings.RENDER_NUM_RAYS * 3 / 4 : CSettings.RENDER_NUM_RAYS;

            int ray = (int)startingFov;

            if (this.Game.Player.IsScoped) {
                rayAngle += CSettings.RENDER_DELTA_ANGLE * ray;
            }

            for (; ray < endingFov; ray++) {
                double sinAngle = Math.Sin(rayAngle);
                double cosAngle = Math.Cos(rayAngle);

                // horizontals
                double yHor;
                double deltaY;
                if (sinAngle > 0) {
                    yHor = playerMapY + 1;
                    deltaY = 1;
                } else {
                    yHor = playerMapY - 0.00001;
                    deltaY = -1;
                }
                double depthHor = (yHor - playerY) / sinAngle;
                double xHor = playerX + depthHor * cosAngle;

                double deltaDepth = deltaY / sinAngle;
                double deltaX = deltaDepth * cosAngle;

                // calculate total value of this ray depth
                // set to 1 to keep from texture errors
                int tileHorValue = 1;
                for (int i = 0; i < CSettings.RENDER_MAX_DEPTH; i++) {
                    int[] tileHor = new int[] { (int)xHor, (int)yHor };

                    string key = this.Game.Map.ConvertCoordsToDictionary(tileHor);

                    if (this.Game.Map.WorldMap.ContainsKey(key)) {
                        tileHorValue = this.Game.Map.WorldMap[key];
                        break;
                    };
                    xHor += deltaX;
                    yHor += deltaY;
                    depthHor += deltaDepth;
                }


                // verticals
                double xVert;
                if (cosAngle > 0) {
                    xVert = playerMapX + 1;
                    deltaX = 1;
                } else {
                    xVert = playerMapX - 0.00001;
                    deltaX = -1;
                }

                double depthVert = (xVert - playerX) / cosAngle;
                double yVert = playerY + depthVert * sinAngle;

                deltaDepth = deltaX / cosAngle;
                deltaY = deltaDepth * sinAngle;

                // calculate total value of this ray depth
                int tileVertValue = 1;
                for (int i = 0; i < CSettings.RENDER_MAX_DEPTH; i++) {
                    int[] tileVert = new int[] { (int)xVert, (int)yVert };

                    string key = this.Game.Map.ConvertCoordsToDictionary(tileVert);

                    // check if this tile is wall
                    if (this.Game.Map.WorldMap.ContainsKey(key)) {
                        tileVertValue = this.Game.Map.WorldMap[key];
                        break;
                    }

                    xVert += deltaX;
                    yVert += deltaY;
                    depthVert += deltaDepth;
                }


                //depth we need
                double depth;
                double offset;
                int textureValue = 1;
                if (depthVert < depthHor) {
                    textureValue = tileVertValue;
                    depth = depthVert;
                    yVert %= 1;
                    offset = cosAngle > 0 ? yVert : 1 - yVert;
                } else {
                    textureValue = tileHorValue;
                    depth = depthHor;
                    xHor %= 1;
                    offset = sinAngle > 0 ? 1 - xHor : xHor;
                }

                // add line to draw for debug
                if (ray  == 0 ||
                    ray + 2 > CSettings.RENDER_NUM_RAYS &&
                    debug) {
                    this.rayPoints = this.rayPoints.Append<double[][]>(
                        new double[][] {
                            new double[] {
                                100 * playerX,
                                100 * playerY,
                            },
                            new double[] {
                                100 * playerX + 100 * depth * cosAngle,
                                100 * playerY + 100 * depth * sinAngle
                            }
                        }
                    ).ToArray();
                }


                // remove a fishbowl effect

                // if we are looking thru a scope, we can add a magnitude to this effect
                int scopeMagnitude = 2;
                if (!this.Game.Player.IsScoped)
                    depth *= Math.Cos(this.Game.Player.Angle - rayAngle);
                else
                    depth /= Math.Cos(this.Game.Player.Angle - rayAngle) * scopeMagnitude;

                // get "3d" projection
                double projectionHeight = CSettings.RENDER_SCREEN_DIST / (depth + 0.0001);

                // create a wall draw struct object (render details)
                double x = ray * CSettings.RENDER_SCALE;
                double y = CSettings.HALF_HEIGHT - (double)(Math.Floor((decimal)(projectionHeight / 2)));
                double width = CSettings.RENDER_SCALE;
                double height = projectionHeight;
                
                WallDrawInfo rd = new WallDrawInfo();
                rd.rect = new Rectangle((int)x, (int)y, (int)width, (int)height);
                rd.depth = depth;
                rd.offset = offset;
                rd.texture = textureValue;
                rd.sourceRect = new Rectangle(
                    (int)(rd.offset * (CSettings.WALL_TEXTURE_SIZE - CSettings.RENDER_SCALE)),
                    0,
                    (int)CSettings.RENDER_SCALE,
                    CSettings.WALL_TEXTURE_SIZE
                );

                this._wallsToDraw = this._wallsToDraw.Append(rd).ToArray();
                
                rayAngle += CSettings.RENDER_DELTA_ANGLE;
            }
        }

        public void Update() {
            this.RayCast(CSettings.DEBUG);
        }

        public void Draw(SpriteBatch spriteBatch) {
            if (this.Game.Map.IsDrawingMap) {
                
                // this needs to be moved to raycastmap

                /*foreach (double[][] points in this.rayPoints) {
                    var playerPos = points[0];
                    var lineEndPos = points[1];

                    this.DrawPlayerLine(spriteBatch, new Vector2((float)playerPos[0], (float)playerPos[1]), new Vector2((float)lineEndPos[0], (float)lineEndPos[1]));
                }*/
            } else {
                // walls

                //Texture2D wallText = new Texture2D(this.Game.GraphicsDevice, 1, 1);
                //Color wallColor = new Color(169, 169, 169);
                //wallText.SetData(new Color[] { wallColor });
                foreach (WallDrawInfo rd in this._wallsToDraw) {
                    float distCalc = (float)(10 / (rd.depth * 5));
                    Color distColor = new Color(distCalc, distCalc, distCalc);

                    string textureString = string.Format("walls/brickwall_{0}", rd.texture);
                    spriteBatch.Draw(
                        //wallText,
                        this.Game.TextureHandler.GetTexture(textureString),
                        rd.rect,
                        rd.sourceRect,
                        distColor
                    );
                }
            }
            // always clear these
            this._wallsToDraw = new WallDrawInfo[0];
            this.rayPoints = new double[0][][];
        }

        private void DrawPlayerLine(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2) {


            Texture2D text = new Texture2D(this.Game.GraphicsDevice, 1, 1);
            text.SetData(new Color[] { Color.CornflowerBlue });

            var distance = Vector2.Distance(point1, point2);
            var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            this.DrawPlayerFov_Debug(spriteBatch, text, point1, distance, angle);
        }

        public void DrawPlayerFov_Debug(SpriteBatch spriteBatch, Texture2D texture, Vector2 point, float length, float angle, float thickness = 2f) {
            var origin = new Vector2(0f, 0.5f);
            var scale = new Vector2(length, thickness);
            spriteBatch.Draw(
                texture,
                point,
                null,
                Color.White,
                angle,
                origin,
                scale,
                SpriteEffects.None,
                0f
            );
        }
    }
}
