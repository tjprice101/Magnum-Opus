using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using MagnumOpus.Content.ClairDeLune.Projectiles;
using MagnumOpus.Content.ClairDeLune.ResonanceEnergies;
using MagnumOpus.Content.ClairDeLune.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common;

namespace MagnumOpus.Content.ClairDeLune.Tools
{
    /// <summary>
    /// Wings of Shattered Time - SUPREME FINAL BOSS wings
    /// Must EXCEED Dies Irae Wings of Damnation:
    /// - flyTime: > 560
    /// - flySpeedOverride: > 28f
    /// - accelerationMultiplier: > 5.8f
    /// 
    /// Features: Temporal trails, clockwork particles, crystal shards, lightning effects
    /// Single-frame wings (no animation)
    /// </summary>
    [AutoloadEquip(EquipType.Wings)]
    public class WingsOfShatteredTime : ModItem
    {
        public override void SetStaticDefaults()
        {
            // SUPREME FINAL BOSS - Must exceed Dies Irae (560, 28f, 5.8f)
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(
                flyTime: 650,                  // SUPREME (Dies: 560)
                flySpeedOverride: 34f,         // SUPREME (Dies: 28f)
                accelerationMultiplier: 6.8f,  // SUPREME (Dies: 5.8f)
                hasHoldDownHoverFeatures: true
            );
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 36;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 25);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FlightTime", "10.8 second flight time") { OverrideColor = ClairDeLuneColors.Crystal });
            tooltips.Add(new TooltipLine(Mod, "FlightSpeed", "34 mph flight speed") { OverrideColor = ClairDeLuneColors.ElectricBlue });
            tooltips.Add(new TooltipLine(Mod, "Acceleration", "Supreme acceleration") { OverrideColor = ClairDeLuneColors.Brass });
            tooltips.Add(new TooltipLine(Mod, "Hover", "Allows hovering") { OverrideColor = ClairDeLuneColors.MoonlightSilver });
            tooltips.Add(new TooltipLine(Mod, "DodgeEffect", "Double-tap to perform a temporal phase dash through enemies") { OverrideColor = ClairDeLuneColors.LightningPurple });
            tooltips.Add(new TooltipLine(Mod, "DefenseBonus", "+25 defense while equipped") { OverrideColor = ClairDeLuneColors.Crystal });
            tooltips.Add(new TooltipLine(Mod, "Immunity", "Grants immunity to time-based debuffs (Slow, Frozen)") { OverrideColor = ClairDeLuneColors.MoonlightSilver });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Crystallized moments of eternity, shattered and reformed into wings that defy the laws of time'") 
            { 
                OverrideColor = Color.Lerp(ClairDeLuneColors.Crystal, ClairDeLuneColors.ElectricBlue, 0.5f) 
            });
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
            ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            // Superior to Dies Irae
            ascentWhenFalling = 2.2f;   // Dies: 1.8f
            ascentWhenRising = 0.42f;   // Dies: 0.36f
            maxCanAscendMultiplier = 2.4f; // Dies: 2.0f
            maxAscentMultiplier = 6.8f;    // Dies: 5.8f
            constantAscend = 0.38f;        // Dies: 0.32f
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<WingsOfShatteredTimePlayer>().hasWingsEquipped = true;
            
            // Defense bonus
            player.statDefense += 25;
            
