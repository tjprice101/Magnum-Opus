using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy
{
    /// <summary>
    /// Centralized lazy-loading registry for all 6 Ode to Joy theme shaders.
    /// Each weapon projectile calls into this to get shader references.
    /// Follows foundation weapon pattern: lazy load → cache → null-safe fallback.
    /// </summary>
    public static class OdeToJoyShaders
    {
        // ─── Shader cache fields ───────────────────────────────────────
        private static Effect _triumphantTrail;
        private static Effect _jubilantHarmony;
        private static Effect _verdantSlash;
        private static Effect _pollenDrift;
        private static Effect _gardenBloom;
        private static Effect _celebrationAura;
        private static Effect _smearDistort;

        private static bool _triedTriumphantTrail;
        private static bool _triedJubilantHarmony;
        private static bool _triedVerdantSlash;
        private static bool _triedPollenDrift;
        private static bool _triedGardenBloom;
        private static bool _triedCelebrationAura;
        private static bool _triedSmearDistort;

        // ─── Shader Properties ─────────────────────────────────────────

        /// <summary>
        /// Golden energy trail with scrolling flow + blossom wind variant.
        /// Techniques: TriumphantTrailTechnique, BlossomWindTrailTechnique
        /// Used by: ExordiumBolt, CrystallineThorn, GoldenDroplet, ThrownRose, etc.
        /// </summary>
        public static Effect TriumphantTrail => LoadShader(ref _triumphantTrail, ref _triedTriumphantTrail,
            "MagnumOpus/Effects/OdeToJoy/TriumphantTrail");

        /// <summary>
        /// Harmonic wave beam with standing wave nodes + symphonic aura.
        /// Techniques: HarmonicBeamTechnique, SymphonicAuraTechnique
        /// Used by: AnthemBeam, HarmonicBlast, JoyWave, etc.
        /// </summary>
        public static Effect JubilantHarmony => LoadShader(ref _jubilantHarmony, ref _triedJubilantHarmony,
            "MagnumOpus/Effects/OdeToJoy/JubilantHarmony");

        /// <summary>
        /// Vine-entwined slash trail with thorns + thorn impact burst.
        /// Techniques: VerdantSlashTechnique, ThornImpactTechnique
        /// Used by: ThornboundSwing, GardenerSwing, ChainsawHoldout.
        /// </summary>
        public static Effect VerdantSlash => LoadShader(ref _verdantSlash, ref _triedVerdantSlash,
            "MagnumOpus/Effects/OdeToJoy/VerdantSlash");

        /// <summary>
        /// Floating pollen seed trail + bloom detonation circle.
        /// Techniques: PollenTrailTechnique, BloomDetonationTechnique
        /// Used by: PollenShot, HurricaneShot, PetalCluster, BloomThorn.
        /// </summary>
        public static Effect PollenDrift => LoadShader(ref _pollenDrift, ref _triedPollenDrift,
            "MagnumOpus/Effects/OdeToJoy/PollenDrift");

        /// <summary>
        /// Radial bloom with 5-petal edge shimmer + jubilant pulse aura.
        /// Techniques: GardenBloomTechnique, JubilantPulseTechnique
        /// Used by: ElysianOrb, ApexOrb, PetalVortexZone, ChorusMinion, FountainMinion.
        /// </summary>
        public static Effect GardenBloom => LoadShader(ref _gardenBloom, ref _triedGardenBloom,
            "MagnumOpus/Effects/OdeToJoy/GardenBloom");

        /// <summary>
        /// Concentric expanding rings + rotating floral sigil.
        /// Techniques: CelebrationAuraTechnique, FloralSigilTechnique
        /// Used by: VictoryFanfare, ElysianExplosion, OvationShockwave, Geyser, ThornDetonation.
        /// </summary>
        public static Effect CelebrationAura => LoadShader(ref _celebrationAura, ref _triedCelebrationAura,
            "MagnumOpus/Effects/OdeToJoy/CelebrationAura");

        /// <summary>
        /// SmearDistortShader from SwordSmearFoundation (for melee swings).
        /// </summary>
        public static Effect SmearDistort => LoadShader(ref _smearDistort, ref _triedSmearDistort,
            "MagnumOpus/Content/FoundationWeapons/SwordSmearFoundation/Shaders/SmearDistortShader");

        // ─── Helper Methods ────────────────────────────────────────────

        /// <summary>
        /// Configures trail shader parameters for TriumphantTrail / BlossomWindTrail.
        /// Call before applying the shader technique pass.
        /// </summary>
        public static void SetTrailParams(Effect shader, float time, Color primary, Color secondary,
            float opacity, float intensity)
        {
            if (shader == null) return;
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uOpacity"]?.SetValue(opacity);
            shader.Parameters["uIntensity"]?.SetValue(intensity);
        }

        /// <summary>
        /// Configures beam shader parameters for JubilantHarmony HarmonicBeam.
        /// </summary>
        public static void SetBeamParams(Effect shader, float time, Color primary, Color secondary,
            float opacity, float intensity, float harmonicFreq = 1f)
        {
            if (shader == null) return;
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uOpacity"]?.SetValue(opacity);
            shader.Parameters["uIntensity"]?.SetValue(intensity);
            shader.Parameters["uHarmonicFreq"]?.SetValue(harmonicFreq);
        }

        /// <summary>
        /// Configures bloom/aura shader parameters for GardenBloom / JubilantPulse.
        /// </summary>
        public static void SetBloomParams(Effect shader, float time, Color primary, Color secondary,
            float opacity, float intensity, float radius, float pulseSpeed = 3f)
        {
            if (shader == null) return;
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uOpacity"]?.SetValue(opacity);
            shader.Parameters["uIntensity"]?.SetValue(intensity);
            shader.Parameters["uRadius"]?.SetValue(radius);
            shader.Parameters["uPulseSpeed"]?.SetValue(pulseSpeed);
        }

        /// <summary>
        /// Configures CelebrationAura parameters for expanding ring effects.
        /// </summary>
        public static void SetAuraParams(Effect shader, float time, Color primary, Color secondary,
            float opacity, float intensity, float radius, float ringCount = 4f, float rotation = 0f)
        {
            if (shader == null) return;
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uOpacity"]?.SetValue(opacity);
            shader.Parameters["uIntensity"]?.SetValue(intensity);
            shader.Parameters["uRadius"]?.SetValue(radius);
            shader.Parameters["uRingCount"]?.SetValue(ringCount);
            shader.Parameters["uRotation"]?.SetValue(rotation);
        }

        /// <summary>
        /// Configures PollenDrift trail parameters.
        /// </summary>
        public static void SetPollenParams(Effect shader, float time, Color primary, Color secondary,
            float opacity, float intensity, float radius = 0.4f)
        {
            if (shader == null) return;
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uOpacity"]?.SetValue(opacity);
            shader.Parameters["uIntensity"]?.SetValue(intensity);
            shader.Parameters["uRadius"]?.SetValue(radius);
        }

        /// <summary>
        /// Configures VerdantSlash melee trail parameters.
        /// </summary>
        public static void SetSlashParams(Effect shader, float time, Color primary, Color secondary,
            float opacity, float intensity, float comboProgress = 0f)
        {
            if (shader == null) return;
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uOpacity"]?.SetValue(opacity);
            shader.Parameters["uIntensity"]?.SetValue(intensity);
            shader.Parameters["uComboProgress"]?.SetValue(comboProgress);
        }

        /// <summary>
        /// Begin a SpriteBatch shader pass (Immediate mode for per-draw param control).
        /// Must be called AFTER sb.End().
        /// </summary>
        public static void BeginShaderBatch(SpriteBatch sb, Effect shader, string technique)
        {
            if (shader != null && shader.Techniques[technique] != null)
                shader.CurrentTechnique = shader.Techniques[technique];

            sb.End();
            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Begin a SpriteBatch deferred shader pass (shader as Effect parameter — all draws get it).
        /// Must be called AFTER sb.End().
        /// </summary>
        public static void BeginDeferredShaderBatch(SpriteBatch sb, Effect shader, string technique)
        {
            if (shader != null && shader.Techniques[technique] != null)
                shader.CurrentTechnique = shader.Techniques[technique];

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, shader,
                Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Restore SpriteBatch to standard AlphaBlend state after shader rendering.
        /// </summary>
        public static void RestoreSpriteBatch(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Begin a standard additive SpriteBatch pass (no shader).
        /// </summary>
        public static void BeginAdditiveBatch(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        // ─── Private Helpers ───────────────────────────────────────────

        private static Effect LoadShader(ref Effect cache, ref bool tried, string path)
        {
            if (cache != null) return cache;
            if (tried) return null;
            tried = true;
            try
            {
                cache = ModContent.Request<Effect>(path, AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                cache = null;
            }
            return cache;
        }
    }
}
