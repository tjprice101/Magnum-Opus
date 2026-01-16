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
    /// COSMIC EXECUTIONER - Melee Greatsword #2
    /// 
    /// UNIQUE ABILITY: "TIMELINE CONVERGENCE"
    /// Charged attacks (hold use) summon 4 phantom blades from alternate timelines
    /// that all strike the same point simultaneously with a massive lens flare explosion.
    /// 
    /// PASSIVE: Every swing has a 15% chance to "blink" the player forward through the swing,
    /// granting brief invincibility and increased damage.
    /// </summary>
    public class Fate2 : ModItem
    {
        private int chargeTime = 0;
        private const int MaxCharge = 60;
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.TrueNightsEdge;
        
        public override void SetDefaults()
        {
            Item.damage = 750;
            Item.DamageType = DamageClass.Melee;
            Item.width = 80;
            Item.height = 80;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 9f;
            Item.value = Item.sellPrice(gold: 32);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noUseGraphic = false;
            Item.shoot = ModContent.ProjectileType<CosmicExecutionerSlash>();
            Item.shootSpeed = 12f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Hold attack to charge Timeline Convergence"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "Fully charged: Summon 4 phantom blades that strike together"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect3", "15% chance on swing to blink forward with invincibility"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'Four fates, one doom'") { OverrideColor = FateLensFlare.FateBrightRed });
        }
        
        public override void HoldItem(Player player)
        {
            Vector2 bladeTip = player.Center + new Vector2(player.direction * 55f, -30f);
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.25f + 0.75f;
            
            // Charging visual
            if (player.channel && player.itemAnimation > 0)
            {
                chargeTime = Math.Min(chargeTime + 1, MaxCharge);
                float chargeProgress = (float)chargeTime / MaxCharge;
                
                // Growing lens flare as charge builds
                if (Main.GameUpdateCount % 10 == 0)
                    FateLensFlareDrawLayer.AddFlare(bladeTip, 0.3f + chargeProgress * 0.7f, 0.4f + chargeProgress * 0.6f, 15);
                
                // Phantom blade previews
                if (chargeProgress > 0.5f)
                {
                    float phantomAlpha = (chargeProgress - 0.5f) * 2f;
                    for (int i = 0; i < 4; i++)
                    {
                        float phantomAngle = MathHelper.TwoPi * i / 4f + Main.GameUpdateCount * 0.02f;
                        Vector2 phantomPos = player.Center + phantomAngle.ToRotationVector2() * (60f + chargeProgress * 30f);
                        
                        if (Main.rand.NextBool(3))
                        {
                            Color phantomColor = FateLensFlare.GetFateGradient((float)i / 4f) * phantomAlpha * 0.5f;
                            CustomParticles.GenericFlare(phantomPos, phantomColor, 0.3f, 10);
                        }
                    }
                }
                
                // Event horizon effect at full charge
                if (chargeProgress >= 1f)
                    FateLensFlare.EventHorizon(bladeTip, 35f, 0.8f);
                
                // Heat wave distortion
                FateLensFlare.DrawHeatWaveDistortion(bladeTip, 30f + chargeProgress * 20f, chargeProgress);
            }
            else
            {
                chargeTime = 0;
            }
            
            // Passive chromatic aura
            FateLensFlare.ChromaticShift(player.Center, 45f, 0.4f);
            
            Lighting.AddLight(bladeTip, FateLensFlare.FateDarkPink.ToVector3() * 0.6f * pulse);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Check if fully charged
            if (chargeTime >= MaxCharge)
            {
                // TIMELINE CONVERGENCE - 4 phantom blades
                Vector2 target = Main.MouseWorld;
                
                for (int i = 0; i < 4; i++)
                {
                    float phantomAngle = MathHelper.TwoPi * i / 4f;
                    Vector2 phantomStart = target + phantomAngle.ToRotationVector2() * 250f;
                    Vector2 phantomVel = (target - phantomStart).SafeNormalize(Vector2.Zero) * 25f;
                    
                    Projectile.NewProjectile(source, phantomStart, phantomVel, 
                        ModContent.ProjectileType<PhantomTimelineBlade>(), damage * 2, knockback * 2f, player.whoAmI, i);
                }
                
                // Massive convergence point lens flare
                FateLensFlareDrawLayer.AddFlare(target, 2f, 1.5f, 60);
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f }, target);
                
                chargeTime = 0;
                return false;
            }
            
            // Normal swing with blink chance
            if (Main.rand.NextFloat() < 0.15f)
            {
                // BLINK STRIKE
                Vector2 blinkDirection = velocity.SafeNormalize(Vector2.UnitX);
                Vector2 blinkTarget = player.Center + blinkDirection * 120f;
                
                // Check for solid tiles
                if (!Collision.SolidCollision(blinkTarget - player.Size / 2f, player.width, player.height))
                {
                    // Departure effect
                    FateLensFlare.KaleidoscopeBurst(player.Center, 0.6f, 4);
                    
                    // Teleport
                    player.Teleport(blinkTarget, TeleportationStyleID.RodOfDiscord);
                    player.immune = true;
                    player.immuneTime = 20;
                    
                    // Arrival effect
                    FateLensFlareDrawLayer.AddFlare(blinkTarget, 1f, 0.8f, 30);
                    FateLensFlare.KaleidoscopeBurst(blinkTarget, 0.8f, 6);
                    
                    // Empowered slash
                    Projectile.NewProjectile(source, blinkTarget, velocity * 1.5f, type, (int)(damage * 1.5f), knockback, player.whoAmI);
                    return false;
                }
            }
            
            // Normal slash
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
        
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            FateLensFlareDrawLayer.AddFlare(target.Center, 0.7f, 0.6f, 20);
            FateLensFlare.KaleidoscopeBurst(target.Center, 0.7f, 5);
        }
    }
    
    public class CosmicExecutionerSlash : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 35;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.alpha += 5;
            if (Projectile.alpha > 255) Projectile.Kill();
            
            float alpha = 1f - Projectile.alpha / 255f;
            
            // Heavy chromatic trail
            if (Main.rand.NextBool(2))
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(15f, 15f);
                    var trail = new GenericGlowParticle(Projectile.Center + offset, -Projectile.velocity * 0.15f,
                        FateLensFlare.GetFateGradient(Main.rand.NextFloat()) * alpha, 0.3f, 15, true);
                    MagnumParticleHandler.SpawnParticle(trail);
                }
                
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(-4, 0), Color.Red * 0.4f * alpha, 0.2f, 10);
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(4, 0), FateLensFlare.FateCyan * 0.4f * alpha, 0.2f, 10);
            }
            
            Lighting.AddLight(Projectile.Center, FateLensFlare.FateBrightRed.ToVector3() * alpha);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            float alpha = 1f - Projectile.alpha / 255f;
            
            // Large sweeping arc
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, FateLensFlare.FateDarkPink * 0.5f * alpha,
                Projectile.rotation, glow.Size() / 2f, new Vector2(2f, 0.6f), SpriteEffects.None, 0f);
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, FateLensFlare.FateBrightRed * 0.7f * alpha,
                Projectile.rotation, glow.Size() / 2f, new Vector2(1.6f, 0.4f), SpriteEffects.None, 0f);
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, Color.White * 0.5f * alpha,
                Projectile.rotation, glow.Size() / 2f, new Vector2(1.2f, 0.2f), SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);
            FateLensFlare.KaleidoscopeBurst(target.Center, 0.5f, 4);
        }
    }
    
    public class PhantomTimelineBlade : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private int TimelineIndex => (int)Projectile.ai[0];
        
        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 2;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Each phantom has slightly different color
            float colorOffset = TimelineIndex / 4f;
            Color phantomColor = FateLensFlare.GetFateGradient((colorOffset + Main.GameUpdateCount * 0.01f) % 1f);
            
            // Ghost trail
            if (Main.rand.NextBool(2))
            {
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    phantomColor * 0.7f, 0.3f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Lens flare at blade tip
            if (Main.GameUpdateCount % 5 == TimelineIndex)
                FateLensFlareDrawLayer.AddFlare(Projectile.Center, 0.4f, 0.4f, 8);
            
            Lighting.AddLight(Projectile.Center, phantomColor.ToVector3() * 0.5f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            
            float colorOffset = TimelineIndex / 4f;
            Color phantomColor = FateLensFlare.GetFateGradient((colorOffset + Main.GameUpdateCount * 0.01f) % 1f);
            
            // Ghostly blade
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, phantomColor * 0.4f,
                Projectile.rotation, glow.Size() / 2f, new Vector2(1.8f, 0.5f), SpriteEffects.None, 0f);
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, Color.White * 0.3f,
                Projectile.rotation, glow.Size() / 2f, new Vector2(1.2f, 0.2f), SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 240);
            FateLensFlareDrawLayer.AddFlare(target.Center, 1f, 0.8f, 25);
            FateLensFlare.KaleidoscopeBurst(target.Center, 0.6f, 4);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Convergence explosion when blade "hits" the target point
            FateLensFlare.KaleidoscopeBurst(Projectile.Center, 1f, 8);
            CustomParticles.GenericFlare(Projectile.Center, FateLensFlare.FateWhite, 1f, 20);
            
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 sparkVel = angle.ToRotationVector2() * 6f;
                var spark = new GlowSparkParticle(Projectile.Center, sparkVel, FateLensFlare.GetFateGradient((float)i / 8f), 0.4f, 15);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }
    }
}
