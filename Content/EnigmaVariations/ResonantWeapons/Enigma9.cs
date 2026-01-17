using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.EnigmaVariations.Debuffs;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons
{
    /// <summary>
    /// Paradox Pendulum - Melee flail that creates temporal echoes
    /// Each swing leaves an echo that repeats the attack pattern
    /// Building up echoes triggers a massive time cascade burst
    /// </summary>
    public class Enigma9 : ModItem
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.Flairon;
        
        public override void SetDefaults()
        {
            Item.damage = 550;
            Item.DamageType = DamageClass.Melee;
            Item.width = 30;
            Item.height = 30;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 18);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<ParadoxPendulumHead>();
            Item.shootSpeed = 18f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect", "Swings leave temporal echoes that repeat your attacks"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect2", "Each echo is slightly shifted in time and space"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect3", "Build up 5 echoes to trigger time cascade"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaLore", "'The pendulum swings both ways - and every way between.'") 
            { 
                OverrideColor = EnigmaPurple 
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Launch VFX
            CustomParticles.GenericFlare(position, EnigmaPurple, 0.6f, 15);
            
            // Music notes on swing - the pendulum's rhythm
            ThemedParticles.EnigmaMusicNotes(position, 4, 30f);
            
            return true;
        }
    }
    
    public class ParadoxPendulumHead : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private List<Vector2> positionHistory = new List<Vector2>();
        private static int echoCount = 0;
        private const int MaxEchoes = 5;
        private bool returning = false;
        private float maxDistance = 350f;
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Flairon;
        
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Record position history for echo trail
            if (Main.GameUpdateCount % 3 == 0)
            {
                positionHistory.Add(Projectile.Center);
                if (positionHistory.Count > 40)
                    positionHistory.RemoveAt(0);
            }
            
            // Flail behavior
            float distFromPlayer = Vector2.Distance(Projectile.Center, owner.Center);
            
            if (!returning)
            {
                // Outward phase - decelerate
                Projectile.velocity *= 0.97f;
                
                if (distFromPlayer > maxDistance || Projectile.velocity.Length() < 4f)
                {
                    returning = true;
                }
            }
            else
            {
                // Return phase
                Vector2 toPlayer = (owner.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toPlayer * 18f, 0.12f);
                
                if (distFromPlayer < 30f)
                {
                    Projectile.Kill();
                    return;
                }
            }
            
            // Rotation
            Projectile.rotation += 0.3f * Math.Sign(Projectile.velocity.X);
            
            // Main trail with temporal distortion
            if (Main.GameUpdateCount % 2 == 0)
            {
                float progress = (float)Projectile.timeLeft / 300f;
                Color trailColor = GetEnigmaGradient(progress);
                var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f, 
                    trailColor * 0.7f, 0.4f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Draw temporal echo trail from history
            if (Main.GameUpdateCount % 4 == 0 && positionHistory.Count > 5)
            {
                for (int i = 0; i < positionHistory.Count; i += 5)
                {
                    float echoProgress = (float)i / positionHistory.Count;
                    Color echoColor = GetEnigmaGradient(echoProgress) * (0.3f * (1f - echoProgress));
                    CustomParticles.GenericFlare(positionHistory[i], echoColor, 0.2f * (1f - echoProgress), 8);
                }
            }
            
            // Sparkles marking the pendulum's path
            if (Main.GameUpdateCount % 30 == 0)
            {
                // Sparkle at past position
                if (positionHistory.Count > 10)
                {
                    Vector2 pastPos = positionHistory[positionHistory.Count / 2];
                    CustomParticles.GenericFlare(pastPos, EnigmaPurple * 0.7f, 0.45f, 16);
                    CustomParticles.HaloRing(pastPos, EnigmaGreen * 0.5f, 0.25f, 12);
                }
                // Sparkle at future position
                Vector2 futurePos = Projectile.Center + Projectile.velocity * 10f;
                CustomParticles.GenericFlare(futurePos, EnigmaGreen * 0.5f, 0.4f, 14);
                CustomParticles.HaloRing(futurePos, EnigmaPurple * 0.4f, 0.2f, 10);
            }
            
            // Periodic glyph aura
            if (Main.GameUpdateCount % 15 == 0)
            {
                CustomParticles.Glyph(Projectile.Center, EnigmaPurple, 0.25f, -1);
            }
            
            // Music notes in pendulum trail - temporal echoes of melody
            if (Main.GameUpdateCount % 10 == 0)
            {
                ThemedParticles.MusicNotes(Projectile.Center, GetEnigmaGradient(0.5f), 1, 12f);
            }
            
            // Keep owner facing the projectile
            owner.ChangeDir(Projectile.Center.X > owner.Center.X ? 1 : -1);
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
            
            Lighting.AddLight(Projectile.Center, GetEnigmaGradient(0.5f).ToVector3() * 0.5f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 360);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 2);
            
            // === TEMPORAL REALITY WARP ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 3.5f, 12);
            
            // Spawn a temporal echo
            if (echoCount < MaxEchoes && positionHistory.Count > 10)
            {
                echoCount++;
                
                // Create echo projectile that follows a delayed version of our path
                List<Vector2> echoPaths = new List<Vector2>(positionHistory);
                Projectile echo = Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromThis(), 
                    echoPaths[0], Vector2.Zero,
                    ModContent.ProjectileType<TemporalEchoAttack>(), 
                    (int)(Projectile.damage * 0.6f), 2f, Projectile.owner, echoCount);
                
                if (echo.ModProjectile is TemporalEchoAttack echoProj)
                {
                    echoProj.SetPath(echoPaths);
                }
                
                SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.2f + echoCount * 0.15f, Volume = 0.5f }, target.Center);
            }
            
            // Check for Time Cascade!
            if (echoCount >= MaxEchoes)
            {
                TriggerTimeCascade(target.Center);
            }
            
            // === NEW UNIFIED VFX HIT EFFECT ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.2f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            // Impact VFX
            CustomParticles.GenericFlare(target.Center, EnigmaGreen, 0.7f, 15);
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
            CustomParticles.HaloRing(target.Center, EnigmaPurple, 0.4f, 12);
            
            // Music notes on impact - the crescendo of time
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 6, 4f);
            
            // Sparkle burst above struck target
            CustomParticles.GenericFlare(target.Center - new Vector2(0, 30f), EnigmaPurple, 0.6f, 18);
            CustomParticles.HaloRing(target.Center - new Vector2(0, 30f), EnigmaGreen * 0.7f, 0.35f, 15);
            for (int impactIdx = 0; impactIdx < 4; impactIdx++)
            {
                float impactAngle = MathHelper.TwoPi * impactIdx / 4f;
                Vector2 impactVel = impactAngle.ToRotationVector2() * 2.5f;
                var impactSparkle = new GenericGlowParticle(target.Center - new Vector2(0, 30f), impactVel, GetEnigmaGradient((float)impactIdx / 4f), 0.3f, 16, true);
                MagnumParticleHandler.SpawnParticle(impactSparkle);
            }
            
            // Glyph stack visualization
            int stacks = brandNPC.paradoxStacks;
            CustomParticles.GlyphStack(target.Center + new Vector2(0, -22f), EnigmaGreen, stacks, 0.22f);
            
            // Fractal impact burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * 25f;
                Color burstColor = GetEnigmaGradient((float)i / 6f);
                CustomParticles.GenericFlare(target.Center + offset, burstColor, 0.45f, 12);
            }
        }
        
        private void TriggerTimeCascade(Vector2 center)
        {
            echoCount = 0; // Reset
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 1f }, center);
            
            // Massive time cascade explosion
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), center, Vector2.Zero,
                ModContent.ProjectileType<TimeCascadeExplosion>(), Projectile.damage * 2, 10f, Projectile.owner);
            
            // Massive visual burst
            for (int layer = 0; layer < 5; layer++)
            {
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f + layer * 0.2f;
                    float radius = 40f + layer * 30f;
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    Color cascadeColor = GetEnigmaGradient((float)(layer * 12 + i) / 60f);
                    CustomParticles.GenericFlare(center + offset, cascadeColor, 0.7f - layer * 0.1f, 22);
                }
            }
            // Sparkle formation around the cascade
            for (int cascadeIdx = 0; cascadeIdx < 8; cascadeIdx++)
            {
                float cascadeAngle = MathHelper.TwoPi * cascadeIdx / 8f + Main.GameUpdateCount * 0.015f;
                Vector2 cascadePos = center + cascadeAngle.ToRotationVector2() * 100f;
                CustomParticles.GenericFlare(cascadePos, EnigmaGreen, 0.55f, 20);
                CustomParticles.HaloRing(cascadePos, EnigmaPurple * 0.6f, 0.28f, 16);
            }
            
            // Glyph magic circles at multiple radii
            CustomParticles.GlyphCircle(center, EnigmaPurple, count: 8, radius: 50f, rotationSpeed: 0.08f);
            CustomParticles.GlyphCircle(center, EnigmaGreen, count: 10, radius: 80f, rotationSpeed: -0.06f);
            
            // Glyph explosion
            CustomParticles.GlyphBurst(center, EnigmaPurple, count: 12, speed: 6f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === TEMPORAL COLLAPSE REALITY WARP ===
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, 4f, 15);
            
            // Death burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * 4f;
                Color burstColor = GetEnigmaGradient((float)i / 8f);
                var glow = new GenericGlowParticle(Projectile.Center, vel, burstColor, 0.4f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            CustomParticles.HaloRing(Projectile.Center, EnigmaPurple, 0.5f, 15);
            
            // === WATCHING EYE at temporal collapse ===
            CustomParticles.EnigmaEyeGaze(Projectile.Center, EnigmaGreen * 0.6f, 0.4f, Projectile.velocity.SafeNormalize(Vector2.UnitX));
        }
    }
    
    public class TemporalEchoAttack : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private List<Vector2> pathToFollow = new List<Vector2>();
        private int pathIndex = 0;
        private int echoNumber => (int)Projectile.ai[0];
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Flairon;
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 100;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        
        public void SetPath(List<Vector2> path)
        {
            pathToFollow = path;
        }
        
        public override void AI()
        {
            // Follow the recorded path
            if (pathToFollow.Count > 0 && pathIndex < pathToFollow.Count)
            {
                Vector2 targetPos = pathToFollow[pathIndex];
                Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.3f);
                
                if (Vector2.Distance(Projectile.Center, targetPos) < 10f)
                {
                    pathIndex += 2; // Move faster through path
                }
            }
            else
            {
                Projectile.Kill();
                return;
            }
            
            Projectile.rotation += 0.25f;
            
            // Ghost trail effect - color shifted based on echo number
            if (Main.GameUpdateCount % 2 == 0)
            {
                float colorShift = (float)echoNumber / 5f;
                Color echoColor = GetEnigmaGradient(colorShift) * 0.5f;
                var glow = new GenericGlowParticle(Projectile.Center, Vector2.Zero, echoColor, 0.3f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Size/angle variation based on echo number
            float scaleVariation = 0.8f + echoNumber * 0.1f;
            Projectile.scale = scaleVariation;
            
            Lighting.AddLight(Projectile.Center, GetEnigmaGradient(echoNumber / 5f).ToVector3() * 0.3f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 240);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            // === TEMPORAL ECHO REALITY WARP ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 2.5f, 8);
            
            // === NEW UNIFIED VFX HIT EFFECT ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.2f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            // Echo impact
            float colorShift = (float)echoNumber / 5f;
            Color impactColor = GetEnigmaGradient(colorShift);
            CustomParticles.GenericFlare(target.Center, impactColor, 0.5f, 12);
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === TEMPORAL DEATH REALITY WARP ===
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, 3f, 10);
            
            Color echoColor = GetEnigmaGradient((float)echoNumber / 5f);
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 vel = angle.ToRotationVector2() * 2f;
                var glow = new GenericGlowParticle(Projectile.Center, vel, echoColor * 0.6f, 0.25f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }
    }
    
    public class TimeCascadeExplosion : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private const float ExplosionRadius = 220f;
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;
        
        public override void SetDefaults()
        {
            Projectile.width = 220;
            Projectile.height = 220;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 40;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 40f);
            float intensity = 1f - lifeProgress;
            float expandProgress = lifeProgress;
            
            // Expanding temporal distortion rings
            if (Main.GameUpdateCount % 2 == 0)
            {
                // Multiple temporal layers expanding
                for (int layer = 0; layer < 3; layer++)
                {
                    float layerProgress = (expandProgress + layer * 0.1f) % 1f;
                    float currentRadius = ExplosionRadius * layerProgress;
                    int points = 12;
                    
                    for (int i = 0; i < points; i++)
                    {
                        float angle = MathHelper.TwoPi * i / points + Main.GameUpdateCount * (0.1f + layer * 0.05f);
                        Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * currentRadius;
                        
                        Color particleColor = GetEnigmaGradient(((float)i / points + layer * 0.3f) % 1f) * intensity;
                        CustomParticles.GenericFlare(particlePos, particleColor, 0.45f * intensity, 10);
                    }
                }
            }
            
            // Central time distortion
            if (Main.GameUpdateCount % 3 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, EnigmaBlack, 0.8f * intensity, 12);
                CustomParticles.HaloRing(Projectile.Center, EnigmaPurple * intensity, 0.5f, 10);
            }
            
            // Sparkles swirling within the cascade
            if (Main.GameUpdateCount % 8 == 0)
            {
                float sparkleRadius = ExplosionRadius * 0.6f * expandProgress;
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2CircularEdge(sparkleRadius, sparkleRadius);
                CustomParticles.GenericFlare(sparklePos, EnigmaGreen * intensity, 0.5f, 15);
                CustomParticles.HaloRing(sparklePos, EnigmaPurple * intensity * 0.6f, 0.25f, 12);
            }
            
            Lighting.AddLight(Projectile.Center, GetEnigmaGradient(0.5f).ToVector3() * intensity);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 4);
            
            // === TIME CASCADE REALITY WARP ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 5f, 18);
            FateRealityDistortion.TriggerInversionPulse(6);
            
            // === NEW UNIFIED VFX EXPLOSION ===
            UnifiedVFX.EnigmaVariations.Explosion(target.Center, 1.5f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            CustomParticles.GenericFlare(target.Center, EnigmaGreen, 0.7f, 15);
            CustomParticles.GlyphStack(target.Center + new Vector2(0, -25f), EnigmaPurple, brandNPC.paradoxStacks, 0.25f);
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
    }
}
