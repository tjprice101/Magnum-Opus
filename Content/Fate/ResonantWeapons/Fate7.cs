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
    /// BOW OF INEVITABLE DOOM - Ranged Bow #2
    /// 
    /// UNIQUE ABILITY: "DESTINY ARROWS"
    /// Charged shots fire an arrow that pierces through reality itself.
    /// At the cursor location, a massive kaleidoscopic portal opens, raining
    /// down a barrage of smaller arrows from parallel timelines.
    /// 
    /// PASSIVE: Arrows phase through walls briefly and leave reality distortions.
    /// Full charge creates massive lens flare and heat wave effects.
    /// </summary>
    public class Fate7 : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Phantasm;
        
        private int chargeTime = 0;
        private const int MaxCharge = 60;
        private const int MinChargeForSpecial = 40;
        
        public override void SetDefaults()
        {
            Item.damage = 285;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 32;
            Item.height = 60;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 26);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 20f;
            Item.useAmmo = AmmoID.Arrow;
            Item.noMelee = true;
            Item.channel = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Hold to charge a destiny arrow"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "Fully charged shots open kaleidoscopic portals"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect3", "Portals rain down arrows from parallel timelines"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect4", "Arrows briefly phase through walls"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'Every arrow finds its mark across infinite timelines'") { OverrideColor = FateLensFlare.FateDarkPink });
        }
        
        public override void HoldItem(Player player)
        {
            Vector2 bowPos = player.Center + new Vector2(player.direction * 20f, -5f);
            
            if (player.channel)
            {
                chargeTime = Math.Min(chargeTime + 1, MaxCharge);
                float chargeProgress = (float)chargeTime / MaxCharge;
                
                // Building charge VFX
                CustomParticles.GenericFlare(bowPos, FateLensFlare.GetFateGradient(chargeProgress), 0.3f + chargeProgress * 0.4f, 8);
                
                // Spiraling charge particles
                if (Main.rand.NextBool(3))
                {
                    float spiralAngle = Main.GameUpdateCount * 0.15f + Main.rand.NextFloat() * MathHelper.TwoPi;
                    float spiralRadius = (1f - chargeProgress) * 50f + 10f;
                    Vector2 spiralPos = bowPos + spiralAngle.ToRotationVector2() * spiralRadius;
                    Color spiralColor = FateLensFlare.GetFateGradient(Main.rand.NextFloat()) * 0.6f;
                    
                    var spiral = new GenericGlowParticle(spiralPos, (bowPos - spiralPos).SafeNormalize(Vector2.Zero) * 3f,
                        spiralColor, 0.2f, 15, true);
                    MagnumParticleHandler.SpawnParticle(spiral);
                }
                
                // Lens flare at max charge
                if (chargeProgress >= 1f && Main.GameUpdateCount % 5 == 0)
                {
                    FateLensFlareDrawLayer.AddFlare(bowPos, 0.6f, 0.6f, 12);
                    FateLensFlare.ChromaticShift(bowPos, 35f, 0.5f);
                }
                
                // Heat wave distortion
                FateLensFlare.DrawHeatWaveDistortion(bowPos, chargeProgress * 30f, chargeProgress * 0.4f);
                
                Lighting.AddLight(bowPos, FateLensFlare.FateBrightRed.ToVector3() * chargeProgress * 0.5f);
            }
            else if (chargeTime > 0)
            {
                // Reset charge if not channeling
                chargeTime = 0;
            }
        }
        
        public override bool CanUseItem(Player player) => !player.channel || chargeTime < 5;
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float chargeProgress = (float)chargeTime / MaxCharge;
            bool fullyCharged = chargeTime >= MinChargeForSpecial;
            
            Vector2 spawnPos = position;
            Vector2 toMouse = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            
            if (fullyCharged)
            {
                // === DESTINY ARROW - Full Charge ===
                Projectile.NewProjectile(source, spawnPos, toMouse * velocity.Length() * 1.3f,
                    ModContent.ProjectileType<DestinyArrow>(), (int)(damage * (1f + chargeProgress)), knockback, player.whoAmI);
                
                // Open kaleidoscopic portal at cursor
                Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero,
                    ModContent.ProjectileType<KaleidoscopicPortal>(), damage / 2, knockback / 2, player.whoAmI);
                
                // === MASSIVE RELEASE VFX ===
                FateLensFlareDrawLayer.AddFlare(spawnPos, 1.5f, 1f, 35);
                FateLensFlare.KaleidoscopeBurst(spawnPos, 1f, 8);
                
                // Expanding chromatic shockwave
                for (int ring = 0; ring < 5; ring++)
                {
                    Color ringColor = FateLensFlare.GetFateGradient((float)ring / 5f);
                    CustomParticles.HaloRing(spawnPos, ringColor * 0.6f, 0.3f + ring * 0.15f, 15 + ring * 4);
                }
                
                SoundEngine.PlaySound(SoundID.Item122, position);
            }
            else
            {
                // Normal arrow with phase effect
                Projectile.NewProjectile(source, spawnPos, velocity,
                    ModContent.ProjectileType<PhaseArrow>(), damage, knockback, player.whoAmI);
                
                // Small VFX
                FateLensFlareDrawLayer.AddFlare(spawnPos, 0.4f, 0.4f, 12);
                CustomParticles.GenericFlare(spawnPos, FateLensFlare.FateDarkPink, 0.4f, 12);
            }
            
            chargeTime = 0;
            return false;
        }
    }
    
    public class DestinyArrow : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private const int PhaseWallDuration = 30;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.arrow = true;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false; // Phases through for initial duration
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Phase through walls only for initial frames
            if (Projectile.timeLeft < 240 - PhaseWallDuration)
                Projectile.tileCollide = true;
            
            // === DESTINY TRAIL ===
            Color trailColor = FateLensFlare.GetFateGradient((Main.GameUpdateCount * 0.02f) % 1f);
            
            // Main trail
            var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f),
                trailColor * 0.6f, 0.3f, 18, true);
            MagnumParticleHandler.SpawnParticle(trail);
            
            // Chromatic aberration trail
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(-3, -2), Color.Red * 0.3f, 0.12f, 8);
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(3, 2), FateLensFlare.FateCyan * 0.3f, 0.12f, 8);
            
            // Reality distortion along path
            if (Main.rand.NextBool(4))
                FateLensFlare.DrawHeatWaveDistortion(Projectile.Center, 20f, 0.2f);
            
            // Periodic lens flare
            if (Main.GameUpdateCount % 8 == 0)
                FateLensFlareDrawLayer.AddFlare(Projectile.Center, 0.35f, 0.4f, 10);
            
            Lighting.AddLight(Projectile.Center, FateLensFlare.FateBrightRed.ToVector3() * 0.5f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            
            // Draw kaleidoscopic trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = 1f - progress;
                Color trailColor = FateLensFlare.GetFateGradient(progress) * alpha * 0.5f;
                
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float scale = (1f - progress * 0.5f) * 0.6f;
                
                sb.Draw(glow, drawPos, null, trailColor, Projectile.oldRot[i], glow.Size() / 2f, 
                    new Vector2(scale, scale * 0.3f), SpriteEffects.None, 0f);
            }
            
            // Arrow head (stretched glow pointing forward)
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f + 0.9f;
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, FateLensFlare.FateDarkPink * 0.7f,
                Projectile.rotation, glow.Size() / 2f, new Vector2(1f * pulse, 0.25f * pulse), SpriteEffects.None, 0f);
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, Color.White * 0.5f,
                Projectile.rotation, glow.Size() / 2f, new Vector2(0.6f * pulse, 0.12f * pulse), SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            
            FateLensFlareDrawLayer.AddFlare(target.Center, 0.8f, 0.6f, 20);
            FateLensFlare.KaleidoscopeBurst(target.Center, 0.7f, 6);
            
            // Chromatic impact
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Color impactColor = FateLensFlare.GetFateGradient((float)i / 8f);
                Vector2 offset = angle.ToRotationVector2() * 25f;
                CustomParticles.GenericFlare(target.Center + offset, impactColor, 0.35f, 15);
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            FateLensFlare.KaleidoscopeBurst(Projectile.Center, 0.5f, 4);
            CustomParticles.ExplosionBurst(Projectile.Center, FateLensFlare.FateDarkPink, 10, 5f);
        }
    }
    
    public class PhaseArrow : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private const int PhaseWallDuration = 15;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.arrow = true;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            if (Projectile.timeLeft < 180 - PhaseWallDuration)
                Projectile.tileCollide = true;
            
            // Simple fate trail
            if (Main.rand.NextBool(2))
            {
                Color trailColor = FateLensFlare.GetFateGradient(Main.rand.NextFloat()) * 0.4f;
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    trailColor, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            Lighting.AddLight(Projectile.Center, FateLensFlare.FatePurple.ToVector3() * 0.25f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            
            // Simple trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float alpha = 1f - (float)i / Projectile.oldPos.Length;
                sb.Draw(glow, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null, 
                    FateLensFlare.FatePurple * alpha * 0.3f, Projectile.oldRot[i], glow.Size() / 2f, 
                    new Vector2(0.4f, 0.15f), SpriteEffects.None, 0f);
            }
            
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, FateLensFlare.FateDarkPink * 0.6f,
                Projectile.rotation, glow.Size() / 2f, new Vector2(0.6f, 0.2f), SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 120);
        }
    }
    
    public class KaleidoscopicPortal : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private const int ActiveDuration = 90;
        private int arrowsFired = 0;
        private const int MaxArrows = 12;
        
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = ActiveDuration;
            Projectile.tileCollide = false;
        }
        
        public override void AI()
        {
            float progress = 1f - (float)Projectile.timeLeft / ActiveDuration;
            float openProgress = Math.Min(progress * 4f, 1f);
            float closeProgress = Math.Max((progress - 0.75f) * 4f, 0f);
            float intensity = openProgress * (1f - closeProgress);
            
            // === KALEIDOSCOPIC PORTAL VISUALS ===
            // Rotating kaleidoscope effect
            FateLensFlare.KaleidoscopeBurst(Projectile.Center, intensity * 0.8f, (int)(8 * intensity + 1));
            
            // Swirling portal edge
            int edgeParticles = (int)(12 * intensity);
            for (int i = 0; i < edgeParticles; i++)
            {
                float angle = Main.GameUpdateCount * 0.1f + MathHelper.TwoPi * i / edgeParticles;
                float radius = 60f * intensity;
                Vector2 edgePos = Projectile.Center + angle.ToRotationVector2() * radius;
                Color edgeColor = FateLensFlare.GetFateGradient((float)i / edgeParticles);
                CustomParticles.GenericFlare(edgePos, edgeColor * 0.5f, 0.2f, 6);
            }
            
            // Central lens flare
            if (Main.GameUpdateCount % 5 == 0)
                FateLensFlareDrawLayer.AddFlare(Projectile.Center, 0.7f * intensity, 0.5f, 12);
            
            // Heat wave distortion
            FateLensFlare.DrawHeatWaveDistortion(Projectile.Center, 80f * intensity, 0.6f * intensity);
            
            // === FIRE TIMELINE ARROWS ===
            if (intensity > 0.3f && arrowsFired < MaxArrows && Main.GameUpdateCount % 7 == 0)
            {
                // Find target direction (toward nearest enemy or random)
                Vector2 fireDir = -Vector2.UnitY;
                float nearestDist = 800f;
                
                foreach (NPC npc in Main.npc)
                {
                    if (!npc.active || npc.friendly || npc.CountsAsACritter) continue;
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        fireDir = (npc.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    }
                }
                
                // Add some spread
                fireDir = fireDir.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f));
                
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), 
                    Projectile.Center + Main.rand.NextVector2Circular(30f, 30f),
                    fireDir * 16f, ModContent.ProjectileType<TimelineArrowRain>(),
                    Projectile.damage, Projectile.knockBack, Projectile.owner);
                
                arrowsFired++;
                
                // Arrow spawn VFX
                CustomParticles.GenericFlare(Projectile.Center, FateLensFlare.FateWhite, 0.4f, 10);
                SoundEngine.PlaySound(SoundID.Item5 with { Volume = 0.5f, Pitch = 0.3f }, Projectile.Center);
            }
            
            Lighting.AddLight(Projectile.Center, FateLensFlare.FateBrightRed.ToVector3() * intensity * 0.6f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            
            float progress = 1f - (float)Projectile.timeLeft / ActiveDuration;
            float openProgress = Math.Min(progress * 4f, 1f);
            float closeProgress = Math.Max((progress - 0.75f) * 4f, 0f);
            float intensity = openProgress * (1f - closeProgress);
            
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Outer ring
            sb.Draw(glow, drawPos, null, FateLensFlare.FatePurple * 0.4f * intensity,
                Main.GameUpdateCount * 0.05f, glow.Size() / 2f, 1.5f * intensity, SpriteEffects.None, 0f);
            
            // Inner swirl
            sb.Draw(glow, drawPos, null, FateLensFlare.FateDarkPink * 0.5f * intensity,
                -Main.GameUpdateCount * 0.08f, glow.Size() / 2f, 1f * intensity, SpriteEffects.None, 0f);
            
            // Core
            sb.Draw(glow, drawPos, null, FateLensFlare.FateBrightRed * 0.6f * intensity,
                0f, glow.Size() / 2f, 0.5f * intensity, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    public class TimelineArrowRain : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            if (Main.rand.NextBool(3))
            {
                Color trailColor = FateLensFlare.GetFateGradient(Main.rand.NextFloat()) * 0.4f;
                CustomParticles.GenericFlare(Projectile.Center, trailColor, 0.12f, 8);
            }
            
            Lighting.AddLight(Projectile.Center, FateLensFlare.FateDarkPink.ToVector3() * 0.2f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float alpha = 1f - (float)i / Projectile.oldPos.Length;
                Color trailColor = FateLensFlare.GetFateGradient((float)i / Projectile.oldPos.Length) * alpha * 0.3f;
                sb.Draw(glow, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null, 
                    trailColor, Projectile.oldRot[i], glow.Size() / 2f, new Vector2(0.3f, 0.1f), SpriteEffects.None, 0f);
            }
            
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, FateLensFlare.FateDarkPink * 0.6f,
                Projectile.rotation, glow.Size() / 2f, new Vector2(0.4f, 0.15f), SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 120);
            CustomParticles.GenericFlare(target.Center, FateLensFlare.FateBrightRed, 0.3f, 12);
        }
    }
}
