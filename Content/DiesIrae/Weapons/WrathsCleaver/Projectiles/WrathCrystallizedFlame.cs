using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Buffs;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Primitives;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Projectiles
{
    /// <summary>
    /// Crystallized Flame — homing fire crystal spawned every 3rd swing.
    /// Orbits briefly in a spiral pattern, then aggressively homes to nearest enemy.
    /// Explodes on impact with ember cascade + wrath note burst.
    /// </summary>
    public class WrathCrystallizedFlame : ModProjectile
    {
        private enum FlameState { Spiral, Homing, Detonating }

        private FlameState State
        {
            get => (FlameState)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }

        private ref float Timer => ref Projectile.ai[1];
        private float spiralAngle;
        private readonly List<Vector2> trail = new List<Vector2>();

        public override string Texture => "MagnumOpus/Content/DiesIrae/ResonantWeapons/WrathsCleaver";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Timer++;

            switch (State)
            {
                case FlameState.Spiral:
                    SpiralPhase();
                    break;
                case FlameState.Homing:
                    HomingPhase();
                    break;
            }

            // Trail tracking
            trail.Add(Projectile.Center);
            if (trail.Count > 16) trail.RemoveAt(0);

            // Particle emission
            if (Main.rand.NextBool(2))
            {
                var ember = new InfernalEmber(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    -Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    WrathsCleaverUtils.PaletteLerp(Main.rand.NextFloat(0.4f, 0.9f)),
                    Main.rand.NextFloat(0.15f, 0.3f), Main.rand.Next(10, 18));
                WrathParticleHandler.SpawnParticle(ember);
            }

            Projectile.rotation += 0.2f;
            Lighting.AddLight(Projectile.Center, 0.6f, 0.2f, 0.05f);
        }

        private void SpiralPhase()
        {
            spiralAngle += 0.15f;
            float expandRadius = Timer * 0.5f;
            Vector2 spiralOffset = new Vector2((float)Math.Cos(spiralAngle), (float)Math.Sin(spiralAngle)) * expandRadius;
            Projectile.velocity = spiralOffset * 0.3f + Projectile.velocity * 0.7f;

            if (Timer > 25)
            {
                State = FlameState.Homing;
                Timer = 0;
            }
        }

        private void HomingPhase()
        {
            NPC target = WrathsCleaverUtils.ClosestNPCAt(Projectile.Center, 800f, true);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                float turnSpeed = 0.15f + Timer * 0.005f; // Gets more aggressive over time
                Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.UnitX), toTarget, turnSpeed)
                    * Math.Min(Projectile.velocity.Length() + 0.3f, 18f);
            }
            else
            {
                // No target, just fly forward
                Projectile.velocity *= 0.98f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<HellfireImmolation>(), 180);
            target.AddBuff(BuffID.OnFire3, 240);

            // Explosion VFX
            var bigBloom = new WrathBloom(target.Center, WrathsCleaverUtils.InfernalRed, 2f, 15);
            WrathParticleHandler.SpawnParticle(bigBloom);

            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.TwoPi * i / 15f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 8f);
                var spark = new CrystallizedFlameSpark(target.Center, vel,
                    WrathsCleaverUtils.PaletteLerp(Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.2f, 0.5f), Main.rand.Next(12, 25));
                WrathParticleHandler.SpawnParticle(spark);
            }

            for (int i = 0; i < 4; i++)
            {
                var note = new HellfireNote(target.Center, Main.rand.NextVector2Circular(3f, 3f),
                    WrathsCleaverUtils.HellfireGold, Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(20, 40));
                WrathParticleHandler.SpawnParticle(note);
            }

            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 0.5f, Pitch = 0.3f }, target.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // Draw trail
            if (trail.Count >= 3)
            {
                sb.End();
                var settings = new WrathTrailSettings(
                    widthFunc: t => 12f * (float)Math.Sin(t * MathHelper.Pi),
                    colorFunc: t =>
                    {
                        Color c = WrathsCleaverUtils.MulticolorLerp(t,
                            WrathsCleaverUtils.HellfireGold, WrathsCleaverUtils.InfernalRed, WrathsCleaverUtils.BloodRed);
                        return WrathsCleaverUtils.Additive(c, (1f - t) * 0.8f);
                    },
                    smoothingSteps: 6
                );
                WrathTrailRenderer.RenderTrail(trail, settings);
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            // Draw orb
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D tex = TextureAssets.MagicPixel.Value;
            float pulse = 1f + (float)Math.Sin(Timer * 0.2f) * 0.15f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Core glow
            sb.Draw(tex, drawPos, new Rectangle(0, 0, 1, 1), WrathsCleaverUtils.Additive(WrathsCleaverUtils.HellfireGold, 0.9f),
                0f, new Vector2(0.5f), 14f * pulse, SpriteEffects.None, 0f);
            // Inner red
            sb.Draw(tex, drawPos, new Rectangle(0, 0, 1, 1), WrathsCleaverUtils.Additive(WrathsCleaverUtils.InfernalRed, 0.6f),
                0f, new Vector2(0.5f), 22f * pulse, SpriteEffects.None, 0f);
            // Outer bloom
            sb.Draw(tex, drawPos, new Rectangle(0, 0, 1, 1), WrathsCleaverUtils.Additive(WrathsCleaverUtils.BloodRed, 0.3f),
                0f, new Vector2(0.5f), 35f * pulse, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
