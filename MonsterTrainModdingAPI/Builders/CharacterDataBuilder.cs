﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;
using BepInEx;
using BepInEx.Harmony;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using ShinyShoe;
using MonsterTrainModdingAPI.Managers;
using MonsterTrainModdingAPI.Enums.MTStatusEffects;
using MonsterTrainModdingAPI.Utilities;

namespace MonsterTrainModdingAPI.Builders
{
    public class CharacterDataBuilder
    {
        /// <summary>
        /// Unique string used to store and retrieve the character data.
        /// </summary>
        public string CharacterID { get; set; }

        /// <summary>
        /// Name displayed on the character.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The character's attack stat.
        /// </summary>
        public int AttackDamage { get; set; }
        /// <summary>
        /// The character's health stat.
        /// </summary>
        public int Health { get; set; }
        /// <summary>
        /// The amount of capacity the character uses up on the floor.
        /// </summary>
        public int Size { get; set; }

        public List<CharacterTriggerData> Triggers { get; set; }

        /// <summary>
        /// Status effects the character starts with when spawned in.
        /// </summary>
        public StatusEffectStackData[] StartingStatusEffects { get; set; }
        public string[] StatusEffectImmunities { get; set; }

        /// <summary>
        /// Custom asset path to load character art from.
        /// </summary>
        public string AssetPath { get; set; }
        /// <summary>
        /// Loading Info for loading a character's sprite
        /// </summary>
        public AssetBundleLoadingInfo SpriteBundleLoadingInfo { get; set; }
        /// <summary>
        /// Loading Info for loading a character's Skeleton Animation
        /// </summary>
        public AssetBundleLoadingInfo SkeletonAnimationBundleLoadingInfo { get; set; }
        /// <summary>
        /// Use an existing base game character's art by filling this in with the appropriate character's asset reference information.
        /// </summary>
        public AssetReferenceGameObject CharacterPrefabVariantRef { get; set; }

        /// <summary>
        /// Whether or not the character is able to attack.
        /// </summary>
        public bool CanAttack { get; set; }
        /// <summary>
        /// Whether or not the character is able to be healed.
        /// </summary>
        public bool CanBeHealed { get; set; }

        /// <summary>
        /// Whether or not the character is a miniboss (a non-flying boss).
        /// </summary>
        public bool IsMiniboss { get; set; }
        /// <summary>
        /// Whether or not the character is a flying boss.
        /// </summary>
        public bool IsOuterTrainBoss { get; set; }

        public bool DeathSlidesBackwards { get; set; }

        public List<ActionGroupData> BossActionGroups { get; set; }

        /// <summary>
        /// Use an existing base game character's lore tooltip by adding its key to this list.
        /// </summary>
        public List<string> CharacterLoreTooltipKeys { get; set; }

        public List<RoomModifierData> RoomModifiers { get; set; }

        public bool AscendsTrainAutomatically { get; set; }

        /// <summary>
        /// "When attacking, this character moves next to its target before hitting it." - base game comment
        /// </summary>
        public bool AttackTeleportsToDefender { get; set; }

        public List<string> SubtypeKeys { get; set; }

        public CharacterSoundData CharacterSoundData { get; set; }

        public CharacterChatterData CharacterChatterData { get; set; }

        public RuntimeAnimatorController AnimationController { get; set; }
        public CharacterDeathVFX.Type DeathType { get; set; }
        public VfxAtLoc DeathVFX { get; set; }
        /// <summary>
        /// "The VFX to put on the target when attacking to override CombatManager CombatReactVFXPrefab" - base game comment
        /// </summary>
        public VfxAtLoc ImpactVFX { get; set; }
        /// <summary>
        /// The projectile to fire when attacking - base game comment
        /// </summary>
        public VfxAtLoc ProjectilePrefab { get; set; }
        public VfxAtLoc BossRoomSpellCastVFX { get; set; }
        public VfxAtLoc BossSpellCastVFX { get; set; }

