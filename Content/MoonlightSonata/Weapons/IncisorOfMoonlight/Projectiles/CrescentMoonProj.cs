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
    /// Crescent Moon Projectile 窶・fired in triplets during Movement I (Adagio Sostenuto).
    /// Pale blue crescents that travel in shallow arcs, pierce once, and leave
    /// glowing music note resonance points on impact.
    /// "The rolling triplets of moonlight, each note slightly offset in time."
    /// </summary>
    public class CrescentMoonProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft";

        private const float ArcStrength = 0.015f;
        private const int MaxPierceCount = 1;
        private int pierceCount = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = true;
            Projectile.penetrate = MaxPierceCount + 1;
            Projectile.timeLeft = 180;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // Shallow arc 窶・gentle curve upward then down
            float arcPhase = Projectile.ai[1];
            Projectile.velocity = Projectile.velocity.RotatedBy(ArcStrength * Math.Sin(arcPhase * 0.05f));
            Projectile.ai[1]++;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Moonlight glow
            Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.35f, 0.6f) * 0.8f);

            // Trailing constellation sparks
            if (Main.rand.NextBool(3))
            {
                Color sparkColor = MulticolorLerp(Main.rand.NextFloat(),
                    new Color(135, 206, 250), new Color(170, 140, 255), new Color(220, 230, 255));
                var spark = new ConstellationSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    false, Main.rand.Next(10, 18), Main.rand.NextFloat(0.08f, 0.18f),
                    sparkColor, new Vector2(0.4f, 1.5f), quickShrink: true);
                IncisorParticleHandler.SpawnParticle(spark);
            }

            // Lunar mote energy trail
            if (Main.rand.NextBool(4))
            {
                var mote = new LunarMoteParticle(
                    Projectile.Center, -Projectile.velocity * 0.05f,
                    Main.rand.NextFloat(0.15f, 0.3f),
                    Color.Lerp(new Color(135, 206, 250), new Color(200, 210, 255), Main.rand.NextFloat()),
                    Main.rand.Next(15, 25), 2f, 2.5f, hueShift: 0.008f);
                IncisorParticleHandler.SpawnParticle(mote);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            pierceCount++;

            // Spawn resonance music note at impact 窶・pulses and deals DoT for 1.5s
            target.AddBuff(ModContent.BuffType<LunarResonanceDebuff>(), 90); // 1.5 seconds

            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.3f, Volume = 0.5f }, target.Center);

            // Musical impact burst 窶・glowing notes erupt
            if (!Main.dedServ)
            {
                MoonlightVFXLibrary.SpawnMusicNotes(target.Center, count: 2, spread: 15f,
                    minScale: 0.5f, maxScale: 0.8f, lifetime: 45);

                // Resonance bloom at impact
                for (int i = 0; i < 4; i++)
                {
                    Color sparkColor = MulticolorLerp(Main.rand.NextFloat(), IncisorPalette);
                    var spark = new ConstellationSparkParticle(
                        target.Center, Main.rand.NextVector2Circular(5f, 5f),
                        true, Main.rand.Next(12, 20), Main.rand.NextFloat(0.12f, 0.25f),
                        sparkColor, new Vector2(0.6f, 1.3f));
                    IncisorParticleHandler.SpawnParticle(spark);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            var bloomTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom",
                AssetRequestMode.ImmediateLoad).Value;
            var crescentTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft",
                AssetRequestMode.ImmediateLoad).Value;

            float pulse = 0.85f + 0.15f * MathF.Sin(Projectile.ai[1] * 0.1f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Switch to Additive blending for glow/bloom layers
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Soft bloom underlayer
            Color glowColor = new Color(100, 140, 220) with { A = 0 };
            Main.spriteBatch.Draw(bloomTex, drawPos, null, glowColor * 0.35f * pulse,
                0f, bloomTex.Size() * 0.5f, 0.06f * Projectile.scale, SpriteEffects.None, 0f);

            // Crescent body — pale blue
            Color bodyColor = Color.Lerp(new Color(135, 206, 250), new Color(200, 210, 255),
                MathF.Sin(Projectile.ai[1] * 0.05f) * 0.5f + 0.5f) with { A = 0 };
            Main.spriteBatch.Draw(crescentTex, drawPos, null, bodyColor * 0.9f,
                Projectile.rotation, crescentTex.Size() * 0.5f, 0.3f * pulse,
                SpriteEffects.None, 0f);

            // White-hot core
            Main.spriteBatch.Draw(crescentTex, drawPos, null, Color.White with { A = 0 } * 0.6f,
                Projectile.rotation + MathHelper.PiOver4, crescentTex.Size() * 0.5f,
                0.15f * pulse, SpriteEffects.None, 0f);

            // Ribbon trail — afterimage sparkles along trail
            for (int i = 1; i < Math.Min(Projectile.oldPos.Length, 12); i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) break;
                float fade = 1f - i / 12f;
                fade *= fade;
                Vector2 oldDrawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;

                // Bloom trail ribbon
                Color ribbonColor = Color.Lerp(new Color(135, 206, 250), new Color(170, 140, 255), i / 12f) with { A = 0 };
                Main.spriteBatch.Draw(bloomTex, oldDrawPos, null, ribbonColor * fade * 0.25f,
                    0f, bloomTex.Size() * 0.5f, 0.035f * fade * Projectile.scale, SpriteEffects.None, 0f);

                // Crescent afterimage
                Main.spriteBatch.Draw(crescentTex, oldDrawPos, null, bodyColor * fade * 0.4f,
                    Projectile.oldRot[i], crescentTex.Size() * 0.5f, 0.25f * fade,
                    SpriteEffects.None, 0f);
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

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;
            // Death burst 窶・few sparkles
            for (int i = 0; i < 3; i++)
            {
                Color sc = MulticolorLerp(Main.rand.NextFloat(), IncisorPalette);
                var spark = new ConstellationSparkParticle(
                    Projectile.Center, Main.rand.NextVector2Circular(3f, 3f),
                    true, Main.rand.Next(8, 14), Main.rand.NextFloat(0.1f, 0.2f),
                    sc, new Vector2(0.5f, 1.4f), quickShrink: true);
                IncisorParticleHandler.SpawnParticle(spark);
            }
        }
    }
}
