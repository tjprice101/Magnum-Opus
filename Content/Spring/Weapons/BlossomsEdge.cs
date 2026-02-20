using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Spring.Weapons
{
    /// <summary>
    /// Blossom's Edge — Spring-themed held-projectile melee sword (Post-WoF tier).
    /// Now uses MeleeSwingItemBase + BlossomsEdgeSwing for Calamity-style swing architecture.
    /// - Petal Trail: Held-projectile swing scatters cherry blossom petals
    /// - Renewal Strike: Every 5th hit heals the player for 8 HP
    /// - Spring Bloom: Critical hits cause flowers to burst from enemies, dealing 50% AoE
    /// - Vernal Vigor: Increased attack speed during daytime
    /// </summary>
    public class BlossomsEdge : MeleeSwingItemBase
    {
        // Spring colors
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringWhite = new Color(255, 250, 250);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color CherryBlossom = new Color(255, 183, 197);

        #region ── Abstract Overrides (MeleeSwingItemBase) ──

        protected override int SwingProjectileType
            => ModContent.ProjectileType<BlossomsEdgeSwing>();

        protected override int ComboStepCount => 3;

        #endregion

        #region ── Virtual Overrides ──

        protected override Color GetLoreColor()
            => Color.Lerp(SpringPink, SpringGreen, 0.5f);

        protected override void SetWeaponDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.damage = 72;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.knockBack = 5f;
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item1;
        }

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "PetalTrail", "Swings scatter damaging cherry blossom petals") { OverrideColor = SpringPink });
            tooltips.Add(new TooltipLine(Mod, "RenewalStrike", "Every 5th hit heals you for 8 HP") { OverrideColor = SpringGreen });
            tooltips.Add(new TooltipLine(Mod, "SpringBloom", "Critical hits cause flowers to burst, dealing 50% damage in area") { OverrideColor = SpringPink });
            tooltips.Add(new TooltipLine(Mod, "VernalVigor", "Increased damage and attack speed during daytime") { OverrideColor = new Color(255, 220, 100) });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Where the blade touches, spring eternally blooms'") { OverrideColor = GetLoreColor() });
        }

        #endregion

        #region ── Vernal Vigor (daytime bonuses) ──

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            if (Main.dayTime)
                damage += 0.12f;
        }

        public override float UseSpeedMultiplier(Player player)
            => Main.dayTime ? 1.15f : 1f;

        #endregion

        #region ── HoldItem — Ambient VFX ──

        public override void HoldItem(Player player)
        {
            base.HoldItem(player); // combo reset timer

            // Ambient petal particles
            if (Main.rand.NextBool(8))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-0.5f, 0.5f));
                Color petalColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat());
                var petal = new GenericGlowParticle(pos, vel, petalColor * 0.8f, 0.3f, 40, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }

            // Orbiting flower petals
            if (Main.rand.NextBool(12))
            {
                float angle = Main.GameUpdateCount * 0.03f;
                for (int i = 0; i < 3; i++)
                {
                    float petalAngle = angle + MathHelper.TwoPi * i / 3f;
                    float radius = 30f + (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 8f;
                    Vector2 petalPos = player.Center + petalAngle.ToRotationVector2() * radius;
                    CustomParticles.GenericFlare(petalPos, CherryBlossom * 0.7f, 0.25f, 15);
                }
            }

            // Spring melody — floating music notes
            if (Main.rand.NextBool(14))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                Vector2 noteVel = new Vector2(0, -Main.rand.NextFloat(0.3f, 0.8f));
                Color noteColor = Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat()) * 0.7f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.75f, 45);

                var sparkle = new SparkleParticle(notePos, noteVel * 0.5f, SpringWhite * 0.4f, 0.18f, 25);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 0.6f;
            Lighting.AddLight(player.Center, SpringPink.ToVector3() * pulse * 0.5f);
        }

        #endregion

        #region ── PreDrawInWorld — Item Glow ──

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.12f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, SpringPink * 0.4f, rotation, origin, scale * pulse * 1.4f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SpringWhite * 0.3f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SpringGreen * 0.25f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, SpringPink.ToVector3() * 0.5f);
            return true;
        }

        #endregion

        #region ── Recipe ──

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<VernalBar>(), 12)
                .AddIngredient(ModContent.ItemType<SpringResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofLight, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        #endregion
    }
}
