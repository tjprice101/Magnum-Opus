using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.TriumphantFractal.Shaders
{
    public class FractalShaderLoader : ModSystem
    {
        // TriumphantFractalShader.fx passes
        public const string FractalEnergyTrailKey = "MagnumOpus:FractalEnergyTrail";
        public const string FractalGlowPassKey = "MagnumOpus:FractalGlowPass";

        // SacredGeometry.fx passes
        public const string SacredGeometryMainKey = "MagnumOpus:SacredGeometryMain";
        public const string SacredGeometryGlowKey = "MagnumOpus:SacredGeometryGlow";

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            // TriumphantFractalShader — fractal energy trail with glow
            var fractalTrail = ModContent.Request<Effect>("MagnumOpus/Effects/Eroica/TriumphantFractal/TriumphantFractalShader", AssetRequestMode.ImmediateLoad);
            GameShaders.Misc[FractalEnergyTrailKey] = new MiscShaderData(fractalTrail, "FractalEnergyTrail");
            GameShaders.Misc[FractalGlowPassKey] = new MiscShaderData(fractalTrail, "FractalGlowPass");

            // SacredGeometry — sacred geometry overlay with glow
            var sacredGeometry = ModContent.Request<Effect>("MagnumOpus/Effects/Eroica/TriumphantFractal/SacredGeometry", AssetRequestMode.ImmediateLoad);
            GameShaders.Misc[SacredGeometryMainKey] = new MiscShaderData(sacredGeometry, "SacredGeometryMain");
            GameShaders.Misc[SacredGeometryGlowKey] = new MiscShaderData(sacredGeometry, "SacredGeometryGlow");
        }
    }
}
