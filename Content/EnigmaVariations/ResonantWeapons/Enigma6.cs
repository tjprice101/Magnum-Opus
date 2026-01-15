using Microsoft.Xna.Framework.Graphics;
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
    /// VOID QUESTION CANNON - Enigma Ranged Gun
    /// ==========================================
    /// UNIQUE MECHANICS:
    /// - Fires MASSIVE, SLOW reality-distorting projectiles
    /// - Shot warps visual space around it as it travels (warped particle trails)
    /// - On hit: creates VOID SINGULARITY that pulls enemies inward
    /// - Singularity collapses with MASSIVE eye-burst explosion
    /// - Alt fire (right-click): shoots rapid "?" shaped bolts that home weakly
    /// - Enemies pulled into singularity get heavy Paradox stacks
    /// - Eyes WATCH from within the singularity
    /// </summary>
    public class Enigma6 : ModItem
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.VortexBeater;
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetDefaults()
        {
            Item.damage = 450;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 52;
            Item.height = 26;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(gold: 22);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
            Item.UseSound = SoundID.Item92;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<VoidQuestionShot>();
            Item.shootSpeed = 5f;
            Item.noMelee = true;
            Item.useAmmo = AmmoID.Bullet;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect1", "Fires slow, massive reality-warping shots"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect2", "Hits create void singularities that pull enemies"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect3", "Right-click fires rapid homing question bolts"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaLore", "'What is the sound of reality collapsing?'") 
            { 
                OverrideColor = EnigmaPurple 
            });
        }
        
        public override void HoldItem(Player player)
        {
            // Ambient void particles around gun
            if (Main.rand.NextBool(8))
            {
                Vector2 gunPos = player.Center + new Vector2(player.direction * 30f, -5f);
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Main.rand.NextFloat(20f, 40f);
                Vector2 particlePos = gunPos + angle.ToRotationVector2() * radius;
                Vector2 vel = (gunPos - particlePos).SafeNormalize(Vector2.Zero) * 1.5f;
                
                var glow = new GenericGlowParticle(particlePos, vel, GetEnigmaGradient(Main.rand.NextFloat()) * 0.5f, 
                    0.22f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Random sparkle peek
            if (Main.rand.NextBool(60))
            {
                Vector2 sparklePos = player.Center + new Vector2(player.direction * 35f, Main.rand.Next(-25, 15));
                CustomParticles.GenericFlare(sparklePos, GetEnigmaGradient(Main.rand.NextFloat()) * 0.5f, 0.32f, 14);
            }
        }
        
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        
        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            if (player.altFunctionUse == 2)
                return Main.rand.NextFloat() < 0.33f; // 33% ammo consumption on alt fire
            return true;
        }
        
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                type = ModContent.ProjectileType<QuestionBolt>();
                damage = (int)(damage * 0.35f);
                knockback *= 0.3f;
            }
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Alt fire - rapid homing question bolts
                SoundEngine.PlaySound(SoundID.Item125 with { Pitch = 0.3f, Volume = 0.6f }, player.Center);
                
                for (int i = -1; i <= 1; i++)
                {
                    Vector2 spreadVel = velocity.RotatedBy(MathHelper.ToRadians(8f * i));
                    Projectile.NewProjectile(source, position, spreadVel, type, damage, knockback, player.whoAmI);
                }
                
                // Quick muzzle flash
                CustomParticles.GenericFlare(position + velocity.SafeNormalize(Vector2.Zero) * 25f, EnigmaPurple, 0.45f, 12);
                CustomParticles.HaloRing(position + velocity.SafeNormalize(Vector2.Zero) * 20f, EnigmaGreen * 0.5f, 0.25f, 10);
            }
            else
            {
                // Primary fire - massive void shot
                SoundEngine.PlaySound(SoundID.Item92 with { Pitch = -0.5f, Volume = 1.0f }, player.Center);
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 0.7f }, player.Center);
                
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                
                // Massive muzzle VFX
                Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 40f;
                
                CustomParticles.GenericFlare(muzzlePos, Color.White, 0.9f, 22);
                CustomParticles.GenericFlare(muzzlePos, EnigmaPurple, 0.75f, 20);
                
                for (int i = 0; i < 8; i++)
                {
                    float angle = velocity.ToRotation() + MathHelper.PiOver4 * ((float)i / 8f - 0.5f);
                    Vector2 offset = angle.ToRotationVector2() * 30f;
                    CustomParticles.GenericFlare(muzzlePos + offset, GetEnigmaGradient((float)i / 8f), 0.5f, 16);
                }
                
                for (int ring = 0; ring < 3; ring++)
                {
                    CustomParticles.HaloRing(muzzlePos, GetEnigmaGradient(ring / 3f), 0.35f + ring * 0.15f, 14 + ring * 3);
                }
                
                CustomParticles.GlyphBurst(muzzlePos, EnigmaPurple, count: 6, speed: 4f);
                
                // Sparkle targeting beam at muzzle
                for (int spark = 0; spark < 3; spark++)
                {
                    Vector2 beamPos = muzzlePos + velocity.SafeNormalize(Vector2.Zero) * (8f + spark * 12f);
                    CustomParticles.GenericFlare(beamPos, GetEnigmaGradient((float)spark / 3f), 0.4f, 14);
                }
                
                // Music notes - the void's haunting melody
                ThemedParticles.EnigmaMusicNoteBurst(muzzlePos, 8, 5f);
                
                // Recoil visual (particles going backward)
                for (int i = 0; i < 8; i++)
                {
                    Vector2 recoilVel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 6f)
                        + Main.rand.NextVector2Circular(2f, 2f);
                    var glow = new GenericGlowParticle(position, recoilVel, EnigmaPurple * 0.6f, 0.28f, 16, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// The massive void shot projectile
    /// </summary>
    public class VoidQuestionShot : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Custom massive void shot rendering
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            float lifeProgress = (180f - Projectile.timeLeft) / 180f;
            
            // Draw intense trail with chromatic aberration
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailProgress = (float)i / Projectile.oldPos.Length;
                Color trailColor = GetEnigmaGradient(trailProgress + lifeProgress) * (1f - trailProgress) * 0.6f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailScale = (1f - trailProgress * 0.4f) * Projectile.scale * 1.3f;
                
                // Chromatic aberration on trail
                spriteBatch.Draw(glowTex, trailPos + new Vector2(-3, 0), null, new Color(180, 50, 120) * (1f - trailProgress) * 0.3f, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(glowTex, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(glowTex, trailPos + new Vector2(3, 0), null, new Color(50, 150, 100) * (1f - trailProgress) * 0.3f, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Outer massive void glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f + 1f;
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaBlack * 0.6f, Projectile.rotation, origin, Projectile.scale * 2.8f * pulse, SpriteEffects.None, 0f);
            
            // Mid purple layer
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaPurple * 0.7f, Projectile.rotation, origin, Projectile.scale * 2.0f, SpriteEffects.None, 0f);
            
            // Inner green core
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaGreen * 0.8f, Projectile.rotation, origin, Projectile.scale * 1.2f, SpriteEffects.None, 0f);
            
            // Central white flash
            spriteBatch.Draw(glowTex, drawPos, null, Color.White * 0.9f, Projectile.rotation, origin, Projectile.scale * 0.6f, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.scale = 1.5f;
        }
        
        public override void AI()
        {
            Projectile.rotation += 0.15f;
            float age = 180 - Projectile.timeLeft;
            float lifeProgress = age / 180f;
            
            // WARPED SPACE EFFECT - particles spiral around the projectile
            for (int i = 0; i < 3; i++)
            {
                float spiralAngle = Projectile.rotation * 2f + i * MathHelper.TwoPi / 3f + age * 0.12f;
                float spiralRadius = 25f + (float)Math.Sin(age * 0.1f + i) * 10f;
                Vector2 spiralPos = Projectile.Center + spiralAngle.ToRotationVector2() * spiralRadius;
                
                CustomParticles.GenericFlare(spiralPos, GetEnigmaGradient((float)i / 3f + lifeProgress) * 0.7f, 0.4f, 14);
            }
            
            // Main projectile glow
            if (Main.rand.NextBool(2))
            {
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), 
                    GetEnigmaGradient(Main.rand.NextFloat()), 0.55f, 18);
            }
            
            // Trailing warp effect
            if (Projectile.timeLeft % 5 == 0)
            {
                for (int ring = 0; ring < 2; ring++)
                {
                    CustomParticles.HaloRing(Projectile.Center, GetEnigmaGradient(ring * 0.5f + lifeProgress) * 0.5f, 
                        0.25f + ring * 0.15f, 12 + ring * 3);
                }
            }
            
            // Sparkles embedded in the void sphere
            if (Projectile.timeLeft % 15 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float sparkAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * Main.rand.NextFloat(5f, 18f);
                    CustomParticles.GenericFlare(sparkPos, GetEnigmaGradient(Main.rand.NextFloat()), 0.38f, 14);
                }
            }
            
            // Glyph orbit
            if (Projectile.timeLeft % 12 == 0)
            {
                CustomParticles.GlyphCircle(Projectile.Center, EnigmaPurple, count: 3, radius: 28f, rotationSpeed: 0.1f);
            }
            
            // Distortion smoke
            if (Main.rand.NextBool(2))
            {
                var smoke = new HeavySmokeParticle(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    EnigmaBlack, Main.rand.Next(20, 35), 0.35f, 0.65f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.8f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 360);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 2);
            
            // === NEW UNIFIED VFX EXPLOSION ===
            UnifiedVFX.EnigmaVariations.Explosion(target.Center, 1.5f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // Spawn void singularity
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                ModContent.ProjectileType<VoidSingularity>(), Projectile.damage / 2, 0f, Projectile.owner);
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.4f }, Projectile.Center);
            
            // Spawn void singularity on terrain hit too
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<VoidSingularity>(), Projectile.damage / 2, 0f, Projectile.owner);
            
            // Impact VFX
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 1.1f, 25);
            
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 offset = angle.ToRotationVector2() * 40f;
                CustomParticles.GenericFlare(Projectile.Center + offset, GetEnigmaGradient((float)i / 12f), 0.6f, 20);
            }
            
            for (int ring = 0; ring < 4; ring++)
            {
                CustomParticles.HaloRing(Projectile.Center, GetEnigmaGradient(ring / 4f), 0.45f + ring * 0.2f, 16 + ring * 4);
            }
            
            // Sparkle burst explosion
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 burstVel = angle.ToRotationVector2() * 5f;
                var sparkle = new GenericGlowParticle(Projectile.Center, burstVel, GetEnigmaGradient((float)i / 8f), 0.4f, 18, true);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaGreen, count: 8, speed: 5f);
        }
    }
    
    /// <summary>
    /// Void singularity that pulls enemies
    /// </summary>
    public class VoidSingularity : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private const int Duration = 120;
        private const float PullRadius = 250f;
        private const float PullStrength = 8f;
        
        private List<int> affectedNPCs = new List<int>();
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Custom singularity rendering - swirling vortex of eyes and glyphs
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            float lifeProgress = 1f - (float)Projectile.timeLeft / Duration;
            float pullMultiplier = (float)Math.Sin(lifeProgress * MathHelper.Pi);
            
            // Outer void darkness
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.15f + 1f;
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaBlack * 0.7f, 0f, origin, 3.0f * pullMultiplier * pulse, SpriteEffects.None, 0f);
            
            // Rotating rings effect
            float spin = Main.GameUpdateCount * 0.08f;
            for (int i = 0; i < 6; i++)
            {
                float angle = spin + MathHelper.TwoPi * i / 6f;
                Vector2 ringOffset = angle.ToRotationVector2() * 20f * pullMultiplier;
                Color ringColor = GetEnigmaGradient((float)i / 6f) * 0.6f;
                spriteBatch.Draw(glowTex, drawPos + ringOffset, null, ringColor, angle, origin, 0.6f * pullMultiplier, SpriteEffects.None, 0f);
            }
            
            // Inner purple glow
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaPurple * 0.8f * pullMultiplier, 0f, origin, 1.8f * pullMultiplier, SpriteEffects.None, 0f);
            
            // Central green eye
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaGreen * 0.9f, 0f, origin, 0.9f * pullMultiplier, SpriteEffects.None, 0f);
            
            // White core
            spriteBatch.Draw(glowTex, drawPos, null, Color.White * pullMultiplier, 0f, origin, 0.4f * pullMultiplier, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (float)Projectile.timeLeft / Duration;
            float pullMultiplier = (float)Math.Sin(lifeProgress * MathHelper.Pi); // Peaks in middle, fades at start/end
            
            // Pull enemies inward
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || !npc.active) continue;
                
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist <= PullRadius && dist > 30f)
                {
                    Vector2 pullDir = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                    float pullForce = PullStrength * pullMultiplier * (1f - dist / PullRadius);
                    npc.velocity += pullDir * pullForce * 0.1f;
                    
                    // Track affected NPCs
                    if (!affectedNPCs.Contains(npc.whoAmI))
                    {
                        affectedNPCs.Add(npc.whoAmI);
                    }
                    
                    // Apply paradox stacks over time
                    if (Projectile.timeLeft % 30 == 0)
                    {
                        npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
                        npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, 1);
                        
                        // Show stacks
                        int stacks = npc.GetGlobalNPC<ParadoxBrandNPC>().paradoxStacks;
                        if (stacks > 0 && stacks < 5)
                        {
                            CustomParticles.GlyphStack(npc.Center + new Vector2(0, -28f), EnigmaGreen, stacks, 0.25f);
                        }
                    }
                    
                    // Visual connection to pulled enemy
                    if (Main.rand.NextBool(8))
                    {
                        MagnumVFX.DrawFractalLightning(Projectile.Center, npc.Center, EnigmaPurple * 0.5f, 6, 15f, 2, 0.25f);
                    }
                }
            }
            
            // SINGULARITY VISUALS
            
            // Central vortex
            float spin = Main.GameUpdateCount * 0.15f;
            for (int i = 0; i < 6; i++)
            {
                float angle = spin + MathHelper.TwoPi * i / 6f;
                float radius = 35f * pullMultiplier;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;
                CustomParticles.GenericFlare(pos, GetEnigmaGradient((float)i / 6f + lifeProgress), 0.5f, 14);
            }
            
            // Particles spiraling inward
            if (Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = PullRadius * 0.6f * Main.rand.NextFloat();
                Vector2 startPos = Projectile.Center + angle.ToRotationVector2() * radius;
                Vector2 vel = (Projectile.Center - startPos).SafeNormalize(Vector2.Zero) * 4f;
                vel = vel.RotatedBy(MathHelper.PiOver4); // Spiral motion
                
                var glow = new GenericGlowParticle(startPos, vel, GetEnigmaGradient(Main.rand.NextFloat()) * 0.6f, 
                    0.28f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Periodic halo rings (growing/shrinking)
            if (Projectile.timeLeft % 10 == 0)
            {
                float ringScale = 0.3f + pullMultiplier * 0.4f;
                CustomParticles.HaloRing(Projectile.Center, GetEnigmaGradient(lifeProgress), ringScale, 16);
            }
            
            // Eyes watching from within
            if (Projectile.timeLeft % 20 == 0)
            {
                // Eyes looking at nearby enemies
                NPC closest = null;
                float closestDist = PullRadius;
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.friendly) continue;
                    float dist = Vector2.Distance(npc.Center, Projectile.Center);
                    if (dist < closestDist)
                    {
                        closest = npc;
                        closestDist = dist;
                    }
                }
                
                if (closest != null)
                {
                    // Sparkle tracker pointing toward target
                    Vector2 trackDir = (closest.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 trackPos = Projectile.Center + trackDir * (15f + i * 10f);
                        CustomParticles.GenericFlare(trackPos, GetEnigmaGradient((float)i / 3f), 0.42f, 14);
                    }
                }
                else
                {
                    // Orbiting sparkle constellation
                    for (int i = 0; i < 4; i++)
                    {
                        float constAngle = Main.GameUpdateCount * 0.05f + MathHelper.TwoPi * i / 4f;
                        Vector2 constPos = Projectile.Center + constAngle.ToRotationVector2() * 35f;
                        CustomParticles.GenericFlare(constPos, GetEnigmaGradient((float)i / 4f), 0.38f, 12);
                    }
                }
            }
            
            // Glyph circle orbiting
            if (Projectile.timeLeft % 15 == 0)
            {
                CustomParticles.GlyphCircle(Projectile.Center, EnigmaPurple, count: 5, radius: 40f * pullMultiplier, rotationSpeed: 0.08f);
            }
            
            // Black void smoke at center
            if (Main.rand.NextBool(2))
            {
                var smoke = new HeavySmokeParticle(Projectile.Center + Main.rand.NextVector2Circular(18f, 18f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    EnigmaBlack, Main.rand.Next(20, 40), 0.4f, 0.7f, 0.025f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * pullMultiplier);
            
            SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.3f, Volume = 0.15f * pullMultiplier }, Projectile.Center);
        }
        
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.3f, Volume = 1.2f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.9f }, Projectile.Center);
            
            // MASSIVE COLLAPSE EXPLOSION
            
            // Central white flash
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 1.6f, 30);
            
            // Expanding fractal burst
            for (int layer = 0; layer < 4; layer++)
            {
                int points = 8 + layer * 4;
                float radius = 40f + layer * 50f;
                
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points + layer * 0.3f;
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    CustomParticles.GenericFlare(Projectile.Center + offset, GetEnigmaGradient((float)i / points), 
                        0.75f - layer * 0.1f, 26);
                }
            }
            
            // Many halo rings
            for (int ring = 0; ring < 8; ring++)
            {
                CustomParticles.HaloRing(Projectile.Center, GetEnigmaGradient(ring / 8f), 0.4f + ring * 0.22f, 18 + ring * 5);
            }
            
            // Explosion particle burst
            CustomParticles.ExplosionBurst(Projectile.Center, EnigmaPurple, 25, 12f);
            CustomParticles.ExplosionBurst(Projectile.Center, EnigmaGreen, 18, 10f);
            
            // Grand sparkle nova burst
            for (int layer = 0; layer < 2; layer++)
            {
                int points = 10 + layer * 4;
                float radius = 50f + layer * 35f;
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points + layer * 0.15f;
                    Vector2 novaPos = Projectile.Center + angle.ToRotationVector2() * radius;
                    CustomParticles.GenericFlare(novaPos, GetEnigmaGradient((float)i / points), 0.55f - layer * 0.1f, 22);
                    
                    Vector2 novaVel = angle.ToRotationVector2() * (5f + layer * 2f);
                    var glow = new GenericGlowParticle(novaPos, novaVel, EnigmaPurple * 0.7f, 0.35f, 18, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }
            ThemedParticles.EnigmaMusicNoteBurst(Projectile.Center, 12, 6f);
            
            // Massive glyph burst
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaGreen, count: 16, speed: 8f);
            CustomParticles.GlyphCircle(Projectile.Center, EnigmaPurple, count: 12, radius: 70f, rotationSpeed: 0.1f);
            
            // Chain lightning to all affected enemies
            Player owner = Main.player[Projectile.owner];
            float explosionRadius = 220f;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist <= explosionRadius)
                {
                    float falloff = 1f - (dist / explosionRadius) * 0.5f;
                    int explosionDamage = (int)(Projectile.damage * 2f * falloff);
                    
                    npc.SimpleStrikeNPC(explosionDamage, owner.direction, true, 12f);
                    npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 360);
                    
                    var brandNPC = npc.GetGlobalNPC<ParadoxBrandNPC>();
                    brandNPC.AddParadoxStack(npc, 3);
                    
                    // Lightning to each enemy
                    MagnumVFX.DrawFractalLightning(Projectile.Center, npc.Center, EnigmaGreen, 12, 35f, 5, 0.4f);
                    
                    // Sparkle crown at struck enemy
                    for (int crown = 0; crown < 5; crown++)
                    {
                        float crownAngle = MathHelper.Pi + MathHelper.Pi * crown / 5f - MathHelper.PiOver2;
                        Vector2 crownPos = npc.Center - new Vector2(0, 30f) + crownAngle.ToRotationVector2() * 20f;
                        CustomParticles.GenericFlare(crownPos, GetEnigmaGradient((float)crown / 5f), 0.42f, 16);
                    }
                    
                    // Trigger paradox explosion at 5 stacks
                    if (brandNPC.paradoxStacks >= 5)
                    {
                        brandNPC.paradoxStacks = 0;
                        
                        CustomParticles.GenericFlare(npc.Center, Color.White, 1.0f, 22);
                        CustomParticles.GlyphBurst(npc.Center, EnigmaGreen, count: 8, speed: 5f);
                        
                        for (int i = 0; i < 8; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 8f;
                            Vector2 offset = angle.ToRotationVector2() * 35f;
                            CustomParticles.GenericFlare(npc.Center + offset, GetEnigmaGradient((float)i / 8f), 0.55f, 18);
                        }
                        
                        // Chain to other nearby enemies
                        foreach (NPC other in Main.ActiveNPCs)
                        {
                            if (other.friendly || other.whoAmI == npc.whoAmI) continue;
                            float chainDist = Vector2.Distance(npc.Center, other.Center);
                            if (chainDist <= 150f)
                            {
                                other.SimpleStrikeNPC(Projectile.damage, owner.direction, false, 6f);
                                other.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(other, 2);
                            }
                        }
                    }
                }
            }
        }
        
        public override bool? CanDamage() => false; // Damage on kill only
    }
    
    /// <summary>
    /// Rapid question bolt for alt fire
    /// </summary>
    public class QuestionBolt : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private float homingStrength = 0f;
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Custom question mark bolt rendering
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            
            // Draw subtle trail
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailProgress = (float)i / Projectile.oldPos.Length;
                Color trailColor = GetEnigmaGradient(trailProgress) * (1f - trailProgress) * 0.5f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailScale = (1f - trailProgress * 0.5f) * 0.6f;
                spriteBatch.Draw(glowTex, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Outer glow
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaPurple * 0.6f, Projectile.rotation, origin, 0.9f, SpriteEffects.None, 0f);
            
            // Inner core
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaGreen * 0.8f, Projectile.rotation, origin, 0.5f, SpriteEffects.None, 0f);
            
            // White center
            spriteBatch.Draw(glowTex, drawPos, null, Color.White * 0.7f, Projectile.rotation, origin, 0.25f, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // Weak homing
            homingStrength = Math.Min(homingStrength + 0.02f, 0.06f);
            
            NPC target = null;
            float closestDist = 350f;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || !npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist < closestDist)
                {
                    target = npc;
                    closestDist = dist;
                }
            }
            
            if (target != null)
            {
                Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, homingStrength);
            }
            
            // "?" shaped trail effect
            if (Main.rand.NextBool(2))
            {
                float gradientProgress = (float)(120 - Projectile.timeLeft) / 120f;
                CustomParticles.GenericFlare(Projectile.Center, GetEnigmaGradient(gradientProgress) * 0.6f, 0.28f, 12);
            }
            
            // Occasional glyph
            if (Projectile.timeLeft % 15 == 0)
            {
                CustomParticles.GlyphTrail(Projectile.Center, Projectile.velocity, EnigmaPurple, 0.22f);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.35f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 180);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            // === NEW UNIFIED VFX HIT EFFECT ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.2f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            // Light impact
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 offset = angle.ToRotationVector2() * 18f;
                CustomParticles.GenericFlare(target.Center + offset, GetEnigmaGradient((float)i / 5f), 0.38f, 14);
            }
            
            CustomParticles.HaloRing(target.Center, EnigmaPurple, 0.3f, 12);
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            int stacks = target.GetGlobalNPC<ParadoxBrandNPC>().paradoxStacks;
            if (stacks > 0 && stacks < 5)
            {
                CustomParticles.GlyphStack(target.Center + new Vector2(0, -25f), EnigmaGreen, stacks, 0.22f);
            }
            else if (stacks >= 5)
            {
                // Quick explosion at 5 stacks
                target.GetGlobalNPC<ParadoxBrandNPC>().paradoxStacks = 0;
                
                // === UNIFIED VFX EXPLOSION FOR STACK PROC ===
                UnifiedVFX.EnigmaVariations.Explosion(target.Center, 1.5f);
                
                CustomParticles.GenericFlare(target.Center, Color.White, 0.8f, 20);
                CustomParticles.GlyphBurst(target.Center, EnigmaGreen, count: 6, speed: 4f);
                
                // Sparkle burst on stack explosion
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 burstVel = angle.ToRotationVector2() * 4f;
                    var sparkle = new GenericGlowParticle(target.Center, burstVel, GetEnigmaGradient((float)i / 6f), 0.38f, 16, true);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                // Chain damage
                Player owner = Main.player[Projectile.owner];
                foreach (NPC other in Main.ActiveNPCs)
                {
                    if (other.friendly || other.whoAmI == target.whoAmI) continue;
                    float dist = Vector2.Distance(target.Center, other.Center);
                    if (dist <= 120f)
                    {
                        other.SimpleStrikeNPC(Projectile.damage, owner.direction, false, 4f);
                        other.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(other, 1);
                    }
                }
            }
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Small burst
            CustomParticles.GenericFlare(Projectile.Center, EnigmaPurple, 0.45f, 14);
            CustomParticles.HaloRing(Projectile.Center, EnigmaGreen * 0.5f, 0.25f, 10);
            
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * 15f;
                CustomParticles.GenericFlare(Projectile.Center + offset, GetEnigmaGradient((float)i / 6f), 0.32f, 12);
            }
        }
    }
}
