using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HD.Asteria;

// Global class must not be in a namespace.
public static class Global
{
    public static void Register()
    {
        Projectiles.RegisterProjectiles();
        Items.RegisterItems();
        Enemies.RegisterEnemies();
    }
}
