using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Seasons.Projectiles
{
    /// <summary>
    /// Vivaldi Seasonal Wave - Main projectile for Four Seasons Blade
    /// Changes appearance and effects based on the season (ai[0])
    /// </summary>
    public class VivaldiSeasonalWave : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color SummerGold = new Color(255, 215, 0);
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnBrown = new Color(139, 90, 43);
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        private static readonly Color WinterWhite = new Color(240, 250, 255);

        private int SeasonIndex => (int)Projectile.ai[0];

        private Color PrimaryColor => SeasonIndex switch
        {
            0 => SpringPink,
            1 => SummerGold,
            2 => AutumnOrange,
            _ => WinterBlue
        };

        private Color SecondaryColor => SeasonIndex switch
        {
            0 => SpringGreen,
            1 => SummerOrange,
            2 => AutumnBrown,
            _ => WinterWhite
        };

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 75;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 30;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Slow down over time
            Projectile.velocity *= 0.97f;

            // Season-specific trail
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f);
                Vector2 trailVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f);
                Color trailColor = Color.Lerp(PrimaryColor, SecondaryColor, Main.rand.NextFloat()) * 0.55f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.32f, 20, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Periodic flares
            if (Projectile.timeLeft % 5 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, PrimaryColor * 0.5f, 0.35f, 12);
            }

            Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * 0.6f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Season-specific effects
            switch (SeasonIndex)
            {
                case 0: // Spring
                    target.AddBuff(BuffID.Poisoned, 180);
                    break;
                case 1: // Summer
                    target.AddBuff(BuffID.OnFire3, 240);
                    target.AddBuff(BuffID.Daybreak, 120);
                    break;
                case 2: // Autumn
                    target.AddBuff(BuffID.CursedInferno, 200);
                    // Small life steal
                    Player owner = Main.player[Projectile.owner];
                    if (Main.rand.NextFloat() < 0.3f)
                    {
                        owner.Heal(Math.Max(1, damageDone / 25));
                    }
                    break;
                case 3: // Winter
                    target.AddBuff(BuffID.Frostburn2, 240);
                    if (Main.rand.NextFloat() < 0.2f)
                    {
                        target.AddBuff(BuffID.Frozen, 60);
                    }
                    break;
            }

            // Impact VFX
            CustomParticles.GenericFlare(target.Center, PrimaryColor, 0.55f, 18);
            CustomParticles.HaloRing(target.Center, SecondaryColor * 0.5f, 0.4f, 15);

            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(5f, 5f);
                Color burstColor = Color.Lerp(PrimaryColor, SecondaryColor, Main.rand.NextFloat()) * 0.5f;
                var burst = new GenericGlowParticle(target.Center, burstVel, burstColor, 0.25f, 16, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.5f;
                float trailScale = 0.6f * (1f - progress * 0.5f);
                Color trailColor = Color.Lerp(PrimaryColor, SecondaryColor, progress) * alpha;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            // Main glow layers
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.12f + 1f;
            spriteBatch.Draw(texture, drawPos, null, SecondaryColor * 0.35f, Projectile.rotation, origin, 0.9f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, PrimaryColor * 0.55f, Projectile.rotation, origin, 0.65f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.7f, Projectile.rotation, origin, 0.35f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, PrimaryColor, 0.65f, 22);
            CustomParticles.HaloRing(Projectile.Center, SecondaryColor * 0.5f, 0.45f, 18);

            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color burstColor = Color.Lerp(PrimaryColor, SecondaryColor, (float)i / 10f) * 0.55f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.3f, 20, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }
    }
}
