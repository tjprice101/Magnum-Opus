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
using MagnumOpus.Content.Materials.EnemyDrops;

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
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Critical strikes accelerate time — next swing is 50% faster"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Every 8th melee hit restores 25% of recent damage taken (max 40 HP)"));
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
                .AddIngredient(ModContent.ItemType<ShardOfClairDeLunesTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class ChronobladeGauntletPlayer : ModPlayer
    {
        public bool gauntletActive;
        public int temporalAccelerationTimer;
        public int meleeHitCounter;
        private int recentDamageTaken;
        private int damageTrackingTimer;

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

            // Track damage taken over last 60 frames
            if (damageTrackingTimer > 0)
                damageTrackingTimer--;
            else
                recentDamageTaken = 0;
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (gauntletActive)
            {
                recentDamageTaken += info.Damage;
                damageTrackingTimer = 60; // 1 second window
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
            if (hit.Crit)
                temporalAccelerationTimer = 30; // 0.5 second of accelerated swings

            // Temporal Rewind: every 8th melee hit restores 25% of recent damage taken
            meleeHitCounter++;
            if (meleeHitCounter >= 8)
            {
                meleeHitCounter = 0;
                int healAmount = System.Math.Min((int)(recentDamageTaken * 0.25f), 40);
                if (healAmount > 0)
                    Player.Heal(healAmount);
                recentDamageTaken = 0;
            }

            // Subtle brass sparkle on hit (keep existing minimal VFX)
            if (Main.rand.NextBool(3))
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 pos = target.Center + Main.rand.NextVector2Circular(16f, 16f);
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
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Ranged critical hits disrupt enemy tempo"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Affected enemies deal 15% less damage and move slowly"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "20% chance to not consume ammo"));
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
                .AddIngredient(ModContent.ItemType<ShardOfClairDeLunesTempo>(), 5)
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
                target.GetGlobalNPC<ClairDeLuneAccessoryGlobalNPC>().ApplyBulletTime(target);

                // Small temporal shimmer on crit (keep existing minimal VFX)
                for (int i = 0; i < 3; i++)
                {
                    Vector2 pos = target.Center + Main.rand.NextVector2Circular(12f, 12f);
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
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Magic attacks may fracture time around enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Enemies in fractured zones take 10% increased damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "-25% mana cost"));
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
                .AddIngredient(ModContent.ItemType<LuneEssence>(), 8)
                .AddIngredient(ModContent.ItemType<ShardOfClairDeLunesTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class FracturedHourglassPlayer : ModPlayer
    {
        public bool hourglassActive;

        // Time Fracture zone tracking (max 2)
        public Vector2[] zonePositions = new Vector2[2];
        public int[] zoneTimers = new int[2];

        public override void ResetEffects()
        {
            hourglassActive = false;
        }

        public override void PostUpdate()
        {
            // Update time fracture zones
            for (int i = 0; i < 2; i++)
            {
                if (zoneTimers[i] > 0)
                {
                    zoneTimers[i]--;

                    // Zone VFX: ice-blue dust swirling at zone center every 30 frames
                    if (zoneTimers[i] % 30 == 0)
                    {
                        for (int d = 0; d < 2; d++)
                        {
                            Vector2 offset = Main.rand.NextVector2CircularEdge(40f, 40f);
                            Dust dust = Dust.NewDustDirect(zonePositions[i] + offset,
                                0, 0, DustID.IceTorch, -offset.X * 0.02f, -offset.Y * 0.02f, 100, default, 0.8f);
                            dust.noGravity = true;
                        }
                    }
                }
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hourglassActive || proj.DamageType != DamageClass.Magic || !proj.friendly) return;

            // 12% chance to create a time fracture zone
            if (Main.rand.NextFloat() < 0.12f)
            {
                // Find an available slot (or overwrite oldest)
                int slot = -1;
                for (int i = 0; i < 2; i++)
                {
                    if (zoneTimers[i] <= 0) { slot = i; break; }
                }
                if (slot == -1)
                {
                    // Overwrite the one with less time remaining
                    slot = zoneTimers[0] < zoneTimers[1] ? 0 : 1;
                }

                zonePositions[slot] = target.Center;
                zoneTimers[slot] = 180; // 3 seconds

                // Zone spawn VFX
                for (int d = 0; d < 4; d++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f);
                    Dust dust = Dust.NewDustDirect(target.Center, 0, 0,
                        DustID.IceTorch, vel.X, vel.Y, 80, default, 1.0f);
                    dust.noGravity = true;
                }
            }

            // Subtle pearl sparkle on magic hit (keep existing minimal VFX)
            if (Main.rand.NextBool(4))
            {
                Vector2 pos = target.Center + Main.rand.NextVector2Circular(10f, 10f);
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
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Minion attacks may summon temporal echoes that strike once"));
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
                .AddIngredient(ModContent.ItemType<ShardOfClairDeLunesTempo>(), 5)
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

            // 8% chance to spawn a temporal echo
            if (Main.rand.NextFloat() < 0.08f && Main.myPlayer == Player.whoAmI)
            {
                // Count active echo projectiles
                int activeEchoes = 0;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].owner == Player.whoAmI &&
                        Main.projectile[i].type == ProjectileID.StardustDragon4)
                        activeEchoes++;
                }

                if (activeEchoes < 2)
                {
                    // Spawn echo at hit location aimed at nearest enemy
                    Vector2 velocity = Main.rand.NextVector2CircularEdge(6f, 6f);
                    int echoDamage = (int)(damageDone * 0.6f);
                    int echoProj = Projectile.NewProjectile(
                        Player.GetSource_Accessory(Player.armor[0]),
                        target.Center, velocity,
                        ProjectileID.StardustDragon4,
                        echoDamage, 2f, Player.whoAmI
                    );
                    if (echoProj >= 0 && echoProj < Main.maxProjectiles)
                        Main.projectile[echoProj].timeLeft = 90; // 1.5 seconds
                }
            }

            // Subtle temporal shimmer when minions hit (keep existing minimal VFX)
            if (Main.rand.NextBool(5))
            {
                Vector2 pos = target.Center + Main.rand.NextVector2Circular(10f, 10f);
            }
        }
    }

    #endregion
}
