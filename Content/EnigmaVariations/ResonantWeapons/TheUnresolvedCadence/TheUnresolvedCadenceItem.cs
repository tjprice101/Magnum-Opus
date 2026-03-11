using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.SandboxExoblade.Utilities;
using MagnumOpus.Content.EnigmaVariations.Debuffs;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence
{
    /// <summary>
    /// THE UNRESOLVED CADENCE — Ultimate Enigma melee broadsword (Item).
    /// Exoblade-architecture swing with right-click dash.
    /// Every swing stacks Inevitability on all enemies; at 10, triggers Paradox Collapse.
    /// </summary>
    public class TheUnresolvedCadenceItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/TheUnresolvedCadence";

        #region Theme Colors

        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);

        #endregion

        #region Inevitability System

        private static int inevitabilityStacks = 0;
        private const int MaxInevitabilityStacks = 10;

        public static void AddInevitabilityStack()
            => inevitabilityStacks = Math.Min(inevitabilityStacks + 1, MaxInevitabilityStacks);

        public static void ResetInevitability()
            => inevitabilityStacks = 0;

        public static int GetInevitabilityStacks() => inevitabilityStacks;

        #endregion

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.autoReuse = true;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.width = 80;
            Item.height = 80;
            Item.damage = 600;
            Item.knockBack = 7f;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
            Item.shoot = ModContent.ProjectileType<TheUnresolvedCadenceSwing>();
            Item.shootSpeed = 1f;
        }

        public override bool CanShoot(Player player)
        {
            bool isDash = player.altFunctionUse == 2;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != player.whoAmI || p.type != Item.shoot)
                    continue;
                if (isDash) return false;
                if (!(p.ai[0] == 1 && p.ai[1] == 1)) return false;
            }
            return true;
        }

        public override void HoldItem(Player player)
        {
            player.ExoBlade().rightClickListener = true;
            player.ExoBlade().mouseWorldListener = true;

            // Inevitability aura VFX
            float intensity = 0.25f + inevitabilityStacks * 0.04f;
            float pulse = MathF.Sin(Main.GameUpdateCount * 0.08f) * 0.1f + 0.9f;
            Color lightColor = Color.Lerp(EnigmaPurple, EnigmaGreen, inevitabilityStacks / 10f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse * intensity);
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float state = player.altFunctionUse == 2 ? 1f : 0f;
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, state, 0);

            // Inevitability stacking on swing (only for normal swings, not dash)
            if (state == 0f)
            {
                bool anyEnemies = false;
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (!npc.friendly && Vector2.Distance(npc.Center, player.Center) < 1200f)
                    {
                        anyEnemies = true;
                        npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
                        var brandNPC = npc.GetGlobalNPC<ParadoxBrandNPC>();
                        brandNPC.AddParadoxStack(npc, 3);
                    }
                }

                if (anyEnemies)
                {
                    for (int i = 0; i < 3; i++)
                        AddInevitabilityStack();
                }

                if (inevitabilityStacks >= MaxInevitabilityStacks)
                    TriggerParadoxCollapse(player, source);
            }

            return false;
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

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "[Ultimate Enigma Weapon]") { OverrideColor = EnigmaGreen });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "3-phase combo: The Question, The Doubt, The Silence"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every swing stacks Inevitability on all enemies on screen"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", $"Inevitability: {inevitabilityStacks}/{MaxInevitabilityStacks} — at max, triggers Paradox Collapse"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Hits brand enemies with Paradox Brand, increasing Enigma damage taken by 20%"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Crits spawn void-green seeking crystals that home on branded enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Resolution is an illusion. There is only the next question.'")
            {
                OverrideColor = EnigmaPurple
            });
        }
    }
}
