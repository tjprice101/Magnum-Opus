using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Shaders
{
    /// <summary>
    /// Self-contained shader loader for Requiem of Reality.
    /// Loads 4 unique shaders from Effects/Fate/RequiemOfReality/:
    ///
    ///   1. RequiemSwingTrail      -- Main melee swing arc trail with cosmic fire
    ///   2. RequiemNoteTrail       -- Seeking music note projectile trail
    ///   3. RequiemComboAura       -- Radial aura for spectral blade combo trigger
    ///   4. RequiemImpactBloom     -- Directional impact flash/bloom
    ///
    /// Keys: "MagnumOpus:Requiem<Purpose>"
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class RequiemShaderLoader : ModSystem
    {
        internal static Asset<Effect> SwingTrailShader;
        internal static Asset<Effect> NoteTrailShader;
        internal static Asset<Effect> ComboAuraShader;
        internal static Asset<Effect> ImpactBloomShader;

        public static bool HasSwingTrail { get; private set; }
        public static bool HasNoteTrail { get; private set; }
        public static bool HasComboAura { get; private set; }
        public static bool HasImpactBloom { get; private set; }

        private const string BasePath = "MagnumOpus/Effects/Fate/RequiemOfReality/";

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            HasSwingTrail = TryLoad(BasePath + "RequiemSwingTrail", "RequiemSwingMain",
                "MagnumOpus:RequiemSwingTrail", out SwingTrailShader);

            HasNoteTrail = TryLoad(BasePath + "RequiemNoteTrail", "NoteTrailMain",
                "MagnumOpus:RequiemNoteTrail", out NoteTrailShader);

            HasComboAura = TryLoad(BasePath + "RequiemComboAura", "ComboAuraMain",
                "MagnumOpus:RequiemComboAura", out ComboAuraShader);

            HasImpactBloom = TryLoad(BasePath + "RequiemImpactBloom", "ImpactBloomMain",
                "MagnumOpus:RequiemImpactBloom", out ImpactBloomShader);

            // Register alternate passes from the same shader files
            if (HasSwingTrail)
            {
                // Wide glow underlayer pass from same shader
                GameShaders.Misc["MagnumOpus:RequiemSwingGlow"] =
                    new MiscShaderData(SwingTrailShader, "P0");
            }
        }

        private static bool TryLoad(string path, string passName, string key, out Asset<Effect> asset)
        {
            asset = null;
            try
            {
                asset = ModContent.Request<Effect>(path, AssetRequestMode.ImmediateLoad);
                if (asset?.Value != null)
                {
                    GameShaders.Misc[key] = new MiscShaderData(asset, passName);
                    return true;
                }
            }
            catch { }

            // Fallback: try shared scrolling trail shader
            try
            {
                var fallback = ModContent.Request<Effect>("MagnumOpus/Effects/ScrollingTrailShader", AssetRequestMode.ImmediateLoad);
                if (fallback?.Value != null)
                {
                    GameShaders.Misc[key] = new MiscShaderData(fallback, "P0");
                    return true;
                }
            }
            catch { }

            return false;
        }

        /// <summary>Get the swing trail shader (main pass) or null.</summary>
        public static MiscShaderData GetSwingTrail()
        {
            if (!HasSwingTrail) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:RequiemSwingTrail", out var s);
            return s;
        }

        /// <summary>Get the swing glow underlayer shader or null.</summary>
        public static MiscShaderData GetSwingGlow()
        {
            if (!HasSwingTrail) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:RequiemSwingGlow", out var s);
            return s;
        }

        /// <summary>Get the music note trail shader or null.</summary>
        public static MiscShaderData GetNoteTrail()
        {
            if (!HasNoteTrail) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:RequiemNoteTrail", out var s);
            return s;
        }

        /// <summary>Get the combo aura shader or null.</summary>
        public static MiscShaderData GetComboAura()
        {
            if (!HasComboAura) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:RequiemComboAura", out var s);
            return s;
        }

        /// <summary>Get the impact bloom shader or null.</summary>
        public static MiscShaderData GetImpactBloom()
        {
            if (!HasImpactBloom) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:RequiemImpactBloom", out var s);
            return s;
        }
    }
}
