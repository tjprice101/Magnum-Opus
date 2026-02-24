using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.Projectiles;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.MoonlightSonata.Enemies;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling
{
    /// <summary>
    /// Moonlight's Calling — "The Serenade".
    /// A magic tome that casts rapid bouncing moonlight beams.
    /// Theme: Musical refraction — beams bounce and split into prismatic spectral colors.
    /// </summary>
    public class MoonlightsCalling : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.damage = 200;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 8;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.buyPrice(gold: 25);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
            Item.UseSound = SoundID.Item72;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<MoonlightBeam>();
            Item.shootSpeed = 16f;
            Item.noMelee = true;
            Item.staff[Item.type] = true;
            Item.maxStack = 1;
        }

        public override void HoldItem(Player player)
        {
            if (Main.dedServ) return;

            float time = Main.GameUpdateCount * 0.04f;
            float pulse = MathF.Sin(time * 1.5f) * 0.12f + 0.88f;

            // Orbiting prismatic motes — tiny refraction points circling the player
            if (Main.rand.NextBool(6))
            {
                float orbitAngle = time + Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 25f + MathF.Sin(time * 2f + Main.rand.NextFloat()) * 8f;
                Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * radius;

                float hueProgress = (MathF.Sin(orbitAngle * 2f) + 1f) * 0.5f;
                Color moteColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple, MoonlightVFXLibrary.IceBlue, hueProgress);
                CustomParticles.GenericFlare(orbitPos, moteColor * 0.7f, 0.25f * pulse, 12);
            }

            // Prismatic sparkle aura
            if (Main.rand.NextBool(8))
            {
                Vector2 offset = Main.rand.NextVector2Circular(22f, 22f);
                Color gradientColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple, MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                CustomParticles.PrismaticSparkle(player.Center + offset, gradientColor * 0.6f, 0.22f);
            }

            // Orbiting music notes — the defining "Serenade" visual (scale 0.75f+)
            if (Main.rand.NextBool(10))
            {
                float noteOrbitAngle = time * 0.8f;
                for (int i = 0; i < 2; i++)
                {
                    float noteAngle = noteOrbitAngle + MathHelper.TwoPi * i / 2f;
                    Vector2 notePos = player.Center + noteAngle.ToRotationVector2() * 30f;
                    MoonlightVFXLibrary.SpawnMusicNotes(notePos, 1, 2f, 0.75f, 0.9f, 40);
                }
            }

            // Ambient purple dust
            if (Main.rand.NextBool(4))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(20f, 20f);
                Color dustColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple, MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(0.8f, 0.8f), 100, dustColor, 1.1f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Pulsing mystical glow
            float lightPulse = 0.5f + MathF.Sin(time * 1.5f) * 0.15f;
            Lighting.AddLight(player.Center, MoonlightVFXLibrary.Violet.ToVector3() * lightPulse * 0.5f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            float pulse = 1f + MathF.Sin(Main.GameUpdateCount * 0.05f) * 0.1f;

            // 4-layer bloom using {A=0} premultiplied alpha trick (no SpriteBatch restart)

            // Layer 1: Outer deep purple halo
            spriteBatch.Draw(texture, position, null,
                (MoonlightVFXLibrary.DarkPurple with { A = 0 }) * 0.3f,
                rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);

            // Layer 2: Mid violet glow
            spriteBatch.Draw(texture, position, null,
                (MoonlightVFXLibrary.Violet with { A = 0 }) * 0.35f,
                rotation, origin, scale * pulse * 1.18f, SpriteEffects.None, 0f);

            // Layer 3: Inner ice blue
            spriteBatch.Draw(texture, position, null,
                (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.4f,
                rotation, origin, scale * pulse * 1.06f, SpriteEffects.None, 0f);

            // Layer 4: White-hot core
            spriteBatch.Draw(texture, position, null,
                (Color.White with { A = 0 }) * 0.25f,
                rotation, origin, scale * pulse, SpriteEffects.None, 0f);

            Lighting.AddLight(Item.Center, MoonlightVFXLibrary.Violet.ToVector3() * 0.4f);

            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
            Vector2 velocity, int type, int damage, float knockback)
        {
            // Slight spread for rapid-fire feel
            float spread = MathHelper.ToRadians(5f);
            velocity = velocity.RotatedByRandom(spread);

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);

            // Muzzle flash via VFX helper
            MoonlightsCallingVFX.MuzzleFlash(position, direction);

            // Directional spark burst
            for (int i = 0; i < 5; i++)
            {
                float sparkAngle = direction.ToRotation() + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 sparkVel = sparkAngle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color sparkColor = Color.Lerp(MoonlightVFXLibrary.IceBlue, Color.White, Main.rand.NextFloat(0.3f));
                var spark = new SparkleParticle(position, sparkVel, sparkColor, 0.3f, 18);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Dense dust burst
            for (int i = 0; i < 4; i++)
            {
                Vector2 dustVel = direction * 3f + Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustPerfect(position, DustID.PurpleTorch, dustVel, 80, default, 1.4f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires rapid moonlight beams that bounce off walls"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Each bounce intensifies the prismatic spectral effect"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Final bounce detonation unleashes a spectral cascade"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The moon whispers secrets to those who listen'")
            {
                OverrideColor = new Color(140, 100, 200)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 5)
                .AddIngredient(ModContent.ItemType<ShardsOfMoonlitTempo>(), 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
