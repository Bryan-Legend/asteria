#if STEAM

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

using Steamworks;
using System.IO;

namespace HD
{
    public class MapPublishCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapPublish: Publishes a map to the steam workshop."; }
        }

        public override string Execute(Player player, string args)
        {
            if (String.IsNullOrEmpty(args))
                args = player.Map.Name;

            var steamFilename = player.Map.Name + ".map";
            var tempFilename = Path.GetTempFileName();

            player.MessageToClient("By submitting this item, you agree to the workshop terms of service.");

            player.MessageToClient("Saving locally to " + tempFilename);
            tempFilename = World.SaveMap(player.Map, tempFilename);

            try
            {
                var bytes = File.ReadAllBytes(tempFilename);

                player.MessageToClient("Saving " + bytes.Length + " bytes to cloud as " + steamFilename + ". This will take a few moments.");

                var result = SteamRemoteStorage.FileWrite(steamFilename, bytes, bytes.Length);
                if (!result)
                {
                    return "Unable to write save file to steam cloud.";
                }
            }
            finally
            {
                File.Delete(tempFilename);
            }

            var fileShareResult = SteamRemoteStorage.FileShare(steamFilename);
            CallResult<RemoteStorageFileShareResult_t>.Create((result, bIOFailure) =>
            {
                player.MessageToClient(steamFilename + " saved to steam cloud.");

                var publishHandle = SteamRemoteStorage.PublishWorkshopFile(steamFilename, null, new AppId_t(Utility.SteamAppId), steamFilename, "", ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic, new string[] { "Map" }, EWorkshopFileType.k_EWorkshopFileTypeCommunity);
                CallResult<RemoteStoragePublishFileResult_t>.Create((publishResult, isIOFailure) =>
                {

                    if (publishResult.m_bUserNeedsToAcceptWorkshopLegalAgreement)
                    {
                        player.MessageToClient("You need to sign the Steam Workshop Legal Agreement.");
                        System.Diagnostics.Process.Start("http://steamcommunity.com/sharedfiles/workshoplegalagreement");                        
                    }

                    if (publishResult.m_eResult == EResult.k_EResultOK)
                    {
                        System.Diagnostics.Process.Start("http://steamcommunity.com/sharedfiles/filedetails/?id=" + publishResult.m_nPublishedFileId);
                    }
                    else
                    {
                        player.MessageToClient("Publish File failed with result code: " + publishResult.m_eResult);
                    }
                }).Set(publishHandle);
            }).Set(fileShareResult);

            return "";
        }
    }
}

#endif