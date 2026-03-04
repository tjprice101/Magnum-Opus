using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Primitives;
using MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Shaders;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Projectiles
{
    /// <summary>
    /// Fractal Orbit Blade — Orbiting spectral star blade.
    /// Spawns on enemy hit. Circles the player at a fixed radius,
    /// periodically firing prismatic beams at nearby enemies.
    ///
    /// BEHAVIOR:
    ///   - Orbits player at 120px radius
    ///   - 3 blades spawn 120° apart (handled by Projectile spawning offsets)
    ///   - Every 60 frames, fires a prismatic beam toward nearest enemy
    ///   - Duration: 10 seconds (600 frames)
    ///   - Max 6 active orbit blades
    ///   - Constellation trail shader on orbit path
    /// </summary>
    public class FractalOrbitBlade : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/FractalOfTheStars";

        private const float OrbitRadius = 120f;
        private const float OrbitSpeed = 0.04f;
        private const int BeamInterval = 60;    // Fire every 60 frames
        private const int Duration = 600;       // 10 seconds
        private const int MaxActive = 6;

        // Trail
        private Vector2[] _trailPositions = new Vector2[16];
        private int _trailCount;

        // Textures
        private static Asset<Texture2D> _glowTex;
        private static Asset<Texture2D> _flareTex;

        private Player Owner => Main.player[Projectile.owner];
        private ref float OrbitAngle => ref Projectile.ai[0];
        private ref float Timer => ref Projectile.ai[1];
        private float SpinRotation;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.timeLeft = Duration;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SpinRotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Owner.Fractal().OrbitBladeCount++;

            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.6f, Volume = 0.5f }, Projectile.Center);
        }

        public override void AI()
        {
            Timer++;

            // Orbit around player
            OrbitAngle += OrbitSpeed;
            Vector2 orbitPos = Owner.MountedCenter + OrbitAngle.ToRotationVector2() * OrbitRadius;
            Projectile.Center = orbitPos;
            Projectile.velocity = Vector2.Zero;

            // Spin rotation
            SpinRotation += 0.12f;

            // Update trail
            UpdateTrail();

            // Periodically fire prismatic beam
            if ((int)Timer % BeamInterval == 0 && Timer > 10)
            {
                FirePrismaticBeam();
            }

            // Spawn ambient star particles
            SpawnAmbientParticles();

            // Light
            float pulse = 0.5f + MathF.Sin((float)Main.timeForVisualEffects * 0.06f) * 0.15f;
            Lighting.AddLight(Projectile.Center, FractalUtils.StarGold.ToVector3() * 0.4f * pulse);

            // Fade out in last 30 frames
            if (Projectile.timeLeft < 30)
                Projectile.alpha = (int)((1f - Projectile.timeLeft / 30f) * 255f);
        }

        private void FirePrismaticBeam()
        {
            NPC target = FractalUtils.ClosestNPCAt(Projectile.Center, 600f);
            if (target == null) return;

            // Visual: beam line of star sparks toward target
            Vector2 toTarget = FractalUtils.SafeDirectionTo(Projectile.Center, target.Center);
            float dist = Vector2.Distance(Projectile.Center, target.Center);
            int sparkCount = (int)(dist / 20f);

            for (int i = 0; i < sparkCount; i++)
            {
                float t = (float)i / sparkCount;
                Vector2 pos = Vector2.Lerp(Projectile.Center, target.Center, t);
                Vector2 vel = toTarget.RotatedByRandom(0.2f) * Main.rand.NextFloat(1f, 3f);
                Color col = FractalUtils.GetStellarGradient(t);
                FractalParticleHandler.SpawnParticle(new FractalSpark(
                    pos + Main.rand.NextVector2Circular(5f, 5f), vel,
                    col, 0.2f, 10));
            }

            // Bloom at source
            FractalParticleHandler.SpawnParticle(new FractalBloomFlare(
                Projectile.Center, FractalUtils.ConstellationWhite, 0.4f, 10));

            // Bloom at target
            FractalParticleHandler.SpawnParticle(new FractalBloomFlare(
                target.Center, FractalUtils.StarGold, 0.35f, 8));

            // Deal damage to target via direct strike
            if (Projectile.owner == Main.myPlayer)
            {
                target.SimpleStrikeNPC(Projectile.damage, Projectile.Center.X < target.Center.X ? 1 : -1, false, 0f, DamageClass.Melee);
                target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);
            }

            SoundEngine.PlaySound(SoundID.Item12 with { Pitch = 0.8f, Volume = 0.4f }, Projectile.Center);
        }

        private void SpawnAmbientParticles()
        {
            if (Main.dedServ) return;

            // Gentle star motes trailing behind
            if (Main.rand.NextBool(3))
            {
                Color moteCol = FractalUtils.GetStarShimmer((float)Main.timeForVisualEffects * 0.04f + Main.rand.NextFloat());
                FractalParticleHandler.SpawnParticle(new FractalMote(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    moteCol, 0.12f, 18));
            }

            // Occasional small star particle
            if (Main.rand.NextBool(8))
            {
                Vector2 starVel = Main.rand.NextVector2Circular(1f, 1f);
                FractalParticleHandler.SpawnParticle(new FractalStarParticle(
                    Projectile.Center, starVel,
                    FractalUtils.ConstellationWhite, 0.12f, 20, 4));
            }
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

        public override void OnKill(int timeLeft)
        {
            if (Owner.active)
                Owner.Fractal().OrbitBladeCount = Math.Max(0, Owner.Fractal().OrbitBladeCount - 1);

            // Death burst
            if (!Main.dedServ)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                    FractalParticleHandler.SpawnParticle(new FractalSpark(
                        Projectile.Center, sparkVel, FractalUtils.StarGold, 0.2f, 12));
                }
                FractalParticleHandler.SpawnParticle(new FractalBloomFlare(
                    Projectile.Center, FractalUtils.ConstellationWhite, 0.3f, 8));
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);

            // Impact sparks
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 5f);
                FractalParticleHandler.SpawnParticle(new FractalSpark(
                    target.Center, sparkVel, FractalUtils.StarGold, 0.2f, 10));
            }

            // Fractal Recursion: on hit, spawn sub-fracture orbit blades at 1/3 size & damage
            int currentDepth = (int)Projectile.localAI[0];
            int maxDepth = Owner.Fractal().MaxRecursionDepth;
            if (currentDepth < maxDepth && Main.myPlayer == Projectile.owner && Main.rand.NextBool(3))
            {
                Owner.Fractal().TotalFracturesTriggered++;
                int subCount = currentDepth == 0 ? 2 : 1; // First split = 2, deeper = 1
                for (int s = 0; s < subCount; s++)
                {
                    float subAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                    int subProj = Projectile.NewProjectile(
                        Projectile.GetSource_OnHit(target),
                        target.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<FractalOrbitBlade>(),
                        Math.Max(1, Projectile.damage / 3),
                        Projectile.knockBack * 0.3f,
                        Projectile.owner,
                        subAngle,
                        0f);
                    if (subProj >= 0 && subProj < Main.maxProjectiles)
                    {
                        Main.projectile[subProj].localAI[0] = currentDepth + 1;
                        Main.projectile[subProj].scale = Projectile.scale * 0.6f; // Visually smaller
                        Main.projectile[subProj].timeLeft = 300; // Half duration for sub-fractures
                    }
                }
                // Sub-fracture VFX flash
                if (!Main.dedServ)
                {
                    FractalParticleHandler.SpawnParticle(new FractalBloomFlare(
                        target.Center, FractalUtils.StarGold, 0.35f, 8));
                }
            }
        }

        public override bool? CanDamage()
        {
            // Only damages enemies on direct contact during orbit
            return true;
        }

        // ======================== RENDERING ========================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            _glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
            _flareTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Flare/flare_16");

            SpriteBatch sb = Main.spriteBatch;
            float opacity = 1f - Projectile.alpha / 255f;

            try
            {
                // End SpriteBatch for GPU primitive trail drawing
                sb.End();
                DrawConstellationTrail(sb, opacity);

                // Restart SpriteBatch for sprite-based layers
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                DrawBladeSprite(sb, lightColor, opacity);
                DrawStarGlow(sb, opacity);
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

        private void DrawConstellationTrail(SpriteBatch sb, float opacity)
        {
            if (_trailCount < 3) return;

            var shader = FractalShaderLoader.GetConstellationTrail();
            if (shader == null) return;

            try
            {
                shader.UseColor(FractalUtils.ConstellationWhite.ToVector3());
                shader.UseSecondaryColor(FractalUtils.StarfieldBlue.ToVector3());
                shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 3f);
                shader.Shader.Parameters["uOpacity"]?.SetValue(0.6f * opacity);
                shader.Shader.Parameters["uIntensity"]?.SetValue(1.5f);
                shader.Shader.Parameters["uOverbrightMult"]?.SetValue(1.8f);

                FractalTrailRenderer.RenderTrail(_trailPositions, new FractalTrailSettings(
                    (p, _) => 14f * (1f - p * 0.6f),
                    (p) => FractalUtils.Additive(FractalUtils.GetConstellationGradient(p), (1f - p) * opacity),
                    shader: shader), _trailCount, 2);
            }
            catch { }
        }

        private void DrawBladeSprite(SpriteBatch sb, Color lightColor, float opacity)
        {
            try
            {
                Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
                Vector2 origin = tex.Size() / 2f;

                // Spectral stellar tint
                Color bladeColor = Color.Lerp(FractalUtils.ConstellationWhite, FractalUtils.StarGold, 0.3f) * opacity;

                sb.Draw(tex, Projectile.Center - Main.screenPosition, null, bladeColor, SpinRotation,
                    origin, Projectile.scale * 0.7f, SpriteEffects.None, 0f);

                // Afterimage
                Color afterColor = FractalUtils.FractalPurple * (opacity * 0.25f);
                sb.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(2f, 2f), null, afterColor,
                    SpinRotation - 0.15f, origin, Projectile.scale * 0.65f, SpriteEffects.None, 0f);
            }
            catch { }
        }

        private void DrawStarGlow(SpriteBatch sb, float opacity)
        {
            if (_glowTex?.Value == null) return;

            try
            {
                FractalUtils.BeginAdditive(sb);

                var tex = _glowTex.Value;
                Vector2 origin = tex.Size() / 2f;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;

                float pulse = 0.4f + MathF.Sin((float)Main.timeForVisualEffects * 0.08f + OrbitAngle) * 0.1f;

                Color innerCol = FractalUtils.Additive(FractalUtils.StarGold, 0.4f * opacity);
                Color outerCol = FractalUtils.Additive(FractalUtils.FractalPurple, 0.2f * opacity);

                sb.Draw(tex, drawPos, null, outerCol, 0f, origin, pulse * 1.5f, SpriteEffects.None, 0f);
                sb.Draw(tex, drawPos, null, innerCol, 0f, origin, pulse, SpriteEffects.None, 0f);

                FractalUtils.EndAdditive(sb);
            }
            catch
            {
                try { FractalUtils.EndAdditive(sb); } catch { }
            }
        }
    }
}
