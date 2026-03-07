using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.FoundationWeapons.SmokeFoundation;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Projectiles
{
    /// <summary>
    /// SupernovaSmokeRing — Foundation-based SmokeFoundation adapted for the
    /// Resurrection of the Moon's supernova detonation.
    ///
    /// Spawns 30 smoke puffs from the shared SmokeRender3x6GRID spritesheet
    /// in a radial ring formation with lunar purple/blue/white coloring:
    ///   Purple core (first 30%) → Ice blue body (mid) → Dark fade edge (late)
    ///
    /// Calamity-style lifecycle: expand ring outward → slow deceleration → fade.
    /// Each puff has random frame from 18-frame grid, random flip, rotation drift.
    /// VFX-only projectile: friendly=false, 0 damage.
    ///
    /// ai[0] = lunar phase multiplier (0.7–1.3) scaling puff count and ring radius
    /// </summary>
    public class SupernovaSmokeRing : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int BasePuffCount = 30;
        private const int MaxLifetime = 60;
        private const float RenderScale = 0.3f;

        private int timer;
        private bool initialized;
        private SmokePuff[] puffs;
        private int actualPuffCount;

        // Lunar smoke palette: purple core → blue body → dark edge
        private static readonly Color LunarSmokeCore = new(100, 80, 200);    // Purple core
        private static readonly Color LunarSmokeBody = new(120, 190, 255);   // Ice blue body
        private static readonly Color LunarSmokeEdge = new(30, 15, 50);      // Dark night edge

        private struct SmokePuff
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Rotation;
            public float RotationSpeed;
            public float Scale;
            public float Opacity;
            public int FrameIndex;
            public int Lifetime;
            public int Time;
            public bool Active;
            public SpriteEffects Flip;
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
                float lunarMult = MathHelper.Clamp(Projectile.ai[0], 0.5f, 2f);
                actualPuffCount = (int)(BasePuffCount * lunarMult);
                puffs = new SmokePuff[actualPuffCount];
                InitializePuffs(lunarMult);
            }

            timer++;
            Projectile.velocity = Vector2.Zero;

            UpdatePuffs();

            float centerGlow = MathHelper.Clamp(1f - timer / 15f, 0f, 1f);
            Lighting.AddLight(Projectile.Center, LunarSmokeBody.ToVector3() * centerGlow * 0.4f);

            if (timer >= MaxLifetime)
                Projectile.Kill();
        }

        private void InitializePuffs(float lunarMult)
        {
            float ringScale = MathHelper.Lerp(0.8f, 1.4f, lunarMult - 0.5f);

            for (int i = 0; i < actualPuffCount; i++)
            {
                Vector2 ring = new Vector2(15f, 15f).RotatedByRandom(100) *
                               Main.rand.NextFloat(0.8f, 1.6f) * ringScale;

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

        private void UpdatePuffs()
        {
            for (int i = 0; i < puffs.Length; i++)
            {
                if (!puffs[i].Active) continue;
                ref SmokePuff p = ref puffs[i];

                p.Time++;
                float completion = (float)p.Time / p.Lifetime;

                // Decelerate outward expansion
                p.Velocity *= 0.85f;
                p.Position += p.Velocity;

                p.Rotation += p.RotationSpeed;
                p.RotationSpeed *= 0.96f;

                // Expand briefly, then contract
                if (completion < 0.2f)
                    p.Scale += 0.01f;
                else
                    p.Scale *= 0.975f;

                p.Opacity *= 0.98f;

                // Final fade-out
                if (completion > 0.85f)
                {
                    float finalFade = Utils.GetLerpValue(1f, 0.85f, completion, true);
                    p.Opacity *= finalFade;
                }

                // Per-puff lighting
                if (p.Opacity > 0.05f)
                {
                    Color lightCol = Color.Lerp(LunarSmokeBody, LunarSmokeCore, completion);
                    Lighting.AddLight(p.Position, lightCol.ToVector3() * p.Opacity * 0.15f);
                }

                if (p.Time >= p.Lifetime || p.Opacity < 0.01f || p.Scale < 0.05f)
                    p.Active = false;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (puffs == null) return false;

            SpriteBatch sb = Main.spriteBatch;
            Texture2D smokeTex = SKFTextures.SmokeGrid.Value;
            Vector2 frameOrigin = SKFTextures.GetFrameOrigin();

            // Pass 1: Smoke puffs (additive for glow)
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawSmokePuffs(sb, smokeTex, frameOrigin);

            // Pass 2: Center flash + glow accents
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawCenterFlash(sb);
            DrawPuffGlowAccents(sb);

            // Restore
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private void DrawSmokePuffs(SpriteBatch sb, Texture2D smokeTex, Vector2 frameOrigin)
        {
            for (int i = 0; i < puffs.Length; i++)
            {
                if (!puffs[i].Active) continue;
                ref SmokePuff p = ref puffs[i];

                float completion = (float)p.Time / p.Lifetime;
                Vector2 drawPos = p.Position - Main.screenPosition;

                // Lunar color progression: purple core → ice blue → dark edge
                Color puffColor;
                if (completion < 0.3f)
                    puffColor = Color.Lerp(LunarSmokeCore, LunarSmokeBody, completion / 0.3f);
                else
                    puffColor = Color.Lerp(LunarSmokeBody, LunarSmokeEdge, (completion - 0.3f) / 0.7f);

                puffColor *= p.Opacity;

                Rectangle frame = SKFTextures.GetFrameRect(p.FrameIndex);
                float drawScale = p.Scale * RenderScale;

                sb.Draw(smokeTex, drawPos, frame, puffColor,
                    p.Rotation, frameOrigin, drawScale, p.Flip, 0f);
            }
        }

        private void DrawCenterFlash(SpriteBatch sb)
        {
            if (timer > 10) return;

            float flashProgress = timer / 10f;
            float flashAlpha = 1f - flashProgress * flashProgress;
            Vector2 center = Projectile.Center - Main.screenPosition;

            Texture2D softGlow = SKFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            // Wide ice blue glow
            sb.Draw(softGlow, center, null, LunarSmokeBody * (flashAlpha * 0.5f),
                0f, glowOrigin, 0.14f * (1f + flashProgress * 0.3f),
                SpriteEffects.None, 0f);

            // Tight purple core
            sb.Draw(softGlow, center, null, LunarSmokeCore * (flashAlpha * 0.3f),
                0f, glowOrigin, 0.06f, SpriteEffects.None, 0f);
        }

        private void DrawPuffGlowAccents(SpriteBatch sb)
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

                float glowAlpha = p.Opacity * MathHelper.Clamp(1f - completion * 1.5f, 0f, 1f);
                if (glowAlpha > 0.02f)
                {
                    float glowScale = p.Scale * RenderScale * 0.6f;
                    sb.Draw(softGlow, drawPos, null, LunarSmokeBody * (glowAlpha * 0.25f),
                        0f, glowOrigin, glowScale, SpriteEffects.None, 0f);
                }
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.Transparent;
    }
}
