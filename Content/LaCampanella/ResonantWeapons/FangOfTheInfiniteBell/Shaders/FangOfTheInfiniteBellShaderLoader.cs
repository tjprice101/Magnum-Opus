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
                    GameShaders.Misc["MagnumOpus:ArcaneOrbTrail"] = new MiscShaderData(fx, "P0");
                    HasOrbShader = true;
                }
            }
            catch
            {
                HasOrbShader = false;
            }

            try
            {
                var fx = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(
                    "MagnumOpus/Effects/LaCampanella/FangOfTheInfiniteBell/EmpoweredLightning",
                    AssetRequestMode.ImmediateLoad);
                if (fx?.Value != null)
                {
                    GameShaders.Misc["MagnumOpus:EmpoweredLightning"] = new MiscShaderData(fx, "P0");
                    HasLightningShader = true;
                }
            }
            catch
            {
                HasLightningShader = false;
            }

            try
            {
                var fx = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(
                    "MagnumOpus/Effects/LaCampanella/FangOfTheInfiniteBell/EmpoweredAura",
                    AssetRequestMode.ImmediateLoad);
                if (fx?.Value != null)
                {
                    GameShaders.Misc["MagnumOpus:FangEmpoweredAura"] = new MiscShaderData(fx, "P0");
                    HasAuraShader = true;
                }
            }
            catch
            {
                HasAuraShader = false;
            }
        }

        public static MiscShaderData GetOrbShader()
        {
            if (!HasOrbShader) return null;
            try
            {
                return GameShaders.Misc["MagnumOpus:ArcaneOrbTrail"];
            }
            catch
            {
                HasOrbShader = false;
                return null;
            }
        }

        public static MiscShaderData GetLightningShader()
        {
            if (!HasLightningShader) return null;
            try
            {
                return GameShaders.Misc["MagnumOpus:EmpoweredLightning"];
            }
            catch
            {
                HasLightningShader = false;
                return null;
            }
        }

        public static MiscShaderData GetAuraShader()
        {
            if (!HasAuraShader) return null;
            try
            {
                return GameShaders.Misc["MagnumOpus:FangEmpoweredAura"];
            }
            catch
            {
                HasAuraShader = false;
                return null;
            }
        }
    }
}
