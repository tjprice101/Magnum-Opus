using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Shaders
{
    /// <summary>
    /// Per-weapon shader loader for PiercingBellsResonance.
    /// Loads: BulletTrail (rapid-fire streaks), ResonantBlast (20th-shot explosion), CrystalGlow (seeking crystal aura).
    /// Falls back to shared La Campanella shaders if per-weapon shaders not found.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class PiercingBellsResonanceShaderLoader : ModSystem
    {
        public static bool HasBulletTrailShader { get; private set; }
        public static bool HasResonantBlastShader { get; private set; }
        public static bool HasCrystalGlowShader { get; private set; }

        private const string ShaderBasePath = "MagnumOpus/Effects/LaCampanella/PiercingBellsResonance/";
        private const string FallbackTrail = "MagnumOpus/Effects/ScrollingTrailShader";
        private const string FallbackBlast = "MagnumOpus/Effects/SimpleBloomShader";
        private const string FallbackGlow = "MagnumOpus/Effects/SimpleBloomShader";

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            HasBulletTrailShader = TryLoadShader(ShaderBasePath + "BulletTrailShader", "PiercingBellsBulletTrail", "P0", FallbackTrail, "Pass0");
            HasResonantBlastShader = TryLoadShader(ShaderBasePath + "ResonantBlastShader", "PiercingBellsResonantBlast", "P0", FallbackBlast, "DefaultPass");
            HasCrystalGlowShader = TryLoadShader(ShaderBasePath + "CrystalGlowShader", "PiercingBellsCrystalGlow", "P0", FallbackGlow, "DefaultPass");
        }

        private bool TryLoadShader(string path, string key, string technique, string fallbackPath, string fallbackTechnique)
        {
            try
            {
                var effect = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(path, AssetRequestMode.ImmediateLoad);
                GameShaders.Misc[key] = new MiscShaderData(effect, technique);
                return true;
            }
            catch
            {
                try
                {
                    var fallback = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(fallbackPath, AssetRequestMode.ImmediateLoad);
                    GameShaders.Misc[key] = new MiscShaderData(fallback, fallbackTechnique);
                    return true;
                }
                catch { }
                return false;
            }
        }

        public static MiscShaderData GetBulletTrailShader()
        {
            if (!HasBulletTrailShader) return null;
            try
            {
                return GameShaders.Misc["PiercingBellsBulletTrail"];
            }
            catch
            {
                HasBulletTrailShader = false;
                return null;
            }
        }

        public static MiscShaderData GetResonantBlastShader()
        {
            if (!HasResonantBlastShader) return null;
            try
            {
                return GameShaders.Misc["PiercingBellsResonantBlast"];
            }
            catch
            {
                HasResonantBlastShader = false;
                return null;
            }
        }

        public static MiscShaderData GetCrystalGlowShader()
        {
            if (!HasCrystalGlowShader) return null;
            try
            {
                return GameShaders.Misc["PiercingBellsCrystalGlow"];
            }
            catch
            {
                HasCrystalGlowShader = false;
                return null;
            }
        }
    }
}
