using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.TestWeapons._05_ArcaneHarmonics
{
    /// <summary>
    /// 🎵 Arcane Harmonics — Music-themed 5-step combo melee weapon.
    /// Step 0: Prelude Strike — quick opening slash
    /// Step 1: Staccato Rend — sharp reverse cut
    /// Step 2: Crescendo Arc — wide sweep spawning 3 orbiting NoteProjectiles
    /// Step 3: Fortissimo Cleave — heavy downward strike spawning HarmonicRingProjectile
    /// Step 4: Grand Finale — massive slam spawning SymphonyBurstProjectile + ResonanceFieldProjectile + screen shake
    ///
    /// Uses EnhancedTrailRenderer (not CalamityStyleTrailRenderer) for primitive trail rendering.
    /// Uses HueShiftingMusicNoteParticle for unique music-themed particle effects.
    /// </summary>
    public class ArcaneHarmonicsItem : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.EnchantedSword;

        private const int MaxComboSteps = 5;
        private const int ComboResetDelay = 45;

        private int comboStep = 0;
        private int comboResetTimer = 0;

        public override void SetDefaults()
        {
            Item.damage = 200;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.width = 70;
            Item.height = 70;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 15);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<ArcaneHarmonicsSwing>();
            Item.shootSpeed = 1f;
        }

        public override void HoldItem(Player player)
        {
            if (comboResetTimer > 0)
            {
                comboResetTimer--;
                if (comboResetTimer <= 0)
                    comboStep = 0;
            }
        }

        public override bool CanShoot(Player player)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == Item.shoot)
                {
                    if (p.ModProjectile is ArcaneHarmonicsSwing swing && swing.InPostSwingStasis)
                        return true;
                    return false;
                }
            }
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            position = player.RotatedRelativePoint(player.MountedCenter, true);
            velocity = position.DirectionTo(Main.MouseWorld);

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback,
                player.whoAmI, ai0: comboStep);

            comboStep = (comboStep + 1) % MaxComboSteps;
            comboResetTimer = ComboResetDelay;
            return false;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "TestInfo", "[Test Weapon 05] 5-step arcane combo with EnhancedTrailRenderer"));
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Five-swing symphonic combo building to a grand finale"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Third strike spawns orbiting notes, fourth releases harmonic shockwaves"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Final strike unleashes a symphonic burst and lingering resonance field"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each swing is a note in the melody of destruction'")
            {
                OverrideColor = new Color(120, 80, 220)
            });
        }
    }
}
