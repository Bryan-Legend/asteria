using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace HD
{
    public enum Sound
    {
        None,
        Tick, // menus
        SmallTick, // drag and drop
        Furnace,
        Craft,
        Equip,
        Equip2,
        PlaceItem,
        Pickup,
        Dematerialize,
        PlayerSpawn,
        PlayerRun,
        PlayerJump,
        PlayerLand,
        PlayerDamaged,
        PlayerDie,
        EnterWater,
        ExitWater,
        EnterLava,
        ExitLava,
        EnterSlime,
        ExitSlime,
        EnterAcid,
        ExitAcid,
        EnterTar,
        ExitTar,
        PlayerSwim,
        DigSmall,
        DigMedium, // NYI
        DigLarge, // NYI
        GunFlameThrower,
        GunFreeze,
        Explosion1, // NYI
        Explosion2, // NYI
        Explosion3,
        Explosion4, // NYI
        Explosion5, // NYI
        EnemySpawn,
        EnemyAttack,
        EnemyDamage,
        EnemyDie,
        ExploderWarning,
        Portal,
        HypercubeOfHoldingOpen,
        HypercubeOfHoldingLocked,
        MiningToolFire,
        WeaponMissed,
        BlasterFire,
        BlasterHit,
        HomingMissileFire,
        HomingMissileHit,
        LaserRifleFire,
        LaserRifleHit,
        DisruptorFire,
        DisruptorHit,
        WaypointActivated,
        Error,
        HoverTick,
        DoorClosed,
        DoorOpen,
        Switch,
        TimedSwitchActive,
        PlaceMaterial,
        PlayerDamaged2,
        PlayerDamaged3,
        ValeeHiveSpawn,
        WorkBench,
        Forge,
        ChemicalReactor,
        SpaceShipBlaster,
        MaterialSourceOn,
        MaterialSourceOff,
        BoseEinsteinCondenserFire,
        BoseEinsteinCondenserHit,
        GrenadeFire,
        Spike,
        FireBreath,
        AcidSplat,
        Static,
        ElectricityHit,
        BomberWarning,
        BlasterCharge,
        BlasterChargedFire,
        TurretDeactivate,
        TurretActivate,
        MiniFlame,
        PickupUseable,
        PickupKeyComponet,
    }
}