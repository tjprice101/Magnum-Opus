using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Shaders
{
    [Autoload(Side = ModSide.Client)]
    public class LamentShaderLoader : ModSystem
    {
        public static bool HasLamentBulletTrailShader { get; private set; }
        public static bool HasDestructionRevelationShader { get; private set; }

        public override void PostSetupContent()
        {
            if (TryLoadMiscShader("MagnumOpus/Effects/SwanLake/TheSwansLament/LamentBulletTrail",
                "P0", "MagnumOpus:LamentBulletTrail"))
                HasLamentBulletTrailShader = true;

            if (TryLoadMiscShader("MagnumOpus/Effects/SwanLake/TheSwansLament/DestructionRevelation",
                "P0", "MagnumOpus:DestructionRevelation"))
                HasDestructionRevelationShader = true;
        }

        private static bool TryLoadMiscShader(string effectPath, string passName, string key)
        {
            try
            {
                var effect = ModContent.Request<Effect>(effectPath, AssetRequestMode.ImmediateLoad);
                if (effect?.Value != null)
                {
                    GameShaders.Misc[key] = new MiscShaderData(effect, passName);
                    return true;
                }
            }
            catch { }
            return false;
        }

        public override void Unload()
        {
            HasLamentBulletTrailShader = false;
            HasDestructionRevelationShader = false;
        }
    }
}
