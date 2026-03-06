using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MagnumOpus.Content.Fate.ResonantWeapons.ResonanceOfABygoneReality
{
    /// <summary>
    /// Resonance Rapid Bullet 遯ｶ繝ｻsmall 8・・・ fast cosmic projectile.
    /// extraUpdates=2, 120-frame life, 1 penetrate.
    /// Every 5th hit (per player via ResonancePlayer) spawns a ResonanceSpectralBlade at 2・・・damage.
    /// Self-contained VFX through own particle system and renderer 遯ｶ繝ｻno FateCosmicVFX / FateVFXLibrary.
    /// </summary>
    public class ResonanceRapidBullet : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft";

        private float pulsePhase;

        // === Foundation Bloom Textures ===
        private static Asset<Texture2D> _pointBloomTex;
        private static Asset<Texture2D> _softRadialBloomTex;
        private static Asset<Texture2D> _starFlareTex;

        private static Color Additive(Color c, float opacity) => c * opacity;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            pulsePhase += 0.15f;

            if (Main.dedServ) return;

            // === COSMIC TRAIL PARTICLES ===
            if (Main.rand.NextBool(2))
            {
                Color trailCol = ResonanceUtils.GradientLerp(Main.rand.NextFloat(0.2f, 0.9f));
                Vector2 trailVel = -Projectile.velocity * 0.06f + Main.rand.NextVector2Circular(0.8f, 0.8f);
                ResonanceParticleHandler.Spawn(ResonanceParticleType.CosmicTrail,
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    trailVel, trailCol * 0.75f, 0.14f, 12);
            }

            // Bullet glow core
            if (Main.rand.NextBool(3))
            {
                ResonanceParticleHandler.Spawn(ResonanceParticleType.BulletGlow,
                    Projectile.Center, Vector2.Zero,
                    ResonanceUtils.CosmicRose * 0.5f, 0.1f, 6);
            }

            // Trailing sparks
            if (Main.rand.NextBool(4))
            {
                Vector2 sparkVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2.5f)
                    + Main.rand.NextVector2Circular(1f, 1f);
                ResonanceParticleHandler.Spawn(ResonanceParticleType.MuzzleSpark,
                    Projectile.Center, sparkVel,
                    ResonanceUtils.StarGold * 0.6f, 0.12f, 8);
            }

            // Nebula mist wisps
            if (Main.rand.NextBool(5))
            {
                Color mistCol = Color.Lerp(ResonanceUtils.NebulaMist, ResonanceUtils.NebulaPurple, Main.rand.NextFloat()) * 0.3f;
                ResonanceParticleHandler.Spawn(ResonanceParticleType.MemoryWisp,
                    Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    Main.rand.NextVector2Circular(0.4f, 0.4f),
                    mistCol, 0.13f, 16);
            }

            // Torch dust for baseline visibility
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch,
                    -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 2f),
                    0, default, 1.0f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, ResonanceUtils.NebulaPurple.ToVector3() * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.myPlayer != Projectile.owner) return;

            Player owner = Main.player[Projectile.owner];
            var rp = owner.Resonance();
            rp.HitCounter++;
            rp.CombinedHitCounter++;

            // Bygone Resonance: track bullet hit for dual-hit detection
            bool resonanceTriggered = rp.OnBulletHit(target.whoAmI);

            // Impact VFX
            SpawnImpactParticles(target.Center);

            // Bygone Resonance explosion on trigger
            if (resonanceTriggered && !Main.dedServ)
            {
                // 12-particle resonance ring burst
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                    ResonanceParticleHandler.Spawn(ResonanceParticleType.MuzzleSpark,
                        target.Center, burstVel, ResonanceUtils.BygoneCrimson, 0.3f, 16);
                }
                ResonanceParticleHandler.Spawn(ResonanceParticleType.EchoRing,
                    target.Center, Vector2.Zero, ResonanceUtils.BygoneCrimson, 0.6f, 20);
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.1f, Volume = 0.7f }, target.Center);
            }

            // Every 5th hit spawns spectral blade
            if (rp.HitCounter >= 5)
            {
                rp.HitCounter = 0;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center + Main.rand.NextVector2Circular(50f, 50f),
                    Vector2.Zero,
                    ModContent.ProjectileType<ResonanceSpectralBlade>(),
                    (int)(Projectile.damage * 2f),
                    0f,
                    Projectile.owner,
                    target.whoAmI
                );

                // Major spawn VFX burst
                SpawnBladeSpawnParticles(target.Center);
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.3f }, target.Center);
            }
        }

        private void SpawnImpactParticles(Vector2 pos)
        {
            if (Main.dedServ) return;

            // === FOUNDATION-TIER SPRITEBATCH BLOOM FLASH ===
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                _pointBloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
                _softRadialBloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
                _starFlareTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare", AssetRequestMode.ImmediateLoad);

                if (_softRadialBloomTex?.IsLoaded == true && _pointBloomTex?.IsLoaded == true && _starFlareTex?.IsLoaded == true)
                {
                    Texture2D softBloom = _softRadialBloomTex.Value;
                    Texture2D pointBloom = _pointBloomTex.Value;
                    Texture2D starFlare = _starFlareTex.Value;
                    Vector2 softOrigin = softBloom.Size() / 2f;
                    Vector2 pointOrigin = pointBloom.Size() / 2f;
                    Vector2 starOrigin = starFlare.Size() / 2f;
                    Vector2 drawPos = pos - Main.screenPosition;
                    float time = (float)Main.timeForVisualEffects;

                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                    // Layer 1: Outer nebula mist haze
                    sb.Draw(softBloom, drawPos, null, Additive(ResonanceUtils.NebulaMist, 0.35f), 0f, softOrigin, 0.8f, SpriteEffects.None, 0f);
                    // Layer 2: Nebula purple glow
                    sb.Draw(softBloom, drawPos, null, Additive(ResonanceUtils.NebulaPurple, 0.5f), 0f, softOrigin, 0.5f, SpriteEffects.None, 0f);
                    // Layer 3: Cosmic rose mid
                    sb.Draw(pointBloom, drawPos, null, Additive(ResonanceUtils.CosmicRose, 0.7f), 0f, pointOrigin, 0.4f, SpriteEffects.None, 0f);
                    // Layer 4: Star gold hot core
                    sb.Draw(pointBloom, drawPos, null, Additive(ResonanceUtils.StarGold, 0.8f), 0f, pointOrigin, 0.22f, SpriteEffects.None, 0f);
                    // Layer 5: StarFlare cross burst
                    sb.Draw(starFlare, drawPos, null, Additive(ResonanceUtils.ConstellationSilver, 0.6f), time * 2.1f, starOrigin, 0.3f, SpriteEffects.None, 0f);
                    sb.Draw(starFlare, drawPos, null, Additive(ResonanceUtils.CosmicRose, 0.4f), -time * 1.7f, starOrigin, 0.2f, SpriteEffects.None, 0f);

                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            catch
            {
                try { sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix); } catch { }
            }

            // Echo ring
            ResonanceParticleHandler.Spawn(ResonanceParticleType.EchoRing,
                pos, Vector2.Zero, ResonanceUtils.CosmicRose * 0.7f, 0.35f, 16);

            // 12 radial sparks — gradient colored nebula → rose → gold
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 6f);
                Color col = ResonanceUtils.GradientLerp((float)i / 12f);
                ResonanceParticleHandler.Spawn(ResonanceParticleType.MuzzleSpark,
                    pos, vel, col * 0.8f, 0.18f, 14);
            }

            // 6 directional slash sparks along incoming velocity
            Vector2 impactDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            for (int i = 0; i < 6; i++)
            {
                float spread = MathHelper.Lerp(-0.6f, 0.6f, (float)i / 5f);
                Vector2 vel = impactDir.RotatedBy(spread) * Main.rand.NextFloat(3f, 7f);
                Color col = Color.Lerp(ResonanceUtils.CosmicRose, ResonanceUtils.StarGold, Main.rand.NextFloat());
                ResonanceParticleHandler.Spawn(ResonanceParticleType.MuzzleSpark,
                    pos, vel, col, 0.2f, 10);
            }

            // Dust ring burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4.5f);
                Dust d = Dust.NewDustPerfect(pos, DustID.PinkTorch, vel, 0, default, 1.4f);
                d.noGravity = true;
            }

            // Memory wisp accents
            for (int i = 0; i < 3; i++)
            {
                Color mistCol = Color.Lerp(ResonanceUtils.NebulaMist, ResonanceUtils.NebulaPurple, Main.rand.NextFloat()) * 0.5f;
                ResonanceParticleHandler.Spawn(ResonanceParticleType.MemoryWisp,
                    pos + Main.rand.NextVector2Circular(8f, 8f),
                    Main.rand.NextVector2Circular(1f, 1f), mistCol, 0.15f, 14);
            }

            Lighting.AddLight(pos, ResonanceUtils.CosmicRose.ToVector3() * 0.6f);
            Lighting.AddLight(pos, ResonanceUtils.NebulaPurple.ToVector3() * 0.3f);
        }

        private void SpawnBladeSpawnParticles(Vector2 pos)
        {
            if (Main.dedServ) return;

            // === SPECTRAL BLADE SPAWN — MAJOR BLOOM FLASH EVENT ===
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                _pointBloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
                _softRadialBloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
                _starFlareTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare", AssetRequestMode.ImmediateLoad);

                if (_softRadialBloomTex?.IsLoaded == true && _pointBloomTex?.IsLoaded == true && _starFlareTex?.IsLoaded == true)
                {
                    Texture2D softBloom = _softRadialBloomTex.Value;
                    Texture2D pointBloom = _pointBloomTex.Value;
                    Texture2D starFlare = _starFlareTex.Value;
                    Vector2 softOrigin = softBloom.Size() / 2f;
                    Vector2 pointOrigin = pointBloom.Size() / 2f;
                    Vector2 starOrigin = starFlare.Size() / 2f;
                    Vector2 drawPos = pos - Main.screenPosition;
                    float time = (float)Main.timeForVisualEffects;

                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                    // Layer 1: Massive bygone crimson outer haze
                    sb.Draw(softBloom, drawPos, null, Additive(ResonanceUtils.BygoneCrimson, 0.4f), 0f, softOrigin, 1.4f, SpriteEffects.None, 0f);
                    // Layer 2: Nebula purple wide glow
                    sb.Draw(softBloom, drawPos, null, Additive(ResonanceUtils.NebulaPurple, 0.55f), 0f, softOrigin, 0.9f, SpriteEffects.None, 0f);
                    // Layer 3: Cosmic rose mid bloom
                    sb.Draw(softBloom, drawPos, null, Additive(ResonanceUtils.CosmicRose, 0.65f), 0f, softOrigin, 0.6f, SpriteEffects.None, 0f);
                    // Layer 4: Star gold intensity
                    sb.Draw(pointBloom, drawPos, null, Additive(ResonanceUtils.StarGold, 0.8f), 0f, pointOrigin, 0.45f, SpriteEffects.None, 0f);
                    // Layer 5: Constellation silver white-hot core
                    sb.Draw(pointBloom, drawPos, null, Additive(ResonanceUtils.ConstellationSilver, 0.9f), 0f, pointOrigin, 0.25f, SpriteEffects.None, 0f);
                    // Layer 6: Triple StarFlare divine cross — the blade is being born
                    sb.Draw(starFlare, drawPos, null, Additive(ResonanceUtils.StarGold, 0.7f), time * 1.8f, starOrigin, 0.55f, SpriteEffects.None, 0f);
                    sb.Draw(starFlare, drawPos, null, Additive(ResonanceUtils.ConstellationSilver, 0.5f), -time * 2.3f, starOrigin, 0.4f, SpriteEffects.None, 0f);
                    sb.Draw(starFlare, drawPos, null, Additive(ResonanceUtils.CosmicRose, 0.35f), time * 3.1f, starOrigin, 0.65f, SpriteEffects.None, 0f);

                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            catch
            {
                try { sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix); } catch { }
            }

            // Large echo ring — the bygone reality tears open
            ResonanceParticleHandler.Spawn(ResonanceParticleType.EchoRing,
                pos, Vector2.Zero, ResonanceUtils.ConstellationSilver, 0.8f, 22);
            // Secondary crimson ring
            ResonanceParticleHandler.Spawn(ResonanceParticleType.EchoRing,
                pos, Vector2.Zero, ResonanceUtils.BygoneCrimson * 0.7f, 0.5f, 18);

            // 16-point star burst — gradient colored constellation
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3.5f, 8f);
                Color col = ResonanceUtils.GradientLerp((float)i / 16f);
                ResonanceParticleHandler.Spawn(ResonanceParticleType.BulletGlow,
                    pos, vel, col * 0.85f, 0.28f, 20);
            }

            // 8 directional memory wisps — the blade's bygone echoes
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color col = Color.Lerp(ResonanceUtils.NebulaMist, ResonanceUtils.ConstellationSilver, Main.rand.NextFloat());
                ResonanceParticleHandler.Spawn(ResonanceParticleType.MemoryWisp,
                    pos + Main.rand.NextVector2Circular(20f, 20f), vel, col * 0.6f, 0.22f, 24);
            }

            // Blade arc flash — the spectral blade materializes
            ResonanceParticleHandler.Spawn(ResonanceParticleType.BladeArc,
                pos, Vector2.Zero, ResonanceUtils.StarGold, 0.7f, 18);

            // Cosmic rose glyphs
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(15f, 35f);
                ResonanceParticleHandler.Spawn(ResonanceParticleType.BulletGlow,
                    pos + offset, Vector2.Zero, ResonanceUtils.CosmicRose * 0.6f, 0.18f, 16);
            }

            Lighting.AddLight(pos, ResonanceUtils.StarGold.ToVector3() * 1.2f);
            Lighting.AddLight(pos, ResonanceUtils.BygoneCrimson.ToVector3() * 0.5f);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Death burst
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Color col = ResonanceUtils.GradientLerp(Main.rand.NextFloat());
                ResonanceParticleHandler.Spawn(ResonanceParticleType.MuzzleSpark,
                    Projectile.Center, vel, col * 0.6f, 0.15f, 10);
            }

            ResonanceParticleHandler.Spawn(ResonanceParticleType.EchoRing,
                Projectile.Center, Vector2.Zero, ResonanceUtils.NebulaPurple * 0.4f, 0.2f, 10);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            try
            {
                _pointBloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
                _softRadialBloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
                _starFlareTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare", AssetRequestMode.ImmediateLoad);

                Texture2D tex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
                Vector2 origin = tex.Size() / 2f;
                float pulse = 1f + MathF.Sin(pulsePhase) * 0.15f;
                float time = (float)Main.timeForVisualEffects;

                // === STEP 1: OLD-POSITION TRAIL WITH BLOOM TEXTURES ===
                if (_softRadialBloomTex?.IsLoaded == true)
                {
                    Texture2D softBloom = _softRadialBloomTex.Value;
                    Vector2 softOrigin = softBloom.Size() / 2f;

                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                    for (int i = 0; i < Projectile.oldPos.Length; i++)
                    {
                        if (Projectile.oldPos[i] == Vector2.Zero) continue;
                        float progress = (float)i / Projectile.oldPos.Length;
                        float fade = (1f - progress);
                        Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

                        // Nebula bloom trail — fading nebula mist halos along the path
                        Color nebulaTrail = Color.Lerp(ResonanceUtils.CosmicRose, ResonanceUtils.NebulaPurple, progress);
                        float trailBloomScale = (0.35f - progress * 0.2f) * pulse;
                        sb.Draw(softBloom, trailPos, null, Additive(nebulaTrail, 0.3f * fade), 0f, softOrigin, trailBloomScale, SpriteEffects.None, 0f);

                        // Sharp star trail on top
                        Color trailColor = ResonanceUtils.GradientLerp(progress * 0.8f + 0.2f) * fade * 0.6f;
                        float trailScale = (0.2f - progress * 0.1f) * pulse;
                        sb.Draw(tex, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                    }
                }
                else
                {
                    // Fallback: original star-only trail
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                    for (int i = 0; i < Projectile.oldPos.Length; i++)
                    {
                        if (Projectile.oldPos[i] == Vector2.Zero) continue;
                        float progress = (float)i / Projectile.oldPos.Length;
                        Color trailColor = ResonanceUtils.GradientLerp(progress * 0.8f + 0.2f) * (1f - progress) * 0.5f;
                        Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                        float trailScale = (0.2f - progress * 0.1f) * pulse;
                        sb.Draw(tex, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                    }
                }

                // === STEP 2: GRADUATED FOUNDATION BLOOM BODY ===
                Vector2 drawPos = Projectile.Center - Main.screenPosition;

                if (_pointBloomTex?.IsLoaded == true && _softRadialBloomTex?.IsLoaded == true && _starFlareTex?.IsLoaded == true)
                {
                    Texture2D softBloom = _softRadialBloomTex.Value;
                    Texture2D pointBloom = _pointBloomTex.Value;
                    Texture2D starFlare = _starFlareTex.Value;
                    Vector2 softOrigin = softBloom.Size() / 2f;
                    Vector2 pointOrigin = pointBloom.Size() / 2f;
                    Vector2 starOrigin = starFlare.Size() / 2f;

                    // Layer 1: Outer void-nebula haze — the bygone reality bleeds through
                    sb.Draw(softBloom, drawPos, null, Additive(ResonanceUtils.VoidBlack, 0.2f), 0f, softOrigin, 0.55f * pulse, SpriteEffects.None, 0f);
                    // Layer 2: Nebula mist atmosphere
                    sb.Draw(softBloom, drawPos, null, Additive(ResonanceUtils.NebulaMist, 0.3f), 0f, softOrigin, 0.45f * pulse, SpriteEffects.None, 0f);
                    // Layer 3: Nebula purple glow
                    sb.Draw(softBloom, drawPos, null, Additive(ResonanceUtils.NebulaPurple, 0.5f), 0f, softOrigin, 0.35f * pulse, SpriteEffects.None, 0f);
                    // Layer 4: Cosmic rose intensity
                    sb.Draw(pointBloom, drawPos, null, Additive(ResonanceUtils.CosmicRose, 0.65f), 0f, pointOrigin, 0.28f * pulse, SpriteEffects.None, 0f);
                    // Layer 5: Star gold hot inner
                    sb.Draw(pointBloom, drawPos, null, Additive(ResonanceUtils.StarGold, 0.6f), 0f, pointOrigin, 0.16f * pulse, SpriteEffects.None, 0f);
                    // Layer 6: Constellation silver white core
                    sb.Draw(pointBloom, drawPos, null, Additive(ResonanceUtils.ConstellationSilver, 0.75f), 0f, pointOrigin, 0.1f * pulse, SpriteEffects.None, 0f);
                    // Layer 7: StarFlare rotating cross — cosmic signature
                    sb.Draw(starFlare, drawPos, null, Additive(ResonanceUtils.CosmicRose, 0.35f), time * 1.4f + pulsePhase, starOrigin, 0.2f * pulse, SpriteEffects.None, 0f);
                    sb.Draw(starFlare, drawPos, null, Additive(ResonanceUtils.ConstellationSilver, 0.25f), -time * 1.9f, starOrigin, 0.15f * pulse, SpriteEffects.None, 0f);

                    // === STEP 3: ORIGINAL STAR SPRITE AS SHARP CORE ===
                    sb.Draw(tex, drawPos, null, Additive(ResonanceUtils.CosmicRose, 0.7f), Projectile.rotation, origin, 0.22f * pulse, SpriteEffects.None, 0f);
                    sb.Draw(tex, drawPos, null, Additive(ResonanceUtils.ConstellationSilver, 0.9f), Projectile.rotation, origin, 0.12f * pulse, SpriteEffects.None, 0f);

                    // === STEP 4: LEADING-EDGE BLOOM AT VELOCITY TIP ===
                    Vector2 velDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                    Vector2 leadPos = drawPos + velDir * 6f;

                    sb.Draw(softBloom, leadPos, null, Additive(ResonanceUtils.NebulaPurple, 0.35f), 0f, softOrigin, 0.2f * pulse, SpriteEffects.None, 0f);
                    sb.Draw(pointBloom, leadPos, null, Additive(ResonanceUtils.CosmicRose, 0.5f), 0f, pointOrigin, 0.12f * pulse, SpriteEffects.None, 0f);
                    sb.Draw(pointBloom, leadPos, null, Additive(ResonanceUtils.ConstellationSilver, 0.7f), 0f, pointOrigin, 0.06f * pulse, SpriteEffects.None, 0f);
                }
                else
                {
                    // Fallback to simple star blooms
                    sb.Draw(tex, drawPos, null, ResonanceUtils.NebulaPurple * 0.3f, Projectile.rotation, origin, 0.4f * pulse, SpriteEffects.None, 0f);
                    sb.Draw(tex, drawPos, null, ResonanceUtils.CosmicRose * 0.6f, Projectile.rotation, origin, 0.28f * pulse, SpriteEffects.None, 0f);
                    sb.Draw(tex, drawPos, null, ResonanceUtils.ConstellationSilver * 0.8f, Projectile.rotation, origin, 0.15f * pulse, SpriteEffects.None, 0f);
                }

                // Restore alpha blend
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
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

            // === STEP 5: THEME ACCENTS (additive pass) ===
            try
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                ResonanceUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch { }

            return false;
        }
    }
}