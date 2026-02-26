using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.Eroica.Projectiles;
using System.Collections.Generic;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Eroica.Weapons.TriumphantFractal
{
    public class TriumphantFractal : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 518;
            Item.DamageType = DamageClass.Magic;
            Item.width = 56;
            Item.height = 56;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<TriumphantFractalProjectile>();
            Item.shootSpeed = 14f;
            Item.mana = 19;
            Item.noMelee = true;
            Item.maxStack = 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int numberOfProjectiles = 3;
            float spreadAngle = MathHelper.ToRadians(15);

            for (int i = 0; i < numberOfProjectiles; i++)
            {
                float angle = spreadAngle * ((float)i / (numberOfProjectiles - 1) - 0.5f);
                Vector2 perturbedVelocity = velocity.RotatedBy(angle);
                Projectile.NewProjectile(source, position, perturbedVelocity, type, (int)(damage * 1.15f), knockback, player.whoAmI);
            }

            float baseRotation = Main.GameUpdateCount * 0.02f;
            TriumphantFractalVFX.CastGeometryBurst(position, baseRotation);

            return false;
        }

        public override void HoldItem(Player player)
        {
            TriumphantFractalVFX.HoldItemVFX(player);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            EroicaPalette.DrawItemBloom(spriteBatch, Item, rotation, scale);
            Lighting.AddLight(Item.Center, EroicaPalette.Scarlet.ToVector3() * 0.6f);
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires three homing fractal projectiles in a geometric spread"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Projectiles explode into recursive hexagonal geometry and seeking crystals"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Each cast blooms a sacred hexagram of golden fire"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Victory branches infinitely — every triumph fractals into a thousand more'")
            {
                OverrideColor = new Color(200, 50, 50)
            });
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 1f;

            // Golden glow behind
            spriteBatch.Draw(texture, position, frame, (EroicaPalette.Gold with { A = 0 }) * 0.2f, 0f, origin, scale * pulse * 1.08f, SpriteEffects.None, 0f);

            // Main item with pulse
            spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale * pulse, SpriteEffects.None, 0f);

            return false;
        }
    }
}
