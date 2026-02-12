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
    /// 🎵 Symphony Burst Projectile — Massive AoE burst spawned by Step 4 (Grand Finale).
    /// A climactic explosion of arcane musical energy: expands rapidly outward,
    /// engulfing everything in a cascade of purple/lavender light, hue-shifting
    /// music notes, and shimmering glyph-like accents. The weapon's grandest moment.
    /// </summary>
    public class SymphonyBurstProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.NebulaBlaze2;

        private const float MaxRadius = 180f;
        private const int ExpandDuration = 24;
        private const int LingerDuration = 44;
        private const int TotalDuration = ExpandDuration + LingerDuration;

        // Rich arcane palette — deeper, more dramatic than HarmonicRing
        private static readonly Color[] SymphonyPalette = new Color[]
        {
            new Color(40, 5, 90),
            new Color(85, 20, 160),
            new Color(130, 55, 210),
            new Color(170, 110, 235),
            new Color(200, 170, 248),
            new Color(240, 230, 255)
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
            Projectile.localNPCHitCooldown = 20;
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

            // ====== EXPANDING RING DUST — DENSE ======
            if (timer <= ExpandDuration)
            {
                int dustCount = 12 + timer * 2;
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustCount + Main.rand.NextFloat(-0.08f, 0.08f);
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;
                    Vector2 vel = angle.ToRotationVector2() * 0.8f;
                    int dustType = Main.rand.NextBool(3) ? DustID.Enchanted_Pink : DustID.PurpleTorch;
                    Dust d = Dust.NewDustPerfect(pos, dustType, vel, 60, default,
                        Main.rand.NextFloat(1.0f, 1.5f));
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }

                // Glow spark accents at expansion edge
                if (timer % 2 == 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;
                        Color sparkColor = GetPaletteColor(Main.rand.NextFloat());
                        var spark = new GlowSparkParticle(pos, angle.ToRotationVector2() * 1.5f,
                            sparkColor, Main.rand.NextFloat(0.2f, 0.35f), Main.rand.Next(8, 14));
                        MagnumParticleHandler.SpawnParticle(spark);
                    }
                }
            }

            // ====== INTERIOR ENERGY SWIRL ======
            if (timer % 2 == 0 && radius > 30f)
            {
                int mistCount = 3 + (timer < ExpandDuration ? 3 : 1);
                for (int i = 0; i < mistCount; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float dist = Main.rand.NextFloat(0.15f, 0.8f) * radius;
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * dist;
                    Color mistColor = GetPaletteColor(Main.rand.NextFloat());
                    var mist = new GenericGlowParticle(pos, Main.rand.NextVector2Circular(0.8f, 0.8f),
                        mistColor * 0.4f, Main.rand.NextFloat(0.12f, 0.28f), Main.rand.Next(6, 14), true);
                    MagnumParticleHandler.SpawnParticle(mist);
                }
            }

            // ====== MUSIC NOTE SHOWER ======
            if (Main.rand.NextBool(3) && radius > 20f)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float noteDist = Main.rand.NextFloat(0.3f, 0.95f) * radius;
                Vector2 notePos = Projectile.Center + angle.ToRotationVector2() * noteDist;
                var note = new HueShiftingMusicNoteParticle(notePos,
                    Main.rand.NextVector2Circular(1f, 1f) + Vector2.UnitY * -0.3f,
                    0.70f, 0.88f, 0.9f, 0.7f,
                    Main.rand.NextFloat(0.5f, 0.75f),
                    Main.rand.Next(16, 28), 0.03f);
                MagnumParticleHandler.SpawnParticle(note);
            }

            // ====== GEM SPARKLES ======
            if (Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(0.2f, 0.9f) * radius;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * dist;
                Dust gem = Dust.NewDustPerfect(pos, DustID.GemAmethyst,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, default, 0.9f);
                gem.noGravity = true;
            }

            // ====== MAX EXPANSION FLASH ======
            if (timer == ExpandDuration)
            {
                // Bloom ring cascade
                for (int i = 0; i < 8; i++)
                {
                    var ring = new BloomRingParticle(Projectile.Center, Vector2.Zero,
                        GetPaletteColor((float)i / 8f) * 0.7f,
                        MaxRadius * 0.004f + i * 0.001f, Main.rand.Next(12, 22));
                    MagnumParticleHandler.SpawnParticle(ring);
                }

                // Radial glow spark burst
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f;
                    Vector2 burstPos = Projectile.Center + angle.ToRotationVector2() * MaxRadius * 0.7f;
                    var spark = new GlowSparkParticle(burstPos, angle.ToRotationVector2() * 2.5f,
                        GetPaletteColor((float)i / 10f), 0.3f, Main.rand.Next(10, 16));
                    MagnumParticleHandler.SpawnParticle(spark);
                }

                SoundEngine.PlaySound(SoundID.Item68 with { Pitch = 0.3f, Volume = 0.6f }, Projectile.Center);

                // Screen shake for this climactic moment
                if (Projectile.owner == Main.myPlayer)
                {
                    var shakePlayer = Main.LocalPlayer.GetModPlayer<global::MagnumOpus.Content.LaCampanella.Debuffs.ScreenShakePlayer>();
                    shakePlayer?.AddShake(4f, 10);
                }
            }

            Lighting.AddLight(Projectile.Center, 0.4f, 0.15f, 0.65f);
        }

        public override void OnKill(int timeLeft)
        {
            // Grand fade-out
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Color c = GetPaletteColor(Main.rand.NextFloat());
                var glow = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(40f, 40f),
                    vel, c * 0.35f, Main.rand.NextFloat(0.1f, 0.22f),
                    Main.rand.Next(10, 16), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Final note scatter
            for (int i = 0; i < 4; i++)
            {
                var note = new HueShiftingMusicNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(30f, 30f),
                    Main.rand.NextVector2Circular(2f, 2f),
                    0.68f, 0.90f, 0.9f, 0.7f,
                    Main.rand.NextFloat(0.5f, 0.7f), Main.rand.Next(14, 24), 0.03f);
                MagnumParticleHandler.SpawnParticle(note);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            int timer = (int)Projectile.ai[1];
            float fadeOut = timer > TotalDuration - 18 ? 1f - (float)(timer - (TotalDuration - 18)) / 18f : 1f;
            float radius = Radius;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.18f) * 0.1f;
            float time = Main.GameUpdateCount * 0.04f;

            SwingShaderSystem.BeginAdditive(sb);

            // Outer glow ring — rotating points along ring
            int ringPoints = 32;
            for (int i = 0; i < ringPoints; i++)
            {
                float angle = MathHelper.TwoPi * i / ringPoints + time;
                Vector2 ringPos = drawPos + angle.ToRotationVector2() * radius;
                Color ringColor = new Color(120, 50, 200, 0) * 0.2f * fadeOut;
                sb.Draw(tex, ringPos, null, ringColor, 0f, origin, 0.16f * pulse, SpriteEffects.None, 0f);
            }

            // Inner rotating glow — opposite rotation
            int innerPoints = 16;
            for (int i = 0; i < innerPoints; i++)
            {
                float angle = MathHelper.TwoPi * i / innerPoints - time * 0.7f;
                Vector2 innerPos = drawPos + angle.ToRotationVector2() * (radius * 0.55f);
                Color innerColor = new Color(180, 120, 240, 0) * 0.15f * fadeOut;
                sb.Draw(tex, innerPos, null, innerColor, 0f, origin, 0.1f * pulse, SpriteEffects.None, 0f);
            }

            // Central glow layers — 4 layers for dramatic bloom
            float centerScale = radius / 80f;
            Color[] glowColors = {
                new Color(60, 15, 120, 0),
                new Color(110, 45, 190, 0),
                new Color(170, 110, 240, 0),
                new Color(230, 210, 255, 0)
            };
            float[] glowScales = { 0.6f, 0.4f, 0.25f, 0.12f };
            float[] glowAlphas = { 0.12f, 0.18f, 0.22f, 0.3f };

            for (int i = 0; i < 4; i++)
            {
                sb.Draw(tex, drawPos, null, glowColors[i] * glowAlphas[i] * fadeOut,
                    time * (i % 2 == 0 ? 1f : -0.6f), origin,
                    centerScale * glowScales[i] * pulse, SpriteEffects.None, 0f);
            }

            SwingShaderSystem.RestoreSpriteBatch(sb);
            return false;
        }

        private Color GetPaletteColor(float progress)
        {
            float scaled = progress * (SymphonyPalette.Length - 1);
            int idx = (int)scaled;
            int next = Math.Min(idx + 1, SymphonyPalette.Length - 1);
            return Color.Lerp(SymphonyPalette[idx], SymphonyPalette[next], scaled - idx);
        }
    }
}
