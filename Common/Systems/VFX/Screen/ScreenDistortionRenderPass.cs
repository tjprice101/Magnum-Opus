using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX.Optimization;

namespace MagnumOpus.Common.Systems.VFX
{
    // ────────────────────────────────────────────────────────────────────
    //  DistortionScreenShaderData
    //  Custom ScreenShaderData that selects a named technique from the
    //  ScreenDistortion effect and owns all parameter state.  Apply() is
    //  fully overridden so the base-class parameter logic cannot stomp
    //  our values.
    // ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// <see cref="ScreenShaderData"/> subclass that targets a specific technique
    /// (<c>RippleTechnique</c>, <c>HeatHazeTechnique</c>, or <c>ChromaticTechnique</c>)
    /// inside <c>Effects/ScreenDistortion.fxc</c> and exposes every custom parameter
    /// as a public property that the render pass writes to each frame.
    /// </summary>
    public class DistortionScreenShaderData : ScreenShaderData
    {
        private readonly string _techniqueName;

        /// <summary>Screen-UV center of the distortion (0–1 range).</summary>
        public Vector2 TargetPositionUV { get; set; }

        /// <summary>Overall distortion strength.  0 = invisible, ≥ 1 = full power.</summary>
        public float DistortionIntensity { get; set; }

        /// <summary>Normalised animation lifetime (0 = just spawned, 1 = expired).</summary>
        public float Progress { get; set; }

        /// <summary>Continuous time feed for animated noise / shimmer.</summary>
        public float Time { get; set; }

        /// <summary>Primary tint colour as linear RGB (0–1 per channel).</summary>
        public Vector3 PrimaryColor { get; set; } = Vector3.One;

        /// <summary>Secondary tint colour as linear RGB (0–1 per channel).</summary>
        public Vector3 SecondaryColor { get; set; } = Vector3.One;

        /// <summary>Master opacity multiplier (0–1).</summary>
        public float MasterOpacity { get; set; } = 1f;

        public DistortionScreenShaderData(Ref<Effect> shader, string techniqueName)
            : base(shader, "P0")
        {
            _techniqueName = techniqueName;
        }

        /// <summary>
        /// Sets every shader parameter and activates the pass.
        /// We bypass <see cref="ScreenShaderData.Apply"/> entirely so that
        /// the base class cannot overwrite our parameter values.
        /// </summary>
        public override void Apply()
        {
            Effect effect = Shader;
            if (effect == null)
                return;

            // Select technique (Ripple / HeatHaze / Chromatic)
            effect.CurrentTechnique = effect.Techniques[_techniqueName];

            // Push every parameter
            effect.Parameters["uTargetPosition"]?.SetValue(TargetPositionUV);
            effect.Parameters["uIntensity"]?.SetValue(DistortionIntensity);
            effect.Parameters["uProgress"]?.SetValue(Progress);
            effect.Parameters["uTime"]?.SetValue(Time);
            effect.Parameters["uColor"]?.SetValue(PrimaryColor);
            effect.Parameters["uSecondaryColor"]?.SetValue(SecondaryColor);
            effect.Parameters["uOpacity"]?.SetValue(MasterOpacity);

            // Activate pass 0 ("P0") of the selected technique
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }

    // ────────────────────────────────────────────────────────────────────
    //  ScreenDistortionRenderPass
    //  ModSystem that bridges ScreenDistortionManager → Filters.Scene.
    //  Uses tModLoader's built-in screen-shader pipeline so the effect
    //  renders correctly even when other mods (FancyLighting, etc.)
    //  hook into the draw loop.
    // ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Reads the dominant active distortion from <see cref="ScreenDistortionManager"/>
    /// each frame and keeps exactly one <c>Filters.Scene</c> filter active with the
    /// matching technique and parameters.  Compatible with all other mods.
    /// </summary>
    public class ScreenDistortionRenderPass : ModSystem
    {
        // One Filters.Scene key per shader technique
        private const string RippleKey    = "MagnumOpus:ScreenRipple";
        private const string HeatHazeKey  = "MagnumOpus:ScreenHeatHaze";
        private const string ChromaticKey = "MagnumOpus:ScreenChromatic";

        private static bool   _registered;
        private static string _activeFilterKey;

        // ── Lifecycle ──────────────────────────────────────────────────

        public override void Load()
        {
            if (Main.dedServ)
                return;

            try
            {
                // Check if the compiled shader asset exists BEFORE requesting it.
                // ModContent.Request registers a tracked asset; if it's missing,
                // Mod.TransferAllAssets() throws MissingResourceException and
                // fatally disables the entire mod.
                if (!ModContent.HasAsset("MagnumOpus/Effects/ScreenDistortion"))
                {
                    Mod.Logger.Info("[ScreenDistortionRenderPass] Effects/ScreenDistortion not found — skipping.");
                    return;
                }

                var asset = ModContent.Request<Effect>(
                    "MagnumOpus/Effects/ScreenDistortion",
                    AssetRequestMode.ImmediateLoad);

                if (asset?.Value == null)
                {
                    Mod.Logger.Warn("[ScreenDistortionRenderPass] Could not load Effects/ScreenDistortion.");
                    return;
                }

                Effect effect = asset.Value;
                var effectRef  = new Ref<Effect>(effect);

                // Register one filter per technique.
                // EffectPriority.Medium sits below sky / weather shaders.
                Filters.Scene[RippleKey] = new Filter(
                    new DistortionScreenShaderData(effectRef, "RippleTechnique"),
                    EffectPriority.Medium);

                Filters.Scene[HeatHazeKey] = new Filter(
                    new DistortionScreenShaderData(effectRef, "HeatHazeTechnique"),
                    EffectPriority.Medium);

                Filters.Scene[ChromaticKey] = new Filter(
                    new DistortionScreenShaderData(effectRef, "ChromaticTechnique"),
                    EffectPriority.Medium);

                _registered = true;
                Mod.Logger.Info("[ScreenDistortionRenderPass] 3 screen distortion filters registered.");
            }
            catch (Exception ex)
            {
                Mod.Logger.Warn($"[ScreenDistortionRenderPass] Registration failed: {ex.Message}");
            }
        }

