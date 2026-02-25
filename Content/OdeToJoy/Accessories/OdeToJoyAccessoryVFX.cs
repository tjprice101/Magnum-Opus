using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.OdeToJoy.Accessories
{
    /// <summary>
    /// Centralized VFX for all Ode to Joy accessories.
    /// Each accessory has its own section with dedicated methods.
    /// All colors come from OdeToJoyPalette, all shared effects from OdeToJoyVFXLibrary.
    /// </summary>
    public static class OdeToJoyAccessoryVFX
    {
        // ===================================================================
        //  THE FLOWERING CODA  (Magic — blooming petals heal, joyous bloom)
        // ===================================================================

        /// <summary>
        /// Ambient particles around the player while The Flowering Coda is equipped.
        /// Drifting petal motes + golden pollen sparkles.
        /// </summary>
        public static void TheFloweringCodaAmbientVFX(Player player)
        {
            if (Main.dedServ) return;

            // Drifting rose petal motes
            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dustPos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(20f, 45f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-1.2f, -0.3f));
                Color dustColor = Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.PetalPink, Main.rand.NextFloat());

                var glow = new GenericGlowParticle(dustPos, vel, dustColor * 0.6f, 0.18f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Occasional golden pollen sparkles
            if (Main.rand.NextBool(10))
            {
                OdeToJoyVFXLibrary.SpawnPollenSparkles(player.Center, 1, 30f);
            }

            // Occasional music note petals
            if (Main.rand.NextBool(18))
            {
                OdeToJoyVFXLibrary.SpawnPetalMusicNotes(player.Center, 1, 25f);
            }

            // Subtle ambient light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 0.9f;
            Lighting.AddLight(player.Center, OdeToJoyPalette.RosePink.ToVector3() * pulse * 0.25f);
        }

        /// <summary>
        /// On-hit VFX for The Flowering Coda — petal burst with healing shimmer.
        /// Blooming petals heal 3% of damage dealt.
        /// </summary>
        public static void TheFloweringCodaOnHitVFX(Vector2 position)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.BlossomImpact(position, 0.5f);
            OdeToJoyVFXLibrary.SpawnRosePetals(position, 3, 20f);
            OdeToJoyVFXLibrary.SpawnPollenSparkles(position, 2, 15f);
            Lighting.AddLight(position, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Joyous bloom explosion VFX — triggered every 10th magic hit.
        /// Large petal burst + garden aura + healing wave.
        /// </summary>
        public static void FloweringCodaBloomExplosionVFX(Vector2 position)
        {
            if (Main.dedServ) return;

            // Central bloom burst
            OdeToJoyVFXLibrary.BloomBurst(position, 0.8f);
            OdeToJoyVFXLibrary.TriumphantCelebration(position, 0.6f);

            // Expanding petal ring
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color petalColor = Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.WhiteBloom, Main.rand.NextFloat());
                var petal = new GenericGlowParticle(position, vel, petalColor * 0.8f, 0.25f, 22, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }

            // Golden pollen shower
            OdeToJoyVFXLibrary.SpawnPollenSparkles(position, 8, 40f);
            OdeToJoyVFXLibrary.SpawnRosePetals(position, 6, 35f);

            // Halo rings
            OdeToJoyVFXLibrary.SpawnPetalHaloRings(position, 2, 0.4f);

            // Music note burst
            OdeToJoyVFXLibrary.MusicNoteBurst(position, OdeToJoyPalette.RosePink, 4, 4f);

            // Healing shimmer flare
            CustomParticles.GenericFlare(position, OdeToJoyPalette.GoldenPollen, 0.6f, 16);
            CustomParticles.GenericFlare(position, OdeToJoyPalette.WhiteBloom * 0.7f, 0.45f, 12);

            Lighting.AddLight(position, OdeToJoyPalette.GoldenPollen.ToVector3() * 1.2f);
        }

        // ===================================================================
        //  THE VERDANT REFRAIN  (Summon — verdant trails, healing flowers)
        // ===================================================================

        /// <summary>
        /// Ambient particles while The Verdant Refrain is equipped.
        /// Orbiting leaf particles + vine trail wisps.
        /// </summary>
        public static void TheVerdantRefrainAmbientVFX(Player player)
        {
            if (Main.dedServ) return;

            // Orbiting leaf particles at varying radii
            Color[] leafColors = { OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.LeafGreen, OdeToJoyPalette.BudGreen };
            float[] orbitSpeeds = { 0.025f, 0.04f, 0.055f };
            float[] orbitRadii = { 28f, 40f, 52f };

            for (int p = 0; p < 3; p++)
            {
                if (Main.GameUpdateCount % 5 != (uint)p) continue;

                float angle = Main.GameUpdateCount * orbitSpeeds[p];
                Vector2 leafPos = player.Center + angle.ToRotationVector2() * orbitRadii[p];

                var leaf = new GenericGlowParticle(leafPos, Vector2.Zero,
                    leafColors[p] * 0.55f, 0.17f, 8, true);
                MagnumParticleHandler.SpawnParticle(leaf);
            }

            // Occasional vine trail wisps
            if (Main.rand.NextBool(8))
            {
                Vector2 vinePos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                OdeToJoyVFXLibrary.SpawnVineTrailDust(vinePos, new Vector2(0f, -0.5f));
            }

            // Subtle garden glow
            if (Main.rand.NextBool(16))
            {
                CustomParticles.GenericFlare(player.Center, OdeToJoyPalette.BudGreen * 0.4f, 0.2f, 10);
            }

            Lighting.AddLight(player.Center, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.2f);
        }

        /// <summary>
        /// On-hit VFX for The Verdant Refrain — vine trail burst on minion hit.
        /// Minions leave verdant trails behind.
        /// </summary>
        public static void TheVerdantRefrainOnHitVFX(Vector2 position)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.GardenImpact(position, 0.5f);
            OdeToJoyVFXLibrary.SpawnVineTrailDust(position, Main.rand.NextVector2Circular(2f, 2f));
            OdeToJoyVFXLibrary.SpawnMusicNotes(position, 2, 18f);
            Lighting.AddLight(position, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Healing flower VFX — 15% chance on minion hit.
        /// Blooming flower with restorative pollen burst.
        /// </summary>
        public static void VerdantRefrainHealingFlowerVFX(Vector2 position)
        {
            if (Main.dedServ) return;

            // Central flower bloom
            OdeToJoyVFXLibrary.BlossomImpact(position, 0.7f);

            // Flower petal ring expanding outward
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color flowerColor = Color.Lerp(OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.RosePink, Main.rand.NextFloat());
                var petal = new GenericGlowParticle(position, vel, flowerColor * 0.7f, 0.2f, 18, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }

            // Rising healing pollen
            for (int i = 0; i < 5; i++)
            {
                Vector2 pollenPos = position + Main.rand.NextVector2Circular(15f, 15f);
                Vector2 pollenVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2f, -1f));
                var pollen = new GenericGlowParticle(pollenPos, pollenVel,
                    OdeToJoyPalette.GoldenPollen * 0.8f, 0.15f, 20, true);
                MagnumParticleHandler.SpawnParticle(pollen);
            }

            // Garden aura
            OdeToJoyVFXLibrary.SpawnGardenAura(position, 30f);
            OdeToJoyVFXLibrary.SpawnPetalMusicNotes(position, 2, 20f);

            // Healing flare
            CustomParticles.GenericFlare(position, OdeToJoyPalette.GoldenPollen, 0.45f, 14);

            Lighting.AddLight(position, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.9f);
        }

        // ===================================================================
        //  CONDUCTOR'S CORSAGE  (Melee — lifesteal, crit bloom explosions)
        // ===================================================================

        /// <summary>
        /// Ambient particles while Conductor's Corsage is equipped.
        /// Warm petal swirl + corsage glow.
        /// </summary>
        public static void ConductorsCorsageAmbientVFX(Player player)
        {
            if (Main.dedServ) return;

            // Warm petal swirl around player
            if (Main.rand.NextBool(6))
            {
                float angle = Main.GameUpdateCount * 0.035f + Main.rand.NextFloat(MathHelper.Pi);
                float radius = 22f + Main.rand.NextFloat(18f);
                Vector2 petalPos = player.Center + angle.ToRotationVector2() * radius;
                Vector2 vel = angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 1.2f;
                Color corsageColor = Color.Lerp(OdeToJoyPalette.PetalPink, OdeToJoyPalette.WarmAmber, Main.rand.NextFloat());

                var petal = new GenericGlowParticle(petalPos, vel, corsageColor * 0.55f, 0.16f, 16, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }

            // Occasional warm amber flare at center
            if (Main.rand.NextBool(14))
            {
                CustomParticles.GenericFlare(player.Center, OdeToJoyPalette.WarmAmber * 0.5f, 0.22f, 10);
            }

            // Corsage glow pulse
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.9f;
            Lighting.AddLight(player.Center, OdeToJoyPalette.WarmAmber.ToVector3() * pulse * 0.22f);
        }

        /// <summary>
        /// On-hit VFX for Conductor's Corsage — warm impact with lifesteal shimmer.
        /// 12% lifesteal on melee hits.
        /// </summary>
        public static void ConductorsCorsageOnHitVFX(Vector2 position)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.MeleeImpact(position, 0);
            OdeToJoyVFXLibrary.SpawnRosePetals(position, 2, 15f);

            // Lifesteal shimmer — golden particles rising
            for (int i = 0; i < 3; i++)
            {
                Vector2 shimmerPos = position + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 shimmerVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-1.5f, -0.8f));
                var shimmer = new GenericGlowParticle(shimmerPos, shimmerVel,
                    OdeToJoyPalette.GoldenPollen * 0.7f, 0.12f, 14, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }

            Lighting.AddLight(position, OdeToJoyPalette.WarmAmber.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Critical hit bloom explosion VFX — blooming corsage burst that heals allies.
        /// Triggered on melee critical hits.
        /// </summary>
        public static void ConductorsCorsageCritBloomVFX(Vector2 position)
        {
            if (Main.dedServ) return;

            // Central bloom explosion
            OdeToJoyVFXLibrary.BloomBurst(position, 0.7f);
            OdeToJoyVFXLibrary.MusicalImpact(position, 0.6f, true);

            // Expanding corsage petal ring
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 5f);
                Color bloomColor = Color.Lerp(OdeToJoyPalette.PetalPink, OdeToJoyPalette.WarmAmber, Main.rand.NextFloat());
                var bloom = new GenericGlowParticle(position, vel, bloomColor * 0.8f, 0.22f, 20, true);
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // Healing ally wave — golden pollen radiating
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 healPos = position + angle.ToRotationVector2() * 25f;
                CustomParticles.GenericFlare(healPos, OdeToJoyPalette.GoldenPollen * 0.9f, 0.3f, 16);
            }

            // Halo rings
            OdeToJoyVFXLibrary.SpawnPetalHaloRings(position, 2, 0.35f);
            OdeToJoyVFXLibrary.SpawnMusicNotes(position, 3, 25f);

            // Central flare
            CustomParticles.GenericFlare(position, OdeToJoyPalette.WhiteBloom, 0.5f, 14);

            Lighting.AddLight(position, OdeToJoyPalette.WarmAmber.ToVector3() * 1.1f);
        }

        // ===================================================================
        //  SYMPHONY OF BLOSSOMS  (Ranged — vine mark debuff, petal storm)
        // ===================================================================

        /// <summary>
        /// Ambient particles while Symphony of Blossoms is equipped.
        /// Drifting blossom particles + pollen motes.
        /// </summary>
        public static void SymphonyOfBlossomsAmbientVFX(Player player)
        {
            if (Main.dedServ) return;

            // Drifting blossom particles
            if (Main.rand.NextBool(7))
            {
                Vector2 blossomPos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-0.6f, -0.2f));
                Color blossomColor = OdeToJoyVFXLibrary.GetPaletteColor(Main.rand.NextFloat());

                var blossom = new GenericGlowParticle(blossomPos, vel, blossomColor * 0.5f, 0.16f, 18, true);
                MagnumParticleHandler.SpawnParticle(blossom);
            }

            // Pollen motes
            if (Main.rand.NextBool(12))
            {
                OdeToJoyVFXLibrary.SpawnPollenSparkles(player.Center, 1, 35f);
            }

            // Occasional vine accent
            if (Main.rand.NextBool(20))
            {
                Vector2 vinePos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                OdeToJoyVFXLibrary.SpawnVineTrailDust(vinePos, new Vector2(0f, -0.4f));
            }

            Lighting.AddLight(player.Center, OdeToJoyPalette.BudGreen.ToVector3() * 0.2f);
        }

        /// <summary>
        /// On-hit VFX for Symphony of Blossoms — vine mark application.
        /// Blooming vine mark debuff applied to target.
        /// </summary>
        public static void SymphonyOfBlossomsOnHitVFX(Vector2 position)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.ProjectileImpact(position, 0.5f);

            // Vine mark application — wrapping vine particles
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vineEnd = position + angle.ToRotationVector2() * Main.rand.NextFloat(12f, 22f);
                OdeToJoyVFXLibrary.SpawnVineTrailDust(vineEnd, (position - vineEnd) * 0.1f);
            }

            OdeToJoyVFXLibrary.SpawnRosePetals(position, 2, 12f);
            Lighting.AddLight(position, OdeToJoyPalette.LeafGreen.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Homing petal storm VFX — triggered every 5th ranged shot.
        /// Spiraling petal vortex that homes in on enemies.
        /// </summary>
        public static void SymphonyOfBlossomsStormTriggerVFX(Vector2 position)
        {
            if (Main.dedServ) return;

            // Central storm vortex
            OdeToJoyVFXLibrary.BloomBurst(position, 0.7f);
            OdeToJoyVFXLibrary.FinisherSlam(position, 0.6f);

            // Spiraling petal vortex expanding outward
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                float spiralOffset = i * 0.15f;
                Vector2 vel = (angle + spiralOffset).ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                float t = (float)i / 16f;
                Color stormColor = Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.VerdantGreen, t);
                var petal = new GenericGlowParticle(position, vel, stormColor * 0.75f, 0.2f, 24, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }

            // Inner vine tendrils radiating
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 tendrilEnd = position + angle.ToRotationVector2() * 35f;
                OdeToJoyVFXLibrary.SpawnVineTrailDust(tendrilEnd, (tendrilEnd - position).SafeNormalize(Vector2.Zero) * 2f);
            }

            // Petal halo rings
            OdeToJoyVFXLibrary.SpawnPetalHaloRings(position, 3, 0.4f);
            OdeToJoyVFXLibrary.SpawnRosePetals(position, 8, 40f);

            // Music note burst
            OdeToJoyVFXLibrary.MusicNoteBurst(position, OdeToJoyPalette.PetalPink, 5, 5f);

            // Storm center flash
            CustomParticles.GenericFlare(position, OdeToJoyPalette.WhiteBloom, 0.55f, 14);
            CustomParticles.GenericFlare(position, OdeToJoyPalette.SunlightYellow * 0.6f, 0.4f, 12);

            Lighting.AddLight(position, OdeToJoyPalette.VerdantGreen.ToVector3() * 1.2f);
        }

        // ===================================================================
        //  SHARED ACCESSORY HELPERS
        // ===================================================================

        /// <summary>
        /// Generic Ode to Joy accessory aura for use by any OdeToJoy accessory.
        /// Subtle garden particles + ambient light.
        /// </summary>
        public static void GenericOdeToJoyAuraVFX(Player player, float intensity = 0.3f)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(10))
            {
                Vector2 offset = Main.rand.NextVector2Circular(35f, 35f);
                Color auraColor = OdeToJoyVFXLibrary.GetPaletteColor(Main.rand.NextFloat()) * intensity;
                var glow = new GenericGlowParticle(player.Center + offset,
                    Main.rand.NextVector2Circular(0.5f, 0.5f), auraColor, 0.15f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            Lighting.AddLight(player.Center, OdeToJoyPalette.VerdantGreen.ToVector3() * intensity * 0.3f);
        }

        /// <summary>
        /// Standard Ode to Joy accessory PreDrawInWorld bloom layers.
        /// </summary>
        public static void DrawWorldItemBloom(SpriteBatch sb, Texture2D texture,
            Vector2 position, Vector2 origin, float rotation, float scale)
        {
            if (Main.dedServ) return;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.08f + 1f;
            OdeToJoyPalette.DrawItemBloom(sb, texture, position, origin, rotation, scale, pulse);
        }
    }
}