        /// <summary>
        /// A cache for the character's sprite so it doesn't have to be reloaded repeatedly.
        /// </summary>
        public Sprite CharacterSpriteCache { get; set; }
        /// <summary>
        /// "The default character prefab to use if one isn't found.  (Which should never happen in the shpped game)" - base game comment
        /// </summary>
        public FallbackData FallBackData { get; set; }

        public CharacterDataBuilder()
        {
            this.AttackDamage = 10;
            this.Size = 2;
            this.AttackTeleportsToDefender = true;
            this.CanAttack = true;
            this.CanBeHealed = true;
            this.DeathSlidesBackwards = true;
            this.Triggers = new List<CharacterTriggerData>();
            this.SubtypeKeys = new List<string>();
            this.BossActionGroups = new List<ActionGroupData>();
            this.RoomModifiers = new List<RoomModifierData>();
            this.CharacterLoreTooltipKeys = new List<string>();
            this.StartingStatusEffects = new StatusEffectStackData[0];
            this.StatusEffectImmunities = new string[0];
            this.ImpactVFX = (VfxAtLoc)FormatterServices.GetUninitializedObject(typeof(VfxAtLoc));
        }

        /// <summary>
        /// Builds the CharacterData represented by this builder's parameters recursively
        /// and registers it and its components with the appropriate managers.
        /// </summary>
        /// <returns>The newly registered CharacterData</returns>
        public CharacterData BuildAndRegister()
        {
            var characterData = this.Build();
            CustomCharacterManager.RegisterCustomCharacter(characterData, SpriteBundleLoadingInfo, SkeletonAnimationBundleLoadingInfo);
            return characterData;
        }

        /// <summary>
        /// Builds the CharacterData represented by this builder's parameters recursively;
        /// all Builders represented in this class's various fields will also be built.
        /// </summary>
        /// <returns>The newly created CharacterData</returns>
        public CharacterData Build()
        {
            CharacterData characterData = ScriptableObject.CreateInstance<CharacterData>();
            AccessTools.Field(typeof(CharacterData), "id").SetValue(characterData, this.CharacterID);
            AccessTools.Field(typeof(CharacterData), "animationController").SetValue(characterData, this.AnimationController);
            AccessTools.Field(typeof(CharacterData), "ascendsTrainAutomatically").SetValue(characterData, this.AscendsTrainAutomatically);
            AccessTools.Field(typeof(CharacterData), "attackDamage").SetValue(characterData, this.AttackDamage);
            AccessTools.Field(typeof(CharacterData), "attackTeleportsToDefender").SetValue(characterData, this.AttackTeleportsToDefender);
            AccessTools.Field(typeof(CharacterData), "bossActionGroups").SetValue(characterData, this.BossActionGroups);
            AccessTools.Field(typeof(CharacterData), "bossRoomSpellCastVFX").SetValue(characterData, this.BossRoomSpellCastVFX);
            AccessTools.Field(typeof(CharacterData), "bossSpellCastVFX").SetValue(characterData, this.BossSpellCastVFX);
            AccessTools.Field(typeof(CharacterData), "canAttack").SetValue(characterData, this.CanAttack);
            AccessTools.Field(typeof(CharacterData), "canBeHealed").SetValue(characterData, this.CanBeHealed);
            AccessTools.Field(typeof(CharacterData), "characterChatterData").SetValue(characterData, this.CharacterChatterData);
            AccessTools.Field(typeof(CharacterData), "characterLoreTooltipKeys").SetValue(characterData, this.CharacterLoreTooltipKeys);
            if (this.CharacterPrefabVariantRef == null)
            {
                this.CreateAndSetCharacterArtPrefabVariantRef(this.AssetPath, this.AssetPath);
            }
            AccessTools.Field(typeof(CharacterData), "characterPrefabVariantRef").SetValue(characterData, this.CharacterPrefabVariantRef);
            AccessTools.Field(typeof(CharacterData), "characterSoundData").SetValue(characterData, this.CharacterSoundData);
            AccessTools.Field(typeof(CharacterData), "characterSpriteCache").SetValue(characterData, this.CharacterSpriteCache);
            AccessTools.Field(typeof(CharacterData), "deathSlidesBackwards").SetValue(characterData, this.DeathSlidesBackwards);
            AccessTools.Field(typeof(CharacterData), "deathType").SetValue(characterData, this.DeathType);
            AccessTools.Field(typeof(CharacterData), "deathVFX").SetValue(characterData, this.DeathVFX);
            AccessTools.Field(typeof(CharacterData), "fallbackData").SetValue(characterData, this.FallBackData);
            AccessTools.Field(typeof(CharacterData), "health").SetValue(characterData, this.Health);
            AccessTools.Field(typeof(CharacterData), "impactVFX").SetValue(characterData, this.ImpactVFX);
            AccessTools.Field(typeof(CharacterData), "isMiniboss").SetValue(characterData, this.IsMiniboss);
            AccessTools.Field(typeof(CharacterData), "isOuterTrainBoss").SetValue(characterData, this.IsOuterTrainBoss);
            AccessTools.Field(typeof(CharacterData), "nameKey").SetValue(characterData, this.Name);
            AccessTools.Field(typeof(CharacterData), "projectilePrefab").SetValue(characterData, this.ProjectilePrefab);
            AccessTools.Field(typeof(CharacterData), "roomModifiers").SetValue(characterData, this.RoomModifiers);
            AccessTools.Field(typeof(CharacterData), "size").SetValue(characterData, this.Size);
            AccessTools.Field(typeof(CharacterData), "startingStatusEffects").SetValue(characterData, this.StartingStatusEffects);
            AccessTools.Field(typeof(CharacterData), "statusEffectImmunities").SetValue(characterData, this.StatusEffectImmunities);
            //AccessTools.Field(typeof(CardData), "stringBuilder").SetValue(cardData, this.);
            AccessTools.Field(typeof(CharacterData), "subtypeKeys").SetValue(characterData, this.SubtypeKeys);
            AccessTools.Field(typeof(CharacterData), "triggers").SetValue(characterData, this.Triggers);
            return characterData;
        }

