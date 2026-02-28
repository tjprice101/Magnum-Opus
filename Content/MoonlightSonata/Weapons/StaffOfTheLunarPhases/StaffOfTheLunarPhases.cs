using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using MagnumOpus.Common;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.MoonlightSonata.Enemies;
using MagnumOpus.Content.MoonlightSonata.Minions;
using MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Utilities;
using MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Particles;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases
{
    /// <summary>
    /// Staff of the Lunar Phases — "The Conductor's Baton".
    /// Summons a Goliath of Moonlight that fires ricocheting moonlight beams.
    /// Right-click toggles Conductor Mode — direct the Goliath's beams toward your cursor.
    /// Beams heal the player 10 HP per hit and inflict Musical Dissonance.
    /// Summoning triggers a summon circle shader VFX ritual.
    /// </summary>
    public class StaffOfTheLunarPhases : ModItem
    {
        // =================================================================
        // VFX STATE
        // =================================================================

        /// <summary>Client-side summon circle animation timer.</summary>
        private float _ritualGlow = 0f;

        // =================================================================
        // SETUP
        // =================================================================

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
        }

        // =================================================================
        // RIGHT-CLICK: CONDUCTOR MODE TOGGLE
        // =================================================================

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: toggle Conductor Mode (no mana cost, no use animation)
                Item.mana = 0;
                Item.useTime = 20;
                Item.useAnimation = 20;
                Item.UseSound = SoundID.Item4;
            }
            else
            {
                // Left-click: summon
                Item.mana = 15;
                Item.useTime = 36;
                Item.useAnimation = 36;
                Item.UseSound = SoundID.Item44;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
            Vector2 velocity, int type, int damage, float knockback)
        {
            // Right-click: toggle Conductor Mode
            if (player.altFunctionUse == 2)
            {
                GoliathPlayer gp = player.Goliath();
                gp.ToggleConductorMode();

                // Visual/audio feedback
                SoundEngine.PlaySound(SoundID.Item4 with
                {
                    Volume = 0.5f,
                    Pitch = gp.ConductorMode ? 0.3f : -0.1f
                }, player.Center);

                // Conductor mode toggle VFX
                if (!Main.dedServ)
                {
                    Color glowColor = gp.ConductorMode ? GoliathUtils.ConductorHighlight : GoliathUtils.NebulaPurple;
                    GoliathParticleHandler.Spawn(new SummonGlowParticle(
                        player.Center, glowColor * 0.6f, 0.8f, 15));

                    if (gp.ConductorMode)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            GoliathParticleHandler.Spawn(new ConductorGlyphParticle(
                                player.Center + Main.rand.NextVector2Circular(20f, 20f),
                                Main.rand.NextVector2Circular(2f, 2f),
                                0.4f + Main.rand.NextFloat(0.3f), 30 + Main.rand.Next(15)));
                        }
                    }
                }

                return false; // Don't summon
            }

            // Left-click: summon Goliath
            player.AddBuff(ModContent.BuffType<GoliathOfMoonlightBuff>(), 18000);
            position = Main.MouseWorld;
            Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);

            // Trigger summon circle ritual VFX
            GoliathPlayer goliathPlayer = player.Goliath();
            goliathPlayer.TriggerRitual();
            _ritualGlow = 1f;

            // Summon ritual VFX burst
            if (!Main.dedServ)
            {
                // Summoning glow at spawn point
                GoliathParticleHandler.Spawn(new SummonGlowParticle(
                    position, GoliathUtils.SupermoonWhite, 1.2f, 25));
                GoliathParticleHandler.Spawn(new SummonGlowParticle(
                    position, GoliathUtils.NebulaPurple * 0.5f, 1.8f, 30));

                // Radial music note burst
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 noteVel = angle.ToRotationVector2() * (2f + Main.rand.NextFloat(2f));
                    noteVel.Y -= 1.5f; // float upward bias
                    Color noteColor = GoliathUtils.GetCastGradient(Main.rand.NextFloat(0.3f, 1f));
                    GoliathParticleHandler.Spawn(new MusicNoteParticle(
                        position + Main.rand.NextVector2Circular(20f, 20f), noteVel,
                        noteColor, 0.5f + Main.rand.NextFloat(0.4f), 50 + Main.rand.Next(25)));
                }

                // Gravity well particles converging on spawn point
                for (int i = 0; i < 10; i++)
                {
                    GoliathParticleHandler.Spawn(new GravityWellParticle(
                        position + Main.rand.NextVector2Circular(80f, 80f),
                        position, 0.3f + Main.rand.NextFloat(0.2f),
                        25 + Main.rand.Next(15)));
                }

                // Cosmic dust burst
                for (int i = 0; i < 12; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                    int d = Dust.NewDust(position - new Vector2(4), 8, 8,
                        ModContent.DustType<Dusts.GoliathDust>(), dustVel.X, dustVel.Y);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].scale = 1.3f;
                }
            }

            return false;
        }

        // =================================================================
        // TOOLTIPS
        // =================================================================

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Parts the veil of moonlight, summoning a Goliath of Moonlight"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "The Goliath fires devastating moonlight beams that ricochet between enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Each beam hit restores 10 health"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "Right-click to toggle Conductor Mode — direct the Goliath's beams toward your cursor"));
            tooltips.Add(new TooltipLine(Mod, "Effect5",
                "Inflicts Musical Dissonance on enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'The conductor raises the baton — and the moonlight obeys'")
            { OverrideColor = new Color(140, 100, 200) });
        }

        // =================================================================
        // RECIPE
        // =================================================================

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 20)
                .AddIngredient(ModContent.ItemType<ShardsOfMoonlitTempo>(), 20)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
