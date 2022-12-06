using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRayCast.Scripts {
    public static class CSettings {

        // Client resolution and render settings
        public static int[] RESOLUTION = new int[] {
            1600, 900
        };
        public static int WIDTH = RESOLUTION[0];
        public static double HALF_WIDTH = (double)Math.Floor((decimal)WIDTH / 2);
        public static int HEIGHT = RESOLUTION[1];
        public static double HALF_HEIGHT = (double)Math.Floor((decimal)HEIGHT / 2);

        // player settings
        public static double PLAYER_ANGLE = 0;
        public static double PLAYER_SPEED = 0.002;
        public static double PLAYER_ROT_SPEED = 0.002;
        public static double PLAYER_SCALE = 8;
        public static double PLAYER_MOUSE_SENSITIVITY = 0.0003;

        public static double PLAYER_FOV = Math.PI / 3;
        public static double PLAYER_HALF_FOV = PLAYER_FOV / 2;
        public static double RENDER_NUM_RAYS = (double)Math.Floor((decimal)WIDTH / 2);
        public static double RENDER_HALF_NUM_RAYS = (double)Math.Floor((decimal)RENDER_NUM_RAYS / 2);
        public static double RENDER_DELTA_ANGLE = PLAYER_FOV / RENDER_NUM_RAYS;
        public static int RENDER_MAX_DEPTH = 20;

        public static double RENDER_SCREEN_DIST = HALF_WIDTH / Math.Tan(PLAYER_HALF_FOV);
        public static double RENDER_SCALE = (double)Math.Floor((decimal)(WIDTH / RENDER_NUM_RAYS));

        public static int WALL_TEXTURE_SIZE = 128;
        public static int WALL_HALF_TEXTURE_SIZE = 64;

        public static bool DEBUG = true;
    }
}
