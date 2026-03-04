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
                    GameShaders.Misc["MagnumOpus:ChoirMinionTrail"] = new MiscShaderData(fx, "P0");
                    HasMinionTrailShader = true;
                }
            }
            catch
            {
                HasMinionTrailShader = false;
            }

            try
            {
                var fx = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(
                    "MagnumOpus/Effects/LaCampanella/InfernalChimesCalling/MusicalShockwave",
                    AssetRequestMode.ImmediateLoad);
                if (fx?.Value != null)
                {
                    GameShaders.Misc["MagnumOpus:MusicalShockwave"] = new MiscShaderData(fx, "P0");
                    HasShockwaveShader = true;
                }
            }
            catch
            {
                HasShockwaveShader = false;
            }

            try
            {
                var fx = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(
                    "MagnumOpus/Effects/LaCampanella/InfernalChimesCalling/ChoirFlameAura",
                    AssetRequestMode.ImmediateLoad);
                if (fx?.Value != null)
                {
                    GameShaders.Misc["MagnumOpus:ChoirFlameAura"] = new MiscShaderData(fx, "P0");
                    HasFlameAuraShader = true;
                }
            }
            catch
            {
                HasFlameAuraShader = false;
            }
        }

        public static MiscShaderData GetMinionTrailShader()
        {
            if (!HasMinionTrailShader) return null;
            try
            {
                return GameShaders.Misc["MagnumOpus:ChoirMinionTrail"];
            }
            catch
            {
                HasMinionTrailShader = false;
                return null;
            }
        }

        public static MiscShaderData GetShockwaveShader()
        {
            if (!HasShockwaveShader) return null;
            try
            {
                return GameShaders.Misc["MagnumOpus:MusicalShockwave"];
            }
            catch
            {
                HasShockwaveShader = false;
                return null;
            }
        }

        public static MiscShaderData GetFlameAuraShader()
        {
            if (!HasFlameAuraShader) return null;
            try
            {
                return GameShaders.Misc["MagnumOpus:ChoirFlameAura"];
            }
            catch
            {
                HasFlameAuraShader = false;
                return null;
            }
        }
    }
}
