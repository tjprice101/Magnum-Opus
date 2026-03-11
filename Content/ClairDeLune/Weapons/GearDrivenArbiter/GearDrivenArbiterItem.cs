using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.ClairDeLune.Weapons.GearDrivenArbiter.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.GearDrivenArbiter
{
    /// <summary>
    /// Gear-Driven Arbiter — Summon weapon. Clockwork construct minion.
    /// Fires spinning gear projectiles (16px, 12°/f spin).
    /// Temporal Judgment: 8 stacks → 3s countdown → Arbiter's Verdict (5x damage).
    /// Clockwork Court: 3+ Arbiters = coordinated barrages every 8s (+30%).
    /// "A court of gears that judges in silence."
    /// </summary>
    public class GearDrivenArbiterItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/GearDrivenArbiter/GearDrivenArbiter";

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.damage = 2900; // Tier 10 (2800-4200 range)
            Item.DamageType = DamageClass.Summon;
            Item.mana = 16;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3.5f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<ArbiterMinionProjectile>();
            Item.buffType = ModContent.BuffType<GearDrivenArbiterBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a Gear-Driven Arbiter clockwork construct"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "Fires spinning gear projectiles at nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Judgment", "Temporal Judgment: gears apply verdict stacks (8 stacks → Arbiter's Verdict for 5x damage)"));
            tooltips.Add(new TooltipLine(Mod, "Court", "Clockwork Court: 3+ Arbiters fire coordinated barrages every 8s (+30% damage)"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A court of gears that judges in silence.'")
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

    /// <summary>
    /// Gear-Driven Arbiter minion buff.
    /// </summary>
    public class GearDrivenArbiterBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_24";

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ArbiterMinionProjectile>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
