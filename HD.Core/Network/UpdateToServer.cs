using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Input;
using System.IO;
using ProtoBuf;

namespace HD
{
    [DataContract]
    public class UpdateToServer
    {
        [DataMember(Order = 1)]
        public Keys[] PressedKeys { get; set; }

        [DataMember(Order = 2)]
        public int MouseX { get; set; }

        [DataMember(Order = 3)]
        public int MouseY { get; set; }

        [DataMember(Order = 4)]
        public bool MouseLeftButtonPressed { get; set; }

        [DataMember(Order = 5)]
        public bool MouseRightButtonPressed { get; set; }

        [DataMember(Order = 6)]
        public byte SelectedActionSlot { get; set; }

        [DataMember(Order = 7)]
        public byte[] GamePad { get; set; }

        [DataMember(Order = 50)]
        public int ResolutionX { get; set; }

        [DataMember(Order = 51)]
        public int ResolutionY { get; set; }

        [DataMember(Order = 52)]
        public bool Disconnect { get; set; }

        [DataMember(Order = 53)]
        public string LoginName { get; set; }
        [DataMember(Order = 54)]
        public string Password { get; set; }
        [DataMember(Order = 65)]
        public string Version { get; set; }
        [DataMember(Order = 67)]
        public int Skin { get; set; }

        [DataMember(Order = 55)]
        public SlotType DragSlotType { get; set; }
        [DataMember(Order = 56)]
        public int DragSlotNumber { get; set; }
        [DataMember(Order = 66)]
        public int DragAmount { get; set; }
        [DataMember(Order = 57)]
        public SlotType DropSlotType { get; set; }
        [DataMember(Order = 58)]
        public int DropSlotNumber { get; set; }

        [DataMember(Order = 59)]
        public bool ClearCreativeMode { get; set; }
        [DataMember(Order = 60)]
        public bool ClearActivePlaceable { get; set; }

        [DataMember(Order = 61)]
        public int CraftRecipe { get; set; }
        [DataMember(Order = 62)]
        public int CraftAmount { get; set; }

        [DataMember(Order = 63)]
        public bool SortInventory { get; set; }

        [DataMember(Order = 64)]
        public string SetValue { get; set; }

        [DataMember(Order = 68)]
        public bool ToggleGodMode { get; set; }
        [DataMember(Order = 69)]
        public CreativeMode? SetCreativeMode { get; set; }

        [DataMember(Order = 70)]
        public bool ReturnToOverworld { get; set; }

        [DataMember(Order = 100)]
        public string[] Messages { get; set; }
    }
}
