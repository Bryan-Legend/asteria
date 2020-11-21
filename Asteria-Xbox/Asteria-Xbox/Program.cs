#define XBOX
#define XBOX360
#define PORTABLE
#define FEAT_SAFE
#define NETCF

// XBOX;XBOX360;PORTABLE;FEAT_SAFE;NETCF

using System;
using HD;

namespace Asteria_Xbox
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (var game = new Main())
            {
                game.Run();
            }
        }
    }
#endif
}

