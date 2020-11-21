using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace HD
{
    // Be sure to edit ItemId.cs to maintain a complete set of these as well.
    // The order of these is very important, it's used for sorting by weight as well as material type.
    public enum Material : byte
    {
        None,

        // gasses
        ReservedGas1,
        ReservedGas2,
        ReservedGas3,
        ReservedGas4,
        ReservedGas5,
        ReservedGas6,
        ReservedGas7,
        ReservedGas8,
        ReservedGas9,
        ReservedGas10,
        ReservedGas11,
        ReservedGas12,
        Air,
        Steam,
        Smoke,
        NaturalGas,
        PoisonGas,
        ReservedGas13,
        ReservedGas14,
        ReservedGas15,
        ReservedGas16,
        ReservedGas17,
        ReservedGas18,
        ReservedGas19,
        ReservedGas20,
        Vacuum,

        // liquids
        Fire,
        BlueFire,
        ReservedLiquid2,
        ReservedLiquid3,
        ReservedLiquid4,
        ReservedLiquid5,
        ReservedLiquid6,
        ReservedLiquid7,
        ReservedLiquid8,
        ReservedLiquid9,
        ReservedLiquid10,
        Oil,
        Slime,
        Water,
        Acid,
        Lava,
        Tar,
        ReservedLiquid11,
        ReservedLiquid12,
        ReservedLiquid13,
        ReservedLiquid14,
        ReservedLiquid15,
        ReservedLiquid16,
        ReservedLiquid17,
        ReservedLiquid18,
        ReservedLiquid19,
        ReservedLiquid20,

        // loose
        ReservedLoose1,
        ReservedLoose2,
        ReservedLoose3,
        ReservedLoose4,
        ReservedLoose5,
        ReservedLoose6,
        ReservedLoose7,
        ReservedLoose8,
        ReservedLoose9,
        ReservedLoose10,
        Snow,
        Sand,
        Gravel,
        Ash,
        ReservedLoose11,
        ReservedLoose12,
        ReservedLoose13,
        ReservedLoose14,
        ReservedLoose15,
        ReservedLoose16,
        ReservedLoose17,
        ReservedLoose18,
        ReservedLoose19,

        // solid
        Platform,
        Dirt,
        Clay,
        Sandstone,
        Limestone,
        Quartzite,
        Granite,
        Marble,
        Rhyolite,
        Basalt,

        IronOre,
        AluminumOre,
        CopperOre,
        GoldOre,
        SilverOre,
        UraniumOre,
        RadiumOre,

        Diamond,
        Ruby,
        Emerald,
        Sapphire,
        Topaz,
        Amethyst,

        Obsidian,
        Dilithium,

        Ice,
        Spike,

        Life,

        Coal,
        Sulfur,
        Sodium,

        // http://web.media.mit.edu/~wad/color/palette.html
        BlackBrick,     // 0, 0, 0
        DarkGrayBrick,  // 87, 87, 87
        LightGrayBrick, // 160, 160, 160
        WhiteBrick,     // 255, 255, 255
        BlueBrick,      // 42, 75, 215
        LightBlueBrick, // 157, 175, 255
        CyanBrick,      // 41, 208, 208
        GreenBrick,     // 29, 105, 20
        LightGreenBrick, // 129, 197, 122
        YellowBrick,    // 255, 238, 51
        BrownBrick,     // 129, 74, 25
        OrangeBrick,    // 255, 146, 51
        TanBrick,       // 233, 222, 187
        RedBrick,       // 173, 35, 35
        PurpleBrick,    // 129, 38, 192
        PinkBrick,      // 255, 205, 243

        ReservedSolid1,
        ReservedSolid2,
        ReservedSolid3,
        ReservedSolid4,
        ReservedSolid5,
        ReservedSolid6,
        ReservedSolid7,
        ReservedSolid8,
        ReservedSolid9,
        ReservedSolid10,
        ReservedSolid11,
        ReservedSolid12,
        ReservedSolid13,
        ReservedSolid14,
        ReservedSolid15,
        ReservedSolid16,
        ReservedSolid17,
        ReservedSolid18,
        ReservedSolid19,
        ReservedSolid20,
        ReservedSolid31,
        ReservedSolid32,
        ReservedSolid33,
        ReservedSolid34,
        ReservedSolid35,
        ReservedSolid36,
        ReservedSolid37,
        ReservedSolid38,
        ReservedSolid39,
        ReservedSolid40,
        ReservedSolid41,
        ReservedSolid42,
        ReservedSolid43,
        ReservedSolid44,
        ReservedSolid45,
        ReservedSolid46,
        ReservedSolid47,
        ReservedSolid48,
        ReservedSolid49,
        ReservedSolid50,
        ReservedSolid51,
        ReservedSolid52,
        ReservedSolid53,
        ReservedSolid54,
        ReservedSolid55,
        ReservedSolid56,
        ReservedSolid57,
        ReservedSolid58,
        ReservedSolid59,
        ReservedSolid60,
        ReservedSolid61,
        ReservedSolid62,
        ReservedSolid63,
        ReservedSolid64,
        ReservedSolid65,
        ReservedSolid66,
        ReservedSolid67,
        ReservedSolid68,
        ReservedSolid69,
        ReservedSolid70,
        ReservedSolid71,
        ReservedSolid72,
        ReservedSolid73,
        ReservedSolid74,
        ReservedSolid75,
        ReservedSolid76,
        ReservedSolid77,
        ReservedSolid78,
        ReservedSolid79,

        Bone,
        Wood,
        GrayMetal,
        Plastic,
        Pipe,
        Wire,
        Glass,

        BlackMetal,

        RedMetal,

        WhiteMetal,
        DarkGrayMetal,
        ReservedSolid84,
        ReservedSolid85,
        ReservedSolid86,
        ReservedSolid87,
        ReservedSolid88,
        ReservedSolid89,
        ReservedSolid90,

        Grass,

        Boundry, // bountry must be last.
    }
}