using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace MagnumOpus.Content.SandboxLastPrism
{
    class SLPCommonTextures
    {
        #region OrbsFolder
        public static readonly string OrbLoc = "MagnumOpus/Assets/SandboxLastPrism/Orbs/";

        public static readonly Asset<Texture2D> circle_05 = ModContent.Request<Texture2D>(OrbLoc + "circle_05", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> feather_circle128PMA = ModContent.Request<Texture2D>(OrbLoc + "feather_circle128PMA", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> SoftGlow = ModContent.Request<Texture2D>(OrbLoc + "SoftGlow", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> SoftGlow64 = ModContent.Request<Texture2D>(OrbLoc + "SoftGlow64", AssetRequestMode.ImmediateLoad);

        #endregion

        #region PixelFolder
        public static readonly string PixelLoc = "MagnumOpus/Assets/SandboxLastPrism/Pixel/";

        public static readonly Asset<Texture2D> Flare = ModContent.Request<Texture2D>(PixelLoc + "Flare", AssetRequestMode.ImmediateLoad);

        #endregion

        #region TrailFolder
        public static readonly string TrailLoc = "MagnumOpus/Assets/SandboxLastPrism/Trails/";

        public static readonly Asset<Texture2D> EnergyTex = ModContent.Request<Texture2D>(TrailLoc + "EnergyTex", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> Extra_196_Black = ModContent.Request<Texture2D>(TrailLoc + "Extra_196_Black", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> spark_06 = ModContent.Request<Texture2D>(TrailLoc + "spark_06", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> ThinGlowLine = ModContent.Request<Texture2D>(TrailLoc + "ThinGlowLine", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> Trail5Loop = ModContent.Request<Texture2D>(TrailLoc + "Trail5Loop", AssetRequestMode.ImmediateLoad);

        #endregion
    }
}
