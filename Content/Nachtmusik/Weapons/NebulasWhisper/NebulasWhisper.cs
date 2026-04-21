using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Weapons.NebulasWhisper.Projectiles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Nachtmusik.Weapons.NebulasWhisper
{
    public class NebulasWhisper : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 58;
            Item.damage = 1200;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3.5f;
            Item.value = Item.sellPrice(gold: 42);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item38 with { Pitch = 0.1f, Volume = 0.85f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<NebulaWhisperShot>();
            Item.shootSpeed = 20f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 18;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: detonate all active Nachtmusik damage zones owned by this player
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == player.whoAmI
                        && proj.type == ModContent.ProjectileType<GenericDamageZone>()
                        && (int)proj.localAI[0] == GenericHomingOrbChild.THEME_NACHTMUSIK)
                    {
                        proj.Kill();
                    }
                }
                return false;
            }

            // Left-click: spawn a decelerating orb that becomes a cloud on death
            GenericHomingOrbChild.SpawnChild(
                source, position, velocity.SafeNormalize(Vector2.UnitX) * 14f,
                damage, knockback, player.whoAmI,
                homingStrength: 0.04f,
                behaviorFlags: GenericHomingOrbChild.FLAG_DECELERATE | GenericHomingOrbChild.FLAG_ZONE_ON_KILL,
                themeIndex: GenericHomingOrbChild.THEME_NACHTMUSIK,
                scaleMult: 1f,
                timeLeft: 90);

            return false;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-3f, 0f);

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires decelerating orbs that become lingering nebula clouds"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Right click to detonate all active clouds"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The nebula does not shout. It barely breathes. But entire stars are born in its exhale.'")
            {
                OverrideColor = NachtmusikPalette.LoreText
            });
        }
    }
}
