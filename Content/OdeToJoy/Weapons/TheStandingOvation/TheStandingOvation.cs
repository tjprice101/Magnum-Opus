using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Dusts;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Utilities;
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

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation
{
    /// <summary>
    /// The Standing Ovation — Summon weapon.
    /// Summons phantom spectator minions that attack with applause waves,
    /// thrown roses, and standing rushes. Ovation Meter → Standing Ovation Event.
    /// +5% damage per additional crowd member. Cross-summon sync with Triumphant Chorus.
    /// Encore: re-summon within 5s of event = 2 minions for 1 slot.
    /// </summary>
    public class TheStandingOvation : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.damage = 2600;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.crit = 4;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<StandingOvationMinion>();
            Item.shootSpeed = 10f;
            Item.buffType = ModContent.BuffType<StandingOvationBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            OvationPlayer op = player.GetModPlayer<OvationPlayer>();

            // Encore bonus: re-summon during encore window spawns 2 minions for cost of 1
            int count = op.EncoreReady ? 2 : 1;
            for (int i = 0; i < count; i++)
            {
                Vector2 spawnVel = velocity.RotatedByRandom(MathHelper.ToRadians(15f));
                Projectile.NewProjectile(source, position, spawnVel, type, damage, knockback, player.whoAmI);
            }

            // Summoning VFX burst — golden applause sparkles
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustDirect(position, 1, 1, ModContent.DustType<ApplauseSparkDust>(), vel.X, vel.Y, 80,
                    default, 0.8f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Summoning celebration VFX
            OdeToJoyVFXLibrary.SpawnGardenSparkleExplosion(position, 4, 4f, 1f);

            return false;
        }

        public override void HoldItem(Player player)
        {
            // Ovation Meter ambient VFX — golden particles proportional to meter
            OvationPlayer op = player.GetModPlayer<OvationPlayer>();
            if (op.OvationMeter > 30f && Main.rand.NextFloat() < op.OvationMeter / 200f)
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Dust d = Dust.NewDustDirect(player.Center + offset - new Vector2(2), 4, 4, ModContent.DustType<ApplauseSparkDust>(),
                    0f, -1f, 100, default, 0.5f);
                d.noGravity = true;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons phantom spectators that attack with applause waves, roses, and charges"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Each additional spectator adds +5% crowd damage bonus"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Kills build Ovation Meter toward a devastating Standing Ovation Event"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Re-summon during Encore window to summon 2 spectators for 1"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The audience loved the performance. The audience demands an encore.'")
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