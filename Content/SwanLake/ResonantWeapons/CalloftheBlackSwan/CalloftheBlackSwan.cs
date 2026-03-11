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
using MagnumOpus.Content.SandboxExoblade.Utilities;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan
{
    /// <summary>
    /// Call of the Black Swan — Melee Greatsword (OVERHAUL).
    /// The Black Swan is Odile — fierce elegance, dark ballet of destruction.
    /// 
    /// COMBAT SYSTEM (OVERHAUL):
    /// • 3-Phase Dance Combo: Entrechat → Fouetté → Grand Jeté
    ///   - Phase 1 (Entrechat): Quick diagonal slash, spawns 3 black feather projectiles in fan arc
    ///   - Phase 2 (Fouetté): Spinning horizontal slash, spawns BlackSwanFlare radial AoE
    ///   - Phase 3 (Grand Jeté): Leaping overhead slam + swan silhouette shockwave + 5 feather rain
    /// • Swan's Grace: Successive hits without getting hit build Grace stacks (max 5)
    ///   - Each stack: +8% swing speed, trail becomes more prismatic
    ///   - At max Grace: next swing releases Prismatic Swan charge projectile
    /// • Black Mirror: Taking damage while swinging converts Grace → Dark Mirror stacks
    ///   - Dark Mirror: +15% damage but -5% speed per stack
    /// • Swan's Mark debuff: -10 defense on marked enemies
    /// 
    /// STATS: Damage 400, UseTime 28, Knockback 7, Sell 60g, SwanRarity
    /// </summary>
    public class CalloftheBlackSwan : ModItem
    {
        /// <summary>Tracks the Swan Dance combo phase (0-2). Entrechat → Fouetté → Grand Jeté.</summary>
        private int dancePhase = 0;

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
            bool isDash = player.altFunctionUse == 2;
            int swingType = ModContent.ProjectileType<BlackSwanSwingProj>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != player.whoAmI || p.type != swingType)
                    continue;
                if (isDash) return false;
                if (!(p.ai[0] == 1 && p.ai[1] == 1)) return false;
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

        public override bool AltFunctionUse(Player player) => true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float state = player.altFunctionUse == 2 ? 1f : 0f;
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, state, 0);

            // --- Swan Dance combo system ---
            // Phase 0 (Entrechat): 3 feather flares in fan arc (alternating white/black)
            // Phase 1 (Fouetté): 4 flares in spinning radial burst
            // Phase 2 (Grand Jeté): 5 empowered flares raining down + 2 shockwave flares
            Vector2 aimDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            int phase = dancePhase % 3;
            dancePhase++;
            int flareType = ModContent.ProjectileType<BlackSwanFlareProj>();
            int flareDmg = (int)(damage * 0.3f);

            var bsp = player.BlackSwan();
            bool maxGrace = bsp.IsMaxGrace;

            switch (phase)
            {
                case 0: // Entrechat — 3 feathers in fan arc
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 flareVel = aimDir.RotatedBy(MathHelper.ToRadians(20 * i)) * 10f;
                        float polarity = (i + 1) % 2; // Alternating white(0)/black(1)
                        Projectile.NewProjectile(source, player.MountedCenter, flareVel,
                            flareType, flareDmg, knockback * 0.4f, player.whoAmI,
                            maxGrace ? 1f : 0f, polarity);
                    }
                    break;

                case 1: // Fouetté — 4 flares in spinning radial burst
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = MathHelper.PiOver2 * i + Projectile.GetSource_None().GetHashCode() * 0.01f;
                        Vector2 flareVel = aimDir.RotatedBy(MathHelper.ToRadians(90 * i - 135)) * 9f;
                        Projectile.NewProjectile(source, player.MountedCenter, flareVel,
                            flareType, flareDmg, knockback * 0.4f, player.whoAmI,
                            maxGrace ? 1f : 0f, i % 2);
                    }
                    break;

                case 2: // Grand Jeté — 5 empowered raining flares + 2 shockwave seeds
                    for (int i = 0; i < 5; i++)
                    {
                        float spread = MathHelper.ToRadians(-40 + 20 * i);
                        Vector2 flareVel = aimDir.RotatedBy(spread) * 11f;
                        Projectile.NewProjectile(source, player.MountedCenter, flareVel,
                            flareType, (int)(damage * 0.4f), knockback * 0.6f, player.whoAmI,
                            1f, i % 2); // All empowered
                    }
                    // 2 shockwave seeds (mode 2) flanking
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 shockVel = aimDir.RotatedBy(MathHelper.ToRadians(50 * i)) * 6f;
                        Projectile.NewProjectile(source, player.MountedCenter, shockVel,
                            flareType, (int)(damage * 0.5f), knockback, player.whoAmI,
                            2f, 0f);
                    }
                    break;
            }

            // Rainbow sparkle burst at player on each swing
            try { SwanLakeVFXLibrary.SpawnPrismaticSparkles(player.MountedCenter, 4, 20f); } catch { }

            return false;
        }

        public override float UseSpeedMultiplier(Player player)
        {
            var bsp = player.BlackSwan();
            return bsp.GetGraceSpeedMultiplier();
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            var bsp = player.BlackSwan();
            if (bsp.DarkMirrorStacks > 0)
                damage *= bsp.GetDarkMirrorDamageMultiplier();
        }

        public override void HoldItem(Player player)
        {
            player.ExoBlade().rightClickListener = true;
            player.ExoBlade().mouseWorldListener = true;

            var bsp = player.BlackSwan();

            // Grace stack visual feedback
            if (bsp.GraceStacks > 0)
            {
                float graceIntensity = bsp.PrismaticIntensity;
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f);

                // Grace glint particles orbiting at arm's reach — 1 per stack
                for (int i = 0; i < bsp.GraceStacks; i++)
                {
                    float angle = Main.GameUpdateCount * 0.03f + MathHelper.TwoPi * i / bsp.GraceStacks;
                    Vector2 orbitPos = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 30f;

                    if (Main.rand.NextBool(8))
                    {
                        Color sparkCol = Color.Lerp(Color.White, Main.hslToRgb((Main.GameUpdateCount * 0.01f + (float)i / 5f) % 1f, 0.8f, 0.8f), graceIntensity);
                        Dust d = Dust.NewDustPerfect(orbitPos, DustID.WhiteTorch,
                            Main.rand.NextVector2Circular(0.3f, 0.3f), 0, sparkCol, 0.5f + graceIntensity * 0.3f);
                        d.noGravity = true;
                    }
                }

                // Max Grace — prismatic aura ready
                if (bsp.IsMaxGrace)
                {
                    float hue = (Main.GameUpdateCount * 0.02f) % 1f;
                    Vector3 rainbowLight = Main.hslToRgb(hue, 0.85f, 0.7f).ToVector3();
                    Lighting.AddLight(player.Center, (0.6f + pulse * 0.2f) * rainbowLight);

                    if (Main.rand.NextBool(4))
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                        Color prismatic = Main.hslToRgb(Main.rand.NextFloat(), 0.9f, 0.8f);
                        Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.RainbowTorch,
                            new Vector2(0, -1.5f) + Main.rand.NextVector2Circular(0.8f, 0.8f), 0, prismatic, 1.1f);
                        d.noGravity = true;
                    }
                }
                else
                {
                    // Gentle white glow that strengthens with stacks
                    Lighting.AddLight(player.Center, new Vector3(0.3f + graceIntensity * 0.3f, 0.3f + graceIntensity * 0.3f, 0.35f + graceIntensity * 0.25f));
                }
            }

            // Dark Mirror visual feedback — ominous dark aura
            if (bsp.DarkMirrorStacks > 0)
            {
                float mirrorIntensity = (float)bsp.DarkMirrorStacks / BlackSwanPlayer.MaxStacks;
                if (Main.rand.NextBool(6))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                    Color darkCol = Color.Lerp(new Color(30, 30, 45), new Color(60, 10, 80), mirrorIntensity);
                    Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.Shadowflame,
                        new Vector2(0, -1f) + Main.rand.NextVector2Circular(0.5f, 0.5f), 100, darkCol, 1.0f + mirrorIntensity * 0.5f);
                    d.noGravity = true;
                }

                Lighting.AddLight(player.Center, new Vector3(0.15f * mirrorIntensity, 0.05f * mirrorIntensity, 0.25f * mirrorIntensity));
            }

            // Base ambient glow if no stacks
            if (bsp.GraceStacks == 0 && bsp.DarkMirrorStacks == 0)
            {
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Three-phase ballet combo: Entrechat, Fouetté, and Grand Jeté"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Successive hits build Swan's Grace, each stack increases swing speed by 8%"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At maximum Grace, the next swing unleashes the Prismatic Swan"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Taking damage while swinging converts Grace to Dark Mirror: +15% damage, -5% speed per stack"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'She danced not for love, but for the ruin of those who watch.'")
            {
                OverrideColor = new Color(240, 240, 255)
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
