using System;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Buffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Projectiles
{
    /// <summary>
    /// Homing lunar beam projectile  Efired during swing.
    /// Flies straight initially, then homes on the nearest NPC.
    /// Draws itself with a sprite + bloom circle + layered sprite-based trail.
    /// Trail uses overlapping stretched BeamStreak1 segments rotated along velocity.
    /// </summary>
    public class LunarBeamProj : ModProjectile
    {
        public static int NoHomeTime = 24;

        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/SoftGlow3";

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
            // Purple-silver dust trail
            if (Main.rand.NextBool(3))
            {
                Color dustColor = IncisorUtils.MulticolorLerp(Main.rand.NextFloat(),
                    IncisorUtils.IncisorPalette);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
                    -Projectile.velocity * 0.3f, 0, dustColor);
                d.scale = 0.4f;
                d.noGravity = true;
                d.fadeIn = Main.rand.NextFloat() * 0.8f;
            }

            // Lighting
            Lighting.AddLight(Projectile.Center, new Vector3(0.4f, 0.3f, 0.7f));

            // Homing after NoHomeTime frames
            if (Projectile.timeLeft < 140 - NoHomeTime)
            {
                NPC target = Projectile.Center.ClosestNPCAt(1200f);
                if (target != null)
                {
                    Vector2 idealDir = Projectile.Center.SafeNormalize(Vector2.UnitX);
                    idealDir = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX));
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
            // Draw sprite
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Bloom circle behind
            Texture2D bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/MoonlightSonata/Shared/Orbs/SoftCircularBloomOrb").Value;
            Color bloomColor = IncisorUtils.MulticolorLerp(
                (Main.GlobalTimeWrappedHourly * 1.5f) % 1, IncisorUtils.IncisorPalette);
            bloomColor.A = 0;
            Main.EntitySpriteDraw(bloomTex, drawPos, null, bloomColor * 0.35f, 0f,
                bloomTex.Size() / 2f, 0.08f, SpriteEffects.None, 0);

            // Core sprite
            Color coreColor = IncisorUtils.MulticolorLerp(
                (Main.GlobalTimeWrappedHourly * 2f) % 1,
                new Color(170, 140, 255), new Color(220, 230, 255), new Color(135, 206, 250));
            coreColor.A = 0;
            Main.EntitySpriteDraw(tex, drawPos, null, coreColor * 0.9f, Projectile.rotation,
                origin, 0.15f, SpriteEffects.None, 0);

            // Draw sprite-based trail
            DrawBeamTrail();

            return false;
        }

        private void DrawBeamTrail()
        {
            // Count valid trail positions
            int count = 0;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) break;
                count++;
            }
            if (count < 2) return;

            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/SoftGlow3").Value;
            Texture2D streakTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX/Beams/BeamStreak1").Value;
            Vector2 glowOrigin = glowTex.Size() / 2f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer,
                null, Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Soft bloom afterimage circles along the trail path
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                float opacity = (1f - progress) * Projectile.Opacity * 0.45f;
                float scale = MathHelper.Lerp(0.1f, 0.025f, progress);

                Color trailColor = IncisorUtils.MulticolorLerp(progress,
                    new Color(170, 140, 255), new Color(220, 230, 255), new Color(135, 206, 250));
                trailColor.A = 0;

                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.EntitySpriteDraw(glowTex, drawPos, null, trailColor * opacity,
                    Projectile.oldRot[i], glowOrigin, scale, SpriteEffects.None, 0);
            }

            // Layer 2: Continuous beam core  Estretch a streak texture between consecutive positions
            for (int i = 0; i < count - 1; i++)
            {
                float progress = (float)i / count;
                float opacity = (1f - progress) * Projectile.Opacity * 0.7f;

                Vector2 start = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 end = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 diff = end - start;
                float segLength = diff.Length();
                if (segLength < 1f) continue;
                float rotation = diff.ToRotation();

                // Taper width from head to tail
                float width = MathHelper.Lerp(5f, 1.5f, progress);

                Color beamColor = IncisorUtils.MulticolorLerp(progress,
                    new Color(170, 140, 255), new Color(230, 235, 255), new Color(135, 206, 250));
                beamColor.A = 0;

                // Stretch the streak texture along the segment, rotated to match direction
                Vector2 scale = new Vector2(segLength / streakTex.Width, width / streakTex.Height);
                Main.EntitySpriteDraw(streakTex, start, null, beamColor * opacity,
                    rotation, new Vector2(0, streakTex.Height / 2f), scale, SpriteEffects.None, 0);
            }

            // Layer 3: Brighter narrow core on top for extra luminance
            for (int i = 0; i < count - 1; i++)
            {
                float progress = (float)i / count;
                float opacity = (1f - progress) * Projectile.Opacity * 0.5f;

                Vector2 start = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 end = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 diff = end - start;
                float segLength = diff.Length();
                if (segLength < 1f) continue;
                float rotation = diff.ToRotation();

                float width = MathHelper.Lerp(2.5f, 0.8f, progress);
                Color coreColor = Color.Lerp(new Color(230, 235, 255), Color.White, 0.6f);
                coreColor.A = 0;

                Vector2 scale = new Vector2(segLength / streakTex.Width, width / streakTex.Height);
                Main.EntitySpriteDraw(streakTex, start, null, coreColor * opacity,
                    rotation, new Vector2(0, streakTex.Height / 2f), scale, SpriteEffects.None, 0);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
