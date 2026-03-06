using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.InfernalBeamFoundation
{
    /// <summary>
    /// InfernalBeam — A channeled beam projectile with a spinning ring at origin.
    ///
    /// Visual layers:
    ///  1. BEAM BODY — VertexStrip rendered with InfernalBeamBodyShader.fx
    ///     - SoundWaveBeam as primary scrolling body texture
    ///     - EnergyMotion + EnergySurge as secondary detail layers
    ///     - Gradient LUT for theme coloring
    ///
    ///  2. ORIGIN RING — InfernalBeamRing.png rendered as additive sprite at beam start
    ///     - Spins with time
    ///     - Squished along beam direction (compressed Y, stretched X relative to aim)
    ///     - Layered with bloom glow behind it
    ///
    ///  3. ENDPOINT FLARES — Additive bloom sprites at beam tip
    ///
    ///  4. DUST — Theme-colored dust along beam body and at endpoint
    /// </summary>
    public class InfernalBeam : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrism;

        // ---- CONSTANTS ----
        private const float BeamStartOffset = 60f;
        private const float MaxBeamLength = 2400f;
        private const float BaseBeamWidth = 90f;
        private const float AimSpeed = 0.08f;

        // Ring rendering
        private const float RingBaseScale = 0.35f;
        private const float RingSpinSpeed = 0.04f;

        // ---- STATE ----
        private float beamLength;
        private float ringRotation;
        private float flareRotation;

        private InfernalBeamTheme CurrentTheme => (InfernalBeamTheme)InfernalBeamFoundation.CurrentThemeIndex;

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

            if (!owner.channel || owner.dead || !owner.active || owner.noItems || owner.CCed)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;

            // ---- AIM TRACKING ----
            Vector2 aimDirection = (Main.MouseWorld - owner.MountedCenter).SafeNormalize(Vector2.UnitX);
            float targetAngle = aimDirection.ToRotation();
            float currentAngle = Projectile.velocity.ToRotation();

            float angleDiff = MathHelper.WrapAngle(targetAngle - currentAngle);
            float clampedDiff = MathHelper.Clamp(angleDiff, -AimSpeed, AimSpeed);
            Projectile.velocity = (currentAngle + clampedDiff).ToRotationVector2();

            Projectile.Center = owner.MountedCenter;

            owner.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
            owner.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * owner.direction,
                Projectile.velocity.X * owner.direction);

            // ---- COLLISION RAYCAST ----
            beamLength = MaxBeamLength;
            Vector2 beamStart = Projectile.Center + Projectile.velocity * BeamStartOffset;

            for (float d = 0; d < MaxBeamLength; d += 16f)
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
            for (float d = 0; d < beamLength; d += 200f)
            {
                Vector2 lightPos = beamStart + Projectile.velocity * d;
                Lighting.AddLight(lightPos, Color.White.ToVector3() * 0.7f);
            }
            Vector2 endPoint = beamStart + Projectile.velocity * beamLength;
            Lighting.AddLight(endPoint, Color.White.ToVector3() * 1.0f);
            Lighting.AddLight(beamStart, Color.White.ToVector3() * 1.2f);

            SpawnBeamDust(beamStart, endPoint);

            // ---- SPIN ----
            ringRotation += RingSpinSpeed * (Projectile.velocity.X > 0 ? 1f : -1f);
            flareRotation += 1.15f * (Projectile.velocity.X > 0 ? 1f : -1f);
        }

        private void SpawnBeamDust(Vector2 beamStart, Vector2 endPoint)
        {
            float rot = Projectile.velocity.ToRotation();
            Color[] themeColors = IBFTextures.GetDustColorsForTheme(CurrentTheme);

            // Along beam body
            for (float d = 0; d < beamLength; d += 150f)
            {
                if (!Main.rand.NextBool(4))
                    continue;

                Vector2 pos = beamStart + Projectile.velocity * d;
                Vector2 perpOffset = rot.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-25f, 25f);
                Vector2 vel = rot.ToRotationVector2().RotatedByRandom(0.05f) * Main.rand.NextFloat(2f, 5f);
                Color col = themeColors[Main.rand.Next(themeColors.Length)];

                Dust dust = Dust.NewDustPerfect(pos + perpOffset, DustID.RainbowMk2, vel, newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.7f));
                dust.noGravity = true;
                dust.fadeIn = 0.7f;
            }

            // At endpoint
            for (int i = 0; i < 2 + Main.rand.Next(0, 2); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(12f, 12f);
                Color col = themeColors[Main.rand.Next(themeColors.Length)];

                Dust dust = Dust.NewDustPerfect(endPoint, DustID.RainbowMk2, vel, newColor: col, Scale: Main.rand.NextFloat(0.4f, 0.7f));
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }

            // At origin ring
            if (Main.rand.NextBool(3))
            {
                Vector2 ringOffset = Main.rand.NextVector2Circular(30f, 30f);
                Color col = themeColors[Main.rand.Next(themeColors.Length)];
                Dust dust = Dust.NewDustPerfect(beamStart + ringOffset, DustID.RainbowMk2, Vector2.Zero, newColor: col, Scale: Main.rand.NextFloat(0.5f, 0.9f));
                dust.noGravity = true;
                dust.fadeIn = 0.8f;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 beamStart = Projectile.Center + Projectile.velocity * BeamStartOffset;
            Vector2 beamEnd = beamStart + Projectile.velocity * beamLength;

            float _ = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                beamStart, beamEnd,
                BaseBeamWidth * 0.5f, ref _);
        }

        public override void CutTiles()
        {
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Vector2 beamStart = Projectile.Center + Projectile.velocity * BeamStartOffset;
            Vector2 beamEnd = beamStart + Projectile.velocity * beamLength;
            Utils.PlotTileLine(beamStart, beamEnd, BaseBeamWidth, DelegateMethods.CutTiles);
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            DrawBeamBody();
            DrawOriginRing();
            DrawEndpointFlares();
            return false;
        }

        // =====================================================================
        //  LAYER 1: BEAM BODY — VertexStrip + InfernalBeamBodyShader
        // =====================================================================
        private void DrawBeamBody()
        {
            float rot = Projectile.velocity.ToRotation();
            Vector2 startPoint = Projectile.Center + Projectile.velocity * BeamStartOffset;
            Vector2 endPoint = startPoint + Projectile.velocity * beamLength;

            Vector2[] positions = { startPoint, endPoint };
            float[] rotations = { rot, rot };

            Color StripColor(float progress) => Color.White;
            float StripWidth(float progress) => BaseBeamWidth;

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
            beamShader.Parameters["gradientTex"].SetValue(IBFTextures.GetGradientForTheme(CurrentTheme));

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
        //  LAYER 2: ORIGIN RING — Circularly spinning InfernalBeamRing.png
        // =====================================================================
        private void DrawOriginRing()
        {
            SpriteBatch sb = Main.spriteBatch;
            float rot = Projectile.velocity.ToRotation();
            Vector2 drawPos = Projectile.Center - Main.screenPosition + Projectile.velocity * BeamStartOffset;

            Texture2D ringTex = IBFTextures.InfernalBeamRing.Value;
            Vector2 ringOrigin = ringTex.Size() / 2f;

            float sinPulse = MathF.Sin((float)Main.timeForVisualEffects * 0.05f);

            // Uniform scale with gentle breathing pulse — the ring stays circular
            float uniformScale = RingBaseScale * (1f + 0.06f * sinPulse);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.EffectMatrix);

            Color[] themeColors = IBFTextures.GetDustColorsForTheme(CurrentTheme);
            Color ringTint = themeColors[0];

            // LAYER 2a: Wide soft glow behind ring
            Texture2D softGlow = IBFTextures.SoftGlow.Value;
            sb.Draw(softGlow, drawPos, null, ringTint * 0.6f, 0f,
                softGlow.Size() / 2f, 0.45f, SpriteEffects.None, 0f);

            // LAYER 2b: Main spinning ring — circular rotation, the ring PNG spins freely
            sb.Draw(ringTex, drawPos, null, Color.White * 0.9f, ringRotation,
                ringOrigin, uniformScale, SpriteEffects.None, 0f);

            // LAYER 2c: Second ring pass, offset rotation for depth
            sb.Draw(ringTex, drawPos, null, ringTint * 0.5f, ringRotation + 0.4f,
                ringOrigin, uniformScale * 1.15f, SpriteEffects.None, 0f);

            // LAYER 2d: Third ring pass — counter-spinning
            sb.Draw(ringTex, drawPos, null, ringTint * 0.35f, -ringRotation * 0.6f,
                ringOrigin, uniformScale * 0.9f, SpriteEffects.None, 0f);

            // LAYER 2e: Small bright point bloom at ring center
            Texture2D pointBloom = IBFTextures.PointBloom.Value;
            sb.Draw(pointBloom, drawPos, null, Color.White * 0.8f, 0f,
                pointBloom.Size() / 2f, 0.15f + 0.03f * sinPulse, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        // =====================================================================
        //  LAYER 3: ENDPOINT FLARES
        // =====================================================================
        private void DrawEndpointFlares()
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 endPoint = Projectile.Center - Main.screenPosition
                + Projectile.velocity * (BeamStartOffset + beamLength);

            float sinPulse = MathF.Sin((float)Main.timeForVisualEffects * 0.04f);

            Texture2D starFlare = IBFTextures.StarFlare.Value;
            Texture2D glowOrb = IBFTextures.GlowOrb.Value;
            Texture2D lensFlare = IBFTextures.LensFlare.Value;

            Color[] themeColors = IBFTextures.GetDustColorsForTheme(CurrentTheme);
            Color endColor = themeColors[0];

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.EffectMatrix);

            // Wide soft glow
            sb.Draw(glowOrb, endPoint, null, endColor * 0.5f, flareRotation * 0.1f,
                glowOrb.Size() / 2f, 0.45f, SpriteEffects.None, 0f);

            // Star flare — spins slowly
            float endScale = 0.5f + 0.1f * sinPulse;
            sb.Draw(starFlare, endPoint, null, Color.White * 0.7f, flareRotation * 0.05f,
                starFlare.Size() / 2f, endScale * 0.5f, SpriteEffects.None, 0f);
            sb.Draw(starFlare, endPoint, null, endColor * 0.4f, flareRotation * 0.077f,
                starFlare.Size() / 2f, endScale * 0.35f, SpriteEffects.None, 0f);

            // Small lens flare
            sb.Draw(lensFlare, endPoint, null, Color.White * 0.5f, flareRotation * 0.02f,
                lensFlare.Size() / 2f, 0.3f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color[] themeColors = IBFTextures.GetDustColorsForTheme(CurrentTheme);
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Color col = themeColors[Main.rand.Next(themeColors.Length)];
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel, newColor: col, Scale: Main.rand.NextFloat(0.5f, 0.9f));
                dust.noGravity = true;
            }
        }
    }
}
