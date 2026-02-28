using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Shaders
{
    [Autoload(Side = ModSide.Client)]
    public class InfernalChimesCallingShaderLoader : ModSystem
    {
        public static bool HasMinionTrailShader { get; private set; }
        public static bool HasShockwaveShader { get; private set; }
        public static bool HasFlameAuraShader { get; private set; }

        public override void OnModLoad()
        {
            HasMinionTrailShader = false;
            HasShockwaveShader = false;
            HasFlameAuraShader = false;

            try
            {
                var fx = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(
                    "MagnumOpus/Effects/LaCampanella/InfernalChimesCalling/ChoirMinionTrail",
                    AssetRequestMode.ImmediateLoad);
                if (fx?.Value != null)
                {
                    GameShaders.Misc["MagnumOpus:ChoirMinionTrail"] = new MiscShaderData(fx, "TrailPass");
                    HasMinionTrailShader = true;
                }
            }
            catch
            {
                try { if (GameShaders.Misc.ContainsKey("MagnumOpus:HeroicFlameTrail")) HasMinionTrailShader = true; } catch { }
            }

            try
            {
                var fx = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(
                    "MagnumOpus/Effects/LaCampanella/InfernalChimesCalling/MusicalShockwave",
                    AssetRequestMode.ImmediateLoad);
                if (fx?.Value != null)
                {
                    GameShaders.Misc["MagnumOpus:MusicalShockwave"] = new MiscShaderData(fx, "TrailPass");
                    HasShockwaveShader = true;
                }
            }
            catch
            {
                try { if (GameShaders.Misc.ContainsKey("MagnumOpus:RadialScrollShader")) HasShockwaveShader = true; } catch { }
            }

            try
            {
                var fx = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(
                    "MagnumOpus/Effects/LaCampanella/InfernalChimesCalling/ChoirFlameAura",
                    AssetRequestMode.ImmediateLoad);
                if (fx?.Value != null)
                {
                    GameShaders.Misc["MagnumOpus:ChoirFlameAura"] = new MiscShaderData(fx, "TrailPass");
                    HasFlameAuraShader = true;
                }
            }
            catch
            {
                try { if (GameShaders.Misc.ContainsKey("MagnumOpus:RadialScrollShader")) HasFlameAuraShader = true; } catch { }
            }
        }

        public static MiscShaderData GetMinionTrailShader()
        {
            if (!HasMinionTrailShader) return null;
            try
            {
                if (GameShaders.Misc.TryGetValue("MagnumOpus:ChoirMinionTrail", out var s)) return s;
                if (GameShaders.Misc.TryGetValue("MagnumOpus:HeroicFlameTrail", out s)) return s;
            }
            catch { }
            return null;
        }

        public static MiscShaderData GetShockwaveShader()
        {
            if (!HasShockwaveShader) return null;
            try
            {
                if (GameShaders.Misc.TryGetValue("MagnumOpus:MusicalShockwave", out var s)) return s;
                if (GameShaders.Misc.TryGetValue("MagnumOpus:RadialScrollShader", out s)) return s;
            }
            catch { }
            return null;
        }

        public static MiscShaderData GetFlameAuraShader()
        {
            if (!HasFlameAuraShader) return null;
            try
            {
                if (GameShaders.Misc.TryGetValue("MagnumOpus:ChoirFlameAura", out var s)) return s;
                if (GameShaders.Misc.TryGetValue("MagnumOpus:RadialScrollShader", out s)) return s;
            }
            catch { }
            return null;
        }
    }
}
