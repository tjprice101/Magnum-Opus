using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.Projectiles;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.OdeToJoy.Accessories
{
    /// <summary>
    /// The Flowering Coda - Magic class accessory
    /// Boosts magic damage, causes spells to bloom with healing energy
    /// Post-Dies Irae tier (stronger than Dies Irae)
    /// </summary>
    public class TheFloweringCoda : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 3);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+60% magic damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+35% magic critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Magic attacks create blooming petals that heal 3% of damage dealt"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "-30% mana cost"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Every 10th magic hit creates a joyous bloom explosion"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The final flourish of nature's triumphant symphony'") 
            { 
                OverrideColor = OdeToJoyColors.VerdantGreen 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Magic) += 0.60f; // POST-DIES IRAE (Dies Irae: 0.45f)
            player.GetCritChance(DamageClass.Magic) += 35;
            player.manaCost -= 0.30f;
            
            player.GetModPlayer<FloweringCodaPlayer>().floweringCodaActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 18)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 1)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class FloweringCodaPlayer : ModPlayer
    {
        public bool floweringCodaActive = false;
        public int magicHitCounter = 0;

        public override void ResetEffects()
        {
            floweringCodaActive = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (floweringCodaActive && proj.DamageType == DamageClass.Magic && proj.friendly)
            {
                // 3% healing
                int healAmount = (int)(damageDone * 0.03f);
                if (healAmount > 0)
                {
                    Player.Heal(healAmount);
                }
                
                // Enhanced chromatic petal VFX
                if (Main.rand.NextBool(3))
                {
                    OdeToJoyVFX.ChromaticRosePetalBurst(target.Center, 6, 4f, 0.45f, false);
                }
                
                // Every 10th hit: joyous bloom explosion with full signature effect
                magicHitCounter++;
                if (magicHitCounter >= 10)
                {
                    magicHitCounter = 0;
                    OdeToJoyVFX.OdeToJoySignatureExplosion(target.Center, 0.9f);
                    
                    // Deal bonus damage in area
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && npc.CanBeChasedBy(proj) && Vector2.Distance(npc.Center, target.Center) < 150f)
                        {
                            Player.ApplyDamageToNPC(npc, damageDone / 3, 0f, 0, false);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// The Verdant Refrain - Summoner class accessory
    /// Boosts summon damage and adds nature-themed effects
    /// Post-Dies Irae tier
    /// </summary>
    public class TheVerdantRefrain : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 3);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+70% summon damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+4 max minions"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Minions leave verdant trails that slow enemies by 40%"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Minion attacks have 15% chance to spawn a healing flower"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "+25% whip speed and range"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The endless melody of nature's eternal chorus'") 
            { 
                OverrideColor = OdeToJoyColors.VerdantGreen 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Summon) += 0.70f; // POST-DIES IRAE (Dies Irae: 0.55f)
            player.maxMinions += 4;
            player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.25f;
            
            player.GetModPlayer<VerdantRefrainPlayer>().verdantRefrainActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 18)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 1)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class VerdantRefrainPlayer : ModPlayer
    {
        public bool verdantRefrainActive = false;

        public override void ResetEffects()
        {
            verdantRefrainActive = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (verdantRefrainActive && proj.minion && proj.friendly)
            {
                // Slow enemies
                target.AddBuff(BuffID.Slow, 180); // 3 seconds
                
                // 15% chance to spawn healing flower with enhanced VFX
                if (Main.rand.NextFloat() < 0.15f)
                {
                    // Heal the player
                    int healAmount = (int)(damageDone * 0.05f);
                    if (healAmount > 0)
                    {
                        Player.Heal(healAmount);
                    }
                    
                    // Enhanced chromatic rose petal burst and harmonic sparkle
                    OdeToJoyVFX.ChromaticRosePetalBurst(target.Center, 8, 5f, 0.5f, true);
                    OdeToJoyVFX.HarmonicNoteSparkle(target.Center, 4, 3f, 0.4f, false);
                }
                
                // Enhanced chromatic vine trail VFX
                if (Main.rand.NextBool(4))
                {
                    OdeToJoyVFX.ChromaticVineGrowthBurst(proj.Center, 2, 2f, 0.35f, false);
                }
            }
        }
    }

    /// <summary>
    /// Conductor's Corsage - Melee class accessory
    /// Boosts melee damage and adds blooming lifesteal
    /// Post-Dies Irae tier
    /// </summary>
    public class ConductorsCorsage : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 3);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+65% melee damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+40% melee speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Melee attacks heal for 12% of damage dealt"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Critical strikes cause blooming explosions that heal nearby allies"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "+20 defense while attacking"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The flower that blooms upon the conductor's heart, spreading joy to all'") 
            { 
                OverrideColor = OdeToJoyColors.RosePink 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Melee) += 0.65f; // POST-DIES IRAE (Dies Irae: 0.50f)
            player.GetAttackSpeed(DamageClass.Melee) += 0.40f;
            
            player.GetModPlayer<ConductorsCorsagePlayer>().conductorsCorsageActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 18)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 1)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class ConductorsCorsagePlayer : ModPlayer
    {
        public bool conductorsCorsageActive = false;
        public int attackTimer = 0;

        public override void ResetEffects()
        {
            conductorsCorsageActive = false;
        }

        public override void PostUpdateEquips()
        {
            // +20 defense while attacking (recent attack within 60 frames)
            if (conductorsCorsageActive && attackTimer > 0)
            {
                Player.statDefense += 20;
                attackTimer--;
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (conductorsCorsageActive && item.DamageType == DamageClass.Melee)
            {
                ProcessMeleeHit(target, hit, damageDone);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (conductorsCorsageActive && proj.DamageType == DamageClass.Melee && proj.friendly)
            {
                ProcessMeleeHit(target, hit, damageDone);
            }
        }

        private void ProcessMeleeHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            attackTimer = 60;
            
            // 12% lifesteal (POST-DIES IRAE ULTIMATE - Dies Irae: 8%)
            int healAmount = (int)(damageDone * 0.12f);
            if (healAmount > 0)
            {
                Player.Heal(healAmount);
            }
            
            // Enhanced chromatic vine trail on hit
            OdeToJoyVFX.ChromaticVineGrowthBurst(target.Center, 3, 3f, 0.4f, false);
            
            // Critical strikes cause blooming explosions that heal nearby allies
            if (hit.Crit)
            {
                // Full chromatic rose petal explosion on crits
                OdeToJoyVFX.ChromaticRosePetalBurst(target.Center, 12, 6f, 0.8f, true);
                
                // Heal nearby allies (multiplayer)
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player ally = Main.player[i];
                    if (ally.active && !ally.dead && ally.whoAmI != Player.whoAmI && 
                        Vector2.Distance(ally.Center, target.Center) < 200f)
                    {
                        int allyHeal = (int)(damageDone * 0.05f);
                        if (allyHeal > 0)
                        {
                            ally.Heal(allyHeal);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Symphony of Blossoms - Ranged class accessory
    /// Boosts ranged damage and adds blooming mark mechanic
    /// Post-Dies Irae tier
    /// </summary>
    public class SymphonyOfBlossoms : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 3);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+65% ranged damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+40% ranged critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Ranged attacks mark enemies with Blooming Vines for 6 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Marked enemies take 25% increased damage and are slowed"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "40% chance to not consume ammo"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Every 5th shot fires a homing petal storm"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A symphony played in petals and thorns, celebrating nature's triumph'") 
            { 
                OverrideColor = OdeToJoyColors.RosePink 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Ranged) += 0.65f; // POST-DIES IRAE (Dies Irae: 0.50f)
            player.GetCritChance(DamageClass.Ranged) += 40;
            player.ammoCost80 = true; // 40% chance to not consume ammo (stacking)
            
            player.GetModPlayer<SymphonyOfBlossomsPlayer>().symphonyActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 18)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 1)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class SymphonyOfBlossomsPlayer : ModPlayer
    {
        public bool symphonyActive = false;
        public int shotCounter = 0;

        public override void ResetEffects()
        {
            symphonyActive = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (symphonyActive && proj.DamageType == DamageClass.Ranged && proj.friendly && !proj.minion)
            {
                // Mark with debuffs (Ichor for damage increase, Slow for movement)
                target.AddBuff(BuffID.Ichor, 360); // 6 seconds - increases damage taken by 15
                target.AddBuff(BuffID.Slow, 360); // 6 seconds
                
                // Enhanced chromatic vine VFX on mark
                if (Main.rand.NextBool(3))
                {
                    OdeToJoyVFX.ChromaticVineGrowthBurst(target.Center, 2, 2f, 0.4f, false);
                }
                
                // Shot counter for bonus petal storm
                shotCounter++;
                if (shotCounter >= 5)
                {
                    shotCounter = 0;
                    
                    // Fire a homing petal storm projectile
                    if (Main.myPlayer == Player.whoAmI)
                    {
                        Vector2 direction = (target.Center - Player.Center).SafeNormalize(Vector2.UnitX);
                        Projectile.NewProjectile(
                            Player.GetSource_Accessory(Player.armor.FirstOrDefault(i => i.type == ModContent.ItemType<SymphonyOfBlossoms>())),
                            Player.Center, 
                            direction * 12f,
                            ModContent.ProjectileType<PetalStormProjectile>(),
                            (int)(damageDone * 0.6f),
                            3f,
                            Player.whoAmI
                        );
                    }
                    
                    // Enhanced harmonic note sparkle on petal storm trigger
                    OdeToJoyVFX.HarmonicNoteSparkle(Player.Center, 8, 5f, 0.7f, true);
                }
            }
        }
    }
}
