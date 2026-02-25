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
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.EnigmaVariations.Debuffs;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons
{
    /// <summary>
    /// VARIATIONS OF THE VOID — Enigma Melee Sword (Item).
    /// Held-projectile swing via MeleeSwingItemBase → VariationsOfTheVoidSwing.
    /// 
    /// Mechanics preserved from legacy:
    ///   • On finisher combo, spawns VoidConvergenceBeamSet tri-beam
    ///   • ParadoxBrand on hit, seeking crystals on crit
    /// </summary>
    public class VariationsOfTheVoidItem : MeleeSwingItemBase
    {
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid";

        #region Theme Colors

        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);

        #endregion

        #region MeleeSwingItemBase Overrides

        protected override int SwingProjectileType
            => ModContent.ProjectileType<VariationsOfTheVoidSwing>();

        protected override int ComboStepCount => 3;

        protected override Color GetLoreColor() => EnigmaPurple;

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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Every third strike summons three converging void beams"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Beams slowly converge toward the cursor and resonate when aligned"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hits apply Paradox Brand with stacking void damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Three questions. One answer. The void.'")
            {
                OverrideColor = EnigmaPurple
            });
        }

        #endregion

        #region OnShoot — Spawn beams on finisher combo

        protected override void OnShoot(Player player, int projectileIndex)
        {
            // Swing VFX
            CustomParticles.GenericFlare(player.Center, EnigmaGreen, 0.5f, 12);
            CustomParticles.HaloRing(player.Center, EnigmaPurple, 0.35f, 10);
            ThemedParticles.EnigmaMusicNotes(player.Center, 2, 25f);

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
