﻿using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;
using MonsterTrainModdingAPI.Builder;
using MonsterTrainModdingAPI.Models;

namespace MonsterTrainModdingAPI.Managers
{
    public class CustomCardManager
    {
        public static IDictionary<string, CardData> CustomCardData { get; } = new Dictionary<string, CardData>();
        public static IDictionary<string, CharacterData> CustomCharacterData { get; } = new Dictionary<string, CharacterData>();
        public static IDictionary<string, List<int>> CustomCardPoolData { get; } = new Dictionary<string, List<int>>();
        public static SaveManager SaveManager { get; set; }

        public static void RegisterAllCustomCards()
        {
            foreach (TrainModule mod in API.plugins)
            {
                var modCards = mod.RegisterCustomCards();
                foreach (CardDataBuilder card in modCards)
                {
                    var cardData = card.Build();
                    API.Log(LogLevel.Debug, "Adding custom card: " + cardData.GetName() + " from " + mod.RegisterPlugin().name);
                    RegisterCustomCardData(cardData, card.CardPoolIDs);
                }
            }
        }

        public static void RegisterCustomCardData(CardData cardData, List<int> cardPoolData)
        {
            CustomCardData.Add(cardData.GetID(), cardData);
            CustomCardPoolData.Add(cardData.GetID(), cardPoolData);
            SaveManager.GetAllGameData().GetAllCardData().Add(cardData);
        }

        public static void RegisterCustomCharacterData(CharacterData characterData)
        {
            CustomCharacterData.Add(characterData.GetID(), characterData);
        }

        public static CardData GetCardDataByID(string cardID)
        {
            if (CustomCardData.ContainsKey(cardID))
            {
                return CustomCardData[cardID];
            }
            return null;
        }

        public static List<CardData> GetCardsForPool(int cardPoolID)
        {
            var validCards = new List<CardData>();
            foreach (KeyValuePair<string, CardData> entry in CustomCardData)
            {
                foreach (int customPoolID in CustomCardPoolData[entry.Key])
                {
                    if (customPoolID == cardPoolID)
                    {
                        validCards.Add(entry.Value);
                        break;
                    }
                }
            }
            return validCards;
        }

        public static List<CardData> GetCardsForPoolSatisfyingConstraints(int cardPoolID, ClassData classData, CollectableRarity paramRarity, CardPoolHelper.RarityCondition rarityCondition, bool testRarityCondition)
        {
            var allValidCards = GetCardsForPool(cardPoolID);
            var validCards = new List<CardData>();
            foreach (CardData cardData in allValidCards)
            {
                if (cardData.GetLinkedClass() == classData && (!testRarityCondition || rarityCondition(paramRarity, cardData.GetRarity())))
                {
                    validCards.Add(cardData);
                }
            }
            return validCards;
        }

        public static ClassData CurrentPrimaryClan()
        {
            var saveData = (SaveData)AccessTools.Property(typeof(SaveManager), "ActiveSaveData").GetValue(SaveManager);
            ClassData mainClass = SaveManager.GetAllGameData().FindClassData(saveData.GetStartingConditions().Class);
            return mainClass;
        }

        public static ClassData CurrentAlliedClan()
        {
            var saveData = (SaveData)AccessTools.Property(typeof(SaveManager), "ActiveSaveData").GetValue(SaveManager);
            ClassData mainClass = SaveManager.GetAllGameData().FindClassData(saveData.GetStartingConditions().Class);
            return mainClass;
        }
    }
}
