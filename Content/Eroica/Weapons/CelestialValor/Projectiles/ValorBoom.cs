using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.FoundationWeapons.SwordSmearFoundation;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Projectiles
{
    /// <summary>
    /// Heroic AoE detonation - expanding shockwave ring with multi-layer bloom flash.
    /// Spawned by CelestialValorSwing Finale Fortissimo phase.
    /// 
    /// ARCHITECTURE: Built on Foundation patterns.
    /// - Impact flash: Foundation multi-scale bloom stack (SMFTextures SoftGlow/StarFlare)
    /// - Shockwave: Spawns RippleEffectProjectile (ThemeEroica) for expanding ring
    /// - Sparks: Foundation SparkExplosion-style radial particle burst
    /// - Bloom rings: BloomRingParticle expanding halos
    /// </summary>
    public class ValorBoom : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

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

            if (!spawnedVFX)
            {
                spawnedVFX = true;

                // Foundation SparkExplosion-style radial particle burst
                int sparkCount = 35;
                for (int i = 0; i < sparkCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloat(-0.15f, 0.15f);
                    float speed = Main.rand.NextFloat(4f, 10f);
                    Vector2 vel = angle.ToRotationVector2() * speed;
                    Color col = i % 3 == 0 ? EroicaPalette.Scarlet :
                                i % 3 == 1 ? EroicaPalette.Gold :
                                             EroicaPalette.HotCore;
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, vel, 0, col);
                    d.scale = Main.rand.NextFloat(0.5f, 1.0f);
                    d.noGravity = true;
                    d.fadeIn = 0.4f;
                }

                // Eroica VFX library helpers
                EroicaVFXLibrary.SpawnRadialDustBurst(Projectile.Center, 20, 8f);
                EroicaVFXLibrary.SpawnValorSparkles(Projectile.Center, 12, 60f);
                EroicaVFXLibrary.SpawnMusicNotes(Projectile.Center, 6, 50f, 0.8f, 1.1f, 40);
                EroicaVFXLibrary.MusicNoteBurst(Projectile.Center, EroicaPalette.Gold, 8, 5f);

                // Spawn Foundation RippleEffectProjectile (ThemeEroica)
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center,
                        Vector2.Zero, ModContent.ProjectileType<RippleEffectProjectile>(),
                        0, 0f, Projectile.owner, RippleEffectProjectile.ThemeEroica);
                }

                // Bloom ring particles
                for (int i = 0; i < 3; i++)
                {
                    float ringScale = 0.5f + i * 0.3f;
                    Color ringColor = Color.Lerp(EroicaPalette.Gold, EroicaPalette.Scarlet, (float)i / 3f);
                    var ring = new BloomRingParticle(Projectile.Center, Vector2.Zero, ringColor, ringScale, 25 + i * 5, 0.12f - i * 0.02f);
                    MagnumParticleHandler.SpawnParticle(ring);
                }

                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.6f, Pitch = -0.2f }, Projectile.Center);
            }

            // Expanding hitbox
            float progress = 1f - (float)Projectile.timeLeft / MaxLife;
            float expandScale = 1f + progress * 0.8f;
            Projectile.width = (int)(280 * expandScale);
            Projectile.height = (int)(280 * expandScale);
            Projectile.Center = Projectile.Center;

            // Lingering embers
            if (Projectile.timeLeft % 4 == 0)
                EroicaVFXLibrary.SpawnHeroicAura(Projectile.Center, 80f * expandScale);

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

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // ���� Foundation bloom stack (SMFTextures) ����
            Texture2D softGlow = SMFTextures.SoftGlow.Value;
            Texture2D starFlare = SMFTextures.StarFlare.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Wide scarlet shockwave haze
            float outerScale = 0.1f + progress * 0.18f;
            sb.Draw(softGlow, drawPos, null,
                (EroicaPalette.Scarlet with { A = 0 }) * (fade * 0.15f), 0f,
                glowOrigin, outerScale, SpriteEffects.None, 0f);

            // Layer 2: Mid gold glow
            float midScale = 0.08f + progress * 0.13f;
            sb.Draw(softGlow, drawPos, null,
                (EroicaPalette.Gold with { A = 0 }) * (fade * 0.25f), 0f,
                glowOrigin, midScale, SpriteEffects.None, 0f);

            // Layer 3: Hot core (fades fast)
            float coreScale = 0.05f + progress * 0.08f;
            float coreFade = MathF.Max(0f, 1f - progress * 2f);
            sb.Draw(softGlow, drawPos, null,
                (EroicaPalette.HotCore with { A = 0 }) * (coreFade * 0.4f), 0f,
                glowOrigin, coreScale, SpriteEffects.None, 0f);

            // Layer 4: Rotating star flare cross (early frames only)
            if (progress < 0.5f)
            {
                float flareFade = 1f - progress * 2f;
                float flareRot = progress * MathHelper.TwoPi * 2f;
                Vector2 flareOrigin = starFlare.Size() / 2f;

                sb.Draw(starFlare, drawPos, null,
                    (EroicaPalette.Gold with { A = 0 }) * (flareFade * 0.35f), flareRot,
                    flareOrigin, MathHelper.Min(0.3f * flareFade, 0.293f), SpriteEffects.None, 0f);

                sb.Draw(starFlare, drawPos, null,
                    (EroicaPalette.HotCore with { A = 0 }) * (flareFade * 0.2f),
                    flareRot + MathHelper.PiOver4,
                    flareOrigin, 0.2f * flareFade, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
