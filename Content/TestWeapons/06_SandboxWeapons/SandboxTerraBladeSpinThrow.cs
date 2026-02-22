using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.TestWeapons.SandboxWeapons.Shaders;

namespace MagnumOpus.Content.TestWeapons.SandboxWeapons
{
    /// <summary>
    /// Thrown spinning blade for the Sandbox Terra Blade right-click combo.
    /// 3-phase AI:
    ///   Phase 0 (Travel)  — Flies toward cursor position, spinning.
    ///   Phase 1 (Spin)    — Hovers at cursor, orbits, spawns cosmic shards. Hold to maintain.
    ///   Phase 2 (Return)  — Snaps back to player with cosmic trail, flash on arrival.
    /// </summary>
    public class SandboxTerraBladeSpinThrow : ModProjectile
    {
        #region Constants

        private const int TrailLength = 40;
        private const int MaxTravelFrames = 40;
        private const float ArrivalDistance = 30f;
        private const int MaxSpinFrames = 180; // 3 seconds
        private const int ShardSpawnInterval = 14;
        private const float OrbitRadius = 15f;
        private const float OrbitSpeed = 0.12f;
        private const float ReturnSpeed = 22f;
        private const float ReturnAccel = 0.08f;
        private const float CatchDistance = 40f;

        #endregion

        #region AI Slot Accessors

        /// <summary>Current phase: 0 = Travel, 1 = Spin, 2 = Return.</summary>
        private int Phase
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        /// <summary>Frame counter within the current phase.</summary>
        private float PhaseTimer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        /// <summary>Stored cursor X position for spin phase.</summary>
        private float CursorX
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        /// <summary>Stored cursor Y position for spin phase.</summary>
        private float CursorY
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }

        #endregion

        #region State

        private Player Owner => Main.player[Projectile.owner];
        private Vector2[] tipPositions = new Vector2[TrailLength];
        private float[] tipRotations = new float[TrailLength];
        private int trailIndex = 0;
        private float orbitAngle = 0f;

        #endregion

        #region Setup

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600; // 10 seconds safety
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        #endregion

