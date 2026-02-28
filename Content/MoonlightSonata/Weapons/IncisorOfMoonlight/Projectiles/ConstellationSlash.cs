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
    /// Wide invisible slash damage hitbox  Espawned by ConstellationSlashCreator.
    /// Line collision along velocity direction. Applies Lunar Resonance debuff.
    /// Spawns ConstellationSparkParticles on creation.
    /// </summary>
    public class ConstellationSlash : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/HardCircleMask";

        public override void SetDefaults()
        {
            Projectile.width = 512;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 35;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 35;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            // First frame: spawn constellation sparks
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                for (int i = 0; i < 6; i++)
                {
                    Vector2 sparkVel = Projectile.velocity.RotatedByRandom(MathHelper.Pi * 0.6) * Main.rand.NextFloat(8f, 18f);
                    Color sparkColor = IncisorUtils.MulticolorLerp(Main.rand.NextFloat(),
                        new Color(170, 140, 255), new Color(220, 230, 255), new Color(135, 206, 250));
                    var spark = new ConstellationSparkParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                        sparkVel, true, 25, Main.rand.NextFloat(0.2f, 0.45f),
                        sparkColor, new Vector2(0.4f, 1.8f), quickShrink: true);
                    IncisorParticleHandler.SpawnParticle(spark);
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = Projectile.Center - Projectile.velocity * 256f;
            Vector2 end = Projectile.Center + Projectile.velocity * 256f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 24f, ref _);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<LunarResonanceDebuff>(), 300);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}
