using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon.Projectiles
{
    // ═══════════════════════════════════════════════════════════
    // PetalBombProjectile — golden-amber orb with growing pulse, arcs with gravity
    // 30x30, pen 1, timeLeft 300, tileCollide true
    // On impact: spawns PetalStormZone + 8 radial PetalShrapnelProjectile
    // ═══════════════════════════════════════════════════════════
    public class PetalBombProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;
        private static Asset<Texture2D> _softBloomTex;

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 255;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void AI()
        {
            Projectile.alpha = Math.Max(Projectile.alpha - 20, 0);
            Projectile.ai[0]++;

            // Gravity arc
            Projectile.velocity.Y += 0.12f;

            Projectile.rotation += 0.06f;

            // Trail particles — cannon smoke puffs
            if (!Main.dedServ && (int)Projectile.ai[0] % 3 == 0)
            {
                var smoke = new CannonSmokeParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.6f, 0.6f),
                    Main.rand.NextFloat(0.25f, 0.45f),
                    Main.rand.Next(20, 40));
                PetalStormParticleHandler.SpawnParticle(smoke);
            }

            Lighting.AddLight(Projectile.Center, 0.5f, 0.4f, 0.08f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Explode();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Explode();
            return true; // kill projectile
        }

        private void Explode()
        {
            if (Projectile.owner == Main.myPlayer)
            {
                // Spawn lingering AoE petal storm zone — 1/5 base weapon damage
                int zoneDmg = Math.Max(Projectile.damage / 5, 1);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<PetalStormZone>(), zoneDmg, 0f, Projectile.owner);

                // 8 radial shrapnel petals — 1/3 base damage
                int shrapDmg = Math.Max(Projectile.damage / 3, 1);
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi / 8f * i + Main.rand.NextFloat(-0.15f, 0.15f);
                    Vector2 shrapVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shrapVel,
                        ModContent.ProjectileType<PetalShrapnelProjectile>(), shrapDmg, Projectile.knockBack * 0.3f, Projectile.owner);
                }
            }

            // Explosion VFX
            if (!Main.dedServ)
            {
                // Big golden bloom flash
                var bloom = new ExplosionBloomParticle(
                    Projectile.Center,
                    Vector2.Zero,
                    2.5f,
                    18);
                PetalStormParticleHandler.SpawnParticle(bloom);

                // Radial smoke burst
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi / 12f * i;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                    var smoke = new CannonSmokeParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                        vel,
                        Main.rand.NextFloat(0.35f, 0.6f),
                        Main.rand.Next(30, 55));
                    PetalStormParticleHandler.SpawnParticle(smoke);
                }

                // Scattered petals
                for (int i = 0; i < 8; i++)
                {
                    var petal = new StormPetalParticle(
                        Projectile.Center,
                        Main.rand.NextFloat(20f, 50f),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        Main.rand.NextFloat(0.3f, 0.55f),
                        Main.rand.Next(40, 70));
                    PetalStormParticleHandler.SpawnParticle(petal);
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

            float pulse = 1f + (float)Math.Sin(Projectile.ai[0] * 0.15f) * 0.2f;
            float alphaFade = 1f - (Projectile.alpha / 255f);
            float lifeProgress = Math.Min(Projectile.ai[0] / 120f, 1f);

            sb.End();

            // ═══ Layer 1: PollenDrift BloomDetonation — growing volatile glow as bomb ages ═══
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Effect pollenShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyPollenDriftShader);
            if (pollenShader != null)
            {
                pollenShader.Parameters["uTime"]?.SetValue(time);
                pollenShader.Parameters["uColor"]?.SetValue(PetalStormUtils.AmberFlame.ToVector3());
                pollenShader.Parameters["uSecondaryColor"]?.SetValue(PetalStormUtils.GoldenExplosion.ToVector3());
                pollenShader.Parameters["uOpacity"]?.SetValue(alphaFade * (0.3f + lifeProgress * 0.4f));
                pollenShader.Parameters["uIntensity"]?.SetValue(1.0f + lifeProgress * 1.2f);
                pollenShader.Parameters["uRadius"]?.SetValue(0.4f);
                pollenShader.Parameters["uWindSpeed"]?.SetValue(0.6f + lifeProgress * 0.8f);
                pollenShader.CurrentTechnique = pollenShader.Techniques["BloomDetonationTechnique"];
                pollenShader.CurrentTechnique.Passes[0].Apply();

                float shaderScale = 0.6f * pulse * (1f + lifeProgress * 0.5f);
                sb.Draw(softBloom, Projectile.Center - Main.screenPosition, null, Color.White,
                    0f, sOrigin, shaderScale, SpriteEffects.None, 0f);
            }

            sb.End();

            // ═══ Layer 2: Additive trail + bloom body ═══
            PetalStormUtils.BeginAdditive(sb);

            // 8-position old position trail
            for (int i = Projectile.oldPos.Length - 1; i >= 1; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailFade = (1f - (float)i / Projectile.oldPos.Length) * alphaFade;
                Color trailCol = PetalStormUtils.Additive(PetalStormUtils.AmberFlame, trailFade * 0.4f);
                sb.Draw(tex, trailPos, null, trailCol, Projectile.oldRot[i], origin,
                    0.3f * pulse * (1f - (float)i / Projectile.oldPos.Length), SpriteEffects.None, 0f);
            }

            // Outer amber glow
            Color outerColor = PetalStormUtils.Additive(PetalStormUtils.AmberFlame, 0.55f * alphaFade);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, outerColor, Projectile.rotation, origin,
                0.55f * pulse, SpriteEffects.None, 0f);

            // Core golden explosion glow
            Color coreColor = PetalStormUtils.Additive(PetalStormUtils.GoldenExplosion, 0.7f * alphaFade);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, coreColor, Projectile.rotation, origin,
                0.3f * pulse, SpriteEffects.None, 0f);

            // Inner white-hot center
            Color whiteCore = PetalStormUtils.Additive(PetalStormUtils.WhiteFlash, 0.45f * alphaFade);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, whiteCore, Projectile.rotation, origin,
                0.15f * pulse, SpriteEffects.None, 0f);

            sb.End();
            PetalStormUtils.BeginDefault(sb);

            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PetalStormZone — lingering AoE, invisible projectile, creates petal vortex
    // 200x200, pen -1, tileCollide false, timeLeft 300 (5 seconds)
    // Damages each NPC every 15 frames via localNPCImmunity
    // Applies Poisoned 120 + Venom 60
    // ═══════════════════════════════════════════════════════════
    public class PetalStormZone : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 200;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            float life = Projectile.ai[0];
            float maxLife = 300f;
            float lifeRatio = life / maxLife;

            // Visual radius: grows for first 20%, full for mid, shrinks last 20%
            float visualRadius;
            if (lifeRatio < 0.2f)
                visualRadius = MathHelper.Lerp(20f, 100f, lifeRatio / 0.2f);
            else if (lifeRatio > 0.8f)
                visualRadius = MathHelper.Lerp(100f, 20f, (lifeRatio - 0.8f) / 0.2f);
            else
                visualRadius = 100f;

            // Spawn storm particles every frame
            if (!Main.dedServ)
            {
                // 2-3 storm petals per frame during main phase
                int petalCount = lifeRatio < 0.15f || lifeRatio > 0.85f ? 1 : Main.rand.Next(2, 4);
                for (int i = 0; i < petalCount; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float radius = Main.rand.NextFloat(visualRadius * 0.3f, visualRadius);
                    var petal = new StormPetalParticle(
                        Projectile.Center,
                        radius,
                        angle,
                        Main.rand.NextFloat(0.2f, 0.45f),
                        Main.rand.Next(30, 60));
                    PetalStormParticleHandler.SpawnParticle(petal);
                }

                // Occasional vortex music notes (every ~5 frames)
                if (Main.rand.NextBool(5))
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float radius = Main.rand.NextFloat(visualRadius * 0.4f, visualRadius * 0.8f);
                    var note = new VortexNoteParticle(
                        Projectile.Center,
                        radius,
                        angle,
                        Main.rand.NextFloat(0.15f, 0.3f),
                        Main.rand.Next(40, 70));
                    PetalStormParticleHandler.SpawnParticle(note);
                }

                // Ambient golden glow light
                float lightIntensity = lifeRatio < 0.2f ? lifeRatio / 0.2f :
                    lifeRatio > 0.8f ? 1f - (lifeRatio - 0.8f) / 0.2f : 1f;
                Lighting.AddLight(Projectile.Center, 0.6f * lightIntensity, 0.5f * lightIntensity, 0.1f * lightIntensity);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 120);
            target.AddBuff(BuffID.Venom, 60);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Zone is visually invisible — all VFX comes from particles
            return false;
        }

        public override bool? CanDamage()
        {
            // Only damage if still alive
            return Projectile.timeLeft > 0 ? null : false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PetalShrapnelProjectile — small green glow, mild homing, Poisoned 60
    // 12x12, pen 2, timeLeft 120, homing lerp 0.04
    // ═══════════════════════════════════════════════════════════
    public class PetalShrapnelProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;
        private static Asset<Texture2D> _softBloomTex;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 200;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.alpha = Math.Max(Projectile.alpha - 20, 0);
            Projectile.ai[0]++;

            // Mild homing toward closest NPC
            NPC target = PetalStormUtils.ClosestNPC(Projectile.Center, 500f);
            if (target != null)
            {
                Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.04f);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Spawn trail particles
            if (!Main.dedServ && (int)Projectile.ai[0] % 2 == 0)
            {
                var trail = new ShrapnelTrailParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(0.4f, 0.4f),
                    Color.Lerp(PetalStormUtils.GunmetalGreen, PetalStormUtils.AmberFlame, Main.rand.NextFloat(0.5f)),
                    Main.rand.NextFloat(0.12f, 0.25f),
                    Main.rand.Next(8, 16));
                PetalStormParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(Projectile.Center, 0.15f, 0.2f, 0.04f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 60);

            // Small impact VFX
            if (!Main.dedServ)
            {
                for (int i = 0; i < 3; i++)
                {
                    var bloom = new ExplosionBloomParticle(
                        target.Center + Main.rand.NextVector2Circular(8f, 8f),
                        Main.rand.NextVector2Circular(1f, 1f),
                        0.3f,
                        8);
                    PetalStormParticleHandler.SpawnParticle(bloom);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (!Main.dedServ)
            {
                for (int i = 0; i < 2; i++)
                {
                    var bloom = new ExplosionBloomParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                        Main.rand.NextVector2Circular(0.5f, 0.5f),
                        0.2f,
                        6);
                    PetalStormParticleHandler.SpawnParticle(bloom);
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

            float pulse = 1f + (float)Math.Sin(Projectile.ai[0] * 0.25f) * 0.12f;
            float alphaFade = 1f - (Projectile.alpha / 255f);

            sb.End();

            // ═══ Layer 1: PollenDrift PollenTrail — organic drifting wake behind shrapnel ═══
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Effect pollenShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyPollenDriftShader);
            if (pollenShader != null)
            {
                pollenShader.Parameters["uTime"]?.SetValue(time);
                pollenShader.Parameters["uColor"]?.SetValue(PetalStormUtils.GunmetalGreen.ToVector3());
                pollenShader.Parameters["uSecondaryColor"]?.SetValue(PetalStormUtils.AmberFlame.ToVector3());
                pollenShader.Parameters["uOpacity"]?.SetValue(alphaFade * 0.35f);
                pollenShader.Parameters["uIntensity"]?.SetValue(0.8f);
                pollenShader.Parameters["uRadius"]?.SetValue(0.25f);
                pollenShader.Parameters["uWindSpeed"]?.SetValue(1.2f);
                pollenShader.CurrentTechnique = pollenShader.Techniques["PollenTrailTechnique"];
                pollenShader.CurrentTechnique.Passes[0].Apply();

                sb.Draw(softBloom, Projectile.Center - Main.screenPosition, null, Color.White,
                    Projectile.rotation, sOrigin, 0.35f * pulse, SpriteEffects.None, 0f);
            }

            sb.End();

            // ═══ Layer 2: Additive bloom body ═══
            PetalStormUtils.BeginAdditive(sb);

            // Outer green glow
            Color outerColor = PetalStormUtils.Additive(
                Color.Lerp(PetalStormUtils.GunmetalGreen, PetalStormUtils.AmberFlame, 0.3f), 0.5f * alphaFade);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, outerColor, Projectile.rotation, origin,
                0.3f * pulse, SpriteEffects.None, 0f);

            // Core golden center
            Color coreColor = PetalStormUtils.Additive(PetalStormUtils.GoldenExplosion, 0.6f * alphaFade);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, coreColor, Projectile.rotation, origin,
                0.15f * pulse, SpriteEffects.None, 0f);

            sb.End();
            PetalStormUtils.BeginDefault(sb);

            return false;
        }
    }
}
