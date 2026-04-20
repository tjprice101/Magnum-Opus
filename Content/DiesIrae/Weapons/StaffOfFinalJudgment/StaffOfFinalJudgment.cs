using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgment
{
    public class StaffOfFinalJudgment : ModItem
    {
        private const int MaxActiveMines = 5;

        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.damage = 1950;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 22;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item117;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.FloatingIgnitionProjectile>();
            Item.shootSpeed = 4f;
            Item.crit = 18;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: direct shot, slightly faster fire rate and more mana
                Item.useTime = 20;
                Item.useAnimation = 20;
                Item.mana = 30;
            }
            else
            {
                // Left-click: mine placement, enforce max mine count
                Item.useTime = 30;
                Item.useAnimation = 30;
                Item.mana = 22;

                int activeMines = CountActiveMines(player);
                if (activeMines >= MaxActiveMines)
                    return false;
            }

            return base.CanUseItem(player);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires floating fire mines that auto-arm after a short delay"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Armed mines detonate when enemies approach, releasing 3 homing child orbs"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Maximum of 5 active mines at once"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "3+ mines detonating within 1 second triggers Judgment Storm, firing 5 child orbs each"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Right click fires a direct homing shot"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Judgment does not chase. Judgment waits.'")
            {
                OverrideColor = DiesIraePalette.LoreText
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: direct shot — faster velocity, ai[1]=1 flags it as direct mode
                Vector2 directVelocity = velocity.SafeNormalize(Vector2.UnitX) * 12f;
                Projectile.NewProjectile(source, player.MountedCenter, directVelocity, type,
                    (int)(damage * 1.2f), knockback, player.whoAmI, ai0: 0f, ai1: 1f);
            }
            else
            {
                // Left-click: mine placement — standard velocity, ai[1]=0 flags mine mode
                int activeMines = CountActiveMines(player);
                if (activeMines >= MaxActiveMines)
                    return false;

                Projectile.NewProjectile(source, player.MountedCenter, velocity, type,
                    damage, knockback, player.whoAmI, ai0: 0f, ai1: 0f);
            }

            return false;
        }

        /// <summary>
        /// Counts active FloatingIgnitionProjectile mines owned by this player.
        /// Only counts mine-mode projectiles (ai[1] == 0).
        /// </summary>
        private int CountActiveMines(Player player)
        {
            int count = 0;
            int mineType = ModContent.ProjectileType<Projectiles.FloatingIgnitionProjectile>();

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == player.whoAmI && proj.type == mineType && proj.ai[1] == 0f)
                    count++;
            }

            return count;
        }

        public override void HoldItem(Player player)
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Color col = DiesIraePalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.Torch,
                    new Vector2(0, -1f) + Main.rand.NextVector2Circular(0.5f, 0.5f), 0, col, 0.6f);
                d.noGravity = true;
            }
            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(player.Center, DiesIraePalette.InfernalRed.ToVector3() * 0.4f * pulse);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
            spriteBatch.Draw(tex, drawPos, null, DiesIraePalette.InfernalRed with { A = 0 } * pulse,
                rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, DiesIraePalette.JudgmentGold with { A = 0 } * (pulse * 0.7f),
                rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);
        }
    }
}
