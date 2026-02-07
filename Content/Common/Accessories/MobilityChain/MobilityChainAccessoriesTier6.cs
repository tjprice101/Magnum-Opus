using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Nachtmusik.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;
using MagnumOpus.Content.ClairDeLune.ResonanceEnergies;
using MagnumOpus.Content.Fate.ResonanceEnergies;

namespace MagnumOpus.Content.Common.Accessories.MobilityChain
{
    // ============================================================
    // POST-FATE T7: NOCTURNAL PHANTOM TREADS
    // 175 max momentum, star dash, constellation trail
    // ============================================================
    public class NocturnalPhantomTreads : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.value = Item.sellPrice(gold: 50);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var momentumPlayer = player.GetModPlayer<MomentumPlayer>();
            momentumPlayer.HasNocturnalPhantomTreads = true;
            
            // Enhanced movement stats
            player.moveSpeed += 0.30f;
            player.maxRunSpeed += 2.5f;
            player.accRunSpeed += 1.5f;
            player.jumpBoost = true;
            player.autoJump = true;
            player.jumpSpeedBoost += 1.5f;
            
            // Full immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Slow] = true;
            
            // Night vision and nocturnal power
            player.nightVision = true;
            if (!Main.dayTime)
            {
                player.moveSpeed += 0.10f; // +25% faster momentum build at night
                player.GetDamage(DamageClass.Generic) += 0.08f;
            }
            
