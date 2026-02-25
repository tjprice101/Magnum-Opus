using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Screen;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.OdeToJoy;

namespace MagnumOpus.Content.OdeToJoy.Weapons.Melee
{
    // =============================================================================
    //  ROSE THORN CHAINSAW  VFX
    //  Identity: Rapid shredding thorns, rose petals flying, nature's fury.
    //  Drill AI chainsaw, 4400 damage. Relentless garden carnage.
    // =============================================================================

    /// <summary>
    /// VFX helper for the RoseThornChainsaw melee weapon.
    /// Handles chainsaw idle ambient, world bloom, per-frame sawdust/petal trail,
    /// chain thorn spawn/impact/ricochet VFX, and crit bursts.
    /// </summary>
    public static class RoseThornChainsawVFX
    {
        /// <summary>
        /// Ambient chainsaw idle particles when holding the item.
        /// Gentle petal drift and green motes around the player.
        /// </summary>
        public static void ChainsawHoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Orbiting thorn embers (2 motes)
            for (int i = 0; i < 2; i++)
            {
                float angle = time * 0.05f + MathHelper.Pi * i;
                float radius = 16f + MathF.Sin(time * 0.07f + i) * 3f;
                Vector2 motePos = center + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(3))
                {
                    Color col = Color.Lerp(OdeToJoyPalette.PetalPink, OdeToJoyPalette.VerdantGreen, (float)i / 2f);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.PinkFairy, Vector2.Zero, 0, col, 0.7f);
                    d.noGravity = true;
                    d.fadeIn = 0.5f;
                }
            }

            // Occasional leaf drift
            if (Main.rand.NextBool(8))
            {
                Vector2 leafPos = center + Main.rand.NextVector2Circular(20f, 20f);
                Vector2 leafVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.4f - Main.rand.NextFloat(0.3f));
                Dust d = Dust.NewDustPerfect(leafPos, DustID.GreenFairy, leafVel, 0, OdeToJoyPalette.LeafGreen, 0.9f);
                d.noGravity = true;
            }

            // Rare music note
            if (Main.rand.NextBool(25))
                OdeToJoyVFXLibrary.SpawnMusicNotes(center, 1, 18f, 0.7f, 0.9f, 30);

            float pulse = 0.4f + MathF.Sin(time * 0.05f) * 0.12f;
            Lighting.AddLight(center, OdeToJoyPalette.VerdantGreen.ToVector3() * pulse);
        }

        /// <summary>
        /// Item world glow using OdeToJoyPalette.DrawItemBloom.
        /// </summary>
        public static void ChainsawPreDrawInWorldBloom(SpriteBatch sb, Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.05f) * 0.03f;
            OdeToJoyPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        /// <summary>
        /// Per-frame sawdust/petal spray trail during chainsaw contact.
        /// Dense but balanced: 2 petal glow particles, 1 leaf particle,
        /// and a golden sparkle every other frame.
        /// </summary>
        public static void ChainsawTrailVFX(Vector2 center, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.UnitY);

            // 2 petal-pink glow particles (sawdust spray)
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = away * Main.rand.NextFloat(1.5f, 3.5f) + Main.rand.NextVector2Circular(1f, 1f);
                Color col = OdeToJoyPalette.GetPetalGradient(Main.rand.NextFloat());
                var glow = new GenericGlowParticle(center, vel, col, Main.rand.NextFloat(0.18f, 0.26f), 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // 1 leaf/vine particle
            {
                Vector2 vel = away * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(0.6f, 0.6f);
                Dust d = Dust.NewDustPerfect(center, DustID.GreenFairy, vel, 0, OdeToJoyPalette.VerdantGreen, 1.3f);
                d.noGravity = true;
            }

            // Golden sparkle every other frame
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkVel = away * Main.rand.NextFloat(0.8f, 2f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                var sparkle = new SparkleParticle(center, sparkVel, OdeToJoyPalette.GoldenPollen, 0.22f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(center, OdeToJoyPalette.PetalPink.ToVector3() * 0.5f);
        }

        /// <summary>
        /// VFX when a chain thorn projectile spawns.
        /// Blossom impact flash with petal music notes.
        /// </summary>
        public static void ChainsawChainBurstVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.BlossomImpact(pos, 0.6f);
            OdeToJoyVFXLibrary.SpawnPetalMusicNotes(pos, 3, 20f);

            Lighting.AddLight(pos, OdeToJoyPalette.RosePink.ToVector3() * 0.7f);
        }

        /// <summary>
        /// On-hit VFX when the chainsaw damages an NPC.
        /// MeleeImpact, 4 music notes, and rose petals.
        /// </summary>
        public static void ChainsawImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.MeleeImpact(pos);
            OdeToJoyVFXLibrary.SpawnMusicNotes(pos, 4, 25f);
            OdeToJoyVFXLibrary.SpawnRosePetals(pos, 4, 30f);

            Lighting.AddLight(pos, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Critical hit VFX with extra thorn chain visuals.
        /// GardenImpact at 1.2f scale, MusicNoteBurst with 8 notes.
        /// </summary>
        public static void ChainsawCritImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.GardenImpact(pos, 1.2f);
            OdeToJoyVFXLibrary.MusicNoteBurst(pos, OdeToJoyPalette.GoldenPollen, 8, 5f);
            OdeToJoyVFXLibrary.SpawnRosePetals(pos, 6, 35f);

            Lighting.AddLight(pos, OdeToJoyPalette.GoldenPollen.ToVector3() * 1.2f);
        }

        /// <summary>
        /// Trail VFX for the thorn chain projectile.
        /// Rose-green gradient glow particles trailing behind.
        /// </summary>
        public static void ThornChainTrailVFX(Vector2 center, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.UnitY);
            Vector2 vel = away * Main.rand.NextFloat(0.8f, 2f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
            Color col = OdeToJoyPalette.GetGardenGradient(Main.rand.NextFloat());
            var glow = new GenericGlowParticle(center, vel, col, Main.rand.NextFloat(0.16f, 0.22f), 16, true);
            MagnumParticleHandler.SpawnParticle(glow);

            // Occasional thorn spark
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(center, DustID.GreenFairy,
                    away * Main.rand.NextFloat(1f, 2f), 0, OdeToJoyPalette.LeafGreen, 1.1f);
                d.noGravity = true;
            }

            Lighting.AddLight(center, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Impact VFX when the thorn chain hits an NPC.
        /// BlossomImpact at 0.7f scale.
        /// </summary>
        public static void ThornChainImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.BlossomImpact(pos, 0.7f);
            OdeToJoyVFXLibrary.SpawnMusicNotes(pos, 2, 20f);

            Lighting.AddLight(pos, OdeToJoyPalette.RosePink.ToVector3() * 0.6f);
        }

        /// <summary>
        /// VFX line between ricochet targets.
        /// Particles along the line with a garden-themed gradient.
        /// </summary>
        public static void ThornChainRicochetVFX(Vector2 from, Vector2 to)
        {
            if (Main.dedServ) return;

            Vector2 dir = (to - from).SafeNormalize(Vector2.UnitX);
            float dist = Vector2.Distance(from, to);
            int segments = (int)(dist / 18f);

            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                Vector2 pos = Vector2.Lerp(from, to, t);
                Color col = OdeToJoyPalette.GetGardenGradient(t);
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(3f, 3f),
                    DustID.GreenFairy, dir * 0.4f, 0, col, 1.0f);
                d.noGravity = true;
            }

            // Bloom at destination
            OdeToJoyVFXLibrary.DrawBloom(to, 0.35f);
            Lighting.AddLight(to, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Chain death burst when the thorn chain expires.
        /// Rose petals plus bloom burst.
        /// </summary>
        public static void ThornChainDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.SpawnRosePetals(pos, 5, 30f);
            OdeToJoyVFXLibrary.BloomBurst(pos, 0.7f);
            OdeToJoyVFXLibrary.SpawnPetalMusicNotes(pos, 2, 15f);

            Lighting.AddLight(pos, OdeToJoyPalette.RosePink.ToVector3() * 0.7f);
        }
    }

    // =============================================================================
    //  THORNBOUND RECKONING  VFX
    //  Identity: Massive vine-wrapped strikes, cascading nature energy.
    //  Greatsword with vine waves, 4200 damage. Every 4th swing is a signature slam.
    // =============================================================================

    /// <summary>
    /// VFX helper for the ThornboundReckoning greatsword.
    /// Handles hold-item vine ambient, swing VFX, per-frame trail,
    /// combo impacts, vine wave projectile, and bloom explosion mark VFX.
    /// </summary>
    public static class ThornboundReckoningVFX
    {
        /// <summary>
        /// Ambient vine/leaf particles floating around the player while holding.
        /// </summary>
        public static void ReckoningHoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // 3-point orbiting vine motes
            for (int i = 0; i < 3; i++)
            {
                float angle = time * 0.03f + MathHelper.TwoPi * i / 3f;
                float radius = 22f + MathF.Sin(time * 0.05f + i * 1.2f) * 5f;
                Vector2 motePos = center + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(4))
                {
                    Color col = Color.Lerp(OdeToJoyPalette.LeafGreen, OdeToJoyPalette.VerdantGreen, Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(motePos, DustID.GreenFairy, Vector2.Zero, 0, col, 0.8f);
                    d.noGravity = true;
                    d.fadeIn = 0.6f;
                }
            }

            // Rising leaf drift
            if (Main.rand.NextBool(6))
            {
                Vector2 leafPos = center + Main.rand.NextVector2Circular(25f, 25f);
                Vector2 leafVel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -0.5f - Main.rand.NextFloat(0.5f));
                Color col = Color.Lerp(OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.BudGreen, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(leafPos, DustID.GreenFairy, leafVel, 0, col, 1.0f);
                d.noGravity = true;
            }

            // Rare music note
            if (Main.rand.NextBool(30))
                OdeToJoyVFXLibrary.SpawnMusicNotes(center, 1, 20f, 0.65f, 0.85f, 30);

            float pulse = 0.45f + MathF.Sin(time * 0.04f) * 0.12f;
            Lighting.AddLight(center, OdeToJoyPalette.VerdantGreen.ToVector3() * pulse);
        }

        /// <summary>
        /// World item bloom using OdeToJoyPalette.DrawItemBloom.
        /// </summary>
        public static void ReckoningPreDrawInWorldBloom(SpriteBatch sb, Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            OdeToJoyPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        /// <summary>
        /// On-swing VFX: directional sparks, music notes, and petal burst.
        /// </summary>
        public static void ReckoningSwingVFX(Vector2 swingCenter, Vector2 direction)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.SpawnDirectionalSparks(swingCenter, direction, 6, 6f);
            OdeToJoyVFXLibrary.SpawnMusicNotes(swingCenter, 2, 20f);

            // Petal burst on swing start
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = direction.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f)) * Main.rand.NextFloat(2f, 5f);
                Color col = OdeToJoyPalette.GetPetalGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(swingCenter, DustID.PinkFairy, vel, 0, col, 1.4f);
                d.noGravity = true;
            }

            Lighting.AddLight(swingCenter, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.7f);
        }

        /// <summary>
        /// Per-frame swing trail: garden gradient glow particles,
        /// JungleGrass dust, and periodic music notes.
        /// </summary>
        public static void ReckoningMeleeEffectsVFX(Vector2 hitboxPos, Vector2 playerVelocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -playerVelocity.SafeNormalize(Vector2.UnitY);

            // Garden gradient glow particle
            {
                Vector2 vel = away * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(0.8f, 0.8f);
                Color col = OdeToJoyPalette.GetGardenGradient(Main.rand.NextFloat());
                var glow = new GenericGlowParticle(hitboxPos, vel, col, Main.rand.NextFloat(0.18f, 0.24f), 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // JungleGrass dust
            {
                Vector2 vel = away * Main.rand.NextFloat(0.5f, 2f) + Main.rand.NextVector2Circular(0.4f, 0.4f);
                Color col = Color.Lerp(OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.LeafGreen, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(hitboxPos, DustID.JungleGrass, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            // Periodic music notes
            if (Main.rand.NextBool(5))
                OdeToJoyVFXLibrary.SpawnMusicNotes(hitboxPos, 1, 10f, 0.7f, 0.9f, 25);

            Lighting.AddLight(hitboxPos, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.5f);
        }

        /// <summary>
        /// OnHitNPC impact VFX. MeleeImpact for normal hits,
        /// GardenImpact at 1.5f for crits with MusicNoteBurst.
        /// </summary>
        public static void ReckoningImpactVFX(Vector2 pos, bool isCrit)
        {
            if (Main.dedServ) return;

            if (isCrit)
            {
                OdeToJoyVFXLibrary.GardenImpact(pos, 1.5f);
                OdeToJoyVFXLibrary.MusicNoteBurst(pos, OdeToJoyPalette.GoldenPollen, 10, 5f);
                OdeToJoyVFXLibrary.SpawnRosePetals(pos, 5, 35f);
                Lighting.AddLight(pos, OdeToJoyPalette.GoldenPollen.ToVector3() * 1.3f);
            }
            else
            {
                OdeToJoyVFXLibrary.MeleeImpact(pos);
                OdeToJoyVFXLibrary.SpawnMusicNotes(pos, 3, 25f);
                Lighting.AddLight(pos, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.8f);
            }
        }

        /// <summary>
        /// Every 4th swing signature VFX: FinisherSlam at 1.1f intensity,
        /// massive music note burst (10 petal notes + 10 green notes).
        /// </summary>
        public static void ReckoningFourthSwingVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.FinisherSlam(pos, 1.1f);

            // 10 petal-hue music notes
            OdeToJoyVFXLibrary.SpawnPetalMusicNotes(pos, 10, 40f);

            // 10 green/gold music notes
            OdeToJoyVFXLibrary.MusicNoteBurst(pos, OdeToJoyPalette.VerdantGreen, 10, 5f);

            OdeToJoyVFXLibrary.SpawnRosePetals(pos, 8, 50f);
            OdeToJoyVFXLibrary.SpawnPollenSparkles(pos, 6, 35f);

            Lighting.AddLight(pos, OdeToJoyPalette.WhiteBloom.ToVector3() * 1.5f);
        }

        /// <summary>
        /// Vine wave projectile trail. Garden gradient glow particles
        /// trailing behind the projectile.
        /// </summary>
        public static void VineWaveTrailVFX(Vector2 center, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.UnitY);

            // Garden gradient glow particle
            Vector2 vel = away * Main.rand.NextFloat(0.5f, 1.8f) + Main.rand.NextVector2Circular(0.6f, 0.6f);
            Color col = OdeToJoyPalette.GetGardenGradient(Main.rand.NextFloat());
            var glow = new GenericGlowParticle(center, vel, col, Main.rand.NextFloat(0.16f, 0.22f), 18, true);
            MagnumParticleHandler.SpawnParticle(glow);

            // Vine dust trail
            OdeToJoyVFXLibrary.SpawnVineTrailDust(center, velocity);

            Lighting.AddLight(center, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.45f);
        }

        /// <summary>
        /// Vine wave hit VFX. BlossomImpact at 0.6f scale.
        /// </summary>
        public static void VineWaveImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.BlossomImpact(pos, 0.6f);
            OdeToJoyVFXLibrary.SpawnMusicNotes(pos, 2, 18f);

            Lighting.AddLight(pos, OdeToJoyPalette.RosePink.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Expanding bloom mark rings. At spawn (progress=0): massive petal ring burst.
        /// As progress increases, the rings expand and fade.
        /// </summary>
        public static void BloomExplosionVFX(Vector2 center, float progress)
        {
            if (Main.dedServ) return;

            // On spawn: massive petal ring burst
            if (progress <= 0f)
            {
                // Petal ring burst (radial petals)
                for (int i = 0; i < 16; i++)
                {
                    float angle = MathHelper.TwoPi * i / 16f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                    Color col = OdeToJoyPalette.GetBlossomGradient((float)i / 16f);
                    Dust d = Dust.NewDustPerfect(center, i % 2 == 0 ? DustID.PinkFairy : DustID.GreenFairy,
                        vel, 0, col, 1.5f);
                    d.noGravity = true;
                }

                OdeToJoyVFXLibrary.SpawnGradientHaloRings(center, 5, 0.3f);
                OdeToJoyVFXLibrary.SpawnPetalHaloRings(center, 4, 0.25f);
                OdeToJoyVFXLibrary.DrawBloom(center, 0.6f);
                OdeToJoyVFXLibrary.SpawnMusicNotes(center, 4, 30f);

                Lighting.AddLight(center, OdeToJoyPalette.GoldenPollen.ToVector3() * 1.2f);
            }
            else
            {
                // Expanding ring markers
                float alpha = 1f - progress;
                if (Main.rand.NextBool(3))
                {
                    float ringAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float ringRadius = 30f + progress * 60f;
                    Vector2 ringPos = center + ringAngle.ToRotationVector2() * ringRadius;
                    Color col = OdeToJoyPalette.GetGardenGradient(progress) * alpha;
                    Dust d = Dust.NewDustPerfect(ringPos, DustID.GreenFairy,
                        Vector2.Zero, 0, col, 1.0f * alpha);
                    d.noGravity = true;
                }

                Lighting.AddLight(center, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.4f * alpha);
            }
        }

        /// <summary>
        /// Mark explosion hit VFX. GardenImpact at 1.5f, MusicNoteBurst,
        /// SpawnRosePetals with 10 petals.
        /// </summary>
        public static void BloomExplosionImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.GardenImpact(pos, 1.5f);
            OdeToJoyVFXLibrary.MusicNoteBurst(pos, OdeToJoyPalette.GoldenPollen, 8, 5f);
            OdeToJoyVFXLibrary.SpawnRosePetals(pos, 10, 45f);
            OdeToJoyVFXLibrary.SpawnPollenSparkles(pos, 6, 30f);

            Lighting.AddLight(pos, OdeToJoyPalette.GoldenPollen.ToVector3() * 1.4f);
        }
    }

    // =============================================================================
    //  THE GARDENER'S FURY  VFX
    //  Identity: Lightning-fast floral strikes, building garden frenzy.
    //  Rapier with combo system, 3200 damage. Speed builds beautiful destruction.
    // =============================================================================

    /// <summary>
    /// VFX helper for TheGardenersFury rapier.
    /// Handles hold-item ambient, combo-scaling thrust trails,
    /// combo-glow PreDraw, scaled impacts, max-combo celebration,
    /// and petal sub-projectile trail.
    /// </summary>
    public static class TheGardenersFuryVFX
    {
        /// <summary>
        /// Subtle petal ambient at the staff/rapier tip while held.
        /// </summary>
        public static void FuryHoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Gentle petal drift at weapon position
            if (Main.rand.NextBool(5))
            {
                Vector2 tipOffset = new Vector2(player.direction * 20f, -8f);
                Vector2 tipPos = center + tipOffset;
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.4f - Main.rand.NextFloat(0.3f));
                Color col = Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.PetalPink, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(tipPos, DustID.PinkFairy, vel, 0, col, 0.7f);
                d.noGravity = true;
            }

            // Rare golden sparkle
            if (Main.rand.NextBool(12))
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(15f, 15f);
                Dust d = Dust.NewDustPerfect(sparkPos, DustID.Enchanted_Gold,
                    Main.rand.NextVector2Circular(0.3f, 0.3f), 0, OdeToJoyPalette.GoldenPollen, 0.6f);
                d.noGravity = true;
            }

            // Very rare music note
            if (Main.rand.NextBool(30))
                OdeToJoyVFXLibrary.SpawnMusicNotes(center, 1, 15f, 0.65f, 0.85f, 28);

            float pulse = 0.35f + MathF.Sin(time * 0.05f) * 0.10f;
            Lighting.AddLight(center, OdeToJoyPalette.RosePink.ToVector3() * pulse);
        }

        /// <summary>
        /// World item bloom using OdeToJoyPalette.DrawItemBloom.
        /// </summary>
        public static void FuryPreDrawInWorldBloom(SpriteBatch sb, Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.05f) * 0.03f;
            OdeToJoyPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        /// <summary>
        /// Per-frame rapier trail. Particle density scales with combo stacks.
        /// 1 + comboStacks/3 glow particles per frame, with sparkles at combo >= 5.
        /// </summary>
        public static void FuryThrustTrailVFX(Vector2 center, Vector2 velocity, float comboStacks)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.UnitY);
            int particleCount = 1 + (int)(comboStacks / 3f);

            for (int i = 0; i < particleCount; i++)
            {
                Vector2 vel = away * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(0.7f, 0.7f);
                Color col = OdeToJoyPalette.GetPetalGradient(Main.rand.NextFloat());
                float glowScale = Main.rand.NextFloat(0.16f, 0.22f) + comboStacks * 0.005f;
                var glow = new GenericGlowParticle(center, vel, col, glowScale, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Golden sparkle at combo >= 5
            if (comboStacks >= 5f && Main.rand.NextBool(2))
            {
                Vector2 sparkVel = away * Main.rand.NextFloat(0.5f, 1.5f) + Main.rand.NextVector2Circular(0.4f, 0.4f);
                var sparkle = new SparkleParticle(center, sparkVel,
                    OdeToJoyPalette.GoldenPollen, 0.20f + comboStacks * 0.01f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            float lightIntensity = 0.4f + comboStacks * 0.05f;
            Lighting.AddLight(center, OdeToJoyPalette.RosePink.ToVector3() * MathHelper.Clamp(lightIntensity, 0f, 1.2f));
        }

        /// <summary>
        /// Glow layers scaled by combo for PreDraw rendering.
        /// RosePink outer glow + GoldenPollen inner glow,
        /// intensity = 0.3f + comboStacks * 0.07f.
        /// </summary>
        public static void FuryThrustPreDraw(SpriteBatch sb, Texture2D tex,
            Vector2 drawPos, Vector2 origin, float rotation, float scale, float comboStacks)
        {
            float intensity = 0.3f + comboStacks * 0.07f;
            intensity = MathHelper.Clamp(intensity, 0.3f, 1.2f);

            // Outer rose pink glow
            sb.Draw(tex, drawPos, null,
                (OdeToJoyPalette.RosePink with { A = 0 }) * intensity * 0.5f,
                rotation, origin, scale * 1.08f, SpriteEffects.None, 0f);

            // Inner golden pollen glow
            sb.Draw(tex, drawPos, null,
                (OdeToJoyPalette.GoldenPollen with { A = 0 }) * intensity * 0.4f,
                rotation, origin, scale * 1.03f, SpriteEffects.None, 0f);

            // White-hot core at high combo
            if (comboStacks >= 5f)
            {
                float coreIntensity = (comboStacks - 5f) * 0.04f;
                sb.Draw(tex, drawPos, null,
                    (OdeToJoyPalette.SunlightYellow with { A = 0 }) * MathHelper.Clamp(coreIntensity, 0f, 0.3f),
                    rotation, origin, scale * 1.01f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// OnHitNPC impact VFX. BlossomImpact scaled by combo,
        /// music notes scaled to combo, pollen sparkles at combo >= 5.
        /// </summary>
        public static void FuryImpactVFX(Vector2 pos, int comboStacks)
        {
            if (Main.dedServ) return;

            float impactScale = 0.5f + comboStacks * 0.1f;
            impactScale = MathHelper.Clamp(impactScale, 0.5f, 1.5f);
            OdeToJoyVFXLibrary.BlossomImpact(pos, impactScale);

            int noteCount = 2 + comboStacks / 2;
            noteCount = Math.Min(noteCount, 8);
            OdeToJoyVFXLibrary.SpawnMusicNotes(pos, noteCount, 20f + comboStacks * 2f);

            // Pollen sparkles at combo >= 5
            if (comboStacks >= 5)
            {
                int sparkleCount = 3 + (comboStacks - 5);
                sparkleCount = Math.Min(sparkleCount, 10);
                OdeToJoyVFXLibrary.SpawnPollenSparkles(pos, sparkleCount, 25f);
            }

            float lightIntensity = 0.6f + comboStacks * 0.08f;
            Lighting.AddLight(pos, OdeToJoyPalette.RosePink.ToVector3() * MathHelper.Clamp(lightIntensity, 0f, 1.5f));
        }

        /// <summary>
        /// At 10 stacks crit VFX. TriumphantCelebration at 1.3f scale.
        /// The ultimate garden frenzy payoff.
        /// </summary>
        public static void FuryMaxComboVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.TriumphantCelebration(pos, 1.3f);

            Lighting.AddLight(pos, OdeToJoyPalette.WhiteBloom.ToVector3() * 2.0f);
        }

        /// <summary>
        /// Small petal sub-projectile trail VFX.
        /// Delicate petal glow particles trailing the sub-projectile.
        /// </summary>
        public static void FuryPetalProjectileTrailVFX(Vector2 center, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.UnitY);
            Vector2 vel = away * Main.rand.NextFloat(0.4f, 1.2f) + Main.rand.NextVector2Circular(0.3f, 0.3f);
            Color col = OdeToJoyPalette.GetPetalGradient(Main.rand.NextFloat());
            var glow = new GenericGlowParticle(center, vel, col, Main.rand.NextFloat(0.12f, 0.18f), 14, true);
            MagnumParticleHandler.SpawnParticle(glow);

            // Occasional pink fairy dust
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(center, DustID.PinkFairy,
                    away * Main.rand.NextFloat(0.5f, 1f), 0, OdeToJoyPalette.RosePink, 0.8f);
                d.noGravity = true;
            }

            Lighting.AddLight(center, OdeToJoyPalette.RosePink.ToVector3() * 0.3f);
        }
    }
}
