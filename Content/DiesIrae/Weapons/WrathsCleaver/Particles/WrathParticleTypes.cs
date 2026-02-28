using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Particles
{
    /// <summary>
    /// Concrete particle types for Wrath's Cleaver.
    /// Each particle type has unique behavior and visual identity.
    /// </summary>

    // ── INFERNAL EMBER ──
    // Stretched spark that follows velocity direction, fading from hot white to blood red
    public class InfernalEmber : WrathParticle
    {
        private readonly float initialScale;

        public InfernalEmber(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
        {
            Position = pos;
            Velocity = vel;
            DrawColor = color;
            Scale = scale;
            initialScale = scale;
            MaxLifetime = lifetime;
            IsAdditive = true;
            Rotation = vel != Vector2.Zero ? vel.ToRotation() : 0f;
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.96f;
            Velocity.Y -= 0.04f; // Embers rise slightly
            Scale = initialScale * (1f - LifeRatio);
            if (Velocity != Vector2.Zero)
                Rotation = Velocity.ToRotation();
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = TextureAssets.MagicPixel.Value;
            Vector2 drawPos = Position - Main.screenPosition;
            float stretch = Math.Max(Velocity.Length() * 0.7f, 2f);
            Vector2 origin = new Vector2(0.5f, 0.5f);
            Color fadeColor = Color.Lerp(DrawColor, WrathsCleaverUtils.BloodRed, LifeRatio) * (1f - LifeRatio);

            sb.Draw(tex, drawPos, new Rectangle(0, 0, 1, 1), fadeColor,
                Rotation, origin, new Vector2(stretch * Scale, Scale * 0.4f),
                SpriteEffects.None, 0f);
        }
    }

    // ── WRATH SMOKE ──
    // Heavy smoke that billows upward with charcoal-to-blood-red gradient
    public class WrathSmoke : WrathParticle
    {
        private readonly float initialScale;
        private readonly float rotationSpeed;
        private readonly float fadeRate;

        public WrathSmoke(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime,
            float rotSpeed = 0.02f, float fade = 0.7f)
        {
            Position = pos;
            Velocity = vel;
            DrawColor = color;
            Scale = scale;
            initialScale = scale;
            MaxLifetime = lifetime;
            IsAdditive = false;
            rotationSpeed = rotSpeed;
            fadeRate = fade;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.97f;
            Velocity.Y -= 0.02f; // Smoke rises
            Scale = initialScale * (1f + LifeRatio * 0.5f); // Expands
            Rotation += rotationSpeed;
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = TextureAssets.MagicPixel.Value;
            Vector2 drawPos = Position - Main.screenPosition;
            float opacity = (1f - LifeRatio) * fadeRate;
            Color smokeColor = Color.Lerp(DrawColor, WrathsCleaverUtils.CharcoalBlack, LifeRatio * 0.5f) * opacity;

            // Draw a soft circle-like shape using scaled pixel (will appear as a dim square — replaced by proper texture at runtime)
            sb.Draw(tex, drawPos, new Rectangle(0, 0, 1, 1), smokeColor,
                Rotation, new Vector2(0.5f), Scale * 8f,
                SpriteEffects.None, 0f);
        }
    }

    // ── HELLFIRE NOTE ──
    // Musical note particle that orbits slightly before drifting upward
    public class HellfireNote : WrathParticle
    {
        private readonly float orbitRadius;
        private readonly float orbitSpeed;
        private readonly Vector2 orbitCenter;
        private float orbitAngle;

        public HellfireNote(Vector2 center, Vector2 vel, Color color, float scale, int lifetime)
        {
            orbitCenter = center;
            Position = center;
            Velocity = vel;
            DrawColor = color;
            Scale = scale;
            MaxLifetime = lifetime;
            IsAdditive = true;
            orbitRadius = Main.rand.NextFloat(8f, 25f);
            orbitSpeed = Main.rand.NextFloat(0.05f, 0.12f) * (Main.rand.NextBool() ? 1f : -1f);
            orbitAngle = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            orbitAngle += orbitSpeed;
            float fadedRadius = orbitRadius * (1f - LifeRatio * 0.5f);
            Vector2 orbit = new Vector2((float)Math.Cos(orbitAngle), (float)Math.Sin(orbitAngle)) * fadedRadius;
            Position = orbitCenter + orbit + Velocity * Lifetime;
            Velocity *= 0.98f;
            Velocity.Y -= 0.03f;
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = TextureAssets.MagicPixel.Value;
            Vector2 drawPos = Position - Main.screenPosition;
            float opacity = (1f - LifeRatio);
            float pulse = 1f + (float)Math.Sin(Lifetime * 0.15f) * 0.2f;
            Color noteColor = Color.Lerp(DrawColor, WrathsCleaverUtils.HellfireGold, (float)Math.Sin(Lifetime * 0.1f) * 0.5f + 0.5f) * opacity;

            // Glow core
            sb.Draw(tex, drawPos, new Rectangle(0, 0, 1, 1), noteColor,
                0f, new Vector2(0.5f), Scale * pulse * 6f,
                SpriteEffects.None, 0f);
            // Bloom halo
            sb.Draw(tex, drawPos, new Rectangle(0, 0, 1, 1), noteColor * 0.3f,
                0f, new Vector2(0.5f), Scale * pulse * 14f,
                SpriteEffects.None, 0f);
        }
    }

    // ── WRATH BLOOM ──
    // Large soft bloom overlay for impacts
    public class WrathBloom : WrathParticle
    {
        private readonly float initialScale;

        public WrathBloom(Vector2 pos, Color color, float scale, int lifetime)
        {
            Position = pos;
            Velocity = Vector2.Zero;
            DrawColor = color;
            Scale = scale;
            initialScale = scale;
            MaxLifetime = lifetime;
            IsAdditive = true;
        }

        public override void Update()
        {
            base.Update();
            Scale = initialScale * (1f + LifeRatio * 0.3f);
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = TextureAssets.MagicPixel.Value;
            Vector2 drawPos = Position - Main.screenPosition;
            float opacity = (1f - (float)Math.Pow(LifeRatio, 2)) * 0.6f;
            Color bloomColor = DrawColor * opacity;

            sb.Draw(tex, drawPos, new Rectangle(0, 0, 1, 1), bloomColor,
                0f, new Vector2(0.5f), Scale * 20f,
                SpriteEffects.None, 0f);
        }
    }

    // ── CRYSTALLIZED FLAME SPARK ──
    // Bright spark for the crystallized flame explosion (every 3rd swing)
    public class CrystallizedFlameSpark : WrathParticle
    {
        private readonly float initialScale;
        private readonly float gravity;

        public CrystallizedFlameSpark(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime, float grav = 0.08f)
        {
            Position = pos;
            Velocity = vel;
            DrawColor = color;
            Scale = scale;
            initialScale = scale;
            MaxLifetime = lifetime;
            IsAdditive = true;
            gravity = grav;
        }

        public override void Update()
        {
            base.Update();
            Velocity.Y += gravity;
            Velocity *= 0.97f;
            Scale = initialScale * (1f - LifeRatio * 0.7f);
            if (Velocity != Vector2.Zero)
                Rotation = Velocity.ToRotation();
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = TextureAssets.MagicPixel.Value;
            Vector2 drawPos = Position - Main.screenPosition;
            float stretch = Math.Max(Velocity.Length() * 0.5f, 1.5f);
            float opacity = 1f - LifeRatio;
            Color sparkColor = Color.Lerp(Color.White, DrawColor, LifeRatio * 0.8f) * opacity;

            sb.Draw(tex, drawPos, new Rectangle(0, 0, 1, 1), sparkColor,
                Rotation, new Vector2(0.5f), new Vector2(stretch * Scale, Scale * 0.3f),
                SpriteEffects.None, 0f);
        }
    }
}
