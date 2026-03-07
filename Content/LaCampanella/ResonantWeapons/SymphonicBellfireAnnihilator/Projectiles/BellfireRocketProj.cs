using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Primitives;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Shaders;
using MagnumOpus.Content.LaCampanella;
using MagnumOpus.Content.LaCampanella.Debuffs;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;
using MagnumOpus.Content.FoundationWeapons.ExplosionParticlesFoundation;
using MagnumOpus.Content.FoundationWeapons.SmokeFoundation;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Projectiles
{
    /// <summary>
    /// Bellfire Rocket — alt-fire rapid arcing rockets that leave fire patches on impact.
    /// Simplified from tier system. Arcs slightly downward due to gravity.
    /// On kill, registers rocket kill for Bellfire Crescendo buff stacking.
    /// </summary>
    public class BellfireRocketProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RocketI;

        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailPoints = 16;
        private SymphonicBellfirePrimitiveRenderer trailRenderer;
        private static int _lastParticleDrawFrame = -1;

        private const float FirePatchDuration = 90; // 1.5 seconds
        private const float ExplosionRadius = 80f;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
        }

        public override void AI()
        {
            // Record trail
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailPoints)
                trailPositions.RemoveAt(trailPositions.Count - 1);

            // Slight arc (gravity)
            Projectile.velocity.Y += 0.12f;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Exhaust trail particles
            if (Main.rand.NextBool(2))
            {
                SymphonicBellfireParticleHandler.SpawnParticle(new RocketExhaustParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4, 4),
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    2.5f, Main.rand.Next(12, 22)));
            }

            // Lighting
            Lighting.AddLight(Projectile.Center, SymphonicBellfireUtils.RocketPalette[2].ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);

            // Register rocket kill for buff stacking
            if (target.life <= 0)
            {
                Player owner = Main.player[Projectile.owner];
                var modPlayer = owner.GetModPlayer<SymphonicBellfirePlayer>();
                modPlayer.RegisterRocketKill();
            }
        }

        public override void OnKill(int timeLeft)
        {
            trailRenderer?.Dispose();
            trailRenderer = null;

            // Explosion VFX — multi-layer bloom stack
            // Outer fire shell
            SymphonicBellfireParticleHandler.SpawnParticle(new ExplosionFireballParticle(
                Projectile.Center, 3.5f, 18));
            // Mid orange ring
            SymphonicBellfireParticleHandler.SpawnParticle(new ExplosionFireballParticle(
                Projectile.Center, 2.0f, 14));
            // White-hot core
            SymphonicBellfireParticleHandler.SpawnParticle(new ExplosionFireballParticle(
                Projectile.Center, 1.0f, 10));

            // Shrapnel embers
            for (int i = 0; i < 8; i++)
            {
                SymphonicBellfireParticleHandler.SpawnParticle(new RocketExhaustParticle(
                    Projectile.Center,
                    Main.rand.NextVector2Circular(5f, 5f),
                    Main.rand.NextFloat(2f, 3.5f),
                    Main.rand.Next(12, 25)));
            }

            // AoE splash damage
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                if (Vector2.Distance(Projectile.Center, npc.Center) <= ExplosionRadius)
                {
                    int dir = Projectile.Center.X < npc.Center.X ? 1 : -1;
                    int splashDmg = (int)(Projectile.damage * 0.4f);
                    npc.SimpleStrikeNPC(splashDmg, dir, false, Projectile.knockBack * 0.4f);
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 1);
                }
            }

            // Fire patch — lingering damage zone (vanilla fire dust)
            SpawnFirePatch();

            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.2f, Volume = 0.7f }, Projectile.Center);

            // === FOUNDATION: RippleEffectProjectile — Rocket detonation shockwave ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<RippleEffectProjectile>(),
                0, 0f, Projectile.owner, ai0: 1f);

            // === FOUNDATION: SparkExplosionProjectile — Rocket detonation spark burst ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<SparkExplosionProjectile>(),
                0, 0f, Projectile.owner,
                ai0: (float)SparkMode.RadialScatter);
        }

        private void SpawnFirePatch()
        {
            Vector2 patchCenter = Projectile.Center;

            // Custom infernal fire particles — multi-layer rising embers + heat shimmer
            int fireCount = 16;
            for (int i = 0; i < fireCount; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(35f, 12f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.8f, 2.5f));
                float stretch = Main.rand.NextFloat(2.5f, 5f);
                SymphonicBellfireParticleHandler.SpawnParticle(new RocketExhaustParticle(
                    patchCenter + offset, vel, stretch, Main.rand.Next(25, 50)));
            }

            // Secondary ember layer — smaller, faster rising sparks
            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 8f);
                Vector2 vel = new Vector2(Main.rand.NextFloatDirection() * 1.5f, -Main.rand.NextFloat(1.5f, 4f));
                SymphonicBellfireParticleHandler.SpawnParticle(new RocketExhaustParticle(
                    patchCenter + offset, vel, Main.rand.NextFloat(1f, 2f), Main.rand.Next(15, 30)));
            }

            // Smoke puffs — thick dark smoke rising
            for (int i = 0; i < 6; i++)
            {
                Vector2 smokeVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(1f, 3f));
                Dust d = Dust.NewDustPerfect(patchCenter + Main.rand.NextVector2Circular(25f, 8f),
                    DustID.Smoke, smokeVel, 120, new Color(30, 20, 15), Main.rand.NextFloat(2f, 3f));
                d.noGravity = true;
            }

            // Musical notes rising from the flames — the bell's infernal song
            for (int i = 0; i < 3; i++)
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloatDirection() * 1f, -Main.rand.NextFloat(1f, 2f));
                SymphonicBellfireParticleHandler.SpawnParticle(new SymphonicNoteParticle(
                    patchCenter + Main.rand.NextVector2Circular(20f, 8f),
                    noteVel, Main.rand.Next(35, 55)));
            }

            // === FOUNDATION: DamageZoneProjectile — Lingering fire damage zone ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), patchCenter, Vector2.Zero,
                ModContent.ProjectileType<DamageZoneProjectile>(),
                (int)(Projectile.damage * 0.15f), 0f, Projectile.owner,
                ai0: 90f, ai1: 40f); // 1.5s duration, 40px radius
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {

            int currentFrame = (int)Main.GameUpdateCount;
            if (_lastParticleDrawFrame != currentFrame)
            {
                _lastParticleDrawFrame = currentFrame;
                SymphonicBellfireParticleHandler.DrawAllParticles(sb);
            }

            // Trail
            if (trailPositions.Count >= 2)
            {
                try
                {
                    trailRenderer ??= new SymphonicBellfirePrimitiveRenderer();

                    // === SHADER: RocketTrailShader — turbulent exhaust plume ===
                    var rocketShader = SymphonicBellfireShaderLoader.GetRocketTrailShader();
                    if (rocketShader != null)
                    {
                        try
                        {
                            rocketShader.UseColor(SymphonicBellfireUtils.RocketPalette[2]);
                            rocketShader.UseSecondaryColor(SymphonicBellfireUtils.RocketPalette[0]);
                            rocketShader.UseOpacity(0.85f);
                            rocketShader.UseSaturation(0.9f); // uIntensity
                            var fx = rocketShader.Shader;
                            if (fx != null)
                            {
                                fx.Parameters["uTime"]?.SetValue((float)Main.GameUpdateCount * 0.025f);
                                fx.Parameters["uOverbrightMult"]?.SetValue(1.4f);
                                fx.Parameters["uScrollSpeed"]?.SetValue(1.8f);
                                fx.Parameters["uNoiseScale"]?.SetValue(4.5f);
                            }
                        }
                        catch { }
                    }

                    var settings = new RocketTrailSettings
                    {
                        ColorStart = SymphonicBellfireUtils.RocketPalette[2],
                        ColorEnd = SymphonicBellfireUtils.RocketPalette[0] * 0.3f,
                        Width = 10f,
                        BloomIntensity = 0.3f,
                        Shader = rocketShader
                    };
                    trailRenderer.DrawTrail(sb, trailPositions, settings, Main.screenPosition);
                }
                catch { }
            }

            // Rocket sprite
            Texture2D tex = null;
            try { tex = ModContent.Request<Texture2D>(Texture, ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value; }
            catch { }
            if (tex != null)
            {
                Color drawColor = Color.Lerp(lightColor, Color.White, 0.2f);
                sb.Draw(tex, Projectile.Center - Main.screenPosition, null, drawColor,
                    Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);
            }

            // Bloom orb + LC flare + LC star in Additive
            try { sb.End(); } catch { }
            try
            {
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D bloomTex = null;
            try
            {
                bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }
            if (bloomTex != null)
            {
                Color bloomCol = SymphonicBellfireUtils.RocketPalette[2] * 0.15f;
                sb.Draw(bloomTex, Projectile.Center - Main.screenPosition, null,
                    bloomCol, 0f, bloomTex.Size() / 2f, 0.07f, SpriteEffects.None, 0f);
            }

            // LC Beam Lens Flare - explosive flare halo behind the rocket
            {
                Vector2 rocketScreen = Projectile.Center - Main.screenPosition;
                float rocketPulse = 0.7f + 0.3f * (float)Math.Sin(Main.GameUpdateCount * 0.2f);
                LaCampanellaVFXLibrary.DrawBeamLensFlare(sb, rocketScreen,
                    0.18f * rocketPulse, Projectile.rotation, 0.2f * rocketPulse,
                    LaCampanellaPalette.InfernalOrange);
            }

            // LC Bright Star Projectile - rotating star at rocket core
            {
                Vector2 rocketScreen = Projectile.Center - Main.screenPosition;
                float starRot = (float)Main.GameUpdateCount * 0.07f;
                LaCampanellaVFXLibrary.DrawBrightStar(sb, rocketScreen,
                    0.12f, starRot, 0.25f,
                    LaCampanellaPalette.FlameYellow, useVariant2: true);
            }

            // Theme texture accents
            SymphonicBellfireUtils.DrawThemeAccents(sb, Projectile.Center - Main.screenPosition, Projectile.scale);
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }

            } // end outer try
            catch
            {
                try
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }
            return false;
        }
    }
}
