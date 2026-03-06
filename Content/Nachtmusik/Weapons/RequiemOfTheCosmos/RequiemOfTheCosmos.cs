using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;
using MagnumOpus.Common;
using MagnumOpus.Content.Nachtmusik.Weapons.RequiemOfTheCosmos.Projectiles;
using MagnumOpus.Content.Nachtmusik.Weapons.RequiemOfTheCosmos.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.RequiemOfTheCosmos
{
    public class RequiemOfTheCosmos : ModItem
    {
        private int cosmicCastCount;
        private const int EventHorizonThreshold = 10;
        
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.damage = 1400;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 22;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 8f;
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.value = Item.sellPrice(gold: 30);
            Item.shoot = ModContent.ProjectileType<CosmicRequiemOrbProjectile>();
            Item.shootSpeed = 10f;
            Item.autoReuse = true;
            Item.crit = 24;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            cosmicCastCount++;
            
            Vector2 toMouse = Main.MouseWorld - player.Center;
            toMouse.Normalize();
            
            if (cosmicCastCount >= EventHorizonThreshold)
            {
                // EVENT HORIZON: Massive cosmic orb at 3x damage
                cosmicCastCount = 0;
                Projectile.NewProjectile(source, position, toMouse * 8f, type, damage * 3, knockback * 2f, player.whoAmI, ai0: 2f);
                RequiemOfTheCosmosVFX.EventHorizonCastVFX(player.Center);
                return false;
            }
            
            // Normal cast: cosmic orb
            float mode = cosmicCastCount % 3 == 0 ? 1f : 0f; // Every 3rd: gravity well variant
            Projectile.NewProjectile(source, position, toMouse * Item.shootSpeed, type, damage, knockback, player.whoAmI, ai0: mode);
            
            RequiemOfTheCosmosVFX.CastVFX(position, toMouse);
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            float chargeProgress = cosmicCastCount / (float)EventHorizonThreshold;
            RequiemOfTheCosmosVFX.HoldItemVFX(player, chargeProgress);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 position = Item.Center - Main.screenPosition;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            
            float pulse = 0.8f + 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.03f);
            // Void outer glow
            spriteBatch.Draw(texture, position, null, new Color(10, 10, 30) * 0.6f * pulse, rotation, origin, scale * 1.8f, SpriteEffects.None, 0f);
            // Deep cosmic
            spriteBatch.Draw(texture, position, null, new Color(40, 30, 100) * 0.5f * pulse, rotation, origin, scale * 1.4f, SpriteEffects.None, 0f);
            // Stellar core
            spriteBatch.Draw(texture, position, null, new Color(180, 200, 230) * 0.4f, rotation, origin, scale * 1.1f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires massive cosmic orbs that collapse on contact"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 3rd cast fires a gravity well variant that pulls enemies inward"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", $"Every {EventHorizonThreshold}th cast unleashes Event Horizon — a colossal singularity at triple damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cosmos has a final note. Those who hear it do not remain.'")
            {
                OverrideColor = new Color(100, 120, 200)
            });
        }
    }
}
