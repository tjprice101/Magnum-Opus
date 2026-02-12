using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.TestWeapons._05_ArcaneHarmonics
{
    /// <summary>
    /// 🎵 Harmonic Ring Projectile — Expanding harmonic shockwave spawned by Step 3 (Crescendo).
    /// Expands outward in a ring of arcane energy, hitting everything within radius,
    /// then lingers briefly before fading. Musical ring pulses with purple/lavender light.
    /// </summary>
    public class HarmonicRingProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.NebulaBlaze2;

        private const float MaxRadius = 130f;
        private const int ExpandDuration = 20;
        private const int LingerDuration = 40;
        private const int TotalDuration = ExpandDuration + LingerDuration;

        // Arcane palette
        private static readonly Color[] ArcanePalette = new Color[]
        {
            new Color(60, 10, 120),
            new Color(100, 30, 180),
            new Color(140, 70, 220),
            new Color(180, 130, 240),
            new Color(210, 180, 250),
            new Color(235, 220, 255)
        };

        private float Radius
        {
            get
            {
                int timer = (int)Projectile.ai[1];
                if (timer <= ExpandDuration)
                {
                    float t = (float)timer / ExpandDuration;
                    return MaxRadius * (1f - (1f - t) * (1f - t)); // Ease-out quadratic
                }
                return MaxRadius;
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = TotalDuration;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 16;
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float radius = Radius;
            Vector2 closest = new Vector2(
                MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right),
                MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
            float dist = Vector2.Distance(Projectile.Center, closest);
            return dist <= radius;
        }

        public override void AI()
        {
            Projectile.ai[1]++;
            int timer = (int)Projectile.ai[1];
            float radius = Radius;

            // ====== EXPANDING RING DUST ======
            if (timer <= ExpandDuration)
            {
                int dustCount = 8 + timer;
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustCount + Main.rand.NextFloat(-0.1f, 0.1f);
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;
                    Vector2 vel = angle.ToRotationVector2() * 0.5f;
                    Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch, vel, 80, default,
                        Main.rand.NextFloat(0.8f, 1.2f));
                    d.noGravity = true;
                }
            }

            // ====== INTERIOR ARCANE MIST ======
            if (timer % 3 == 0 && radius > 20f)
            {
                int mistCount = 2 + (timer < ExpandDuration ? 2 : 0);
                for (int i = 0; i < mistCount; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float dist = Main.rand.NextFloat(0.3f, 0.85f) * radius;
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * dist;
                    Color mistColor = GetPaletteColor(Main.rand.NextFloat());
                    var mist = new GenericGlowParticle(pos, Main.rand.NextVector2Circular(0.6f, 0.6f),
                        mistColor * 0.35f, Main.rand.NextFloat(0.1f, 0.22f), Main.rand.Next(6, 12), true);
                    MagnumParticleHandler.SpawnParticle(mist);
                }
            }

            // ====== GEM SPARKLE ACCENTS ======
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(0.5f, 1f) * radius;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * dist;
                Dust gem = Dust.NewDustPerfect(pos, DustID.GemAmethyst,
                    Main.rand.NextVector2Circular(1f, 1f), 0, default, 0.7f);
                gem.noGravity = true;
            }

            // ====== MUSIC NOTES AT RING EDGE ======
            if (Main.rand.NextBool(5) && radius > 30f)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 notePos = Projectile.Center + angle.ToRotationVector2() * (radius * 0.9f);
                var note = new HueShiftingMusicNoteParticle(notePos,
                    angle.ToRotationVector2() * 0.6f,
                    0.72f, 0.85f, 0.85f, 0.65f,
                    Main.rand.NextFloat(0.45f, 0.65f),
                    Main.rand.Next(14, 22), 0.025f);
                MagnumParticleHandler.SpawnParticle(note);
            }

            // ====== RING FLASH AT MAX EXPANSION ======
            if (timer == ExpandDuration)
            {
                for (int i = 0; i < 6; i++)
                {
                    var ring = new BloomRingParticle(Projectile.Center, Vector2.Zero,
                        GetPaletteColor((float)i / 6f) * 0.6f,
                        MaxRadius * 0.004f + i * 0.0008f, Main.rand.Next(10, 18));
                    MagnumParticleHandler.SpawnParticle(ring);
                }

                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.4f, Volume = 0.5f }, Projectile.Center);
            }

            Lighting.AddLight(Projectile.Center, 0.25f, 0.1f, 0.45f);
        }

        public override void OnKill(int timeLeft)
        {
            // Final fade-out burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                Color c = GetPaletteColor(Main.rand.NextFloat());
                var glow = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    sparkVel, c * 0.4f, Main.rand.NextFloat(0.12f, 0.2f), Main.rand.Next(8, 14), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            int timer = (int)Projectile.ai[1];
            float fadeOut = timer > TotalDuration - 15 ? 1f - (float)(timer - (TotalDuration - 15)) / 15f : 1f;
            float radius = Radius;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.08f;

            SwingShaderSystem.BeginAdditive(sb);

            // Outer ring glow — draw ring of glows at the ring edge
            int ringPoints = 24;
            for (int i = 0; i < ringPoints; i++)
            {
                float angle = MathHelper.TwoPi * i / ringPoints;
                Vector2 ringPos = drawPos + angle.ToRotationVector2() * radius;
                Color outerColor = new Color(100, 40, 180, 0) * 0.18f * fadeOut;
                sb.Draw(tex, ringPos, null, outerColor, 0f, origin, 0.14f * pulse, SpriteEffects.None, 0f);
            }

            // Central glow layers
            float centerScale = radius / 80f;
            Color coreOuter = new Color(80, 20, 150, 0) * 0.15f * fadeOut;
            Color coreInner = new Color(160, 100, 230, 0) * 0.12f * fadeOut;

            sb.Draw(tex, drawPos, null, coreOuter, 0f, origin, centerScale * 0.5f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, coreInner, 0f, origin, centerScale * 0.25f * pulse, SpriteEffects.None, 0f);

            SwingShaderSystem.RestoreSpriteBatch(sb);
            return false;
        }

        private Color GetPaletteColor(float progress)
        {
            float scaled = progress * (ArcanePalette.Length - 1);
            int idx = (int)scaled;
            int next = Math.Min(idx + 1, ArcanePalette.Length - 1);
            return Color.Lerp(ArcanePalette[idx], ArcanePalette[next], scaled - idx);
        }
    }
}
