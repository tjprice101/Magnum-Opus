using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Fate.ResonantWeapons.LightOfTheFuture.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.LightOfTheFuture.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.LightOfTheFuture.Projectiles;

namespace MagnumOpus.Content.Fate.ResonantWeapons.LightOfTheFuture
{
    /// <summary>
    /// Light of the Future  -- The Cosmic Railgun.
    /// Fires destiny itself. Each shot starts slow then ACCELERATES to insane speed.
    ///
    /// SELF-CONTAINED WEAPON SYSTEM (no shared VFX libraries):
    ///   - Own particle system (LightParticleHandler)
    ///   - Own GPU trail renderer (LightTrailRenderer)
    ///   - Own shader pipeline (LightShaderLoader ->4 .fx files)
    ///   - Own ModPlayer state (LightPlayer via player.LightOfFuture())
    ///   - Own projectiles (LightAcceleratingBullet, LightCosmicRocket)
    ///
    /// ATTACK PATTERN:
    ///   Normal shots: Accelerating bullets that leave ripple trails, VFX intensifies as combo builds.
    ///   Every 3rd shot: Also fires 3 homing cosmic rockets in a spread that spiral toward targets.
    ///   Muzzle flash is a cosmic burst with star particles and distortion.
    /// </summary>
    public class LightOfTheFutureItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/LightOfTheFuture";

