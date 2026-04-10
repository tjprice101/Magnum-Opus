using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Projectiles;
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

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator
{
    public class ThePollinator : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 28;
            Item.damage = 3200;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.crit = 15;
            Item.shoot = ModContent.ProjectileType<PollinatorProjectile>();
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Bullet;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int projType = ModContent.ProjectileType<PollinatorProjectile>();
            Projectile.NewProjectile(source, position, velocity, projType, damage, knockback, player.whoAmI);
            return false;
        }

        public override void HoldItem(Player player)
        {
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Color col = OdeToJoyPalette.GetGardenGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.GreenTorch,
                    new Vector2(0, -0.8f) + Main.rand.NextVector2Circular(0.4f, 0.4f), 0, col, 0.5f);
                d.noGravity = true;
            }

            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(player.Center, new Vector3(0.4f, 0.35f, 0.15f) * pulse);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
            spriteBatch.Draw(tex, drawPos, null, OdeToJoyPalette.GoldenPollen with { A = 0 } * pulse, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, OdeToJoyPalette.BudGreen with { A = 0 } * (pulse * 0.6f), rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Converts bullets into pollen shots that apply Pollinated — 1% HP/s DoT that spreads to nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Pollinated enemies trigger Mass Bloom on death — golden explosion + 3 homing seed projectiles"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Mass Bloom sites become Golden Fields that heal allies 3 HP/s and grant +5% damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "After 5 Mass Blooms within 10 seconds, triggers Harvest Season — 3x DoT for 5s"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The pollen does not hate. The pollen simply is. And soon, everything else simply was.'")
            {
                OverrideColor = OdeToJoyPalette.LoreText
            });
        }
    }
}
