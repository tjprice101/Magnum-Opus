using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract.Utilities
{
    public static class ContractUtils
    {
        public static readonly Color AbyssBlack = new Color(8, 4, 4);
        public static readonly Color DemonCrimson = new Color(180, 15, 10);
        public static readonly Color WrathFlame = new Color(255, 70, 10);
        public static readonly Color ContractGold = new Color(255, 160, 30);
        public static readonly Color HellcoreWhite = new Color(255, 235, 210);

        public static readonly Color[] ContractPalette = { AbyssBlack, DemonCrimson, WrathFlame, ContractGold, HellcoreWhite };

        public static Color GetContractColor(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (ContractPalette.Length - 1);
            int low = (int)scaled;
            int high = Math.Min(low + 1, ContractPalette.Length - 1);
            return Color.Lerp(ContractPalette[low], ContractPalette[high], scaled - low);
        }

        public static Color Additive(Color c, float alpha) => new Color(c.R, c.G, c.B, 0) * alpha;
    }
}
