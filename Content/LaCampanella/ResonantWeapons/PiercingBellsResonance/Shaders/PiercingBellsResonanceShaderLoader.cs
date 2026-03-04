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

            HasBulletTrailShader = TryLoadShader(ShaderBasePath + "BulletTrailShader", "PiercingBellsBulletTrail", FallbackTrail);
            HasResonantBlastShader = TryLoadShader(ShaderBasePath + "ResonantBlastShader", "PiercingBellsResonantBlast", FallbackBlast);
            HasCrystalGlowShader = TryLoadShader(ShaderBasePath + "CrystalGlowShader", "PiercingBellsCrystalGlow", FallbackGlow);
        }

        private bool TryLoadShader(string path, string key, string fallbackPath)
        {
            try
            {
                var effect = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(path, AssetRequestMode.ImmediateLoad).Value;
                GameShaders.Misc[key] = new MiscShaderData(new Terraria.Ref<Microsoft.Xna.Framework.Graphics.Effect>(effect), "P0");
                return true;
            }
            catch
            {
                try
                {
                    var fallback = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(fallbackPath, AssetRequestMode.ImmediateLoad).Value;
                    GameShaders.Misc[key] = new MiscShaderData(new Terraria.Ref<Microsoft.Xna.Framework.Graphics.Effect>(fallback), "P0");
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
