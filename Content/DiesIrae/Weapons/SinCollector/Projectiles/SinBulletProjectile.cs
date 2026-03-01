using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.SinCollector.Utilities;
using MagnumOpus.Content.DiesIrae.Weapons.SinCollector.Particles;

namespace MagnumOpus.Content.DiesIrae.Weapons.SinCollector.Projectiles
{
    /// <summary>
    /// Sin Bullet — A high-velocity sin-seeking round that chains lightning to 3 nearby enemies on hit.
    /// Leaves a stretched ember trail, explodes with a crimson bloom.
    /// </summary>
    public class SinBulletProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> bloomTexture;
        private const int TrailLength = 8;
        private Vector2[] trailCache = new Vector2[TrailLength];

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // Update trail
            for (int i = TrailLength - 1; i > 0; i--) trailCache[i] = trailCache[i - 1];
            trailCache[0] = Projectile.Center;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail particles
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color c = SinUtils.MulticolorLerp(Main.rand.NextFloat(), SinUtils.SinCrimson, SinUtils.TrackingEmber);
                SinParticleHandler.Spawn(new SinBulletTrailParticle(Projectile.Center, vel, c, 0.15f, 10));
            }

            Lighting.AddLight(Projectile.Center, SinUtils.TrackingEmber.ToVector3() * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);

            // Chain lightning to 3 nearby enemies
            ChainLightning(target);

            // Impact VFX
            SinParticleHandler.Spawn(new SinImpactBloomParticle(target.Center, SinUtils.TrackingEmber, 1.5f, 15));
            SinParticleHandler.Spawn(new SinImpactBloomParticle(target.Center, SinUtils.WhiteFlash, 0.5f, 10));

            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                SinParticleHandler.Spawn(new SinBulletTrailParticle(target.Center, vel,
                    SinUtils.GetSinColor(Main.rand.NextFloat()), 0.2f, 15));
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                SinParticleHandler.Spawn(new SinSmokeParticle(target.Center, vel, 0.4f, 25));
            }

            if (Main.rand.NextBool(2))
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), -1.5f);
                SinParticleHandler.Spawn(new SinNoteParticle(target.Center, vel, SinUtils.MuzzleGold, 0.4f, 35));
            }
        }

        private void ChainLightning(NPC source)
        {
            int chains = 0;
            int maxChains = 3;
            float chainRange = 400f;

            for (int i = 0; i < Main.maxNPCs && chains < maxChains; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || i == source.whoAmI) continue;
                if (Vector2.Distance(source.Center, npc.Center) > chainRange) continue;

                // Deal chain damage
                NPC.HitInfo chainHit = new NPC.HitInfo()
                {
                    Damage = Projectile.damage / 3,
                    Knockback = 2f,
                    HitDirection = source.Center.X < npc.Center.X ? 1 : -1,
                    DamageType = DamageClass.Ranged,
                    Crit = false
                };
                npc.StrikeNPC(chainHit);
                npc.AddBuff(BuffID.OnFire3, 120);

                // Lightning VFX between source and target
                SpawnLightningVFX(source.Center, npc.Center);
                chains++;
            }
        }

        private void SpawnLightningVFX(Vector2 from, Vector2 to)
        {
            // Spawn particles along the lightning path
            int segments = 8;
            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)segments;
                Vector2 pos = Vector2.Lerp(from, to, t);
                Vector2 offset = Main.rand.NextVector2Circular(8f, 8f);
                Color c = SinUtils.MulticolorLerp(t, SinUtils.SinCrimson, SinUtils.MuzzleGold, SinUtils.WhiteFlash);
                SinParticleHandler.Spawn(new SinBulletTrailParticle(pos + offset,
                    Main.rand.NextVector2Circular(1f, 1f), c, 0.25f, 10));
            }

            SinParticleHandler.Spawn(new SinImpactBloomParticle(to, SinUtils.TrackingEmber, 0.8f, 10));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            bloomTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!bloomTexture.IsLoaded) return false;
            var tex = bloomTexture.Value;

            // Trail — dual-layer with outer crimson and inner gold for tracer depth
            for (int i = 1; i < TrailLength; i++)
            {
                if (trailCache[i] == Vector2.Zero) continue;
                float p = i / (float)TrailLength;
                float alpha = (1f - p) * 0.5f;

                // Outer crimson layer — wider for ambient glow
                Color outerColor = SinUtils.GetSinColor(p * 0.6f);
                Main.EntitySpriteDraw(tex, trailCache[i] - Main.screenPosition, null, SinUtils.Additive(outerColor, alpha * 0.7f),
                    Projectile.rotation, tex.Size() / 2f, new Vector2((1f - p) * 0.6f, (1f - p) * 0.2f), SpriteEffects.None, 0);

                // Inner gold hot tracer — tighter, brighter
                Color innerColor = Color.Lerp(SinUtils.MuzzleGold, SinUtils.WhiteFlash, (1f - p) * 0.5f);
                Main.EntitySpriteDraw(tex, trailCache[i] - Main.screenPosition, null, SinUtils.Additive(innerColor, alpha * 0.4f),
                    Projectile.rotation, tex.Size() / 2f, new Vector2((1f - p) * 0.35f, (1f - p) * 0.08f), SpriteEffects.None, 0);
            }

            // Core — outer ember glow
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                SinUtils.Additive(SinUtils.TrackingEmber, 0.7f), Projectile.rotation, tex.Size() / 2f,
                new Vector2(0.6f, 0.2f), SpriteEffects.None, 0);
            // Core — hot white center
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                SinUtils.Additive(SinUtils.WhiteFlash, 0.4f), Projectile.rotation, tex.Size() / 2f,
                new Vector2(0.3f, 0.1f), SpriteEffects.None, 0);

            // Muzzle cross-flare at bullet tip — gives the round a distinctive sharp flash
            float crossAlpha = 0.3f + 0.1f * (float)Math.Sin(Main.GameUpdateCount * 0.4f);
            Color crossColor = SinUtils.Additive(SinUtils.MuzzleGold, crossAlpha);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, crossColor,
                Projectile.rotation, tex.Size() / 2f, new Vector2(0.08f, 0.8f), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, crossColor,
                Projectile.rotation + MathHelper.PiOver2, tex.Size() / 2f, new Vector2(0.08f, 0.45f), SpriteEffects.None, 0);

            // Leading velocity line — thin bright tracer extending ahead
            Vector2 ahead = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 12f;
            Main.EntitySpriteDraw(tex, ahead - Main.screenPosition, null,
                SinUtils.Additive(SinUtils.WhiteFlash, 0.2f), Projectile.rotation, tex.Size() / 2f,
                new Vector2(0.5f, 0.04f), SpriteEffects.None, 0);

            return false;
        }
    }

    /// <summary>
    /// Spinning Cleaver Copy — Phantom cleaver spawned every 5th shot.
    /// Spins rapidly, homing slightly, deals area damage on death.
    /// </summary>
    public class SpinningCleaverCopyProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> bloomTexture;

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            Projectile.rotation += 0.25f;

            // Gentle homing
            NPC target = null;
            float closest = 500f * 500f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float d = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                if (d < closest) { closest = d; target = npc; }
            }

            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 12f, 0.04f);
            }

            // Trail
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = -Projectile.velocity * 0.05f;
                SinParticleHandler.Spawn(new SinBulletTrailParticle(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    vel, SinUtils.SinCrimson, 0.15f, 10));
            }

            Lighting.AddLight(Projectile.Center, SinUtils.SinCrimson.ToVector3() * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 120);
            SinParticleHandler.Spawn(new SinImpactBloomParticle(target.Center, SinUtils.SinCrimson, 1f, 12));

            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                SinParticleHandler.Spawn(new SinBulletTrailParticle(target.Center, vel, SinUtils.TrackingEmber, 0.18f, 12));
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Death explosion
            SinParticleHandler.Spawn(new SinImpactBloomParticle(Projectile.Center, SinUtils.TrackingEmber, 1.2f, 15));
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                SinParticleHandler.Spawn(new SinBulletTrailParticle(Projectile.Center, vel, SinUtils.MuzzleGold, 0.2f, 15));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float alpha = Projectile.timeLeft < 20 ? Projectile.timeLeft / 20f : 1f;

            // Glow
            bloomTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (bloomTexture.IsLoaded)
            {
                var glow = bloomTexture.Value;
                Main.EntitySpriteDraw(glow, drawPos, null, SinUtils.Additive(SinUtils.SinCrimson, 0.3f * alpha),
                    0f, glow.Size() / 2f, 0.8f, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(tex, drawPos, null, Color.White * alpha, Projectile.rotation,
                tex.Size() / 2f, 0.8f, SpriteEffects.None, 0);

            return false;
        }
    }
}
