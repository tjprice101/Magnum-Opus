using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Particles;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Projectiles
{
    /// <summary>
    /// Resonant Note — lingering landmine spawned by Resonant Detonation.
    /// Drifts slowly, hovers in place, damages enemies passing through for 3 seconds.
    /// Musical note identity with gold aura.
    /// </summary>
    public class ResonantNoteProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/MusicNote";

        private const float DamageRadius = 50f;
        private float hoverAngle;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180; // 3 seconds
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30; // Tick damage every 0.5s
        }

        public override void AI()
        {
            // Decelerate to hover
            Projectile.velocity *= 0.93f;
            hoverAngle += 0.05f;
            Projectile.position.Y += (float)Math.Sin(hoverAngle) * 0.25f;
            Projectile.rotation += 0.02f;

            // Ambient sparkle
            if (Main.rand.NextBool(8))
            {
                PiercingBellsParticleHandler.SpawnParticle(new BulletTracerParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8, 8),
                    Main.rand.NextVector2Circular(0.3f, 0.3f),
                    Main.rand.Next(15, 25)));
            }

            float fade = (float)Projectile.timeLeft / 180f;
            Lighting.AddLight(Projectile.Center, PiercingBellsResonanceUtils.ResonancePalette[2].ToVector3() * 0.3f * fade);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float dist = Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2());
            return dist <= DamageRadius;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            target.GetGlobalNPC<ResonantMarkerNPC>().AddMarker(target);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                PiercingBellsParticleHandler.SpawnParticle(new BulletTracerParticle(
                    Projectile.Center, Main.rand.NextVector2Circular(2f, 2f),
                    Main.rand.Next(10, 18)));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            var tex = ModContent.Request<Texture2D>(Texture).Value;
            float fade = Math.Min(1f, (float)Projectile.timeLeft / 30f); // Fade out in last 0.5s
            float pulse = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.12f + Projectile.whoAmI) * 0.15f;
            Color drawColor = PiercingBellsResonanceUtils.ResonancePalette[2] * pulse * fade;

            sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                drawColor, Projectile.rotation, tex.Size() / 2f, 0.5f * pulse, SpriteEffects.None, 0f);

            // Aura glow
            Texture2D bloomTex = null;
            try { bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow", ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value; } catch { }
            if (bloomTex != null)
            {
                Color aura = PiercingBellsResonanceUtils.ResonancePalette[1] * 0.15f * fade;
                float auraScale = DamageRadius / (bloomTex.Width * 0.5f);
                sb.Draw(bloomTex, Projectile.Center - Main.screenPosition, null,
                    aura, 0f, bloomTex.Size() / 2f, auraScale, SpriteEffects.None, 0f);
            }

            return false;
        }
    }
}
