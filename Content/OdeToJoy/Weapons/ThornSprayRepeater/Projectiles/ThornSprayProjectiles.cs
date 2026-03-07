using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Buffs;
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

            // Ambient sparkle dust
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 150, ThornSprayTextures.RadiantAmber, 0.5f);
                d.noGravity = true;
                d.velocity *= 0.3f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
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
            Texture2D thornTex = ThornSprayTextures.OJThornFragment;
            Texture2D glowTex = ThornSprayTextures.SoftGlow;
            Vector2 thornOrigin = thornTex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;

            float lifeProgress = 1f - (Projectile.timeLeft / 240f);
            float fade = MathHelper.Clamp(lifeProgress * 8f, 0f, 1f) * MathHelper.Clamp((Projectile.timeLeft / 30f), 0f, 1f);
            float time = (float)Main.timeForVisualEffects * 0.015f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            sb.End();

            // ── LAYER 0: VerdantSlash shader trail via VertexStrip ──
            Effect slashShader = OdeToJoyShaders.VerdantSlash;
            int validCount = 0;
            for (int i = 0; i < TrailLength; i++)
            {
                int idx = (_trailHead - 1 - i + TrailLength) % TrailLength;
                if (_trailPositions[idx] != Vector2.Zero) validCount++; else break;
            }
            if (slashShader != null && validCount >= 2)
            {
                Vector2[] positions = new Vector2[validCount];
                float[] rotations = new float[validCount];
                for (int i = 0; i < validCount; i++)
                {
                    int idx = (_trailHead - 1 - i + TrailLength) % TrailLength;
                    positions[validCount - 1 - i] = _trailPositions[idx];
                }
                for (int i = 0; i < validCount; i++)
                {
                    if (i < validCount - 1) rotations[i] = (positions[i + 1] - positions[i]).ToRotation();
                    else rotations[i] = rotations[Math.Max(0, i - 1)];
                }

                VertexStrip strip = new VertexStrip();
                strip.PrepareStrip(positions, rotations,
                    (float p) => ThornSprayTextures.GetThornGradient(1f - p, false) * fade * p * 0.45f,
                    (float p) => MathHelper.Lerp(1f, 10f, p),
                    -Main.screenPosition, includeBacksides: true);
                OdeToJoyShaders.SetSlashParams(slashShader, time, ThornSprayTextures.RadiantAmber,
                    ThornSprayTextures.PetalPink, fade * 0.6f, 1.8f, 0f);
                slashShader.CurrentTechnique = slashShader.Techniques["VerdantSlashTechnique"];
                slashShader.Parameters["WorldViewProjection"]?.SetValue(
                    Main.GameViewMatrix.NormalizedTransformationmatrix);
                slashShader.CurrentTechnique.Passes["P0"].Apply();
                strip.DrawTrail();
                Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            }

            // ── LAYER 1: Additive bloom layers ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            // Ambient shimmer
            float shimmer = 0.85f + 0.15f * (float)Math.Sin(Projectile.ai[0]++ * 0.15f);
            sb.Draw(glowTex, pos, null, ThornSprayTextures.BloomGold * fade * 0.25f * shimmer, Projectile.rotation,
                glowOrigin, 0.28f, SpriteEffects.None, 0f);
            // Inner glow
            sb.Draw(glowTex, pos, null, ThornSprayTextures.RadiantAmber * fade * 0.5f, 0f, glowOrigin,
                0.2f, SpriteEffects.None, 0f);
            // Thorn body
            sb.Draw(thornTex, pos, null, ThornSprayTextures.PetalPink * fade * 0.9f, Projectile.rotation,
                thornOrigin, 0.8f, SpriteEffects.None, 0f);
            // Hot core
            sb.Draw(glowTex, pos, null, ThornSprayTextures.JubilantLight * fade * 0.4f, 0f, glowOrigin,
                0.1f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
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

            // Golden sparkle dust (more prominent than standard)
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 100, ThornSprayTextures.BloomGold, 0.8f);
                d.noGravity = true;
                d.velocity *= 0.4f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
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
            Texture2D thornTex = ThornSprayTextures.OJThornFragment;
            Texture2D glowTex = ThornSprayTextures.SoftGlow;
            Vector2 thornOrigin = thornTex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;

            float fade = MathHelper.Clamp((240 - Projectile.timeLeft) / 10f, 0f, 1f) * MathHelper.Clamp(Projectile.timeLeft / 30f, 0f, 1f);
            float time = (float)Main.timeForVisualEffects * 0.015f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            sb.End();

            // ── LAYER 0: TriumphantTrail VertexStrip — golden bloom trail ──
            Effect trailShader = OdeToJoyShaders.TriumphantTrail;
            int validCount = 0;
            for (int i = 0; i < TrailLength; i++)
            {
                int idx = (_trailHead - 1 - i + TrailLength) % TrailLength;
                if (_trailPositions[idx] != Vector2.Zero) validCount++; else break;
            }
            if (trailShader != null && validCount >= 2)
            {
                Vector2[] positions = new Vector2[validCount];
                float[] rotations = new float[validCount];
                for (int i = 0; i < validCount; i++)
                {
                    int idx = (_trailHead - 1 - i + TrailLength) % TrailLength;
                    positions[validCount - 1 - i] = _trailPositions[idx];
                }
                for (int i = 0; i < validCount; i++)
                {
                    if (i < validCount - 1) rotations[i] = (positions[i + 1] - positions[i]).ToRotation();
                    else rotations[i] = rotations[Math.Max(0, i - 1)];
                }

                VertexStrip strip = new VertexStrip();
                strip.PrepareStrip(positions, rotations,
                    (float p) => ThornSprayTextures.BloomGold * fade * p * 0.45f,
                    (float p) => MathHelper.Lerp(1f, 14f, p),
                    -Main.screenPosition, includeBacksides: true);
                OdeToJoyShaders.SetTrailParams(trailShader, time, ThornSprayTextures.BloomGold,
                    ThornSprayTextures.JubilantLight, fade * 0.7f, 2.0f);
                trailShader.CurrentTechnique = trailShader.Techniques["TriumphantTrailTechnique"];
                trailShader.Parameters["WorldViewProjection"]?.SetValue(
                    Main.GameViewMatrix.NormalizedTransformationmatrix);
                trailShader.CurrentTechnique.Passes["P0"].Apply();
                strip.DrawTrail();
                Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            }

            // ── LAYER 1: Additive bloom layers ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            // Bright golden shimmer
            float shimmer = 0.8f + 0.2f * (float)Math.Sin(Projectile.ai[0]++ * 0.12f);
            sb.Draw(glowTex, pos, null, ThornSprayTextures.BloomGold * fade * 0.4f * shimmer, 0f, glowOrigin,
                0.28f, SpriteEffects.None, 0f);
            // Bloom inner glow
            sb.Draw(glowTex, pos, null, ThornSprayTextures.JubilantLight * fade * 0.6f, 0f, glowOrigin,
                0.25f, SpriteEffects.None, 0f);
            // Golden thorn body
            sb.Draw(thornTex, pos, null, ThornSprayTextures.BloomGold * fade * 0.9f, Projectile.rotation,
                thornOrigin, 0.9f, SpriteEffects.None, 0f);
            // Hot core
            sb.Draw(glowTex, pos, null, ThornSprayTextures.PureJoyWhite * fade * 0.5f, 0f, glowOrigin,
                0.12f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
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

                // 55 radial crystalline thorn shards (dust)
                for (int i = 0; i < 55; i++)
                {
                    float angle = MathHelper.TwoPi * i / 55f;
                    float speed = 4f + Main.rand.NextFloat() * 6f;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                    Color sparkColor = ThornSprayTextures.DetonationColors[i % ThornSprayTextures.DetonationColors.Length];
                    Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.GoldFlame, vel.X, vel.Y, 100, sparkColor, 1.2f);
                    d.noGravity = true;
                    d.fadeIn = 1.8f;
                }

                // Secondary petal accents
                for (int i = 0; i < 20; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                    Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.YellowTorch, vel.X, vel.Y, 80, ThornSprayTextures.PetalPink, 0.9f);
                    d.noGravity = true;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_timer < 1) return false;

            SpriteBatch sb = Main.spriteBatch;
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

            // Trail dust
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 150, ThornSprayTextures.RoseShadow, 0.4f);
                d.noGravity = true;
                d.velocity *= 0.2f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Splinters also apply accumulation
            var accumNPC = target.GetGlobalNPC<ThornAccumulationNPC>();
            accumNPC.AddStack(target);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D thornTex = ThornSprayTextures.OJThornFragment;
            Texture2D glowTex = ThornSprayTextures.SoftGlow;
            Vector2 thornOrigin = thornTex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            float fade = MathHelper.Clamp(Projectile.timeLeft / 20f, 0f, 1f);
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: VerdantSlash shader accent ──
            Effect slashShader = OdeToJoyShaders.VerdantSlash;
            if (slashShader != null)
            {
                OdeToJoyShaders.SetSlashParams(slashShader, time * 1.5f, ThornSprayTextures.RoseShadow,
                    ThornSprayTextures.PetalPink, fade * 0.35f, 1.3f, 0f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, slashShader, "ThornImpactTechnique");
                sb.Draw(glowTex, pos, null, Color.White * fade, Projectile.rotation, glowOrigin,
                    0.2f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive splinter body ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            sb.Draw(glowTex, pos, null, ThornSprayTextures.RoseShadow * fade * 0.3f, 0f, glowOrigin,
                0.18f, SpriteEffects.None, 0f);
            sb.Draw(thornTex, pos, null, ThornSprayTextures.PetalPink * fade * 0.7f, Projectile.rotation,
                thornOrigin, 0.5f, SpriteEffects.None, 0f);
            sb.Draw(glowTex, pos, null, ThornSprayTextures.JubilantLight * fade * 0.25f, 0f, glowOrigin,
                0.06f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }
}