using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Steamworks;

namespace HD
{
    public static class Counters
    {
        public static Dictionary<Counter, int> Stats;

        public static void Initalize()
        {
            Stats = new Dictionary<Counter, int>();
            Utility.LogMessage("Loading Stats Counters Recieved");
            foreach (var counter in Utility.GetEnumValues<Counter>())
            {
                if (counter != Counter.None)
                {
                    var name = counter.ToString();
                    int value = 0;
#if STEAM
                    if (!World.IsServer)
                        //try
                        //{
                            SteamUserStats.GetStat(name, out value);
                        //}
                        //catch (InvalidOperationException e)
                        //{
                        //    Utility.LogMessage(e.ToString());
                        //}
#endif
                    Stats[counter] = value;
                    Utility.LogMessage(name + " = " + value);
                }
            }
        }

        public static void Commit()
        {
#if STEAM
            if (Counters.Stats != null && !World.IsServer)
                //try
                //{
                    SteamUserStats.StoreStats();
                //}
                //catch (InvalidOperationException e)
                //{
                //    Utility.LogMessage(e.ToString());
                //}
#endif
        }

        public static void Increment(Counter counter, int amount = 1)
        {
            if (counter != Counter.None && Stats != null)
            {
                var name = counter.ToString();
                Stats[counter] += amount;
#if STEAM
                if (!World.IsServer)
                    //try
                    //{
                        SteamUserStats.SetStat(name, Stats[counter]);
                    //}
                    //catch (InvalidOperationException e)
                    //{
                    //    Utility.LogMessage(e.ToString());
                    //}
#endif
            }
        }
    }
}
