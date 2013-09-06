using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HD;
using Microsoft.Xna.Framework;

// Global class must not be in a namespace.
public static class Global
{
    public static void Register()
    {
        EnemyBase.AddItem(new EnemyType {
            Id = 3000,
            Name = "Proximity Mine",
            SpriteWidth = 20,
            SpriteHeight = 20,
            SpriteFramesPerRow = 2,
            MaxHealth = 1000,
            Defense = Double.MaxValue,
            IsAutoSpawn = true,
            BloodColor = Color.Black,
            OnSpawn = (enemy, tier) => {
                enemy.SetAnimation(Animation.Idle1);
            },
            OnLongThink = (enemy) => {
                if (enemy.Target != null && enemy.IsTargetCloserThan(300)) {
                    enemy.PlayingSound = Sound.ExploderWarning;
                    enemy.SetAnimation(Animation.Attack1);
                    enemy.Delay(1000, () => {
                        enemy.Map.Explode(enemy.Position, 100);
                        enemy.Remove();
                    });
                }
            },
            GetFrame = (animation, age) => {
                switch (animation) {
                    case Animation.Attack1:
                        return age / 50 % 2;
                    default:
                        return age / 1000 % 2;
                }
            },
        });
    }
}
