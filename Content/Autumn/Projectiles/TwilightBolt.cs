using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

// Dynamic particle effects for aesthetically pleasing animations
using static MagnumOpus.Common.Systems.DynamicParticleEffects;

namespace MagnumOpus.Content.Autumn.Projectiles
{
    /// <summary>
    /// Twilight Bolt - Main projectile for Twilight Arbalest
    /// Gains damage over distance traveled (Fading Light mechanic)
    /// </summary>
    public class TwilightBolt : ModProjectile
    {
        // Use a visible energy flare texture for bright projectile
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare4";
        
        private static readonly Color TwilightPurple = new Color(120, 60, 140);
        private static readonly Color TwilightOrange = new Color(255, 120, 60);
        private static readonly Color AutumnGold = new Color(218, 165, 32);

        private float distanceTraveled = 0f;
        private const float MaxDistanceBonus = 400f; // Max distance for bonus
        private const float MaxDamageMultiplier = 1.5f; // +50% max

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.arrow = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Track distance
            distanceTraveled += Projectile.velocity.Length();

            // Trail effect
            if (Main.rand.NextBool(2))
            {
                Vector2 trailVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                float distProgress = Math.Min(1f, distanceTraveled / MaxDistanceBonus);
                Color trailColor = Color.Lerp(TwilightPurple, TwilightOrange, distProgress) * 0.55f;
                var trail = new GenericGlowParticle(Projectile.Center, trailVel, trailColor, 0.27f, 20, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // === VFX VARIATION #15: TWILIGHT GRADIENT SHIFT ===
            // Colors shift from purple to orange as distance increases
            float distProgress2 = Math.Min(1f, distanceTraveled / MaxDistanceBonus);
            Color coreColor = Color.Lerp(TwilightPurple, TwilightOrange, distProgress2);
            CustomParticles.GenericFlare(Projectile.Center, coreColor * 0.4f, 0.22f, 5);

            // === VFX VARIATION #16: FALLING LEAF PARTICLES ===
            // Autumn leaves drift down from the bolt's path
            if (Main.rand.NextBool(4))
            {
                Vector2 leafOffset = Main.rand.NextVector2Circular(12f, 12f);
                Vector2 leafVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(1.5f, 3.5f));
                Color leafColor = Main.rand.NextBool() ? TwilightOrange * 0.55f : AutumnGold * 0.5f;
                var leaf = new GenericGlowParticle(Projectile.Center + leafOffset, leafVel, leafColor, 0.22f, 35, true);
                MagnumParticleHandler.SpawnParticle(leaf);
            }

            // === VFX VARIATION #17: DUSK WISPS ===
            // Mystical wisps orbit and trail behind
            if (Main.GameUpdateCount % 5 == 0)
            {
                float wispAngle = Main.GameUpdateCount * 0.1f;
                for (int w = 0; w < 2; w++)
                {
                    float thisWispAngle = wispAngle + MathHelper.Pi * w;
                    float wispRadius = 10f + (float)Math.Sin(Main.GameUpdateCount * 0.12f + w) * 4f;
                    Vector2 wispPos = Projectile.Center + thisWispAngle.ToRotationVector2() * wispRadius;
                    Color wispColor = w == 0 ? TwilightPurple * 0.65f : TwilightOrange * 0.6f;
                    CustomParticles.GenericFlare(wispPos, wispColor, 0.18f, 10);
                }
            }

            // Twilight notes - scale increases with distance (0.72f to 0.88f)
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f));
                Color noteColor = Color.Lerp(TwilightPurple, TwilightOrange, distProgress2);
                float noteScale = 0.72f + distProgress2 * 0.16f;
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, noteScale, 32);
            }
            
            // Sparkle accents that grow with distance
            if (Main.rand.NextBool(3))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -Projectile.velocity * 0.12f, Color.Lerp(TwilightPurple, AutumnGold, distProgress2) * 0.65f, 0.22f + distProgress2 * 0.12f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // === DYNAMIC PARTICLE EFFECTS - Twilight aura (intensity grows with distance) ===
            if (Main.GameUpdateCount % 6 == 0)
            {
                float intensityMod = 1f + distProgress2 * 0.3f;
                PulsingGlow(Projectile.Center, Vector2.Zero, TwilightPurple, TwilightOrange, 0.26f * intensityMod, 18, 0.12f, 0.22f);
            }
            if (Main.rand.NextBool(4))
            {
                TwinklingSparks(Projectile.Center, AutumnGold, 2, 10f, 0.18f + distProgress2 * 0.06f, 18);
            }

            Lighting.AddLight(Projectile.Center, coreColor.ToVector3() * 0.45f);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Fading Light: Bonus damage based on distance traveled
            float distProgress = Math.Min(1f, distanceTraveled / MaxDistanceBonus);
            float damageMultiplier = MathHelper.Lerp(1f, MaxDamageMultiplier, distProgress);
            modifiers.FinalDamage *= damageMultiplier;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Dusk's Embrace: Crits spawn homing leaf shards
            if (hit.Crit)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 leafVel = Main.rand.NextVector2Circular(6f, 6f);
                        Projectile.NewProjectile(
                            Projectile.GetSource_OnHit(target),
                            target.Center,
                            leafVel,
                            ModContent.ProjectileType<HomingLeafShard>(),
                            Projectile.damage / 3,
                            1f,
                            Projectile.owner
                        );
                    }
                }

                // Crit VFX
                CustomParticles.GenericFlare(target.Center, AutumnGold, 0.6f, 18);
                CustomParticles.GenericFlare(target.Center, TwilightOrange, 0.5f, 16);
                CustomParticles.GenericFlare(target.Center, TwilightOrange * 0.6f, 0.35f, 12);
                // Twilight wisp burst
                for (int wisp = 0; wisp < 4; wisp++)
                {
                    float wispAngle = MathHelper.TwoPi * wisp / 4f;
                    Vector2 wispPos = target.Center + wispAngle.ToRotationVector2() * 14f;
                    CustomParticles.GenericFlare(wispPos, TwilightOrange * 0.7f, 0.18f, 10);
                }

                // ☁EMUSICAL CRIT - VISIBLE triumphant twilight chord (scale 0.85f)
                for (int n = 0; n < 8; n++)
                {
                    float angle = MathHelper.TwoPi * n / 8f;
                    Vector2 noteVel = angle.ToRotationVector2() * 4.5f;
                    ThemedParticles.MusicNote(target.Center, noteVel, AutumnGold, 0.85f, 40);
                }
                // Sparkle ring for golden crit
                for (int s = 0; s < 6; s++)
                {
                    float sAngle = MathHelper.TwoPi * s / 6f;
                    var sparkle = new SparkleParticle(target.Center, sAngle.ToRotationVector2() * 3.5f, AutumnGold * 0.8f, 0.35f, 20);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }

            // Standard hit VFX
            CustomParticles.GenericFlare(target.Center, TwilightPurple, 0.4f, 14);

            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                Color sparkColor = Color.Lerp(TwilightPurple, TwilightOrange, Main.rand.NextFloat()) * 0.5f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // ☁EMUSICAL IMPACT - VISIBLE twilight melody (scale 0.75f)
            for (int n = 0; n < 5; n++)
            {
                float angle = MathHelper.TwoPi * n / 5f;
                Vector2 noteVel = angle.ToRotationVector2() * 3.5f;
                ThemedParticles.MusicNote(target.Center, noteVel, TwilightPurple, 0.75f, 32);
            }

            // === DYNAMIC PARTICLE EFFECTS - Autumn twilight impact ===
            AutumnImpact(target.Center, 1f);
            DramaticImpact(target.Center, TwilightPurple, TwilightOrange, 0.48f, 20);
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, TwilightOrange, 0.4f, 15);
            CustomParticles.GenericFlare(Projectile.Center, TwilightPurple, 0.35f, 14);
            CustomParticles.GenericFlare(Projectile.Center, TwilightPurple * 0.6f, 0.25f, 10);
            // Twilight wisp burst
            for (int wisp = 0; wisp < 4; wisp++)
            {
                float wispAngle = MathHelper.TwoPi * wisp / 4f;
                Vector2 wispPos = Projectile.Center + wispAngle.ToRotationVector2() * 10f;
                CustomParticles.GenericFlare(wispPos, TwilightPurple * 0.7f, 0.15f, 8);
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(4f, 4f);
                Color burstColor = Color.Lerp(TwilightPurple, TwilightOrange, Main.rand.NextFloat()) * 0.5f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // ☁EMUSICAL FINALE - Twilight bolt fading note
            ThemedParticles.MusicNoteBurst(Projectile.Center, TwilightPurple, 6, 3.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare4").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.15f + 1f;
            float distProgress = Math.Min(1f, distanceTraveled / MaxDistanceBonus);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Color mainColor = Color.Lerp(TwilightPurple, TwilightOrange, distProgress);
            
            // ✁EBRIGHT MULTI-LAYER BLOOM - Outer glow
            spriteBatch.Draw(texture, drawPos, null, mainColor with { A = 0 } * 0.35f, 0f, origin, 0.65f * pulse * (1f + distProgress * 0.4f), SpriteEffects.None, 0f);
            // Middle energy layer
            spriteBatch.Draw(texture, drawPos, null, mainColor with { A = 0 } * 0.55f, 0f, origin, 0.45f * pulse, SpriteEffects.None, 0f);
            // Core layer - brighter
            spriteBatch.Draw(texture, drawPos, null, mainColor with { A = 0 } * 0.75f, 0f, origin, 0.28f * pulse, SpriteEffects.None, 0f);
            // White-hot center
            spriteBatch.Draw(texture, drawPos, null, Color.White with { A = 0 } * 0.85f, 0f, origin, 0.15f * pulse, SpriteEffects.None, 0f);
            
            // Orbiting spark points for extra visibility
            float orbitAngle = Main.GameUpdateCount * 0.15f;
            for (int i = 0; i < 3; i++)
            {
                float sparkAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                Vector2 sparkPos = drawPos + sparkAngle.ToRotationVector2() * (12f + distProgress * 6f);
                spriteBatch.Draw(texture, sparkPos, null, TwilightOrange with { A = 0 } * 0.6f, 0f, origin, 0.12f * pulse, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Harvest Moon Bolt - Large seeking projectile fired every 6th shot
    /// </summary>
    public class HarvestMoonBolt : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo6";
        
        private static readonly Color MoonSilver = new Color(200, 200, 220);
        private static readonly Color MoonGold = new Color(218, 165, 32);
        private static readonly Color TwilightPurple = new Color(120, 60, 140);

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            // Gentle homing
            NPC target = FindTarget();
            if (target != null)
            {
                Vector2 toTarget = target.Center - Projectile.Center;
                toTarget.Normalize();
                float homingStrength = 0.04f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), homingStrength);
            }

            Projectile.rotation += 0.04f;

            // Intense trail
            for (int i = 0; i < 2; i++)
            {
                Vector2 trailVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f);
                Color trailColor = Color.Lerp(MoonSilver, MoonGold, Main.rand.NextFloat()) * 0.6f;
                var trail = new GenericGlowParticle(Projectile.Center, trailVel, trailColor, 0.35f, 22, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Core moon glow
            CustomParticles.GenericFlare(Projectile.Center, MoonSilver * 0.4f, 0.35f, 6);

            // ☁EMUSICAL NOTATION - Harvest moon carries lunar melody - VISIBLE SCALE 0.75f+
            if (Main.rand.NextBool(4))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-2.5f, -1f));
                Color noteColor = Color.Lerp(MoonSilver, MoonGold, Main.rand.NextFloat());
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.75f, 35);
                
                // Lunar sparkle
                var sparkle = new SparkleParticle(Projectile.Center, noteVel * 0.5f, MoonSilver * 0.4f, 0.22f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, MoonSilver.ToVector3() * 0.7f);
        }

        private NPC FindTarget()
        {
            float maxDist = 400f;
            NPC closest = null;
            float closestDist = float.MaxValue;

            foreach (NPC npc in Main.npc)
            {
                if (!npc.CanBeChasedBy()) continue;
                
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < maxDist && dist < closestDist)
                {
                    closest = npc;
                    closestDist = dist;
                }
            }

            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Heavy VFX
            CustomParticles.GenericFlare(target.Center, MoonSilver, 0.7f, 20);
            CustomParticles.GenericFlare(target.Center, MoonGold, 0.6f, 18);
            CustomParticles.GenericFlare(target.Center, MoonGold * 0.6f, 0.45f, 14);
            CustomParticles.GenericFlare(target.Center, TwilightPurple, 0.45f, 16);
            CustomParticles.GenericFlare(target.Center, TwilightPurple * 0.5f, 0.3f, 12);
            // Moon wisp burst
            for (int wisp = 0; wisp < 5; wisp++)
            {
                float wispAngle = MathHelper.TwoPi * wisp / 5f;
                Vector2 wispPos = target.Center + wispAngle.ToRotationVector2() * 16f;
                CustomParticles.GenericFlare(wispPos, MoonGold * 0.7f, 0.2f, 12);
            }

            for (int i = 0; i < 10; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(6f, 6f);
                Color sparkColor = Color.Lerp(MoonSilver, MoonGold, Main.rand.NextFloat()) * 0.6f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.28f, 20, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // ☁EMUSICAL IMPACT - Grand harvest moon chord
            ThemedParticles.MusicNoteBurst(target.Center, MoonGold, 10, 5f);
            ThemedParticles.MusicNoteRing(target.Center, MoonSilver, 45f, 6);
        }

        public override void OnKill(int timeLeft)
        {
            // Moon explosion
            CustomParticles.GenericFlare(Projectile.Center, MoonSilver, 0.65f, 22);
            CustomParticles.GenericFlare(Projectile.Center, MoonGold, 0.55f, 18);
            CustomParticles.GenericFlare(Projectile.Center, MoonGold * 0.6f, 0.4f, 14);
            // Moon wisp burst
            for (int wisp = 0; wisp < 5; wisp++)
            {
                float wispAngle = MathHelper.TwoPi * wisp / 5f;
                Vector2 wispPos = Projectile.Center + wispAngle.ToRotationVector2() * 14f;
                CustomParticles.GenericFlare(wispPos, MoonGold * 0.7f, 0.18f, 10);
            }

            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color burstColor = Color.Lerp(MoonSilver, MoonGold, (float)i / 12f) * 0.5f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.28f, 22, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // ☁EMUSICAL FINALE - Harvest moon finale crescendo
            ThemedParticles.MusicNoteBurst(Projectile.Center, MoonGold, 12, 5.5f);
            ThemedParticles.MusicNoteRing(Projectile.Center, MoonSilver, 55f, 8);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/GlowingHalo6").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.12f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Moon layers
            spriteBatch.Draw(texture, drawPos, null, TwilightPurple * 0.25f, Projectile.rotation, origin, 0.65f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, MoonGold * 0.35f, Projectile.rotation, origin, 0.45f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, MoonSilver * 0.5f, Projectile.rotation, origin, 0.3f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.6f, Projectile.rotation, origin, 0.15f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Homing Leaf Shard - Spawned on critical hits
    /// </summary>
    public class HomingLeafShard : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle11";
        
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnRed = new Color(178, 34, 34);
        private static readonly Color AutumnGold = new Color(218, 165, 32);

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            // Find and home to target
            NPC target = FindTarget();
            if (target != null)
            {
                Vector2 toTarget = target.Center - Projectile.Center;
                toTarget.Normalize();
                float homingStrength = 0.08f;
                float targetSpeed = 14f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * targetSpeed, homingStrength);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Leaf trail
            if (Main.rand.NextBool(3))
            {
                Vector2 trailVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                Color trailColor = Color.Lerp(AutumnOrange, AutumnRed, Main.rand.NextFloat()) * 0.5f;
                var trail = new GenericGlowParticle(Projectile.Center, trailVel, trailColor, 0.18f, 15, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // ☁EMUSICAL NOTATION - Leaf shard whispers autumn tune - VISIBLE SCALE 0.68f+
            if (Main.rand.NextBool(6))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-1.5f, -0.5f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, AutumnOrange, 0.68f, 25);
                
                // Autumn glyph accent
                CustomParticles.Glyph(Projectile.Center, AutumnOrange * 0.3f, 0.16f, -1);
            }

            Lighting.AddLight(Projectile.Center, AutumnOrange.ToVector3() * 0.3f);
        }

        private NPC FindTarget()
        {
            float maxDist = 350f;
            NPC closest = null;
            float closestDist = float.MaxValue;

            foreach (NPC npc in Main.npc)
            {
                if (!npc.CanBeChasedBy()) continue;
                
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < maxDist && dist < closestDist)
                {
                    closest = npc;
                    closestDist = dist;
                }
            }

            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            CustomParticles.GenericFlare(target.Center, AutumnGold, 0.35f, 12);
            
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(3f, 3f);
                Color sparkColor = Color.Lerp(AutumnOrange, AutumnRed, Main.rand.NextFloat()) * 0.5f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.18f, 14, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // ☁EMUSICAL IMPACT - Leaf shard note
            ThemedParticles.MusicNoteBurst(target.Center, AutumnGold, 4, 3f);
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, AutumnOrange, 0.3f, 12);

            for (int i = 0; i < 4; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(3f, 3f);
                Color burstColor = Color.Lerp(AutumnOrange, AutumnRed, Main.rand.NextFloat()) * 0.4f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.15f, 14, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // ☁EMUSICAL FINALE - Leaf shard final note
            ThemedParticles.MusicNoteBurst(Projectile.Center, AutumnOrange, 4, 3f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle11").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f + Projectile.whoAmI) * 0.12f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, drawPos, null, AutumnOrange * 0.4f, Projectile.rotation, origin, 0.25f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, AutumnGold * 0.5f, Projectile.rotation, origin, 0.15f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
