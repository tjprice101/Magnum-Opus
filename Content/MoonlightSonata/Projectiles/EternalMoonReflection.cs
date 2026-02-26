using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Ghost swing mirror projectile for Eternal Moon Phase 2 ("Half Moon" — Equilibrium).
    /// Creates a translucent mirrored copy of the swing arc that trails behind the main slash
    /// and fades out over time, giving the swing a lingering ethereal afterimage.
    /// 30% of parent swing damage, penetrates 3 times with local iframes.
    /// </summary>
    public class EternalMoonReflection : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/CurvedSwordSlash";

        private float CurveDirection
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private float InitialAngle
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private int _timer;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 25;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 44;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 40;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 100;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.scale = 0.85f;
        }

        public override void AI()
        {
            _timer++;

            // Set initial angle on first frame
            if (_timer == 1)
                InitialAngle = Projectile.velocity.ToRotation();

            // Gentle mirrored arc — sweeps in the direction opposite to parent swing
            float curveRate = 0.03f * CurveDirection;
            Projectile.velocity = Projectile.velocity.RotatedBy(curveRate);
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Gradual deceleration after 15 frames
            if (_timer > 15)
                Projectile.velocity *= 0.97f;

            // Gradual fade out — alpha increases by 4 each frame
            Projectile.alpha += 4;
            if (Projectile.alpha >= 255)
            {
                Projectile.alpha = 255;
                Projectile.Kill();
                return;
            }

            // Scale pulse — gentle breathing
            Projectile.scale = 0.85f + MathF.Sin(_timer * 0.12f) * 0.08f;

            // Fade factor used for dust/lighting intensity
            float fadeFactor = 1f - Projectile.alpha / 255f;

            // TidalMoonDust trail — semi-transparent tidal particles every 3 frames
            if (_timer % 3 == 0)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(8f, 8f);
                Color dustColor = EternalMoonVFX.GetLunarPhaseColor(Main.rand.NextFloat(), 1) * fadeFactor;
                Dust tidal = Dust.NewDustPerfect(
                    Projectile.Center + dustOffset,
                    ModContent.DustType<TidalMoonDust>(),
                    -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.8f, 0.8f),
                    0, dustColor, 0.25f);
                tidal.customData = new TidalMoonBehavior(2f, 20);
            }

            // LunarMote crescent sparkle orbiting projectile center every 8 frames
            if (_timer % 8 == 0)
            {
                Color moteColor = Color.Lerp(MoonlightVFXLibrary.Violet,
                    MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat()) * fadeFactor;
                Dust mote = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<LunarMote>(),
                    -Projectile.velocity * 0.04f,
                    0, moteColor, 0.25f);
                mote.customData = new LunarMoteBehavior(Projectile.Center,
                    Main.rand.NextFloat(MathHelper.TwoPi))
                {
                    OrbitRadius = 8f,
                    OrbitSpeed = 0.07f,
                    Lifetime = 18,
                    FadePower = 0.88f
                };
            }

            // Dynamic lighting — Violet * 0.4f with gentle pulse
            float pulse = 0.4f + MathF.Sin(Main.GlobalTimeWrappedHourly * 6f) * 0.1f;
            Lighting.AddLight(Projectile.Center, MoonlightVFXLibrary.Violet.ToVector3() * pulse * fadeFactor);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float fadeFactor = 1f - Projectile.alpha / 255f;

            // === GHOST TRAIL ===
            if (Projectile.oldPos.Length > 1)
            {
                Vector2[] trailPos = new Vector2[Projectile.oldPos.Length];
                float[] trailRot = new float[Projectile.oldPos.Length];
                int validCount = 0;

                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) break;
                    trailPos[i] = Projectile.oldPos[i] + Projectile.Size / 2f;
                    trailRot[i] = Projectile.oldRot[i];
                    validCount++;
                }

                if (validCount > 1)
                {
                    if (validCount < trailPos.Length)
                    {
                        Array.Resize(ref trailPos, validCount);
                        Array.Resize(ref trailRot, validCount);
                    }

                    CalamityStyleTrailRenderer.DrawTrailWithBloom(
                        trailPos, trailRot,
                        CalamityStyleTrailRenderer.TrailStyle.Cosmic,
                        baseWidth: 12f,
                        primaryColor: MoonlightVFXLibrary.DarkPurple,
                        secondaryColor: MoonlightVFXLibrary.IceBlue,
                        intensity: 0.5f * fadeFactor,
                        bloomMultiplier: 1.8f);
                }
            }

            // === BLOOM ORB — 3-layer center glow ===
            var bloomTex = MagnumTextureRegistry.GetBloom();
            if (bloomTex != null)
            {
                Vector2 bloomOrigin = bloomTex.Size() * 0.5f;
                float bPulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.1f;
                float bloomScale = 0.2f * bPulse * Projectile.scale;

                // Layer 1: Outer dark purple haze
                sb.Draw(bloomTex, drawPos, null,
                    (MoonlightVFXLibrary.DarkPurple with { A = 0 }) * 0.15f * fadeFactor,
                    0f, bloomOrigin, bloomScale * 2.0f, SpriteEffects.None, 0f);

                // Layer 2: Mid violet glow
                sb.Draw(bloomTex, drawPos, null,
                    (MoonlightVFXLibrary.Violet with { A = 0 }) * 0.3f * fadeFactor,
                    0f, bloomOrigin, bloomScale * 1.2f, SpriteEffects.None, 0f);

                // Layer 3: Inner ice blue core
                sb.Draw(bloomTex, drawPos, null,
                    (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.5f * fadeFactor,
                    0f, bloomOrigin, bloomScale * 0.6f, SpriteEffects.None, 0f);
            }

            // === GHOST SLASH SPRITE ===
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;

            sb.Draw(texture, drawPos, null,
                (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.5f * fadeFactor,
                Projectile.rotation, origin, Projectile.scale,
                CurveDirection < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Musics Dissonance debuff for 120 frames
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 120);

            // Small tidal dust burst — 4 dusts in a circle
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                Color dustColor = Color.Lerp(MoonlightVFXLibrary.Violet,
                    MoonlightVFXLibrary.IceBlue, (float)i / 4f);
                Dust tidal = Dust.NewDustPerfect(target.Center,
                    ModContent.DustType<TidalMoonDust>(),
                    vel, 0, dustColor, 0.25f);
                tidal.customData = new TidalMoonBehavior(2f, 16);
            }

            // Impact flare
            CustomParticles.GenericFlare(target.Center, MoonlightVFXLibrary.IceBlue, 0.25f, 12);
        }

        public override void OnKill(int timeLeft)
        {
            // Death burst — 6 TidalMoonDust in a circle
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                Color tidalColor = Color.Lerp(MoonlightVFXLibrary.Violet,
                    MoonlightVFXLibrary.IceBlue, (float)i / 6f);
                Dust tidal = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<TidalMoonDust>(),
                    vel, 0, tidalColor, 0.3f);
                tidal.customData = new TidalMoonBehavior(2.5f, 18);
            }

            // Violet farewell flare
            CustomParticles.GenericFlare(Projectile.Center,
                MoonlightVFXLibrary.Violet, 0.2f, 10);
        }
    }
}
