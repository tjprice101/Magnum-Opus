using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Projectiles
{
    // ═══════════════════════════════════════════════════════════
    // VineWaveProjectile — rolling wave of thorny golden vines
    // 8-point trail cache, stretched bloom rendering, green→gold gradient
    // ═══════════════════════════════════════════════════════════
    public class VineWaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;
        private Vector2[] trailCache = new Vector2[8];
        private float[] trailRotations = new float[8];
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

            Texture2D tex = _bloomTex.Value;
            Vector2 origin = tex.Size() / 2f;
            SpriteBatch sb = Main.spriteBatch;

            sb.End();

            // Draw trail in additive mode
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float lifeProgress = 1f - (Projectile.timeLeft / 60f);

            // Draw trail points (oldest to newest)
            for (int i = trailCache.Length - 1; i >= 0; i--)
            {
                float trailT = (float)i / (trailCache.Length - 1); // 0 = newest, 1 = oldest
                float trailFade = 1f - trailT;
                float trailScale = MathHelper.Lerp(0.8f, 0.2f, trailT);

                // Green→Gold gradient along trail
                Color trailColor = ReckoningUtils.Additive(
                    Color.Lerp(ReckoningUtils.ForestGreen, ReckoningUtils.JubilantGold, trailT),
                    trailFade * 0.6f);

                // Stretch along velocity direction
                float rot = i < trailCache.Length - 1 ?
                    (trailCache[i] - trailCache[i + 1]).ToRotation() : trailRotations[i];

                sb.Draw(tex, trailCache[i] - Main.screenPosition, null, trailColor, rot, origin,
                    new Vector2(trailScale * 1.5f, trailScale * 0.6f), SpriteEffects.None, 0f);
            }

            // Draw main body bloom
            float pulse = 1f + (float)Math.Sin(Projectile.ai[0] * 0.3f) * 0.15f;
            Color mainColor = ReckoningUtils.Additive(ReckoningUtils.JubilantGold, 0.8f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, mainColor,
                Projectile.rotation, origin, 0.9f * pulse, SpriteEffects.None, 0f);

            // Inner white core
            Color coreColor = ReckoningUtils.Additive(ReckoningUtils.WhiteBloom, 0.5f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, coreColor,
                Projectile.rotation, origin, 0.45f * pulse, SpriteEffects.None, 0f);

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

            Texture2D tex = _bloomTex.Value;
            Vector2 origin = tex.Size() / 2f;
            SpriteBatch sb = Main.spriteBatch;

            float fade = Projectile.timeLeft / 30f;

            sb.End();

            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer golden glow — expanding ring
            Color outerColor = ReckoningUtils.Additive(ReckoningUtils.JubilantGold, fade * 0.5f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, outerColor,
                0f, origin, currentScale * 2f, SpriteEffects.None, 0f);

            // Mid-layer — verdant gold
            Color midColor = ReckoningUtils.Additive(ReckoningUtils.VerdantGold, fade * 0.6f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, midColor,
                0f, origin, currentScale * 1.3f, SpriteEffects.None, 0f);

            // Inner white-hot core
            Color coreColor = ReckoningUtils.Additive(ReckoningUtils.WhiteBloom, fade * 0.7f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, coreColor,
                0f, origin, currentScale * 0.6f, SpriteEffects.None, 0f);

            sb.End();
            ReckoningUtils.BeginDefault(sb);

            return false;
        }
    }
}
