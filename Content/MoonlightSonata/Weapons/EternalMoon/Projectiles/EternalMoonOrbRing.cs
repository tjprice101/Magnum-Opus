using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX.Sparkle;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Projectiles
{
    /// <summary>
    /// EternalMoonOrbRing -- A coordinator projectile that manages 10 sparkly orbs
    /// expanding outward in a ring from the player. Each orb homes weakly toward
    /// nearby enemies and detonates on contact with a Moonlight Sonata sparkle explosion.
    /// The ring expands steadily, and orbs shimmer with deep purple/ice-blue light.
    /// </summary>
    public class EternalMoonOrbRing : ModProjectile
    {
        // ---- INTERNAL ORB DATA ----
        private struct Orb
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Angle;
            public bool Active;
            public float Scale;
        }

        private Orb[] _orbs = new Orb[10];
        private float _ringRadius = 40f;
        private bool _initialized = false;

        // ---- CACHED TEXTURES (lazy-loaded) ----
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _star4Point;
        private static Asset<Texture2D> _pointBloom;

        private static Texture2D SoftGlow =>
            (_softGlow ??= ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                AssetRequestMode.ImmediateLoad)).Value;

        private static Texture2D Star4Point =>
            (_star4Point ??= ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/Projectiles/4PointStarShiningProjectile",
                AssetRequestMode.ImmediateLoad)).Value;

        private static Texture2D PointBloom =>
            (_pointBloom ??= ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom",
                AssetRequestMode.ImmediateLoad)).Value;

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        public Player Owner => Main.player[Projectile.owner];

        // ---- DEFAULTS ----

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 800;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        // ---- AI ----

        public override void AI()
        {
            Player owner = Owner;
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            Vector2 ownerCenter = owner.MountedCenter;

            // 1. Initialize orbs on first frame
            if (!_initialized)
            {
                _initialized = true;
                float angleStep = MathHelper.TwoPi / 10f;
                for (int i = 0; i < 10; i++)
                {
                    float angle = angleStep * i;
                    _orbs[i] = new Orb
                    {
                        Position = ownerCenter + angle.ToRotationVector2() * _ringRadius,
                        Velocity = Vector2.Zero,
                        Angle = angle,
                        Active = true,
                        Scale = 1f
                    };
                }
            }

            // 2. Expand ring radius
            _ringRadius += 3f;

            // 3. Update each active orb
            bool anyActive = false;
            for (int i = 0; i < 10; i++)
            {
                if (!_orbs[i].Active)
                    continue;

                anyActive = true;

                // Base ring position from angle + expanding radius
                Vector2 ringPos = ownerCenter + _orbs[i].Angle.ToRotationVector2() * _ringRadius;

                // Find nearest NPC within 400px for homing
                float closestDist = 400f;
                NPC target = null;
                for (int n = 0; n < Main.maxNPCs; n++)
                {
                    NPC npc = Main.npc[n];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                        continue;

                    float dist = Vector2.Distance(_orbs[i].Position, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        target = npc;
                    }
                }

                // Nudge velocity toward target
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - _orbs[i].Position).SafeNormalize(Vector2.Zero);
                    _orbs[i].Velocity += toTarget * 0.5f;
                }

                // Limit velocity magnitude to 5f
                if (_orbs[i].Velocity.Length() > 5f)
                    _orbs[i].Velocity = Vector2.Normalize(_orbs[i].Velocity) * 5f;

                // Position = ring base + velocity offset
                _orbs[i].Position = ringPos + _orbs[i].Velocity;

                // Shimmer scale
                _orbs[i].Scale = 0.8f + 0.2f * MathF.Sin(Main.GlobalTimeWrappedHourly * 8f + _orbs[i].Angle);

                // Lighting -- Moonlight Sonata palette: deep purple / blue
                Lighting.AddLight(_orbs[i].Position, 0.4f, 0.3f, 0.7f);

                // Tile collision check -- deactivate orb if it hits solid tiles
                if (Collision.SolidCollision(_orbs[i].Position - new Vector2(6), 12, 12))
                {
                    _orbs[i].Active = false;
                    SpawnOrbExplosion(_orbs[i].Position);
                }
            }

            // 4. If all orbs inactive, kill projectile
            if (!anyActive)
            {
                Projectile.Kill();
                return;
            }

            // 5. Center projectile at owner for draw purposes
            Projectile.Center = ownerCenter;
        }

        // ---- COLLISION ----

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Check each active orb against the target hitbox
            for (int i = 0; i < 10; i++)
            {
                if (!_orbs[i].Active)
                    continue;

                // Create a small 16x16 hitbox centered on the orb
                Rectangle orbRect = new Rectangle(
                    (int)(_orbs[i].Position.X - 8f),
                    (int)(_orbs[i].Position.Y - 8f),
                    16, 16);

                if (orbRect.Intersects(targetHitbox))
                    return true;
            }

            return false;
        }

        // ---- HIT HANDLING ----

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Find the closest active orb to the target and consume it
            float closestDist = float.MaxValue;
            int closestIdx = -1;

            for (int i = 0; i < 10; i++)
            {
                if (!_orbs[i].Active)
                    continue;

                float dist = Vector2.Distance(_orbs[i].Position, target.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestIdx = i;
                }
            }

            if (closestIdx >= 0)
            {
                Vector2 orbPos = _orbs[closestIdx].Position;
                _orbs[closestIdx].Active = false;

                SpawnOrbExplosion(orbPos);
                SoundEngine.PlaySound(SoundID.Item125 with { Pitch = 0.3f, Volume = 0.6f }, orbPos);
            }
        }

        /// <summary>
        /// Spawns the Moonlight Sonata sparkle explosion at the orb's position.
        /// </summary>
        private void SpawnOrbExplosion(Vector2 position)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                ThemeSparkleExplosion.Spawn(
                    Projectile.GetSource_FromThis(),
                    position,
                    SparkleTheme.MoonlightSonata,
                    0.8f);
            }
        }

        // ---- RENDERING ----

        public override bool PreDraw(ref Color lightColor)
        {
            if (!_initialized)
                return false;

            SpriteBatch sb = Main.spriteBatch;

            Texture2D softGlow = SoftGlow;
            Texture2D star = Star4Point;
            Texture2D bloom = PointBloom;

            Vector2 glowOrigin = softGlow.Size() / 2f;
            Vector2 starOrigin = star.Size() / 2f;
            Vector2 bloomOrigin = bloom.Size() / 2f;

            float time = Main.GlobalTimeWrappedHourly;

            try
            {
                // Switch to additive blending for glow rendering
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null,
                    Main.GameViewMatrix.TransformationMatrix);

                for (int i = 0; i < 10; i++)
                {
                    if (!_orbs[i].Active)
                        continue;

                    Vector2 drawPos = _orbs[i].Position - Main.screenPosition;
                    float orbScale = _orbs[i].Scale;

                    // Shimmer interpolation for color shifting
                    float shimmerT = 0.5f + 0.5f * MathF.Sin(time * 5f + _orbs[i].Angle * 2f);

                    // Layer 1: Outer glow -- soft purple haze
                    sb.Draw(softGlow, drawPos, null,
                        new Color(170, 140, 255) * 0.3f,
                        0f, glowOrigin, 0.15f * orbScale, SpriteEffects.None, 0f);

                    // Layer 2: Star body -- ice-blue to white shimmer, rotating
                    Color starColor = Color.Lerp(
                        new Color(135, 206, 250),
                        new Color(230, 235, 255),
                        shimmerT);

                    float rotation = time * 3f + _orbs[i].Angle;
                    sb.Draw(star, drawPos, null,
                        starColor,
                        rotation, starOrigin, 0.06f * orbScale, SpriteEffects.None, 0f);

                    // Layer 3: Hot core -- small bright center
                    sb.Draw(bloom, drawPos, null,
                        Color.White * 0.6f,
                        0f, bloomOrigin, 0.03f * orbScale, SpriteEffects.None, 0f);
                }

                // Restore default SpriteBatch state
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }
            catch
            {
                // Safety: restore SpriteBatch to default state on any error
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }

        public override Color? GetAlpha(Color lightColor) => Color.Transparent;
    }
}
