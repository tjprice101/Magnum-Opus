using System;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Buffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Projectiles
{
    /// <summary>
    /// Lunar Nova  Eexpanding ring AoE explosion on empowered slash hit.
    /// Spawns a shower of LunarMoteParticles and MoonlightMistParticles.
    /// A moonlight-themed supernova that radiates purple and silver light.
    /// </summary>
    public class LunarNova : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle";

        public override void SetDefaults()
        {
            Projectile.width = 260;
            Projectile.height = 260;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            // First frame: spawn burst of lunar mote cinders
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;

                // 18 lunar mote cinders radiating outward
                for (int i = 0; i < 18; i++)
                {
                    float angle = MathHelper.TwoPi * i / 18f + Main.rand.NextFloat(-0.15f, 0.15f);
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 10f);
                    Color c = IncisorUtils.MulticolorLerp(Main.rand.NextFloat(),
                        IncisorUtils.IncisorPalette);
                    var mote = new LunarMoteParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                        vel, Main.rand.NextFloat(0.4f, 0.7f), c, 40,
                        opacity: 1f, squishStrength: 2.5f, maxSquish: 4f, hueShift: 0.008f);
                    IncisorParticleHandler.SpawnParticle(mote);
                }
            }

            // Each frame: spawn gentle mist clouds
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Color mistColor = IncisorUtils.MulticolorLerp(Main.rand.NextFloat(),
                    new Color(90, 50, 160), new Color(170, 140, 255), new Color(135, 206, 250));
                var mist = new MoonlightMistParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(60f, 60f),
                    vel, mistColor, 35, Main.rand.NextFloat(0.4f, 0.8f),
                    0.7f, Main.rand.NextFloat(0.02f, 0.06f), glowing: true, hueShift: 0.005f);
                IncisorParticleHandler.SpawnParticle(mist);
            }

            // Lighting
            Lighting.AddLight(Projectile.Center, new Vector3(0.6f, 0.4f, 0.9f) * (1f - Projectile.timeLeft / 30f));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<LunarResonanceDebuff>(), 300);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom").Value;
            Texture2D ringTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 bloomOrigin = bloomTex.Size() / 2f;
            Vector2 ringOrigin = ringTex.Size() / 2f;

            // Lifetime progress: 0 at spawn, 1 at death
            float life = 1f - Projectile.timeLeft / 30f;
            // Smooth expansion curve (fast start, gentle end)
            float expand = (float)Math.Pow(life, 0.5);
            // Fade out over the last half of lifetime
            float fade = life < 0.5f ? 1f : 1f - (life - 0.5f) * 2f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer,
                null, Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Large expanding soft bloom pulse (background glow)
            Color pulseColor = IncisorUtils.MulticolorLerp(life,
                new Color(90, 50, 160), new Color(170, 140, 255), new Color(135, 206, 250));
            pulseColor.A = 0;
            float pulseScale = MathHelper.Lerp(0.05f, 0.8f, expand);
            Main.EntitySpriteDraw(bloomTex, drawPos, null, pulseColor * fade * 0.5f, 0f,
                bloomOrigin, pulseScale, SpriteEffects.None, 0);

            // Layer 2: Expanding ring (drawn as a soft circle that scales up)
            Color ringColor = Color.Lerp(new Color(170, 140, 255), new Color(230, 235, 255), expand);
            ringColor.A = 0;
            float ringScale = MathHelper.Lerp(0.1f, 1.6f, expand);
            float ringOpacity = fade * 0.6f;
            Main.EntitySpriteDraw(ringTex, drawPos, null, ringColor * ringOpacity, 0f,
                ringOrigin, ringScale, SpriteEffects.None, 0);

            // Layer 3: White-hot core flash (bright, shrinks as it expands)
            float coreScale = MathHelper.Lerp(0.15f, 0.04f, expand);
            float coreFade = life < 0.3f ? 1f : MathHelper.Lerp(1f, 0f, (life - 0.3f) / 0.7f);
            Main.EntitySpriteDraw(bloomTex, drawPos, null, Color.White * coreFade * 0.8f, 0f,
                bloomOrigin, coreScale, SpriteEffects.None, 0);

            // Layer 4: Secondary colored ring, slightly behind the main one
            Color ring2Color = new Color(135, 206, 250) { A = 0 };
            float ring2Scale = MathHelper.Lerp(0.05f, 1.3f, expand);
            Main.EntitySpriteDraw(ringTex, drawPos, null, ring2Color * fade * 0.3f,
                MathHelper.PiOver4 * life, ringOrigin, ring2Scale, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
