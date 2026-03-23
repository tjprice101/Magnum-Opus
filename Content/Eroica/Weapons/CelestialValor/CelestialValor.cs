using MagnumOpus.Common;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Projectiles;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Utilities;
using MagnumOpus.Content.SandboxExoblade.Utilities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor
{
    /// <summary>
    /// Celestial Valor — Eroica's signature melee broadsword embodying the hero's triumphant first movement.
    /// Features a 4-phase Heroic Crescendo combo with escalating valor slash arcs and beam projectiles.
    /// Hits build a Valor Charge meter — at full, right-click launches the blade skyward.
    /// Right-click again to ignite and hurl the blade at the nearest enemy, creating a devastating inferno zone.
    /// </summary>
    public class CelestialValor : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.scale = 0.09f;
            Item.damage = 320;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useTurn = true;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.knockBack = 7.5f;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;
            Item.value = Item.sellPrice(gold: 45);
            Item.shoot = ModContent.ProjectileType<CelestialValorSwing>();
            Item.shootSpeed = 8f;
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
        }

        public override bool CanShoot(Player player)
        {
            // Right-click: always allow (Shoot handles deny/flying blade logic)
            if (player.altFunctionUse == 2)
                return true;

            // Normal swing: don't overlap with an active swing
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != player.whoAmI || p.type != Item.shoot)
                    continue;
                return false;
            }
            return true;
        }

        public override void HoldItem(Player player)
        {
            player.CelestialValor().IsHoldingCelestialValor = true;
            player.ExoBlade().rightClickListener = true;
            player.ExoBlade().mouseWorldListener = true;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Right-click: charge-gated special
            if (player.altFunctionUse == 2)
            {
                var cvp = player.CelestialValor();

                if (cvp.HasActiveFlyingBlade)
                {
                    // Second right-click: signal the flying blade to ignite and hurl
                    cvp.TriggerBladeHurl = true;
                    SoundEngine.PlaySound(SoundID.Item74 with { Pitch = 0.2f }, player.MountedCenter);
                }
                else if (cvp.IsChargeFull)
                {
                    // First right-click at full charge: spawn flying blade
                    cvp.ConsumeCharge();
                    int bladeDmg = (int)(damage * 1.5f);
                    Projectile.NewProjectile(source, player.MountedCenter, Vector2.UnitY * -4f,
                        ModContent.ProjectileType<CelestialValorFlyingBlade>(),
                        bladeDmg, knockback, player.whoAmI);
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.9f }, player.MountedCenter);
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.3f, Volume = 0.4f }, player.MountedCenter);
                }
                return false;
            }

            // Normal left-click: spawn swing projectile
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, 0f, 0);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Heroic Crescendo — 4-phase combo that builds to a devastating Finale Fortissimo"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Combo spawns valor beams and culminates in a massive heroic detonation"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Hits build valor charge — at maximum, right-click to launch the blade skyward"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "Right-click again to ignite and hurl the blade, creating a devastating inferno zone")
            { OverrideColor = EroicaPalette.Gold });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Rise, even when the world says fall.'")
            { OverrideColor = new Color(200, 50, 50) });
        }
    }
}
