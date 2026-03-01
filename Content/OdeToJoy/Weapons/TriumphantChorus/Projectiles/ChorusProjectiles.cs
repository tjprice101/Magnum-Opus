using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Utilities;
using MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Particles;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Projectiles
{
    // ═══════════════════════════════════════════════════════════
    // TriumphantChorusMinion — flying chorus entity (2-slot minion).
    // Orbits player when idle at ~120px. Targets nearest enemy within
    // 800px. Fires HarmonicBlastProjectile every 25 frames toward target.
    // Grand Finale: every 300 frames, fires 8 radial HarmonicBlast at
    // 2x damage with massive particle VFX explosion.
    // ═══════════════════════════════════════════════════════════
    public class TriumphantChorusMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/Summon/TriumphantChorusMinion";

        private ref float AttackTimer => ref Projectile.ai[0];
        private ref float FinaleTimer => ref Projectile.ai[1];

        private float orbitAngle;

        private static Asset<Texture2D> _softBloomTex;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 2f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => false;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!CheckActive(owner))
                return;

            NPC target = ChorusUtils.ClosestNPC(Projectile.Center, 800f);

            if (target != null)
            {
                // ── FLY TOWARD TARGET ──
                Vector2 toTarget = target.Center - Projectile.Center;
                float dist = toTarget.Length();
                if (dist > 200f)
                {
                    toTarget.Normalize();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 12f, 0.08f);
                }
                else
                {
                    // Hover near target at ~200px
                    Projectile.velocity *= 0.92f;
                }

                // ── REGULAR ATTACK ──
                AttackTimer++;
                if (AttackTimer >= 25)
                {
                    AttackTimer = 0;

                    Vector2 attackDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        attackDir * 14f,
                        ModContent.ProjectileType<HarmonicBlastProjectile>(),
                        Projectile.damage,
                        Projectile.knockBack,
                        Projectile.owner,
                        0f // ai[0] = 0 means normal blast
                    );

                    SoundEngine.PlaySound(SoundID.Item43 with { Volume = 0.5f, Pitch = 0.4f }, Projectile.Center);

                    // Attack spark VFX
                    if (!Main.dedServ)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Vector2 sparkVel = attackDir.RotatedByRandom(0.4f) * Main.rand.NextFloat(2f, 5f);
                            ChorusParticleHandler.SpawnParticle(new ChorusSparkParticle(
                                Projectile.Center + attackDir * 16f,
                                sparkVel, Main.rand.NextFloat(0.15f, 0.3f), Main.rand.Next(8, 14)));
                        }
                    }
                }

                // ── GRAND FINALE ──
                FinaleTimer++;
                if (FinaleTimer >= 300)
                {
                    FinaleTimer = 0;
                    PerformGrandFinale(target);
                }
            }
            else
            {
                // ── IDLE: ORBIT PLAYER ──
                orbitAngle += 0.03f;
                Vector2 desiredPos = owner.Center + new Vector2(
                    (float)Math.Cos(orbitAngle) * 120f,
                    (float)Math.Sin(orbitAngle) * 80f - 40f);

                Vector2 toDesired = desiredPos - Projectile.Center;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toDesired * 0.1f, 0.15f);

                AttackTimer = 0;
                // Finale timer still counts up even when idle
                FinaleTimer++;
                if (FinaleTimer >= 300)
                    FinaleTimer = 300; // cap, ready to fire when target appears
            }

            Projectile.rotation = Projectile.velocity.X * 0.02f;

            // ── AMBIENT VFX ──
            if (!Main.dedServ && Main.GameUpdateCount % 5 == 0)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 glowPos = Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(12f, 28f);
                ChorusParticleHandler.SpawnParticle(new ChorusGlowParticle(
                    glowPos, Main.rand.NextVector2Circular(0.4f, 0.4f),
                    Main.rand.NextFloat(0.1f, 0.2f), Main.rand.Next(18, 30)));
            }

            Lighting.AddLight(Projectile.Center, ChorusUtils.TriumphGold.ToVector3() * 0.45f);
        }

        private void PerformGrandFinale(NPC target)
        {
            // Spawn 8 radial blasts at 2x damage
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 dir = angle.ToRotationVector2();

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    dir * 12f,
                    ModContent.ProjectileType<HarmonicBlastProjectile>(),
                    Projectile.damage * 2,
                    Projectile.knockBack * 1.5f,
                    Projectile.owner,
                    1f // ai[0] = 1 means finale blast (enhanced VFX)
                );
            }

            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.9f, Pitch = -0.2f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item105 with { Volume = 0.7f, Pitch = 0.5f }, Projectile.Center);

            // ── MASSIVE FINALE VFX ──
            if (!Main.dedServ)
            {
                // Grand bloom explosion
                ChorusParticleHandler.SpawnParticle(new GrandFinaleBloomParticle(
                    Projectile.Center, 0.5f, 35));

                // Ring of outward-flowing music notes
                for (int i = 0; i < 16; i++)
                {
                    float angle = MathHelper.TwoPi * i / 16f;
                    Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                    ChorusParticleHandler.SpawnParticle(new FinaleNoteParticle(
                        Projectile.Center + angle.ToRotationVector2() * 10f,
                        noteVel, Main.rand.NextFloat(0.35f, 0.6f), Main.rand.Next(40, 65)));
                }

                // Particle shower — sparks everywhere
                for (int i = 0; i < 24; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2CircularEdge(5f, 5f) * Main.rand.NextFloat(0.6f, 1.2f);
                    ChorusParticleHandler.SpawnParticle(new ChorusSparkParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                        sparkVel, Main.rand.NextFloat(0.2f, 0.45f), Main.rand.Next(12, 25)));
                }

                // Extra glow ring burst
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 glowVel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                    ChorusParticleHandler.SpawnParticle(new ChorusGlowParticle(
                        Projectile.Center, glowVel,
                        Main.rand.NextFloat(0.25f, 0.5f), Main.rand.Next(20, 35)));
                }
            }
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<Buffs.TriumphantChorusBuff>());
                Projectile.Kill();
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<Buffs.TriumphantChorusBuff>()))
                Projectile.timeLeft = 2;

            return true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            _softBloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D softBloom = _softBloomTex.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            Vector2 sOrigin = softBloom.Size() / 2f;

            SpriteBatch sb = Main.spriteBatch;
            float time = (float)Main.GameUpdateCount / 60f;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.07f) * 0.08f;

            // ═══ Layer 1: JubilantHarmony — choral harmonic rings around the minion ═══
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Effect harmonyShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyJubilantHarmonyShader);
            if (harmonyShader != null)
            {
                // Swell intensity based on Finale timer — minion glows brighter as Finale approaches
                float finaleCharge = Math.Min(FinaleTimer / 300f, 1f);
                float choralSwell = 0.3f + finaleCharge * 0.4f;

                harmonyShader.Parameters["uTime"]?.SetValue(time);
                harmonyShader.Parameters["uColor"]?.SetValue(ChorusUtils.TriumphGold.ToVector3());
                harmonyShader.Parameters["uSecondaryColor"]?.SetValue(ChorusUtils.CrescendoRose.ToVector3());
                harmonyShader.Parameters["uOpacity"]?.SetValue(choralSwell);
                harmonyShader.Parameters["uIntensity"]?.SetValue(1.0f + finaleCharge * 0.8f);
                harmonyShader.Parameters["uRadius"]?.SetValue(0.35f);
                harmonyShader.Parameters["uHarmonicFreq"]?.SetValue(1.2f + finaleCharge * 1.5f);
                harmonyShader.CurrentTechnique = harmonyShader.Techniques["SymphonicAuraTechnique"];
                harmonyShader.CurrentTechnique.Passes[0].Apply();

                sb.Draw(softBloom, drawPos, null, Color.White,
                    0f, sOrigin, 0.7f * pulse * (1f + finaleCharge * 0.3f), SpriteEffects.None, 0f);
            }

            sb.End();

            // ═══ Layer 2: Additive glow layers ═══
            ChorusUtils.BeginAdditive(sb);

            Color glow = ChorusUtils.Additive(ChorusUtils.TriumphGold, 0.3f);
            sb.Draw(tex, drawPos, null, glow, Projectile.rotation, origin,
                Projectile.scale * pulse * 1.25f, SpriteEffects.None, 0f);

            // Rose tint halo
            Color roseGlow = ChorusUtils.Additive(ChorusUtils.CrescendoRose, 0.15f);
            sb.Draw(tex, drawPos, null, roseGlow, Projectile.rotation, origin,
                Projectile.scale * pulse * 1.5f, SpriteEffects.None, 0f);

            sb.End();
            ChorusUtils.BeginDefault(sb);

            // Normal draw
            sb.Draw(tex, drawPos, null, lightColor, Projectile.rotation, origin,
                Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // HarmonicBlastProjectile — golden homing bolt fired by the chorus.
    // 20x20, pen 2, timeLeft 120, homing 0.06 toward nearest enemy.
    // Applies Poisoned 120 + Confused 60. On hit: golden burst + music notes.
    // ai[0]: 0 = normal, 1 = finale (2x size glow, extra particles).
    // ═══════════════════════════════════════════════════════════
    public class HarmonicBlastProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private bool IsFinaleBlast => Projectile.ai[0] == 1f;

        private static Asset<Texture2D> _softBloomTex;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
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
            // Homing toward nearest enemy
            NPC target = ChorusUtils.ClosestNPC(Projectile.Center, 800f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.06f);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail particles
            if (!Main.dedServ)
            {
                int trailRate = IsFinaleBlast ? 1 : 2;
                if (Main.GameUpdateCount % trailRate == 0)
                {
                    ChorusParticleHandler.SpawnParticle(new HarmonicTrailParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                        -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                        Main.rand.NextFloat(0.12f, IsFinaleBlast ? 0.3f : 0.2f),
                        Main.rand.Next(8, 16)));
                }

                // Finale blasts get extra spark trails
                if (IsFinaleBlast && Main.GameUpdateCount % 3 == 0)
                {
                    ChorusParticleHandler.SpawnParticle(new ChorusSparkParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                        Main.rand.NextVector2Circular(1f, 1f),
                        Main.rand.NextFloat(0.08f, 0.15f), Main.rand.Next(6, 12)));
                }
            }

            float lightMult = IsFinaleBlast ? 0.6f : 0.35f;
            Lighting.AddLight(Projectile.Center, ChorusUtils.TriumphGold.ToVector3() * lightMult);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 120);
            target.AddBuff(BuffID.Confused, 60);

            // Impact VFX — golden burst + music notes
            if (!Main.dedServ)
            {
                // Golden particle burst
                int burstCount = IsFinaleBlast ? 14 : 8;
                for (int i = 0; i < burstCount; i++)
                {
                    Vector2 burstVel = Main.rand.NextVector2CircularEdge(4f, 4f) * Main.rand.NextFloat(0.5f, 1.2f);
                    ChorusParticleHandler.SpawnParticle(new ChorusSparkParticle(
                        Projectile.Center, burstVel,
                        Main.rand.NextFloat(0.15f, 0.35f), Main.rand.Next(10, 20)));
                }

                // Music notes scattering from impact
                int noteCount = IsFinaleBlast ? 6 : 3;
                for (int i = 0; i < noteCount; i++)
                {
                    Vector2 noteVel = Main.rand.NextVector2CircularEdge(2.5f, 2.5f);
                    noteVel.Y -= 1.5f; // bias upward
                    ChorusParticleHandler.SpawnParticle(new FinaleNoteParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                        noteVel, Main.rand.NextFloat(0.25f, 0.45f), Main.rand.Next(30, 50)));
                }

                // Bloom flash on impact
                if (IsFinaleBlast)
                {
                    ChorusParticleHandler.SpawnParticle(new GrandFinaleBloomParticle(
                        Projectile.Center, 0.2f, 15));
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            _softBloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D softBloom = _softBloomTex.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            Vector2 sOrigin = softBloom.Size() / 2f;

            SpriteBatch sb = Main.spriteBatch;
            float time = (float)Main.GameUpdateCount / 60f;
            float sizeMult = IsFinaleBlast ? 2f : 1f;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;

            sb.End();

            // ═══ Layer 1: Shader-driven aura — Finale blasts get CelebrationAura expanding rings ═══
            if (IsFinaleBlast)
            {
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Effect auraShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyCelebrationAuraShader);
                if (auraShader != null)
                {
                    auraShader.Parameters["uTime"]?.SetValue(time * 1.5f);
                    auraShader.Parameters["uColor"]?.SetValue(ChorusUtils.TriumphGold.ToVector3());
                    auraShader.Parameters["uSecondaryColor"]?.SetValue(ChorusUtils.FinaleWhite.ToVector3());
                    auraShader.Parameters["uOpacity"]?.SetValue(0.55f);
                    auraShader.Parameters["uIntensity"]?.SetValue(1.8f);
                    auraShader.Parameters["uRadius"]?.SetValue(0.5f);
                    auraShader.Parameters["uRingCount"]?.SetValue(4f);
                    auraShader.CurrentTechnique = auraShader.Techniques["CelebrationAuraTechnique"];
                    auraShader.CurrentTechnique.Passes[0].Apply();

                    sb.Draw(softBloom, drawPos, null, Color.White,
                        0f, sOrigin, 1.2f * pulse, SpriteEffects.None, 0f);
                }

                sb.End();
            }
            else
            {
                // Normal blasts get subtle JubilantHarmony harmonic shimmer
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Effect harmonyShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyJubilantHarmonyShader);
                if (harmonyShader != null)
                {
                    harmonyShader.Parameters["uTime"]?.SetValue(time);
                    harmonyShader.Parameters["uColor"]?.SetValue(ChorusUtils.HarmonyGold.ToVector3());
                    harmonyShader.Parameters["uSecondaryColor"]?.SetValue(ChorusUtils.CrescendoRose.ToVector3());
                    harmonyShader.Parameters["uOpacity"]?.SetValue(0.35f);
                    harmonyShader.Parameters["uIntensity"]?.SetValue(1.0f);
                    harmonyShader.Parameters["uRadius"]?.SetValue(0.3f);
                    harmonyShader.Parameters["uHarmonicFreq"]?.SetValue(2.0f);
                    harmonyShader.CurrentTechnique = harmonyShader.Techniques["SymphonicAuraTechnique"];
                    harmonyShader.CurrentTechnique.Passes[0].Apply();

                    sb.Draw(softBloom, drawPos, null, Color.White,
                        0f, sOrigin, 0.45f * pulse, SpriteEffects.None, 0f);
                }

                sb.End();
            }

            // ═══ Layer 2: Additive bloom body ═══
            ChorusUtils.BeginAdditive(sb);

            // Outer golden glow
            Color outer = ChorusUtils.Additive(ChorusUtils.HarmonyGold, 0.4f);
            sb.Draw(tex, drawPos, null, outer, 0f, origin,
                0.5f * sizeMult * pulse, SpriteEffects.None, 0f);

            // Mid triumph gold
            Color mid = ChorusUtils.Additive(ChorusUtils.TriumphGold, 0.7f);
            sb.Draw(tex, drawPos, null, mid, 0f, origin,
                0.3f * sizeMult * pulse, SpriteEffects.None, 0f);

            // Hot white core
            Color core = ChorusUtils.Additive(ChorusUtils.FinaleWhite, 0.9f);
            sb.Draw(tex, drawPos, null, core, 0f, origin,
                0.12f * sizeMult * pulse, SpriteEffects.None, 0f);

            sb.End();
            ChorusUtils.BeginDefault(sb);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Death particles
            if (!Main.dedServ)
            {
                int count = IsFinaleBlast ? 8 : 4;
                for (int i = 0; i < count; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(2f, 2f);
                    ChorusParticleHandler.SpawnParticle(new ChorusGlowParticle(
                        Projectile.Center, vel,
                        Main.rand.NextFloat(0.1f, 0.2f), Main.rand.Next(10, 18)));
                }
            }
        }
    }
}
