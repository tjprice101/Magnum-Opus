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
    /// DESTINY CLEAVER - Melee Sword #1
    /// 
    /// UNIQUE ABILITY: "FATE SEVER"
    /// Every 5th strike tears a rift in reality that persists for 3 seconds,
    /// damaging all enemies that touch it. The rift has a kaleidoscopic shimmer
    /// and creates lens flares at both endpoints.
    /// 
    /// PASSIVE: Swings leave "temporal echoes" - ghostly afterimages that
    /// deal 30% damage to enemies they pass through.
    /// </summary>
    public class Fate1 : ModItem
    {
        private int strikeCounter = 0;
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.Meowmere;
        
        public override void SetDefaults()
        {
            Item.damage = 680;
            Item.DamageType = DamageClass.Melee;
            Item.width = 70;
            Item.height = 70;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(gold: 30);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<TemporalEchoSlash>();
            Item.shootSpeed = 14f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Swings create temporal echoes that damage enemies"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "Every 5th strike tears a reality rift that persists and damages enemies"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'The blade that severs destiny itself'") { OverrideColor = FateLensFlare.FateBrightRed });
        }
        
        public override void HoldItem(Player player)
        {
            Vector2 bladeTip = player.Center + new Vector2(player.direction * 45f, -25f);
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.3f + 0.7f;
            
            if (Main.GameUpdateCount % 30 == 0)
                FateLensFlareDrawLayer.AddFlare(bladeTip, 0.3f * pulse, 0.5f, 20);
            
            FateLensFlare.ChromaticShift(player.Center, 40f, 0.5f);
            
            if (Main.rand.NextBool(4))
                FateLensFlare.DrawHeatWaveDistortion(bladeTip, 25f, 0.6f);
            
            float orbitTime = Main.GameUpdateCount * 0.025f;
            for (int i = 0; i < 4; i++)
            {
                float angle = orbitTime + MathHelper.TwoPi * i / 4f;
                Vector2 glyphPos = player.Center + angle.ToRotationVector2() * 35f;
                if (Main.rand.NextBool(8))
                    CustomParticles.Glyph(glyphPos, FateLensFlare.GetFateGradient((float)i / 4f) * 0.6f, 0.25f, -1);
            }
            
            Lighting.AddLight(bladeTip, FateLensFlare.FateDarkPink.ToVector3() * 0.5f * pulse);
        }
        
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            strikeCounter++;
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            
            FateLensFlareDrawLayer.AddFlare(target.Center, 0.8f, 0.7f, 25);
            FateLensFlare.KaleidoscopeBurst(target.Center, 0.8f, 6);
            
            CustomParticles.GenericFlare(target.Center, FateLensFlare.FateWhite, 0.7f, 18);
            CustomParticles.GenericFlare(target.Center, FateLensFlare.FateBrightRed, 0.55f, 15);
            CustomParticles.HaloRing(target.Center, FateLensFlare.FateDarkPink, 0.4f, 14);
            
            if (strikeCounter >= 5)
            {
                strikeCounter = 0;
                Vector2 riftDirection = (target.Center - player.Center).SafeNormalize(Vector2.UnitX);
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), target.Center, riftDirection,
                    ModContent.ProjectileType<RealityRift>(), Item.damage * 2, 10f, player.whoAmI);
                
                FateLensFlareDrawLayer.AddFlare(target.Center, 1.5f, 1.2f, 45);
                FateLensFlare.RealityFracture(target.Center, 100f, 8);
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f }, target.Center);
            }
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, (int)(damage * 0.3f), knockback * 0.5f, player.whoAmI);
            return false;
        }
    }
    
    public class TemporalEchoSlash : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 40;
            Projectile.tileCollide = false;
            Projectile.alpha = 100;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.alpha += 3;
            if (Projectile.alpha > 255) Projectile.Kill();
            
            float alpha = 1f - Projectile.alpha / 255f;
            if (Main.rand.NextBool(2))
            {
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    FateLensFlare.GetFateGradient(Main.rand.NextFloat()) * alpha, 0.25f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(-3, 0), Color.Red * 0.3f * alpha, 0.15f, 8);
                CustomParticles.GenericFlare(Projectile.Center + new Vector2(3, 0), FateLensFlare.FateCyan * 0.3f * alpha, 0.15f, 8);
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            float alpha = 1f - Projectile.alpha / 255f;
            
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, FateLensFlare.FateDarkPink * 0.4f * alpha,
                Projectile.rotation, glow.Size() / 2f, new Vector2(1.5f, 0.4f), SpriteEffects.None, 0f);
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, FateLensFlare.FateBrightRed * 0.6f * alpha,
                Projectile.rotation, glow.Size() / 2f, new Vector2(1.2f, 0.3f), SpriteEffects.None, 0f);
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, Color.White * 0.4f * alpha,
                Projectile.rotation, glow.Size() / 2f, new Vector2(0.8f, 0.15f), SpriteEffects.None, 0f);
            return false;
        }
    }
    
    public class RealityRift : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        private const int RiftDuration = 180;
        
        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = RiftDuration;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        
        public override void AI()
        {
            Projectile.velocity *= 0.95f;
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            float progress = 1f - (float)Projectile.timeLeft / RiftDuration;
            float intensity = (float)Math.Sin(progress * MathHelper.Pi);
            
            for (int side = -1; side <= 1; side += 2)
            {
                Vector2 edgeOffset = (Projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * side * 15f;
                Vector2 edgePos = Projectile.Center + edgeOffset + Projectile.rotation.ToRotationVector2() * Main.rand.NextFloat(-80f, 80f);
                
                if (Main.rand.NextBool(2))
                {
                    Color edgeColor = FateLensFlare.GetFateGradient(Main.rand.NextFloat()) * intensity;
                    CustomParticles.GenericFlare(edgePos, edgeColor, 0.2f * intensity, 10);
                    CustomParticles.GenericFlare(edgePos + new Vector2(-2, 0), Color.Red * 0.3f * intensity, 0.12f, 8);
                    CustomParticles.GenericFlare(edgePos + new Vector2(2, 0), FateLensFlare.FateCyan * 0.3f * intensity, 0.12f, 8);
                }
            }
            
            if (Main.rand.NextBool(3))
            {
                Vector2 riftPoint = Projectile.Center + Projectile.rotation.ToRotationVector2() * Main.rand.NextFloat(-90f, 90f);
                CustomParticles.GenericFlare(riftPoint, FateLensFlare.FateWhite * 0.6f * intensity, 0.35f, 12);
            }
            
            if (Main.GameUpdateCount % 20 == 0)
            {
                Vector2 endPoint1 = Projectile.Center + Projectile.rotation.ToRotationVector2() * 90f;
                Vector2 endPoint2 = Projectile.Center - Projectile.rotation.ToRotationVector2() * 90f;
                FateLensFlareDrawLayer.AddFlare(endPoint1, 0.5f * intensity, 0.6f, 15);
                FateLensFlareDrawLayer.AddFlare(endPoint2, 0.5f * intensity, 0.6f, 15);
            }
            
            if (Main.rand.NextBool(10))
                FateLensFlare.RealityFracture(Projectile.Center, 50f * intensity, 2);
            
            Lighting.AddLight(Projectile.Center, FateLensFlare.FateBrightRed.ToVector3() * intensity);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            
            float progress = 1f - (float)Projectile.timeLeft / RiftDuration;
            float intensity = (float)Math.Sin(progress * MathHelper.Pi);
            float time = Main.GameUpdateCount * 0.03f;
            
            for (int layer = 0; layer < 5; layer++)
            {
                float layerProgress = layer / 5f;
                Color layerColor = FateLensFlare.GetFateGradient((layerProgress + time) % 1f);
                float scale = 1f - layerProgress * 0.15f;
                sb.Draw(glow, Projectile.Center - Main.screenPosition, null, layerColor * 0.3f * intensity,
                    Projectile.rotation, glow.Size() / 2f, new Vector2(4f * scale, 0.15f + layerProgress * 0.1f), SpriteEffects.None, 0f);
            }
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, Color.White * 0.5f * intensity,
                Projectile.rotation, glow.Size() / 2f, new Vector2(3.5f, 0.08f), SpriteEffects.None, 0f);
            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);
            FateLensFlare.KaleidoscopeBurst(target.Center, 0.5f, 4);
        }
    }
}
