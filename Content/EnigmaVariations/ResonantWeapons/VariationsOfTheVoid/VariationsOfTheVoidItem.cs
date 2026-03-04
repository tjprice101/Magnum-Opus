using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.EnigmaVariations.Debuffs;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid
{
    /// <summary>
    /// VARIATIONS OF THE VOID — Enigma Melee Sword (Item).
    /// Held-projectile swing via MeleeSwingItemBase → VariationsOfTheVoidSwing.
    /// 
    /// Mechanics:
    ///   • 3-Phase combo: Horizontal Sweep → Diagonal Slash → Heavy Slam
    ///   • Phase 1 spawns DimensionalSlash (33%), Phase 2 spawns 3 DimSlash + 3 seekers
    ///   • Every 3rd strike spawns VoidConvergenceBeamSet tri-beam convergence
    ///   • Beams converge over 120 frames → Void Resonance Explosion (3x, 100→300 AoE)
    ///   • ParadoxBrand on hit (8s), seeking crystals on crit
    /// </summary>
    public class VariationsOfTheVoidItem : MeleeSwingItemBase
    {
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/VariationsOfTheVoid";

        #region Theme Colors

        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);

        #endregion

        #region MeleeSwingItemBase Overrides

        protected override int SwingProjectileType
            => ModContent.ProjectileType<VariationsOfTheVoidSwing>();

        protected override int ComboStepCount => 3;

        protected override Color GetLoreColor() => EnigmaPurple;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        protected override void SetWeaponDefaults()
        {
            Item.damage = 380;
            Item.knockBack = 6f;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.value = Item.sellPrice(gold: 18);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
        }

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "3-phase combo: Horizontal Sweep, Diagonal Slash, Heavy Slam"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Spawns dimensional slashes that tear through enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every third strike summons three converging void beams"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Beams that converge create a Void Resonance Explosion"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Hits apply Paradox Brand, crits spawn seeking crystals"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'The void does not vary. You do.'")
            {
                OverrideColor = EnigmaPurple
            });
        }

        #endregion

        #region OnShoot — Spawn beams on finisher combo

        protected override void OnShoot(Player player, int projectileIndex)
        {
            // On finisher combo (step 2 → just launched step 2 swing, but combo advanced to 0 already)
            // MeleeSwingItemBase advances combo BEFORE calling OnShoot, so step just used = (current - 1 + count) % count
            int justUsedStep = (CurrentComboStep + ComboStepCount - 1) % ComboStepCount;
            if (justUsedStep == 2)
            {
                // Spawn the void beam set
                int beamType = ModContent.ProjectileType<VoidConvergenceBeamSet>();
                int beamCount = player.ownedProjectileCounts[beamType];
                if (beamCount == 0)
                {
                    Vector2 toCursor = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.NewProjectile(
                        player.GetSource_ItemUse(Item), player.Center, toCursor,
                        beamType, Item.damage, Item.knockBack, player.whoAmI);

                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.1f, Volume = 0.7f }, player.Center);
                }
            }
        }

        #endregion
    }
}
