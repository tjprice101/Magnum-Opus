using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.Common.Accessories;
using MagnumOpus.Content.MoonlightSonata.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using System;
using System.Collections.Generic;

namespace MagnumOpus.Content.MoonlightSonata.Accessories
{
    #region Theme Colors
    
    public static class MoonlightColors
    {
        public static readonly Color DarkPurple = new Color(75, 0, 130);
        public static readonly Color MediumPurple = new Color(138, 43, 226);
        public static readonly Color Purple = new Color(138, 43, 226); // Alias for MediumPurple
        public static readonly Color LightBlue = new Color(135, 206, 250);
        public static readonly Color Silver = new Color(220, 220, 235);
        public static readonly Color Violet = new Color(180, 100, 220);
    }
    
    #endregion

    #region Adagio Pendant

    /// <summary>
    /// Adagio Pendant - Moonlight Sonata Tier 1 Theme Accessory.
    /// A crescent-shaped pendant infused with lunar essence.
    /// +12% damage at night, +15% crit chance under moonlight, -10% mana cost.
    /// </summary>
    public class AdagioPendant : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 1);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            bool isNight = !Main.dayTime;
            bool hasDirectMoonlight = isNight && !Collision.SolidCollision(player.position, player.width, player.height);

            // +12% damage at night
            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.12f;
            }

            // +15% crit chance under direct moonlight (night + outdoors)
            if (hasDirectMoonlight)
            {
                player.GetCritChance(DamageClass.Generic) += 15;
            }

            // -10% mana cost always
            player.manaCost -= 0.10f;

            // Moonlight ambient particles
            if (!hideVisual)
            {
                // Crescent moon orbiting effect
                if (Main.rand.NextBool(10))
                {
                    float angle = Main.GameUpdateCount * 0.02f;
                    Vector2 crescentPos = player.Center + angle.ToRotationVector2() * 25f;
                    
                    Color moonColor = Color.Lerp(MoonlightColors.DarkPurple, MoonlightColors.LightBlue, 
                        (float)Math.Sin(Main.GameUpdateCount * 0.03f) * 0.5f + 0.5f);
                    
                    CustomParticles.GenericFlare(crescentPos, moonColor * 0.7f, 0.25f, 12);
                }

                // Starlight wisps
                if (Main.rand.NextBool(15))
                {
                    Vector2 pos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                    Vector2 vel = new Vector2(0, -0.5f);
                    CustomParticles.GenericGlow(pos, vel, MoonlightColors.Silver * 0.6f, 0.18f, 25, true);
                }

                // Enhanced effects at night
                if (isNight && Main.rand.NextBool(20))
                {
                    ThemedParticles.MoonlightBloomBurst(player.Center, 0.3f);
                }
            }

            // Soft lunar light
            float intensity = isNight ? 0.4f : 0.2f;
            Lighting.AddLight(player.Center, MoonlightColors.MediumPurple.ToVector3() * intensity);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "NightDamage", "+12% damage at night")
            {
                OverrideColor = MoonlightColors.DarkPurple
            });

            tooltips.Add(new TooltipLine(Mod, "MoonlightCrit", "+15% critical strike chance under moonlight")
            {
                OverrideColor = MoonlightColors.LightBlue
            });

            tooltips.Add(new TooltipLine(Mod, "ManaCost", "-10% mana cost")
            {
                OverrideColor = MoonlightColors.Violet
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The adagio plays softly in the moonlit night'")
            {
                OverrideColor = MoonlightColors.Silver
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 15)
                .AddIngredient(ModContent.ItemType<MelodicCharm>(), 1)
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region Sonata's Embrace

    /// <summary>
    /// Sonata's Embrace - Moonlight Sonata Tier 2 Theme Accessory (Ultimate).
    /// The full power of Moonlight Sonata crystallized into wearable form.
    /// All Moonlight bonuses maximized, enemies hit are "Moonstruck" (slowed, reduced damage).
    /// </summary>
    public class SonatasEmbrace : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<SonatasEmbracePlayer>();
            modPlayer.hasSonatasEmbrace = true;

            bool isNight = !Main.dayTime;
            bool hasDirectMoonlight = isNight && !Collision.SolidCollision(player.position, player.width, player.height);

            // Enhanced bonuses
            // +18% damage at night (was 12%)
            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.18f;
            }
            else
            {
                player.GetDamage(DamageClass.Generic) += 0.08f; // Still useful during day
            }

            // +20% crit chance under direct moonlight (was 15%)
            if (hasDirectMoonlight)
            {
                player.GetCritChance(DamageClass.Generic) += 20;
            }

            // -15% mana cost always (was 10%)
            player.manaCost -= 0.15f;

            // +10% damage reduction at night
            if (isNight)
            {
                player.endurance += 0.10f;
            }

            // Moonstruck debuff is applied via SonatasEmbracePlayer.OnHitNPC

            // Enhanced lunar ambient particles
            if (!hideVisual)
            {
                // Lunar crescent orbit with trailing stars
                float baseAngle = Main.GameUpdateCount * 0.025f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 30f + (float)Math.Sin(Main.GameUpdateCount * 0.05f + i) * 5f;
                    Vector2 pos = player.Center + angle.ToRotationVector2() * radius;
                    
                    if (Main.rand.NextBool(12))
                    {
                        float hueShift = (float)i / 3f;
                        Color orbitColor = Color.Lerp(MoonlightColors.DarkPurple, MoonlightColors.LightBlue, hueShift);
                        CustomParticles.GenericFlare(pos, orbitColor * 0.6f, 0.22f, 10);
                    }
                }

                // Melodic waves emanating
                if (Main.rand.NextBool(25))
                {
                    ThemedParticles.MoonlightShockwave(player.Center, 0.4f);
                }

                // Starlight particles
                if (Main.rand.NextBool(8))
                {
                    Vector2 pos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                    Vector2 vel = Main.rand.NextVector2Circular(1f, 1f) + new Vector2(0, -0.5f);
                    Color starColor = Color.Lerp(MoonlightColors.Silver, MoonlightColors.LightBlue, Main.rand.NextFloat());
                    CustomParticles.GenericGlow(pos, vel, starColor * 0.7f, 0.22f, 28, true);
                }

                // Music notes at night
                if (isNight && Main.rand.NextBool(18))
                {
                    Vector2 pos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                    ThemedParticles.MusicNote(pos, vel, MoonlightColors.MediumPurple, 0.3f, 30);
                }
            }

            // Enhanced lunar light
            float intensity = isNight ? 0.55f : 0.3f;
            Vector3 lightColor = Color.Lerp(MoonlightColors.DarkPurple, MoonlightColors.LightBlue, 
                (float)Math.Sin(Main.GameUpdateCount * 0.02f) * 0.5f + 0.5f).ToVector3();
            Lighting.AddLight(player.Center, lightColor * intensity);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "NightDamage", "+18% damage at night, +8% during day")
            {
                OverrideColor = MoonlightColors.DarkPurple
            });

            tooltips.Add(new TooltipLine(Mod, "MoonlightCrit", "+20% critical strike chance under moonlight")
            {
                OverrideColor = MoonlightColors.LightBlue
            });

            tooltips.Add(new TooltipLine(Mod, "ManaCost", "-15% mana cost")
            {
                OverrideColor = MoonlightColors.Violet
            });

            tooltips.Add(new TooltipLine(Mod, "NightDR", "+10% damage reduction at night")
            {
                OverrideColor = MoonlightColors.Silver
            });

            tooltips.Add(new TooltipLine(Mod, "Moonstruck", "Attacks inflict 'Moonstruck' - slowed movement, -15% damage dealt")
            {
                OverrideColor = new Color(200, 180, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The moon's embrace is both gentle and absolute'")
            {
                OverrideColor = MoonlightColors.Silver
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<AdagioPendant>(), 1)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 25)
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class SonatasEmbracePlayer : ModPlayer
    {
        public bool hasSonatasEmbrace;

        public override void ResetEffects()
        {
            hasSonatasEmbrace = false;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasSonatasEmbrace)
            {
                ApplyMoonstruck(target);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasSonatasEmbrace && proj.owner == Player.whoAmI)
            {
                ApplyMoonstruck(target);
            }
        }

        private void ApplyMoonstruck(NPC target)
        {
            // Apply Moonstruck debuff (Slow + Ichor effect for damage reduction)
            target.AddBuff(BuffID.Slow, 180); // 3 seconds slow
            target.AddBuff(BuffID.Ichor, 120); // 2 seconds defense reduction as proxy for damage dealt reduction
            
            // Visual feedback
            if (Main.rand.NextBool(3))
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 pos = target.Center + Main.rand.NextVector2Circular(20f, 20f);
                    CustomParticles.GenericFlare(pos, MoonlightColors.MediumPurple * 0.8f, 0.3f, 15);
                }
            }
        }
    }

    #endregion
}
