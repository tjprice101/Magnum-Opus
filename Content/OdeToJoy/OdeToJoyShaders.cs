using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy
{
    /// <summary>
    /// Centralized lazy-loading registry for all Ode to Joy theme and per-weapon shaders.
    /// Each weapon projectile calls into this to get shader references.
    /// Follows foundation weapon pattern: lazy load → cache → null-safe fallback.
    /// </summary>
    public static class OdeToJoyShaders
    {
        // ─── Theme shader cache fields ─────────────────────────────────
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

        // ─── Per-weapon shader cache fields ────────────────────────────
        // Melee
        private static Effect _verdantReckoningTrail;
        private static Effect _gardenGrowthTrail;
        private static Effect _roseChainTrail;
        private static bool _triedVerdantReckoningTrail;
        private static bool _triedGardenGrowthTrail;
        private static bool _triedRoseChainTrail;

        // Ranged
        private static Effect _crystallineThornTrail;
        private static Effect _pollenFieldTrail;
        private static Effect _petalVortexField;
        private static bool _triedCrystallineThornTrail;
        private static bool _triedPollenFieldTrail;
        private static bool _triedPetalVortexField;

        // Magic
        private static Effect _anthemBeamBody;
        private static Effect _hymnVerseTrail;
        private static Effect _elysianJudgmentTrail;
        private static bool _triedAnthemBeamBody;
        private static bool _triedHymnVerseTrail;
        private static bool _triedElysianJudgmentTrail;

        // Summon
        private static Effect _chorusVoiceAura;
        private static Effect _ovationSpectatorAura;
        private static Effect _harmonyFountainField;
        private static bool _triedChorusVoiceAura;
        private static bool _triedOvationSpectatorAura;
        private static bool _triedHarmonyFountainField;

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

        // ─── Per-Weapon Shader Properties ──────────────────────────────

        // Melee weapons
        /// <summary>ThornboundReckoning: vine-entwined trail with thorn overlay + LUT ramp.</summary>
        public static Effect VerdantReckoningTrail => LoadShader(ref _verdantReckoningTrail, ref _triedVerdantReckoningTrail,
            "MagnumOpus/Effects/OdeToJoy/ThornboundReckoning/VerdantReckoningTrail");

        /// <summary>TheGardenersFury: petal-scatter growth trail with seasonal color cycling.</summary>
        public static Effect GardenGrowthTrail => LoadShader(ref _gardenGrowthTrail, ref _triedGardenGrowthTrail,
            "MagnumOpus/Effects/OdeToJoy/TheGardenersFury/GardenGrowthTrail");

        /// <summary>RoseThornChainsaw: spinning ribbon with thorn silhouette, speed-responsive UV.</summary>
        public static Effect RoseChainTrail => LoadShader(ref _roseChainTrail, ref _triedRoseChainTrail,
            "MagnumOpus/Effects/OdeToJoy/RoseThornChainsaw/RoseChainTrail");

        // Ranged weapons
        /// <summary>ThornSprayRepeater: crystalline/Voronoi thorn trail, sharp edges, green-gold LUT.</summary>
        public static Effect CrystallineThornTrail => LoadShader(ref _crystallineThornTrail, ref _triedCrystallineThornTrail,
            "MagnumOpus/Effects/OdeToJoy/ThornSprayRepeater/CrystallineThornTrail");

        /// <summary>ThePollinator: soft pollen cloud trail with Perlin drift, warm golden glow.</summary>
        public static Effect PollenFieldTrail => LoadShader(ref _pollenFieldTrail, ref _triedPollenFieldTrail,
            "MagnumOpus/Effects/OdeToJoy/ThePollinator/PollenFieldTrail");

        /// <summary>PetalStormCannon: rotational vortex field with petal overlay, expandable radius.</summary>
        public static Effect PetalVortexField => LoadShader(ref _petalVortexField, ref _triedPetalVortexField,
            "MagnumOpus/Effects/OdeToJoy/PetalStormCannon/PetalVortexField");

        // Magic weapons
        /// <summary>AnthemOfGlory: dual-layer beam with staff lines + harmonic standing wave nodes.</summary>
        public static Effect AnthemBeamBody => LoadShader(ref _anthemBeamBody, ref _triedAnthemBeamBody,
            "MagnumOpus/Effects/OdeToJoy/AnthemOfGlory/AnthemBeamBody");

        /// <summary>HymnOfTheVictorious: per-verse configurable trail (4 technique variants).</summary>
        public static Effect HymnVerseTrail => LoadShader(ref _hymnVerseTrail, ref _triedHymnVerseTrail,
            "MagnumOpus/Effects/OdeToJoy/HymnOfTheVictorious/HymnVerseTrail");

        /// <summary>ElysianVerdict: radial judgment glyph GPU trail, pulsing intensity w/ mark count.</summary>
        public static Effect ElysianJudgmentTrail => LoadShader(ref _elysianJudgmentTrail, ref _triedElysianJudgmentTrail,
            "MagnumOpus/Effects/OdeToJoy/ElysianVerdict/ElysianJudgmentTrail");

        // Summon weapons
        /// <summary>TriumphantChorus: per-voice harmonic aura with musical staff overlay.</summary>
        public static Effect ChorusVoiceAura => LoadShader(ref _chorusVoiceAura, ref _triedChorusVoiceAura,
            "MagnumOpus/Effects/OdeToJoy/TriumphantChorus/ChorusVoiceAura");

        /// <summary>TheStandingOvation: ghostly spectator aura with applause ripple pattern.</summary>
        public static Effect OvationSpectatorAura => LoadShader(ref _ovationSpectatorAura, ref _triedOvationSpectatorAura,
            "MagnumOpus/Effects/OdeToJoy/TheStandingOvation/OvationSpectatorAura");

        /// <summary>FountainOfJoyousHarmony: concentric harmony rings, floral overlay, tier-responsive.</summary>
        public static Effect HarmonyFountainField => LoadShader(ref _harmonyFountainField, ref _triedHarmonyFountainField,
            "MagnumOpus/Effects/OdeToJoy/FountainOfJoyousHarmony/HarmonyFountainField");

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

        // ─── High-Level Apply Helpers ──────────────────────────────────

        /// <summary>
        /// Apply a trail shader with standard OdeToJoy parameters + optional noise/LUT texture binding.
        /// Call this right before drawing a VertexStrip or primitive trail.
        /// Returns the shader Effect (or null if unavailable).
        /// </summary>
        public static Effect ApplyTrailShader(Effect shader, float time, Color primary, Color secondary,
            float opacity, float intensity, Texture2D noiseTex = null, Texture2D lutTex = null)
        {
            if (shader == null) return null;

            SetTrailParams(shader, time, primary, secondary, opacity, intensity);

            var device = Main.graphics.GraphicsDevice;

            // Bind noise texture to sampler slot 1
            if (noiseTex != null)
            {
                device.Textures[1] = noiseTex;
                device.SamplerStates[1] = SamplerState.LinearWrap;
                shader.Parameters["uHasNoise"]?.SetValue(1f);
            }

            // Bind LUT gradient to sampler slot 2
            if (lutTex != null)
            {
                device.Textures[2] = lutTex;
                device.SamplerStates[2] = SamplerState.LinearClamp;
                shader.Parameters["uHasLUT"]?.SetValue(1f);
            }

            return shader;
        }

        /// <summary>
        /// Clean up secondary/tertiary texture sampler bindings after trail rendering.
        /// Call in finally block after drawing primitives.
        /// </summary>
        public static void CleanupSamplers()
        {
            var device = Main.graphics.GraphicsDevice;
            device.Textures[1] = null;
            device.Textures[2] = null;
        }

        /// <summary>
        /// Apply a bloom/aura shader with standard OdeToJoy parameters + optional noise binding.
        /// For use with additive SpriteBatch draw passes (GardenBloom, CelebrationAura, etc.).
        /// </summary>
        public static Effect ApplyBloomShader(Effect shader, string technique, float time,
            Color primary, Color secondary, float opacity, float intensity,
            float radius, float pulseSpeed = 3f, Texture2D noiseTex = null)
        {
            if (shader == null) return null;

            SetBloomParams(shader, time, primary, secondary, opacity, intensity, radius, pulseSpeed);

            if (noiseTex != null)
            {
                var device = Main.graphics.GraphicsDevice;
                device.Textures[1] = noiseTex;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }

            if (shader.Techniques[technique] != null)
                shader.CurrentTechnique = shader.Techniques[technique];

            return shader;
        }

        /// <summary>
        /// Apply a beam shader with OdeToJoy parameters + optional texture binding.
        /// </summary>
        public static Effect ApplyBeamShader(Effect shader, float time, Color primary, Color secondary,
            float opacity, float intensity, float harmonicFreq = 1f,
            Texture2D bodyTex = null, Texture2D noiseTex = null)
        {
            if (shader == null) return null;

            SetBeamParams(shader, time, primary, secondary, opacity, intensity, harmonicFreq);

            var device = Main.graphics.GraphicsDevice;
            if (bodyTex != null)
            {
                device.Textures[1] = bodyTex;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
            if (noiseTex != null)
            {
                device.Textures[2] = noiseTex;
                device.SamplerStates[2] = SamplerState.LinearWrap;
            }

            return shader;
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
