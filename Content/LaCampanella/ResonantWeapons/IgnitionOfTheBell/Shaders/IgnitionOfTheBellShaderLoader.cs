using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Shaders
{
    /// <summary>
    /// Loads and manages IgnitionOfTheBell-specific shaders.
    /// - Thrust: Directional flame jet trail for stab projectiles.
    /// - Cyclone: Swirling fire vortex shader for cyclone explosions.
    /// - Geyser: Concentrated column of bell fire for alt-fire charge release.
    /// Falls back to shared La Campanella shaders if per-weapon shaders are absent.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class IgnitionOfTheBellShaderLoader : ModSystem
    {
        public static bool HasThrustShader { get; private set; }
        public static bool HasCycloneShader { get; private set; }
        public static bool HasGeyserShader { get; private set; }

        public override void OnModLoad()
        {
            HasThrustShader = false;
            HasCycloneShader = false;
            HasGeyserShader = false;

            // Thrust trail shader
            try
            {
                var thrustFx = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(
                    "MagnumOpus/Effects/LaCampanella/IgnitionOfTheBell/IgnitionThrustTrail",
                    AssetRequestMode.ImmediateLoad);
                if (thrustFx?.Value != null)
                {
                    GameShaders.Misc["MagnumOpus:IgnitionThrustTrail"] = new MiscShaderData(thrustFx, "P0");
                    HasThrustShader = true;
                }
            }
            catch
            {
                HasThrustShader = false;
            }

            // Cyclone shader
            try
            {
                var cycloneFx = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(
                    "MagnumOpus/Effects/LaCampanella/IgnitionOfTheBell/CycloneFlameShader",
                    AssetRequestMode.ImmediateLoad);
                if (cycloneFx?.Value != null)
                {
                    GameShaders.Misc["MagnumOpus:CycloneFlameShader"] = new MiscShaderData(cycloneFx, "P0");
                    HasCycloneShader = true;
                }
            }
            catch
            {
                HasCycloneShader = false;
            }

            // Geyser shader
            try
            {
                var geyserFx = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(
                    "MagnumOpus/Effects/LaCampanella/IgnitionOfTheBell/InfernalGeyserShader",
                    AssetRequestMode.ImmediateLoad);
                if (geyserFx?.Value != null)
                {
                    GameShaders.Misc["MagnumOpus:InfernalGeyserShader"] = new MiscShaderData(geyserFx, "P0");
                    HasGeyserShader = true;
                }
            }
            catch
            {
                HasGeyserShader = false;
            }
        }

        public static MiscShaderData GetThrustShader()
        {
            if (!HasThrustShader) return null;
            try
            {
                return GameShaders.Misc["MagnumOpus:IgnitionThrustTrail"];
            }
            catch
            {
                HasThrustShader = false;
                return null;
            }
        }

        public static MiscShaderData GetCycloneShader()
        {
            if (!HasCycloneShader) return null;
            try
            {
                return GameShaders.Misc["MagnumOpus:CycloneFlameShader"];
            }
            catch
            {
                HasCycloneShader = false;
                return null;
            }
        }

        public static MiscShaderData GetGeyserShader()
        {
            if (!HasGeyserShader) return null;
            try
            {
                return GameShaders.Misc["MagnumOpus:InfernalGeyserShader"];
            }
            catch
            {
                HasGeyserShader = false;
                return null;
            }
        }
    }
}
