using MagnumOpus.Common;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.SandboxExoblade.Utilities;
using MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Projectiles;
using MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Utilities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner
{
    /// <summary>
    /// Nocturnal Executioner — Nachtmusik's void greatsword. Exoblade-architecture weapon item.
    /// The blade doesn't shine — it CONSUMES. Features a 4-phase combo with escalating
    /// nocturnal blade projectiles and devastating void burst finale.
    /// </summary>
    public class NocturnalExecutioner : ModItem, IOverdriveItem
    {
        public IResonantOverdrive GetOverdrivePlayer(Player player) => player.GetModPlayer<NocturnalExecutionerPlayer>();

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.scale = 0.09f;
            Item.damage = 350;
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
            Item.shoot = ModContent.ProjectileType<NocturnalExecutionerSwing>();
            Item.shootSpeed = 8f;
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
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
            player.GetModPlayer<NocturnalExecutionerPlayer>().IsHoldingNocturnalExecutioner = true;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                var ep = player.GetModPlayer<NocturnalExecutionerPlayer>();
                if (ep.IsChargeFull)
                {
                    ep.ConsumeCharge();
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.5f }, player.Center);
                    if (!Main.dayTime)
                    {
                        // Nighttime: massive damage marker on all nearby enemies
                        foreach (NPC npc in Main.ActiveNPCs)
                        {
                            if (!npc.CanBeChasedBy() || Vector2.Distance(npc.Center, player.Center) > 1000f) continue;
                            Projectile.NewProjectile(source, npc.Center, Vector2.Zero,
                                ModContent.ProjectileType<NocturnalExecutionerSpecialProj>(),
                                damage * 3, knockback, player.whoAmI);
                        }
                    }
                    else
                    {
                        // Daytime: spawn 5 seeking void orbs
                        for (int i = 0; i < 5; i++)
                        {
                            Vector2 offset = Main.rand.NextVector2CircularEdge(100f, 100f);
                            Vector2 orbVel = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX) * 8f;
                            Projectile.NewProjectile(source, player.Center + offset, orbVel,
                                ModContent.ProjectileType<NocturnalExecutionerSpecialProj>(),
                                damage * 2, knockback, player.whoAmI);
                        }
                    }
                }
                else
                    SoundEngine.PlaySound(SoundID.Item16 with { Pitch = 0.5f, Volume = 0.5f }, player.Center);
                return false;
            }
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, 0f, 0);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "Devastating void greatsword with 4-phase combo spawning nocturnal blades"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Right-click performs a void dash strike through enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Combo escalates: flanking blades, wide spreads, then cardinal burst finale"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
            "Hits apply Frostburn and release void-edged starlight sparks"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'At midnight, the executioner does not knock. The stars simply go dark.'")
            { OverrideColor = new Color(100, 120, 200) });
        }
    }
}
