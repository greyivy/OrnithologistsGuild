using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ContentPatcher;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using OrnithologistsGuild.Game;
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
                if (cueDef.sounds.Count > 0)
                    cueDef.sounds[0].volume = ConfigManager.Config.CallVolume / 100f;

                Game1.soundBank.AddCue(cueDef);
            }
        }

        public void ParseConditions()
        {
            foreach (var condition in this.Conditions)
            {
                condition.ManagedConditions = ModEntry.CP.ParseConditions(
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
        public string[] VariantIDs = new string[0];

        public string UniqueID; // Generated
        public string AssetPath;

        public string SoundAssetPath;
        public string SoundID; // Generated

        public int BaseFrame = 0;

        public int BathingClipBottom = 8;

        public ContentPackDef ContentPackDef; // Generated

        public int Attributes;

        public int MaxFlockSize = 1;
        public int Cautiousness = 5;
        public int FlapDuration = 500;
        public float FlySpeed = 5f;

        public float BaseWt = 0.5f;
        public Dictionary<string, float> FoodBaseWts = new Dictionary<string, float>() { };
        public Dictionary<string, float> FeederBaseWts = new Dictionary<string, float>() { };
        public List<BirdDefCondition> Conditions;

        public bool CanUseBaths = true;
        public bool CanNestInTrees = true;

        public float LandPreference = 1f;
        public float PerchPreference = 1f;
        public float WaterPreference = 0f;

        public float GetContextualWeight(bool updateContext = true, GameLocation gameLocation = null, FeederProperties feederProperties = null, FoodDef foodDef = null, bool debug = false)
        {
            var debugLines = new List<(string, double?)>();

            void PrintDebug()
            {
                ModEntry.Instance.Monitor.Log($"ogc: {this.ID}", LogLevel.Info);
                var maxDescriptionLength = Math.Max(56, debugLines.Select(tuple => tuple.Item1.Length).Max());

                for (var i = 0; i < debugLines.Count; i++)
                {
                    var line = debugLines[i];
                    var prefix = "    ";

                    if (i > 0)
                    {
                        if (!line.Item2.HasValue) prefix = " nil";
                        else if (i == debugLines.Count - 1) prefix = "   =";
                        else if (line.Item2.HasValue) prefix = line.Item2 >= 0 ? "   +" : "   -";
                    }

                    var value = line.Item2.HasValue ? line.Item2.Value.ToString("0.00") : "condition blocked";

                    ModEntry.Instance.Monitor.Log($"{prefix} {line.Item1.PadRight(maxDescriptionLength + 3, '.')} {value}", LogLevel.Info);
                }
            }

            try
            {
                if (debug) debugLines.Add(("base weight", BaseWt));
                float weight = this.BaseWt;

                if (gameLocation != null)
                {
                    var nestBirdieDefs = gameLocation.GetTreesWithNests().Select(tree => tree.GetNest().Owner);
                    if (nestBirdieDefs.Contains(this))
                    {
                        if (debug) debugLines.Add(("nest(s) present", 0.25f));
                        weight += 0.25f;
                    }
                }

                if (feederProperties != null)
                {
                    if (this.CanPerchAt(feederProperties))
                    {
                        if (debug) debugLines.Add(($"feeder ({feederProperties.Type})", this.FeederBaseWts[feederProperties.Type]));
                        weight += this.FeederBaseWts[feederProperties.Type];
                    }
                    else
                    {
                        if (debug) debugLines.Add(($"feeder ({feederProperties.Type})", null));
                        return 0; // Bird does not eat at feeder
                    }
                }

                if (foodDef != null)
                {
                    if (this.CanEat(foodDef))
                    {
                        if (debug) debugLines.Add(($"food ({foodDef.Type})", this.FoodBaseWts[foodDef.Type]));
                        weight += this.FoodBaseWts[foodDef.Type];
                    }
                    else
                    {
                        if (debug) debugLines.Add(($"food ({foodDef.Type})", null));
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
                            if (debug) debugLines.Add((string.Join(", ", condition.When.Keys), null));
                            return 0; // Bird not added
                        }

                        if (debug) debugLines.Add((string.Join(", ", condition.When.Select(kv => $"{kv.Key}={kv.Value}")), condition.AddWt + condition.SubWt));

                        weight += condition.AddWt;
                        weight -= condition.SubWt;
                    }
                }

                if (debug) debugLines.Add(("TOTAL", MathHelper.Clamp(weight, 0, 1)));
                return MathHelper.Clamp(weight, 0, 1);
            }
            finally
            {
                if (debug) PrintDebug();
            }
        }

        public int GetContextualCautiousness()
        {
            var modifier = -Math.Clamp((int)Math.Round(Game1.player.DailyLuck * 10), -1, 1);

            return this.Cautiousness + modifier;
        }

        public bool CanPerchAt(FeederProperties feederProperties)
        {
            return FeederBaseWts.ContainsKey(feederProperties.Type);
        }

        public bool CanPerchAt(Perch perch)
        {
            if (PerchPreference == 0) return false;

            if (perch.Type == PerchType.Feeder) return CanPerchAt(perch.Feeder.GetFeederProperties());
            else if (perch.Type == PerchType.Bath) return CanUseBaths;

            return PerchPreference > 0;
        }

        public bool CanEat(FoodDef foodDef)
        {
            return FoodBaseWts.ContainsKey(foodDef.Type);
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

