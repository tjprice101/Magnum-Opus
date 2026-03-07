using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Enums;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.FoundationWeapons.InfernalBeamFoundation;

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
        private const float AimSpeed = 0.14f;

        // Ring rendering (matches InfernalBeam pattern)
        private const float RingBaseScale = 0.35f;
        private const float RingSpinSpeed = 0.04f;

        // ---- STATE ----
        private float beamLength;
        private float flareRotation;
        private float ringRotation;
        private int warmupTimer;
        private const int WarmupFrames = 12;

        private Effect beamShader;

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

            // Per-tick mana drain — channeled beams cost mana over time
            if (warmupTimer % 15 == 0)
            {
                if (owner.statMana < 4)
                {
                    Projectile.Kill();
                    return;
                }
                owner.statMana -= 4;
                owner.manaRegenDelay = (int)owner.maxRegenDelay;
            }

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
            ringRotation += RingSpinSpeed * (Projectile.velocity.X > 0 ? 1f : -1f);
            flareRotation += 1.15f * (Projectile.velocity.X > 0 ? 1f : -1f);
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

            DrawBeamBody();
            DrawOriginRing();
            DrawEndpointFlares();

            return false;
        }

        // =====================================================================
        //  LAYER 1: BEAM BODY — VertexStrip + InfernalBeamBodyShader
        //  (Exact InfernalBeam foundation pattern, Eroica Funeral themed)
        // =====================================================================
        private void DrawBeamBody()
        {
            float warmup = (float)warmupTimer / WarmupFrames;
            float rot = Projectile.velocity.ToRotation();
            Vector2 startPoint = Projectile.Center + Projectile.velocity * BeamStartOffset;
            Vector2 endPoint = startPoint + Projectile.velocity * beamLength;

            Vector2[] positions = { startPoint, endPoint };
            float[] rotations = { rot, rot };

            float beamWidth = BaseBeamWidth * MathHelper.Clamp(warmup * 1.5f, 0f, 1f);
            float beamAlpha = MathHelper.Clamp(warmup * 2f, 0f, 1f);

            Color StripColor(float progress) => Color.White * beamAlpha;
            float StripWidth(float progress) => beamWidth;

            VertexStrip strip = new VertexStrip();
            strip.PrepareStrip(positions, rotations, StripColor, StripWidth,
                -Main.screenPosition, includeBacksides: true);

            if (beamShader == null)
            {
                beamShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/InfernalBeamFoundation/Shaders/InfernalBeamBodyShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            beamShader.Parameters["WorldViewProjection"].SetValue(
                Main.GameViewMatrix.NormalizedTransformationmatrix);

            beamShader.Parameters["onTex"].SetValue(IBFTextures.BeamAlphaMask.Value);
            beamShader.Parameters["gradientTex"].SetValue(IBFTextures.GradEroica.Value);

            // SoundWaveBeam as primary body texture
            beamShader.Parameters["bodyTex"].SetValue(IBFTextures.SoundWaveBeam.Value);
            // Secondary detail layers
            beamShader.Parameters["detailTex1"].SetValue(IBFTextures.EnergyMotion.Value);
            beamShader.Parameters["detailTex2"].SetValue(IBFTextures.EnergySurge.Value);
            beamShader.Parameters["noiseTex"].SetValue(IBFTextures.NoiseFBM.Value);

            float dist = (endPoint - startPoint).Length();
            float repVal = dist / 2000f;

            beamShader.Parameters["bodyReps"].SetValue(1.5f * repVal);
            beamShader.Parameters["detail1Reps"].SetValue(2.0f * repVal);
            beamShader.Parameters["detail2Reps"].SetValue(1.2f * repVal);
            beamShader.Parameters["gradientReps"].SetValue(0.75f * repVal);
            beamShader.Parameters["bodyScrollSpeed"].SetValue(0.8f);
            beamShader.Parameters["detail1ScrollSpeed"].SetValue(1.2f);
            beamShader.Parameters["detail2ScrollSpeed"].SetValue(-0.6f);
            beamShader.Parameters["noiseDistortion"].SetValue(0.03f);
            beamShader.Parameters["totalMult"].SetValue(1.2f);
            beamShader.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.024f);

            beamShader.CurrentTechnique.Passes["MainPS"].Apply();
            strip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  LAYER 2: ORIGIN RING — Spinning InfernalBeamRing.png at beam start
        //  (Exact InfernalBeam foundation pattern, Eroica Funeral tinted)
        // =====================================================================
        private void DrawOriginRing()
        {
            float warmup = (float)warmupTimer / WarmupFrames;

            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition + Projectile.velocity * BeamStartOffset;

            Texture2D ringTex = IBFTextures.InfernalBeamRing.Value;
            Vector2 ringOrigin = ringTex.Size() / 2f;

            float sinPulse = MathF.Sin((float)Main.timeForVisualEffects * 0.05f);

            // Uniform scale with gentle breathing pulse — the ring stays circular
            float uniformScale = RingBaseScale * (1f + 0.06f * sinPulse) * warmup;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.EffectMatrix);

            // Eroica funeral theme tint
            Color ringTint = FuneralUtils.PrayerFlame;

            // LAYER 2a: Wide soft glow behind ring (reduced brightness)
            Texture2D softGlow = IBFTextures.SoftGlow.Value;
            sb.Draw(softGlow, drawPos, null, ringTint * (0.3f * warmup), 0f,
                softGlow.Size() / 2f, 0.29f * warmup, SpriteEffects.None, 0f);

            // LAYER 2b: Main spinning ring — circular rotation (tinted, reduced)
            sb.Draw(ringTex, drawPos, null, ringTint * (0.4f * warmup), ringRotation,
                ringOrigin, uniformScale, SpriteEffects.None, 0f);

            // LAYER 2c: Second ring pass, offset rotation for depth (reduced)
            sb.Draw(ringTex, drawPos, null, ringTint * (0.2f * warmup), ringRotation + 0.4f,
                ringOrigin, uniformScale * 1.15f, SpriteEffects.None, 0f);

            // LAYER 2d: Third ring pass — counter-spinning (reduced)
            sb.Draw(ringTex, drawPos, null, ringTint * (0.15f * warmup), -ringRotation * 0.6f,
                ringOrigin, uniformScale * 0.9f, SpriteEffects.None, 0f);

            // LAYER 2e: Small bright point bloom at ring center (tinted, reduced)
            Texture2D pointBloom = IBFTextures.PointBloom.Value;
            sb.Draw(pointBloom, drawPos, null, ringTint * (0.3f * warmup), 0f,
                pointBloom.Size() / 2f, (0.1f + 0.035f * sinPulse) * warmup, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        // =====================================================================
        //  LAYER 3: ENDPOINT FLARES
        //  (Exact InfernalBeam foundation pattern, Eroica Funeral tinted)
        // =====================================================================
        private void DrawEndpointFlares()
        {
            if (beamLength < 50f) return;

            float warmup = (float)warmupTimer / WarmupFrames;

            SpriteBatch sb = Main.spriteBatch;
            Vector2 endPoint = Projectile.Center - Main.screenPosition
                + Projectile.velocity * (BeamStartOffset + beamLength);

            float sinPulse = MathF.Sin((float)Main.timeForVisualEffects * 0.04f);
            float endAlpha = warmup * MathHelper.Clamp(beamLength / 200f, 0f, 1f);

            Texture2D starFlare = IBFTextures.StarFlare.Value;
            Texture2D glowOrb = IBFTextures.GlowOrb.Value;
            Texture2D lensFlare = IBFTextures.LensFlare.Value;

            // Eroica funeral theme tint
            Color endColor = FuneralUtils.PrayerFlame;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.EffectMatrix);

            // Wide soft glow
            sb.Draw(glowOrb, endPoint, null, endColor * (0.5f * endAlpha), flareRotation * 0.1f,
                glowOrb.Size() / 2f, 0.45f, SpriteEffects.None, 0f);

            // Star flare — spins slowly
            float endScale = 0.5f + 0.1f * sinPulse;
            sb.Draw(starFlare, endPoint, null, Color.White * (0.7f * endAlpha), flareRotation * 0.05f,
                starFlare.Size() / 2f, endScale * 0.5f, SpriteEffects.None, 0f);
            sb.Draw(starFlare, endPoint, null, endColor * (0.4f * endAlpha), flareRotation * 0.077f,
                starFlare.Size() / 2f, endScale * 0.35f, SpriteEffects.None, 0f);

            // Small lens flare
            sb.Draw(lensFlare, endPoint, null, Color.White * (0.5f * endAlpha), flareRotation * 0.02f,
                lensFlare.Size() / 2f, 0.3f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
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
