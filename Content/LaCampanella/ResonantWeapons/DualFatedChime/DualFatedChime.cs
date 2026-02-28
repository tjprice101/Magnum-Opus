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
    /// Dual Fated Chime — Melee Zenith-style Greatsword.
    /// Exoblade-inspired channel-held swing with 3-phase combo.
    /// 
    /// COMBAT SYSTEM:
    /// • Left-click: 3-phase infernal combo (BellStrike → TollSweep → GrandToll)
    /// • Each phase escalates in power, blade length, and VFX intensity
    /// • Hits charge the Inferno Waltz gauge (8 charge per hit, 100 max)
    /// • Right-click at full charge: Inferno Waltz — devastating spinning flame dance
    /// • Grand Toll (phase 3) fires 3 homing Bell Flame Waves from blade tip
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
            // Exoblade pattern: channel-held with invisible item sprite
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.autoReuse = true;

            // Preserved stats
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
            // Prevent overlapping swing projectiles
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

        public override bool AltFunctionUse(Player player)
        {
            return player.DualFatedChime().IsWaltzReady;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Inferno Waltz mode — slower wind-up
                Item.useTime = 60;
                Item.useAnimation = 60;
            }
            else
            {
                Item.useTime = 16;
                Item.useAnimation = 16;
            }
            return true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity,
            ref int type, ref int damage, ref float knockback)
        {
            position = player.MountedCenter;
            velocity = player.MountedCenter.SafeDirectionTo(Main.MouseWorld);

            if (player.altFunctionUse == 2)
            {
                // Spawn Inferno Waltz instead of swing
                type = ModContent.ProjectileType<InfernoWaltzProj>();
                damage = (int)(damage * 2f);
                knockback = 10f;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var dfc = player.DualFatedChime();

            if (player.altFunctionUse == 2)
            {
                // Inferno Waltz — consume charge, spawn waltz projectile
                dfc.ConsumeCharge();
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                return false;
            }

            // Normal swing — pass combo step
            int comboStep = dfc.ComboStep;
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback,
                player.whoAmI, ai0: comboStep);

            dfc.AdvanceCombo();
            return false;
        }

        public override void HoldItem(Player player)
        {
            var dfc = player.DualFatedChime();

            // Charge-level visual feedback
            float chargeRatio = dfc.ChargeBar / DualFatedChimePlayer.MaxCharge;

            if (dfc.IsWaltzReady)
            {
                // Full charge — intense pulsing flames
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f);

                if (Main.rand.NextBool(3))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                    int dustType = Main.rand.NextBool() ? DustID.Torch : DustID.Smoke;
                    Color col = DualFatedChimeUtils.GetFireFlicker(Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(player.Center + offset, dustType,
                        new Vector2(0, -2f) + Main.rand.NextVector2Circular(1f, 1f), 0, col, 1.4f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }

                Vector3 fireLight = new Vector3(0.8f, 0.4f, 0.1f) * (0.7f + pulse * 0.3f);
                Lighting.AddLight(player.Center, fireLight);
            }
            else if (chargeRatio > 0.1f)
            {
                // Partial charge — subtle ember glow
                if (Main.rand.NextBool(8))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(15f, 15f);
                    Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.Torch,
                        new Vector2(0, -1f) + Main.rand.NextVector2Circular(0.5f, 0.5f), 0,
                        new Color(255, 140, 40), 0.7f * chargeRatio);
                    d.noGravity = true;
                }

                Lighting.AddLight(player.Center, new Vector3(0.4f, 0.2f, 0.05f) * chargeRatio);
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Swings cast spectral blades wreathed in bell-music flames"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Attacks fill a charge bar — right-click to unleash Inferno Waltz"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Inferno Waltz creates a devastating spinning flame dance"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Applies Resonant Toll on hit"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Two fates intertwined in the dance of the eternal chime'")
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
