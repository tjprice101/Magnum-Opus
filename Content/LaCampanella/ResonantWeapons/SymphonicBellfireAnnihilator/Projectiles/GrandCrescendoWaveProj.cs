using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Shaders;
using MagnumOpus.Content.LaCampanella;
using MagnumOpus.Content.LaCampanella.Debuffs;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Projectiles
{
    /// <summary>
    /// Grand Crescendo Wave — slow-moving bell-shaped shockwave that expands as it travels.
    /// Pierces enemies but slows on each pierce. Empowered by Grand Crescendo buff stacks.
    /// ai[0] = 0 for normal wave, 1 for Symphonic Overture (ignores pierce slowdown, triple width).
    /// ai[1] = Grand Crescendo stack count (for size scaling).
    /// </summary>
    public class GrandCrescendoWaveProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        private bool IsSymphonicOverture => Projectile.ai[0] >= 1f;
        private int CrescendoStacks => (int)Projectile.ai[1];

        private float _baseWidth = 60f;
        private float _currentWidth;
        private static int _lastParticleDrawFrame = -1;
        private int _pierceCount;
        private bool _initialized;

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180; // 3 seconds travel
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _initialized = true;
                _pierceCount = 0;

                // Calculate base width from crescendo stacks
                float sizeBonus = 1f + CrescendoStacks * 0.10f;
                _baseWidth = IsSymphonicOverture ? 180f : 60f * sizeBonus;
                _currentWidth = _baseWidth;

                SoundEngine.PlaySound(SoundID.Item45 with
                {
                    Pitch = IsSymphonicOverture ? -0.4f : 0f,
                    Volume = IsSymphonicOverture ? 1.2f : 0.8f
                }, Projectile.Center);

                if (IsSymphonicOverture)
                {
                    // Massive overture flash
                    SymphonicBellfireParticleHandler.SpawnParticle(new CrescendoWaveParticle(
                        Projectile.Center, 200f, 30));
                    SymphonicBellfireParticleHandler.SpawnParticle(new ExplosionFireballParticle(
                        Projectile.Center, 5f, 20));
                }
            }

            // Expand as it travels (slow growth)
            float travelProgress = 1f - (float)Projectile.timeLeft / 180f;
            _currentWidth = _baseWidth * (1f + travelProgress * 0.8f);

            // Update hitbox to match width
            Projectile.width = (int)_currentWidth;
            Projectile.height = (int)(_currentWidth * 0.6f);

            // Rotation towards velocity
            if (Projectile.velocity.Length() > 0.5f)
                Projectile.rotation = Projectile.velocity.ToRotation();

            // Wave particles along edges
            if (Main.rand.NextBool(2))
            {
                float perpAngle = Projectile.rotation + MathHelper.PiOver2;
                float side = Main.rand.NextBool() ? 1f : -1f;
                Vector2 edgePos = Projectile.Center + perpAngle.ToRotationVector2() * (_currentWidth * 0.4f * side);
                Vector2 edgeVel = perpAngle.ToRotationVector2() * side * Main.rand.NextFloat(1f, 3f);

                SymphonicBellfireParticleHandler.SpawnParticle(new RocketExhaustParticle(
                    edgePos, edgeVel, Main.rand.NextFloat(1.5f, 3f), Main.rand.Next(10, 18)));
            }

            // Musical notes shedding from wave — generous spawn for "Symphonic" identity
            if (Main.rand.NextBool(3))
            {
                Vector2 noteVel = Main.rand.NextVector2Circular(2f, 2f) - Projectile.velocity * 0.05f;
                SymphonicBellfireParticleHandler.SpawnParticle(new SymphonicNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(_currentWidth * 0.3f, _currentWidth * 0.2f),
                    noteVel, Main.rand.Next(30, 50)));
            }

            // Lighting
            float lightMul = IsSymphonicOverture ? 1.5f : 0.8f;
            Lighting.AddLight(Projectile.Center, SymphonicBellfireUtils.CrescendoPalette[1].ToVector3() * lightMul);

            // Slow velocity slightly over time (natural drag)
            if (!IsSymphonicOverture)
                Projectile.velocity *= 0.995f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, IsSymphonicOverture ? 5 : 2);

            // === FOUNDATION: RippleEffectProjectile — Grand Crescendo Wave expanding ring ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                ModContent.ProjectileType<RippleEffectProjectile>(),
                0, 0f, Projectile.owner, ai0: 1f);

            _pierceCount++;

            // Slow on pierce (unless Symphonic Overture)
            if (!IsSymphonicOverture)
            {
                Projectile.velocity *= 0.8f; // 20% slow per pierce

                // Kill if too slow
                if (Projectile.velocity.Length() < 1.5f)
                    Projectile.Kill();
            }

            // Pierce impact VFX
            SymphonicBellfireParticleHandler.SpawnParticle(new CrescendoWaveParticle(
                target.Center, 60f + _pierceCount * 10f, 15));

            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                SymphonicBellfireParticleHandler.SpawnParticle(new RocketExhaustParticle(
                    target.Center, sparkVel, Main.rand.NextFloat(2f, 4f), Main.rand.Next(10, 20)));
            }

            // Register wave kill for buff stacking (on kill)
            if (target.life <= 0)
            {
                Player owner = Main.player[Projectile.owner];
                var modPlayer = owner.GetModPlayer<SymphonicBellfirePlayer>();
                modPlayer.RegisterWaveKill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            float travelProgress = 1f - (float)Projectile.timeLeft / 180f;
            float fade = Math.Min(1f, (float)Projectile.timeLeft / 30f); // Fade in last 0.5s

            int currentFrame = (int)Main.GameUpdateCount;
            if (_lastParticleDrawFrame != currentFrame)
            {
                _lastParticleDrawFrame = currentFrame;
                SymphonicBellfireParticleHandler.DrawAllParticles(sb);
            }

            Texture2D tex = null;
            try
            {
                tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }
            if (tex == null) return false;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);

            float scaleX = _currentWidth / tex.Width * 2.5f;
            float scaleY = _currentWidth * 0.5f / tex.Height * 2.5f;

            try { sb.End(); } catch { }
            try
            {
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.15f);

            // Outer wave glow
            Color outerColor = SymphonicBellfireUtils.CrescendoPalette[0] * fade * 0.3f * pulse;
            sb.Draw(tex, drawPos, null, outerColor, Projectile.rotation,
                origin, new Vector2(scaleX * 1.3f, scaleY * 1.5f), SpriteEffects.None, 0f);

            // Mid body
            Color midColor = (IsSymphonicOverture ? SymphonicBellfireUtils.CrescendoPalette[2] : SymphonicBellfireUtils.CrescendoPalette[1]) * fade * 0.5f * pulse;
            sb.Draw(tex, drawPos, null, midColor, Projectile.rotation,
                origin, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);

            // Hot core
            Color coreColor = SymphonicBellfireUtils.CrescendoPalette[2] * fade * 0.7f;
            sb.Draw(tex, drawPos, null, coreColor, Projectile.rotation,
                origin, new Vector2(scaleX * 0.4f, scaleY * 0.4f), SpriteEffects.None, 0f);

            // LC Impact Ellipse - expanding shockwave ring on crescendo wave
            {
                float ellipsePulse = 0.6f + 0.4f * (float)Math.Sin(Main.GameUpdateCount * 0.18f);
                LaCampanellaVFXLibrary.DrawImpactEllipse(sb, drawPos,
                    scaleX * 0.4f * ellipsePulse, Projectile.rotation,
                    0.2f * fade * ellipsePulse, LaCampanellaPalette.FlameYellow);
            }

            // LC Power Effect Ring - concentric ring overlay behind wave
            if (IsSymphonicOverture)
            {
                float ringPulse = 0.5f + 0.5f * (float)Math.Sin(Main.GameUpdateCount * 0.14f);
                LaCampanellaVFXLibrary.DrawPowerEffectRing(sb, drawPos,
                    scaleX * 0.35f, (float)Main.GameUpdateCount * 0.02f,
                    0.25f * fade * ringPulse, LaCampanellaPalette.BellGold);
            }

            // === SHADER: CrescendoShader — musical crescendo energy overlay ===
            var crescShader = SymphonicBellfireShaderLoader.GetCrescendoShader();
            if (crescShader != null)
            {
                try
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                    crescShader.UseColor(SymphonicBellfireUtils.CrescendoPalette[1]);
                    crescShader.UseSecondaryColor(SymphonicBellfireUtils.CrescendoPalette[3]);
                    crescShader.UseOpacity(fade * 0.5f);
                    crescShader.UseSaturation(fade * pulse); // uIntensity
                    var crescFx = crescShader.Shader;
                    if (crescFx != null)
                    {
                        crescFx.Parameters["uTime"]?.SetValue((float)Main.GameUpdateCount * 0.02f);
                        crescFx.Parameters["uOverbrightMult"]?.SetValue(IsSymphonicOverture ? 1.5f : 1.2f);
                        crescFx.Parameters["uPhase"]?.SetValue(travelProgress);
                        crescFx.Parameters["uNoiseScale"]?.SetValue(3.5f);
                    }
                    crescShader.Apply();

                    // Shader wave body
                    Color shaderBody = Color.White * fade * 0.4f;
                    sb.Draw(tex, drawPos, null, shaderBody, Projectile.rotation,
                        origin, new Vector2(scaleX * 0.9f, scaleY * 0.9f), SpriteEffects.None, 0f);

                    // Shader inner core
                    Color shaderCore = Color.White * fade * 0.25f;
                    sb.Draw(tex, drawPos, null, shaderCore, Projectile.rotation,
                        origin, new Vector2(scaleX * 0.35f, scaleY * 0.35f), SpriteEffects.None, 0f);

                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch
                {
                    try
                    {
                        sb.End();
                        sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                            DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    }
                    catch { }
                }
            }
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

        public override void OnKill(int timeLeft)
        {
            // Final dissipation burst
            SymphonicBellfireParticleHandler.SpawnParticle(new CrescendoWaveParticle(
                Projectile.Center, _currentWidth * 0.5f, 20));

            for (int i = 0; i < 8; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                SymphonicBellfireParticleHandler.SpawnParticle(new RocketExhaustParticle(
                    Projectile.Center, burstVel, Main.rand.NextFloat(2f, 4f), Main.rand.Next(12, 22)));
            }
        }
    }
}
