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
    /// Collapse Staff of Fate - Fires beams that trigger Collapse Overloads
    /// </summary>
    public class Fate8 : ModItem
    {
        // Dark Prismatic color palette
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        private int shotCounter = 0;
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.LastPrism;
        
        public override void SetDefaults()
        {
            Item.damage = 320;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 24);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item75;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<CollapseBeam>();
            Item.shootSpeed = 20f;
            Item.noMelee = true;
            Item.staff[Item.type] = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Fires rapid cosmic beams"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "Every 8th shot triggers a Collapse Overload"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'The last light before oblivion'") 
            { 
                OverrideColor = FateDarkPink 
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;
            
            bool isOverload = shotCounter % 8 == 0;
            
            if (isOverload)
            {
                // Overload burst - 5 beams
                for (int i = 0; i < 5; i++)
                {
                    float spread = MathHelper.ToRadians(-20f + i * 10f);
                    Vector2 spreadVel = velocity.RotatedBy(spread);
                    int proj = Projectile.NewProjectile(source, position, spreadVel, ModContent.ProjectileType<CollapseOverloadBeam>(), (int)(damage * 1.5f), knockback, player.whoAmI);
                }
                
                // Dark prismatic overload VFX with reality-tearing
                CustomParticles.GenericFlare(position, FateBlack, 1.4f, 28);
                CustomParticles.GenericFlare(position, FateBrightRed, 1.2f, 25);
                CustomParticles.HaloRing(position, FateDarkPink, 0.8f, 20);
                
                // Chromatic overload burst
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f;
                    Vector2 offset = angle.ToRotationVector2() * 35f;
                    CustomParticles.GenericFlare(position + offset + new Vector2(-4, 0), Color.Red * 0.5f, 0.4f, 15);
                    CustomParticles.GenericFlare(position + offset + new Vector2(4, 0), Color.Cyan * 0.5f, 0.4f, 15);
                }
                
                // Overload cosmic glyph explosion - reality-breaking burst
                CustomParticles.GlyphCircle(position, FateBrightRed, 10, 60f, 0.05f);
                CustomParticles.GlyphBurst(position, FateDarkPink, 10, 5f);
                CustomParticles.GlyphTower(position, FatePurple, 4, 0.5f);
                
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f;
                    Vector2 offset = angle.ToRotationVector2() * 35f;
                    float progress = (float)i / 10f;
                    Color burstColor = Color.Lerp(FateBlack, FateBrightRed, progress);
                    CustomParticles.GenericFlare(position + offset, burstColor, 0.55f, 17);
                }
                
                // COLLAPSE OVERLOAD - Massive cosmic music burst!
                ThemedParticles.FateMusicNoteBurst(position, 14, 7f);
                ThemedParticles.FateMusicNotes(position, 10, 50f);
                
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.8f }, position);
            }
            else
            {
                // Normal beam
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                
                // Dark prismatic normal cast VFX
                CustomParticles.GenericFlare(position, FateBlack, 0.5f, 12);
                CustomParticles.GenericFlare(position, FateBrightRed * 0.85f, 0.42f, 11);
                
                // Chromatic flash
                CustomParticles.GenericFlare(position + new Vector2(-2, 0), Color.Red * 0.3f, 0.2f, 8);
                CustomParticles.GenericFlare(position + new Vector2(2, 0), Color.Cyan * 0.3f, 0.2f, 8);
                
                // Normal shots occasionally spawn music notes!
                if (Main.rand.NextBool(3))
                {
                    ThemedParticles.FateMusicNotes(position, 2, 18f);
                }
            }
            
            return false;
        }
    }
    
    public class CollapseBeam : ModProjectile
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
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 3;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Dark prismatic reality-tearing trail
            if (Main.rand.NextBool(2))
            {
                float progress = Main.rand.NextFloat();
                Color trailColor = GetFateGradient(progress);
                CustomParticles.GenericFlare(Projectile.Center, trailColor * 0.55f, 0.18f, 12);
            }
            
            // Chromatic aberration trail
            if (Main.rand.NextBool(4))
            {
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(-2, 0), Color.Red * 0.25f, 0.1f, 6);
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(2, 0), Color.Cyan * 0.25f, 0.1f, 6);
            }
            
            // Destiny glyph trail on beams
            if (Main.rand.NextBool(5))
            {
                CustomParticles.GlyphTrail(Projectile.Center, Projectile.velocity, FateBrightRed * 0.55f, 0.22f);
            }
            
            Lighting.AddLight(Projectile.Center, FateDarkPink.ToVector3() * 0.35f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            target.GetGlobalNPC<DestinyCollapseNPC>().AddStack(target, 1);
            
            // === UNIFIED VFX HIT EFFECT - FATE THEME (compact for rapid beams) ===
            UnifiedVFX.Fate.HitEffect(target.Center, 0.8f);
            
            // Dark prismatic hit
            CustomParticles.GenericFlare(target.Center, FateBlack, 0.35f, 10);
            CustomParticles.GenericFlare(target.Center, FateBrightRed * 0.7f, 0.32f, 9);
            
            // Destiny glyph on beam hit
            if (Main.rand.NextBool(2))
            {
                CustomParticles.Glyph(target.Center, FateDarkPink, 0.32f, -1);
            }
            CustomParticles.GlyphImpact(target.Center, FateBlack, FateBrightRed, 0.5f);
            
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
            
            // Cosmic Revisit - rapid beam echoes
            int revisitDamage = (int)(damageDone * 0.18f);
            target.GetGlobalNPC<DestinyCollapseNPC>().QueueCosmicRevisit(target, revisitDamage, 25, Projectile.Center, 0.6f);
            
            Lighting.AddLight(target.Center, FateBrightRed.ToVector3() * 0.9f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Dark prismatic death burst
            CustomParticles.GenericFlare(Projectile.Center, FateBlack, 0.35f, 12);
            
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 vel = angle.ToRotationVector2() * 2.5f;
                Color burstColor = GetFateGradient((float)i / 5f);
                var glow = new GenericGlowParticle(Projectile.Center, vel, burstColor, 0.28f, 14, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Chromatic flash
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(-2, 0), Color.Red * 0.25f, 0.12f, 8);
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(2, 0), Color.Cyan * 0.25f, 0.12f, 8);
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
                    float trailAlpha = (1f - trailProgress) * 0.5f;
                    float trailScale = (1f - trailProgress * 0.4f) * 0.3f;
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                    
                    spriteBatch.Draw(glowTex, trailPos + new Vector2(-2, 0), null, Color.Red * trailAlpha * 0.4f, 0f, origin, trailScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(glowTex, trailPos, null, FateDarkPink * trailAlpha * 0.5f, 0f, origin, trailScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(glowTex, trailPos + new Vector2(2, 0), null, Color.Cyan * trailAlpha * 0.4f, 0f, origin, trailScale, SpriteEffects.None, 0f);
                }
            }
            
            // Draw compact beam glow
            spriteBatch.Draw(glowTex, drawPos, null, FateBlack * 0.9f, 0f, origin, 0.4f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateDarkPink * 0.7f, 0f, origin, 0.28f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateBrightRed * 0.8f, 0f, origin, 0.16f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateWhite * 0.9f, 0f, origin, 0.08f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    public class CollapseOverloadBeam : ModProjectile
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
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 4;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Dark prismatic bigger trail for overload beams
            float progress = Main.rand.NextFloat();
            Color trailColor = GetFateGradient(progress);
            CustomParticles.GenericFlare(Projectile.Center, trailColor * 0.75f, 0.28f, 14);
            
            // Enhanced chromatic aberration for overload
            if (Main.rand.NextBool(2))
            {
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(-3, 0), Color.Red * 0.38f, 0.15f, 8);
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(3, 0), Color.Cyan * 0.38f, 0.15f, 8);
            }
            
            // Temporal echo trail
            if (Main.GameUpdateCount % 3 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center - Projectile.velocity * 0.2f, FateBlack * 0.4f, 0.12f, 6);
            }
            
            Lighting.AddLight(Projectile.Center, FateBrightRed.ToVector3() * 0.55f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            target.GetGlobalNPC<DestinyCollapseNPC>().AddStack(target, 3);
            
            // === UNIFIED VFX HIT EFFECT - FATE THEME (heavy for overload) ===
            UnifiedVFX.Fate.HitEffect(target.Center, 1.3f);
            
            // Dark prismatic bigger hit VFX
            CustomParticles.GenericFlare(target.Center, FateBlack, 0.55f, 14);
            CustomParticles.GenericFlare(target.Center, FateBrightRed * 0.85f, 0.48f, 13);
            CustomParticles.HaloRing(target.Center, FateDarkPink * 0.65f, 0.28f, 12);
            
            // Cosmic glyph impact on overload hits
            CustomParticles.GlyphImpact(target.Center, FateBlack, FateBrightRed, 0.6f);
            
            // Visualize stacks with glyphs
            int stacks = target.GetGlobalNPC<DestinyCollapseNPC>().GetStacks(target);
            if (stacks > 0)
                CustomParticles.GlyphStack(target.Center, FateBrightRed, stacks, 0.25f);
            
            // === CHROMATIC ABERRATION ===
            CustomParticles.GenericFlare(target.Center + new Vector2(-3, 0), FateBrightRed * 0.4f, 0.3f, 12);
            CustomParticles.GenericFlare(target.Center + new Vector2(3, 0), FatePurple * 0.4f, 0.3f, 12);
            
            // === GLYPH FORMATIONS ===
            CustomParticles.GlyphCircle(target.Center, FateDarkPink, 5, 40f, 0.08f);
            
            // Chromatic impact burst
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 offset = angle.ToRotationVector2() * 18f;
                Color hitColor = GetFateGradient((float)i / 5f);
                CustomParticles.GenericFlare(target.Center + offset, hitColor * 0.55f, 0.28f, 10);
                
                // Chromatic split on each hit particle
                if (i % 2 == 0)
                {
                    CustomParticles.GenericFlare(target.Center + offset + new Vector2(-2, 0), Color.Red * 0.3f, 0.12f, 7);
                    CustomParticles.GenericFlare(target.Center + offset + new Vector2(2, 0), Color.Cyan * 0.3f, 0.12f, 7);
                }
            }
            
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
            
            // Cosmic Revisit - powerful overload revisit
            int revisitDamage = (int)(damageDone * 0.35f);
            target.GetGlobalNPC<DestinyCollapseNPC>().QueueCosmicRevisit(target, revisitDamage, 20, Projectile.Center, 1.0f);
            
            Lighting.AddLight(target.Center, FateBrightRed.ToVector3() * 0.9f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Dark prismatic death burst
            CustomParticles.GenericFlare(Projectile.Center, FateBlack, 0.6f, 17);
            CustomParticles.GenericFlare(Projectile.Center, FateBrightRed * 0.75f, 0.55f, 16);
            
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * 4.5f;
                Color burstColor = GetFateGradient((float)i / 10f);
                var glow = new GenericGlowParticle(Projectile.Center, vel, burstColor, 0.4f, 17, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Chromatic death flash
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 offset = angle.ToRotationVector2() * 15f;
                CustomParticles.GenericFlare(Projectile.Center + offset + new Vector2(-3, 0), Color.Red * 0.35f, 0.2f, 10);
                CustomParticles.GenericFlare(Projectile.Center + offset + new Vector2(3, 0), Color.Cyan * 0.35f, 0.2f, 10);
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            
            // Draw chromatic aberration trail - enhanced for overload
            if (ProjectileID.Sets.TrailCacheLength[Projectile.type] > 0)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) continue;
                    float trailProgress = (float)i / Projectile.oldPos.Length;
                    float trailAlpha = (1f - trailProgress) * 0.7f;
                    float trailScale = (1f - trailProgress * 0.4f) * 0.5f;
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                    
                    spriteBatch.Draw(glowTex, trailPos + new Vector2(-3, 0), null, Color.Red * trailAlpha * 0.5f, 0f, origin, trailScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(glowTex, trailPos, null, FateDarkPink * trailAlpha * 0.6f, 0f, origin, trailScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(glowTex, trailPos + new Vector2(3, 0), null, Color.Cyan * trailAlpha * 0.5f, 0f, origin, trailScale, SpriteEffects.None, 0f);
                }
            }
            
            // Draw larger beam glow for overload
            spriteBatch.Draw(glowTex, drawPos, null, FateBlack * 0.9f, 0f, origin, 0.6f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateDarkPink * 0.7f, 0f, origin, 0.4f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateBrightRed * 0.8f, 0f, origin, 0.25f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateWhite * 0.9f, 0f, origin, 0.12f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
}
