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
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.EnigmaVariations.Debuffs;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons
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
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence";

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

            // Orbiting glyphs proportional to stacks
            if (inevitabilityStacks > 0 && Main.rand.NextBool(20))
            {
                float stackAngle = Main.GameUpdateCount * 0.03f;
                float stackRadius = 40f + MathF.Sin(Main.GameUpdateCount * 0.05f) * 8f;
                Vector2 stackPos = player.Center + stackAngle.ToRotationVector2() * stackRadius;
                Color stackColor = Color.Lerp(EnigmaPurple, EnigmaGreen, inevitabilityStacks / 10f);
                CustomParticles.Glyph(stackPos, stackColor, 0.3f);
            }

            // Near-max reality warp
            if (inevitabilityStacks >= 7 && Main.rand.NextBool(25))
            {
                Vector2 warpPos = player.Center + Main.rand.NextVector2Circular(50f, 50f);
                CustomParticles.GenericFlare(warpPos, EnigmaGreen, 0.3f, 12);
            }

            // Dimensional rift aura
            if (Main.rand.NextBool(20))
            {
                Vector2 riftPos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                float p = Main.rand.NextFloat();
                Color riftColor = p < 0.5f
                    ? Color.Lerp(EnigmaBlack, EnigmaPurple, p * 2f)
                    : Color.Lerp(EnigmaPurple, EnigmaGreen, (p - 0.5f) * 2f);
                var rift = new GenericGlowParticle(riftPos, Main.rand.NextVector2Circular(1f, 1f),
                    riftColor, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(rift);
            }

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

                    // Visual indicator
                    if (Main.GameUpdateCount % 2 == 0)
                    {
                        Vector2 sparkPos = npc.Center - new Vector2(0, npc.height / 2f + 15f);
                        float gp = Main.rand.NextFloat();
                        Color sparkColor = (gp < 0.5f
                            ? Color.Lerp(EnigmaBlack, EnigmaPurple, gp * 2f)
                            : Color.Lerp(EnigmaPurple, EnigmaGreen, (gp - 0.5f) * 2f)) * 0.6f;
                        CustomParticles.GenericFlare(sparkPos, sparkColor, 0.35f, 12);
                    }
                }
            }

            if (anyEnemies)
                AddInevitabilityStack();

            // Swing VFX
            CustomParticles.GenericFlare(player.Center, EnigmaGreen, 0.7f, 15);
            CustomParticles.HaloRing(player.Center, EnigmaPurple, 0.4f, 12);
            ThemedParticles.EnigmaMusicNotes(player.Center, 4, 35f);

            // Glyph stack indicator
            if (inevitabilityStacks > 0)
                CustomParticles.GlyphStack(player.Center - new Vector2(0, 50f), EnigmaGreen, inevitabilityStacks, 0.25f);

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

            UnifiedVFX.EnigmaVariations.Explosion(player.Center, 1.5f);
            CustomParticles.GlyphCircle(player.Center, EnigmaPurple, count: 8, radius: 100f, rotationSpeed: 0.08f);
            CustomParticles.GlyphBurst(player.Center, EnigmaGreen, count: 6, speed: 5f);
            ThemedParticles.EnigmaMusicNoteBurst(player.Center, 6, 5f);
        }

        #endregion
    }
}
