using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.Projectiles
{
    #region Fate1Sword Projectiles - Spectral Terrablade Beams

    /// <summary>
    /// Spectral Sword Beam - Terrablade-style homing beams
    /// Renders as copies of The Conductor's Last Constellation flying through the air
    /// </summary>
    public class SpectralSwordBeam : ModProjectile
    {
        // Use The Conductor's Last Constellation texture - the weapon itself as the projectile
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/TheConductorsLastConstellation";
        
        private float pulsePhase = 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            pulsePhase += 0.1f;

            // Homing behavior after initial flight
            if (Projectile.timeLeft < 160)
            {
                float homingRange = 500f;
                float homingStrength = 0.08f;
                float maxSpeed = 18f;

                NPC closestTarget = null;
                float closestDist = homingRange;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.CanBeChasedBy(Projectile))
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestTarget = npc;
                        }
                    }
                }

                if (closestTarget != null)
                {
                    Vector2 targetDir = (closestTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * maxSpeed, homingStrength);
                }
            }

            // Cosmic trail with music notes
            FateCosmicVFX.SpawnSpectralSwordTrail(Projectile.Center, Projectile.velocity, 0.8f);
            
            // Occasional music note in trail
            if (Main.rand.NextBool(6))
            {
                FateCosmicVFX.SpawnCosmicMusicNotes(Projectile.Center, 1, 8f, 0.25f);
            }

            // Cosmic cloud wisps
            if (Main.rand.NextBool(4))
            {
                FateCosmicVFX.SpawnCosmicCloudTrail(Projectile.Center, Projectile.velocity, 0.5f);
            }
            
            // Star sparkles orbiting the flying sword
            if (Main.rand.NextBool(5))
            {
                FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 1, 15f, 0.2f);
            }
            
            // Orbiting glyphs for celestial feel
            if (Main.rand.NextBool(10))
            {
                CustomParticles.Glyph(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), 
                    FateCosmicVFX.FatePurple, 0.2f, -1);
            }
            
            // ☁EMUSICAL NOTATION - Cosmic destiny trail
            if (Main.rand.NextBool(6))
            {
                Color noteColor = Color.Lerp(new Color(180, 50, 100), new Color(255, 60, 80), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.35f, 35);
            }

            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateWhite.ToVector3() * 0.6f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);

            // === ENHANCED COSMIC LIGHTNING WITH MULTI-LAYER BLOOM ===
            // Central flash with proper bloom stacking
            EnhancedParticles.BloomFlare(target.Center, FateCosmicVFX.FateWhite, 0.9f, 22, 4, 1.3f);
            EnhancedParticles.BloomFlare(target.Center, FateCosmicVFX.FateDarkPink, 0.7f, 20, 3, 1.1f);

            // Cosmic lightning strikes 3 times in quick succession
            for (int strike = 0; strike < 3; strike++)
            {
                Vector2 strikeOffset = Main.rand.NextVector2Circular(20f, 20f);
                FateCosmicVFX.SpawnCosmicLightningStrike(target.Center + strikeOffset, 0.8f);
            }

            // Enhanced impact burst with full bloom
            UnifiedVFXBloom.Fate.ImpactEnhanced(target.Center, 0.8f);
            
            // ☁EMUSICAL IMPACT - Fate's chord burst
            ThemedParticles.MusicNoteBurst(target.Center, new Color(180, 50, 100), 5, 3.5f);
            
            // === SEEKING CRYSTALS - Cosmic lightning burst ===
            if (Main.rand.NextBool(3))
            {
                SeekingCrystalHelper.SpawnFateCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.20f),
                    Projectile.knockBack,
                    Projectile.owner,
                    4
                );
            }
        }

        public override void OnKill(int timeLeft)
        {
            // === ENHANCED DEATH BURST WITH BLOOM ===
            EnhancedThemedParticles.FateBloomBurstEnhanced(Projectile.Center, 0.8f);
            FateCosmicVFX.SpawnGlyphBurst(Projectile.Center, 6, 4f, 0.3f);
            FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 8, 25f, 0.25f);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
            
            // ☁EMUSICAL FINALE - Cosmic symphony
            ThemedParticles.MusicNoteBurst(Projectile.Center, Color.White, 6, 4f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            
            // Pulsing effects for magical feel
            float pulse = 1f + (float)Math.Sin(pulsePhase) * 0.08f;
            float glowIntensity = 0.6f + (float)Math.Sin(pulsePhase * 0.7f) * 0.25f;

            // Draw trail with copies of the sword fading away
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = Color.Lerp(FateCosmicVFX.FateWhite, FateCosmicVFX.FateDarkPink, progress) * (1f - progress) * 0.5f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailScale = (1f - progress * 0.4f) * 0.8f;
                spriteBatch.Draw(tex, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            // === MULTI-LAYER BLOOM STACK FOR CELESTIAL GLOW ===
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Outermost purple cosmic nebula
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FatePurple * 0.15f * glowIntensity, Projectile.rotation, origin, 1.8f * pulse, SpriteEffects.None, 0f);
            
            // Outer bright red energy
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateBrightRed * 0.25f * glowIntensity, Projectile.rotation, origin, 1.4f * pulse, SpriteEffects.None, 0f);
            
            // Middle dark pink glow
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateDarkPink * 0.4f * glowIntensity, Projectile.rotation, origin, 1.15f * pulse, SpriteEffects.None, 0f);
            
            // Inner white celestial core
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateWhite * 0.35f * glowIntensity, Projectile.rotation, origin, 1.0f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Main sword - the Conductor's Last Constellation flying through the air
            spriteBatch.Draw(tex, drawPos, null, Color.White, Projectile.rotation, origin, 0.85f * pulse, SpriteEffects.None, 0f);

            return false;
        }
    }

    /// <summary>
    /// Glass-like visual distortion effect near the player on swing
    /// </summary>
    public class GlassDistortionEffect : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/Glyphs1";

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.Center = owner.Center;

            float progress = 1f - (float)Projectile.timeLeft / 20f;
            float radius = 40f + progress * 30f;
            float alpha = 1f - progress;

            // Glass shard effect - angular fragments
            int shardCount = 8;
            for (int i = 0; i < shardCount; i++)
            {
                float angle = MathHelper.TwoPi * i / shardCount + Main.GameUpdateCount * 0.03f;
                Vector2 shardPos = owner.Center + angle.ToRotationVector2() * radius;
                
                // Prismatic glass colors
                Color glassColor = Color.Lerp(FateCosmicVFX.FateWhite, FateCosmicVFX.FateCyan, (float)i / shardCount) * alpha * 0.4f;
                var shard = new GenericGlowParticle(shardPos, angle.ToRotationVector2() * 2f, glassColor, 0.15f, 5, true);
                MagnumParticleHandler.SpawnParticle(shard);
            }
            
            // ☁EMUSICAL NOTATION - Glass distortion shimmer
            if (Main.rand.NextBool(4))
            {
                Color noteColor = Color.Lerp(new Color(180, 50, 100), Color.White, Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -0.5f);
                ThemedParticles.MusicNote(owner.Center + Main.rand.NextVector2Circular(radius, radius), noteVel, noteColor * alpha, 0.3f, 25);
            }
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    #endregion

    #region Fate2Sword Projectiles - Cosmic Energy Ball

    /// <summary>
    /// Big cosmic energy ball that explodes into seekers
    /// </summary>
    public class CosmicEnergyBall : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/StarBurst1";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation += 0.1f;

            // Heavy cosmic cloud trail
            FateCosmicVFX.SpawnCosmicCloudTrail(Projectile.Center, Projectile.velocity, 1.2f);
            
            // Orbiting glyphs
            float orbitAngle = Main.GameUpdateCount * 0.1f;
            FateCosmicVFX.SpawnOrbitingGlyphs(Projectile.Center, 4, 30f, orbitAngle, 0.3f);
            
            // Star sparkles
            if (Main.rand.NextBool(3))
            {
                FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 2, 20f, 0.3f);
            }

            // Music notes occasionally
            if (Main.rand.NextBool(8))
            {
                FateCosmicVFX.SpawnCosmicMusicNotes(Projectile.Center, 1, 15f, 0.3f);
            }
            
            // ☁EMUSICAL NOTATION - Cosmic destiny trail
            if (Main.rand.NextBool(6))
            {
                Color noteColor = Color.Lerp(new Color(180, 50, 100), new Color(255, 60, 80), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.35f, 35);
            }

            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateDarkPink.ToVector3() * 1.2f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Explode into 5 seeker balls
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 velocity = angle.ToRotationVector2() * 8f;
                
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    velocity,
                    ModContent.ProjectileType<CosmicSeekerBall>(),
                    (int)(Projectile.damage * 0.6f),
                    Projectile.knockBack * 0.5f,
                    Projectile.owner
                );
            }

            // Big explosion
            FateCosmicVFX.SpawnCosmicExplosion(target.Center, 1.2f);
            SoundEngine.PlaySound(SoundID.Item14, target.Center);
            
            // ☁EMUSICAL IMPACT - Fate's chord burst
            ThemedParticles.MusicNoteBurst(target.Center, new Color(180, 50, 100), 6, 4f);
        }

        public override void OnKill(int timeLeft)
        {
            FateCosmicVFX.SpawnCosmicCloudBurst(Projectile.Center, 0.8f, 12);
            
            // ☁EMUSICAL FINALE - Cosmic symphony
            ThemedParticles.MusicNoteBurst(Projectile.Center, Color.White, 6, 4f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/StarBurst1").Value;
            Vector2 origin = tex.Size() / 2f;

            // Trail - scaled for player-sized projectile
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = FateCosmicVFX.GetCosmicGradient(progress) * (1f - progress) * 0.5f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(tex, trailPos, null, trailColor, Projectile.oldRot[i], origin, (1f - progress) * 0.5f, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f;
            
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.CosmicBlack * 0.6f, Projectile.rotation, origin, 0.6f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateDarkPink * 0.8f, Projectile.rotation, origin, 0.45f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateWhite * 0.7f, Projectile.rotation, origin, 0.25f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Smaller seeker ball that tracks enemies
    /// </summary>
    public class CosmicSeekerBall : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/Star";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation += 0.15f;

            // Aggressive homing
            float homingRange = 400f;
            NPC closestTarget = null;
            float closestDist = homingRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestTarget = npc;
                    }
                }
            }

            if (closestTarget != null)
            {
                Vector2 targetDir = (closestTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * 16f, 0.15f);
            }

            // Trail
            FateCosmicVFX.SpawnCosmicCloudTrail(Projectile.Center, Projectile.velocity, 0.5f);
            
            // ☁EMUSICAL NOTATION - Cosmic seeker trail
            if (Main.rand.NextBool(7))
            {
                Color noteColor = Color.Lerp(new Color(180, 50, 100), new Color(255, 60, 80), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.3f, 30);
            }
            
            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateBrightRed.ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);
            FateCosmicVFX.SpawnCosmicExplosion(target.Center, 0.5f);
            
            // ☁EMUSICAL IMPACT - Fate's chord burst
            ThemedParticles.MusicNoteBurst(target.Center, new Color(180, 50, 100), 4, 3f);
            
            // === SEEKING CRYSTALS - Cosmic seeker burst ===
            if (Main.rand.NextBool(4))
            {
                SeekingCrystalHelper.SpawnFateCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.15f),
                    Projectile.knockBack,
                    Projectile.owner,
                    3
                );
            }
        }

        public override void OnKill(int timeLeft)
        {
            FateCosmicVFX.SpawnGlyphBurst(Projectile.Center, 4, 3f, 0.2f);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
            
            // ☁EMUSICAL FINALE - Cosmic symphony
            ThemedParticles.MusicNoteBurst(Projectile.Center, Color.White, 5, 3.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/Star").Value;
            Vector2 origin = tex.Size() / 2f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateBrightRed * 0.8f, Projectile.rotation, origin, 0.8f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateWhite * 0.6f, Projectile.rotation, origin, 0.4f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    #endregion

    #region Fate3Sword Projectiles - Cosmic Beam

    /// <summary>
    /// Held sword that channels a cosmic beam - gets brighter as held
    /// Renders as Coda of Annihilation - the channeling blade
    /// </summary>
    public class CosmicBeamHeldSword : ModProjectile
    {
        // Use Coda of Annihilation texture
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/CodaOfAnnihilation";

        private float chargeIntensity = 0f;
        private const float MaxCharge = 2f;
        private float pulsePhase = 0f;

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!owner.active || owner.dead || !owner.channel)
            {
                Projectile.Kill();
                return;
            }

            // Increase intensity while held
            chargeIntensity = Math.Min(chargeIntensity + 0.015f, MaxCharge);
            pulsePhase += 0.08f + chargeIntensity * 0.03f; // Pulse faster as charge builds

            Vector2 aimDir = (Main.MouseWorld - owner.Center).SafeNormalize(Vector2.UnitX);
            Projectile.Center = owner.Center + aimDir * 40f;
            Projectile.rotation = aimDir.ToRotation() + MathHelper.PiOver4;
            owner.ChangeDir(aimDir.X > 0 ? 1 : -1);
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
            owner.itemRotation = aimDir.ToRotation();

            // Fire beam from sword tip
            Vector2 beamStart = Projectile.Center + aimDir * 30f;
            Vector2 beamEnd = beamStart + aimDir * (400f + chargeIntensity * 200f);

            // Check for enemies in beam path and deal damage
            float beamWidth = 20f + chargeIntensity * 15f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;

                float dist = DistanceToLine(npc.Center, beamStart, beamEnd);
                if (dist < beamWidth + npc.width / 2f)
                {
                    // Apply damage periodically
                    if (Main.GameUpdateCount % 6 == 0)
                    {
                        npc.SimpleStrikeNPC((int)(Projectile.damage * (0.5f + chargeIntensity * 0.3f)), owner.direction, false, 0f);
                        npc.AddBuff(ModContent.BuffType<DestinyCollapse>(), 60);

                        // Lightning strikes on hit
                        if (Main.rand.NextBool(3))
                        {
                            FateCosmicVFX.SpawnCosmicLightningStrike(npc.Center, 0.6f + chargeIntensity * 0.3f);
                        }
                    }
                }
            }

            // Beam VFX - increases with charge
            float beamIntensity = 0.5f + chargeIntensity * 0.5f;
            FateCosmicVFX.SpawnCosmicBeamParticles(beamStart, beamEnd, beamIntensity);

            // More particles as beam gets stronger
            if (chargeIntensity > 1f && Main.rand.NextBool(3))
            {
                FateCosmicVFX.SpawnCosmicMusicNotes(beamStart, 1, 20f, 0.3f);
            }
            
            // === ENHANCED CHANNELING VFX ===
            // Orbiting glyphs around the held sword
            if (Main.rand.NextBool(4))
            {
                float glyphAngle = pulsePhase + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 glyphPos = Projectile.Center + glyphAngle.ToRotationVector2() * (20f + chargeIntensity * 10f);
                CustomParticles.Glyph(glyphPos, FateCosmicVFX.FatePurple, 0.2f + chargeIntensity * 0.1f, -1);
            }
            
            // Star sparkles around the blade
            if (Main.rand.NextBool(5) && chargeIntensity > 0.5f)
            {
                FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 1, 25f, 0.2f + chargeIntensity * 0.1f);
            }
            
            // Cosmic cloud at tip
            if (Main.rand.NextBool(4))
            {
                FateCosmicVFX.SpawnCosmicCloudTrail(beamStart, aimDir * 2f, 0.3f + chargeIntensity * 0.2f);
            }
            
            // ☁EMUSICAL NOTATION - Cosmic beam channeling
            if (Main.rand.NextBool(5))
            {
                Color noteColor = Color.Lerp(new Color(180, 50, 100), Color.White, chargeIntensity / MaxCharge);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1.2f);
                ThemedParticles.MusicNote(beamStart + Main.rand.NextVector2Circular(20f, 20f), noteVel, noteColor, 0.3f + chargeIntensity * 0.1f, 35);
            }

            // Glow at sword tip
            float tipGlow = chargeIntensity / MaxCharge;
            Lighting.AddLight(beamStart, FateCosmicVFX.FateWhite.ToVector3() * tipGlow * 1.5f);
            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateDarkPink.ToVector3() * tipGlow * 0.8f);
        }

        private float DistanceToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 line = lineEnd - lineStart;
            float len = line.Length();
            if (len == 0) return Vector2.Distance(point, lineStart);
            
            float t = Math.Max(0, Math.Min(1, Vector2.Dot(point - lineStart, line) / (len * len)));
            Vector2 projection = lineStart + t * line;
            return Vector2.Distance(point, projection);
        }

        public override bool? CanDamage() => false; // Damage handled manually

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float glowIntensity = chargeIntensity / MaxCharge;
            float pulse = 1f + (float)Math.Sin(pulsePhase) * 0.08f * glowIntensity;

            // === MULTI-LAYER BLOOM STACK - INTENSIFIES WITH CHARGE ===
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer cosmic nebula - grows with charge
            if (glowIntensity > 0.1f)
            {
                spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FatePurple * 0.12f * glowIntensity, Projectile.rotation, origin, (1.6f + glowIntensity * 0.4f) * pulse, SpriteEffects.None, 0f);
            }
            
            // Middle bright red energy - grows with charge
            if (glowIntensity > 0.2f)
            {
                spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateBrightRed * 0.2f * glowIntensity, Projectile.rotation, origin, (1.35f + glowIntensity * 0.3f) * pulse, SpriteEffects.None, 0f);
            }
            
            // Inner dark pink glow
            if (glowIntensity > 0.15f)
            {
                spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateDarkPink * 0.35f * glowIntensity, Projectile.rotation, origin, (1.15f + glowIntensity * 0.15f) * pulse, SpriteEffects.None, 0f);
            }
            
            // Innermost white-hot celestial core
            if (glowIntensity > 0.3f)
            {
                spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateWhite * 0.25f * glowIntensity, Projectile.rotation, origin, 1.05f * pulse, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Main Coda of Annihilation blade
            spriteBatch.Draw(tex, drawPos, null, Color.White, Projectile.rotation, origin, pulse, SpriteEffects.None, 0f);

            return false;
        }
    }

    #endregion

    #region Fate4Sword Projectiles - Rotating Spectral Blades

    /// <summary>
    /// Spectral blade that orbits the player and shoots prismatic beams
    /// Renders as Coda of Annihilation - the blade containing infinite celestial patterns
    /// </summary>
    public class OrbitingSpectralBlade : ModProjectile
    {
        // Use Coda of Annihilation texture - the celestial blade
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/CodaOfAnnihilation";

        private int bladeIndex = 0;
        private int beamCooldown = 0;
        private float beamSize = 1f;
        private NPC lockedTarget = null;
        private int lockTime = 0;
        private float pulsePhase = 0f; // For pulsing glow effect

        public ref float OrbitAngle => ref Projectile.ai[0];
        public ref float BladeCount => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600; // 10 seconds
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            // Orbit around player
            float orbitRadius = 80f;
            float orbitSpeed = 0.03f;
            OrbitAngle += orbitSpeed;

            // Calculate this blade's offset based on total blade count
            float angleOffset = MathHelper.TwoPi * bladeIndex / Math.Max(1, BladeCount);
            Vector2 orbitPos = owner.Center + (OrbitAngle + angleOffset).ToRotationVector2() * orbitRadius;
            Projectile.Center = orbitPos;

            // Find nearest enemy and rotate blade toward it
            NPC target = FindNearestEnemy(600f);
            if (target != null)
            {
                // Track target
                if (target == lockedTarget)
                {
                    lockTime++;
                }
                else
                {
                    lockedTarget = target;
                    lockTime = 0;
                }

                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                float targetRotation = toTarget.ToRotation() + MathHelper.PiOver4;
                Projectile.rotation = MathHelper.Lerp(Projectile.rotation, targetRotation, 0.15f);

                // Shoot prismatic beam
                beamCooldown--;
                if (beamCooldown <= 0)
                {
                    beamCooldown = 20;
                    ShootPrismaticBeam(target);
                }

                // Beam gets larger and more colorful as it keeps firing at same target
                if (lockTime > 30)
                {
                    beamSize = Math.Min(beamSize + 0.02f, 3f);
                }
            }
            else
            {
                // Idle rotation
                Projectile.rotation += 0.02f;
                lockedTarget = null;
                lockTime = 0;
                beamSize = Math.Max(1f, beamSize - 0.01f);
            }

            // VFX - cosmic energy emanating
            if (Main.rand.NextBool(4))
            {
                Color energyColor = Main.rand.NextBool() ? FateCosmicVFX.FateDarkPink : FateCosmicVFX.FateBrightRed;
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                var energy = new GenericGlowParticle(Projectile.Center + offset, offset.SafeNormalize(Vector2.Zero) * 1f, energyColor * 0.6f, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(energy);
            }
            
            // Orbiting star sparkles around the spectral blade
            if (Main.rand.NextBool(6))
            {
                FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 1, 25f, 0.2f);
            }
            
            // Occasional glyph orbiting
            if (Main.rand.NextBool(12))
            {
                CustomParticles.Glyph(Projectile.Center + Main.rand.NextVector2Circular(18f, 18f), 
                    FateCosmicVFX.FatePurple, 0.25f, -1);
            }
            
            // ☁EMUSICAL NOTATION - Cosmic destiny trail
            if (Main.rand.NextBool(8))
            {
                Color noteColor = Color.Lerp(new Color(180, 50, 100), new Color(255, 60, 80), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.32f, 32);
            }
            
            // Update pulse phase for drawing
            pulsePhase += 0.08f;

            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateDarkPink.ToVector3() * 0.5f);
        }

        private void ShootPrismaticBeam(NPC target)
        {
            Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                direction * 20f,
                ModContent.ProjectileType<PrismaticBeam>(),
                (int)(Projectile.damage * 0.5f),
                0f,
                Projectile.owner,
                beamSize,
                lockTime / 60f // Color progression
            );

            // Firing VFX
            FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 3, 15f, 0.2f);
        }

        private NPC FindNearestEnemy(float range)
        {
            NPC closest = null;
            float closestDist = range;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }

            return closest;
        }

        public void SetBladeIndex(int index, int total)
        {
            bladeIndex = index;
            BladeCount = total;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Pulsing scale and glow intensity
            float pulse = 1f + (float)Math.Sin(pulsePhase) * 0.1f;
            float glowIntensity = 0.5f + (float)Math.Sin(pulsePhase * 0.7f) * 0.3f;

            // === MULTI-LAYER BLOOM STACK FOR CELESTIAL GLOW ===
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outermost cosmic nebula glow - purple/crimson
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FatePurple * 0.15f * glowIntensity, Projectile.rotation, origin, 2.0f * pulse, SpriteEffects.None, 0f);
            
            // Outer bright red energy corona
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateBrightRed * 0.25f * glowIntensity, Projectile.rotation, origin, 1.6f * pulse, SpriteEffects.None, 0f);
            
            // Middle dark pink energy field
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateDarkPink * 0.4f * glowIntensity, Projectile.rotation, origin, 1.3f * pulse, SpriteEffects.None, 0f);
            
            // Inner white-hot celestial core
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateWhite * 0.3f * glowIntensity, Projectile.rotation, origin, 1.1f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Main Coda of Annihilation blade - slightly tinted with celestial white
            spriteBatch.Draw(tex, drawPos, null, Color.White * 0.95f, Projectile.rotation, origin, pulse, SpriteEffects.None, 0f);

            return false;
        }
    }

    /// <summary>
    /// Prismatic beam that grows larger and more colorful
    /// </summary>
    public class PrismaticBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/TwilightSparkle";

        public ref float BeamSize => ref Projectile.ai[0];
        public ref float ColorProgress => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Scale hitbox with beam size
            int size = (int)(10 * BeamSize);
            Projectile.width = Projectile.height = size;

            // Rainbow cycling based on ColorProgress
            float hue = (ColorProgress + Main.GameUpdateCount * 0.02f) % 1f;
            Color prismaticColor = Main.hslToRgb(hue, 0.9f, 0.7f);

            // Trail particles
            var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f, prismaticColor * 0.7f, 0.15f * BeamSize, 12, true);
            MagnumParticleHandler.SpawnParticle(trail);

            if (Main.rand.NextBool(4))
            {
                FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 1, 10f * BeamSize, 0.2f);
            }
            
            // ☁EMUSICAL NOTATION - Prismatic trail
            if (Main.rand.NextBool(6))
            {
                Color noteColor = Color.Lerp(prismaticColor, Color.White, Main.rand.NextFloat(0.3f));
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.28f * BeamSize, 28);
            }

            Lighting.AddLight(Projectile.Center, prismaticColor.ToVector3() * BeamSize * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);
            
            // ☁EMUSICAL IMPACT - Fate's chord burst
            ThemedParticles.MusicNoteBurst(target.Center, new Color(180, 50, 100), 4, 3f);
            
            // === SEEKING CRYSTALS - Cosmic beam impact ===
            if (Main.rand.NextBool(4))
            {
                SeekingCrystalHelper.SpawnFateCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.15f),
                    Projectile.knockBack,
                    Projectile.owner,
                    3
                );
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/TwilightSparkle").Value;
            Vector2 origin = tex.Size() / 2f;

            float hue = (ColorProgress + Main.GameUpdateCount * 0.02f) % 1f;
            Color prismaticColor = Main.hslToRgb(hue, 0.9f, 0.7f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(tex, drawPos, null, prismaticColor * 0.8f, Projectile.rotation, origin, 0.5f * BeamSize, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateWhite * 0.6f, Projectile.rotation, origin, 0.25f * BeamSize, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    #endregion

    #region Fate5Sword Projectiles - Cosmic Music Notes

    /// <summary>
    /// Music note that floats around player before seeking enemies
    /// </summary>
    public class CosmicMusicNoteProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/MusicNote";

        private int phase = 0; // 0 = floating, 1 = seeking
        private int floatTimer = 0;
        private const int FloatDuration = 60;
        private NPC seekTarget = null;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (phase == 0)
            {
                // Floating phase - orbit around player with some chaos
                floatTimer++;
                float orbitRadius = 60f + (float)Math.Sin(floatTimer * 0.1f) * 20f;
                float orbitAngle = Projectile.ai[0] + floatTimer * 0.05f;
                
                Vector2 targetPos = owner.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                Projectile.velocity = (targetPos - Projectile.Center) * 0.15f;

                // Floating particles
                if (Main.rand.NextBool(6))
                {
                    Color noteColor = FateCosmicVFX.GetCosmicGradient(Main.rand.NextFloat());
                    var glow = new GenericGlowParticle(Projectile.Center, Main.rand.NextVector2Circular(1f, 1f), noteColor * 0.5f, 0.15f, 15, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }

                // Transition to seeking after float duration
                if (floatTimer >= FloatDuration)
                {
                    phase = 1;
                    seekTarget = FindNearestEnemy(800f);
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.5f, Volume = 0.5f }, Projectile.Center);
                }
            }
            else
            {
                // Seeking phase - aggressive homing
                if (seekTarget != null && seekTarget.active)
                {
                    Vector2 targetDir = (seekTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * 20f, 0.2f);
                }
                else
                {
                    seekTarget = FindNearestEnemy(800f);
                }

                // Cosmic flame trail
                Color flameColor = FateCosmicVFX.GetCosmicGradient(Main.rand.NextFloat());
                var flame = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f), flameColor, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(flame);

                // Electricity sparks
                if (Main.rand.NextBool(6))
                {
                    FateCosmicVFX.SpawnCosmicElectricity(Projectile.Center, 1, 20f, 0.5f);
                }
            }

            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateDarkPink.ToVector3() * 0.4f);
        }

        private NPC FindNearestEnemy(float range)
        {
            NPC closest = null;
            float closestDist = range;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }

            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 240);

            // Cosmic flames and electricity on impact
            FateCosmicVFX.SpawnCosmicFlames(target.Center, 12, 30f, 0.8f);
            FateCosmicVFX.SpawnCosmicElectricity(target.Center, 4, 40f, 0.8f);
            FateCosmicVFX.SpawnCosmicExplosion(target.Center, 0.6f);
            
            // === SEEKING CRYSTALS - Cosmic music note burst ===
            if (Main.rand.NextBool(3))
            {
                SeekingCrystalHelper.SpawnFateCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.20f),
                    Projectile.knockBack,
                    Projectile.owner,
                    5
                );
            }

            SoundEngine.PlaySound(SoundID.Item14, target.Center);
        }

        public override void OnKill(int timeLeft)
        {
            FateCosmicVFX.SpawnMusicNoteExplosion(Projectile.Center, 6, 4f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/MusicNote").Value;
            Vector2 origin = tex.Size() / 2f;

            float colorCycle = Main.GameUpdateCount * 0.03f + Projectile.ai[0];
            Color noteColor = FateCosmicVFX.GetCosmicGradient((colorCycle % 1f));

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(tex, drawPos, null, noteColor * 0.8f, Projectile.rotation, origin, 0.5f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateWhite * 0.5f, Projectile.rotation, origin, 0.3f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    #endregion
}
