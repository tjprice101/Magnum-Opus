using System;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Buffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Utilities;
using Microsoft.Xna.Framework;
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
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/CircularMask";

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

        public override bool PreDraw(ref Color lightColor) => false;
    }
}
