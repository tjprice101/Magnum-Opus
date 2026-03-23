using MagnumOpus.Common;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Utilities;
using MagnumOpus.Content.SandboxExoblade.Utilities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning
{
    /// <summary>
    /// Thornbound Reckoning — Ode to Joy's botanical broadsword.
    /// Exoblade-architecture melee with verdant green slash arcs and golden pollen accents.
    /// </summary>
    public class ThornboundReckoning : ModItem, IOverdriveItem
    {
        public IResonantOverdrive GetOverdrivePlayer(Player player) => player.GetModPlayer<ThornboundReckoningPlayer>();

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.scale = 0.09f;
            Item.damage = 290;
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
            Item.shoot = ModContent.ProjectileType<ThornboundSwingProj>();
            Item.shootSpeed = 8f;
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
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
            player.GetModPlayer<ThornboundReckoningPlayer>().IsHoldingThornboundReckoning = true;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Right-click: charge-gated special — spawn 3 bouncing blades aimed at nearest enemies
            if (player.altFunctionUse == 2)
            {
                var tbp = player.GetModPlayer<ThornboundReckoningPlayer>();

                if (tbp.IsChargeFull)
                {
                    tbp.ConsumeCharge();
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.9f }, player.MountedCenter);

                    // Find up to 3 nearest enemies
                    int bladeCount = 3;
                    int spawned = 0;

                    for (int b = 0; b < bladeCount; b++)
                    {
                        NPC target = null;
                        float closestDist = 800f;
                        foreach (NPC npc in Main.ActiveNPCs)
                        {
                            if (npc.CanBeChasedBy())
                            {
                                float dist = Vector2.Distance(player.MountedCenter, npc.Center);
                                if (dist < closestDist)
                                {
                                    closestDist = dist;
                                    target = npc;
                                }
                            }
                        }

                        Vector2 dir = target != null
                            ? (target.Center - player.MountedCenter).SafeNormalize(Vector2.UnitX)
                            : (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);

                        // Spread the blades slightly
                        float spread = (b - 1) * 0.2f;
                        dir = dir.RotatedBy(spread);

                        Projectile.NewProjectile(source, player.MountedCenter, dir * 12f,
                            ModContent.ProjectileType<ThornboundReckoningSpecialProj>(),
                            (int)(damage * 1.2f), knockback, player.whoAmI);
                        spawned++;
                    }
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
            "Botanical Reckoning — sweeping vine slashes with thorn-laced arcs"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Right-click to dash through enemies with a verdant blade charge"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Successive hits scatter golden pollen that amplifies damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'The vine does not ask permission to grow. It simply overcomes.'")
            { OverrideColor = new Color(255, 200, 50) });
        }
    }
}
