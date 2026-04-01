using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Buffs;
using static MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Utilities.IncisorUtils;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Projectiles
{
    /// <summary>
    /// Crescent Wave  -- expanding shockwave projectile.
    /// Created by Movement III's 5th slash and the Grand Finale's radial burst.
    /// Expands outward as a growing crescent ring, slowing all enemies in range.
    /// Applies Moonlit Silence debuff (40% speed reduction for 3s).
    /// "The storm subsides, and silence descends like moonlight on still water."
    /// </summary>
    public class CrescentWaveProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/HardCircleMask";

        private const int ExpandDuration = 40;
        private const float MaxRadius = 350f;

        public ref float Timer => ref Projectile.ai[0];
        public ref float MaxScale => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = ExpandDuration;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // Only hit each NPC once
        }

        public override void AI()
        {
            Timer++;
            float progress = Timer / ExpandDuration;

            // Expand hitbox with the wave
            float currentRadius = MaxRadius * EaseOutQuart(progress);
            Projectile.width = Projectile.height = (int)(currentRadius * 2);
            Projectile.Center = Projectile.position + new Vector2(Projectile.width / 2f, Projectile.height / 2f);

            // Lighting
            float intensity = 1f - progress;
            Lighting.AddLight(Projectile.Center, new Vector3(0.4f, 0.35f, 0.7f) * intensity);

            // Expanding ring particles along the edge
            if (!Main.dedServ && Timer % 3 == 0)
            {
                int particleCount = (int)(8 * (1f - progress * 0.5f));
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / particleCount + Main.rand.NextFloat(0.2f);
                    Vector2 edgePos = Projectile.Center + angle.ToRotationVector2() * currentRadius;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1f, 3f);

                    Color sparkColor = MulticolorLerp(Main.rand.NextFloat(), IncisorPalette);
                    var spark = new ConstellationSparkParticle(
                        edgePos, vel, false, Main.rand.Next(10, 18),
                        Main.rand.NextFloat(0.08f, 0.2f), sparkColor,
                        new Vector2(0.4f, 1.5f), quickShrink: true);
                    IncisorParticleHandler.SpawnParticle(spark);
                }
            }

            // Lunar motes spiraling outward
            if (!Main.dedServ && Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 motePos = Projectile.Center + angle.ToRotationVector2() * currentRadius * Main.rand.NextFloat(0.6f, 1f);
                var mote = new LunarMoteParticle(
                    motePos, angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f),
                    Main.rand.NextFloat(0.2f, 0.4f),
                    MulticolorLerp(Main.rand.NextFloat(), IncisorPalette),
                    Main.rand.Next(15, 25), 2f, 3f, hueShift: 0.01f);
                IncisorParticleHandler.SpawnParticle(mote);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Moonlit Silence  -- 40% speed reduction for 3 seconds
            target.AddBuff(ModContent.BuffType<MoonlitSilenceDebuff>(), 180);

            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.2f, Volume = 0.5f }, target.Center);

            if (!Main.dedServ)
            {
                // Impact sparkles on hit
                for (int i = 0; i < 5; i++)
                {
                    Color sc = MulticolorLerp(Main.rand.NextFloat(), IncisorPalette);
                    var spark = new ConstellationSparkParticle(
                        target.Center, Main.rand.NextVector2Circular(6f, 6f),
                        true, Main.rand.Next(10, 18), Main.rand.NextFloat(0.1f, 0.22f),
                        sc, new Vector2(0.5f, 1.4f));
                    IncisorParticleHandler.SpawnParticle(spark);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            float progress = Timer / ExpandDuration;
            float radius = MaxRadius * EaseOutQuart(progress);
            float fade = 1f - MathF.Pow(progress, 2f);

            var ringTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/PowerEffectRing",
                AssetRequestMode.ImmediateLoad).Value;
            var bloomTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom",
                AssetRequestMode.ImmediateLoad).Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float ringScale = radius * 2f / ringTex.Width;
            float bloomDrawScale = MathF.Min(radius * 3f / bloomTex.Width, 0.139f);

            // Switch to Additive blending for glow/bloom layers
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Wide ambient bloom underlayer
            Color ambientColor = new Color(90, 50, 160) with { A = 0 };
            Main.spriteBatch.Draw(bloomTex, drawPos, null, ambientColor * fade * 0.15f,
                0f, bloomTex.Size() * 0.5f, bloomDrawScale, SpriteEffects.None, 0f);

            // Main expanding ring — ice blue to violet
            Color ringColor = Color.Lerp(new Color(135, 206, 250), new Color(170, 140, 255), progress) with { A = 0 };
            Main.spriteBatch.Draw(ringTex, drawPos, null, ringColor * fade * 0.7f,
                0f, ringTex.Size() * 0.5f, ringScale, SpriteEffects.None, 0f);

            // Inner bright ring
            Color innerColor = Color.Lerp(new Color(220, 230, 255), Color.White, progress * 0.5f) with { A = 0 };
            Main.spriteBatch.Draw(ringTex, drawPos, null, innerColor * fade * 0.4f,
                MathHelper.PiOver4, ringTex.Size() * 0.5f, ringScale * 0.85f, SpriteEffects.None, 0f);

            // Center flash (fades quickly)
            if (progress < 0.3f)
            {
                float flashFade = 1f - progress / 0.3f;
                Main.spriteBatch.Draw(bloomTex, drawPos, null, Color.White with { A = 0 } * flashFade * 0.5f,
                    0f, bloomTex.Size() * 0.5f, 0.08f * flashFade, SpriteEffects.None, 0f);
            }

            // Restore to AlphaBlend
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

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

        private static float EaseOutQuart(float t)
        {
            return 1f - MathF.Pow(1f - t, 4f);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Final ring of sparkles
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Color sc = MulticolorLerp(Main.rand.NextFloat(), IncisorPalette);
                var spark = new ConstellationSparkParticle(
                    Projectile.Center + angle.ToRotationVector2() * MaxRadius * 0.8f,
                    angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f),
                    true, Main.rand.Next(12, 20), Main.rand.NextFloat(0.1f, 0.2f),
                    sc, new Vector2(0.5f, 1.3f), quickShrink: true);
                IncisorParticleHandler.SpawnParticle(spark);
            }
        }
    }
}
