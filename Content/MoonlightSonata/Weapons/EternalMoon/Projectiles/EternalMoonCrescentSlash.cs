using System;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Projectiles
{
    /// <summary>
    /// Crescent Slash — a wide visual slash projectile spawned at the target location
    /// after a successful Lunar Surge hit. Multiple are staggered in time for a cascading
    /// crescent slash effect. Deals secondary damage with a hitbox.
    /// Renders as a rotating crescent arc with bloom glow.
    /// </summary>
    public class EternalMoonCrescentSlash : ModProjectile
    {
        private const int SlashLifetime = 24;
        private float _initialRotation;

        public int TargetIndex => (int)Projectile.ai[0];

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/WideSoftEllipse";

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = SlashLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = SlashLifetime;
            Projectile.Opacity = 0f;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                _initialRotation = Main.rand.NextFloat(MathHelper.TwoPi);
                Projectile.localAI[0] = 1;
                SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.3f, Pitch = 0.5f }, Projectile.Center);
            }

            // Anchor to target if alive
            if (Main.npc.IndexInRange(TargetIndex) && Main.npc[TargetIndex].active)
                Projectile.Center = Main.npc[TargetIndex].Center;

            float progress = 1f - (Projectile.timeLeft / (float)SlashLifetime);

            // Rotate the slash arc
            Projectile.rotation = _initialRotation + progress * MathHelper.Pi * 0.6f;

            // Scale up quickly then hold
            Projectile.scale = progress < 0.3f ? progress / 0.3f : 1f;

            // Opacity: ramp up fast, hold, then fade
            Projectile.Opacity = progress < 0.15f ? progress / 0.15f :
                                 progress > 0.7f ? (1f - progress) / 0.3f : 1f;

            // Crescent spark trail
            if (Main.rand.NextBool(3) && !Main.dedServ)
            {
                Vector2 offset = Projectile.rotation.ToRotationVector2() * 50f * Projectile.scale;
                LunarParticleHandler.SpawnParticle(new CrescentSparkParticle(
                    Projectile.Center + offset, Main.rand.NextVector2Unit() * 2f,
                    Main.rand.NextFloat(0.2f, 0.4f), EternalMoonUtils.CrescentGlow, 10));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;

            // Draw the crescent slash arc
            Color slashColor = Color.Lerp(EternalMoonUtils.IceBlue, EternalMoonUtils.MoonWhite, 0.5f);
            slashColor.A = 0;

            Vector2 scale = new Vector2(2.5f, 0.5f) * Projectile.scale;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null,
                slashColor * Projectile.Opacity * 0.8f, Projectile.rotation,
                texture.Size() / 2f, scale, SpriteEffects.None, 0);

            // Wider glow layer
            Color glowColor = EternalMoonUtils.Violet;
            glowColor.A = 0;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null,
                glowColor * Projectile.Opacity * 0.3f, Projectile.rotation,
                texture.Size() / 2f, scale * 1.4f, SpriteEffects.None, 0);

            Lighting.AddLight(Projectile.Center, EternalMoonUtils.IceBlue.ToVector3() * Projectile.Opacity * 0.5f);

            return false;
        }
    }
}
