using MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Projectiles
{
    /// <summary>
    /// ElysianOrbProjectile — Golden judgment orb with prismatic edges.
    /// Applies Elysian Marks on hit. Crits = 2 marks. 3 marks = Verdict detonation.
    /// ai[0] = Paradise Lost flag (1 = corrupted, 0 = normal).
    /// MagicOrbFoundation-style rendering.
    /// </summary>
    public class ElysianOrbProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int TrailLength = 24;
        private Vector2[] _trail = new Vector2[TrailLength];
        private int _head;
        private int _timer;

        private bool IsParadiseLost => Projectile.ai[0] >= 1f;

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            _timer++;
            _trail[_head] = Projectile.Center;
            _head = (_head + 1) % TrailLength;
            Projectile.rotation += 0.06f;

            // Prismatic edge shimmer particles
            if (Main.rand.NextBool(2))
            {
                Color c = IsParadiseLost ? ElysianTextures.CrimsonEdge : ElysianTextures.BloomGold;
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, -Projectile.velocity.X * 0.1f, -Projectile.velocity.Y * 0.1f, 100, c, 0.5f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var markNPC = target.GetGlobalNPC<ElysianMarkNPC>();
            bool detonate = markNPC.AddMark(target, hit.Crit);

            if (detonate)
            {
                // Elysian Verdict detonation!
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                        ModContent.ProjectileType<ElysianVerdictExplosion>(), Projectile.damage * 2, 10f, Projectile.owner, IsParadiseLost ? 1f : 0f);
                }

                // Heal player 10% of damage (unless Paradise Lost)
                if (!IsParadiseLost)
                {
                    Player owner = Main.player[Projectile.owner];
                    int healAmount = (int)(damageDone * 0.10f);
                    owner.Heal(Math.Max(1, healAmount));
                }

                markNPC.ResetMarks();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ElysianTextures.SoftGlow;
            Texture2D sparkle = ElysianTextures.OJBlossomSparkle;
            Vector2 glowOrigin = glow.Size() / 2f;
            Vector2 sparkleOrigin = sparkle.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            float fadeIn = MathHelper.Clamp(_timer / 8f, 0f, 1f);
            float fadeOut = MathHelper.Clamp(Projectile.timeLeft / 20f, 0f, 1f);
            float fade = fadeIn * fadeOut;
            float pulse = 0.85f + 0.15f * (float)Math.Sin(_timer * 0.1f);
            float time = (float)Main.timeForVisualEffects * 0.015f;

            Color orbColor = ElysianTextures.GetOrbColor(IsParadiseLost, 0.3f);
            Color coreColor = IsParadiseLost ? ElysianTextures.CrimsonEdge : ElysianTextures.JubilantLight;
            Color edgeColor = IsParadiseLost ? new Color(180, 40, 40) : ElysianTextures.PureJoyWhite;
            Color accentColor = IsParadiseLost ? new Color(120, 30, 30) : new Color(90, 200, 60);

            sb.End();

            // ── LAYER 0: TriumphantTrail shader trail (new) ──
            Effect trailShader = OdeToJoyShaders.TriumphantTrail;
            if (trailShader != null)
            {
                int validCount = 0;
                for (int i = 0; i < TrailLength; i++)
                {
                    int idx = (_head - 1 - i + TrailLength) % TrailLength;
                    if (_trail[idx] != Vector2.Zero) validCount++;
                    else break;
                }
                if (validCount >= 2)
                {
                    Vector2[] positions = new Vector2[validCount];
                    float[] rotations = new float[validCount];
                    for (int i = 0; i < validCount; i++)
                    {
                        int idx = (_head - 1 - i + TrailLength) % TrailLength;
                        positions[validCount - 1 - i] = _trail[idx];
                    }
                    for (int i = 0; i < validCount; i++)
                    {
                        if (i < validCount - 1)
                            rotations[i] = (positions[i + 1] - positions[i]).ToRotation();
                        else
                            rotations[i] = rotations[Math.Max(0, i - 1)];
                    }

                    Terraria.Graphics.VertexStrip strip = new Terraria.Graphics.VertexStrip();
                    strip.PrepareStrip(positions, rotations,
                        (float p) => orbColor * fade * p * 0.5f,
                        (float p) => MathHelper.Lerp(1f, 12f, p),
                        -Main.screenPosition, includeBacksides: true);

                    OdeToJoyShaders.SetTrailParams(trailShader, time, orbColor,
                        accentColor, fade * 0.6f, 1.4f);
                    trailShader.CurrentTechnique = trailShader.Techniques["TriumphantTrailTechnique"];
                    trailShader.Parameters["WorldViewProjection"]?.SetValue(
                        Main.GameViewMatrix.NormalizedTransformationmatrix);
                    trailShader.CurrentTechnique.Passes["P0"].Apply();
                    strip.DrawTrail();
                    Main.pixelShader.CurrentTechnique.Passes[0].Apply();
                }
            }

            // ── LAYER 1: GardenBloom shader orb body (new) ──
            Effect bloomShader = OdeToJoyShaders.GardenBloom;
            if (bloomShader != null)
            {
                OdeToJoyShaders.SetBloomParams(bloomShader, time, orbColor,
                    accentColor, fade * 0.6f * pulse, 2.0f, 0.4f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, bloomShader, "GardenBloomTechnique");
                sb.Draw(glow, pos, null, Color.White * fade * pulse, 0f, glowOrigin,
                    0.45f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 2: Additive bloom overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            // Bloom trail (lighter alongside shader trail)
            for (int i = 0; i < TrailLength; i++)
            {
                int idx = (_head - 1 - i + TrailLength) % TrailLength;
                if (_trail[idx] == Vector2.Zero) continue;
                float t = 1f - i / (float)TrailLength;
                sb.Draw(glow, _trail[idx] - Main.screenPosition, null, orbColor * fade * t * 0.15f,
                    0f, glowOrigin, 0.1f * t, SpriteEffects.None, 0f);
            }

            // Outer prismatic glow
            sb.Draw(glow, pos, null, orbColor * fade * 0.35f * pulse, 0f, glowOrigin, 0.45f,
                SpriteEffects.None, 0f);
            // Sparkle body
            sb.Draw(sparkle, pos, null, orbColor * fade * 0.6f, Projectile.rotation, sparkleOrigin,
                0.35f, SpriteEffects.None, 0f);
            // Inner core
            sb.Draw(glow, pos, null, coreColor * fade * 0.45f, 0f, glowOrigin, 0.18f,
                SpriteEffects.None, 0f);
            // Hot center
            sb.Draw(glow, pos, null, edgeColor * fade * 0.35f, 0f, glowOrigin, 0.06f,
                SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }

    /// <summary>
    /// ElysianVerdictExplosion — Massive golden detonation at 3 marks.
    /// ImpactFoundation + ExplosionParticles pattern.
    /// ai[0] = Paradise Lost flag.
    /// </summary>
    public class ElysianVerdictExplosion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";
        private int _timer;
        private bool IsParadiseLost => Projectile.ai[0] >= 1f;

        public override void SetDefaults()
        {
            Projectile.width = 250;
            Projectile.height = 250;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 45;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            _timer++;
            Projectile.velocity = Vector2.Zero;

            // Radial golden sunburst particles
            if (_timer <= 10)
            {
                int particleCount = IsParadiseLost ? 65 : 55;
                for (int i = 0; i < particleCount / 10; i++)
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    float speed = 4f + Main.rand.NextFloat() * 6f;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                    Color c = IsParadiseLost
                        ? Color.Lerp(ElysianTextures.CrimsonEdge, ElysianTextures.CorruptedGold, Main.rand.NextFloat())
                        : Color.Lerp(ElysianTextures.BloomGold, ElysianTextures.PureJoyWhite, Main.rand.NextFloat());
                    Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.GoldFlame, vel.X, vel.Y, 60, c, 1.1f);
                    d.noGravity = true;
                    d.fadeIn = 1.5f;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ElysianTextures.SoftGlow;
            Texture2D ring = ElysianTextures.OJPowerRing;
            Texture2D impact = ElysianTextures.OJHarmonicImpact;
            Texture2D floral = ElysianTextures.OJFloralImpact;
            Vector2 glowO = glow.Size() / 2f;
            Vector2 ringO = ring.Size() / 2f;
            Vector2 impactO = impact.Size() / 2f;
            Vector2 floralO = floral.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            float fade = MathHelper.Clamp(1f - _timer / 45f, 0f, 1f);
            float expand = _timer * 0.04f;
            float time = (float)Main.timeForVisualEffects * 0.015f;

            Color primary = IsParadiseLost ? ElysianTextures.CrimsonEdge : ElysianTextures.BloomGold;
            Color secondary = IsParadiseLost ? ElysianTextures.CorruptedGold : ElysianTextures.RadiantAmber;
            Color bright = IsParadiseLost ? new Color(220, 80, 60) : ElysianTextures.JubilantLight;
            Color accent = IsParadiseLost ? new Color(120, 30, 30) : new Color(90, 200, 60);

            sb.End();

            // ── LAYER 0: CelebrationAura shader — expanding rings of judgment ──
            Effect auraShader = OdeToJoyShaders.CelebrationAura;
            if (auraShader != null)
            {
                float auraRadius = 0.1f + _timer * 0.01f;
                OdeToJoyShaders.SetAuraParams(auraShader, time + _timer * 0.05f, primary, accent,
                    fade * 0.65f, 2.2f, auraRadius, 5f);
                OdeToJoyShaders.BeginShaderBatch(sb, auraShader, "CelebrationAuraTechnique");
                auraShader.CurrentTechnique.Passes["P0"].Apply();
                float shaderScale = 0.06f + expand * 0.25f;
                sb.Draw(glow, pos, null, Color.White * fade, 0f, glowO, shaderScale, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive texture overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            // Expanding ring
            sb.Draw(ring, pos, null, primary * fade * 0.4f, _timer * 0.05f, ringO,
                (0.3f + expand) * 0.8f, SpriteEffects.None, 0f);
            // Harmonic impact overlay
            sb.Draw(impact, pos, null, secondary * fade * 0.35f, -_timer * 0.03f, impactO,
                (0.3f + expand * 0.7f), SpriteEffects.None, 0f);
            // Floral burst
            sb.Draw(floral, pos, null, primary * fade * 0.3f, _timer * 0.02f, floralO,
                (0.25f + expand * 0.5f), SpriteEffects.None, 0f);
            // Core flash (3-tier bloom)
            float flashFade = MathHelper.Clamp(1f - _timer / 12f, 0f, 1f);
            sb.Draw(glow, pos, null, primary * flashFade * 0.4f, 0f, glowO, 1.6f,
                SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, ElysianTextures.PureJoyWhite * flashFade * 0.6f, 0f, glowO, 1.0f,
                SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, bright * flashFade * 0.45f, 0f, glowO, 0.4f,
                SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }
}