using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Spring.Materials;
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
    public class BlossomsEdge : ModItem
    {
        // Spring colors
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringWhite = new Color(255, 250, 250);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color CherryBlossom = new Color(255, 183, 197);

        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.damage = 72;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<BlossomsEdgeSwing>();
            Item.shootSpeed = 1f;
        }

        public override bool CanShoot(Player player)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == Item.shoot)
                    return false;
            }
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, 0f, 0);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "PetalTrail", "Swings scatter damaging cherry blossom petals") { OverrideColor = SpringPink });
            tooltips.Add(new TooltipLine(Mod, "RenewalStrike", "Every 5th hit heals you for 8 HP") { OverrideColor = SpringGreen });
            tooltips.Add(new TooltipLine(Mod, "SpringBloom", "Critical hits cause flowers to burst, dealing 50% damage in area") { OverrideColor = SpringPink });
            tooltips.Add(new TooltipLine(Mod, "VernalVigor", "Increased damage and attack speed during daytime") { OverrideColor = new Color(255, 220, 100) });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Where the blade touches, spring eternally blooms'") { OverrideColor = Color.Lerp(SpringPink, SpringGreen, 0.5f) });
        }

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
            try
            {
                Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
                Vector2 position = Item.Center - Main.screenPosition;
                Vector2 bloomOrigin = bloomTex.Size() / 2f;
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.12f + 1f;

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                spriteBatch.Draw(bloomTex, position, null, SpringPink * 0.35f, 0f, bloomOrigin, scale * pulse * 0.15f, SpriteEffects.None, 0f);
                spriteBatch.Draw(bloomTex, position, null, SpringWhite * 0.25f, 0f, bloomOrigin, scale * pulse * 0.11f, SpriteEffects.None, 0f);
                spriteBatch.Draw(bloomTex, position, null, SpringGreen * 0.2f, 0f, bloomOrigin, scale * 0.08f, SpriteEffects.None, 0f);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch { }
            finally
            {
                try { spriteBatch.End(); } catch { }
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

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
                .AddIngredient(ModContent.ItemType<DormantSpringCore>(), 1)
                .AddIngredient(ItemID.SoulofLight, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        #endregion
    }
}
