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
    /// Fate's Supernova Bow - Bow that creates Destiny Rifts
    /// </summary>
    public class Fate4 : ModItem
    {
        // Dark Prismatic color palette
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.Phantasm;
        
        public override void SetDefaults()
        {
            Item.damage = 320;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 30;
            Item.height = 70;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 22);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Arrow;
            Item.noMelee = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Arrows create Destiny Rifts that warp reality"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'Stars die by its will'") 
            { 
                OverrideColor = FateDarkPink 
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<FateRiftArrow>(), damage, knockback, player.whoAmI);
            
            // Dark prismatic muzzle flash with chromatic aberration
            CustomParticles.GenericFlare(position, FateBlack, 0.6f, 15);
            CustomParticles.GenericFlare(position, FateDarkPink, 0.5f, 12);
            CustomParticles.HaloRing(position, FatePurple * 0.7f, 0.35f, 12);
            
            // Chromatic aberration at muzzle
            CustomParticles.GenericFlare(position + new Vector2(-3, 0), Color.Red * 0.4f, 0.25f, 10);
            CustomParticles.GenericFlare(position + new Vector2(3, 0), Color.Cyan * 0.4f, 0.25f, 10);
            
            // Cosmic destiny glyph on arrow release
            CustomParticles.GlyphCircle(position, FateDarkPink, 4, 25f, 0.03f);
            CustomParticles.Glyph(position, FateBrightRed, 0.45f, -1);
            
            // Cosmic music notes accompany the rift arrows!
            ThemedParticles.FateMusicNotes(position, 5, 30f);
            
            return false;
        }
    }
    
    public class FateRiftArrow : ModProjectile
    {
        // Dark Prismatic color palette
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
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
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.arrow = true;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // Dark prismatic trail with temporal afterimages
            if (Main.rand.NextBool(2))
            {
                float progress = Main.rand.NextFloat();
                Color trailColor = GetFateGradient(progress);
                var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.05f, trailColor * 0.6f, 0.25f, 14, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Temporal afterimage trail - sharp echoes
            if (Main.GameUpdateCount % 3 == 0)
            {
                float echoProgress = (Main.GameUpdateCount % 15) / 15f;
                Color echoColor = GetFateGradient(echoProgress) * 0.4f;
                CustomParticles.GenericFlare(Projectile.Center - Projectile.velocity * 0.2f, echoColor, 0.18f, 8);
            }
            
            // Chromatic aberration trail
            if (Main.rand.NextBool(4))
            {
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(-2, 0), Color.Red * 0.3f, 0.12f, 6);
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(2, 0), Color.Cyan * 0.3f, 0.12f, 6);
            }
            
            // Destiny glyph trail on arrows
            if (Main.rand.NextBool(4))
            {
                CustomParticles.GlyphTrail(Projectile.Center, Projectile.velocity, FateBrightRed * 0.7f, 0.28f);
            }
            
            Lighting.AddLight(Projectile.Center, FateDarkPink.ToVector3() * 0.35f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnDestinyRift(target.Center);
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            target.GetGlobalNPC<DestinyCollapseNPC>().AddStack(target, 2);
            
            // === UNIFIED VFX HIT EFFECT - FATE THEME ===
            UnifiedVFX.Fate.HitEffect(target.Center, 1.2f);
            
            // Impact VFX with dark prismatic burst
            CustomParticles.GenericFlare(target.Center, FateBlack, 0.5f, 12);
            CustomParticles.GenericFlare(target.Center, FateBrightRed, 0.4f, 10);
            CustomParticles.GlyphImpact(target.Center, FateDarkPink, FateBrightRed, 0.55f);
            
            // Chromatic impact flash
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 offset = angle.ToRotationVector2() * 12f;
                CustomParticles.GenericFlare(target.Center + offset + new Vector2(-2, 0), Color.Red * 0.35f, 0.18f, 8);
                CustomParticles.GenericFlare(target.Center + offset + new Vector2(2, 0), Color.Cyan * 0.35f, 0.18f, 8);
            }
            
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
            
            // Destiny rift spawns music notes!
            ThemedParticles.FateMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.FateMusicNotes(target.Center, 4, 25f);
            
            // Cosmic Revisit - delayed cosmic flare that strikes again
            int revisitDamage = (int)(damageDone * 0.25f);
            target.GetGlobalNPC<DestinyCollapseNPC>().QueueCosmicRevisit(target, revisitDamage, 25, Projectile.Center, 0.8f);
            
            Lighting.AddLight(target.Center, FateBrightRed.ToVector3() * 0.9f);
        }
        
        public override void OnKill(int timeLeft)
        {
            SpawnDestinyRift(Projectile.Center);
        }
        
        private void SpawnDestinyRift(Vector2 position)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, Vector2.Zero, 
                ModContent.ProjectileType<DestinyRift>(), Projectile.damage / 2, 0f, Projectile.owner);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            
            // Draw chromatic aberration trail
            if (ProjectileID.Sets.TrailCacheLength[Projectile.type] > 0)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) continue;
                    float trailProgress = (float)i / Projectile.oldPos.Length;
                    float trailAlpha = (1f - trailProgress) * 0.6f;
                    float trailScale = (1f - trailProgress * 0.4f) * 0.5f;
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                    
                    spriteBatch.Draw(glowTex, trailPos + new Vector2(-3, 0), null, Color.Red * trailAlpha * 0.4f, 0f, origin, trailScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(glowTex, trailPos, null, FateDarkPink * trailAlpha * 0.5f, 0f, origin, trailScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(glowTex, trailPos + new Vector2(3, 0), null, Color.Cyan * trailAlpha * 0.4f, 0f, origin, trailScale, SpriteEffects.None, 0f);
                }
            }
            
            // Draw layered glow core
            spriteBatch.Draw(glowTex, drawPos, null, FateBlack * 0.9f, 0f, origin, 1.0f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateDarkPink * 0.7f, 0f, origin, 0.7f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateBrightRed * 0.8f, 0f, origin, 0.45f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateWhite * 0.9f, 0f, origin, 0.25f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    public class DestinyRift : ModProjectile
    {
        // Dark Prismatic color palette
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        private const float RiftRadius = 160f;
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            
            float lifeProgress = 1f - (Projectile.timeLeft / 90f);
            float intensity = (float)Math.Sin(lifeProgress * MathHelper.Pi);
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f + 1f;
            
            // Draw expanding void glow
            float scale = (1.0f + intensity * 1.5f) * pulse;
            spriteBatch.Draw(glowTex, drawPos, null, FateBlack * 0.9f * intensity, 0f, origin, scale * 1.4f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateDarkPink * 0.5f * intensity, 0f, origin, scale * 1.0f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateBrightRed * 0.6f * intensity, 0f, origin, scale * 0.6f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateWhite * 0.7f * intensity, 0f, origin, scale * 0.3f, SpriteEffects.None, 0f);
            
            return false;
        }
        
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
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 90f);
            float intensity = (float)Math.Sin(lifeProgress * MathHelper.Pi);
            
            // Reality-tearing dark prismatic visuals
            if (Main.GameUpdateCount % 2 == 0)
            {
                // Swirling void particles with dark core
                int spiralCount = 5;
                for (int i = 0; i < spiralCount; i++)
                {
                    float angle = Main.GameUpdateCount * 0.14f + MathHelper.TwoPi * i / spiralCount;
                    float radius = RiftRadius * (0.25f + intensity * 0.55f) * ((Main.GameUpdateCount % 30) / 30f);
                    Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * radius;
                    
                    Color spiralColor = GetFateGradient((float)i / spiralCount);
                    CustomParticles.GenericFlare(particlePos, spiralColor * intensity * 0.75f, 0.28f, 12);
                }
            }
            
            // Central void pulsing - emphasis on dark core
            if (Main.GameUpdateCount % 5 == 0)
            {
                float pulse = 0.4f + intensity * 0.35f;
                CustomParticles.GenericFlare(Projectile.Center, FateBlack, pulse, 14);
                CustomParticles.HaloRing(Projectile.Center, FateDarkPink * intensity * 0.6f, 0.28f, 12);
            }
            
            // Reality tear effect - chromatic distortion ring
            if (Main.GameUpdateCount % 4 == 0)
            {
                float tearAngle = Main.GameUpdateCount * 0.08f;
                for (int i = 0; i < 6; i++)
                {
                    float angle = tearAngle + MathHelper.TwoPi * i / 6f;
                    Vector2 tearPos = Projectile.Center + angle.ToRotationVector2() * RiftRadius * intensity * 0.6f;
                    CustomParticles.GenericFlare(tearPos + new Vector2(-3, 0), Color.Red * 0.35f * intensity, 0.16f, 8);
                    CustomParticles.GenericFlare(tearPos + new Vector2(3, 0), Color.Cyan * 0.35f * intensity, 0.16f, 8);
                }
            }
            
            // Destiny glyph circle orbiting the rift
            if (Main.GameUpdateCount % 12 == 0 && intensity > 0.3f)
            {
                CustomParticles.GlyphCircle(Projectile.Center, FateBrightRed * intensity, 5, RiftRadius * 0.5f * intensity, 0.04f);
            }
            
            // Floating glyphs within the rift
            if (Main.GameUpdateCount % 18 == 0)
            {
                CustomParticles.GlyphAura(Projectile.Center, FateDarkPink * intensity * 0.7f, RiftRadius * 0.7f * intensity, 2);
            }
            
            // Slow and debuff enemies - increased effect
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist <= RiftRadius * intensity)
                {
                    npc.velocity *= 0.91f;
                    
                    // Pull slightly toward center
                    Vector2 pullDir = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                    npc.velocity += pullDir * 0.15f * intensity;
                }
            }
            
            Lighting.AddLight(Projectile.Center, FateDarkPink.ToVector3() * intensity * 0.65f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            target.GetGlobalNPC<DestinyCollapseNPC>().AddStack(target, 2);
            
            // === UNIFIED VFX HIT EFFECT - FATE THEME ===
            UnifiedVFX.Fate.HitEffect(target.Center, 1.0f);
            
            // Dark prismatic hit burst
            CustomParticles.GenericFlare(target.Center, FateBlack, 0.45f, 12);
            
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * 18f;
                Color hitColor = GetFateGradient((float)i / 6f);
                CustomParticles.GenericFlare(target.Center + offset, hitColor, 0.32f, 12);
                
                // Chromatic split
                if (i % 2 == 0)
                {
                    CustomParticles.GenericFlare(target.Center + offset + new Vector2(-2, 0), Color.Red * 0.3f, 0.15f, 8);
                    CustomParticles.GenericFlare(target.Center + offset + new Vector2(2, 0), Color.Cyan * 0.3f, 0.15f, 8);
                }
            }
            
            // === CHROMATIC ABERRATION ===
            CustomParticles.GenericFlare(target.Center + new Vector2(-3, 0), FateBrightRed * 0.4f, 0.3f, 12);
            CustomParticles.GenericFlare(target.Center + new Vector2(3, 0), FatePurple * 0.4f, 0.3f, 12);
            
            // Glyph impact
            CustomParticles.GlyphImpact(target.Center, FateBlack, FateBrightRed, 0.55f);
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
            
            // Cosmic Revisit - rift tears reality again
            int revisitDamage = (int)(damageDone * 0.30f);
            target.GetGlobalNPC<DestinyCollapseNPC>().QueueCosmicRevisit(target, revisitDamage, 25, Projectile.Center, 0.9f);
            
            Lighting.AddLight(target.Center, FateBrightRed.ToVector3() * 0.9f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Reality collapse VFX - dark prismatic implosion
            CustomParticles.GenericFlare(Projectile.Center, FateBlack, 0.8f, 18);
            CustomParticles.GenericFlare(Projectile.Center, FateBrightRed, 0.6f, 15);
            
            for (int ring = 0; ring < 5; ring++)
            {
                Color ringColor = GetFateGradient(ring / 5f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.45f - ring * 0.06f, 14 + ring * 2);
            }
            
            // Chromatic collapse burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 35f;
                CustomParticles.GenericFlare(Projectile.Center + offset + new Vector2(-4, 0), Color.Red * 0.45f, 0.25f, 12);
                CustomParticles.GenericFlare(Projectile.Center + offset, FateBlack * 0.6f, 0.25f, 12);
                CustomParticles.GenericFlare(Projectile.Center + offset + new Vector2(4, 0), Color.Cyan * 0.45f, 0.25f, 12);
            }
            
            // Destiny rift collapse with massive glyph explosion
            CustomParticles.GlyphBurst(Projectile.Center, FateBrightRed, 10, 5f);
            CustomParticles.GlyphCircle(Projectile.Center, FateDarkPink, 6, 50f, 0.05f);
            
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * 5f;
                Color burstColor = GetFateGradient((float)i / 12f);
                var glow = new GenericGlowParticle(Projectile.Center, vel, burstColor, 0.45f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = -0.3f, Volume = 0.6f }, Projectile.Center);
        }
    }
}
