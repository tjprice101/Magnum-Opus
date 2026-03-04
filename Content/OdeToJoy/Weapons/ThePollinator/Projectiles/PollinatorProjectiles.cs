using MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Projectiles
{
    /// <summary>
    /// PollenShotProjectile — primary fire. Pollen-laden projectile with billowing pollen cloud trail.
    /// On hit: applies Pollinated debuff. SmokeFoundation-style pollen rendering.
    /// </summary>
    public class PollenShotProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // SmokeFoundation-style pollen cloud trail
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position - new Vector2(4), Projectile.width + 8, Projectile.height + 8, DustID.GoldFlame, -Projectile.velocity.X * 0.1f, -Projectile.velocity.Y * 0.1f, 150, PollinatorTextures.BloomGold, 0.8f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
                d.velocity *= 0.5f;
            }
            // Blossom accent sparkles
            if (Main.rand.NextBool(6))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center - new Vector2(2), 4, 4, DustID.YellowTorch, 0f, 0f, 100, PollinatorTextures.JubilantLight, 0.5f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<PollinatedDebuff>(), 600);

            // Pollen burst on impact
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustDirect(target.Center - new Vector2(4), 8, 8, DustID.GoldFlame, vel.X, vel.Y, 120, PollinatorTextures.BloomGold, 1.0f);
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glowTex = PollinatorTextures.SoftGlow;
            Texture2D sparkleTex = PollinatorTextures.OJBlossomSparkle;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 sparkleOrigin = sparkleTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float fade = MathHelper.Clamp((300 - Projectile.timeLeft) / 8f, 0f, 1f) * MathHelper.Clamp(Projectile.timeLeft / 20f, 0f, 1f);
            float pulse = 0.85f + 0.15f * (float)Math.Sin(Projectile.ai[0]++ * 0.1f);
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: PollenDrift shader — seed trail body ──
            Effect pollenShader = OdeToJoyShaders.PollenDrift;
            if (pollenShader != null)
            {
                OdeToJoyShaders.SetPollenParams(pollenShader, time, PollinatorTextures.BloomGold,
                    PollinatorTextures.RadiantAmber, fade * 0.5f * pulse, 1.8f, 0.25f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, pollenShader, "PollenTrailTechnique");
                sb.Draw(glowTex, drawPos, null, Color.White * fade * pulse, Projectile.rotation,
                    glowOrigin, 0.4f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive bloom layers ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            sb.Draw(glowTex, drawPos, null, PollinatorTextures.BloomGold * fade * 0.35f * pulse, 0f,
                glowOrigin, 0.45f, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, PollinatorTextures.RadiantAmber * fade * 0.5f, 0f,
                glowOrigin, 0.22f, SpriteEffects.None, 0f);
            sb.Draw(sparkleTex, drawPos, null, PollinatorTextures.JubilantLight * fade * 0.7f,
                Projectile.rotation, sparkleOrigin, 0.5f, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, PollinatorTextures.PureJoyWhite * fade * 0.4f, 0f,
                glowOrigin, 0.1f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }

    /// <summary>
    /// MassBloomProjectile — golden AoE explosion on pollinated enemy death.
    /// ExplosionParticlesFoundation RadialScatter + ImpactFoundation ripple rings.
    /// Spawns HomingSeedProjectiles and GoldenFieldProjectile.
    /// </summary>
    public class MassBloomProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int _timer;

        public override void SetDefaults()
        {
            Projectile.width = 160;
            Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 25;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 25;
        }

        public override void AI()
        {
            _timer++;

            if (_timer == 1)
            {
                // Spawn 3 homing seeds at non-pollinated enemies
                if (Main.myPlayer == Projectile.owner)
                {
                    int seedCount = 0;
                    for (int i = 0; i < Main.maxNPCs && seedCount < 3; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                        if (npc.HasBuff(ModContent.BuffType<PollinatedDebuff>())) continue;
                        if (Vector2.Distance(Projectile.Center, npc.Center) > 640f) continue;

                        Vector2 dir = (npc.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * 6f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, dir,
                            ModContent.ProjectileType<HomingSeedProjectile>(), Projectile.damage / 2, 2f, Projectile.owner);
                        seedCount++;
                    }

                    // Spawn golden field zone
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                        ModContent.ProjectileType<GoldenFieldProjectile>(), 0, 0f, Projectile.owner);
                }

                // 55 radial pollen/petal burst particles
                for (int i = 0; i < 55; i++)
                {
                    float angle = MathHelper.TwoPi * i / 55f;
                    float speed = 3f + Main.rand.NextFloat() * 5f;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                    Color sparkColor = PollinatorTextures.MassBloomColors[i % PollinatorTextures.MassBloomColors.Length];
                    Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.GoldFlame, vel.X, vel.Y, 100, sparkColor, 1.1f);
                    d.noGravity = true;
                    d.fadeIn = 1.6f;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<PollinatedDebuff>(), 600);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_timer < 1) return false;

            SpriteBatch sb = Main.spriteBatch;
            Texture2D ringTex = PollinatorTextures.OJPowerRing;
            Texture2D floralTex = PollinatorTextures.OJFloralImpact;
            Texture2D glowTex = PollinatorTextures.SoftGlow;
            Vector2 ringOrigin = ringTex.Size() / 2f;
            Vector2 floralOrigin = floralTex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float progress = _timer / 25f;
            float fadeOut = 1f - progress;
            float expand = 0.4f + progress * 1.2f;
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: PollenDrift BloomDetonation shader ──
            Effect pollenShader = OdeToJoyShaders.PollenDrift;
            if (pollenShader != null)
            {
                OdeToJoyShaders.SetPollenParams(pollenShader, time, PollinatorTextures.BloomGold,
                    PollinatorTextures.PetalPink, fadeOut * 0.5f, 2.2f, expand * 0.25f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, pollenShader, "BloomDetonationTechnique");
                sb.Draw(glowTex, drawPos, null, Color.White * fadeOut, progress * 2f, glowOrigin,
                    expand * 0.6f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            sb.Draw(ringTex, drawPos, null, PollinatorTextures.BloomGold * fadeOut * 0.55f, progress * 2f,
                ringOrigin, expand * 0.7f, SpriteEffects.None, 0f);
            sb.Draw(floralTex, drawPos, null, PollinatorTextures.PetalPink * fadeOut * 0.4f, -progress,
                floralOrigin, expand * 0.5f, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, PollinatorTextures.BloomGold * fadeOut * 0.25f, 0f, glowOrigin,
                expand * 1.0f, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, PollinatorTextures.JubilantLight * fadeOut * 0.5f, 0f, glowOrigin,
                expand * 0.2f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }

    /// <summary>
    /// HomingSeedProjectile — homing seed from Mass Bloom.
    /// Applies Pollinated debuff to non-pollinated targets.
    /// </summary>
    public class HomingSeedProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Homing toward nearest non-pollinated enemy
            float homingRange = 400f;
            NPC closest = null;
            float closestDist = homingRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (npc.HasBuff(ModContent.BuffType<PollinatedDebuff>())) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist) { closestDist = dist; closest = npc; }
            }
            if (closest != null)
            {
                Vector2 dir = (closest.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * 8f, 0.06f);
            }

            // Golden sparkle trail
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 130, PollinatorTextures.BloomGold, 0.5f);
                d.noGravity = true;
                d.velocity *= 0.2f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<PollinatedDebuff>(), 600);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D sparkleTex = PollinatorTextures.OJBlossomSparkle;
            Texture2D glowTex = PollinatorTextures.SoftGlow;
            Vector2 sparkleOrigin = sparkleTex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float fade = MathHelper.Clamp(Projectile.timeLeft / 30f, 0f, 1f);
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: GardenBloom shader accent ──
            Effect bloomShader = OdeToJoyShaders.GardenBloom;
            if (bloomShader != null)
            {
                OdeToJoyShaders.SetBloomParams(bloomShader, time * 1.5f, PollinatorTextures.BloomGold,
                    PollinatorTextures.JubilantLight, fade * 0.4f, 1.5f, 0.2f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, bloomShader, "GardenBloomTechnique");
                sb.Draw(glowTex, drawPos, null, Color.White * fade, Projectile.rotation, glowOrigin,
                    0.25f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive bloom ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            sb.Draw(glowTex, drawPos, null, PollinatorTextures.BloomGold * fade * 0.35f, 0f, glowOrigin,
                0.28f, SpriteEffects.None, 0f);
            sb.Draw(sparkleTex, drawPos, null, PollinatorTextures.JubilantLight * fade * 0.6f,
                Projectile.rotation, sparkleOrigin, 0.35f, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, PollinatorTextures.PureJoyWhite * fade * 0.3f, 0f, glowOrigin,
                0.08f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }

    /// <summary>
    /// GoldenFieldProjectile — persistent healing zone from Mass Bloom.
    /// Heals allies 3 HP/s, grants +5% damage within 3 tile radius. 5s duration.
    /// MaskFoundation-style rendering with FBM+PerlinFlow golden field.
    /// </summary>
    public class GoldenFieldProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int _timer;

        public override void SetDefaults()
        {
            Projectile.width = 96;
            Projectile.height = 96;
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300; // 5 seconds
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            _timer++;
            Projectile.velocity = Vector2.Zero;

            // Heal allies within range
            if (_timer % 60 == 0) // Every second
            {
                float healRange = 48f; // 3 tiles
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player p = Main.player[i];
                    if (!p.active || p.dead) continue;
                    if (Vector2.Distance(Projectile.Center, p.Center) <= healRange)
                    {
                        p.Heal(3);
                    }
                }
            }

            // Ambient pollen particles
            if (Main.rand.NextBool(4))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat() * 48f;
                Vector2 pos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;
                Dust d = Dust.NewDustDirect(pos, 1, 1, DustID.GoldFlame, 0f, -0.5f, 150, PollinatorTextures.BloomGold, 0.5f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glowTex = PollinatorTextures.SoftGlow;
            Texture2D maskTex = PollinatorTextures.CircularMask;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 maskOrigin = maskTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float fadeIn = MathHelper.Clamp(_timer / 15f, 0f, 1f);
            float fadeOut = MathHelper.Clamp(Projectile.timeLeft / 30f, 0f, 1f);
            float fade = fadeIn * fadeOut;
            float pulse = 0.85f + 0.15f * (float)Math.Sin(_timer * 0.06f);
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: CelebrationAura FloralSigil shader — flower-of-life field ──
            Effect auraShader = OdeToJoyShaders.CelebrationAura;
            if (auraShader != null)
            {
                OdeToJoyShaders.SetAuraParams(auraShader, time, PollinatorTextures.BloomGold,
                    PollinatorTextures.JubilantLight, fade * 0.35f * pulse, 1.5f, 0.3f, 3f);
                auraShader.Parameters["uRotation"]?.SetValue(_timer * 0.005f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, auraShader, "FloralSigilTechnique");
                sb.Draw(glowTex, drawPos, null, Color.White * fade * pulse, 0f, glowOrigin,
                    0.7f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            sb.Draw(maskTex, drawPos, null, PollinatorTextures.BloomGold * fade * 0.15f * pulse,
                _timer * 0.005f, maskOrigin, 0.6f, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, PollinatorTextures.JubilantLight * fade * 0.25f * pulse,
                0f, glowOrigin, 0.7f, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, PollinatorTextures.PureJoyWhite * fade * 0.15f, 0f, glowOrigin,
                0.25f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }
}