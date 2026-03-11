using MagnumOpus.Common;
using MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer.Projectiles;
using MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer.Utilities;
using MagnumOpus.Content.SandboxExoblade.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer
{
    /// <summary>
    /// Temporal Piercer — Rapier that punctures through time.
    /// Ultra-precise thrusts leave Temporal Puncture marks (max 5).
    /// At 5 marks → Frozen Moment (stun + massive burst).
    /// Alt fire launches a time-pierce boomerang through marked enemies.
    /// </summary>
    public class TemporalPiercer : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 84;
            Item.height = 84;
            Item.DamageType = DamageClass.Melee;
            Item.damage = 3600; // Tier 10 (2800-4200 range)
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = null;
            Item.channel = true;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.crit = 22;
            Item.shoot = ModContent.ProjectileType<TemporalThrustProjectile>();
            Item.shootSpeed = 6f;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanShoot(Player player)
        {
            // Only allow re-swing during ExoBlade post-dash stasis
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.owner == player.whoAmI && p.type == Item.shoot)
                {
                    if (!(p.ai[0] == 1 && p.ai[1] == 1))
                        return false;
                }
            }
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float state = player.altFunctionUse == 2 ? 1f : 0f;
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, state);
            return false;
        }

        public override void HoldItem(Player player)
        {
            player.ExoBlade().rightClickListener = true;
            player.ExoBlade().mouseWorldListener = true;
        }

        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Ultra-precise rapier thrusts inflict Temporal Puncture marks"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Five marks trigger Frozen Moment — a burst of shattered time"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Right click to perform a Time-Pierce Lunge that inflicts two marks"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Five marks upon the hours. And when the fifth chimes — the moment freezes.'")
            {
                OverrideColor = ClairDeLunePalette.LoreText
            });
        }
    
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2.2f) * 0.05f
                + (float)Math.Sin(time * 3.8f) * 0.03f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            ClairDeLunePalette.DrawItemBloom(spriteBatch, tex, pos, origin, rotation, scale, pulse);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, ClairDeLunePalette.SoftBlue.ToVector3() * 0.35f);
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            float time = Main.GameUpdateCount * 0.04f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.06f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            float cycle = (float)Math.Sin(time * 0.7f) * 0.5f + 0.5f;
            Color glowColor = Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlWhite, cycle) * 0.24f;
            spriteBatch.Draw(tex, position, frame, glowColor with { A = 0 }, 0f, origin, scale * pulse * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
