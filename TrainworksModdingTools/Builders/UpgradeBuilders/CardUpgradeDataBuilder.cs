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
using System.Linq;

namespace Trainworks.Builders
{
    public class CardUpgradeDataBuilder
    {
        // Note: add status effect adder, it's different than the one in BuilderUtils

        /// <summary>
        /// Don't set directly; use UpgradeTitle instead.
        /// </summary>
        public string upgradeTitle;

        /// <summary>
        /// Overrides UpgradeTitleKey
        /// Implicitly sets UpgradeTitleKey, UpgradeDescriptionKey, and UpgradeNotificationKey if null
        /// </summary>
        public string UpgradeTitle
        {
            get { return this.upgradeTitle; }
            set
            {
                this.upgradeTitle = value;
                if (this.UpgradeTitleKey == null)
                {
                    this.UpgradeTitleKey = this.upgradeTitle + "_CardUpgradeData_UpgradeTitleKey";
                }
                if (this.UpgradeDescriptionKey == null)
                {
                    this.UpgradeDescriptionKey = this.upgradeTitle + "_CardUpgradeData_UpgradeDescriptionKey";
                }
                if (this.UpgradeNotificationKey == null)
                {
                    this.UpgradeNotificationKey = this.upgradeTitle + "_CardUpgradeData_UpgradeNotificationKey";
                }
            }
        }
        /// <summary>
        /// Overrides UpgradeDescriptionKey
        /// </summary>
        public string UpgradeDescription { get; set; }
        /// <summary>
        /// Overrides UpgradeNotificationKey
        /// </summary>
        public string UpgradeNotification { get; set; }
        public string UpgradeTitleKey { get; set; }
        public string UpgradeDescriptionKey { get; set; }
        public string UpgradeNotificationKey { get; set; }
        public string UpgradeIconPath { get; set; }
        public bool HideUpgradeIconOnCard { get; set; }
        public bool UseUpgradeHighlightTextTags { get; set; }
        public int BonusDamage { get; set; }
        public int BonusHP { get; set; }
        public int CostReduction { get; set; }
        public int XCostReduction { get; set; }
        public int BonusHeal { get; set; }
        public int BonusSize { get; set; }

        public List<CardTraitDataBuilder> TraitDataUpgradeBuilders { get; set; }
        public List<CharacterTriggerDataBuilder> TriggerUpgradeBuilders { get; set; }
        public List<CardTriggerEffectDataBuilder> CardTriggerUpgradeBuilders { get; set; }
        public List<RoomModifierDataBuilder> RoomModifierUpgradeBuilders { get; set; }
        public List<CardUpgradeMaskDataBuilder> FiltersBuilders { get; set; }
        public List<CardUpgradeDataBuilder> UpgradesToRemoveBuilders { get; set; }

        /// <summary>
        /// To add a status effect, no need for a builder. new StatusEffectStackData with properties statusId (string) and count (int) are sufficient.
        /// Get the string with -> statusEffectID = MTStatusEffectIDs.GetIDForType(statusEffectType);
        /// </summary>
        public List<StatusEffectStackData> StatusEffectUpgrades { get; set; }
        public List<CardTraitData> TraitDataUpgrades { get; set; }
        public List<string> RemoveTraitUpgrades { get; set; }
        public List<CharacterTriggerData> TriggerUpgrades { get; set; }
        public List<CardTriggerEffectData> CardTriggerUpgrades { get; set; }
        public List<RoomModifierData> RoomModifierUpgrades { get; set; }
        public List<CardUpgradeMaskData> Filters { get; set; }
        public List<CardUpgradeData> UpgradesToRemove { get; set; }

        public CharacterData SourceSynthesisUnit { get; set; }
        public bool IsUnitSynthesisUpgrade { get => SourceSynthesisUnit != null; }

        public string BaseAssetPath { get; set; }

        public CardUpgradeDataBuilder()
        {
            this.UpgradeNotificationKey = null;
            this.UseUpgradeHighlightTextTags = true;

            this.TraitDataUpgradeBuilders = new List<CardTraitDataBuilder>();
            this.TriggerUpgradeBuilders = new List<CharacterTriggerDataBuilder>();
            this.CardTriggerUpgradeBuilders = new List<CardTriggerEffectDataBuilder>();
            this.RoomModifierUpgradeBuilders = new List<RoomModifierDataBuilder>();
            this.FiltersBuilders = new List<CardUpgradeMaskDataBuilder>();
            this.UpgradesToRemoveBuilders = new List<CardUpgradeDataBuilder>();

            this.StatusEffectUpgrades = new List<StatusEffectStackData>();
            this.TraitDataUpgrades = new List<CardTraitData>();
            this.RemoveTraitUpgrades = new List<string>();
            this.TriggerUpgrades = new List<CharacterTriggerData>();
            this.CardTriggerUpgrades = new List<CardTriggerEffectData>();
            this.RoomModifierUpgrades = new List<RoomModifierData>();
            this.Filters = new List<CardUpgradeMaskData>();
            this.UpgradesToRemove = new List<CardUpgradeData>();

            var assembly = Assembly.GetCallingAssembly();
            this.BaseAssetPath = PluginManager.PluginGUIDToPath[PluginManager.AssemblyNameToPluginGUID[assembly.FullName]];
        }

