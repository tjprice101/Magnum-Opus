using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Nachtmusik.Debuffs;

namespace MagnumOpus.Content.Nachtmusik.Projectiles
{
    #region Melee Projectiles
    
    /// <summary>
    /// Nocturnal Executioner's massive spectral blade projectile.
    /// Homes toward enemies and explodes on contact.
    /// </summary>
    public class NocturnalBladeProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/Glyphs10";
        
        private float orbitAngle;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            orbitAngle += 0.12f;
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Homing toward enemies
            NPC target = FindClosestEnemy(600f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 18f, 0.06f);
            }
            
            // Trail VFX
            if (Projectile.timeLeft % 2 == 0)
            {
                NachtmusikCosmicVFX.SpawnCelestialCloudTrail(Projectile.Center, Projectile.velocity, 0.6f);
            }
            
            // Orbiting star particles
            if (Projectile.timeLeft % 6 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float sparkAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * 16f;
                    CustomParticles.GenericFlare(sparkPos, NachtmusikCosmicVFX.Gold, 0.25f, 10);
                }
            }
            
            // Sparkle dust trail
            if (Main.rand.NextBool(2))
            {
                Color trailColor = NachtmusikCosmicVFX.GetCelestialGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    trailColor * 0.7f, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
                
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, -Projectile.velocity * 0.3f, 0, default, 0.9f);
                dust.noGravity = true;
            }
            
            // 笘・MUSICAL NOTATION - Nachtmusik celestial melody (VISIBLE SCALE 0.75f+)
            if (Main.rand.NextBool(6))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, NachtmusikCosmicVFX.Gold, 0.75f, 35);
            }
            
            // 笘・SPARKLE ACCENT - Celestial shimmer
            if (Main.rand.NextBool(4))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), -Projectile.velocity * 0.1f, NachtmusikCosmicVFX.StarWhite, 0.35f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.Violet.ToVector3() * 0.6f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Celestial Harmony debuff
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 480);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 2);
            
            // Impact VFX with star burst
            NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 1.2f);
            NachtmusikCosmicVFX.SpawnStarBurstImpact(target.Center, 0.8f, 2);
            
            // 笘・MUSICAL IMPACT - Celestial chord burst
            ThemedParticles.MusicNoteBurst(target.Center, NachtmusikCosmicVFX.Gold, 5, 3.5f);
            
            // === SEEKING CRYSTALS - Celestial burst ===
            if (Main.rand.NextBool(3))
            {
                SeekingCrystalHelper.SpawnNachtmusikCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.18f),
                    Projectile.knockBack,
                    Projectile.owner,
                    4
                );
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // Death explosion with shattered starlight
            NachtmusikCosmicVFX.SpawnCelestialProjectileDeath(Projectile.Center, 0.9f);
            SoundEngine.PlaySound(SoundID.Item62 with { Pitch = 0.3f, Volume = 0.7f }, Projectile.Center);
            
            // 笘・MUSICAL FINALE - Starlight symphony
            ThemedParticles.MusicNoteBurst(Projectile.Center, NachtmusikCosmicVFX.Violet, 6, 4f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/Glyphs10").Value;
            Vector2 origin = tex.Size() / 2f;
            
            // Draw trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color trailColor = NachtmusikCosmicVFX.GetCelestialGradient(progress) * (1f - progress) * 0.6f;
                float scale = 0.5f * (1f - progress * 0.6f);
                sb.Draw(tex, drawPos, null, trailColor, Projectile.oldRot[i], origin, scale, SpriteEffects.None, 0f);
            }
            
            // Draw core
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f + 0.9f;
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.DeepPurple * 0.4f, 0f, origin, 0.7f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.Violet * 0.6f, 0f, origin, 0.5f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.Gold * 0.7f, 0f, origin, 0.35f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.StarWhite * 0.9f, 0f, origin, 0.2f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
        
        private NPC FindClosestEnemy(float range)
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
    }
    
    /// <summary>
    /// Midnight's Crescendo's crescendo wave projectile.
    /// Expands as it travels, dealing more damage at larger sizes.
    /// </summary>
    public class CrescendoWaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SwordArc7";
        
        private float growthFactor = 1f;
        
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }
        
        public override void AI()
        {
            // Grow over time
            growthFactor = 1f + (120 - Projectile.timeLeft) / 60f; // Grows up to 3x
            Projectile.scale = growthFactor;
            
            // Slow down as it grows
            Projectile.velocity *= 0.985f;
            
            // Rotation
            Projectile.rotation += 0.08f;
            
            // Trail VFX
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f * growthFactor, 20f * growthFactor);
                Color trailColor = NachtmusikCosmicVFX.GetCelestialGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(Projectile.Center + offset, Main.rand.NextVector2Circular(2f, 2f),
                    trailColor * 0.5f, 0.25f * growthFactor, 15, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Music notes spinning around (VISIBLE SCALE 0.75f+)
            if (Projectile.timeLeft % 8 == 0)
            {
                float noteAngle = Projectile.rotation * 2f;
                Vector2 notePos = Projectile.Center + noteAngle.ToRotationVector2() * 25f * growthFactor;
                ThemedParticles.MusicNote(notePos, noteAngle.ToRotationVector2() * 2f, NachtmusikCosmicVFX.Gold, 0.75f, 20);
                
                // 笘・SPARKLE ACCENT - Crescendo shimmer
                var sparkle = new SparkleParticle(notePos, Main.rand.NextVector2Circular(2f, 2f), NachtmusikCosmicVFX.Violet, 0.3f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.Violet.ToVector3() * 0.8f * growthFactor);
        }
        
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // More damage at larger sizes
            modifiers.FinalDamage *= growthFactor;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 360);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);
            
            NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 0.8f * growthFactor);
            NachtmusikCosmicVFX.SpawnStarBurstImpact(target.Center, 0.5f * growthFactor, 2);
            
            // 笘・MUSICAL IMPACT - Crescendo chord burst
            ThemedParticles.MusicNoteBurst(target.Center, NachtmusikCosmicVFX.Gold, 4, 3f);
            
            // === SEEKING CRYSTALS - Crescendo burst ===
            if (Main.rand.NextBool(4))
            {
                SeekingCrystalHelper.SpawnNachtmusikCrystals(
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
            NachtmusikCosmicVFX.SpawnCelestialProjectileDeath(Projectile.Center, growthFactor * 0.8f);
            NachtmusikCosmicVFX.SpawnMusicNoteBurst(Projectile.Center, 8, 5f * growthFactor);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SwordArc7").Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.15f + 0.85f;
            float scale = growthFactor * pulse;
            
            // === RADIANT TRANSPARENT MULTI-LAYER BLOOM SLASHES ===
            // Using { A = 0 } pattern for proper additive blending - much more transparent and ethereal
            
            // Outermost ethereal glow - very transparent, large, soft
            Color outerGlow = NachtmusikCosmicVFX.DeepPurple with { A = 0 } * 0.18f;
            sb.Draw(tex, drawPos, null, outerGlow, Projectile.rotation, origin, scale * 1.5f, SpriteEffects.None, 0f);
            
            // Second outer layer - purple bloom
            Color purpleBloom = NachtmusikCosmicVFX.Violet with { A = 0 } * 0.22f;
            sb.Draw(tex, drawPos, null, purpleBloom, Projectile.rotation * 0.95f, origin, scale * 1.3f, SpriteEffects.None, 0f);
            
            // Middle glow layer - vibrant violet
            Color midGlow = NachtmusikCosmicVFX.Violet with { A = 0 } * 0.28f;
            sb.Draw(tex, drawPos, null, midGlow, Projectile.rotation * 0.85f, origin, scale * 1.1f, SpriteEffects.None, 0f);
            
            // Inner gold layer - warm celestial glow
            Color goldGlow = NachtmusikCosmicVFX.Gold with { A = 0 } * 0.35f;
            sb.Draw(tex, drawPos, null, goldGlow, Projectile.rotation * 0.7f, origin, scale * 0.85f, SpriteEffects.None, 0f);
            
            // Bright core layer - intense star white
            Color coreGlow = NachtmusikCosmicVFX.StarWhite with { A = 0 } * 0.45f;
            sb.Draw(tex, drawPos, null, coreGlow, Projectile.rotation * 0.5f, origin, scale * 0.6f, SpriteEffects.None, 0f);
            
            // White-hot center - maximum brightness
            Color innerWhite = Color.White with { A = 0 } * 0.55f;
            sb.Draw(tex, drawPos, null, innerWhite, Projectile.rotation * 0.3f, origin, scale * 0.35f, SpriteEffects.None, 0f);
            
            // === EXTRA SPARKLE LAYER - Star points along the arc ===
            if (Main.rand.NextBool(3))
            {
                for (int i = 0; i < 3; i++)
                {
                    float sparkleAngle = Projectile.rotation + MathHelper.TwoPi * i / 3f;
                    Vector2 sparkleOffset = sparkleAngle.ToRotationVector2() * 15f * scale;
                    var sparkle = new SparkleParticle(Projectile.Center + sparkleOffset + Main.rand.NextVector2Circular(8f, 8f),
                        Main.rand.NextVector2Circular(1f, 1f), NachtmusikCosmicVFX.StarWhite * 0.8f, 0.25f, 12);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            return false;
        }
    }
    
    #endregion
    
    #region Ranged Projectiles
    
    /// <summary>
    /// Constellation Piercer's star bolt that chains to nearby enemies.
    /// </summary>
    public class ConstellationBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/StarBurst1";
        
        private int chainCount = 0;
        private const int MaxChains = 4;
        private List<int> hitEnemies = new List<int>();
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.extraUpdates = 1;
            Projectile.arrow = true;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // Light trail
            NachtmusikCosmicVFX.SpawnRadiantBeamTrail(Projectile.Center, Projectile.velocity, 0.5f);
            
            // Star sparkles
            if (Main.rand.NextBool(3))
            {
                Dust star = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, -Projectile.velocity * 0.2f, 0, default, 0.8f);
                star.noGravity = true;
            }
            
            // 笘・MUSICAL NOTATION - Constellation melody (VISIBLE SCALE 0.7f+)
            if (Main.rand.NextBool(6))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, NachtmusikCosmicVFX.StarWhite, 0.7f, 30);
            }
            
            // 笘・SPARKLE ACCENT - Star twinkle
            if (Main.rand.NextBool(5))
            {
                var sparkle = new SparkleParticle(Projectile.Center, -Projectile.velocity * 0.05f, NachtmusikCosmicVFX.Gold, 0.25f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.Gold.ToVector3() * 0.5f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 300);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);
            
            NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 0.7f);
            
            // 笘・MUSICAL IMPACT - Constellation chord
            ThemedParticles.MusicNoteBurst(target.Center, NachtmusikCosmicVFX.Gold, 4, 3f);
            
            hitEnemies.Add(target.whoAmI);
            
            // Chain to next target
            if (chainCount < MaxChains)
            {
                NPC nextTarget = FindNextChainTarget(target.Center, 300f);
                if (nextTarget != null)
                {
                    chainCount++;
                    Vector2 toNext = (nextTarget.Center - target.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = toNext * Projectile.velocity.Length() * 0.95f;
                    Projectile.Center = target.Center;
                    
                    // Chain lightning VFX
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 linePos = Vector2.Lerp(target.Center, target.Center + toNext * 50f, i / 6f);
                        var line = new GenericGlowParticle(linePos, Vector2.Zero, NachtmusikCosmicVFX.Gold, 0.2f, 10, true);
                        MagnumParticleHandler.SpawnParticle(line);
                    }
                }
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            NachtmusikCosmicVFX.SpawnCelestialImpact(Projectile.Center, 0.6f);
            
            // 笘・MUSICAL FINALE - Starlight finale
            ThemedParticles.MusicNoteBurst(Projectile.Center, NachtmusikCosmicVFX.Violet, 5, 3.5f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/StarBurst1").Value;
            Vector2 origin = tex.Size() / 2f;
            
            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color trailColor = Color.Lerp(NachtmusikCosmicVFX.Gold, NachtmusikCosmicVFX.StarWhite, progress) * (1f - progress) * 0.5f;
                sb.Draw(tex, drawPos, null, trailColor, 0f, origin, 0.3f * (1f - progress * 0.5f), SpriteEffects.None, 0f);
            }
            
            // Core
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.Gold * 0.7f, 0f, origin, 0.4f, SpriteEffects.None, 0f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.StarWhite * 0.9f, 0f, origin, 0.25f, SpriteEffects.None, 0f);
            
            return false;
        }
        
        private NPC FindNextChainTarget(Vector2 from, float range)
        {
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (hitEnemies.Contains(i)) continue;
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(from, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
    }
    
    /// <summary>
    /// Nebula's Whisper's ethereal arrow - UNIQUE NEBULA CLOUD AESTHETIC
    /// A swirling vortex of cosmic gas with orbiting star motes that leaves 
    /// a billowing nebula trail and rains down starfall on impact.
    /// </summary>
    public class NebulaArrowProjectile : ModProjectile
    {
        // Use StarBurst texture for unique nebula core look
        public override string Texture => "MagnumOpus/Assets/Particles/StarBurst2";
        
        private int splitCount = 0;
        private float nebulaRotation = 0f;
        private float[] starMoteAngles = new float[5];
        private float[] starMoteDistances = new float[5];
        
        // Nebula color palette - distinct pinks and purples
        private static readonly Color NebulaCore = new Color(255, 120, 200);
        private static readonly Color NebulaOuter = new Color(180, 80, 220);
        private static readonly Color NebulaGas = new Color(120, 60, 180);
        private static readonly Color StarMote = new Color(255, 255, 220);
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 18;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.arrow = true;
            Projectile.extraUpdates = 1;
            
            // Initialize orbiting star motes with varied distances
            for (int i = 0; i < starMoteAngles.Length; i++)
            {
                starMoteAngles[i] = MathHelper.TwoPi * i / starMoteAngles.Length;
                starMoteDistances[i] = 12f + Main.rand.NextFloat(6f);
            }
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            nebulaRotation += 0.08f;
            
            // Update orbiting star motes
            for (int i = 0; i < starMoteAngles.Length; i++)
            {
                starMoteAngles[i] += 0.12f + i * 0.02f;
                starMoteDistances[i] += (float)Math.Sin(Main.GameUpdateCount * 0.1f + i) * 0.3f;
            }
            
            // BILLOWING NEBULA GAS TRAIL - multiple layers of cosmic clouds
            if (Main.rand.NextBool(2))
            {
                for (int layer = 0; layer < 3; layer++)
                {
                    float layerOffset = layer * 0.15f;
                    Color gasColor = Color.Lerp(NebulaCore, NebulaGas, layerOffset + Main.rand.NextFloat(0.3f));
                    Vector2 gasOffset = Main.rand.NextVector2Circular(6f + layer * 3f, 6f + layer * 3f);
                    Vector2 gasVel = -Projectile.velocity * (0.06f + layer * 0.02f) + Main.rand.NextVector2Circular(0.8f, 0.8f);
                    var cloud = new GenericGlowParticle(Projectile.Center + gasOffset, gasVel, gasColor * (0.4f - layer * 0.1f), 
                        0.28f - layer * 0.05f, 22 + layer * 4, true);
                    MagnumParticleHandler.SpawnParticle(cloud);
                }
            }
            
            // STAR MOTE SPARKLE TRAIL - tiny stars left behind from orbiting motes
            if (Projectile.timeLeft % 3 == 0)
            {
                int moteIndex = Projectile.timeLeft % starMoteAngles.Length;
                Vector2 motePos = Projectile.Center + starMoteAngles[moteIndex].ToRotationVector2() * starMoteDistances[moteIndex];
                var starTrail = new SparkleParticle(motePos, -Projectile.velocity * 0.05f, StarMote * 0.7f, 0.18f, 15);
                MagnumParticleHandler.SpawnParticle(starTrail);
            }
            
            // MUSIC NOTES - Nebula whisper melody floating upward
            if (Main.rand.NextBool(6))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), Main.rand.NextFloat(-1.2f, -0.5f));
                Color noteColor = Color.Lerp(NebulaCore, StarMote, Main.rand.NextFloat(0.3f));
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), noteVel, noteColor, 0.72f, 35);
            }
            
            // PRISMATIC SPARKLE accents
            if (Main.rand.NextBool(5))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), 
                    Vector2.Zero, Color.Lerp(NebulaCore, StarMote, Main.rand.NextFloat()), 0.32f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Dynamic lighting that pulses
            float lightPulse = 0.4f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
            Lighting.AddLight(Projectile.Center, NebulaCore.ToVector3() * lightPulse);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 300);
            
            // STARFALL RAIN - Split creates falling stars that impact ground
            if (splitCount == 0 && Projectile.ai[0] == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    float angle = MathHelper.ToRadians(-150f + i * 30f); // Arc above
                    Vector2 splitVel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 12f);
                    int split = Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center + new Vector2(0, -30f), splitVel,
                        ModContent.ProjectileType<NebulaStarfallProjectile>(), Projectile.damage / 2, Projectile.knockBack / 2, Projectile.owner);
                }
            }
            
            // NEBULA BURST VFX - Expanding gas cloud explosion
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color burstColor = Color.Lerp(NebulaCore, NebulaGas, Main.rand.NextFloat());
                var burst = new GenericGlowParticle(target.Center, burstVel, burstColor * 0.6f, 0.35f, 25, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
            
            // Star sparkle explosion
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparklePos = target.Center + Main.rand.NextVector2Circular(25f, 25f);
                var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(2f, 2f), StarMote, 0.4f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Music chord burst
            ThemedParticles.MusicNoteBurst(target.Center, NebulaCore, 6, 4f);
            
            // Cascading halos
            for (int i = 0; i < 4; i++)
            {
                Color haloColor = Color.Lerp(NebulaCore, NebulaOuter, i / 4f);
                CustomParticles.HaloRing(target.Center, haloColor * (0.6f - i * 0.1f), 0.3f + i * 0.15f, 15 + i * 3);
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // Nebula dissipation - gas clouds scatter
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 gasVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color gasColor = Color.Lerp(NebulaCore, NebulaGas, Main.rand.NextFloat());
                var gas = new GenericGlowParticle(Projectile.Center, gasVel, gasColor * 0.5f, 0.3f, 28, true);
                MagnumParticleHandler.SpawnParticle(gas);
            }
            
            ThemedParticles.MusicNoteBurst(Projectile.Center, NebulaOuter, 5, 3.5f);
            CustomParticles.GenericFlare(Projectile.Center, StarMote, 0.6f, 18);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D coreTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/StarBurst2").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow3").Value;
            Texture2D sparkleTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle5").Value;
            Vector2 coreOrigin = coreTex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 sparkleOrigin = sparkleTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.15f + 1f;
            
            // NEBULA GAS TRAIL - Billowing cloud effect behind projectile
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // Multiple gas layers per trail point
                Color gasColor = Color.Lerp(NebulaCore, NebulaGas, progress) * (1f - progress) * 0.4f;
                float gasScale = (0.5f - progress * 0.3f) * (1f + (float)Math.Sin(i * 0.5f) * 0.2f);
                sb.Draw(glowTex, trailPos, null, gasColor with { A = 0 }, nebulaRotation + i * 0.3f, glowOrigin, gasScale, SpriteEffects.None, 0f);
            }
            
            // OUTER NEBULA GLOW - Large diffuse halo
            sb.Draw(glowTex, drawPos, null, (NebulaGas * 0.25f) with { A = 0 }, nebulaRotation, glowOrigin, 0.8f * pulse, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, (NebulaOuter * 0.35f) with { A = 0 }, -nebulaRotation * 0.7f, glowOrigin, 0.6f * pulse, SpriteEffects.None, 0f);
            
            // ORBITING STAR MOTES - Tiny stars circling the core
            for (int i = 0; i < starMoteAngles.Length; i++)
            {
                Vector2 moteOffset = starMoteAngles[i].ToRotationVector2() * starMoteDistances[i];
                Vector2 motePos = drawPos + moteOffset;
                float moteScale = 0.12f + (float)Math.Sin(Main.GameUpdateCount * 0.2f + i) * 0.03f;
                Color moteColor = StarMote * (0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + i * 0.5f) * 0.3f);
                sb.Draw(sparkleTex, motePos, null, moteColor with { A = 0 }, starMoteAngles[i], sparkleOrigin, moteScale, SpriteEffects.None, 0f);
            }
            
            // NEBULA CORE - Swirling starburst center
            sb.Draw(coreTex, drawPos, null, (NebulaOuter * 0.5f) with { A = 0 }, nebulaRotation, coreOrigin, 0.35f * pulse, SpriteEffects.None, 0f);
            sb.Draw(coreTex, drawPos, null, (NebulaCore * 0.7f) with { A = 0 }, -nebulaRotation * 1.3f, coreOrigin, 0.25f * pulse, SpriteEffects.None, 0f);
            sb.Draw(coreTex, drawPos, null, (StarMote * 0.9f) with { A = 0 }, nebulaRotation * 0.5f, coreOrigin, 0.15f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Starfall projectile spawned by Nebula Arrow on impact - falls and explodes on ground
    /// </summary>
    public class NebulaStarfallProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle8";
        
        private static readonly Color StarCore = new Color(255, 255, 220);
        private static readonly Color StarTrail = new Color(255, 180, 220);
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            // Gravity - falls like a star
            Projectile.velocity.Y += 0.25f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // Sparkling trail
            if (Main.rand.NextBool(2))
            {
                var sparkle = new SparkleParticle(Projectile.Center, -Projectile.velocity * 0.1f, StarCore * 0.6f, 0.2f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(Projectile.Center, StarCore.ToVector3() * 0.5f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Ground explosion - star impact crater effect
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, StarTrail * 0.5f, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
            CustomParticles.GenericFlare(Projectile.Center, StarCore, 0.5f, 15);
            CustomParticles.HaloRing(Projectile.Center, StarTrail * 0.6f, 0.25f, 12);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            
            // Falling star trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color trailColor = Color.Lerp(StarCore, StarTrail, progress) * (1f - progress) * 0.6f;
                float trailScale = 0.2f * (1f - progress * 0.7f);
                sb.Draw(tex, trailPos, null, trailColor with { A = 0 }, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Star core
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, (StarCore * 0.9f) with { A = 0 }, Projectile.rotation, origin, 0.25f, SpriteEffects.None, 0f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 }, Projectile.rotation, origin, 0.12f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Serenade of Distant Stars' homing star projectile.
    /// </summary>
    public class SerenadeStarProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle13";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            Projectile.rotation += 0.15f;
            
            // Strong homing after initial delay
            if (Projectile.timeLeft < 270)
            {
                NPC target = FindClosestEnemy(800f);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 16f, 0.08f);
                }
            }
            
            // Star trail
            if (Main.rand.NextBool(2))
            {
                var star = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    NachtmusikCosmicVFX.StarWhite * 0.6f, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            // 笘・MUSICAL NOTATION - Serenade melody (VISIBLE SCALE 0.75f+)
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), -1.2f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, NachtmusikCosmicVFX.Gold, 0.75f, 35);
            }
            
            // 笘・SPARKLE ACCENT - Distant star twinkle
            if (Main.rand.NextBool(4))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), Main.rand.NextVector2Circular(1f, 1f), NachtmusikCosmicVFX.StarWhite, 0.3f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.StarWhite.ToVector3() * 0.6f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 360);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);
            NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 0.9f);
            
            // 笘・MUSICAL IMPACT - Serenade chord burst
            ThemedParticles.MusicNoteBurst(target.Center, NachtmusikCosmicVFX.StarWhite, 5, 3.5f);
            
            // === SEEKING CRYSTALS - Star burst ===
            if (Main.rand.NextBool(4))
            {
                SeekingCrystalHelper.SpawnNachtmusikCrystals(
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
            NachtmusikCosmicVFX.SpawnCelestialExplosion(Projectile.Center, 0.6f);
            
            // 笘・MUSICAL FINALE - Distant star symphony
            ThemedParticles.MusicNoteBurst(Projectile.Center, NachtmusikCosmicVFX.Gold, 6, 4f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle13").Value;
            Vector2 origin = tex.Size() / 2f;
            
            // Star trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color trailColor = NachtmusikCosmicVFX.GetCelestialGradient(progress) * (1f - progress) * 0.5f;
                sb.Draw(tex, drawPos, null, trailColor, Projectile.oldRot[i], origin, 0.35f * (1f - progress * 0.5f), SpriteEffects.None, 0f);
            }
            
            // Star core
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.15f + 0.85f;
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.Gold * 0.5f, Projectile.rotation, origin, 0.5f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.StarWhite * 0.9f, Projectile.rotation * 0.5f, origin, 0.3f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
        
        private NPC FindClosestEnemy(float range)
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
    }
    
    #endregion
    
    #region Magic Projectiles
    
    /// <summary>
    /// Starweaver's Grimoire's cosmic orb projectile.
    /// Creates mini-explosions along its path.
    /// </summary>
    public class StarweaverOrbProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/MagicSparklField8";
        
        private int explosionTimer = 0;
        
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        
        public override void AI()
        {
            Projectile.rotation += 0.08f;
            explosionTimer++;
            
            // Slow homing
            NPC target = FindClosestEnemy(600f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 12f, 0.04f);
            }
            
            // Periodic mini-explosions
            if (explosionTimer % 20 == 0)
            {
                // Mini star burst
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 burstPos = Projectile.Center + angle.ToRotationVector2() * 20f;
                    CustomParticles.GenericFlare(burstPos, NachtmusikCosmicVFX.Violet, 0.3f, 12);
                }
                
                // Damage enemies in radius
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.CanBeChasedBy(Projectile) && Vector2.Distance(npc.Center, Projectile.Center) < 80f)
                    {
                        npc.SimpleStrikeNPC(Projectile.damage / 3, 0, false, 0f, null, false, 0, false);
                    }
                }
            }
            
            // Ambient particles
            if (Main.rand.NextBool(2))
            {
                Color orbColor = NachtmusikCosmicVFX.GetCelestialGradient(Main.rand.NextFloat());
                var particle = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(1f, 1f), orbColor * 0.6f, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            // 笘・MUSICAL NOTATION - Starweaver cosmic melody (VISIBLE SCALE 0.72f+)
            if (Main.rand.NextBool(8))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -0.9f);
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), noteVel, NachtmusikCosmicVFX.Violet, 0.72f, 30);
            }
            
            // 笘・SPARKLE ACCENT - Cosmic weave shimmer
            if (Main.rand.NextBool(6))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), Main.rand.NextVector2Circular(1.5f, 1.5f), NachtmusikCosmicVFX.Gold, 0.25f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.Violet.ToVector3() * 0.7f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 420);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 2);
            NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 1f);
            
            // 笘・MUSICAL IMPACT - Cosmic grimoire chord
            ThemedParticles.MusicNoteBurst(target.Center, NachtmusikCosmicVFX.Violet, 5, 3.5f);
        }
        
        public override void OnKill(int timeLeft)
        {
            NachtmusikCosmicVFX.SpawnCelestialExplosion(Projectile.Center, 1.2f);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.7f }, Projectile.Center);
            
            // 笘・MUSICAL FINALE - Starweaver grand finale
            ThemedParticles.MusicNoteBurst(Projectile.Center, NachtmusikCosmicVFX.Gold, 7, 4.5f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/MagicSparklField8").Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f + 0.8f;
            
            // Multi-layer orb
            sb.Draw(tex, drawPos, null, NachtmusikCosmicVFX.DeepPurple * 0.4f, Projectile.rotation, origin, 0.8f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, NachtmusikCosmicVFX.Violet * 0.6f, -Projectile.rotation * 0.5f, origin, 0.55f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, NachtmusikCosmicVFX.Gold * 0.7f, Projectile.rotation * 0.3f, origin, 0.35f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, NachtmusikCosmicVFX.StarWhite, 0f, origin, 0.2f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
        
        private NPC FindClosestEnemy(float range)
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
    }
    
    /// <summary>
    /// Requiem of the Cosmos' cosmic beam that pierces everything.
    /// </summary>
    /// Requiem of the Cosmos - UNIQUE COSMIC BEAM AESTHETIC
    /// A flowing river of cosmic energy with orbiting galaxy motes, 
    /// constellation trail connections, and reality-warping visual effects.
    /// </summary>
    public class CosmicRequiemBeamProjectile : ModProjectile
    {
        // Use unique textures for cosmic beam look
        public override string Texture => "MagnumOpus/Assets/Particles/MagicSparklField7";
        
        private float cosmicRotation = 0f;
        private float[] galaxyMoteAngles = new float[4];
        private float warpPulse = 0f;
        
        // Cosmic Requiem color palette - deep space
        private static readonly Color CosmicCore = new Color(255, 255, 255);
        private static readonly Color CosmicMid = new Color(160, 120, 255);
        private static readonly Color CosmicOuter = new Color(80, 40, 160);
        private static readonly Color CosmicVoid = new Color(20, 10, 40);
        private static readonly Color GalaxyMote = new Color(255, 200, 255);
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
            Projectile.extraUpdates = 2;
            
            // Initialize galaxy mote orbit angles
            for (int i = 0; i < galaxyMoteAngles.Length; i++)
            {
                galaxyMoteAngles[i] = MathHelper.TwoPi * i / galaxyMoteAngles.Length;
            }
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            cosmicRotation += 0.15f;
            warpPulse += 0.2f;
            
            // Update orbiting galaxy motes
            for (int i = 0; i < galaxyMoteAngles.Length; i++)
            {
                galaxyMoteAngles[i] += 0.18f + i * 0.03f;
            }
            
            // COSMIC RIVER TRAIL - Flowing energy particles
            for (int layer = 0; layer < 2; layer++)
            {
                if (Main.rand.NextBool(2 - layer))
                {
                    float offset = (layer == 0) ? 0f : Main.rand.NextFloat(-4f, 4f);
                    Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * offset;
                    Color streamColor = Color.Lerp(CosmicMid, CosmicOuter, layer * 0.5f + Main.rand.NextFloat(0.2f));
                    Vector2 streamVel = -Projectile.velocity * (0.03f + layer * 0.02f) + Main.rand.NextVector2Circular(0.6f, 0.6f);
                    var stream = new GenericGlowParticle(Projectile.Center + perpendicular, streamVel, streamColor * (0.5f - layer * 0.15f), 
                        0.25f - layer * 0.06f, 16 + layer * 4, true);
                    MagnumParticleHandler.SpawnParticle(stream);
                }
            }
            
            // GALAXY MOTE SPARKLE TRAIL - Tiny galaxies shed from orbiting motes
            if (Projectile.timeLeft % 2 == 0)
            {
                int moteIndex = (Projectile.timeLeft / 2) % galaxyMoteAngles.Length;
                float moteRadius = 10f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + moteIndex) * 3f;
                Vector2 motePos = Projectile.Center + galaxyMoteAngles[moteIndex].ToRotationVector2() * moteRadius;
                var galaxyTrail = new SparkleParticle(motePos, -Projectile.velocity * 0.04f, GalaxyMote * 0.6f, 0.15f, 12);
                MagnumParticleHandler.SpawnParticle(galaxyTrail);
            }
            
            // GLYPHS - Ancient cosmic runes occasionally appear
            if (Main.rand.NextBool(12))
            {
                Vector2 glyphPos = Projectile.Center + Main.rand.NextVector2Circular(12f, 12f);
                CustomParticles.Glyph(glyphPos, CosmicMid * 0.7f, 0.35f, Main.rand.Next(1, 13));
            }
            
            // MUSIC NOTES - The requiem's eternal melody
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.8f, 0.8f)) * -1.2f;
                Color noteColor = Color.Lerp(CosmicMid, GalaxyMote, Main.rand.NextFloat(0.5f));
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), noteVel, noteColor, 0.75f, 28);
            }
            
            // PRISMATIC SPARKLES - Cosmic shimmer
            if (Main.rand.NextBool(3))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), 
                    -Projectile.velocity * 0.06f + Main.rand.NextVector2Circular(1.5f, 1.5f), CosmicCore * 0.8f, 0.28f, 14);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Pulsing cosmic light
            float lightIntensity = 0.8f + (float)Math.Sin(warpPulse) * 0.2f;
            Lighting.AddLight(Projectile.Center, CosmicMid.ToVector3() * lightIntensity);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 480);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 3);
            
            // COSMIC IMPACT - Reality ripple effect
            CustomParticles.GenericFlare(target.Center, CosmicCore, 0.9f, 18);
            CustomParticles.GenericFlare(target.Center, CosmicMid, 0.7f, 16);
            
            // Glyph burst on impact
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 glyphPos = target.Center + angle.ToRotationVector2() * 25f;
                CustomParticles.Glyph(glyphPos, CosmicMid * 0.6f, 0.4f, Main.rand.Next(1, 13));
            }
            
            // Cascading halos
            for (int i = 0; i < 3; i++)
            {
                Color haloColor = Color.Lerp(CosmicCore, CosmicOuter, i / 3f);
                CustomParticles.HaloRing(target.Center, haloColor * (0.6f - i * 0.15f), 0.25f + i * 0.12f, 12 + i * 2);
            }
            
            // Music chord impact
            ThemedParticles.MusicNoteBurst(target.Center, CosmicMid, 4, 3f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // COSMIC DISSIPATION - Beam fades into stardust
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color burstColor = Color.Lerp(CosmicMid, GalaxyMote, Main.rand.NextFloat());
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor * 0.5f, 0.25f, 22, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
            
            // Star sparkle finale
            for (int i = 0; i < 6; i++)
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), 
                    Main.rand.NextVector2Circular(2f, 2f), CosmicCore * 0.7f, 0.3f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            ThemedParticles.MusicNoteBurst(Projectile.Center, GalaxyMote, 6, 4f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D beamTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/MagicSparklField7").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            Texture2D sparkleTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle3").Value;
            Vector2 beamOrigin = beamTex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 sparkleOrigin = sparkleTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float pulse = (float)Math.Sin(warpPulse) * 0.12f + 1f;
            
            // COSMIC RIVER TRAIL - Flowing beam with constellation connections
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // Outer void glow
                Color voidColor = CosmicVoid * (1f - progress) * 0.5f;
                float voidScale = 0.6f * (1f - progress * 0.5f);
                sb.Draw(glowTex, trailPos, null, voidColor with { A = 0 }, cosmicRotation + i * 0.2f, glowOrigin, voidScale, SpriteEffects.None, 0f);
                
                // Mid cosmic glow
                Color trailColor = Color.Lerp(CosmicMid, CosmicOuter, progress) * (1f - progress) * 0.6f;
                float trailScale = 0.4f * (1f - progress * 0.6f);
                sb.Draw(beamTex, trailPos, null, trailColor with { A = 0 }, Projectile.oldRot[i], beamOrigin, trailScale, SpriteEffects.None, 0f);
                
                // Draw constellation lines connecting trail points
                if (i > 0 && i % 3 == 0 && Projectile.oldPos[i - 3] != Vector2.Zero)
                {
                    Vector2 prevPos = Projectile.oldPos[i - 3] + Projectile.Size / 2f - Main.screenPosition;
                    // Mini sparkle at connection point
                    Color connectColor = GalaxyMote * (1f - progress) * 0.4f;
                    sb.Draw(sparkleTex, trailPos, null, connectColor with { A = 0 }, 0f, sparkleOrigin, 0.1f * (1f - progress), SpriteEffects.None, 0f);
                }
            }
            
            // OUTER COSMIC HALO - Large diffuse glow
            sb.Draw(glowTex, drawPos, null, (CosmicOuter * 0.3f) with { A = 0 }, cosmicRotation, glowOrigin, 0.7f * pulse, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, (CosmicMid * 0.4f) with { A = 0 }, -cosmicRotation * 0.6f, glowOrigin, 0.5f * pulse, SpriteEffects.None, 0f);
            
            // ORBITING GALAXY MOTES - Tiny spiraling galaxies
            for (int i = 0; i < galaxyMoteAngles.Length; i++)
            {
                float moteRadius = 10f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + i * 0.7f) * 3f;
                Vector2 moteOffset = galaxyMoteAngles[i].ToRotationVector2() * moteRadius;
                Vector2 motePos = drawPos + moteOffset;
                float moteScale = 0.1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f + i) * 0.02f;
                Color moteColor = GalaxyMote * (0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.12f + i * 0.4f) * 0.3f);
                sb.Draw(sparkleTex, motePos, null, moteColor with { A = 0 }, galaxyMoteAngles[i] * 2f, sparkleOrigin, moteScale, SpriteEffects.None, 0f);
            }
            
            // BEAM CORE - Layered cosmic energy center
            sb.Draw(beamTex, drawPos, null, (CosmicOuter * 0.5f) with { A = 0 }, Projectile.rotation + cosmicRotation, beamOrigin, 0.45f * pulse, SpriteEffects.None, 0f);
            sb.Draw(beamTex, drawPos, null, (CosmicMid * 0.7f) with { A = 0 }, Projectile.rotation - cosmicRotation * 0.5f, beamOrigin, 0.32f * pulse, SpriteEffects.None, 0f);
            sb.Draw(beamTex, drawPos, null, (CosmicCore * 0.9f) with { A = 0 }, Projectile.rotation, beamOrigin, 0.2f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Twilight Severance's fast dimension-cutting slash projectile.
    /// Ultra-fast, short-range, high damage slashes.
    /// </summary>
    public class TwilightSlashProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SwordArc4";
        
        private bool isDimensionSever => Projectile.ai[0] == 1f;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 40; // Short-lived fast slash
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.extraUpdates = 2;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Faster for dimension sever
            if (isDimensionSever)
            {
                Projectile.velocity *= 1.02f;
                
                // More intense trail
                if (Main.rand.NextBool(2))
                {
                    Color trailColor = Color.Lerp(NachtmusikCosmicVFX.Gold, NachtmusikCosmicVFX.StarWhite, Main.rand.NextFloat());
                    var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f),
                        trailColor, 0.4f, 15, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                
                // 笘・MUSICAL NOTATION - Dimension sever crescendo (VISIBLE SCALE 0.78f+)
                if (Main.rand.NextBool(4))
                {
                    Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.7f, 0.7f), -1.3f);
                    ThemedParticles.MusicNote(Projectile.Center, noteVel, NachtmusikCosmicVFX.Gold, 0.78f, 25);
                }
                
                // 笘・SPARKLE ACCENT - Dimension shimmer
                if (Main.rand.NextBool(3))
                {
                    var sparkle = new SparkleParticle(Projectile.Center, -Projectile.velocity * 0.1f, NachtmusikCosmicVFX.StarWhite, 0.4f, 12);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            else
            {
                // Normal slash trail
                if (Main.rand.NextBool(3))
                {
                    Color trailColor = NachtmusikCosmicVFX.GetCelestialGradient(Main.rand.NextFloat());
                    var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.05f,
                        trailColor * 0.7f, 0.25f, 10, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                
                // 笘・MUSICAL NOTATION - Twilight melody (VISIBLE SCALE 0.7f+)
                if (Main.rand.NextBool(6))
                {
                    Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                    ThemedParticles.MusicNote(Projectile.Center, noteVel, NachtmusikCosmicVFX.Violet, 0.7f, 28);
                }
                
                // 笘・SPARKLE ACCENT - Twilight gleam
                if (Main.rand.NextBool(5))
                {
                    var sparkle = new SparkleParticle(Projectile.Center, Vector2.Zero, NachtmusikCosmicVFX.Gold, 0.25f, 15);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.Violet.ToVector3() * (isDimensionSever ? 0.6f : 0.35f));
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Celestial Harmony
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 240);
            if (target.TryGetGlobalNPC(out CelestialHarmonyNPC harmonyNPC))
            {
                harmonyNPC.AddStack(target, isDimensionSever ? 2 : 1);
            }
            
            // Hit VFX
            if (isDimensionSever)
            {
                NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 0.8f);
                // Starburst 
                var hitBurst = new StarBurstParticle(target.Center, Vector2.Zero, NachtmusikCosmicVFX.Gold, 0.35f, 12);
                MagnumParticleHandler.SpawnParticle(hitBurst);
                
                // 笘・MUSICAL IMPACT - Dimension sever chord
                ThemedParticles.MusicNoteBurst(target.Center, NachtmusikCosmicVFX.Gold, 5, 3.5f);
            }
            else
            {
                CustomParticles.GenericFlare(target.Center, NachtmusikCosmicVFX.Violet, 0.4f, 10);
                
                // 笘・MUSICAL IMPACT - Twilight chord
                ThemedParticles.MusicNoteBurst(target.Center, NachtmusikCosmicVFX.Violet, 3, 2.5f);
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            if (isDimensionSever)
            {
                NachtmusikCosmicVFX.SpawnCelestialExplosion(Projectile.Center, 0.6f);
                
                // 笘・MUSICAL FINALE - Dimension sever finale
                ThemedParticles.MusicNoteBurst(Projectile.Center, NachtmusikCosmicVFX.Gold, 6, 4f);
            }
            else
            {
                CustomParticles.GenericFlare(Projectile.Center, NachtmusikCosmicVFX.Violet, 0.35f, 12);
                
                // 笘・MUSICAL FINALE - Twilight fade
                ThemedParticles.MusicNoteBurst(Projectile.Center, NachtmusikCosmicVFX.Violet, 4, 3f);
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SwordArc4").Value;
            Vector2 origin = tex.Size() / 2f;
            
            Color coreColor = isDimensionSever ? NachtmusikCosmicVFX.Gold : NachtmusikCosmicVFX.Violet;
            float baseScale = isDimensionSever ? 0.5f : 0.35f;
            
            // Slash trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color trailColor = Color.Lerp(coreColor, NachtmusikCosmicVFX.DeepPurple, progress) * (1f - progress) * 0.6f;
                float scale = baseScale * (1f - progress * 0.5f);
                
                // Elongated for slash effect
                sb.Draw(tex, drawPos, null, trailColor, Projectile.oldRot[i], origin, new Vector2(scale * 2f, scale * 0.6f), SpriteEffects.None, 0f);
            }
            
            // Core
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.1f + 0.9f;
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, coreColor * 0.7f, Projectile.rotation, origin, new Vector2(baseScale * 1.5f * pulse, baseScale * 0.5f * pulse), SpriteEffects.None, 0f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.StarWhite, Projectile.rotation, origin, new Vector2(baseScale * 0.8f * pulse, baseScale * 0.25f * pulse), SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    #endregion
}