            // Immunity to time-based debuffs
            player.buffImmune[BuffID.Slow] = true;
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Stoned] = true;
            
            // Visual effects when flying
            if (!hideVisual && player.velocity.Y != 0)
            {
                SpawnFlightParticles(player);
            }
            
            // Ambient glow
            Lighting.AddLight(player.Center, ClairDeLuneColors.Crystal.ToVector3() * 0.3f);
        }

        private void SpawnFlightParticles(Player player)
        {
            // Crystal shard trails
            if (Main.rand.NextBool(2))
            {
                float progress = Main.rand.NextFloat();
                Color trailColor = Color.Lerp(ClairDeLuneColors.Crystal, ClairDeLuneColors.ElectricBlue, progress);
                
                var glow = new GenericGlowParticle(
                    player.Center + Main.rand.NextVector2Circular(30f, 25f),
                    new Vector2(player.velocity.X * -0.08f, -1.5f),
                    trailColor * 0.75f, 0.4f, 28, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Clockwork gear particles
            if (Main.rand.NextBool(4))
            {
                Vector2 gearPos = player.Center + Main.rand.NextVector2Circular(35f, 30f);
                ClairDeLuneVFX.SpawnClockworkGear(gearPos, 
                    new Vector2(player.velocity.X * -0.05f, Main.rand.NextFloat(-0.5f, 0.5f)),
                    false, 0.35f);
            }
            
            // Lightning arcs (occasional)
            if (Main.rand.NextBool(8))
            {
                Vector2 startPos = player.Center + Main.rand.NextVector2Circular(20f, 20f);
                Vector2 endPos = startPos + Main.rand.NextVector2Circular(40f, 40f);
                ClairDeLuneVFX.LightningArc(startPos, endPos, 6, 12f, 0.4f);
            }
            
            // Temporal trail particles
            if (Main.rand.NextBool(3))
            {
                ClairDeLuneVFX.TemporalTrail(
                    player.Center + Main.rand.NextVector2Circular(25f, 20f),
                    new Vector2(player.velocity.X * -0.1f, Main.rand.NextFloat(-1f, 1f)),
                    0.4f);
            }
            
            // Crystal shard VFX
            if (Main.rand.NextBool(5))
            {
                ClairDeLuneVFX.SpawnCrystalShard(
                    player.Center + Main.rand.NextVector2Circular(35f, 30f),
                    new Vector2(player.velocity.X * -0.06f, Main.rand.NextFloat(-0.5f, 1f)),
                    false, 0.35f);
            }
            
            // Moonlight silver wisps
            if (Main.rand.NextBool(4))
            {
                var wisp = new GenericGlowParticle(
                    player.Center + Main.rand.NextVector2Circular(40f, 35f),
                    new Vector2(player.velocity.X * -0.05f, Main.rand.NextFloat(-0.3f, 0.3f)),
                    ClairDeLuneColors.MoonlightSilver * 0.5f, 0.25f, 32, true);
                MagnumParticleHandler.SpawnParticle(wisp);
            }
            
            // Purple lightning sparks
            if (Main.rand.NextBool(5))
            {
                CustomParticles.GenericFlare(
                    player.Center + Main.rand.NextVector2Circular(30f, 25f),
                    ClairDeLuneColors.LightningPurple * 0.6f, 0.25f, 15);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 35)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 25)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 4)
                .AddIngredient(ItemID.LunarBar, 25)
                .AddIngredient(ItemID.SoulofFlight, 50)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class WingsOfShatteredTimePlayer : ModPlayer
    {
        public bool hasWingsEquipped = false;
        private int dashCooldown = 0;
        private const int DashCooldownMax = 45; // 0.75 second cooldown
        private int lastDirectionX = 0;
        private int doubleTapTimer = 0;
        private const int DoubleTapWindow = 15; // frames to double-tap

        public override void ResetEffects()
        {
            hasWingsEquipped = false;
        }

        public override void PostUpdate()
        {
            if (dashCooldown > 0)
                dashCooldown--;
            
            if (doubleTapTimer > 0)
                doubleTapTimer--;
            
            // Double-tap dash detection
            if (hasWingsEquipped && dashCooldown <= 0)
            {
                int currentDirection = 0;
                if (Player.controlLeft && !Player.controlRight)
                    currentDirection = -1;
                else if (Player.controlRight && !Player.controlLeft)
                    currentDirection = 1;
                
                if (currentDirection != 0 && currentDirection != lastDirectionX)
                {
                    // New direction pressed
                    if (doubleTapTimer > 0 && currentDirection == -lastDirectionX)
                    {
                        // Double-tap detected in opposite then same direction (release and re-press)
                    }
                    else
                    {
                        doubleTapTimer = DoubleTapWindow;
                    }
                }
                else if (currentDirection != 0 && doubleTapTimer > 0)
                {
                    // Check for double-tap in same direction
                    if (Player.controlLeft && Player.releaseLeft)
                    {
                        PerformTemporalDash(-1);
                    }
                    else if (Player.controlRight && Player.releaseRight)
                    {
                        PerformTemporalDash(1);
                    }
                }
                
                if (currentDirection != 0)
                    lastDirectionX = currentDirection;
            }
        }

        private void PerformTemporalDash(int direction)
        {
            dashCooldown = DashCooldownMax;
            doubleTapTimer = 0;
            
            float dashSpeed = 28f;
            Vector2 dashVelocity = new Vector2(direction * dashSpeed, 0);
            
            // Store starting position for trail
            Vector2 startPos = Player.Center;
            
            // Perform dash
            Player.velocity = dashVelocity;
            Player.immune = true;
            Player.immuneTime = 20;
            
            // VFX at start
            ClairDeLuneVFX.TemporalChargeRelease(startPos, 0.8f);
            ClairDeLuneVFX.ClockworkGearCascade(startPos, 10, 6f, 0.6f);
            
            // Sound
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f, Volume = 0.8f }, startPos);
            
            // Damage enemies in dash path
            Rectangle dashHitbox = new Rectangle(
                (int)(Player.Center.X - 100 * Math.Sign(direction)),
                (int)(Player.Center.Y - 30),
                200,
                60
            );
            
            int dashDamage = 500 + (int)(Player.GetTotalDamage(DamageClass.Generic).ApplyTo(500));
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.Hitbox.Intersects(dashHitbox))
                {
                    Player.ApplyDamageToNPC(npc, dashDamage, 10f, direction, true);
                    
                    // VFX on hit
                    ClairDeLuneVFX.TemporalImpact(npc.Center, 0.6f);
                    ClairDeLuneVFX.LightningStrikeExplosion(npc.Center, 0.5f);
                    
                    // Apply temporal debuffs
                    npc.AddBuff(BuffID.Slow, 120);
                    npc.AddBuff(BuffID.Frostburn2, 180);
                }
            }
            
            // Create temporal afterimages along dash path
            for (int i = 0; i < 8; i++)
            {
                Vector2 afterimagePos = startPos + new Vector2(direction * dashSpeed * 0.4f * i, 0);
                ClairDeLuneVFX.TemporalTrail(afterimagePos, new Vector2(direction * 2f, 0), 0.5f);
                
                if (i % 2 == 0)
                {
                    ClairDeLuneVFX.SpawnCrystalShard(afterimagePos, new Vector2(direction * 1f, Main.rand.NextFloat(-1f, 1f)), false, 0.4f);
                }
            }
        }
    }
}
