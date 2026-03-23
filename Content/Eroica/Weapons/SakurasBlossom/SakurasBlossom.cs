using MagnumOpus.Common;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Projectiles;
using MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Utilities;
using MagnumOpus.Content.SandboxExoblade.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom
{
    /// <summary>
    /// Sakura's Blossom — Eroica melee weapon embodying the hero's beauty and sacrifice.
    /// Features a fluid 3-phase Petal Dance combo that scatters cherry blossom petals.
    /// True melee strikes build blossom charge very slowly — at full, right-click summons
    /// 4 spectral blade turrets that fire sparkling lasers for 5 seconds.
    /// </summary>
    public class SakurasBlossom : ModItem
    {

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.width = 70;
            Item.height = 70;
            Item.damage = 350;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.scale = 0.08f;
            Item.value = Item.sellPrice(gold: 40);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<SakurasBlossomSwing>();
            Item.shootSpeed = 6f;
            Item.maxStack = 1;
        }

        public override bool CanShoot(Player player)
        {
            // Right-click: always allow (Shoot handles deny logic)
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
            player.SakurasBlossom().IsHoldingSakura = true;
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
                var sbp = player.SakurasBlossom();
                if (sbp.IsChargeFull)
                {
                    sbp.ConsumeCharge();
                    // Spawn 4 spectral turrets at cardinal offsets from player
                    Vector2[] offsets = { new(0, -80), new(0, 80), new(-80, 0), new(80, 0) };
                    int turretDmg = (int)(damage * 0.8f);
                    for (int i = 0; i < 4; i++)
                    {
                        Projectile.NewProjectile(source, player.MountedCenter + offsets[i],
                            Vector2.Zero, ModContent.ProjectileType<SakuraSpectralTurret>(),
                            turretDmg, 0f, player.whoAmI, i);
                    }
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.9f }, player.MountedCenter);
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.3f, Volume = 0.4f }, player.MountedCenter);
                }
                return false;
            }

            // Normal left-click: spawn swing projectile (petal combo handled in SakurasBlossomSwing.OnSwingStart)
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, 0f, 0);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Petal Dance — 3-phase flowing combo scatters sakura petals and homing spectral copies")
            { OverrideColor = new Color(255, 180, 200) });
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Final Bloom unleashes a 360-degree petal burst that converges on struck enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "True melee strikes build blossom charge very slowly"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "At full charge, right-click summons 4 spectral blade turrets that fire sparkling lasers for 5 seconds")
            { OverrideColor = EroicaPalette.Gold });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'A single petal falls. An army follows.'")
            { OverrideColor = new Color(200, 50, 50) });
        }
    }
}
