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
    /// 🎵 Resonance Field Projectile — Persistent damage zone spawned alongside the
    /// Symphony Burst at Step 4 (Grand Finale). Lingers after the burst fades, dealing
    /// continuous arcane damage. Expands → lingers with pulsing arcane mist and music
    /// notes → collapses inward in an implosion.
    /// </summary>
    public class ResonanceFieldProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.NebulaBlaze2;

        private const float MaxRadius = 140f;
        private const int ExpandDuration = 22;
        private const int LingerDuration = 60;
        private const int TotalDuration = ExpandDuration + LingerDuration;
        private const int CollapseStart = TotalDuration - 14;

        // Resonance palette — muted arcane purples
        private static readonly Color ResonancePurple = new Color(90, 30, 160);
        private static readonly Color ResonanceLavender = new Color(160, 110, 220);
        private static readonly Color ResonanceMist = new Color(120, 70, 190);

        private float Radius
        {
            get
            {
                int timer = (int)Projectile.ai[1];
                if (timer <= ExpandDuration)
                {
                    float t = (float)timer / ExpandDuration;
                    return MaxRadius * (1f - (1f - t) * (1f - t));
                }
                // Collapse in final frames
                if (timer >= CollapseStart)
                {
                    float collapseT = (float)(timer - CollapseStart) / (TotalDuration - CollapseStart);
                    return MaxRadius * (1f - collapseT * collapseT);
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
            Projectile.localNPCHitCooldown = 22;
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
            bool isCollapsing = timer >= CollapseStart;

            // ====== EXPANDING RING DUST ======
            if (timer <= ExpandDuration)
            {
                int dustCount = 6 + timer;
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustCount + Main.rand.NextFloat(-0.1f, 0.1f);
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;
                    Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch,
                        angle.ToRotationVector2() * 0.4f, 80, default,
                        Main.rand.NextFloat(0.7f, 1.0f));
                    d.noGravity = true;
                }
            }

            // ====== INTERIOR RESONANCE MIST ======
            if (timer % 4 == 0 && radius > 15f)
            {
                int count = isCollapsing ? 4 : 2;
                for (int i = 0; i < count; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float dist = Main.rand.NextFloat(0.2f, 0.8f) * radius;
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * dist;

                    Vector2 vel;
                    if (isCollapsing)
                    {
                        // During collapse, particles pull inward
                        vel = (Projectile.Center - pos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2.5f);
                    }
                    else
                    {
                        vel = Main.rand.NextVector2Circular(0.5f, 0.5f);
                    }

                    Color mistColor = Color.Lerp(ResonancePurple, ResonanceLavender, Main.rand.NextFloat());
                    var mist = new GenericGlowParticle(pos, vel, mistColor * 0.3f,
                        Main.rand.NextFloat(0.1f, 0.2f), Main.rand.Next(6, 12), true);
                    MagnumParticleHandler.SpawnParticle(mist);
                }
            }

            // ====== PULSING RING OUTLINE ======
            if (timer % 6 == 0 && !isCollapsing && timer > ExpandDuration && radius > 20f)
            {
                int ringCount = 10;
                for (int i = 0; i < ringCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / ringCount + Main.GameUpdateCount * 0.02f;
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;
                    Dust d = Dust.NewDustPerfect(pos, DustID.GemAmethyst,
                        Vector2.Zero, 0, default, 0.6f);
                    d.noGravity = true;
                }
            }

            // ====== COLLAPSE IMPLOSION PARTICLES ======
            if (isCollapsing)
            {
                int implosionCount = 3 + (timer - CollapseStart);
                for (int i = 0; i < implosionCount; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float dist = Main.rand.NextFloat(0.7f, 1.1f) * radius;
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * dist;
                    Vector2 vel = (Projectile.Center - pos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 5f);

                    Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch, vel, 40, default,
                        Main.rand.NextFloat(0.8f, 1.2f));
                    d.noGravity = true;
                }
            }

            // ====== MUSIC NOTES FLOATING ======
            if (Main.rand.NextBool(7) && radius > 25f && !isCollapsing)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(0.2f, 0.7f) * radius;
                Vector2 notePos = Projectile.Center + angle.ToRotationVector2() * dist;
                var note = new HueShiftingMusicNoteParticle(notePos,
                    Vector2.UnitY * -0.4f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    0.72f, 0.85f, 0.8f, 0.6f,
                    Main.rand.NextFloat(0.4f, 0.6f),
                    Main.rand.Next(14, 22), 0.02f);
                MagnumParticleHandler.SpawnParticle(note);
            }

            // ====== MAX EXPANSION RING FLASH ======
            if (timer == ExpandDuration)
            {
                for (int i = 0; i < 4; i++)
                {
                    var ring = new BloomRingParticle(Projectile.Center, Vector2.Zero,
                        Color.Lerp(ResonancePurple, ResonanceLavender, (float)i / 4f) * 0.5f,
                        MaxRadius * 0.003f + i * 0.0006f, Main.rand.Next(8, 14));
                    MagnumParticleHandler.SpawnParticle(ring);
                }

                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.2f, Volume = 0.4f }, Projectile.Center);
            }

            float lightMult = isCollapsing ? (1f - (float)(timer - CollapseStart) / (TotalDuration - CollapseStart)) : 0.85f;
            Lighting.AddLight(Projectile.Center, 0.2f * lightMult, 0.08f * lightMult, 0.38f * lightMult);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item25 with { Pitch = 0.7f, Volume = 0.35f }, Projectile.Center);

            // Implosion burst — particles converge then scatter
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * 15f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color c = Color.Lerp(ResonancePurple, ResonanceLavender, (float)i / 12f);
                var glow = new GenericGlowParticle(pos, vel, c * 0.45f,
                    Main.rand.NextFloat(0.12f, 0.22f), Main.rand.Next(8, 14), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(4f, 4f), 0, default, 1.0f);
                d.noGravity = true;
            }

            // Final note burst
            for (int i = 0; i < 3; i++)
            {
                var note = new HueShiftingMusicNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2Circular(1.5f, 1.5f),
                    0.72f, 0.86f, 0.85f, 0.65f,
                    Main.rand.NextFloat(0.45f, 0.65f), Main.rand.Next(12, 20), 0.025f);
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
            float fadeOut = timer > TotalDuration - 16 ? 1f - (float)(timer - (TotalDuration - 16)) / 16f : 1f;
            float radius = Radius;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.08f;
            float time = Main.GameUpdateCount * 0.03f;

            SwingShaderSystem.BeginAdditive(sb);

            // Outer ring glow
            int ringPoints = 20;
            for (int i = 0; i < ringPoints; i++)
            {
                float angle = MathHelper.TwoPi * i / ringPoints + time;
                Vector2 ringPos = drawPos + angle.ToRotationVector2() * radius;
                Color ringColor = new Color(90, 30, 160, 0) * 0.14f * fadeOut;
                sb.Draw(tex, ringPos, null, ringColor, 0f, origin, 0.1f * pulse, SpriteEffects.None, 0f);
            }

            // Core glow — 3 layers with gentle rotation
            float centerScale = radius / 90f;
            Color outerCore = new Color(70, 20, 130, 0) * 0.12f * fadeOut;
            Color innerCore = new Color(140, 80, 210, 0) * 0.14f * fadeOut;
            Color whiteCore = new Color(210, 190, 245, 0) * 0.1f * fadeOut;

            sb.Draw(tex, drawPos, null, outerCore, time * 0.5f, origin,
                centerScale * 0.45f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, innerCore, -time * 0.35f, origin,
                centerScale * 0.25f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, whiteCore, time * 0.2f, origin,
                centerScale * 0.12f * pulse, SpriteEffects.None, 0f);

            SwingShaderSystem.RestoreSpriteBatch(sb);
            return false;
        }
    }
}
