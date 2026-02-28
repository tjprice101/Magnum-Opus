using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Utilities;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Particles
{
    /// <summary>
    /// Heavy, slow-dissipating dark smoke — the mourning trail of each bullet.
    /// </summary>
    public class GriefSmoke : LamentParticle
    {
        public override bool UseAdditiveBlend => false;

        public override void Spawn(Vector2 position, Vector2 velocity, Color color, float scale, int time)
        {
            base.Spawn(position, velocity, color, scale, time);
            Velocity *= 0.3f; // smoke drifts slowly
            RotationSpeed = Main.rand.NextFloat(-0.03f, 0.03f);
        }

        public override void Update()
        {
            base.Update();
            if (!Active) return;
            Velocity *= 0.96f;
            Scale += 0.01f; // slowly expands
            Opacity = LifeRatio * LifeRatio; // squared falloff — lingers then fades
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke",
                ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            var origin = new Vector2(tex.Width, tex.Height) * 0.5f;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null,
                DrawColor * Opacity * 0.35f, Rotation, origin, Scale * 0.3f,
                SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Sudden, brief prismatic flash that tears through the darkness — 
    /// the "revelation" flicker of The Swan's Lament.
    /// </summary>
    public class PrismaticFlashParticle : LamentParticle
    {
        public override bool UseAdditiveBlend => true;

        public override void Spawn(Vector2 position, Vector2 velocity, Color color, float scale, int time)
        {
            base.Spawn(position, velocity, color, scale, time);
            MaxTime = Math.Max(time, 6); // very brief
            TimeLeft = MaxTime;
        }

        public override void Update()
        {
            base.Update();
            if (!Active) return;
            // Extremely fast falloff — flash in, flash out
            float t = 1f - LifeRatio;
            Opacity = (float)Math.Sin(t * Math.PI); // peaks at mid-life
            Scale *= 0.92f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Pixel/PartiGlow",
                ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            var origin = new Vector2(tex.Width, tex.Height) * 0.5f;

            // Rainbow-shifted color for the prismatic effect
            float hueShift = (float)(Main.GameUpdateCount % 60) / 60f + LifeRatio;
            Color prismatic = Main.hslToRgb(hueShift % 1f, 0.9f, 0.85f);

            spriteBatch.Draw(tex, Position - Main.screenPosition, null,
                prismatic * Opacity, Rotation, origin, Scale * 0.5f,
                SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Expanding ring of destruction spawned on enemy death.
    /// Conveys the weight of revelation.
    /// </summary>
    public class DestructionRingParticle : LamentParticle
    {
        public override bool UseAdditiveBlend => true;

        public override void Update()
        {
            base.Update();
            if (!Active) return;
            Scale += 0.15f; // expands quickly
            Opacity = LifeRatio; // fades as it grows
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/HardCircleMask",
                ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            var origin = new Vector2(tex.Width, tex.Height) * 0.5f;

            // Outer ring: mostly grey/white, flashes gold at peak
            float flashIntensity = LamentUtils.GetGriefFlashIntensity(LifeRatio);
            Color ringColor = Color.Lerp(LamentUtils.GriefGrey, LamentUtils.RevelationGold, flashIntensity);

            // Draw just the ring (outer circle minus inner)
            spriteBatch.Draw(tex, Position - Main.screenPosition, null,
                ringColor * Opacity * 0.6f, 0f, origin, Scale,
                SpriteEffects.None, 0f);
            // Hollow out center
            spriteBatch.Draw(tex, Position - Main.screenPosition, null,
                Color.Black * Opacity * 0.4f, 0f, origin, Scale * 0.85f,
                SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Tiny bright ember — flies out from bullets and impacts.
    /// Starts dark, occasionally flickers white-gold (grief → catharsis).
    /// </summary>
    public class LamentEmberParticle : LamentParticle
    {
        public override bool UseAdditiveBlend => true;

        public override void Spawn(Vector2 position, Vector2 velocity, Color color, float scale, int time)
        {
            base.Spawn(position, velocity, color, scale, time);
            RotationSpeed = Main.rand.NextFloat(-0.15f, 0.15f);
        }

        public override void Update()
        {
            base.Update();
            if (!Active) return;
            Velocity *= 0.97f;
            Velocity.Y += 0.02f; // slight gravity
            Opacity = MathHelper.Clamp(LifeRatio * 1.5f, 0f, 1f);

            // Flicker between dark and gold
            float flicker = LamentUtils.GetGriefFlashIntensity(LifeRatio + Main.rand.NextFloat(-0.05f, 0.05f));
            DrawColor = Color.Lerp(LamentUtils.MourningBlack, LamentUtils.CatharsisWhite, flicker);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft",
                ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            var origin = new Vector2(tex.Width, tex.Height) * 0.5f;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null,
                DrawColor * Opacity, Rotation, origin, Scale * 0.35f,
                SpriteEffects.None, 0f);
        }
    }
}