        #region Collision

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Phase == 1)
            {
                // Circular collision during spin
                Vector2 closestPoint = new Vector2(
                    MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right),
                    MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
                return Vector2.Distance(Projectile.Center, closestPoint) <= 50f;
            }
            else
            {
                // Line collision during travel/return
                float point = 0f;
                Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 60f;
                return Collision.CheckAABBvLineCollision(
                    targetHitbox.TopLeft(), targetHitbox.Size(),
                    Projectile.Center - dir, Projectile.Center + dir,
                    30f, ref point);
            }
        }

        #endregion

        #region AI

        public override void AI()
        {
            if (Owner.dead || !Owner.active)
            {
                Projectile.Kill();
                return;
            }

            // Keep player locked while blade is out
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            PhaseTimer++;

            switch (Phase)
            {
                case 0: AI_Travel(); break;
                case 1: AI_Spin(); break;
                case 2: AI_Return(); break;
            }

            // Trail tracking
            tipPositions[trailIndex % TrailLength] = Projectile.Center;
            tipRotations[trailIndex % TrailLength] = Projectile.rotation;
            trailIndex++;

            // Dynamic lighting
            Color light = TerraBladeShaderManager.GetPaletteColor(0.5f);
            float lightIntensity = Phase == 1 ? 0.8f : 0.5f;
            Lighting.AddLight(Projectile.Center, light.ToVector3() * lightIntensity);
        }

        private void AI_Travel()
        {
            // Spin during travel
            Projectile.rotation += 0.3f;

            // Check arrival
            Vector2 target = Main.myPlayer == Projectile.owner ? Main.MouseWorld : new Vector2(CursorX, CursorY);
            float dist = Vector2.Distance(Projectile.Center, target);

            if (dist < ArrivalDistance || PhaseTimer >= MaxTravelFrames)
            {
                // Transition to spin
                Phase = 1;
                PhaseTimer = 0;
                CursorX = Projectile.Center.X;
                CursorY = Projectile.Center.Y;
                Projectile.velocity = Vector2.Zero;

                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.5f, Volume = 0.7f }, Projectile.Center);
            }

            // Travel dust
            if (Main.rand.NextBool(2))
            {
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch,
                    -Projectile.velocity * 0.2f, 0, dustColor, 1.3f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
        }

        private void AI_Spin()
        {
            // Fast spin — accelerates over time
            float spinAccel = MathHelper.Lerp(0.35f, 0.55f, Math.Min(PhaseTimer / 60f, 1f));
            Projectile.rotation += spinAccel;

            // Track mouse position with slow lerp (only for owner)
            if (Main.myPlayer == Projectile.owner)
            {
                CursorX = MathHelper.Lerp(CursorX, Main.MouseWorld.X, 0.05f);
                CursorY = MathHelper.Lerp(CursorY, Main.MouseWorld.Y, 0.05f);
            }

            // Orbit around stored cursor position
            orbitAngle += OrbitSpeed;
            Vector2 orbitCenter = new Vector2(CursorX, CursorY);
            Vector2 orbitOffset = orbitAngle.ToRotationVector2() * OrbitRadius;
            Projectile.Center = orbitCenter + orbitOffset;
            Projectile.velocity = Vector2.Zero;

            // Face player direction
            Owner.direction = Projectile.Center.X > Owner.Center.X ? 1 : -1;

            // Spawn cosmic shards
            if ((int)PhaseTimer % ShardSpawnInterval == 0 && PhaseTimer > 5)
            {
                SpawnCosmicShard();
            }

            // Dust ring
            if (Main.rand.NextBool(2))
            {
                float dustAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dustPos = orbitCenter + dustAngle.ToRotationVector2() * (OrbitRadius + 10f);
                Vector2 dustVel = dustAngle.ToRotationVector2() * 0.5f;
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(dustPos, DustID.GreenTorch, dustVel, 0, dustColor, 1.0f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Music notes every 20 frames
            if ((int)PhaseTimer % 20 == 0)
            {
                Vector2 noteVel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0f, -1f);
                Color noteColor = TerraBladeShaderManager.GetPaletteColor(0.3f + Main.rand.NextFloat() * 0.5f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.8f, 30);
            }

            // Check end conditions: release mouse OR max duration
            bool shouldReturn = PhaseTimer >= MaxSpinFrames;
            if (Main.myPlayer == Projectile.owner && !Owner.channel)
                shouldReturn = true;

            if (shouldReturn)
            {
                Phase = 2;
                PhaseTimer = 0;
                trailIndex = 0; // Reset trail for return phase

                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = -0.3f, Volume = 0.8f }, Projectile.Center);
            }
        }

        private void AI_Return()
        {
            // Accelerate toward player
            Vector2 toPlayer = Owner.MountedCenter - Projectile.Center;
            float dist = toPlayer.Length();

            if (dist > 0.1f)
            {
                Vector2 targetVel = toPlayer.SafeNormalize(Vector2.UnitX) * ReturnSpeed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVel, ReturnAccel);
            }

            // Spin during return
            Projectile.rotation += 0.4f;

            // Return dust — denser than travel
            for (int i = 0; i < 2; i++)
            {
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.4f, 0.8f));
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.GreenTorch,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f),
                    0, dustColor, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Catch — arrived at player
            if (dist < CatchDistance)
            {
                OnCatch();
                Projectile.Kill();
            }
        }

        private void SpawnCosmicShard()
        {
            if (Main.myPlayer != Projectile.owner) return;

            // Tangential velocity from spin
            Vector2 tangent = new Vector2(-MathF.Sin(orbitAngle), MathF.Cos(orbitAngle));
            // Add some randomness
            tangent = tangent.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f));
            Vector2 shardVel = tangent * 12f;

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), Projectile.Center, shardVel,
                ModContent.ProjectileType<CosmicSpinShard>(),
                Projectile.damage / 2, Projectile.knockBack * 0.5f, Projectile.owner);
        }

        private void OnCatch()
        {
            // Flash VFX at player position
            Vector2 flashPos = Owner.MountedCenter;

            // Radial dust burst
            for (int i = 0; i < 16; i++)
            {
                float angle = i / 16f * MathHelper.TwoPi;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(flashPos, DustID.GreenTorch, vel, 0, dustColor, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Gold sparks
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustPerfect(flashPos, DustID.Enchanted_Gold, vel, 0, Color.White, 1.0f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Music notes
            for (int i = 0; i < 4; i++)
            {
                Vector2 noteVel = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0f, -2f);
                Color noteColor = TerraBladeShaderManager.GetPaletteColor(0.3f + Main.rand.NextFloat() * 0.5f);
                ThemedParticles.MusicNote(flashPos, noteVel, noteColor, 0.9f, 40);
            }

            // Bright lighting spike
            Lighting.AddLight(flashPos, 1.0f, 1.5f, 1.0f);

            // Catch sound
            SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.3f, Volume = 0.9f }, flashPos);
        }

        #endregion

        #region Trail Helpers

        private Vector2[] BuildTrailPositions()
        {
            int count = Math.Min(trailIndex, TrailLength);
            Vector2[] ordered = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                int idx = ((trailIndex - 1 - i) % TrailLength + TrailLength) % TrailLength;
                ordered[i] = tipPositions[idx];
            }
            return ordered;
        }

        private float[] BuildTrailRotations()
        {
            int count = Math.Min(trailIndex, TrailLength);
            float[] ordered = new float[count];
            for (int i = 0; i < count; i++)
            {
                int idx = ((trailIndex - 1 - i) % TrailLength + TrailLength) % TrailLength;
                ordered[i] = tipRotations[idx];
            }
            return ordered;
        }

        #endregion

        #region Rendering

        private static Texture2D SafeRequest(string path)
        {
            try
            {
                if (ModContent.HasAsset(path))
                    return ModContent.Request<Texture2D>(path).Value;
            }
            catch { }
            return null;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D bladeTex = Terraria.GameContent.TextureAssets.Item[ItemID.TerraBlade].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = bladeTex.Size() * 0.5f;
            float scale = 1f;
            float time = Main.GlobalTimeWrappedHourly;

            // 1. Trail
            DrawTrail(sb);

            // 2. Motion blur
            DrawMotionBlur(sb, bladeTex, drawPos);

            // 3. Blade sprite
            sb.Draw(bladeTex, drawPos, null, lightColor, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);

            // 4. Shimmer overlay during spin phase
            if (Phase == 1)
                DrawShimmerOverlay(sb, bladeTex, drawPos, origin, scale, time);

            // 5. Bloom layers
            DrawBloomLayers(sb, bladeTex, drawPos, origin, scale, time);

            // 6. Tip flare
            DrawTipFlare(sb, drawPos, time);

            // 7. Orbit ring glow during spin
            if (Phase == 1)
                DrawOrbitRing(sb, time);

            return false;
        }

        private void DrawTrail(SpriteBatch sb)
        {
            if (trailIndex < 3) return;

            Vector2[] positions = BuildTrailPositions();
            float[] rotations = BuildTrailRotations();

            // Use Cosmic trail during spin/return, Nature during travel
            var style = Phase >= 1
                ? CalamityStyleTrailRenderer.TrailStyle.Cosmic
                : CalamityStyleTrailRenderer.TrailStyle.Nature;

            // Bind appropriate noise texture
            Texture2D noise = Phase >= 1
                ? ShaderLoader.GetNoiseTexture("CosmicNebulaClouds")
                : ShaderLoader.GetNoiseTexture("TileableFBMNoise");
            if (noise != null)
            {
                var device = Main.instance.GraphicsDevice;
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }

            CalamityStyleTrailRenderer.DrawTrailWithBloom(
                positions, rotations, style,
                18f,
                TerraBladeShaderManager.EnergyGreen,
                TerraBladeShaderManager.BrightCyan,
                1f, 2.0f);
        }

        private void DrawMotionBlur(SpriteBatch sb, Texture2D bladeTex, Vector2 drawPos)
        {
            if (Projectile.velocity.LengthSquared() < 1f && Phase != 1) return;

            if (Phase == 1)
            {
                // Radial blur during spin
                MotionBlurBloomRenderer.DrawExplosion(
                    sb, bladeTex, drawPos,
                    TerraBladeShaderManager.GetPaletteColor(0.5f),
                    TerraBladeShaderManager.GetPaletteColor(0.8f),
                    1f, 0.015f, 0.4f);
            }
            else
            {
                // Directional blur during travel/return
                MotionBlurBloomRenderer.DrawProjectile(
                    sb, bladeTex, Projectile,
                    TerraBladeShaderManager.GetPaletteColor(0.5f),
                    TerraBladeShaderManager.GetPaletteColor(0.8f),
                    Phase == 2 ? 1.2f : 0.8f);
            }
        }

        private void DrawShimmerOverlay(SpriteBatch sb, Texture2D bladeTex, Vector2 drawPos,
            Vector2 origin, float scale, float time)
        {
            Effect shader = TerraBladeShaderManager.GetShader();
            if (shader != null && TerraBladeShaderManager.IsAvailable)
            {
                TerraBladeShaderManager.BindShimmerTexture(Main.instance.GraphicsDevice);
                TerraBladeShaderManager.BeginShaderAdditive(sb);

                float spinProgress = Math.Min(PhaseTimer / (float)MaxSpinFrames, 1f);
                TerraBladeShaderManager.ApplyShimmerOverlay(
                    shader, spinProgress, 1f, 1f, time,
                    intensity: 2.0f, overbright: 3.5f);

                Color shimmerColor = (Color.White with { A = 0 }) * 0.6f;
                sb.Draw(bladeTex, drawPos, null, shimmerColor,
                    Projectile.rotation, origin, scale, SpriteEffects.None, 0f);

                TerraBladeShaderManager.RestoreSpriteBatch(sb);
            }
            else
            {
                // Manual fallback
                TerraBladeShaderManager.BeginAdditive(sb);

                float pulse = 0.8f + MathF.Sin(time * 8f) * 0.2f;
                Color outerColor = TerraBladeShaderManager.GetPaletteColor((time * 0.3f) % 1f) with { A = 0 };
                sb.Draw(bladeTex, drawPos, null, outerColor * 0.25f * pulse,
                    Projectile.rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);

                Color midColor = TerraBladeShaderManager.GetPaletteColor((time * 0.3f + 0.33f) % 1f) with { A = 0 };
                sb.Draw(bladeTex, drawPos, null, midColor * 0.35f * pulse,
                    Projectile.rotation, origin, scale * 1.005f, SpriteEffects.None, 0f);

                TerraBladeShaderManager.RestoreSpriteBatch(sb);
            }
        }

        private void DrawBloomLayers(SpriteBatch sb, Texture2D bladeTex, Vector2 drawPos,
            Vector2 origin, float scale, float time)
        {
            float pulse = 1f + MathF.Sin(time * 6f) * 0.08f;

            TerraBladeShaderManager.BeginAdditive(sb);

            // Calamity 4-layer bloom stack with { A = 0 }
            Color outerGlow = TerraBladeShaderManager.GetPaletteColor(0.3f) with { A = 0 };
            sb.Draw(bladeTex, drawPos, null, outerGlow * 0.30f,
                Projectile.rotation, origin, scale * 1.08f * pulse, SpriteEffects.None, 0f);

            Color midGlow = TerraBladeShaderManager.GetPaletteColor(0.5f) with { A = 0 };
            sb.Draw(bladeTex, drawPos, null, midGlow * 0.50f,
                Projectile.rotation, origin, scale * 1.04f * pulse, SpriteEffects.None, 0f);

            Color innerGlow = TerraBladeShaderManager.GetPaletteColor(0.7f) with { A = 0 };
            sb.Draw(bladeTex, drawPos, null, innerGlow * 0.70f,
                Projectile.rotation, origin, scale * 1.01f * pulse, SpriteEffects.None, 0f);

            Color coreGlow = Color.White with { A = 0 };
            sb.Draw(bladeTex, drawPos, null, coreGlow * 0.85f,
                Projectile.rotation, origin, scale * 0.97f, SpriteEffects.None, 0f);

            TerraBladeShaderManager.RestoreSpriteBatch(sb);
        }

        private void DrawTipFlare(SpriteBatch sb, Vector2 drawPos, float time)
        {
            float pulse = 1f + MathF.Sin(time * 10f) * 0.15f;
            Color flareColor = TerraBladeShaderManager.GetPaletteColor(0.6f) with { A = 0 };

            Texture2D starTex = SafeRequest("MagnumOpus/Assets/VFX/Impacts/4-Point Star Impact Burst");

            TerraBladeShaderManager.BeginAdditive(sb);

            if (starTex != null)
            {
                Vector2 starOrigin = starTex.Size() * 0.5f;
                float starScale = 0.12f * pulse;
                sb.Draw(starTex, drawPos, null, flareColor * 0.6f,
                    time * 2f, starOrigin, starScale, SpriteEffects.None, 0f);
            }
            else
            {
                Texture2D flareTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
                Vector2 flareOrigin = flareTex.Size() * 0.5f;
                sb.Draw(flareTex, drawPos, null, flareColor * 0.5f,
                    0f, flareOrigin, 0.12f * pulse, SpriteEffects.None, 0f);
            }

            TerraBladeShaderManager.RestoreSpriteBatch(sb);
        }

        private void DrawOrbitRing(SpriteBatch sb, float time)
        {
            Texture2D bloomTex = SafeRequest("MagnumOpus/Assets/VFX/Blooms/Perfect Soft Color Bloom");
            if (bloomTex == null) return;

            Vector2 orbitCenter = new Vector2(CursorX, CursorY) - Main.screenPosition;
            Vector2 bloomOrigin = bloomTex.Size() * 0.5f;

            float pulse = 1f + MathF.Sin(time * 5f) * 0.1f;
            float ringScale = (OrbitRadius * 2f + 20f) / Math.Max(bloomTex.Width, bloomTex.Height) * pulse;

            TerraBladeShaderManager.BeginAdditive(sb);

            Color ringColor = TerraBladeShaderManager.GetPaletteColor(0.4f) with { A = 0 } * 0.15f;
            sb.Draw(bloomTex, orbitCenter, null, ringColor,
                time * 0.5f, bloomOrigin, ringScale, SpriteEffects.None, 0f);

            TerraBladeShaderManager.RestoreSpriteBatch(sb);
        }

        #endregion

        #region Hit Effects

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(target.Center, DustID.GreenTorch, vel, 0, dustColor, 1.3f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            Lighting.AddLight(target.Center, 0.5f, 1f, 0.6f);
        }

        #endregion

        #region Networking

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Phase);
            writer.Write(PhaseTimer);
            writer.Write(CursorX);
            writer.Write(CursorY);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Phase = reader.ReadInt32();
            PhaseTimer = reader.ReadSingle();
            CursorX = reader.ReadSingle();
            CursorY = reader.ReadSingle();
        }

        #endregion
    }
}
