using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Projectiles
{
    // ═══════════════════════════════════════════════════════════
    // GloryShardProjectile — golden-amber glow bolt, velocity-stretched,
    // short trail. On hit: spawns ChainLightningBolt to nearest enemy.
    // ═══════════════════════════════════════════════════════════
    public class GloryShardProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;
        private static Asset<Texture2D> _softBloomTex;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            // Rotation follows velocity
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Emit golden sparks along travel path
            if (Main.rand.NextBool(2) && !Main.dedServ)
            {
                Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(-0.1f, 0.15f)
                    + Main.rand.NextVector2Circular(1.5f, 1.5f);
                AnthemParticleHandler.SpawnParticle(new ShardSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    sparkVel,
                    Main.rand.NextFloat(0.25f, 0.45f),
                    Main.rand.Next(12, 22)));
            }

            // Golden light
            Lighting.AddLight(Projectile.Center, 0.5f, 0.4f, 0.1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Confused, 60);

            // Spawn chain lightning to nearest enemy within 300px (excluding this target)
            NPC chainTarget = AnthemUtils.ClosestNPC(target.Center, 300f, target.whoAmI);
            if (chainTarget != null)
            {
                int boltType = ModContent.ProjectileType<ChainLightningBolt>();
                int damage = Projectile.damage / 2;
                var proj = Projectile.NewProjectileDirect(
                    Projectile.GetSource_OnHit(target),
                    target.Center,
                    Vector2.Zero,
                    boltType,
                    damage,
                    Projectile.knockBack * 0.5f,
                    Projectile.owner,
                    ai0: 0f,               // chain count = 0
                    ai1: chainTarget.whoAmI // target NPC index
                );
            }

            // Impact spark burst
            if (!Main.dedServ)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                    AnthemParticleHandler.SpawnParticle(new ShardSparkParticle(
                        target.Center + Main.rand.NextVector2Circular(8f, 8f),
                        sparkVel,
                        Main.rand.NextFloat(0.3f, 0.55f),
                        Main.rand.Next(15, 28)));
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            _bloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _softBloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);

            Texture2D bloom = _bloomTex.Value;
            Texture2D softBloom = _softBloomTex.Value;
            Vector2 bOrigin = bloom.Size() / 2f;
            Vector2 sOrigin = softBloom.Size() / 2f;
            SpriteBatch sb = Main.spriteBatch;

            float rot = Projectile.velocity.ToRotation();
            float speed = Projectile.velocity.Length();
            float stretchX = 0.5f + speed * 0.04f;
            float stretchY = 0.2f;
            float time = (float)Main.GameUpdateCount / 60f;

            sb.End();

            // ═══ Layer 1: JubilantHarmony — harmonic wave aura around the shard ═══
            Effect harmonyShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyJubilantHarmonyShader);

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (harmonyShader != null)
            {
                harmonyShader.Parameters["uTime"]?.SetValue(time);
                harmonyShader.Parameters["uColor"]?.SetValue(AnthemUtils.BrilliantAmber.ToVector3());
                harmonyShader.Parameters["uSecondaryColor"]?.SetValue(AnthemUtils.RoseTint.ToVector3());
                harmonyShader.Parameters["uOpacity"]?.SetValue(0.6f);
                harmonyShader.Parameters["uIntensity"]?.SetValue(1.3f);
                harmonyShader.Parameters["uRadius"]?.SetValue(0.4f);
                harmonyShader.Parameters["uHarmonicFreq"]?.SetValue(2.0f);
                harmonyShader.CurrentTechnique = harmonyShader.Techniques["SymphonicAuraTechnique"];
                harmonyShader.CurrentTechnique.Passes[0].Apply();

                sb.Draw(softBloom, Projectile.Center - Main.screenPosition, null, Color.White, rot, sOrigin,
                    new Vector2(stretchX * 1.6f, stretchY * 3f), SpriteEffects.None, 0f);
            }

            // ═══ Layer 2: Additive glow body + core ═══
            // Outer golden glow
            Color outer = AnthemUtils.Additive(AnthemUtils.BrilliantAmber, 0.55f);
            sb.Draw(bloom, Projectile.Center - Main.screenPosition, null, outer, rot, bOrigin,
                new Vector2(stretchX * 1.3f, stretchY * 1.5f), SpriteEffects.None, 0f);

            // Mid rose-tinted shimmer
            float shimmer = (float)Math.Sin(time * 4f) * 0.5f + 0.5f;
            Color roseGlow = AnthemUtils.Additive(AnthemUtils.RoseTint, shimmer * 0.2f);
            sb.Draw(softBloom, Projectile.Center - Main.screenPosition, null, roseGlow, rot, sOrigin,
                new Vector2(stretchX * 1.1f, stretchY * 2f), SpriteEffects.None, 0f);

            // Inner bright core
            Color core = AnthemUtils.Additive(AnthemUtils.GloryWhite, 0.75f);
            sb.Draw(bloom, Projectile.Center - Main.screenPosition, null, core, rot, bOrigin,
                new Vector2(stretchX * 0.7f, stretchY * 0.7f), SpriteEffects.None, 0f);

            sb.End();
            AnthemUtils.BeginDefault(sb);

            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ChainLightningBolt — instant travel to target NPC,
    // jagged line visual, can chain up to 2 times.
    // ai[0] = chain count, ai[1] = target NPC whoAmI
    // ═══════════════════════════════════════════════════════════
    public class ChainLightningBolt : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;
        private Vector2 spawnPosition;
        private bool hasInitialized;
        private float[] randomOffsets; // Pre-generated random offsets for lightning segments

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (!hasInitialized)
            {
                spawnPosition = Projectile.Center;
                hasInitialized = true;

                // Generate random offsets for lightning visual
                randomOffsets = new float[12];
                for (int i = 0; i < randomOffsets.Length; i++)
                    randomOffsets[i] = Main.rand.NextFloat(-18f, 18f);

                // Flash particles at source & target
                if (!Main.dedServ)
                {
                    AnthemParticleHandler.SpawnParticle(new LightningFlashParticle(
                        spawnPosition, Main.rand.NextFloat(0.5f, 0.8f), Main.rand.Next(8, 14)));
                }
            }

            // Instantly move to target NPC
            int targetIndex = (int)Projectile.ai[1];
            if (targetIndex >= 0 && targetIndex < Main.maxNPCs)
            {
                NPC target = Main.npc[targetIndex];
                if (target.active && !target.friendly)
                {
                    Projectile.Center = target.Center;

                    // Flash at target end
                    if (Projectile.timeLeft == 29 && !Main.dedServ)
                    {
                        AnthemParticleHandler.SpawnParticle(new LightningFlashParticle(
                            target.Center, Main.rand.NextFloat(0.4f, 0.7f), Main.rand.Next(6, 12)));
                    }
                }
                else
                {
                    Projectile.Kill();
                }
            }
            else
            {
                Projectile.Kill();
            }

            // Golden-blue light
            Lighting.AddLight(Projectile.Center, 0.4f, 0.4f, 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 90);

            int chainCount = (int)Projectile.ai[0];

            // Chain to another enemy if chains < 2
            if (chainCount < 2)
            {
                NPC nextTarget = AnthemUtils.ClosestNPC(target.Center, 300f, target.whoAmI);
                if (nextTarget != null)
                {
                    int boltType = ModContent.ProjectileType<ChainLightningBolt>();
                    Projectile.NewProjectile(
                        Projectile.GetSource_OnHit(target),
                        target.Center,
                        Vector2.Zero,
                        boltType,
                        Projectile.damage,
                        Projectile.knockBack * 0.5f,
                        Projectile.owner,
                        ai0: chainCount + 1,
                        ai1: nextTarget.whoAmI
                    );
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (!hasInitialized || randomOffsets == null)
                return false;

            _bloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _bloomTex.Value;
            Vector2 origin = tex.Size() / 2f;
            SpriteBatch sb = Main.spriteBatch;

            sb.End();
            AnthemUtils.BeginAdditive(sb);

            float fade = MathHelper.Clamp((float)Projectile.timeLeft / 30f, 0f, 1f);
            Vector2 start = spawnPosition - Main.screenPosition;
            Vector2 end = Projectile.Center - Main.screenPosition;
            Vector2 diff = end - start;
            float totalDist = diff.Length();

            if (totalDist > 2f)
            {
                Vector2 dir = diff / totalDist;
                Vector2 perp = new Vector2(-dir.Y, dir.X);

                int segments = randomOffsets.Length;
                Vector2 prevPos = start;

                for (int i = 1; i <= segments; i++)
                {
                    float t = (float)i / segments;
                    Vector2 basePos = start + diff * t;

                    // Add random perpendicular offset (not on last segment)
                    if (i < segments)
                        basePos += perp * randomOffsets[i - 1] * (1f - Math.Abs(t - 0.5f) * 2f);

                    // Draw line segment as stretched PointBloom
                    Vector2 segDiff = basePos - prevPos;
                    float segLen = segDiff.Length();
                    float segRot = segDiff.ToRotation();
                    Vector2 midpoint = (prevPos + basePos) / 2f;

                    float scaleX = segLen / tex.Width * 1.2f;
                    float scaleY = 0.06f * fade;

                    // Lightning body — white-gold
                    Color lineCol = AnthemUtils.Additive(
                        Color.Lerp(AnthemUtils.LightningBlue, AnthemUtils.GloryWhite, 0.6f),
                        fade * 0.8f);
                    sb.Draw(tex, midpoint, null, lineCol, segRot, origin,
                        new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);

                    // Outer glow — golden
                    Color glowCol = AnthemUtils.Additive(AnthemUtils.BrilliantAmber, fade * 0.35f);
                    sb.Draw(tex, midpoint, null, glowCol, segRot, origin,
                        new Vector2(scaleX * 1.2f, scaleY * 3f), SpriteEffects.None, 0f);

                    prevPos = basePos;
                }
            }

            sb.End();
            AnthemUtils.BeginDefault(sb);

            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // GloryBeamProjectile — massive golden beam, pen -1,
    // stretched PointBloom along direction, particles spraying off sides,
    // bloom at origin, music notes scattering.
    // ═══════════════════════════════════════════════════════════
    public class GloryBeamProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;
        private static Asset<Texture2D> _softBloomTex;
        private static Asset<Texture2D> _noteTex;
        private static Asset<Texture2D> _beamTex;
        private Vector2 beamDirection;
        private bool hasInitialized;

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 900;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            if (!hasInitialized)
            {
                beamDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Zero; // Beam is stationary once placed
                hasInitialized = true;

                // Origin bloom burst
                if (!Main.dedServ)
                {
                    AnthemParticleHandler.SpawnParticle(new BeamBloomParticle(
                        Projectile.Center, Main.rand.NextFloat(2.5f, 3.5f), 25));
                }
            }

            // Spray particles along beam length
            if (!Main.dedServ && Projectile.timeLeft > 5)
            {
                float beamLength = 900f;
                for (int i = 0; i < 4; i++)
                {
                    float dist = Main.rand.NextFloat(beamLength);
                    Vector2 beamPos = Projectile.Center + beamDirection * dist;
                    Vector2 perp = new Vector2(-beamDirection.Y, beamDirection.X);

                    // Side-spray particles
                    Vector2 sideVel = perp * Main.rand.NextFloat(-3f, 3f) + beamDirection * Main.rand.NextFloat(-0.5f, 0.5f);
                    AnthemParticleHandler.SpawnParticle(new GloryBeamParticle(
                        beamPos + perp * Main.rand.NextFloat(-10f, 10f),
                        sideVel,
                        Main.rand.NextFloat(0.2f, 0.5f),
                        Main.rand.Next(10, 20)));
                }

                // Music notes every other frame
                if (Projectile.timeLeft % 2 == 0)
                {
                    float dist = Main.rand.NextFloat(beamLength);
                    Vector2 notePos = Projectile.Center + beamDirection * dist;
                    Vector2 perp2 = new Vector2(-beamDirection.Y, beamDirection.X);
                    Vector2 noteVel = perp2 * Main.rand.NextFloat(-2f, 2f) + new Vector2(0, -0.5f);
                    AnthemParticleHandler.SpawnParticle(new AnthemNoteParticle(
                        notePos + perp2 * Main.rand.NextFloat(-15f, 15f),
                        noteVel,
                        Main.rand.NextFloat(0.4f, 0.7f),
                        Main.rand.Next(30, 55)));
                }
            }

            // Bright light along beam
            for (float d = 0; d < 900f; d += 100f)
            {
                Vector2 lightPos = Projectile.Center + beamDirection * d;
                Lighting.AddLight(lightPos, 0.6f, 0.5f, 0.15f);
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 2x weapon damage
            modifiers.SourceDamage *= 2f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 240);
            target.AddBuff(BuffID.Venom, 120);
            target.AddBuff(BuffID.Confused, 120);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Line collision along the beam direction
            if (!hasInitialized) return false;

            float collisionPoint = 0f;
            Vector2 beamEnd = Projectile.Center + beamDirection * 900f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center, beamEnd, 30f, ref collisionPoint);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (!hasInitialized) return false;

            _bloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _softBloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _beamTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/BeamTextures/HorizontalBeamStreakSegment", AssetRequestMode.ImmediateLoad);

            Texture2D bloom = _bloomTex.Value;
            Texture2D softBloom = _softBloomTex.Value;
            Texture2D beamStreak = _beamTex.Value;
            Vector2 bOrigin = bloom.Size() / 2f;
            Vector2 sOrigin = softBloom.Size() / 2f;
            Vector2 beamOrigin = new Vector2(0f, beamStreak.Height / 2f);
            SpriteBatch sb = Main.spriteBatch;

            float fade = MathHelper.Clamp((float)Projectile.timeLeft / 20f, 0f, 1f);
            float rot = beamDirection.ToRotation();
            float time = (float)Main.GameUpdateCount / 60f;
            float beamLength = 900f;
            Vector2 beamMid = Projectile.Center + beamDirection * (beamLength * 0.5f) - Main.screenPosition;

            sb.End();

            // ═══ Layer 1: HarmonicBeam shader — standing-wave beam body with staff lines ═══
            Effect harmonyShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyJubilantHarmonyShader);

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (harmonyShader != null)
            {
                harmonyShader.Parameters["uTime"]?.SetValue(time);
                harmonyShader.Parameters["uColor"]?.SetValue(AnthemUtils.BrilliantAmber.ToVector3());
                harmonyShader.Parameters["uSecondaryColor"]?.SetValue(AnthemUtils.RoseTint.ToVector3());
                harmonyShader.Parameters["uOpacity"]?.SetValue(fade * 0.85f);
                harmonyShader.Parameters["uIntensity"]?.SetValue(1.5f);
                harmonyShader.Parameters["uHarmonicFreq"]?.SetValue(1.5f);
                harmonyShader.CurrentTechnique = harmonyShader.Techniques["HarmonicBeamTechnique"];
                harmonyShader.CurrentTechnique.Passes[0].Apply();

                // Draw as a stretched beam quad; UV.x maps along length, UV.y maps across width
                float beamScaleX = beamLength / bloom.Width;
                float beamScaleY = 0.55f * fade;
                sb.Draw(bloom, beamMid, null, Color.White, rot, bOrigin,
                    new Vector2(beamScaleX, beamScaleY), SpriteEffects.None, 0f);
            }

            sb.End();

            // ═══ Layer 2: Additive glow layers ═══
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float scaleX = beamLength / bloom.Width;
            float scaleY = 0.35f * fade;

            // Outer warm amber glow
            Color outerGlow = AnthemUtils.Additive(AnthemUtils.BrilliantAmber, fade * 0.4f);
            sb.Draw(bloom, beamMid, null, outerGlow, rot, bOrigin,
                new Vector2(scaleX, scaleY * 2.5f), SpriteEffects.None, 0f);

            // Mid golden body
            Color midGold = AnthemUtils.Additive(
                Color.Lerp(AnthemUtils.RichGold, AnthemUtils.GloryWhite, fade * 0.3f),
                fade * 0.55f);
            sb.Draw(bloom, beamMid, null, midGold, rot, bOrigin,
                new Vector2(scaleX, scaleY * 1.2f), SpriteEffects.None, 0f);

            // Inner white-gold core
            Color coreCol = AnthemUtils.Additive(AnthemUtils.GloryWhite, fade * 0.8f);
            sb.Draw(bloom, beamMid, null, coreCol, rot, bOrigin,
                new Vector2(scaleX, scaleY * 0.35f), SpriteEffects.None, 0f);

            // Origin bloom flare with SoftRadialBloom
            Color originBloom = AnthemUtils.Additive(AnthemUtils.BrilliantAmber, fade * 0.65f);
            sb.Draw(softBloom, Projectile.Center - Main.screenPosition, null, originBloom, rot, sOrigin,
                new Vector2(1.6f * fade, 1.6f * fade), SpriteEffects.None, 0f);

            // Rose-tinted pulsing shimmer
            float shimmer = (float)Math.Sin(time * 4f) * 0.5f + 0.5f;
            Color roseShimmer = AnthemUtils.Additive(AnthemUtils.RoseTint, fade * shimmer * 0.3f);
            sb.Draw(bloom, beamMid, null, roseShimmer, rot, bOrigin,
                new Vector2(scaleX * 0.9f, scaleY * 1.8f), SpriteEffects.None, 0f);

            sb.End();
            AnthemUtils.BeginDefault(sb);

            return false;
        }
    }
}
