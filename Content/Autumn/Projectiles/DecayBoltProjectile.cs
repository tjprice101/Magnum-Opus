using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Autumn.Projectiles
{
    /// <summary>
    /// Decay Bolt - Main projectile for Withering Grimoire
    /// Creates entropic fields on impact
    /// </summary>
    public class DecayBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow2";
        
        private static readonly Color DecayPurple = new Color(100, 50, 120);
        private static readonly Color DeathGreen = new Color(80, 120, 60);
        private static readonly Color WitherBrown = new Color(90, 60, 40);

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // === VFX VARIATION #18: ENTROPIC SPIRAL TRAIL ===
            // Decaying energy spirals behind in a vortex pattern
            if (Main.rand.NextBool(2))
            {
                float spiralAngle = Main.GameUpdateCount * 0.2f;
                Vector2 spiralOffset = spiralAngle.ToRotationVector2() * 7f;
                Vector2 trailVel = -Projectile.velocity * 0.1f + spiralAngle.ToRotationVector2() * 1.2f;
                Color trailColor = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.55f;
                var trail = new GenericGlowParticle(Projectile.Center + spiralOffset, trailVel, trailColor, 0.3f, 22, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // === VFX VARIATION #19: ARCANE GLYPH ORBIT ===
            // Mystical glyphs orbit the bolt
            if (Main.GameUpdateCount % 10 == 0)
            {
                float glyphAngle = Main.GameUpdateCount * 0.08f;
                for (int g = 0; g < 2; g++)
                {
                    float thisGlyphAngle = glyphAngle + MathHelper.Pi * g;
                    Vector2 glyphPos = Projectile.Center + thisGlyphAngle.ToRotationVector2() * 14f;
                    CustomParticles.Glyph(glyphPos, DecayPurple * 0.7f, 0.28f, Main.rand.Next(1, 13));
                }
            }

            // === VFX VARIATION #20: DEATH MOTE CLOUD ===
            // Tiny floating motes of decay surround the projectile
            if (Main.rand.NextBool(3))
            {
                float moteAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float moteRadius = Main.rand.NextFloat(10f, 20f);
                Vector2 motePos = Projectile.Center + moteAngle.ToRotationVector2() * moteRadius;
                Vector2 moteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.5f, -0.3f));
                Color moteColor = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.4f;
                var mote = new GenericGlowParticle(motePos, moteVel, moteColor, 0.14f, 28, true);
                MagnumParticleHandler.SpawnParticle(mote);
            }

            // Decaying notes - VISIBLE (scale 0.75f)
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2.5f, -1f));
                Color noteColor = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat());
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.75f, 38);
            }
            
            // Additional glyph accents
            if (Main.rand.NextBool(8))
            {
                CustomParticles.Glyph(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), DecayPurple * 0.65f, 0.38f, Main.rand.Next(1, 13));
            }

            // Core glow - pulsing
            float corePulse = 0.35f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.08f;
            CustomParticles.GenericFlare(Projectile.Center, DecayPurple * corePulse, 0.24f, 6);

            Lighting.AddLight(Projectile.Center, DecayPurple.ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Life Drain: 5% heal
            Player owner = Main.player[Projectile.owner];
            int healAmount = Math.Max(1, damageDone / 20);
            owner.Heal(healAmount);

            // Apply withering debuff
            target.AddBuff(BuffID.CursedInferno, 180);

            // Create entropic field
            if (Main.myPlayer == Projectile.owner && Projectile.penetrate <= 1)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_OnHit(target),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<EntropicField>(),
                    Projectile.damage / 4,
                    0f,
                    Projectile.owner
                );
            }

            // Hit VFX
            CustomParticles.GenericFlare(target.Center, DeathGreen, 0.5f, 16);
            CustomParticles.GenericFlare(target.Center, DecayPurple, 0.45f, 15);
            CustomParticles.GenericFlare(target.Center, DecayPurple * 0.6f, 0.32f, 11);
            // Decay wisp burst
            for (int wisp = 0; wisp < 4; wisp++)
            {
                float wispAngle = MathHelper.TwoPi * wisp / 4f;
                Vector2 wispPos = target.Center + wispAngle.ToRotationVector2() * 12f;
                CustomParticles.GenericFlare(wispPos, DecayPurple * 0.7f, 0.18f, 10);
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                Color sparkColor = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.5f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.22f, 18, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // ☁EMUSICAL IMPACT - VISIBLE decaying melody (scale 0.75f)
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * 3.5f;
                Color noteColor = Color.Lerp(DecayPurple, DeathGreen, i / 6f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.75f, 32);
            }
            
            // Glyph burst for arcane impact
            CustomParticles.GlyphBurst(target.Center, DecayPurple, 4, 4f);
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, DecayPurple, 0.45f, 16);
            CustomParticles.GenericFlare(Projectile.Center, DeathGreen, 0.4f, 15);
            CustomParticles.GenericFlare(Projectile.Center, DeathGreen * 0.6f, 0.28f, 11);
            // Decay wisp burst
            for (int wisp = 0; wisp < 4; wisp++)
            {
                float wispAngle = MathHelper.TwoPi * wisp / 4f;
                Vector2 wispPos = Projectile.Center + wispAngle.ToRotationVector2() * 11f;
                CustomParticles.GenericFlare(wispPos, DeathGreen * 0.7f, 0.16f, 9);
            }

            for (int i = 0; i < 8; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(4f, 4f);
                Color burstColor = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.5f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.22f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // ☁EMUSICAL FINALE - VISIBLE fading requiem (scale 0.8f)
            for (int i = 0; i < 7; i++)
            {
                float angle = MathHelper.TwoPi * i / 7f;
                Vector2 noteVel = angle.ToRotationVector2() * 4f;
                ThemedParticles.MusicNote(Projectile.Center, noteVel, DecayPurple, 0.8f, 38);
            }
            
            // Glyph circle on death
            CustomParticles.GlyphBurst(Projectile.Center, DeathGreen, 5, 3.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.1f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, drawPos, null, DecayPurple * 0.4f, 0f, origin, 0.35f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, DeathGreen * 0.5f, 0f, origin, 0.22f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.5f, 0f, origin, 0.1f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Entropic Field - Damaging zone created on bolt impact
    /// </summary>
    public class EntropicField : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo2";
        
        private static readonly Color DecayPurple = new Color(100, 50, 120);
        private static readonly Color DeathGreen = new Color(80, 120, 60);

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            float lifeProgress = 1f - (float)Projectile.timeLeft / 180f;
            float fadeOut = Projectile.timeLeft < 30 ? (float)Projectile.timeLeft / 30f : 1f;

            // Swirling decay particles
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(10f, 40f);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * dist;
                Vector2 vel = (angle + MathHelper.PiOver2).ToRotationVector2() * Main.rand.NextFloat(1f, 3f);
                vel += new Vector2(0, -0.5f);
                Color color = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.5f * fadeOut;
                var particle = new GenericGlowParticle(pos, vel, color, 0.25f, 25, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Central glow
            CustomParticles.GenericFlare(Projectile.Center, DecayPurple * 0.3f * fadeOut, 0.3f, 6);

            // ☁EMUSICAL NOTATION - Haunting notes rise from the decay field - VISIBLE SCALE 0.7f+
            if (Main.rand.NextBool(8))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-2f, -0.5f));
                Color noteColor = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * fadeOut;
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(25f, 25f), noteVel, noteColor, 0.7f, 30);
                
                // Decay glyph accent
                CustomParticles.Glyph(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), DeathGreen * 0.3f * fadeOut, 0.18f, -1);
            }

            Lighting.AddLight(Projectile.Center, DecayPurple.ToVector3() * 0.35f * fadeOut);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Life Drain: 5% heal
            Player owner = Main.player[Projectile.owner];
            int healAmount = Math.Max(1, damageDone / 20);
            owner.Heal(healAmount);

            target.AddBuff(BuffID.CursedInferno, 60);

            // Small hit VFX
            for (int i = 0; i < 3; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(3f, 3f);
                Color sparkColor = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.4f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.18f, 12, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // ☁EMUSICAL IMPACT - Entropic note on hit - VISIBLE SCALE 0.68f+
            if (Main.rand.NextBool(2))
            {
                ThemedParticles.MusicNote(target.Center, new Vector2(0, -1.5f), DeathGreen, 0.68f, 25);
                
                // Decay glyph burst
                CustomParticles.Glyph(target.Center, DecayPurple * 0.4f, 0.2f, -1);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/GlowingHalo2").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.1f + 1f;
            float fadeOut = Projectile.timeLeft < 30 ? (float)Projectile.timeLeft / 30f : 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, drawPos, null, DecayPurple * 0.2f * fadeOut, 0f, origin, 0.8f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, DeathGreen * 0.15f * fadeOut, 0f, origin, 0.6f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Withering Wave - Charged attack projectile
    /// </summary>
    public class WitheringWave : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/ParticleTrail2";
        
        private static readonly Color DecayPurple = new Color(100, 50, 120);
        private static readonly Color DeathGreen = new Color(80, 120, 60);
        private static readonly Color WitherBrown = new Color(90, 60, 40);

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0.97f;

            // Expand hitbox as it travels
            if (Projectile.ai[0] < 30)
            {
                Projectile.ai[0]++;
                Projectile.width = (int)MathHelper.Lerp(80, 150, Projectile.ai[0] / 30f);
                Projectile.height = (int)MathHelper.Lerp(40, 80, Projectile.ai[0] / 30f);
            }

            float fadeOut = Projectile.timeLeft < 30 ? (float)Projectile.timeLeft / 30f : 1f;

            // Intense wave particles
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(Projectile.width / 3f, Projectile.height / 3f);
                Vector2 particleVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f);
                Color color = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.5f * fadeOut;
                var particle = new GenericGlowParticle(Projectile.Center + offset, particleVel, color, 0.35f, 22, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Core wave glow
            CustomParticles.GenericFlare(Projectile.Center, DecayPurple * 0.4f * fadeOut, 0.4f, 6);

            // ☁EMUSICAL NOTATION - Withering notes scatter from the wave - VISIBLE SCALE 0.72f+
            if (Main.rand.NextBool(4))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f));
                Color noteColor = Color.Lerp(DecayPurple, WitherBrown, Main.rand.NextFloat()) * fadeOut;
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(20f, 10f), noteVel, noteColor, 0.72f, 28);
                
                // Wither glyph accent
                CustomParticles.Glyph(Projectile.Center, WitherBrown * 0.3f * fadeOut, 0.18f, -1);
            }

            Lighting.AddLight(Projectile.Center, DeathGreen.ToVector3() * 0.6f * fadeOut);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Life Drain: 5% heal
            Player owner = Main.player[Projectile.owner];
            int healAmount = Math.Max(1, damageDone / 20);
            owner.Heal(healAmount);

            // Heavy debuffs
            target.AddBuff(BuffID.CursedInferno, 300);
            target.AddBuff(BuffID.Ichor, 240);

            // Create entropic field
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_OnHit(target),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<EntropicField>(),
                    Projectile.damage / 3,
                    0f,
                    Projectile.owner
                );
            }

            // Heavy hit VFX
            CustomParticles.GenericFlare(target.Center, DeathGreen, 0.7f, 20);
            CustomParticles.GenericFlare(target.Center, DecayPurple, 0.6f, 18);
            CustomParticles.GenericFlare(target.Center, DecayPurple * 0.6f, 0.45f, 14);
            CustomParticles.GenericFlare(target.Center, WitherBrown, 0.45f, 16);
            CustomParticles.GenericFlare(target.Center, WitherBrown * 0.5f, 0.3f, 12);
            // Heavy decay wisp burst
            for (int wisp = 0; wisp < 5; wisp++)
            {
                float wispAngle = MathHelper.TwoPi * wisp / 5f;
                Vector2 wispPos = target.Center + wispAngle.ToRotationVector2() * 16f;
                Color wispColor = Color.Lerp(DecayPurple, WitherBrown, (float)wisp / 5f) * 0.7f;
                CustomParticles.GenericFlare(wispPos, wispColor, 0.22f, 12);
            }

            for (int i = 0; i < 10; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(6f, 6f);
                Color sparkColor = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.6f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.28f, 20, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // ☁EMUSICAL IMPACT - Powerful withering chord
            ThemedParticles.MusicNoteBurst(target.Center, Color.Lerp(DecayPurple, WitherBrown, 0.5f), 8, 4.5f);
        }

        public override void OnKill(int timeLeft)
        {
            // Dissipation VFX
            CustomParticles.GenericFlare(Projectile.Center, DecayPurple, 0.6f, 20);
            CustomParticles.GenericFlare(Projectile.Center, DeathGreen, 0.55f, 18);
            CustomParticles.GenericFlare(Projectile.Center, DeathGreen * 0.6f, 0.4f, 14);
            // Decay wisp burst
            for (int wisp = 0; wisp < 5; wisp++)
            {
                float wispAngle = MathHelper.TwoPi * wisp / 5f;
                Vector2 wispPos = Projectile.Center + wispAngle.ToRotationVector2() * 14f;
                CustomParticles.GenericFlare(wispPos, DeathGreen * 0.7f, 0.2f, 11);
            }

            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color burstColor = Color.Lerp(DecayPurple, DeathGreen, (float)i / 14f) * 0.5f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.28f, 22, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // ☁EMUSICAL FINALE - Grand withering crescendo
            ThemedParticles.MusicNoteBurst(Projectile.Center, WitherBrown, 10, 5f);
            ThemedParticles.MusicNoteRing(Projectile.Center, DecayPurple, 45f, 6);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/ParticleTrail2").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.12f + 1f;
            float fadeOut = Projectile.timeLeft < 30 ? (float)Projectile.timeLeft / 30f : 1f;
            float expansion = Projectile.ai[0] / 30f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Wave shape - stretched horizontally
            float stretch = 2.5f + expansion;
            spriteBatch.Draw(texture, drawPos, null, DecayPurple * 0.4f * fadeOut, Projectile.rotation, origin, new Vector2(0.5f * stretch, 0.25f) * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, DeathGreen * 0.35f * fadeOut, Projectile.rotation, origin, new Vector2(0.4f * stretch, 0.2f) * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.4f * fadeOut, Projectile.rotation, origin, new Vector2(0.25f * stretch, 0.12f) * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
