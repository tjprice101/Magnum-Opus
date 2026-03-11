using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Dusts
{
    /// <summary>
    /// Abstract base for all Ode to Joy custom ModDust types.
    /// Provides additive multi-layer bloom rendering, Ode to Joy palette integration,
    /// noGravity default, scale decay, and consistent lighting.
    /// Per-weapon dusts inherit this and override behavior specifics.
    /// </summary>
    public abstract class OdeToJoyDustBase : ModDust
    {
        private static Texture2D _glowTex;

        /// <summary>Override to provide the dust sprite texture path.</summary>
        public abstract override string Texture { get; }

        /// <summary>Base scale multiplier applied on spawn. Override per-dust.</summary>
        protected virtual float BaseScale => 0.25f;

        /// <summary>Frame dimensions of the sprite. Override if not 64x64.</summary>
        protected virtual Rectangle SpriteFrame => new(0, 0, 64, 64);

        /// <summary>Whether this dust emits light. Default true.</summary>
        protected virtual bool EmitsLight => true;

        /// <summary>Light intensity multiplier. Default 0.35f.</summary>
        protected virtual float LightIntensity => 0.35f;

        /// <summary>Number of bloom layers in PreDraw. Override for more/fewer layers.</summary>
        protected virtual int BloomLayers => 3;

        public override void Load()
        {
            _glowTex ??= ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow", AssetRequestMode.ImmediateLoad).Value;
        }

        public override void Unload()
        {
            _glowTex = null;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.alpha = 0;
            dust.fadeIn = 1f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = SpriteFrame;
            dust.scale *= BaseScale;
            OnSpawnExtra(dust);
        }

        /// <summary>Override to add extra spawn logic without replacing base OnSpawn.</summary>
        protected virtual void OnSpawnExtra(Dust dust) { }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * dust.fadeIn;
        }

        public override bool Update(Dust dust)
        {
            dust.alpha++;
            dust.position += dust.velocity;

            UpdateBehavior(dust);

            if (EmitsLight && dust.fadeIn > 0.01f)
            {
                Lighting.AddLight(dust.position,
                    dust.color.ToVector3() * LightIntensity * dust.scale * dust.fadeIn);
            }

            return false;
        }

        /// <summary>
        /// Core update logic. Override this per-dust type.
        /// Default: velocity decay, rotation, scale shrink, lifetime kill.
        /// </summary>
        protected virtual void UpdateBehavior(Dust dust)
        {
            dust.velocity *= 0.95f;
            dust.rotation += dust.velocity.X * 0.05f;

            if (dust.alpha > 12)
            {
                dust.scale *= 0.97f;
                dust.fadeIn *= 0.96f;
            }

            if (dust.scale < 0.03f || dust.alpha > 50)
                dust.active = false;
        }

        public override bool PreDraw(Dust dust)
        {
            var mainTex = Texture2D.Value;
            Vector2 drawPos = dust.position - Main.screenPosition;
            Vector2 origin = SpriteFrame.Size() / 2f;
            float fade = dust.fadeIn;

            // Layer 1: Soft glow halo (wide, dim)
            if (_glowTex != null && BloomLayers >= 1)
            {
                Main.EntitySpriteDraw(_glowTex, drawPos, null,
                    (dust.color with { A = 0 }) * 0.2f * fade,
                    0f, _glowTex.Size() / 2f, dust.scale * 2.2f,
                    SpriteEffects.None);
            }

            // Layer 2: Main sprite (full detail)
            if (BloomLayers >= 2)
            {
                Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                    (dust.color with { A = 0 }) * 0.85f * fade,
                    dust.rotation, origin, dust.scale,
                    SpriteEffects.None);
            }

            // Layer 3: Inner hot core (small, bright)
            if (BloomLayers >= 3)
            {
                Color coreColor = Color.Lerp(dust.color, OdeToJoyPalette.WhiteBloom, 0.4f);
                Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                    (coreColor with { A = 0 }) * 0.5f * fade,
                    dust.rotation, origin, dust.scale * 0.5f,
                    SpriteEffects.None);
            }

            // Optional extra draw layers for child classes
            DrawExtraLayers(dust, mainTex, drawPos, origin, fade);

            return false;
        }

        /// <summary>Override to add extra draw layers after the base 3-layer bloom.</summary>
        protected virtual void DrawExtraLayers(Dust dust, Texture2D mainTex, Vector2 drawPos, Vector2 origin, float fade) { }

        /// <summary>Helper: get a random Ode to Joy palette color along the general gradient.</summary>
        protected static Color RandomPaletteColor()
        {
            return OdeToJoyPalette.GetGradient(Main.rand.NextFloat());
        }

        /// <summary>Helper: get a garden gradient color.</summary>
        protected static Color GardenColor(float t)
        {
            return OdeToJoyPalette.GetGardenGradient(t);
        }

        /// <summary>Helper: get a blossom gradient color.</summary>
        protected static Color BlossomColor(float t)
        {
            return OdeToJoyPalette.GetBlossomGradient(t);
        }
    }

    /// <summary>
    /// Reusable behavior container for Ode to Joy dust instances.
    /// Attach via dust.customData = new OdeToJoyDustBehavior { ... }.
    /// </summary>
    public class OdeToJoyDustBehavior
    {
        public float VelocityDecay = 0.95f;
        public float RotationSpeed = 0.04f;
        public float ScaleDecay = 0.97f;
        public float FadeDecay = 0.96f;
        public int FadeStartTime = 12;
        public int Lifetime = 50;
        public float DriftAmplitude = 0f;
        public float DriftFrequency = 0f;
        public float PhaseOffset = 0f;
        public int LifeTimer = 0;

        public OdeToJoyDustBehavior() { }

        public OdeToJoyDustBehavior(int lifetime, float velDecay = 0.95f)
        {
            Lifetime = lifetime;
            VelocityDecay = velDecay;
        }
    }
}