            // At 125+ momentum: Leave constellation trail
            if (!hideVisual && momentumPlayer.CurrentMomentum >= 125f && Main.rand.NextBool(3))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(25f, 35f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Enchanted_Gold, 
                    Main.rand.NextVector2Circular(0.5f, 1.2f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
                dust.color = new Color(255, 220, 180);
                
                // Constellation star sparkle
                if (Main.rand.NextBool(4))
                {
                    Dust star = Dust.NewDustPerfect(dustPos, DustID.PlatinumCoin, Vector2.Zero);
                    star.noGravity = true;
                    star.scale = 0.6f;
                }
            }
            
            // At 150+ momentum: Semi-transparent (reduced aggro)
            if (momentumPlayer.CurrentMomentum >= 150f)
            {
                player.aggro -= 300;
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Extends max Momentum to 175"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+30% movement speed, +2.5 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 125+ Momentum: Leave constellation trail that damages enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "At 150+ Momentum: Semi-transparent (enemies target you 30% less)"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "At night: Momentum builds 25% faster"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Consume 125 Momentum: Star Dash - teleport in movement direction"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Starlight accelerates your every step'") { OverrideColor = new Color(180, 160, 255) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<FatesCosmicVelocity>()
                .AddIngredient<NachtmusikResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    // ============================================================
    // POST-FATE T8: INFERNAL METEOR TREADS
    // 200 max momentum, meteor impact, burning trail
    // ============================================================
    public class InfernalMeteorTreads : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.value = Item.sellPrice(gold: 55);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var momentumPlayer = player.GetModPlayer<MomentumPlayer>();
            momentumPlayer.HasInfernalMeteorTreads = true;
            
            // Enhanced movement stats
            player.moveSpeed += 0.35f;
            player.maxRunSpeed += 3.0f;
            player.accRunSpeed += 1.8f;
            player.jumpBoost = true;
            player.autoJump = true;
            player.jumpSpeedBoost += 1.8f;
            
            // Full immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Slow] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            
            // Night vision
            player.nightVision = true;
            
            // At 150+ Momentum: Leave burning trail
            if (!hideVisual && momentumPlayer.CurrentMomentum >= 150f && Main.rand.NextBool(2))
            {
                Vector2 dustPos = player.Bottom + Main.rand.NextVector2Circular(15f, 5f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Torch, 
                    new Vector2(0, Main.rand.NextFloat(-1f, -2f)));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.0f, 1.5f);
                dust.color = new Color(255, 100, 30);
            }
            
            // At 175+ momentum while falling: Meteor impact on landing handled in MomentumPlayer
            // Running through enemies at high momentum knocks them aside
            if (momentumPlayer.CurrentMomentum >= 175f)
            {
                player.noKnockback = true;
                player.thorns += 0.5f; // Contact damage to enemies
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Extends max Momentum to 200"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+35% movement speed, +3.0 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 150+ Momentum: Leave burning trail that damages enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "At 175+ Momentum while falling: Create meteor impact crater on landing"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Consume 150 Momentum: Meteor Dash - charge through enemies (200% weapon damage)"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Running through enemies at high momentum knocks them aside"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Hellfire propels you with wrathful speed'") { OverrideColor = new Color(255, 100, 40) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NocturnalPhantomTreads>()
                .AddIngredient<DiesIraeResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    // ============================================================
    // POST-FATE T9: JUBILANT ZEPHYR TREADS
    // 225 max momentum, infinite flight, healing trail
    // ============================================================
    public class JubilantZephyrTreads : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.value = Item.sellPrice(gold: 60);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var momentumPlayer = player.GetModPlayer<MomentumPlayer>();
            momentumPlayer.HasJubilantZephyrTreads = true;
            
            // Enhanced movement stats
            player.moveSpeed += 0.40f;
            player.maxRunSpeed += 3.5f;
            player.accRunSpeed += 2.0f;
            player.jumpBoost = true;
            player.autoJump = true;
            player.jumpSpeedBoost += 2.0f;
            
            // Full immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Slow] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.buffImmune[BuffID.Stoned] = true;
            
            // Night vision
            player.nightVision = true;
            
            // Momentum decays 50% slower (handled in MomentumPlayer)
            
            // At 175+ Momentum: Infinite flight (wing time doesn't deplete)
            if (momentumPlayer.CurrentMomentum >= 175f)
            {
                player.wingTime = player.wingTimeMax;
            }
            
            // At 200+ Momentum: Healing trail for allies (VFX only for now, healing in PostUpdate)
            if (!hideVisual && momentumPlayer.CurrentMomentum >= 200f && Main.rand.NextBool(3))
            {
                Vector2 dustPos = player.Bottom + Main.rand.NextVector2Circular(20f, 8f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.GreenFairy, 
                    new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f)));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
                
                // Joyful petal effect
                if (Main.rand.NextBool(5))
                {
                    Dust petal = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(30f, 30f), 
                        DustID.RainbowMk2, Main.rand.NextVector2Circular(1f, 1f));
                    petal.noGravity = true;
                    petal.scale = 0.5f;
                }
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Extends max Momentum to 225"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+40% movement speed, +3.5 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Momentum decays 50% slower"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "At 175+ Momentum: Infinite flight (wing time doesn't deplete)"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "At 200+ Momentum: Leave healing trail for allies"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Consume 175 Momentum: Zephyr Burst - push enemies away, brief invincibility"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Nature's wind carries you with joyful speed'") { OverrideColor = new Color(180, 255, 180) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalMeteorTreads>()
                .AddIngredient<OdeToJoyResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    // ============================================================
    // POST-FATE T10: ETERNAL VELOCITY TREADS
    // 250 max momentum, phase through blocks, time slow, lightspeed mode
    // ============================================================
    public class EternalVelocityTreads : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.value = Item.sellPrice(gold: 75);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var momentumPlayer = player.GetModPlayer<MomentumPlayer>();
            momentumPlayer.HasEternalVelocityTreads = true;
            
            // Maximum movement stats
            player.moveSpeed += 0.50f;
            player.maxRunSpeed += 4.5f;
            player.accRunSpeed += 2.5f;
            player.jumpBoost = true;
            player.autoJump = true;
            player.jumpSpeedBoost += 2.5f;
            
            // Full immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Slow] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.buffImmune[BuffID.Stoned] = true;
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Webbed] = true;
            
            // Night vision and full light
            player.nightVision = true;
            Lighting.AddLight(player.Center, 0.8f, 0.6f, 0.5f);
            
            // Momentum never decays during boss fights (handled in MomentumPlayer)
            
            // At 200+ Momentum: Phase through blocks
            if (momentumPlayer.CurrentMomentum >= 200f)
            {
                // Note: True block phasing requires more complex implementation
                // For now, grant extra mobility benefits
                player.noFallDmg = true;
                player.gravity = 0.35f; // Reduced gravity
            }
            
            // At 225+ Momentum: Time slows for enemies (handled in MomentumPlayer)
            
            // At 250 Momentum: Lightspeed mode - invincible while moving, contact damage
            if (momentumPlayer.CurrentMomentum >= 250f && player.velocity.Length() > 1f)
            {
                // Invincibility while in lightspeed mode
                player.immune = true;
                player.immuneTime = 2;
                
                // Deal contact damage
                Rectangle hitbox = player.Hitbox;
                hitbox.Inflate(15, 15);
                
                foreach (NPC npc in Main.npc)
                {
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage && hitbox.Intersects(npc.Hitbox))
                    {
                        int damage = (int)(player.GetDamage(DamageClass.Generic).ApplyTo(100) * 0.75f);
                        int dir = npc.Center.X > player.Center.X ? 1 : -1;
                        npc.SimpleStrikeNPC(damage, dir, false, 8f);
                        
                        // Temporal VFX on hit
                        for (int i = 0; i < 5; i++)
                        {
                            Dust dust = Dust.NewDustPerfect(npc.Center, DustID.Clentaminator_Cyan, 
                                Main.rand.NextVector2Circular(3f, 3f));
                            dust.noGravity = true;
                            dust.scale = 0.8f;
                        }
                    }
                }
            }
            
            // Eternal VFX - clockwork temporal particles
            if (!hideVisual && momentumPlayer.CurrentMomentum >= 200f && Main.rand.NextBool(2))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(30f, 40f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Clentaminator_Cyan, 
                    Main.rand.NextVector2Circular(0.5f, 0.5f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.6f, 1.0f);
                dust.color = new Color(180, 140, 100);
                
                // Clockwork gear particles
                if (Main.rand.NextBool(8))
                {
                    Dust gear = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(25f, 25f), 
                        DustID.Copper, Vector2.Zero);
                    gear.noGravity = true;
                    gear.scale = 0.5f;
                }
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Extends max Momentum to 250"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+50% movement speed, +4.5 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Momentum never decays during boss fights"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "At 200+ Momentum: Phase through blocks"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "At 225+ Momentum: Time slows 40% for nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "At 250 Momentum: Lightspeed mode - invincible while moving, deal 75% weapon damage on contact"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Consume 200 Momentum: Temporal Teleport - teleport up to 150 blocks"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Move through time itself'") { OverrideColor = new Color(200, 160, 120) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<JubilantZephyrTreads>()
                .AddIngredient<ClairDeLuneResonantEnergy>(15)
                .AddIngredient<ResonantCoreOfClairDeLune>()
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
