using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HD
{
    public static class ProjectileBase
    {
        public static List<ProjectileType> ProjectileTypes;
        public static Dictionary<ProjectileId, ProjectileType> ProjectileTypesById;

        static ProjectileBase()
        {
            ProjectileTypes = new List<ProjectileType>();
            ProjectileTypesById = new Dictionary<ProjectileId, ProjectileType>();
        }

        public static void AddType(ProjectileType type)
        {
            if (type.Name == null)
                type.Name = type.Id.ToString();
            ProjectileTypesById[type.Id] = type;
            ProjectileTypes.Add(type);
        }

        public static ProjectileType Get(ProjectileId id)
        {
            return ProjectileTypesById[id];
        }

        public static void LoadContent(GraphicsDevice device)
        {
            Utility.LogMessage("Loading projectile textures.");

            foreach (var type in ProjectileTypes)
            {
                type.Texture = Utility.LoadTexture(device, String.Format("Projectiles/{0}.png", type.TechName ?? type.Name));

                if (type.UseTierTextures)
                {
                    type.TierTextures = new Texture2D[9];
                    type.TierTextures[0] = Utility.LoadTexture(device, String.Format("Projectiles/{0} {1}.png", type.TechName ?? type.Name, Utility.GetTierName(1)));
                    type.TierTextures[1] = Utility.LoadTexture(device, String.Format("Projectiles/{0} {1}.png", type.TechName ?? type.Name, Utility.GetTierName(2)));
                    type.TierTextures[2] = Utility.LoadTexture(device, String.Format("Projectiles/{0} {1}.png", type.TechName ?? type.Name, Utility.GetTierName(3)));
                    type.TierTextures[3] = Utility.LoadTexture(device, String.Format("Projectiles/{0} {1}.png", type.TechName ?? type.Name, Utility.GetTierName(4)));
                    type.TierTextures[4] = Utility.LoadTexture(device, String.Format("Projectiles/{0} {1}.png", type.TechName ?? type.Name, Utility.GetTierName(5)));
                    type.TierTextures[5] = Utility.LoadTexture(device, String.Format("Projectiles/{0} {1}.png", type.TechName ?? type.Name, Utility.GetTierName(6)));
                    type.TierTextures[6] = Utility.LoadTexture(device, String.Format("Projectiles/{0} {1}.png", type.TechName ?? type.Name, Utility.GetTierName(7)));
                    type.TierTextures[7] = Utility.LoadTexture(device, String.Format("Projectiles/{0} {1}.png", type.TechName ?? type.Name, Utility.GetTierName(8)));
                    type.TierTextures[8] = Utility.LoadTexture(device, String.Format("Projectiles/{0} {1}.png", type.TechName ?? type.Name, Utility.GetTierName(9)));
                }
            }

            //if (watcher == null)
            //{
            //    watcher = new FileSystemWatcher(Path.Combine(Utility.ContentDirectory, "Items"));
            //    watcher.Created += (sender, e) => { LoadContent(device); };
            //    watcher.Changed += (sender, e) => { LoadContent(device); };
            //    watcher.EnableRaisingEvents = true;
            //}
        }
    }
}