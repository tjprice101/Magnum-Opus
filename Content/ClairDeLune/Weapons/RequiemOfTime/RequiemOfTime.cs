using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.ClairDeLune.Weapons.RequiemOfTime.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.RequiemOfTime
{
    /// <summary>
    /// Requiem of Time — Magic weapon with Forward/Reverse time fields.
    /// Left click: Forward Field (speeds allies 30%, PerlinFlow, 6s, 12 tiles)
    /// Right click: Reverse Field (slows enemies 40%, MarbleSwirl, costs 5% HP)
    /// Overlap = Temporal Paradox (VoronoiCell, massive AoE, screen distortion)
    /// "Time is not a river — it is an ocean, and you are the tide."
    /// </summary>
    public class RequiemOfTime : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 4000; // Tier 10 (2800-4200 range), very slow heavy magic
            Item.DamageType = DamageClass.Magic;
            Item.mana = 25;
            Item.useTime = 50;
            Item.useAnimation = 50;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item117;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<ForwardFieldProjectile>();
            Item.shootSpeed = 0f;
            Item.crit = 8;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Reverse Field costs 5% HP
                int hpCost = Math.Max(1, (int)(player.statLifeMax2 * 0.05f));
                if (player.statLife <= hpCost)
                    return false;

                Item.mana = 15;
            }
            else
            {
                Item.mana = 25;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Reverse Field — slows enemies, costs HP
                int hpCost = Math.Max(1, (int)(player.statLifeMax2 * 0.05f));
                player.statLife -= hpCost;
                if (player.statLife < 1) player.statLife = 1;
                player.HealEffect(-hpCost);

                Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero,
                    ModContent.ProjectileType<ReverseFieldProjectile>(),
                    (int)(damage * 0.6f), knockback, player.whoAmI);

                SoundEngine.PlaySound(SoundID.Item117 with { Pitch = -0.5f, Volume = 0.6f }, Main.MouseWorld);

                // Blood cost particle
                var blood = new BloomParticle(player.Center, Vector2.Zero,
                    ClairDeLunePalette.TemporalCrimson with { A = 0 } * 0.5f, 0.3f, 15);
                MagnumParticleHandler.SpawnParticle(blood);
            }
            else
            {
                // Forward Field — speeds allies
                Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero,
                    ModContent.ProjectileType<ForwardFieldProjectile>(),
                    damage, knockback, player.whoAmI);

                SoundEngine.PlaySound(SoundID.Item117 with { Pitch = 0.3f, Volume = 0.5f }, Main.MouseWorld);
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Deploys a Forward Field that speeds allies by 30%"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Right click deploys a Reverse Field that slows enemies by 40%"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Reverse Field costs 5% of max HP instead of extra mana"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Overlapping fields create a Temporal Paradox — massive damage burst"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Time is not a river — it is an ocean, and you are the tide.'")
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
