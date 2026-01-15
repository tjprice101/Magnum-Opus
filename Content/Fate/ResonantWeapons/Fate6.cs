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
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.ResonantWeapons
{
    /// <summary>
    /// Fate Rocket Cataclysm - Launcher with reality-shattering explosions
    /// </summary>
    public class Fate6 : ModItem
    {
        // Dark Prismatic color palette
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.Celeb2;
        
        public override void SetDefaults()
        {
            Item.damage = 580;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 70;
            Item.height = 35;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 10f;
            Item.value = Item.sellPrice(gold: 30);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.RocketI;
            Item.shootSpeed = 12f;
            Item.useAmmo = AmmoID.Rocket;
            Item.noMelee = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Launches rockets that create Cataclysmic Collapses"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "Explosions shatter reality and spread Destiny Collapse"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'The universe trembles at its firing'") 
            { 
                OverrideColor = FateDarkPink 
            });
        }
        
        public override Vector2? HoldoutOffset() => new Vector2(-15f, 0f);
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<FateCataclysmRocket>(), damage, knockback, player.whoAmI);
            
            // Dark prismatic massive muzzle flash with chromatic aberration
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 45f;
            CustomParticles.GenericFlare(muzzlePos, FateBlack, 1.0f, 18);
            CustomParticles.GenericFlare(muzzlePos, FateBrightRed, 0.85f, 16);
            CustomParticles.HaloRing(muzzlePos, FateDarkPink, 0.6f, 14);
            
            // Massive chromatic aberration burst
            for (int i = 0; i < 8; i++)
            {
                float angle = velocity.ToRotation() + MathHelper.PiOver2 * (i - 3.5f) * 0.35f;
                Vector2 sparkVel = angle.ToRotationVector2() * 7f;
                float progress = (float)i / 8f;
                
                // RGB separation
                var glowR = new GenericGlowParticle(muzzlePos + new Vector2(-4, 0), sparkVel, Color.Red * 0.45f, 0.38f, 14, true);
                var glowG = new GenericGlowParticle(muzzlePos, sparkVel, FateDarkPink, 0.4f, 14, true);
                var glowB = new GenericGlowParticle(muzzlePos + new Vector2(4, 0), sparkVel, Color.Cyan * 0.45f, 0.38f, 14, true);
                MagnumParticleHandler.SpawnParticle(glowR);
                MagnumParticleHandler.SpawnParticle(glowG);
                MagnumParticleHandler.SpawnParticle(glowB);
            }
            
            // Cosmic glyph tower on rocket launch - reality-breaking launch sigil
            CustomParticles.GlyphTower(muzzlePos, FateBrightRed, 4, 0.5f);
            CustomParticles.GlyphCircle(muzzlePos, FateDarkPink, 6, 40f, 0.04f);
            
            // Rocket launch with cosmic music notes!
            ThemedParticles.FateMusicNotes(muzzlePos, 8, 45f);
            
            return false;
        }
    }
    
    public class FateCataclysmRocket : ModProjectile
    {
        // Dark Prismatic color palette
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private Color GetFateGradient(float progress)
        {
            if (progress < 0.4f)
                return Color.Lerp(FateBlack, FateDarkPink, progress / 0.4f);
            else if (progress < 0.8f)
                return Color.Lerp(FateDarkPink, FateBrightRed, (progress - 0.4f) / 0.4f);
            else
                return Color.Lerp(FateBrightRed, FateWhite, (progress - 0.8f) / 0.2f);
        }
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 14;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Custom cosmic rocket rendering - dark prismatic with chromatic aberration
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            float rotation = Projectile.rotation;
            
            // Intense chromatic aberration trail
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailProgress = (float)i / Projectile.oldPos.Length;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailScale = (1f - trailProgress * 0.4f) * 1.1f;
                
                // RGB separation for chromatic aberration
                float separation = 4f * (1f - trailProgress);
                spriteBatch.Draw(glowTex, trailPos + new Vector2(-separation, 0), null, Color.Red * (1f - trailProgress) * 0.4f, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(glowTex, trailPos, null, GetFateGradient(trailProgress) * (1f - trailProgress) * 0.6f, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(glowTex, trailPos + new Vector2(separation, 0), null, Color.Cyan * (1f - trailProgress) * 0.4f, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Outer void darkness
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.18f) * 0.2f + 1f;
            spriteBatch.Draw(glowTex, drawPos, null, FateBlack * 0.7f, rotation, origin, 2.2f * pulse, SpriteEffects.None, 0f);
            
            // Dark pink mid layer
            spriteBatch.Draw(glowTex, drawPos, null, FateDarkPink * 0.8f, rotation, origin, 1.5f, SpriteEffects.None, 0f);
            
            // Bright red inner
            spriteBatch.Draw(glowTex, drawPos, null, FateBrightRed * 0.9f, rotation, origin, 0.9f, SpriteEffects.None, 0f);
            
            // White core
            spriteBatch.Draw(glowTex, drawPos, null, FateWhite, rotation, origin, 0.4f, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // Dark prismatic void smoke trail
            if (Main.rand.NextBool(2))
            {
                var smoke = new HeavySmokeParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    FateBlack, Main.rand.Next(35, 55), 0.45f, 0.55f, 0.018f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Dark prismatic glow trail
            if (Main.rand.NextBool(2))
            {
                float progress = Main.rand.NextFloat();
                Color trailColor = GetFateGradient(progress);
                var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.12f, trailColor * 0.75f, 0.4f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Reality-tearing chromatic aberration trail
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center - Projectile.velocity * 0.15f;
                CustomParticles.GenericFlare(trailPos + new Vector2(-3, 0), Color.Red * 0.4f, 0.18f, 10);
                CustomParticles.GenericFlare(trailPos + new Vector2(3, 0), Color.Cyan * 0.4f, 0.18f, 10);
            }
            
            // Temporal afterimage echoes
            if (Main.GameUpdateCount % 3 == 0)
            {
                float echoProgress = (Main.GameUpdateCount % 12) / 12f;
                Color echoColor = GetFateGradient(echoProgress) * 0.35f;
                CustomParticles.GenericFlare(Projectile.Center - Projectile.velocity * 0.3f, echoColor, 0.25f, 8);
            }
            
            if (Projectile.timeLeft % 4 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, FateBrightRed * 0.8f, 0.45f, 12);
            }
            
            // Destiny glyph trail on rocket
            if (Main.rand.NextBool(5))
            {
                CustomParticles.GlyphTrail(Projectile.Center, Projectile.velocity, FateDarkPink * 0.75f, 0.38f);
            }
            
            Lighting.AddLight(Projectile.Center, FateBrightRed.ToVector3() * 0.55f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            target.GetGlobalNPC<DestinyCollapseNPC>().AddStack(target, 4);
            
            // === UNIFIED VFX HIT EFFECT - FATE THEME (heavy for rocket) ===
            UnifiedVFX.Fate.HitEffect(target.Center, 1.5f);
            
            // Dark prismatic impact flash
            CustomParticles.GenericFlare(target.Center, FateBlack, 0.6f, 14);
            CustomParticles.GenericFlare(target.Center, FateBrightRed, 0.5f, 12);
            CustomParticles.GlyphImpact(target.Center, FateBlack, FateBrightRed, 0.55f);
            
            // === CHROMATIC ABERRATION ===
            CustomParticles.GenericFlare(target.Center + new Vector2(-3, 0), FateBrightRed * 0.4f, 0.3f, 12);
            CustomParticles.GenericFlare(target.Center + new Vector2(3, 0), FatePurple * 0.4f, 0.3f, 12);
            
            // === GLYPH FORMATIONS ===
            CustomParticles.GlyphCircle(target.Center, FateDarkPink, 5, 40f, 0.08f);
            
            // === TEMPORAL ECHO AFTERIMAGES ===
            for (int echo = 0; echo < 4; echo++)
            {
                Vector2 echoPos = target.Center + new Vector2(0, -echo * 10f);
                float echoAlpha = 1f - echo * 0.2f;
                CustomParticles.GenericFlare(echoPos, GetFateGradient((float)echo / 4f) * echoAlpha * 0.5f, 0.4f, 15);
            }
            
            // === COSMIC MUSIC NOTES ===
            ThemedParticles.FateMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.FateMusicNotes(target.Center, 4, 30f);
            
            // Cosmic Revisit - powerful rocket revisit
            int revisitDamage = (int)(damageDone * 0.35f);
            target.GetGlobalNPC<DestinyCollapseNPC>().QueueCosmicRevisit(target, revisitDamage, 20, Projectile.Center, 1.0f);
            
            Lighting.AddLight(target.Center, FateBrightRed.ToVector3() * 0.9f);
        }
        
        public override void OnKill(int timeLeft)
        {
            TriggerCataclysmicCollapse(Projectile.Center);
        }
        
        private void TriggerCataclysmicCollapse(Vector2 position)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f, Volume = 1.4f }, position);
            SoundEngine.PlaySound(SoundID.Item162 with { Pitch = 0.3f, Volume = 1.1f }, position);
            
            float explosionRadius = 280f;
            
            // PHASE 1: Central void flash - dark prismatic core
            CustomParticles.GenericFlare(position, FateBlack, 2.5f, 35);
            CustomParticles.GenericFlare(position, FateBrightRed, 2.0f, 30);
            CustomParticles.GenericFlare(position, FateWhite, 1.5f, 25);
            
            // PHASE 2: Multi-layered reality shatter halo rings
            for (int ring = 0; ring < 12; ring++)
            {
                float ringProgress = ring / 12f;
                Color ringColor = GetFateGradient(ringProgress);
                float scale = 0.35f + ring * 0.38f;
                int lifetime = 22 + ring * 5;
                CustomParticles.HaloRing(position, ringColor, scale, lifetime);
            }
            
            // PHASE 3: MASSIVE chromatic aberration reality shatter burst - 6 layers
            for (int layer = 0; layer < 6; layer++)
            {
                int points = 10 + layer * 5;
                float radius = 40f + layer * 50f;
                
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points + layer * 0.18f;
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    float gradientProgress = ((float)i / points + layer * 0.15f) % 1f;
                    Color burstColor = GetFateGradient(gradientProgress);
                    float flareScale = 0.9f - layer * 0.1f;
                    
                    // Core particle
                    CustomParticles.GenericFlare(position + offset, burstColor, flareScale, 28);
                    
                    // Chromatic aberration on each point
                    if (i % 2 == 0)
                    {
                        CustomParticles.GenericFlare(position + offset + new Vector2(-5, 0), Color.Red * 0.5f, flareScale * 0.7f, 22);
                        CustomParticles.GenericFlare(position + offset + new Vector2(5, 0), Color.Cyan * 0.5f, flareScale * 0.7f, 22);
                    }
                }
            }
            
            // PHASE 4: MASSIVE chromatic aberration shockwave ring
            for (int i = 0; i < 32; i++)
            {
                float angle = MathHelper.TwoPi * i / 32f;
                Vector2 baseOffset = angle.ToRotationVector2() * 90f;
                
                // RGB separation creating chromatic shockwave
                CustomParticles.GenericFlare(position + baseOffset + new Vector2(-6, 0), Color.Red * 0.55f, 0.55f, 22);
                CustomParticles.GenericFlare(position + baseOffset, FateBlack * 0.7f, 0.55f, 22);
                CustomParticles.GenericFlare(position + baseOffset + new Vector2(6, 0), Color.Cyan * 0.55f, 0.55f, 22);
            }
            
            // PHASE 5: Radial particle explosion with dark prismatic gradient
            for (int i = 0; i < 60; i++)
            {
                float angle = MathHelper.TwoPi * i / 60f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(14f, 26f);
                Color burstColor = GetFateGradient((float)i / 60f);
                
                var glow = new GenericGlowParticle(position, vel, burstColor, 
                    Main.rand.NextFloat(0.55f, 1.0f), Main.rand.Next(45, 70), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // PHASE 6: Heavy void smoke cloud - dark emphasis
            for (int i = 0; i < 35; i++)
            {
                Vector2 smokePos = position + Main.rand.NextVector2Circular(60f, 60f);
                Vector2 smokeVel = Main.rand.NextVector2Circular(7f, 7f) + new Vector2(0, -2.5f);
                var smoke = new HeavySmokeParticle(smokePos, smokeVel, FateBlack, 
                    Main.rand.Next(80, 130), Main.rand.NextFloat(1.0f, 1.6f), 0.65f, 0.01f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // PHASE 7: COSMIC glyph reality shatter - ultimate destiny explosion
            CustomParticles.GlyphCircle(position, FateBrightRed, 16, 120f, 0.05f);
            CustomParticles.GlyphBurst(position, FateDarkPink, 20, 10f);
            CustomParticles.GlyphTower(position, FatePurple, 8, 0.7f);
            
            // Extra glyph ring at outer edge
            CustomParticles.GlyphCircle(position, FateWhite * 0.8f, 10, 180f, -0.03f);
            
            // CATACLYSMIC COLLAPSE - Maximum cosmic music note explosion!
            ThemedParticles.FateMusicNoteBurst(position, 18, 10f);
            ThemedParticles.FateMusicNotes(position, 14, 70f);
            
            // Damage and debuff enemies with enhanced stacking
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.friendly)
                {
                    float dist = Vector2.Distance(npc.Center, position);
                    if (dist <= explosionRadius)
                    {
                        float falloff = 1f - (dist / explosionRadius) * 0.25f;
                        int damage = (int)(Projectile.damage * 2.5f * falloff);
                        npc.SimpleStrikeNPC(damage, 0, true, 14f);
                        
                        int stacks = dist < explosionRadius * 0.25f ? 6 : (dist < explosionRadius * 0.5f ? 4 : 3);
                        npc.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
                        npc.GetGlobalNPC<DestinyCollapseNPC>().AddStack(npc, stacks);
                        
                        // Hit flash on each enemy
                        CustomParticles.GenericFlare(npc.Center, FateBrightRed, 0.55f, 16);
                        CustomParticles.GlyphImpact(npc.Center, FateDarkPink, FateBrightRed, 0.45f);
                    }
                }
            }
        }
    }
}
