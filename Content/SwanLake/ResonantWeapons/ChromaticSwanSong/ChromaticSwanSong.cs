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
using MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Projectiles;
using MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Utilities;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong
{
    /// <summary>
    /// Chromatic Swan Song — Magic Pistol.
    /// 
    /// COMBAT SYSTEM:
    /// • Fires chromatic bolts that shift through the full rainbow spectrum
    /// • Each bolt leaves a prismatic shader-driven trail
    /// • 3 consecutive hits on the same enemy triggers an "Aria Detonation"
    ///   — a massive chromatic explosion at the target with expanding rainbow rings
    /// • Hitting DIFFERENT enemies builds "Harmonic Stack" — at 5 stacks,
    ///   next Aria Detonation releases all stored energy as rainbow shards in all directions
    /// • Harmonic Notes float upward from the player while Harmonic Stack is building
    /// 
    /// STATS PRESERVED FROM ORIGINAL:
    /// Damage 290, UseTime 12, Mana 8, Knockback 4, Sell 60g, SwanRarity
    /// </summary>
    public class ChromaticSwanSong : ModItem
    {
        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/ChromaticSwanSong/ChromaticSwanSong";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 290;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 8;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 60);
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.UseSound = SoundID.Item43 with { Pitch = 0.4f, Volume = 0.7f };
            Item.autoReuse = true;

            Item.width = 40;
            Item.height = 24;
            Item.shoot = ModContent.ProjectileType<ChromaticBoltProj>();
            Item.shootSpeed = 16f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var csp = player.ChromaticSwan();

            // Advance chromatic scale on each cast
            csp.RegisterCast();
            int scalePos = csp.ChromaticScalePosition;

            // Slight random spread for pistol feel
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(4f));

            // Dying Breath: below 30% HP, bolts travel at double speed
            if (csp.DyingBreathActive)
                velocity *= 2f;

            // ai[0]: 0 = normal, 1 = harmonic-ready, 2 = opus detonation
            float ai0 = 0f;
            if (csp.OpusReady)
            {
                ai0 = 2f;
                csp.ConsumeOpus();
            }
            else if (csp.HarmonicStack >= 5)
            {
                ai0 = 1f;
            }

            // ai[1] = chromatic scale position (0-6)
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback,
                player.whoAmI, ai0: ai0, ai1: scalePos);

            // Muzzle chromatic sparks — color from current scale note
            Color noteColor = ChromaticSwanPlayer.GetScaleColor(scalePos);
            for (int i = 0; i < 4; i++)
            {
                Vector2 dustVel = velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 4f)
                    + Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustPerfect(position, DustID.RainbowTorch, dustVel, 0, noteColor, 0.8f);
                d.noGravity = true;
            }

            // Rainbow muzzle sparkle
            try { SwanLakeVFXLibrary.SpawnPrismaticSparkles(position, 3, 12f); } catch { }

            // Opus ready flash — golden sparkle burst on the 7th cast
            if (ai0 == 2f)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2Circular(6f, 6f);
                    Dust d = Dust.NewDustPerfect(position, DustID.RainbowTorch, sparkVel, 0, Color.White, 1.4f);
                    d.noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 1.0f, Volume = 0.9f }, position);
            }

            return false; // We manually spawned the projectile
        }

        public override void HoldItem(Player player)
        {
            var csp = player.ChromaticSwan();

            // Visual feedback for harmonic stack
            if (csp.HarmonicStack > 0)
            {
                float intensity = Math.Min(csp.HarmonicStack / 5f, 1f);
                float hue = (Main.GameUpdateCount * 0.02f) % 1f;
                Color stackColor = Main.hslToRgb(hue, 0.8f, 0.7f);

                if (Main.rand.NextFloat() < intensity * 0.3f)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                    Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.RainbowTorch,
                        new Vector2(0, -1.2f) + Main.rand.NextVector2Circular(0.5f, 0.5f),
                        0, stackColor, 0.7f * intensity);
                    d.noGravity = true;
                }

                Lighting.AddLight(player.Center, stackColor.ToVector3() * 0.3f * intensity);
            }

            // Chromatic Scale progress — rising notes showing octave position
            if (csp.ConsecutiveCasts > 0)
            {
                float progress = csp.ConsecutiveCasts / 7f;
                if (Main.rand.NextFloat() < progress * 0.4f)
                {
                    Color noteCol = ChromaticSwanPlayer.GetScaleColor(csp.ChromaticScalePosition);
                    Vector2 noteOffset = new Vector2(Main.rand.NextFloat(-16f, 16f), 10f);
                    Dust d = Dust.NewDustPerfect(player.Top + noteOffset, DustID.RainbowTorch,
                        new Vector2(0, -1.5f), 0, noteCol, 0.6f + progress * 0.4f);
                    d.noGravity = true;
                }
            }

            // Dying Breath indicator — black wisps when below 30% HP
            if (csp.DyingBreathActive && Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(16f, 16f);
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.Smoke,
                    new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f)),
                    150, Color.Black, 0.8f);
                d.noGravity = true;
            }

            // Chromatic ambient
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f);
            Color ambient = ChromaticSwanUtils.GetChromatic(0f);
            Lighting.AddLight(player.Center, ambient.ToVector3() * (0.15f + pulse * 0.05f));
        }

        public override Vector2? HoldoutOffset() => new Vector2(-4, 0);

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Fires chromatic bolts that detonate into structured Aria explosions"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Consecutive casts cycle through the chromatic scale (C-D-E-F-G-A-B)"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Completing a full octave (7 casts) triggers an Opus Detonation — all 7 colors at once"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "Below 30% HP: Dying Breath doubles bolt speed and expands detonation radius by 50%"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'The final song is always the most beautiful. It has to be.'")
            {
                OverrideColor = ChromaticSwanUtils.LoreColor
            });
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.07f) * 0.12f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
            Color glow = ChromaticSwanUtils.GetChromatic(0f);

            spriteBatch.Draw(tex, drawPos, null, glow * pulse, rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
