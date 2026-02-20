using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Winter.Weapons
{
    /// <summary>
    /// Glacial Executioner — Winter-themed melee weapon (Post-Golem tier).
    /// A massive frost claymore that shatters permafrost, using held-projectile combo architecture.
    /// Unique Mechanic: Frozen enemies take 30% bonus damage (Permafrost).
    /// </summary>
    public class GlacialExecutioner : MeleeSwingItemBase
    {
        #region ── Theme Colors ──

        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color DeepBlue = new Color(60, 100, 180);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);

        #endregion

        #region ── Abstract Overrides (MeleeSwingItemBase) ──

        protected override int SwingProjectileType => ModContent.ProjectileType<GlacialExecutionerSwing>();
        protected override int ComboStepCount => 4;

        #endregion

        #region ── Virtual Overrides ──

        protected override Color GetLoreColor() => Color.Lerp(IceBlue, FrostWhite, 0.5f);

        protected override void SetWeaponDefaults()
        {
            Item.width = 72;
            Item.height = 72;
            Item.damage = 195;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 32;
            Item.useAnimation = 32;
            Item.knockBack = 8f;
            Item.value = Item.buyPrice(gold: 45);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item1;
        }

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FrozenCleave", "Devastating swings that leave trails of frost") { OverrideColor = IceBlue });
            tooltips.Add(new TooltipLine(Mod, "AbsoluteZero", "25% chance to freeze enemies solid on hit") { OverrideColor = CrystalCyan });
            tooltips.Add(new TooltipLine(Mod, "AvalancheStrike", "Every 6th swing unleashes a cascading ice wave") { OverrideColor = FrostWhite });
            tooltips.Add(new TooltipLine(Mod, "Permafrost", "Frozen enemies take 30% bonus damage") { OverrideColor = DeepBlue });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cold embrace of eternal winter'") { OverrideColor = GetLoreColor() });
        }

        #endregion

        #region ── Permafrost (damage bonus vs Frozen) ──

        public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.HasBuff(BuffID.Frozen))
            {
                modifiers.FinalDamage *= 1.3f;
            }
        }

        #endregion

        #region ── HoldItem — Ambient VFX ──

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            if (Main.gameMenu) return;

            // Frost mist particles — cold breath effect
            if (Main.rand.NextBool(20))
            {
                Vector2 offset = Main.rand.NextVector2Circular(18f, 18f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.6f, -0.2f));
                Color mistColor = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat()) * 0.5f;
                var mist = new GenericGlowParticle(player.Center + offset, vel, mistColor, 0.22f, 35, true);
                MagnumParticleHandler.SpawnParticle(mist);
            }

            // Ambient pulse lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 0.5f;
            Lighting.AddLight(player.Center, IceBlue.ToVector3() * pulse * 0.5f);
        }

        #endregion

        #region ── PreDrawInWorld — Item Glow ──

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.045f) * 0.1f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, IceBlue * 0.35f, rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, CrystalCyan * 0.25f, rotation, origin, scale * 1.15f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, FrostWhite * 0.2f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, IceBlue.ToVector3() * 0.5f);

            return true;
        }

        #endregion

        #region ── Recipe ──

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<PermafrostBar>(), 20)
                .AddIngredient(ModContent.ItemType<WinterResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofMight, 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        #endregion
    }
}
