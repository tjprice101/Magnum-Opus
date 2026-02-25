using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.TestWeapons.SandboxWeapons.Shaders;

namespace MagnumOpus.Content.TestWeapons.SandboxWeapons
{
    /// <summary>
    /// Cosmic homing shard flung from the spinning Terra Blade.
    /// Rendered as HIGHLY stretched/squished flare textures with sparkle overlays.
    /// Brief outward flight with deceleration, then homes toward nearest enemy.
    /// Fluid water-like wobble movement for organic, non-rigid motion.
    /// </summary>
    public class CosmicSpinShard : ModProjectile
    {
        #region Constants

        private const int TrailCacheSize = 24;
        private const int LaunchFrames = 15;
        private const float HomingRange = 600f;
        private const float HomingLerp = 0.06f;
        private const float HomingSpeed = 14f;
        private const float Deceleration = 0.96f;

        // Stretched flare rendering
        private const float FlareStretchX = 7f;
        private const float FlareSquishY = 0.14f;

        // Fluid wobble
        private const float WobbleFreq1 = 3.8f;
        private const float WobbleFreq2 = 6.1f;
        private const float WobbleAmp1 = 2.0f;
        private const float WobbleAmp2 = 1.2f;

        #endregion

        #region State

        private int cachedTargetWhoAmI = -1;
        private int timer = 0;
        private float wobblePhase;

        #endregion

        #region Setup

        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailCacheSize;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.alpha = 255;

            wobblePhase = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        #endregion

        #region AI

        public override void AI()
        {
            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (timer <= LaunchFrames)
            {
                Projectile.velocity *= Deceleration;
            }
            else
            {
                if (cachedTargetWhoAmI < 0)
                    AcquireTarget();

                if (cachedTargetWhoAmI >= 0 && cachedTargetWhoAmI < Main.maxNPCs)
                {
                    NPC target = Main.npc[cachedTargetWhoAmI];
                    if (target.active && !target.friendly && !target.dontTakeDamage)
                    {
                        Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * HomingSpeed, HomingLerp);
                    }
                    else
                    {
                        cachedTargetWhoAmI = -1;
                    }
                }
                else
                {
                    Projectile.velocity *= 0.99f;
                }
            }

            // Fluid wobble — organic water-like motion
            float t = timer * 0.1f + wobblePhase;
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 perp = new Vector2(-dir.Y, dir.X);
            float wobble = MathF.Sin(t * WobbleFreq1) * WobbleAmp1
                         + MathF.Sin(t * WobbleFreq2 + 1.9f) * WobbleAmp2;
            Projectile.position += perp * wobble * 0.25f;

            // Sparkle dust trail
            if (Main.rand.NextBool(2))
            {
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.GreenTorch,
                    -Projectile.velocity * 0.15f,
                    0, dustColor, 1.0f);
                d.noGravity = true;
            }

