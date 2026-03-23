using MagnumOpus.Common;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.SandboxExoblade.Utilities;
using MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Projectiles;
using MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Utilities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance
{
    /// <summary>
    /// Twilight Severance — Nachtmusik's dimensional katana. Exoblade-architecture weapon item.
    /// Severs the boundary between dusk and starlight with indigo-silver slash arcs.
    /// </summary>
    public class TwilightSeverance : ModItem, IOverdriveItem
    {
        public IResonantOverdrive GetOverdrivePlayer(Player player) => player.GetModPlayer<TwilightSeverancePlayer>();

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.scale = 0.09f;
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
            Item.value = Item.sellPrice(gold: 40);
            Item.shoot = ModContent.ProjectileType<TwilightSeveranceSwing>();
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
            player.GetModPlayer<TwilightSeverancePlayer>().IsHoldingTwilightSeverance = true;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                var tp = player.GetModPlayer<TwilightSeverancePlayer>();
                if (tp.IsChargeFull)
                {
                    tp.ConsumeCharge();
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.3f }, player.Center);
                    foreach (NPC npc in Main.ActiveNPCs)
                    {
                        if (!npc.CanBeChasedBy() || Vector2.Distance(npc.Center, player.Center) > 800f) continue;
                        Projectile.NewProjectile(source, npc.Center, Vector2.Zero,
                            ModContent.ProjectileType<TwilightSeveranceSpecialProj>(),
                            damage * 2, knockback, player.whoAmI);
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
            "Dimensional katana that severs the boundary between dusk and starlight"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Right-click performs a stellar dash strike through enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Swings leave trails of indigo starlight and silver sparks"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'Between dusk and starlight, every cut severs what was from what will be.'")
            { OverrideColor = new Color(100, 120, 200) });
        }
    }
}
