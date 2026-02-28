using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Shaders
{
    [Autoload(Side = ModSide.Client)]
    public class FangOfTheInfiniteBellShaderLoader : ModSystem
    {
        public static bool HasOrbShader { get; private set; }
        public static bool HasLightningShader { get; private set; }
        public static bool HasAuraShader { get; private set; }

        public override void OnModLoad()
        {
            HasOrbShader = false;
            HasLightningShader = false;
            HasAuraShader = false;

            try
            {
                var fx = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(
                    "MagnumOpus/Effects/LaCampanella/FangOfTheInfiniteBell/ArcaneOrbTrail",
                    AssetRequestMode.ImmediateLoad);
                if (fx?.Value != null)
                {
                    GameShaders.Misc["MagnumOpus:ArcaneOrbTrail"] = new MiscShaderData(fx, "TrailPass");
                    HasOrbShader = true;
                }
            }
            catch
            {
                try { if (GameShaders.Misc.ContainsKey("MagnumOpus:HeroicFlameTrail")) HasOrbShader = true; } catch { }
            }

            try
            {
                var fx = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(
                    "MagnumOpus/Effects/LaCampanella/FangOfTheInfiniteBell/EmpoweredLightning",
                    AssetRequestMode.ImmediateLoad);
                if (fx?.Value != null)
                {
                    GameShaders.Misc["MagnumOpus:EmpoweredLightning"] = new MiscShaderData(fx, "TrailPass");
                    HasLightningShader = true;
                }
            }
            catch
            {
                try { if (GameShaders.Misc.ContainsKey("MagnumOpus:ScrollingTrailShader")) HasLightningShader = true; } catch { }
            }

            try
            {
                var fx = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(
                    "MagnumOpus/Effects/LaCampanella/FangOfTheInfiniteBell/EmpoweredAura",
                    AssetRequestMode.ImmediateLoad);
                if (fx?.Value != null)
                {
                    GameShaders.Misc["MagnumOpus:FangEmpoweredAura"] = new MiscShaderData(fx, "TrailPass");
                    HasAuraShader = true;
                }
            }
            catch
            {
                try { if (GameShaders.Misc.ContainsKey("MagnumOpus:RadialScrollShader")) HasAuraShader = true; } catch { }
            }
        }

        public static MiscShaderData GetOrbShader()
        {
            if (!HasOrbShader) return null;
            try
            {
                if (GameShaders.Misc.TryGetValue("MagnumOpus:ArcaneOrbTrail", out var s)) return s;
                if (GameShaders.Misc.TryGetValue("MagnumOpus:HeroicFlameTrail", out s)) return s;
            }
            catch { }
            return null;
        }

        public static MiscShaderData GetLightningShader()
        {
            if (!HasLightningShader) return null;
            try
            {
                if (GameShaders.Misc.TryGetValue("MagnumOpus:EmpoweredLightning", out var s)) return s;
                if (GameShaders.Misc.TryGetValue("MagnumOpus:ScrollingTrailShader", out s)) return s;
            }
            catch { }
            return null;
        }

        public static MiscShaderData GetAuraShader()
        {
            if (!HasAuraShader) return null;
            try
            {
                if (GameShaders.Misc.TryGetValue("MagnumOpus:FangEmpoweredAura", out var s)) return s;
                if (GameShaders.Misc.TryGetValue("MagnumOpus:RadialScrollShader", out s)) return s;
            }
            catch { }
            return null;
        }
    }
}
