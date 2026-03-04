using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy;

namespace MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Projectiles
{
    /// <summary>
    /// JoyousFountainMinion — Stationary golden fountain.
    /// Heals allies (5HP/s base + tier scaling), fires golden droplets,
    /// provides Harmony Zone (+8% all damage), and erupts Joyous Geyser every 15s.
    /// ai[0] = unused. ai[1] = fountain tier (0-4+).
    /// </summary>
    public class JoyousFountainMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int FountainTier => (int)Projectile.ai[1];
        private int _attackTimer;
        private int _healTimer;
        private int _geyserTimer;
        private int _dropletIndex;

        // Tier bonuses
        private float HealPerSecond => 5f + FountainTier switch
        {
            1 => 3f,
            2 => 8f,    // 13 total at tier 3
            >= 3 => 8f + (FountainTier - 2) * 2f,
            _ => 0f
        };
        private float DamageMultiplier => 1f + FountainTier switch
        {
            1 => 0.2f,
            2 => 0.4f,
            >= 3 => 0.4f + (FountainTier - 2) * 0.1f,
            _ => 0f
        };
        private float HarmonyRadius => (15f + (FountainTier >= 2 ? 5f : 0f)) * 16f; // tiles to pixels
        private bool DropletsPierce => FountainTier >= 2;

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 60;
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
            if (!player.active || player.dead || !player.HasBuff(ModContent.BuffType<Buffs.JoyousFountainBuff>()))
            {
                Projectile.Kill();
                return;
            }
            Projectile.timeLeft = 2;
            Projectile.velocity = Vector2.Zero; // Stationary

            _attackTimer++;
            _healTimer++;
            _geyserTimer++;