        public override void Unload()
        {
            DeactivateAll();
            _registered      = false;
            _activeFilterKey  = null;
        }

        // ── Per-Frame Update ───────────────────────────────────────────

        public override void PostUpdateEverything()
        {
            if (Main.dedServ || !_registered)
                return;

            // Quality gate — require at least Medium for any shader work.
            // Defaults to "allowed" if the quality manager hasn't loaded yet.
            if (!(AdaptiveQualityManager.Instance?.EnableShaders ?? true))
            {
                DeactivateAll();
                return;
            }

            // Ask the distortion manager for the strongest effect this frame.
            ScreenDistortionManager.DistortionRenderData? renderData =
                ScreenDistortionManager.GetDominantRenderData();

            if (renderData == null)
            {
                DeactivateAll();
                return;
            }

            var data = renderData.Value;

            // Map the abstract style to one of our three filter keys.
            string targetKey = MapStyleToFilterKey(data.Style);
            if (targetKey == null)
            {
                DeactivateAll();
                return;
            }

            // If the technique changed, deactivate the old one first.
            if (_activeFilterKey != null && _activeFilterKey != targetKey)
                TryDeactivate(_activeFilterKey);

            // Activate the target filter (safe to call when already active).
            try
            {
                if (Filters.Scene[targetKey]?.IsActive() != true)
                    Filters.Scene.Activate(targetKey, data.WorldPosition);
            }
            catch
            {
                return;
            }

            _activeFilterKey = targetKey;

            // Push parameter snapshot into the shader data.
            ConfigureShader(targetKey, in data);
        }

        // ── Helpers ────────────────────────────────────────────────────

        /// <summary>
        /// Maps a <see cref="ShaderStyleRegistry.ScreenStyle"/> value to the
        /// Filters.Scene key whose technique best represents it.
        /// </summary>
        private static string MapStyleToFilterKey(ShaderStyleRegistry.ScreenStyle style)
        {
            return style switch
            {
                ShaderStyleRegistry.ScreenStyle.Ripple  => RippleKey,
                ShaderStyleRegistry.ScreenStyle.Pulse   => RippleKey,    // pulse ≈ ripple with pulse easing
                ShaderStyleRegistry.ScreenStyle.Warp    => HeatHazeKey,  // warp ≈ wavy heat distortion
                ShaderStyleRegistry.ScreenStyle.Shatter => ChromaticKey, // shatter = RGB channel split
                ShaderStyleRegistry.ScreenStyle.Tear    => ChromaticKey, // tear ≈ directional split
                _                                       => null
            };
        }

        /// <summary>
        /// Writes the current frame's distortion data into the
        /// <see cref="DistortionScreenShaderData"/> stored inside the filter.
        /// </summary>
        private static void ConfigureShader(
            string key,
            in ScreenDistortionManager.DistortionRenderData data)
        {
            try
            {
                if (Filters.Scene[key]?.GetShader() is not DistortionScreenShaderData shaderData)
                    return;

                // Convert world position → screen UV (0–1)
                Vector2 screenPx = data.WorldPosition - Main.screenPosition;
                Vector2 uv = new Vector2(
                    MathHelper.Clamp(screenPx.X / Main.screenWidth,  0f, 1f),
                    MathHelper.Clamp(screenPx.Y / Main.screenHeight, 0f, 1f));

                shaderData.TargetPositionUV    = uv;
                shaderData.DistortionIntensity = data.Intensity;
                shaderData.Progress            = data.Progress;
                shaderData.Time                = Main.GlobalTimeWrappedHourly;
                shaderData.PrimaryColor        = data.PrimaryColor.ToVector3();
                shaderData.SecondaryColor      = data.SecondaryColor.ToVector3();
                shaderData.MasterOpacity       = MathHelper.Clamp(data.Intensity, 0f, 1f);
            }
            catch
            {
                // Shader unavailable — skip silently this frame.
            }
        }

        private static void DeactivateAll()
        {
            if (!_registered)
                return;

            TryDeactivate(RippleKey);
            TryDeactivate(HeatHazeKey);
            TryDeactivate(ChromaticKey);
            _activeFilterKey = null;
        }

        private static void TryDeactivate(string key)
        {
            try
            {
                if (Filters.Scene[key]?.IsActive() == true)
                    Filters.Scene[key].Deactivate();
            }
            catch
            {
                // Filter not available — nothing to do.
            }
        }
    }
}
