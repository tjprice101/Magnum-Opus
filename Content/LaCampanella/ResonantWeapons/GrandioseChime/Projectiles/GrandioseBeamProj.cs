using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Primitives;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Projectiles
{
    /// <summary>
    /// Primary beam projectile  Efast-moving energy beam shot with fiery trail.
    /// On NPC kill, spawns kill echo burst if player has active KillEchoTimer.
    /// </summary>
    public class GrandioseBeamProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow";

        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailPoints = 14;
        private GrandioseChimePrimitiveRenderer trailRenderer;

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
            Lighting.AddLight(Projectile.Center, GrandioseChimeUtils.BeamPalette[2].ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);

            // Kill echo check
            if (target.life <= 0)
            {
                Player owner = Main.player[Projectile.owner];
                var modPlayer = owner.GetModPlayer<GrandioseChimePlayer>();
                if (modPlayer.HasKillEcho)
                {
                    // Spawn kill echo projectile at death position
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center, Vector2.Zero,
                        ModContent.ProjectileType<KillEchoProj>(), Projectile.damage / 2, 4f, Projectile.owner);

                    // Echo death VFX
                    GrandioseChimeParticleHandler.SpawnParticle(new KillEchoParticle(
                        target.Center, 2f, 20));
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // Trail
            if (trailPositions.Count >= 2)
            {
                try
                {
                    trailRenderer ??= new GrandioseChimePrimitiveRenderer();
                    var settings = new GrandioseBeamTrailSettings
                    {
                        ColorStart = GrandioseChimeUtils.BeamPalette[2],
                        ColorEnd = GrandioseChimeUtils.BeamPalette[0] * 0.3f,
                        Width = 8f,
                        BloomIntensity = 0.3f
                    };
                    trailRenderer.DrawTrail(sb, trailPositions, settings, Main.screenPosition);
                }
                catch { }
            }

            // Beam core
            var tex = ModContent.Request<Texture2D>(Texture).Value;
            float pulse = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.15f;
            Color coreColor = GrandioseChimeUtils.BeamPalette[3] * pulse;
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                coreColor, 0f, tex.Size() / 2f, 0.15f, SpriteEffects.None, 0f);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            trailRenderer?.Dispose();
            trailRenderer = null;
        }
    }
}
