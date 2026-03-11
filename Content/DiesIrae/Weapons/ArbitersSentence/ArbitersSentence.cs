using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria;
using Terraria.GameContent;


namespace MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence
{
    public class ArbitersSentence : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 24;
            Item.damage = 400; // Tier 8 (1600-2400 range), speed-proportional for useTime=3 burst
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 3;
            Item.useAnimation = 9;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 1f;
            Item.value = Item.sellPrice(platinum: 1, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item34 with { Pitch = 0.15f, Volume = 0.6f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<JudgmentFlameProjectile>();
            Item.shootSpeed = 11f;
            Item.useAmmo = AmmoID.Gel;
            Item.crit = 15;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10f, 0f);

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spray slight random spread for flamethrower feel
            float spread = MathHelper.ToRadians(4f);
            velocity = velocity.RotatedByRandom(spread);

            // Occasionally spawn lingering purgatory embers (every ~10th shot)
            if (Main.rand.NextBool(10))
            {
                Vector2 emberVel = velocity * 0.4f;
                Projectile.NewProjectile(source, position, emberVel,
                    ModContent.ProjectileType<PurgatoryEmberProjectile>(),
                    (int)(damage * 0.3f), knockback * 0.2f, player.whoAmI);
            }

            // Always spawn our custom VFX flame, not the ammo's vanilla type
            Projectile.NewProjectile(source, position, velocity,
                ModContent.ProjectileType<JudgmentFlameProjectile>(),
                damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Precision flamethrower that applies stacking Judgment Flame (15 damage/s per stack)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "At 5 stacks: Sentence Cage roots enemy, next hit deals double damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "5 consecutive hits on same target activates Arbiter's Focus — 3 precision shots with +40% damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Killing sentenced enemies transfers flames to nearby foes"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The arbiter does not miss. The arbiter does not forgive.'")
            {
                OverrideColor = new Color(200, 50, 30)
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

            // Switch to additive blend for glow layers
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            DiesIraePalette.DrawItemBloom(spriteBatch, tex, pos, origin, rotation, scale, pulse);

            // Restore alpha blend for vanilla drawing
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, DiesIraePalette.InfernalRed.ToVector3() * 0.35f);
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
            Color glowColor = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold, cycle) * 0.24f;
            spriteBatch.Draw(tex, position, frame, glowColor with { A = 0 }, 0f, origin, scale * pulse * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}