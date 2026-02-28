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

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence
{
    /// <summary>
    /// THE UNRESOLVED CADENCE — Ultimate Enigma melee broadsword (Item).
    /// Held-projectile swing via MeleeSwingItemBase → TheUnresolvedCadenceSwing.
    /// 
    /// Mechanics preserved from legacy:
    ///   • Every swing stacks "Inevitability" on ALL enemies on screen
    ///   • At 10 stacks → triggers Paradox Collapse (ParadoxCollapseUltimate)
    ///   • Each swing spawns DimensionalSlash sub-projectiles
    ///   • On-hit: ParadoxBrand + SeekingCrystals
    /// </summary>
    public class TheUnresolvedCadenceItem : MeleeSwingItemBase
    {
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/TheUnresolvedCadence";

        #region Theme Colors

        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);

        #endregion

        #region Inevitability System (static — persists across swings)

        private static int inevitabilityStacks = 0;
        private const int MaxInevitabilityStacks = 10;

        public static void AddInevitabilityStack()
            => inevitabilityStacks = Math.Min(inevitabilityStacks + 1, MaxInevitabilityStacks);

        public static void ResetInevitability()
            => inevitabilityStacks = 0;

        public static int GetInevitabilityStacks() => inevitabilityStacks;

        #endregion

        #region MeleeSwingItemBase Overrides

        protected override int SwingProjectileType
            => ModContent.ProjectileType<TheUnresolvedCadenceSwing>();

        protected override int ComboStepCount => 3;

        protected override Color GetLoreColor() => EnigmaPurple;

        protected override void SetWeaponDefaults()
        {
            Item.damage = 600;
            Item.knockBack = 7f;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
        }

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "[Ultimate Enigma Weapon]") { OverrideColor = EnigmaGreen });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Creates dimensional slashes that persist and warp space"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every swing stacks Inevitability on all enemies on screen"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", $"Current stacks: {inevitabilityStacks}/{MaxInevitabilityStacks}"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "At max stacks triggers Paradox Collapse with massive devastation"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'What is the answer to everything? There is none. That is the final question.'")
            {
                OverrideColor = EnigmaPurple
            });
        }

        #endregion

        #region HoldItem — Inevitability Aura

        public override void HoldItem(Player player)
        {
            base.HoldItem(player); // combo reset timer

            // Lighting
            float intensity = 0.25f + inevitabilityStacks * 0.04f;
            float pulse = MathF.Sin(Main.GameUpdateCount * 0.08f) * 0.1f + 0.9f;
            Color lightColor = Color.Lerp(EnigmaPurple, EnigmaGreen, inevitabilityStacks / 10f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse * intensity);
        }

        #endregion

        #region OnShoot — Inevitability + Paradox Collapse

        protected override void OnShoot(Player player, int projectileIndex)
        {
            // Stack Inevitability on ALL on-screen enemies
            bool anyEnemies = false;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.friendly && Vector2.Distance(npc.Center, player.Center) < 1200f)
                {
                    anyEnemies = true;
                    npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 600);
                    var brandNPC = npc.GetGlobalNPC<ParadoxBrandNPC>();
                    brandNPC.AddParadoxStack(npc, 1);
                }
            }

            if (anyEnemies)
                AddInevitabilityStack();

            // === PARADOX COLLAPSE CHECK ===
            if (inevitabilityStacks >= MaxInevitabilityStacks)
                TriggerParadoxCollapse(player, player.GetSource_ItemUse(Item));
        }

        private void TriggerParadoxCollapse(Player player, IEntitySource source)
        {
            ResetInevitability();

            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f, Volume = 1.2f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.3f, Volume = 1f }, player.Center);

            Projectile.NewProjectile(source, player.Center, Vector2.Zero,
                ModContent.ProjectileType<ParadoxCollapseUltimate>(),
                Item.damage * 3, 15f, player.whoAmI);
        }

        #endregion
    }
}
