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
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Weapons.StarweaversGrimoire.Projectiles;


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
            Item.noUseGraphic = true;
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
                // Tapestry Weave: activate all placed nodes — each fires a homing child toward cursor
                weaveNodeCount = 0;
                int orbType = ModContent.ProjectileType<StarweaverOrbProjectile>();
                Vector2 cursorWorld = Main.MouseWorld;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (!p.active || p.owner != player.whoAmI || p.type != orbType) continue;
                    if (p.localAI[0] != 1f) continue; // Not a node

                    Vector2 dir = (cursorWorld - p.Center).SafeNormalize(Vector2.UnitX) * 10f;
                    GenericHomingOrbChild.SpawnChild(
                        player.GetSource_FromThis(),
                        p.Center, dir,
                        (int)(Item.damage * player.GetTotalDamage(DamageClass.Magic).ApplyTo(1f) * 0.6f),
                        Item.knockBack * 0.5f, player.whoAmI,
                        0.08f, GenericHomingOrbChild.FLAG_ACCELERATE, GenericHomingOrbChild.THEME_NACHTMUSIK,
                        0.8f, 75);
                }
            }
            return base.UseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Tapestry Weave: fire a cluster of 6 seeking star bolts
                int projType = ModContent.ProjectileType<StarweaverOrbProjectile>();
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi / 6f * i;
                    Vector2 dir = angle.ToRotationVector2() * 10f;
                    Projectile.NewProjectile(source, player.Center, dir, projType, (int)(damage * 1.5f), knockback, player.whoAmI, ai0: 1f);
                }
                return false;
            }

            castCount++;

            // Normal cast: fire orb that places a weave node on impact
            Vector2 toMouse = Main.MouseWorld - player.Center;
            toMouse.Normalize();
            toMouse *= Item.shootSpeed;

            int orbType = ModContent.ProjectileType<StarweaverOrbProjectile>();
            Projectile.NewProjectile(source, position, toMouse, orbType, damage, knockback, player.whoAmI);

            // Every 4th cast: bonus seeking orb
            if (castCount % 4 == 0)
            {
                Vector2 offset = toMouse.RotatedByRandom(MathHelper.ToRadians(15));
                Projectile.NewProjectile(source, position, offset, orbType, (int)(damage * 0.6f), knockback, player.whoAmI, ai0: 2f);
            }
            
            
            if (weaveNodeCount < MaxWeaveNodes)
                weaveNodeCount++;
                
            return false;
        }

        public override void HoldItem(Player player)
        {
            // Ambient arcane dust — 2 every 5 frames
            if (Main.GameUpdateCount % 5 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(20f, 20f);
                    Color col = NachtmusikPalette.PaletteLerp(NachtmusikPalette.StarweaversGrimoireCast, Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.WhiteTorch,
                        new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.8f), 0, col, 0.5f);
                    d.noGravity = true;
                }
            }

            // Pulsing ambient light — violet for arcane theme
            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(player.Center, NachtmusikPalette.Violet.ToVector3() * 0.25f * pulse);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 position = Item.Center - Main.screenPosition;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            
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

            float time = Main.GameUpdateCount * 0.04f;
            float pulse = 1f + 0.08f * MathF.Sin(time * 2f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Violet arcane bloom overlay
            spriteBatch.Draw(texture, pos, null, NachtmusikPalette.Violet with { A = 0 } * 0.2f,
                rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);
            // SerenadeGlow highlight
            spriteBatch.Draw(texture, pos, null, NachtmusikPalette.SerenadeGlow with { A = 0 } * 0.15f,
                rotation, origin, scale * pulse * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires star orbs that decelerate into constellation nodes (max 8)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Nodes tether to nearby nodes, damaging enemies crossing the lines"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 4th cast fires a bonus seeking orb"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Right click unleashes Tapestry Weave — nodes fire homing bolts toward cursor, plus a burst of 6 star seekers"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'She opened the book and read the sky. Every star rearranged itself to listen.'")
            {
                OverrideColor = NachtmusikPalette.LoreText
            });
        }
    }
}
