using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Optimization;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.Enemies;
using MagnumOpus.Content.MoonlightSonata.Minions;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases
{
    /// <summary>
    /// Staff of the Lunar Phases — "The Conductor's Baton".
    /// Summons a Goliath of Moonlight — a massive lunar guardian.
    /// Theme: Conductor's baton aesthetic, summon circle with lunar phases,
    /// GodRaySystem burst on summon completion.
    /// </summary>
    public class StaffOfTheLunarPhases : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 280;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 15;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.buyPrice(gold: 30);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<GoliathOfMoonlight>();
            Item.buffType = ModContent.BuffType<GoliathOfMoonlightBuff>();
            Item.maxStack = 1;
        }

        public override void HoldItem(Player player)
        {
            if (Main.dedServ) return;

            float time = Main.GameUpdateCount * 0.04f;

            // Orbiting lunar motes — 3 crescent points cycling
            if (Main.rand.NextBool(6))
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = time + MathHelper.TwoPi * i / 3f;
                    float radius = 30f + MathF.Sin(Main.GameUpdateCount * 0.05f + i * 0.7f) * 6f;
                    Vector2 orbitPos = player.Center + angle.ToRotationVector2() * radius;
                    float progress = (float)i / 3f;
                    Color orbitColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple, MoonlightVFXLibrary.IceBlue, progress);
                    CustomParticles.GenericFlare(orbitPos, orbitColor * 0.6f, 0.25f, 12);
                }
            }

            // Prismatic sparkle aura
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(28f, 28f);
                Color gradientColor = Color.Lerp(MoonlightVFXLibrary.Violet, MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                CustomParticles.PrismaticSparkle(player.Center + offset, gradientColor * 0.6f, 0.22f);

                var sparkle = new SparkleParticle(player.Center + offset, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    MoonlightVFXLibrary.MoonWhite * 0.4f, 0.18f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Orbiting music notes — conductor's baton signature
            if (Main.rand.NextBool(8))
            {
                float noteOrbit = Main.GameUpdateCount * 0.06f;

                for (int i = 0; i < 2; i++)
                {
                    float noteAngle = noteOrbit + MathHelper.Pi * i;
                    Vector2 notePos = player.Center + noteAngle.ToRotationVector2() * 22f;
                    MoonlightVFXLibrary.SpawnMusicNotes(notePos, 1, 2f, 0.75f, 0.9f, 35);

                    // Sparkle companion for visibility
                    CustomParticles.PrismaticSparkle(notePos + Main.rand.NextVector2Circular(4f, 4f),
                        MoonlightVFXLibrary.MoonWhite * 0.4f, 0.15f);
                }
            }

            // Dense lunar dust
            if (Main.rand.NextBool(3))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(0.8f, 0.8f), 80, default, 1.1f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }

            // Pulsing mystical glow
            float pulse = MathF.Sin(Main.GameUpdateCount * 0.05f) * 0.15f + 0.95f;
            Lighting.AddLight(player.Center, MoonlightVFXLibrary.Violet.ToVector3() * pulse * 0.6f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            float pulse = 1f + MathF.Sin(Main.GameUpdateCount * 0.04f) * 0.12f;

            // 4-layer bloom using {A=0} — no SpriteBatch restart needed

            // Layer 1: Outer deep indigo aura
            spriteBatch.Draw(texture, position, null,
                (MoonlightVFXLibrary.NightPurple with { A = 0 }) * 0.35f,
                rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);

            // Layer 2: Mid violet glow
            spriteBatch.Draw(texture, position, null,
                (MoonlightVFXLibrary.Violet with { A = 0 }) * 0.30f,
                rotation, origin, scale * pulse * 1.18f, SpriteEffects.None, 0f);

            // Layer 3: Inner ice blue
            spriteBatch.Draw(texture, position, null,
                (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.25f,
                rotation, origin, scale * pulse * 1.06f, SpriteEffects.None, 0f);

            // Layer 4: White core
            spriteBatch.Draw(texture, position, null,
                (Color.White with { A = 0 }) * 0.18f,
                rotation, origin, scale * pulse, SpriteEffects.None, 0f);

            Lighting.AddLight(Item.Center, MoonlightVFXLibrary.Violet.ToVector3() * 0.45f);

            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
            Vector2 velocity, int type, int damage, float knockback)
        {
            // Apply the buff
            player.AddBuff(Item.buffType, 18000);

            // Spawn position at mouse
            position = Main.MouseWorld;

            // === GRAND SUMMONING RITUAL ===

            // Central flash cascade
            CustomParticles.GenericFlare(position, Color.White, 1.0f, 22);
            CustomParticles.GenericFlare(position, MoonlightVFXLibrary.MoonWhite, 0.8f, 20);
            CustomParticles.GenericFlare(position, MoonlightVFXLibrary.Violet, 0.6f, 18);

            // Magic circle — 6 glyph flares in expanding ring
            float magicCircleAngle = Main.GameUpdateCount * 0.05f;
            for (int i = 0; i < 6; i++)
            {
                float glyphAngle = magicCircleAngle + MathHelper.TwoPi * i / 6f;
                Vector2 glyphPos = position + glyphAngle.ToRotationVector2() * 50f;
                Color glyphColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple, MoonlightVFXLibrary.IceBlue, (float)i / 6f);
                CustomParticles.GenericFlare(glyphPos, glyphColor, 0.4f, 18);
            }

            // 8 lunar phase symbols — crescent flares at different orientations
            for (int i = 0; i < 8; i++)
            {
                float phaseAngle = MathHelper.TwoPi * i / 8f;
                Vector2 phasePos = position + phaseAngle.ToRotationVector2() * 35f;
                Color phaseColor = Color.Lerp(MoonlightVFXLibrary.Lavender, MoonlightVFXLibrary.MoonWhite, (float)i / 8f);
                CustomParticles.GenericFlare(phasePos, phaseColor, 0.3f, 16);
            }

            // Halo ring cascade
            for (int ring = 0; ring < 3; ring++)
            {
                Color ringColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple, MoonlightVFXLibrary.MoonWhite, ring / 3f);
                CustomParticles.HaloRing(position, ringColor, 0.5f + ring * 0.2f, 20 + ring * 5);
            }

            // Radial spark burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color sparkColor = Color.Lerp(MoonlightVFXLibrary.Violet, MoonlightVFXLibrary.IceBlue, (float)i / 12f);
                var spark = new SparkleParticle(position, sparkVel, sparkColor, 0.3f, 20);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // GodRaySystem burst — summoning completion
            GodRaySystem.CreateBurst(position, MoonlightVFXLibrary.Violet, 6, 60f, 22,
                GodRaySystem.GodRayStyle.Explosion, MoonlightVFXLibrary.IceBlue);

            // Screen distortion on summon
            if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
            {
                ScreenDistortionManager.TriggerRipple(position, MoonlightVFXLibrary.Violet, 0.4f, 20);
            }

            // Music notes — the summoning song
            MoonlightVFXLibrary.SpawnMusicNotes(position, 8, 50f, 0.8f, 1.1f, 35);

            // Summoning sounds
            SoundEngine.PlaySound(SoundID.Item119 with { Volume = 1f, Pitch = -0.2f }, position);
            SoundEngine.PlaySound(SoundID.Item82 with { Volume = 0.6f }, position);

            // Spawn the Goliath
            Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 20)
                .AddIngredient(ModContent.ItemType<ShardsOfMoonlitTempo>(), 20)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MinionInfo", "Summons a Goliath of Moonlight")
            {
                OverrideColor = new Color(180, 150, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "BeamInfo", "Fires explosive moonlight beams that heal you")
            {
                OverrideColor = new Color(150, 200, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "HealInfo", "Each beam hit restores 10 health")
            {
                OverrideColor = new Color(100, 255, 150)
            });
        }
    }
}
