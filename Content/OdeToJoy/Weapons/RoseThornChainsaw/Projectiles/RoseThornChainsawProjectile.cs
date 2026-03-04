using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Buffs;
using MagnumOpus.Content.OdeToJoy;

namespace MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Projectiles
{
    /// <summary>
    /// Chainsaw Holdout — continuous held projectile.
    /// Renders a spinning thorn ribbon around the weapon,
    /// flings thorn shrapnel periodically, and erupts in Petal Storm.
    /// </summary>
    public class ChainsawHoldoutProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int holdTimer;
        private int thornFlingTimer;
        private float revSpeed; // 0 → 1 (max RPM)
        private float petalStormMeter; // 0 → 1 (erupts at 1)
        private float ribbonAngle;
        private bool hasFiredPetalStorm;

        private const int ThornFlingInterval = 30; // 0.5s
        private const int PetalStormChargeFrames = 240; // 4s
        private const float MaxRevTime = 120f; // 2s to max rev

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // Channel check
            if (!owner.channel || owner.dead || !owner.active)
            {
                Projectile.Kill();
                return;
            }

            holdTimer++;
            thornFlingTimer++;
            owner.heldProj = Projectile.whoAmI;

            // Aim toward mouse
            Vector2 aimDir = (Main.MouseWorld - owner.MountedCenter).SafeNormalize(Vector2.UnitX);
            Projectile.Center = owner.MountedCenter + aimDir * 40f;
            Projectile.rotation = aimDir.ToRotation();
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

            // Direction
            owner.direction = Math.Sign(aimDir.X) != 0 ? Math.Sign(aimDir.X) : 1;
            owner.itemTime = 2;
            owner.itemAnimation = 2;

            // Rev up
            revSpeed = MathHelper.Clamp(holdTimer / MaxRevTime, 0f, 1f);

            // Petal Storm meter
            petalStormMeter = MathHelper.Clamp(holdTimer / (float)PetalStormChargeFrames, 0f, 1f);

            // Petal Storm eruption
            if (petalStormMeter >= 1f && !hasFiredPetalStorm)
            {
                hasFiredPetalStorm = true;
                FirePetalStorm(owner);
            }

            // Thorn fling every 0.5s
            if (thornFlingTimer >= ThornFlingInterval)
            {
                thornFlingTimer = 0;
                FlingThorns(owner);
            }

            // Ribbon spin (faster with rev)
            ribbonAngle += 0.3f + revSpeed * 0.4f;

            // Damage scales with rev
            float revBonus = revSpeed * 0.4f; // up to +40%
            Projectile.damage = (int)(owner.HeldItem.damage * (1f + revBonus));

            // VFX: continuous particles
            SpawnChainsawParticles(owner, aimDir);

