using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;
using MagnumOpus.Common;
using MagnumOpus.Content.Nachtmusik.Weapons.StarweaversGrimoire.Projectiles;
using MagnumOpus.Content.Nachtmusik.Weapons.StarweaversGrimoire.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.StarweaversGrimoire
{
    public class StarweaversGrimoire : ModItem
    {
        // Weave system - nodes placed in world form constellations
        private int weaveNodeCount;
        private const int MaxWeaveNodes = 8;
        private int castCount;
        
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 1200;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 14;
            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4f;
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.value = Item.sellPrice(gold: 25);
            Item.shoot = ModContent.ProjectileType<StarweaverOrbProjectile>();
            Item.shootSpeed = 14f;
            Item.autoReuse = true;
            Item.crit = 20;
            Item.channel = false;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Tapestry Weave: activate all placed nodes into a damaging constellation web
                StarweaversGrimoireVFX.TapestryWeaveVFX(player.Center);
                weaveNodeCount = 0;
            }
            return base.UseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Tapestry Weave: fire a cluster of 6 seeking star bolts
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi / 6f * i;
                    Vector2 dir = angle.ToRotationVector2() * 10f;
                    Projectile.NewProjectile(source, player.Center, dir, type, (int)(damage * 1.5f), knockback, player.whoAmI, ai0: 1f);
                }
                return false;
            }

            castCount++;
            
            // Normal cast: fire orb that places a weave node on impact
            Vector2 toMouse = Main.MouseWorld - player.Center;
            toMouse.Normalize();
            toMouse *= Item.shootSpeed;
            
            Projectile.NewProjectile(source, position, toMouse, type, damage, knockback, player.whoAmI);
            
            // Every 4th cast: bonus seeking orb
            if (castCount % 4 == 0)
            {
                Vector2 offset = toMouse.RotatedByRandom(MathHelper.ToRadians(15));
                Projectile.NewProjectile(source, position, offset, type, (int)(damage * 0.6f), knockback, player.whoAmI, ai0: 2f);
            }
            
            StarweaversGrimoireVFX.CastVFX(position, toMouse);
            
            if (weaveNodeCount < MaxWeaveNodes)
                weaveNodeCount++;
                
            return false;
        }

        public override void HoldItem(Player player)
        {
            StarweaversGrimoireVFX.HoldItemVFX(player, weaveNodeCount / (float)MaxWeaveNodes);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 position = Item.Center - Main.screenPosition;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Arcane outer glow
            float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.04f);
            spriteBatch.Draw(texture, position, null, new Color(40, 30, 100) * 0.4f * pulse, rotation, origin, scale * 1.6f, SpriteEffects.None, 0f);
            // Stellar mid glow
            spriteBatch.Draw(texture, position, null, new Color(60, 80, 180) * 0.5f * pulse, rotation, origin, scale * 1.25f, SpriteEffects.None, 0f);
            // Core shimmer
            spriteBatch.Draw(texture, position, null, new Color(180, 200, 230) * 0.3f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires weaving star orbs that place arcane constellation nodes"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 4th cast fires a bonus seeking orb"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Right click unleashes Tapestry Weave — a burst of 6 star bolts at 150% damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'She opened the book and read the sky. Every star rearranged itself to listen.'")
            {
                OverrideColor = new Color(100, 120, 200)
            });
        }
    }
}
