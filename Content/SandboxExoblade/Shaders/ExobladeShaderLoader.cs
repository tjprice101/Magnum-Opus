using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SandboxExoblade.Shaders
{
    [Autoload(Side = ModSide.Client)]
    public sealed class ExobladeShaderLoader : ModSystem
    {
        internal static Asset<Effect> ExobladeSlashShader;
        internal static Asset<Effect> ExobladePierceShader;
        internal static Asset<Effect> SwingSpriteShader;

        public override void PostSetupContent()
        {
            var assets = Mod.Assets;
            Asset<Effect> LoadShader(string path) => assets.Request<Effect>(path, AssetRequestMode.ImmediateLoad);

            ExobladeSlashShader = LoadShader("Content/SandboxExoblade/Shaders/ExobladeSlashShader");
            MiscShaderData slashPass = new(ExobladeSlashShader, "TrailPass");
            GameShaders.Misc["MagnumOpus:ExobladeSlash"] = slashPass;

            ExobladePierceShader = LoadShader("Content/SandboxExoblade/Shaders/ExobladePierceShader");
            MiscShaderData piercePass = new(ExobladePierceShader, "PiercePass");
            GameShaders.Misc["MagnumOpus:ExobladePierce"] = piercePass;

            SwingSpriteShader = LoadShader("Content/SandboxExoblade/Shaders/SwingSprite");
            ScreenShaderData swingPass = new(SwingSpriteShader, "SwingPass");
            Filters.Scene["MagnumOpus:ExobladeSwingSprite"] = new Filter(swingPass, EffectPriority.High);
            Filters.Scene["MagnumOpus:ExobladeSwingSprite"].Load();

            // Register a standard primitive shader for fallback
            GameShaders.Misc["MagnumOpus:ExobladeStandardPrimitive"] = slashPass;
        }
    }
}
