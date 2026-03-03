using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.SmokeFoundation
{
    /// <summary>
    /// SmokeRingProjectile — Spawns a circular ring of smoke puff clouds that expand
    /// and fade exactly like Calamity's Supernova HeavySmokeParticle behaviour:
    ///
    /// Lifecycle per puff:
    ///   - First 20% of life: Scale grows by +0.01/frame (expansion phase)
    ///   - After 20%: Scale shrinks by ×0.975/frame (contraction/dissipation)
    ///   - Opacity decays ×0.98/frame throughout
    ///   - Velocity decays ×0.85/frame (heavy drag — smoke doesn't travel far)
    ///   - Last 15%: Additional rapid alpha fade via Utils.GetLerpValue(1, 0.85, completion)
    ///   - Color shifts from core (hot) → body → edge (cool/dark) over lifetime
    ///
    /// Spawns 30 puffs in a randomised ring using Calamity's offset pattern:
    ///   offset = new Vector2(15, 15).RotatedByRandom(100) * rand(0.8, 1.6)
    ///   (same vector used as both spawn offset AND initial velocity)
    ///
    /// Each puff picks a random frame from the 3×6 smoke spritesheet grid.
    /// The projectile is invisible — all visuals are the internal SmokePuff structs
    /// drawn in PreDraw.
    ///
    /// ai[0] = SmokeCloudStyle index
    /// </summary>
    public class SmokeRingProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        // ---- CONFIGURATION ----
        private const int PuffCount = 30;
        private const int MaxLifetime = 60;
        private const float DamageRadius = 80f;

        /// <summary>
        /// Multiplier applied to puff Scale when rendering, because
        /// grid frames are ~142×157px and would be enormous at scale 1.0.
        /// </summary>
        private const float RenderScale = 0.3f;

        // ---- STATE ----
        private int timer;
        private bool initialized;
        private bool damageDone;
        private SmokeCloudStyle style;
        private SmokePuff[] puffs;

        // ---- SMOKE PUFF STRUCT ----
        private struct SmokePuff
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Rotation;
            public float RotationSpeed;
            public float Scale;          // Gameplay scale (0.9–2.3 range)
            public float Opacity;
            public int FrameIndex;       // Which frame in the 3×6 grid (0–17)
            public int Lifetime;         // Individual puff lifetime in frames
            public int Time;             // Current age in frames
            public bool Active;
            public SpriteEffects Flip;   // Random horizontal/vertical flip for variety
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 600;
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = MaxLifetime + 10;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (!initialized)
            {
                initialized = true;
                style = (SmokeCloudStyle)(int)Projectile.ai[0];
                puffs = new SmokePuff[PuffCount];
                InitializePuffs();
            }

            timer++;
            Projectile.velocity = Vector2.Zero;

            // AoE damage on first frame
            if (!damageDone)
            {
                damageDone = true;
                DealAreaDamage();
            }

            UpdatePuffs();

            // Centre lighting (fading)
            Color[] colors = SKFTextures.GetStyleColors(style);
            float centerGlow = MathHelper.Clamp(1f - timer / 15f, 0f, 1f);
            Lighting.AddLight(Projectile.Center, colors[1].ToVector3() * centerGlow * 0.5f);

            if (timer >= MaxLifetime)
                Projectile.Kill();
        }

        // =====================================================================
        // PUFF INITIALISATION — Calamity Supernova ring pattern
        // =====================================================================

        private void InitializePuffs()
        {
            for (int i = 0; i < PuffCount; i++)
            {
                // Calamity's SupernovaBoom pattern:
                //   Vector2 smokeVel = new Vector2(15, 15).RotatedByRandom(100) * rand(0.8f, 1.6f)
                // This produces a randomised circular ring — the same vector is used
                // as both the spawn offset AND the initial velocity.
                Vector2 ring = new Vector2(15f, 15f).RotatedByRandom(100) *
                               Main.rand.NextFloat(0.8f, 1.6f);

                // Random flip for visual variety (avoids all puffs looking identical)
                SpriteEffects flip = SpriteEffects.None;
                if (Main.rand.NextBool()) flip |= SpriteEffects.FlipHorizontally;
                if (Main.rand.NextBool()) flip |= SpriteEffects.FlipVertically;

                puffs[i] = new SmokePuff
                {
                    Position = Projectile.Center + ring,
                    Velocity = ring,
                    Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                    RotationSpeed = Main.rand.NextFloat(-0.04f, 0.04f),
                    Scale = Main.rand.NextFloat(0.9f, 2.3f),
                    Opacity = 0.4f,
                    FrameIndex = Main.rand.Next(SKFTextures.TotalFrames),
                    Lifetime = Main.rand.Next(25, 36),
                    Time = 0,
                    Active = true,
                    Flip = flip,
                };
            }
        }

        // =====================================================================
        // PUFF UPDATE — Calamity HeavySmokeParticle lifecycle
        // =====================================================================

        private void UpdatePuffs()
        {
            for (int i = 0; i < puffs.Length; i++)
            {
                if (!puffs[i].Active) continue;

                ref SmokePuff p = ref puffs[i];
                p.Time++;

                float completion = (float)p.Time / p.Lifetime;

                // ---- VELOCITY: heavy drag ×0.85 per frame ----
                p.Velocity *= 0.85f;

                // Move
                p.Position += p.Velocity;

                // ---- ROTATION: decelerating spin ----
                p.Rotation += p.RotationSpeed;
                p.RotationSpeed *= 0.96f;

                // ---- SCALE: grow first 20%, then shrink ----
                if (completion < 0.2f)
                {
                    // Expansion phase
                    p.Scale += 0.01f;
                }
                else
                {
                    // Contraction/dissipation
                    p.Scale *= 0.975f;
                }

                // ---- OPACITY: ×0.98 per frame, with rapid fade in final 15% ----
                p.Opacity *= 0.98f;

                if (completion > 0.85f)
                {
                    // Final rapid fade — Calamity uses Utils.GetLerpValue(1, 0.85, completion)
                    float finalFade = Utils.GetLerpValue(1f, 0.85f, completion, true);
                    p.Opacity *= finalFade;
                }

                // Per-puff lighting
                if (p.Opacity > 0.05f)
                {
                    Color[] colors = SKFTextures.GetStyleColors(style);
                    Color lightCol = Color.Lerp(colors[1], colors[0], completion);
                    Lighting.AddLight(p.Position, lightCol.ToVector3() * p.Opacity * 0.2f);
                }

                // Kill when faded or lifetime over
                if (p.Time >= p.Lifetime || p.Opacity < 0.01f || p.Scale < 0.05f)
                    p.Active = false;
            }
        }

        // =====================================================================
        // AREA DAMAGE
        // =====================================================================

        private void DealAreaDamage()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;

                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist < DamageRadius)
                {
                    float falloff = 1f - (dist / DamageRadius) * 0.5f;
                    int damage = (int)(Projectile.damage * falloff);
                    int dir = (npc.Center.X > Projectile.Center.X) ? 1 : -1;

                    npc.StrikeNPC(npc.CalculateHitInfo(damage, dir, false,
                        Projectile.knockBack, DamageClass.Melee, true));
                }
            }
        }

        // =====================================================================
        // RENDERING
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            if (puffs == null) return false;

            SpriteBatch sb = Main.spriteBatch;
            Color[] colors = SKFTextures.GetStyleColors(style);
            Texture2D smokeTex = SKFTextures.SmokeGrid.Value;
            Vector2 frameOrigin = SKFTextures.GetFrameOrigin();

            // ---- ADDITIVE PASS (smoke body — Additive makes black backgrounds invisible) ----
            // Black pixels in the smoke spritesheet add nothing to the scene,
            // while bright smoke areas glow naturally.
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawSmokePuffs(sb, colors, smokeTex, frameOrigin);

            // ---- ADDITIVE PASS (center flash + glow accents) ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawCenterFlash(sb, colors);
            DrawPuffGlowAccents(sb, colors);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        /// <summary>
        /// Draws the smoke puff bodies from the 3×6 grid spritesheet.
        /// Color shifts from core (hot) → body → edge (cool) over lifetime.
        /// </summary>
        private void DrawSmokePuffs(SpriteBatch sb, Color[] colors,
            Texture2D smokeTex, Vector2 frameOrigin)
        {
            for (int i = 0; i < puffs.Length; i++)
            {
                if (!puffs[i].Active) continue;
                ref SmokePuff p = ref puffs[i];

                float completion = (float)p.Time / p.Lifetime;
                Vector2 drawPos = p.Position - Main.screenPosition;

                // Color lifecycle: core → body → edge
                Color puffColor;
                if (completion < 0.3f)
                {
                    // Hot phase: core → body
                    float t = completion / 0.3f;
                    puffColor = Color.Lerp(colors[1], colors[0], t);
                }
                else
                {
                    // Cool phase: body → edge
                    float t = (completion - 0.3f) / 0.7f;
                    puffColor = Color.Lerp(colors[0], colors[2], t);
                }

                puffColor *= p.Opacity;

                Rectangle frame = SKFTextures.GetFrameRect(p.FrameIndex);
                float drawScale = p.Scale * RenderScale;

                sb.Draw(smokeTex, drawPos, frame, puffColor,
                    p.Rotation, frameOrigin, drawScale, p.Flip, 0f);
            }
        }

        /// <summary>
        /// Bright flash at the explosion center for the first few frames.
        /// </summary>
        private void DrawCenterFlash(SpriteBatch sb, Color[] colors)
        {
            if (timer > 10) return;

            float flashProgress = timer / 10f;
            float flashAlpha = 1f - flashProgress * flashProgress;
            Vector2 center = Projectile.Center - Main.screenPosition;

            Texture2D softGlow = SKFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            // Wide soft flash
            sb.Draw(softGlow, center, null, colors[1] * (flashAlpha * 0.5f),
                0f, glowOrigin, 0.35f * (1f + flashProgress * 0.3f),
                SpriteEffects.None, 0f);

            // Tight core
            sb.Draw(softGlow, center, null, colors[1] * (flashAlpha * 0.3f),
                0f, glowOrigin, 0.15f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Small additive glow behind each puff for a subtle inner luminosity.
        /// Only drawn for puffs that are still fairly visible.
        /// </summary>
        private void DrawPuffGlowAccents(SpriteBatch sb, Color[] colors)
        {
            Texture2D softGlow = SKFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            for (int i = 0; i < puffs.Length; i++)
            {
                if (!puffs[i].Active) continue;
                ref SmokePuff p = ref puffs[i];

                if (p.Opacity < 0.1f) continue;

                float completion = (float)p.Time / p.Lifetime;
                Vector2 drawPos = p.Position - Main.screenPosition;

                // Core glow — visible more in the early hot phase
                float glowAlpha = p.Opacity * MathHelper.Clamp(1f - completion * 1.5f, 0f, 1f);
                if (glowAlpha > 0.02f)
                {
                    float glowScale = p.Scale * RenderScale * 0.6f;
                    sb.Draw(softGlow, drawPos, null, colors[1] * (glowAlpha * 0.25f),
                        0f, glowOrigin, glowScale, SpriteEffects.None, 0f);
                }
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.Transparent;
    }
}
