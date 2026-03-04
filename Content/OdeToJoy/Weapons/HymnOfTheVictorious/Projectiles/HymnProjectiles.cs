using MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Projectiles
{
    /// <summary>
    /// V1 Exordium — Pure gold piercing bolt. High damage, clean lines.
    /// FBM noise internally, 3-layer additive rendering.
    /// </summary>
    public class ExordiumBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int TrailLength = 20;
        private Vector2[] _trail = new Vector2[TrailLength];
        private int _head;
        private int _timer;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            _timer++;
            _trail[_head] = Projectile.Center;
            _head = (_head + 1) % TrailLength;
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, -Projectile.velocity.X * 0.1f, -Projectile.velocity.Y * 0.1f, 100, HymnTextures.BloomGold, 0.5f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<HymnDebuffNPC>().RegisterVerseHit(target, 0);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = HymnTextures.SoftGlow;
            Vector2 origin = glow.Size() / 2f;
            float fade = MathHelper.Clamp(_timer / 6f, 0f, 1f) * MathHelper.Clamp(Projectile.timeLeft / 15f, 0f, 1f);
            float time = (float)Main.timeForVisualEffects * 0.015f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            sb.End();

            // ── LAYER 0: TriumphantTrail shader VertexStrip trail ──
            Effect trailShader = OdeToJoyShaders.TriumphantTrail;
            int validCount = 0;
            for (int i = 0; i < TrailLength; i++)
            {
                int idx = (_head - 1 - i + TrailLength) % TrailLength;
                if (_trail[idx] != Vector2.Zero) validCount++; else break;
            }
            if (trailShader != null && validCount >= 2)
            {
                Vector2[] positions = new Vector2[validCount];
                float[] rotations = new float[validCount];
                for (int i = 0; i < validCount; i++)
                {
                    int idx = (_head - 1 - i + TrailLength) % TrailLength;
                    positions[validCount - 1 - i] = _trail[idx];
                }
                for (int i = 0; i < validCount; i++)
                {
                    if (i < validCount - 1) rotations[i] = (positions[i + 1] - positions[i]).ToRotation();
                    else rotations[i] = rotations[Math.Max(0, i - 1)];
                }

                // Wide glow underlayer
                VertexStrip glowStrip = new VertexStrip();
                glowStrip.PrepareStrip(positions, rotations,
                    (float p) => HymnTextures.BloomGold * fade * p * 0.3f,
                    (float p) => MathHelper.Lerp(2f, 18f, p),
                    -Main.screenPosition, includeBacksides: true);
                OdeToJoyShaders.SetTrailParams(trailShader, time, HymnTextures.BloomGold,
                    HymnTextures.RadiantAmber, fade * 0.5f, 1.5f);
                trailShader.CurrentTechnique = trailShader.Techniques["TriumphantTrailTechnique"];
                trailShader.Parameters["WorldViewProjection"]?.SetValue(
                    Main.GameViewMatrix.NormalizedTransformationmatrix);
                trailShader.CurrentTechnique.Passes["P0"].Apply();
                glowStrip.DrawTrail();

                // Narrow bright core trail
                VertexStrip coreStrip = new VertexStrip();
                coreStrip.PrepareStrip(positions, rotations,
                    (float p) => HymnTextures.JubilantLight * fade * p * 0.6f,
                    (float p) => MathHelper.Lerp(1f, 8f, p),
                    -Main.screenPosition, includeBacksides: true);
                OdeToJoyShaders.SetTrailParams(trailShader, time * 1.3f, HymnTextures.JubilantLight,
                    HymnTextures.BloomGold, fade * 0.8f, 2.0f);
                trailShader.CurrentTechnique.Passes["P0"].Apply();
                coreStrip.DrawTrail();
                Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            }

            // ── LAYER 1: Additive bloom head ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            // Lightweight trail sparkles
            for (int i = 0; i < TrailLength; i++)
            {
                int idx = (_head - 1 - i + TrailLength) % TrailLength;
                if (_trail[idx] == Vector2.Zero) continue;
                float t = 1f - i / (float)TrailLength;
                sb.Draw(glow, _trail[idx] - Main.screenPosition, null,
                    HymnTextures.BloomGold * fade * t * 0.1f, 0f, origin, 0.06f * t, SpriteEffects.None, 0f);
            }

            // 3-tier bloom head
            sb.Draw(glow, pos, null, HymnTextures.BloomGold * fade * 0.5f, 0f, origin, 0.35f, SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, HymnTextures.RadiantAmber * fade * 0.45f, 0f, origin, 0.18f, SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, HymnTextures.JubilantLight * fade * 0.35f, 0f, origin, 0.08f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }

    /// <summary>
    /// V2 Rising — Petal pink spreading bolt. 3-way fan, applies Jubilant Burn.
    /// Warmer tones, flowing FBM noise.
    /// </summary>
    public class RisingBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";
        private int _timer;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            _timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 120, HymnTextures.PetalPink, 0.4f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<JubilantBurnDebuff>(), 240);
            var hymnNPC = target.GetGlobalNPC<HymnDebuffNPC>();
            hymnNPC.BurnBaseDamage = hit.Damage;
            hymnNPC.RegisterVerseHit(target, 1);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = HymnTextures.SoftGlow;
            Vector2 origin = glow.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float fade = MathHelper.Clamp(_timer / 6f, 0f, 1f) * MathHelper.Clamp(Projectile.timeLeft / 15f, 0f, 1f);
            float time = (float)Main.timeForVisualEffects * 0.015f;
            float pulse = 0.85f + 0.15f * (float)Math.Sin(_timer * 0.15f);

            sb.End();

            // ── LAYER 0: GardenBloom shader petal body ──
            Effect bloomShader = OdeToJoyShaders.GardenBloom;
            if (bloomShader != null)
            {
                OdeToJoyShaders.SetBloomParams(bloomShader, time, HymnTextures.PetalPink,
                    HymnTextures.BloomGold, fade * 0.55f * pulse, 1.8f, 0.35f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, bloomShader, "GardenBloomTechnique");
                sb.Draw(glow, pos, null, Color.White * fade * pulse, Projectile.rotation, origin,
                    0.3f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive bloom layers ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            sb.Draw(glow, pos, null, HymnTextures.PetalPink * fade * 0.5f, 0f, origin, 0.28f, SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, HymnTextures.BloomGold * fade * 0.35f, 0f, origin, 0.15f, SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, HymnTextures.JubilantLight * fade * 0.25f, 0f, origin, 0.06f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }

    /// <summary>
    /// V3 Apex — Largest orb. Hovers at target position for 60 frames, then detonates.
    /// PerlinFlow noise, golden + jubilant colors.
    /// </summary>
    public class ApexOrbProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";
        private int _timer;
        private bool _hovering;
        private Vector2 _hoverPos;

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            _timer++;

            if (!_hovering)
            {
                // Travel toward initial target
                if (_timer > 30 || Projectile.velocity.Length() < 2f)
                {
                    _hovering = true;
                    _hoverPos = Projectile.Center;
                    Projectile.velocity = Vector2.Zero;
                    Projectile.timeLeft = 80; // 60 hover + 20 detonation
                }
            }
            else
            {
                Projectile.Center = _hoverPos;
                Projectile.velocity = Vector2.Zero;

                // Pulsing glow particles
                float pulse = 0.7f + 0.3f * (float)Math.Sin(_timer * 0.15f);
                if (Main.rand.NextBool(2))
                {
                    Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                    Dust d = Dust.NewDustDirect(Projectile.Center - new Vector2(15), 30, 30, DustID.GoldFlame, vel.X, vel.Y, 100, HymnTextures.JubilantLight, 0.5f * pulse);
                    d.noGravity = true;
                }

                // Detonate at end of hover
                if (Projectile.timeLeft <= 20 && Projectile.timeLeft == 20)
                {
                    // Set hitbox large for AoE
                    Projectile.Resize(200, 200);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<HymnDebuffNPC>().RegisterVerseHit(target, 2);
        }

        public override void OnKill(int timeLeft)
        {
            // AoE detonation VFX
            for (int i = 0; i < 45; i++)
            {
                float angle = MathHelper.TwoPi * i / 45f;
                float speed = 4f + Main.rand.NextFloat() * 3f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                Color c = Color.Lerp(HymnTextures.BloomGold, HymnTextures.JubilantLight, Main.rand.NextFloat());
                Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.GoldFlame, vel.X, vel.Y, 80, c, 0.9f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = HymnTextures.SoftGlow;
            Texture2D ring = HymnTextures.OJPowerRing;
            Vector2 glowOrigin = glow.Size() / 2f;
            Vector2 ringOrigin = ring.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            float fade = MathHelper.Clamp(_timer / 8f, 0f, 1f) * MathHelper.Clamp(Projectile.timeLeft / 15f, 0f, 1f);
            float pulse = 0.85f + 0.15f * (float)Math.Sin(_timer * 0.12f);
            float scale = _hovering ? 0.5f * pulse : 0.3f;
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: GardenBloom JubilantPulse shader — pulsing orb body ──
            Effect bloomShader = OdeToJoyShaders.GardenBloom;
            if (bloomShader != null)
            {
                OdeToJoyShaders.SetBloomParams(bloomShader, time, HymnTextures.BloomGold,
                    HymnTextures.JubilantLight, fade * 0.5f * pulse, 2.2f, 0.45f);
                bloomShader.Parameters["uPulseSpeed"]?.SetValue(1.2f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, bloomShader, "JubilantPulseTechnique");
                sb.Draw(glow, pos, null, Color.White * fade * pulse, 0f, glowOrigin,
                    scale * 1.8f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: CelebrationAura shader — concentric rings (hover mode) ──
            Effect auraShader = OdeToJoyShaders.CelebrationAura;
            if (auraShader != null && _hovering)
            {
                float auraRadius = 0.3f + 0.1f * (float)Math.Sin(_timer * 0.08f);
                OdeToJoyShaders.SetAuraParams(auraShader, time, HymnTextures.RadiantAmber,
                    HymnTextures.BloomGold, fade * 0.35f, 1.5f, auraRadius, 3f);
                OdeToJoyShaders.BeginShaderBatch(sb, auraShader, "CelebrationAuraTechnique");
                auraShader.CurrentTechnique.Passes["P0"].Apply();
                sb.Draw(glow, pos, null, Color.White * fade * 0.4f, 0f, glowOrigin,
                    scale * 1.2f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 2: Additive bloom overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            // Outer glow
            sb.Draw(glow, pos, null, HymnTextures.BloomGold * fade * 0.35f, 0f, glowOrigin,
                scale * 2f, SpriteEffects.None, 0f);
            // Orb body
            sb.Draw(glow, pos, null, HymnTextures.JubilantLight * fade * 0.5f, 0f, glowOrigin,
                scale, SpriteEffects.None, 0f);
            // Power ring (hover mode)
            if (_hovering)
                sb.Draw(ring, pos, null, HymnTextures.RadiantAmber * fade * 0.35f, _timer * 0.03f,
                    ringOrigin, scale * 0.6f, SpriteEffects.None, 0f);
            // Hot center
            sb.Draw(glow, pos, null, HymnTextures.PureJoyWhite * fade * 0.4f, 0f, glowOrigin,
                scale * 0.3f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }

    /// <summary>
    /// V4 Gloria — Fracturing bolt that splits into 6 homing fragments on contact.
    /// VoronoiCell noise, amber → white. Encore triggers on kills from Complete Hymn.
    /// </summary>
    public class GloriaBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";
        private int _timer;

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            _timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 100, HymnTextures.RadiantAmber, 0.5f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<HymnDebuffNPC>().RegisterVerseHit(target, 3);

            // Split into 6 homing fragments
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 8f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel,
                        ModContent.ProjectileType<GloriaFragmentProjectile>(), Projectile.damage / 3, 1f, Projectile.owner);
                }
            }

            // Split VFX
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.GoldFlame, vel.X, vel.Y, 80, HymnTextures.PureJoyWhite, 0.7f);
                d.noGravity = true;
            }

            // Encore check — if part of Complete Hymn and kill happens
            if (target.life <= 0)
            {
                Main.player[Projectile.owner].GetModPlayer<HymnPlayer>().TriggerEncore();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = HymnTextures.SoftGlow;
            Texture2D sparkle = HymnTextures.OJBlossomSparkle;
            Vector2 glowOrigin = glow.Size() / 2f;
            Vector2 sparkleOrigin = sparkle.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float fade = MathHelper.Clamp(_timer / 6f, 0f, 1f) * MathHelper.Clamp(Projectile.timeLeft / 15f, 0f, 1f);
            float time = (float)Main.timeForVisualEffects * 0.015f;
            float pulse = 0.9f + 0.1f * (float)Math.Sin(_timer * 0.2f);

            sb.End();

            // ── LAYER 0: VerdantSlash shader — fracture pattern overlay ──
            Effect slashShader = OdeToJoyShaders.VerdantSlash;
            if (slashShader != null)
            {
                OdeToJoyShaders.SetSlashParams(slashShader, time, HymnTextures.RadiantAmber,
                    HymnTextures.PureJoyWhite, fade * 0.5f, 2.0f, 0f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, slashShader, "ThornImpactTechnique");
                sb.Draw(glow, pos, null, Color.White * fade * pulse, Projectile.rotation, glowOrigin,
                    0.35f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive bloom + sparkle body ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            // Fracture sparkle body
            sb.Draw(sparkle, pos, null, HymnTextures.RadiantAmber * fade * 0.6f, Projectile.rotation,
                sparkleOrigin, 0.35f, SpriteEffects.None, 0f);
            // Outer glow
            sb.Draw(glow, pos, null, HymnTextures.RadiantAmber * fade * 0.4f, 0f, glowOrigin, 0.32f,
                SpriteEffects.None, 0f);
            // Mid glow
            sb.Draw(glow, pos, null, HymnTextures.JubilantLight * fade * 0.35f, 0f, glowOrigin, 0.15f,
                SpriteEffects.None, 0f);
            // Hot core
            sb.Draw(glow, pos, null, HymnTextures.PureJoyWhite * fade * 0.4f, 0f, glowOrigin, 0.06f,
                SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }

    /// <summary>
    /// Gloria Fragment — Small homing fragment from Gloria split.
    /// AttackFoundation Mode 4. Golden sparkle trail.
    /// </summary>
    public class GloriaFragmentProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";
        private int _timer;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            _timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Homing after brief scatter
            if (_timer > 10)
            {
                NPC closest = null;
                float closestDist = 500f;
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
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 12f, 0.06f);
                }
            }

            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 120, HymnTextures.BloomGold, 0.3f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = HymnTextures.SoftGlow;
            Vector2 origin = glow.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float fade = MathHelper.Clamp(_timer / 5f, 0f, 1f) * MathHelper.Clamp(Projectile.timeLeft / 10f, 0f, 1f);
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: GardenBloom shader — small petal accent ──
            Effect bloomShader = OdeToJoyShaders.GardenBloom;
            if (bloomShader != null)
            {
                OdeToJoyShaders.SetBloomParams(bloomShader, time * 1.5f, HymnTextures.BloomGold,
                    HymnTextures.RadiantAmber, fade * 0.4f, 1.5f, 0.25f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, bloomShader, "GardenBloomTechnique");
                sb.Draw(glow, pos, null, Color.White * fade, Projectile.rotation, origin,
                    0.18f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive bloom ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            sb.Draw(glow, pos, null, HymnTextures.BloomGold * fade * 0.4f, 0f, origin, 0.15f, SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, HymnTextures.JubilantLight * fade * 0.3f, 0f, origin, 0.06f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }
}