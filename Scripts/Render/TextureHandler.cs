using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleRayCast.Scripts.Render {
    public class TextureHandler {
        RayCastGame _game;
        public RayCastGame Game { get { return this._game; } }

        ContentManager _content;
        public ContentManager Content { get { return this._content; } }


        // We should have a better way to update this list later...
        readonly string[] _textureList = new string[]{
            "walls/brickwall_1",
            "walls/brickwall_2",
            "walls/brickwall_3",
            "walls/brickwall_4",
            "walls/brickwall_5",
            "walls/brickwall_6",
            "viewmodels/viewmodel1",
            "viewmodels/scope_overlay",
            "viewmodels/muzzle_flash_1",
        };
        readonly string _texturePath = "textures/";


        Dictionary<string, Texture2D> _textures;
        public Dictionary<string, Texture2D> Textures { get { return _textures; } }
        public TextureHandler(RayCastGame game, ContentManager content) {
            this._game = game;
            this._content = content;
            this._textures = new Dictionary<string, Texture2D>();
        }

        public void InitializeTextures() {
            foreach (string s in _textureList) {
                this._textures.Add(
                    s,
                    this._content.Load<Texture2D>(this._texturePath + s)
                );
            }
        }

        public Texture2D GetTexture(string name) {
            return this._textures[name];
        }
    }
}
