using System;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Projectiles
{
    /// <summary>
    /// Ghost Reflection — a spectral echo blade that orbits above the player's head.
    /// Spawned at Half Moon phase (phase 2+), appearing as a spinning phantom blade
    /// circling above the player in a tilted orbit. Deals reduced damage and has ethereal VFX.
    /// </summary>
    public class EternalMoonGhost : ModProjectile
    {
        private const float BladeLength = 120f;
        private const int GhostLifetime = 55;
        private const float OrbitRadius = 60f;
        private const float OrbitHeightOffset = -70f; // Above player's head
        private const float OrbitSpeed = MathHelper.TwoPi * 1.8f; // Radians per second equivalent

        public Player Owner => Main.player[Projectile.owner];
        public int LunarPhase => (int)Projectile.ai[0];
        public int GhostSide => (int)Projectile.ai[1]; // -1 or +1, determines initial orbit angle offset

        private float _orbitAngle;
        private float _lifeProgress;

        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Weapons/EternalMoon/EternalMoon";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 70;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = GhostLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.Opacity = 0.35f;
        }

        /// <summary>Gets the current orbit center (above the player's head).</summary>
        private Vector2 OrbitCenter => Owner.MountedCenter + new Vector2(0f, OrbitHeightOffset);

        /// <summary>Gets the current blade tip position based on orbit angle.</summary>
        private Vector2 GetBladeTipPosition()
        {
            // Tilted elliptical orbit: wider horizontally, compressed vertically for perspective
            Vector2 orbitOffset = new Vector2(
                (float)Math.Cos(_orbitAngle) * OrbitRadius,
                (float)Math.Sin(_orbitAngle) * OrbitRadius * 0.4f); // Flatten Y for perspective tilt
            return OrbitCenter + orbitOffset;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 bladeCenter = GetBladeTipPosition();
            Vector2 bladeDir = _orbitAngle.ToRotationVector2();
            Vector2 start = bladeCenter - bladeDir * BladeLength * 0.4f * Projectile.scale;
            Vector2 end = bladeCenter + bladeDir * BladeLength * 0.6f * Projectile.scale;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 20f, ref _);
        }

        public override void AI()
        {
            _lifeProgress = 1f - (Projectile.timeLeft / (float)GhostLifetime);

            // Initialize orbit angle on first frame
            if (_lifeProgress < 0.02f)
                _orbitAngle = GhostSide * MathHelper.Pi; // Start on opposite sides if 2 ghosts

            // Spin the orbit
            float speedMult = 1f + LunarPhase * 0.15f; // Faster at higher phases
            _orbitAngle += OrbitSpeed / 60f * speedMult; // Per-tick increment

            // Anchor to owner (orbit center follows player)
            Projectile.Center = GetBladeTipPosition();

            // Ethereal lunar particles from ghost blade
            if (!Main.dedServ)
            {
                Vector2 tipPos = GetBladeTipPosition();

                // Tidal mote trail along orbit path
                if (Main.rand.NextBool(3))
                {
                    Vector2 tangent = new Vector2(-(float)Math.Sin(_orbitAngle), (float)Math.Cos(_orbitAngle) * 0.4f);
                    Vector2 moteVel = tangent * Main.rand.NextFloat(1f, 2.5f);
                    Color moteColor = Color.Lerp(EternalMoonUtils.Violet, EternalMoonUtils.IceBlue, Main.rand.NextFloat()) * 0.6f;
                    LunarParticleHandler.SpawnParticle(new TidalMoteParticle(
                        tipPos + Main.rand.NextVector2Circular(8f, 8f), moteVel,
                        Main.rand.NextFloat(0.2f, 0.4f), moteColor, Main.rand.Next(15, 30)));
                }

                // Moon glint sparkles
                if (Main.rand.NextBool(7))
                {
                    LunarParticleHandler.SpawnParticle(new MoonGlintParticle(
                        tipPos + Main.rand.NextVector2Circular(5f, 5f),
                        Main.rand.NextFloat(0.15f, 0.3f),
                        EternalMoonUtils.MoonWhite * 0.5f, Main.rand.Next(10, 18)));
                }

                // Subtle tidal droplets falling from ghost blade
                if (Main.rand.NextBool(8))
                {
                    Vector2 dropVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(0.2f, 0.8f));
                    LunarParticleHandler.SpawnParticle(new TidalDropletParticle(
                        tipPos, dropVel, Main.rand.NextFloat(0.15f, 0.3f),
                        EternalMoonUtils.IceBlue * 0.4f, Main.rand.Next(15, 25)));
                }
            }

            // Fade in then out
            Projectile.Opacity = _lifeProgress < 0.15f ? _lifeProgress / 0.15f * 0.4f :
                                 _lifeProgress > 0.8f ? (1f - _lifeProgress) / 0.2f * 0.4f : 0.4f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            var texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = GetBladeTipPosition() - Main.screenPosition;

            // Blade rotates to follow the orbit tangent direction for a natural spinning look
            float bladeRotation = _orbitAngle + MathHelper.PiOver4;
            SpriteEffects effects = SpriteEffects.None;

            // Ghostly tint: translucent lunar colors — phase-dependent intensity
            float phaseGlow = MathHelper.Lerp(0.3f, 0.5f, LunarPhase / 4f);
            Color ghostColor = Color.Lerp(EternalMoonUtils.Violet, EternalMoonUtils.IceBlue,
                (float)Math.Sin(_orbitAngle * 0.5f) * 0.5f + 0.5f);
            ghostColor *= Projectile.Opacity;

            // Switch to Additive for glow layers
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Wide outer glow (additive bloom under the ghost blade)
            Color outerGlow = EternalMoonUtils.DarkPurple with { A = 0 };
            Main.EntitySpriteDraw(texture, drawPos, null,
                outerGlow * Projectile.Opacity * 0.12f, bladeRotation, texture.Size() / 2f,
                2.6f * Projectile.scale, effects, 0);

            // Layer 2: Ghost blade body
            Main.EntitySpriteDraw(texture, drawPos, null,
                ghostColor, bladeRotation, texture.Size() / 2f, 2.0f * Projectile.scale, effects, 0);

            // Layer 3: Core glow overlay (additive)
            Color glowColor = EternalMoonUtils.IceBlue with { A = 0 };
            Main.EntitySpriteDraw(texture, drawPos, null,
                glowColor * Projectile.Opacity * phaseGlow, bladeRotation, texture.Size() / 2f,
                2.2f * Projectile.scale, effects, 0);

            // Restore to AlphaBlend
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Add moonlight at orbit position
            Vector2 tipPos = GetBladeTipPosition();
            Lighting.AddLight(tipPos, EternalMoonUtils.IceBlue.ToVector3() * Projectile.Opacity * 0.3f);

            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!Main.dedServ)
            {
                // Ghost impact: crescent sparks + moon glint + subtle bloom
                for (int i = 0; i < 5; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 6f);
                    Color sparkColor = Color.Lerp(EternalMoonUtils.Violet, EternalMoonUtils.IceBlue, Main.rand.NextFloat());
                    LunarParticleHandler.SpawnParticle(new CrescentSparkParticle(
                        target.Center, sparkVel, Main.rand.NextFloat(0.2f, 0.5f),
                        sparkColor, Main.rand.Next(10, 18)));
                }

                // Moon glint at impact point
                LunarParticleHandler.SpawnParticle(new MoonGlintParticle(
                    target.Center, Main.rand.NextFloat(0.2f, 0.4f),
                    EternalMoonUtils.MoonWhite * 0.7f, 12));

                // Subtle ghost impact bloom
                LunarParticleHandler.SpawnParticle(new LunarBloomParticle(
                    target.Center, 0.3f, EternalMoonUtils.Violet * 0.5f, 12, 0.03f));
            }
        }
    }
}
