using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.TestWeapons.SandboxWeapons.Shaders;

namespace MagnumOpus.Content.TestWeapons.SandboxWeapons
{
    /// <summary>
    /// Cosmic homing shard flung from the spinning Terra Blade.
    /// Brief outward flight with deceleration, then homes toward nearest enemy.
    /// On hit: spawns CosmicImpactZone. On miss: fizzles out with particles.
    /// Rendered with Cosmic trail, motion blur, bloom stack, and sparkle.
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

        #endregion

        #region State

        private int cachedTargetWhoAmI = -1;
        private int timer = 0;

        #endregion

        #region Setup

        public override string Texture => "Terraria/Images/Extra_" + 98;

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
        }

        #endregion

        #region AI

        public override void AI()
        {
            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (timer <= LaunchFrames)
            {
                // Phase 1: Outward flight with deceleration
                Projectile.velocity *= Deceleration;
            }
            else
            {
                // Phase 2: Home toward nearest enemy
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
                        // Target died, reacquire
                        cachedTargetWhoAmI = -1;
                    }
                }
                else
                {
                    // No target: slow deceleration, eventually dies at timeLeft = 0
                    Projectile.velocity *= 0.99f;
                }
            }

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
            // Spawn CosmicImpactZone at target center
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                    ModContent.ProjectileType<CosmicImpactZone>(),
                    Projectile.damage / 2, 0f, Projectile.owner);
            }

            // Dust burst
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

            // Music notes
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
            // Fizzle-out particles
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
            Texture2D coreTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = coreTex.Size() * 0.5f;
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 1f + MathF.Sin(time * 8f) * 0.1f;

            // 1. Cosmic trail via oldPos
            DrawCosmicTrail(sb);

            // 2. Motion blur
            MotionBlurBloomRenderer.DrawProjectile(
                sb, coreTex, Projectile,
                TerraBladeShaderManager.GetPaletteColor(0.5f),
                TerraBladeShaderManager.GetPaletteColor(0.8f),
                0.8f);

            // 3-5. Core sprite + bloom + sparkle in one additive batch
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Bloom stack
            Color outerColor = TerraBladeShaderManager.GetPaletteColor(0.3f);
            sb.Draw(coreTex, drawPos, null, outerColor * 0.30f, 0f, origin, 0.8f * pulse, SpriteEffects.None, 0f);

            Color midColor = TerraBladeShaderManager.GetPaletteColor(0.5f);
            sb.Draw(coreTex, drawPos, null, midColor * 0.50f, 0f, origin, 0.5f * pulse, SpriteEffects.None, 0f);

            Color coreColor = TerraBladeShaderManager.GetPaletteColor(0.7f);
            sb.Draw(coreTex, drawPos, null, coreColor * 0.70f, 0f, origin, 0.3f * pulse, SpriteEffects.None, 0f);

            // White-hot core
            sb.Draw(coreTex, drawPos, null, Color.White * 0.85f,
                0f, origin, 0.15f * pulse, SpriteEffects.None, 0f);

            // FlareSparkle counter-rotating at center
            Texture2D sparkleTex = SafeRequest("MagnumOpus/Assets/Particles/FlareSparkle");
            if (sparkleTex != null)
            {
                Vector2 sparkleOrigin = sparkleTex.Size() * 0.5f;
                Color sparkleColor = TerraBladeShaderManager.GetPaletteColor(0.6f);
                sb.Draw(sparkleTex, drawPos, null, sparkleColor * 0.5f,
                    -time * 3f, sparkleOrigin, 0.12f * pulse, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private void DrawCosmicTrail(SpriteBatch sb)
        {
            // Use oldPos for trail (tModLoader manages this via TrailingMode)
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

            // Bind cosmic noise
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
