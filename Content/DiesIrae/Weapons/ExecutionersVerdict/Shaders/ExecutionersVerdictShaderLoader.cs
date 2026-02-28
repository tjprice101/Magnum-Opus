using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Shaders
{
    /// <summary>
    /// Loads and registers Executioner's Verdict weapon-specific shaders.
    /// GuillotineBlade shader: dark blade slash + execution mark techniques.
    /// Also loads the shared HellfireBloom and JudgmentAura.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class ExecutionersVerdictShaderLoader : ModSystem
    {
        internal static Asset<Effect> GuillotineShader;
        internal static Asset<Effect> HellfireBloomShader;
        internal static Asset<Effect> JudgmentAuraShader;

        public const string GuillotineSlashKey = "MagnumOpus:GuillotineSlash";
        public const string ExecutionMarkKey = "MagnumOpus:ExecutionMark";
        public const string HellfireBloomKey = "MagnumOpus:HellfireBloom";
        public const string JudgmentAuraKey = "MagnumOpus:JudgmentAura";
        public const string StandardPrimitiveKey = "MagnumOpus:VerdictStandardPrimitive";

        public static bool HasGuillotine => GuillotineShader?.IsLoaded == true;
        public static bool HasHellfireBloom => HellfireBloomShader?.IsLoaded == true;
        public static bool HasJudgmentAura => JudgmentAuraShader?.IsLoaded == true;

        public override void PostSetupContent()
        {
            var assets = Mod.Assets;

            // Guillotine Blade shader (per-weapon)
            try
            {
                GuillotineShader = assets.Request<Effect>("Effects/DiesIrae/ExecutionersVerdict/GuillotineBlade", AssetRequestMode.ImmediateLoad);
                if (GuillotineShader?.Value != null)
                {
                    GameShaders.Misc[GuillotineSlashKey] = new MiscShaderData(GuillotineShader, "GuillotineSlashMain");
                    GameShaders.Misc[ExecutionMarkKey] = new MiscShaderData(GuillotineShader, "ExecutionMarkMain");
                    GameShaders.Misc[StandardPrimitiveKey] = new MiscShaderData(GuillotineShader, "GuillotineSlashMain");
                }
            }
            catch { GuillotineShader = null; }

            // Shared Hellfire Bloom
            try
            {
                HellfireBloomShader = assets.Request<Effect>("Effects/DiesIrae/HellfireBloom", AssetRequestMode.ImmediateLoad);
                if (HellfireBloomShader?.Value != null)
                {
                    GameShaders.Misc[HellfireBloomKey] = new MiscShaderData(HellfireBloomShader, "HellfireBloomMain");
                }
            }
            catch { HellfireBloomShader = null; }

            // Shared Judgment Aura
            try
            {
                JudgmentAuraShader = assets.Request<Effect>("Effects/DiesIrae/JudgmentAura", AssetRequestMode.ImmediateLoad);
                if (JudgmentAuraShader?.Value != null)
                {
                    GameShaders.Misc[JudgmentAuraKey] = new MiscShaderData(JudgmentAuraShader, "JudgmentAuraMain");
                }
            }
            catch { JudgmentAuraShader = null; }
        }
    }
}
