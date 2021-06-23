using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using HarmonyLib;
using Trainworks.Builders;
using Trainworks.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Trainworks.Managers
{
    /// <summary>
    /// Handles registration and storage of custom class data.
    /// </summary>
    public class CustomClassManager
    {
        /// <summary>
        /// Maps custom class IDs to their respective ClassData.
        /// </summary>
        public static IDictionary<string, ClassData> CustomClassData { get; } = new Dictionary<string, ClassData>();
        /// <summary>
        /// Maps custom class IDs to their respective Class Frame Sprites. The list should be in the format Unit, Spell.
        /// </summary>
        public static IDictionary<string, List<Sprite>> CustomClassFrame { get; } = new Dictionary<string, List<Sprite>> ();
        /// <summary>
        /// Maps custom class IDs to their respective Class Draft Icon
        /// </summary>
        public static IDictionary<string, Sprite> CustomClassDraftIcons { get; } = new Dictionary<string, Sprite>();
        /// <summary>
        /// Maps custom class IDs to the IDs of their main class select screen characters
        /// </summary>
        public static IDictionary<string, string[]> CustomClassSelectScreenCharacterIDsMain { get; } = new Dictionary<string, string[]>();
        /// <summary>
        /// Maps custom class IDs to the IDs of their sub class select screen characters
        /// </summary>
        public static IDictionary<string, string[]> CustomClassSelectScreenCharacterIDsSub { get; } = new Dictionary<string, string[]>();

        /// <summary>
        /// Register a custom class with the manager, allowing it to show up in game.
        /// </summary>
        /// <param name="classData">The custom class data to register</param>
        public static void RegisterCustomClass(ClassData classData)
        {
            if (!CustomClassData.ContainsKey(classData.GetID()))
            {
                CustomClassData.Add(classData.GetID(), classData);
                ProviderManager.SaveManager.GetAllGameData().GetAllClassDatas().Add(classData);
                ProviderManager.SaveManager.GetAllGameData().GetBalanceData().GetClassDatas().Add(classData);
            }
            else
            {
                Trainworks.Log(LogLevel.Warning, "Attempted to register duplicate class data with name: " + classData.name);
            }
        }

        /// <summary>
        /// Get the class data corresponding to the given ID.
        /// </summary>
        /// <param name="classID">ID of the class to get</param>
        /// <returns>The class data for the given ID</returns>
        public static ClassData GetClassDataByID(string classID)
        {
            if (classID == null)
            {
                throw new System.Exception("Cannot get ClassData from ID of null");
            }
            // Search for custom clan matching ID
            var guid = GUIDGenerator.GenerateDeterministicGUID(classID);
            if (CustomClassData.ContainsKey(guid))
            {
                return CustomClassData[guid];
            }

            // No custom clan found; search for vanilla clan matching ID
            var vanillaClan = ProviderManager.SaveManager.GetAllGameData().FindClassData(classID);
            if (vanillaClan == null)
            {
                Trainworks.Log(LogLevel.All, "Couldn't find clan: " + classID + " - This will cause crashes.");
            }
            return vanillaClan;
        }

        /// <summary>
        /// Get the player's current primary clan.
        /// </summary>
        /// <returns>ClassData of the player's primary clan</returns>
        public static ClassData CurrentPrimaryClan()
        {
            var saveData = (SaveData)AccessTools.Property(typeof(SaveManager), "ActiveSaveData").GetValue(ProviderManager.SaveManager);
            ClassData mainClass = ProviderManager.SaveManager.GetAllGameData().FindClassData(saveData.GetStartingConditions().Class);
            return mainClass;
        }

        /// <summary>
        /// Get the player's current allied clan.
        /// </summary>
        /// <returns>ClassData of the player's allied clan</returns>
        public static ClassData CurrentAlliedClan()
        {
            var saveData = (SaveData)AccessTools.Property(typeof(SaveManager), "ActiveSaveData").GetValue(ProviderManager.SaveManager);
            ClassData mainClass = ProviderManager.SaveManager.GetAllGameData().FindClassData(saveData.GetStartingConditions().Class);
            return mainClass;
        }
    }
}
