using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Content.Nachtmusik.Projectiles;

namespace MagnumOpus.Content.Nachtmusik.ResonantWeapons
{
    /// <summary>
    /// Celestial Chorus Baton - Summons a Nocturnal Guardian.
    /// The guardian orbits the player and dashes to attack enemies.
    /// DAMAGE: 520 (aggressive melee minion)
    /// </summary>
    public class CelestialChorusBaton : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 1100; // POST-FATE ULTIMATE SUMMON - 40%+ above Fate tier
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 44);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<NocturnalGuardianMinion>();
            Item.buffType = ModContent.BuffType<CelestialChorusBatonBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            Vector2 spawnPos = Main.MouseWorld;

            // Entrance VFX
            CelestialChorusBatonVFX.SummonVFX(spawnPos);

            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);

            return false;
        }

        public override void HoldItem(Player player)
        {
            int minionCount = player.ownedProjectileCounts[ModContent.ProjectileType<NocturnalGuardianMinion>()];
            CelestialChorusBatonVFX.HoldItemVFX(player, minionCount);
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a Nocturnal Guardian to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "The guardian orbits you and dashes to attack enemies"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Guardian attacks inflict Celestial Harmony"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Conduct the symphony of the night'")
            {
                OverrideColor = NachtmusikPalette.CosmicPurple
            });
        }
    }

    /// <summary>
    /// Galactic Overture - Summons a Celestial Muse.
    /// The muse hovers near the player and fires musical projectiles.
    /// DAMAGE: 420 (ranged attacks)
    /// </summary>
    public class GalacticOverture : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 980; // POST-FATE ULTIMATE SUMMON - 38%+ above Fate tier
            Item.DamageType = DamageClass.Summon;
            Item.mana = 18;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 42);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<CelestialMuseMinion>();
            Item.buffType = ModContent.BuffType<GalacticOvertureBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            Vector2 spawnPos = Main.MouseWorld;

            // Musical entrance VFX
            GalacticOvertureVFX.SummonVFX(spawnPos);

            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);

            return false;
        }

        public override void HoldItem(Player player)
        {
            int minionCount = player.ownedProjectileCounts[ModContent.ProjectileType<CelestialMuseMinion>()];
            GalacticOvertureVFX.HoldItemVFX(player, minionCount);
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a Celestial Muse to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "The muse hovers nearby and fires musical projectiles"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Attacks inflict Celestial Harmony"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Let the overture begin'")
            {
                OverrideColor = NachtmusikPalette.RadianceGold
            });
        }
    }

    /// <summary>
    /// Conductor of Constellations - The ultimate summon weapon from Nachtmusik.
    /// Summons a Stellar Conductor that commands star attacks.
    /// DAMAGE: 550 (2 minion slots, very powerful attacks)
    /// </summary>
    public class ConductorOfConstellations : ModItem
    {
        // Using placeholder texture since the weapon PNG doesn't exist (only the minion PNG)
        public override string Texture => "Terraria/Images/Item_" + ItemID.RainbowCrystalStaff;

        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.damage = 1250; // POST-FATE ULTIMATE SUMMON - 45%+ above Fate tier
            Item.DamageType = DamageClass.Summon;
            Item.mana = 35;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item82;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<StellarConductorMinion>();
            Item.buffType = ModContent.BuffType<ConductorOfConstellationsBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            Vector2 spawnPos = Main.MouseWorld;

            // Grand entrance VFX
            ConductorOfConstellationsVFX.SummonVFX(spawnPos);
            MagnumScreenEffects.AddScreenShake(4f);

            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);

            return false;
        }

        public override void HoldItem(Player player)
        {
            int minionCount = player.ownedProjectileCounts[ModContent.ProjectileType<StellarConductorMinion>()];
            ConductorOfConstellationsVFX.HoldItemVFX(player, minionCount);
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a Stellar Conductor to command the cosmos"));
            tooltips.Add(new TooltipLine(Mod, "Slots", "Uses 2 minion slots"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "The conductor fires star barrages and periodic burst attacks"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "All attacks inflict Celestial Harmony"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Raise your baton and command the stars themselves'")
            {
                OverrideColor = NachtmusikPalette.StarWhite
            });
        }
    }
}
