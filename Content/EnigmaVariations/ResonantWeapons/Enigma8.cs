using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    /// Uncertainty Launcher - Ranged launcher that fires rockets existing in quantum superposition
    /// On impact, the rocket collapses into one of several random powerful effects
    /// </summary>
    public class Enigma8 : ModItem
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
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.RocketLauncher;
        
        public override void SetDefaults()
        {
            Item.damage = 430;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 50;
            Item.height = 24;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 18);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<SuperpositionRocket>();
            Item.shootSpeed = 14f;
            Item.noMelee = true;
            Item.useAmmo = AmmoID.Rocket;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect", "Fires rockets that exist in quantum superposition"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect2", "On impact, reality collapses into a random powerful effect:"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect3", "  - Fractal Split: Splits into 6 homing fragments"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect4", "  - Void Singularity: Creates a pulling black hole"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect5", "  - Chain Cascade: Lightning chains between enemies"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect6", "  - Paradox Nova: Massive AOE explosion"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaLore", "'Until observed, all outcomes are true.'") 
            { 
                OverrideColor = EnigmaPurple 
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Override to use our superposition rocket
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<SuperpositionRocket>(), 
                damage, knockback, player.whoAmI);
            
            // Muzzle flash with quantum uncertainty visual
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 30f;
            CustomParticles.GenericFlare(muzzlePos, EnigmaGreen, 0.7f, 15);
            CustomParticles.HaloRing(muzzlePos, EnigmaPurple, 0.5f, 12);
            
            // Glyph symbols representing uncertainty
            CustomParticles.GlyphBurst(muzzlePos, EnigmaPurple, count: 4, speed: 2f);
            
            // Multiple ghost flares showing possible trajectories
            for (int i = 0; i < 5; i++)
            {
                float angleOffset = MathHelper.ToRadians(-10f + i * 5f);
                Vector2 ghostVel = velocity.RotatedBy(angleOffset);
                Vector2 ghostPos = muzzlePos + ghostVel.SafeNormalize(Vector2.Zero) * (20f + i * 15f);
                Color ghostColor = GetEnigmaGradient((float)i / 5f) * 0.4f;
                CustomParticles.GenericFlare(ghostPos, ghostColor, 0.3f, 10);
            }
            
            // Music notes - quantum superposition of melodies
            ThemedParticles.EnigmaMusicNotes(muzzlePos, 5, 35f);
            
            return false;
        }
    }
    
    public class SuperpositionRocket : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        // Ghost trail positions for superposition effect
        private Vector2[] ghostPositions = new Vector2[4];
        private int ghostIndex = 0;
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Switch to additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D eyeTex = CustomParticleSystem.EnigmaEyes[((int)(Main.GameUpdateCount * 0.04f)) % 8].Value;
            Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
            Texture2D sparkleTex = CustomParticleSystem.RandomPrismaticSparkle().Value;
            Texture2D flareTex = CustomParticleSystem.EnergyFlares[0].Value;
            
            // Draw quantum ghost trails (superposition states)
            foreach (Vector2 ghostPos in ghostPositions)
            {
                if (ghostPos != Vector2.Zero)
                {
                    Vector2 ghostDrawPos = ghostPos - Main.screenPosition;
                    spriteBatch.Draw(sparkleTex, ghostDrawPos, null, EnigmaPurple * 0.25f, Main.GameUpdateCount * 0.03f, sparkleTex.Size() / 2f, 0.2f, SpriteEffects.None, 0f);
                }
            }
            
            // Eerie glyph trail showing uncertainty
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailProgress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress) * 0.6f;
                float trailScale = (1f - trailProgress * 0.4f) * 0.18f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                Color trailColor = Color.Lerp(EnigmaGreenFlame, EnigmaPurple, trailProgress);
                spriteBatch.Draw(sparkleTex, trailPos, null, trailColor * trailAlpha, i * 0.12f, sparkleTex.Size() / 2f, trailScale, SpriteEffects.None, 0f);
            }
            
            // Orbiting glyphs - quantum symbols
            for (int i = 0; i < 6; i++)
            {
                float angle = Main.GameUpdateCount * 0.05f + MathHelper.TwoPi * i / 6f;
                float radius = 22f + (float)Math.Sin(Main.GameUpdateCount * 0.07f + i) * 7f;
                Vector2 glyphPos = drawPos + angle.ToRotationVector2() * radius;
                Color glyphColor = Color.Lerp(EnigmaDeepPurple, EnigmaGreenFlame, (float)i / 6f) * 0.6f;
                spriteBatch.Draw(glyphTex, glyphPos, null, glyphColor, angle * 2f, glyphTex.Size() / 2f, 0.16f, SpriteEffects.None, 0f);
            }
            
            // Orbiting sparkles
            for (int i = 0; i < 4; i++)
            {
                float angle = Main.GameUpdateCount * 0.08f + MathHelper.TwoPi * i / 4f;
                float radius = 14f;
                Vector2 sparkPos = drawPos + angle.ToRotationVector2() * radius;
                spriteBatch.Draw(sparkleTex, sparkPos, null, EnigmaPurple * 0.55f, angle * 1.5f, sparkleTex.Size() / 2f, 0.12f, SpriteEffects.None, 0f);
            }
            
            // Central watching eye - quantum observer
            spriteBatch.Draw(eyeTex, drawPos, null, EnigmaPurple * 0.85f, Main.GameUpdateCount * 0.02f, eyeTex.Size() / 2f, 0.35f, SpriteEffects.None, 0f);
            
            // Quantum core
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaDeepPurple * 0.8f, Main.GameUpdateCount * 0.04f, flareTex.Size() / 2f, 0.5f, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaPurple * 0.7f, -Main.GameUpdateCount * 0.05f, flareTex.Size() / 2f, 0.32f, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreenFlame * 0.55f, 0f, flareTex.Size() / 2f, 0.15f, SpriteEffects.None, 0f);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // Update ghost positions for superposition effect
            if (Main.GameUpdateCount % 5 == 0)
            {
                ghostPositions[ghostIndex] = Projectile.Center;
                ghostIndex = (ghostIndex + 1) % 4;
            }
            
            // Main trail with quantum uncertainty glitching
            if (Main.GameUpdateCount % 2 == 0)
            {
                // Core trail
                var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.15f, 
                    EnigmaPurple * 0.7f, 0.4f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
                
                // Glitching ghost trails at offset positions
                for (int i = 0; i < 3; i++)
                {
                    Vector2 glitchOffset = Main.rand.NextVector2Circular(15f, 15f);
                    Color glitchColor = GetEnigmaGradient(Main.rand.NextFloat()) * 0.35f;
                    CustomParticles.GenericFlare(Projectile.Center + glitchOffset, glitchColor, 0.25f, 8);
                }
            }
            
            // Draw ghost images at previous positions (superposition visualization)
            if (Main.GameUpdateCount % 3 == 0)
            {
                foreach (Vector2 ghostPos in ghostPositions)
                {
                    if (ghostPos != Vector2.Zero)
                    {
                        Color ghostColor = EnigmaPurple * 0.2f;
                        CustomParticles.GenericFlare(ghostPos, ghostColor, 0.2f, 6);
                    }
                }
            }
            
            // Periodic uncertainty flicker
            if (Main.GameUpdateCount % 10 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, EnigmaGreen, 0.5f, 12);
                CustomParticles.Glyph(Projectile.Center, EnigmaPurple, 0.2f, -1);
            }
            
            // Sparkle trail showing uncertainty
            if (Main.GameUpdateCount % 25 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, EnigmaPurple, 0.45f, 15);
                CustomParticles.HaloRing(Projectile.Center, EnigmaGreen * 0.7f, 0.25f, 12);
                var trailSparkle = new GenericGlowParticle(Projectile.Center, -Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f, GetEnigmaGradient(Main.rand.NextFloat()), 0.3f, 18, true);
                MagnumParticleHandler.SpawnParticle(trailSparkle);
            }
            
            // Music notes in trail - each note a possible future
            if (Main.GameUpdateCount % 8 == 0)
            {
                ThemedParticles.MusicNotes(Projectile.Center, GetEnigmaGradient(Main.rand.NextFloat()), 1, 10f);
            }
            
            Lighting.AddLight(Projectile.Center, GetEnigmaGradient(0.5f).ToVector3() * 0.5f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // === QUANTUM COLLAPSE REALITY WARP ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 5f, 18);
            FateRealityDistortion.TriggerInversionPulse(6);
            
            // === NEW UNIFIED VFX EXPLOSION ===
            UnifiedVFX.EnigmaVariations.Explosion(target.Center, 1.5f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
            
            // Collapse the wavefunction!
            CollapseWavefunction(target.Center);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === QUANTUM COLLAPSE ON DEATH ===
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, 5f, 18);
            FateRealityDistortion.TriggerInversionPulse(6);
            
            // Also collapse if it hits a tile
            CollapseWavefunction(Projectile.Center);
        }
        
        private void CollapseWavefunction(Vector2 position)
        {
            // Play collapse sound
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f, Volume = 0.9f }, position);
            
            // Eyes watching the quantum collapse
            CustomParticles.EnigmaEyeExplosion(position, EnigmaPurple, 5, 3.5f);
            CustomParticles.EnigmaEyeFormation(position, EnigmaGreen, count: 3, radius: 45f);
            
            // Visual wavefunction collapse - multiple possible states becoming one
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 offset = angle.ToRotationVector2() * 40f;
                Color collapseColor = GetEnigmaGradient((float)i / 12f);
                CustomParticles.GenericFlare(position + offset, collapseColor, 0.5f, 15);
            }
            
            // Sparkle formation around collapse
            for (int sparkIdx = 0; sparkIdx < 4; sparkIdx++)
            {
                float formAngle = MathHelper.TwoPi * sparkIdx / 4f + Main.GameUpdateCount * 0.02f;
                Vector2 formPos = position + formAngle.ToRotationVector2() * 50f;
                CustomParticles.GenericFlare(formPos, EnigmaGreen, 0.5f, 18);
                CustomParticles.HaloRing(formPos, EnigmaPurple * 0.6f, 0.25f, 14);
            }
            
            // Glyph circle showing quantum state
            CustomParticles.GlyphCircle(position, EnigmaPurple, count: 6, radius: 40f, rotationSpeed: 0.1f);
            
            // Random collapse outcome
            int outcome = Main.rand.Next(4);
            
            switch (outcome)
            {
                case 0: // Fractal Split - splits into 6 homing fragments
                    CreateFractalSplit(position);
                    break;
                case 1: // Void Singularity - creates pulling black hole
                    CreateVoidSingularity(position);
                    break;
                case 2: // Chain Cascade - lightning chains between enemies
                    CreateChainCascade(position);
                    break;
                case 3: // Paradox Nova - massive AOE explosion
                    CreateParadoxNova(position);
                    break;
            }
        }
        
        private void CreateFractalSplit(Vector2 position)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.7f }, position);
            
            // Spawn 6 homing fragments
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * 10f;
                
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, vel,
                    ModContent.ProjectileType<UncertaintyFragment>(), Projectile.damage / 2, 2f, Projectile.owner);
                
                Color fragmentColor = GetEnigmaGradient((float)i / 6f);
                CustomParticles.GenericFlare(position + vel.SafeNormalize(Vector2.Zero) * 20f, fragmentColor, 0.6f, 15);
            }
            
            CustomParticles.HaloRing(position, EnigmaGreen, 0.7f, 18);
        }
        
        private void CreateVoidSingularity(Vector2 position)
        {
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = -0.5f, Volume = 0.8f }, position);
            
            // Spawn black hole projectile
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, Vector2.Zero,
                ModContent.ProjectileType<QuantumSingularity>(), Projectile.damage, 0f, Projectile.owner);
            
            CustomParticles.GenericFlare(position, EnigmaBlack, 1f, 20);
            CustomParticles.HaloRing(position, EnigmaPurple, 0.6f, 15);
        }
        
        private void CreateChainCascade(Vector2 position)
        {
            SoundEngine.PlaySound(SoundID.Item94 with { Pitch = 0.2f, Volume = 0.8f }, position);
            
            // Find and chain between enemies
            List<NPC> targets = new List<NPC>();
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.friendly && Vector2.Distance(npc.Center, position) < 400f)
                    targets.Add(npc);
            }
            
            if (targets.Count == 0)
            {
                // No targets - just create a lightning burst
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 endPos = position + angle.ToRotationVector2() * 100f;
                    MagnumVFX.DrawEnigmaLightning(position, endPos, 8, 20f, 3, 0.4f);
                }
            }
            else
            {
                // Chain lightning between targets
                Vector2 lastPos = position;
                int chainCount = Math.Min(targets.Count, 6);
                targets.Sort((a, b) => Vector2.Distance(a.Center, position).CompareTo(Vector2.Distance(b.Center, position)));
                
                for (int i = 0; i < chainCount; i++)
                {
                    NPC target = targets[i];
                    
                    // Draw lightning
                    MagnumVFX.DrawEnigmaLightning(lastPos, target.Center, 10, 30f, 4, 0.5f);
                    
                    // Deal damage
                    target.SimpleStrikeNPC(Projectile.damage / 2, 0, false, 0f, null, false, 0f, true);
                    target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
                    
                    // VFX at each target
                    CustomParticles.GenericFlare(target.Center, EnigmaGreen, 0.6f, 15);
                    // Sparkle burst above target
                    CustomParticles.GenericFlare(target.Center - new Vector2(0, 25f), EnigmaPurple, 0.5f, 16);
                    CustomParticles.HaloRing(target.Center - new Vector2(0, 25f), EnigmaGreen * 0.7f, 0.3f, 14);
                    
                    lastPos = target.Center;
                }
            }
            
            CustomParticles.HaloRing(position, EnigmaGreen, 0.5f, 15);
        }
        
        private void CreateParadoxNova(Vector2 position)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.2f, Volume = 1f }, position);
            
            // Massive explosion projectile
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, Vector2.Zero,
                ModContent.ProjectileType<ParadoxNovaExplosion>(), (int)(Projectile.damage * 1.5f), 8f, Projectile.owner);
            
            // Massive visual effect
            for (int layer = 0; layer < 4; layer++)
            {
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f + layer * 0.15f;
                    float radius = 30f + layer * 25f;
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    Color novaColor = GetEnigmaGradient((float)(layer * 12 + i) / 48f);
                    CustomParticles.GenericFlare(position + offset, novaColor, 0.6f - layer * 0.1f, 20);
                }
            }
            
            // Multiple halo rings
            for (int ring = 0; ring < 5; ring++)
            {
                Color ringColor = GetEnigmaGradient(ring / 5f);
                CustomParticles.HaloRing(position, ringColor, 0.8f - ring * 0.1f, 18 + ring * 3);
            }
            
            // Sparkles radiating from destruction
            for (int expIdx = 0; expIdx < 6; expIdx++)
            {
                float expAngle = MathHelper.TwoPi * expIdx / 6f;
                Vector2 expVel = expAngle.ToRotationVector2() * 5f;
                var expSparkle = new GenericGlowParticle(position, expVel, GetEnigmaGradient((float)expIdx / 6f), 0.45f, 22, true);
                MagnumParticleHandler.SpawnParticle(expSparkle);
                CustomParticles.GenericFlare(position + expVel * 3f, EnigmaGreen, 0.4f, 15);
            }
            
            // Glyph explosion
            CustomParticles.GlyphBurst(position, EnigmaPurple, count: 10, speed: 5f);
        }
    }
    
    public class UncertaintyFragment : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Switch to additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
            Texture2D sparkleTex = CustomParticleSystem.RandomPrismaticSparkle().Value;
            Texture2D flareTex = CustomParticleSystem.EnergyFlares[0].Value;
            
            // Eerie sparkle trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailProgress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress) * 0.5f;
                float trailScale = (1f - trailProgress * 0.4f) * 0.12f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                Color trailColor = Color.Lerp(EnigmaGreenFlame, EnigmaPurple, trailProgress);
                spriteBatch.Draw(sparkleTex, trailPos, null, trailColor * trailAlpha, i * 0.15f, sparkleTex.Size() / 2f, trailScale, SpriteEffects.None, 0f);
            }
            
            // Small orbiting glyphs
            for (int i = 0; i < 3; i++)
            {
                float angle = Main.GameUpdateCount * 0.1f + MathHelper.TwoPi * i / 3f;
                float radius = 10f + (float)Math.Sin(Main.GameUpdateCount * 0.12f + i) * 3f;
                Vector2 glyphPos = drawPos + angle.ToRotationVector2() * radius;
                Color glyphColor = Color.Lerp(EnigmaDeepPurple, EnigmaGreenFlame, (float)i / 3f) * 0.55f;
                spriteBatch.Draw(glyphTex, glyphPos, null, glyphColor, angle * 2f, glyphTex.Size() / 2f, 0.1f, SpriteEffects.None, 0f);
            }
            
            // Orbiting sparkles
            for (int i = 0; i < 2; i++)
            {
                float angle = Main.GameUpdateCount * 0.12f + MathHelper.Pi * i;
                float radius = 6f;
                Vector2 sparkPos = drawPos + angle.ToRotationVector2() * radius;
                spriteBatch.Draw(sparkleTex, sparkPos, null, EnigmaPurple * 0.5f, angle * 1.5f, sparkleTex.Size() / 2f, 0.08f, SpriteEffects.None, 0f);
            }
            
            // Fragment core
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaDeepPurple * 0.8f, Main.GameUpdateCount * 0.05f, flareTex.Size() / 2f, 0.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaPurple * 0.65f, -Main.GameUpdateCount * 0.06f, flareTex.Size() / 2f, 0.18f, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreenFlame * 0.5f, 0f, flareTex.Size() / 2f, 0.08f, SpriteEffects.None, 0f);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // Home towards nearest enemy
            NPC target = FindClosestEnemy(350f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 14f, 0.08f);
            }
            
            // Trail
            if (Main.GameUpdateCount % 2 == 0)
            {
                float progress = (120 - Projectile.timeLeft) / 120f;
                Color trailColor = GetEnigmaGradient(progress);
                var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f, 
                    trailColor * 0.6f, 0.25f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.3f);
        }
        
        private NPC FindClosestEnemy(float range)
        {
            NPC closest = null;
            float closestDist = range;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 240);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            // === REALITY WARP DISTORTION ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 3f, 10);
            
            // === NEW UNIFIED VFX HIT EFFECT ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.2f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            CustomParticles.GenericFlare(target.Center, EnigmaPurple, 0.5f, 12);
            // Sparkle above target
            CustomParticles.GenericFlare(target.Center - new Vector2(0, 20f), EnigmaGreen, 0.4f, 14);
            CustomParticles.HaloRing(target.Center - new Vector2(0, 20f), EnigmaPurple * 0.6f, 0.22f, 12);
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === REALITY WARP ON DEATH ===
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, 3.5f, 12);
            
            // Eye watching the fragment's demise
            CustomParticles.EnigmaEyeGaze(Projectile.Center, EnigmaPurple, 0.4f);
            
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * 2.5f;
                Color burstColor = GetEnigmaGradient((float)i / 6f);
                var glow = new GenericGlowParticle(Projectile.Center, vel, burstColor, 0.3f, 15, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }
    }
    
    public class QuantumSingularity : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private const float PullRadius = 200f;
        private const float PullStrength = 8f;
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float lifeProgress = 1f - (Projectile.timeLeft / 90f);
            float intensity = (float)Math.Sin(lifeProgress * MathHelper.Pi);
            float pulse = 0.8f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.2f;
            
            // Switch to additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D eyeTex = CustomParticleSystem.EnigmaEyes[((int)(Main.GameUpdateCount * 0.06f)) % 8].Value;
            Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
            Texture2D sparkleTex = CustomParticleSystem.RandomPrismaticSparkle().Value;
            Texture2D flareTex = CustomParticleSystem.EnergyFlares[0].Value;
            
            // Outer ring of swirling glyphs - event horizon
            for (int i = 0; i < 8; i++)
            {
                float angle = Main.GameUpdateCount * 0.08f + MathHelper.TwoPi * i / 8f;
                float radius = 45f * pulse * intensity;
                Vector2 glyphPos = drawPos + angle.ToRotationVector2() * radius;
                Color glyphColor = Color.Lerp(EnigmaDeepPurple, EnigmaGreenFlame, (float)i / 8f) * 0.6f * intensity;
                spriteBatch.Draw(glyphTex, glyphPos, null, glyphColor, angle * 2f, glyphTex.Size() / 2f, 0.2f * pulse, SpriteEffects.None, 0f);
            }
            
            // Inner ring of sparkles spiraling in
            for (int i = 0; i < 5; i++)
            {
                float angle = -Main.GameUpdateCount * 0.12f + MathHelper.TwoPi * i / 5f;
                float radius = 25f * pulse * intensity;
                Vector2 sparkPos = drawPos + angle.ToRotationVector2() * radius;
                spriteBatch.Draw(sparkleTex, sparkPos, null, EnigmaPurple * 0.55f * intensity, angle * 1.5f, sparkleTex.Size() / 2f, 0.14f, SpriteEffects.None, 0f);
            }
            
            // Central void eye - the singularity observes
            spriteBatch.Draw(eyeTex, drawPos, null, EnigmaPurple * 0.9f * intensity, Main.GameUpdateCount * 0.02f, eyeTex.Size() / 2f, 0.5f * pulse, SpriteEffects.None, 0f);
            
            // Void core - deep black hole
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaDeepPurple * 0.75f * intensity, Main.GameUpdateCount * 0.03f, flareTex.Size() / 2f, 0.7f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaPurple * 0.6f * intensity, -Main.GameUpdateCount * 0.04f, flareTex.Size() / 2f, 0.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreenFlame * 0.5f * intensity, 0f, flareTex.Size() / 2f, 0.2f * pulse, SpriteEffects.None, 0f);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 90f);
            float intensity = (float)Math.Sin(lifeProgress * MathHelper.Pi);
            
            // Swirling vortex particles pulling inward
            if (Main.GameUpdateCount % 2 == 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = Main.GameUpdateCount * 0.2f + MathHelper.TwoPi * i / 8f;
                    float radius = PullRadius * (0.3f + Main.rand.NextFloat() * 0.7f) * intensity;
                    Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * radius;
                    Vector2 vel = (Projectile.Center - particlePos).SafeNormalize(Vector2.Zero) * 5f;
                    
                    Color spiralColor = GetEnigmaGradient((float)i / 8f);
                    var glow = new GenericGlowParticle(particlePos, vel, spiralColor * intensity, 
                        0.35f, 15, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }
            
            // Central void
            if (Main.GameUpdateCount % 4 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, EnigmaBlack, 0.8f * intensity, 15);
                CustomParticles.HaloRing(Projectile.Center, EnigmaPurple * intensity, 0.4f, 12);
            }
            
            // Orbiting sparkles in the void
            if (Main.GameUpdateCount % 20 == 0)
            {
                for (int orbitIdx = 0; orbitIdx < 3; orbitIdx++)
                {
                    float orbitAngle = Main.GameUpdateCount * 0.08f + MathHelper.TwoPi * orbitIdx / 3f;
                    Vector2 orbitPos = Projectile.Center + orbitAngle.ToRotationVector2() * 35f;
                    CustomParticles.GenericFlare(orbitPos, EnigmaGreen * intensity, 0.4f, 16);
                    CustomParticles.HaloRing(orbitPos, EnigmaPurple * intensity * 0.5f, 0.2f, 12);
                }
            }
            
            // Glyphs orbiting the singularity
            if (Main.GameUpdateCount % 12 == 0)
            {
                CustomParticles.GlyphAura(Projectile.Center, EnigmaPurple * intensity, radius: PullRadius * 0.4f, count: 2);
            }
            
            // Pull enemies
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.knockBackResist <= 0f) continue;
                
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist <= PullRadius && dist > 20f)
                {
                    Vector2 pullDir = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                    float pullMag = PullStrength * intensity * (1f - dist / PullRadius) * npc.knockBackResist;
                    npc.velocity += pullDir * pullMag * 0.3f;
                }
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * intensity * 0.6f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 360);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 2);
            
            // === SINGULARITY REALITY WARP ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 4f, 15);
            
            // === NEW UNIFIED VFX HIT EFFECT ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.2f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            // Sparkle burst above target
            CustomParticles.GenericFlare(target.Center - new Vector2(0, 25f), EnigmaPurple, 0.55f, 18);
            CustomParticles.HaloRing(target.Center - new Vector2(0, 25f), EnigmaGreen * 0.7f, 0.3f, 14);
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === SINGULARITY COLLAPSE REALITY WARP ===
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, 5f, 18);
            FateRealityDistortion.TriggerInversionPulse(6);
            
            // Eyes watching the implosion
            CustomParticles.EnigmaEyeExplosion(Projectile.Center, EnigmaPurple, 4, 3f);
            
            // Implosion then explosion
            for (int ring = 0; ring < 5; ring++)
            {
                Color ringColor = GetEnigmaGradient(ring / 5f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.6f - ring * 0.08f, 15 + ring * 3);
            }
            
            // Sparkle explosion burst
            for (int killIdx = 0; killIdx < 5; killIdx++)
            {
                float killAngle = MathHelper.TwoPi * killIdx / 5f;
                Vector2 killVel = killAngle.ToRotationVector2() * 4f;
                var killSparkle = new GenericGlowParticle(Projectile.Center, killVel, GetEnigmaGradient((float)killIdx / 5f), 0.4f, 20, true);
                MagnumParticleHandler.SpawnParticle(killSparkle);
            }
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaPurple, count: 8, speed: 4f);
            
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.4f, Volume = 0.7f }, Projectile.Center);
        }
    }
    
    public class ParadoxNovaExplosion : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private const float ExplosionRadius = 180f;
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float lifeProgress = 1f - (Projectile.timeLeft / 30f);
            float intensity = 1f - lifeProgress;
            float expandProgress = lifeProgress;
            float scale = 1f + expandProgress * 3f;
            
            // Switch to additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D eyeTex = CustomParticleSystem.EnigmaEyes[((int)(Main.GameUpdateCount * 0.1f)) % 8].Value;
            Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
            Texture2D sparkleTex = CustomParticleSystem.RandomPrismaticSparkle().Value;
            Texture2D flareTex = CustomParticleSystem.EnergyFlares[0].Value;
            
            // Expanding ring of glyphs - nova shockwave
            for (int i = 0; i < 12; i++)
            {
                float angle = Main.GameUpdateCount * 0.05f + MathHelper.TwoPi * i / 12f;
                float radius = 60f * scale;
                Vector2 glyphPos = drawPos + angle.ToRotationVector2() * radius;
                Color glyphColor = Color.Lerp(EnigmaDeepPurple, EnigmaGreenFlame, (float)i / 12f) * 0.5f * intensity;
                spriteBatch.Draw(glyphTex, glyphPos, null, glyphColor, angle * 2f, glyphTex.Size() / 2f, 0.25f * (1f - expandProgress * 0.5f), SpriteEffects.None, 0f);
            }
            
            // Inner sparkle ring
            for (int i = 0; i < 8; i++)
            {
                float angle = -Main.GameUpdateCount * 0.08f + MathHelper.TwoPi * i / 8f;
                float radius = 35f * scale;
                Vector2 sparkPos = drawPos + angle.ToRotationVector2() * radius;
                spriteBatch.Draw(sparkleTex, sparkPos, null, EnigmaPurple * 0.6f * intensity, angle * 1.5f, sparkleTex.Size() / 2f, 0.18f, SpriteEffects.None, 0f);
            }
            
            // Central paradox eye - expanding with the nova
            spriteBatch.Draw(eyeTex, drawPos, null, EnigmaPurple * 0.8f * intensity, Main.GameUpdateCount * 0.03f, eyeTex.Size() / 2f, 0.6f * scale * 0.5f, SpriteEffects.None, 0f);
            
            // Nova core - expanding explosion
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaDeepPurple * 0.6f * intensity, Main.GameUpdateCount * 0.04f, flareTex.Size() / 2f, scale * 0.8f, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaPurple * 0.5f * intensity, -Main.GameUpdateCount * 0.05f, flareTex.Size() / 2f, scale * 0.5f, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreenFlame * 0.4f * intensity, 0f, flareTex.Size() / 2f, scale * 0.25f, SpriteEffects.None, 0f);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 180;
            Projectile.height = 180;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 30f);
            float intensity = 1f - lifeProgress; // Fades out
            float expandProgress = lifeProgress; // Expands outward
            
            // Expanding ring of destruction
            if (Main.GameUpdateCount % 2 == 0)
            {
                float currentRadius = ExplosionRadius * expandProgress;
                int points = 16;
                
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points + Main.GameUpdateCount * 0.1f;
                    Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * currentRadius;
                    
                    Color particleColor = GetEnigmaGradient((float)i / points) * intensity;
                    CustomParticles.GenericFlare(particlePos, particleColor, 0.5f * intensity, 12);
                }
            }
            
            // Central pulsing
            if (Main.GameUpdateCount % 3 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, EnigmaGreen * intensity, 0.7f * intensity, 10);
            }
            
            Lighting.AddLight(Projectile.Center, GetEnigmaGradient(0.5f).ToVector3() * intensity);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 420);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 3);
            
            // === PARADOX NOVA REALITY WARP ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 4f, 15);
            
            // === NEW UNIFIED VFX HIT EFFECT ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.2f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            CustomParticles.GenericFlare(target.Center, EnigmaPurple, 0.6f, 15);
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            CustomParticles.GlyphStack(target.Center + new Vector2(0, -20f), EnigmaGreen, brandNPC.paradoxStacks, 0.22f);
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
    }
}
