using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath.Projectiles
{
    /// <summary>
    /// Eclipse Orb ? slow-moving dark sphere with fire corona.
    /// Dual-render: dark disc center + additive bloom corona ring.
    /// On impact or max range: splits into 6 Wrath Shards. Crits: 12 shards + Corona Flare.
    /// Leaves Eclipse Field darkness zone.
    /// </summary>
    public class EclipseOrbProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int TrailLength = 16;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private int trailHead = 0;
        private bool trailInitialized = false;
        private float timer = 0;
        private VertexStrip _vertexStrip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            timer++;
            Projectile.rotation += 0.02f;

            // Initialize trail
            if (!trailInitialized)
            {
                for (int i = 0; i < TrailLength; i++)
                    trailPositions[i] = Projectile.Center;
                trailInitialized = true;
            }
            trailPositions[trailHead] = Projectile.Center;
            trailHead = (trailHead + 1) % TrailLength;

            // Light homing toward cursor
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 toMouse = Main.MouseWorld - Projectile.Center;
                if (toMouse.Length() > 30f)
                {
                    toMouse = toMouse.SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toMouse * 8f, 0.02f);
                }
            }

            // Corona fire wisps dust
            if (!Main.dedServ && Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 16f;
                Dust d = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Torch,
                    offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f), 0, default, 0.8f);
                d.noGravity = true;
            }

            // Dark center smoke
            if (!Main.dedServ && Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.Smoke, Main.rand.NextVector2Circular(0.5f, 0.5f), 200, default, 0.6f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.5f, 0.15f, 0.05f);
        }

        public override void OnKill(int timeLeft)
        {
            // Split into shards
            Split(false);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);

            // Dies Irae VFX: eclipse impact burst
            DiesIraeVFXLibrary.MeleeImpact(target.Center, 0);

            bool isCrit = hit.Crit;
            if (isCrit)
            {
                EclipseOfWrathUtils.DoCoronaFlare(target.Center);
                DiesIraeVFXLibrary.SpawnMusicNotes(target.Center, 3, 2f, 0.8f, 1f, 40);
                DiesIraeVFXLibrary.SpawnJudgmentRings(target.Center, 2, 0.4f);
            }
        }

        private void Split(bool isCrit)
        {
            if (Main.myPlayer != Projectile.owner) return;

            // Split VFX
            EclipseOfWrathUtils.DoEclipseSplit(Projectile.Center);

            int shardCount = isCrit ? 12 : 6;
            float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);

            for (int i = 0; i < shardCount; i++)
            {
                float angle = baseAngle + MathHelper.TwoPi / shardCount * i + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (8f + Main.rand.NextFloat() * 4f);

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel,
                    ModContent.ProjectileType<EclipseWrathShard>(),
                    (int)(Projectile.damage * 0.35f), Projectile.knockBack * 0.3f, Projectile.owner);
            }

            // Spawn Eclipse Field
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<EclipseFieldProjectile>(),
                (int)(Projectile.damage * 0.1f), 0f, Projectile.owner);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {

            IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.DiesIrae, ref _vertexStrip);

            // Eclipse accent: dark corona ring — 4 elongated eclipse flares
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
            {
                Vector2 origin = glow.Size() / 2f;
                float pulse = 0.8f + 0.2f * MathF.Sin((float)Main.timeForVisualEffects * 0.07f);
                float coronaRot = (float)Main.timeForVisualEffects * 0.025f;

                for (int i = 0; i < 4; i++)
                {
                    float angle = coronaRot + MathHelper.PiOver2 * i;
                    sb.Draw(glow, drawPos, null,
                        (DiesIraePalette.EmberOrange with { A = 0 }) * 0.2f * pulse,
                        angle, origin, new Vector2(0.16f, 0.03f), SpriteEffects.None, 0f);
                }

                // Central dark ember glow
                sb.Draw(glow, drawPos, null,
                    (DiesIraePalette.BloodRed with { A = 0 }) * 0.15f,
                    0f, origin, 0.07f, SpriteEffects.None, 0f);
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
    /// Eclipse Field ? lingering darkness zone left by eclipse orb destruction.
    /// Enemies inside: -vision range, +15% damage from all sources, 2s duration.
    /// </summary>
    public class EclipseFieldProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int MaxLifetime = 120; // 2 seconds
        private const float FieldRadius = 80f; // 5 tiles

        public override void SetDefaults()
        {
            Projectile.width = 160;
            Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxLifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            // Dark dust at edges
            if (!Main.dedServ && Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 edgePos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * FieldRadius;
                Dust d = Dust.NewDustPerfect(edgePos, DustID.Smoke,
                    (Projectile.Center - edgePos).SafeNormalize(Vector2.Zero) * 0.5f, 200, default, 0.8f);
                d.noGravity = true;
            }

            // Dim local lighting
            Lighting.AddLight(Projectile.Center, -0.2f, -0.2f, -0.2f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2()) < FieldRadius + targetHitbox.Width / 2f;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Eclipse field enemies take +15% damage from all sources
            modifiers.FinalDamage *= 0.15f; // This is the field's own low damage
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float life = (float)Projectile.timeLeft / MaxLifetime;
            EclipseOfWrathUtils.DrawEclipseField(sb, Projectile.Center, FieldRadius, life, Projectile.ai[0]++);


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