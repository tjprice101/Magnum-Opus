using System.Linq;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Utilities;
using MagnumOpus.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor
{
    /// <summary>
    /// Celestial Valor — "The Hero's Burning Oath"
    /// 
    /// An Eroica-themed endgame melee greatsword forged from the crystallized
    /// valor of countless fallen heroes. Self-contained weapon system with own
    /// primitive renderer, particle system, shader loader, utility library, and
    /// ModPlayer — modeled after the Eternal Moon architecture.
    /// 
    /// Left-click: 3-hit escalating combo (Whisper → Declaration → Finale)
    /// Right-click: Valor Dash — charge forward trailing heroic flames. On hit,
    ///   applies stagger + spawns cross-slash VFX. If dash connects, the next
    ///   swing becomes an empowered Heroic Finale regardless of combo position.
    /// </summary>
    public class CelestialValor : ModItem
    {
        // === BALANCE CONSTANTS ===
        public static int ValorDashTime = 40;
        public static int BaseUseTime = 34;

        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.damage = 320;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = BaseUseTime;
            Item.useAnimation = BaseUseTime;
            Item.useTurn = true;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.knockBack = 7.5f;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;
            Item.value = Item.sellPrice(gold: 45);
            Item.shoot = ProjectileType<CelestialValorSwing>();
            Item.shootSpeed = 8f;
            Item.rare = RarityType<EroicaRainbowRarity>();
        }

        public override bool CanShoot(Player player)
        {
            // Alt-click (Valor Dash): only one swing projectile at a time
            if (player.altFunctionUse == 2)
                return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI
                    && n.type == ProjectileType<CelestialValorSwing>());

            // Normal swing: don't overlap with an active non-stasis swing
            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI
                && n.type == ProjectileType<CelestialValorSwing>()
                && !(n.ai[0] == 1 && n.ai[1] == 1));
        }

        public override void HoldItem(Player player)
        {
            player.CelestialValor().RightClickListener = true;
            player.CelestialValor().MouseWorldListener = true;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
            Vector2 velocity, int type, int damage, float knockback)
        {
            float state = 0;

            // Check for empowered Heroic Finale opportunity (after successful dash hit)
            bool empoweredSlash = false;
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.owner == player.whoAmI && p.type == Item.shoot &&
                    p.ai[0] == 1 && p.ai[1] == 1 &&
                    p.timeLeft > 60 * 3)
                {
                    empoweredSlash = true;
                    break;
                }
            }

            if (empoweredSlash)
            {
                state = 2; // Empowered Heroic Finale
                foreach (Projectile p in Main.ActiveProjectiles)
                {
                    if (p.owner != player.whoAmI || p.type != Item.shoot || p.ai[0] != 1 || p.ai[1] != 1)
                        continue;
                    p.timeLeft = 60 * 3;
                    p.netUpdate = true;
                }
            }

            if (player.altFunctionUse == 2)
                state = 1; // Valor Dash

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, state, 0);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var player = Main.LocalPlayer;
            int comboStep = player.CelestialValor().ComboStep;
            string[] phaseNames = { "Valor's Whisper", "Crimson Declaration", "Heroic Finale" };
            string phaseName = phaseNames[comboStep % 3];

            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "3-hit escalating combo launches heroic energy slashes"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Right-click for Valor Dash — charge through enemies in blazing glory"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Dash hits empower the next swing to a devastating Heroic Finale"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "Critical strikes unleash seeking valor crystals"));
            tooltips.Add(new TooltipLine(Mod, "Phase",
                $"Current phase: {phaseName}")
            { OverrideColor = ValorUtils.GetHeroicGradient(comboStep / 2f) });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Each swing carries the final words of heroes who fell with their oath unbroken'")
            { OverrideColor = new Color(200, 50, 50) });
        }
    }
}
