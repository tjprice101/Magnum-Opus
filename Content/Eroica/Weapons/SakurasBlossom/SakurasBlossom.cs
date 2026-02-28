using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Utilities;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom
{
    /// <summary>
    /// Sakura's Blossom — Eroica endgame melee weapon. Direct ModItem pattern.
    /// 4-phase sakura combo that escalates spectral copies:
    ///   Phase 0: Petal Slash       — 1 spectral copy
    ///   Phase 1: Crimson Scatter   — 2 spectral copies
    ///   Phase 2: Blossom Bloom     — 3 spectral copies
    ///   Phase 3: Storm of Petals   — 4 spectral copies + finisher VFX
    /// Alt-click: Petal Dash — charge forward scattering sakura petals.
    /// </summary>
    public class SakurasBlossom : ModItem
    {
        public const int PetalDashTime = 40;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.damage = 350;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.scale = 1.3f;
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

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanShoot(Player player)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == Item.shoot)
                    return false;
            }
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int state = 0;
            if (player.altFunctionUse == 2)
                state = 1; // Petal Dash

            var sakura = player.SakuraBlossom();
            if (sakura.ComboStep == 3 && state == 0)
                state = 2; // Empowered Storm of Petals

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai0: state);
            return false;
        }

        public override void HoldItem(Player player)
        {
            var sakura = player.SakuraBlossom();
            sakura.RightClickListener = player.altFunctionUse == 2;
            sakura.MouseWorldListener = Main.MouseWorld;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var sakura = player.SakuraBlossom();
            string[] phases = { "Petal Slash", "Crimson Scatter", "Blossom Bloom", "Storm of Petals" };

            tooltips.Add(new TooltipLine(Mod, "SakuraCombo",
                $"4-phase sakura combo — current: {phases[sakura.ComboStep]}")
            { OverrideColor = EroicaPalette.Sakura });
            tooltips.Add(new TooltipLine(Mod, "Spectral",
                "Spectral copies home to enemies and scatter petal bursts on impact")
            { OverrideColor = new Color(255, 180, 200) });
            tooltips.Add(new TooltipLine(Mod, "Dash",
                "Right-click to perform a Petal Dash through enemies")
            { OverrideColor = EroicaPalette.Gold });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Each petal carries the memory of a hero who chose beauty over survival'")
            { OverrideColor = new Color(200, 50, 50) });
        }
    }
}
