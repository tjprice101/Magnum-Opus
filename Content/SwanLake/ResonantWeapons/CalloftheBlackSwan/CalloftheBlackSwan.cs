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
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Projectiles;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Utilities;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan
{
    /// <summary>
    /// Call of the Black Swan — Melee Greatsword.
    /// Exoblade-inspired self-contained weapon with channel-held swing projectile.
    /// 
    /// COMBAT SYSTEM:
    /// • Left-click: 3-phase ballet combo (Plié → Arabesque → Grand Jeté)
    /// • Phase 2 (Grand Jeté) fires 3 homing Black Swan Flares
    /// • Landing 3 flares triggers empowerment (next Phase 2 fires 8 flares at 2× damage)
    /// • Each phase has unique CurveSegment animation with per-phase blade length and damage
    /// 
    /// STATS PRESERVED FROM ORIGINAL:
    /// Damage 400, UseTime 28, Knockback 7, Sell 60g, SwanRarity
    /// </summary>
    public class CalloftheBlackSwan : ModItem
    {
        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/CalloftheBlackSwan/CalloftheBlackSwan";

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
            Item.damage = 400;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(gold: 60);
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.UseSound = SoundID.Item29 with { Pitch = -0.1f, Volume = 0.85f };

            Item.width = 80;
            Item.height = 80;
            Item.shoot = ModContent.ProjectileType<BlackSwanSwingProj>();
            Item.shootSpeed = 1f;
        }

        public override bool CanShoot(Player player)
        {
            // Prevent overlapping swing projectiles (Exoblade pattern)
            int swingType = ModContent.ProjectileType<BlackSwanSwingProj>();
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

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity,
            ref int type, ref int damage, ref float knockback)
        {
            position = player.MountedCenter;
            velocity = player.MountedCenter.SafeDirectionTo(Main.MouseWorld);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var bsp = player.BlackSwan();
            int comboStep = bsp.ComboStep;

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback,
                player.whoAmI, ai0: comboStep);

            bsp.AdvanceCombo();
            return false;
        }

        public override void HoldItem(Player player)
        {
            var bsp = player.BlackSwan();

            // Empowered visual feedback — self-contained particles only
            if (bsp.IsEmpowered)
            {
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f);

                // Gentle dual-polarity dust rising
                if (Main.rand.NextBool(5))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                    bool isBlack = Main.rand.NextBool();
                    int dustType = isBlack ? DustID.Shadowflame : DustID.WhiteTorch;
                    Color col = isBlack ? Color.Black : Color.White;
                    Dust d = Dust.NewDustPerfect(player.Center + offset, dustType,
                        new Vector2(0, -1.5f) + Main.rand.NextVector2Circular(0.8f, 0.8f), 0, col, 1.3f);
                    d.noGravity = true;
                    d.fadeIn = 1.1f;
                }

                // Pulsing rainbow light
                float hue = (Main.GameUpdateCount * 0.02f) % 1f;
                Vector3 rainbowLight = Main.hslToRgb(hue, 0.8f, 0.6f).ToVector3();
                Lighting.AddLight(player.Center, (0.6f + pulse * 0.2f) * rainbowLight);
            }
            else
            {
                // Subtle monochrome ambient
                if (Main.rand.NextBool(12))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                    bool isBlack = Main.rand.NextBool();
                    int dustType = isBlack ? DustID.Shadowflame : DustID.WhiteTorch;
                    Dust d = Dust.NewDustPerfect(player.Center + offset, dustType,
                        new Vector2(0, -0.8f) + Main.rand.NextVector2Circular(0.5f, 0.5f), 0,
                        isBlack ? new Color(40, 40, 50) : new Color(220, 220, 230), 0.8f);
                    d.noGravity = true;
                }

                Lighting.AddLight(player.Center, new Vector3(0.3f, 0.3f, 0.35f));
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Swings send black and white flares that track enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Landing 3 flares empowers the next swing with devastating force"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Where black meets white, the swan takes flight'")
            {
                OverrideColor = BlackSwanUtils.LoreColor
            });
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            float rotation, float scale, int whoAmI)
        {
            // Simple dual-polarity glow on ground item
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);

            // Black underglow
            spriteBatch.Draw(tex, drawPos + new Vector2(-1, -1), null,
                new Color(15, 15, 25, 0) * pulse, rotation, origin, scale, SpriteEffects.None, 0f);
            // White overglow
            spriteBatch.Draw(tex, drawPos + new Vector2(1, 1), null,
                new Color(255, 255, 255, 0) * pulse, rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
