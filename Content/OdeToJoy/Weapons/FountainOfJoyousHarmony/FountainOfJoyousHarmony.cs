using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony
{
    /// <summary>
    /// Fountain of Joyous Harmony — Stationary summon weapon.
    /// Places a golden fountain that heals allies (5HP/s within 15 tiles),
    /// fires homing golden droplets, provides Harmony Zone (+8% all damage),
    /// and erupts Joyous Geyser every 15s.
    /// Tier system: additional summons upgrade the single fountain, not duplicate it.
    /// </summary>
    public class FountainOfJoyousHarmony : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.damage = 2200;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 25;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 2f;
            Item.crit = 4;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<JoyousFountainMinion>();
            Item.shootSpeed = 0.01f;
            Item.buffType = ModContent.BuffType<JoyousFountainBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            // Check for existing fountain — upgrade tier instead of duplicating
            int fountainType = ModContent.ProjectileType<JoyousFountainMinion>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == fountainType)
                {
                    // Upgrade existing fountain's tier (stored in ai[1])
                    p.ai[1] = Math.Min(p.ai[1] + 1f, 4f);

                    // Tier-up VFX burst
                    for (int j = 0; j < 25; j++)
                    {
                        Vector2 vel = new Vector2(0f, -Main.rand.NextFloat(3f, 8f)).RotatedByRandom(MathHelper.PiOver4);
                        Dust d = Dust.NewDustDirect(p.Center - new Vector2(16, 30), 32, 10, DustID.GoldFlame,
                            vel.X, vel.Y, 60, FountainTextures.FountainCore, 0.9f);
                        d.noGravity = true;
                        d.fadeIn = 1.3f;
                    }

                    return false; // Don't spawn new projectile
                }
            }

            // No existing fountain — spawn at player position (stationary)
            Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI);

            // Spawning VFX — golden water splash
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = new Vector2(0f, -Main.rand.NextFloat(2f, 5f)).RotatedByRandom(MathHelper.PiOver2);
                Dust d = Dust.NewDustDirect(Main.MouseWorld - new Vector2(8), 16, 4, DustID.GoldFlame,
                    vel.X, vel.Y, 80, FountainTextures.BloomGold, 0.7f);
                d.noGravity = true;
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Places a stationary golden fountain at the cursor"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Heals allies within 15 tiles for 5 HP/s and fires homing golden droplets"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Harmony Zone grants +8% all damage to nearby allies"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Additional summons upgrade the fountain's tier instead of creating duplicates"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Joyous Geyser erupts every 15 seconds, damaging all nearby enemies and healing 30 HP"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Where the fountain flows, joy follows. Where joy flows, nothing can stand against it.'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }
    }
}