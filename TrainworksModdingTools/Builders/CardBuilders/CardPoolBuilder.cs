using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Harmony;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using ShinyShoe;
using Trainworks.Managers;
using Trainworks.Utilities;
using System.IO;
using Trainworks.Constants;

namespace Trainworks.Builders
{
    public class CardPoolBuilder
    {
        /// <summary>
        /// This is both its ID and its name. Must be unique.
        /// </summary>
        public string CardPoolID { get; set; }

        /// <summary>
        /// The IDs of cards to put into the card pool
        /// </summary>
        public List<string> CardIDs { get; set; }

        /// <summary>
        /// CardData of cards to put into the card pool
        /// </summary>
        public List<CardData> Cards { get; set; }

        public CardPoolBuilder()
        {
            this.CardIDs = new List<string>();
            this.Cards = new List<CardData>();
        }

        /// <summary>
        /// Builds the CardPool represented by this builder's parameters
        /// and registers it with the CustomCardPoolManager.
        /// </summary>
        /// <returns>The newly registered CardPool</returns>
        public CardPool BuildAndRegister()
        {
            var cardPool = this.Build();
            CustomCardPoolManager.RegisterCustomCardPool(cardPool);
            return cardPool;
        }

        /// <summary>
        /// Builds the CardPool represented by this builder's parameters
        /// </summary>
        /// <returns>The newly created CardPool</returns>
        public CardPool Build()
        {
            CardPool cardPool = ScriptableObject.CreateInstance<CardPool>();
            cardPool.name = this.CardPoolID;
            var cardDataList = (Malee.ReorderableArray<CardData>)AccessTools.Field(typeof(CardPool), "cardDataList").GetValue(cardPool);
            foreach (string cardID in this.CardIDs)
            {
                if(cardID == null)
                {
                    Trainworks.Log(LogLevel.Warning, "Null Card ID in CardPool, Skipping...");
                    continue;
                }
                var card = CustomCardManager.GetCardDataByID(cardID);
                if (card != null)
                {
                    cardDataList.Add(card);
                }
            }
            foreach (CardData cardData in this.Cards)
            {
                cardDataList.Add(cardData);
            }
            return cardPool;
        }
    }
}
