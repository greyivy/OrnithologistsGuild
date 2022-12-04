using System;
using System.Collections.Generic;
using System.IO;
using ContentPatcher;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using OrnithologistsGuild.Models;
using StardewModdingAPI;
using StardewValley;

namespace OrnithologistsGuild.Content
{
    public class BirdieDef
    {
        public void LoadSoundAssets()
        {
            if (SoundAssetPath != null)
            {
                SoundID = $"{UniqueID}_Sound";

                CueDefinition cueDef = new CueDefinition();

                cueDef.name = SoundID;
                cueDef.instanceLimit = 1;
                cueDef.limitBehavior = CueDefinition.LimitBehavior.FailToPlay;

                string filePathCombined = Path.Combine(ContentPackDef.ContentPack.DirectoryPath, SoundAssetPath);
                SoundEffect audio = SoundEffect.FromFile(filePathCombined);

                cueDef.SetSound(audio, Game1.audioEngine.GetCategoryIndex("Sound"), false);

                Game1.soundBank.AddCue(cueDef);
            }
        }

        public void ParseConditions(IContentPatcherAPI contentPatcher)
        {
            foreach (var condition in this.Conditions)
            {
                condition.ManagedConditions = contentPatcher.ParseConditions(
                       manifest: ModEntry.Instance.ModManifest,
                       rawConditions: condition.When,
                       formatVersion: new SemanticVersion("1.20.0")
                 );

                if (!condition.ManagedConditions.IsValid)
                {
                    throw new Exception(condition.ManagedConditions.ValidationError);
                }
            }
        }

        public string ID;
        public string UniqueID; // Generated
        public string AssetPath;

        public string SoundAssetPath;
        public string SoundID; // Generated

        public int BaseFrame = 0;

        public ContentPackDef ContentPackDef; // Generated

        // TODO sounds

        public int Attributes;

        public bool ShouldUseBath = true;
        public int MaxFlockSize = 1;
        public int Cautiousness = 5;
        public int FlapDuration = 500;
        public float FlySpeed = 5f;

        public float BaseWt = 0.5f;
        public Dictionary<string, float> FoodBaseWts;
        public Dictionary<string, float> FeederBaseWts;
        public List<BirdDefCondition> Conditions;

        public float GetContextualWeight(bool updateContext = true, Models.FeederDef feeder = null, Models.FoodDef food = null)
        {
            ModEntry.Instance.Monitor.Log("GetContextualWeight");
            float weight = this.BaseWt;

            if (feeder != null)
            {
                if (this.FeederBaseWts.ContainsKey(feeder.type))
                {
                    weight += this.FeederBaseWts[feeder.type];
                } else
                {
                    ModEntry.Instance.Monitor.Log($@"{this.ID} 0");
                    return 0; // Bird does not eat at feeder
                }
            }

            if (food != null)
            {
                if (this.FoodBaseWts.ContainsKey(food.type))
                {
                    weight += this.FoodBaseWts[food.type];
                }
                else
                {
                    ModEntry.Instance.Monitor.Log($@"{this.ID} 0");
                    return 0; // Bird does not eat food
                }
            }

            foreach (var condition in this.Conditions)
            {
                if (updateContext) condition.ManagedConditions.UpdateContext();

                if (condition.ManagedConditions.IsMatch)
                {
                    if (condition.NilWt)
                    {
                        ModEntry.Instance.Monitor.Log($@"{this.ID} 0");
                        return 0; // Bird not added
                    }

                    weight += condition.AddWt;
                    weight -= condition.SubWt;
                }
            }

            ModEntry.Instance.Monitor.Log($@"{this.ID} {MathHelper.Clamp(weight, 0, 1).ToString()}");
            return MathHelper.Clamp(weight, 0, 1);
        }

        public int GetContextualCautiousness()
        {
            var modifier = -Math.Clamp((int)Math.Round(Game1.player.DailyLuck * 10), -1, 1);
            // TODO verify
            //ModEntry.Instance.Monitor.Log($"Luck level: {Game1.player.DailyLuck} / Cautiousness modifier: {modifier}");

            return this.Cautiousness + modifier;
        }

        public bool CanPerchAt(FeederDef feederDef)
        {
            return FeederBaseWts.ContainsKey(feederDef.type);
        }

        public bool CanEat(FoodDef foodDef)
        {
            return FoodBaseWts.ContainsKey(foodDef.type);
        }
    }

    public class BirdDefCondition
    {
        public Dictionary<string, string> When;
        public IManagedConditions ManagedConditions; // Generated

        public bool NilWt = false;
        public float AddWt = 0;
        public float SubWt = 0;
    }
}

