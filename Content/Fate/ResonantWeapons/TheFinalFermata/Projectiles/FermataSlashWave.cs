using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.Debuffs;
using MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Primitives;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Projectiles
{
    /// <summary>
    /// FermataSlashWave — Short-lived slash projectile created during the
    /// synchronized 90-frame slash attack. Travels fast in a straight line,
    /// deals damage, and leaves a crimson-gold trail.
    /// </summary>
    public class FermataSlashWave : ModProjectile
    {
        // Use the item texture (staff) as a stand-in for the slash wave
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/TheFinalFermata";

        private FermataTrailRenderer _trail;
        private int _frameCounter;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 45; // Short-lived
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 80;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            _frameCounter++;

            // Initialize trail
            if (_trail == null)
            {
                _trail = new FermataTrailRenderer(FermataTrailSettings.SlashTrail());
                _trail.Reset(Projectile.Center);
            }

            // Record trail
            _trail.RecordPosition(Projectile.Center, Projectile.rotation);

            // Rotation follows velocity
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Slight homing toward nearest enemy
            NPC target = FindNearestEnemy(400f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center);
                if (toTarget != Vector2.Zero)
                {
                    toTarget.Normalize();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.04f);
                }
            }

            // Fade out near end of life
            if (Projectile.timeLeft < 15)
            {
                Projectile.alpha = (int)MathHelper.Lerp(80, 255, 1f - Projectile.timeLeft / 15f);
            }

            // === VFX ===
            if (!Main.dedServ)
            {
                // Trailing sparks
                if (_frameCounter % 2 == 0)
                {
                    Vector2 backDir = -Projectile.velocity.SafeNormalize(Vector2.UnitX);
                    FermataParticleTypes.SpawnSpark(
                        Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                        backDir * Main.rand.NextFloat(1f, 3f),
                        Color.Lerp(FermataUtils.FermataCrimson, FermataUtils.TimeGold, Main.rand.NextFloat()),
                        0.14f, 10);
                }

                // Nebula wisps
                if (_frameCounter % 3 == 0)
                {
                    FermataParticleTypes.SpawnNebulaWisp(
                        Projectile.Center,
                        -Projectile.velocity * 0.05f,
                        FermataUtils.PaletteLerp(Main.rand.NextFloat(0.2f, 0.6f)) * 0.6f,
                        0.18f, 20);
                }

                // Vanilla dust
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    DustID.PurpleTorch,
                    -Projectile.velocity * 0.1f, 0,
                    FermataUtils.FermataCrimson * 0.7f, 1f);
                d.noGravity = true;

                Lighting.AddLight(Projectile.Center,
                    FermataUtils.FermataCrimson.ToVector3() * 0.35f);
            }
        }

        private NPC FindNearestEnemy(float maxDist)
        {
            NPC closest = null;
            float closestDist = maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);

            // Impact VFX
            FermataParticleTypes.SyncSlashImpact(target.Center);

            SoundEngine.PlaySound(SoundID.Item60 with { Pitch = 0.4f, Volume = 0.5f }, target.Center);
        }

        public override void OnKill(int timeLeft)
        {
            // Terminal burst
            FermataParticleTypes.SpawnSparkBurst(Projectile.Center, 6, 3f, FermataUtils.FermataCrimson);
            FermataParticleTypes.SpawnBloomFlare(Projectile.Center, FermataUtils.TimeGold, 0.3f, 12);

            for (int i = 0; i < 3; i++)
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(2f, 2f), 0,
                    FermataUtils.FermataCrimson * 0.5f, 1f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float alpha = 1f - Projectile.alpha / 255f;

            // Draw trail
            _trail?.Draw(sb, alpha);

            // Additive glow layers
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            float pulse = 1f + MathF.Sin(Main.GameUpdateCount * 0.2f) * 0.08f;

            sb.Draw(texture, drawPos, null,
                FermataUtils.FermataCrimson * 0.3f * alpha,
                Projectile.rotation, origin, 1.2f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, drawPos, null,
                FermataUtils.TimeGold * 0.2f * alpha,
                Projectile.rotation, origin, 1.1f * pulse, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Main sprite
            sb.Draw(texture, drawPos, null,
                Color.White * alpha * 0.85f,
                Projectile.rotation, origin, 0.7f, SpriteEffects.None, 0f);

            return false;
        }
    }
}
