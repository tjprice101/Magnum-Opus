using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Projectiles
{
    /// <summary>
    /// Spectral blade copy that floats at a cardinal position around the owner
    /// and periodically fires homing <see cref="SakuraSparkLaser"/> projectiles
    /// at the nearest enemy.
    /// ai[0] = cardinal index (0=above, 1=below, 2=left, 3=right)
    /// </summary>
    public class SakuraSpectralTurret : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        private static readonly Vector2[] CardinalOffsets =
        {
            new(0, -80),   // above
            new(0, 80),    // below
            new(-80, 0),   // left
            new(80, 0),    // right
        };

        private int _timer;
        private float _fadeAlpha;

        /// <summary>Base draw scale for the oversized blade sprite (source texture is very large).</summary>
        private const float BladeDrawScale = 0.10f;

        // Cached texture references
        private static Asset<Texture2D> _softGlowTex;
        private static Asset<Texture2D> _bladeTex;

        public override void SetStaticDefaults()
        {
            _softGlowTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow");
            _bladeTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Content/Eroica/Weapons/SakurasBlossom/SakurasBlossom");
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            _timer++;

            // --- Fade logic ---
            // First 15 frames: fade in (0 -> 1)
            if (_timer <= 15)
            {
                _fadeAlpha = _timer / 15f;
            }
            // Last 30 frames: fade out (1 -> 0)
            else if (Projectile.timeLeft <= 30)
            {
                _fadeAlpha = Projectile.timeLeft / 30f;
            }
            else
            {
                _fadeAlpha = 1f;
            }

            _fadeAlpha = MathHelper.Clamp(_fadeAlpha, 0f, 1f);

            // --- Position tracking ---
            int cardinalIndex = (int)MathHelper.Clamp(Projectile.ai[0], 0f, 3f);
            Vector2 targetPos = owner.MountedCenter + CardinalOffsets[cardinalIndex];
            Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.15f);

            // --- Slow rotation ---
            Projectile.rotation += 0.02f;

            // --- Pulsing scale (stored in localAI for PreDraw access) ---
            Projectile.localAI[0] = BladeDrawScale * (0.8f + 0.1f * MathF.Sin(Main.GlobalTimeWrappedHourly * 4f + Projectile.ai[0]));

            // --- Lighting ---
            Lighting.AddLight(Projectile.Center, 0.5f * _fadeAlpha, 0.2f * _fadeAlpha, 0.3f * _fadeAlpha);

            // --- Fire laser every 12 ticks (after fade-in) ---
            if (_timer % 12 == 0 && _timer > 15)
            {
                NPC target = FindNearestEnemy(600f);
                if (target != null && Main.myPlayer == Projectile.owner)
                {
                    Vector2 vel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 18f;
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        vel,
                        ModContent.ProjectileType<SakuraSparkLaser>(),
                        Projectile.damage,
                        0f,
                        Projectile.owner);
                }
            }
        }

        /// <summary>
        /// Finds the nearest active, targetable NPC within the given range.
        /// </summary>
        private NPC FindNearestEnemy(float maxDist)
        {
            NPC closest = null;
            float closestDistSq = maxDist * maxDist;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || !npc.CanBeChasedBy())
                    continue;

                float distSq = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                if (distSq < closestDistSq)
                {
                    closestDistSq = distSq;
                    closest = npc;
                }
            }

            return closest;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float drawScale = Projectile.localAI[0];
            float fade = _fadeAlpha;

            Texture2D glowTex = _softGlowTex?.Value;
            Texture2D bladeTex = _bladeTex?.Value;

            if (glowTex == null || bladeTex == null)
                return false;

            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 bladeOrigin = bladeTex.Size() / 2f;

            // --- Pass 1: Additive SoftGlow underlayer ---
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Color glowColor = new Color(255, 120, 140) * 0.3f * fade;
            glowColor.A = 0;
            spriteBatch.Draw(glowTex, drawPos, null, glowColor, 0f, glowOrigin, 0.2f, SpriteEffects.None, 0f);

            spriteBatch.End();

            // --- Pass 2: AlphaBlend blade drawing ---
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            // Ghostly blade body
            Color bladeColor = Color.White * 0.6f * fade;
            spriteBatch.Draw(bladeTex, drawPos, null, bladeColor, Projectile.rotation, bladeOrigin, drawScale, SpriteEffects.None, 0f);

            // Spectral sakura glow overlay
            Color spectralColor = new Color(255, 180, 200) * 0.2f * fade;
            spriteBatch.Draw(bladeTex, drawPos, null, spectralColor, Projectile.rotation, bladeOrigin, drawScale * 1.05f, SpriteEffects.None, 0f);

            return false;
        }
    }
}
