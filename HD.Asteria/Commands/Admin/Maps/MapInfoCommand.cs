using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace HD
{
    public class MapInfoCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapInfo: Gives detailed map info."; }
        }

        public override string Execute(Player player, string args)
        {
            var result = new StringBuilder();
            var map = player.Map;

            result.AppendLine(map.Name + " Info");
            result.AppendLine("Width: " + map.Width);
            result.AppendLine("Height: " + map.Height);
            result.AppendLine("Pixel Width: " + map.PixelWidth);
            result.AppendLine("Pixel Height: " + map.PixelHeight);
            result.AppendLine("Extra Lives: " + map.ExtraLives);
            result.AppendLine("Sea Level: " + map.SeaLevel);
            result.AppendLine("Lava Level: " + map.LavaLevel);
            result.AppendLine("Ambient Light: " + map.AmbientLight);
            result.AppendLine();
            result.AppendLine("Music: " + map.Music);
            result.AppendLine();
            result.AppendLine("Extra Lives: " + map.ExtraLives);
            result.AppendLine("Tier: " + map.Tier);
            result.AppendLine("Gravity: " + map.Gravity);
            result.AppendLine("Auto Spawn: " + map.IsAutospawn);
            result.AppendLine("Reset On Die: " + map.IsResetOnDie);
            result.AppendLine("Reset On Leave: " + map.IsResetOnLeave);
            result.AppendLine("Terrain Locked: " + map.IsTerrainLocked);
            result.AppendLine("Terrain Lock Tier (Includes Enemy and Placeables): " + (map.LockTier == 0 ? "Disabled" : map.LockTier.ToString()));
            result.AppendLine("Despawn Out Of Range Enemies: " + map.DespawnOutOfRangeEnemies);
            result.AppendLine();
            result.AppendLine("Player Count: " + map.Players.Count);
            result.AppendLine("Entity Count: " + map.Entities.Count);
            result.AppendLine("Spawn Count: " + map.Spawns.Count);
            result.AppendLine("Next Entity Id: " + map.NextEntityId);

            result.AppendLine();
            result.AppendLine("Spawned Enemies");
            foreach (var entity in map.Entities) {
                //                result.AppendLine(String.Format("{0} {1:N0}px away", entity.ToString(), (player.Position - entity.Position).Length()));

                var enemy = entity as Enemy;
                if (enemy != null) {
                    result.AppendLine(String.Format("{0} {1:N0}px away", enemy.Type.Name, (player.Position - enemy.Position).Length()));
                }
            }

            return result.ToString();
        }
    }
}