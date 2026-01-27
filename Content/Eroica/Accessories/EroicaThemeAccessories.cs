using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.Common.Accessories;
using MagnumOpus.Content.Eroica.HarmonicCores;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using System;
using System.Collections.Generic;

namespace MagnumOpus.Content.Eroica.Accessories
{
    #region Theme Colors
    
    public static class EroicaColors
    {
        public static readonly Color Scarlet = new Color(139, 0, 0);
        public static readonly Color Crimson = new Color(220, 50, 50);
        public static readonly Color Gold = new Color(255, 215, 0);
        public static readonly Color Sakura = new Color(255, 150, 180);
        public static readonly Color DarkRed = new Color(180, 30, 60);
    }
    
    #endregion

    #region Badge of Valor

    /// <summary>
    /// Badge of Valor - Eroica Tier 1 Theme Accessory.
    /// A shield-shaped badge adorned with golden laurels and crimson gems.
    /// +15% melee damage, +10% melee speed, brief invulnerability after killing an enemy.
    /// </summary>
    public class BadgeOfValor : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 1);
            Item.rare = ModContent.RarityType<EroicaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<BadgeOfValorPlayer>();
            modPlayer.hasBadgeOfValor = true;

            // +15% melee damage
            player.GetDamage(DamageClass.Melee) += 0.15f;

            // +10% melee speed
            player.GetAttackSpeed(DamageClass.Melee) += 0.10f;

            // Invulnerability timer effect is handled in player class on kill

            // Heroic ambient particles
            if (!hideVisual)
            {
                // Golden laurel leaves orbiting
                if (Main.rand.NextBool(12))
                {
                    float angle = Main.GameUpdateCount * 0.03f;
                    Vector2 laurelPos = player.Center + angle.ToRotationVector2() * 22f;
                    
                    CustomParticles.GenericFlare(laurelPos, EroicaColors.Gold * 0.7f, 0.22f, 10);
                }

                // Sakura petals drifting
                if (Main.rand.NextBool(18))
                {
                    ThemedParticles.SakuraPetals(player.Center, 2, 30f);
                }

                // Scarlet embers
                if (Main.rand.NextBool(15))
                {
                    Vector2 pos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                    Vector2 vel = new Vector2(0, -Main.rand.NextFloat(0.5f, 1.5f));
                    CustomParticles.GenericGlow(pos, vel, EroicaColors.Crimson * 0.6f, 0.2f, 22, true);
                }
            }

            // Warm heroic light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 0.3f;
            Lighting.AddLight(player.Center, EroicaColors.Gold.ToVector3() * pulse);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MeleeDamage", "+15% melee damage")
            {
                OverrideColor = EroicaColors.Crimson
            });

            tooltips.Add(new TooltipLine(Mod, "MeleeSpeed", "+10% melee speed")
            {
                OverrideColor = EroicaColors.Gold
            });

            tooltips.Add(new TooltipLine(Mod, "HeroicMoment", "Killing an enemy grants 0.5 seconds of invulnerability")
            {
                OverrideColor = EroicaColors.Sakura
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Worn by those who charge into battle without hesitation'")
            {
                OverrideColor = new Color(200, 180, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 15)
                .AddIngredient(ModContent.ItemType<MelodicCharm>(), 1)
                .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class BadgeOfValorPlayer : ModPlayer
    {
        public bool hasBadgeOfValor;
        public int invulnFramesRemaining;

        public override void ResetEffects()
        {
            hasBadgeOfValor = false;
        }

        public override void PostUpdate()
        {
            if (invulnFramesRemaining > 0)
            {
                Player.immune = true;
                Player.immuneTime = 2;
                invulnFramesRemaining--;

                // Heroic shimmer effect during invuln
                if (Main.rand.NextBool(3))
                {
                    Vector2 pos = Player.Center + Main.rand.NextVector2Circular(20f, 30f);
                    CustomParticles.GenericFlare(pos, EroicaColors.Gold * 0.8f, 0.35f, 8);
                }
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            CheckForKill(target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
            {
                CheckForKill(target);
            }
        }

        private void CheckForKill(NPC target)
        {
            if (hasBadgeOfValor && target.life <= 0 && !target.friendly && target.lifeMax > 5)
            {
                // Grant 0.5 seconds (30 frames) of invulnerability
                invulnFramesRemaining = 30;

                // Heroic flash effect
                CustomParticles.GenericFlare(Player.Center, Color.White, 0.6f, 15);
                CustomParticles.HaloRing(Player.Center, EroicaColors.Gold, 0.4f, 15);
                ThemedParticles.SakuraPetals(Player.Center, 5, 40f);
            }
        }
    }

    #endregion

    #region Hero's Symphony

    /// <summary>
    /// Hero's Symphony - Eroica Tier 2 Theme Accessory (Ultimate).
    /// The triumphant power of Eroica crystallized into wearable form.
    /// All Eroica bonuses maximized, kills trigger "Heroic Surge" (+25% damage for 5s).
    /// </summary>
    public class HerosSymphony : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<EroicaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<HerosSymphonyPlayer>();
            modPlayer.hasHerosSymphony = true;

            // Enhanced bonuses
            // +20% melee damage (was 15%)
            player.GetDamage(DamageClass.Melee) += 0.20f;

            // +15% melee speed (was 10%)
            player.GetAttackSpeed(DamageClass.Melee) += 0.15f;

            // +10% melee crit
            player.GetCritChance(DamageClass.Melee) += 10;

            // +8% generic damage always
            player.GetDamage(DamageClass.Generic) += 0.08f;

            // Heroic Surge damage bonus (when active)
            if (modPlayer.heroicSurgeDuration > 0)
            {
                player.GetDamage(DamageClass.Generic) += 0.25f;
            }

            // Enhanced heroic ambient particles
            if (!hideVisual)
            {
                // Phoenix-like flames orbiting
                float baseAngle = Main.GameUpdateCount * 0.035f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 28f + (float)Math.Sin(Main.GameUpdateCount * 0.06f + i * 0.5f) * 6f;
                    Vector2 pos = player.Center + angle.ToRotationVector2() * radius;

                    if (Main.rand.NextBool(10))
                    {
                        Color flameColor = Color.Lerp(EroicaColors.Scarlet, EroicaColors.Gold, (float)i / 3f);
                        CustomParticles.GenericFlare(pos, flameColor * 0.7f, 0.25f, 12);
                    }
                }

                // Sakura petal storm
                if (Main.rand.NextBool(12))
                {
                    ThemedParticles.SakuraPetals(player.Center, 3, 35f);
                }

                // Rising embers
                if (Main.rand.NextBool(10))
                {
                    Vector2 pos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1f, 2f));
                    Color emberColor = Color.Lerp(EroicaColors.Crimson, EroicaColors.Gold, Main.rand.NextFloat());
                    CustomParticles.GenericGlow(pos, vel, emberColor * 0.75f, 0.24f, 25, true);
                }

                // Heroic surge visual
                if (modPlayer.heroicSurgeDuration > 0 && Main.rand.NextBool(4))
                {
                    CustomParticles.HaloRing(player.Center, EroicaColors.Gold * 0.5f, 0.3f, 10);
                    ThemedParticles.EroicaSparkles(player.Center, 4, 40f);
                }
            }

            // Enhanced heroic light
            float intensity = modPlayer.heroicSurgeDuration > 0 ? 0.6f : 0.4f;
            Vector3 lightColor = Color.Lerp(EroicaColors.Crimson, EroicaColors.Gold,
                (float)Math.Sin(Main.GameUpdateCount * 0.03f) * 0.5f + 0.5f).ToVector3();
            Lighting.AddLight(player.Center, lightColor * intensity);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MeleeDamage", "+20% melee damage")
            {
                OverrideColor = EroicaColors.Crimson
            });

            tooltips.Add(new TooltipLine(Mod, "MeleeSpeed", "+15% melee speed")
            {
                OverrideColor = EroicaColors.Gold
            });

            tooltips.Add(new TooltipLine(Mod, "MeleeCrit", "+10% melee critical strike chance")
            {
                OverrideColor = EroicaColors.Sakura
            });

            tooltips.Add(new TooltipLine(Mod, "GenericDamage", "+8% damage (all types)")
            {
                OverrideColor = new Color(255, 200, 150)
            });

            tooltips.Add(new TooltipLine(Mod, "HeroicMoment", "Killing an enemy grants 1 second of invulnerability")
            {
                OverrideColor = EroicaColors.Sakura
            });

            tooltips.Add(new TooltipLine(Mod, "HeroicSurge", "Kills trigger 'Heroic Surge': +25% damage for 5 seconds")
            {
                OverrideColor = EroicaColors.Gold
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The symphony of heroes echoes through eternity'")
            {
                OverrideColor = new Color(200, 180, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<BadgeOfValor>(), 1)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 25)
                .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class HerosSymphonyPlayer : ModPlayer
    {
        public bool hasHerosSymphony;
        public int invulnFramesRemaining;
        public int heroicSurgeDuration;

        public override void ResetEffects()
        {
            hasHerosSymphony = false;
        }

        public override void PostUpdate()
        {
            // Handle invulnerability
            if (invulnFramesRemaining > 0)
            {
                Player.immune = true;
                Player.immuneTime = 2;
                invulnFramesRemaining--;

                if (Main.rand.NextBool(3))
                {
                    Vector2 pos = Player.Center + Main.rand.NextVector2Circular(20f, 30f);
                    CustomParticles.GenericFlare(pos, EroicaColors.Gold * 0.8f, 0.35f, 8);
                }
            }

            // Heroic Surge timer
            if (heroicSurgeDuration > 0)
            {
                heroicSurgeDuration--;
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            CheckForKill(target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
            {
                CheckForKill(target);
            }
        }

        private void CheckForKill(NPC target)
        {
            if (hasHerosSymphony && target.life <= 0 && !target.friendly && target.lifeMax > 5)
            {
                // Grant 1 second (60 frames) of invulnerability
                invulnFramesRemaining = 60;

                // Trigger Heroic Surge (5 seconds = 300 frames)
                heroicSurgeDuration = 300;

                // Triumphant visual burst
                CustomParticles.GenericFlare(Player.Center, Color.White, 0.8f, 20);
                for (int i = 0; i < 3; i++)
                {
                    CustomParticles.HaloRing(Player.Center, Color.Lerp(EroicaColors.Scarlet, EroicaColors.Gold, (float)i / 3f), 
                        0.3f + i * 0.15f, 12 + i * 3);
                }
                ThemedParticles.SakuraPetals(Player.Center, 10, 50f);
            }
        }
    }

    #endregion
}
