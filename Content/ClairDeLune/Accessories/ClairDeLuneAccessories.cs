using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.ClairDeLune.ResonanceEnergies;
using MagnumOpus.Content.ClairDeLune.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.ClairDeLune.Accessories
{
    #region Chronoblade Gauntlet (Melee)

    /// <summary>
    /// Chronoblade Gauntlet — Tier 10 melee accessory.
    /// Temporal acceleration on melee hits; subtle brass sparkles on swing.
    /// </summary>
    public class ChronobladeGauntlet : ModItem
    {
        private static readonly Color Brass = new Color(205, 127, 50);
        private static readonly Color IceBlue = new Color(150, 200, 255);

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+60% melee damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+35% melee speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Melee hits heal for 10% of damage dealt"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Critical strikes accelerate time — next swing is 50% faster"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The gauntlet tightens, and seconds shatter like glass'")
            {
                OverrideColor = IceBlue
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Melee) += 0.60f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.35f;
            player.GetModPlayer<ChronobladeGauntletPlayer>().gauntletActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 15)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 1)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class ChronobladeGauntletPlayer : ModPlayer
    {
        public bool gauntletActive;
        public int temporalAccelerationTimer;

        public override void ResetEffects()
        {
            gauntletActive = false;
        }

        public override void PostUpdate()
        {
            if (temporalAccelerationTimer > 0)
            {
                Player.GetAttackSpeed(DamageClass.Melee) += 0.50f;
                temporalAccelerationTimer--;
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!gauntletActive || item.DamageType != DamageClass.Melee) return;
            ApplyEffects(target, hit, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!gauntletActive || proj.DamageType != DamageClass.Melee || !proj.friendly) return;
            ApplyEffects(target, hit, damageDone);
        }

        private void ApplyEffects(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int healAmount = (int)(damageDone * 0.10f);
            if (healAmount > 0)
                Player.Heal(healAmount);

            if (hit.Crit)
                temporalAccelerationTimer = 30; // 0.5 second of accelerated swings

            // Subtle brass sparkle on hit
            if (Main.rand.NextBool(3))
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 pos = target.Center + Main.rand.NextVector2Circular(16f, 16f);
                    var sparkle = new SparkleParticle(pos, Main.rand.NextVector2Circular(2f, 2f),
                        new Color(205, 127, 50) * 0.6f, 0.3f, 15);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
        }
    }

    #endregion

    #region Chronodisruptor of Harmony (Ranged)

    /// <summary>
    /// Chronodisruptor of Harmony — Tier 10 ranged accessory.
    /// Ranged crits disrupt enemies in time, slowing them.
    /// </summary>
    public class ChronodisruptorOfHarmony : ModItem
    {
        private static readonly Color MistBlue = new Color(100, 140, 200);
        private static readonly Color IceBlue = new Color(150, 200, 255);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+55% ranged damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+30% ranged critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Ranged critical hits slow enemies by 50% for 2 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "20% chance to not consume ammo"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Harmony fractures, and the world slows to a crawl'")
            {
                OverrideColor = IceBlue
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Ranged) += 0.55f;
            player.GetCritChance(DamageClass.Ranged) += 30;
            player.GetModPlayer<ChronodisruptorPlayer>().disruptorActive = true;
            player.ammoCost80 = true; // 20% chance to not consume ammo
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 15)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 1)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class ChronodisruptorPlayer : ModPlayer
    {
        public bool disruptorActive;

        public override void ResetEffects()
        {
            disruptorActive = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!disruptorActive || proj.DamageType != DamageClass.Ranged || !proj.friendly) return;

            if (hit.Crit)
            {
                target.AddBuff(BuffID.Slow, 120); // 2 seconds slow

                // Small temporal shimmer on crit
                for (int i = 0; i < 3; i++)
                {
                    Vector2 pos = target.Center + Main.rand.NextVector2Circular(12f, 12f);
                    var glow = new GenericGlowParticle(pos, Main.rand.NextVector2Circular(1f, 1f),
                        new Color(150, 200, 255) * 0.5f, 0.2f, 10, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }
        }
    }

    #endregion

    #region Fractured Hourglass Pendant (Magic)

    /// <summary>
    /// Fractured Hourglass Pendant — Tier 10 magic accessory.
    /// Temporal magic amplification with mana cost reduction.
    /// </summary>
    public class FracturedHourglassPendant : ModItem
    {
        private static readonly Color PearlWhite = new Color(220, 225, 240);
        private static readonly Color IceBlue = new Color(150, 200, 255);

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+55% magic damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+30% magic critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Magic attacks inflict Frostburn, dealing damage over time"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "-25% mana cost"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The sand still falls, but time has long since stopped'")
            {
                OverrideColor = IceBlue
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Magic) += 0.55f;
            player.GetCritChance(DamageClass.Magic) += 30;
            player.manaCost -= 0.25f;
            player.GetModPlayer<FracturedHourglassPlayer>().hourglassActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 15)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 1)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class FracturedHourglassPlayer : ModPlayer
    {
        public bool hourglassActive;

        public override void ResetEffects()
        {
            hourglassActive = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hourglassActive || proj.DamageType != DamageClass.Magic || !proj.friendly) return;

            target.AddBuff(BuffID.Frostburn2, 180); // Frostburn for 3 seconds

            // Subtle pearl sparkle on magic hit
            if (Main.rand.NextBool(4))
            {
                Vector2 pos = target.Center + Main.rand.NextVector2Circular(10f, 10f);
                var sparkle = new SparkleParticle(pos, Main.rand.NextVector2Circular(1.5f, 1.5f),
                    new Color(220, 225, 240) * 0.5f, 0.25f, 12);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
    }

    #endregion

    #region Timesinger Sigil (Summoner)

    /// <summary>
    /// Timesinger Sigil — Tier 10 summoner accessory.
    /// Increases max minions and whip range with temporal command.
    /// </summary>
    public class TimesingerSigil : ModItem
    {
        private static readonly Color Brass = new Color(205, 127, 50);
        private static readonly Color IceBlue = new Color(150, 200, 255);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+65% summon damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+4 max minions"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Minions inflict Shadowflame on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+25% whip speed and range"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The sigil hums, and forgotten servants answer across the ages'")
            {
                OverrideColor = IceBlue
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Summon) += 0.65f;
            player.maxMinions += 4;
            player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.25f;
            player.whipRangeMultiplier += 0.25f;
            player.GetModPlayer<TimesingerSigilPlayer>().sigilActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 15)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 1)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class TimesingerSigilPlayer : ModPlayer
    {
        public bool sigilActive;

        public override void ResetEffects()
        {
            sigilActive = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!sigilActive || !proj.minion || !proj.friendly) return;

            target.AddBuff(BuffID.ShadowFlame, 180); // Shadowflame for 3 seconds

            // Subtle temporal shimmer when minions hit
            if (Main.rand.NextBool(5))
            {
                Vector2 pos = target.Center + Main.rand.NextVector2Circular(10f, 10f);
                var glow = new GenericGlowParticle(pos, Main.rand.NextVector2Circular(1f, 1f),
                    new Color(205, 127, 50) * 0.4f, 0.2f, 10, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }
    }

    #endregion
}
