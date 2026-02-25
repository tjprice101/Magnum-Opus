using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX.Core;

namespace MagnumOpus.Content.MoonlightSonata.Enemies
{
    /// <summary>
    /// Waning Deer Chandelier - A floating chandelier that orbits around the Waning Deer.
    /// Two to three of these gently follow the creature, providing ambient lighting.
    /// </summary>
    public class WaningDeerChandelierOrbiting : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Enemies/WaningDeer/WaningDeerFloat";

        private int ParentNPCIndex => (int)Projectile.ai[0];
        private float OrbitAngle
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        // Orbit parameters
        private const float OrbitRadiusX = 90f;
        private const float OrbitRadiusY = 55f;
        private const float OrbitSpeed = 0.018f;
        private const float BobSpeed = 0.035f;
        private const float BobAmount = 10f;

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 40;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 0f;
        }

        public override void AI()
        {
            // Check if parent Waning Deer still exists
            NPC parent = null;
            if (ParentNPCIndex >= 0 && ParentNPCIndex < Main.maxNPCs)
            {
                NPC potentialParent = Main.npc[ParentNPCIndex];
                if (potentialParent.active && potentialParent.type == ModContent.NPCType<WaningDeer>())
                {
                    parent = potentialParent;
                }
            }

            if (parent == null)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 60;

            // Update orbit angle
            OrbitAngle += OrbitSpeed;
            if (OrbitAngle > MathHelper.TwoPi)
                OrbitAngle -= MathHelper.TwoPi;

            // Calculate target position (elliptical orbit around parent)
            float bobOffset = (float)Math.Sin(Main.GameUpdateCount * BobSpeed + OrbitAngle) * BobAmount;
            Vector2 orbitOffset = new Vector2(
                (float)Math.Cos(OrbitAngle) * OrbitRadiusX,
                (float)Math.Sin(OrbitAngle) * OrbitRadiusY * 0.4f + bobOffset - 50f
            );

            Vector2 targetPos = parent.Center + orbitOffset;

            // Smooth movement to target position
            Vector2 direction = targetPos - Projectile.Center;
            float distance = direction.Length();

            if (distance > 2f)
            {
                float speed = Math.Min(distance * 0.08f, 7f);
                Projectile.velocity = direction.SafeNormalize(Vector2.Zero) * speed;
            }
            else
            {
                Projectile.velocity *= 0.5f;
            }

            // Gentle swaying rotation
            Projectile.rotation = (float)Math.Sin(Main.GameUpdateCount * 0.025f + OrbitAngle) * 0.08f;

            // Palette-based ambient lighting
            float lightPulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f + OrbitAngle * 2f) * 0.15f + 0.85f;
            Lighting.AddLight(Projectile.Center, MoonlightVFXLibrary.IceBlue.ToVector3() * 0.5f * lightPulse +
                MoonlightVFXLibrary.DarkPurple.ToVector3() * 0.3f * lightPulse);

            // Occasional gentle particles — palette colors
            if (Main.rand.NextBool(18))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Color dustColor = Main.rand.NextBool() ? MoonlightVFXLibrary.DarkPurple : MoonlightVFXLibrary.IceBlue;
                Dust glow = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 150, dustColor, 0.6f);
                glow.noGravity = true;
                glow.velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1f, -0.3f));
                glow.fadeIn = 0.5f;
            }

            // Snow sparkle particles
            if (Main.rand.NextBool(25))
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(12f, 24f), 1, 1, DustID.BlueFairy, 0f, 0f, 0, default, 0.5f);
                sparkle.noGravity = true;
                sparkle.velocity = new Vector2(0f, Main.rand.NextFloat(-0.5f, 0f));
            }

            // Occasional snowflake
            if (Main.rand.NextBool(40))
            {
                Dust snow = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Snow, 0f, 0f, 100, default, 0.6f);
                snow.noGravity = true;
                snow.velocity = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(0.2f, 0.5f));
            }

            // Music notes — sparse for ambient chandelier
            if (Main.rand.NextBool(30))
            {
                MoonlightVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 8f, 0.55f, 0.72f, 28);
            }

            // Unified chandelier ambient VFX
            MoonlightEnemyVFX.ChandelierAmbientVFX(Projectile);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f + OrbitAngle * 2f) * 0.3f + 0.7f;
            float glowMult = pulse;

            // === 4-layer {A=0} bloom stack ===
            // Layer 1: Outer dark purple aura
            Color outerGlow = (MoonlightVFXLibrary.DarkPurple with { A = 0 }) * 0.20f * glowMult;
            Main.EntitySpriteDraw(texture, drawPos, null, outerGlow, Projectile.rotation, origin, Projectile.scale * 1.15f, SpriteEffects.None, 0);

            // Layer 2: Mid violet bloom
            Color midGlow = (MoonlightVFXLibrary.Violet with { A = 0 }) * 0.25f * glowMult;
            Main.EntitySpriteDraw(texture, drawPos, null, midGlow, Projectile.rotation, origin, Projectile.scale * 1.08f, SpriteEffects.None, 0);

            // Layer 3: Inner ice blue glow
            Color innerGlow = (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.30f * glowMult;
            Main.EntitySpriteDraw(texture, drawPos, null, innerGlow, Projectile.rotation, origin, Projectile.scale * 1.04f, SpriteEffects.None, 0);

            // Layer 4: White core
            Color coreGlow = (MoonlightVFXLibrary.MoonWhite with { A = 0 }) * 0.15f * glowMult;
            Main.EntitySpriteDraw(texture, drawPos, null, coreGlow, Projectile.rotation, origin, Projectile.scale * 1.02f, SpriteEffects.None, 0);

            // Draw main sprite with enhanced brightness
            Color drawColor = new Color(255, 255, 255, 220);
            Main.EntitySpriteDraw(texture, drawPos, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 200);
        }

        public override void OnKill(int timeLeft)
        {
            // Chandelier death — subtle themed dissipation
            MoonlightVFXLibrary.ProjectileImpact(Projectile.Center, 0.4f);
            for (int i = 0; i < 6; i++)
            {
                Color dustColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple, MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(3f, 3f), 0, dustColor, 1.0f);
                d.noGravity = true;
            }

            // Unified chandelier death VFX
            MoonlightEnemyVFX.ChandelierDeathVFX(Projectile.Center);
        }
    }
}
