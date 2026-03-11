using MagnumOpus.Common;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.SandboxExoblade.Utilities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor
{
    /// <summary>
    /// Celestial Valor — Eroica's signature melee broadsword embodying the hero's triumphant first movement.
    /// Features a 4-phase Heroic Crescendo combo with escalating valor slash arcs, beam projectiles,
    /// a Valor Gauge that builds toward a devastating Gloria finale, and Hero's Resolve empowerment below 30% HP.
    /// Combo tracking lives in CelestialValorSwing so it advances on hold re-swings.
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

            // Combo projectiles are now spawned by CelestialValorSwing.OnSwingStart()
            // so they advance on hold re-swings, not just on initial click.

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "Heroic Crescendo — 4-phase combo that builds to a devastating Finale Fortissimo"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Combo spawns valor beams and culminates in a massive heroic detonation"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Valor Gauge builds on successive hits — at maximum, Finale becomes Gloria"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
            "Hero's Resolve: below 30% HP, all swings deal 25% more damage")
            { OverrideColor = EroicaPalette.Gold });
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'Rise, even when the world says fall.'")
            { OverrideColor = new Color(200, 50, 50) });
        }
    }
}
