using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Spectral child beam — spawned when the parent MoonlightBeam bounces on surfaces (bounces 3+).
    /// Each child has a fixed spectral hue (ai[0]) that determines its unique color, making it
    /// visually distinct from the parent beam and other children.
    ///
    /// Short-lived, non-bouncing beam that carries a slice of the parent's prismatic spectrum.
    /// Dies on tile contact rather than reflecting, ensuring clean visual separation from
    /// the parent beam's bouncing behavior.
    ///
    /// ai[0] = spectral hue (0-1 float), ai[1] = bounce count inherited from parent.
    /// </summary>
    public class SpectralChildBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow3";

        /// <summary>
        /// The fixed spectral hue assigned at spawn time (0-1 range).
        /// </summary>
        private float SpectralHue => Projectile.ai[0];

        /// <summary>
        /// Bounce count inherited from the parent MoonlightBeam at the time of splitting.
        /// </summary>
        private int ParentBounceCount => (int)Projectile.ai[1];

        /// <summary>
        /// Returns the beam's unique color derived from its fixed spectral hue.
        /// </summary>
        private Color SpectralColor => Main.hslToRgb(SpectralHue, 0.8f, 0.7f);

        private int frameCounter = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 15;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 75;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Spectral children do not bounce — they die on tile contact
            return true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Color spectralColor = SpectralColor;

            frameCounter++;

            // PrismaticShardDust core trail — colored by this beam's fixed hue
            if (Main.rand.NextBool(3))
            {
                Dust shard = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<PrismaticShardDust>(),
                    -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, spectralColor, 0.18f);
                shard.customData = new PrismaticShardBehavior
                {
                    BaseHue = SpectralHue,
                    HueRange = 0.08f,
                    RotationSpeed = 0.1f,
                    BaseScale = 0.18f,
                    Lifetime = 18
                };
            }

            // Occasional LunarMote — crescent notes in this beam's hue
            if (frameCounter % 10 == 0)
            {
                float orbitPhase = frameCounter * 0.25f;
                Color moteColor = Color.Lerp(spectralColor, Color.White, 0.2f);
                Dust mote = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<LunarMote>(),
                    -Projectile.velocity * 0.02f,
                    0, moteColor, 0.2f);
                mote.customData = new LunarMoteBehavior(Projectile.Center, orbitPhase)
                {
                    OrbitRadius = 6f,
                    OrbitSpeed = 0.12f,
                    Lifetime = 16,
                    FadePower = 0.9f
                };
            }

            // StarPointDust sharp twinkles along the path
            if (frameCounter % 7 == 0)
            {
                float orbitPhase = frameCounter * 0.3f;
                for (int i = 0; i < 2; i++)
                {
                    float angle = orbitPhase + MathHelper.Pi * i;
                    float radius = 5f + MathF.Sin(orbitPhase + i) * 2.5f;
                    Vector2 starPos = Projectile.Center + angle.ToRotationVector2() * radius;
                    Color starColor = Color.Lerp(spectralColor, Color.White, 0.3f * i);
                    Dust star = Dust.NewDustPerfect(starPos,
                        ModContent.DustType<StarPointDust>(),
                        -Projectile.velocity * 0.03f, 0, starColor, 0.13f);
                    star.customData = new StarPointBehavior
                    {
                        RotationSpeed = 0.15f,
                        TwinkleFrequency = 0.5f,
                        Lifetime = 14,
                        FadeStartTime = 4
                    };
                }
            }

            // Dynamic lighting matching spectral hue
            Vector3 lightVec = spectralColor.ToVector3();
            Lighting.AddLight(Projectile.Center, lightVec * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Debuffs.MusicsDissonance>(), 120);

            // Impact VFX via shared MoonlightsCallingVFX system
            MoonlightsCallingVFX.OnHitImpact(target.Center, ParentBounceCount, hit.Crit);
        }

        public override void OnKill(int timeLeft)
        {
            Color spectralColor = SpectralColor;

            // Prismatic shard scatter on death
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3.5f);
                Color shardColor = Color.Lerp(spectralColor, Color.White, i / 8f);
                Dust shard = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<PrismaticShardDust>(),
                    vel, 0, shardColor, 0.2f);
                shard.customData = new PrismaticShardBehavior(SpectralHue + i * 0.05f, 0.12f, 16);
            }

            // ResonantPulseDust ring on death
            Dust pulse = Dust.NewDustPerfect(Projectile.Center,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0, spectralColor, 0.18f);
            pulse.customData = new ResonantPulseBehavior(0.03f, 12);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Color spectralColor = SpectralColor;

            // === CALAMITY-STYLE TRAIL RENDERING ===
            if (Projectile.oldPos.Length > 1)
            {
                Vector2[] trailPositions = new Vector2[Projectile.oldPos.Length];
                float[] trailRotations = new float[Projectile.oldPos.Length];
                int validCount = 0;

                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) break;
                    trailPositions[i] = Projectile.oldPos[i] + Projectile.Size / 2f;
                    trailRotations[i] = Projectile.oldRot[i];
                    validCount++;
                }

                if (validCount > 1)
                {
                    if (validCount < trailPositions.Length)
                    {
                        Array.Resize(ref trailPositions, validCount);
                        Array.Resize(ref trailRotations, validCount);
                    }

                    CalamityStyleTrailRenderer.DrawTrailWithBloom(
                        trailPositions, trailRotations,
                        CalamityStyleTrailRenderer.TrailStyle.Ice,
                        baseWidth: 8f,
                        primaryColor: spectralColor,
                        secondaryColor: Color.Lerp(spectralColor, Color.White, 0.3f),
                        intensity: 0.7f,
                        bloomMultiplier: 1.8f);
                }
            }

            // === BLOOM BODY ===
            MoonlightsCallingVFX.DrawBeamBloom(sb, Projectile.Center, ParentBounceCount);

            // === MOTION BLUR BLOOM (velocity-based stretch) ===
            if (Projectile.velocity.LengthSquared() > 4f)
            {
                Texture2D texture = TextureAssets.Projectile[Type].Value;
                Color secondaryColor = Color.Lerp(spectralColor, Color.White, 0.4f);
                MotionBlurBloomRenderer.DrawProjectile(sb, texture, Projectile,
                    spectralColor, secondaryColor,
                    intensityMult: 0.35f);
            }

            return false;
        }
    }
}
