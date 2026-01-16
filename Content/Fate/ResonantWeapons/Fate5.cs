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
    /// COSMIC SCYTHE OF REAPING - Melee Scythe #5
    /// 
    /// UNIQUE ABILITY: "SOUL HARVEST"
    /// Enemies killed within 3 seconds of being hit release their "destiny echo" -
    /// a homing projectile that seeks other enemies and applies stacking debuffs.
    /// 
    /// PASSIVE: Swings create crescent moon arcs that travel through enemies,
    /// leaving kaleidoscopic trails and lens flare coronas.
    /// </summary>
    public class Fate5 : ModItem
    {
        private HashSet<int> recentlyHitNPCs = new HashSet<int>();
        private Dictionary<int, int> hitTimers = new Dictionary<int, int>();
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.DeathSickle;
        
        public override void SetDefaults()
        {
            Item.damage = 590;
            Item.DamageType = DamageClass.Melee;
            Item.width = 60;
            Item.height = 60;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 26);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<CosmicCrescentArc>();
            Item.shootSpeed = 16f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Swings release cosmic crescent arcs that pierce enemies"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "Enemies killed shortly after being hit release homing destiny echoes"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'Death comes on kaleidoscopic wings'") { OverrideColor = FateLensFlare.FateBrightRed });
        }
        
        public override void HoldItem(Player player)
        {
            Vector2 bladeEdge = player.Center + new Vector2(player.direction * 35f, -15f);
            
            // Crescent aura
            float crescentAngle = Main.GameUpdateCount * 0.03f;
            for (int i = 0; i < 5; i++)
            {
                float arcAngle = crescentAngle + MathHelper.Pi * 0.3f * (i - 2f) / 2f;
                Vector2 arcPos = bladeEdge + new Vector2((float)Math.Cos(arcAngle) * 30f, (float)Math.Sin(arcAngle) * 15f);
                if (Main.rand.NextBool(8))
                {
                    CustomParticles.GenericFlare(arcPos, FateLensFlare.GetFateGradient((float)i / 5f) * 0.5f, 0.2f, 10);
                }
            }
            
            // Ambient chromatic shift
            FateLensFlare.ChromaticShift(bladeEdge, 30f, 0.4f);
            
            // Lens flare at blade tip
            if (Main.GameUpdateCount % 30 == 0)
                FateLensFlareDrawLayer.AddFlare(bladeEdge, 0.3f, 0.4f, 15);
            
            Lighting.AddLight(bladeEdge, FateLensFlare.FateDarkPink.ToVector3() * 0.4f);
            
            // Update hit timers and check for soul harvest
            UpdateSoulHarvest(player);
        }
        
        private void UpdateSoulHarvest(Player player)
        {
            List<int> expiredTimers = new List<int>();
            
            foreach (var kvp in hitTimers)
            {
                int npcIndex = kvp.Key;
                int timer = kvp.Value - 1;
                
                if (timer <= 0)
                {
                    expiredTimers.Add(npcIndex);
                    recentlyHitNPCs.Remove(npcIndex);
                }
                else
                {
                    hitTimers[npcIndex] = timer;
                    
                    // Check if NPC died
                    NPC npc = Main.npc[npcIndex];
                    if (!npc.active && npc.life <= 0)
                    {
                        // SOUL HARVEST - spawn destiny echo
                        SpawnDestinyEcho(player, npc.Center);
                        expiredTimers.Add(npcIndex);
                        recentlyHitNPCs.Remove(npcIndex);
                    }
                }
            }
            
            foreach (int index in expiredTimers)
            {
                hitTimers.Remove(index);
            }
        }
        
        private void SpawnDestinyEcho(Player player, Vector2 position)
        {
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, Vector2.Zero,
                ModContent.ProjectileType<DestinyEcho>(), Item.damage / 2, 0f, player.whoAmI);
            
            // Harvest VFX
            FateLensFlareDrawLayer.AddFlare(position, 1f, 0.8f, 30);
            FateLensFlare.KaleidoscopeBurst(position, 0.8f, 6);
            SoundEngine.PlaySound(SoundID.NPCDeath6 with { Pitch = 0.5f }, position);
        }
        
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            
            // Track this NPC for soul harvest (3 second window = 180 ticks)
            if (!recentlyHitNPCs.Contains(target.whoAmI))
            {
                recentlyHitNPCs.Add(target.whoAmI);
                hitTimers[target.whoAmI] = 180;
            }
            else
            {
                hitTimers[target.whoAmI] = 180; // Refresh timer
            }
            
            // Hit VFX
            FateLensFlareDrawLayer.AddFlare(target.Center, 0.6f, 0.5f, 18);
            FateLensFlare.KaleidoscopeBurst(target.Center, 0.5f, 4);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spawn cosmic crescent arc
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            // Spawn VFX at swing origin
            FateLensFlare.KaleidoscopeBurst(position, 0.5f, 4);
            
            return false;
        }
    }
    
    public class CosmicCrescentArc : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 6;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Curved motion (crescent arc)
            Projectile.velocity = Projectile.velocity.RotatedBy(0.02f * Projectile.direction);
            
            // Kaleidoscopic trail
            float time = Main.GameUpdateCount * 0.05f;
            for (int i = 0; i < 2; i++)
            {
                float trailProgress = (time + i * 0.5f) % 1f;
                Color trailColor = FateLensFlare.GetFateGradient(trailProgress);
                
                Vector2 trailOffset = Main.rand.NextVector2Circular(8f, 8f);
                var trail = new GenericGlowParticle(Projectile.Center + trailOffset, -Projectile.velocity * 0.15f,
                    trailColor * 0.7f, 0.25f, 15, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Chromatic aberration trail
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(-3, 0), Color.Red * 0.4f, 0.15f, 8);
            CustomParticles.GenericFlare(Projectile.Center + new Vector2(3, 0), FateLensFlare.FateCyan * 0.4f, 0.15f, 8);
            
            // Periodic lens flare
            if (Main.GameUpdateCount % 8 == 0)
                FateLensFlareDrawLayer.AddFlare(Projectile.Center, 0.4f, 0.4f, 10);
            
            Lighting.AddLight(Projectile.Center, FateLensFlare.FateBrightRed.ToVector3() * 0.5f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            
            // Draw trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float trailProgress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = 1f - trailProgress;
                Color trailColor = FateLensFlare.GetFateGradient(trailProgress) * trailAlpha * 0.5f;
                float trailScale = (1f - trailProgress * 0.5f);
                
                sb.Draw(glow, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null, trailColor,
                    Projectile.oldRot[i], glow.Size() / 2f, new Vector2(1f * trailScale, 0.4f * trailScale), SpriteEffects.None, 0f);
            }
            
            // Main crescent
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, FateLensFlare.FateDarkPink * 0.7f,
                Projectile.rotation, glow.Size() / 2f, new Vector2(1.5f, 0.6f), SpriteEffects.None, 0f);
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, FateLensFlare.FateBrightRed * 0.8f,
                Projectile.rotation, glow.Size() / 2f, new Vector2(1.2f, 0.4f), SpriteEffects.None, 0f);
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, Color.White * 0.6f,
                Projectile.rotation, glow.Size() / 2f, new Vector2(0.8f, 0.15f), SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);
            FateLensFlare.KaleidoscopeBurst(target.Center, 0.4f, 3);
        }
        
        public override void OnKill(int timeLeft)
        {
            FateLensFlare.KaleidoscopeBurst(Projectile.Center, 0.6f, 5);
        }
    }
    
    public class DestinyEcho : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private NPC targetNPC = null;
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
        }
        
        public override void AI()
        {
            // Find target
            if (targetNPC == null || !targetNPC.active)
            {
                float closestDist = 600f;
                foreach (NPC npc in Main.npc)
                {
                    if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                    
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        targetNPC = npc;
                    }
                }
            }
            
            // Home toward target
            if (targetNPC != null && targetNPC.active)
            {
                Vector2 toTarget = (targetNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 12f, 0.08f);
            }
            else
            {
                // Drift slowly
                Projectile.velocity *= 0.98f;
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Ghostly echo visuals
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.3f + 0.7f;
            
            if (Main.rand.NextBool(2))
            {
                var ghost = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    -Projectile.velocity * 0.1f, FateLensFlare.GetFateGradient(Main.rand.NextFloat()) * 0.5f, 0.2f, 12, true);
                MagnumParticleHandler.SpawnParticle(ghost);
            }
            
            // Orbiting soul particles
            float orbitTime = Main.GameUpdateCount * 0.08f;
            for (int i = 0; i < 3; i++)
            {
                float angle = orbitTime + MathHelper.TwoPi * i / 3f;
                Vector2 orbitPos = Projectile.Center + angle.ToRotationVector2() * 15f;
                if (Main.rand.NextBool(4))
                    CustomParticles.GenericFlare(orbitPos, FateLensFlare.GetFateGradient((float)i / 3f) * pulse, 0.15f, 8);
            }
            
            // Periodic lens flare
            if (Main.GameUpdateCount % 15 == 0)
                FateLensFlareDrawLayer.AddFlare(Projectile.Center, 0.3f * pulse, 0.35f, 10);
            
            Lighting.AddLight(Projectile.Center, FateLensFlare.FatePurple.ToVector3() * 0.4f * pulse);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f + 0.8f;
            
            // Ghostly orb
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, FateLensFlare.FatePurple * 0.5f * pulse,
                0f, glow.Size() / 2f, 0.6f, SpriteEffects.None, 0f);
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, FateLensFlare.FateDarkPink * 0.6f * pulse,
                0f, glow.Size() / 2f, 0.4f, SpriteEffects.None, 0f);
            sb.Draw(glow, Projectile.Center - Main.screenPosition, null, Color.White * 0.4f * pulse,
                0f, glow.Size() / 2f, 0.2f, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply stacking debuff
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 360);
            
            FateLensFlareDrawLayer.AddFlare(target.Center, 0.6f, 0.5f, 18);
            FateLensFlare.KaleidoscopeBurst(target.Center, 0.5f, 4);
            
            // Reset target to find new one
            targetNPC = null;
        }
    }
}
