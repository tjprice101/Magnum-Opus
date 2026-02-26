using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.Projectiles;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.MoonlightSonata.Enemies;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling
{
    /// <summary>
    /// Moonlight's Calling — "The Serenade".
    /// A magic tome that casts rapid bouncing moonlight beams with prismatic refraction.
    ///
    /// Left-click: Rapid-fire MoonlightBeam — bouncing beams that split into SpectralChildBeams.
    /// Right-click: Serenade Mode — charges and fires a PrismaticCrescendo mega-beam.
    ///
    /// Theme: Musical refraction — beams bounce and split into prismatic spectral colors.
    /// Bounces 3+ spawn spectral child beams. Final bounce detonates in a spectral cascade.
    /// Serenade Mode channels all spectral energy into a single devastating beam.
    /// </summary>
    public class MoonlightsCalling : ModItem
    {
        /// <summary>Cooldown frames remaining for Serenade Mode alt-fire.</summary>
        private int _serenadeCooldown;

        /// <summary>Serenade Mode cooldown in frames (3 seconds).</summary>
        private const int SerenadeCooldownMax = 180;

        /// <summary>Mana cost for Serenade Mode.</summary>
        private const int SerenadeManaCost = 40;

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

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Serenade Mode — slower, costlier, more powerful
                if (_serenadeCooldown > 0)
                    return false;

                Item.mana = SerenadeManaCost;
                Item.useTime = 50;
                Item.useAnimation = 50;
                Item.UseSound = SoundID.Item122 with { Pitch = 0.2f, Volume = 0.8f };
            }
            else
            {
                // Normal rapid-fire mode
                Item.mana = 8;
                Item.useTime = 12;
                Item.useAnimation = 12;
                Item.UseSound = SoundID.Item72;
            }

            return base.CanUseItem(player);
        }

        public override void UpdateInventory(Player player)
        {
            if (_serenadeCooldown > 0)
                _serenadeCooldown--;
        }

        public override void HoldItem(Player player)
        {
            if (Main.dedServ) return;

            // Tick cooldown while held
            if (_serenadeCooldown > 0)
                _serenadeCooldown--;

            float time = Main.GameUpdateCount * 0.04f;
            float pulse = MathF.Sin(time * 1.5f) * 0.12f + 0.88f;

            // === SERENADE MODE CHARGE VFX ===
            // If player is using the item with right-click, show charging VFX
            if (player.altFunctionUse == 2 && player.itemAnimation > 0 && player.itemAnimation < player.itemAnimationMax)
            {
                float chargeProgress = 1f - (float)player.itemAnimation / player.itemAnimationMax;
                Vector2 aimDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                MoonlightsCallingVFX.SerenadeChargeVFX(player.Center, aimDir, chargeProgress);
            }

            // Orbiting PrismaticShardDust — spectral refraction points circling player
            if (Main.rand.NextBool(6))
            {
                float orbitAngle = time + Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 25f + MathF.Sin(time * 2f + Main.rand.NextFloat()) * 8f;
                Vector2 shardPos = player.Center + orbitAngle.ToRotationVector2() * radius;
                Color shardColor = MoonlightsCallingVFX.GetRefractionColor(
                    (MathF.Sin(orbitAngle * 2f) + 1f) * 0.5f, 0);
                Dust shard = Dust.NewDustPerfect(shardPos,
                    ModContent.DustType<PrismaticShardDust>(),
                    Vector2.Zero, 0, shardColor, 0.18f * pulse);
                shard.customData = new PrismaticShardBehavior
                {
                    BaseHue = 0.7f + orbitAngle * 0.05f,
                    HueRange = 0.2f,
                    HueCycleSpeed = 0.8f,
                    RotationSpeed = 0.06f,
                    BaseScale = 0.18f * pulse,
                    Lifetime = 25
                };
            }

            // LunarMote crescent notes — 2 orbiting crescents like musical notation
            if (Main.rand.NextBool(10))
            {
                float moteAngle = time * 0.8f;
                for (int i = 0; i < 2; i++)
                {
                    float noteAngle = moteAngle + MathHelper.Pi * i;
                    float noteRadius = 28f + MathF.Sin(time * 1.2f + i * 0.7f) * 5f;
                    Vector2 notePos = player.Center + noteAngle.ToRotationVector2() * noteRadius;
                    Color moteColor = Color.Lerp(MoonlightsCallingVFX.PrismViolet,
                        MoonlightsCallingVFX.RefractedBlue, (float)i / 2f);
                    Dust mote = Dust.NewDustPerfect(notePos,
                        ModContent.DustType<LunarMote>(),
                        Vector2.Zero, 0, moteColor, 0.22f);
                    mote.customData = new LunarMoteBehavior(player.Center, noteAngle)
                    {
                        OrbitRadius = noteRadius,
                        OrbitSpeed = 0.04f,
                        Lifetime = 28,
                        FadePower = 0.92f
                    };
                }
            }

            // StarPointDust twinkles — sharp prismatic sparkles around player
            if (Main.rand.NextBool(12))
            {
                Vector2 offset = Main.rand.NextVector2Circular(22f, 22f);
                Color starColor = Color.Lerp(MoonlightsCallingVFX.TomeSilver,
                    MoonlightsCallingVFX.RefractionLavender, Main.rand.NextFloat());
                Dust star = Dust.NewDustPerfect(player.Center + offset,
                    ModContent.DustType<StarPointDust>(),
                    Vector2.Zero, 0, starColor, 0.17f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.08f,
                    TwinkleFrequency = 0.4f,
                    Lifetime = 26,
                    FadeStartTime = 8
                };
            }

            // Music notes — the defining "Serenade" visual
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

            // Pulsing prismatic glow
            float lightPulse = 0.5f + MathF.Sin(time * 1.5f) * 0.15f;
            Color lightColor = Color.Lerp(MoonlightVFXLibrary.Violet,
                MoonlightsCallingVFX.RefractedBlue,
                MathF.Sin(time * 0.7f) * 0.5f + 0.5f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * lightPulse * 0.45f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            float pulse = 1f + MathF.Sin(Main.GameUpdateCount * 0.05f) * 0.1f;

            // 5-layer prismatic bloom using {A=0} premultiplied alpha trick

            // Layer 1: Outer spectral halo (cycling color)
            Color outerColor = MoonlightsCallingVFX.GetRefractionColor(
                Main.GlobalTimeWrappedHourly % 1f, 0);
            spriteBatch.Draw(texture, position, null,
                (outerColor with { A = 0 }) * 0.2f,
                rotation, origin, scale * pulse * 1.4f, SpriteEffects.None, 0f);

            // Layer 2: DarkPurple halo
            spriteBatch.Draw(texture, position, null,
                (MoonlightVFXLibrary.DarkPurple with { A = 0 }) * 0.3f,
                rotation, origin, scale * pulse * 1.25f, SpriteEffects.None, 0f);

            // Layer 3: Prism violet glow
            spriteBatch.Draw(texture, position, null,
                (MoonlightsCallingVFX.PrismViolet with { A = 0 }) * 0.35f,
                rotation, origin, scale * pulse * 1.12f, SpriteEffects.None, 0f);

            // Layer 4: Refracted blue inner
            spriteBatch.Draw(texture, position, null,
                (MoonlightsCallingVFX.RefractedBlue with { A = 0 }) * 0.4f,
                rotation, origin, scale * pulse * 1.05f, SpriteEffects.None, 0f);

            // Layer 5: White-hot core
            spriteBatch.Draw(texture, position, null,
                (Color.White with { A = 0 }) * 0.25f,
                rotation, origin, scale * pulse, SpriteEffects.None, 0f);

            Lighting.AddLight(Item.Center, MoonlightsCallingVFX.PrismViolet.ToVector3() * 0.4f);

            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
            Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);

            // === SERENADE MODE (Right-click) ===
            if (player.altFunctionUse == 2)
            {
                // Fire PrismaticCrescendo mega-beam
                Projectile.NewProjectile(source, position, velocity * 1.2f,
                    ModContent.ProjectileType<PrismaticCrescendo>(),
                    (int)(damage * 1.8f), knockback * 1.5f, player.whoAmI);

                // Serenade release VFX — massive prismatic nova
                MoonlightsCallingVFX.SerenadeReleaseVFX(position, direction);

                // Start cooldown
                _serenadeCooldown = SerenadeCooldownMax;

                // Climactic sound
                SoundEngine.PlaySound(SoundID.Item105 with
                {
                    Volume = 0.6f,
                    Pitch = 0.3f
                }, position);

                return false;
            }

            // === NORMAL MODE (Left-click) ===

            // Slight spread for rapid-fire feel
            float spread = MathHelper.ToRadians(5f);
            velocity = velocity.RotatedByRandom(spread);

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            // Enhanced muzzle flash via VFX helper
            MoonlightsCallingVFX.MuzzleFlash(position, direction);

            // Directional prismatic spark burst
            for (int i = 0; i < 5; i++)
            {
                float sparkAngle = direction.ToRotation() + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 sparkVel = sparkAngle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color sparkColor = Color.Lerp(MoonlightVFXLibrary.IceBlue, Color.White, Main.rand.NextFloat(0.3f));
                var spark = new SparkleParticle(position, sparkVel, sparkColor, 0.3f, 18);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // PrismaticShardDust burst from muzzle
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustVel = direction * 3f + Main.rand.NextVector2Circular(2f, 2f);
                Color shardColor = MoonlightsCallingVFX.GetRefractionColor(Main.rand.NextFloat(), 0);
                Dust shard = Dust.NewDustPerfect(position,
                    ModContent.DustType<PrismaticShardDust>(),
                    dustVel, 0, shardColor, 0.2f);
                shard.customData = new PrismaticShardBehavior(0.7f + i * 0.05f, 0.15f, 18);
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "PrismaticBeams",
                "Fires rapid moonlight beams that bounce off walls with prismatic refraction")
            { OverrideColor = MoonlightsCallingVFX.PrismViolet });
            tooltips.Add(new TooltipLine(Mod, "SpectralSplit",
                "Bounces 3+ split into spectral child beams — light refracting through a crystal prism")
            { OverrideColor = MoonlightsCallingVFX.RefractedBlue });
            tooltips.Add(new TooltipLine(Mod, "Crescendo",
                "Final bounce detonation unleashes a prismatic spectral cascade")
            { OverrideColor = MoonlightsCallingVFX.TomeSilver });
            tooltips.Add(new TooltipLine(Mod, "SerenadeMode",
                "Right-click unleashes Serenade Mode — a devastating prismatic mega-beam that pierces everything")
            { OverrideColor = MoonlightsCallingVFX.SpectralCyan });
            tooltips.Add(new TooltipLine(Mod, "SerenadeAura",
                "Prismatic serenade aura while held")
            { OverrideColor = MoonlightsCallingVFX.RefractionLavender });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'The moon whispers secrets to those who listen — each note a color, each color a truth'")
            { OverrideColor = new Color(140, 100, 200) });
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
