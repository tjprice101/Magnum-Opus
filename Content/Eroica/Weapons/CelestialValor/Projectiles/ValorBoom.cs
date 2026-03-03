using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Projectiles
{
    /// <summary>
    /// Heroic AoE detonation — expanding shockwave ring with multi-layer bloom flash.
    /// Spawned by CelestialValorSwing's Finale Fortissimo phase.
    /// </summary>
    public class ValorBoom : ModProjectile
    {
        private const int MaxLife = 30;
        private bool spawnedVFX = false;

        public override void SetDefaults()
        {
            Projectile.width = 280;
            Projectile.height = 280;
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxLife;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            // Spawn VFX burst on first tick
            if (!spawnedVFX)
            {
                spawnedVFX = true;
                EroicaVFXLibrary.SpawnRadialDustBurst(Projectile.Center, 20, 8f);
                EroicaVFXLibrary.SpawnValorSparkles(Projectile.Center, 12, 60f);
                EroicaVFXLibrary.SpawnMusicNotes(Projectile.Center, 6, 50f, 0.8f, 1.1f, 40);

                // Expanding bloom rings
                for (int i = 0; i < 3; i++)
                {
                    float ringScale = 0.5f + i * 0.3f;
                    Color ringColor = Color.Lerp(EroicaPalette.Gold, EroicaPalette.Scarlet, (float)i / 3f);
                    var ring = new BloomRingParticle(Projectile.Center, Vector2.Zero, ringColor, ringScale, 25 + i * 5, 0.12f - i * 0.02f);
                    MagnumParticleHandler.SpawnParticle(ring);
                }
            }

            // Expanding hitbox
            float progress = 1f - (float)Projectile.timeLeft / MaxLife;
            float expandScale = 1f + progress * 0.8f;
            Projectile.width = (int)(280 * expandScale);
            Projectile.height = (int)(280 * expandScale);
            Projectile.Center = Projectile.Center; // Re-center after resize

            // Lingering embers
            if (Projectile.timeLeft % 4 == 0)
            {
                EroicaVFXLibrary.SpawnHeroicAura(Projectile.Center, 80f * expandScale);
            }

            EroicaVFXLibrary.AddPaletteLighting(Projectile.Center, 0.5f, 1.2f * (1f - progress));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 300);
            EroicaVFXLibrary.MeleeImpact(target.Center, 3);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            float progress = 1f - (float)Projectile.timeLeft / MaxLife;
            float fade = 1f - progress;

            // ── Layer 1: Wide scarlet shockwave ──
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom != null)
            {
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Vector2 origin = bloom.Size() * 0.5f;

                // Outer expanding ring glow
                float outerScale = 2f + progress * 4f;
                Color outerColor = EroicaPalette.Scarlet with { A = 0 };
                sb.Draw(bloom, drawPos, null, outerColor * (fade * 0.25f), 0f, origin, outerScale, SpriteEffects.None, 0f);

                // Mid-layer gold
                float midScale = 1.5f + progress * 2.5f;
                Color midColor = EroicaPalette.Gold with { A = 0 };
                sb.Draw(bloom, drawPos, null, midColor * (fade * 0.35f), 0f, origin, midScale, SpriteEffects.None, 0f);

                // Hot core — bright early, fades fast
                float coreScale = 0.8f + progress * 1f;
                float coreFade = MathF.Max(0f, 1f - progress * 2f);
                Color coreColor = EroicaPalette.HotCore with { A = 0 };
                sb.Draw(bloom, drawPos, null, coreColor * (coreFade * 0.6f), 0f, origin, coreScale, SpriteEffects.None, 0f);
            }

            // ── Layer 2: Rotating flare cross ──
            Texture2D flare = MagnumTextureRegistry.GetFlare();
            if (flare != null && progress < 0.5f)
            {
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Vector2 flareOrigin = flare.Size() * 0.5f;
                float flareRot = progress * MathHelper.TwoPi * 2f;
                float flareScale = 0.6f * (1f - progress * 2f);
                Color flareColor = EroicaPalette.Gold with { A = 0 };
                sb.Draw(flare, drawPos, null, flareColor * (0.5f * (1f - progress * 2f)), flareRot, flareOrigin, flareScale, SpriteEffects.None, 0f);
                sb.Draw(flare, drawPos, null, flareColor * (0.3f * (1f - progress * 2f)), flareRot + MathHelper.PiOver4, flareOrigin, flareScale * 0.7f, SpriteEffects.None, 0f);
            }

            return false;
        }
    }
}