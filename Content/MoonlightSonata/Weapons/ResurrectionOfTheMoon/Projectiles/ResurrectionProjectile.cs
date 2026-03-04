using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Utilities;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Primitives;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Buffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Dusts;
using MagnumOpus.Content.MoonlightSonata;
using ReLogic.Content;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Projectiles
{
    /// <summary>
    /// Standard Chamber 窶・Moonlight Comet Round.
    /// Ricochets off tiles up to 10 times with escalating crater detonations.
    /// Each bounce creates a small AoE blast and spawns VFX.
    /// Smart homing begins after bounce 7.
    /// Trail intensity escalates with each ricochet using CometTrail shader.
    /// </summary>
    public class ResurrectionProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1";

        // =================================================================
        // CONSTANTS
        // =================================================================

        public const int MaxBounces = 10;
        public const int HomingStartBounce = 7;
        public const float HomingRange = 1200f;
        public const float HomingStrength = 0.04f;
        public const float BulletWidth = 20f;
        public const int TrailLength = 25;
        public const float CraterDamageMultiplier = 0.35f;

        // =================================================================
        // AI FIELDS
        // =================================================================

        /// <summary>ai[0] = bounce count.</summary>
        public ref float BounceCount => ref Projectile.ai[0];

        /// <summary>ai[1] = unused.</summary>
        public ref float Reserved => ref Projectile.ai[1];

        /// <summary>localAI[0] = alive time.</summary>
        public ref float AliveTime => ref Projectile.localAI[0];

        /// <summary>localAI[1] = comet phase (0=cold, 1=white-hot).</summary>
        public ref float CometPhase => ref Projectile.localAI[1];

        // =================================================================
        // SETUP
        // =================================================================

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = MaxBounces + 2; // bounces + final kill + buffer
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 0.6f;
        }

        // =================================================================
        // AI
        // =================================================================

        public override void AI()
        {
            AliveTime++;
            CometPhase = CometUtils.GetCometPhase((int)BounceCount, MaxBounces);

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Smart homing after bounce threshold
            if ((int)BounceCount >= HomingStartBounce)
            {
                int target = CometUtils.ClosestNPCAt(Projectile.Center, HomingRange);
                if (target != -1)
                {
                    Vector2 toTarget = Main.npc[target].Center - Projectile.Center;
                    toTarget.Normalize();
                    float strength = HomingStrength + (BounceCount - HomingStartBounce) * 0.01f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), strength);
                }
            }

            // Lighting
            float intensity = 0.3f + CometPhase * 0.5f;
            Color lightCol = CometUtils.GetCometGradient(CometPhase);
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * intensity);

            // Flight particles
            SpawnFlightParticles();
        }

        private void SpawnFlightParticles()
        {
            if (Main.dedServ) return;

            // Ember trail 窶・every tick
            if (AliveTime % 1 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(4f, 4f);
                Vector2 emberVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f)
                    + Main.rand.NextVector2Circular(1f, 1f);
                Color emberColor = CometUtils.GetCometGradient(CometPhase * 0.5f + Main.rand.NextFloat(0.3f));
                CometParticleHandler.Spawn(new EmberTrailParticle(
                    Projectile.Center + offset, emberVel,
                    0.3f + CometPhase * 0.3f, 15 + Main.rand.Next(10)));
            }

            // Dust trail 窶・every 2 ticks
            if (AliveTime % 2 == 0)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<CometDust>(), -Projectile.velocity.X * 0.1f, -Projectile.velocity.Y * 0.1f);
                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 0.8f + CometPhase * 0.5f;
            }

            // Head mist 窶・every 4 ticks
            if (AliveTime % 4 == 0)
            {
                CometParticleHandler.Spawn(new CometMistParticle(
                    Projectile.Center, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    CometUtils.GetCometGradient(CometPhase) * 0.5f,
                    0.4f + CometPhase * 0.3f, 20));
            }
        }

        // =================================================================
        // RICOCHET
        // =================================================================

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            BounceCount++;

            if ((int)BounceCount > MaxBounces)
                return true; // kill projectile

            // Mirror reflection
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > 0.01f)
                Projectile.velocity.X = -oldVelocity.X;
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > 0.01f)
                Projectile.velocity.Y = -oldVelocity.Y;

            // Speed boost on bounce
            Projectile.velocity *= 1.04f;

            // Impact sound
            SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.5f + CometPhase * 0.3f, Pitch = -0.2f + CometPhase * 0.3f },
                Projectile.Center);

            // Crater detonation VFX
            SpawnCraterVFX();

            // Crater AoE damage
            SpawnCraterDamage();

            // Add charge to player
            if (Projectile.owner == Main.myPlayer)
                Main.LocalPlayer.Resurrection().AddCharge();

            return false;
        }

        private void SpawnCraterVFX()
        {
            if (Main.dedServ) return;

            float phase = CometPhase;
            Color craterColor = CometUtils.GetCometGradient(phase);

            // Crater bloom 窶・size escalates with bounces
            float bloomScale = 1.0f + phase * 1.5f;
            CometParticleHandler.Spawn(new CraterBloomParticle(
                Projectile.Center, craterColor, bloomScale, 20 + (int)(phase * 15)));

            // Radial ember burst
            int emberCount = 6 + (int)(phase * 10);
            for (int i = 0; i < emberCount; i++)
            {
                float angle = MathHelper.TwoPi * i / emberCount + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * (3f + phase * 5f + Main.rand.NextFloat(2f));
                CometParticleHandler.Spawn(new EmberTrailParticle(
                    Projectile.Center, vel, 0.4f + phase * 0.4f, 20 + Main.rand.Next(15)));
            }

            // Lunar shards
            int shardCount = 3 + (int)(phase * 5);
            for (int i = 0; i < shardCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f) + Main.rand.NextVector2Circular(1f, 1f);
                CometParticleHandler.Spawn(new LunarShardParticle(
                    Projectile.Center, vel, CometUtils.GetCometGradient(Main.rand.NextFloat()),
                    0.4f + Main.rand.NextFloat(0.3f), 25 + Main.rand.Next(15)));
            }

            // Dust burst
            for (int i = 0; i < 8 + (int)(phase * 8); i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                int d = Dust.NewDust(Projectile.Center - new Vector2(4), 8, 8,
                    ModContent.DustType<CometDust>(), dustVel.X, dustVel.Y);
                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 1.0f + phase * 0.8f;
            }

            // Hue-shifting music notes 窶・the gun's ricochets ring out like a timpani finale
            int noteCount = 2 + (int)(phase * 4);
            MoonlightVFXLibrary.SpawnMusicNotes(Projectile.Center, count: noteCount,
                spread: 20f + phase * 25f, minScale: 0.5f, maxScale: 1.0f,
                lifetime: 40 + (int)(phase * 20));
        }

        private void SpawnCraterDamage()
        {
            if (Projectile.owner != Main.myPlayer) return;

            // Expanding hitbox check for crater AoE
            float radius = 48f + CometPhase * 64f;
            int craterDamage = (int)(Projectile.damage * CraterDamageMultiplier * (1f + CometPhase * 0.5f));

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy(Projectile)) continue;
                if (Vector2.Distance(npc.Center, Projectile.Center) > radius) continue;

                // Apply damage
                Player owner = Main.player[Projectile.owner];
                NPC.HitInfo hitInfo = npc.CalculateHitInfo(craterDamage, Projectile.velocity.X > 0 ? 1 : -1,
                    false, Projectile.knockBack * 0.5f, Projectile.DamageType);
                npc.StrikeNPC(hitInfo, false, false);

                // Apply Lunar Impact debuff
                npc.AddBuff(ModContent.BuffType<LunarImpact>(), 240);
                var impactNpc = npc.GetGlobalNPC<LunarImpactNPC>();
                impactNpc.AddStack();
            }
        }

        // =================================================================
        // ON HIT
        // =================================================================

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Lunar Impact debuff
            target.AddBuff(ModContent.BuffType<LunarImpact>(), 300);
            var impactNpc = target.GetGlobalNPC<LunarImpactNPC>();
            impactNpc.AddStack();

            // Hit VFX
            if (!Main.dedServ)
            {
                // Impact bloom
                CometParticleHandler.Spawn(new CraterBloomParticle(
                    target.Center, CometUtils.ImpactFlash, 0.8f + CometPhase * 0.5f, 15));

                // Radial spark burst
                for (int i = 0; i < 6; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f);
                    CometParticleHandler.Spawn(new EmberTrailParticle(
                        target.Center, vel, 0.3f + CometPhase * 0.2f, 15 + Main.rand.Next(8)));
                }
            }
        }

        // =================================================================
        // RENDERING
        // =================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            // End the active SpriteBatch before GPU primitive drawing
            Main.spriteBatch.End();
            try
            {
                // Pass 1: Glow underlayer trail
                DrawGlowTrail();

                // Pass 2: Main comet body trail
                DrawMainTrail();
            }
            finally
            {
                // Restore SpriteBatch to Terraria's expected state
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            // Pass 3: Head glow orb (uses SpriteBatch)
            DrawHeadGlow();

            return false;
        }

        private void DrawGlowTrail()
        {
            MiscShaderData glowShader = null;
            if (GameShaders.Misc.TryGetValue("MagnumOpus:CometTrailGlow", out var shader))
            {
                glowShader = shader;
                glowShader.UseColor(CometUtils.GetCometGradient(CometPhase * 0.7f));
                glowShader.UseSecondaryColor(CometUtils.DeepSpaceViolet);
                glowShader.UseOpacity(0.4f + CometPhase * 0.3f);
                glowShader.UseSaturation(CometPhase); // uPhase for the shader

                // Nebula wisp noise 窶・gaseous comet wake texture for each ricochet
                glowShader.UseImage1(ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NebulaWispNoise"));
            }

            CometTrailRenderer.RenderTrail(Projectile.oldPos, new CometTrailSettings(
                widthFunction: (completion, _) =>
                {
                    float taper = 1f - completion;
                    return (BulletWidth * 2.5f + CometPhase * 20f) * taper * CometUtils.SineOut(1f - completion);
                },
                colorFunction: (completion, _) =>
                {
                    Color col = CometUtils.GetCometGradient(completion * 0.6f + CometPhase * 0.3f);
                    return col * (0.3f + CometPhase * 0.2f) * (1f - completion * 0.5f);
                },
                offsetFunction: (_, _) => Projectile.Size * 0.5f,
                smoothen: true,
                shader: glowShader
            ), TrailLength);
        }

        private void DrawMainTrail()
        {
            MiscShaderData mainShader = null;
            if (GameShaders.Misc.TryGetValue("MagnumOpus:CometTrailMain", out var shader))
            {
                mainShader = shader;
                mainShader.UseColor(CometUtils.CometCoreWhite);
                mainShader.UseSecondaryColor(CometUtils.CometTrail);
                mainShader.UseOpacity(0.7f + CometPhase * 0.3f);
                mainShader.UseSaturation(CometPhase);

                // Nebula wisp noise 窶・inner comet body distortion
                mainShader.UseImage1(ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NebulaWispNoise"));
            }

            CometTrailRenderer.RenderTrail(Projectile.oldPos, new CometTrailSettings(
                widthFunction: (completion, _) =>
                {
                    float taper = 1f - completion;
                    return (BulletWidth + CometPhase * 8f) * taper * CometUtils.SineOut(1f - completion);
                },
                colorFunction: (completion, _) =>
                {
                    Color col = CometUtils.GetCometGradient(completion + CometPhase * 0.2f);
                    return col * (0.8f + CometPhase * 0.2f) * (1f - completion * 0.3f);
                },
                offsetFunction: (_, _) => Projectile.Size * 0.5f,
                smoothen: true,
                shader: mainShader
            ), TrailLength);
        }

        /// <summary>Multi-layered comet head glow with directional stretch along velocity.
        /// Uses WideSoftEllipse for comet-shaped silhouette + SoftRadialBloom for atmospheric halo.</summary>
        private void DrawHeadGlow()
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D bloom = CometTextures.SoftRadialBloom;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float velRotation = Projectile.velocity.ToRotation();

            // Layer 1: Directional comet ellipse — stretched along velocity for comet shape
            var ellipseTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/WideSoftEllipse", AssetRequestMode.ImmediateLoad).Value;

            // Switch to Additive for bloom rendering
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Color ellipseColor = CometUtils.GetCometGradient(CometPhase * 0.8f) with { A = 0 };
            float ellipseScale = 0.3f + CometPhase * 0.25f;
            sb.Draw(ellipseTex, drawPos, null, ellipseColor * (0.3f + CometPhase * 0.2f),
                velRotation, ellipseTex.Size() * 0.5f,
                new Vector2(ellipseScale * 1.8f, ellipseScale), SpriteEffects.None, 0f);

            // Layer 2: Wide atmospheric bloom — soft outer halo (scaled down some)
            Color outerColor = CometUtils.GetCometGradient(CometPhase) with { A = 0 };
            float outerScale = 0.45f + CometPhase * 0.3f;
            sb.Draw(bloom, drawPos, null, outerColor * (0.35f + CometPhase * 0.25f),
                0f, bloom.Size() * 0.5f, outerScale, SpriteEffects.None, 0f);

            // Layer 3: Mid glow — comet coma
            Color midColor = CometUtils.GetCometGradient(CometPhase * 0.5f) with { A = 0 };
            sb.Draw(bloom, drawPos, null, midColor * 0.25f,
                0f, bloom.Size() * 0.5f, outerScale * 0.6f, SpriteEffects.None, 0f);

            // Layer 4: Bright inner core
            Color coreColor = CometUtils.FrigidImpact with { A = 0 };
            float coreScale = 0.25f + CometPhase * 0.15f;
            sb.Draw(bloom, drawPos, null, coreColor * (0.5f + CometPhase * 0.35f),
                0f, bloom.Size() * 0.5f, coreScale, SpriteEffects.None, 0f);

            // Restore to AlphaBlend
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        // =================================================================
        // DEATH VFX
        // =================================================================

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Final death burst
            CometParticleHandler.Spawn(new CraterBloomParticle(
                Projectile.Center, CometUtils.FrigidImpact, 1.5f + CometPhase, 25));

            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(5f, 5f);
                CometParticleHandler.Spawn(new EmberTrailParticle(
                    Projectile.Center, vel, 0.4f, 20 + Main.rand.Next(10)));
            }

            SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.4f, Pitch = 0.3f }, Projectile.Center);
        }
    }
}
