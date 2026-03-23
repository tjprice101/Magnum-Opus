using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Utilities;
using MagnumOpus.Content.SandboxExoblade.Utilities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment
{
    /// <summary>
    /// Chain of Judgment — Dies Irae's burning chain weapon that binds and judges.
    /// Exoblade-architecture weapon item with channel-hold swing and dash attack.
    /// </summary>
    public class ChainOfJudgment : ModItem
    {

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 65;
            Item.height = 65;
            Item.scale = 0.10f;
            Item.damage = 280;
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
            Item.shoot = ModContent.ProjectileType<Projectiles.JudgmentChainProjectile>();
            Item.shootSpeed = 8f;
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
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
            player.GetModPlayer<ChainOfJudgmentPlayer>().IsHoldingChainOfJudgment = true;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                var wp = player.GetModPlayer<ChainOfJudgmentPlayer>();
                if (wp.IsChargeFull)
                {
                    wp.ConsumeCharge();
                    SoundEngine.PlaySound(SoundID.Item45 with { Pitch = -0.2f }, player.Center);
                    Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero,
                        ModContent.ProjectileType<Projectiles.ChainJudgmentSpecialProj>(),
                        0, 0f, player.whoAmI);
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.Item16 with { Pitch = 0.5f, Volume = 0.5f }, player.Center);
                }
                return false;
            }

            // Normal left-click swing
            float state = 0f;
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, state, 0);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "Burning chain swings that bind and judge all who are struck"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Each swing trails ember dust and solar flare sparks"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Hits spawn fire and solar flare bursts at impact point"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
            "Right-click dash attack unleashes a massive infernal chain burst")
            { OverrideColor = DiesIraePalette.JudgmentGold });
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'No sinner escapes the chain. It finds them in the dark.'")
            { OverrideColor = new Color(200, 50, 30) });
        }
    }
}
