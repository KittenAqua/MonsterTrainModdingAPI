using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BepInEx.Logging;
using Trainworks.Builders;
using HarmonyLib;
using UnityEngine;
using ShinyShoe;
using UnityEngine.AddressableAssets;
using Trainworks.Utilities;

namespace Trainworks.Managers
{
    /// <summary>
    /// Handles registration and storage of custom relic data.
    /// </summary>
    public class CustomCollectableRelicManager
    {
        /// <summary>
        /// Maps custom relic IDs to their respective RelicData.
        /// </summary>
        public static IDictionary<string, CollectableRelicData> CustomRelicData { get; } = new Dictionary<string, CollectableRelicData>();

        /// <summary>
        /// Register a custom relic with the manager, allowing it to show up in game.
        /// </summary>
        /// <param name="relicData">The custom relic data to register</param>
        /// <param name="relicPoolData">The pools to insert the custom relic data into</param>
        public static void RegisterCustomRelic(CollectableRelicData relicData, List<string> relicPoolData)
        {
            if (!CustomRelicData.ContainsKey(relicData.GetID()))
            {
                CustomRelicData.Add(relicData.GetID(), relicData);
                CustomRelicPoolManager.AddRelicToPools(relicData, relicPoolData);
                ProviderManager.SaveManager.GetAllGameData().GetAllCollectableRelicData().Add(relicData);
            }
            else
            {
                Trainworks.Log(LogLevel.Warning, "Attempted to register duplicate relic data with name: " + relicData.name);
            }
        }

        /// <summary>
        /// Get the relic data corresponding to the given ID
        /// </summary>
        /// <param name="relicID">ID of the relic to get</param>
        /// <returns>The relic data for the given ID</returns>
        public static CollectableRelicData GetRelicDataByID(string relicID)
        {
            if(relicID == null)
            {
                throw new Exception("Cannot find RelicData from ID of null"); 
            }
            // Search for custom relic matching ID
            var guid = GUIDGenerator.GenerateDeterministicGUID(relicID);
            if (CustomRelicData.ContainsKey(guid))
            {
                return CustomRelicData[guid];
            }

            // No custom relic found; search for vanilla relic matching ID
            var vanillaRelic = ProviderManager.SaveManager.GetAllGameData().FindCollectableRelicData(relicID);
            if (vanillaRelic == null)
            {
                Trainworks.Log(LogLevel.All, "Couldn't find relic: " + relicID + " - This will cause crashes.");
            }
            return vanillaRelic;
        }
    }
}
