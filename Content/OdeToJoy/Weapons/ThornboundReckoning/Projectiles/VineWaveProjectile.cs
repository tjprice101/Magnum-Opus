using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Projectiles
{
    // ═══════════════════════════════════════════════════════════
    // VineWaveProjectile — rolling wave of thorny golden vines
    // 16-point trail cache, shader-driven TriumphantTrail rendering,
    // green→gold gradient with vine energy veins, layered additive bloom
    // ═══════════════════════════════════════════════════════════
    public class VineWaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;
        private static Asset<Texture2D> _softBloomTex;
        private static Asset<Texture2D> _noiseTex;
        private Vector2[] trailCache = new Vector2[16];
        private float[] trailRotations = new float[16];
        private float[] trailWidths = new float[16];
        private bool trailInitialized = false;

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            // Initialize trail
            if (!trailInitialized)
            {
                for (int i = 0; i < trailCache.Length; i++)
                    trailCache[i] = Projectile.Center;
                trailInitialized = true;
            }

            // Update trail cache (shift positions backward, newest at [0])
            for (int i = trailCache.Length - 1; i > 0; i--)
            {
                trailCache[i] = trailCache[i - 1];
                trailRotations[i] = trailRotations[i - 1];
            }
            trailCache[0] = Projectile.Center;
            trailRotations[0] = Projectile.velocity.ToRotation();

            // Gentle sine wave motion perpendicular to velocity
            float sineWave = (float)Math.Sin(Projectile.ai[0] * 0.15f) * 2.5f;
            Vector2 perp = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
            Projectile.Center += perp * sineWave * 0.3f;
            Projectile.ai[0]++;

            // Slight deceleration
            Projectile.velocity *= 0.985f;

            // Spawn particles every 3 ticks
            if ((int)Projectile.ai[0] % 3 == 0 && !Main.dedServ)
            {
                // Vine spark along path
                Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.5f) * 0.3f + perp * Main.rand.NextFloat(-1.5f, 1.5f);
                var spark = new VineSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 10f),
                    sparkVel,
                    Color.Lerp(ReckoningUtils.ForestGreen, ReckoningUtils.VerdantGold, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.3f, 0.6f),
                    Main.rand.Next(15, 30));
                ReckoningParticleHandler.SpawnParticle(spark);

                // Petal swirl
                var petal = new PetalSwirlParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(20f, 12f),
                    Main.rand.NextVector2Circular(1.5f, 1.5f) + new Vector2(0f, -0.5f),
                    Main.rand.NextFloat(0.2f, 0.45f),
                    Main.rand.Next(25, 50));
                ReckoningParticleHandler.SpawnParticle(petal);
            }

            // Rotation follows velocity
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Lighting
            Lighting.AddLight(Projectile.Center, 0.3f, 0.5f, 0.1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 180);

            // Impact sparks
            if (!Main.dedServ)
            {
                for (int i = 0; i < 4; i++)
                {
                    var spark = new VineSparkParticle(
                        target.Center + Main.rand.NextVector2Circular(10f, 10f),
                        Main.rand.NextVector2Circular(4f, 4f),
                        ReckoningUtils.VerdantGold,
                        Main.rand.NextFloat(0.3f, 0.5f),
                        Main.rand.Next(10, 20));
                    ReckoningParticleHandler.SpawnParticle(spark);
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
            Vector2 bloomOrigin = bloom.Size() / 2f;
            Vector2 softOrigin = softBloom.Size() / 2f;
            SpriteBatch sb = Main.spriteBatch;

            float time = (float)Main.GameUpdateCount / 60f;
            float pulse = 1f + (float)Math.Sin(Projectile.ai[0] * 0.3f) * 0.15f;

            sb.End();

            // ═══ Layer 1: Shader-driven trail (TriumphantTrail — golden energy veins) ═══
            Effect trailShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyTriumphantTrailShader);
            bool hasTrailShader = trailShader != null;

            if (hasTrailShader)
            {
                trailShader.Parameters["uTime"]?.SetValue(time);
                trailShader.Parameters["uColor"]?.SetValue(ReckoningUtils.JubilantGold.ToVector3());
                trailShader.Parameters["uSecondaryColor"]?.SetValue(ReckoningUtils.ForestGreen.ToVector3());
                trailShader.Parameters["uIntensity"]?.SetValue(1.4f);
                trailShader.CurrentTechnique = trailShader.Techniques["TriumphantTrailTechnique"];

                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                // Draw trail segments connecting consecutive cache points (oldest→newest)
                for (int i = trailCache.Length - 2; i >= 0; i--)
                {
                    float trailT = (float)i / (trailCache.Length - 1); // 0 = newest, 1 = oldest
                    float trailFade = 1f - trailT;

                    Vector2 segStart = trailCache[i + 1];
                    Vector2 segEnd = trailCache[i];
                    float segLen = Vector2.Distance(segStart, segEnd);
                    if (segLen < 1f) continue;

                    float segAngle = (segEnd - segStart).ToRotation();
                    float segWidth = MathHelper.Lerp(10f, 38f, trailFade);
                    Vector2 midpoint = (segStart + segEnd) / 2f;

                    // Per-segment opacity fade along trail
                    trailShader.Parameters["uOpacity"]?.SetValue(trailFade * 0.85f);
                    trailShader.CurrentTechnique.Passes[0].Apply();

                    sb.Draw(bloom, midpoint - Main.screenPosition, null, Color.White, segAngle, bloomOrigin,
                        new Vector2(segLen / bloom.Width * 1.1f, segWidth / bloom.Height), SpriteEffects.None, 0f);
                }

                sb.End();
            }
            else
            {
                // Fallback: particle-based trail (original rendering)
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                for (int i = trailCache.Length - 1; i >= 0; i--)
                {
                    float trailT = (float)i / (trailCache.Length - 1);
                    float trailFade = 1f - trailT;
                    float trailScale = MathHelper.Lerp(0.8f, 0.2f, trailT);
                    Color trailColor = ReckoningUtils.Additive(
                        Color.Lerp(ReckoningUtils.ForestGreen, ReckoningUtils.JubilantGold, trailT),
                        trailFade * 0.6f);
                    float rot = i < trailCache.Length - 1 ?
                        (trailCache[i] - trailCache[i + 1]).ToRotation() : trailRotations[i];
                    sb.Draw(bloom, trailCache[i] - Main.screenPosition, null, trailColor, rot, bloomOrigin,
                        new Vector2(trailScale * 1.5f, trailScale * 0.6f), SpriteEffects.None, 0f);
                }

                sb.End();
            }

            // ═══ Layer 2: Pulsing garden bloom body (GardenBloom — JubilantPulse) ═══
            Effect bloomShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyGardenBloomShader);

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (bloomShader != null)
            {
                bloomShader.Parameters["uTime"]?.SetValue(time);
                bloomShader.Parameters["uColor"]?.SetValue(ReckoningUtils.JubilantGold.ToVector3());
                bloomShader.Parameters["uSecondaryColor"]?.SetValue(ReckoningUtils.VerdantGold.ToVector3());
                bloomShader.Parameters["uOpacity"]?.SetValue(0.7f);
                bloomShader.Parameters["uIntensity"]?.SetValue(1.3f);
                bloomShader.Parameters["uRadius"]?.SetValue(0.42f);
                bloomShader.Parameters["uPulseSpeed"]?.SetValue(3.5f);
                bloomShader.CurrentTechnique = bloomShader.Techniques["JubilantPulseTechnique"];
                bloomShader.CurrentTechnique.Passes[0].Apply();

                sb.Draw(softBloom, Projectile.Center - Main.screenPosition, null, Color.White,
                    Projectile.rotation, softOrigin, 1.1f * pulse, SpriteEffects.None, 0f);
            }
            else
            {
                Color mainColor = ReckoningUtils.Additive(ReckoningUtils.JubilantGold, 0.8f);
                sb.Draw(bloom, Projectile.Center - Main.screenPosition, null, mainColor,
                    Projectile.rotation, bloomOrigin, 0.9f * pulse, SpriteEffects.None, 0f);
            }

            sb.End();

            // ═══ Layer 3: Inner white-hot core + verdant accent (simple additive) ═══
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Color coreColor = ReckoningUtils.Additive(ReckoningUtils.WhiteBloom, 0.55f);
            sb.Draw(bloom, Projectile.Center - Main.screenPosition, null, coreColor,
                Projectile.rotation, bloomOrigin, 0.4f * pulse, SpriteEffects.None, 0f);

            Color accentColor = ReckoningUtils.Additive(ReckoningUtils.ForestGreen, 0.2f);
            sb.Draw(softBloom, Projectile.Center - Main.screenPosition, null, accentColor,
                Projectile.rotation, softOrigin, 0.65f * pulse, SpriteEffects.None, 0f);

            sb.End();
            ReckoningUtils.BeginDefault(sb);

            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // BloomExplosionProjectile — massive jubilant flower explosion
    // Huge AoE burst with radial particle effects
    // ═══════════════════════════════════════════════════════════
    public class BloomExplosionProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;
        private static Asset<Texture2D> _softBloomTex;
        private bool hasSpawnedBurst = false;
        private float currentScale = 0f;

        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 150;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            // Spawn massive particle burst on first frame
            if (!hasSpawnedBurst && !Main.dedServ)
            {
                hasSpawnedBurst = true;
                SpawnBloomBurst();
            }

            // Expand scale from 0 to 1 over lifetime
            float progress = 1f - (Projectile.timeLeft / 30f);
            currentScale = MathHelper.Lerp(0f, 1.5f, MathHelper.Clamp(progress * 2f, 0f, 1f));

            // Fading lighting
            float fade = Projectile.timeLeft / 30f;
            Lighting.AddLight(Projectile.Center, 0.8f * fade, 0.6f * fade, 0.2f * fade);
        }

        private void SpawnBloomBurst()
        {
            // 12 Golden Bloom Bursts — radial spread
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                var bloom = new GoldenBloomBurstParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    vel,
                    Main.rand.NextFloat(1.2f, 2.0f),
                    Main.rand.Next(25, 45));
                ReckoningParticleHandler.SpawnParticle(bloom);
            }

            // 20 Petal Swirls — scattered outward
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f) + new Vector2(0f, -1f);
                var petal = new PetalSwirlParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(30f, 30f),
                    vel,
                    Main.rand.NextFloat(0.3f, 0.7f),
                    Main.rand.Next(40, 70));
                ReckoningParticleHandler.SpawnParticle(petal);
            }

            // 6 Reckoning Notes — musical accents rising
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f));
                var note = new ReckoningNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(40f, 40f),
                    vel,
                    Main.rand.NextFloat(0.4f, 0.7f),
                    Main.rand.Next(40, 65));
                ReckoningParticleHandler.SpawnParticle(note);
            }

            // 8 Verdant Mist clouds — ambient fog
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f) + new Vector2(0f, -0.3f);
                var mist = new VerdantMistParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(40f, 40f),
                    vel,
                    Main.rand.NextFloat(0.8f, 1.4f),
                    Main.rand.Next(35, 55));
                ReckoningParticleHandler.SpawnParticle(mist);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Midas, 300);
            target.AddBuff(BuffID.Poisoned, 300);
            target.AddBuff(BuffID.Venom, 180);

            // Hit sparks
            if (!Main.dedServ)
            {
                for (int i = 0; i < 3; i++)
                {
                    var spark = new VineSparkParticle(
                        target.Center,
                        Main.rand.NextVector2Circular(5f, 5f),
                        ReckoningUtils.JubilantGold,
                        Main.rand.NextFloat(0.4f, 0.7f),
                        Main.rand.Next(12, 22));
                    ReckoningParticleHandler.SpawnParticle(spark);
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

            float fade = Projectile.timeLeft / 30f;
            float time = (float)Main.GameUpdateCount / 60f;

            sb.End();

            // ═══ Layer 1: CelebrationAura — expanding concentric golden rings ═══
            Effect auraShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyCelebrationAuraShader);

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (auraShader != null)
            {
                auraShader.Parameters["uTime"]?.SetValue(time);
                auraShader.Parameters["uColor"]?.SetValue(ReckoningUtils.JubilantGold.ToVector3());
                auraShader.Parameters["uSecondaryColor"]?.SetValue(ReckoningUtils.VerdantGold.ToVector3());
                auraShader.Parameters["uOpacity"]?.SetValue(fade * 0.6f);
                auraShader.Parameters["uIntensity"]?.SetValue(1.5f);
                auraShader.Parameters["uRadius"]?.SetValue(0.45f);
                auraShader.Parameters["uRingCount"]?.SetValue(5f);
                auraShader.CurrentTechnique = auraShader.Techniques["CelebrationAuraTechnique"];
                auraShader.CurrentTechnique.Passes[0].Apply();

                sb.Draw(softBloom, Projectile.Center - Main.screenPosition, null, Color.White,
                    0f, sOrigin, currentScale * 2.8f, SpriteEffects.None, 0f);
            }

            // ═══ Layer 2: GardenBloom — 5-petal floral bloom shape ═══
            Effect bloomShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyGardenBloomShader);

            if (bloomShader != null)
            {
                bloomShader.Parameters["uTime"]?.SetValue(time);
                bloomShader.Parameters["uColor"]?.SetValue(ReckoningUtils.JubilantGold.ToVector3());
                bloomShader.Parameters["uSecondaryColor"]?.SetValue(ReckoningUtils.RoseGold.ToVector3());
                bloomShader.Parameters["uOpacity"]?.SetValue(fade * 0.75f);
                bloomShader.Parameters["uIntensity"]?.SetValue(1.5f);
                bloomShader.Parameters["uRadius"]?.SetValue(0.4f);
                bloomShader.Parameters["uPulseSpeed"]?.SetValue(5f);
                bloomShader.CurrentTechnique = bloomShader.Techniques["GardenBloomTechnique"];
                bloomShader.CurrentTechnique.Passes[0].Apply();

                sb.Draw(softBloom, Projectile.Center - Main.screenPosition, null, Color.White,
                    0f, sOrigin, currentScale * 2f, SpriteEffects.None, 0f);
            }

            sb.End();

            // ═══ Layer 3: Bright additive core and outer glow ═══
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer golden glow
            Color outerColor = ReckoningUtils.Additive(ReckoningUtils.JubilantGold, fade * 0.45f);
            sb.Draw(softBloom, Projectile.Center - Main.screenPosition, null, outerColor,
                0f, sOrigin, currentScale * 2.2f, SpriteEffects.None, 0f);

            // Mid-layer verdant gold
            Color midColor = ReckoningUtils.Additive(ReckoningUtils.VerdantGold, fade * 0.55f);
            sb.Draw(bloom, Projectile.Center - Main.screenPosition, null, midColor,
                0f, bOrigin, currentScale * 1.3f, SpriteEffects.None, 0f);

            // White-hot core
            Color coreColor = ReckoningUtils.Additive(ReckoningUtils.WhiteBloom, fade * 0.75f);
            sb.Draw(bloom, Projectile.Center - Main.screenPosition, null, coreColor,
                0f, bOrigin, currentScale * 0.55f, SpriteEffects.None, 0f);

            sb.End();
            ReckoningUtils.BeginDefault(sb);

            return false;
        }
    }
}
