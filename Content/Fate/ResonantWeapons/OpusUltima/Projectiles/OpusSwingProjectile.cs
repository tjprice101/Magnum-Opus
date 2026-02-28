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
using MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Primitives;
using MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Shaders;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Projectiles
{
    /// <summary>
    /// Opus Ultima — Main swing projectile.
    ///
    /// ATTACK SYSTEM: 3-movement combo cycle (musical movements)
    ///   Movement I   (Exposition):     Standard sweep — fires single energy ball
    ///   Movement II  (Development):    Faster cross-slash — fires twin energy balls at angles
    ///   Movement III (Recapitulation): Wide arc — fires massive energy ball (1.5x size, 1.5x damage)
    ///
    /// 5-LAYER RENDERING:
    ///   Layer 1: Wide cosmic glow underlayer (OpusSwingGlow shader)
    ///   Layer 2: Core trail arc (OpusSwingTrail shader + noise texture)
    ///   Layer 3: Golden spark accents along the arc
    ///   Layer 4: UV-rotated weapon sprite + tip lens flare
    ///   Layer 5: Combo intensity aura (glow rings at high combo)
    /// </summary>
    public class OpusSwingProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/OpusUltima";

        // Swing arc parameters per movement [Exposition, Development, Recapitulation]
        private static readonly float[] ArcAngles = { 150f, 120f, 180f };
        private static readonly float[] SwingDurations = { 28f, 20f, 32f };
        private static readonly float[] DamageMultipliers = { 1f, 0.9f, 1.2f };

        // Trail system
        private Vector2[] _trailPoints = new Vector2[24];
        private int _trailCount;
        private float _currentAngle;
        private float _startAngle;
        private int _direction;
        private int _movement; // 0-2

        // Energy ball firing state
        private bool _ballsFired;

        // Textures (lazy loaded)
        private static Asset<Texture2D> _noiseTex;
        private static Asset<Texture2D> _glowTex;
        private static Asset<Texture2D> _flareTex;

        // Properties
        private Player Owner => Main.player[Projectile.owner];
        private float SwingProgress => Projectile.ai[1] / (SwingDurations[_movement] * 2f);
        private float ArcRadians => MathHelper.ToRadians(ArcAngles[_movement]);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 24;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.extraUpdates = 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            var op = Owner.Opus();
            _movement = op.CurrentMovement;

            // Direction: alternate, but Recapitulation always clockwise
            _direction = _movement == 2 ? 1 : (op.SwingCounter % 2 == 0 ? 1 : -1);

            // Start angle: aim toward mouse, offset by half arc
            Vector2 toMouse = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX);
            _startAngle = toMouse.ToRotation() - ArcRadians * 0.5f * _direction;
            _currentAngle = _startAngle;

            Projectile.timeLeft = (int)(SwingDurations[_movement] * 2) + 4;
            Projectile.damage = (int)(Projectile.damage * DamageMultipliers[_movement]);
            _ballsFired = false;
        }

        public override void AI()
        {
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
            Projectile.Center = Owner.MountedCenter;

            float duration = SwingDurations[_movement] * 2f;
            Projectile.ai[1]++;

            if (Projectile.ai[1] >= duration)
            {
                Projectile.Kill();
                return;
            }

            float progress = Projectile.ai[1] / duration;

            // Swing easing per movement
            float easedProgress;
            switch (_movement)
            {
                case 0: // Exposition: smooth sine
                    easedProgress = OpusUtils.SineInOut(progress);
                    break;
                case 1: // Development: fast start
                    easedProgress = OpusUtils.QuadOut(progress);
                    break;
                case 2: // Recapitulation: slow windup, accelerating
                    easedProgress = OpusUtils.CubicIn(progress) * 0.3f + OpusUtils.SineInOut(progress) * 0.7f;
                    break;
                default:
                    easedProgress = progress;
                    break;
            }

            _currentAngle = _startAngle + ArcRadians * easedProgress * _direction;
            float reach = _movement == 2 ? 95f : 78f;
            Projectile.rotation = _currentAngle;
            Vector2 tipPos = Owner.MountedCenter + _currentAngle.ToRotationVector2() * reach;

            // Update trail
            if (_trailCount < _trailPoints.Length)
            {
                _trailPoints[_trailCount] = tipPos;
                _trailCount++;
            }
            else
            {
                Array.Copy(_trailPoints, 1, _trailPoints, 0, _trailPoints.Length - 1);
                _trailPoints[_trailPoints.Length - 1] = tipPos;
            }

            Owner.ChangeDir(Math.Sign(_currentAngle.ToRotationVector2().X));

            // Fire energy balls at ~40% swing progress
            if (!_ballsFired && progress >= 0.4f)
            {
                _ballsFired = true;
                FireEnergyBalls();
            }

            // Spawn particles along arc
            SpawnSwingParticles(tipPos, progress);

            // Lighting with increasing combo intensity
            var op = Owner.Opus();
            Color lightCol = OpusUtils.PaletteLerp(progress * 0.5f + op.ComboIntensity * 0.5f);
            Lighting.AddLight(tipPos, lightCol.ToVector3() * (0.5f + op.ComboIntensity * 0.5f));

            // Sound at ~30% progress
            if ((int)Projectile.ai[1] == (int)(duration * 0.3f))
            {
                SoundStyle sound = _movement == 2
                    ? SoundID.Item71 with { Pitch = -0.2f, Volume = 0.9f }
                    : SoundID.Item1 with { Pitch = 0.2f + _movement * 0.2f, Volume = 0.7f };
                SoundEngine.PlaySound(sound, Owner.Center);
            }
        }

        private void FireEnergyBalls()
        {
            if (Main.myPlayer != Projectile.owner) return;

            var source = Projectile.GetSource_FromThis();
            Vector2 toMouse = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX);
            float speed = 12f;

            switch (_movement)
            {
                case 0: // Exposition: single energy ball
                {
                    Vector2 vel = toMouse * speed;
                    Projectile.NewProjectile(source, Owner.Center + toMouse * 40f, vel,
                        ModContent.ProjectileType<OpusEnergyBallProjectile>(),
                        Projectile.damage, Projectile.knockBack, Projectile.owner,
                        0f, 1f); // ai[0]=EnergyBall, ai[1]=sizeMult
                    break;
                }
                case 1: // Development: twin energy balls at ±15°
                {
                    for (int i = -1; i <= 1; i += 2)
                    {
                        float angle = toMouse.ToRotation() + MathHelper.ToRadians(15f * i);
                        Vector2 vel = angle.ToRotationVector2() * speed;
                        Projectile.NewProjectile(source, Owner.Center + toMouse * 40f, vel,
                            ModContent.ProjectileType<OpusEnergyBallProjectile>(),
                            Projectile.damage, Projectile.knockBack, Projectile.owner,
                            0f, 1f);
                    }
                    break;
                }
                case 2: // Recapitulation: massive energy ball (1.5x size, 1.5x damage)
                {
                    Vector2 vel = toMouse * speed * 0.8f; // Slightly slower but bigger
                    Projectile.NewProjectile(source, Owner.Center + toMouse * 40f, vel,
                        ModContent.ProjectileType<OpusEnergyBallProjectile>(),
                        (int)(Projectile.damage * 1.5f), Projectile.knockBack, Projectile.owner,
                        0f, 1.5f); // ai[1]=1.5 size multiplier
                    break;
                }
            }

            SoundEngine.PlaySound(SoundID.Item117 with { Pitch = 0.3f, Volume = 0.9f }, Owner.Center);
        }

        private void SpawnSwingParticles(Vector2 tipPos, float progress)
        {
            if (Main.dedServ) return;

            var op = Owner.Opus();
            float intensity = 0.5f + op.ComboIntensity * 0.5f;

            // Cosmic sparks at blade tip
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkVel = (_currentAngle + MathHelper.PiOver2 * _direction).ToRotationVector2()
                    * Main.rand.NextFloat(2f, 5f);
                Color sparkCol = OpusUtils.GetCosmicGradient(Main.rand.NextFloat());
                OpusParticleHandler.SpawnParticle(new OpusSpark(
                    tipPos + Main.rand.NextVector2Circular(5f, 5f), sparkVel,
                    sparkCol, 0.25f * intensity, 14));
            }

            // Music notes scatter (more at higher combo)
            if (Main.rand.NextBool(op.ComboIntensity > 0.5f ? 3 : 6))
            {
                Vector2 noteVel = Main.rand.NextVector2Circular(2f, 2f);
                noteVel.Y -= 1.5f;
                Color noteCol = OpusUtils.PaletteLerp(Main.rand.NextFloat(0.3f, 0.8f));
                OpusParticleHandler.SpawnParticle(new OpusNoteParticle(
                    tipPos + Main.rand.NextVector2Circular(10f, 10f), noteVel,
                    noteCol, 0.3f, 30));
            }

            // Nebula wisps during Recapitulation
            if (_movement == 2 && Main.rand.NextBool(3))
            {
                Vector2 wispVel = Main.rand.NextVector2Circular(1f, 1f);
                Color wispCol = Color.Lerp(OpusUtils.RoyalPurple, OpusUtils.CosmicRose, Main.rand.NextFloat());
                OpusParticleHandler.SpawnParticle(new OpusNebulaWisp(
                    tipPos + Main.rand.NextVector2Circular(15f, 15f), wispVel,
                    wispCol, 0.2f, 40));
            }

            // Recapitulation: heavy bloom flares near end of swing
            if (_movement == 2 && progress > 0.7f && Main.rand.NextBool(2))
            {
                Color flareCol = OpusUtils.WithWhitePush(OpusUtils.GloryGold, progress - 0.7f);
                OpusParticleHandler.SpawnParticle(new OpusBloomFlare(
                    tipPos, flareCol, 0.4f * intensity, 12));
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // DestinyCollapse debuff (4 seconds = 240 ticks)
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 240);

            // Spawn seeking crystal shards (3-5, crit=5)
            if (Main.myPlayer == Projectile.owner)
            {
                int shardCount = hit.Crit ? 5 : Main.rand.Next(3, 6);
                for (int i = 0; i < shardCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / shardCount + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 shardVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, shardVel,
                        ModContent.ProjectileType<OpusEnergyBallProjectile>(),
                        (int)(damageDone * 0.4f), 2f, Projectile.owner,
                        2f, 0f); // ai[0]=2 (crystal shard mode)
                }
            }

            SpawnImpactVFX(target.Center);
        }

        private void SpawnImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            var op = Owner.Opus();
            float intensity = 0.6f + op.ComboIntensity * 0.4f;

            // Central bloom flash
            OpusParticleHandler.SpawnParticle(new OpusBloomFlare(
                pos, OpusUtils.OpusWhite, 0.6f * intensity, 15));
            OpusParticleHandler.SpawnParticle(new OpusBloomFlare(
                pos, OpusUtils.OpusCrimson, 0.45f * intensity, 12));

            // Radial spark burst
            int sparkCount = 8 + (int)(op.ComboIntensity * 4);
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f) * intensity;
                Color sparkCol = OpusUtils.GetCosmicGradient((float)i / sparkCount);
                OpusParticleHandler.SpawnParticle(new OpusSpark(
                    pos, sparkVel, sparkCol, 0.3f * intensity, 16));
            }

            // Glyph accents
            int glyphCount = 2 + (int)(op.ComboIntensity * 3);
            for (int i = 0; i < glyphCount; i++)
            {
                Vector2 glyphPos = pos + Main.rand.NextVector2Circular(20f, 20f);
                Color glyphCol = OpusUtils.PaletteLerp(Main.rand.NextFloat());
                OpusParticleHandler.SpawnParticle(new OpusGlyph(
                    glyphPos, glyphCol, 0.28f * intensity, 25));
            }

            // Music notes on impact
            for (int i = 0; i < 3; i++)
            {
                Vector2 noteVel = Main.rand.NextVector2Circular(3f, 3f);
                noteVel.Y -= 2f;
                Color noteCol = OpusUtils.PaletteLerp(Main.rand.NextFloat(0.3f, 0.8f));
                OpusParticleHandler.SpawnParticle(new OpusNoteParticle(
                    pos + Main.rand.NextVector2Circular(10f, 10f), noteVel,
                    noteCol, 0.35f, 28));
            }

            Lighting.AddLight(pos, OpusUtils.GloryGold.ToVector3() * 1.0f * intensity);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float reach = _movement == 2 ? 95f : 78f;
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + _currentAngle.ToRotationVector2() * reach;
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 30f, ref _);
        }

        // ======================== 5-LAYER RENDERING ========================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ || _trailCount < 2) return false;

            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/PerlinNoise");
            _glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
            _flareTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Flare/flare_16");

            SpriteBatch sb = Main.spriteBatch;
            var op = Owner.Opus();
            float comboIntensity = op.ComboIntensity;
            float progress = SwingProgress;

            DrawLayer1_CosmicGlow(sb, comboIntensity);
            DrawLayer2_CoreTrail(sb, comboIntensity);
            DrawLayer3_GoldenSparks(sb, progress, comboIntensity);
            DrawLayer4_WeaponSprite(sb, lightColor);
            DrawLayer5_ComboAura(sb, comboIntensity);

            return false;
        }

        private void DrawLayer1_CosmicGlow(SpriteBatch sb, float combo)
        {
            var shader = OpusShaderLoader.GetSwingGlow();
            if (shader == null || _trailCount < 2) return;

            try
            {
                shader.UseColor(OpusUtils.VoidBlack.ToVector3());
                shader.UseSecondaryColor(OpusUtils.RoyalPurple.ToVector3());
                shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2f);
                shader.Shader.Parameters["uOpacity"]?.SetValue(0.4f + combo * 0.2f);
                shader.Shader.Parameters["uIntensity"]?.SetValue(1.2f);
                shader.Shader.Parameters["uOverbrightMult"]?.SetValue(1.5f);
                shader.Shader.Parameters["uPhase"]?.SetValue(combo);

                OpusTrailRenderer.RenderTrail(_trailPoints, new OpusTrailSettings(
                    (p, _) => (40f + combo * 15f) * (1f - p * 0.5f),
                    (p) => Color.Lerp(OpusUtils.Additive(OpusUtils.RoyalPurple, 0.3f),
                                      OpusUtils.Additive(OpusUtils.VoidBlack, 0.1f), p),
                    shader: shader), _trailCount, 3);
            }
            catch { }
        }

        private void DrawLayer2_CoreTrail(SpriteBatch sb, float combo)
        {
            var shader = OpusShaderLoader.GetSwingTrail();
            if (shader == null || _trailCount < 2) return;

            try
            {
                if (_noiseTex?.Value != null)
                    shader.UseImage1(_noiseTex);

                shader.UseColor(OpusUtils.OpusCrimson.ToVector3());
                shader.UseSecondaryColor(OpusUtils.GloryGold.ToVector3());
                shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 3f);
                shader.Shader.Parameters["uOpacity"]?.SetValue(0.8f + combo * 0.2f);
                shader.Shader.Parameters["uIntensity"]?.SetValue(1.5f + combo * 0.5f);
                shader.Shader.Parameters["uOverbrightMult"]?.SetValue(1.8f);
                shader.Shader.Parameters["uScrollSpeed"]?.SetValue(1.2f);
                shader.Shader.Parameters["uNoiseScale"]?.SetValue(2.5f);
                shader.Shader.Parameters["uPhase"]?.SetValue(combo);
                shader.Shader.Parameters["uHasSecondaryTex"]?.SetValue(_noiseTex?.Value != null ? 1f : 0f);
                shader.Shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);

                OpusTrailRenderer.RenderTrail(_trailPoints, new OpusTrailSettings(
                    (p, _) =>
                    {
                        float baseWidth = 22f + combo * 8f;
                        float taper = MathF.Sin(p * MathHelper.Pi);
                        return baseWidth * (0.3f + taper * 0.7f);
                    },
                    (p) =>
                    {
                        Color c = OpusUtils.GetCosmicGradient(p);
                        float alpha = (1f - p * 0.6f);
                        return OpusUtils.Additive(c, alpha);
                    },
                    shader: shader), _trailCount, 3);
            }
            catch { }
        }

        private void DrawLayer3_GoldenSparks(SpriteBatch sb, float progress, float combo)
        {
            if (_flareTex?.Value == null || _trailCount < 3) return;

            try
            {
                OpusUtils.BeginAdditive(sb);

                var tex = _flareTex.Value;
                Vector2 origin = tex.Size() / 2f;
                float time = (float)Main.timeForVisualEffects;

                for (int i = 1; i < _trailCount - 1; i += 2)
                {
                    float t = (float)i / _trailCount;
                    float sparkAlpha = (1f - t) * (0.3f + combo * 0.4f);

                    float twinkle = MathF.Sin(time * 0.2f + i * 1.7f) * 0.3f + 0.7f;
                    sparkAlpha *= twinkle;

                    if (sparkAlpha < 0.05f) continue;

                    Color sparkCol = Color.Lerp(OpusUtils.OpusCrimson, OpusUtils.GloryGold, t);
                    Vector2 drawPos = _trailPoints[i] - Main.screenPosition;
                    float sparkScale = 0.15f + combo * 0.08f;

                    sb.Draw(tex, drawPos, null, OpusUtils.Additive(sparkCol, sparkAlpha),
                        time * 0.5f + i, origin, sparkScale * twinkle, SpriteEffects.None, 0f);
                }

                OpusUtils.EndAdditive(sb);
            }
            catch
            {
                try { OpusUtils.EndAdditive(sb); } catch { }
            }
        }

        private void DrawLayer4_WeaponSprite(SpriteBatch sb, Color lightColor)
        {
            try
            {
                Texture2D weaponTex = TextureAssets.Projectile[Projectile.type].Value;
                Vector2 origin = weaponTex.Size() / 2f;
                float reach = _movement == 2 ? 95f : 78f;
                Vector2 drawPos = Owner.MountedCenter + _currentAngle.ToRotationVector2() * reach * 0.5f - Main.screenPosition;

                Color weaponColor = Color.Lerp(lightColor, OpusUtils.StarSilver, 0.3f);
                float drawRotation = _currentAngle + MathHelper.PiOver4;
                SpriteEffects fx = _direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                sb.Draw(weaponTex, drawPos, null, weaponColor, drawRotation, origin, Projectile.scale, fx, 0f);

                // Tip lens flare
                if (_glowTex?.Value != null)
                {
                    Vector2 tipPos = Owner.MountedCenter + _currentAngle.ToRotationVector2() * reach - Main.screenPosition;
                    float flarePulse = 0.8f + MathF.Sin((float)Main.timeForVisualEffects * 0.08f) * 0.2f;
                    var op = Owner.Opus();

                    OpusUtils.BeginAdditive(sb);
                    // Crimson core flare
                    sb.Draw(_glowTex.Value, tipPos, null, OpusUtils.Additive(OpusUtils.OpusCrimson, 0.5f * flarePulse),
                        0f, _glowTex.Value.Size() / 2f, 0.35f * flarePulse, SpriteEffects.None, 0f);
                    // Gold outer halo (grows with combo)
                    sb.Draw(_glowTex.Value, tipPos, null, OpusUtils.Additive(OpusUtils.GloryGold, 0.25f * flarePulse * op.ComboIntensity),
                        0f, _glowTex.Value.Size() / 2f, 0.5f * flarePulse * (0.5f + op.ComboIntensity * 0.5f), SpriteEffects.None, 0f);
                    // White-hot core
                    sb.Draw(_glowTex.Value, tipPos, null, OpusUtils.Additive(OpusUtils.OpusWhite, 0.3f * flarePulse),
                        0f, _glowTex.Value.Size() / 2f, 0.18f * flarePulse, SpriteEffects.None, 0f);
                    OpusUtils.EndAdditive(sb);
                }
            }
            catch
            {
                try { OpusUtils.EndAdditive(sb); } catch { }
            }
        }

        private void DrawLayer5_ComboAura(SpriteBatch sb, float combo)
        {
            if (combo < 0.3f) return;
            if (_glowTex?.Value == null) return;

            try
            {
                float auraAlpha = (combo - 0.3f) / 0.7f;
                float pulse = MathF.Sin((float)Main.timeForVisualEffects * 0.06f) * 0.15f + 0.85f;

                OpusUtils.BeginAdditive(sb);

                Vector2 center = Owner.MountedCenter - Main.screenPosition;
                var tex = _glowTex.Value;
                Vector2 origin = tex.Size() / 2f;

                // Concentric rings: crimson → gold → white (grows with combo)
                Color[] ringColors = { OpusUtils.OpusCrimson, OpusUtils.GloryGold, OpusUtils.OpusWhite };
                for (int ring = 0; ring < 3; ring++)
                {
                    float ringScale = (0.8f + ring * 0.5f) * pulse;
                    float ringAlpha = auraAlpha * (1f - ring * 0.25f) * 0.15f;
                    sb.Draw(tex, center, null, OpusUtils.Additive(ringColors[ring], ringAlpha),
                        0f, origin, ringScale, SpriteEffects.None, 0f);
                }

                OpusUtils.EndAdditive(sb);
            }
            catch
            {
                try { OpusUtils.EndAdditive(sb); } catch { }
            }
        }
    }
}
