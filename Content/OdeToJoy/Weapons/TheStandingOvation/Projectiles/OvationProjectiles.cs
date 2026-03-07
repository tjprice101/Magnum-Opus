using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Projectiles
{
    /// <summary>
    /// StandingOvationMinion — Phantom spectator minion.
    /// Multi-mode: Applause Wave (3s), Thrown Rose (tracking, Thorned debuff), Standing Rush (<20% HP).
    /// Ovation Meter → Standing Ovation Event (shockwave + rose rain).
    /// ai[0] = crowd index for color variety.
    /// </summary>
    public class StandingOvationMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int CrowdIndex => (int)Projectile.ai[0];
        private int _attackTimer;
        private int _roseTimer;
        private int _eventPulseTimer;
        private float _hoverOffset;

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead || !player.HasBuff(ModContent.BuffType<Buffs.StandingOvationBuff>()))
            {
                Projectile.Kill();
                return;
            }
            Projectile.timeLeft = 2;
            _attackTimer++;
            _roseTimer++;
            _hoverOffset = (float)Math.Sin(Main.GameUpdateCount * 0.04f + CrowdIndex * 2f) * 8f;

            OvationPlayer op = player.GetModPlayer<OvationPlayer>();
            float crowdMult = op.GetCrowdMultiplier();
            bool chorusSync = op.HasChorusSync();
            float syncMult = chorusSync ? 1.15f : 1f;

            // Hover above player in a spread formation
            float xSpread = (CrowdIndex - op.CrowdSize / 2f) * 40f;
            Vector2 targetPos = player.Center + new Vector2(xSpread, -80f + _hoverOffset);
            Projectile.velocity = (targetPos - Projectile.Center) * 0.1f;
            Projectile.Center += Projectile.velocity;
            Projectile.velocity = Vector2.Zero;

            if (Main.myPlayer != Projectile.owner) return;

            NPC target = FindTarget(900f);

            // Standing Ovation Event active — spawn rose rain + shockwave pulses
            if (op.EventTimer > 0)
            {
                _eventPulseTimer++;
                if (_eventPulseTimer % 10 == 0 && target != null)
                {
                    // Rose rain from above
                    Vector2 rosePos = target.Center + new Vector2(Main.rand.NextFloat(-200f, 200f), -400f);
                    Vector2 roseVel = new Vector2(Main.rand.NextFloat(-1f, 1f), 8f);
                    int roseDmg = (int)(Projectile.damage * 0.5f * crowdMult * syncMult);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), rosePos, roseVel,
                        ModContent.ProjectileType<RoseRainPetalProjectile>(), roseDmg, 1f, Projectile.owner);
                }
                if (_eventPulseTimer % 30 == 0)
                {
                    // Shockwave pulse from minion
                    int waveDmg = (int)(Projectile.damage * 1.5f * crowdMult * syncMult);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                        ModContent.ProjectileType<OvationShockwaveProjectile>(), waveDmg, 5f, Projectile.owner);
                }

                // Event ending — set encore window
                if (op.EventTimer == 1)
                    op.EncoreTimer = 300; // 5 seconds

                return; // During event, don't do normal attacks
            }
            _eventPulseTimer = 0;

            if (target == null) return;
            float dist = Vector2.Distance(Projectile.Center, target.Center);

            // Mode 1: Standing Rush — charge at low HP enemies
            if (target.life < target.lifeMax * 0.2f && dist < 400f && _attackTimer > 30)
            {
                _attackTimer = 0;
                Vector2 rushVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 16f;
                int rushDmg = (int)(Projectile.damage * 2f * crowdMult * syncMult);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, rushVel,
                    ModContent.ProjectileType<JoyWaveProjectile>(), rushDmg, 6f, Projectile.owner, 1f); // ai[0]=1 for rush mode
                return;
            }

            // Mode 2: Applause Wave (every 3s = 180 frames)
            if (_attackTimer >= 180)
            {
                _attackTimer = 0;
                // 3-way spread
                Vector2 baseDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                for (int i = -1; i <= 1; i++)
                {
                    Vector2 dir = baseDir.RotatedBy(i * MathHelper.ToRadians(12f)) * 8f;
                    int waveDmg = (int)(Projectile.damage * crowdMult * syncMult);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, dir,
                        ModContent.ProjectileType<JoyWaveProjectile>(), waveDmg, 3f, Projectile.owner, 0f); // ai[0]=0 for wave mode
                }
            }

            // Mode 3: Thrown Rose (every 2.5s = 150 frames, offset from applause)
            if (_roseTimer >= 150)
            {
                _roseTimer = 0;
                Vector2 roseDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 7f;
                int roseDmg = (int)(Projectile.damage * 0.8f * crowdMult * syncMult);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, roseDir,
                    ModContent.ProjectileType<ThrownRoseProjectile>(), roseDmg, 2f, Projectile.owner);
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.Center - new Vector2(8), 16, 16, DustID.GoldFlame,
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 100, OvationTextures.BloomGold, 0.5f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Register kill if fatal
            if (target.life <= 0)
            {
                OvationPlayer op = Main.player[Projectile.owner].GetModPlayer<OvationPlayer>();
                op.RegisterKill();
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
                float d = Vector2.Distance(Projectile.Center, npc.Center);
                if (d < closestDist)
                {
                    closestDist = d;
                    closest = npc;
                }
            }
            return closest;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = OvationTextures.SoftGlow;
            Texture2D sparkle = OvationTextures.OJBlossomSparkle;
            Vector2 glowOrigin = glow.Size() / 2f;
            Vector2 sparkleOrigin = sparkle.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            Color spectatorColor = OvationTextures.GetSpectatorColor(CrowdIndex);
            float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.06f + CrowdIndex * 1.8f);
            float time = (float)Main.timeForVisualEffects * 0.015f;

            OvationPlayer op = Main.player[Projectile.owner].GetModPlayer<OvationPlayer>();
            float meterGlow = op.OvationMeter / 100f;

            sb.End();

            // ── LAYER 0: GardenBloom JubilantPulse shader — spectator aura ──
            Effect bloomShader = OdeToJoyShaders.GardenBloom;
            if (bloomShader != null)
            {
                OdeToJoyShaders.SetBloomParams(bloomShader, time + CrowdIndex * 0.3f, spectatorColor,
                    OvationTextures.BloomGold, 0.35f * pulse, 1.5f, 0.3f);
                bloomShader.Parameters["uPulseSpeed"]?.SetValue(0.8f + CrowdIndex * 0.15f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, bloomShader, "JubilantPulseTechnique");
                sb.Draw(glow, pos, null, Color.White * pulse, 0f, glowOrigin,
                    Math.Min(0.4f + meterGlow * 0.15f, 0.293f), SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive bloom overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            sb.Draw(glow, pos, null, spectatorColor * 0.25f * pulse, 0f, glowOrigin,
                Math.Min(0.35f + meterGlow * 0.1f, 0.293f), SpriteEffects.None, 0f);
            sb.Draw(sparkle, pos, null, spectatorColor * 0.6f, Main.GameUpdateCount * 0.03f + CrowdIndex,
                sparkleOrigin, 0.25f, SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, OvationTextures.PureJoyWhite * 0.3f * pulse, 0f, glowOrigin,
                0.08f, SpriteEffects.None, 0f);
            if (meterGlow > 0.3f)
                sb.Draw(glow, pos, null, OvationTextures.ApplauseFlash * meterGlow * 0.15f, 0f, glowOrigin,
                    Math.Min(0.5f * meterGlow, 0.293f), SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }

    /// <summary>
    /// JoyWaveProjectile — Applause wave or standing rush attack.
    /// ai[0] = 0 for applause wave (expanding arc), 1 for standing rush (fast charge).
    /// </summary>
    public class JoyWaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";
        private int _timer;
        private bool IsRush => Projectile.ai[0] >= 1f;

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = IsRush ? 3 : 2;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            _timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (IsRush)
            {
                // Rush mode — fast, leaves golden streak
                Projectile.velocity *= 0.98f;
                if (Main.rand.NextBool(2))
                {
                    Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame,
                        0f, 0f, 100, OvationTextures.BloomGold, 0.5f);
                    d.noGravity = true;
                }
            }
            else
            {
                // Applause wave — expands slightly over time
                Projectile.scale = 1f + _timer * 0.015f;
                Projectile.velocity *= 0.97f;
            }

            // Kill registration 
            if (Projectile.numHits > 0)
            {
                // Handled in OnHitNPC
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.life <= 0)
            {
                OvationPlayer op = Main.player[Projectile.owner].GetModPlayer<OvationPlayer>();
                op.RegisterKill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = OvationTextures.SoftGlow;
            Texture2D impact = OvationTextures.OJHarmonicImpact;
            Vector2 glowOrigin = glow.Size() / 2f;
            Vector2 impactOrigin = impact.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            float fade = MathHelper.Clamp(_timer / 4f, 0f, 1f) * MathHelper.Clamp(Projectile.timeLeft / 10f, 0f, 1f);
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: JubilantHarmony SymphonicAura shader ──
            Effect harmonyShader = OdeToJoyShaders.JubilantHarmony;
            if (harmonyShader != null)
            {
                Color waveColor = IsRush ? OvationTextures.BloomGold : OvationTextures.ApplauseFlash;
                OdeToJoyShaders.SetBeamParams(harmonyShader, time, waveColor,
                    OvationTextures.PureJoyWhite, fade * 0.45f, 1.8f, 2.5f);
                harmonyShader.Parameters["uRadius"]?.SetValue(0.25f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, harmonyShader, "SymphonicAuraTechnique");
                float shaderScale = IsRush ? 0.25f : Math.Min(0.15f + _timer * 0.005f, 0.293f);
                sb.Draw(glow, pos, null, Color.White * fade, Projectile.rotation, glowOrigin,
                    shaderScale, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            if (IsRush)
            {
                float stretch = Math.Min(0.3f + Projectile.velocity.Length() * 0.02f, 0.293f);
                sb.Draw(glow, pos, null, OvationTextures.BloomGold * fade * 0.6f, Projectile.rotation,
                    glowOrigin, new Vector2(stretch, 0.12f), SpriteEffects.None, 0f);
                sb.Draw(glow, pos, null, OvationTextures.PureJoyWhite * fade * 0.35f, Projectile.rotation,
                    glowOrigin, new Vector2(stretch * 0.5f, 0.05f), SpriteEffects.None, 0f);
            }
            else
            {
                float scale = Math.Min(0.15f + _timer * 0.005f, 0.293f);
                sb.Draw(impact, pos, null, OvationTextures.BloomGold * fade * 0.4f, Projectile.rotation,
                    impactOrigin, scale, SpriteEffects.None, 0f);
                sb.Draw(glow, pos, null, OvationTextures.ApplauseFlash * fade * 0.25f, 0f, glowOrigin,
                    scale * 0.8f, SpriteEffects.None, 0f);
            }

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }

    /// <summary>
    /// ThrownRoseProjectile — Arcing homing rose that applies Thorned debuff (2% bleed).
    /// Petal Pink trail, curved trajectory.
    /// </summary>
    public class ThrownRoseProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";
        private int _timer;
        private readonly Vector2[] _trail = new Vector2[12];
        private int _trailIdx;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            _timer++;
            Projectile.rotation += 0.15f; // Tumbling rotation

            // Record trail
            _trail[_trailIdx % _trail.Length] = Projectile.Center;
            _trailIdx++;

            // Homing after initial arc (20 frames)
            if (_timer > 20)
            {
                NPC closest = null;
                float closestDist = 500f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                    float d = Vector2.Distance(Projectile.Center, npc.Center);
                    if (d < closestDist) { closestDist = d; closest = npc; }
                }
                if (closest != null)
                {
                    Vector2 toTarget = (closest.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 9f, 0.05f);
                }
            }
            else
            {
                // Initial arc — slight upward then curve
                Projectile.velocity.Y += 0.1f;
            }

            // Petal trail particles
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PinkTorch,
                    Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.5f, 0.5f), 100, OvationTextures.RosePetalPink, 0.4f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Buffs.ThornedDebuff>(), 180); // 3 seconds

            if (target.life <= 0)
            {
                OvationPlayer op = Main.player[Projectile.owner].GetModPlayer<OvationPlayer>();
                op.RegisterKill();
            }

            // Rose petal burst on impact
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustDirect(target.Center, 1, 1, DustID.PinkTorch,
                    vel.X, vel.Y, 80, OvationTextures.PetalPink, 0.6f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = OvationTextures.SoftGlow;
            Texture2D rosePetal = OvationTextures.OJRosePetal;
            Vector2 glowOrigin = glow.Size() / 2f;
            Vector2 roseOrigin = rosePetal.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            float fade = MathHelper.Clamp(_timer / 4f, 0f, 1f);
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: TriumphantTrail BlossomWind VertexStrip — petal trail ──
            Effect trailShader = OdeToJoyShaders.TriumphantTrail;
            int validCount = 0;
            for (int i = 0; i < _trail.Length; i++)
            {
                int idx = (_trailIdx - 1 - i + _trail.Length * 2) % _trail.Length;
                if (_trail[idx] != Vector2.Zero) validCount++; else break;
            }
            if (trailShader != null && validCount >= 2)
            {
                Vector2[] positions = new Vector2[validCount];
                float[] rotations = new float[validCount];
                for (int i = 0; i < validCount; i++)
                {
                    int idx = (_trailIdx - 1 - i + _trail.Length * 2) % _trail.Length;
                    positions[validCount - 1 - i] = _trail[idx];
                }
                for (int i = 0; i < validCount; i++)
                {
                    if (i < validCount - 1) rotations[i] = (positions[i + 1] - positions[i]).ToRotation();
                    else rotations[i] = rotations[Math.Max(0, i - 1)];
                }

                Terraria.Graphics.VertexStrip strip = new Terraria.Graphics.VertexStrip();
                strip.PrepareStrip(positions, rotations,
                    (float p) => OvationTextures.RosePetalPink * fade * p * 0.35f,
                    (float p) => MathHelper.Lerp(1f, 6f, p),
                    -Main.screenPosition, includeBacksides: true);
                OdeToJoyShaders.SetTrailParams(trailShader, time, OvationTextures.RosePetalPink,
                    OvationTextures.PetalPink, fade * 0.5f, 1.5f);
                trailShader.CurrentTechnique = trailShader.Techniques["BlossomWindTrailTechnique"];
                trailShader.Parameters["WorldViewProjection"]?.SetValue(
                    Main.GameViewMatrix.NormalizedTransformationmatrix);
                trailShader.CurrentTechnique.Passes["P0"].Apply();
                strip.DrawTrail();
                Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            }

            // ── LAYER 1: Additive bloom ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            sb.Draw(rosePetal, pos, null, OvationTextures.RoseRed * fade * 0.7f, Projectile.rotation,
                roseOrigin, 0.15f, SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, OvationTextures.PetalPink * fade * 0.35f, 0f, glowOrigin,
                0.1f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }

    /// <summary>
    /// RoseRainPetalProjectile — Falling rose petal during Standing Ovation Event.
    /// Falls from above, gentle tumble, deals damage on contact.
    /// </summary>
    public class RoseRainPetalProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";
        private int _timer;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            _timer++;
            // Gentle tumbling descent
            Projectile.velocity.X += (float)Math.Sin(Main.GameUpdateCount * 0.08f + Projectile.identity) * 0.05f;
            Projectile.velocity.Y = MathHelper.Clamp(Projectile.velocity.Y, 3f, 10f);
            Projectile.rotation += Projectile.velocity.X * 0.05f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.life <= 0)
            {
                OvationPlayer op = Main.player[Projectile.owner].GetModPlayer<OvationPlayer>();
                op.RegisterKill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D rosePetal = OvationTextures.OJRosePetal;
            Texture2D glow = OvationTextures.SoftGlow;
            Vector2 roseOrigin = rosePetal.Size() / 2f;
            Vector2 glowOrigin = glow.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            float fade = MathHelper.Clamp(_timer / 3f, 0f, 1f) * MathHelper.Clamp(Projectile.timeLeft / 8f, 0f, 1f);
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: GardenBloom GardenBloomTechnique — petal shimmer ──
            Effect bloomShader = OdeToJoyShaders.GardenBloom;
            if (bloomShader != null)
            {
                OdeToJoyShaders.SetBloomParams(bloomShader, time + Projectile.identity * 0.5f,
                    OvationTextures.RosePetalPink, OvationTextures.PetalPink, fade * 0.4f, 1.2f, 0.15f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, bloomShader, "GardenBloomTechnique");
                sb.Draw(glow, pos, null, Color.White * fade, Projectile.rotation, glowOrigin,
                    0.1f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive bloom ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            sb.Draw(rosePetal, pos, null, OvationTextures.RosePetalPink * fade * 0.65f, Projectile.rotation,
                roseOrigin, 0.12f, SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, OvationTextures.PetalPink * fade * 0.2f, 0f, glowOrigin,
                0.06f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }

    /// <summary>
    /// OvationShockwaveProjectile — Expanding golden shockwave during Standing Ovation Event.
    /// Rapidly expands, damages all enemies in radius, ImpactFoundation-style ripple.
    /// </summary>
    public class OvationShockwaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";
        private int _timer;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            _timer++;
            // Expand hitbox rapidly
            float progress = _timer / 30f;
            int newSize = (int)(300f * progress);
            Projectile.Resize(newSize, newSize);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.life <= 0)
            {
                OvationPlayer op = Main.player[Projectile.owner].GetModPlayer<OvationPlayer>();
                op.RegisterKill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D ring = OvationTextures.OJPowerRing;
            Texture2D glow = OvationTextures.SoftGlow;
            Texture2D harmonic = OvationTextures.OJHarmonicImpact;
            Vector2 ringOrigin = ring.Size() / 2f;
            Vector2 glowOrigin = glow.Size() / 2f;
            Vector2 harmonicOrigin = harmonic.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            float progress = _timer / 30f;
            float fade = 1f - progress;
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: CelebrationAura shader — expanding concentric rings ──
            Effect auraShader = OdeToJoyShaders.CelebrationAura;
            if (auraShader != null)
            {
                float auraRadius = 0.1f + progress * 0.4f;
                OdeToJoyShaders.SetAuraParams(auraShader, time + progress * 3f, OvationTextures.BloomGold,
                    OvationTextures.ApplauseFlash, fade * 0.55f, 2.5f, auraRadius, 5f);
                OdeToJoyShaders.BeginShaderBatch(sb, auraShader, "CelebrationAuraTechnique");
                auraShader.CurrentTechnique.Passes["P0"].Apply();
                float shaderScale = Math.Min(0.15f + progress * 0.5f, 0.293f);
                sb.Draw(glow, pos, null, Color.White * fade, 0f, glowOrigin, shaderScale,
                    SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            for (int i = 0; i < 4; i++)
            {
                float ringProgress = MathHelper.Clamp(progress - i * 0.08f, 0f, 1f);
                float ringFade = (1f - ringProgress) * 0.45f;
                float ringScale = 0.2f + ringProgress * 0.8f;
                sb.Draw(ring, pos, null, OvationTextures.BloomGold * ringFade, i * 0.3f, ringOrigin,
                    ringScale, SpriteEffects.None, 0f);
            }
            sb.Draw(harmonic, pos, null, OvationTextures.ApplauseFlash * fade * 0.4f, 0f, harmonicOrigin,
                0.3f * (1f + progress), SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, OvationTextures.PureJoyWhite * fade * 0.5f, 0f, glowOrigin,
                0.15f * (1f - progress * 0.5f), SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }
}