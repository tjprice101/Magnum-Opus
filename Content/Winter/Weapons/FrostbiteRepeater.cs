using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.Winter.Projectiles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Winter.Weapons
{
    /// <summary>
    /// Frostbite Repeater - Winter-themed ranged weapon (Post-Golem tier)
    /// A crystalline crossbow that fires volleys of ice bolts.
    /// - Icicle Volley: Fires 3 icicle bolts per shot (135 damage)
    /// - Crystalline Penetration: Icicles pierce through 3 enemies
    /// - Hypothermia: Stacking slow effect, at 5 stacks enemies freeze
    /// - Blizzard Barrage: Right-click to fire a spread of 7 homing ice shards
    /// </summary>
    public class FrostbiteRepeater : ModItem
    {
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color DeepBlue = new Color(60, 100, 180);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);

        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 26;
            Item.damage = 135;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.buyPrice(gold: 40);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item75;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<IcicleBolt>();
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Arrow;
        }

        public override void HoldItem(Player player)
        {
            // Frost aura
            if (Main.rand.NextBool(10))
            {
                Vector2 auraPos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 auraVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.5f, 0.5f));
                Color auraColor = Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat()) * 0.35f;
                var aura = new GenericGlowParticle(auraPos, auraVel, auraColor, 0.18f, 25, true);
                MagnumParticleHandler.SpawnParticle(aura);
            }

            // Floating winter melody notes (drifting like snowflakes)
            if (Main.rand.NextBool(12))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(38f, 38f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(0.1f, 0.5f)); // Gentle downward drift
                Color noteColor = Color.Lerp(new Color(150, 200, 255), new Color(240, 250, 255), Main.rand.NextFloat()) * 0.6f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.75f, 40);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.08f + 0.35f;
            Lighting.AddLight(player.Center, IceBlue.ToVector3() * pulse);
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Blizzard Barrage - right click
                Item.useTime = 45;
                Item.useAnimation = 45;
                Item.UseSound = SoundID.Item30;
            }
            else
            {
                // Normal shot
                Item.useTime = 22;
                Item.useAnimation = 22;
                Item.UseSound = SoundID.Item75;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 40f;

            if (player.altFunctionUse == 2)
            {
                // Blizzard Barrage - 7 homing shards in a spread
                for (int i = -3; i <= 3; i++)
                {
                    float spreadAngle = MathHelper.ToRadians(i * 8f);
                    Vector2 shardVel = velocity.RotatedBy(spreadAngle) * 0.85f;
                    Projectile.NewProjectile(source, muzzlePos, shardVel, ModContent.ProjectileType<BlizzardShardProjectile>(),
                        (int)(damage * 0.7f), knockback * 0.6f, player.whoAmI);
                }

                // Barrage VFX
                CustomParticles.GenericFlare(muzzlePos, FrostWhite, 0.85f, 22);
                // Frost sparkle burst 
                var frostSparkle = new SparkleParticle(muzzlePos, Vector2.Zero, IceBlue * 0.6f, 0.5f * 0.6f, 18);
                MagnumParticleHandler.SpawnParticle(frostSparkle);

                // Music note ring and burst for blizzard barrage
                ThemedParticles.MusicNoteRing(muzzlePos, new Color(150, 200, 255), 40f, 6);
                ThemedParticles.MusicNoteBurst(muzzlePos, new Color(240, 250, 255), 5, 4f);

                // Icy sparkle accents
                for (int j = 0; j < 5; j++)
                {
                    var sparkle = new SparkleParticle(muzzlePos + Main.rand.NextVector2Circular(15f, 15f),
                        Main.rand.NextVector2Circular(2f, 2f), new Color(240, 250, 255) * 0.6f, 0.22f, 18);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }

                for (int i = 0; i < 10; i++)
                {
                    float angle = velocity.ToRotation() + MathHelper.ToRadians(Main.rand.NextFloat(-30f, 30f));
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                    Color burstColor = Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat()) * 0.5f;
                    var burst = new GenericGlowParticle(muzzlePos, burstVel, burstColor, 0.28f, 18, true);
                    MagnumParticleHandler.SpawnParticle(burst);
                }

                return false;
            }

            // Normal shot - 3 icicle volley
            for (int i = -1; i <= 1; i++)
            {
                float spreadAngle = MathHelper.ToRadians(i * 5f);
                Vector2 boltVel = velocity.RotatedBy(spreadAngle);
                Projectile.NewProjectile(source, muzzlePos, boltVel, ModContent.ProjectileType<IcicleBolt>(),
                    damage, knockback, player.whoAmI);
            }

            // Muzzle VFX
            CustomParticles.GenericFlare(muzzlePos, IceBlue, 0.55f, 15);

            // Music note on shot
            ThemedParticles.MusicNote(muzzlePos, velocity * 0.1f, new Color(180, 230, 255) * 0.8f, 0.7f, 25);

            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 7f) + Main.rand.NextVector2Circular(2f, 2f);
                Color sparkColor = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat()) * 0.45f;
                var spark = new GenericGlowParticle(muzzlePos, sparkVel, sparkColor, 0.22f, 14, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.08f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, IceBlue * 0.3f, rotation, origin, scale * pulse * 1.25f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, CrystalCyan * 0.2f, rotation, origin, scale * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, IceBlue.ToVector3() * 0.4f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "IcicleVolley", "Fires a volley of 3 piercing icicle bolts") { OverrideColor = IceBlue });
            tooltips.Add(new TooltipLine(Mod, "Hypothermia", "Hits inflict stacking Hypothermia - at 5 stacks, enemies freeze") { OverrideColor = CrystalCyan });
            tooltips.Add(new TooltipLine(Mod, "BlizzardBarrage", "Right-click to fire 7 homing frost shards") { OverrideColor = FrostWhite });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The winds of the north made manifest'") { OverrideColor = Color.Lerp(IceBlue, FrostWhite, 0.5f) });
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-6f, 0f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<PermafrostBar>(), 18)
                .AddIngredient(ModContent.ItemType<WinterResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofMight, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
