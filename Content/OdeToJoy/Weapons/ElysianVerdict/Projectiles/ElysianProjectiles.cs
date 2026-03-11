using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
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

        private VertexStrip _vertexStrip;
        private const int TrailLength = 24;
        private Vector2[] _trail = new Vector2[TrailLength];
        private int _head;
        private int _timer;

        private bool IsParadiseLost => Projectile.ai[0] >= 1f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

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
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<ElysianJudgmentDust>(), -Projectile.velocity.X * 0.1f, -Projectile.velocity.Y * 0.1f, 100, c, 0.5f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            OdeToJoyVFXLibrary.SpawnGardenSparkleExplosion(target.Center, 3, 3f, 0.6f);

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
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _vertexStrip);

                // Elysian accent: jubilant golden cross-flare
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null)
                {
                    Vector2 origin = glow.Size() / 2f;
                    float pulse = 0.8f + 0.2f * MathF.Sin((float)Main.timeForVisualEffects * 0.1f);
                    float scale = IsParadiseLost ? 1.3f : 1f;

                    // Golden jubilant cross
                    float rot = Projectile.velocity.ToRotation();
                    sb.Draw(glow, drawPos, null,
                        (OdeToJoyPalette.GoldenPollen with { A = 0 }) * 0.22f * pulse * scale,
                        rot, origin, new Vector2(0.12f * scale, 0.025f), SpriteEffects.None, 0f);
                    sb.Draw(glow, drawPos, null,
                        (OdeToJoyPalette.WarmAmber with { A = 0 }) * 0.15f * pulse * scale,
                        rot + MathHelper.PiOver2, origin, new Vector2(0.06f * scale, 0.018f), SpriteEffects.None, 0f);
                }

                sb.End();
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
                    Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, ModContent.DustType<ElysianJudgmentDust>(), vel.X, vel.Y, 60, c, 1.1f);
                    d.noGravity = true;
                    d.fadeIn = 1.5f;
                }
            }

            // Elysian Verdict detonation screen effects
            if (_timer == 1)
            {
                OdeToJoyVFXLibrary.ScreenShake(10f, 20);
                OdeToJoyVFXLibrary.ScreenFlash(OdeToJoyPalette.GoldenPollen, 1.4f);
                OdeToJoyVFXLibrary.HarmonicPulseRing(Projectile.Center, 1.8f, 16, OdeToJoyPalette.GoldenPollen);
                OdeToJoyVFXLibrary.SpawnTriumphantStarburst(Projectile.Center);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
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
            // Core flash (3-tier bloom — capped for visual sanity)
            float flashFade = MathHelper.Clamp(1f - _timer / 12f, 0f, 1f);
            sb.Draw(glow, pos, null, primary * flashFade * 0.4f, 0f, glowO, 0.8f,
                SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, ElysianTextures.PureJoyWhite * flashFade * 0.6f, 0f, glowO, 0.5f,
                SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, bright * flashFade * 0.45f, 0f, glowO, 0.25f,
                SpriteEffects.None, 0f);

            // Star4Soft sparkle ring — celebratory 4-point star accents
            Texture2D starTex = MagnumTextureRegistry.GetStar4Soft();
            if (starTex != null && flashFade > 0.05f)
            {
                Vector2 starOrigin = starTex.Size() / 2f;
                float starRot = _timer * 0.15f;
                float starScale = 0.14f * flashFade;
                sb.Draw(starTex, pos, null, primary * flashFade * 0.55f, starRot,
                    starOrigin, starScale, SpriteEffects.None, 0f);
                sb.Draw(starTex, pos, null, ElysianTextures.PureJoyWhite * flashFade * 0.35f, -starRot * 0.7f,
                    starOrigin, starScale * 0.65f, SpriteEffects.None, 0f);
            }

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
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
    }
}
