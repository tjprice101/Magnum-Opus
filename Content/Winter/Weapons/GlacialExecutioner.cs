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
    /// Glacial Executioner - Winter-themed melee weapon (Post-Golem tier)
    /// A massive frozen greataxe that channels winter's wrath.
    /// - Frozen Cleave: Devastating swings that leave ice trails (195 damage)
    /// - Absolute Zero: Every hit has 25% chance to freeze enemies solid
    /// - Avalanche Strike: Every 6th swing creates a cascading ice wave
    /// - Permafrost: Frozen enemies take 30% more damage from all sources
    /// </summary>
    public class GlacialExecutioner : ModItem
    {
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color DeepBlue = new Color(60, 100, 180);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);

        private int swingCount = 0;

        public override void SetDefaults()
        {
            Item.width = 72;
            Item.height = 72;
            Item.damage = 195;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 32;
            Item.useAnimation = 32;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.value = Item.buyPrice(gold: 45);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<AvalancheWave>();
            Item.shootSpeed = 14f;
        }

        public override void HoldItem(Player player)
        {
            // Ambient frost particles
            if (Main.rand.NextBool(8))
            {
                Vector2 auraPos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                Vector2 auraVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-0.5f, 0.5f));
                Color auraColor = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat()) * 0.4f;
                var aura = new GenericGlowParticle(auraPos, auraVel, auraColor, 0.22f, 30, true);
                MagnumParticleHandler.SpawnParticle(aura);
            }

            // Falling snowflakes
            if (Main.rand.NextBool(12))
            {
                Vector2 snowPos = player.Center + new Vector2(Main.rand.NextFloat(-50f, 50f), -40f);
                Vector2 snowVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(1f, 2f));
                Color snowColor = FrostWhite * 0.5f;
                var snow = new GenericGlowParticle(snowPos, snowVel, snowColor, 0.15f, 45, true);
                MagnumParticleHandler.SpawnParticle(snow);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 0.45f;
            Lighting.AddLight(player.Center, IceBlue.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            swingCount++;

            // Ice trail along swing arc
            float arcAngle = velocity.ToRotation();
            for (int i = 0; i < 10; i++)
            {
                float angle = arcAngle + MathHelper.ToRadians(-45f + i * 10f);
                float dist = Main.rand.NextFloat(45f, 90f);
                Vector2 particlePos = player.Center + angle.ToRotationVector2() * dist;
                Vector2 particleVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color particleColor = Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat()) * 0.6f;
                var particle = new GenericGlowParticle(particlePos, particleVel, particleColor, 0.28f, 22, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Avalanche Strike - every 6th swing
            if (swingCount >= 6)
            {
                swingCount = 0;

                // Spawn avalanche wave
                Projectile.NewProjectile(source, player.Center, velocity * 1.2f, type, (int)(damage * 1.5f), knockback, player.whoAmI);

                // VFX burst
                CustomParticles.GenericFlare(player.Center, FrostWhite, 1.0f, 25);
                CustomParticles.HaloRing(player.Center, IceBlue * 0.7f, 0.7f, 20);
                CustomParticles.HaloRing(player.Center, CrystalCyan * 0.5f, 0.5f, 18);
                CustomParticles.HaloRing(player.Center, DeepBlue * 0.4f, 0.35f, 15);

                // Ice crystal explosion
                for (int i = 0; i < 14; i++)
                {
                    float angle = MathHelper.TwoPi * i / 14f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                    Color burstColor = Color.Lerp(IceBlue, FrostWhite, (float)i / 14f) * 0.7f;
                    var burst = new GenericGlowParticle(player.Center, burstVel, burstColor, 0.38f, 26, true);
                    MagnumParticleHandler.SpawnParticle(burst);
                }
            }

            return false;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Frost trail particles
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                Vector2 trailVel = player.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f);
                Color trailColor = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat()) * 0.55f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.3f, 20, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Absolute Zero: 25% freeze chance
            if (Main.rand.NextFloat() < 0.25f)
            {
                // Freeze effect (Frozen debuff)
                target.AddBuff(BuffID.Frozen, 90);
                
                // Freeze VFX
                CustomParticles.GenericFlare(target.Center, CrystalCyan, 0.75f, 22);
                CustomParticles.HaloRing(target.Center, IceBlue * 0.7f, 0.55f, 18);

                // Ice crystal burst
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f;
                    Vector2 crystalVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                    Color crystalColor = Color.Lerp(CrystalCyan, FrostWhite, Main.rand.NextFloat()) * 0.7f;
                    var crystal = new GenericGlowParticle(target.Center, crystalVel, crystalColor, 0.3f, 22, true);
                    MagnumParticleHandler.SpawnParticle(crystal);
                }
            }

            // Always apply Frostburn
            target.AddBuff(BuffID.Frostburn2, 240);

            // Standard hit VFX
            CustomParticles.GenericFlare(target.Center, IceBlue, 0.55f, 16);
            
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                Color sparkColor = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat()) * 0.5f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            // Permafrost: Frozen enemies take 30% more damage
            if (target.HasBuff(BuffID.Frozen))
            {
                modifiers.FinalDamage *= 1.3f;
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.045f) * 0.1f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, IceBlue * 0.35f, rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, CrystalCyan * 0.25f, rotation, origin, scale * 1.15f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, FrostWhite * 0.2f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, IceBlue.ToVector3() * 0.5f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FrozenCleave", "Devastating swings that leave trails of frost") { OverrideColor = IceBlue });
            tooltips.Add(new TooltipLine(Mod, "AbsoluteZero", "25% chance to freeze enemies solid on hit") { OverrideColor = CrystalCyan });
            tooltips.Add(new TooltipLine(Mod, "AvalancheStrike", "Every 6th swing unleashes a cascading ice wave") { OverrideColor = FrostWhite });
            tooltips.Add(new TooltipLine(Mod, "Permafrost", "Frozen enemies take 30% bonus damage") { OverrideColor = DeepBlue });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cold embrace of eternal winter'") { OverrideColor = Color.Lerp(IceBlue, FrostWhite, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<PermafrostBar>(), 20)
                .AddIngredient(ModContent.ItemType<WinterResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofMight, 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
