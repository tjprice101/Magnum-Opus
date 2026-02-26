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
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.Enemies;
using MagnumOpus.Content.MoonlightSonata.Minions;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.MoonlightSonata.Accessories;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases
{
    /// <summary>
    /// Staff of the Lunar Phases — "The Conductor's Baton".
    /// Summons a Goliath of Moonlight — a massive lunar guardian.
    ///
    /// Conductor Mode (right-click to toggle):
    ///   OFF — Goliath auto-targets the nearest enemy
    ///   ON  — Goliath aims beams toward the player's cursor position,
    ///         granting precise control over devastating beam placement.
    ///         Staff pulses with GravitationalRift shader aura.
    ///
    /// Theme: Conductor's baton aesthetic, summon circle with lunar phases,
    /// GodRaySystem burst on summon completion, shader-driven gravitational aura.
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

        public override bool AltFunctionUse(Player player) => true;

        public override void HoldItem(Player player)
        {
            if (Main.dedServ) return;

            var modPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();
            float time = Main.GameUpdateCount * 0.04f;

            // Increment conductor pulse timer when in conductor mode
            if (modPlayer.staffConductorMode)
                modPlayer.conductorPulseTimer++;

            // Delegate to VFX system with conductor mode state
            StaffOfTheLunarPhasesVFX.HoldItemVFX(player, modPlayer.staffConductorMode);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;

            StaffOfTheLunarPhasesVFX.DrawWorldItemBloom(spriteBatch, texture, position, origin, rotation, scale);

            return true;
        }

        public override bool CanUseItem(Player player)
        {
            // Alt-fire (right-click) toggles Conductor Mode — always allowed, no mana cost
            if (player.altFunctionUse == 2)
            {
                Item.useTime = 20;
                Item.useAnimation = 20;
                Item.mana = 0;
                Item.UseSound = null;
                Item.buffType = 0;
                return true;
            }

            // Primary fire — normal summon
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.mana = 15;
            Item.UseSound = SoundID.Item44;
            Item.buffType = ModContent.BuffType<GoliathOfMoonlightBuff>();
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
            Vector2 velocity, int type, int damage, float knockback)
        {
            var modPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();

            // === ALT-FIRE: CONDUCTOR MODE TOGGLE ===
            if (player.altFunctionUse == 2)
            {
                modPlayer.staffConductorMode = !modPlayer.staffConductorMode;
                modPlayer.conductorPulseTimer = 0;

                // Toggle VFX + sound
                Vector2 batonTip = player.Center + new Vector2(player.direction * 16f, -18f);
                StaffOfTheLunarPhasesVFX.ConductorModeToggleVFX(batonTip, modPlayer.staffConductorMode);

                float pitch = modPlayer.staffConductorMode ? 0.5f : 0.1f;
                SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.8f, Pitch = pitch }, batonTip);
                SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.5f, Pitch = 0.3f }, batonTip);

                return false;
            }

            // === PRIMARY FIRE: SUMMON GOLIATH ===
            // Apply the buff
            player.AddBuff(ModContent.BuffType<GoliathOfMoonlightBuff>(), 18000);

            // Spawn position at mouse
            position = Main.MouseWorld;

            // Grand summoning ritual VFX (now with shader integration)
            StaffOfTheLunarPhasesVFX.SummoningRitualVFX(position);

            // Summoning sounds
            SoundEngine.PlaySound(SoundID.Item119 with { Volume = 1f, Pitch = -0.2f }, position);
            SoundEngine.PlaySound(SoundID.Item82 with { Volume = 0.6f }, position);

            // Spawn the Goliath
            Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);

            return false;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity,
            ref int type, ref int damage, ref float knockback)
        {
            var modPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();

            // Fractal of Moonlight synergy — enhanced Goliath
            if (modPlayer.hasFractalOfMoonlight)
            {
                damage = (int)(damage * 1.15f);
            }
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
            Player player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();

            tooltips.Add(new TooltipLine(Mod, "MinionInfo",
                "Parts the veil of moonlight, summoning a Goliath of Moonlight")
            { OverrideColor = GoliathVFX.NebulaPurple });

            tooltips.Add(new TooltipLine(Mod, "BeamInfo",
                "The Goliath fires devastating beams that ricochet between enemies")
            { OverrideColor = GoliathVFX.EnergyTendril });

            tooltips.Add(new TooltipLine(Mod, "HealInfo",
                "Each beam hit restores 10 health — the moon's quiet benevolence")
            { OverrideColor = GoliathVFX.StarCore });

            // Conductor Mode description
            tooltips.Add(new TooltipLine(Mod, "ConductorMechanic",
                "Right-click to toggle Conductor Mode:")
            { OverrideColor = new Color(200, 200, 200) });

            string conductorStatus = modPlayer.staffConductorMode ? " [Active]" : "";
            tooltips.Add(new TooltipLine(Mod, "ConductorDesc",
                $"  Conductor — Direct the Goliath's beams toward your cursor{conductorStatus}")
            { OverrideColor = modPlayer.staffConductorMode
                ? StaffOfTheLunarPhasesVFX.BatonGlowColor : new Color(160, 140, 200) });

            tooltips.Add(new TooltipLine(Mod, "GravityInfo",
                "Its luminous presence bends the night around it")
            { OverrideColor = GoliathVFX.GravityWell });

            // Fractal of Moonlight synergy
            if (modPlayer.hasFractalOfMoonlight)
            {
                tooltips.Add(new TooltipLine(Mod, "FractalSynergy",
                    "Fractal of Moonlight: +15% damage, -25% Goliath cooldown")
                { OverrideColor = new Color(100, 255, 150) });
            }

            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'The conductor raises the baton — and the moonlight obeys'")
            { OverrideColor = new Color(140, 100, 200) });
        }
    }
}
