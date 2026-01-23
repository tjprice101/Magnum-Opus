using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.Common.Accessories;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using System;
using System.Collections.Generic;

namespace MagnumOpus.Content.LaCampanella.Accessories
{
    #region Theme Colors
    
    public static class CampanellaColors
    {
        public static readonly Color Black = new Color(30, 20, 25);
        public static readonly Color Orange = new Color(255, 140, 40);
        public static readonly Color Gold = new Color(218, 165, 32);
        public static readonly Color Yellow = new Color(255, 200, 50);
        public static readonly Color DarkOrange = new Color(200, 80, 20);
    }
    
    #endregion

    #region Chime of Flames

    /// <summary>
    /// Chime of Flames - La Campanella Tier 1 Theme Accessory.
    /// A smoky black bell shape with blazing orange flames.
    /// +15% magic damage, spells leave fire trails, attacks have chance to ring (stun).
    /// </summary>
    public class ChimeOfFlames : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 1);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ChimeOfFlamesPlayer>();
            modPlayer.hasChimeOfFlames = true;

            // +15% magic damage
            player.GetDamage(DamageClass.Magic) += 0.15f;

            // Fire trail effect is handled in player class

            // Infernal ambient particles
            if (!hideVisual)
            {
                // Smoky black particles
                if (Main.rand.NextBool(10))
                {
                    Vector2 pos = player.Center + Main.rand.NextVector2Circular(20f, 20f);
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.5f, 1.5f));
                    
                    var smoke = new HeavySmokeParticle(pos, vel, CampanellaColors.Black, 
                        Main.rand.Next(20, 35), 0.25f, 0.4f, 0.015f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }

                // Orange flame licks
                if (Main.rand.NextBool(8))
                {
                    Vector2 pos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(1f, 2f));
                    Color flameColor = Color.Lerp(CampanellaColors.Orange, CampanellaColors.Yellow, Main.rand.NextFloat());
                    CustomParticles.GenericGlow(pos, vel, flameColor * 0.7f, 0.25f, 20, true);
                }

                // Bell shimmer
                if (Main.rand.NextBool(20))
                {
                    ThemedParticles.LaCampanellaSparkles(player.Center, 3, 30f);
                }
            }

            // Warm infernal light
            float flicker = Main.rand.NextFloat(0.8f, 1.0f);
            Lighting.AddLight(player.Center, CampanellaColors.Orange.ToVector3() * 0.35f * flicker);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MagicDamage", "+15% magic damage")
            {
                OverrideColor = CampanellaColors.Orange
            });

            tooltips.Add(new TooltipLine(Mod, "FireTrail", "Magic attacks leave lingering fire trails")
            {
                OverrideColor = CampanellaColors.Yellow
            });

            tooltips.Add(new TooltipLine(Mod, "BellChime", "8% chance for attacks to 'ring' - briefly stunning enemies")
            {
                OverrideColor = CampanellaColors.Gold
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Each chime carries the heat of a thousand flames'")
            {
                OverrideColor = new Color(180, 150, 130)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfLaCampanella>(), 15)
                .AddIngredient(ModContent.ItemType<MelodicCharm>(), 1)
                .AddIngredient(ModContent.ItemType<LaCampanellaResonantEnergy>(), 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class ChimeOfFlamesPlayer : ModPlayer
    {
        public bool hasChimeOfFlames;

        public override void ResetEffects()
        {
            hasChimeOfFlames = false;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TryRingBell(target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
            {
                TryRingBell(target);

                // Fire trail for magic projectiles
                if (hasChimeOfFlames && proj.DamageType == DamageClass.Magic && Main.rand.NextBool(3))
                {
                    Vector2 trailPos = proj.Center + Main.rand.NextVector2Circular(8f, 8f);
                    Dust fire = Dust.NewDustPerfect(trailPos, DustID.Torch, Vector2.Zero, 0, default, 1.5f);
                    fire.noGravity = true;
                }
            }
        }

        private void TryRingBell(NPC target)
        {
            if (hasChimeOfFlames && Main.rand.NextFloat() < 0.08f)
            {
                // Apply stun (Confused debuff as proxy)
                target.AddBuff(BuffID.Confused, 60); // 1 second stun

                // Bell ring visual
                CustomParticles.GenericFlare(target.Center, CampanellaColors.Gold, 0.5f, 15);
                CustomParticles.HaloRing(target.Center, CampanellaColors.Orange, 0.4f, 12);
                
                // Sound effect would go here
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f, Volume = 0.6f }, target.Center);
            }
        }
    }

    #endregion

    #region Infernal Virtuoso

    /// <summary>
    /// Infernal Virtuoso - La Campanella Tier 2 Theme Accessory (Ultimate).
    /// The infernal power of La Campanella crystallized into wearable form.
    /// All Campanella bonuses maximized, spells ring the bell (AoE fire damage on hit).
    /// </summary>
    public class InfernalVirtuoso : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<InfernalVirtuosoPlayer>();
            modPlayer.hasInfernalVirtuoso = true;

            // Enhanced bonuses
            // +22% magic damage (was 15%)
            player.GetDamage(DamageClass.Magic) += 0.22f;

            // +10% magic crit
            player.GetCritChance(DamageClass.Magic) += 10;

            // -12% mana cost
            player.manaCost -= 0.12f;

            // Fire immunity
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;

            // Enhanced infernal ambient particles
            if (!hideVisual)
            {
                // Heavy smoke billowing
                if (Main.rand.NextBool(6))
                {
                    Vector2 pos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -Main.rand.NextFloat(1f, 2f));
                    
                    var smoke = new HeavySmokeParticle(pos, vel, CampanellaColors.Black, 
                        Main.rand.Next(25, 45), 0.35f, 0.55f, 0.018f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }

                // Intense flame orbiting
                float baseAngle = Main.GameUpdateCount * 0.04f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 25f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + i) * 5f;
                    Vector2 pos = player.Center + angle.ToRotationVector2() * radius;

                    if (Main.rand.NextBool(8))
                    {
                        Color flameColor = Color.Lerp(CampanellaColors.DarkOrange, CampanellaColors.Yellow, (float)i / 3f);
                        CustomParticles.GenericFlare(pos, flameColor * 0.8f, 0.28f, 10);
                    }
                }

                // Rising infernal embers
                if (Main.rand.NextBool(6))
                {
                    Vector2 pos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1.5f, 3f));
                    Color emberColor = Color.Lerp(CampanellaColors.Orange, CampanellaColors.Gold, Main.rand.NextFloat());
                    CustomParticles.GenericGlow(pos, vel, emberColor * 0.8f, 0.28f, 28, true);
                }

                // Bell shimmer bursts
                if (Main.rand.NextBool(15))
                {
                    ThemedParticles.LaCampanellaHaloBurst(player.Center, 0.4f);
                }
            }

            // Intense infernal light
            float flicker = Main.rand.NextFloat(0.85f, 1.0f);
            Vector3 lightColor = Color.Lerp(CampanellaColors.Orange, CampanellaColors.Gold,
                (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.5f + 0.5f).ToVector3();
            Lighting.AddLight(player.Center, lightColor * 0.5f * flicker);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MagicDamage", "+22% magic damage")
            {
                OverrideColor = CampanellaColors.Orange
            });

            tooltips.Add(new TooltipLine(Mod, "MagicCrit", "+10% magic critical strike chance")
            {
                OverrideColor = CampanellaColors.Yellow
            });

            tooltips.Add(new TooltipLine(Mod, "ManaCost", "-12% mana cost")
            {
                OverrideColor = CampanellaColors.Gold
            });

            tooltips.Add(new TooltipLine(Mod, "FireImmune", "Immune to fire debuffs")
            {
                OverrideColor = new Color(255, 150, 100)
            });

            tooltips.Add(new TooltipLine(Mod, "BellChime", "15% chance for attacks to 'ring' - stunning enemies")
            {
                OverrideColor = CampanellaColors.Gold
            });

            tooltips.Add(new TooltipLine(Mod, "BellAoE", "Bell rings trigger fire explosion dealing 50% of hit damage in AoE")
            {
                OverrideColor = CampanellaColors.DarkOrange
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The virtuoso's fingers dance across keys of flame and shadow'")
            {
                OverrideColor = new Color(180, 150, 130)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ChimeOfFlames>(), 1)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfLaCampanella>(), 25)
                .AddIngredient(ModContent.ItemType<LaCampanellaResonantEnergy>(), 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class InfernalVirtuosoPlayer : ModPlayer
    {
        public bool hasInfernalVirtuoso;

        public override void ResetEffects()
        {
            hasInfernalVirtuoso = false;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TryRingBell(target, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
            {
                TryRingBell(target, damageDone);
            }
        }

        private void TryRingBell(NPC target, int damageDone)
        {
            if (hasInfernalVirtuoso && Main.rand.NextFloat() < 0.15f)
            {
                // Apply stun
                target.AddBuff(BuffID.Confused, 90); // 1.5 second stun

                // Bell ring visual
                CustomParticles.GenericFlare(target.Center, Color.White, 0.7f, 18);
                CustomParticles.GenericFlare(target.Center, CampanellaColors.Gold, 0.6f, 15);
                
                for (int i = 0; i < 3; i++)
                {
                    float delay = i * 0.1f;
                    Color ringColor = Color.Lerp(CampanellaColors.Orange, CampanellaColors.Gold, (float)i / 3f);
                    CustomParticles.HaloRing(target.Center, ringColor, 0.35f + i * 0.15f, 12 + i * 2);
                }

                // AoE fire explosion
                int aoeDamage = (int)(damageDone * 0.5f);
                float aoeRadius = 100f;

                foreach (NPC npc in Main.npc)
                {
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && 
                        Vector2.Distance(npc.Center, target.Center) < aoeRadius)
                    {
                        Player.ApplyDamageToNPC(npc, aoeDamage, 0f, 0, false);
                        npc.AddBuff(BuffID.OnFire, 180); // 3 seconds on fire

                        // Fire explosion on each hit enemy
                        CustomParticles.GenericFlare(npc.Center, CampanellaColors.Orange, 0.4f, 12);
                    }
                }

                // Sound effect
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.3f, Volume = 0.8f }, target.Center);
            }
        }
    }

    #endregion
}
