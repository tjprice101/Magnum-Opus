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
    /// • Summons orbiting crystal feathers that orbit the player
    /// • Each crystal costs 0.34 mana slots (can summon up to ~9 with maxed slots)
    /// • Crystals orbit at varying distances, periodically lashing out at nearby enemies
    /// • When 3+ crystals are active, they form a "Flock Formation":
    ///   — Connected by iridescent energy lines
    ///   — Formation periodically fires a focused beam at the targeted enemy
    ///   — Formation beam grows stronger with more crystals
    /// • Right-clicking while holding retargets all crystals to cursor position
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

            // Calculate orbit slot for this crystal
            int existingCount = player.ownedProjectileCounts[Item.shoot];
            float orbitAngle = existingCount * MathHelper.TwoPi / 7f; // Distribute evenly

            Projectile.NewProjectile(source, player.Center, Vector2.Zero, type,
                damage, knockback, player.whoAmI, ai0: orbitAngle);

            // Summoning VFX
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 dustVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f;
                Color col = FlockUtils.GetIridescent((float)i / 12f);
                Dust d = Dust.NewDustPerfect(player.Center, DustID.RainbowTorch, dustVel, 0, col, 1f);
                d.noGravity = true;
            }

            return false;
        }

        public override void HoldItem(Player player)
        {
            int crystalCount = player.ownedProjectileCounts[ModContent.ProjectileType<IridescentCrystalProj>()];

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
                "Summons iridescent crystal feathers that orbit and strike enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Each crystal uses a third of a minion slot"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Three or more crystals form a flock that fires focused energy beams"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'A thousand feathers catch the light — each one a different color of grief'")
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
