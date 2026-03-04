using MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon.Projectiles
{
    /// <summary>
    /// PetalClusterProjectile — arcing petal bomb that creates a persistent vortex zone on impact.
    /// ai[0] = seasonal color index (0=pink, 1=gold, 2=white cycle).
    /// ExplosionParticlesFoundation SpiralShrapnel on impact.
    /// </summary>
    public class PetalClusterProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
        }

        public override void AI()
        {
            Projectile.rotation += 0.15f;
            Projectile.velocity.Y += 0.12f; // Slight arc

            // Spinning petal trail
            if (Main.rand.NextBool(2))
            {
                Color seasonal = PetalStormTextures.GetSeasonalColor((int)Projectile.ai[0]);
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, -Projectile.velocity.X * 0.15f, -Projectile.velocity.Y * 0.15f, 120, seasonal, 0.7f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Spawn persistent vortex zone
            if (Main.myPlayer == Projectile.owner)
            {
                int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<PetalVortexZone>(), Projectile.damage / 3, 0f, Projectile.owner);
                if (proj >= 0 && proj < Main.maxProjectiles)
                    Main.projectile[proj].ai[0] = Projectile.ai[0]; // Pass seasonal color
            }

            // SpiralShrapnel particles — 40 per cluster
            Color seasonal = PetalStormTextures.GetSeasonalColor((int)Projectile.ai[0]);
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f + (i * 0.15f); // Spiral offset
                float speed = 3f + Main.rand.NextFloat() * 4f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.GoldFlame, vel.X, vel.Y, 100, seasonal, 1.0f);
                d.noGravity = true;
                d.fadeIn = 1.4f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D petalTex = PetalStormTextures.OJRosePetal;
            Texture2D glowTex = PetalStormTextures.SoftGlow;
            Vector2 petalOrigin = petalTex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color seasonal = PetalStormTextures.GetSeasonalColor((int)Projectile.ai[0]);

            float fade = MathHelper.Clamp((300 - Projectile.timeLeft) / 8f, 0f, 1f);
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: PollenDrift shader — seed trail body ──
            Effect pollenShader = OdeToJoyShaders.PollenDrift;
            if (pollenShader != null)
            {
                OdeToJoyShaders.SetPollenParams(pollenShader, time, seasonal,
                    PetalStormTextures.BloomGold, fade * 0.5f, 1.6f, 0.3f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, pollenShader, "PollenTrailTechnique");
                sb.Draw(glowTex, drawPos, null, Color.White * fade, Projectile.rotation, glowOrigin,
                    0.35f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive bloom layers ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            // Spinning petal body
            sb.Draw(petalTex, drawPos, null, seasonal * fade * 0.8f, Projectile.rotation, petalOrigin,
                0.7f, SpriteEffects.None, 0f);
            // Glow
            sb.Draw(glowTex, drawPos, null, PetalStormTextures.BloomGold * fade * 0.35f, 0f, glowOrigin,
                0.4f, SpriteEffects.None, 0f);
            // Core
            sb.Draw(glowTex, drawPos, null, PetalStormTextures.JubilantLight * fade * 0.4f, 0f, glowOrigin,
                0.12f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }

    /// <summary>
    /// PetalVortexZone — persistent spinning vortex AoE (2s).
    /// MaskFoundation-style rendering with CosmicVortex noise.
    /// ai[0] = seasonal color. Merges with nearby vortexes (+50% radius).
    /// </summary>
    public class PetalVortexZone : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int _timer;
        private float _radiusMultiplier = 1f;
        private bool _merged;

        public override void SetDefaults()
        {
            Projectile.width = 128;
            Projectile.height = 128;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120; // 2 seconds
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            _timer++;
            Projectile.velocity = Vector2.Zero;

            // Check for merge with nearby vortexes
            if (!_merged && _timer > 5)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile other = Main.projectile[i];
                    if (!other.active || other.whoAmI == Projectile.whoAmI) continue;
                    if (other.type != Projectile.type) continue;
                    if (Vector2.Distance(Projectile.Center, other.Center) < 100f * _radiusMultiplier)
                    {
                        _radiusMultiplier = MathHelper.Min(_radiusMultiplier + 0.5f, 4f);
                        _merged = true;
                        other.Kill();
                        Projectile.timeLeft = Math.Max(Projectile.timeLeft, 120);

                        // Merge VFX burst
                        for (int j = 0; j < 15; j++)
                        {
                            Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                            Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.GoldFlame, vel.X, vel.Y, 100, PetalStormTextures.BloomGold, 0.8f);
                            d.noGravity = true;
                        }
                        break;
                    }
                }
            }

            // Eye of the Storm buff for player inside
            Player owner = Main.player[Projectile.owner];
            float stormRadius = 64f * _radiusMultiplier;
            if (Vector2.Distance(Projectile.Center, owner.Center) < stormRadius)
            {
                // Simple buff: +8% damage via modifier would need a buff;
                // for now just spawn some visual healing particles
                if (Main.rand.NextBool(8))
                {
                    Dust d = Dust.NewDustDirect(owner.Center - new Vector2(4), 8, 8, DustID.GoldFlame, 0f, -1f, 100, PetalStormTextures.JubilantLight, 0.4f);
                    d.noGravity = true;
                }
            }

            // Ambient swirling petal dust
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat() * stormRadius;
                Vector2 pos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;
                float tangentAngle = angle + MathHelper.PiOver2;
                Vector2 vel = new Vector2((float)Math.Cos(tangentAngle), (float)Math.Sin(tangentAngle)) * 2f;
                Color seasonal = PetalStormTextures.GetSeasonalColor((int)Projectile.ai[0]);
                Dust d = Dust.NewDustDirect(pos, 1, 1, DustID.GoldFlame, vel.X, vel.Y, 120, seasonal, 0.6f);
                d.noGravity = true;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float stormRadius = 64f * _radiusMultiplier;
            Vector2 closestPoint = new Vector2(
                MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right),
                MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
            return Vector2.Distance(Projectile.Center, closestPoint) <= stormRadius;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D maskTex = PetalStormTextures.CircularMask;
            Texture2D glowTex = PetalStormTextures.SoftGlow;
            Texture2D ringTex = PetalStormTextures.OJPowerRing;
            Vector2 maskOrigin = maskTex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 ringOrigin = ringTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float fadeIn = MathHelper.Clamp(_timer / 10f, 0f, 1f);
            float fadeOut = MathHelper.Clamp(Projectile.timeLeft / 20f, 0f, 1f);
            float fade = fadeIn * fadeOut;
            float spin = _timer * 0.05f;
            float scale = 0.5f * _radiusMultiplier;
            float time = (float)Main.timeForVisualEffects * 0.015f;
            Color seasonal = PetalStormTextures.GetSeasonalColor((int)Projectile.ai[0]);

            sb.End();

            // ── LAYER 0: CelebrationAura shader — concentric vortex rings ──
            Effect auraShader = OdeToJoyShaders.CelebrationAura;
            if (auraShader != null)
            {
                float auraRadius = 0.25f + 0.05f * (float)Math.Sin(_timer * 0.06f);
                OdeToJoyShaders.SetAuraParams(auraShader, time + spin, seasonal,
                    PetalStormTextures.BloomGold, fade * 0.45f, 1.8f, auraRadius, 4f);
                OdeToJoyShaders.BeginShaderBatch(sb, auraShader, "CelebrationAuraTechnique");
                auraShader.CurrentTechnique.Passes["P0"].Apply();
                sb.Draw(glowTex, drawPos, null, Color.White * fade, spin, glowOrigin,
                    scale * 1.3f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: GardenBloom JubilantPulse shader — pulsing zone center ──
            Effect bloomShader = OdeToJoyShaders.GardenBloom;
            if (bloomShader != null)
            {
                OdeToJoyShaders.SetBloomParams(bloomShader, time, seasonal,
                    PetalStormTextures.RadiantAmber, fade * 0.35f, 2.0f, 0.4f);
                bloomShader.Parameters["uPulseSpeed"]?.SetValue(0.8f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, bloomShader, "JubilantPulseTechnique");
                sb.Draw(glowTex, drawPos, null, Color.White * fade, 0f, glowOrigin,
                    scale * 0.7f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 2: Additive overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            // Vortex base (spinning mask)
            sb.Draw(maskTex, drawPos, null, seasonal * fade * 0.2f, spin, maskOrigin, scale,
                SpriteEffects.None, 0f);
            // Counter-rotating overlay
            sb.Draw(maskTex, drawPos, null, PetalStormTextures.BloomGold * fade * 0.15f, -spin * 0.7f,
                maskOrigin, scale * 0.8f, SpriteEffects.None, 0f);
            // Power ring border
            sb.Draw(ringTex, drawPos, null, seasonal * fade * 0.3f, spin * 0.3f, ringOrigin,
                scale * 0.6f, SpriteEffects.None, 0f);
            // Soft outer glow
            sb.Draw(glowTex, drawPos, null, PetalStormTextures.BloomGold * fade * 0.15f, 0f, glowOrigin,
                scale * 1.5f, SpriteEffects.None, 0f);
            // Inner bright core
            sb.Draw(glowTex, drawPos, null, PetalStormTextures.JubilantLight * fade * 0.25f, 0f, glowOrigin,
                scale * 0.3f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }

    /// <summary>
    /// HurricaneShotProjectile — massive moving petal storm that sweeps the battlefield.
    /// Travels forward with persistent AoE. RibbonFoundation-style trail.
    /// </summary>
    public class HurricaneShotProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int TrailLength = 40;
        private Vector2[] _trailPositions = new Vector2[TrailLength];
        private int _trailHead;
        private int _timer;

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            _timer++;
            Projectile.rotation += 0.08f;

            _trailPositions[_trailHead] = Projectile.Center;
            _trailHead = (_trailHead + 1) % TrailLength;

            // Dense swirling petal debris
            for (int i = 0; i < 3; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat() * 40f;
                Vector2 pos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;
                Color seasonal = PetalStormTextures.GetSeasonalColor(i);
                Dust d = Dust.NewDustDirect(pos, 1, 1, DustID.GoldFlame, (float)Math.Cos(angle + MathHelper.PiOver2) * 3f, (float)Math.Sin(angle + MathHelper.PiOver2) * 3f, 100, seasonal, 0.9f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float radius = 60f;
            Vector2 closestPoint = new Vector2(
                MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right),
                MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
            return Vector2.Distance(Projectile.Center, closestPoint) <= radius;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glowTex = PetalStormTextures.SoftGlow;
            Texture2D maskTex = PetalStormTextures.CircularMask;
            Texture2D ringTex = PetalStormTextures.OJPowerRing;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 maskOrigin = maskTex.Size() / 2f;
            Vector2 ringOrigin = ringTex.Size() / 2f;

            float fade = MathHelper.Clamp(_timer / 10f, 0f, 1f) * MathHelper.Clamp(Projectile.timeLeft / 25f, 0f, 1f);
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: TriumphantTrail VertexStrip — 40-point hurricane ribbon ──
            Effect trailShader = OdeToJoyShaders.TriumphantTrail;
            int validCount = 0;
            for (int i = 0; i < TrailLength; i++)
            {
                int idx = (_trailHead - 1 - i + TrailLength) % TrailLength;
                if (_trailPositions[idx] != Vector2.Zero) validCount++; else break;
            }
            if (trailShader != null && validCount >= 2)
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
                    if (i < validCount - 1) rotations[i] = (positions[i + 1] - positions[i]).ToRotation();
                    else rotations[i] = rotations[Math.Max(0, i - 1)];
                }

                // Wide glow underlayer
                VertexStrip glowStrip = new VertexStrip();
                glowStrip.PrepareStrip(positions, rotations,
                    (float p) =>
                    {
                        Color c = PetalStormTextures.VortexColors[(int)(p * 2.99f) % PetalStormTextures.VortexColors.Length];
                        return c * fade * p * 0.25f;
                    },
                    (float p) => MathHelper.Lerp(4f, 30f, p),
                    -Main.screenPosition, includeBacksides: true);
                OdeToJoyShaders.SetTrailParams(trailShader, time, PetalStormTextures.PetalPink,
                    PetalStormTextures.BloomGold, fade * 0.5f, 1.8f);
                trailShader.CurrentTechnique = trailShader.Techniques["BlossomWindTrailTechnique"];
                trailShader.Parameters["WorldViewProjection"]?.SetValue(
                    Main.GameViewMatrix.NormalizedTransformationmatrix);
                trailShader.CurrentTechnique.Passes["P0"].Apply();
                glowStrip.DrawTrail();

                // Narrow core trail
                VertexStrip coreStrip = new VertexStrip();
                coreStrip.PrepareStrip(positions, rotations,
                    (float p) => PetalStormTextures.RadiantAmber * fade * p * 0.5f,
                    (float p) => MathHelper.Lerp(2f, 14f, p),
                    -Main.screenPosition, includeBacksides: true);
                OdeToJoyShaders.SetTrailParams(trailShader, time * 1.2f, PetalStormTextures.RadiantAmber,
                    PetalStormTextures.JubilantLight, fade * 0.7f, 2.0f);
                trailShader.CurrentTechnique = trailShader.Techniques["TriumphantTrailTechnique"];
                trailShader.CurrentTechnique.Passes["P0"].Apply();
                coreStrip.DrawTrail();
                Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            }

            // ── LAYER 1: PollenDrift shader — hurricane body ──
            Effect pollenShader = OdeToJoyShaders.PollenDrift;
            if (pollenShader != null)
            {
                float pollenRadius = 0.3f + 0.05f * (float)Math.Sin(_timer * 0.08f);
                OdeToJoyShaders.SetPollenParams(pollenShader, time, PetalStormTextures.PetalPink,
                    PetalStormTextures.BloomGold, fade * 0.4f, 2.0f, pollenRadius);
                pollenShader.CurrentTechnique = pollenShader.Techniques["BloomDetonationTechnique"];
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, pollenShader, "BloomDetonationTechnique");
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                sb.Draw(glowTex, drawPos, null, Color.White * fade, _timer * 0.06f, glowOrigin,
                    0.7f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 2: Additive overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            Vector2 pos = Projectile.Center - Main.screenPosition;

            // Main vortex body
            sb.Draw(maskTex, pos, null, PetalStormTextures.PetalPink * fade * 0.3f, _timer * 0.06f,
                maskOrigin, 0.8f, SpriteEffects.None, 0f);
            sb.Draw(maskTex, pos, null, PetalStormTextures.BloomGold * fade * 0.2f, -_timer * 0.04f,
                maskOrigin, 0.6f, SpriteEffects.None, 0f);
            // Power ring
            sb.Draw(ringTex, pos, null, PetalStormTextures.RadiantAmber * fade * 0.4f, _timer * 0.03f,
                ringOrigin, 0.5f, SpriteEffects.None, 0f);
            // Core glow
            sb.Draw(glowTex, pos, null, PetalStormTextures.PureJoyWhite * fade * 0.4f, 0f, glowOrigin,
                0.25f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }
}