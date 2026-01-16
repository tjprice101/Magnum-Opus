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
    /// DESTINY'S HAMMER - Melee Hammer #4
    /// 
    /// UNIQUE ABILITY: "REALITY SHATTER"
    /// Ground slams create expanding shockwaves of kaleidoscopic energy.
    /// Hitting airborne enemies causes them to crash down with bonus damage.
    /// 
    /// PASSIVE: Heavy attacks (every 3rd hit) create "fate fractures" - 
    /// visual cracks in reality that persist briefly and damage enemies.
    /// </summary>
    public class Fate4 : ModItem
    {
        private int swingCounter = 0;
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.SolarEruption;
        
        public override void SetDefaults()
        {
            Item.damage = 820;
            Item.DamageType = DamageClass.Melee;
            Item.width = 70;
            Item.height = 70;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 12f;
            Item.value = Item.sellPrice(gold: 30);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FateShockwave>();
            Item.shootSpeed = 0f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Swings create expanding shockwaves of kaleidoscopic energy"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "Every 3rd hit creates persistent reality fractures"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect3", "Airborne enemies are slammed down for bonus damage"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'Each swing reshapes the cosmos'") { OverrideColor = FateLensFlare.FateBrightRed });
        }
        
        public override void HoldItem(Player player)
        {
            Vector2 hammerHead = player.Center + new Vector2(player.direction * 40f, -20f);
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.3f + 0.7f;
            
            // Heavy gravitational presence
            FateLensFlare.DrawHeatWaveDistortion(hammerHead, 30f, 0.5f);
            
            // Orbiting cosmic debris
            float orbitTime = Main.GameUpdateCount * 0.02f;
            for (int i = 0; i < 3; i++)
            {
                float angle = orbitTime + MathHelper.TwoPi * i / 3f;
                Vector2 debrisPos = hammerHead + angle.ToRotationVector2() * 25f;
                if (Main.rand.NextBool(6))
                {
                    CustomParticles.GenericFlare(debrisPos, FateLensFlare.GetFateGradient((float)i / 3f) * 0.5f, 0.2f, 10);
                }
            }
            
            // Ambient lens flare
            if (Main.GameUpdateCount % 35 == 0)
                FateLensFlareDrawLayer.AddFlare(hammerHead, 0.35f * pulse, 0.4f, 15);
            
            Lighting.AddLight(hammerHead, FateLensFlare.FatePurple.ToVector3() * 0.5f * pulse);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            swingCounter++;
            
            // Ground slam shockwave
            Vector2 groundPos = player.Bottom + new Vector2(player.direction * 30f, 0f);
            Projectile.NewProjectile(source, groundPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            // Heavy impact VFX
            FateLensFlareDrawLayer.AddFlare(groundPos, 1f, 0.9f, 30);
            FateLensFlare.KaleidoscopeBurst(groundPos, 1f, 8);
            
            // Every 3rd hit - create fate fractures
            if (swingCounter >= 3)
            {
                swingCounter = 0;
                
                // Spawn persistent fractures
                for (int i = 0; i < 5; i++)
                {
                    float angle = MathHelper.ToRadians(-60f + i * 30f);
                    Vector2 fractureDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle) - 0.5f);
                    Projectile.NewProjectile(source, groundPos, fractureDir * 8f,
                        ModContent.ProjectileType<FateFractureLine>(), (int)(damage * 0.5f), 0f, player.whoAmI);
                }
                
                FateLensFlare.RealityFracture(groundPos, 120f, 10);
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.4f }, groundPos);
            }
            
            return false;
        }
        
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            
            // Airborne slam - extra damage and force down
            if (!target.noGravity && target.velocity.Y != 0)
            {
                target.velocity.Y += 20f; // Slam down
                
                // Bonus impact
                FateLensFlareDrawLayer.AddFlare(target.Center, 1.2f, 0.9f, 30);
                FateLensFlare.KaleidoscopeBurst(target.Center, 1f, 6);
                
                // Deal bonus damage
                target.SimpleStrikeNPC((int)(damageDone * 0.5f), hit.HitDirection, false, 0f);
            }
            
            FateLensFlare.KaleidoscopeBurst(target.Center, 0.6f, 4);
        }
    }
    
    public class FateShockwave : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private const int ShockwaveDuration = 30;
        
        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = ShockwaveDuration;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = ShockwaveDuration;
        }
        
        public override void AI()
        {
            float progress = 1f - (float)Projectile.timeLeft / ShockwaveDuration;
            float expansion = progress * 300f; // Expands outward
            float intensity = 1f - progress;
            
            Projectile.width = (int)(100 + expansion * 2);
            Projectile.height = 40;
            Projectile.Center = Projectile.Center; // Recenter
            
            // Kaleidoscopic shockwave visuals
            int particleCount = (int)(12 * intensity);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.Pi + MathHelper.Pi * (float)i / particleCount; // Semi-circle upward
                float waveRadius = expansion;
                Vector2 wavePos = Projectile.Center + new Vector2((float)Math.Cos(angle) * waveRadius, -5f);
                
                Color waveColor = FateLensFlare.GetFateGradient((float)i / particleCount);
                
                if (Main.rand.NextBool(2))
                {
                    CustomParticles.GenericFlare(wavePos, waveColor * intensity, 0.3f * intensity, 8);
                    
                    // Chromatic aberration
                    CustomParticles.GenericFlare(wavePos + new Vector2(-3, 0), Color.Red * 0.3f * intensity, 0.15f, 6);
                    CustomParticles.GenericFlare(wavePos + new Vector2(3, 0), FateLensFlare.FateCyan * 0.3f * intensity, 0.15f, 6);
                }
            }
            
            // Rising particles
            if (Main.rand.NextBool(2))
            {
                Vector2 risePos = Projectile.Center + new Vector2(Main.rand.NextFloat(-expansion, expansion), 0f);
                var rise = new GenericGlowParticle(risePos, new Vector2(0, -Main.rand.NextFloat(2f, 5f)),
                    FateLensFlare.GetFateGradient(Main.rand.NextFloat()) * intensity, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(rise);
            }
            
            Lighting.AddLight(Projectile.Center, FateLensFlare.FateBrightRed.ToVector3() * intensity * 0.5f);
        }
        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float progress = 1f - (float)Projectile.timeLeft / ShockwaveDuration;
            float expansion = progress * 300f;
            
            // Check if target is within the expanding wave
            float distX = Math.Abs(targetHitbox.Center.X - Projectile.Center.X);
            float distY = targetHitbox.Bottom - Projectile.Center.Y;
            
            return distX < expansion + targetHitbox.Width / 2 && distY > -50 && distY < 100;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);
            FateLensFlare.KaleidoscopeBurst(target.Center, 0.4f, 3);
        }
    }
    
    public class FateFractureLine : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private const int FractureDuration = 90;
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = FractureDuration;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }
        
        public override void AI()
        {
            Projectile.velocity *= 0.92f;
            
            float progress = 1f - (float)Projectile.timeLeft / FractureDuration;
            float intensity = (float)Math.Sin(progress * MathHelper.Pi);
            
            // Fracture line visuals
            if (Main.rand.NextBool(2))
            {
                Color fractureColor = Color.Lerp(FateLensFlare.FateWhite, FateLensFlare.FateBrightRed, progress);
                CustomParticles.GenericFlare(Projectile.Center, fractureColor * intensity, 0.25f, 8);
                
                // Jagged edges
                Vector2 edgeOffset = Main.rand.NextVector2Circular(8f, 8f);
                CustomParticles.GenericFlare(Projectile.Center + edgeOffset, FateLensFlare.FateDarkPink * intensity * 0.5f, 0.15f, 6);
            }
            
            // Chromatic bleed
            if (Main.rand.NextBool(4))
            {
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(-2, 0), Color.Red * 0.4f * intensity, 0.1f, 5);
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(2, 0), FateLensFlare.FateCyan * 0.4f * intensity, 0.1f, 5);
            }
            
            Lighting.AddLight(Projectile.Center, FateLensFlare.FateBrightRed.ToVector3() * intensity * 0.3f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            
            float progress = 1f - (float)Projectile.timeLeft / FractureDuration;
            float intensity = (float)Math.Sin(progress * MathHelper.Pi);
            
            // Jagged line
            Color lineColor = Color.Lerp(FateLensFlare.FateWhite, FateLensFlare.FateBrightRed, progress);
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, lineColor * 0.6f * intensity,
                Projectile.velocity.ToRotation(), glow.Size() / 2f, new Vector2(0.8f, 0.1f), SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 120);
        }
    }
}
