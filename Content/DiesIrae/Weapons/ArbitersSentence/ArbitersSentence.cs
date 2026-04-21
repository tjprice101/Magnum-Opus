using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Systems;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence
{
    /// <summary>
    /// Arbiter's Sentence — Dies Irae precision flamethrower.
    /// Rapid fire applies stacking Judgment Flame debuff.
    /// At 5 stacks: Sentence Cage roots enemy, next hit = 2x damage.
    /// 5 consecutive hits: Arbiter's Focus — 3 precision shots with +40% damage.
    /// </summary>
    public class ArbitersSentence : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 24;
            Item.damage = 400;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 3;
            Item.useAnimation = 9;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 1f;
            Item.value = Item.sellPrice(platinum: 1, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item34 with { Pitch = 0.15f, Volume = 0.6f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.JudgmentFlameProjectile>();
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Gel;
            Item.crit = 15;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10f, 0f);

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            var combat = player.GetModPlayer<DiesIraeCombatPlayer>();

            // Arbiter's Focus bonus
            if (combat.ArbiterFocusShots > 0)
            {
                damage = (int)(damage * 1.4f);
            }

            // Add spread for flamethrower feel
            velocity = velocity.RotatedByRandom(0.08f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var combat = player.GetModPlayer<DiesIraeCombatPlayer>();

            bool isFocusShot = combat.ArbiterFocusShots > 0;
            if (isFocusShot)
            {
                combat.ArbiterFocusShots--;
                if (combat.ArbiterFocusShots == 0)
                {
                    // Focus ended
                    SoundEngine.PlaySound(SoundID.Item8 with { Pitch = -0.3f, Volume = 0.5f }, player.Center);
                }
            }

            // Spawn flame with focus shot flag
            Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI,
                ai0: isFocusShot ? 1f : 0f);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var combat = player.GetModPlayer<DiesIraeCombatPlayer>();

            if (combat.ArbiterFocusShots > 0)
            {
                string focusText = $"[c/C8AA32:ARBITER'S FOCUS ACTIVE] ({combat.ArbiterFocusShots} shots)";
                tooltips.Add(new TooltipLine(Mod, "FocusActive", focusText));
            }
            else
            {
                string hitText = $"Consecutive hits: {combat.ArbiterConsecutiveHits}/5";
                tooltips.Add(new TooltipLine(Mod, "HitCounter", hitText));
            }

            tooltips.Add(new TooltipLine(Mod, "Effect1", "Precision flamethrower applying stacking Judgment Flame"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "At 5 stacks: Sentence Cage roots enemy, next hit deals 2x damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "5 consecutive hits activates Arbiter's Focus — 3 precision shots +40% damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Killing sentenced enemies transfers flames to nearby foes"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The arbiter does not miss. The arbiter does not forgive.'")
            {
                OverrideColor = DiesIraePalette.LoreText
            });
        }
    }
}
