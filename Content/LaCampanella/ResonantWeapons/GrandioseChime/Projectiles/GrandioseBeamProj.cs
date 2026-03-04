using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Primitives;
using MagnumOpus.Content.LaCampanella;
using MagnumOpus.Content.LaCampanella.Debuffs;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Projectiles
{
    /// <summary>
    /// Primary golden beam projectile. Fast-moving, pierces 2 enemies.
    /// On NPC kill, triggers Kill Echo Chain (propagate to nearest enemy within 15 tiles, 60% damage, 3 chains max).
    /// ai[0] = 1 for Grandiose Crescendo variant (triple width visual).
    /// </summary>
    public class GrandioseBeamProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailPoints = 14;
        private GrandioseChimePrimitiveRenderer trailRenderer;

        private bool IsGrandiose => Projectile.ai[0] > 0f;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 80;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailPoints)
                trailPositions.RemoveAt(trailPositions.Count - 1);

            Projectile.rotation = Projectile.velocity.ToRotation();
            float intensity = IsGrandiose ? 0.8f : 0.5f;
            Lighting.AddLight(Projectile.Center, GrandioseChimeUtils.BeamPalette[2].ToVector3() * intensity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, IsGrandiose ? 2 : 1);

            // === FOUNDATION: RippleEffectProjectile — Beam impact zone ring ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                ModContent.ProjectileType<RippleEffectProjectile>(),
                0, 0f, Projectile.owner);

            // Kill echo chain on enemy death
            if (target.life <= 0)
            {
                TriggerKillEchoChain(target.Center, damageDone, 0);
            }
        }

        private void TriggerKillEchoChain(Vector2 killPos, int killDamage, int chainDepth)
        {
            const int MaxChains = 3;
            const float ChainRange = 240f; // 15 tiles
            const float DamageScale = 0.6f;

            if (chainDepth >= MaxChains) return;

            // Spawn Kill Echo projectile that seeks nearest enemy
            int echoDmg = (int)(killDamage * DamageScale);
            if (echoDmg < 1) return;

            Projectile.NewProjectile(Projectile.GetSource_FromAI(), killPos, Vector2.Zero,
                ModContent.ProjectileType<KillEchoProj>(), echoDmg, 4f, Projectile.owner,
                ai0: ChainRange, ai1: chainDepth);

            // VFX
            GrandioseChimeParticleHandler.SpawnParticle(new KillEchoParticle(killPos, 2f, 15));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {

            if (trailPositions.Count >= 2)
            {
                try
                {
                    trailRenderer ??= new GrandioseChimePrimitiveRenderer();
                    float trailWidth = IsGrandiose ? 24f : 8f;
                    var settings = new GrandioseBeamTrailSettings
                    {
                        ColorStart = GrandioseChimeUtils.BeamPalette[2],
                        ColorEnd = GrandioseChimeUtils.BeamPalette[0] * 0.3f,
                        Width = trailWidth,
                        BloomIntensity = IsGrandiose ? 0.6f : 0.3f
                    };
                    trailRenderer.DrawTrail(sb, trailPositions, settings, Main.screenPosition);
                }
                catch { }
            }

            var tex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            float pulse = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.15f;
            float coreScale = IsGrandiose ? 0.3f : 0.15f;
            Color coreColor = GrandioseChimeUtils.BeamPalette[3] * pulse;

            // Draw bloom core + LC ring in Additive
            try { sb.End(); } catch { }
            try
            {
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                coreColor, 0f, tex.Size() / 2f, coreScale, SpriteEffects.None, 0f);

            // LC Infernal Beam Ring - fiery halo around beam core
            {
                Vector2 beamScreen = Projectile.Center - Main.screenPosition;
                float ringScale = IsGrandiose ? 0.3f : 0.18f;
                float ringRot = (float)Main.GameUpdateCount * 0.04f;
                LaCampanellaVFXLibrary.DrawInfernalBeamRing(sb, beamScreen,
                    ringScale * pulse, ringRot, 0.25f * pulse,
                    LaCampanellaPalette.InfernalOrange);
            }

            // Theme texture accents
            GrandioseChimeUtils.DrawThemeAccents(sb, Projectile.Center - Main.screenPosition, Projectile.scale);
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }

            } // end outer try
            catch
            {
                try
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            trailRenderer?.Dispose();
            trailRenderer = null;
        }
    }
}