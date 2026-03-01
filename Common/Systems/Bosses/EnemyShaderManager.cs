using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Common.Systems.Bosses
{
    /// <summary>
    /// Manages enemy-specific shader keys and access. Each mini-boss enemy gets
    /// 2 dedicated shaders: an ambient aura and a movement/attack trail.
    /// Uses the same ShaderLoader pipeline as boss shaders.
    /// </summary>
    public class EnemyShaderManager : ModSystem
    {
        #region Shader Keys

        // ─── Moonlight Sonata — Waning Deer ────────────────────
        public const string WaningDeerLunarAura = "MoonlightSonata/Enemy/WaningDeerLunarAura";
        public const string WaningDeerMoonbeamTrail = "MoonlightSonata/Enemy/WaningDeerMoonbeamTrail";

        // ─── La Campanella — Crawler of the Bell ────────────────
        public const string CrawlerBellAura = "LaCampanella/Enemy/CrawlerBellAura";
        public const string CrawlerInfernalTrail = "LaCampanella/Enemy/CrawlerInfernalTrail";

        // ─── Swan Lake — Shattered Prima ────────────────────────
        public const string PrimaFeatherAura = "SwanLake/Enemy/PrimaFeatherAura";
        public const string PrimaGraceTrail = "SwanLake/Enemy/PrimaGraceTrail";

        // ─── Eroica — Eroican Centurion ─────────────────────────
        public const string CenturionValorAura = "Eroica/Enemy/CenturionValorAura";
        public const string CenturionChargeTrail = "Eroica/Enemy/CenturionChargeTrail";

        // ─── Eroica — Behemoth of Valor ─────────────────────────
        public const string BehemothWarAura = "Eroica/Enemy/BehemothWarAura";
        public const string BehemothSlamWave = "Eroica/Enemy/BehemothSlamWave";

        // ─── Eroica — Funeral Blitzer ───────────────────────────
        public const string BlitzerFuneralAura = "Eroica/Enemy/BlitzerFuneralAura";
        public const string BlitzerExplosionFlash = "Eroica/Enemy/BlitzerExplosionFlash";

        // ─── Eroica — Stolen Valor ──────────────────────────────
        public const string StolenValorAura = "Eroica/Enemy/StolenValorAura";
        public const string StolenValorMinionLink = "Eroica/Enemy/StolenValorMinionLink";

        // ─── Enigma Variations — Mystery's End ──────────────────
        public const string MysteryVoidAura = "EnigmaVariations/Enemy/MysteryVoidAura";
        public const string MysteryParadoxTrail = "EnigmaVariations/Enemy/MysteryParadoxTrail";

        // ─── Fate — Herald of Fate ──────────────────────────────
        public const string HeraldCosmicAura = "Fate/Enemy/HeraldCosmicAura";
        public const string HeraldConstellationTrail = "Fate/Enemy/HeraldConstellationTrail";

        #endregion

        #region Shader Access

        /// <summary>
        /// Gets an enemy shader by key. Returns null if shader not loaded.
        /// </summary>
        public static Effect GetShader(string key)
        {
            return ShaderLoader.GetShader(key);
        }

        /// <summary>
        /// Applies standard aura parameters to an enemy shader.
        /// </summary>
        public static void ApplyAuraParams(Effect shader, NPC npc, Color primary, Color secondary, float intensity)
        {
            if (shader == null) return;

            shader.Parameters["uCenter"]?.SetValue(npc.Center - Main.screenPosition);
            shader.Parameters["uRadius"]?.SetValue(Math.Max(npc.width, npc.height) * 0.6f);
            shader.Parameters["uIntensity"]?.SetValue(intensity);
            shader.Parameters["uPrimaryColor"]?.SetValue(primary.ToVector4());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector4());
            shader.Parameters["uTime"]?.SetValue((float)Main.gameTimeCache.TotalGameTime.TotalSeconds);
        }

        /// <summary>
        /// Applies standard trail parameters to an enemy shader.
        /// </summary>
        public static void ApplyTrailParams(Effect shader, NPC npc, Color primary, Color secondary, float intensity)
        {
            if (shader == null) return;

            shader.Parameters["uCenter"]?.SetValue(npc.Center - Main.screenPosition);
            shader.Parameters["uRadius"]?.SetValue(npc.velocity.Length() * 3f);
            shader.Parameters["uIntensity"]?.SetValue(intensity);
            shader.Parameters["uPrimaryColor"]?.SetValue(primary.ToVector4());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector4());
            shader.Parameters["uTime"]?.SetValue((float)Main.gameTimeCache.TotalGameTime.TotalSeconds);
        }

        #endregion
    }
}
