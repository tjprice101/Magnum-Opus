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
    /// Crescent tidal wash projectile — sweeps in a curved arc like a wave crashing.
    /// Inspired by Coralite's NoctiflairStrike curved flight paths and Calamity's
    /// Ark of Cosmos curved energy slashes.
    /// Curves perpendicular to initial direction, creating a sweeping crescent wash
    /// that covers a wide area with tidal moon energy.
    /// </summary>
    public class EternalMoonTidalWash : ModProjectile
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
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 50;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.scale = 0.7f;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            _timer++;

            // Set initial angle on first frame
            if (_timer == 1)
                InitialAngle = Projectile.velocity.ToRotation();

            // Curved flight path — sweeps in an arc like a tidal wave crashing
            float curveRate = 0.04f * CurveDirection;
            Projectile.velocity = Projectile.velocity.RotatedBy(curveRate);
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Slight deceleration over time
            if (_timer > 20)
                Projectile.velocity *= 0.98f;

            // Scale pulse — breathing wave
            Projectile.scale = 0.7f + MathF.Sin(_timer * 0.15f) * 0.1f;

            // TidalMoonDust trail — flowing water particles
            if (Main.rand.NextBool(2))
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(10f, 10f);
                Color dustColor = EternalMoonVFX.GetLunarPhaseColor(Main.rand.NextFloat(), 1);
                Dust tidal = Dust.NewDustPerfect(
                    Projectile.Center + dustOffset,
                    ModContent.DustType<TidalMoonDust>(),
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    0, dustColor, 0.3f);
                tidal.customData = new TidalMoonBehavior(2.5f, 25);
            }

            // LunarMote crescent sparkles along path
            if (_timer % 6 == 0)
            {
                Color moteColor = Color.Lerp(MoonlightVFXLibrary.Violet,
                    MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                Dust mote = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<LunarMote>(),
                    -Projectile.velocity * 0.05f,
                    0, moteColor, 0.3f);
                mote.customData = new LunarMoteBehavior(Projectile.Center,
                    Main.rand.NextFloat(MathHelper.TwoPi))
                {
                    OrbitRadius = 6f,
                    OrbitSpeed = 0.08f,
                    Lifetime = 20,
                    FadePower = 0.9f
                };
            }

            // Lighting
            float pulse = 0.5f + MathF.Sin(Main.GlobalTimeWrappedHourly * 6f) * 0.15f;
            Lighting.AddLight(Projectile.Center, MoonlightVFXLibrary.Violet.ToVector3() * pulse);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // === CURVED TRAIL ===
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
                        baseWidth: 16f,
                        primaryColor: MoonlightVFXLibrary.Violet,
                        secondaryColor: MoonlightVFXLibrary.IceBlue,
                        intensity: 0.7f,
                        bloomMultiplier: 2.0f);
                }
            }

            // === BLOOM BODY ===
            var bloomTex = MagnumTextureRegistry.GetBloom();
            if (bloomTex != null)
            {
                Vector2 bloomOrigin = bloomTex.Size() * 0.5f;
                float bPulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.12f;
                float bloomScale = 0.25f * bPulse * Projectile.scale;

                sb.Draw(bloomTex, drawPos, null,
                    (MoonlightVFXLibrary.DarkPurple with { A = 0 }) * 0.25f,
                    0f, bloomOrigin, bloomScale * 2.5f, SpriteEffects.None, 0f);
                sb.Draw(bloomTex, drawPos, null,
                    (MoonlightVFXLibrary.Violet with { A = 0 }) * 0.45f,
                    0f, bloomOrigin, bloomScale * 1.6f, SpriteEffects.None, 0f);
                sb.Draw(bloomTex, drawPos, null,
                    (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.6f,
                    0f, bloomOrigin, bloomScale * 0.9f, SpriteEffects.None, 0f);
                sb.Draw(bloomTex, drawPos, null,
                    (Color.White with { A = 0 }) * 0.7f,
                    0f, bloomOrigin, bloomScale * 0.35f, SpriteEffects.None, 0f);
            }

            // === CRESCENT SLASH SPRITE ===
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;

            sb.Draw(texture, drawPos, null,
                (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.7f,
                Projectile.rotation, origin, Projectile.scale,
                CurveDirection < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);
            sb.Draw(texture, drawPos, null,
                (Color.White with { A = 0 }) * 0.3f,
                Projectile.rotation, origin, Projectile.scale * 0.6f,
                CurveDirection < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 180);

            // Tidal impact — crescent pulse dust ring
            Dust pulse = Dust.NewDustPerfect(target.Center,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0,
                MoonlightVFXLibrary.Violet, 0.5f);
            pulse.customData = new ResonantPulseBehavior(0.04f, 18);

            CustomParticles.GenericFlare(target.Center, MoonlightVFXLibrary.IceBlue, 0.35f, 14);
        }

        public override void OnKill(int timeLeft)
        {
            // Death: tidal splash — fan of tidal dust
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color tidalColor = Color.Lerp(MoonlightVFXLibrary.Violet,
                    MoonlightVFXLibrary.IceBlue, (float)i / 8f);
                Dust tidal = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<TidalMoonDust>(),
                    vel, 0, tidalColor, 0.35f);
                tidal.customData = new TidalMoonBehavior(3f, 20);
            }

            CustomParticles.GenericFlare(Projectile.Center,
                MoonlightVFXLibrary.Violet, 0.25f, 12);
        }
    }
}
