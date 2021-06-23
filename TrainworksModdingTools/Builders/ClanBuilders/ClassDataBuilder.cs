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

namespace Trainworks.Builders
{
    /// <summary>
    /// Builder class to aid in creating custom clans.
    /// </summary>
    public class ClassDataBuilder
    {
        /// <summary>
        /// Don't set directly; use ClassID instead.
        /// </summary>
        public string classID;

        /// <summary>
        /// Unique string used to store and retrieve the clan data.
        /// Implictly sets TitleLoc, DescriptionLoc, and SubclassDescriptionLoc if null.
        /// </summary>
        public string ClassID
        {
            get { return this.classID; }
            set
            {
                this.classID = value;
                if (this.TitleLoc == null)
                {
                    this.TitleLoc = this.classID + "_ClassData_TitleLoc";
                }
                if (this.DescriptionLoc == null)
                {
                    this.DescriptionLoc = this.classID + "_ClassData_DescriptionLoc";
                }
                if (this.SubclassDescriptionLoc == null)
                {
                    this.SubclassDescriptionLoc = this.classID + "_ClassData_SubclassDescriptionLoc";
                }
            }
        }
        /// <summary>
        /// Name of the clan.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description of the clan.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Description of the clan when selected as the allied clan.
        /// </summary>
        public string SubclassDescription { get; set; }

        /// <summary>
        /// Please set storyChampionData and championCharacterArt
        /// </summary>
        public List<ChampionData> Champions { get; set; }

        /// <summary>
        /// Set automatically in the constructor. Base asset path, usually the plugin directory.
        /// </summary>
        public string BaseAssetPath { get; set; }

        /// <summary>
        /// Must contain 4 sprite paths; in order:
        /// small icon (32x32)
        /// medium icon (??x??)
        /// large icon (89x89)
        /// silhouette icon (43x43)
        /// </summary>
        public List<string> IconAssetPaths { get; set; }
        /// <summary>
        /// Use CardStyle only for accessing the base game card frames. Otherwise use CardFrame for custom frames
        /// </summary>
        public ClassCardStyle CardStyle { get; set; }
        /// <summary>
        /// Add a custom CardFrame as a sprite for unit cards.
        /// </summary>
        public string CardFrameUnitPath { get; set; }
        /// <summary>
        /// Add a custom CardFrame as a sprite for spell cards.
        /// </summary>
        public string CardFrameSpellPath { get; set; }
        /// <summary>
        /// Add a custom icon for the card draft on battle victory.
        /// </summary>
        public string DraftIconPath { get; set; }

        public List<ClassData.StartingCardOptions> MainClassStartingCards { get; set; }
        public List<ClassData.StartingCardOptions> SubclassStartingCards { get; set; }

        public Color UiColor { get; set; }
        public Color UiColorDark { get; set; }

        public Dictionary<MetagameSaveData.TrackedValue, string> UnlockKeys { get; set; }
        public MetagameSaveData.TrackedValue ClassUnlockCondition { get; set; }
        public int ClassUnlockParam { get; set; }
        public List<string> ClassUnlockPreviewTexts { get; set; }

        public List<RelicData> StarterRelics { get; set; } = new List<RelicData>();
        public DLC RequiredDlc { get; set; } = DLC.None;

        public string TitleLoc { get; set; }
        public string DescriptionLoc { get; set; }
        public string SubclassDescriptionLoc { get; set; }

        public string[] ClassSelectScreenCharacterIDsMain { get; set; }
        public string[] ClassSelectScreenCharacterIDsSub { get; set; }


        public ClassDataBuilder()
        {
            this.IconAssetPaths = new List<string>();
            this.MainClassStartingCards = new List<ClassData.StartingCardOptions>();
            this.SubclassStartingCards = new List<ClassData.StartingCardOptions>();
            this.UnlockKeys = new Dictionary<MetagameSaveData.TrackedValue, string>();
            this.ClassUnlockPreviewTexts = new List<string>();
            this.Champions = new List<ChampionData>
            {
                (ChampionData)UnityEngine.ScriptableObject.CreateInstance("ChampionData"),
                (ChampionData)UnityEngine.ScriptableObject.CreateInstance("ChampionData"),
            };

            var assembly = Assembly.GetCallingAssembly();
            this.BaseAssetPath = PluginManager.PluginGUIDToPath[PluginManager.AssemblyNameToPluginGUID[assembly.FullName]];
        }

        /// <summary>
        /// Builds the ClassData represented by this builder's parameters recursively
        /// and registers it and its components with the appropriate managers.
        /// </summary>
        /// <returns>The newly registered ClassData</returns>
        public ClassData BuildAndRegister()
        {
            var classData = this.Build();
            CustomClassManager.RegisterCustomClass(classData);
            return classData;
        }

