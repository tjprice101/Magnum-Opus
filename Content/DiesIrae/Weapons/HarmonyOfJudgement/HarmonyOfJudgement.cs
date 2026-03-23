using MagnumOpus.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgement
{
    public class HarmonyOfJudgement : ModItem, IOverdriveItem
    {
        public IResonantOverdrive GetOverdrivePlayer(Player player) => player.GetModPlayer<ChainOfJudgmentPlayer>();

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 1650;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3.5f;
            Item.value = Item.sellPrice(platinum: 1, gold: 75);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.JudgmentSigilMinion>();
            Item.shootSpeed = 0f;
            Item.buffType = ModContent.BuffType<Buffs.HarmonyOfJudgementBuff>();
            Item.buffTime = 18000;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons a judgment sigil that autonomously processes targets"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Sigils cycle through Scan, Judge, and Execute phases"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Multiple sigils targeting the same enemy trigger Collective Judgment for double damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Five rapid executions within 10 seconds enters Harmonized Verdict state"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'When many voices speak as one, there is no appeal'")
            {
                OverrideColor = DiesIraePalette.LoreText
            });
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
