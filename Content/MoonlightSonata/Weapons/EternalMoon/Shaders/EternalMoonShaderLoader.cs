using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Shaders
{
    /// <summary>
    /// Loads and registers all Eternal Moon weapon-specific shaders.
    /// Shader keys:
    ///   "MagnumOpus:EternalMoonTidalTrail"      窶・Tidal wave trail (swing arc)
    ///   "MagnumOpus:EternalMoonTidalGlow"        窶・Tidal trail glow pass (wider bloom underlayer)
    ///   "MagnumOpus:EternalMoonCrescentBloom"    窶・Procedural crescent bloom overlay
    ///   "MagnumOpus:EternalMoonAura"             窶・Concentric lunar phase ring aura
    ///   "MagnumOpus:EternalMoonSwingSprite"      窶・UV-rotation for blade sprite during swing
    ///   "MagnumOpus:EternalMoonSurgeTrail"       窶・Surge dash trail (reuses TidalTrail glow pass)
    ///   "MagnumOpus:EternalMoonStandardPrimitive"窶・Fallback standard primitive shader
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class EternalMoonShaderLoader : ModSystem
    {
        internal static Asset<Effect> TidalTrailShader;
        internal static Asset<Effect> CrescentBloomShader;
        internal static Asset<Effect> LunarPhaseAuraShader;
        internal static Asset<Effect> SwingSpriteShader;

        public override void PostSetupContent()
        {
            var assets = Mod.Assets;
            Asset<Effect> Load(string path) => assets.Request<Effect>(path, AssetRequestMode.ImmediateLoad);

            // Tidal Trail: Main trail pass + glow pass
            TidalTrailShader = Load("Effects/MoonlightSonata/EternalMoon/TidalTrail");
            GameShaders.Misc["MagnumOpus:EternalMoonTidalTrail"] = new MiscShaderData(TidalTrailShader, "P0");
            GameShaders.Misc["MagnumOpus:EternalMoonTidalGlow"] = new MiscShaderData(TidalTrailShader, "P0");

            // Crescent Bloom: bloom overlay + glow pass
            CrescentBloomShader = Load("Effects/MoonlightSonata/EternalMoon/CrescentBloom");
            GameShaders.Misc["MagnumOpus:EternalMoonCrescentBloom"] = new MiscShaderData(CrescentBloomShader, "P0");

            // Lunar Phase Aura: concentric rings
            LunarPhaseAuraShader = Load("Effects/MoonlightSonata/EternalMoon/LunarPhaseAura");
            GameShaders.Misc["MagnumOpus:EternalMoonAura"] = new MiscShaderData(LunarPhaseAuraShader, "P0");

            // Swing sprite shader for UV rotation of blade texture
            // Reuses existing SwingSprite shader from Exoblade (same technique applies)
            SwingSpriteShader = Load("Content/SandboxExoblade/Shaders/SwingSprite");
            ScreenShaderData swingPass = new(SwingSpriteShader, "SwingPass");
            Filters.Scene["MagnumOpus:EternalMoonSwingSprite"] = new Filter(swingPass, EffectPriority.High);
            Filters.Scene["MagnumOpus:EternalMoonSwingSprite"].Load();

            // Surge trail reuses tidal glow pass
            GameShaders.Misc["MagnumOpus:EternalMoonSurgeTrail"] = new MiscShaderData(TidalTrailShader, "P0");

            // Fallback standard primitive
            GameShaders.Misc["MagnumOpus:EternalMoonStandardPrimitive"] = new MiscShaderData(TidalTrailShader, "P0");
        }
    }
}
