using System;
using System.Collections.Generic;
using MagnumOpus.Content.ClairDeLune;
using MagnumOpus.Content.ClairDeLune.Weapons.GearDrivenArbiter.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.GearDrivenArbiter
{
    public class GearDrivenArbiterItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/GearDrivenArbiter/GearDrivenArbiter";

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.damage = 2900;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 16;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3.5f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<ArbiterMinionProjectile>();
            Item.buffType = ModContent.BuffType<GearDrivenArbiterBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void HoldItem(Player player)
        {
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Color col = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.WhiteTorch,
                    new Vector2(0, -0.8f) + Main.rand.NextVector2Circular(0.4f, 0.4f), 0, col, 0.5f);
                d.noGravity = true;
            }

            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(player.Center, ClairDeLunePalette.SoftBlue.ToVector3() * 0.35f * pulse);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);

            spriteBatch.Draw(tex, drawPos, null, ClairDeLunePalette.SoftBlue with { A = 0 } * pulse,
                rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, ClairDeLunePalette.PearlWhite with { A = 0 } * (pulse * 0.7f),
                rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a Gear-Driven Arbiter clockwork construct"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "Fires homing orbs that apply verdict stacks to enemies"));
            tooltips.Add(new TooltipLine(Mod, "Stack2", "2+ stacks: orbs gain aggressive homing"));
            tooltips.Add(new TooltipLine(Mod, "Stack4", "4+ stacks: orbs pierce through enemies"));
            tooltips.Add(new TooltipLine(Mod, "Stack6", "6+ stacks: orbs gain 30% bonus speed"));
            tooltips.Add(new TooltipLine(Mod, "Judgment", "8 stacks: Arbiter's Verdict deals 5x damage and resets stacks"));
            tooltips.Add(new TooltipLine(Mod, "Decay", "Stacks decay by 1 every 3 seconds if not refreshed"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A court of gears that judges in silence.'")
            {
                OverrideColor = ClairDeLunePalette.LoreText
            });
        }
    }

    public class GearDrivenArbiterBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ArbiterMinionProjectile>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
