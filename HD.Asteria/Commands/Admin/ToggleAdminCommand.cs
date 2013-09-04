using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class ToggleAdminCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/ToggleAdmin <Character name>: Toggle admin."; }
        }
        
        public override string Execute(Player player, string args)
        {

            if (String.IsNullOrEmpty(args))
                return "Character name required.";

            using (var db = Server.CreateDBConnection())
            {
                var characterAccount = db.EvaluateRow("select * from `character` c join account a on a.Id = c.AccountId where c.Name = '{0}'", LT.DBConnection.AddSlashes(args));
                if (characterAccount == null)
                    return "Unknown character name.";

                if ((int)characterAccount["IsAdmin"] == 1)
                {
                    db.Execute("update account set IsAdmin = 0 where id = {0}", (int)characterAccount["AccountId"]);
                    return args + " is no longer an admin.";
                }
                else
                {
                    db.Execute("update account set IsAdmin = 1 where id = {0}", (int)characterAccount["AccountId"]);
                    return args + " is now an admin.";
                }
            }
        }
    }
}