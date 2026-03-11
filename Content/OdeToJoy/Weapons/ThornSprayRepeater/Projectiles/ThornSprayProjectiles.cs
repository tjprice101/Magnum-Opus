using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Projectiles
{
    /// <summary>
    /// CrystallineThornProjectile — rapid-fire thorn with 5-layer sparkle rendering.
    /// SparkleProjectileFoundation pattern: ring buffer trail + core + inner glow + shimmer + ambient sparkles.
    /// On hit: applies Thorn Accumulation stacks. At 25 stacks: spawns ThornDetonationProjectile.
    /// </summary>
    public class CrystallineThornProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int TrailLength = 24;
        private Vector2[] _trailPositions = new Vector2[TrailLength];
        private float[] _trailRotations = new float[TrailLength];
        private int _trailHead;
        private VertexStrip _vertexStrip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Record trail
            _trailPositions[_trailHead] = Projectile.Center;
            _trailRotations[_trailHead] = Projectile.rotation;
            _trailHead = (_trailHead + 1) % TrailLength;

            // Ambient sparkle dust — crystalline thorn chips
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<CrystallineThornSparkDust>(), 0f, 0f, 150, default, 0.5f);
                d.noGravity = true;
                d.velocity *= 0.3f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            OdeToJoyVFXLibrary.SpawnGardenSparkleExplosion(target.Center, 3, 3f, 0.5f);

            var accumNPC = target.GetGlobalNPC<ThornAccumulationNPC>();
            bool detonate = accumNPC.AddStack(target);

            if (detonate)
            {
                // Spawn detonation burst
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(Projectile.GetSource_OnHit(target), target.Center, Vector2.Zero,
                        ModContent.ProjectileType<ThornDetonationProjectile>(), Projectile.damage * 3, 8f, Projectile.owner);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _vertexStrip);

                // Crystalline thorn: verdant directional streak
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null)
                {
                    Vector2 origin = glow.Size() / 2f;
                    float rot = Projectile.velocity.ToRotation();

                    sb.Draw(glow, drawPos, null,
                        (OdeToJoyPalette.LeafGreen with { A = 0 }) * 0.2f,
                        rot, origin, new Vector2(0.07f, 0.025f), SpriteEffects.None, 0f);
                }

                sb.End();
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

    /// <summary>
    /// BloomThornProjectile — enhanced post-reload thorn with golden palette.
    /// 50% more damage, brighter shimmer, golden core. Same accumulation stacking.
    /// </summary>
    public class BloomThornProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int TrailLength = 24;
        private Vector2[] _trailPositions = new Vector2[TrailLength];
        private int _trailHead;
        private VertexStrip _vertexStrip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            _trailPositions[_trailHead] = Projectile.Center;
            _trailHead = (_trailHead + 1) % TrailLength;

            // Golden sparkle dust (more prominent than standard) — bloom variant
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<ThornBloomBurstDust>(), 0f, 0f, 100, default, 0.8f);
                d.noGravity = true;
                d.velocity *= 0.4f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            OdeToJoyVFXLibrary.SpawnGardenSparkleExplosion(target.Center, 3, 3f, 0.5f);

            var accumNPC = target.GetGlobalNPC<ThornAccumulationNPC>();
            bool detonate = accumNPC.AddStack(target);

            if (detonate)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(Projectile.GetSource_OnHit(target), target.Center, Vector2.Zero,
                        ModContent.ProjectileType<ThornDetonationProjectile>(), Projectile.damage * 3, 8f, Projectile.owner);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _vertexStrip);

                // Bloom thorn: golden bloom glow (enhanced variant)
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null)
                {
                    Vector2 origin = glow.Size() / 2f;
                    float rot = Projectile.velocity.ToRotation();

                    // Brighter golden streak for bloom variant
                    sb.Draw(glow, drawPos, null,
                        (OdeToJoyPalette.GoldenPollen with { A = 0 }) * 0.25f,
                        rot, origin, new Vector2(0.08f, 0.03f), SpriteEffects.None, 0f);
                    // Warm amber halo
                    sb.Draw(glow, drawPos, null,
                        (OdeToJoyPalette.WarmAmber with { A = 0 }) * 0.12f,
                        0f, origin, 0.04f, SpriteEffects.None, 0f);
                }

                sb.End();
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

    /// <summary>
    /// ThornDetonationProjectile — massive burst at 25 Thorn Accumulation stacks.
    /// ExplosionParticlesFoundation pattern: 55 radial crystalline thorn shards + ImpactFoundation ripple rings.
    /// </summary>
    public class ThornDetonationProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int _timer;

        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 200;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            _timer++;

            if (_timer == 1)
            {
                // Screen shake
                if (Main.myPlayer == Projectile.owner)
                {
                    for (int i = 0; i < 8; i++)
                        Main.player[Projectile.owner].velocity += Main.rand.NextVector2Circular(0.5f, 0.5f);
                }

                // 55 radial crystalline thorn shards — custom dust
                for (int i = 0; i < 55; i++)
                {
                    float angle = MathHelper.TwoPi * i / 55f;
                    float speed = 4f + Main.rand.NextFloat() * 6f;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                    Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, ModContent.DustType<CrystallineThornSparkDust>(), vel.X, vel.Y, 100, default, 1.2f);
                    d.noGravity = true;
                    d.fadeIn = 1.8f;
                }

                // Secondary petal accents — bloom burst
                for (int i = 0; i < 20; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                    Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, ModContent.DustType<ThornBloomBurstDust>(), vel.X, vel.Y, 80, default, 0.9f);
                    d.noGravity = true;
                }

                // Screen effects on detonation
                OdeToJoyVFXLibrary.ScreenShake(8f, 16);
                OdeToJoyVFXLibrary.ScreenFlash(OdeToJoyPalette.GoldenPollen, 1.0f);
                OdeToJoyVFXLibrary.HarmonicPulseRing(Projectile.Center, 1.2f, 12, OdeToJoyPalette.GoldenPollen);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_timer < 1) return false;

            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Texture2D ringTex = ThornSprayTextures.OJPowerRing;
            Texture2D glowTex = ThornSprayTextures.SoftGlow;
            Texture2D surgeTex = ThornSprayTextures.OJBeamSurge;
            Vector2 ringOrigin = ringTex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 surgeOrigin = surgeTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float progress = _timer / 30f;
            float fadeOut = 1f - progress;
            float expand = 0.5f + progress * 1.5f;
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: CelebrationAura shader — expanding detonation rings ──
            Effect auraShader = OdeToJoyShaders.CelebrationAura;
            if (auraShader != null)
            {
                float auraRadius = 0.1f + progress * 0.4f;
                OdeToJoyShaders.SetAuraParams(auraShader, time + progress * 3f, ThornSprayTextures.BloomGold,
                    ThornSprayTextures.RadiantAmber, fadeOut * 0.6f, 2.5f, auraRadius, 5f);
                OdeToJoyShaders.BeginShaderBatch(sb, auraShader, "CelebrationAuraTechnique");
                auraShader.CurrentTechnique.Passes["P0"].Apply();
                sb.Draw(glowTex, drawPos, null, Color.White * fadeOut, progress * 2f, glowOrigin,
                    Math.Min(expand * 0.5f, 0.293f), SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: PollenDrift BloomDetonation shader — expanding bloom circle ──
            Effect pollenShader = OdeToJoyShaders.PollenDrift;
            if (pollenShader != null)
            {
                OdeToJoyShaders.SetPollenParams(pollenShader, time, ThornSprayTextures.PetalPink,
                    ThornSprayTextures.BloomGold, fadeOut * 0.4f, 2.0f, expand * 0.3f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, pollenShader, "BloomDetonationTechnique");
                sb.Draw(glowTex, drawPos, null, Color.White * fadeOut, 0f, glowOrigin,
                    Math.Min(expand * 0.7f, 0.293f), SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 2: Additive overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            // Expanding power ring
            sb.Draw(ringTex, drawPos, null, ThornSprayTextures.BloomGold * fadeOut * 0.6f, progress * 2f,
                ringOrigin, expand * 0.8f, SpriteEffects.None, 0f);
            // Beam surge overlay
            sb.Draw(surgeTex, drawPos, null, ThornSprayTextures.RadiantAmber * fadeOut * 0.45f, -progress * 1.5f,
                surgeOrigin, expand * 0.6f, SpriteEffects.None, 0f);
            // Outer glow
            sb.Draw(glowTex, drawPos, null, ThornSprayTextures.PetalPink * fadeOut * 0.3f, 0f, glowOrigin,
                Math.Min(expand * 1.2f, 0.293f), SpriteEffects.None, 0f);
            // Hot core
            sb.Draw(glowTex, drawPos, null, ThornSprayTextures.JubilantLight * fadeOut * 0.55f, 0f, glowOrigin,
                Math.Min(expand * 0.25f, 0.293f), SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
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

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= 1.5f;
        }
    }

    /// <summary>
    /// ThornSplinterProjectile — small homing splinter from detonation aftermath.
    /// </summary>
    public class ThornSplinterProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private VertexStrip _vertexStrip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Gentle homing after 15 frames
            if (Projectile.timeLeft < 75)
            {
                float homingRange = 256f;
                NPC closest = null;
                float closestDist = homingRange;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist) { closestDist = dist; closest = npc; }
                }
                if (closest != null)
                {
                    Vector2 dir = (closest.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * Projectile.velocity.Length(), 0.05f);
                }
            }

            // Trail dust — crystalline thorn splinter
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<CrystallineThornSparkDust>(), 0f, 0f, 150, default, 0.4f);
                d.noGravity = true;
                d.velocity *= 0.2f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            OdeToJoyVFXLibrary.SpawnGardenSparkleExplosion(target.Center, 2, 2f, 0.3f);

            // Splinters also apply accumulation
            var accumNPC = target.GetGlobalNPC<ThornAccumulationNPC>();
            accumNPC.AddStack(target);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _vertexStrip);

                // Thorn splinter: rose-thorn directional glow
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null)
                {
                    Vector2 origin = glow.Size() / 2f;
                    float rot = Projectile.velocity.ToRotation();

                    sb.Draw(glow, drawPos, null,
                        (OdeToJoyPalette.RosePink with { A = 0 }) * 0.18f,
                        rot, origin, new Vector2(0.05f, 0.02f), SpriteEffects.None, 0f);
                }

                sb.End();
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
