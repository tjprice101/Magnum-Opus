using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Projectiles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Utilities;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime
{
    /// <summary>
    /// Dual Fated Chime — Twin bell-blades that ring with every clash.
    /// 
    /// COMBAT SYSTEM (Inferno Waltz Combo):
    /// • 5-phase alternating left/right combo:
    ///   Toll 1 "Opening Peal": Right chime horizontal slash + bell shockwave ring
    ///   Toll 2 "Answer": Left chime diagonal slash (faster)
    ///   Toll 3 "Escalation": Right chime upward arc + flame wave projectile
    ///   Toll 4 "Resonance": Left chime downward slam + double shockwave + ground fire
    ///   Toll 5 "Grand Toll": Both chimes cross-slash + 12 directional Bell Flame Waves
    /// • Bell Resonance Stacking: Hits add Resonance Rings (max 5). At 5, Bell Shatter detonates.
    /// • Flame Waltz Dodge: During Toll 2 or 4, player sways with 0.2s iframes.
    /// • Applies Resonant Toll debuff on all hits
    /// 
    /// PRESERVED STATS: Damage 380, UseTime 16, Knockback 6.5, MeleeNoSpeed, Sell 50g
    /// </summary>
    public class DualFatedChime : ModItem
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/DualFatedChime/DualFatedChime";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.autoReuse = true;

            Item.damage = 380;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.knockBack = 6.5f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.UseSound = SoundID.Item1 with { Pitch = -0.2f, Volume = 0.8f };

            Item.width = 80;
            Item.height = 80;
            Item.shoot = ModContent.ProjectileType<DualFatedChimeSwingProj>();
            Item.shootSpeed = 1f;
        }

        public override bool CanShoot(Player player)
        {
            int swingType = ModContent.ProjectileType<DualFatedChimeSwingProj>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == swingType)
                    return false;
            }
            return true;
        }

        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        // No alt-fire — the 5-phase combo IS the full system
        public override bool AltFunctionUse(Player player) => false;

        public override bool CanUseItem(Player player)
        {
            Item.useTime = 16;
            Item.useAnimation = 16;
            return true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity,
            ref int type, ref int damage, ref float knockback)
        {
            position = player.MountedCenter;
            velocity = player.MountedCenter.SafeDirectionTo(Main.MouseWorld);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var dfc = player.DualFatedChime();

            // 5-phase combo: pass combo step to swing projectile
            int comboStep = dfc.ComboStep;
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback,
                player.whoAmI, ai0: comboStep);

            dfc.AdvanceCombo();

            // Flame Waltz Dodge: during Toll 2 (step 1) and Toll 4 (step 3), grant brief iframes + dash
            if (comboStep == 1 || comboStep == 3)
            {
                player.immune = true;
                player.immuneTime = 12; // 0.2s at 60fps
                player.immuneNoBlink = true;
                // Sway in swing direction
                float swayDir = (comboStep == 1) ? -1f : 1f;
                player.velocity += velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2 * swayDir) * 4f;
            }

            return false;
        }

        public override void HoldItem(Player player)
        {
            var dfc = player.DualFatedChime();

            // Infernal ambient based on combo progression
            float comboIntensity = dfc.ComboStep / 4f; // 0 to 1 across 5 phases

            if (comboIntensity > 0.1f)
            {
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f + comboIntensity * 3f);

                if (Main.rand.NextBool((int)MathHelper.Lerp(8, 2, comboIntensity)))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(15f + comboIntensity * 15f, 15f + comboIntensity * 15f);
                    int dustType = Main.rand.NextBool() ? DustID.Torch : DustID.Smoke;
                    Color col = DualFatedChimeUtils.GetFireFlicker(Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(player.Center + offset, dustType,
                        new Vector2(0, -1.5f) + Main.rand.NextVector2Circular(1f, 1f), 0, col, 0.8f + comboIntensity * 0.8f);
                    d.noGravity = true;
                    d.fadeIn = 1f;
                }

                Vector3 fireLight = new Vector3(0.5f + comboIntensity * 0.4f, 0.25f + comboIntensity * 0.2f, 0.05f) * (0.6f + pulse * 0.2f);
                Lighting.AddLight(player.Center, fireLight);
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "5-phase alternating inferno waltz combo with escalating power"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Hits apply Bell Resonance stacks — at 5 stacks, triggers devastating Bell Shatter"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Toll 2 and Toll 4 grant a brief dodge with invincibility frames"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Grand Toll unleashes 12 directional bell flame waves"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Two bells toll as one — their song turns steel to cinder'")
            {
                OverrideColor = DualFatedChimeUtils.LoreColor
            });
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.15f + 0.25f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);

            // Fiery underglow
            spriteBatch.Draw(tex, drawPos + new Vector2(-1, -1), null,
                new Color(255, 100, 0, 0) * pulse, rotation, origin, scale, SpriteEffects.None, 0f);
            // Gold overglow
            spriteBatch.Draw(tex, drawPos + new Vector2(1, 1), null,
                new Color(255, 200, 50, 0) * pulse * 0.5f, rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
