using System;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;

namespace MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Utilities
{
    /// <summary>
    /// VFX library for the Nocturnal Executioner.
    /// Identity: The Void Between Stars — gravitational consumption, void implosion → starfire burst.
    /// All effects emphasize INWARD pull and dark authority with gold detonation payoff.
    /// </summary>
    public static class NocturnalExecutionerVFX
    {
        // Void palette shortcuts
        private static readonly Color VoidBlack = NachtmusikPalette.CosmicVoid;
        private static readonly Color CosmicPurple = NachtmusikPalette.CosmicPurple;
        private static readonly Color Violet = NachtmusikPalette.Violet;
        private static readonly Color StarGold = NachtmusikPalette.StarGold;
        private static readonly Color StellarWhite = NachtmusikPalette.TwinklingWhite;

        /// <summary>
        /// Two-beat impact: Beat 1 (implosion) — particles rush INWARD to hit point.
        /// Beat 2 (detonation) — gold starfire erupts outward. Scales with combo step.
        /// </summary>
        public static void VoidImplosionStarfireBurst(Vector2 hitPos, int comboStep, Vector2 swingDirection)
        {
            if (Main.dedServ) return;

            int implosionCount = 6 + comboStep * 3;
            int burstCount = 5 + comboStep * 3;

            // Beat 1: Void implosion — dust spawns OUTWARD then rushes inward
            for (int i = 0; i < implosionCount; i++)
            {
                float angle = MathHelper.TwoPi * i / implosionCount;
                float dist = 30f + Main.rand.NextFloat(15f);
                Vector2 spawnPos = hitPos + angle.ToRotationVector2() * dist;

                // Velocity toward impact point (implosion)
                Vector2 vel = (hitPos - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(4f, 8f);

                Color c = Color.Lerp(VoidBlack, CosmicPurple, Main.rand.NextFloat(0.3f, 0.8f));
                Dust d = Dust.NewDustPerfect(spawnPos, DustID.PurpleTorch, vel, 0, c,
                    1.0f + comboStep * 0.2f);
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }

            // Central void flash (dark purple, compressed)
            Dust voidFlash = Dust.NewDustPerfect(hitPos, DustID.PurpleTorch, Vector2.Zero, 0,
                CosmicPurple, 2.0f + comboStep * 0.3f);
            voidFlash.noGravity = true;
            voidFlash.fadeIn = 0.3f;

            // Beat 2: Gold starfire detonation — outward burst
            for (int i = 0; i < burstCount; i++)
            {
                Vector2 vel = swingDirection.RotatedByRandom(1.2f) * Main.rand.NextFloat(3f, 7f);
                Dust g = Dust.NewDustPerfect(hitPos, DustID.GoldFlame, vel, 0,
                    Color.Lerp(StarGold, StellarWhite, Main.rand.NextFloat(0.3f, 1f)),
                    0.8f + comboStep * 0.15f);
                g.noGravity = true;
            }

            // Bloom ring particle on hit
            var ring = new BloomRingParticle(hitPos, Vector2.Zero,
                Color.Lerp(CosmicPurple, StarGold, 0.3f), 0.5f + comboStep * 0.1f, 18 + comboStep * 3);
            MagnumParticleHandler.SpawnParticle(ring);

            // Phase 2+ gets extra implosion ring
            if (comboStep >= 1)
            {
                var innerRing = new BloomRingParticle(hitPos, Vector2.Zero,
                    StarGold, 0.3f, 12);
                MagnumParticleHandler.SpawnParticle(innerRing);
            }

            // Phase 2 gets shattered starlight
            if (comboStep >= 2)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector2 shardVel = Main.rand.NextVector2Circular(5f, 5f);
                    var shard = new ShatteredStarlightParticle(hitPos, shardVel,
                        Color.Lerp(Violet, StarGold, Main.rand.NextFloat()),
                        Main.rand.NextFloat(0.5f, 0.9f), Main.rand.Next(20, 35));
                    MagnumParticleHandler.SpawnParticle(shard);
                }
            }

            Lighting.AddLight(hitPos, StarGold.ToVector3() * (0.5f + comboStep * 0.1f));
        }

        /// <summary>
        /// Spawn a void-consumed music note that drifts TOWARD the blade.
        /// Dark indigo body with gold edge accent. Consumed by the void.
        /// </summary>
        public static void SpawnVoidConsumedNote(Vector2 bladeTipPos, Vector2 swordDirection)
        {
            if (Main.dedServ) return;

            // Spawn note offset from blade tip
            float perpDist = Main.rand.NextFloat(25f, 50f) * (Main.rand.NextBool() ? 1f : -1f);
            Vector2 perp = swordDirection.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.UnitY);
            Vector2 spawnPos = bladeTipPos + perp * perpDist + Main.rand.NextVector2Circular(10f, 10f);

