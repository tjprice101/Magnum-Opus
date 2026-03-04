using System;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Buffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Projectiles
{
    /// <summary>
    /// Homing lunar beam projectile 遯ｶ繝ｻfired during swing.
    /// Flies straight initially, then homes on the nearest NPC.
    /// Draws itself with a sprite + bloom circle + layered sprite-based trail.
    /// Trail uses overlapping stretched BeamStreak1 segments rotated along velocity.
    /// </summary>
    public class LunarBeamProj : ModProjectile
    {
        public static int NoHomeTime = 24;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 140;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            // Custom constellation spark trail (replaces generic dust)
            if (Main.rand.NextBool(5))
            {
                Color sparkColor = IncisorUtils.MulticolorLerp(Main.rand.NextFloat(),
                    IncisorUtils.IncisorPalette);
                var spark = new ConstellationSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    false, Main.rand.Next(10, 18), Main.rand.NextFloat(0.08f, 0.18f),
                    sparkColor, new Vector2(0.5f, 1.3f), quickShrink: true);
                IncisorParticleHandler.SpawnParticle(spark);
            }

            // Lighting
            Lighting.AddLight(Projectile.Center, new Vector3(0.4f, 0.3f, 0.7f));

            // Homing after NoHomeTime frames
            if (Projectile.timeLeft < 140 - NoHomeTime)
            {
                NPC target = Projectile.Center.ClosestNPCAt(1200f);
                if (target != null)
                {
                    Vector2 idealDir = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX));
                    float homeStrength = MathHelper.Clamp((140 - Projectile.timeLeft - NoHomeTime) / 40f, 0f, 1f) * 0.08f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealDir * Projectile.velocity.Length(), homeStrength);
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<LunarResonanceDebuff>(), 180);

            // Spawn a constellation slash creator at target
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center,
                    Projectile.velocity * 0.1f, ModContent.ProjectileType<ConstellationSlashCreator>(),
                    (int)(Projectile.damage * 0.6f), 0f, Projectile.owner, target.whoAmI, 25);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad).Value;
            Texture2D streakTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/BeamTextures/HorizontalBeamStreakSegment", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 bloomOrigin = bloomTex.Size() / 2f;

            // Count valid trail positions
            int count = 0;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) break;
                count++;
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer,
                null, Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Beam body streak segments
            if (count >= 2)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    float progress = (float)i / count;
                    float opacity = (1f - progress) * Projectile.Opacity * 0.65f;

                    Vector2 start = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                    Vector2 end = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                    Vector2 diff = end - start;
                    float segLength = diff.Length();
                    if (segLength < 1f) continue;
                    float rotation = diff.ToRotation();

                    float width = MathHelper.Lerp(5f, 1f, progress);

                    Color beamColor = IncisorUtils.MulticolorLerp(progress,
                        new Color(170, 140, 255), new Color(230, 235, 255), new Color(135, 206, 250));
                    beamColor.A = 0;

                    Vector2 scale = new Vector2(segLength / streakTex.Width, width / streakTex.Height);
                    Main.EntitySpriteDraw(streakTex, start, null, beamColor * opacity,
                        rotation, new Vector2(0, streakTex.Height / 2f), scale, SpriteEffects.None, 0);
                }

                // Layer 2: Brighter narrow white-hot core
                for (int i = 0; i < count - 1; i++)
                {
                    float progress = (float)i / count;
                    float opacity = (1f - progress) * Projectile.Opacity * 0.4f;

                    Vector2 start = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                    Vector2 end = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                    Vector2 diff = end - start;
                    float segLength = diff.Length();
                    if (segLength < 1f) continue;
                    float rotation = diff.ToRotation();

                    float width = MathHelper.Lerp(2.5f, 0.5f, progress);
                    Color coreColor = Color.Lerp(new Color(230, 235, 255), Color.White, 0.6f);
                    coreColor.A = 0;

                    Vector2 scale = new Vector2(segLength / streakTex.Width, width / streakTex.Height);
                    Main.EntitySpriteDraw(streakTex, start, null, coreColor * opacity,
                        rotation, new Vector2(0, streakTex.Height / 2f), scale, SpriteEffects.None, 0);
                }
            }

            // Layer 3: Bloom head glow
            Color headColor = IncisorUtils.MulticolorLerp(
                (Main.GlobalTimeWrappedHourly * 2f) % 1,
                new Color(170, 140, 255), new Color(220, 230, 255), new Color(135, 206, 250));
            headColor.A = 0;
            Main.EntitySpriteDraw(bloomTex, drawPos, null, headColor * 0.5f, 0f,
                bloomOrigin, 0.12f, SpriteEffects.None, 0);
            // White-hot center point
            Main.EntitySpriteDraw(bloomTex, drawPos, null, Color.White * 0.7f, 0f,
                bloomOrigin, 0.05f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}