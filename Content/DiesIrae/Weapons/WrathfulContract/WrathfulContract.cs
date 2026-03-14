using MagnumOpus.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract
{
    public class WrathfulContract : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 1650;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 40;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(platinum: 2, gold: 25);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item82;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.WrathDemonMinion>();
            Item.shootSpeed = 0f;
            Item.buffType = ModContent.BuffType<Buffs.WrathfulContractBuff>();
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons a wrathful demon bound by a blood contract"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Constantly drains 1 HP/s while active, heals 5% of enemy max HP on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "After 3 kills, enters Frenzy: 2x attack speed, +30% damage, but 3 HP/s drain"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Below 10% HP triggers Breach of Contract — the demon turns hostile"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Costs 2 minion slots"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The contract demands payment in blood. Yours or theirs — it cares not which'")
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

    public class WrathfulContractPlayer : ModPlayer
    {
        public bool HasActiveDemon;
        public bool DemonInFrenzy;
        public int BloodSacrificeTimer;

        public override void ResetEffects()
        {
            HasActiveDemon = false;
            DemonInFrenzy = false;
        }

        public override void PostUpdate()
        {
            if (BloodSacrificeTimer > 0)
                BloodSacrificeTimer--;
        }

        public bool IsBelowBreachThreshold()
        {
            if (Player.statLifeMax2 <= 0)
                return false;

            return Player.statLife <= Player.statLifeMax2 * 0.10f;
        }
    }
}
