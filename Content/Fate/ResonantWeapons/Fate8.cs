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
    /// TOME OF INFINITE FATES - Magic Weapon #1
    /// 
    /// UNIQUE ABILITY: "REALITY MANUSCRIPT"
    /// Left click fires seeking fate orbs that mark enemies.
    /// Right click triggers "READ THE STARS" - all marked enemies take
    /// massive damage as reality itself judges them. Kaleidoscopic star
    /// symbols appear above marked enemies and crash down.
    /// 
    /// PASSIVE: Marked enemies have visible fate threads connecting them.
    /// Maximum spectacle when multiple enemies are marked and judged together.
    /// </summary>
    public class Fate8 : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.LastPrism;
        
        public override void SetDefaults()
        {
            Item.damage = 250;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 15;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item105;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FateOrbSeeker>();
            Item.shootSpeed = 16f;
            Item.noMelee = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Left click fires seeking orbs that mark enemies"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "Right click reads the stars, judging all marked foes"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect3", "Marked enemies are visibly connected by fate threads"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'The book that writes the endings of all things'") { OverrideColor = FateLensFlare.FateBrightRed });
        }
        
        public override void HoldItem(Player player)
        {
            Vector2 bookPos = player.Center + new Vector2(player.direction * 25f, -8f);
            
            // Floating pages/glyphs around the tome
            if (Main.rand.NextBool(8))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pagePos = bookPos + angle.ToRotationVector2() * Main.rand.NextFloat(20f, 35f);
                Color pageColor = FateLensFlare.GetFateGradient(Main.rand.NextFloat()) * 0.4f;
                
                var page = new GenericGlowParticle(pagePos, angle.ToRotationVector2() * 0.3f,
                    pageColor, 0.15f, 25, true);
                MagnumParticleHandler.SpawnParticle(page);
            }
            
            // Ambient lens flare
            if (Main.GameUpdateCount % 30 == 0)
                FateLensFlareDrawLayer.AddFlare(bookPos, 0.25f, 0.3f, 12);
            
            // Draw fate threads to nearby marked enemies
            if (Main.GameUpdateCount % 3 == 0)
            {
                foreach (NPC npc in Main.npc)
                {
                    if (!npc.active || npc.friendly) continue;
                    if (npc.HasBuff(ModContent.BuffType<FateMarked>()))
                    {
                        // Draw thread line
                        Vector2 toTarget = (npc.Center - bookPos);
                        for (int i = 0; i < 5; i++)
                        {
                            float progress = (float)i / 5f;
                            Vector2 linePos = Vector2.Lerp(bookPos, npc.Center, progress);
                            Color threadColor = FateLensFlare.GetFateGradient(progress) * 0.3f;
                            CustomParticles.GenericFlare(linePos, threadColor, 0.08f, 5);
                        }
                    }
                }
            }
            
            Lighting.AddLight(bookPos, FateLensFlare.FatePurple.ToVector3() * 0.3f);
        }
        
        public override bool AltFunctionUse(Player player) => true;
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // === RIGHT CLICK: READ THE STARS ===
                bool anyMarked = false;
                
                foreach (NPC npc in Main.npc)
                {
                    if (!npc.active || npc.friendly) continue;
                    if (npc.HasBuff(ModContent.BuffType<FateMarked>()))
                    {
                        anyMarked = true;
                        
                        // Spawn judgment star above enemy
                        Projectile.NewProjectile(source, npc.Center - new Vector2(0, 400f), Vector2.Zero,
                            ModContent.ProjectileType<StarJudgment>(), damage * 3, knockback * 2, player.whoAmI,
                            npc.whoAmI); // ai[0] = target NPC
                        
                        // Clear the mark
                        int buffIndex = npc.FindBuffIndex(ModContent.BuffType<FateMarked>());
                        if (buffIndex != -1)
                            npc.DelBuff(buffIndex);
                    }
                }
                
                if (anyMarked)
                {
                    // Massive VFX at player
                    FateLensFlareDrawLayer.AddFlare(player.Center, 1.2f, 0.8f, 30);
                    FateLensFlare.KaleidoscopeBurst(player.Center, 0.8f, 6);
                    SoundEngine.PlaySound(SoundID.Item122, player.Center);
                }
                
                return false;
            }
            
            // === LEFT CLICK: Seeking fate orb ===
            Vector2 toMouse = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            Projectile.NewProjectile(source, position, toMouse * Item.shootSpeed,
                ModContent.ProjectileType<FateOrbSeeker>(), damage, knockback, player.whoAmI);
            
            // Cast VFX
            FateLensFlareDrawLayer.AddFlare(position, 0.4f, 0.4f, 12);
            CustomParticles.GenericFlare(position, FateLensFlare.FateDarkPink, 0.4f, 12);
            
            return false;
        }
        
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.mana = 40; // More mana for judgment
                Item.useTime = 45;
                Item.useAnimation = 45;
            }
            else
            {
                Item.mana = 15;
                Item.useTime = 18;
                Item.useAnimation = 18;
            }
            return base.CanUseItem(player);
        }
    }
    
    public class FateOrbSeeker : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private const float HomingStrength = 0.08f;
        private const float MaxSpeed = 20f;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
        }
        
        public override void AI()
        {
            Projectile.rotation += 0.1f;
            
            // Homing behavior
            NPC target = FindTarget(800f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * MaxSpeed, HomingStrength);
            }
            
            // === KALEIDOSCOPIC ORB TRAIL ===
            Color orbColor = FateLensFlare.GetFateGradient((Main.GameUpdateCount * 0.02f) % 1f);
            
            // Main trail
            var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                orbColor * 0.5f, 0.2f, 15, true);
            MagnumParticleHandler.SpawnParticle(trail);
            
            // Orbiting particles
            if (Main.rand.NextBool(3))
            {
                float orbitAngle = Main.GameUpdateCount * 0.2f + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 orbitPos = Projectile.Center + orbitAngle.ToRotationVector2() * 15f;
                Color orbitColor = FateLensFlare.GetFateGradient(Main.rand.NextFloat()) * 0.4f;
                CustomParticles.GenericFlare(orbitPos, orbitColor, 0.12f, 8);
            }
            
            // Chromatic shimmer
            FateLensFlare.ChromaticShift(Projectile.Center, 15f, 0.25f);
            
            Lighting.AddLight(Projectile.Center, orbColor.ToVector3() * 0.4f);
        }
        
        private NPC FindTarget(float maxDist)
        {
            NPC closest = null;
            float closestDist = maxDist;
            
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.CountsAsACritter) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            
            // Kaleidoscopic trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = 1f - progress;
                Color trailColor = FateLensFlare.GetFateGradient(progress) * alpha * 0.4f;
                
                sb.Draw(glow, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null,
                    trailColor, Projectile.oldRot[i], glow.Size() / 2f, 0.35f * (1f - progress * 0.5f), SpriteEffects.None, 0f);
            }
            
            // Main orb
            Color orbColor = FateLensFlare.GetFateGradient((Main.GameUpdateCount * 0.02f) % 1f);
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f + 0.9f;
            
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, orbColor * 0.6f,
                Projectile.rotation, glow.Size() / 2f, 0.5f * pulse, SpriteEffects.None, 0f);
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, Color.White * 0.4f,
                Projectile.rotation, glow.Size() / 2f, 0.25f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply fate mark
            target.AddBuff(ModContent.BuffType<FateMarked>(), 600); // 10 seconds
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);
            
            // Mark VFX
            FateLensFlareDrawLayer.AddFlare(target.Center, 0.5f, 0.5f, 15);
            
            // Star symbol above target
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f - MathHelper.PiOver2;
                Vector2 starPoint = target.Top - new Vector2(0, 40f) + angle.ToRotationVector2() * 20f;
                CustomParticles.GenericFlare(starPoint, FateLensFlare.FateBrightRed, 0.25f, 20);
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            FateLensFlare.KaleidoscopeBurst(Projectile.Center, 0.4f, 4);
        }
    }
    
    public class StarJudgment : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private int TargetNPC => (int)Projectile.ai[0];
        private const int ChargeTime = 40;
        private const int StrikeTime = 60;
        
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = ChargeTime + StrikeTime;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        
        public override void AI()
        {
            NPC target = TargetNPC >= 0 && TargetNPC < Main.npc.Length ? Main.npc[TargetNPC] : null;
            if (target == null || !target.active)
            {
                Projectile.Kill();
                return;
            }
            
            int timer = (ChargeTime + StrikeTime) - Projectile.timeLeft;
            bool charging = timer < ChargeTime;
            
            if (charging)
            {
                // === CHARGING PHASE - Star forms above target ===
                float chargeProgress = (float)timer / ChargeTime;
                Projectile.Center = target.Top - new Vector2(0, 200f * (1f - chargeProgress * 0.5f));
                
                // Kaleidoscopic star formation
                int points = 8;
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points + Main.GameUpdateCount * 0.05f;
                    float radius = 30f + chargeProgress * 40f;
                    Vector2 starPos = Projectile.Center + angle.ToRotationVector2() * radius;
                    Color starColor = FateLensFlare.GetFateGradient((float)i / points);
                    CustomParticles.GenericFlare(starPos, starColor * 0.6f, 0.3f * chargeProgress, 8);
                }
                
                // Central charging glow
                FateLensFlare.KaleidoscopeBurst(Projectile.Center, chargeProgress * 0.5f, (int)(4 + chargeProgress * 4));
                
                // Lens flare buildup
                if (Main.GameUpdateCount % 5 == 0)
                    FateLensFlareDrawLayer.AddFlare(Projectile.Center, 0.3f + chargeProgress * 0.5f, 0.4f, 10);
                
                // Warning line to target
                for (int i = 0; i < 5; i++)
                {
                    float progress = (float)i / 5f;
                    Vector2 linePos = Vector2.Lerp(Projectile.Center, target.Center, progress);
                    Color lineColor = FateLensFlare.GetFateGradient(progress) * chargeProgress * 0.3f;
                    CustomParticles.GenericFlare(linePos, lineColor, 0.1f, 4);
                }
                
                Lighting.AddLight(Projectile.Center, FateLensFlare.FateBrightRed.ToVector3() * chargeProgress * 0.6f);
            }
            else
            {
                // === STRIKE PHASE - Star crashes down ===
                float strikeProgress = (float)(timer - ChargeTime) / StrikeTime;
                Vector2 startPos = target.Top - new Vector2(0, 100f);
                Projectile.Center = Vector2.Lerp(startPos, target.Center, MathF.Pow(strikeProgress, 0.5f));
                
                // Streaking trail as it falls
                for (int i = 0; i < 5; i++)
                {
                    Vector2 trailPos = Projectile.Center - new Vector2(0, i * 15f);
                    Color trailColor = FateLensFlare.GetFateGradient((float)i / 5f) * (1f - (float)i / 5f) * 0.6f;
                    CustomParticles.GenericFlare(trailPos, trailColor, 0.3f - i * 0.05f, 8);
                }
                
                // Lens flare following
                FateLensFlareDrawLayer.AddFlare(Projectile.Center, 0.6f, 0.5f, 6);
                
                // Impact when reaching target
                if (strikeProgress > 0.9f && Projectile.ai[1] == 0)
                {
                    Projectile.ai[1] = 1; // Mark as struck
                    
                    // === MASSIVE JUDGMENT IMPACT ===
                    FateLensFlareDrawLayer.AddFlare(target.Center, 1.5f, 1f, 40);
                    FateLensFlare.KaleidoscopeBurst(target.Center, 1.2f, 12);
                    
                    // Reality fracture lines
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f;
                        Vector2 fractureEnd = target.Center + angle.ToRotationVector2() * 150f;
                        
                        for (int j = 0; j < 8; j++)
                        {
                            float progress = (float)j / 8f;
                            Vector2 fracturePos = Vector2.Lerp(target.Center, fractureEnd, progress);
                            Color fractureColor = FateLensFlare.GetFateGradient(progress) * (1f - progress);
                            CustomParticles.GenericFlare(fracturePos, fractureColor, 0.4f - progress * 0.3f, 15);
                        }
                    }
                    
                    // Heat wave shockwave
                    FateLensFlare.DrawHeatWaveDistortion(target.Center, 120f, 0.8f);
                    
                    SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.3f }, target.Center);
                    
                    Lighting.AddLight(target.Center, FateLensFlare.FateBrightRed.ToVector3() * 1.5f);
                }
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            
            int timer = (ChargeTime + StrikeTime) - Projectile.timeLeft;
            bool charging = timer < ChargeTime;
            
            float scale = charging ? 0.5f + (float)timer / ChargeTime * 0.5f : 1f;
            Color starColor = FateLensFlare.GetFateGradient((Main.GameUpdateCount * 0.02f) % 1f);
            
            // Outer glow
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, starColor * 0.4f,
                Main.GameUpdateCount * 0.05f, glow.Size() / 2f, scale * 1.5f, SpriteEffects.None, 0f);
            // Inner glow
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, FateLensFlare.FateWhite * 0.5f,
                0f, glow.Size() / 2f, scale * 0.6f, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.SetCrit();
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 480);
        }
    }
    
    /// <summary>
    /// Fate Mark debuff - enemies take extra damage from Read the Stars
    /// </summary>
    public class FateMarked : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Cursed;
        
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
        
        public override void Update(NPC npc, ref int buffIndex)
        {
            // Visual indicator - floating fate symbol
            if (Main.GameUpdateCount % 10 == 0)
            {
                float angle = Main.GameUpdateCount * 0.05f;
                Vector2 symbolPos = npc.Top - new Vector2(0, 30f) + angle.ToRotationVector2() * 8f;
                Color symbolColor = FateLensFlare.GetFateGradient((Main.GameUpdateCount * 0.01f) % 1f);
                CustomParticles.GenericFlare(symbolPos, symbolColor * 0.5f, 0.2f, 12);
            }
        }
    }
}
