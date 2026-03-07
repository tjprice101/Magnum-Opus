using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Primitives;
using MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Shaders;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Projectiles
{
    /// <summary>
    /// Opus Ultima — Cosmic Energy Ball + Homing Seeker + Crystal Shard projectile.
    ///
    /// Three modes controlled via ai[0]:
    ///   Mode 0 (Energy Ball):   Travels forward, on enemy hit → explodes into 5 homing seekers.
    ///                           ai[1] = size multiplier (1.0 normal, 1.5 massive for Recapitulation).
    ///   Mode 1 (Seeker):        Homes toward nearest enemy, deals damage on contact.
    ///   Mode 2 (Crystal Shard): Spawned on melee hit, homes to enemies at 40% damage.
    ///
    /// All three modes share the same projectile type for self-containment.
    /// </summary>
    public class OpusEnergyBallProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/OpusUltima";

        // Mode constants
        private const int ModeEnergyBall = 0;
        private const int ModeSeeker = 1;
        private const int ModeCrystalShard = 2;

        // Trail points for seekers/shards
        private Vector2[] _trail = new Vector2[16];
        private int _trailCount;

        // Textures
        private static Asset<Texture2D> _glowTex;
        private static Asset<Texture2D> _flareTex;
        private static Asset<Texture2D> _noiseTex;

        private int Mode => (int)Projectile.ai[0];
        private float SizeMult => Mode == ModeEnergyBall ? Math.Max(Projectile.ai[1], 1f) : 1f;
        private Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            switch (Mode)
            {
                case ModeEnergyBall:
                    EnergyBallAI();
                    break;
                case ModeSeeker:
                    SeekerAI();
                    break;
                case ModeCrystalShard:
                    CrystalShardAI();
                    break;
            }

            // Update trail
            if (_trailCount < _trail.Length)
            {
                _trail[_trailCount] = Projectile.Center;
                _trailCount++;
            }
            else
            {
                Array.Copy(_trail, 1, _trail, 0, _trail.Length - 1);
                _trail[_trail.Length - 1] = Projectile.Center;
            }
        }

        // ======================== ENERGY BALL AI ========================

        private void EnergyBallAI()
        {
            float scale = SizeMult;
            Projectile.scale = 1f * scale;

            // Slight gravity pull toward nearest enemy (gentle homing)
            NPC target = OpusUtils.ClosestNPCAt(Projectile.Center, 400f);
            if (target != null)
            {
                Vector2 toTarget = OpusUtils.SafeDirectionTo(Projectile.Center, target.Center);
                Projectile.velocity += toTarget * 0.3f;
                float maxSpeed = 14f;
                if (Projectile.velocity.Length() > maxSpeed)
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * maxSpeed;
            }

            Projectile.rotation += 0.08f * scale;

            // Spawn ambient particles
            if (!Main.dedServ && Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(15f * scale, 15f * scale);
                Color moteCol = OpusUtils.GetCosmicGradient(Main.rand.NextFloat());
                OpusParticleHandler.SpawnParticle(new OpusMote(
                    Projectile.Center + offset, -Projectile.velocity * 0.1f,
                    moteCol, 0.2f * scale, 20));
            }

            // Golden aura glow
            Lighting.AddLight(Projectile.Center, OpusUtils.GloryGold.ToVector3() * 0.6f * scale);
        }

        // ======================== SEEKER AI ========================

        private void SeekerAI()
        {
            Projectile.scale = 0.6f;
            Projectile.timeLeft = Math.Min(Projectile.timeLeft, 180);

            // Aggressive homing
            NPC target = OpusUtils.ClosestNPCAt(Projectile.Center, 600f);
            if (target != null)
            {
                Vector2 toTarget = OpusUtils.SafeDirectionTo(Projectile.Center, target.Center);
                float homingStrength = 0.8f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 12f, homingStrength * 0.1f);
            }

            float maxSpeed = 14f;
            if (Projectile.velocity.Length() > maxSpeed)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * maxSpeed;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail particles
            if (!Main.dedServ && Main.rand.NextBool(3))
            {
                Color sparkCol = Color.Lerp(OpusUtils.OpusCrimson, OpusUtils.GloryGold, Main.rand.NextFloat());
                OpusParticleHandler.SpawnParticle(new OpusSpark(
                    Projectile.Center, -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f),
                    sparkCol, 0.15f, 10));
            }

            Lighting.AddLight(Projectile.Center, OpusUtils.OpusCrimson.ToVector3() * 0.4f);
        }

        // ======================== CRYSTAL SHARD AI ========================

        private void CrystalShardAI()
        {
            Projectile.scale = 0.5f;
            Projectile.timeLeft = Math.Min(Projectile.timeLeft, 120);

            // Homing after brief delay
            Projectile.ai[1] += 1f;
            if (Projectile.ai[1] > 15f)
            {
                NPC target = OpusUtils.ClosestNPCAt(Projectile.Center, 500f);
                if (target != null)
                {
                    Vector2 toTarget = OpusUtils.SafeDirectionTo(Projectile.Center, target.Center);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 10f, 0.12f);
                }
            }

            float maxSpeed = 12f;
            if (Projectile.velocity.Length() > maxSpeed)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * maxSpeed;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Crystal sparkle
            if (!Main.dedServ && Main.rand.NextBool(4))
            {
                Color col = Color.Lerp(OpusUtils.GloryGold, OpusUtils.OpusWhite, Main.rand.NextFloat());
                OpusParticleHandler.SpawnParticle(new OpusMote(
                    Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    -Projectile.velocity * 0.05f, col, 0.12f, 12));
            }

            Lighting.AddLight(Projectile.Center, OpusUtils.GloryGold.ToVector3() * 0.3f);
        }

        // ======================== ON HIT ========================

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // DestinyCollapse for all modes
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 240);

            if (Mode == ModeEnergyBall)
            {
                // EXPLODE into 5 homing seekers
                ExplodeIntoSeekers(target);
            }

            // Impact VFX for all modes
            SpawnHitVFX(target.Center, Mode == ModeEnergyBall ? SizeMult : 0.5f);
        }

        public override void OnKill(int timeLeft)
        {
            // If energy ball expires without hitting, still spawn a small VFX burst
            if (Mode == ModeEnergyBall)
            {
                SpawnHitVFX(Projectile.Center, SizeMult * 0.5f);
            }
        }

        private void ExplodeIntoSeekers(NPC hitTarget)
        {
            if (Main.myPlayer != Projectile.owner) return;

            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.4f, Volume = 0.8f }, Projectile.Center);

            int seekerCount = 5;
            for (int i = 0; i < seekerCount; i++)
            {
                float angle = MathHelper.TwoPi * i / seekerCount + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 seekerVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, seekerVel,
                    Projectile.type, (int)(Projectile.damage * 0.6f), 3f, Projectile.owner,
                    1f, 0f); // ai[0]=1 (seeker mode)
            }
        }

        private void SpawnHitVFX(Vector2 pos, float scale)
        {
            if (Main.dedServ) return;

            // Central bloom flash
            OpusParticleHandler.SpawnParticle(new OpusBloomFlare(
                pos, OpusUtils.OpusWhite, 0.5f * scale, 18));
            OpusParticleHandler.SpawnParticle(new OpusBloomFlare(
                pos, OpusUtils.GloryGold, 0.4f * scale, 14));
            OpusParticleHandler.SpawnParticle(new OpusBloomFlare(
                pos, OpusUtils.OpusCrimson, 0.35f * scale, 12));

            // Radial sparks (more for energy ball, fewer for seekers/shards)
            int sparkCount = Mode == ModeEnergyBall ? 12 : 6;
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f) * scale;
                Color sparkCol = OpusUtils.GetCosmicGradient((float)i / sparkCount);
                OpusParticleHandler.SpawnParticle(new OpusSpark(
                    pos, sparkVel, sparkCol, 0.25f * scale, 14));
            }

            // Glyphs for energy ball explosion
            if (Mode == ModeEnergyBall)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 glyphPos = pos + Main.rand.NextVector2Circular(25f * scale, 25f * scale);
                    Color glyphCol = OpusUtils.PaletteLerp(Main.rand.NextFloat());
                    OpusParticleHandler.SpawnParticle(new OpusGlyph(
                        glyphPos, glyphCol, 0.35f * scale, 30));
                }

                // Music notes cascade from explosion
                for (int i = 0; i < 5; i++)
                {
                    Vector2 noteVel = Main.rand.NextVector2Circular(4f, 4f);
                    noteVel.Y -= 2.5f;
                    Color noteCol = OpusUtils.PaletteLerp(Main.rand.NextFloat(0.2f, 0.9f));
                    OpusParticleHandler.SpawnParticle(new OpusNoteParticle(
                        pos + Main.rand.NextVector2Circular(15f, 15f), noteVel,
                        noteCol, 0.35f * scale, 35));
                }
            }

            Lighting.AddLight(pos, OpusUtils.GloryGold.ToVector3() * 0.8f * scale);
        }

        // ======================== RENDERING ========================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            SpriteBatch sb = Main.spriteBatch;

            try
            {
                _glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
                _flareTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Flare/flare_16");
                _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/CosmicNebulaClouds");

                switch (Mode)
                {
                    case ModeEnergyBall:
                        DrawEnergyBall(sb);
                        break;
                    case ModeSeeker:
                        DrawSeeker(sb);
                        break;
                    case ModeCrystalShard:
                        DrawCrystalShard(sb);
                        break;
                }
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

        private void DrawEnergyBall(SpriteBatch sb)
        {
            if (_glowTex?.Value == null) return;

            try
            {
                OpusUtils.BeginAdditive(sb);

                float scale = SizeMult;
                float time = (float)Main.timeForVisualEffects;
                float pulse = 1f + MathF.Sin(time * 0.08f) * 0.1f;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                var tex = _glowTex.Value;
                Vector2 origin = tex.Size() / 2f;

                // Layer 1: Void black outer shell
                sb.Draw(tex, drawPos, null, OpusUtils.Additive(OpusUtils.VoidBlack, 0.15f),
                    0f, origin, MathHelper.Min(1.35f * scale * pulse, 0.586f), SpriteEffects.None, 0f);

                // Layer 2: Purple haze
                sb.Draw(tex, drawPos, null, OpusUtils.Additive(OpusUtils.RoyalPurple, 0.23f),
                    0f, origin, MathHelper.Min(0.98f * scale * pulse, 0.586f), SpriteEffects.None, 0f);

                // Layer 3: Crimson fire
                float crimsonPulse = pulse * (1f + MathF.Sin(time * 0.12f + 1.3f) * 0.08f);
                sb.Draw(tex, drawPos, null, OpusUtils.Additive(OpusUtils.OpusCrimson, 0.38f),
                    0f, origin, MathHelper.Min(0.68f * scale * crimsonPulse, 0.586f), SpriteEffects.None, 0f);

                // Layer 4: Gold glory ring
                sb.Draw(tex, drawPos, null, OpusUtils.Additive(OpusUtils.GloryGold, 0.34f),
                    0f, origin, MathHelper.Min(0.49f * scale * pulse, 0.586f), SpriteEffects.None, 0f);

                // Layer 5: White-hot core
                sb.Draw(tex, drawPos, null, OpusUtils.Additive(OpusUtils.OpusWhite, 0.45f),
                    0f, origin, 0.23f * scale * pulse, SpriteEffects.None, 0f);

                // Rotating flare spike cross
                if (_flareTex?.Value != null)
                {
                    float rot = time * 0.03f;
                    for (int i = 0; i < 4; i++)
                    {
                        float fRot = rot + MathHelper.PiOver2 * i;
                        Color fCol = Color.Lerp(OpusUtils.GloryGold, OpusUtils.OpusCrimson, (i % 2) * 0.5f);
                        sb.Draw(_flareTex.Value, drawPos, null, OpusUtils.Additive(fCol, 0.26f),
                            fRot, _flareTex.Value.Size() / 2f, 0.23f * scale, SpriteEffects.None, 0f);
                    }
                }

                OpusUtils.EndAdditive(sb);
            }
            catch
            {
                try { OpusUtils.EndAdditive(sb); } catch { }
            }
        }

        private void DrawSeeker(SpriteBatch sb)
        {
            if (_glowTex?.Value == null) return;

            try
            {
                // === GPU Shader Trail (Opus Seeker) ===
                if (OpusShaderLoader.HasSeekerTrail)
                {
                    try
                    {
                        sb.End();
                        var shader = OpusShaderLoader.GetSeekerTrail();
                        if (shader != null)
                        {
                            shader.UseColor(OpusUtils.GloryGold.ToVector3());
                            shader.UseSecondaryColor(OpusUtils.OpusCrimson.ToVector3());
                            shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 3f);
                            shader.Shader.Parameters["uOpacity"]?.SetValue(0.7f);

                            var trailSettings = new OpusTrailSettings(
                                width: (progress, idx) => 18f * (1f - progress * 0.85f),
                                color: progress => Color.Lerp(
                                    OpusUtils.GloryGold with { A = 0 },
                                    OpusUtils.RoyalPurple with { A = 0 },
                                    progress) * (1f - progress * 0.8f),
                                offset: null,
                                shader: shader);

                            OpusTrailRenderer.RenderTrail(Projectile.oldPos, trailSettings, Projectile.oldPos.Length, 2);
                        }
                    }
                    catch { }
                    finally
                    {
                        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    }
                }

                OpusUtils.BeginAdditive(sb);

                float time = (float)Main.timeForVisualEffects;
                float pulse = 0.9f + MathF.Sin(time * 0.15f) * 0.1f;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                var tex = _glowTex.Value;
                Vector2 origin = tex.Size() / 2f;

                // Trail afterimages
                if (_trailCount > 2)
                {
                    for (int i = 0; i < _trailCount - 1; i++)
                    {
                        float t = (float)i / _trailCount;
                        float alpha = (1f - t) * 0.25f;
                        Vector2 trailPos = _trail[i] - Main.screenPosition;
                        Color trailCol = Color.Lerp(OpusUtils.GloryGold, OpusUtils.OpusCrimson, t);
                        sb.Draw(tex, trailPos, null, OpusUtils.Additive(trailCol, alpha * 0.75f),
                            0f, origin, 0.19f * (1f - t * 0.5f), SpriteEffects.None, 0f);
                    }
                }

                // Core glow
                sb.Draw(tex, drawPos, null, OpusUtils.Additive(OpusUtils.OpusCrimson, 0.38f),
                    0f, origin, 0.3f * pulse, SpriteEffects.None, 0f);
                sb.Draw(tex, drawPos, null, OpusUtils.Additive(OpusUtils.GloryGold, 0.3f),
                    0f, origin, 0.19f * pulse, SpriteEffects.None, 0f);
                sb.Draw(tex, drawPos, null, OpusUtils.Additive(OpusUtils.OpusWhite, 0.38f),
                    0f, origin, 0.09f * pulse, SpriteEffects.None, 0f);

                OpusUtils.EndAdditive(sb);
            }
            catch
            {
                try { OpusUtils.EndAdditive(sb); } catch { }
            }
        }

        private void DrawCrystalShard(SpriteBatch sb)
        {
            if (_glowTex?.Value == null) return;

            try
            {
                OpusUtils.BeginAdditive(sb);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                var tex = _glowTex.Value;
                Vector2 origin = tex.Size() / 2f;

                // Crystal-like pointed shape using flare
                if (_flareTex?.Value != null)
                {
                    float rot = Projectile.rotation;
                    sb.Draw(_flareTex.Value, drawPos, null, OpusUtils.Additive(OpusUtils.GloryGold, 0.38f),
                        rot, _flareTex.Value.Size() / 2f, new Vector2(0.15f, 0.075f), SpriteEffects.None, 0f);
                }

                // Small glow core
                sb.Draw(tex, drawPos, null, OpusUtils.Additive(OpusUtils.GloryGold, 0.3f),
                    0f, origin, 0.15f, SpriteEffects.None, 0f);
                sb.Draw(tex, drawPos, null, OpusUtils.Additive(OpusUtils.OpusWhite, 0.26f),
                    0f, origin, 0.08f, SpriteEffects.None, 0f);

                OpusUtils.EndAdditive(sb);
            }
            catch
            {
                try { OpusUtils.EndAdditive(sb); } catch { }
            }
        }
    }
}
