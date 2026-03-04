using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Primitives;
using MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Shaders;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Projectiles
{
    /// <summary>
    /// Conductor Sword Beam — Homing beam projectile fired by ConductorSwingProjectile.
    ///
    /// BEHAVIOR:
    ///   - Launched in 18° spread (3 per swing)
    ///   - Homes aggressively toward nearest enemy after brief travel
    ///   - Lightning zigzag trail via ConductorTrailRenderer
    ///   - ai[0] = phase (0=Downbeat, 1=Crescendo, 2=Forte, 3=CrystalShard mode)
    ///   - Crystal shard mode: faster, smaller, 25% damage seekers spawned on hit
    ///
    /// RENDERING:
    ///   - Shader-driven trail with ConductorBeamShader
    ///   - Cyan-gold energy body
    ///   - Lightning spark accents
    ///   - Tip glow flare
    /// </summary>
    public class ConductorSwordBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/TheConductorsLastConstellation";

        private const float HomingRange = 700f;
        private const float HomingStrength = 0.12f;
        private const int HomingDelay = 8; // Start homing after 8 frames
        private const int BeamDuration = 120;
        private const int ShardDuration = 60;

        // Trail
        private Vector2[] _trailPositions = new Vector2[18];
        private int _trailCount;

        // Textures
        private static Asset<Texture2D> _glowTex;
        private static Asset<Texture2D> _flareTex;
        private static Asset<Texture2D> _noiseTex;

        private Player Owner => Main.player[Projectile.owner];
        private ref float Phase => ref Projectile.ai[0];
        private ref float Timer => ref Projectile.ai[1];
        private bool IsCrystalShard => Phase >= 3f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 18;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.timeLeft = BeamDuration;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (IsCrystalShard)
            {
                Projectile.timeLeft = ShardDuration;
                Projectile.penetrate = 1;
                Projectile.scale = 0.6f;
            }

            SoundEngine.PlaySound(SoundID.Item12 with { Pitch = 0.6f + Phase * 0.1f, Volume = 0.5f }, Projectile.Center);
        }

        public override void AI()
        {
            Timer++;

            // Homing after delay
            if (Timer > HomingDelay)
            {
                NPC target = ConductorUtils.ClosestNPCAt(Projectile.Center, HomingRange);
                if (target != null)
                {
                    Vector2 toTarget = ConductorUtils.SafeDirectionTo(Projectile.Center, target.Center);
                    float homingMult = IsCrystalShard ? HomingStrength * 1.5f : HomingStrength;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), homingMult);
                }
            }

            // Maintain speed
            float targetSpeed = IsCrystalShard ? 16f : 14f;
            if (Projectile.velocity.Length() < targetSpeed)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * targetSpeed;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Update trail
            UpdateTrail();

            // Particles
            SpawnAmbientParticles();

            // Light
            float pulse = 0.5f + MathF.Sin((float)Main.timeForVisualEffects * 0.07f) * 0.15f;
            Color lightCol = IsCrystalShard ? ConductorUtils.LightningGold : ConductorUtils.ConductorCyan;
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * 0.5f * pulse);

            // Fade in last 15 frames
            if (Projectile.timeLeft < 15)
                Projectile.alpha = (int)((1f - Projectile.timeLeft / 15f) * 255f);
        }

        private void UpdateTrail()
        {
            if (_trailCount < _trailPositions.Length)
            {
                _trailPositions[_trailCount] = Projectile.Center;
                _trailCount++;
            }
            else
            {
                Array.Copy(_trailPositions, 1, _trailPositions, 0, _trailPositions.Length - 1);
                _trailPositions[_trailPositions.Length - 1] = Projectile.Center;
            }
        }

        private void SpawnAmbientParticles()
        {
            if (Main.dedServ) return;

            // Lightning zigzag sparks trailing behind
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f);
                sparkVel += Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color col = IsCrystalShard
                    ? ConductorUtils.LightningGold
                    : ConductorUtils.GetConductorShimmer((float)Main.timeForVisualEffects * 0.04f + Main.rand.NextFloat());
                float scale = IsCrystalShard ? 0.12f : 0.18f;
                ConductorParticleHandler.SpawnParticle(new LightningSpark(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    sparkVel, col, scale, 12, 3f, 0.45f));
            }

            // Occasional mote
            if (Main.rand.NextBool(6))
            {
                Vector2 moteVel = Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color moteCol = ConductorUtils.GetConductorShimmer((float)Main.timeForVisualEffects * 0.03f + Main.rand.NextFloat());
                ConductorParticleHandler.SpawnParticle(new ConductorMote(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    moteVel, moteCol, 0.1f, 15));
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);

            // Constellation Shatter: if the beam kills and charge is high, spawn homing star projectiles
            if (target.life <= 0 && Projectile.owner == Main.myPlayer)
            {
                Player owner = Main.player[Projectile.owner];
                var cp = owner.Conductor();
                if (cp.ChargeLevel >= 0.5f && cp.ShatterCooldown <= 0)
                {
                    cp.ConstellationShatterTriggered = true;
                    cp.ShatterCooldown = 120; // 2s cooldown
                    int starCount = 8 + (int)(cp.ChargeLevel * 4); // 8-12 stars
                    for (int s = 0; s < starCount; s++)
                    {
                        float angle = MathHelper.TwoPi * s / starCount + Main.rand.NextFloat(-0.15f, 0.15f);
                        Vector2 starVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            target.Center + Main.rand.NextVector2Circular(10f, 10f),
                            starVel,
                            ModContent.ProjectileType<ConductorSwordBeam>(),
                            (int)(Projectile.damage * 0.35f),
                            Projectile.knockBack * 0.2f,
                            Projectile.owner,
                            3f, // crystal shard mode (homing)
                            0f);
                    }
                    // Shatter VFX burst
                    if (!Main.dedServ)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            float burstAngle = MathHelper.TwoPi * i / 16f;
                            Vector2 burstVel = burstAngle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                            ConductorParticleHandler.SpawnParticle(new ConductorSpark(
                                target.Center, burstVel, ConductorUtils.LightningGold, 0.3f, 18));
                        }
                        ConductorParticleHandler.SpawnParticle(new ConductorBloomFlare(
                            target.Center, ConductorUtils.CelestialWhite, 0.8f, 20));
                        ConductorParticleHandler.SpawnParticle(new ConductorBloomFlare(
                            target.Center, ConductorUtils.LightningGold, 0.6f, 16));
                        SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.9f }, target.Center);
                    }
                }
            }

            if (Main.dedServ) return;

            // Impact burst
            ConductorParticleHandler.SpawnParticle(new ConductorBloomFlare(
                target.Center, ConductorUtils.CelestialWhite, 0.4f, 10));
            ConductorParticleHandler.SpawnParticle(new ConductorBloomFlare(
                target.Center, ConductorUtils.ConductorCyan, 0.35f, 8));

            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color col = i % 2 == 0 ? ConductorUtils.LightningGold : ConductorUtils.ConductorCyan;
                ConductorParticleHandler.SpawnParticle(new LightningSpark(
                    target.Center, sparkVel, col, 0.22f, 14, 4f, 0.4f));
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Death burst
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                ConductorParticleHandler.SpawnParticle(new ConductorSpark(
                    Projectile.Center, sparkVel, ConductorUtils.ConductorCyan, 0.18f, 10));
            }
            ConductorParticleHandler.SpawnParticle(new ConductorBloomFlare(
                Projectile.Center, ConductorUtils.StarSilver, 0.25f, 8));
        }

        // ======================== RENDERING ========================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            _glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
            _flareTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Flare/flare_16");
            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/MusicalWavePattern");

            SpriteBatch sb = Main.spriteBatch;
            float opacity = 1f - Projectile.alpha / 255f;

            try
            {
                // End SpriteBatch for GPU primitive trail drawing
                sb.End();
                DrawBeamTrail(sb, opacity);

                // Restart SpriteBatch for sprite-based drawing
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                DrawBeamBody(sb, lightColor, opacity);
                DrawTipGlow(sb, opacity);
            }
            catch
            {
                try
                {
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }

            return false;
        }

        private void DrawBeamTrail(SpriteBatch sb, float opacity)
        {
            if (_trailCount < 2) return;

            var shader = ConductorShaderLoader.GetBeamShader();

            try
            {
                float scaleMult = IsCrystalShard ? 0.5f : 1f;

                if (shader != null)
                {
                    if (_noiseTex?.Value != null)
                        shader.UseImage1(_noiseTex);

                    shader.UseColor(ConductorUtils.ConductorCyan.ToVector3());
                    shader.UseSecondaryColor(ConductorUtils.LightningGold.ToVector3());
                    shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                    shader.Shader.Parameters["uOpacity"]?.SetValue(0.8f * opacity);
                    shader.Shader.Parameters["uIntensity"]?.SetValue(1.5f);
                    shader.Shader.Parameters["uOverbrightMult"]?.SetValue(1.6f);
                    shader.Shader.Parameters["uScrollSpeed"]?.SetValue(2f);
                    shader.Shader.Parameters["uNoiseScale"]?.SetValue(3f);
                }

                ConductorTrailRenderer.RenderTrail(_trailPositions, new ConductorTrailSettings(
                    (p, _) =>
                    {
                        float baseW = 18f * scaleMult;
                        float taper = MathF.Sin(p * MathHelper.Pi);
                        return baseW * (0.3f + taper * 0.7f);
                    },
                    (p) =>
                    {
                        Color c = ConductorUtils.GetLightningGradient(p);
                        return ConductorUtils.Additive(c, (1f - p * 0.5f) * opacity);
                    },
                    shader: shader), _trailCount, 2);
            }
            catch { }
        }

        private void DrawBeamBody(SpriteBatch sb, Color lightColor, float opacity)
        {
            try
            {
                Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
                Vector2 origin = tex.Size() / 2f;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float scale = Projectile.scale;
                Color bodyColor = Color.Lerp(lightColor, ConductorUtils.ConductorCyan, 0.5f) * opacity;

                sb.Draw(tex, drawPos, null, bodyColor, Projectile.rotation + MathHelper.PiOver4, origin, scale, SpriteEffects.None, 0f);
            }
            catch { }
        }

        private void DrawTipGlow(SpriteBatch sb, float opacity)
        {
            if (_glowTex?.Value == null) return;

            try
            {
                ConductorUtils.BeginAdditive(sb);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float pulse = 0.7f + MathF.Sin((float)Main.timeForVisualEffects * 0.1f) * 0.3f;
                float tipScale = IsCrystalShard ? 0.18f : 0.3f;

                sb.Draw(_glowTex.Value, drawPos, null,
                    ConductorUtils.Additive(ConductorUtils.ConductorCyan, 0.5f * opacity * pulse),
                    0f, _glowTex.Value.Size() / 2f, tipScale * pulse, SpriteEffects.None, 0f);
                sb.Draw(_glowTex.Value, drawPos, null,
                    ConductorUtils.Additive(ConductorUtils.CelestialWhite, 0.3f * opacity * pulse),
                    0f, _glowTex.Value.Size() / 2f, tipScale * 0.5f * pulse, SpriteEffects.None, 0f);

                // Flare spike at tip
                if (_flareTex?.Value != null)
                {
                    sb.Draw(_flareTex.Value, drawPos, null,
                        ConductorUtils.Additive(ConductorUtils.LightningGold, 0.3f * opacity * pulse),
                        Projectile.rotation, _flareTex.Value.Size() / 2f,
                        tipScale * 0.8f, SpriteEffects.None, 0f);
                }

                ConductorUtils.EndAdditive(sb);
            }
            catch
            {
                try { ConductorUtils.EndAdditive(sb); } catch { }
            }
        }
    }
}
