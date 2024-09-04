using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace OrnithologistsGuild
{
    public class Utilities
    {
        public static T WeightedRandom<T>(IEnumerable<T> values, Func<T, float> getWeight)
        {
            List<float> weights = values.Select(v => getWeight(v)).ToList();

            float random = Utility.RandomFloat(0, weights.Sum());
            float sum = 0;

            for (int i = 0; i < values.Count(); i++)
            {
                var weight = weights.ElementAt(i);
                if (weight == 0) continue;

                if (random < (sum + weight))
                {
                    return values.ElementAt(i);
                }
                else
                {
                    sum += weight;
                }
            }

            return default;
        }

        public static IEnumerable<T> Randomize<T>(IEnumerable<T> source)
        {
            return source.OrderBy((item) => Game1.random.NextDouble());
        }

        public static float EaseOutSine(float x)
        {
            return MathF.Sin((x * MathF.PI) / 2);
        }

        public static string GetLocaleSeparator()
        {
            try
            {
                var locale = ModEntry.Instance.Helper.Translation.Locale;
                return $"{System.Globalization.CultureInfo.GetCultureInfo(locale).TextInfo.ListSeparator} ";
            } catch
            {
                return ", ";
            }
        }

        public static string LocaleToUpper(string value)
        {
            var locale = ModEntry.Instance.Helper.Translation.Locale;
            return System.Globalization.CultureInfo.GetCultureInfo(locale).TextInfo.ToUpper(value);
        }

        public static Vector2 XY(Vector3 value)
        {
            return new Vector2(value.X, value.Y);
        }

        public static bool TryGetNonPublicFieldValue<TInstance, TValue>(TInstance instance, string fieldName, out TValue value)
        {
            FieldInfo privateFieldInfo = typeof(TInstance).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (privateFieldInfo != null)
            {
                value = (TValue)privateFieldInfo.GetValue(instance);
                return true;
            }

            value = default;
            return false;
        }

        public static Texture2D CensorTexture(Texture2D texture)
        {
            // Get the texture data
            Color[] originalData = new Color[texture.Width * texture.Height];
            texture.GetData(originalData);

            // Create an array to hold the new pixel data
            Color[] newData = new Color[originalData.Length];

            for (int i = 0; i < originalData.Length; i++)
            {
                int r = 100;
                int g = 100;
                int b = 100;

                if (originalData[i].A > 0)
                {
                    // Set the new color
                    newData[i] = new Color(r, g, b, originalData[i].A);
                }
                else
                {
                    newData[i] = Color.Transparent;
                }
            }

            // Create a new texture to hold the adjusted data
            Texture2D newTexture = new Texture2D(texture.GraphicsDevice, texture.Width, texture.Height);
            newTexture.SetData(newData);

            return newTexture;
        }
    }
}

