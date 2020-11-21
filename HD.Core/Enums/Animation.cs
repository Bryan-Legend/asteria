using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace HD
{
    public enum Animation : byte
    {
        None,
        Spawn,
        Idle1,
        Idle2,
        Move,
        MoveBackwards,
        Jump,
        Land,
        Hit,
        Dead,
        Attack1,
        Attack2,
        Attack3,
        Hovering,
        LatchedOn,
        TurnLeft,
        TurnRight,
        Blocking,
        Enrage,
        Opening,
        Closing,
        Pull,
        Push,
        Off,
        On,
    }
}