            // Lighting
            Lighting.AddLight(Projectile.Center,
                ChainsawTextures.RadiantAmber.ToVector3() * (0.3f + revSpeed * 0.3f));
        }

        private void SpawnChainsawParticles(Player owner, Vector2 aimDir)
        {
            // Thorn debris flying off
            if (holdTimer % 3 == 0)
            {
                float angle = ribbonAngle + Main.rand.NextFloat(-0.5f, 0.5f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f * (1f + revSpeed));
                Color col = Color.Lerp(ChainsawTextures.PetalPink, ChainsawTextures.RadiantAmber,
                    Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.2f, 0.5f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
            }

            // Rev trail sparks at high RPM
            if (revSpeed > 0.5f && holdTimer % 2 == 0)
            {
                Vector2 sparkPos = Projectile.Center + aimDir * 25f
                    + Main.rand.NextVector2Circular(5f, 5f);
                Vector2 vel = aimDir.RotatedByRandom(0.8f) * Main.rand.NextFloat(3f, 6f);
                Dust dust = Dust.NewDustPerfect(sparkPos, DustID.RainbowMk2, vel,
                    newColor: ChainsawTextures.BloomGold,
                    Scale: Main.rand.NextFloat(0.15f, 0.35f));
                dust.noGravity = true;
            }
        }

        private void FlingThorns(Player owner)
        {
            // Find 2 closest enemies within 8 tiles (128px)
            int flinged = 0;
            for (int i = 0; i < Main.maxNPCs && flinged < 2; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (Vector2.Distance(owner.Center, npc.Center) > 128f) continue;

                Vector2 dir = (npc.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.NewProjectile(
                    owner.GetSource_ItemUse(owner.HeldItem),
                    Projectile.Center, dir * 12f,
                    ModContent.ProjectileType<ThornShrapnelProjectile>(),
                    (int)(Projectile.damage * 0.3f), Projectile.knockBack * 0.3f,
                    owner.whoAmI);
                flinged++;
            }
        }

        private void FirePetalStorm(Player owner)
        {
            SoundEngine.PlaySound(SoundID.Item119 with { Pitch = 0.2f, Volume = 0.8f },
                Projectile.Center);

            // 12 petal projectiles in all directions
            int petalCount = 12;
            for (int i = 0; i < petalCount; i++)
            {
                float angle = MathHelper.TwoPi / petalCount * i;
                Vector2 vel = angle.ToRotationVector2() * 10f;
                Projectile.NewProjectile(
                    owner.GetSource_ItemUse(owner.HeldItem),
                    Projectile.Center, vel,
                    ModContent.ProjectileType<PetalStormProjectile>(),
                    (int)(Projectile.damage * 0.6f), Projectile.knockBack * 0.5f,
                    owner.whoAmI);
            }

            // Burst VFX: spiral petal particles
            for (int i = 0; i < 55; i++)
            {
                float angle = MathHelper.TwoPi / 55f * i;
                float speed = Main.rand.NextFloat(3f, 8f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                // Spiral: add tangential component
                vel += angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * speed * 0.3f;

                Color col = (i % 4) switch
                {
                    0 => ChainsawTextures.PetalPink,
                    1 => ChainsawTextures.BloomGold,
                    2 => ChainsawTextures.JubilantLight,
                    _ => ChainsawTextures.PureJoyWhite
                };

                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.5f, 1f));
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<RoseThornBleedDebuff>(), 180);

            // Contact impact sparks
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero).RotatedByRandom(0.5f)
                    * Main.rand.NextFloat(2f, 5f);
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel,
                    newColor: ChainsawTextures.RadiantAmber,
                    Scale: Main.rand.NextFloat(0.3f, 0.5f));
                dust.noGravity = true;
            }

            // Crit: bonus shrapnel
            if (hit.Crit)
            {
                Player owner = Main.player[Projectile.owner];
                Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.NewProjectile(
                    owner.GetSource_ItemUse(owner.HeldItem),
                    Projectile.Center, dir * 10f,
                    ModContent.ProjectileType<ThornShrapnelProjectile>(),
                    (int)(Projectile.damage * 0.2f), 2f, owner.whoAmI);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Player owner = Main.player[Projectile.owner];
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: VerdantSlash shader — spinning thorn aura ──
            Effect slashShader = OdeToJoyShaders.VerdantSlash;
            if (slashShader != null)
            {
                OdeToJoyShaders.SetSlashParams(slashShader, time + ribbonAngle * 0.5f,
                    ChainsawTextures.RadiantAmber, ChainsawTextures.BloomGold,
                    (0.4f + revSpeed * 0.4f), 2f + revSpeed, revSpeed);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, slashShader, "VerdantSlashTechnique");
                Texture2D softGlow = ChainsawTextures.SoftGlow.Value;
                Vector2 glowOrigin = softGlow.Size() / 2f;
                sb.Draw(softGlow, drawPos, null, Color.White, ribbonAngle, glowOrigin,
                    0.15f + revSpeed * 0.1f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            // Spinning ribbon thorn fragments
            Texture2D trailTex = ChainsawTextures.OJTrail.Value;
            int ribbonSegments = 6;
            for (int i = 0; i < ribbonSegments; i++)
            {
                float segAngle = ribbonAngle + MathHelper.TwoPi / ribbonSegments * i;
                float dist = 20f + revSpeed * 10f;
                Vector2 segPos = drawPos + segAngle.ToRotationVector2() * dist;

                Color segColor = Color.Lerp(ChainsawTextures.RoseShadow, ChainsawTextures.RadiantAmber,
                    (float)Math.Sin(segAngle * 2f + holdTimer * 0.1f) * 0.5f + 0.5f);

                Texture2D thornTex = ChainsawTextures.OJThornFragment.Value;
                Vector2 thornOrigin = thornTex.Size() / 2f;
                sb.Draw(thornTex, segPos, null,
                    segColor * (0.45f + revSpeed * 0.35f),
                    segAngle + MathHelper.PiOver2, thornOrigin,
                    0.35f + revSpeed * 0.1f, SpriteEffects.None, 0f);
            }

            // Core glow at chainsaw tip
            {
                Texture2D softGlow2 = ChainsawTextures.SoftGlow.Value;
                Vector2 glowOrigin2 = softGlow2.Size() / 2f;
                sb.Draw(softGlow2, drawPos, null,
                    ChainsawTextures.RadiantAmber * (0.25f + revSpeed * 0.25f),
                    0f, glowOrigin2, 0.08f + revSpeed * 0.04f, SpriteEffects.None, 0f);
                sb.Draw(softGlow2, drawPos, null,
                    ChainsawTextures.PureJoyWhite * (0.15f + revSpeed * 0.15f),
                    0f, glowOrigin2, 0.03f, SpriteEffects.None, 0f);
            }

            // Petal storm meter indicator
            if (petalStormMeter > 0.1f && !hasFiredPetalStorm)
            {
                Texture2D ringTex = ChainsawTextures.OJPowerEffectRing.Value;
                Vector2 ringOrigin = ringTex.Size() / 2f;
                float ringScale = petalStormMeter * 0.4f;
                Color ringColor = Color.Lerp(ChainsawTextures.PetalPink,
                    ChainsawTextures.JubilantLight, petalStormMeter);
                sb.Draw(ringTex, drawPos, null,
                    ringColor * petalStormMeter * 0.35f,
                    holdTimer * 0.02f, ringOrigin, ringScale,
                    SpriteEffects.None, 0f);
            }

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: ChainsawTextures.PetalPink,
                    Scale: Main.rand.NextFloat(0.2f, 0.5f));
                dust.noGravity = true;
            }
        }
    }

    /// <summary>
    /// Thorn Shrapnel — flung from chainsaw at nearby enemies.
    /// Rose Shadow trailing sparks, embeds on contact with bleed.
    /// </summary>
    public class ThornShrapnelProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int timer;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity.Y += 0.12f; // slight gravity

            // Rose Shadow trail
            if (timer % 2 == 0)
            {
                Vector2 vel = -Projectile.velocity * 0.05f;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: ChainsawTextures.RoseShadow,
                    Scale: Main.rand.NextFloat(0.15f, 0.3f));
                dust.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center,
                ChainsawTextures.PetalPink.ToVector3() * 0.15f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<RoseThornBleedDebuff>(), 180);

            for (int i = 0; i < 4; i++)
            {
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2,
                    Main.rand.NextVector2Circular(3f, 3f),
                    newColor: ChainsawTextures.RoseShadow,
                    Scale: Main.rand.NextFloat(0.2f, 0.4f));
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float fadeIn = MathHelper.Clamp(timer / 5f, 0f, 1f);
            float fadeOut = Projectile.timeLeft < 15 ? Projectile.timeLeft / 15f : 1f;
            float alpha = fadeIn * fadeOut;
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: VerdantSlash ThornImpact shader — thorn accent ──
            Effect slashShader = OdeToJoyShaders.VerdantSlash;
            if (slashShader != null)
            {
                OdeToJoyShaders.SetSlashParams(slashShader, time, ChainsawTextures.RoseShadow,
                    ChainsawTextures.RadiantAmber, alpha * 0.5f, 1.5f, 0f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, slashShader, "ThornImpactTechnique");
                Texture2D thornTex = ChainsawTextures.OJThornFragment.Value;
                Vector2 thornOrigin = thornTex.Size() / 2f;
                sb.Draw(thornTex, drawPos, null, Color.White * alpha,
                    Projectile.rotation, thornOrigin, 0.4f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive bloom ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            Texture2D glow = ChainsawTextures.SoftGlow.Value;
            Vector2 glowOrigin = glow.Size() / 2f;
            sb.Draw(glow, drawPos, null,
                ChainsawTextures.PetalPink * alpha * 0.25f,
                0f, glowOrigin, 0.05f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }

    /// <summary>
    /// Petal Storm Projectile — razor petal from 360° eruption.
    /// Travels outward 12 tiles with petal sprite and pink trail.
    /// </summary>
    public class PetalStormProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int timer;
        private const int MaxLifetime = 60;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 3;
            Projectile.timeLeft = MaxLifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            timer++;
            Projectile.rotation += 0.2f;

            // Petal trail
            if (timer % 2 == 0)
            {
                Vector2 vel = -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = Color.Lerp(ChainsawTextures.PetalPink,
                    ChainsawTextures.JubilantLight, Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.2f, 0.4f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
            }

            Lighting.AddLight(Projectile.Center,
                ChainsawTextures.PetalPink.ToVector3() * 0.2f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2,
                    Main.rand.NextVector2Circular(3f, 3f),
                    newColor: ChainsawTextures.PetalPink,
                    Scale: Main.rand.NextFloat(0.3f, 0.6f));
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float fadeIn = MathHelper.Clamp(timer / 5f, 0f, 1f);
            float fadeOut = Projectile.timeLeft < 12 ? Projectile.timeLeft / 12f : 1f;
            float alpha = fadeIn * fadeOut;
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: GardenBloom GardenBloomTechnique shader — petal shimmer ──
            Effect bloomShader = OdeToJoyShaders.GardenBloom;
            if (bloomShader != null)
            {
                OdeToJoyShaders.SetBloomParams(bloomShader, time + Projectile.identity * 0.4f,
                    ChainsawTextures.PetalPink, ChainsawTextures.BloomGold, alpha * 0.5f, 1.5f, 0.12f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, bloomShader, "GardenBloomTechnique");
                Texture2D petalTex = ChainsawTextures.OJRosePetal.Value;
                Vector2 petalOrigin = petalTex.Size() / 2f;
                sb.Draw(petalTex, drawPos, null, Color.White * alpha,
                    Projectile.rotation, petalOrigin, 0.5f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive bloom ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            Texture2D glow = ChainsawTextures.SoftGlow.Value;
            Vector2 glowOrigin = glow.Size() / 2f;
            sb.Draw(glow, drawPos, null,
                ChainsawTextures.BloomGold * alpha * 0.3f,
                0f, glowOrigin, 0.07f, SpriteEffects.None, 0f);
            sb.Draw(glow, drawPos, null,
                ChainsawTextures.PureJoyWhite * alpha * 0.15f,
                0f, glowOrigin, 0.025f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2,
                    Main.rand.NextVector2Circular(2f, 2f),
                    newColor: ChainsawTextures.PetalPink,
                    Scale: Main.rand.NextFloat(0.2f, 0.4f));
                dust.noGravity = true;
            }
        }
    }

    /// <summary>
    /// ThornChainProjectile — kept for backward compatibility.
    /// Bouncing thorn chain that ricochets off terrain.
    /// </summary>
    public class ThornChainProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int timer;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 6;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Bounce
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X;
            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y;

            // Bounce particles
            for (int i = 0; i < 3; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2,
                    Main.rand.NextVector2Circular(2f, 2f),
                    newColor: ChainsawTextures.RadiantAmber,
                    Scale: Main.rand.NextFloat(0.2f, 0.4f));
                dust.noGravity = true;
            }

            Projectile.penetrate--;
            return false;
        }

        public override void AI()
        {
            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (timer % 3 == 0)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2,
                    -Projectile.velocity * 0.05f,
                    newColor: ChainsawTextures.RoseShadow,
                    Scale: Main.rand.NextFloat(0.15f, 0.25f));
                dust.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<RoseThornBleedDebuff>(), 120);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float alpha = Projectile.timeLeft < 20 ? Projectile.timeLeft / 20f : 1f;
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: VerdantSlash VerdantSlashTechnique shader — vine trail accent ──
            Effect slashShader = OdeToJoyShaders.VerdantSlash;
            if (slashShader != null)
            {
                OdeToJoyShaders.SetSlashParams(slashShader, time, ChainsawTextures.RadiantAmber,
                    ChainsawTextures.RoseShadow, alpha * 0.45f, 1.5f, 0f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, slashShader, "VerdantSlashTechnique");
                Texture2D thornTex = ChainsawTextures.OJThornFragment.Value;
                Vector2 thornOrigin = thornTex.Size() / 2f;
                sb.Draw(thornTex, drawPos, null, Color.White * alpha,
                    Projectile.rotation, thornOrigin, 0.5f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive bloom ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            Texture2D glow = ChainsawTextures.SoftGlow.Value;
            Vector2 glowOrigin = glow.Size() / 2f;
            sb.Draw(glow, drawPos, null,
                ChainsawTextures.PetalPink * alpha * 0.2f,
                0f, glowOrigin, 0.06f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }
    }
}