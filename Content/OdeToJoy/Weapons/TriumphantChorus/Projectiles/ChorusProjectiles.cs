using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Projectiles
{
    /// <summary>
    /// TriumphantChorusMinion — Vocal part minion (Soprano/Alto/Tenor/Bass).
    /// ai[0] = voice type (0-3). Orbits player, fires sound wave attacks.
    /// 4 voices = Harmony Bonus (+20% damage). Ensemble Attack every 10s.
    /// </summary>
    public class TriumphantChorusMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int VoiceType => (int)Projectile.ai[0];
        private int _attackTimer;
        private int _ensembleTimer;

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 2f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead || !player.HasBuff(ModContent.BuffType<Buffs.TriumphantChorusBuff>()))
            {
                Projectile.Kill();
                return;
            }
            Projectile.timeLeft = 2;
            _attackTimer++;
            _ensembleTimer++;

            // Orbit around player — each voice type at different phase offset
            float orbitPhase = VoiceType * MathHelper.PiOver2;
            float orbitAngle = Main.GameUpdateCount * 0.03f + orbitPhase;
            float orbitRadius = 60f + VoiceType * 15f;
            Vector2 targetPos = player.Center + new Vector2((float)Math.Cos(orbitAngle), (float)Math.Sin(orbitAngle)) * orbitRadius;

            // Smooth movement
            Projectile.velocity = (targetPos - Projectile.Center) * 0.12f;
            Projectile.Center += Projectile.velocity;
            Projectile.velocity = Vector2.Zero;

            // Check for Harmony Bonus (all 4 voice types present)
            bool hasHarmony = CheckHarmonyBonus(player);

            // Regular attack
            if (_attackTimer >= 60 && Main.myPlayer == Projectile.owner) // Attack every 1s
            {
                NPC target = FindTarget(800f);
                if (target != null)
                {
                    _attackTimer = 0;
                    Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 10f;
                    int dmg = hasHarmony ? (int)(Projectile.damage * 1.2f) : Projectile.damage;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, dir,
                        ModContent.ProjectileType<HarmonicBlastProjectile>(), dmg, 2f, Projectile.owner, VoiceType);
                }
            }

            // Ensemble Attack every 10s (only the first minion triggers)
            if (_ensembleTimer >= 600 && VoiceType == 0 && Main.myPlayer == Projectile.owner)
            {
                _ensembleTimer = 0;
                NPC target = FindTarget(1000f);
                if (target != null)
                {
                    // 8-way radial synchronized burst
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f;
                        Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 12f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel,
                            ModContent.ProjectileType<HarmonicBlastProjectile>(), Projectile.damage * 2, 5f, Projectile.owner, 4f); // 4 = ensemble color
                    }

                    // Ensemble VFX burst
                    for (int i = 0; i < 30; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                        Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.GoldFlame, vel.X, vel.Y, 80, ChorusTextures.BloomGold, 1.0f);
                        d.noGravity = true;
                        d.fadeIn = 1.4f;
                    }
                }
            }

            // Ambient voice particles
            if (Main.rand.NextBool(6))
            {
                Color voiceColor = ChorusTextures.GetVoiceColor(VoiceType);
                Dust d = Dust.NewDustDirect(Projectile.Center - new Vector2(8), 16, 16, DustID.GoldFlame, 0f, -0.5f, 120, voiceColor, 0.4f);
                d.noGravity = true;
            }
        }

        private NPC FindTarget(float range)
        {
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        private bool CheckHarmonyBonus(Player player)
        {
            bool[] voices = new bool[4];
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != player.whoAmI || p.type != Projectile.type) continue;
                int v = (int)p.ai[0];
                if (v >= 0 && v < 4) voices[v] = true;
            }
            return voices[0] && voices[1] && voices[2] && voices[3];
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ChorusTextures.SoftGlow;
            Texture2D sparkle = ChorusTextures.OJBlossomSparkle;
            Vector2 glowOrigin = glow.Size() / 2f;
            Vector2 sparkleOrigin = sparkle.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            Color voiceColor = ChorusTextures.GetVoiceColor(VoiceType);
            float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.08f + VoiceType * 1.5f);
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: GardenBloom JubilantPulse shader — pulsing voice aura ──
            Effect bloomShader = OdeToJoyShaders.GardenBloom;
            if (bloomShader != null)
            {
                OdeToJoyShaders.SetBloomParams(bloomShader, time + VoiceType * 0.5f, voiceColor,
                    ChorusTextures.BloomGold, 0.4f * pulse, 1.8f, 0.35f);
                bloomShader.Parameters["uPulseSpeed"]?.SetValue(1.0f + VoiceType * 0.2f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, bloomShader, "JubilantPulseTechnique");
                sb.Draw(glow, pos, null, Color.White * pulse, 0f, glowOrigin,
                    0.45f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive bloom overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            // Outer voice glow
            sb.Draw(glow, pos, null, voiceColor * 0.35f * pulse, 0f, glowOrigin, 0.4f,
                SpriteEffects.None, 0f);
            // Sparkle body
            sb.Draw(sparkle, pos, null, voiceColor * 0.6f, Main.GameUpdateCount * 0.05f, sparkleOrigin,
                0.3f, SpriteEffects.None, 0f);
            // Core
            sb.Draw(glow, pos, null, ChorusTextures.PureJoyWhite * 0.35f * pulse, 0f, glowOrigin,
                0.1f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }

    /// <summary>
    /// HarmonicBlastProjectile — Sound wave attack from chorus minions.
    /// ai[0] = voice type (0-3) or 4 for ensemble. Homing, golden VFX.
    /// </summary>
    public class HarmonicBlastProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";
        private int _timer;

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            _timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Moderate homing after initial burst
            if (_timer > 10)
            {
                NPC closest = null;
                float closestDist = 600f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
                if (closest != null)
                {
                    Vector2 toTarget = (closest.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.04f);
                }
            }

            // Voice-colored trail
            if (Main.rand.NextBool(3))
            {
                int voice = (int)Projectile.ai[0];
                Color c = voice >= 4 ? ChorusTextures.BloomGold : ChorusTextures.GetVoiceColor(voice);
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 120, c, 0.4f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = ChorusTextures.SoftGlow;
            Vector2 origin = glow.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            int voice = (int)Projectile.ai[0];
            Color c = voice >= 4 ? ChorusTextures.BloomGold : ChorusTextures.GetVoiceColor(voice);
            float fade = MathHelper.Clamp(_timer / 5f, 0f, 1f) * MathHelper.Clamp(Projectile.timeLeft / 12f, 0f, 1f);
            float scale = voice >= 4 ? 0.3f : 0.2f;
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: JubilantHarmony SymphonicAura shader — radial harmonic pulse ──
            Effect harmonyShader = OdeToJoyShaders.JubilantHarmony;
            if (harmonyShader != null)
            {
                OdeToJoyShaders.SetBeamParams(harmonyShader, time, c,
                    ChorusTextures.PureJoyWhite, fade * 0.5f, 1.6f, 3.0f);
                harmonyShader.Parameters["uRadius"]?.SetValue(0.3f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, harmonyShader, "SymphonicAuraTechnique");
                sb.Draw(glow, pos, null, Color.White * fade, Projectile.rotation, origin,
                    scale * 1.5f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive bloom ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            sb.Draw(glow, pos, null, c * fade * 0.5f, 0f, origin, scale, SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, ChorusTextures.PureJoyWhite * fade * 0.3f, 0f, origin,
                scale * 0.35f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }
}