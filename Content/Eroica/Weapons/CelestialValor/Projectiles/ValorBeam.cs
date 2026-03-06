using System;
using MagnumOpus.Common;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Buffs;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.FoundationWeapons.SwordSmearFoundation;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Projectiles
{
    /// <summary>
    /// Homing valor beam - blazing scarlet-gold energy bolt that seeks enemies.
    /// 
    /// ARCHITECTURE: Built on Foundation bloom rendering (SMFTextures SoftGlow/StarFlare/PointBloom).
    /// - Head: Multi-layered additive bloom (outer haze + main body + hot core + star flare)
    /// - Trail: Afterimage chain using oldPos with Foundation-style fade
    /// - On hit: Spawns ValorSlash directly + RippleEffectProjectile (ThemeEroica)
    /// 
    /// No longer depends on SandboxExoblade PrimitiveRenderer or ExobladePierce shader.
    /// </summary>
    public class ValorBeam : ModProjectile
    {
        private int TargetIndex = -1;
        private const int NoHomeTime = 20;

        private ref float Time => ref Projectile.ai[0];

        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 255;
            Projectile.timeLeft = 300;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = Projectile.MaxUpdates * 12;
        }

        public override void AI()
        {
            // Homing after initial delay
            if (Time >= NoHomeTime)
            {
                if (TargetIndex >= 0)
                {
                    if (!Main.npc[TargetIndex].active || !Main.npc[TargetIndex].CanBeChasedBy())
                        TargetIndex = -1;
                    else
                    {
                        Vector2 dirToTarget = (Main.npc[TargetIndex].Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        Vector2 ideal = dirToTarget * (Projectile.velocity.Length() + 5.5f);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, ideal, 0.09f);
                    }
                }

                if (TargetIndex == -1)
                {
                    float bestDist = 1400f;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (!npc.active || !npc.CanBeChasedBy()) continue;
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            TargetIndex = i;
                        }
                    }
                    if (TargetIndex == -1) Projectile.velocity *= 0.99f;
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Heroic ember dust trail
            if (Main.rand.NextBool())
            {
                Color dustColor = EroicaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                    DustID.GoldFlame, Projectile.velocity * -1.8f, 0, dustColor);
                d.scale = 0.35f;
                d.fadeIn = Main.rand.NextFloat() * 1f;
                d.noGravity = true;
            }

            Projectile.scale = Utils.GetLerpValue(0f, 0.12f, Projectile.timeLeft / 400f, true);

            // Dynamic lighting
            Vector3 lightColor = Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Gold, 0.4f).ToVector3() * 0.5f;
            Lighting.AddLight(Projectile.Center, lightColor);

            if (Projectile.numUpdates == 0) Time++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item60 with { Volume = 0.5f, Pitch = 0.3f }, target.Center);

            // Spawn cross-slash at impact (Foundation: ValorSlash directly, no creator middleman)
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 slashDir = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 3f;
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center,
                    slashDir, ModContent.ProjectileType<ValorSlash>(),
                    (int)(Projectile.damage * 0.4f), 0f, Projectile.owner);

                // Foundation: spawn RippleEffectProjectile (ThemeEroica = 2f)
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center,
                    Vector2.Zero, ModContent.ProjectileType<RippleEffectProjectile>(),
                    0, 0f, Projectile.owner, RippleEffectProjectile.ThemeEroica);
            }

            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 180);
            target.AddBuff(ModContent.BuffType<HeroicBurn>(), 120);

            // Impact dust burst
            for (int i = 0; i < 5; i++)
            {
                Color sparkColor = Main.rand.NextBool() ? EroicaPalette.Gold : EroicaPalette.Scarlet;
                Dust d = Dust.NewDustPerfect(
                    target.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.GoldFlame, Main.rand.NextVector2Circular(6f, 6f), 0, sparkColor);
                d.scale = 0.4f;
                d.noGravity = true;
            }

            EroicaVFXLibrary.ProjectileImpact(target.Center, 0.8f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 5f);
                Color dustColor = EroicaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, vel, 0, dustColor);
                d.scale = 0.35f;
                d.noGravity = true;
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White with { A = 0 } * Projectile.Opacity;

        #region Foundation Bloom Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.timeLeft > 295) return false;

            SpriteBatch sb = Main.spriteBatch;
            Texture2D softGlow = SMFTextures.SoftGlow.Value;
            Texture2D starFlare = SMFTextures.StarFlare.Value;
            Texture2D pointBloom = SMFTextures.PointBloom.Value;

            float scale = Projectile.scale;
            float time = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;

            // Cycle through Eroica fire colors
            Color mainColor = MulticolorLerp(
                (time * 1.5f + Projectile.whoAmI * 0.15f) % 1f,
                EroicaPalette.Scarlet, EroicaPalette.Flame, EroicaPalette.Gold, EroicaPalette.HotCore);

            // ���� LAYER 1: Afterimage trail chain (Foundation bloom-per-point) ����
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Vector2 glowOrigin = softGlow.Size() / 2f;
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) break;

                float progress = (float)i / Projectile.oldPos.Length;
                float fade = (1f - progress);
                fade *= fade;
                if (fade < 0.02f) continue;

                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailScale = 0.15f * fade * scale;

                // Outer glow
                Color outerCol = Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.DeepScarlet, progress) with { A = 0 };
                sb.Draw(softGlow, drawPos, null, outerCol * (fade * 0.4f), 0f, glowOrigin, trailScale * 1.5f, SpriteEffects.None, 0f);

                // Inner core
                Color coreCol = Color.Lerp(EroicaPalette.Gold, Color.White, fade * 0.3f) with { A = 0 };
                sb.Draw(pointBloom, drawPos, null, coreCol * (fade * 0.5f), 0f,
                    pointBloom.Size() / 2f, trailScale * 0.6f, SpriteEffects.None, 0f);
            }

            // ���� LAYER 2: Head bloom (Foundation multi-scale stack) ����
            Vector2 headPos = Projectile.Center - Main.screenPosition;

            // Wide ambient haze
            sb.Draw(softGlow, headPos, null,
                (EroicaPalette.Scarlet with { A = 0 }) * 0.12f, 0f, glowOrigin,
                1.1f * scale, SpriteEffects.None, 0f);

            // Main body glow
            sb.Draw(softGlow, headPos, null,
                (mainColor with { A = 0 }) * 0.5f, 0f, glowOrigin,
                0.3f * scale, SpriteEffects.None, 0f);

            // Hot white core
            sb.Draw(pointBloom, headPos, null,
                (Color.White with { A = 0 }) * 0.4f, 0f, pointBloom.Size() / 2f,
                0.12f * scale, SpriteEffects.None, 0f);

            // Star flare accent
            float flareRot = time * 2f + Projectile.whoAmI;
            sb.Draw(starFlare, headPos, null,
                (EroicaPalette.Gold with { A = 0 }) * 0.35f, flareRot,
                starFlare.Size() / 2f, 0.15f * scale, SpriteEffects.None, 0f);

            // Counter-rotating flare
            sb.Draw(starFlare, headPos, null,
                (mainColor with { A = 0 }) * 0.2f, -flareRot * 0.7f,
                starFlare.Size() / 2f, 0.1f * scale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        #endregion

        private static Color MulticolorLerp(float t, params Color[] colors)
        {
            t = MathHelper.Clamp(t, 0f, 0.999f);
            float scaled = t * (colors.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, colors.Length - 1);
            return Color.Lerp(colors[lo], colors[hi], scaled - lo);
        }
    }
}
