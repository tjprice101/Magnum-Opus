using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.DiesIrae.Accessories
{
    /// <summary>
    /// Ember of the Condemned - Magic class accessory. Boosts magic damage and causes spells to ignite enemies.
    /// Post-Nachtmusik tier.
    /// </summary>
    public class EmberOfTheCondemned : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+45% magic damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+25% magic critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Magic attacks stack Wrathfire on enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "At 5 stacks, enemies erupt in a chain of fire"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "-20% mana cost"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A cinder from the flames of eternal condemnation'") 
            { 
                OverrideColor = new Color(200, 50, 30) 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Magic) += 0.45f; // POST-NACHTMUSIK (Nachtmusik: 0.35f)
            player.GetCritChance(DamageClass.Magic) += 25;
            player.manaCost -= 0.20f;
            
            // Add on-hit effect via player buff tracking (simplified implementation)
            player.GetModPlayer<EmberOfCondemnedPlayer>().emberActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 15)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 1)
                .AddIngredient(ModContent.ItemType<ShardOfDiesIraesTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class EmberOfCondemnedPlayer : ModPlayer
    {
        public bool emberActive = false;

        public override void ResetEffects()
        {
            emberActive = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (emberActive && proj.DamageType == DamageClass.Magic && proj.friendly)
            {
                target.GetGlobalNPC<DiesIraeAccessoryGlobalNPC>().AddWrathfireStack(target, damageDone);
            }
        }
    }

    /// <summary>
    /// Seal of Damnation - Summoner class accessory. Boosts summon damage and minion count.
    /// Post-Nachtmusik tier.
    /// </summary>
    public class SealOfDamnation : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+55% summon damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+3 max minions"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Minions condemn enemies on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Condemned enemies release vengeful spirits on death"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "+20% whip speed and range"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Bound by the seal, they serve judgment eternal'") 
            { 
                OverrideColor = new Color(200, 50, 30) 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Summon) += 0.55f; // POST-NACHTMUSIK (Nachtmusik: 0.45f)
            player.maxMinions += 3;
            player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.20f;
            player.whipRangeMultiplier += 0.20f;
            
            // Add on-hit effect via player buff tracking
            player.GetModPlayer<SealOfDamnationPlayer>().sealActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 15)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 1)
                .AddIngredient(ModContent.ItemType<WrathEssence>(), 8)
                .AddIngredient(ModContent.ItemType<ShardOfDiesIraesTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class SealOfDamnationPlayer : ModPlayer
    {
        public bool sealActive = false;

        public override void ResetEffects()
        {
            sealActive = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (sealActive && proj.minion && proj.friendly)
            {
                // Mark enemy as condemned
                target.GetGlobalNPC<DiesIraeAccessoryGlobalNPC>().condemnedMark = true;

                // Subtle crimson dust on hit
                if (Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustDirect(target.Center + Main.rand.NextVector2Circular(10f, 10f),
                        0, 0, DustID.RedTorch, 0f, -0.5f, 100, default, 0.8f);
                    dust.noGravity = true;
                }
            }
        }
    }

    /// <summary>
    /// Handles Condemned Spirit spawning when condemned enemies die.
    /// </summary>
    public class SealOfDamnationGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override void OnKill(NPC npc)
        {
            if (!npc.GetGlobalNPC<DiesIraeAccessoryGlobalNPC>().condemnedMark) return;

            // Find the player with seal active
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active || player.dead) continue;
                var sealPlayer = player.GetModPlayer<SealOfDamnationPlayer>();
                if (!sealPlayer.sealActive) continue;

                // Count active spirits for this player
                int activeSpirits = 0;
                for (int p = 0; p < Main.maxProjectiles; p++)
                {
                    if (Main.projectile[p].active && Main.projectile[p].owner == i &&
                        Main.projectile[p].type == ProjectileID.VampireHeal)
                        activeSpirits++;
                }

                if (activeSpirits < 3 && Main.myPlayer == i)
                {
                    // Find nearest enemy to home towards
                    NPC nearest = null;
                    float nearestDist = 600f;
                    for (int n = 0; n < Main.maxNPCs; n++)
                    {
                        NPC candidate = Main.npc[n];
                        if (candidate.active && candidate.CanBeChasedBy() && candidate.whoAmI != npc.whoAmI)
                        {
                            float dist = Vector2.Distance(candidate.Center, npc.Center);
                            if (dist < nearestDist)
                            {
                                nearestDist = dist;
                                nearest = candidate;
                            }
                        }
                    }

                    Vector2 velocity = nearest != null
                        ? (nearest.Center - npc.Center).SafeNormalize(Vector2.UnitX) * 8f
                        : Main.rand.NextVector2CircularEdge(6f, 6f);

                    int damage = (int)(npc.lifeMax * 0.40f);
                    Projectile.NewProjectile(
                        player.GetSource_Accessory(player.armor[0]),
                        npc.Center, velocity,
                        ProjectileID.VampireHeal,
                        damage, 3f, player.whoAmI
                    );
                }

                // Death VFX: rising crimson dust
                for (int d = 0; d < 5; d++)
                {
                    Dust dust = Dust.NewDustDirect(npc.Center + Main.rand.NextVector2Circular(16f, 16f),
                        0, 0, DustID.RedTorch, Main.rand.NextFloat(-1f, 1f), -2f, 100, default, 1.2f);
                    dust.noGravity = true;
                }
                break;
            }
        }
    }

    /// <summary>
    /// Chain of Final Judgment - Melee class accessory. Boosts melee damage and adds lifesteal.
    /// Post-Nachtmusik tier.
    /// </summary>
    public class ChainOfFinalJudgment : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+50% melee damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+30% melee speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Critical strikes may execute non-boss enemies below 15% health"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Melee kills restore health and grant stacking Judgment"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The chains that bind all sinners to their fate'") 
            { 
                OverrideColor = new Color(200, 50, 30) 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Melee) += 0.50f; // POST-NACHTMUSIK (Nachtmusik: 0.38f)
            player.GetAttackSpeed(DamageClass.Melee) += 0.30f;
            
            // Add effects via player class
            player.GetModPlayer<ChainOfFinalJudgmentPlayer>().chainActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 15)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 1)
                .AddIngredient(ModContent.ItemType<ShardOfDiesIraesTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class ChainOfFinalJudgmentPlayer : ModPlayer
    {
        public bool chainActive = false;
        public int judgmentStacks = 0;
        public int judgmentTimer = 0;

        public override void ResetEffects()
        {
            chainActive = false;
        }

        public override void PostUpdateEquips()
        {
            // Judgment buff: +3% melee damage per stack, up to 5 stacks (+15%)
            if (judgmentTimer > 0)
            {
                judgmentTimer--;
                Player.GetDamage(DamageClass.Melee) += 0.03f * judgmentStacks;
            }
            else
            {
                judgmentStacks = 0;
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (chainActive && item.DamageType == DamageClass.Melee)
            {
                ProcessMeleeHit(target, hit, damageDone);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (chainActive && proj.DamageType == DamageClass.Melee && proj.friendly)
            {
                ProcessMeleeHit(target, hit, damageDone);
            }
        }

        private void ProcessMeleeHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Execute chance on crit (20% chance at 15% HP threshold)
            if (hit.Crit && !target.boss && target.life < target.lifeMax * 0.15f && Main.rand.NextFloat() < 0.20f)
            {
                target.life = 0;
                target.HitEffect();
                target.checkDead();
            }

            // Subtle crimson dust every 3rd hit
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(target.Center + Main.rand.NextVector2Circular(10f, 10f),
                    0, 0, DustID.RedTorch, 0f, -0.5f, 100, default, 0.8f);
                dust.noGravity = true;
            }

            // Check if target was killed (for kill-based sustain)
            if (target.life <= 0)
            {
                OnMeleeKill(target);
            }
        }

        private void OnMeleeKill(NPC target)
        {
            // Restore 5% of killed enemy's max HP (capped at 50)
            int healAmount = System.Math.Min((int)(target.lifeMax * 0.05f), 50);
            if (healAmount > 0)
                Player.Heal(healAmount);

            // Grant Judgment stack
            judgmentStacks = System.Math.Min(judgmentStacks + 1, 5);
            judgmentTimer = 240; // 4 seconds

            // Kill VFX: rising red dust
            for (int i = 0; i < 4; i++)
            {
                Dust dust = Dust.NewDustDirect(target.Center + Main.rand.NextVector2Circular(14f, 14f),
                    0, 0, DustID.RedTorch, Main.rand.NextFloat(-1f, 1f), -2f, 80, default, 1.1f);
                dust.noGravity = true;
            }
        }
    }

    /// <summary>
    /// Requiem's Shackle - Ranger class accessory. Boosts ranged damage and adds marking mechanic.
    /// Post-Nachtmusik tier.
    /// </summary>
    public class RequiemsShackle : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+50% ranged damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+30% ranged critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Ranged attacks shackle enemies in chains of judgment"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Shackled enemies take 20% increased damage from all sources"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "25% chance to not consume ammo"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The shackles that bind souls to their requiem'") 
            { 
                OverrideColor = new Color(200, 50, 30) 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Ranged) += 0.50f; // POST-NACHTMUSIK (Nachtmusik: 0.40f)
            player.GetCritChance(DamageClass.Ranged) += 30;
            player.ammoCost75 = true; // 25% chance to not consume ammo
            
            // Add marking effect via player class
            player.GetModPlayer<RequiemsShacklePlayer>().shackleActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 15)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 1)
                .AddIngredient(ModContent.ItemType<ShardOfDiesIraesTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class RequiemsShacklePlayer : ModPlayer
    {
        public bool shackleActive = false;

        public override void ResetEffects()
        {
            shackleActive = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (shackleActive && proj.DamageType == DamageClass.Ranged && proj.friendly)
            {
                target.GetGlobalNPC<DiesIraeAccessoryGlobalNPC>().ApplyChainsOfJudgment(target);
            }
        }
    }
}
