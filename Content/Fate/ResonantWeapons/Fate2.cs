using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.ResonantWeapons
{
    /// <summary>
    /// Cannon of Inevitability - Ultimate cosmic gun
    /// Bullets exist in multiple timelines, all converging on target
    /// DARK PRISMATIC: Black → Dark Pink → Bright Red
    /// </summary>
    public class Fate2 : ModItem
    {
        // DARK PRISMATIC COLOR PALETTE
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.SDMG;
        
        private Color GetFateGradient(float progress)
        {
            if (progress < 0.4f)
                return Color.Lerp(FateBlack, FateDarkPink, progress / 0.4f);
            else if (progress < 0.8f)
                return Color.Lerp(FateDarkPink, FateBrightRed, (progress - 0.4f) / 0.4f);
            else
                return Color.Lerp(FateBrightRed, FateWhite, (progress - 0.8f) / 0.2f);
        }
        
        public override void SetDefaults()
        {
            Item.damage = 290;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 50;
            Item.height = 24;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 28);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 20f;
            Item.useAmmo = AmmoID.Bullet;
            Item.noMelee = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Fire bullets that exist across multiple timelines"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "All timelines converge on your target for devastating damage"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'Escape is not within the realm of possibility'") 
            { 
                OverrideColor = FateBrightRed 
            });
        }
        
        public override void HoldItem(Player player)
        {
            // Dark prismatic ambient aura
            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Main.rand.NextFloat(25f, 50f);
                Vector2 flarePos = player.Center + angle.ToRotationVector2() * radius;
                float progress = Main.rand.NextFloat();
                Color auraColor = GetFateGradient(progress) * 0.55f;
                CustomParticles.GenericFlare(flarePos, auraColor, 0.26f, 15);
            }
            
            // Timeline shimmer at bowstring
            if (Main.rand.NextBool(10))
            {
                Vector2 stringPos = player.Center + new Vector2(player.direction * 15f, 0);
                for (int i = 0; i < 3; i++)
                {
                    Vector2 offset = new Vector2(i * 3 - 3, i * 2 - 2);
                    CustomParticles.GenericFlare(stringPos + offset, GetFateGradient((float)i / 3f) * 0.4f, 0.18f, 10);
                }
            }
            
            Lighting.AddLight(player.Center, FateDarkPink.ToVector3() * 0.3f);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spawn main bullet + 4 timeline ghost bullets
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);
            
            // Main bullet
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<InevitabilityBolt>(), damage, knockback, player.whoAmI, 0);
            
            // Timeline ghost bullets (slightly offset trajectories that converge)
            for (int i = 0; i < 4; i++)
            {
                float offsetAngle = MathHelper.ToRadians(12f) * (i - 1.5f);
                Vector2 ghostVel = velocity.RotatedBy(offsetAngle) * 0.85f;
                Projectile.NewProjectile(source, position + Main.rand.NextVector2Circular(10f, 10f), ghostVel, 
                    ModContent.ProjectileType<TimelineGhostBolt>(), damage / 2, knockback / 2, player.whoAmI, i + 1);
            }
            
            // Muzzle flash with temporal distortion
            Vector2 muzzlePos = position + direction * 35f;
            
            // Multi-timeline muzzle effect
            for (int timeline = 0; timeline < 5; timeline++)
            {
                Vector2 timelineOffset = new Vector2((timeline - 2) * 4, (timeline - 2) * 2);
                float progress = (float)timeline / 5f;
                Color timeColor = GetFateGradient(progress) * (0.8f - timeline * 0.12f);
                CustomParticles.GenericFlare(muzzlePos + timelineOffset, timeColor, 0.45f - timeline * 0.06f, 14);
            }
            
            CustomParticles.HaloRing(muzzlePos, FateDarkPink * 0.7f, 0.4f, 14);
            CustomParticles.GlyphBurst(muzzlePos, FatePurple, 5, 4f);
            
            // Chromatic aberration at fire
            CustomParticles.GenericFlare(muzzlePos + new Vector2(-3, 0), FateBrightRed * 0.4f, 0.3f, 10);
            CustomParticles.GenericFlare(muzzlePos + new Vector2(3, 0), FatePurple * 0.4f, 0.3f, 10);
            
            // Cosmic music notes at arrow release - timeline echoes in musical form!
            ThemedParticles.FateMusicNotes(muzzlePos, 6, 35f);
            
            Lighting.AddLight(muzzlePos, FateBrightRed.ToVector3() * 0.7f);
            
            return false;
        }
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 1f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Dark prismatic glow with timeline echoes
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = new Vector2((i - 1) * 2, (i - 1) * 1);
                Color echoColor = GetFateGradient((float)i / 3f) * (0.3f - i * 0.08f);
                spriteBatch.Draw(texture, position + offset, null, echoColor, rotation, origin, scale * pulse * (1.25f - i * 0.08f), SpriteEffects.None, 0f);
            }
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            spriteBatch.Draw(texture, position, null, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);
            
            Lighting.AddLight(Item.Center, FateDarkPink.ToVector3() * 0.35f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Main Bullet - Inevitability Bolt
    /// COSMIC SPARKLE PROJECTILE with astral flares and dazzling trails
    /// </summary>
    public class InevitabilityBolt : ModProjectile
    {
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        private List<int> hitTargets = new List<int>();
        
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        private Color GetFateGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(FateBlack, FateDarkPink, progress * 2f);
            else
                return Color.Lerp(FateDarkPink, FateBrightRed, (progress - 0.5f) * 2f);
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // === HEAVY COSMIC DUST TRAIL ===
            for (int d = 0; d < 3; d++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), 
                    DustID.PinkTorch, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f));
                dust.noGravity = true;
                dust.scale = 1.3f + Main.rand.NextFloat(0.5f);
            }
            
            // Red sparkle dust
            if (Main.rand.NextBool(2))
            {
                Dust redDust = Dust.NewDustPerfect(Projectile.Center, DustID.RedTorch, 
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f));
                redDust.noGravity = true;
                redDust.scale = 1.1f + Main.rand.NextFloat(0.3f);
            }
            
            // White sparkle dust for brilliance
            if (Main.rand.NextBool(3))
            {
                Dust whiteDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), 
                    DustID.WhiteTorch, -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(1f, 1f));
                whiteDust.noGravity = true;
                whiteDust.scale = 0.9f + Main.rand.NextFloat(0.3f);
            }
            
            // Particle flares
            float lifeProgress = 1f - (Projectile.timeLeft / 300f);
            for (int i = 0; i < 2; i++)
            {
                float progress = (lifeProgress + i * 0.2f) % 1f;
                Color trailColor = GetFateGradient(progress);
                Vector2 offset = Main.rand.NextVector2Circular(5f, 5f);
                CustomParticles.GenericFlare(Projectile.Center + offset, trailColor * 0.6f, 0.32f, 12);
            }
            
            // Chromatic aberration trail flares
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(-2, 0), FateBrightRed * 0.3f, 0.2f, 8);
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(2, 0), FatePurple * 0.3f, 0.2f, 8);
            
            // Glyph trail
            if (Main.rand.NextBool(10))
            {
                CustomParticles.GlyphTrail(Projectile.Center, Projectile.velocity, FateDarkPink * 0.5f, 0.2f);
            }
            
            // Light homing toward enemies
            float homingRange = 300f;
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
                Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), targetDir, 0.05f) * Projectile.velocity.Length();
            }
            
            Lighting.AddLight(Projectile.Center, FateDarkPink.ToVector3() * 0.6f);
            Lighting.AddLight(Projectile.Center, FateBrightRed.ToVector3() * 0.3f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            target.GetGlobalNPC<DestinyCollapseNPC>().AddStack(target, 1);
            
            hitTargets.Add(target.whoAmI);
            
            // === UNIFIED VFX HIT EFFECT - FATE THEME ===
            UnifiedVFX.Fate.HitEffect(target.Center, 1.2f);
            
            // Temporal collapse explosion on impact
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 offset = angle.ToRotationVector2() * 35f;
                float progress = (float)i / 10f;
                CustomParticles.GenericFlare(target.Center + offset, GetFateGradient(progress), 0.55f, 18);
                
                // RGB separation burst
                CustomParticles.GenericFlare(target.Center + offset + new Vector2(-3, 0), FateBrightRed * 0.4f, 0.28f, 12);
                CustomParticles.GenericFlare(target.Center + offset + new Vector2(3, 0), FatePurple * 0.4f, 0.28f, 12);
            }
            
            CustomParticles.HaloRing(target.Center, FateDarkPink, 0.55f, 18);
            CustomParticles.HaloRing(target.Center, FateBrightRed * 0.7f, 0.4f, 14);
            CustomParticles.GlyphImpact(target.Center, FateBlack, FateBrightRed, 0.6f);
            
            // Timeline convergence glyph circle
            CustomParticles.GlyphCircle(target.Center, FateDarkPink, 8, 45f, 0.06f);
            
            // === TEMPORAL ECHO AFTERIMAGES ===
            for (int echo = 0; echo < 4; echo++)
            {
                Vector2 echoPos = target.Center + new Vector2(0, -echo * 10f);
                float echoAlpha = 1f - echo * 0.2f;
                CustomParticles.GenericFlare(echoPos, GetFateGradient((float)echo / 4f) * echoAlpha * 0.5f, 0.4f, 15);
            }
            
            // Cosmic music notes!
            ThemedParticles.FateMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.FateMusicNotes(target.Center, 6, 35f);
            
            // Cosmic Revisit - delayed cosmic flare that strikes again
            int revisitDamage = (int)(damageDone * 0.25f);
            target.GetGlobalNPC<DestinyCollapseNPC>().QueueCosmicRevisit(target, revisitDamage, 25, Projectile.Center, 0.8f);
            
            Lighting.AddLight(target.Center, FateBrightRed.ToVector3() * 0.9f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Death burst sparkles
            for (int i = 0; i < 10; i++)
            {
                float progress = (float)i / 10f;
                Vector2 burstVel = (MathHelper.TwoPi * i / 10f).ToRotationVector2() * 4f;
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), GetFateGradient(progress), 0.45f, 16);
                
                // Dust sparkle burst
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch, burstVel);
                dust.noGravity = true;
                dust.scale = 1.2f;
            }
            CustomParticles.HaloRing(Projectile.Center, FateDarkPink * 0.7f, 0.4f, 14);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Load sparkle textures for cosmic dazzling trail
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D sparkleTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle" + (1 + (Projectile.whoAmI % 10))).Value;
            
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 flareOrigin = flareTex.Size() / 2f;
            Vector2 sparkleOrigin = sparkleTex.Size() / 2f;
            
            // Switch to additive blending for maximum sparkle
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // === COSMIC SPARKLE TRAIL with chromatic aberration ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailProgress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress) * 0.9f;
                float trailScale = (1f - trailProgress * 0.4f) * 0.5f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // Chromatic aberration - RGB separation for cosmic effect
                float chromaticOffset = 3f + trailProgress * 2f;
                spriteBatch.Draw(sparkleTex, trailPos + new Vector2(-chromaticOffset, 0), null, FateBrightRed * trailAlpha * 0.6f, Main.GameUpdateCount * 0.12f + i * 0.4f, sparkleOrigin, trailScale * 0.7f, SpriteEffects.None, 0f);
                spriteBatch.Draw(sparkleTex, trailPos, null, FateDarkPink * trailAlpha, Main.GameUpdateCount * 0.15f + i * 0.5f, sparkleOrigin, trailScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(sparkleTex, trailPos + new Vector2(chromaticOffset, 0), null, FatePurple * trailAlpha * 0.6f, Main.GameUpdateCount * 0.12f + i * 0.4f, sparkleOrigin, trailScale * 0.7f, SpriteEffects.None, 0f);
                
                // Core flare at each trail position
                if (i % 2 == 0)
                {
                    Color coreColor = GetFateGradient(trailProgress);
                    spriteBatch.Draw(flareTex, trailPos, null, coreColor * trailAlpha * 0.5f, 0f, flareOrigin, trailScale * 0.5f, SpriteEffects.None, 0f);
                }
            }
            
            // === COSMIC CORE FLARE with pulsing ===
            float pulse = 0.9f + MathF.Sin(Main.GameUpdateCount * 0.2f) * 0.2f;
            
            // Chromatic aberration on core
            spriteBatch.Draw(flareTex, drawPos + new Vector2(-4, 0), null, FateBrightRed * 0.6f, 0f, flareOrigin, 0.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos + new Vector2(4, 0), null, FatePurple * 0.6f, 0f, flareOrigin, 0.4f * pulse, SpriteEffects.None, 0f);
            
            // Main core layers
            spriteBatch.Draw(flareTex, drawPos, null, FateDarkPink * 0.95f, 0f, flareOrigin, 0.55f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, FateBrightRed * 0.8f, 0f, flareOrigin, 0.38f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, FateWhite * 0.95f, 0f, flareOrigin, 0.2f * pulse, SpriteEffects.None, 0f);
            
            // === ORBITING SPARKLES - cosmic stars ===
            for (int i = 0; i < 6; i++)
            {
                float angle = Main.GameUpdateCount * 0.12f + i * MathHelper.Pi / 3f;
                float radius = 14f + MathF.Sin(Main.GameUpdateCount * 0.15f + i) * 4f;
                Vector2 sparkleOffset = angle.ToRotationVector2() * radius;
                Color sparkleColor = Color.Lerp(FateDarkPink, FateBrightRed, (float)i / 6f);
                spriteBatch.Draw(sparkleTex, drawPos + sparkleOffset, null, sparkleColor * 0.75f, angle * 2f, sparkleOrigin, 0.25f, SpriteEffects.None, 0f);
            }
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }
    
    /// <summary>
    /// Timeline Ghost Bolt - semi-transparent cosmic sparkle, converges on main bolt's targets
    /// </summary>
    public class TimelineGhostBolt : ModProjectile
    {
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 14;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        private Color GetFateGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(FateBlack, FateDarkPink, progress * 2f);
            else
                return Color.Lerp(FateDarkPink, FateBrightRed, (progress - 0.5f) * 2f);
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.alpha = 128; // Semi-transparent
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Ghostly sparkle dust trail
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), 
                    DustID.PinkTorch, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f));
                dust.noGravity = true;
                dust.scale = 0.9f + Main.rand.NextFloat(0.3f);
                dust.alpha = 100;
            }
            
            float timelineIndex = Projectile.ai[0];
            float progress = (timelineIndex / 5f + Main.GameUpdateCount * 0.02f) % 1f;
            Color trailColor = GetFateGradient(progress) * 0.6f;
            
            if (Main.rand.NextBool(3))
            {
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), trailColor, 0.24f, 10);
            }
            
            // Strong homing - these converge on targets
            float homingRange = 400f;
            float homingStrength = 0.08f;
            
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
                Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), targetDir, homingStrength) * Projectile.velocity.Length();
                
                // Speed up as we approach
                if (closestDist < 150f)
                    Projectile.velocity *= 1.02f;
            }
            
            Lighting.AddLight(Projectile.Center, FateDarkPink.ToVector3() * 0.25f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);
            target.GetGlobalNPC<DestinyCollapseNPC>().AddStack(target, 1);
            
            // === UNIFIED VFX HIT EFFECT - FATE THEME ===
            UnifiedVFX.Fate.HitEffect(target.Center, 0.9f);
            
            // Timeline convergence effect with sparkles
            float timelineIndex = Projectile.ai[0];
            Color convergenceColor = GetFateGradient(timelineIndex / 5f);
            
            for (int i = 0; i < 6; i++)
            {
                CustomParticles.GenericFlare(target.Center + Main.rand.NextVector2Circular(12f, 12f), convergenceColor * 0.75f, 0.38f, 14);
                
                // Dust burst
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.PinkTorch, 
                    (MathHelper.TwoPi * i / 6f).ToRotationVector2() * 3f);
                dust.noGravity = true;
                dust.scale = 1.1f;
            }
            
            // === CHROMATIC ABERRATION ===
            CustomParticles.GenericFlare(target.Center + new Vector2(-3, 0), FateBrightRed * 0.4f, 0.3f, 12);
            CustomParticles.GenericFlare(target.Center + new Vector2(3, 0), FatePurple * 0.4f, 0.3f, 12);
            
            CustomParticles.Glyph(target.Center, FateDarkPink * 0.6f, 0.32f, -1);
            CustomParticles.HaloRing(target.Center, convergenceColor * 0.6f, 0.35f, 12);
            CustomParticles.GlyphCircle(target.Center, FateDarkPink, 5, 40f, 0.08f);
            
            // === TEMPORAL ECHO AFTERIMAGES ===
            for (int echo = 0; echo < 4; echo++)
            {
                Vector2 echoPos = target.Center + new Vector2(0, -echo * 10f);
                float echoAlpha = 1f - echo * 0.2f;
                CustomParticles.GenericFlare(echoPos, GetFateGradient((float)echo / 4f) * echoAlpha * 0.5f, 0.4f, 15);
            }
            
            // Cosmic music notes!
            ThemedParticles.FateMusicNotes(target.Center, 4, 30f);
            
            // Cosmic Revisit - ghost bolt echoes
            int revisitDamage = (int)(damageDone * 0.20f);
            target.GetGlobalNPC<DestinyCollapseNPC>().QueueCosmicRevisit(target, revisitDamage, 30, Projectile.Center, 0.6f);
            
            Lighting.AddLight(target.Center, FateBrightRed.ToVector3() * 0.9f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D sparkleTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle" + (1 + (Projectile.whoAmI % 8))).Value;
            
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 flareOrigin = flareTex.Size() / 2f;
            Vector2 sparkleOrigin = sparkleTex.Size() / 2f;
            
            float timelineIndex = Projectile.ai[0];
            Color timelineColor = GetFateGradient(timelineIndex / 5f);
            
            // Switch to additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // === GHOSTLY SPARKLE TRAIL with chromatic aberration ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailProgress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress) * 0.5f; // More ghostly
                float trailScale = (1f - trailProgress * 0.4f) * 0.35f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // Chromatic aberration
                float chromaticOffset = 2f + trailProgress * 1.5f;
                spriteBatch.Draw(sparkleTex, trailPos + new Vector2(-chromaticOffset, 0), null, FateBrightRed * trailAlpha * 0.4f, Main.GameUpdateCount * 0.1f + i * 0.3f, sparkleOrigin, trailScale * 0.6f, SpriteEffects.None, 0f);
                spriteBatch.Draw(sparkleTex, trailPos, null, timelineColor * trailAlpha, Main.GameUpdateCount * 0.12f + i * 0.4f, sparkleOrigin, trailScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(sparkleTex, trailPos + new Vector2(chromaticOffset, 0), null, FatePurple * trailAlpha * 0.4f, Main.GameUpdateCount * 0.1f + i * 0.3f, sparkleOrigin, trailScale * 0.6f, SpriteEffects.None, 0f);
            }
            
            // === GHOSTLY CORE - Semi-transparent sparkle ===
            float pulse = 0.8f + MathF.Sin(Main.GameUpdateCount * 0.18f) * 0.15f;
            spriteBatch.Draw(flareTex, drawPos, null, timelineColor * 0.6f, 0f, flareOrigin, 0.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, FateWhite * 0.5f, 0f, flareOrigin, 0.2f * pulse, SpriteEffects.None, 0f);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }
}
