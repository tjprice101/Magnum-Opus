using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Utilities;
using MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Primitives;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using Terraria.Graphics.Shaders;
using ReLogic.Content;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Projectiles
{
    /// <summary>
    /// Small homing crystal shard fired by IridescentCrystalProj in 3-burst volleys.
    /// Gentle homing, iridescent shimmer trail.
    /// Foundation-pattern rendering: 2-layer bloom + vanilla dust.
    /// </summary>
    public class CrystalShardProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        public ref float Timer => ref Projectile.ai[0];

        private const int TrailLength = 10;
        private Vector2[] oldPos = new Vector2[TrailLength];
        private FlockPrimitiveRenderer _trailRenderer;

        public override void SetStaticDefaults() { ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength; }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            Timer++;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // --- Gentle homing ---
            if (Timer > 8)
            {
                NPC target = FindClosestNPC(400f);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.03f);
                }
            }

            // --- Trail recording ---
            for (int i = TrailLength - 1; i > 0; i--)
                oldPos[i] = oldPos[i - 1];
            oldPos[0] = Projectile.Center;

            // --- Iridescent shimmer dust ---
            if (Timer % 3 == 0)
            {
                Color c = FlockUtils.GetIridescent(Timer * 0.03f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1, 1), 0, c, 0.4f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.3f, 0.3f, 0.4f);
        }

        private NPC FindClosestNPC(float range)
        {
            NPC closest = null;
            float bestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SwansMark>(), 180);

            for (int i = 0; i < 4; i++)
            {
                Color c = FlockUtils.GetIridescent(i / 4f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(3, 3), 0, c, 0.6f);
                d.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            _trailRenderer?.Dispose();
            _trailRenderer = null;

            for (int i = 0; i < 4; i++)
            {
                Color c = FlockUtils.GetIridescent(i / 4f + Timer * 0.01f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(2, 2), 0, c, 0.4f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 screenPos = Main.screenPosition;

            try
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Texture2D bloom = MagnumTextureRegistry.GetSoftGlow();
                Texture2D point = MagnumTextureRegistry.GetPointBloom();
                Texture2D star = MagnumTextureRegistry.GetStar4Soft();

                Vector2 drawPos = Projectile.Center - screenPos;
                float life = MathHelper.Clamp(Timer / 120f, 0f, 1f);
                float shardHue = Timer * 0.03f; // shifting hue over lifetime

                // ========================================================
                // GPU SHADER TRAIL — CrystalOrbitTrail (3-pass faceted shard trail)
                // ========================================================
                if (oldPos[1] != Vector2.Zero && GameShaders.Misc.ContainsKey("MagnumOpus:CrystalOrbitTrail"))
                {
                    // End SpriteBatch before GPU primitive rendering
                    sb.End();

                    _trailRenderer ??= new FlockPrimitiveRenderer();

                    var trailShader = GameShaders.Misc["MagnumOpus:CrystalOrbitTrail"];
                    var effect = trailShader.Shader;

                    float trailWidth = 8f + 4f * (1f - life); // wider when fresh, narrows as it ages

                    // Set shared shader params
                    effect.Parameters["uPhase"]?.SetValue(shardHue);
                    effect.Parameters["uOpacity"]?.SetValue(0.6f);
                    effect.Parameters["uIntensity"]?.SetValue(1.0f);
                    effect.Parameters["uOverbrightMult"]?.SetValue(0.15f);
                    trailShader.UseImage1(MagnumTextureRegistry.PerlinNoise);

                    // Pass 1: CrystalOrbitGlow @ 2.5x width (prismatic bloom underlay)
                    effect.CurrentTechnique = effect.Techniques["CrystalOrbitGlow"];
                    var glowSettings = new FlockTrailSettings(
                        t => trailWidth * 2.5f * (1f - t * 0.75f),
                        t => FlockUtils.GetIridescent(t + shardHue) * (0.2f * (1f - t)));
                    glowSettings = new FlockTrailSettings(glowSettings.Width, glowSettings.TrailColor, trailShader);
                    _trailRenderer.RenderTrail(oldPos, glowSettings);

                    // Pass 2: CrystalOrbitMain @ 1x width (sharp facets)
                    effect.CurrentTechnique = effect.Techniques["CrystalOrbitMain"];
                    var mainSettings = new FlockTrailSettings(
                        t => trailWidth * (1f - t * 0.65f),
                        t => Color.Lerp(Color.White, FlockUtils.GetIridescent(t * 0.4f + shardHue), t * 0.6f) * (0.7f * (1f - t * 0.4f)));
                    mainSettings = new FlockTrailSettings(mainSettings.Width, mainSettings.TrailColor, trailShader);
                    _trailRenderer.RenderTrail(oldPos, mainSettings);

                    // Pass 3: CrystalOrbitGlow @ 1.3x width (overbright halo)
                    effect.CurrentTechnique = effect.Techniques["CrystalOrbitGlow"];
                    effect.Parameters["uOverbrightMult"]?.SetValue(0.25f);
                    var haloSettings = new FlockTrailSettings(
                        t => trailWidth * 1.3f * (1f - t * 0.8f),
                        t => Color.Lerp(Color.White, FlockUtils.GetIridescent(shardHue + 0.1f), t) * (0.2f * (1f - t)));
                    haloSettings = new FlockTrailSettings(haloSettings.Width, haloSettings.TrailColor, trailShader);
                    _trailRenderer.RenderTrail(oldPos, haloSettings);

                    // Restart sprite batch after GPU primitives
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }

                // ========================================================
                // LAYER 1: Outer iridescent glow
                // ========================================================
                if (bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    Color outerColor = FlockUtils.GetIridescent(shardHue);
                    sb.Draw(bloom, drawPos, null, outerColor * 0.3f, 0f, bOrigin, 0.18f, SpriteEffects.None, 0f);
                }

                // ========================================================
                // LAYER 2: White-hot core
                // ========================================================
                if (point != null)
                {
                    Vector2 pOrigin = point.Size() * 0.5f;
                    sb.Draw(point, drawPos, null, Color.White * 0.7f, 0f, pOrigin, 0.1f, SpriteEffects.None, 0f);
                }

                // ========================================================
                // LAYER 3: Tiny star sparkle
                // ========================================================
                if (star != null)
                {
                    Vector2 sOrigin = star.Size() * 0.5f;
                    float starRot = (float)Main.timeForVisualEffects * 0.05f;
                    Color starC = FlockUtils.GetIridescent(shardHue + 0.25f);
                    sb.Draw(star, drawPos, null, starC * 0.2f, starRot, sOrigin, 0.1f, SpriteEffects.None, 0f);
                }

                // --- Draw shard sprite ---
                Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
                if (tex != null)
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                    Vector2 origin = tex.Size() * 0.5f;
                    Color tint = FlockUtils.GetIridescent(Timer * 0.015f);
                    Color drawColor = Color.Lerp(Projectile.GetAlpha(lightColor), tint, 0.2f);
                    sb.Draw(tex, drawPos, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
                }
            }
            catch { }
            finally
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }
    }
}
