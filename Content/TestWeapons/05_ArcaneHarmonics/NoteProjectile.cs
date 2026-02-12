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
    /// 🎵 Note Projectile — Homing arcane music note spawned by Step 2 (Crescendo Arc).
    /// Three notes launch in a spread, gently home toward enemies, leave shimmering
    /// purple/lavender trails with hue-shifting music note particles, and burst into
    /// a harmonic shimmer on death.
    /// </summary>
    public class NoteProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.NebulaBlaze2;

        private const float HomingStrength = 0.04f;
        private const float MaxSpeed = 14f;
        private const float HomingRange = 500f;

        // Arcane palette — deep purple → lavender → white
        private static readonly Color DeepPurple = new Color(100, 30, 180);
        private static readonly Color Lavender = new Color(180, 130, 240);
        private static readonly Color ArcaneWhite = new Color(230, 210, 255);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 14;
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 250;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 160;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 80;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            // Gentle homing toward nearest NPC
            float bestDist = HomingRange;
            Vector2 bestTarget = Vector2.Zero;
            bool foundTarget = false;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestTarget = npc.Center;
                    foundTarget = true;
                }
            }

            if (foundTarget)
            {
                Vector2 desired = (bestTarget - Projectile.Center).SafeNormalize(Vector2.Zero) * MaxSpeed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, HomingStrength);
            }

            // Cap speed
            if (Projectile.velocity.Length() > MaxSpeed)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * MaxSpeed;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // ====== VFX TRAILS ======

            // Dense purple dust trail — 2 per frame
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.PurpleTorch,
                    -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(0.8f, 0.8f),
                    60, default, Main.rand.NextFloat(1.0f, 1.4f));
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // Contrasting gem sparkles — 1-in-2
            if (Main.rand.NextBool(2))
            {
                Dust gem = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(7f, 7f),
                    DustID.GemAmethyst, -Projectile.velocity * 0.08f, 0, default, 0.8f);
                gem.noGravity = true;
            }

            // Glow spark trail — 1-in-3
            if (Main.rand.NextBool(3))
            {
                Color sparkColor = Color.Lerp(DeepPurple, Lavender, Main.rand.NextFloat());
                var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.08f,
                    sparkColor * 0.55f, Main.rand.NextFloat(0.15f, 0.25f), Main.rand.Next(8, 14), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Hue-shifting music note particles — 1-in-6
            if (Main.rand.NextBool(6))
            {
                var note = new HueShiftingMusicNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -Projectile.velocity * 0.06f + Main.rand.NextVector2Circular(0.4f, 0.4f),
                    hueMin: 0.72f, hueMax: 0.85f, saturation: 0.85f, luminosity: 0.65f,
                    scale: Main.rand.NextFloat(0.55f, 0.75f),
                    lifetime: Main.rand.Next(18, 28), hueSpeed: 0.025f);
                MagnumParticleHandler.SpawnParticle(note);
            }

            Lighting.AddLight(Projectile.Center, 0.3f, 0.12f, 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Purple spark burst on hit
            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(4f, 4f), 0, default, 1.3f);
                d.noGravity = true;
            }

            // Glow spark accent
            var spark = new GlowSparkParticle(target.Center, Main.rand.NextVector2Circular(3f, 3f),
                Lavender, Main.rand.NextFloat(0.2f, 0.35f), Main.rand.Next(8, 14));
            MagnumParticleHandler.SpawnParticle(spark);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item25 with { Pitch = 0.5f, Volume = 0.5f }, Projectile.Center);

            // Harmonic shimmer burst — glow sparks
            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                Color sparkColor = Color.Lerp(DeepPurple, ArcaneWhite, Main.rand.NextFloat());
                var spark = new GlowSparkParticle(Projectile.Center, sparkVel, sparkColor,
                    Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(10, 18));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Arcane dust burst
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.2f);
                d.noGravity = true;
            }

            // Final music note scatter
            for (int i = 0; i < 2; i++)
            {
                var note = new HueShiftingMusicNoteParticle(
                    Projectile.Center, Main.rand.NextVector2Circular(3f, 3f),
                    0.70f, 0.88f, 0.9f, 0.7f,
                    Main.rand.NextFloat(0.5f, 0.7f), Main.rand.Next(16, 26), 0.03f);
                MagnumParticleHandler.SpawnParticle(note);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Afterimage trail — purple → lavender gradient
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.45f;
                float scale = 0.16f * (1f - progress * 0.5f);
                Color trailColor = Color.Lerp(new Color(180, 120, 240, 0), new Color(80, 20, 140, 0), progress);
                sb.Draw(tex, Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition, null,
                    trailColor * alpha, 0f, origin, scale, SpriteEffects.None, 0f);
            }

            // Core glow — pulsing arcane orb
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.22f) * 0.12f;
            Color outerGlow = new Color(100, 40, 180, 0) * 0.4f;
            Color midGlow = new Color(160, 100, 230, 0) * 0.55f;
            Color coreGlow = new Color(230, 200, 255, 0) * 0.7f;

            sb.Draw(tex, drawPos, null, outerGlow, 0f, origin, 0.24f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, midGlow, 0f, origin, 0.14f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, coreGlow, 0f, origin, 0.07f * pulse, SpriteEffects.None, 0f);

            return false;
        }
    }
}
