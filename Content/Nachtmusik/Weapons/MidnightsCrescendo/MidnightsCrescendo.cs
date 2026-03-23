using MagnumOpus.Common;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.SandboxExoblade.Utilities;
using MagnumOpus.Content.Nachtmusik.Weapons.MidnightsCrescendo.Projectiles;
using MagnumOpus.Content.Nachtmusik.Weapons.MidnightsCrescendo.Utilities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Weapons.MidnightsCrescendo
{
    /// <summary>
    /// Midnight's Crescendo — Nachtmusik's ascending blade. Exoblade-architecture weapon item.
    /// A star being born — starlight shimmer and indigo crescendo arcs.
    /// </summary>
    public class MidnightsCrescendo : ModItem, IOverdriveItem
    {
        public IResonantOverdrive GetOverdrivePlayer(Player player) => player.GetModPlayer<MidnightsCrescendoPlayer>();

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 65;
            Item.height = 65;
            Item.scale = 0.09f;
            Item.damage = 260;
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
            Item.shoot = ModContent.ProjectileType<MidnightsCrescendoSwing>();
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
            player.GetModPlayer<MidnightsCrescendoPlayer>().IsHoldingMidnightsCrescendo = true;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                var mp = player.GetModPlayer<MidnightsCrescendoPlayer>();
                if (mp.IsChargeFull)
                {
                    mp.ConsumeCharge();
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.2f }, player.Center);
                    // Rain 12 falling stars from above
                    for (int i = 0; i < 12; i++)
                    {
                        Vector2 spawnPos = Main.MouseWorld + new Vector2(Main.rand.NextFloat(-300f, 300f), -800f);
                        Vector2 starVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(10f, 14f));
                        Projectile.NewProjectile(source, spawnPos, starVel,
                            ModContent.ProjectileType<MidnightsCrescendoSpecialProj>(),
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
            "Ascending starlight blade with shimmering indigo-silver slash arcs"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Right-click performs a stellar dash strike through enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Swings trail starlight shimmer and crescendo sparks"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'The night starts quiet. It does not end that way.'")
            { OverrideColor = new Color(100, 120, 200) });
        }
    }
}
