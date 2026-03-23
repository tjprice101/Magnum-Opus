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
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Utilities;
using MagnumOpus.Common.Systems.UI;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid
{
    /// <summary>
    /// VARIATIONS OF THE VOID — Enigma Melee Sword (Item).
    /// Exoblade-architecture swing with right-click dash.
    /// Every 3rd swing spawns VoidConvergenceBeamSet.
    /// </summary>
    public class VariationsOfTheVoidItem : ModItem, IOverdriveItem
    {
        public IResonantOverdrive GetOverdrivePlayer(Player player) => player.GetModPlayer<VoidVariationPlayer>();

        public override string Texture => "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/VariationsOfTheVoid";

        private static readonly Color EnigmaPurple = new Color(140, 60, 200);

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
            Item.damage = 380;
            Item.knockBack = 6f;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.value = Item.sellPrice(gold: 18);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
            Item.shoot = ModContent.ProjectileType<VariationsOfTheVoidSwing>();
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
            player.GetModPlayer<VoidVariationPlayer>().IsHoldingVariationsOfTheVoid = true;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Right-click: charge-gated special — spawn 1 massive energy wave
            if (player.altFunctionUse == 2)
            {
                var vvp = player.GetModPlayer<VoidVariationPlayer>();

                if (vvp.IsChargeFull)
                {
                    vvp.ConsumeCharge();
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.4f, Volume = 1.1f }, player.MountedCenter);

                    Vector2 dir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
                    Projectile.NewProjectile(source, player.MountedCenter, dir * 6f,
                        ModContent.ProjectileType<Projectiles.VoidSpecialProj>(),
                        (int)(damage * 2.0f), knockback * 2f, player.whoAmI);
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.3f, Volume = 0.4f }, player.MountedCenter);
                }
                return false;
            }

            // Normal left-click: spawn swing projectile
            float state = 0f;
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, state, 0);

            // Combo projectiles (VoidConvergenceBeamSet) are now spawned by VariationsOfTheVoidSwing.OnSwingStart()

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
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
    }
}
