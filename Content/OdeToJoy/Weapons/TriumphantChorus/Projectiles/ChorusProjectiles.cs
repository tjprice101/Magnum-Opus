using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Dusts;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Projectiles
{
    /// <summary>
    /// TriumphantChorusMinion — Vocal part minion (Soprano/Alto/Tenor/Bass).
    /// ai[0] = voice type (0-3). Orbits player, fires sound wave attacks.
    /// 4 voices = Harmony Bonus (+20% damage). Ensemble Attack every 10s.
    /// </summary>
    public class TriumphantChorusMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/Summon/TriumphantChorusMinion";

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
                        Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, ModContent.DustType<ChorusVoiceDust>(), vel.X, vel.Y, 80, ChorusTextures.BloomGold, 1.0f);
                        d.noGravity = true;
                        d.fadeIn = 1.4f;
                    }

                    // Ensemble climax VFX
                    OdeToJoyVFXLibrary.ScreenShake(10f, 20);
                    OdeToJoyVFXLibrary.ScreenFlash(OdeToJoyPalette.GoldenPollen, 1.3f);
                    OdeToJoyVFXLibrary.HarmonicPulseRing(Projectile.Center, 1.5f, 16, OdeToJoyPalette.GoldenPollen);
                    OdeToJoyVFXLibrary.CelebrationBurst(Projectile.Center, 1.5f);
                }
            }

            // Ambient voice particles
            if (Main.rand.NextBool(6))
            {
                Color voiceColor = ChorusTextures.GetVoiceColor(VoiceType);
                Dust d = Dust.NewDustDirect(Projectile.Center - new Vector2(8), 16, 16, ModContent.DustType<ChorusVoiceDust>(), 0f, -0.5f, 120, voiceColor, 0.4f);
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
            try
            {
            Texture2D glow = ChorusTextures.SoftGlow;
            Texture2D sparkle = ChorusTextures.OJBlossomSparkle;
            Vector2 glowOrigin = glow.Size() / 2f;
            Vector2 sparkleOrigin = sparkle.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            Color voiceColor = ChorusTextures.GetVoiceColor(VoiceType);
            float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.08f + VoiceType * 1.5f);
            float time = (float)Main.timeForVisualEffects * 0.015f;

            // ── MINION SPRITE: Draw base PNG sprite ──
            Texture2D minionTex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 minionOrigin = minionTex.Size() / 2f;
            sb.Draw(minionTex, pos, null, lightColor * Projectile.Opacity, Projectile.rotation, minionOrigin, Projectile.scale, SpriteEffects.None, 0f);

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
                    0.28f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive bloom overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            // Outer voice glow
            sb.Draw(glow, pos, null, voiceColor * 0.35f * pulse, 0f, glowOrigin, 0.28f,
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
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

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
        private VertexStrip _vertexStrip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

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
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<ChorusVoiceDust>(), 0f, 0f, 120, c, 0.4f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _vertexStrip);

                // Harmonic blast: golden voice-colored pulse
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null)
                {
                    Vector2 origin = glow.Size() / 2f;
                    float rot = Projectile.velocity.ToRotation();
                    float pulse = 0.8f + 0.2f * MathF.Sin((float)Main.timeForVisualEffects * 0.15f + Projectile.whoAmI);

                    // Golden directional harmonic glow
                    sb.Draw(glow, drawPos, null,
                        (OdeToJoyPalette.GoldenPollen with { A = 0 }) * 0.2f * pulse,
                        rot, origin, new Vector2(0.06f, 0.025f), SpriteEffects.None, 0f);
                }

                sb.End();
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }
    }
}