        /// <summary>
        /// Creates an asset reference to an existing game file.
        /// Primarily useful for reusing base game art.
        /// Cards with custom art should not use this method.
        /// </summary>
        /// <param name="m_debugName">The asset's debug name (usually the path to it)</param>
        /// <param name="m_AssetGUID">The asset's GUID</param>
        public void CreateAndSetCharacterArtPrefabVariantRef(string m_debugName, string m_AssetGUID)
        {
            var assetReferenceGameObject = new AssetReferenceGameObject();
            AccessTools.Field(typeof(AssetReferenceGameObject), "m_debugName")
                    .SetValue(assetReferenceGameObject, m_debugName);
            AccessTools.Field(typeof(AssetReferenceGameObject), "m_AssetGUID")
                .SetValue(assetReferenceGameObject, m_AssetGUID);
            this.CharacterPrefabVariantRef = assetReferenceGameObject;

            this.AssetPath = m_AssetGUID;
        }

        /// <summary>
        /// Add a status effect to this character's starting status effect array.
        /// </summary>
        /// <param name="statusEffectType">Must implement IMTStatusEffect</param>
        /// <param name="stackCount">Number of stacks to apply</param>
        public void AddStartingStatusEffect(Type statusEffectType, int stackCount)
        {
            string statusEffectID = MTStatusEffectIDs.GetIDForType(statusEffectType);
            this.AddStartingStatusEffect(statusEffectID, stackCount);
        }

        /// <summary>
        /// Add a status effect to this character's starting status effect array.
        /// </summary>
        /// <param name="statusEffectID">ID of the status effect, most easily retrieved using the helper class "MTStatusEffectIDs"</param>
        /// <param name="stackCount">Number of stacks to apply</param>
        public void AddStartingStatusEffect(string statusEffectID, int stackCount)
        {
            this.StartingStatusEffects = BuilderUtils.AddStatusEffect(statusEffectID, stackCount, this.StartingStatusEffects);
        }
    }
}
