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
    /// SINGULARITY SPEAR - Melee Spear #3
    /// 
    /// UNIQUE ABILITY: "GRAVITATIONAL IMPALE"
    /// Thrust creates a micro-singularity at the tip that pulls in enemies.
    /// Enemies pulled in take 150% damage and are briefly stunned.
    /// 
    /// PASSIVE: Spear tip creates an event horizon effect - a swirling darkness
    /// with bright accretion disk that distorts light around it.
    /// </summary>
    public class Fate3 : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.NorthPole;
        
        public override void SetDefaults()
        {
            Item.damage = 620;
            Item.DamageType = DamageClass.Melee;
            Item.width = 60;
            Item.height = 60;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 28);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<SingularitySpearProj>();
            Item.shootSpeed = 8f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Thrusts create micro-singularities that pull in enemies"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "Pulled enemies take 150% damage and are briefly stunned"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'A black hole on a stick'") { OverrideColor = FateLensFlare.FateBrightRed });
        }
        
        public override void HoldItem(Player player)
        {
            Vector2 spearTip = player.Center + new Vector2(player.direction * 50f, 0f);
            
            // Constant event horizon effect
            FateLensFlare.EventHorizon(spearTip, 25f, 0.5f);
            
            // Gravitational distortion
            if (Main.rand.NextBool(3))
                FateLensFlare.DrawHeatWaveDistortion(spearTip, 35f, 0.6f);
            
            // Ambient lens flare
            if (Main.GameUpdateCount % 25 == 0)
                FateLensFlareDrawLayer.AddFlare(spearTip, 0.4f, 0.5f, 18);
            
            Lighting.AddLight(spearTip, FateLensFlare.FateDarkPink.ToVector3() * 0.4f);
        }
        
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<SingularitySpearProj>()] < 1;
        }
    }
    
    public class SingularitySpearProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.NorthPoleSpear;
        
        private bool hasCreatedSingularity = false;
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.aiStyle = 19;
            Projectile.hide = true;
        }
        
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            // Spear movement
            Vector2 mountedCenter = player.RotatedRelativePoint(player.MountedCenter);
            Projectile.direction = player.direction;
            player.heldProj = Projectile.whoAmI;
            Projectile.Center = mountedCenter;
            
            if (!player.frozen)
            {
                if (Projectile.ai[0] == 0f)
                {
                    Projectile.ai[0] = 3f;
                    Projectile.netUpdate = true;
                }
                if (player.itemAnimation < player.itemAnimationMax / 3)
                    Projectile.ai[0] -= 2.4f;
                else
                    Projectile.ai[0] += 2.1f;
            }
            
            Projectile.position += Projectile.velocity * Projectile.ai[0];
            
            // Spear tip position
            Vector2 tipPos = Projectile.Center + Projectile.velocity * 40f;
            
            // Create singularity at max extension
            if (!hasCreatedSingularity && player.itemAnimation < player.itemAnimationMax / 2)
            {
                hasCreatedSingularity = true;
                
                // Spawn gravitational singularity
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, Vector2.Zero,
                    ModContent.ProjectileType<MicroSingularity>(), Projectile.damage, 0f, Projectile.owner);
                
                // Massive lens flare on singularity creation
                FateLensFlareDrawLayer.AddFlare(tipPos, 1.2f, 1f, 35);
                SoundEngine.PlaySound(SoundID.Item117 with { Pitch = -0.4f }, tipPos);
            }
            
            // Constant event horizon at tip
            FateLensFlare.EventHorizon(tipPos, 20f, 0.7f);
            
            // Chromatic trail
            if (Main.rand.NextBool(2))
            {
                var trail = new GenericGlowParticle(tipPos + Main.rand.NextVector2Circular(10f, 10f),
                    -Projectile.velocity * 0.2f, FateLensFlare.GetFateGradient(Main.rand.NextFloat()), 0.25f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Handle animation end
            if (player.itemAnimation == 0)
                Projectile.Kill();
            
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 * 3f;
            Lighting.AddLight(tipPos, FateLensFlare.FateBrightRed.ToVector3() * 0.6f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 240);
            FateLensFlare.KaleidoscopeBurst(target.Center, 0.6f, 5);
        }
    }
    
    public class MicroSingularity : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private const int SingularityDuration = 45;
        private const float PullRadius = 180f;
        private const float PullForce = 8f;
        
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = SingularityDuration;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        
        public override void AI()
        {
            float progress = 1f - (float)Projectile.timeLeft / SingularityDuration;
            float intensity = (float)Math.Sin(progress * MathHelper.Pi);
            
            // === GRAVITATIONAL PULL ===
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.boss) continue;
                
                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (distance < PullRadius && distance > 30f)
                {
                    Vector2 pullDirection = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                    float pullStrength = PullForce * (1f - distance / PullRadius) * intensity;
                    npc.velocity += pullDirection * pullStrength;
                    
                    // Visual pull effect
                    if (Main.rand.NextBool(4))
                    {
                        Vector2 effectPos = Vector2.Lerp(npc.Center, Projectile.Center, 0.3f);
                        CustomParticles.GenericFlare(effectPos, FateLensFlare.GetFateGradient(Main.rand.NextFloat()) * 0.5f, 0.15f, 8);
                    }
                }
            }
            
            // === VISUAL EFFECTS ===
            
            // Central void
            CustomParticles.GenericFlare(Projectile.Center, FateLensFlare.FateBlack, 0.5f * intensity, 6);
            
            // Accretion disk
            float time = Main.GameUpdateCount * 0.1f;
            int diskParticles = (int)(10 * intensity);
            for (int i = 0; i < diskParticles; i++)
            {
                float angle = time + MathHelper.TwoPi * i / diskParticles;
                float diskRadius = 25f + (float)Math.Sin(time + i) * 8f;
                Vector2 diskPos = Projectile.Center + angle.ToRotationVector2() * diskRadius;
                
                Color diskColor = FateLensFlare.GetFateGradient((float)i / diskParticles);
                var disk = new GenericGlowParticle(diskPos, (angle + MathHelper.PiOver2).ToRotationVector2() * 3f,
                    diskColor * 0.6f * intensity, 0.15f, 10, true);
                MagnumParticleHandler.SpawnParticle(disk);
            }
            
            // Event horizon shimmer
            FateLensFlare.EventHorizon(Projectile.Center, 30f * intensity, intensity);
            
            // Lens flare pulses
            if (Main.GameUpdateCount % 8 == 0)
                FateLensFlareDrawLayer.AddFlare(Projectile.Center, 0.6f * intensity, 0.7f * intensity, 12);
            
            // Reality fractures
            if (Main.rand.NextBool(8))
                FateLensFlare.RealityFracture(Projectile.Center, 40f * intensity, 2);
            
            Lighting.AddLight(Projectile.Center, FateLensFlare.FateDarkPink.ToVector3() * intensity);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            
            float progress = 1f - (float)Projectile.timeLeft / SingularityDuration;
            float intensity = (float)Math.Sin(progress * MathHelper.Pi);
            float time = Main.GameUpdateCount * 0.03f;
            
            // Dark core
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, FateLensFlare.FateBlack * 0.8f * intensity,
                0f, glow.Size() / 2f, 0.4f, SpriteEffects.None, 0f);
            
            // Bright ring
            for (int ring = 0; ring < 3; ring++)
            {
                float ringProgress = ring / 3f;
                Color ringColor = FateLensFlare.GetFateGradient((ringProgress + time) % 1f);
                sb.Draw(glow, Projectile.Center - Main.screenPosition, null, ringColor * 0.4f * intensity,
                    0f, glow.Size() / 2f, 0.6f + ring * 0.15f, SpriteEffects.None, 0f);
            }
            
            return false;
        }
        
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 150% damage for pulled enemies
            modifiers.FinalDamage *= 1.5f;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            target.AddBuff(BuffID.Slow, 60); // Stunned effect
            
            FateLensFlareDrawLayer.AddFlare(target.Center, 0.8f, 0.6f, 20);
            FateLensFlare.KaleidoscopeBurst(target.Center, 0.7f, 5);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Singularity collapse
            FateLensFlareDrawLayer.AddFlare(Projectile.Center, 1.5f, 1.2f, 40);
            FateLensFlare.KaleidoscopeBurst(Projectile.Center, 1.2f, 10);
            
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 burstVel = angle.ToRotationVector2() * 8f;
                var burst = new GlowSparkParticle(Projectile.Center, burstVel, FateLensFlare.GetFateGradient((float)i / 12f), 0.4f, 20);
                MagnumParticleHandler.SpawnParticle(burst);
            }
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f }, Projectile.Center);
        }
    }
}
