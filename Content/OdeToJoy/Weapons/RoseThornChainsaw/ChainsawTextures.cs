using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw
{
    /// <summary>
    /// Self-contained texture registry for Rose Thorn Chainsaw.
    /// </summary>
    internal static class ChainsawTextures
    {
        private static readonly string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string NoiseLib = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";
        private static readonly string GradientLib = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";
        private static readonly string Impacts = "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/";
        private static readonly string SlashArcs = "MagnumOpus/Assets/VFX Asset Library/SlashArcs/";
        private static readonly string ThemeOJ = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/";

        public static readonly Asset<Texture2D> SoftGlow =
            ModContent.Request<Texture2D>(Bloom + "SoftGlow", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> GlowOrb =
            ModContent.Request<Texture2D>(Bloom + "GlowOrb", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> StarFlare =
            ModContent.Request<Texture2D>(Bloom + "StarFlare", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> SoftRadialBloom =
            ModContent.Request<Texture2D>(Bloom + "SoftRadialBloom", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> VoronoiCell =
            ModContent.Request<Texture2D>(NoiseLib + "VoronoiCellNoise", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> FBMNoise =
            ModContent.Request<Texture2D>(NoiseLib + "TileableFBMNoise", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradOdeToJoy =
            ModContent.Request<Texture2D>(GradientLib + "OdeToJoyGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> PowerRing =
            ModContent.Request<Texture2D>(Impacts + "PowerEffectRing", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> SwordArcSmear =
            ModContent.Request<Texture2D>(SlashArcs + "SwordArcSmear", AssetRequestMode.ImmediateLoad);

        // Theme-specific
        public static readonly Asset<Texture2D> OJThornFragment =
            ModContent.Request<Texture2D>(ThemeOJ + "Particles/OJ Thorn Fragment", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> OJRosePetal =
            ModContent.Request<Texture2D>(ThemeOJ + "Projectiles/OJ Rose Petal", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> OJBlossomSparkle =
            ModContent.Request<Texture2D>(ThemeOJ + "Particles/OJ Blossom Sparkle", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> OJPowerEffectRing =
            ModContent.Request<Texture2D>(ThemeOJ + "Impact Effects/OJ Power Effect Ring", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> OJTrail =
            ModContent.Request<Texture2D>(ThemeOJ + "Trails and Ribbons/OJ Basic Trail", AssetRequestMode.ImmediateLoad);

        // Smear shader
        private static Effect _smearShader;
        private static bool _smearLoaded;
        public static Effect SmearDistortShader
        {
            get
            {
                if (!_smearLoaded)
                {
                    _smearLoaded = true;
                    try
                    {
                        _smearShader = ModContent.Request<Effect>(
                            "MagnumOpus/Content/FoundationWeapons/SwordSmearFoundation/Shaders/SmearDistortShader",
                            AssetRequestMode.ImmediateLoad).Value;
                    }
                    catch { _smearShader = null; }
                }
                return _smearShader;
            }
        }

        // ODE TO JOY PALETTE
        public static readonly Color RoseShadow = new Color(100, 30, 50);
        public static readonly Color PetalPink = new Color(220, 100, 120);
        public static readonly Color BloomGold = new Color(255, 200, 50);
        public static readonly Color RadiantAmber = new Color(255, 170, 40);
        public static readonly Color JubilantLight = new Color(255, 250, 200);
        public static readonly Color PureJoyWhite = new Color(255, 255, 240);
        public static readonly Color LoreColor = new Color(255, 200, 50);
    }
}
