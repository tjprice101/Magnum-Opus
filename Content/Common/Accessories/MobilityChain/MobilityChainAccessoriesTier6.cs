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
            player.moveSpeed += 0.39f;
            player.maxRunSpeed += 2.7f;
            player.accRunSpeed += 1.5f;
            player.jumpBoost = true;
            player.autoJump = true;
            player.jumpSpeedBoost += 2.2f;
            
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
            
            // At 150+ momentum: Semi-transparent (reduced aggro)
            if (momentumPlayer.CurrentMomentum >= 150f)
            {
                player.aggro -= 300;
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Extends max Momentum to 175"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+39% movement speed, +2.7 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Night vision, +8% damage and extra speed at night"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "At 150+ Momentum: Reduced enemy aggro"));
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
            player.moveSpeed += 0.42f;
            player.maxRunSpeed += 3.0f;
            player.accRunSpeed += 1.8f;
            player.jumpBoost = true;
            player.autoJump = true;
            player.jumpSpeedBoost += 2.4f;
            
            // Full immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Slow] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            
            // Night vision
            player.nightVision = true;
            
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
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+42% movement speed, +3.0 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 175+ Momentum: Knockback immunity and contact thorns"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Scorched Earth — at max Momentum: nearby enemies burn and slow"));
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
            player.moveSpeed += 0.45f;
            player.maxRunSpeed += 3.5f;
            player.accRunSpeed += 2.0f;
            player.jumpBoost = true;
            player.autoJump = true;
            player.jumpSpeedBoost += 2.6f;
            
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
            
            // At 200+ Momentum: Healing trail for allies (healing in PostUpdate)
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Extends max Momentum to 225"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+45% movement speed, +3.5 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 175+ Momentum: Infinite flight"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "No fall damage at 200+ Momentum"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Jubilant Stride — at max Momentum: +5% damage, +3% DR"));
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
            player.jumpSpeedBoost += 2.8f;
            
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
            
            // Night vision
            player.nightVision = true;
            
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
                    }
                }
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Extends max Momentum to 250"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+50% movement speed, +4.5 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 200+ Momentum: No fall damage and reduced gravity"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "At 225+ Momentum: Nearby enemies are slowed"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "At 250 Momentum: Lightspeed mode - invincible while moving, deal contact damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Move through time itself'") { OverrideColor = new Color(200, 160, 120) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<JubilantZephyrTreads>()
                .AddIngredient<ClairDeLuneResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
