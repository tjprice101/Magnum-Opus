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

namespace MagnumOpus.Content.Winter.Weapons
{
    /// <summary>
    /// Permafrost Codex - Winter-themed magic weapon (Post-Golem tier)
    /// An ancient tome of frost magic that channels winter's might.
    /// - Frost Barrage: Fires a spread of 5 frost bolts (165 damage)
    /// - Permafrost Barrier: Creates a defensive ice shield after channeling
    /// - Ice Storm: Charge attack summons a devastating blizzard
    /// - Frostbite: All attacks apply stacking frost damage over time
    /// </summary>
    public class PermafrostCodex : ModItem
    {
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color DeepBlue = new Color(60, 100, 180);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);
        private static readonly Color GlacialPurple = new Color(120, 130, 200);

        private int chargeTime = 0;
        private const int MaxCharge = 90;
        private bool isCharging = false;

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 36;
            Item.damage = 165;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.buyPrice(gold: 42);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item28;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<PermafrostBolt>();
            Item.shootSpeed = 16f;
            Item.channel = true;
        }

        public override void HoldItem(Player player)
        {
            // Frost rune aura
            if (Main.rand.NextBool(10))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Main.rand.NextFloat(30f, 45f);
                Vector2 runePos = player.Center + angle.ToRotationVector2() * radius;
                Vector2 runeVel = angle.ToRotationVector2() * 0.5f;
                Color runeColor = Color.Lerp(GlacialPurple, IceBlue, Main.rand.NextFloat()) * 0.4f;
                var rune = new GenericGlowParticle(runePos, runeVel, runeColor, 0.2f, 25, true);
                MagnumParticleHandler.SpawnParticle(rune);
            }

            // Charging effects
            if (isCharging && chargeTime > 0)
            {
                float chargeProgress = (float)chargeTime / MaxCharge;
                
                // Intensifying frost aura
                if (Main.rand.NextBool(3))
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    float radius = 60f - chargeProgress * 30f;
                    Vector2 chargePos = player.Center + angle.ToRotationVector2() * radius;
                    Vector2 chargeVel = (player.Center - chargePos).SafeNormalize(Vector2.Zero) * (2f + chargeProgress * 4f);
                    Color chargeColor = Color.Lerp(IceBlue, FrostWhite, chargeProgress) * (0.4f + chargeProgress * 0.4f);
                    var charge = new GenericGlowParticle(chargePos, chargeVel, chargeColor, 0.25f + chargeProgress * 0.2f, 18, true);
                    MagnumParticleHandler.SpawnParticle(charge);
                }

                // Frost swirl
                if (chargeProgress > 0.3f && Main.rand.NextBool(4))
                {
                    float swirlAngle = Main.GameUpdateCount * 0.1f;
                    Vector2 swirlPos = player.Center + swirlAngle.ToRotationVector2() * (30f * chargeProgress);
                    CustomParticles.GenericFlare(swirlPos, CrystalCyan * chargeProgress, 0.2f + chargeProgress * 0.2f, 10);
                }
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 0.4f;
            Lighting.AddLight(player.Center, GlacialPurple.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 spawnPos = position + velocity.SafeNormalize(Vector2.Zero) * 30f;

            // Check if fully charged
            if (chargeTime >= MaxCharge)
            {
                // Ice Storm - charged attack
                chargeTime = 0;
                isCharging = false;

                // Spawn blizzard projectile
                Projectile.NewProjectile(source, spawnPos, velocity * 0.5f, ModContent.ProjectileType<IceStormProjectile>(),
                    (int)(damage * 2.5f), knockback * 1.5f, player.whoAmI);

                // VFX burst
                CustomParticles.GenericFlare(spawnPos, FrostWhite, 1.2f, 30);
                // Frost sparkle burst (replacing banned HaloRing)
                var frostSparkle1 = new SparkleParticle(spawnPos, Vector2.Zero, IceBlue * 0.7f, 0.8f * 0.6f, 22);
                MagnumParticleHandler.SpawnParticle(frostSparkle1);
                var frostSparkle2 = new SparkleParticle(spawnPos, Vector2.Zero, CrystalCyan * 0.5f, 0.6f * 0.6f, 18);
                MagnumParticleHandler.SpawnParticle(frostSparkle2);
                var frostSparkle3 = new SparkleParticle(spawnPos, Vector2.Zero, GlacialPurple * 0.4f, 0.4f * 0.6f, 15);
                MagnumParticleHandler.SpawnParticle(frostSparkle3);

                for (int i = 0; i < 16; i++)
                {
                    float angle = MathHelper.TwoPi * i / 16f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f);
                    Color burstColor = Color.Lerp(IceBlue, FrostWhite, (float)i / 16f) * 0.65f;
                    var burst = new GenericGlowParticle(spawnPos, burstVel, burstColor, 0.35f, 25, true);
                    MagnumParticleHandler.SpawnParticle(burst);
                }

                return false;
            }

            // Normal attack - frost barrage (5 bolts)
            for (int i = -2; i <= 2; i++)
            {
                float spreadAngle = MathHelper.ToRadians(i * 7f);
                Vector2 boltVel = velocity.RotatedBy(spreadAngle);
                Projectile.NewProjectile(source, spawnPos, boltVel, type, damage, knockback, player.whoAmI);
            }

            // Muzzle VFX
            CustomParticles.GenericFlare(spawnPos, IceBlue, 0.55f, 16);

            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 6f) + Main.rand.NextVector2Circular(2f, 2f);
                Color sparkColor = Color.Lerp(IceBlue, GlacialPurple, Main.rand.NextFloat()) * 0.45f;
                var spark = new GenericGlowParticle(spawnPos, sparkVel, sparkColor, 0.22f, 15, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Start charging for next shot
            isCharging = true;

            return false;
        }

        public override void UpdateInventory(Player player)
        {
            // Update charge while channeling
            if (player.channel && player.HeldItem == Item)
            {
                if (chargeTime < MaxCharge)
                {
                    chargeTime++;
                }
            }
            else
            {
                chargeTime = Math.Max(0, chargeTime - 2);
                if (chargeTime == 0)
                    isCharging = false;
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, GlacialPurple * 0.35f, rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, IceBlue * 0.25f, rotation, origin, scale * 1.15f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, FrostWhite * 0.2f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, GlacialPurple.ToVector3() * 0.4f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FrostBarrage", "Fires a spread of 5 frost bolts") { OverrideColor = IceBlue });
            tooltips.Add(new TooltipLine(Mod, "IceStorm", "Hold to charge, release a devastating blizzard") { OverrideColor = CrystalCyan });
            tooltips.Add(new TooltipLine(Mod, "Frostbite", "All attacks apply stacking frost damage") { OverrideColor = GlacialPurple });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Written in the tongue of eternal frost'") { OverrideColor = Color.Lerp(GlacialPurple, FrostWhite, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<PermafrostBar>(), 18)
                .AddIngredient(ModContent.ItemType<WinterResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofSight, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
