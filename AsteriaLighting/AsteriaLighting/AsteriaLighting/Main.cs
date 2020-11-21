using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace AsteriaLighting
{
    public static class Main
    {
        public static IntVector2 ScreenOffset
        {
            get { return new IntVector2(); }
        }

        public static int ResolutionWidth
        {
            get { return 1024; }
        }

        public static int ResolutionHeight
        {
            get { return 768; }
        }

        public static Map Map
        {
            get;
            private set;
        }

        static Main()
        {
            Map = new Map();
        }
    }

    public class Map
    {
        public int SeaLevel = 100;
        public static int BlockWidth = 8;
        public static int BlockHeight = 8;
        public List<Entity> Entities;
        public static Material solid;
        public static Material gas;
        public Material[,] cells;

        static Map()
        {
            solid = new Material() { Color = Color.White, Solid = true };
            gas = new Material() { Color = Color.Transparent, Solid = false };
        }

        public Map()
        {
            Entities = new List<Entity>();
            Entities.Add(new Entity());

            MaterialInfo.MaterialTypes.Add(solid, new Entity() { IsSolid = true });
            MaterialInfo.MaterialTypes.Add(gas, new Entity() { IsSolid = false });

            int horizontalBlocks = 1024 / BlockWidth;
            int verticalBlocks = 768 / BlockHeight;
            cells = new Material[horizontalBlocks, verticalBlocks];
        }

        public Material GetMaterialAtPixel(int x, int y)
        {
            int h = x / 1024;
            int v = y / 768;

            if (x > 400)
            {
                if (y < 300 || y > 500)
                {
                    return solid;
                }
            }

            return gas;
        }
    }

    public class Entity
    {
        public Color Light;
        public Vector2 Position;
        public bool IsSolid;

        public Entity()
        {
            Light = Color.White;
            Position = new Vector2(300, 400);
        }
    }
}
