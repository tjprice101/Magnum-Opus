using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Projectiles;
using MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Utilities;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock
{
    /// <summary>
    /// Feather of the Iridescent Flock — Summoner Staff.
    /// 
    /// COMBAT SYSTEM:
    /// • Summons iridescent crystal swans that fly in V-formation behind the player
    /// • Each crystal costs 0.34 mana slots (can summon up to ~9 with maxed slots)
    /// • ATTACK CYCLE (per crystal):
    ///   1. Formation Flight (2s) — V-formation behind player
    ///   2. Shard Volley — fires 3 CrystalShardProj at nearest enemy
    ///   3. Dive Attack — charges through target (synchronized when 3+ swans)
    ///   4. Returns to formation
    /// • +5% damage per swan beyond the first (Flock Strength)
    /// • Crystal Resonance: 4+ swans in formation → nearby allies gain +3% crit
    /// 
    /// STATS PRESERVED:
    /// Damage 260, Mana 20, Knockback 3, Sell 60g, SwanRarity
    /// </summary>
    public class FeatheroftheIridescentFlock : ModItem
    {
        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/FeatheroftheIridescentFlock/FeatheroftheIridescentFlock";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            ItemID.Sets.StaffMinionSlotsRequired[Type] = 0; // Handled on the projectile
        }

        public override void SetDefaults()
        {
            Item.damage = 260;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 60);
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.UseSound = SoundID.Item44 with { Pitch = 0.3f, Volume = 0.8f };

            Item.width = 40;
            Item.height = 40;
            Item.shoot = ModContent.ProjectileType<IridescentCrystalProj>();
            Item.buffType = ModContent.BuffType<IridescentFlockBuff>();
            Item.shootSpeed = 1f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            // Pass sequential formation slot index so crystals know their V-formation position
            int existingCount = player.ownedProjectileCounts[Item.shoot];

            Projectile.NewProjectile(source, player.Center, Vector2.Zero, type,
                damage, knockback, player.whoAmI, ai0: existingCount);

            // Summoning VFX — prismatic crystal burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 dustVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f;
                Color col = FlockUtils.GetIridescent((float)i / 12f);
                Dust d = Dust.NewDustPerfect(player.Center, DustID.RainbowTorch, dustVel, 0, col, 1f);
                d.noGravity = true;
            }

            // Rainbow sparkle burst on summon
            try { SwanLakeVFXLibrary.SpawnPrismaticSparkles(player.Center, 5, 18f); } catch { }

            return false;
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            // Flock Strength: +5% damage per swan beyond the first
            int crystalCount = player.ownedProjectileCounts[ModContent.ProjectileType<IridescentCrystalProj>()];
            if (crystalCount > 1)
            {
                damage *= 1f + 0.05f * (crystalCount - 1);
            }
        }

        public override void HoldItem(Player player)
        {
            int crystalCount = player.ownedProjectileCounts[ModContent.ProjectileType<IridescentCrystalProj>()];

            // Crystal Resonance: 4+ swans in formation grant +3% crit to holder
            if (crystalCount >= 4)
            {
                player.GetCritChance(DamageClass.Generic) += 3;

                // Resonance aura particle
                if (Main.rand.NextBool(4))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(50f, 50f);
                    Color resColor = FlockUtils.GetIridescent(Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.RainbowTorch,
                        Vector2.UnitY * -0.5f, 0, resColor, 0.5f);
                    d.noGravity = true;
                    d.fadeIn = 0.8f;
                }
            }

            // Formation glow when 3+ crystals
            if (crystalCount >= 3)
            {
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f + 0.85f;
                Color formationColor = FlockUtils.GetIridescent((float)Main.GameUpdateCount * 0.01f);
                Lighting.AddLight(player.Center, formationColor.ToVector3() * 0.4f * pulse);
            }

            // Ambient oil shimmer
            if (Main.rand.NextBool(8))
            {
                Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                Color col = FlockUtils.GetOilSheen(Main.rand.NextFloat(MathHelper.TwoPi), Main.GameUpdateCount);
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(0.5f, 0.5f), 0, col, 0.6f);
                d.noGravity = true;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Summons iridescent crystal swans that fly in V-formation"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Crystals cycle through shard volleys and synchronized dive attacks"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Each additional swan increases flock damage by 5%"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "Four or more swans create Crystal Resonance, granting +3% crit chance"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Alone, a swan is beautiful. Together, they are devastating.'")
            {
                OverrideColor = FlockUtils.LoreColor
            });
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.12f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
            Color glow = FlockUtils.GetIridescent(0f);

            spriteBatch.Draw(tex, drawPos, null, glow * pulse, rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