        private static Asset<Texture2D> _glowTex;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 680;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 58;
            Item.height = 22;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item40;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<LightAcceleratingBullet>();
            Item.shootSpeed = 6f; // Starts slow  -- bullet accelerates
            Item.useAmmo = AmmoID.Bullet;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires accelerating cosmic rounds — speed ramps from 6 to 42, damage scales with velocity"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 3rd shot fires 3 homing cosmic rockets in a spread at 1.5x damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "VFX intensifies at speed thresholds: tracers at 30%, sparks at 50%, smoke at 60%"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Cascade: peak-speed kills spawn 2 new full-speed bullets that continue the chain"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "50% chance to not consume ammo"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The fastest light is the one that hasn't arrived yet.'")
            {
                OverrideColor = new Color(180, 40, 80) // Cosmic Crimson
            });
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return Main.rand.NextFloat() > 0.5f; // 50% ammo conservation
        }

        public override void HoldItem(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Spiraling energy motes around barrel
            if (Main.rand.NextBool(5))
            {
                float spiralAngle = time * 0.06f + Main.rand.NextFloat(MathHelper.TwoPi);
                float spiralRadius = 14f + MathF.Sin(time * 0.04f) * 4f;
                Vector2 muzzleBase = center + new Vector2(player.direction * 28f, -6f);
                Vector2 spiralPos = muzzleBase + spiralAngle.ToRotationVector2() * spiralRadius;
                Color spiralCol = LightUtils.BulletGradient(Main.rand.NextFloat(0.3f, 0.8f));
                LightParticleHandler.SpawnParticle(new LightMote(spiralPos,
                    Main.rand.NextVector2Circular(0.5f, 0.5f) + new Vector2(0, -0.3f),
                    spiralCol * 0.45f, 0.13f, 16));
            }

            // Star sparkle motes
            if (Main.rand.NextBool(8))
            {
                Vector2 starPos = center + Main.rand.NextVector2Circular(30f, 30f);
                Color starCol = Main.rand.NextBool(3) ? LightUtils.MuzzleGold : LightUtils.PlasmaWhite;
                LightParticleHandler.SpawnParticle(new LightMote(starPos,
                    Main.rand.NextVector2Circular(0.4f, 0.4f),
                    starCol * 0.35f, 0.11f, 14));
            }

            // Ambient light pulse
            float pulse = 0.2f + MathF.Sin(time * 0.08f) * 0.1f;
            Lighting.AddLight(center, LightUtils.LaserCyan.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var lp = player.LightOfFuture();

            // Always fire the accelerating bullet
            Projectile.NewProjectile(source, position, velocity,
                ModContent.ProjectileType<LightAcceleratingBullet>(),
                damage, knockback, player.whoAmI);

            // Track shot and check for rocket volley
            bool fireRockets = lp.OnShot();

            if (fireRockets)
            {
                // Fire 3 homing rockets in a spread
                float baseAngle = velocity.ToRotation();
                float[] offsets = { -0.25f, 0f, 0.25f }; // ~15 degree spread

                for (int i = 0; i < 3; i++)
                {
                    float rocketAngle = baseAngle + offsets[i];
                    Vector2 rocketVel = rocketAngle.ToRotationVector2() * 8f;
                    Projectile.NewProjectile(source, position, rocketVel,
                        ModContent.ProjectileType<LightCosmicRocket>(),
                        (int)(damage * 1.5f), knockback * 1.5f, player.whoAmI);
                }

                // Enhanced muzzle flash for rocket volley
                if (!Main.dedServ)
                    SpawnRocketMuzzleFlash(position, velocity.SafeNormalize(Vector2.UnitX));

                SoundEngine.PlaySound(SoundID.Item38 with { Pitch = -0.4f, Volume = 0.9f }, position);
            }

            // Standard muzzle flash VFX
            if (!Main.dedServ)
            {
                Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 40f;
                SpawnMuzzleFlashVFX(muzzlePos, velocity.SafeNormalize(Vector2.UnitX), lp.ComboIntensity);
            }

            SoundEngine.PlaySound(SoundID.Item38 with { Pitch = -0.3f }, position);

            return false; // We handle projectile spawning manually
        }

        private void SpawnMuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction, float comboIntensity)
        {
            // Central flash bloom
            LightParticleHandler.SpawnParticle(new LightBloomFlare(muzzlePos, LightUtils.MuzzleGold, 0.5f, 14));
            LightParticleHandler.SpawnParticle(new LightBloomFlare(muzzlePos, LightUtils.PlasmaWhite, 0.3f, 10));

            // Directional sparks  -- tight cone along barrel
            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = direction.RotatedByRandom(0.3f) * Main.rand.NextFloat(4f, 8f);
                Color sparkCol = Color.Lerp(LightUtils.MuzzleGold, LightUtils.LaserCyan, Main.rand.NextFloat());
                LightParticleHandler.SpawnParticle(new LightSpark(muzzlePos, sparkVel,
                    sparkCol * 0.7f, 0.15f, 10));
            }

            // Speed line tracers from muzzle
            for (int i = 0; i < 3; i++)
            {
                Vector2 tracerVel = direction.RotatedByRandom(0.15f) * Main.rand.NextFloat(6f, 10f);
                LightParticleHandler.SpawnParticle(new LightTracer(muzzlePos, tracerVel,
                    LightUtils.LaserCyan * 0.6f, 0.12f, 6));
            }

            // Dust cone
            for (int i = 0; i < 4; i++)
            {
                Vector2 dustVel = direction.RotatedByRandom(0.25f) * Main.rand.NextFloat(3f, 6f);
                Dust d = Dust.NewDustPerfect(muzzlePos, DustID.BlueTorch, dustVel, 0, LightUtils.LaserCyan, 1.0f);
                d.noGravity = true;
            }

            // Glyph at higher combo
            if (comboIntensity > 0.4f)
            {
                LightParticleHandler.SpawnParticle(new LightGlyph(muzzlePos,
                    LightUtils.TrailViolet * 0.5f, 0.18f, 16));
            }

            Lighting.AddLight(muzzlePos, LightUtils.MuzzleGold.ToVector3() * 0.8f);
        }

        private void SpawnRocketMuzzleFlash(Vector2 pos, Vector2 direction)
        {
            // Enhanced flash  -- bigger, with crimson accent
            LightParticleHandler.SpawnParticle(new LightBloomFlare(pos, LightUtils.ImpactCrimson, 0.7f, 18));
            LightParticleHandler.SpawnParticle(new LightBloomFlare(pos, LightUtils.MuzzleGold, 0.6f, 16));
            LightParticleHandler.SpawnParticle(new LightBloomFlare(pos, LightUtils.PlasmaWhite, 0.4f, 12));

            // Wide spark burst
            for (int i = 0; i < 10; i++)
            {
                Vector2 sparkVel = direction.RotatedByRandom(0.6f) * Main.rand.NextFloat(5f, 10f);
                Color sparkCol = Color.Lerp(LightUtils.ImpactCrimson, LightUtils.MuzzleGold, Main.rand.NextFloat());
                LightParticleHandler.SpawnParticle(new LightSpark(pos, sparkVel, sparkCol * 0.8f, 0.2f, 12));
            }

            // Three directional glyphs  -- one for each rocket
            for (int i = 0; i < 3; i++)
            {
                float angle = direction.ToRotation() + (i - 1) * 0.25f;
                Vector2 glyphPos = pos + angle.ToRotationVector2() * 18f;
                LightParticleHandler.SpawnParticle(new LightGlyph(glyphPos,
                    LightUtils.TrailViolet * 0.6f, 0.22f, 20));
            }

            // Smoke puffs
            for (int i = 0; i < 4; i++)
            {
                Vector2 smokeVel = direction.RotatedByRandom(0.5f) * Main.rand.NextFloat(1f, 3f);
                LightParticleHandler.SpawnParticle(new LightSmoke(pos, smokeVel,
                    LightUtils.DeepViolet * 0.35f, 0.2f, 25));
            }

            Lighting.AddLight(pos, LightUtils.ImpactCrimson.ToVector3() * 1.0f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI)
        {
            // Bloom behind the weapon sprite in world
            _glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
            Texture2D tex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height / 2f);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.06f;

            LightUtils.DrawItemBloom(spriteBatch, tex, drawPos, origin, rotation, scale, pulse);

            return true; // Still draw the normal sprite
        }
    }
}
