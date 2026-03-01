using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Projectiles
{
    // ═══════════════════════════════════════════════════════════
    // PollenSeedProjectile — glowing pollen seed with mild homing
    // 16x16, pen 1, timeLeft 300, 0.03 lerp homing
    // On hit: spawns 3 HomingPetalProjectile + applies Poisoned 120
    // ═══════════════════════════════════════════════════════════
    public class PollenSeedProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;
        private static Asset<Texture2D> _softBloomTex;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.alpha = Math.Max(Projectile.alpha - 25, 0);

            // Mild homing toward closest NPC
            NPC target = PollinatorUtils.ClosestNPC(Projectile.Center, 600f);
            if (target != null)
            {
                Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.03f);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Spawn trail particles
            if (!Main.dedServ && (int)Projectile.ai[0] % 2 == 0)
            {
                var trail = new SeedTrailParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    Color.Lerp(PollinatorUtils.LeafGreen, PollinatorUtils.PollenGold, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.15f, 0.3f),
                    Main.rand.Next(10, 20));
                PollinatorParticleHandler.SpawnParticle(trail);
            }

            Projectile.ai[0]++;
            Lighting.AddLight(Projectile.Center, 0.2f, 0.3f, 0.05f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 120);

            // Spawn 3 homing petals
            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = MathHelper.TwoPi / 3f * i + Main.rand.NextFloat(-0.3f, 0.3f);
                    Vector2 petalVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                    int dmg = Math.Max(Projectile.damage / 4, 1);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, petalVel,
                        ModContent.ProjectileType<HomingPetalProjectile>(), dmg, Projectile.knockBack * 0.5f, Projectile.owner);
                }
            }

            // Impact VFX
            if (!Main.dedServ)
            {
                for (int i = 0; i < 5; i++)
                {
                    var dust = new PollenDustParticle(
                        target.Center + Main.rand.NextVector2Circular(12f, 12f),
                        Main.rand.NextVector2Circular(2.5f, 2.5f),
                        Main.rand.NextFloat(0.2f, 0.4f),
                        Main.rand.Next(15, 30));
                    PollinatorParticleHandler.SpawnParticle(dust);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (!Main.dedServ)
            {
                for (int i = 0; i < 3; i++)
                {
                    var dust = new PollenDustParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                        Main.rand.NextVector2Circular(1.5f, 1.5f),
                        Main.rand.NextFloat(0.15f, 0.3f),
                        Main.rand.Next(10, 20));
                    PollinatorParticleHandler.SpawnParticle(dust);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            _bloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _softBloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _bloomTex.Value;
            Texture2D softBloom = _softBloomTex.Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 sOrigin = softBloom.Size() / 2f;
            SpriteBatch sb = Main.spriteBatch;
            float time = (float)Main.GameUpdateCount / 60f;

            float pulse = 1f + (float)Math.Sin(Projectile.ai[0] * 0.2f) * 0.15f;
            float alphaFade = 1f - (Projectile.alpha / 255f);

            sb.End();

            // ═══ Layer 1: PollenDrift shader — drifting pollen aura around the seed ═══
            Effect pollenShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyPollenDriftShader);

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (pollenShader != null)
            {
                pollenShader.Parameters["uTime"]?.SetValue(time);
                pollenShader.Parameters["uColor"]?.SetValue(PollinatorUtils.PollenGold.ToVector3());
                pollenShader.Parameters["uSecondaryColor"]?.SetValue(PollinatorUtils.LeafGreen.ToVector3());
                pollenShader.Parameters["uOpacity"]?.SetValue(0.45f * alphaFade);
                pollenShader.Parameters["uIntensity"]?.SetValue(1.0f);
                pollenShader.CurrentTechnique = pollenShader.Techniques["PollenTrailTechnique"];
                pollenShader.CurrentTechnique.Passes[0].Apply();

                sb.Draw(softBloom, Projectile.Center - Main.screenPosition, null, Color.White,
                    Projectile.rotation, sOrigin, 0.6f * pulse, SpriteEffects.None, 0f);
            }

            // Outer green-gold glow
            Color outerColor = PollinatorUtils.Additive(PollinatorUtils.LeafGreen, 0.4f * alphaFade);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, outerColor, Projectile.rotation, origin,
                0.4f * pulse, SpriteEffects.None, 0f);

            // Core pollen gold
            Color coreColor = PollinatorUtils.Additive(PollinatorUtils.PollenGold, 0.65f * alphaFade);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, coreColor, Projectile.rotation, origin,
                0.22f * pulse, SpriteEffects.None, 0f);

            // Inner white hot center
            Color whiteCore = PollinatorUtils.Additive(PollinatorUtils.PureLight, 0.4f * alphaFade);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, whiteCore, Projectile.rotation, origin,
                0.1f * pulse, SpriteEffects.None, 0f);

            sb.End();
            PollinatorUtils.BeginDefault(sb);

            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PollenBurstProjectile — larger golden orb, explodes into 6 homing petals
    // 24x24, pen 1, timeLeft 180, no homing
    // On hit OR Kill(): 6 radial HomingPetal + particle shower
    // ═══════════════════════════════════════════════════════════
    public class PollenBurstProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;
        private static Asset<Texture2D> _softBloomTex;
        private bool hasExploded = false;

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.alpha = Math.Max(Projectile.alpha - 20, 0);
            Projectile.rotation += 0.08f;

            // Growing pulse
            Projectile.ai[0]++;

            // Trail particles
            if (!Main.dedServ && (int)Projectile.ai[0] % 2 == 0)
            {
                var trail = new SeedTrailParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.8f, 0.8f),
                    Color.Lerp(PollinatorUtils.PollenGold, PollinatorUtils.SunGold, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.2f, 0.4f),
                    Main.rand.Next(12, 22));
                PollinatorParticleHandler.SpawnParticle(trail);

                // Occasional pollen dust
                if (Main.rand.NextBool(3))
                {
                    var dust = new PollenDustParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                        Main.rand.NextVector2Circular(1f, 1f),
                        Main.rand.NextFloat(0.15f, 0.25f),
                        Main.rand.Next(15, 25));
                    PollinatorParticleHandler.SpawnParticle(dust);
                }
            }

            Lighting.AddLight(Projectile.Center, 0.4f, 0.35f, 0.1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 180);
            TriggerExplosion(target.Center);
        }

        public override void OnKill(int timeLeft)
        {
            TriggerExplosion(Projectile.Center);
        }

        private void TriggerExplosion(Vector2 center)
        {
            if (hasExploded)
                return;
            hasExploded = true;

            // Spawn 6 radial homing petals
            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi / 6f * i + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 petalVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                    int dmg = Math.Max(Projectile.damage / 4, 1);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), center, petalVel,
                        ModContent.ProjectileType<HomingPetalProjectile>(), dmg, Projectile.knockBack * 0.5f, Projectile.owner);
                }
            }

            // Massive particle shower
            if (!Main.dedServ)
            {
                // Pollen dust burst
                for (int i = 0; i < 15; i++)
                {
                    var dust = new PollenDustParticle(
                        center + Main.rand.NextVector2Circular(20f, 20f),
                        Main.rand.NextVector2Circular(4f, 4f),
                        Main.rand.NextFloat(0.25f, 0.5f),
                        Main.rand.Next(20, 40));
                    PollinatorParticleHandler.SpawnParticle(dust);
                }

                // Petal flutter burst
                for (int i = 0; i < 10; i++)
                {
                    var petal = new PetalFlutterParticle(
                        center + Main.rand.NextVector2Circular(15f, 15f),
                        Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0f, -1f),
                        Main.rand.NextFloat(0.3f, 0.55f),
                        Main.rand.Next(35, 60));
                    PollinatorParticleHandler.SpawnParticle(petal);
                }

                // Central golden bloom flash
                var bloom = new MuzzleBloomParticle(
                    center,
                    Vector2.Zero,
                    1.2f,
                    18);
                PollinatorParticleHandler.SpawnParticle(bloom);

                // Music notes celebrating the burst
                for (int i = 0; i < 3; i++)
                {
                    var note = new HarvestNoteParticle(
                        center + Main.rand.NextVector2Circular(15f, 15f),
                        new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-3f, -1f)),
                        Main.rand.NextFloat(0.3f, 0.5f),
                        Main.rand.Next(40, 60));
                    PollinatorParticleHandler.SpawnParticle(note);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            _bloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _softBloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _bloomTex.Value;
            Texture2D softBloom = _softBloomTex.Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 sOrigin = softBloom.Size() / 2f;
            SpriteBatch sb = Main.spriteBatch;
            float time = (float)Main.GameUpdateCount / 60f;

            float lifeProgress = Projectile.ai[0] / 180f;
            float growPulse = 1f + lifeProgress * 0.4f + (float)Math.Sin(Projectile.ai[0] * 0.25f) * 0.2f;
            float alphaFade = 1f - (Projectile.alpha / 255f);

            sb.End();

            // ═══ Layer 1: GardenBloom shader — growing floral body with petal structure ═══
            Effect gardenShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyGardenBloomShader);

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (gardenShader != null)
            {
                gardenShader.Parameters["uTime"]?.SetValue(time);
                gardenShader.Parameters["uColor"]?.SetValue(PollinatorUtils.SunGold.ToVector3());
                gardenShader.Parameters["uSecondaryColor"]?.SetValue(PollinatorUtils.PollenGold.ToVector3());
                gardenShader.Parameters["uOpacity"]?.SetValue(0.5f * alphaFade);
                gardenShader.Parameters["uIntensity"]?.SetValue(1.3f + lifeProgress * 0.5f);
                gardenShader.Parameters["uRadius"]?.SetValue(0.35f);
                gardenShader.Parameters["uPulseSpeed"]?.SetValue(2.0f);
                gardenShader.CurrentTechnique = gardenShader.Techniques["GardenBloomTechnique"];
                gardenShader.CurrentTechnique.Passes[0].Apply();

                sb.Draw(softBloom, Projectile.Center - Main.screenPosition, null, Color.White,
                    Projectile.rotation, sOrigin, 0.8f * growPulse, SpriteEffects.None, 0f);
            }

            // Outer golden glow — grows over time
            Color outerColor = PollinatorUtils.Additive(PollinatorUtils.SunGold, 0.45f * alphaFade);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, outerColor, Projectile.rotation, origin,
                0.55f * growPulse, SpriteEffects.None, 0f);

            // Middle pollen gold
            Color midColor = PollinatorUtils.Additive(PollinatorUtils.PollenGold, 0.6f * alphaFade);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, midColor, Projectile.rotation, origin,
                0.3f * growPulse, SpriteEffects.None, 0f);

            // Core white center
            Color coreColor = PollinatorUtils.Additive(PollinatorUtils.PureLight, 0.5f * alphaFade);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, coreColor, Projectile.rotation, origin,
                0.15f * growPulse, SpriteEffects.None, 0f);

            sb.End();
            PollinatorUtils.BeginDefault(sb);

            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // HomingPetalProjectile — small rose petal that homes aggressively
    // 10x10, pen 1, timeLeft 120, 0.08 lerp homing, 1/4 weapon damage
    // Applies Poisoned 60, pink-rose glow with fading petal trail
    // ═══════════════════════════════════════════════════════════
    public class HomingPetalProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;
        private static Asset<Texture2D> _softBloomTex;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 200;
        }

        public override void AI()
        {
            Projectile.alpha = Math.Max(Projectile.alpha - 30, 0);

            // Aggressive homing
            NPC target = PollinatorUtils.ClosestNPC(Projectile.Center, 800f);
            if (target != null)
            {
                Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.08f);
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.ai[0]++;

            // Fading petal trail
            if (!Main.dedServ && (int)Projectile.ai[0] % 3 == 0)
            {
                var trail = new PetalFlutterParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    Main.rand.NextFloat(0.1f, 0.2f),
                    Main.rand.Next(15, 25));
                PollinatorParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(Projectile.Center, 0.15f, 0.08f, 0.1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 60);

            // Hit VFX
            if (!Main.dedServ)
            {
                for (int i = 0; i < 3; i++)
                {
                    var dust = new PollenDustParticle(
                        target.Center + Main.rand.NextVector2Circular(8f, 8f),
                        Main.rand.NextVector2Circular(1.5f, 1.5f),
                        Main.rand.NextFloat(0.1f, 0.25f),
                        Main.rand.Next(10, 18));
                    PollinatorParticleHandler.SpawnParticle(dust);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (!Main.dedServ)
            {
                for (int i = 0; i < 2; i++)
                {
                    var petal = new PetalFlutterParticle(
                        Projectile.Center,
                        Main.rand.NextVector2Circular(1.5f, 1.5f),
                        Main.rand.NextFloat(0.15f, 0.25f),
                        Main.rand.Next(15, 25));
                    PollinatorParticleHandler.SpawnParticle(petal);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            _bloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _softBloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _bloomTex.Value;
            Texture2D softBloom = _softBloomTex.Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 sOrigin = softBloom.Size() / 2f;
            SpriteBatch sb = Main.spriteBatch;
            float time = (float)Main.GameUpdateCount / 60f;

            float pulse = 1f + (float)Math.Sin(Projectile.ai[0] * 0.3f) * 0.1f;
            float alphaFade = 1f - (Projectile.alpha / 255f);
            float lifeFade = Projectile.timeLeft / 120f;

            sb.End();

            // ═══ Layer 1: GardenBloom JubilantPulse — petal-shaped pulsing glow ═══
            Effect gardenShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyGardenBloomShader);

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (gardenShader != null)
            {
                gardenShader.Parameters["uTime"]?.SetValue(time);
                gardenShader.Parameters["uColor"]?.SetValue(PollinatorUtils.RoseBlush.ToVector3());
                gardenShader.Parameters["uSecondaryColor"]?.SetValue(PollinatorUtils.PollenGold.ToVector3());
                gardenShader.Parameters["uOpacity"]?.SetValue(0.4f * alphaFade * lifeFade);
                gardenShader.Parameters["uIntensity"]?.SetValue(1.0f);
                gardenShader.Parameters["uRadius"]?.SetValue(0.3f);
                gardenShader.Parameters["uPulseSpeed"]?.SetValue(3.0f);
                gardenShader.CurrentTechnique = gardenShader.Techniques["JubilantPulseTechnique"];
                gardenShader.CurrentTechnique.Passes[0].Apply();

                sb.Draw(softBloom, Projectile.Center - Main.screenPosition, null, Color.White,
                    Projectile.rotation, sOrigin, 0.4f * pulse, SpriteEffects.None, 0f);
            }

            // Outer rose glow
            Color outerColor = PollinatorUtils.Additive(PollinatorUtils.RoseBlush, 0.4f * alphaFade * lifeFade);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, outerColor, Projectile.rotation, origin,
                0.25f * pulse, SpriteEffects.None, 0f);

            // Core pink-white
            Color coreColor = PollinatorUtils.Additive(
                Color.Lerp(PollinatorUtils.RoseBlush, PollinatorUtils.PureLight, 0.4f),
                0.55f * alphaFade * lifeFade);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, coreColor, Projectile.rotation, origin,
                0.12f * pulse, SpriteEffects.None, 0f);

            sb.End();
            PollinatorUtils.BeginDefault(sb);

            return false;
        }
    }
}