        public CardUpgradeData Build()
        {
            CardUpgradeData cardUpgradeData = ScriptableObject.CreateInstance<CardUpgradeData>();

            foreach (var builder in this.TraitDataUpgradeBuilders)
            {
                this.TraitDataUpgrades.Add(builder.Build());
            }
            foreach (var builder in this.TriggerUpgradeBuilders)
            {
                this.TriggerUpgrades.Add(builder.Build());
            }
            foreach (var builder in this.CardTriggerUpgradeBuilders)
            {
                this.CardTriggerUpgrades.Add(builder.Build());
            }
            foreach (var builder in this.RoomModifierUpgradeBuilders)
            {
                this.RoomModifierUpgrades.Add(builder.Build());
            }
            foreach (var builder in this.FiltersBuilders)
            {
                this.Filters.Add(builder.Build());
            }
            foreach (var builder in this.UpgradesToRemoveBuilders)
            {
                this.UpgradesToRemove.Add(builder.Build());
            }

            AccessTools.Field(typeof(CardUpgradeData), "bonusDamage").SetValue(cardUpgradeData, this.BonusDamage);
            AccessTools.Field(typeof(CardUpgradeData), "bonusHeal").SetValue(cardUpgradeData, this.BonusHeal);
            AccessTools.Field(typeof(CardUpgradeData), "bonusHP").SetValue(cardUpgradeData, this.BonusHP);
            AccessTools.Field(typeof(CardUpgradeData), "bonusSize").SetValue(cardUpgradeData, this.BonusSize);
            AccessTools.Field(typeof(CardUpgradeData), "cardTriggerUpgrades").SetValue(cardUpgradeData, this.CardTriggerUpgrades);
            AccessTools.Field(typeof(CardUpgradeData), "costReduction").SetValue(cardUpgradeData, this.CostReduction);
            AccessTools.Field(typeof(CardUpgradeData), "filters").SetValue(cardUpgradeData, this.Filters);
            AccessTools.Field(typeof(CardUpgradeData), "hideUpgradeIconOnCard").SetValue(cardUpgradeData, this.HideUpgradeIconOnCard);
            AccessTools.Field(typeof(CardUpgradeData), "removeTraitUpgrades").SetValue(cardUpgradeData, this.RemoveTraitUpgrades);
            AccessTools.Field(typeof(CardUpgradeData), "roomModifierUpgrades").SetValue(cardUpgradeData, this.RoomModifierUpgrades);
            AccessTools.Field(typeof(CardUpgradeData), "statusEffectUpgrades").SetValue(cardUpgradeData, this.StatusEffectUpgrades);
            AccessTools.Field(typeof(CardUpgradeData), "traitDataUpgrades").SetValue(cardUpgradeData, this.TraitDataUpgrades);
            AccessTools.Field(typeof(CardUpgradeData), "triggerUpgrades").SetValue(cardUpgradeData, this.TriggerUpgrades);
            BuilderUtils.ImportStandardLocalization(this.UpgradeDescriptionKey, this.UpgradeDescription);
            AccessTools.Field(typeof(CardUpgradeData), "upgradeDescriptionKey").SetValue(cardUpgradeData, this.UpgradeDescriptionKey);
            if (this.UpgradeIconPath != null && this.UpgradeIconPath != "")
                AccessTools.Field(typeof(CardUpgradeData), "upgradeIcon").SetValue(cardUpgradeData, CustomAssetManager.LoadSpriteFromPath(this.BaseAssetPath + "/" + this.UpgradeIconPath));
            BuilderUtils.ImportStandardLocalization(this.UpgradeNotificationKey, this.UpgradeNotification);
            AccessTools.Field(typeof(CardUpgradeData), "upgradeNotificationKey").SetValue(cardUpgradeData, this.UpgradeNotificationKey);
            AccessTools.Field(typeof(CardUpgradeData), "upgradesToRemove").SetValue(cardUpgradeData, this.UpgradesToRemove);
            BuilderUtils.ImportStandardLocalization(this.UpgradeTitleKey, this.UpgradeTitle);
            AccessTools.Field(typeof(CardUpgradeData), "upgradeTitleKey").SetValue(cardUpgradeData, this.UpgradeTitleKey);
            AccessTools.Field(typeof(CardUpgradeData), "useUpgradeHighlightTextTags").SetValue(cardUpgradeData, this.UseUpgradeHighlightTextTags);
            AccessTools.Field(typeof(CardUpgradeData), "xCostReduction").SetValue(cardUpgradeData, this.XCostReduction);

            AccessTools.Field(typeof(CardUpgradeData), "isUnitSynthesisUpgrade").SetValue(cardUpgradeData, IsUnitSynthesisUpgrade);
            AccessTools.Field(typeof(CardUpgradeData), "sourceSynthesisUnit").SetValue(cardUpgradeData, SourceSynthesisUnit);

            cardUpgradeData.name = UpgradeTitleKey;
            Traverse.Create(cardUpgradeData).Field("id").SetValue(UpgradeTitleKey);

            // If CardUpgrades are not added to allGameData, there are many troubles.
            var field = Traverse.Create(ProviderManager.SaveManager.GetAllGameData()).Field("cardUpgradeDatas");
            var upgradeList = field.GetValue<List<CardUpgradeData>>();

            // If upgrade already exists, update it by removing the previously added version
            // This might happen if Build() is called twice (e.g. when defining a synthesis for a unit and calling Build())
            var existingEntry = upgradeList
                .Where(u => UpgradeTitleKey == (string)AccessTools.Field(typeof(CardUpgradeData), "upgradeTitleKey").GetValue(u))
                .FirstOrDefault();

            upgradeList.Remove(existingEntry);

            upgradeList.Add(cardUpgradeData);

            return cardUpgradeData;
        }
    }
}
