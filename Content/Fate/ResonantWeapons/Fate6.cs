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
    /// CANNON OF INEVITABILITY - Ranged Gun #1
    /// 
    /// UNIQUE ABILITY: "TIMELINE CONVERGENCE SHOT"
    /// Every shot exists across 5 parallel timelines simultaneously.
    /// All 5 bullets converge on the same target point for devastating damage.
    /// Each timeline bullet has a different chromatic hue creating a kaleidoscopic spread.
    /// 
    /// PASSIVE: Bullets leave "fate trails" - persistent light beams that damage
    /// enemies who cross them. Massive lens flare on every shot.
    /// </summary>
    public class Fate6 : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.SDMG;
        
        public override void SetDefaults()
        {
            Item.damage = 340;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 30;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 28);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item40;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet;
            Item.noMelee = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Each shot exists across 5 parallel timelines"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "All timeline bullets converge on your target"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect3", "Bullets leave persistent fate trails that damage enemies"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'Escape is not within the realm of possibility'") { OverrideColor = FateLensFlare.FateBrightRed });
        }
        
        public override Vector2? HoldoutOffset() => new Vector2(-8f, 0f);
        
        public override void HoldItem(Player player)
        {
            Vector2 muzzle = player.Center + new Vector2(player.direction * 45f, -2f);
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.07f) * 0.25f + 0.75f;
            
            // Timeline shimmer - 5 ghostly gun images
            if (Main.rand.NextBool(6))
            {
                for (int i = 0; i < 5; i++)
                {
                    float timelineOffset = (i - 2f) * 4f;
                    Vector2 ghostPos = muzzle + new Vector2(0, timelineOffset);
                    Color ghostColor = FateLensFlare.GetFateGradient((float)i / 5f) * 0.3f;
                    CustomParticles.GenericFlare(ghostPos, ghostColor, 0.15f, 8);
                }
            }
            
            // Chromatic barrel glow
            FateLensFlare.ChromaticShift(muzzle, 25f, 0.4f);
            
            // Ambient lens flare at barrel
            if (Main.GameUpdateCount % 25 == 0)
                FateLensFlareDrawLayer.AddFlare(muzzle, 0.3f * pulse, 0.4f, 15);
            
            Lighting.AddLight(muzzle, FateLensFlare.FateDarkPink.ToVector3() * 0.4f * pulse);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 muzzle = position + velocity.SafeNormalize(Vector2.Zero) * 40f;
            
            // === TIMELINE CONVERGENCE - 5 bullets from different timelines ===
            for (int timeline = 0; timeline < 5; timeline++)
            {
                // Each timeline bullet starts from slightly different position
                float timelineOffset = MathHelper.ToRadians(-8f + timeline * 4f);
                Vector2 timelineVel = velocity.RotatedBy(timelineOffset);
                
                // All converge toward mouse position
                Vector2 convergencePoint = Main.MouseWorld;
                Vector2 toConvergence = (convergencePoint - muzzle).SafeNormalize(Vector2.Zero);
                
                // Blend between spread and convergence
                Vector2 finalVel = Vector2.Lerp(timelineVel, toConvergence * velocity.Length(), 0.3f);
                
                Projectile.NewProjectile(source, muzzle, finalVel,
                    ModContent.ProjectileType<TimelineBullet>(), damage, knockback, player.whoAmI, timeline);
            }
            
            // === MASSIVE MUZZLE FLASH ===
            FateLensFlareDrawLayer.AddFlare(muzzle, 1.2f, 1f, 30);
            FateLensFlare.KaleidoscopeBurst(muzzle, 0.8f, 5);
            
            // Chromatic muzzle flash
            CustomParticles.GenericFlare(muzzle, FateLensFlare.FateWhite, 0.9f, 18);
            CustomParticles.GenericFlare(muzzle, FateLensFlare.FateBrightRed, 0.7f, 15);
            CustomParticles.HaloRing(muzzle, FateLensFlare.FateDarkPink, 0.5f, 14);
            
            // RGB separation muzzle
            CustomParticles.GenericFlare(muzzle + new Vector2(-5, 0), Color.Red * 0.5f, 0.4f, 12);
            CustomParticles.GenericFlare(muzzle + new Vector2(5, 0), FateLensFlare.FateCyan * 0.5f, 0.4f, 12);
            
            return false;
        }
    }
    
    public class TimelineBullet : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private int TimelineIndex => (int)Projectile.ai[0];
        private List<Vector2> trailPositions = new List<Vector2>();
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 2;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Store positions for fate trail
            if (Main.GameUpdateCount % 5 == 0)
            {
                trailPositions.Add(Projectile.Center);
                if (trailPositions.Count > 30)
                    trailPositions.RemoveAt(0);
            }
            
            // Each timeline has distinct color
            float colorOffset = TimelineIndex / 5f;
            Color bulletColor = FateLensFlare.GetFateGradient((colorOffset + Main.GameUpdateCount * 0.01f) % 1f);
            
            // Chromatic trail particles
            if (Main.rand.NextBool(2))
            {
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    bulletColor * 0.7f, 0.2f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
                
                // RGB split
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(-2, 0), Color.Red * 0.3f, 0.1f, 6);
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(2, 0), FateLensFlare.FateCyan * 0.3f, 0.1f, 6);
            }
            
            // Periodic mini lens flare
            if (Main.GameUpdateCount % 10 == TimelineIndex * 2)
                FateLensFlareDrawLayer.AddFlare(Projectile.Center, 0.25f, 0.3f, 8);
            
            Lighting.AddLight(Projectile.Center, bulletColor.ToVector3() * 0.4f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            
            float colorOffset = TimelineIndex / 5f;
            Color bulletColor = FateLensFlare.GetFateGradient((colorOffset + Main.GameUpdateCount * 0.01f) % 1f);
            
            // Draw trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float trailProgress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = 1f - trailProgress;
                Color trailColor = FateLensFlare.GetFateGradient((colorOffset + trailProgress) % 1f) * trailAlpha * 0.4f;
                
                sb.Draw(glow, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null, trailColor,
                    Projectile.oldRot[i], glow.Size() / 2f, new Vector2(0.6f - trailProgress * 0.3f, 0.2f), SpriteEffects.None, 0f);
            }
            
            // Main bullet
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, bulletColor * 0.7f,
                Projectile.rotation, glow.Size() / 2f, new Vector2(0.8f, 0.35f), SpriteEffects.None, 0f);
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, Color.White * 0.5f,
                Projectile.rotation, glow.Size() / 2f, new Vector2(0.5f, 0.15f), SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 240);
            
            FateLensFlareDrawLayer.AddFlare(target.Center, 0.6f, 0.5f, 18);
            FateLensFlare.KaleidoscopeBurst(target.Center, 0.5f, 4);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Spawn fate trail (persistent damage line)
            if (trailPositions.Count >= 2)
            {
                Player owner = Main.player[Projectile.owner];
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<FateTrailLine>(), Projectile.damage / 3, 0f, Projectile.owner,
                    trailPositions[0].X, trailPositions[0].Y);
            }
            
            FateLensFlare.KaleidoscopeBurst(Projectile.Center, 0.4f, 3);
        }
    }
    
    public class FateTrailLine : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private const int TrailDuration = 60;
        private Vector2 StartPoint => new Vector2(Projectile.ai[0], Projectile.ai[1]);
        
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = TrailDuration;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        
        public override void AI()
        {
            float progress = 1f - (float)Projectile.timeLeft / TrailDuration;
            float intensity = (float)Math.Sin(progress * MathHelper.Pi);
            
            Vector2 lineDir = (Projectile.Center - StartPoint).SafeNormalize(Vector2.UnitX);
            float lineLength = Vector2.Distance(StartPoint, Projectile.Center);
            
            // Trail line particles
            if (Main.rand.NextBool(2))
            {
                float along = Main.rand.NextFloat();
                Vector2 particlePos = Vector2.Lerp(StartPoint, Projectile.Center, along);
                Color trailColor = FateLensFlare.GetFateGradient(along) * intensity * 0.5f;
                CustomParticles.GenericFlare(particlePos, trailColor, 0.15f, 8);
            }
            
            Lighting.AddLight(Vector2.Lerp(StartPoint, Projectile.Center, 0.5f), FateLensFlare.FateDarkPink.ToVector3() * intensity * 0.3f);
        }
        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Line collision
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), StartPoint, Projectile.Center, 15f, ref point);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            
            float progress = 1f - (float)Projectile.timeLeft / TrailDuration;
            float intensity = (float)Math.Sin(progress * MathHelper.Pi);
            
            Vector2 lineDir = (Projectile.Center - StartPoint).SafeNormalize(Vector2.UnitX);
            float lineLength = Vector2.Distance(StartPoint, Projectile.Center);
            Vector2 midpoint = (StartPoint + Projectile.Center) / 2f;
            
            // Draw line as stretched glow
            float rotation = lineDir.ToRotation();
            float scaleX = lineLength / glow.Width;
            
            sb.Draw(glow, midpoint - Main.screenPosition, null, FateLensFlare.FateDarkPink * 0.4f * intensity,
                rotation, glow.Size() / 2f, new Vector2(scaleX, 0.08f), SpriteEffects.None, 0f);
            sb.Draw(glow, midpoint - Main.screenPosition, null, FateLensFlare.FateBrightRed * 0.3f * intensity,
                rotation, glow.Size() / 2f, new Vector2(scaleX, 0.04f), SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 120);
        }
    }
}
