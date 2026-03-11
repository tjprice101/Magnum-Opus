using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.ClairDeLune.Weapons.MidnightMechanism.Projectiles;
using MagnumOpus.Content.ClairDeLune.Weapons.MidnightMechanism.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.MidnightMechanism
{
    /// <summary>
    /// Midnight Mechanism — Ranged gatling gun with 5-phase spin-up,
    /// tick mark accumulation → Midnight Strike, Gear Jam, and Mechanism Eject.
    /// "The clock does not care if you are ready. Midnight comes regardless."
    /// </summary>
    public class MidnightMechanism : ModItem
    {
        private int _fireTimer;

        public override void SetDefaults()
        {
            Item.width = 72;
            Item.height = 36;
            Item.damage = 2900; // Tier 10 (2800-4200 range)
            Item.DamageType = DamageClass.Ranged;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<MechanismBulletProjectile>();
            Item.shootSpeed = 22f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 16;
            Item.channel = true;
        }

        public override bool CanUseItem(Player player)
        {
            var mp = player.GetModPlayer<MidnightMechanismPlayer>();
            if (mp.IsJammed) return false;
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var mp = player.GetModPlayer<MidnightMechanismPlayer>();
            mp.AdvanceFire();

            // Phase-based fire rate
            int delay = mp.GetFireDelay();
            _fireTimer++;
            if (_fireTimer % delay != 0) return false;

            // Check for Midnight Strike
            if (mp.MidnightReady)
            {
                // Fire Midnight Strike instead
                Projectile.NewProjectile(source, position, velocity * 0.8f,
                    ModContent.ProjectileType<MidnightStrikeShotProjectile>(),
                    damage * 10, knockback * 3f, player.whoAmI);
                mp.ConsumeMidnight();

                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f, Volume = 1.2f }, position);

                // Screen flash
                var flash = new BloomParticle(player.Center, Vector2.Zero,
                    ClairDeLunePalette.PearlWhite with { A = 0 } * 0.6f, 1.5f, 15);
                MagnumParticleHandler.SpawnParticle(flash);

                return false;
            }

            // Accuracy spread per phase
            float spread = mp.CurrentPhase switch
            {
                1 => 0.01f,
                2 => 0.02f,
                3 => 0.04f,
                4 => 0.06f,
                5 => 0.08f,
                _ => 0.01f
            };
            Vector2 perturbedVel = velocity.RotatedByRandom(spread);

            // Fire mechanism bullet
            int proj = Projectile.NewProjectile(source, position, perturbedVel,
                ModContent.ProjectileType<MechanismBulletProjectile>(),
                damage, knockback, player.whoAmI, mp.CurrentPhase);

            // Muzzle flash VFX
            float flashScale = mp.GetMuzzleFlashScale();
            Color flashCol = Color.Lerp(ClairDeLunePalette.MoonbeamGold, ClairDeLunePalette.PearlWhite, (mp.CurrentPhase - 1) / 4f);
            var muzzle = new BloomParticle(position, Vector2.Zero,
                flashCol with { A = 0 } * 0.6f, flashScale, 4);
            MagnumParticleHandler.SpawnParticle(muzzle);

            // Phase 3+ screen shake
            float shake = mp.GetScreenShake();
            if (shake > 0f)
            {
                player.velocity += -velocity.SafeNormalize(Vector2.Zero) * shake * 0.1f;
            }

            return false;
        }

        public override void HoldItem(Player player)
        {
            var mp = player.GetModPlayer<MidnightMechanismPlayer>();

            // Jam VFX — sparks and smoke during jam
            if (mp.IsJammed && mp.JamCooldown > 45)
            {
                // Eject gears on first jam frame
                if (mp.JamCooldown == 59)
                {
                    SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f, Volume = 0.6f }, player.Center);

                    // Spawn Mechanism Eject gears
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f + Main.rand.NextFloat(-0.3f, 0.3f);
                        Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                        Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, vel,
                            ModContent.ProjectileType<MechanismEjectGearProjectile>(),
                            (int)(Item.damage * 0.5f), 2f, player.whoAmI);
                    }
                }

                // Smoke particles
                if (Main.rand.NextBool(3))
                {
                    var smoke = new GenericGlowParticle(
                        player.Center + Main.rand.NextVector2Circular(10f, 10f),
                        new Vector2(0, -1f) + Main.rand.NextVector2Circular(0.5f, 0.5f),
                        ClairDeLunePalette.NightMist with { A = 0 } * 0.3f, 0.15f, 15, true);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
            }

            // Tick mark glow (visual indicator)
            if (mp.TickMarks > 0 && Main.rand.NextBool(20))
            {
                float tickProgress = mp.TickMarks / 12f;
                Color tickCol = Color.Lerp(ClairDeLunePalette.NightMist, ClairDeLunePalette.MoonbeamGold, tickProgress);
                var tickGlow = new GenericGlowParticle(
                    player.Center + Main.rand.NextVector2Circular(16f, 16f),
                    Vector2.Zero,
                    tickCol with { A = 0 } * 0.2f, 0.06f, 12, true);
                MagnumParticleHandler.SpawnParticle(tickGlow);
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "5-phase gatling spin-up accelerates from 3 to 24 shots per second"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 50 hits accumulates a tick mark — 12 tick marks triggers Midnight Strike at 10x damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Stopping fire at Phase 3+ causes a Gear Jam, ejecting shrapnel"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The clock does not care if you are ready. Midnight comes regardless.'")
            {
                OverrideColor = ClairDeLunePalette.LoreText
            });
        }
    
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2.2f) * 0.05f
                + (float)Math.Sin(time * 3.8f) * 0.03f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            ClairDeLunePalette.DrawItemBloom(spriteBatch, tex, pos, origin, rotation, scale, pulse);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, ClairDeLunePalette.SoftBlue.ToVector3() * 0.35f);
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            float time = Main.GameUpdateCount * 0.04f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.06f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            float cycle = (float)Math.Sin(time * 0.7f) * 0.5f + 0.5f;
            Color glowColor = Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlWhite, cycle) * 0.24f;
            spriteBatch.Draw(tex, position, frame, glowColor with { A = 0 }, 0f, origin, scale * pulse * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
