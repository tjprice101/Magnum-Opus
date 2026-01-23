using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.Materials.Foundation;
using System;
using System.Collections.Generic;

namespace MagnumOpus.Content.Common.Accessories
{
    #region Composer's Notebook

    /// <summary>
    /// Composer's Notebook - Pre-Hardmode base accessory.
    /// A leather-bound journal containing the foundations of musical power.
    /// +5% all damage, shows enemy health bars.
    /// </summary>
    public class ComposersNotebook : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 26;
            Item.accessory = true;
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // +5% all damage
            player.GetDamage(DamageClass.Generic) += 0.05f;

            // Show enemy health bars (Hunter Potion effect)
            player.detectCreature = true;

            // Ambient particles - soft purple and gold musical energy
            if (!hideVisual && Main.rand.NextBool(12))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.5f, 1f));
                Color noteColor = Color.Lerp(new Color(138, 43, 226), new Color(255, 215, 0), Main.rand.NextFloat());
                
                CustomParticles.GenericGlow(pos, vel, noteColor * 0.7f, 0.2f, 20, true);
                
                // Occasional music note
                if (Main.rand.NextBool(3))
                {
                    ThemedParticles.MusicNote(pos, vel * 0.5f, noteColor, 0.25f, 25);
                }
            }

            // Soft ambient light
            Lighting.AddLight(player.Center, new Vector3(0.2f, 0.15f, 0.25f));
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DamageBoost", "+5% damage")
            {
                OverrideColor = new Color(180, 150, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Detection", "Highlights nearby enemies")
            {
                OverrideColor = new Color(255, 200, 100)
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The first steps of a grand composition'")
            {
                OverrideColor = new Color(150, 150, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCrystalShard>(), 5)
                .AddIngredient(ModContent.ItemType<FadedSheetMusic>(), 1)
                .AddIngredient(ItemID.Book, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    #endregion

    #region Resonant Pendant

    /// <summary>
    /// Resonant Pendant - Pre-Hardmode base accessory.
    /// A circular metal disc that resonates with harmonic frequencies.
    /// +3% damage, enemies have small chance to drop Minor Music Notes.
    /// </summary>
    public class ResonantPendant : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 24;
            Item.accessory = true;
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // +3% all damage
            player.GetDamage(DamageClass.Generic) += 0.03f;

            // Music note drop chance handled by global NPC
            player.GetModPlayer<ResonantPendantPlayer>().hasResonantPendant = true;

            // Ambient particles - harmonic sound waves
            if (!hideVisual && Main.rand.NextBool(15))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 20f + Main.rand.NextFloat(10f);
                Vector2 pos = player.Center + angle.ToRotationVector2() * radius;
                
                // Concentric ring effect
                Color waveColor = Color.Lerp(new Color(138, 43, 226), new Color(192, 192, 192), Main.rand.NextFloat()) * 0.6f;
                CustomParticles.HaloRing(pos, waveColor, 0.15f, 12);
            }

            // Soft purple light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 0.2f;
            Lighting.AddLight(player.Center, new Vector3(0.15f, 0.1f, 0.2f) * pulse);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DamageBoost", "+3% damage")
            {
                OverrideColor = new Color(180, 150, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "NoteDrop", "Enemies may drop Minor Music Notes")
            {
                OverrideColor = new Color(255, 215, 0)
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Attuned to the frequencies of creation'")
            {
                OverrideColor = new Color(150, 150, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<TuningFork>(), 1)
                .AddIngredient(ModContent.ItemType<DullResonator>(), 1)
                .AddIngredient(ModContent.ItemType<ResonantCrystalShard>(), 3)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class ResonantPendantPlayer : ModPlayer
    {
        public bool hasResonantPendant;

        public override void ResetEffects()
        {
            hasResonantPendant = false;
        }
    }

    public class ResonantPendantGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            if (Main.player[Main.myPlayer].GetModPlayer<ResonantPendantPlayer>().hasResonantPendant)
            {
                // 5% chance to drop Minor Music Note
                if (Main.rand.NextFloat() < 0.05f && npc.lifeMax > 5 && !npc.friendly)
                {
                    Item.NewItem(npc.GetSource_Loot(), npc.getRect(), ModContent.ItemType<MinorMusicNote>(), 1);
                }
            }
        }
    }

    #endregion

    #region Melodic Charm

    /// <summary>
    /// Melodic Charm - Pre-Hardmode combined accessory.
    /// Combines the Composer's Notebook and Resonant Pendant.
    /// +8% damage, mana regeneration improved, all previous effects.
    /// </summary>
    public class MelodicCharm : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.accessory = true;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Green;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // +8% all damage (combined and enhanced)
            player.GetDamage(DamageClass.Generic) += 0.08f;

            // Show enemy health bars
            player.detectCreature = true;

            // Improved mana regeneration
            player.manaRegenBonus += 15;

            // Music note drop chance
            player.GetModPlayer<ResonantPendantPlayer>().hasResonantPendant = true;

            // Enhanced ambient particles
            if (!hideVisual)
            {
                // Orbiting musical energy
                if (Main.rand.NextBool(8))
                {
                    float baseAngle = Main.GameUpdateCount * 0.03f;
                    for (int i = 0; i < 2; i++)
                    {
                        float angle = baseAngle + MathHelper.Pi * i;
                        float radius = 28f + (float)Math.Sin(Main.GameUpdateCount * 0.05f + i) * 5f;
                        Vector2 pos = player.Center + angle.ToRotationVector2() * radius;
                        
                        Color orbColor = i == 0 
                            ? Color.Lerp(new Color(138, 43, 226), new Color(255, 215, 0), 0.3f)
                            : Color.Lerp(new Color(255, 215, 0), new Color(138, 43, 226), 0.3f);
                        
                        CustomParticles.GenericFlare(pos, orbColor * 0.6f, 0.2f, 8);
                    }
                }

                // Occasional music notes rising
                if (Main.rand.NextBool(20))
                {
                    Vector2 pos = player.Center + Main.rand.NextVector2Circular(20f, 20f);
                    Vector2 vel = new Vector2(0, -1f);
                    Color noteColor = Main.hslToRgb(Main.rand.NextFloat(), 0.7f, 0.7f);
                    ThemedParticles.MusicNote(pos, vel, noteColor, 0.3f, 30);
                }
            }

            // Warm ambient light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.15f + 0.35f;
            Lighting.AddLight(player.Center, new Vector3(0.25f, 0.2f, 0.3f) * pulse);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DamageBoost", "+8% damage")
            {
                OverrideColor = new Color(200, 170, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "ManaRegen", "Improved mana regeneration")
            {
                OverrideColor = new Color(100, 150, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Detection", "Highlights nearby enemies")
            {
                OverrideColor = new Color(255, 200, 100)
            });

            tooltips.Add(new TooltipLine(Mod, "NoteDrop", "Enemies may drop Minor Music Notes")
            {
                OverrideColor = new Color(255, 215, 0)
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The melody begins to take shape'")
            {
                OverrideColor = new Color(150, 150, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ComposersNotebook>(), 1)
                .AddIngredient(ModContent.ItemType<ResonantPendant>(), 1)
                .AddIngredient(ModContent.ItemType<MinorMusicNote>(), 5)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    #endregion
}