            // Healing aura (every 60 frames = 1 second)
            if (_healTimer >= 60)
            {
                _healTimer = 0;
                int healAmount = (int)HealPerSecond;
                float healRange = HarmonyRadius;

                // Heal owner
                float ownerDist = Vector2.Distance(Projectile.Center, player.Center);
                if (ownerDist < healRange)
                {
                    // Inner Harmony Aura (3 tiles = 48px) gives triple healing
                    int actualHeal = ownerDist < 48f ? healAmount * 3 : healAmount;
                    player.statLife = Math.Min(player.statLife + actualHeal, player.statLifeMax2);
                    player.HealEffect(actualHeal);
                }

                // Heal other team members
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player p = Main.player[i];
                    if (!p.active || p.dead || i == Projectile.owner) continue;
                    if (p.team != player.team && player.team != 0) continue;
                    if (Vector2.Distance(Projectile.Center, p.Center) < healRange)
                    {
                        p.statLife = Math.Min(p.statLife + healAmount, p.statLifeMax2);
                        p.HealEffect(healAmount);
                    }
                }
            }

            // Harmony Zone — damage buff for nearby allies (applied every frame via player buffs)
            float harmonyDist = Vector2.Distance(Projectile.Center, player.Center);
            if (harmonyDist < HarmonyRadius)
            {
                // +8% all damage — applied as generic damage bonus
                // Inner aura gives +15%
                float bonus = harmonyDist < 48f ? 0.15f : 0.08f;
                player.GetDamage(DamageClass.Generic) += bonus;
            }

            // Fire golden droplets (1/s base, scaling with tier)
            int fireRate = Math.Max(30, 60 - FountainTier * 10); // Faster at higher tiers
            if (_attackTimer >= fireRate && Main.myPlayer == Projectile.owner)
            {
                _attackTimer = 0;
                NPC target = FindTarget(HarmonyRadius + 200f); // slightly beyond zone
                if (target != null)
                {
                    // Arcing upward trajectory (fountain spray)
                    Vector2 toTarget = target.Center - Projectile.Center;
                    float angle = (float)Math.Atan2(toTarget.Y, toTarget.X);
                    // Arc upward by biasing angle
                    float arcAngle = angle - MathHelper.PiOver4 + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 vel = new Vector2((float)Math.Cos(arcAngle), (float)Math.Sin(arcAngle)) * 8f;
                    vel.Y = Math.Min(vel.Y, -3f); // Always starts upward

                    int dmg = (int)(Projectile.damage * DamageMultiplier);
                    float pierce = DropletsPierce ? 2f : 0f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(0, -20),
                        vel, ModContent.ProjectileType<GoldenDropletProjectile>(), dmg, 2f, Projectile.owner, pierce, _dropletIndex++);
                }
            }

            // Joyous Geyser (every 15s = 900 frames)
            if (_geyserTimer >= 900 && Main.myPlayer == Projectile.owner)
            {
                _geyserTimer = 0;

                // Geyser burst projectile
                int geyserDmg = (int)(Projectile.damage * 2.5f * DamageMultiplier);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(0, -10),
                    Vector2.Zero, ModContent.ProjectileType<JoyousGeyserBurstProjectile>(), geyserDmg, 6f, Projectile.owner, FountainTier);

                // Instant heal for allies in range
                int geyserHeal = 30 + FountainTier * 10;
                if (Vector2.Distance(Projectile.Center, player.Center) < 20f * 16f)
                {
                    player.statLife = Math.Min(player.statLife + geyserHeal, player.statLifeMax2);
                    player.HealEffect(geyserHeal);
                }
            }

            // Ambient fountain particles — golden water spray upward
            if (Main.rand.NextBool(3))
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -Main.rand.NextFloat(2f, 5f));
                Dust d = Dust.NewDustDirect(Projectile.Center + new Vector2(-8, -30), 16, 4, DustID.GoldFlame,
                    vel.X, vel.Y, 80, FountainTextures.DropletGold, 0.4f);
                d.noGravity = false; // Falls back down like water
                d.fadeIn = 0.8f;
            }

            // Tier glow particles
            if (FountainTier > 0 && Main.rand.NextBool(4))
            {
                float ang = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(HarmonyRadius * 0.8f);
                Vector2 fieldPos = Projectile.Center + new Vector2((float)Math.Cos(ang), (float)Math.Sin(ang)) * dist;
                Dust d = Dust.NewDustDirect(fieldPos, 1, 1, DustID.GoldFlame, 0f, -0.3f, 120,
                    FountainTextures.JubilantLight * 0.5f, 0.3f);
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
                float d = Vector2.Distance(Projectile.Center, npc.Center);
                if (d < closestDist) { closestDist = d; closest = npc; }
            }
            return closest;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => false; // Stationary — attacks via droplets

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = FountainTextures.SoftGlow;
            Texture2D mask = FountainTextures.CircularMask;
            Texture2D sparkle = FountainTextures.OJBlossomSparkle;
            Vector2 glowOrigin = glow.Size() / 2f;
            Vector2 maskOrigin = mask.Size() / 2f;
            Vector2 sparkleOrigin = sparkle.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.05f);
            float tierScale = 1f + FountainTier * 0.15f;
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: CelebrationAura FloralSigil shader — harmony zone field ──
            Effect auraShader = OdeToJoyShaders.CelebrationAura;
            if (auraShader != null)
            {
                float zoneScale = HarmonyRadius / (mask.Width / 2f) * 0.5f;
                OdeToJoyShaders.SetAuraParams(auraShader, time, FountainTextures.BloomGold,
                    FountainTextures.JubilantLight, 0.12f * pulse, 1.2f, zoneScale * 0.5f, 3f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, auraShader, "FloralSigilTechnique");
                sb.Draw(mask, pos, null, Color.White * pulse, Main.GameUpdateCount * 0.003f, maskOrigin,
                    zoneScale, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: GardenBloom JubilantPulse shader — fountain body ──
            Effect bloomShader = OdeToJoyShaders.GardenBloom;
            if (bloomShader != null)
            {
                OdeToJoyShaders.SetBloomParams(bloomShader, time, FountainTextures.FountainCore,
                    FountainTextures.BloomGold, 0.6f * pulse, 2f, 0.25f * tierScale);
                bloomShader.Parameters["uPulseSpeed"]?.SetValue(1.2f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, bloomShader, "JubilantPulseTechnique");
                sb.Draw(glow, pos + new Vector2(0, -15), null, Color.White * pulse, 0f, glowOrigin,
                    0.25f * tierScale, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 2: Additive bloom overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            // Inner harmony aura (brighter, smaller)
            float innerScale = 48f / (mask.Width / 2f) * 0.5f;
            sb.Draw(mask, pos, null, FountainTextures.JubilantLight * 0.1f * pulse, 0f, maskOrigin,
                innerScale, SpriteEffects.None, 0f);
            // Sparkle body
            sb.Draw(sparkle, pos + new Vector2(0, -20), null, FountainTextures.FountainCore * 0.55f,
                Main.GameUpdateCount * 0.02f, sparkleOrigin, 0.2f * tierScale, SpriteEffects.None, 0f);
            // Core
            sb.Draw(glow, pos + new Vector2(0, -15), null, FountainTextures.PureJoyWhite * 0.3f * pulse,
                0f, glowOrigin, 0.08f * tierScale, SpriteEffects.None, 0f);

            // Tier indicators — small orbiting sparkles
            for (int i = 0; i < FountainTier; i++)
            {
                float orbAngle = Main.GameUpdateCount * 0.06f + i * MathHelper.TwoPi / Math.Max(1, FountainTier);
                Vector2 orbPos = pos + new Vector2((float)Math.Cos(orbAngle), (float)Math.Sin(orbAngle)) * (25f + i * 5f) + new Vector2(0, -15);
                sb.Draw(glow, orbPos, null, FountainTextures.RadiantAmber * 0.4f, 0f, glowOrigin, 0.04f, SpriteEffects.None, 0f);
            }

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }

    /// <summary>
    /// GoldenDropletProjectile — Arcing homing golden droplet.
    /// SparkleProjectileFoundation-style. Parabolic arc trajectory then homes to target.
    /// ai[0] = pierce count (0 = none, 2 = pierces at tier 3+). ai[1] = visual index.
    /// </summary>
    public class GoldenDropletProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";
        private int _timer;
        private readonly Vector2[] _trail = new Vector2[16];
        private int _trailIdx;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            _timer++;

            // Set pierce from ai[0]
            if (_timer == 1 && Projectile.ai[0] >= 2f)
                Projectile.penetrate = 3;

            // Record trail
            _trail[_trailIdx % _trail.Length] = Projectile.Center;
            _trailIdx++;

            // Phase 1: Arc upward (frames 1-20)
            if (_timer < 20)
            {
                Projectile.velocity.Y += 0.25f; // Gravity for arc
            }
            // Phase 2: Homing descent (frames 20+)
            else
            {
                NPC closest = null;
                float closestDist = 600f;
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
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 10f, 0.06f);
                }
                else
                {
                    Projectile.velocity.Y += 0.15f; // Fall if no target
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Sparkle trail
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame,
                    0f, 0f, 100, FountainTextures.GetDropletColor((int)Projectile.ai[1]), 0.3f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Splash particles
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustDirect(target.Center, 1, 1, DustID.GoldFlame,
                    vel.X, vel.Y, 60, FountainTextures.BloomGold, 0.5f);
                d.noGravity = false;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = FountainTextures.SoftGlow;
            Vector2 glowOrigin = glow.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            Color dropletColor = FountainTextures.GetDropletColor((int)Projectile.ai[1]);
            float fade = MathHelper.Clamp(_timer / 4f, 0f, 1f) * MathHelper.Clamp(Projectile.timeLeft / 10f, 0f, 1f);
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: TriumphantTrail VertexStrip — golden droplet trail ──
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

                VertexStrip strip = new VertexStrip();
                strip.PrepareStrip(positions, rotations,
                    (float p) => dropletColor * fade * p * 0.3f,
                    (float p) => MathHelper.Lerp(1f, 5f, p),
                    -Main.screenPosition, includeBacksides: true);
                OdeToJoyShaders.SetTrailParams(trailShader, time, dropletColor,
                    FountainTextures.PureJoyWhite, fade * 0.45f, 1.3f);
                trailShader.CurrentTechnique = trailShader.Techniques["TriumphantTrailTechnique"];
                trailShader.Parameters["WorldViewProjection"]?.SetValue(
                    Main.GameViewMatrix.NormalizedTransformationmatrix);
                trailShader.CurrentTechnique.Passes["P0"].Apply();
                strip.DrawTrail();
                Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            }

            // ── LAYER 1: Additive bloom ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            sb.Draw(glow, pos, null, dropletColor * fade * 0.55f, 0f, glowOrigin, 0.08f, SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, FountainTextures.PureJoyWhite * fade * 0.35f, 0f, glowOrigin, 0.03f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }

    /// <summary>
    /// JoyousGeyserBurstProjectile — Massive vertical burst every 15s.
    /// Damages all enemies within 20 tiles. ImpactFoundation-style expanding rings.
    /// ai[0] = fountain tier for scaling.
    /// </summary>
    public class JoyousGeyserBurstProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";
        private int _timer;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 45;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            _timer++;
            // Expand to 20 tile radius (320px)
            float targetSize = 320f + Projectile.ai[0] * 40f;
            float progress = MathHelper.Clamp(_timer / 20f, 0f, 1f);
            int newSize = (int)(targetSize * progress);
            if (newSize > 0)
                Projectile.Resize(newSize, newSize);

            // Geyser spray particles
            if (_timer < 25)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), -Main.rand.NextFloat(6f, 14f));
                    Dust d = Dust.NewDustDirect(Projectile.Center + new Vector2(Main.rand.NextFloat(-15f, 15f), 0), 1, 1,
                        DustID.GoldFlame, vel.X, vel.Y, 60, FountainTextures.DropletGold, 0.8f);
                    d.noGravity = false;
                    d.fadeIn = 1.2f;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D ring = FountainTextures.OJPowerRing;
            Texture2D glow = FountainTextures.SoftGlow;
            Texture2D harmonic = FountainTextures.OJHarmonicWave2;
            Vector2 ringOrigin = ring.Size() / 2f;
            Vector2 glowOrigin = glow.Size() / 2f;
            Vector2 harmonicOrigin = harmonic.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            float progress = MathHelper.Clamp(_timer / 20f, 0f, 1f);
            float fade = _timer < 30 ? 1f : 1f - (_timer - 30f) / 15f;
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: CelebrationAura shader — expanding geyser rings ──
            Effect auraShader = OdeToJoyShaders.CelebrationAura;
            if (auraShader != null)
            {
                float auraRadius = 0.1f + progress * 0.5f;
                OdeToJoyShaders.SetAuraParams(auraShader, time + progress * 4f, FountainTextures.BloomGold,
                    FountainTextures.FountainCore, fade * 0.5f, 2.5f, auraRadius, 6f);
                OdeToJoyShaders.BeginShaderBatch(sb, auraShader, "CelebrationAuraTechnique");
                auraShader.CurrentTechnique.Passes["P0"].Apply();
                float shaderScale = 0.2f + progress * 0.6f;
                sb.Draw(glow, pos, null, Color.White * fade, 0f, glowOrigin, shaderScale,
                    SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: JubilantHarmony SymphonicAura shader — harmonic burst center ──
            Effect harmonyShader = OdeToJoyShaders.JubilantHarmony;
            if (harmonyShader != null)
            {
                OdeToJoyShaders.SetBeamParams(harmonyShader, time, FountainTextures.JubilantLight,
                    FountainTextures.GeyserWhite, fade * 0.5f, 2f, 3f);
                harmonyShader.Parameters["uRadius"]?.SetValue(0.2f + progress * 0.3f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, harmonyShader, "SymphonicAuraTechnique");
                float burstScale = 0.2f + progress * 0.4f;
                sb.Draw(harmonic, pos, null, Color.White * fade, Main.GameUpdateCount * 0.02f,
                    harmonicOrigin, burstScale, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 2: Additive overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            // 6 staggered expanding rings
            for (int i = 0; i < 6; i++)
            {
                float ringProg = MathHelper.Clamp(progress - i * 0.06f, 0f, 1f);
                float ringFade = (1f - ringProg) * 0.4f * fade;
                float ringScale = 0.15f + ringProg * 0.6f;
                sb.Draw(ring, pos, null, FountainTextures.BloomGold * ringFade, i * 0.5f, ringOrigin,
                    ringScale, SpriteEffects.None, 0f);
            }

            // Vertical geyser column
            float columnHeight = progress * 3f;
            sb.Draw(glow, pos + new Vector2(0, -40), null, FountainTextures.FountainCore * fade * 0.35f, 0f,
                glowOrigin, new Vector2(0.08f, columnHeight), SpriteEffects.None, 0f);
            sb.Draw(glow, pos + new Vector2(0, -40), null, FountainTextures.PureJoyWhite * fade * 0.2f, 0f,
                glowOrigin, new Vector2(0.03f, columnHeight * 0.7f), SpriteEffects.None, 0f);
            // Core flash
            sb.Draw(glow, pos, null, FountainTextures.GeyserWhite * fade * 0.45f * (1f - progress * 0.5f),
                0f, glowOrigin, 0.12f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }
}