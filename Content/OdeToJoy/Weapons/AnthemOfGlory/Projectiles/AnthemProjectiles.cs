using MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Projectiles
{
    /// <summary>
    /// AnthemBeamProjectile — Channeled continuous golden beam (LaserFoundation).
    /// Persists while player channels. Width/brightness scale with Crescendo (1x→2x).
    /// Spawns GloryNoteProjectile every 2s. Conductor gesture sway.
    /// </summary>
    public class AnthemBeamProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const float MaxBeamLength = 2400f;
        private const float BaseBeamWidth = 40f;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 999999;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // Kill if not channeling
            if (!owner.channel || owner.dead || !owner.active)
            {
                owner.GetModPlayer<AnthemPlayer>().ResetChannel();
                Projectile.Kill();
                return;
            }

            // Update crescendo
            AnthemPlayer ap = owner.GetModPlayer<AnthemPlayer>();
            ap.UpdateChannel();

            // Beam direction toward cursor
            Vector2 dir = (Main.MouseWorld - owner.Center).SafeNormalize(Vector2.UnitX);
            Projectile.Center = owner.Center;
            Projectile.velocity = dir;
            Projectile.rotation = dir.ToRotation();

            // Visual direction for owner sprite
            owner.ChangeDir(dir.X >= 0 ? 1 : -1);
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;

            // Conductor sway — sinusoidal lateral offset
            float sway = (float)Math.Sin(ap.ChannelFrames * 0.05f) * 8f * ap.CrescendoProgress;
            Vector2 perpendicular = new Vector2(-dir.Y, dir.X);
            Projectile.Center += perpendicular * sway;

            // Spawn Glory Notes
            if (ap.ShouldSpawnGloryNote() && Main.myPlayer == Projectile.owner)
            {
                float noteAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 notePos = owner.Center + new Vector2((float)Math.Cos(noteAngle), (float)Math.Sin(noteAngle)) * 40f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), notePos, dir * 6f,
                    ModContent.ProjectileType<GloryNoteProjectile>(), Projectile.damage / 3, 1f, Projectile.owner);
            }

            // Beam impact dust
            Vector2 beamEnd = owner.Center + dir * MaxBeamLength;
            for (float t = 0; t < MaxBeamLength; t += 16f)
            {
                Vector2 checkPos = owner.Center + dir * t;
                Point tilePos = checkPos.ToTileCoordinates();
                if (WorldGen.InWorld(tilePos.X, tilePos.Y) && Main.tile[tilePos.X, tilePos.Y].HasTile &&
                    Main.tileSolid[Main.tile[tilePos.X, tilePos.Y].TileType])
                {
                    beamEnd = checkPos;
                    break;
                }
            }

            // Sparkle particles along beam
            if (Main.rand.NextBool(3))
            {
                float dist = Main.rand.NextFloat() * Vector2.Distance(owner.Center, beamEnd);
                float beamWidth = BaseBeamWidth * ap.CrescendoMultiplier;
                Vector2 particlePos = owner.Center + dir * dist + perpendicular * Main.rand.NextFloat(-beamWidth * 0.5f, beamWidth * 0.5f);
                Dust d = Dust.NewDustDirect(particlePos, 1, 1, DustID.GoldFlame, 0f, -0.5f, 100, AnthemTextures.BloomGold, 0.5f + ap.CrescendoProgress * 0.3f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player owner = Main.player[Projectile.owner];
            AnthemPlayer ap = owner.GetModPlayer<AnthemPlayer>();
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float beamWidth = BaseBeamWidth * ap.CrescendoMultiplier;

            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                owner.Center, owner.Center + dir * MaxBeamLength, beamWidth, ref _);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player owner = Main.player[Projectile.owner];
            AnthemPlayer ap = owner.GetModPlayer<AnthemPlayer>();
            modifiers.FinalDamage *= ap.CrescendoMultiplier;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.life <= 0)
            {
                Player owner = Main.player[Projectile.owner];
                AnthemPlayer ap = owner.GetModPlayer<AnthemPlayer>();
                if (ap.RegisterKill() && Main.myPlayer == Projectile.owner)
                {
                    // Victory Fanfare — golden shockwave
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                        ModContent.ProjectileType<VictoryFanfareProjectile>(), Projectile.damage * 2, 8f, Projectile.owner);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Player owner = Main.player[Projectile.owner];
            AnthemPlayer ap = owner.GetModPlayer<AnthemPlayer>();

            Texture2D beamTex = AnthemTextures.OJBeamEnergy;
            Texture2D glowTex = AnthemTextures.SoftGlow;
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float rotation = dir.ToRotation();

            float crescendo = ap.CrescendoMultiplier;
            float beamWidth = BaseBeamWidth * crescendo;
            float brightness = 0.5f + ap.CrescendoProgress * 0.5f;
            float time = (float)Main.timeForVisualEffects * 0.015f;

            // Compute beam length
            float beamLength = MaxBeamLength;
            for (float t = 0; t < MaxBeamLength; t += 16f)
            {
                Vector2 checkPos = owner.Center + dir * t;
                Point tilePos = checkPos.ToTileCoordinates();
                if (WorldGen.InWorld(tilePos.X, tilePos.Y) && Main.tile[tilePos.X, tilePos.Y].HasTile &&
                    Main.tileSolid[Main.tile[tilePos.X, tilePos.Y].TileType])
                {
                    beamLength = t;
                    break;
                }
            }

            sb.End();

            // ── LAYER 0: VertexStrip beam body with JubilantHarmony shader ──
            Effect beamShader = OdeToJoyShaders.JubilantHarmony;
            if (beamShader != null)
            {
                Vector2 startPoint = owner.Center;
                Vector2 endPoint = owner.Center + dir * beamLength;
                Vector2[] positions = { startPoint, endPoint };
                float[] rotations = { rotation, rotation };

                // Glow underlayer (wide, soft)
                VertexStrip glowStrip = new VertexStrip();
                float glowWidth = beamWidth * 2.5f;
                glowStrip.PrepareStrip(positions, rotations,
                    (float p) => AnthemTextures.BloomGold * brightness * 0.35f,
                    (float p) => glowWidth,
                    -Main.screenPosition, includeBacksides: true);

                OdeToJoyShaders.SetBeamParams(beamShader, time, AnthemTextures.BloomGold,
                    new Color(90, 200, 60), brightness * 0.5f, 1.2f, 1f + ap.CrescendoProgress);
                beamShader.CurrentTechnique = beamShader.Techniques["HarmonicBeamTechnique"];
                beamShader.Parameters["WorldViewProjection"]?.SetValue(
                    Main.GameViewMatrix.NormalizedTransformationmatrix);
                beamShader.CurrentTechnique.Passes["P0"].Apply();
                glowStrip.DrawTrail();
                Main.pixelShader.CurrentTechnique.Passes[0].Apply();

                // Core body (narrow, bright)
                VertexStrip coreStrip = new VertexStrip();
                coreStrip.PrepareStrip(positions, rotations,
                    (float p) => Color.White * brightness * 0.8f,
                    (float p) => beamWidth,
                    -Main.screenPosition, includeBacksides: true);

                OdeToJoyShaders.SetBeamParams(beamShader, time * 1.3f, AnthemTextures.RadiantAmber,
                    AnthemTextures.BloomGold, brightness * 0.8f, 1.8f, 2f + ap.CrescendoProgress);
                beamShader.CurrentTechnique.Passes["P0"].Apply();
                coreStrip.DrawTrail();
                Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            }

            // ── LAYER 1: Additive bloom overlays ──
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Segmented beam texture overlay for extra body detail
            float segmentLength = beamTex.Width;
            if (segmentLength <= 0) segmentLength = 64;
            Vector2 beamOrigin = new Vector2(0, beamTex.Height / 2f);

            for (float t = 0; t < beamLength; t += segmentLength)
            {
                float remaining = Math.Min(segmentLength, beamLength - t);
                Rectangle sourceRect = new Rectangle(0, 0, (int)remaining, beamTex.Height);
                Vector2 segPos = owner.Center + dir * t - Main.screenPosition;
                float widthScale = beamWidth / beamTex.Height;

                // Outer golden body
                sb.Draw(beamTex, segPos, sourceRect, AnthemTextures.BloomGold * brightness * 0.3f,
                    rotation, beamOrigin, new Vector2(1f, widthScale * 1.2f), SpriteEffects.None, 0f);
                // Hot center line
                sb.Draw(beamTex, segPos, sourceRect, AnthemTextures.PureJoyWhite * brightness * 0.25f,
                    rotation, beamOrigin, new Vector2(1f, widthScale * 0.3f), SpriteEffects.None, 0f);
            }

            // ── LAYER 2: Endpoint flares with bloom stacking ──
            Vector2 originPos = owner.Center - Main.screenPosition;
            // Origin: wide outer → medium → bright core (3-layer bloom)
            sb.Draw(glowTex, originPos, null, AnthemTextures.BloomGold * brightness * 0.3f, 0f,
                glowTex.Size() / 2f, 0.8f * crescendo, SpriteEffects.None, 0f);
            sb.Draw(glowTex, originPos, null, AnthemTextures.RadiantAmber * brightness * 0.4f, 0f,
                glowTex.Size() / 2f, 0.5f * crescendo, SpriteEffects.None, 0f);
            sb.Draw(glowTex, originPos, null, AnthemTextures.JubilantLight * brightness * 0.3f, 0f,
                glowTex.Size() / 2f, 0.2f * crescendo, SpriteEffects.None, 0f);

            // Endpoint: bloom + pulse
            Vector2 endPos = owner.Center + dir * beamLength - Main.screenPosition;
            float endPulse = 0.8f + 0.2f * (float)Math.Sin(ap.ChannelFrames * 0.1f);
            sb.Draw(glowTex, endPos, null, AnthemTextures.BloomGold * brightness * 0.4f * endPulse, 0f,
                glowTex.Size() / 2f, 0.7f * crescendo, SpriteEffects.None, 0f);
            sb.Draw(glowTex, endPos, null, AnthemTextures.RadiantAmber * brightness * 0.5f * endPulse, 0f,
                glowTex.Size() / 2f, 0.4f * crescendo, SpriteEffects.None, 0f);
            sb.Draw(glowTex, endPos, null, AnthemTextures.PureJoyWhite * brightness * 0.3f * endPulse, 0f,
                glowTex.Size() / 2f, 0.15f * crescendo, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// GloryNoteProjectile — Homing golden music note (SparkleProjectileFoundation).
    /// Spawns every 2s during beam channel. Homes toward beam target.
    /// </summary>
    public class GloryNoteProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int TrailLength = 16;
        private Vector2[] _trailPositions = new Vector2[TrailLength];
        private int _trailHead;
        private int _timer;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            _timer++;
            _trailPositions[_trailHead] = Projectile.Center;
            _trailHead = (_trailHead + 1) % TrailLength;

            Projectile.rotation += 0.12f;

            // Moderate homing toward nearest enemy
            if (_timer > 15)
            {
                float homingRange = 400f;
                NPC closest = null;
                float closestDist = homingRange;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
                if (closest != null)
                {
                    Vector2 toTarget = (closest.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 10f, 0.05f);
                }
            }

            // Musical sparkle trail
            if (Main.rand.NextBool(2))
            {
                Color noteColor = AnthemTextures.NoteColors[_timer / 8 % AnthemTextures.NoteColors.Length];
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, -Projectile.velocity.X * 0.1f, -Projectile.velocity.Y * 0.1f, 120, noteColor, 0.5f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D sparkleTex = AnthemTextures.OJBlossomSparkle;
            Texture2D glowTex = AnthemTextures.SoftGlow;
            Vector2 sparkleOrigin = sparkleTex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float fadeIn = MathHelper.Clamp(_timer / 10f, 0f, 1f);
            float fadeOut = MathHelper.Clamp(Projectile.timeLeft / 20f, 0f, 1f);
            float fade = fadeIn * fadeOut;
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: Shader trail with TriumphantTrail BlossomWind ──
            Effect trailShader = OdeToJoyShaders.TriumphantTrail;
            if (trailShader != null)
            {
                // Build ordered position/rotation arrays from ring buffer
                int validCount = 0;
                for (int i = 0; i < TrailLength; i++)
                {
                    int idx = (_trailHead - 1 - i + TrailLength) % TrailLength;
                    if (_trailPositions[idx] != Vector2.Zero) validCount++;
                    else break;
                }

                if (validCount >= 2)
                {
                    Vector2[] positions = new Vector2[validCount];
                    float[] rotations = new float[validCount];
                    for (int i = 0; i < validCount; i++)
                    {
                        int idx = (_trailHead - 1 - i + TrailLength) % TrailLength;
                        positions[validCount - 1 - i] = _trailPositions[idx];
                    }
                    for (int i = 0; i < validCount; i++)
                    {
                        if (i < validCount - 1)
                            rotations[i] = (positions[i + 1] - positions[i]).ToRotation();
                        else
                            rotations[i] = rotations[Math.Max(0, i - 1)];
                    }

                    VertexStrip strip = new VertexStrip();
                    strip.PrepareStrip(positions, rotations,
                        (float p) => AnthemTextures.NoteColors[(int)(p * 3.99f) % AnthemTextures.NoteColors.Length] * fade * p * 0.6f,
                        (float p) => MathHelper.Lerp(1f, 8f, p),
                        -Main.screenPosition, includeBacksides: true);

                    OdeToJoyShaders.SetTrailParams(trailShader, time, AnthemTextures.BloomGold,
                        AnthemTextures.RadiantAmber, fade * 0.7f, 1.5f);
                    trailShader.CurrentTechnique = trailShader.Techniques["BlossomWindTrailTechnique"];
                    trailShader.Parameters["WorldViewProjection"]?.SetValue(
                        Main.GameViewMatrix.NormalizedTransformationmatrix);
                    trailShader.CurrentTechnique.Passes["P0"].Apply();
                    strip.DrawTrail();
                    Main.pixelShader.CurrentTechnique.Passes[0].Apply();
                }
            }

            // ── LAYER 1: Additive bloom trail + glow head ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            // Trail sparkle bloom (lighter version alongside shader trail)
            for (int i = 0; i < TrailLength; i++)
            {
                int idx = (_trailHead - 1 - i + TrailLength) % TrailLength;
                if (_trailPositions[idx] == Vector2.Zero) continue;
                float trailFade = (1f - i / (float)TrailLength) * fade * 0.25f;
                Color trailColor = AnthemTextures.NoteColors[i % AnthemTextures.NoteColors.Length] * trailFade;
                float trailScale = 0.12f * (1f - i / (float)TrailLength);
                sb.Draw(glowTex, _trailPositions[idx] - Main.screenPosition, null, trailColor, 0f,
                    glowOrigin, trailScale, SpriteEffects.None, 0f);
            }

            // Note body sparkle (3-layer bloom head)
            sb.Draw(glowTex, drawPos, null, AnthemTextures.BloomGold * fade * 0.4f, 0f,
                glowOrigin, 0.3f, SpriteEffects.None, 0f);
            sb.Draw(sparkleTex, drawPos, null, AnthemTextures.BloomGold * fade * 0.7f, Projectile.rotation,
                sparkleOrigin, 0.35f, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, AnthemTextures.RadiantAmber * fade * 0.4f, 0f,
                glowOrigin, 0.18f, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, AnthemTextures.JubilantLight * fade * 0.35f, 0f,
                glowOrigin, 0.07f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);

            return false;
        }
    }

    /// <summary>
    /// VictoryFanfareProjectile — Screen-wide golden shockwave on 3+ kills.
    /// ImpactFoundation — 12 expanding golden ripple rings.
    /// </summary>
    public class VictoryFanfareProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int _timer;

        public override void SetDefaults()
        {
            Projectile.width = 300;
            Projectile.height = 300;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            _timer++;
            Projectile.velocity = Vector2.Zero;

            // Expanding golden ring particles
            if (_timer < 30)
            {
                float ringCount = 12;
                for (int r = 0; r < ringCount; r++)
                {
                    if (_timer != r * 2 + 1) continue;
                    for (int i = 0; i < 30; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 30f;
                        float speed = 5f + r * 1.5f;
                        Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                        Color ringColor = Color.Lerp(AnthemTextures.BloomGold, AnthemTextures.PureJoyWhite, r / ringCount);
                        Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.GoldFlame, vel.X, vel.Y, 80, ringColor, 1.0f + r * 0.15f);
                        d.noGravity = true;
                        d.fadeIn = 1.5f;
                    }
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float expandRadius = _timer * 12f;
            Vector2 closestPoint = new Vector2(
                MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right),
                MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
            float dist = Vector2.Distance(Projectile.Center, closestPoint);
            return dist <= expandRadius && dist >= expandRadius - 40f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D ringTex = AnthemTextures.OJPowerRing;
            Texture2D glowTex = AnthemTextures.SoftGlow;
            Texture2D impactTex = AnthemTextures.OJHarmonicImpact;
            Vector2 ringOrigin = ringTex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 impactOrigin = impactTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float fade = MathHelper.Clamp(1f - _timer / 60f, 0f, 1f);
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: CelebrationAura shader — expanding golden rings ──
            Effect auraShader = OdeToJoyShaders.CelebrationAura;
            if (auraShader != null)
            {
                float expandRadius = 0.15f + _timer * 0.008f;
                OdeToJoyShaders.SetAuraParams(auraShader, time + _timer * 0.04f,
                    AnthemTextures.BloomGold, new Color(90, 200, 60),
                    fade * 0.7f, 2.0f, expandRadius, 6f);

                OdeToJoyShaders.BeginShaderBatch(sb, auraShader, "CelebrationAuraTechnique");
                auraShader.CurrentTechnique.Passes["P0"].Apply();

                float shaderScale = 0.08f + _timer * 0.004f;
                sb.Draw(glowTex, drawPos, null, Color.White * fade, 0f, glowOrigin,
                    shaderScale, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive texture overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            // Expanding ring layers
            for (int r = 0; r < 4; r++)
            {
                float ringScale = (0.3f + _timer * 0.03f) * (1f + r * 0.3f);
                float ringFade = fade * (1f - r * 0.2f);
                Color ringColor = Color.Lerp(AnthemTextures.BloomGold, AnthemTextures.PureJoyWhite, r / 4f);
                sb.Draw(ringTex, drawPos, null, ringColor * ringFade * 0.4f,
                    _timer * 0.02f * (r + 1), ringOrigin, ringScale, SpriteEffects.None, 0f);
            }

            // Harmonic impact overlay
            float impactScale = 0.5f + _timer * 0.02f;
            sb.Draw(impactTex, drawPos, null, AnthemTextures.RadiantAmber * fade * 0.35f,
                -_timer * 0.01f, impactOrigin, impactScale * 0.8f, SpriteEffects.None, 0f);

            // Core flash (3-tier bloom: wide → medium → tight)
            float flashFade = MathHelper.Clamp(1f - _timer / 15f, 0f, 1f);
            sb.Draw(glowTex, drawPos, null, AnthemTextures.BloomGold * flashFade * 0.4f, 0f,
                glowOrigin, 2.0f, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, AnthemTextures.PureJoyWhite * flashFade * 0.6f, 0f,
                glowOrigin, 1.2f, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, AnthemTextures.JubilantLight * flashFade * 0.4f, 0f,
                glowOrigin, 0.5f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);

            return false;
        }
    }
}