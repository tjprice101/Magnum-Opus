using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Particles;
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
    /// Rendering uses IncisorOrbRenderer.Fate for consistent Fate-themed shader trail + bloom head.
    /// </summary>
    public class OpusEnergyBallProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/OpusUltima";

        // Mode constants
        private const int ModeEnergyBall = 0;
        private const int ModeSeeker = 1;
        private const int ModeCrystalShard = 2;

        // VertexStrip for IncisorOrbRenderer shader trail
        private VertexStrip _strip;

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

            // All 3 modes use the same IncisorOrbRenderer Fate pipeline:
            // LAYER 1: Shader-driven VertexStrip beam body with Fate gradient LUT
            // LAYER 2: Multi-layer bloom head with Fate palette cycling
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.Fate, ref _strip);

            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }
    }
}
