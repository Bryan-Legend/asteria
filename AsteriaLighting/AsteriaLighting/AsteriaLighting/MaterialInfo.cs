// -----------------------------------------------------------------------
// <copyright file="MaterialInfo.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace AsteriaLighting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class MaterialInfo
    {
        public static Dictionary<Material, Entity> MaterialTypes;

        public static bool IsGas(Material material)
        {
            return !material.Solid;
        }

        public static bool IsLooseOrSolid(Material material)
        {
            return material.Solid;
        }

        static MaterialInfo()
        {
            MaterialTypes = new Dictionary<Material, Entity>();
        }
    }
}