            // Sparkle particles
            if (timer % 4 == 0)
            {
                Color sparkleColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.4f, 0.8f));
                var sparkle = new SparkleParticle(
                    Projectile.Center, Main.rand.NextVector2Circular(1f, 1f),
                    sparkleColor, sparkleColor * 0.5f,
                    Main.rand.NextFloat(0.15f, 0.35f), 8);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Dynamic lighting
            Color light = TerraBladeShaderManager.GetPaletteColor(0.5f);
            Lighting.AddLight(Projectile.Center, light.ToVector3() * 0.6f);
        }

        private void AcquireTarget()
        {
            float bestDist = HomingRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    cachedTargetWhoAmI = i;
                }
            }
        }

        #endregion

        #region Hit Effects

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                    ModContent.ProjectileType<CosmicImpactZone>(),
                    Projectile.damage / 2, 0f, Projectile.owner);
            }

            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(target.Center, DustID.GreenTorch, vel, 0, dustColor, 1.4f);
                d.noGravity = true;
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Enchanted_Gold, vel, 0, Color.White, 0.9f);
                d.noGravity = true;
            }

            for (int i = 0; i < 3; i++)
            {
                Vector2 noteVel = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0f, -1.5f);
                Color noteColor = TerraBladeShaderManager.GetPaletteColor(0.3f + Main.rand.NextFloat() * 0.5f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.8f, 35);
            }

            Lighting.AddLight(target.Center, 0.8f, 1.2f, 0.8f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, vel, 0, dustColor, 1.0f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.3f, 0.5f, 0.3f);
        }

        #endregion

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;
            float velRot = Projectile.velocity.ToRotation();
            float pulse = 1f + MathF.Sin(time * 8f) * 0.1f;
            float speed = Projectile.velocity.Length();
            float dynamicStretch = 1f + speed * 0.05f;

            // Cosmic trail via oldPos
            DrawCosmicTrail(sb);

            // Stretched flare body
            Texture2D flare1 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flare2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/FlareSparkle").Value;
            Texture2D flare3 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/ThinSparkleFlare").Value;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Outer glow — wide stretched flare
            {
                Vector2 origin = flare1.Size() * 0.5f;
                Color outerColor = TerraBladeShaderManager.GetPaletteColor(0.3f) with { A = 0 };
                Vector2 stretchScale = new Vector2(FlareStretchX * dynamicStretch, FlareSquishY) * pulse;
                sb.Draw(flare1, drawPos, null, outerColor * 0.28f,
                    velRot, origin, stretchScale, SpriteEffects.None, 0f);
            }

            // Layer 2: Sparkle overlay — FlareSparkle with slight wobble rotation
            {
                Vector2 origin = flare2.Size() * 0.5f;
                Color sparkleColor = TerraBladeShaderManager.GetPaletteColor(0.5f) with { A = 0 };
                Vector2 stretchScale = new Vector2(FlareStretchX * 0.7f * dynamicStretch, FlareSquishY * 1.2f) * pulse;
                sb.Draw(flare2, drawPos, null, sparkleColor * 0.40f,
                    velRot + MathF.Sin(time * 5f) * 0.03f, origin, stretchScale, SpriteEffects.None, 0f);
            }

            // Layer 3: Thin sparkle core
            {
                Vector2 origin = flare3.Size() * 0.5f;
                Color coreColor = TerraBladeShaderManager.GetPaletteColor(0.7f) with { A = 0 };
                Vector2 stretchScale = new Vector2(FlareStretchX * 0.45f * dynamicStretch, FlareSquishY * 0.5f) * pulse;
                sb.Draw(flare3, drawPos, null, coreColor * 0.50f,
                    velRot, origin, stretchScale, SpriteEffects.None, 0f);
            }

            // Layer 4: White-hot center
            {
                Vector2 origin = flare1.Size() * 0.5f;
                Vector2 stretchScale = new Vector2(FlareStretchX * 0.25f * dynamicStretch, FlareSquishY * 0.3f) * pulse;
                sb.Draw(flare1, drawPos, null, (Color.White with { A = 0 }) * 0.55f,
                    velRot, origin, stretchScale, SpriteEffects.None, 0f);
            }

            // Counter-rotating sparkle overlay
            {
                Vector2 origin = flare2.Size() * 0.5f;
                Color sparkColor = TerraBladeShaderManager.GetPaletteColor(0.6f) with { A = 0 };
                sb.Draw(flare2, drawPos, null, sparkColor * 0.20f,
                    -time * 2.5f + timer * 0.1f, origin, 0.10f * pulse, SpriteEffects.None, 0f);
            }

            // Afterimage stretched flares at old positions
            Vector2 flareOrigin = flare1.Size() * 0.5f;
            for (int i = 1; i < TrailCacheSize; i += 2)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailProgress = (float)i / TrailCacheSize;
                float trailAlpha = (1f - trailProgress) * 0.18f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float trailRot = Projectile.oldRot[i];
                float trailStretch = MathHelper.Lerp(FlareStretchX * 0.4f, FlareStretchX * 0.08f, trailProgress);
                float trailSquish = MathHelper.Lerp(FlareSquishY * 0.6f, FlareSquishY * 0.15f, trailProgress);
                Color trailColor = TerraBladeShaderManager.GetPaletteColor(0.3f + trailProgress * 0.4f) with { A = 0 };
                sb.Draw(flare1, trailPos, null, trailColor * trailAlpha,
                    trailRot, flareOrigin, new Vector2(trailStretch, trailSquish), SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private void DrawCosmicTrail(SpriteBatch sb)
        {
            int validCount = 0;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] != Vector2.Zero)
                    validCount++;
                else
                    break;
            }

            if (validCount < 3) return;

            Vector2[] positions = new Vector2[validCount];
            float[] rotations = new float[validCount];
            for (int i = 0; i < validCount; i++)
            {
                positions[i] = Projectile.oldPos[i] + Projectile.Size * 0.5f;
                rotations[i] = Projectile.oldRot[i];
            }

            Texture2D noise = ShaderLoader.GetNoiseTexture("CosmicNebulaClouds");
            if (noise != null)
            {
                var device = Main.instance.GraphicsDevice;
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }

            CalamityStyleTrailRenderer.DrawTrailWithBloom(
                positions, rotations,
                CalamityStyleTrailRenderer.TrailStyle.Cosmic,
                14f,
                TerraBladeShaderManager.EnergyGreen,
                TerraBladeShaderManager.BrightCyan,
                1f, 2.0f);
        }

        #endregion

        #region Networking

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(cachedTargetWhoAmI);
            writer.Write(timer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            cachedTargetWhoAmI = reader.ReadInt32();
            timer = reader.ReadInt32();
        }

        #endregion
    }
}
