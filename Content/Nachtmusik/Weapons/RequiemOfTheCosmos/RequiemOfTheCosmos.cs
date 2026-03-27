using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;
using MagnumOpus.Common;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Weapons.RequiemOfTheCosmos.Projectiles;


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
            Item.noUseGraphic = true;
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
                int orbType = ModContent.ProjectileType<CosmicRequiemOrbProjectile>();
                Projectile.NewProjectile(source, position, toMouse * 8f, orbType, damage * 3, knockback * 2f, player.whoAmI, ai0: 2f);
                return false;
            }

            // Normal cast: cosmic orb
            float mode = cosmicCastCount % 3 == 0 ? 1f : 0f; // Every 3rd: gravity well variant
            int normalOrbType = ModContent.ProjectileType<CosmicRequiemOrbProjectile>();
            Projectile.NewProjectile(source, position, toMouse * Item.shootSpeed, normalOrbType, damage, knockback, player.whoAmI, ai0: mode);
            
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            float chargeProgress = cosmicCastCount / (float)EventHorizonThreshold;

            // Ambient cosmic dust — 2 every 5 frames, intensity scales with charge
            if (Main.GameUpdateCount % 5 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(20f, 20f);
                    Color col = NachtmusikPalette.PaletteLerp(NachtmusikPalette.RequiemOfTheCosmosCast, Main.rand.NextFloat());
                    float dustScale = 0.5f + chargeProgress * 0.3f;
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.WhiteTorch,
                        new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.8f), 0, col, dustScale);
                    d.noGravity = true;
                }
            }

            // Pulsing ambient light — deep blue with gold accent at higher charge
            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Color lightCol = Color.Lerp(NachtmusikPalette.DeepBlue, NachtmusikPalette.RadianceGold, chargeProgress * 0.5f);
            Lighting.AddLight(player.Center, lightCol.ToVector3() * 0.3f * pulse);
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


        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            float time = Main.GameUpdateCount * 0.04f;
            float twinkle = 1f + (float)Math.Sin(time * 2.3f) * 0.07f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            float cycle = (float)Math.Sin(time * 0.7f) * 0.5f + 0.5f;
            Color glowColor = Color.Lerp(NachtmusikPalette.StarlitBlue, NachtmusikPalette.StarGold, cycle) * 0.24f;
            spriteBatch.Draw(tex, position, frame, glowColor, 0f, origin, scale * twinkle * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;

            float time = Main.GameUpdateCount * 0.03f;
            float pulse = 1f + 0.1f * MathF.Sin(time * 1.8f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Deep blue cosmic bloom overlay
            spriteBatch.Draw(texture, pos, null, NachtmusikPalette.DeepBlue with { A = 0 } * 0.22f,
                rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            // Radiance gold highlight
            spriteBatch.Draw(texture, pos, null, NachtmusikPalette.RadianceGold with { A = 0 } * 0.15f,
                rotation, origin, scale * pulse * 1.08f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires massive cosmic orbs that collapse on contact"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 3rd cast fires a gravity well variant that pulls enemies inward"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", $"Every {EventHorizonThreshold}th cast unleashes Event Horizon ? a colossal singularity at triple damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cosmos has a final note. Those who hear it do not remain.'")
            {
                OverrideColor = NachtmusikPalette.LoreText
            });
        }
    }
}
