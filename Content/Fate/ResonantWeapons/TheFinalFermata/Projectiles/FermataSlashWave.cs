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
using MagnumOpus.Common.Systems;
using ReLogic.Content;

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

        /// <summary>Additive-friendly color with premultiplied alpha and zero alpha channel.</summary>
        private static Color Additive(Color c, float opacity)
            => new Color((int)(c.R * opacity), (int)(c.G * opacity), (int)(c.B * opacity), 0);

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

            if (Main.dedServ) return;

            Vector2 hitPos = target.Center;

            // ═══ ENHANCED PARTICLE IMPACT ═══
            FermataParticleTypes.SyncSlashImpact(hitPos);

            // 12 radial crimson-gold sparks
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color sparkCol = FermataUtils.PaletteLerp((float)i / 12f);
                FermataParticleTypes.SpawnSpark(hitPos, sparkVel, sparkCol * 0.8f, 0.14f, 14);
            }

            // 6 directional slash marks
            Vector2 slashDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 slashPerp = new Vector2(-slashDir.Y, slashDir.X);
            for (int i = 0; i < 6; i++)
            {
                float spread = (i - 2.5f) / 2.5f;
                Vector2 dirVel = (slashDir * 4f + slashPerp * spread * 5f) * Main.rand.NextFloat(0.8f, 1.2f);
                Color col = Color.Lerp(FermataUtils.FermataCrimson, FermataUtils.FlashWhite, MathF.Abs(spread));
                FermataParticleTypes.SpawnSpark(hitPos, dirVel, col * 0.7f, 0.1f, 12);
            }

            // Time shard burst with glyphs
            FermataParticleTypes.SpawnTimeShardBurst(hitPos, 6, 4f);
            FermataParticleTypes.SpawnGlyph(hitPos, FermataUtils.TimeGold * 0.6f, 0.3f, 24);

            // Dual lighting
            Lighting.AddLight(hitPos, FermataUtils.FermataCrimson.ToVector3() * 0.9f);
            Lighting.AddLight(hitPos + slashDir * 16f, FermataUtils.TimeGold.ToVector3() * 0.6f);

            SoundEngine.PlaySound(SoundID.Item60 with { Pitch = 0.4f, Volume = 0.5f }, hitPos);
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
            try
            {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float alpha = 1f - Projectile.alpha / 255f;

            // Draw trail
            _trail?.Draw(sb, alpha);

            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.2f) * 0.08f;
            float breathe = 1f + MathF.Sin(time * 0.06f) * 0.05f;

            // ═══ FOUNDATION-TIER GRADUATED BLOOM ═══
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Graduated orb bloom head enhancement
            MagnumVFX.DrawGraduatedOrbHead(sb, drawPos, FermataUtils.FermataCrimson, FermataUtils.FermataPurple, 0.8f, alpha);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Main sprite
            sb.Draw(texture, drawPos, null,
                Color.White * alpha * 0.85f,
                Projectile.rotation, origin, 0.7f, SpriteEffects.None, 0f);

            }
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
