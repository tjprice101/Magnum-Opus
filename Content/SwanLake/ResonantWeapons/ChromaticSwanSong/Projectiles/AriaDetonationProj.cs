using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Utilities;
using MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Particles;
using MagnumOpus.Content.SwanLake.Debuffs;
using ReLogic.Content;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Projectiles
{
    /// <summary>
    /// Aria Detonation — triggered when 3 consecutive chromatic bolts hit the same target.
    /// 
    /// BEHAVIOR:
    /// • Spawns at target center, deals massive AoE damage in expanding radius
    /// • Expanding chromatic rings (4 layers) with rainbow particle shards
    /// • If ai[0] >= 1, this is a Harmonic Release: fires 12 rainbow shards outward
    /// • Applies FlameOfTheSwan for 8 seconds
    /// • No collision, pure VFX explosion with damage tick
    /// </summary>
    public class AriaDetonationProj : ModProjectile
    {
        private bool IsHarmonicRelease => Projectile.ai[0] >= 1f;
        private int _ticksAlive;

        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/ChromaticSwanSong/ChromaticSwanSong";

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            _ticksAlive++;

            if (_ticksAlive == 1)
            {
                // Initial burst effects
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.8f, Volume = 0.8f }, Projectile.Center);

                // Expanding chromatic rings
                int ringCount = IsHarmonicRelease ? 5 : 3;
                for (int r = 0; r < ringCount; r++)
                {
                    var ring = new AriaBurstParticle();
                    ring.Initialize(
                        Projectile.Center, Vector2.Zero,
                        ChromaticSwanUtils.GetSpectrumColor((float)r / ringCount),
                        0.9f - r * 0.1f
                    );
                    ring.Setup((IsHarmonicRelease ? 150f : 80f) + r * 30f);
                    ChromaticParticleHandler.Spawn(ring);
                }

                // Prismatic shard burst
                int shardCount = IsHarmonicRelease ? 24 : 12;
                for (int i = 0; i < shardCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / shardCount;
                    float speed = Main.rand.NextFloat(3f, 8f) * (IsHarmonicRelease ? 1.5f : 1f);

                    var shard = new PrismaticShardParticle();
                    shard.Initialize(
                        Projectile.Center,
                        new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed,
                        ChromaticSwanUtils.GetSpectrumColor((float)i / shardCount),
                        Main.rand.NextFloat(0.6f, 1.2f)
                    );
                    ChromaticParticleHandler.Spawn(shard);
                }

                // Harmonic notes bursting outward
                for (int i = 0; i < 8; i++)
                {
                    var note = new HarmonicNoteParticle();
                    note.Initialize(
                        Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                        new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f)),
                        ChromaticSwanUtils.GetChromatic(Main.rand.NextFloat()),
                        Main.rand.NextFloat(0.5f, 1.0f)
                    );
                    ChromaticParticleHandler.Spawn(note);
                }

                // Chromatic sparks
                for (int i = 0; i < 20; i++)
                {
                    var spark = new ChromaticSparkParticle();
                    spark.Initialize(
                        Projectile.Center + Main.rand.NextVector2Circular(30f, 30f),
                        Main.rand.NextVector2Circular(6f, 6f),
                        ChromaticSwanUtils.GetChromatic(Main.rand.NextFloat()),
                        Main.rand.NextFloat(0.5f, 1.2f)
                    );
                    ChromaticParticleHandler.Spawn(spark);
                }

                // Vanilla dust for fullness
                for (int i = 0; i < 15; i++)
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                        DustID.RainbowTorch,
                        Main.rand.NextVector2Circular(8f, 8f), 0,
                        ChromaticSwanUtils.GetChromatic(Main.rand.NextFloat()), 1.3f);
                    d.noGravity = true;
                }

                // VFX Library: Feather burst for the aria's grand finale
                SwanLakeVFXLibrary.SpawnFeatherBurst(Projectile.Center, IsHarmonicRelease ? 6 : 3, 0.4f);

                // VFX Library: Prismatic sparkles cascading outward
                SwanLakeVFXLibrary.SpawnPrismaticSparkles(Projectile.Center, IsHarmonicRelease ? 8 : 4,
                    IsHarmonicRelease ? 40f : 25f);

                // VFX Library: Music notes rising from the detonation
                SwanLakeVFXLibrary.SpawnMusicNotes(Projectile.Center, IsHarmonicRelease ? 6 : 3,
                    IsHarmonicRelease ? 35f : 20f, 0.7f, 1.1f, 30);
            }

            // Expanding hitbox over lifetime
            float expansion = (float)_ticksAlive / Projectile.timeLeft;
            int newSize = (int)MathHelper.Lerp(60f, IsHarmonicRelease ? 250f : 150f, expansion);
            Projectile.Resize(newSize, newSize);

            // Continuous light
            Color lightCol = ChromaticSwanUtils.GetChromatic(0f);
            float intensity = 1f - (float)_ticksAlive / 20f;
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * intensity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 480); // 8 seconds
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw bloom flash
            SpriteBatch sb = Main.spriteBatch;
            float progress = (float)_ticksAlive / 20f;
            float alpha = (1f - progress) * (1f - progress);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float scale = MathHelper.Lerp(0.3f, IsHarmonicRelease ? 2.5f : 1.5f, progress);

            // Load VFX Asset Library bloom textures
            Texture2D softRadial = null;
            Texture2D pointBloom = null;
            Texture2D starAccent = null;
            try
            {
                softRadial = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom",
                    AssetRequestMode.ImmediateLoad)?.Value;
                pointBloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom",
                    AssetRequestMode.ImmediateLoad)?.Value;
                starAccent = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar",
                    AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Outer chromatic shockwave halo (SoftRadialBloom)
            if (softRadial != null)
            {
                Vector2 srOrigin = new Vector2(softRadial.Width / 2f, softRadial.Height / 2f);
                Color outer = ChromaticSwanUtils.GetChromatic(progress);
                sb.Draw(softRadial, drawPos, null, new Color(outer.R, outer.G, outer.B, 0) * alpha * 0.45f,
                    0f, srOrigin, scale * 1.2f, SpriteEffects.None, 0f);
            }

            // Layer 2: Mid-blast chromatic glow (SoftRadialBloom, shifted)
            if (softRadial != null)
            {
                Vector2 srOrigin = new Vector2(softRadial.Width / 2f, softRadial.Height / 2f);
                Color midCol = ChromaticSwanUtils.GetSpectrumColor(progress * 0.5f);
                sb.Draw(softRadial, drawPos, null, new Color(midCol.R, midCol.G, midCol.B, 0) * alpha * 0.35f,
                    0f, srOrigin, scale * 0.6f, SpriteEffects.None, 0f);
            }

            // Layer 3: White-hot core flash (PointBloom)
            if (pointBloom != null)
            {
                Vector2 pbOrigin = new Vector2(pointBloom.Width / 2f, pointBloom.Height / 2f);
                sb.Draw(pointBloom, drawPos, null, new Color(255, 255, 255, 0) * alpha * 0.7f,
                    0f, pbOrigin, scale * 0.25f, SpriteEffects.None, 0f);
            }

            // Layer 4: Radiating star burst at detonation apex (ThinTall4PointedStar)
            if (starAccent != null && progress < 0.5f)
            {
                Vector2 starOrigin = new Vector2(starAccent.Width / 2f, starAccent.Height / 2f);
                float starAlpha = (1f - progress * 2f);
                Color starCol = ChromaticSwanUtils.GetChromatic(progress * 2f);
                sb.Draw(starAccent, drawPos, null, new Color(starCol.R, starCol.G, starCol.B, 0) * starAlpha * 0.6f,
                    progress * MathHelper.TwoPi, starOrigin,
                    (IsHarmonicRelease ? 0.5f : 0.3f) * (1f + progress), SpriteEffects.None, 0f);
                // Second star rotated 45° for cross pattern
                sb.Draw(starAccent, drawPos, null, new Color(255, 255, 255, 0) * starAlpha * 0.4f,
                    progress * MathHelper.TwoPi + MathHelper.PiOver4, starOrigin,
                    (IsHarmonicRelease ? 0.4f : 0.25f) * (1f + progress), SpriteEffects.None, 0f);
            }

            // Draw particles on top
            ChromaticParticleHandler.DrawAllParticles(sb);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
