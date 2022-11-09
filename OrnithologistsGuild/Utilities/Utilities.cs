using System;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace OrnithologistsGuild
{
    public class Utilities
    {
        public static T WeightedRandom<T>(IEnumerable<T> values, Func<T, int> getWeight)
        {
            IEnumerable<int> weights = values.Select(v => getWeight(v));

            int random = Game1.random.Next(0, weights.Sum());
            int sum = 0;

            for (int i = 0; i < values.Count(); i++)
            {
                var weight = weights.ElementAt(i);

                if (random < (sum + weight))
                {
                    return values.ElementAt(i);
                }
                else
                {
                    sum += weight;
                }
            }

            return values.Last();
        }

        public static float EaseOutSine(float x)
        {
            return MathF.Sin((x * MathF.PI) / 2);
        }
    }
}

