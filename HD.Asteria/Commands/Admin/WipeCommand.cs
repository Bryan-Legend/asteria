using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class WipeCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/Wipe: Resets everything about the current character to start."; }
        }

        public override string Execute(Player player, string args)
        {
            if (player == null)
                return "No character";

            using (var database = Server.CreateDBConnection())
            {
                CharacterStore.WipeCharacter(database, player.Character.Id);

                Hashtable row;
                player.Character = CharacterStore.Load(database, player.Character.Id, out row);
            }

            //player.LogOff();
        
            //source.Character.Level = 1;
            //source.Character.Money = 0;
            //source.Character.Inventory.Clear();

            //CharacterStore.Save(source.Character, player.ZoneServer.Id, 0, 0, null);

            //foreach (var quest in source.Character.Quests)
            //{
            //    CharacterStore.RemoveQuest(source.Character, quest, false);
            //}

            //CharacterStore.ClearQuestHistory(source.Character.Id);

            //using (var db = Server.CreateDBConnection())
            //{
            //    CharacterStore.InitalizeInventory(db, source.Character.Class, source.Character.Id);
            //    CharacterStore.InitalizeCounters(db, source.Character.Class, source.Character.Race, source.Character.Id);
            //    CharacterStore.LoadInventory(db, source.Character);
            //}

            return "Character Wiped";
        }
    }
}