        /// <summary>
        /// Builds the ClassData represented by this builder's parameters recursively;
        /// all Builders represented in this class's various fields will also be built.
        /// </summary>
        /// <returns>The newly created ClassData</returns>
        public ClassData Build()
        {
            ClassData classData = ScriptableObject.CreateInstance<ClassData>();
            if(ClassID == null)
            {
                throw new Exception("Class ID is Required");
            }
            string GUID = GUIDGenerator.GenerateDeterministicGUID(this.ClassID);
            classData.name = this.ClassID;

            AccessTools.Field(typeof(ClassData), "id").SetValue(classData, GUID);
            AccessTools.Field(typeof(ClassData), "cardStyle").SetValue(classData, this.CardStyle);
            AccessTools.Field(typeof(ClassData), "classUnlockCondition").SetValue(classData, this.ClassUnlockCondition);
            AccessTools.Field(typeof(ClassData), "classUnlockParam").SetValue(classData, this.ClassUnlockParam);
            AccessTools.Field(typeof(ClassData), "classUnlockPreviewTexts").SetValue(classData, this.ClassUnlockPreviewTexts);
            BuilderUtils.ImportStandardLocalization(this.DescriptionLoc, this.Description);
            AccessTools.Field(typeof(ClassData), "descriptionLoc").SetValue(classData, this.DescriptionLoc);
            var icons = new List<Sprite>();
            foreach (string iconPath in this.IconAssetPaths)
            {
                icons.Add(CustomAssetManager.LoadSpriteFromPath(this.BaseAssetPath + "/" + iconPath));
            }
            Type iconSetType = AccessTools.Inner(typeof(ClassData), "IconSet");
            var iconSet = Activator.CreateInstance(iconSetType);
            AccessTools.Field(iconSetType, "small").SetValue(iconSet, icons[0]);
            AccessTools.Field(iconSetType, "medium").SetValue(iconSet, icons[1]);
            AccessTools.Field(iconSetType, "large").SetValue(iconSet, icons[2]);
            AccessTools.Field(iconSetType, "silhouette").SetValue(iconSet, icons[3]);
            AccessTools.Field(typeof(ClassData), "icons").SetValue(classData, iconSet);
            AccessTools.Field(typeof(ClassData), "champions").SetValue(classData, this.Champions);
            BuilderUtils.ImportStandardLocalization(this.SubclassDescriptionLoc, this.SubclassDescription);
            AccessTools.Field(typeof(ClassData), "subclassDescriptionLoc").SetValue(classData, this.SubclassDescriptionLoc);
            BuilderUtils.ImportStandardLocalization(this.TitleLoc, this.Name);
            AccessTools.Field(typeof(ClassData), "titleLoc").SetValue(classData, this.TitleLoc);
            AccessTools.Field(typeof(ClassData), "uiColor").SetValue(classData, this.UiColor);
            AccessTools.Field(typeof(ClassData), "uiColorDark").SetValue(classData, this.UiColorDark);
            //AccessTools.Field(typeof(ClassData), "UNLOCK_KEYS").SetValue(classData, this.);

            AccessTools.Field(typeof(ClassData), "starterRelics").SetValue(classData, StarterRelics);
            AccessTools.Field(typeof(ClassData), "requiredDlc").SetValue(classData, RequiredDlc);

            // Card Frame
            if (this.CardFrameSpellPath != null && this.CardFrameUnitPath != null)
            {
                Sprite cardFrameSpellSprite = CustomAssetManager.LoadSpriteFromPath(this.BaseAssetPath + "/" + this.CardFrameSpellPath);
                Sprite cardFrameUnitSprite = CustomAssetManager.LoadSpriteFromPath(this.BaseAssetPath + "/" + this.CardFrameUnitPath);
                CustomClassManager.CustomClassFrame.Add(GUID, new List<Sprite>() { cardFrameUnitSprite, cardFrameSpellSprite });
            }

            // Draft Icon
            if (this.DraftIconPath != null)
            {
                Sprite draftIconSprite = CustomAssetManager.LoadSpriteFromPath(this.BaseAssetPath + "/" + this.DraftIconPath);
                CustomClassManager.CustomClassDraftIcons.Add(GUID, draftIconSprite);
            }
            // Class select character IDs
            CustomClassManager.CustomClassSelectScreenCharacterIDsMain.Add(GUID, this.ClassSelectScreenCharacterIDsMain);
            CustomClassManager.CustomClassSelectScreenCharacterIDsSub.Add(GUID, this.ClassSelectScreenCharacterIDsSub);


            return classData;
        }
    }
}
