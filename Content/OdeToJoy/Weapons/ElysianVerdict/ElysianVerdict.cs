using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Dusts;
using MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict
{
    /// <summary>
    /// Elysian Verdict — Golden judgment orb with 3-tier Elysian Mark system.
    /// MagicOrbFoundation primary. Paradise Lost below 25% HP.
    /// Crits apply 2 marks. 3 marks = Elysian Verdict detonation + 10% healing.
    /// </summary>
    public class ElysianVerdict : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.damage = 3200;
            Item.DamageType = DamageClass.Magic;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item66;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.mana = 30;
            Item.crit = 18;
            Item.shoot = ModContent.ProjectileType<ElysianOrbProjectile>();
            Item.shootSpeed = 12f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            bool isParadiseLost = player.statLife < player.statLifeMax2 * 0.25f;

            // Paradise Lost: +50% damage but no healing from Verdict
            int finalDamage = isParadiseLost ? (int)(damage * 1.5f) : damage;
            float ai0 = isParadiseLost ? 1f : 0f;

            Projectile.NewProjectile(source, position, velocity, type, finalDamage, knockback, player.whoAmI, ai0);

            return false;
        }

        public override void HoldItem(Player player)
        {
            // Elysian Radiance aura — soft golden glow, 5 tile radius, +3% dmg for allies
            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = 40f + Main.rand.NextFloat() * 40f;
                Vector2 pos = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;
                Dust d = Dust.NewDustDirect(pos, 1, 1, ModContent.DustType<ElysianJudgmentDust>(), 0f, -0.3f, 120, default, 0.3f);
                d.noGravity = true;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires golden judgment orbs that apply Elysian Marks on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Mark tiers: 1 = +10% magic vulnerability, 2 = +20% + burn DoT, 3 = Elysian Verdict detonation"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Critical hits apply 2 marks instead of 1"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Paradise Lost: below 25% HP, orbs deal +50% damage but Verdict healing is disabled"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Elysium's gates open only for those the light deems worthy. None have been worthy.'")
            {
                OverrideColor = new Color(255, 200, 50)
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

            OdeToJoyPalette.DrawItemBloom(spriteBatch, tex, pos, origin, rotation, scale, pulse);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.35f);
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
            Color glowColor = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.RosePink, cycle) * 0.24f;
            spriteBatch.Draw(tex, position, frame, glowColor with { A = 0 }, 0f, origin, scale * pulse * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}