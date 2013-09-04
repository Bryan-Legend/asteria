using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace HD
{
    public class MapDownloadCommand : MultiplayerCommand
    {
        public override string Description
        {
            get { return "/MapDownload: Downloads the current map to your computer."; }
        }

        public override string Execute(Player player, string args)
        {
            var filename = World.SaveMap(player.Map, player.Map.Name);
            player.PendingFileTransfers.Enqueue(FileTransfer.Load(filename));
            return "File transfer created.";
        }
    }
}