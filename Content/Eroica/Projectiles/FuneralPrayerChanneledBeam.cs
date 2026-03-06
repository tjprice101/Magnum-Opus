using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// FuneralPrayerChanneledBeam — A channeled beam for the Funeral Prayer weapon.
    /// Hold to sustain a somber funeral flame beam that tracks the cursor.
    ///
    /// Visual layers:
    ///  1. BEAM BODY — Segment-based strip with EroicaFuneralTrailShader (dual-pass body + glow)
    ///  2. ORIGIN CONVERGENCE — Additive bloom and convergence aura at beam start
    ///  3. ENDPOINT FLARES — Stacked bloom, star flare, and ember sparks at beam tip
    ///  4. DUST — FuneralFlame particles along beam body + PrayerAsh at endpoints
    /// </summary>
    public class FuneralPrayerChanneledBeam : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrism;

        // ---- CONSTANTS ----
        private const float BeamStartOffset = 50f;
        private const float MaxBeamLength = 2200f;
        private const float BaseBeamWidth = 70f;
        private const float AimSpeed = 0.06f;
        private const int BeamSegments = 40;

        // ---- STATE ----
        private float beamLength;
        private float flareRotation;
        private float convergenceRotation;
        private int warmupTimer;
        private const int WarmupFrames = 12;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 2400;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // Channel check — kill beam if player stops channeling
            if (!owner.channel || owner.dead || !owner.active || owner.noItems || owner.CCed)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;
            warmupTimer = Math.Min(warmupTimer + 1, WarmupFrames);
            float warmupProgress = (float)warmupTimer / WarmupFrames;

            // ---- AIM TRACKING ----
            Vector2 aimDirection = (Main.MouseWorld - owner.MountedCenter).SafeNormalize(Vector2.UnitX);
            float targetAngle = aimDirection.ToRotation();
            float currentAngle = Projectile.velocity.ToRotation();

            float angleDiff = MathHelper.WrapAngle(targetAngle - currentAngle);
            float clampedDiff = MathHelper.Clamp(angleDiff, -AimSpeed, AimSpeed);
            Projectile.velocity = (currentAngle + clampedDiff).ToRotationVector2();

            Projectile.Center = owner.MountedCenter;

            // Lock player animation
            owner.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
            owner.itemRotation = (float)Math.Atan2(
                Projectile.velocity.Y * owner.direction,
                Projectile.velocity.X * owner.direction);

            // ---- COLLISION RAYCAST ----
            beamLength = MaxBeamLength * warmupProgress;
            Vector2 beamStart = Projectile.Center + Projectile.velocity * BeamStartOffset;

            for (float d = 0; d < beamLength; d += 16f)
            {
                Vector2 checkPoint = beamStart + Projectile.velocity * d;
                Point tileCoords = checkPoint.ToTileCoordinates();

                if (tileCoords.X < 0 || tileCoords.X >= Main.maxTilesX ||
                    tileCoords.Y < 0 || tileCoords.Y >= Main.maxTilesY)
                    continue;

                Tile tile = Main.tile[tileCoords.X, tileCoords.Y];
                if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                {
                    beamLength = d;
                    break;
                }
            }

            // ---- LIGHTING ----
            Color funeralLight = FuneralUtils.PrayerFlame;
            Vector3 lightVec = funeralLight.ToVector3() * 0.6f * warmupProgress;
            for (float d = 0; d < beamLength; d += 180f)
            {
                Vector2 lightPos = beamStart + Projectile.velocity * d;
                Lighting.AddLight(lightPos, lightVec);
            }

            Vector2 endPoint = beamStart + Projectile.velocity * beamLength;
            Lighting.AddLight(endPoint, funeralLight.ToVector3() * 0.9f * warmupProgress);
            Lighting.AddLight(beamStart, FuneralUtils.EmberCore.ToVector3() * 1.0f * warmupProgress);

            // ---- PARTICLES ----
            SpawnBeamParticles(beamStart, endPoint, warmupProgress);

            // ---- SPIN ----
            convergenceRotation += 0.03f * (Projectile.velocity.X > 0 ? 1f : -1f);
            flareRotation += 0.08f;
        }

        private void SpawnBeamParticles(Vector2 beamStart, Vector2 endPoint, float warmup)
        {
            if (warmup < 0.5f) return;

            float rot = Projectile.velocity.ToRotation();

            // Flame particles along beam body
            for (float d = 0; d < beamLength; d += 120f)
            {
                if (!Main.rand.NextBool(3)) continue;

                Vector2 pos = beamStart + Projectile.velocity * d;
                Vector2 perpOffset = rot.ToRotationVector2().RotatedBy(MathHelper.PiOver2)
                    * Main.rand.NextFloat(-BaseBeamWidth * 0.3f, BaseBeamWidth * 0.3f);
                Vector2 vel = (-Projectile.velocity).RotatedByRandom(0.3f) * Main.rand.NextFloat(0.5f, 2f)
                    + new Vector2(0, -Main.rand.NextFloat(0.3f, 1.2f));

                Color col = Main.rand.NextBool()
                    ? FuneralUtils.PrayerFlame
                    : FuneralUtils.SmolderingAmber;

                Dust dust = Dust.NewDustPerfect(pos + perpOffset, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.6f));
                dust.noGravity = true;
                dust.fadeIn = 0.6f;
            }

            // Ash at endpoint
            if (beamLength > 100f)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(6f, 6f) + new Vector2(0, -1f);
                    Dust dust = Dust.NewDustPerfect(endPoint, DustID.RainbowMk2, vel,
                        newColor: FuneralUtils.AshGray, Scale: Main.rand.NextFloat(0.3f, 0.5f));
                    dust.noGravity = true;
                    dust.fadeIn = 0.4f;
                }
            }

            // Embers at origin
            if (Main.rand.NextBool(3))
            {
                Vector2 ringOffset = Main.rand.NextVector2Circular(20f, 20f);
                Dust dust = Dust.NewDustPerfect(beamStart + ringOffset, DustID.RainbowMk2,
                    Vector2.Zero, newColor: FuneralUtils.EmberCore, Scale: Main.rand.NextFloat(0.4f, 0.8f));
                dust.noGravity = true;
                dust.fadeIn = 0.7f;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (warmupTimer < WarmupFrames / 2) return false;

            Vector2 beamStart = Projectile.Center + Projectile.velocity * BeamStartOffset;
            Vector2 beamEnd = beamStart + Projectile.velocity * beamLength;

            float _ = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                beamStart, beamEnd,
                BaseBeamWidth * 0.4f, ref _);
        }

        public override void CutTiles()
        {
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Vector2 beamStart = Projectile.Center + Projectile.velocity * BeamStartOffset;
            Vector2 beamEnd = beamStart + Projectile.velocity * beamLength;
            Utils.PlotTileLine(beamStart, beamEnd, BaseBeamWidth * 0.6f, DelegateMethods.CutTiles);
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            if (warmupTimer < 2) return false;

            float warmup = (float)warmupTimer / WarmupFrames;

            DrawBeamBody(warmup);
            DrawOriginConvergence(warmup);
            DrawEndpointFlares(warmup);

            return false;
        }

        // =====================================================================
        //  LAYER 1: BEAM BODY — Segment-based strip with funeral trail shader
        // =====================================================================
        private void DrawBeamBody(float warmup)
        {
            SpriteBatch sb = Main.spriteBatch;
            float time = (float)Main.timeForVisualEffects * 0.008f;

            Vector2 beamStart = Projectile.Center + Projectile.velocity * BeamStartOffset;
            float rot = Projectile.velocity.ToRotation();

            Texture2D stripTex = EroicaTextures.EmberScatter?.Value ?? EroicaTextures.EnergyTrailUV?.Value;
            if (stripTex == null) return;

            int texW = stripTex.Width;
            int texH = stripTex.Height;
            float scrollOffset = (float)Main.timeForVisualEffects * 0.004f;
            int segCount = Math.Max(4, (int)(beamLength / 60f));
            segCount = Math.Min(segCount, BeamSegments);
            int srcWidth = Math.Max(1, texW / segCount);

            float beamWidthMult = MathHelper.Clamp(warmup * 1.5f, 0f, 1f);
            float beamAlpha = MathHelper.Clamp(warmup * 2f, 0f, 1f);

            bool hasShader = EroicaShaderManager.HasFuneralTrail;

            if (hasShader)
            {
                // PASS 1: Funeral flame body — deep crimson/ember beam
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplyFuneralPrayerBeamTrail(time, glowPass: false);
                    DrawBeamSegments(sb, stripTex, beamStart, rot, segCount, srcWidth, texW, texH,
                        scrollOffset, beamWidthMult, beamAlpha * 0.7f, 1f);
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }

                // PASS 2: Funeral glow — wider, softer smolder overlay
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplyFuneralPrayerBeamTrail(time, glowPass: true);
                    DrawBeamSegments(sb, stripTex, beamStart, rot, segCount, srcWidth, texW, texH,
                        scrollOffset, beamWidthMult * 1.6f, beamAlpha * 0.3f, 1f);
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
            else
            {
                // Fallback: palette-colored additive beam
                EroicaShaderManager.BeginAdditive(sb);
                try
                {
                    DrawBeamSegmentsFallback(sb, stripTex, beamStart, rot, segCount, srcWidth, texW, texH,
                        scrollOffset, beamWidthMult, beamAlpha * 0.6f);
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
        }

        private void DrawBeamSegments(SpriteBatch sb, Texture2D stripTex, Vector2 beamStart,
            float rot, int segCount, int srcWidth, int texW, int texH,
            float scrollOffset, float widthMult, float alpha, float tipFadeStart)
        {
            for (int i = 0; i < segCount; i++)
            {
                float progress = (float)i / segCount;
                float nextProgress = (float)(i + 1) / segCount;
                float segStartDist = progress * beamLength;
                float segEndDist = nextProgress * beamLength;
                float segLength = segEndDist - segStartDist;

                if (segLength < 0.5f) continue;

                // Width: thicker at origin, taper toward tip
                float widthCurve = 1f - progress * progress * 0.4f;
                // Smooth tip fade
                float tipFade = MathHelper.SmoothStep(1f, 0f,
                    MathHelper.Clamp((progress - 0.8f) / 0.2f, 0f, 1f));
                // Smooth start fade
                float startFade = MathHelper.SmoothStep(0f, 1f,
                    MathHelper.Clamp(progress / 0.08f, 0f, 1f));

                float width = BaseBeamWidth * widthCurve * widthMult;
                float segAlpha = alpha * tipFade * startFade;
                if (segAlpha < 0.005f) continue;

                // UV scrolling
                float uStart = (progress + scrollOffset) % 1f;
                int srcX = (int)(uStart * texW) % texW;
                Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                float scaleX = segLength / (float)srcWidth;
                float scaleY = width / (float)texH;
                Vector2 pos = beamStart + Projectile.velocity * segStartDist - Main.screenPosition;
                Vector2 drawOrigin = new Vector2(0, texH / 2f);

                sb.Draw(stripTex, pos, srcRect, Color.White * segAlpha, rot, drawOrigin,
                    new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
            }
        }

        private void DrawBeamSegmentsFallback(SpriteBatch sb, Texture2D stripTex, Vector2 beamStart,
            float rot, int segCount, int srcWidth, int texW, int texH,
            float scrollOffset, float widthMult, float alpha)
        {
            for (int i = 0; i < segCount; i++)
            {
                float progress = (float)i / segCount;
                float nextProgress = (float)(i + 1) / segCount;
                float segStartDist = progress * beamLength;
                float segEndDist = nextProgress * beamLength;
                float segLength = segEndDist - segStartDist;

                if (segLength < 0.5f) continue;

                float widthCurve = 1f - progress * progress * 0.4f;
                float tipFade = MathHelper.SmoothStep(1f, 0f,
                    MathHelper.Clamp((progress - 0.8f) / 0.2f, 0f, 1f));
                float startFade = MathHelper.SmoothStep(0f, 1f,
                    MathHelper.Clamp(progress / 0.08f, 0f, 1f));

                float width = BaseBeamWidth * widthCurve * widthMult;
                float segAlpha = alpha * tipFade * startFade;
                if (segAlpha < 0.005f) continue;

                float uStart = (progress + scrollOffset) % 1f;
                int srcX = (int)(uStart * texW) % texW;
                Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                float scaleX = segLength / (float)srcWidth;
                float scaleY = width / (float)texH;
                Vector2 pos = beamStart + Projectile.velocity * segStartDist - Main.screenPosition;
                Vector2 drawOrigin = new Vector2(0, texH / 2f);

                // Palette-driven color: ember at origin transitioning to deep crimson at tip
                Color bodyColor = Color.Lerp(FuneralUtils.EmberCore, FuneralUtils.DeepCrimson, progress) with { A = 0 };
                sb.Draw(stripTex, pos, srcRect, bodyColor * segAlpha, rot, drawOrigin,
                    new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
            }
        }

        // =====================================================================
        //  LAYER 2: ORIGIN CONVERGENCE — Bloom aura at beam emission point
        // =====================================================================
        private void DrawOriginConvergence(float warmup)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition + Projectile.velocity * BeamStartOffset;
            float sinPulse = MathF.Sin((float)Main.timeForVisualEffects * 0.04f);
            float convergenceScale = 0.35f * warmup;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // LAYER 2a: Wide somber glow — deep crimson haze
            var bloomTex = MagnumTextureRegistry.GetBloom();
            if (bloomTex != null)
            {
                Vector2 bloomOrigin = bloomTex.Size() / 2f;

                // Outer funeral haze
                Color hazeColor = FuneralUtils.DeepCrimson with { A = 0 };
                sb.Draw(bloomTex, drawPos, null, hazeColor * (0.35f * warmup),
                    0f, bloomOrigin, convergenceScale * 2.5f + sinPulse * 0.05f,
                    SpriteEffects.None, 0f);

                // Mid glow — prayer flame
                Color midColor = FuneralUtils.PrayerFlame with { A = 0 };
                sb.Draw(bloomTex, drawPos, null, midColor * (0.5f * warmup),
                    0f, bloomOrigin, convergenceScale * 1.5f,
                    SpriteEffects.None, 0f);

                // Core — hot ember
                Color coreColor = FuneralUtils.EmberCore with { A = 0 };
                sb.Draw(bloomTex, drawPos, null, coreColor * (0.7f * warmup),
                    0f, bloomOrigin, convergenceScale * 0.8f,
                    SpriteEffects.None, 0f);

                // White-hot center point
                Color soulColor = FuneralUtils.SoulWhite with { A = 0 };
                sb.Draw(bloomTex, drawPos, null, soulColor * (0.6f * warmup),
                    0f, bloomOrigin, convergenceScale * 0.3f + sinPulse * 0.02f,
                    SpriteEffects.None, 0f);
            }

            // LAYER 2b: Spinning convergence ring using EnergyFlare
            if (EroicaTextures.EnergyFlare?.Value is Texture2D flareTex)
            {
                Vector2 flareOrigin = flareTex.Size() / 2f;
                Color ringColor = FuneralUtils.SmolderingAmber with { A = 0 };

                sb.Draw(flareTex, drawPos, null, ringColor * (0.4f * warmup),
                    convergenceRotation, flareOrigin, convergenceScale * 0.6f,
                    SpriteEffects.None, 0f);

                // Counter-spinning layer
                sb.Draw(flareTex, drawPos, null, (FuneralUtils.PrayerFlame with { A = 0 }) * (0.25f * warmup),
                    -convergenceRotation * 0.7f, flareOrigin, convergenceScale * 0.45f,
                    SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        // =====================================================================
        //  LAYER 3: ENDPOINT FLARES — Bloom, star, and embers at beam tip
        // =====================================================================
        private void DrawEndpointFlares(float warmup)
        {
            if (beamLength < 50f) return;

            SpriteBatch sb = Main.spriteBatch;
            Vector2 endPoint = Projectile.Center - Main.screenPosition
                + Projectile.velocity * (BeamStartOffset + beamLength);

            float sinPulse = MathF.Sin((float)Main.timeForVisualEffects * 0.05f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            float endAlpha = warmup * MathHelper.Clamp(beamLength / 200f, 0f, 1f);

            // Wide smoldering glow
            var bloomTex = MagnumTextureRegistry.GetBloom();
            if (bloomTex != null)
            {
                Vector2 bloomOrigin = bloomTex.Size() / 2f;

                // Outer ember haze
                Color hazeColor = FuneralUtils.SmolderingAmber with { A = 0 };
                sb.Draw(bloomTex, endPoint, null, hazeColor * (0.3f * endAlpha),
                    0f, bloomOrigin, 0.5f + sinPulse * 0.04f,
                    SpriteEffects.None, 0f);

                // Mid prayer flame glow
                Color midColor = FuneralUtils.PrayerFlame with { A = 0 };
                sb.Draw(bloomTex, endPoint, null, midColor * (0.45f * endAlpha),
                    0f, bloomOrigin, 0.3f,
                    SpriteEffects.None, 0f);

                // Bright core flash
                Color coreColor = FuneralUtils.SoulWhite with { A = 0 };
                sb.Draw(bloomTex, endPoint, null, coreColor * (0.35f * endAlpha),
                    0f, bloomOrigin, 0.12f + sinPulse * 0.02f,
                    SpriteEffects.None, 0f);
            }

            // Star flare — slowly rotating
            if (EroicaTextures.Star4Point?.Value is Texture2D starTex)
            {
                Vector2 starOrigin = starTex.Size() / 2f;
                float starScale = (0.25f + 0.05f * sinPulse) * endAlpha;

                sb.Draw(starTex, endPoint, null, (FuneralUtils.EmberCore with { A = 0 }) * (0.5f * endAlpha),
                    flareRotation * 0.3f, starOrigin, starScale,
                    SpriteEffects.None, 0f);
                sb.Draw(starTex, endPoint, null, (FuneralUtils.SoulWhite with { A = 0 }) * (0.3f * endAlpha),
                    -flareRotation * 0.2f, starOrigin, starScale * 0.7f,
                    SpriteEffects.None, 0f);
            }

            // Energy flare at impact
            if (EroicaTextures.EnergyFlare?.Value is Texture2D flareTex)
            {
                Vector2 flareOrigin = flareTex.Size() / 2f;
                sb.Draw(flareTex, endPoint, null, (FuneralUtils.PrayerFlame with { A = 0 }) * (0.3f * endAlpha),
                    flareRotation * 0.15f, flareOrigin, 0.2f * endAlpha,
                    SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Impact burst — funeral sparks
            Color[] impactColors = new Color[]
            {
                FuneralUtils.PrayerFlame,
                FuneralUtils.SmolderingAmber,
                FuneralUtils.EmberCore,
                FuneralUtils.DeepCrimson,
            };

            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Color col = impactColors[Main.rand.Next(impactColors.Length)];
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.4f, 0.8f));
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }

            // Rising ash on impact
            if (Main.rand.NextBool(2))
            {
                Vector2 ashVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(1f, 3f));
                Dust ash = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, ashVel,
                    newColor: FuneralUtils.AshGray, Scale: Main.rand.NextFloat(0.3f, 0.5f));
                ash.noGravity = true;
                ash.fadeIn = 0.3f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Final dissipation burst at beam origin
            Vector2 beamStart = Projectile.Center + Projectile.velocity * BeamStartOffset;
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f) + new Vector2(0, -1f);
                Color col = Main.rand.NextBool()
                    ? FuneralUtils.PrayerFlame
                    : FuneralUtils.AshGray;
                Dust dust = Dust.NewDustPerfect(beamStart, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.4f, 0.7f));
                dust.noGravity = true;
                dust.fadeIn = 0.6f;
            }
        }
    }
}
