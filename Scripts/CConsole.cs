using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleRayCast.Scripts.Maps;
using SimpleRayCast.Scripts.Player;

namespace SimpleRayCast.Scripts {
    public class CConsole {
        RayCastGame _game;
        CPlayer _client;
        RayCastMap _map;
        public CConsole(RayCastGame game) {
            this._game = game;
            this._client = game.Player;
            this._map = game.Map;
        }

        public void RunCommand(string command) {
            switch (command) {
                case "ng":
                    this._game.NewGame();
                    break;
            }
        }
    }
}
