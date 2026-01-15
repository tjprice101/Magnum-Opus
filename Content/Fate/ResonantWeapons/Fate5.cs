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
    /// Cosmic Pulse Repeater - Rapid fire gun with Pulse Fractures
    /// </summary>
    public class Fate5 : ModItem
    {
        // Dark Prismatic color palette
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.ChainGun;
        
        public override void SetDefaults()
        {
            Item.damage = 265;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 30;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 1.5f;
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet;
            Item.noMelee = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Fires Cosmic Pulses that create Fate Fractures"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "Fractures slow enemies and reduce their damage"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'Rapid-fire destiny'") 
            { 
                OverrideColor = FateDarkPink 
            });
        }
        
        public override Vector2? HoldoutOffset() => new Vector2(-10f, 0f);
        
        public override void HoldItem(Player player)
        {
            // === UNIQUE: COSMIC PULSE AURA ===
            // The repeater thrums with pulsing cosmic energy
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.3f + 0.7f;
            Vector2 barrelPos = player.Center + new Vector2(player.direction * 35f, -2f);
            
            // === PULSING BARREL GLOW ===
            // The barrel visibly pulses with cosmic energy
            if (Main.rand.NextBool(4))
            {
                float pulseScale = 0.2f + pulse * 0.15f;
                CustomParticles.GenericFlare(barrelPos, FateBrightRed * pulse, pulseScale, 8);
                CustomParticles.GenericFlare(barrelPos, FateDarkPink * (1f - pulse * 0.3f), pulseScale * 0.7f, 6);
            }
            
            // === CHROMATIC WISPS ===
            // Small chromatic aberration particles drifting from gun
            if (Main.rand.NextBool(8))
            {
                Vector2 wispPos = player.Center + new Vector2(player.direction * Main.rand.NextFloat(10f, 40f), Main.rand.NextFloat(-10f, 10f));
                Color wispColor = Main.rand.NextBool() ? FateBrightRed : FateDarkPink;
                var wisp = new GenericGlowParticle(wispPos, new Vector2(player.direction * 0.5f, -0.3f), wispColor * 0.7f, 0.12f, 15, true);
                MagnumParticleHandler.SpawnParticle(wisp);
            }
            
            // === PULSE RHYTHM RINGS ===
            // Periodic pulse rings expand from barrel
            if (Main.GameUpdateCount % 30 == 0)
            {
                CustomParticles.HaloRing(barrelPos, FateDarkPink * 0.5f, 0.15f, 12);
            }
            
            // === MUSIC NOTES - The rhythm of fate ===
            if (Main.rand.NextBool(25))
            {
                ThemedParticles.FateMusicNotes(barrelPos, 1, 18f);
            }
            
            // Pulsing light
            Lighting.AddLight(barrelPos, FateDarkPink.ToVector3() * 0.3f * pulse);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire cosmic pulse
            float spread = MathHelper.ToRadians(3f);
            Vector2 spreadVel = velocity.RotatedBy(Main.rand.NextFloat(-spread, spread));
            
            Projectile.NewProjectile(source, position, spreadVel, ModContent.ProjectileType<CosmicPulseBullet>(), damage, knockback, player.whoAmI);
            
            // Dark prismatic muzzle flash with chromatic aberration
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 25f;
            CustomParticles.GenericFlare(muzzlePos, FateBlack, 0.45f, 10);
            CustomParticles.GenericFlare(muzzlePos, FateBrightRed * 0.9f, 0.38f, 9);
            
            // Chromatic split muzzle
            CustomParticles.GenericFlare(muzzlePos + new Vector2(-2, 0), Color.Red * 0.35f, 0.2f, 7);
            CustomParticles.GenericFlare(muzzlePos + new Vector2(2, 0), Color.Cyan * 0.35f, 0.2f, 7);
            
            // Rapid fire cosmic pulses with occasional music notes!
            if (Main.rand.NextBool(3))
            {
                ThemedParticles.FateMusicNotes(muzzlePos, 2, 18f);
            }
            
            return false;
        }
    }
    
    public class CosmicPulseBullet : ModProjectile
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
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 3;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // Dark prismatic reality-tearing trail
            if (Main.rand.NextBool(2))
            {
                float progress = Main.rand.NextFloat();
                Color trailColor = GetFateGradient(progress);
                CustomParticles.GenericFlare(Projectile.Center, trailColor * 0.6f, 0.15f, 10);
            }
            
            // Chromatic aberration trail for reality-tearing effect
            if (Main.rand.NextBool(3))
            {
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(-2, 0), Color.Red * 0.25f, 0.1f, 6);
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(2, 0), Color.Cyan * 0.25f, 0.1f, 6);
            }
            
            // Temporal echo trail
            if (Main.GameUpdateCount % 4 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center - Projectile.velocity * 0.15f, FateBlack * 0.4f, 0.1f, 6);
            }
            
            Lighting.AddLight(Projectile.Center, FateDarkPink.ToVector3() * 0.25f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            target.GetGlobalNPC<DestinyCollapseNPC>().AddStack(target, 1);
            
            // === UNIFIED VFX HIT EFFECT - FATE THEME (compact for rapid-fire) ===
            UnifiedVFX.Fate.HitEffect(target.Center, 0.7f);
            
            // Dark prismatic impact
            CustomParticles.GenericFlare(target.Center, FateBlack, 0.35f, 10);
            CustomParticles.GenericFlare(target.Center, FateBrightRed * 0.8f, 0.3f, 9);
            
            // === CHROMATIC ABERRATION ===
            CustomParticles.GenericFlare(target.Center + new Vector2(-3, 0), FateBrightRed * 0.4f, 0.3f, 12);
            CustomParticles.GenericFlare(target.Center + new Vector2(3, 0), FatePurple * 0.4f, 0.3f, 12);
            
            // Quick destiny glyph on rapid-fire hits
            if (Main.rand.NextBool(2))
            {
                CustomParticles.Glyph(target.Center, FateDarkPink, 0.32f, -1);
            }
            CustomParticles.GlyphImpact(target.Center, FateBlack, FateBrightRed, 0.45f);
            
            // === TEMPORAL ECHO AFTERIMAGES ===
            for (int echo = 0; echo < 4; echo++)
            {
                Vector2 echoPos = target.Center + new Vector2(0, -echo * 10f);
                float echoAlpha = 1f - echo * 0.2f;
                CustomParticles.GenericFlare(echoPos, GetFateGradient((float)echo / 4f) * echoAlpha * 0.5f, 0.4f, 15);
            }
            
            // Cosmic pulse impact with music notes!
            ThemedParticles.FateMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.FateMusicNotes(target.Center, 4, 30f);
            
            // Chain reaction fracture - higher chance and links to nearby enemies
            if (Main.rand.NextBool(3))
            {
                SpawnChainFracture(target);
            }
            
            // Cosmic Revisit - rapid-fire smaller echoes
            int revisitDamage = (int)(damageDone * 0.15f);
            target.GetGlobalNPC<DestinyCollapseNPC>().QueueCosmicRevisit(target, revisitDamage, 25, Projectile.Center, 0.6f);
            
            Lighting.AddLight(target.Center, FateBrightRed.ToVector3() * 0.9f);
        }
        
        private void SpawnChainFracture(NPC sourceTarget)
        {
            // Create fracture at hit location
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), sourceTarget.Center, Vector2.Zero,
                ModContent.ProjectileType<FateFracture>(), 0, 0f, Projectile.owner);
            
            // Chain to nearby enemies with visual connection
            float chainRange = 200f;
            int chainCount = 0;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.whoAmI == sourceTarget.whoAmI || chainCount >= 2) continue;
                
                float dist = Vector2.Distance(sourceTarget.Center, npc.Center);
                if (dist <= chainRange)
                {
                    // Draw chain lightning between targets
                    Vector2 midpoint = (sourceTarget.Center + npc.Center) / 2f;
                    for (int i = 0; i < 5; i++)
                    {
                        float t = i / 5f;
                        Vector2 chainPos = Vector2.Lerp(sourceTarget.Center, npc.Center, t);
                        chainPos += Main.rand.NextVector2Circular(8f, 8f);
                        Color chainColor = GetFateGradient(t);
                        CustomParticles.GenericFlare(chainPos, chainColor * 0.6f, 0.15f, 8);
                    }
                    
                    // Apply debuff to chained enemy
                    npc.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);
                    npc.GetGlobalNPC<DestinyCollapseNPC>().AddStack(npc, 1);
                    
                    chainCount++;
                }
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // Dark prismatic death burst
            CustomParticles.GenericFlare(Projectile.Center, FateBlack, 0.3f, 10);
            
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 vel = angle.ToRotationVector2() * 2.5f;
                Color burstColor = GetFateGradient((float)i / 5f);
                var glow = new GenericGlowParticle(Projectile.Center, vel, burstColor, 0.25f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Chromatic flash on death
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(-3, 0), Color.Red * 0.3f, 0.15f, 8);
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(3, 0), Color.Cyan * 0.3f, 0.15f, 8);
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
            
            // Draw compact layered glow core for bullet
            spriteBatch.Draw(glowTex, drawPos, null, FateBlack * 0.9f, 0f, origin, 0.5f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateDarkPink * 0.7f, 0f, origin, 0.35f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateBrightRed * 0.8f, 0f, origin, 0.2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateWhite * 0.9f, 0f, origin, 0.1f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    public class FateFracture : ModProjectile
    {
        // Dark Prismatic color palette
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            
            float lifeProgress = 1f - (Projectile.timeLeft / 120f);
            float intensity = (float)Math.Sin(lifeProgress * MathHelper.Pi);
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.1f + 1f;
            
            // Draw fracture glow
            float scale = (0.5f + intensity * 0.8f) * pulse;
            spriteBatch.Draw(glowTex, drawPos, null, FateBlack * 0.8f * intensity, 0f, origin, scale * 1.2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateDarkPink * 0.5f * intensity, 0f, origin, scale * 0.8f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, FateBrightRed * 0.6f * intensity, 0f, origin, scale * 0.5f, SpriteEffects.None, 0f);
            
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
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 120f);
            float intensity = (float)Math.Sin(lifeProgress * MathHelper.Pi);
            
            // Dark prismatic visual fracture effect with reality tears
            if (Main.GameUpdateCount % 3 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float radius = Main.rand.NextFloat(8f, 35f) * intensity;
                    Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * radius;
                    Color fractureColor = GetFateGradient(Main.rand.NextFloat());
                    CustomParticles.GenericFlare(particlePos, fractureColor * intensity * 0.6f, 0.22f, 12);
                }
            }
            
            // Central void pulsing
            if (Main.GameUpdateCount % 6 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, FateBlack, 0.35f * intensity, 10);
            }
            
            // Reality tear chromatic edges
            if (Main.GameUpdateCount % 4 == 0 && intensity > 0.3f)
            {
                float tearAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 tearPos = Projectile.Center + tearAngle.ToRotationVector2() * 25f * intensity;
                CustomParticles.GenericFlare(tearPos + new Vector2(-3, 0), Color.Red * 0.4f * intensity, 0.15f, 8);
                CustomParticles.GenericFlare(tearPos + new Vector2(3, 0), Color.Cyan * 0.4f * intensity, 0.15f, 8);
            }
            
            // Floating destiny glyphs around the fracture zone
            if (Main.GameUpdateCount % 10 == 0 && intensity > 0.35f)
            {
                CustomParticles.GlyphAura(Projectile.Center, FateBrightRed * intensity * 0.65f, 45f * intensity, 1);
            }
            
            // Slow enemies in area
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist <= 70f * intensity)
                {
                    npc.velocity *= 0.94f;
                }
            }
            
            Lighting.AddLight(Projectile.Center, FateDarkPink.ToVector3() * intensity * 0.35f);
        }
    }
}
