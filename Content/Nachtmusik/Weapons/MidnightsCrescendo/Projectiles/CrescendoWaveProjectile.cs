using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Content.Nachtmusik.Weapons.MidnightsCrescendo.Utilities;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.Nachtmusik.Weapons.MidnightsCrescendo.Projectiles
{
    /// <summary>
    /// Crescendo Wave — expanding crescent wave arc released at 8+ crescendo stacks.
    /// Grows over lifetime (scale 1.0 → 2.0), intensity driven by ai[0] (stack ratio 0-1).
    /// Cosmic Blue → Starlight Silver → Stellar White progression based on intensity.
    /// Dense cosmic dust trail, music notes, halo rings on hit, dissipation burst on kill.
    /// </summary>
    public class CrescendoWaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/ImpactEllipse";

        #region Theme Colors

        private static readonly Color NightVoid = new Color(10, 10, 30);
        private static readonly Color DeepIndigo = new Color(40, 30, 100);
        private static readonly Color CosmicBlue = new Color(60, 80, 180);
        private static readonly Color StarlightSilver = new Color(180, 200, 230);
        private static readonly Color MoonPearl = new Color(220, 225, 245);
        private static readonly Color StellarWhite = new Color(240, 245, 255);

        #endregion

        #region Properties

        /// <summary>Stack intensity (0-1), stored in ai[0]. Controls brightness and width.</summary>
        private float StackIntensity => Projectile.ai[0];

        /// <summary>Lifetime progress (0 = just spawned, 1 = about to die).</summary>
        private float LifetimeProgress => 1f - (Projectile.timeLeft / 60f);

        /// <summary>Dynamic scale: grows from 1.0 to 2.0 over lifetime.</summary>
        private float DynamicScale => 1f + LifetimeProgress;

        /// <summary>Get the current wave color based on intensity and lifetime.</summary>
        private Color GetWaveColor(float t)
        {
            Color baseColor = Color.Lerp(CosmicBlue, StarlightSilver, t * StackIntensity);
            Color brightColor = Color.Lerp(StarlightSilver, StellarWhite, StackIntensity);
            return Color.Lerp(baseColor, brightColor, LifetimeProgress * 0.4f);
        }

        #endregion

        #region Setup

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.alpha = 0;
            Projectile.scale = 1f;
            Projectile.extraUpdates = 1;
        }

        #endregion

        #region AI

        public override void AI()
        {
            // Expand hitbox as wave grows
            float scaleFactor = DynamicScale;
            Projectile.width = (int)(60 * scaleFactor);
            Projectile.height = (int)(60 * scaleFactor);

            // Rotation follows velocity
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Slight drag so it doesn't fly forever
            Projectile.velocity *= 0.985f;

            // Fade in alpha near end
            if (Projectile.timeLeft < 15)
                Projectile.alpha = (int)(255 * (1f - Projectile.timeLeft / 15f));

            // === COSMIC DUST TRAIL ===
            int trailCount = 2 + (int)(StackIntensity * 4);
            for (int i = 0; i < trailCount; i++)
            {
                float dp = Main.rand.NextFloat();
                Color dc = Color.Lerp(DeepIndigo, Color.Lerp(StarlightSilver, StellarWhite, StackIntensity), dp);
                Vector2 dustOffset = Main.rand.NextVector2Circular(20f * scaleFactor, 20f * scaleFactor);
                Vector2 dustVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 2f)
                    + Main.rand.NextVector2Circular(1f, 1f);
                Dust d = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.PurpleTorch,
                    dustVel, 0, dc, 1.0f + StackIntensity * 0.6f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // === MUSIC NOTES — trailing behind the wave ===
            if (Main.rand.NextBool(Math.Max(2, 5 - (int)(StackIntensity * 3))))
            {
                Vector2 noteVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * 1f
                    + new Vector2(Main.rand.NextFloat(-1f, 1f), -0.8f);
                MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), noteVel,
                    hueMin: 0.58f, hueMax: 0.73f,
                    saturation: 0.7f, luminosity: 0.6f + StackIntensity * 0.15f,
                    scale: 0.7f + StackIntensity * 0.2f, lifetime: 24, hueSpeed: 0.025f));
            }

            // === EDGE SHIMMER — starlight sparkles along wave edges ===
            if (Main.rand.NextBool(3))
            {
                float edgeAngle = Projectile.rotation + MathHelper.PiOver2 * (Main.rand.NextBool() ? 1 : -1);
                Vector2 edgePos = Projectile.Center + edgeAngle.ToRotationVector2() * 25f * scaleFactor;
                Color edgeColor = Color.Lerp(MoonPearl, StellarWhite, Main.rand.NextFloat());
                Dust edge = Dust.NewDustPerfect(edgePos, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, edgeColor, 0.6f + StackIntensity * 0.3f);
                edge.noGravity = true;
            }

            // Dynamic lighting
            float lightIntensity = 0.4f + StackIntensity * 0.4f;
            Color lightColor = Color.Lerp(CosmicBlue, StarlightSilver, StackIntensity);
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * lightIntensity);
        }

        #endregion

        #region Hit Effects

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Celestial Harmony debuff
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 480);
            if (target.TryGetGlobalNPC(out CelestialHarmonyNPC harmonyNPC))
                harmonyNPC.AddStack(target, 1);

            // Halo rings on hit — multi-layered cosmic ripple
            int ringCount = 2 + (int)(StackIntensity * 2);
            for (int ring = 0; ring < ringCount; ring++)
            {
                float p = (float)ring / ringCount;
                Color ringColor = Color.Lerp(DeepIndigo, StarlightSilver, p + StackIntensity * 0.2f);
                CustomParticles.HaloRing(target.Center, ringColor, 0.3f + ring * 0.1f, 12 + ring * 2);
            }

            // Radial cosmic dust burst
            int dustCount = 8 + (int)(StackIntensity * 8);
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Color dc = Color.Lerp(CosmicBlue, StellarWhite, (float)i / dustCount * StackIntensity);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f) * (1f + StackIntensity * 0.3f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch, vel, 0, dc, 1.2f + StackIntensity * 0.3f);
                d.noGravity = true;
            }

            // Music notes burst on impact
            NachtmusikVFXLibrary.SpawnMusicNotes(target.Center, 2 + (int)(StackIntensity * 3), 20f, 0.6f, 0.9f, 25);
            NachtmusikVFXLibrary.SpawnTwinklingStars(target.Center, 2, 15f);

            // Stack VFX — flare at impact point
            CustomParticles.GenericFlare(target.Center, Color.Lerp(CosmicBlue, StellarWhite, StackIntensity),
                0.4f + StackIntensity * 0.3f, 14);

            if (hit.Crit)
            {
                NachtmusikVFXLibrary.SpawnStarBurst(target.Center, 4, 0.6f + StackIntensity * 0.3f);
                NachtmusikVFXLibrary.SpawnShatteredStarlight(target.Center, 3, 4f, 0.5f, false);
            }

            Lighting.AddLight(target.Center, Color.Lerp(CosmicBlue, StellarWhite, StackIntensity).ToVector3() * 0.7f);
        }

        #endregion

        #region Drawing

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float rotation = Projectile.rotation;
            float scaleFactor = DynamicScale;
            float fadeAlpha = 1f - Projectile.alpha / 255f;

            // === ADDITIVE RENDERING — glow layers ===
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Wide outer halo — Deep Indigo
            Color outerColor = Color.Lerp(NightVoid, DeepIndigo, 0.5f + StackIntensity * 0.3f) with { A = 0 };
            sb.Draw(tex, drawPos, null, outerColor * 0.3f * fadeAlpha, rotation, origin,
                scaleFactor * 1.4f * (0.8f + StackIntensity * 0.4f), SpriteEffects.None, 0f);

            // Layer 2: Mid glow — Cosmic Blue
            Color midColor = Color.Lerp(DeepIndigo, CosmicBlue, StackIntensity) with { A = 0 };
            sb.Draw(tex, drawPos, null, midColor * 0.5f * fadeAlpha, rotation, origin,
                scaleFactor * 1.1f * (0.8f + StackIntensity * 0.3f), SpriteEffects.None, 0f);

            // Layer 3: Core — Starlight Silver to Stellar White
            Color coreColor = Color.Lerp(StarlightSilver, StellarWhite, StackIntensity) with { A = 0 };
            sb.Draw(tex, drawPos, null, coreColor * 0.7f * fadeAlpha, rotation, origin,
                scaleFactor * 0.85f * (0.8f + StackIntensity * 0.2f), SpriteEffects.None, 0f);

            // Layer 4: White-hot nucleus at high intensity
            if (StackIntensity > 0.5f)
            {
                float nucleusPulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.1f;
                Color nucleusColor = StellarWhite with { A = 0 };
                sb.Draw(tex, drawPos, null, nucleusColor * (StackIntensity - 0.5f) * 1.2f * fadeAlpha, rotation, origin,
                    scaleFactor * 0.5f * nucleusPulse, SpriteEffects.None, 0f);
            }

            // === INTENSITY-SCALED BLOOM at center ===
            {
                float bloomScale = 0.3f + StackIntensity * 0.25f + LifetimeProgress * 0.1f;
                float bloomOpacity = (0.5f + StackIntensity * 0.4f) * fadeAlpha;
                Color bloomPrimary = Color.Lerp(DeepIndigo, CosmicBlue, StackIntensity);
                Color bloomSecondary = Color.Lerp(StarlightSilver, StellarWhite, StackIntensity);

                Texture2D bloomTex = MagnumTextureRegistry.GetBloom();
                if (bloomTex != null)
                {
                    Vector2 bloomOrigin = bloomTex.Size() * 0.5f;
                    sb.Draw(bloomTex, drawPos, null, (bloomPrimary with { A = 0 }) * bloomOpacity * 0.4f,
                        0f, bloomOrigin, bloomScale * 0.16f, SpriteEffects.None, 0f);
                    sb.Draw(bloomTex, drawPos, null, (bloomSecondary with { A = 0 }) * bloomOpacity * 0.6f,
                        0f, bloomOrigin, bloomScale * 0.1f, SpriteEffects.None, 0f);
                    sb.Draw(bloomTex, drawPos, null, (StellarWhite with { A = 0 }) * bloomOpacity * 0.5f,
                        0f, bloomOrigin, bloomScale * 0.045f, SpriteEffects.None, 0f);
                }
            }

            // === AFTERIMAGE TRAIL from old positions ===
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float trailProgress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress) * 0.4f * fadeAlpha;
                Color trailColor = Color.Lerp(CosmicBlue, StarlightSilver, trailProgress * StackIntensity) with { A = 0 };
                float trailScale = scaleFactor * (1f - trailProgress * 0.3f) * 0.8f;
                sb.Draw(tex, trailPos, null, trailColor * trailAlpha, Projectile.oldRot[i], origin,
                    trailScale, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Nachtmusik theme star flare accent
            NachtmusikShaderManager.BeginAdditive(sb);
            NachtmusikVFXLibrary.DrawThemeStarFlare(sb, Projectile.Center, 1f, 0.5f);
            NachtmusikShaderManager.RestoreSpriteBatch(sb);

            return false;
        }

        #endregion

        #region Kill — Dissipation Burst

        public override void OnKill(int timeLeft)
        {
            // Cosmic dissipation burst
            int burstCount = 8 + (int)(StackIntensity * 10);
            for (int i = 0; i < burstCount; i++)
            {
                float angle = MathHelper.TwoPi * i / burstCount;
                float dp = (float)i / burstCount;
                Color dc = Color.Lerp(CosmicBlue, Color.Lerp(StarlightSilver, StellarWhite, StackIntensity), dp);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, vel, 0, dc, 1.0f + StackIntensity * 0.4f);
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }

            // Fading halo ring
            CustomParticles.HaloRing(Projectile.Center, Color.Lerp(DeepIndigo, StarlightSilver, StackIntensity), 0.35f + StackIntensity * 0.15f, 16);

            // Music notes scattering on dissipation
            NachtmusikVFXLibrary.SpawnMusicNotes(Projectile.Center, 2 + (int)(StackIntensity * 2), 25f, 0.5f, 0.8f, 22);

            // Twinkling star remnants
            NachtmusikVFXLibrary.SpawnTwinklingStars(Projectile.Center, 2, 20f);

            // Flash of light
            Lighting.AddLight(Projectile.Center, Color.Lerp(CosmicBlue, StellarWhite, StackIntensity).ToVector3() * 0.6f);

            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.3f + StackIntensity * 0.3f, Volume = 0.6f }, Projectile.Center);
        }

        #endregion
    }
}