            // Velocity toward blade (consumed)
            Vector2 towardBlade = (bladeTipPos - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.8f, 2f);

            Color noteColor = Color.Lerp(NachtmusikPalette.DeepBlue, NachtmusikPalette.Violet, Main.rand.NextFloat());
            Color edgeColor = StarGold;

            var note = new HueShiftingMusicNoteParticle(
                spawnPos, towardBlade, noteColor, edgeColor,
                Main.rand.NextFloat(0.25f, 0.5f), Main.rand.Next(25, 45));
            MagnumParticleHandler.SpawnParticle(note);
        }

        /// <summary>
        /// Hold item VFX — void accumulation visual that scales with charge progress (0-1).
        /// Blade darkens, void particles pool, gold fire at high charge.
        /// </summary>
        public static void HoldItemVFX(Player player, float chargeProgress)
        {
            if (Main.dedServ) return;
            if (chargeProgress <= 0.01f) return;

            // System 1: Gravity-well dust spiraling inward to player (always active above 0)
            if (Main.rand.NextBool(Math.Max(1, 6 - (int)(chargeProgress * 5))))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float startRadius = 35f + Main.rand.NextFloat(20f);
                Vector2 spawnPos = player.Center + angle.ToRotationVector2() * startRadius;
                Vector2 toPlayer = (player.Center - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2.5f);
                // Add slight tangential component for spiral
                Vector2 tangent = toPlayer.RotatedBy(MathHelper.PiOver4) * 0.4f;

                Color spiralColor = Color.Lerp(VoidBlack, CosmicPurple, Main.rand.NextFloat(0.3f, 0.6f));
                Dust d = Dust.NewDustPerfect(spawnPos, DustID.PurpleTorch, toPlayer + tangent, 0,
                    spiralColor, 0.5f + chargeProgress * 0.3f);
                d.noGravity = true;
                d.fadeIn = 0.9f;
            }

            // System 2: Stellar shimmer twinkle at 50%+ charge
            if (chargeProgress >= 0.5f && Main.rand.NextBool(4))
            {
                Vector2 twinklePos = player.Center + Main.rand.NextVector2Circular(20f, 28f);
                Dust tw = Dust.NewDustPerfect(twinklePos, DustID.ShimmerSpark, Vector2.Zero, 0,
                    Color.Lerp(Violet, StellarWhite, Main.rand.NextFloat()), 0.4f);
                tw.noGravity = true;
                tw.fadeIn = 0.6f;
            }

            // System 3: Void crack flash lines at 70%+ charge
            if (chargeProgress >= 0.7f && Main.rand.NextBool(8))
            {
                Vector2 crackDir = Main.rand.NextVector2Unit() * Main.rand.NextFloat(15f, 30f);
                var crack = new LineParticle(
                    player.Center + crackDir * 0.3f,
                    crackDir * 0.1f,
                    Color.Lerp(CosmicPurple, StarGold, chargeProgress - 0.7f),
                    Main.rand.NextFloat(0.3f, 0.6f),
                    Main.rand.Next(6, 12));
                MagnumParticleHandler.SpawnParticle(crack);
            }

            // System 4: Gold fire corona at 90%+ charge
            if (chargeProgress >= 0.9f && Main.rand.NextBool(3))
            {
                Vector2 fireVel = new Vector2(0, -Main.rand.NextFloat(1f, 2.5f))
                    .RotatedByRandom(0.3f);
                Dust fire = Dust.NewDustPerfect(player.Center + new Vector2(0, -10), DustID.GoldFlame,
                    fireVel, 0, StarGold, 0.7f);
                fire.noGravity = true;
            }
        }

        /// <summary>
        /// Execution Fan launch VFX — full cinematic burst.
        /// Screen darken → void cracks → gold flash → screen shake → aftershock ripple.
        /// </summary>
        public static void ExecutionFanLaunchVFX(Vector2 center, bool isMaxCharge)
        {
            if (Main.dedServ) return;

            float intensity = isMaxCharge ? 1.5f : 1.0f;

            // Screen shake
            try
            {
                global::MagnumOpus.Common.Systems.MagnumScreenEffects.AddScreenShake(isMaxCharge ? 12f : 8f);
            }
            catch { }

            // Void implosion ring
            int ringCount = isMaxCharge ? 3 : 2;
            for (int r = 0; r < ringCount; r++)
            {
                float delay = r * 0.12f;
                Color ringColor = r == 0 ? CosmicPurple : (r == 1 ? Violet : StarGold);
                var ring = new BloomRingParticle(center, Vector2.Zero,
                    ringColor, (0.6f + r * 0.2f) * intensity, 20 + r * 5);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            // Radial void particle burst (inward then outward)
            int burstCount = isMaxCharge ? 30 : 20;
            for (int i = 0; i < burstCount; i++)
            {
                float angle = MathHelper.TwoPi * i / burstCount;
                float speed = Main.rand.NextFloat(4f, 10f) * intensity;
                Vector2 vel = angle.ToRotationVector2() * speed;

                Color c = i % 3 == 0
                    ? Color.Lerp(StarGold, StellarWhite, Main.rand.NextFloat())
                    : Color.Lerp(VoidBlack, CosmicPurple, Main.rand.NextFloat());

                Dust d = Dust.NewDustPerfect(center, i % 3 == 0 ? DustID.GoldFlame : DustID.PurpleTorch,
                    vel, 0, c, (1.2f + Main.rand.NextFloat(0.5f)) * intensity);
                d.noGravity = true;
            }

            // Shattered starlight (max charge gets more)
            int shardCount = isMaxCharge ? 12 : 6;
            for (int i = 0; i < shardCount; i++)
            {
                Vector2 shardVel = Main.rand.NextVector2Circular(8f, 8f) * intensity;
                Color shardColor = Color.Lerp(CosmicPurple, StarGold, Main.rand.NextFloat());
                var shard = new ShatteredStarlightParticle(center, shardVel,
                    shardColor, Main.rand.NextFloat(0.5f, 1.0f) * intensity, Main.rand.Next(25, 45));
                MagnumParticleHandler.SpawnParticle(shard);
            }

            // Void-consumed music notes spiraling inward
            for (int i = 0; i < 5; i++)
            {
                float noteAngle = MathHelper.TwoPi * i / 5f;
                Vector2 notePos = center + noteAngle.ToRotationVector2() * Main.rand.NextFloat(40f, 70f);
                Vector2 noteVel = (center - notePos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 4f);
                Color noteColor = Color.Lerp(NachtmusikPalette.DeepBlue, Violet, Main.rand.NextFloat());

                var note = new HueShiftingMusicNoteParticle(
                    notePos, noteVel, noteColor, StarGold,
                    Main.rand.NextFloat(0.35f, 0.6f), Main.rand.Next(20, 35));
                MagnumParticleHandler.SpawnParticle(note);
            }

            // Dramatic flare at center
            var flare = new DramaticFlareParticle(center, Vector2.Zero,
                isMaxCharge ? StarGold : CosmicPurple, 1.2f * intensity, 15);
            MagnumParticleHandler.SpawnParticle(flare);

            Lighting.AddLight(center, StarGold.ToVector3() * 0.8f * intensity);
        }

        /// <summary>
        /// Draw void accumulation on the blade in PreDrawInWorld.
        /// Blade progressively darkens with void + gold fire edge at high charge.
        /// Uses 4 additive bloom layers that react to charge level.
        /// </summary>
        public static void DrawVoidAccumulation(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale, float chargeProgress)
        {
            if (chargeProgress <= 0.01f) return;

            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + MathF.Sin(time * 1.8f) * 0.06f * (1f + chargeProgress);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Deep void outer (always, scales with charge)
            Color outerVoid = NachtmusikPalette.Additive(VoidBlack, 0.30f * chargeProgress);
            sb.Draw(tex, pos, null, outerVoid,
                rotation, origin, scale * pulse * (1.25f + chargeProgress * 0.15f), SpriteEffects.None, 0f);

            // Layer 2: Cosmic purple corona (>25%)
            if (chargeProgress > 0.25f)
            {
                float purpleIntensity = (chargeProgress - 0.25f) / 0.75f;
                Color purpleCorona = NachtmusikPalette.Additive(CosmicPurple, 0.25f * purpleIntensity);
                sb.Draw(tex, pos, null, purpleCorona,
                    rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);
            }

            // Layer 3: Violet shimmer (>50%)
            if (chargeProgress > 0.5f)
            {
                float violetIntensity = (chargeProgress - 0.5f) / 0.5f;
                Color violetShimmer = NachtmusikPalette.Additive(Violet, 0.20f * violetIntensity);
                sb.Draw(tex, pos, null, violetShimmer,
                    rotation, origin, scale * pulse * 1.08f, SpriteEffects.None, 0f);
            }

            // Layer 4: Gold fire edge (>75% — the blade's edge ignites)
            if (chargeProgress > 0.75f)
            {
                float goldIntensity = (chargeProgress - 0.75f) / 0.25f;
                float goldPulse = 1f + MathF.Sin(time * 4f) * 0.12f * goldIntensity;
                Color goldFire = NachtmusikPalette.Additive(StarGold, 0.22f * goldIntensity);
                sb.Draw(tex, pos, null, goldFire,
                    rotation, origin, scale * goldPulse * 1.03f, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
