using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common
{
    public class EnigmaVariationsRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                // Cycling between black, deep purple, and eerie green
                float time = Main.GameUpdateCount * 0.02f;
                float cycle = (float)System.Math.Sin(time) * 0.5f + 0.5f;
                
                Color black = new Color(15, 10, 20);
                Color purple = new Color(140, 60, 200);
                Color green = new Color(50, 220, 100);
                
                if (cycle < 0.5f)
                    return Color.Lerp(black, purple, cycle * 2f);
                else
                    return Color.Lerp(purple, green, (cycle - 0.5f) * 2f);
            }
        }

        public override int GetPrefixedRarity(int offset, float valueMult)
        {
            return Type;
        }
    }
}
