using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.Common.Accessories;
using MagnumOpus.Content.Eroica.HarmonicCores;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using System;
using System.Collections.Generic;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.Eroica.Accessories.Shared
{
    #region Badge of Valor

    /// <summary>
    /// Badge of Valor - Eroica Tier 1 Theme Accessory.
    /// A shield-shaped badge adorned with golden laurels and crimson gems.
    /// +15% melee damage, +10% melee speed, brief invulnerability after killing an enemy.
    /// </summary>
    public class BadgeOfValor : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 1);
            Item.rare = ModContent.RarityType<EroicaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<BadgeOfValorPlayer>();
            modPlayer.hasBadgeOfValor = true;

            // +15% melee damage
            player.GetDamage(DamageClass.Melee) += 0.15f;

            // +10% melee speed
            player.GetAttackSpeed(DamageClass.Melee) += 0.10f;

            // Invulnerability timer effect is handled in player class on kill
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MeleeDamage", "+15% melee damage")
            {
                OverrideColor = EroicaPalette.Crimson
            });

            tooltips.Add(new TooltipLine(Mod, "MeleeSpeed", "+10% melee speed")
            {
                OverrideColor = EroicaPalette.Gold
            });

            tooltips.Add(new TooltipLine(Mod, "HeroicMoment", "Killing an enemy grants 0.5 seconds of invulnerability")
            {
                OverrideColor = EroicaPalette.Sakura
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Worn by those who charge into battle without hesitation'")
            {
                OverrideColor = new Color(200, 180, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 15)
                .AddIngredient(ModContent.ItemType<MelodicCharm>(), 1)
                .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class BadgeOfValorPlayer : ModPlayer
    {
        public bool hasBadgeOfValor;
        public int invulnFramesRemaining;

        public override void ResetEffects()
        {
            hasBadgeOfValor = false;
        }

        public override void PostUpdate()
        {
            if (invulnFramesRemaining > 0)
            {
                Player.immune = true;
                Player.immuneTime = 2;
                invulnFramesRemaining--;

                // Heroic shimmer effect during invuln
                if (Main.rand.NextBool(3))
                {
                    Vector2 pos = Player.Center + Main.rand.NextVector2Circular(20f, 30f);
                }
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            CheckForKill(target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
            {
                CheckForKill(target);
            }
        }

        private void CheckForKill(NPC target)
        {
            if (hasBadgeOfValor && target.life <= 0 && !target.friendly && target.lifeMax > 5)
            {
                // Grant 0.5 seconds (30 frames) of invulnerability
                invulnFramesRemaining = 30;

                // Heroic flash effect
            }
        }
    }

    #endregion

    #region Hero's Symphony

    /// <summary>
    /// Hero's Symphony - Eroica Tier 2 Theme Accessory (Ultimate).
    /// The triumphant power of Eroica crystallized into wearable form.
    /// All Eroica bonuses maximized, kills trigger "Heroic Surge" (+25% damage for 5s).
    /// </summary>
    public class HerosSymphony : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<EroicaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<HerosSymphonyPlayer>();
            modPlayer.hasHerosSymphony = true;

            // Enhanced bonuses
            // +20% melee damage (was 15%)
            player.GetDamage(DamageClass.Melee) += 0.20f;

            // +15% melee speed (was 10%)
            player.GetAttackSpeed(DamageClass.Melee) += 0.15f;

            // +10% melee crit
            player.GetCritChance(DamageClass.Melee) += 10;

            // +8% generic damage always
            player.GetDamage(DamageClass.Generic) += 0.08f;

            // Heroic Surge damage bonus (when active)
            if (modPlayer.heroicSurgeDuration > 0)
            {
                player.GetDamage(DamageClass.Generic) += 0.25f;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MeleeDamage", "+20% melee damage")
            {
                OverrideColor = EroicaPalette.Crimson
            });

            tooltips.Add(new TooltipLine(Mod, "MeleeSpeed", "+15% melee speed")
            {
                OverrideColor = EroicaPalette.Gold
            });

            tooltips.Add(new TooltipLine(Mod, "MeleeCrit", "+10% melee critical strike chance")
            {
                OverrideColor = EroicaPalette.Sakura
            });

            tooltips.Add(new TooltipLine(Mod, "GenericDamage", "+8% damage (all types)")
            {
                OverrideColor = new Color(255, 200, 150)
            });

            tooltips.Add(new TooltipLine(Mod, "HeroicMoment", "Killing an enemy grants 1 second of invulnerability")
            {
                OverrideColor = EroicaPalette.Sakura
            });

            tooltips.Add(new TooltipLine(Mod, "HeroicSurge", "Kills trigger 'Heroic Surge': +25% damage for 5 seconds")
            {
                OverrideColor = EroicaPalette.Gold
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The symphony of heroes echoes through eternity'")
            {
                OverrideColor = new Color(200, 180, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<BadgeOfValor>(), 1)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 25)
                .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<ValorEssence>(), 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class HerosSymphonyPlayer : ModPlayer
    {
        public bool hasHerosSymphony;
        public int invulnFramesRemaining;
        public int heroicSurgeDuration;

        public override void ResetEffects()
        {
            hasHerosSymphony = false;
        }

        public override void PostUpdate()
        {
            // Handle invulnerability
            if (invulnFramesRemaining > 0)
            {
                Player.immune = true;
                Player.immuneTime = 2;
                invulnFramesRemaining--;

                if (Main.rand.NextBool(3))
                {
                    Vector2 pos = Player.Center + Main.rand.NextVector2Circular(20f, 30f);
                }
            }

            // Heroic Surge timer
            if (heroicSurgeDuration > 0)
            {
                heroicSurgeDuration--;
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            CheckForKill(target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
            {
                CheckForKill(target);
            }
        }

        private void CheckForKill(NPC target)
        {
            if (hasHerosSymphony && target.life <= 0 && !target.friendly && target.lifeMax > 5)
            {
                // Grant 1 second (60 frames) of invulnerability
                invulnFramesRemaining = 60;

                // Trigger Heroic Surge (5 seconds = 300 frames)
                heroicSurgeDuration = 300;

                // Triumphant visual burst
                for (int i = 0; i < 3; i++)
                {
                }
            }
        }
    }

    #endregion
}
