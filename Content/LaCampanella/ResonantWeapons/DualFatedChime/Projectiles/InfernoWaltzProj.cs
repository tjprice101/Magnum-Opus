using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Primitives;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Shaders;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Projectiles
{
    /// <summary>
    /// Inferno Waltz  EAlt-fire AoE projectile. Player spins in a fiery dance,
    /// dealing continuous damage around them, spawning flame waves at intervals,
    /// and cloaking the surroundings in black smoke and golden bell fire.
    /// Requires full Charge Bar (100) to activate. Lasts 2 seconds (120 ticks).
    /// </summary>
    public class InfernoWaltzProj : ModProjectile
    {
        #region Properties

        private const int Duration = 120;
        private const float SpinRate = 0.30f;
        private const int WaveInterval = 15;
        private const int WaveCount = 8;
        private const float WaveDamageMult = 0.5f;

        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private float _spinAngle;
        private int _waveTimer;

        private ref float Timer => ref Projectile.ai[0];

        #endregion

        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/DualFatedChime/DualFatedChime";

        public override void SetDefaults()
        {
            Projectile.width = 300;
            Projectile.height = 300;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _initialized = true;
                _spinAngle = Owner.direction == 1 ? 0f : MathHelper.Pi;
                Timer = 0;
                _waveTimer = 0;
            }

            Timer++;
            if (Timer >= Duration)
            {
                Projectile.Kill();
                return;
            }

            // Lock projectile to player center
            Projectile.Center = Owner.Center;
            Owner.heldProj = Projectile.whoAmI;

            // Spin the player visually
            _spinAngle += SpinRate;
            Owner.fullRotation = _spinAngle;
            Owner.fullRotationOrigin = Owner.Size / 2f;

            // Movement speed boost during waltz
            Owner.velocity = Owner.velocity * 0.96f;

            // Spawn fire waves at intervals
            _waveTimer++;
            if (_waveTimer >= WaveInterval && Projectile.owner == Main.myPlayer)
            {
                _waveTimer = 0;
                SpawnFlameWaves();
            }

            // Ambient fire vfx
            SpawnWaltzParticles();

            // Pulsing infernal light
            float intensity = 1.0f + 0.4f * (float)Math.Sin(Timer * 0.15f);
            Lighting.AddLight(Projectile.Center, new Vector3(0.7f, 0.3f, 0.05f) * intensity);
        }

        private void SpawnFlameWaves()
        {
            int waveDamage = (int)(Projectile.damage * WaveDamageMult);
            float angleStep = MathHelper.TwoPi / WaveCount;

            for (int i = 0; i < WaveCount; i++)
            {
                float angle = _spinAngle + i * angleStep;
                Vector2 velocity = angle.ToRotationVector2() * 10f;

                int proj = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    velocity,
                    ModContent.ProjectileType<BellFlameWaveProj>(),
                    waveDamage,
                    Projectile.knockBack * 0.5f,
                    Projectile.owner
                );

                if (proj >= 0 && proj < Main.maxProjectiles)
                    Main.projectile[proj].timeLeft = 45;
            }
        }

        private void SpawnWaltzParticles()
        {
            float progress = Timer / Duration;

            // Swirling embers
            for (int i = 0; i < 3; i++)
            {
                float angle = _spinAngle + Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Main.rand.NextFloat(40f, 120f);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;
                Vector2 vel = (angle + MathHelper.PiOver2).ToRotationVector2() * Main.rand.NextFloat(1f, 3f);

                float heat = Main.rand.NextFloat(0.3f, 0.9f);
                DualFatedChimeParticleHandler.SpawnParticle(
                    new InfernalEmberParticle(pos, vel, heat, Main.rand.Next(15, 30), 0.4f));
            }

            // Billowing black smoke ring
            if (Main.rand.NextBool(2))
            {
                float smokeAngle = _spinAngle + Main.rand.NextFloat() * MathHelper.TwoPi;
                float smokeRadius = Main.rand.NextFloat(60f, 140f);
                Vector2 smokePos = Projectile.Center + smokeAngle.ToRotationVector2() * smokeRadius;
                Vector2 smokeVel = Vector2.UnitY * -Main.rand.NextFloat(0.5f, 1.5f);

                DualFatedChimeParticleHandler.SpawnParticle(
                    new BellSmokeParticle(smokePos, smokeVel, Main.rand.Next(25, 45), 1.2f, 0.5f));
            }

            // Musical flames orbiting
            if (Main.rand.NextBool(4))
            {
                float noteAngle = _spinAngle * 2f + Main.rand.NextFloat() * MathHelper.TwoPi;
                float noteRadius = Main.rand.NextFloat(80f, 150f);
                Vector2 notePos = Projectile.Center + noteAngle.ToRotationVector2() * noteRadius;
                Vector2 noteVel = Vector2.UnitY * -Main.rand.NextFloat(1f, 2f);

                float noteHeat = Main.rand.NextFloat(0.4f, 0.8f);
                DualFatedChimeParticleHandler.SpawnParticle(
                    new MusicalFlameParticle(notePos, noteVel, Main.rand.Next(25, 45), 0.6f));
            }

            // Periodic bell chime flash at crescendo points
            if (Timer % 30 == 0)
            {
                float flashScale = MathHelper.Lerp(1.5f, 2.5f, progress);
                DualFatedChimeParticleHandler.SpawnParticle(
                    new BellChimeFlashParticle(Projectile.Center, 10, flashScale));
            }

            // Vanilla dust for additional density
            for (int i = 0; i < 2; i++)
            {
                float dustAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dustRadius = Main.rand.NextFloat(20f, 100f);
                Vector2 dustPos = Projectile.Center + dustAngle.ToRotationVector2() * dustRadius;

                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch,
                    (dustAngle + MathHelper.PiOver2).ToRotationVector2() * 2f,
                    0, DualFatedChimeUtils.GetFireFlicker(Main.rand.NextFloat()), 1.2f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 2);

            // Heavy impact VFX
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                float heat = Main.rand.NextFloat(0.5f, 1f);
                DualFatedChimeParticleHandler.SpawnParticle(
                    new InfernalEmberParticle(target.Center, sparkVel, heat, 20, 0.5f));
            }

            DualFatedChimeParticleHandler.SpawnParticle(
                new BellChimeFlashParticle(target.Center, 8, 1.5f));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            DrawWaltzAura(sb);
            DrawWaltzParticles(sb);
            return false;
        }

        private void DrawWaltzAura(SpriteBatch sb)
        {
            Texture2D bloomTex = null;
            try
            {
                bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }

            if (bloomTex == null) return;

            Vector2 screenPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(bloomTex.Width / 2f, bloomTex.Height / 2f);
            float progress = Timer / Duration;
            float pulse = 0.6f + 0.3f * (float)Math.Sin(Timer * 0.12f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer fire ring (large, orange)
            float outerScale = MathHelper.Lerp(3f, 4.5f, progress) * pulse;
            sb.Draw(bloomTex, screenPos, null,
                DualFatedChimeUtils.Additive(new Color(200, 80, 0), 0.15f * pulse),
                _spinAngle, origin, outerScale, SpriteEffects.None, 0f);

            // Mid ring (gold)
            float midScale = MathHelper.Lerp(2f, 3f, progress) * pulse;
            sb.Draw(bloomTex, screenPos, null,
                DualFatedChimeUtils.Additive(new Color(255, 180, 40), 0.2f * pulse),
                -_spinAngle * 0.7f, origin, midScale, SpriteEffects.None, 0f);

            // Core (white-hot)
            float coreScale = MathHelper.Lerp(1f, 1.5f, progress) * pulse;
            sb.Draw(bloomTex, screenPos, null,
                DualFatedChimeUtils.Additive(new Color(255, 240, 200), 0.35f),
                0f, origin, coreScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawWaltzParticles(SpriteBatch sb)
        {
            DualFatedChimeParticleHandler.DrawAllParticles(sb);
        }

        public override void OnKill(int timeLeft)
        {
            // Reset rotation
            if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
            {
                Player p = Main.player[Projectile.owner];
                p.fullRotation = 0f;

                // Apply Waltz buff via player tracker
                var tracker = p.GetModPlayer<DualFatedChimePlayer>();
                tracker.WaltzBuffTimer = 900;
            }

            // Grand finale burst
            for (int i = 0; i < 15; i++)
            {
                float angle = i / 15f * MathHelper.TwoPi;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                float heat = Main.rand.NextFloat(0.5f, 1f);
                DualFatedChimeParticleHandler.SpawnParticle(
                    new InfernalEmberParticle(Projectile.Center, burstVel, heat, 30, 0.5f));
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                DualFatedChimeParticleHandler.SpawnParticle(
                    new BellSmokeParticle(Projectile.Center + smokeVel * 10f, smokeVel * 0.3f, 40, 1.5f, 0.6f));
            }

            DualFatedChimeParticleHandler.SpawnParticle(
                new BellChimeFlashParticle(Projectile.Center, 15, 3f));
        }
    }
}
