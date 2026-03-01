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
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Utilities;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Particles;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Projectiles
{
    // ═══════════════════════════════════════════════════════════
    // StandingOvationMinion — flying spirit minion, orbits player when idle,
    // fires JoyWaveProjectile at enemies every 40 frames.
    // +20% damage per additional ovation minion the player owns.
    // ═══════════════════════════════════════════════════════════
    public class StandingOvationMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/Summon/TheStandingOvationMinion";

        private ref float AttackTimer => ref Projectile.ai[0];
        private ref float HoverAngle => ref Projectile.ai[1];

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
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!CheckActive(owner))
                return;

            // Hover orbit around player when idle
            HoverAngle += 0.03f;
            Vector2 idlePosition = owner.Center + new Vector2(0, -80f)
                + HoverAngle.ToRotationVector2() * 100f;

            // Find target
            NPC target = OvationUtils.ClosestNPC(Projectile.Center, 800f);

            if (target != null)
            {
                // Move toward combat position above target
                Vector2 targetPos = target.Center + new Vector2(0, -100f);
                Vector2 toTarget = targetPos - Projectile.Center;

                if (toTarget.Length() > 50f)
                {
                    toTarget = toTarget.SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 12f, 0.1f);
                }
                else
                {
                    Projectile.velocity *= 0.9f;
                }

                // Count other spirits for sync bonus: +20% per additional minion
                int spiritCount = CountOtherSpirits();
                float damageBonus = 1f + spiritCount * 0.2f;

                // Attack every 40 frames
                AttackTimer++;
                if (AttackTimer >= 40)
                {
                    AttackTimer = 0;

                    Vector2 attackDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        attackDir * 14f,
                        ModContent.ProjectileType<JoyWaveProjectile>(),
                        (int)(Projectile.damage * 0.5f * damageBonus),
                        Projectile.knockBack,
                        Projectile.owner
                    );

                    SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.6f, Pitch = 0.4f }, Projectile.Center);

                    // Applause spark burst on attack
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 sparkVel = attackDir.RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 7f);
                        Color sparkColor = Color.Lerp(OvationUtils.SpotlightGold, OvationUtils.RoseApplause, Main.rand.NextFloat());
                        OvationParticleHandler.SpawnParticle(new ApplauseSparkParticle(
                            Projectile.Center, sparkVel, sparkColor, Main.rand.NextFloat(0.2f, 0.4f), 25));
                    }
                }
            }
            else
            {
                // Return to orbit position
                Vector2 toIdle = idlePosition - Projectile.Center;

                if (toIdle.Length() > 30f)
                {
                    toIdle = toIdle.SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdle * 10f, 0.08f);
                }
                else
                {
                    Projectile.velocity *= 0.9f;
                }
            }

            // Face movement direction
            if (Math.Abs(Projectile.velocity.X) > 0.5f)
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;

            // Ambient glow particles
            if (Main.rand.NextBool(8))
            {
                Color glowColor = Color.Lerp(OvationUtils.StageGold, OvationUtils.SpotlightGold, Main.rand.NextFloat());
                OvationParticleHandler.SpawnParticle(new OvationGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(18f, 18f),
                    Main.rand.NextVector2Circular(0.6f, 0.6f),
                    glowColor, Main.rand.NextFloat(0.15f, 0.3f), 20));
            }

            // Celebration mote particles drifting around
            if (Main.rand.NextBool(12))
            {
                Color moteColor = Color.Lerp(OvationUtils.RoseApplause, OvationUtils.EncoreGreen, Main.rand.NextFloat());
                OvationParticleHandler.SpawnParticle(new CelebrationMoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(25f, 25f),
                    Main.rand.NextVector2Circular(0.4f, 0.4f) + new Vector2(0, -0.3f),
                    moteColor, Main.rand.NextFloat(0.15f, 0.25f), 35));
            }

            Lighting.AddLight(Projectile.Center, OvationUtils.SpotlightGold.ToVector3() * 0.4f);
        }

        public override void OnKill(int timeLeft)
        {
            // Burst of music notes + applause sparks on death
            for (int i = 0; i < 5; i++)
            {
                Vector2 noteVel = new Vector2(0, -1.5f).RotatedByRandom(0.8f) * Main.rand.NextFloat(0.8f, 1.5f);
                OvationParticleHandler.SpawnParticle(new OvationNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    noteVel, Main.rand.NextFloat(0.3f, 0.5f), 45));
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                Color sparkColor = Color.Lerp(OvationUtils.SpotlightGold, OvationUtils.JoyfulWhite, Main.rand.NextFloat());
                OvationParticleHandler.SpawnParticle(new ApplauseSparkParticle(
                    Projectile.Center, sparkVel, sparkColor, Main.rand.NextFloat(0.2f, 0.35f), 20));
            }
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<Buffs.StandingOvationBuff>());
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<Buffs.StandingOvationBuff>()))
                Projectile.timeLeft = 2;

            return true;
        }

        private int CountOtherSpirits()
        {
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == Projectile.owner && proj.type == Type && proj.whoAmI != Projectile.whoAmI)
                    count++;
            }
            return count;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f;
            float time = (float)Main.GameUpdateCount / 60f;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            SpriteBatch sb = Main.spriteBatch;

            sb.End();

            // ═══ Layer 1: CelebrationAura shader — golden ring aura behind minion ═══
            Effect auraShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyCelebrationAuraShader);

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (auraShader != null)
            {
                auraShader.Parameters["uTime"]?.SetValue(time);
                auraShader.Parameters["uColor"]?.SetValue(OvationUtils.SpotlightGold.ToVector3());
                auraShader.Parameters["uSecondaryColor"]?.SetValue(OvationUtils.EncoreGreen.ToVector3());
                auraShader.Parameters["uOpacity"]?.SetValue(0.25f);
                auraShader.Parameters["uIntensity"]?.SetValue(1.0f);
                auraShader.Parameters["uRadius"]?.SetValue(0.35f);
                auraShader.Parameters["uRingCount"]?.SetValue(3f);
                auraShader.CurrentTechnique = auraShader.Techniques["CelebrationAuraTechnique"];
                auraShader.CurrentTechnique.Passes[0].Apply();

                sb.Draw(texture, drawPos, null, Color.White, 0f, origin,
                    pulse * 1.6f, effects, 0f);
            }

            // Golden glow behind minion
            Color glowColor = OvationUtils.Additive(OvationUtils.SpotlightGold, 0.35f);
            sb.Draw(texture, drawPos, null, glowColor, 0f, origin, pulse * 1.3f, effects, 0f);

            sb.End();
            OvationUtils.BeginDefault(sb);

            // Main sprite
            sb.Draw(texture, drawPos, null, lightColor, 0f, origin, pulse, effects, 0f);

            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // JoyWaveProjectile — golden crescent wave fired by minion.
    // 30x16, pen 2, timeLeft 120, homing 0.05, 1/2 damage, Poisoned 90.
    // ═══════════════════════════════════════════════════════════
    public class JoyWaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Gentle homing toward nearest enemy
            NPC target = OvationUtils.ClosestNPC(Projectile.Center, 600f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.05f);
            }

            // Golden wave-shaped glow scale pulsing
            Projectile.scale = 1f + (float)Math.Sin((120 - Projectile.timeLeft) * 0.1f) * 0.3f;

            // Joy wave trail particles
            if (Main.rand.NextBool(2))
            {
                Color trailColor = Color.Lerp(OvationUtils.StageGold, OvationUtils.SpotlightGold, Main.rand.NextFloat());
                OvationParticleHandler.SpawnParticle(new JoyWaveTrailParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    trailColor, Main.rand.NextFloat(0.2f, 0.35f), 18));
            }

            Lighting.AddLight(Projectile.Center, OvationUtils.SpotlightGold.ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 90);

            // Sparkle burst on hit
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                Color sparkColor = Color.Lerp(OvationUtils.SpotlightGold, OvationUtils.RoseApplause, Main.rand.NextFloat());
                OvationParticleHandler.SpawnParticle(new ApplauseSparkParticle(
                    target.Center, sparkVel, sparkColor, Main.rand.NextFloat(0.15f, 0.3f), 18));
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Burst of music notes + applause spark shower
            for (int i = 0; i < 4; i++)
            {
                Vector2 noteVel = new Vector2(0, -1.5f).RotatedByRandom(1.0f) * Main.rand.NextFloat(0.8f, 1.5f);
                OvationParticleHandler.SpawnParticle(new OvationNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    noteVel, Main.rand.NextFloat(0.3f, 0.45f), 40));
            }
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                Color sparkColor = Color.Lerp(OvationUtils.SpotlightGold, OvationUtils.JoyfulWhite, Main.rand.NextFloat());
                OvationParticleHandler.SpawnParticle(new ApplauseSparkParticle(
                    Projectile.Center, sparkVel, sparkColor, Main.rand.NextFloat(0.2f, 0.35f), 22, true));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            SpriteBatch sb = Main.spriteBatch;
            float time = (float)Main.GameUpdateCount / 60f;

            sb.End();

            // ═══ Layer 1: TriumphantTrail shader on afterimages — golden energy trail ═══
            Effect trailShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyTriumphantTrailShader);

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (trailShader != null)
            {
                trailShader.Parameters["uTime"]?.SetValue(time);
                trailShader.Parameters["uColor"]?.SetValue(OvationUtils.StageGold.ToVector3());
                trailShader.Parameters["uSecondaryColor"]?.SetValue(OvationUtils.SpotlightGold.ToVector3());
                trailShader.Parameters["uIntensity"]?.SetValue(1.2f);
                trailShader.CurrentTechnique = trailShader.Techniques["TriumphantTrailTechnique"];
            }

            // Draw afterimages with shader
            for (int i = 0; i < Projectile.oldPos.Length && i < 5; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                Vector2 oldDraw = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float afterFade = 1f - (i / 5f);

                if (trailShader != null)
                {
                    trailShader.Parameters["uOpacity"]?.SetValue(afterFade * 0.35f);
                    trailShader.CurrentTechnique.Passes[0].Apply();
                }

                Color afterColor = OvationUtils.Additive(OvationUtils.StageGold, afterFade * 0.3f);
                sb.Draw(texture, oldDraw, null, afterColor, Projectile.rotation, origin,
                    Projectile.scale * 0.5f * (1f - i * 0.12f), SpriteEffects.None, 0f);
            }

            // Main golden wave glow
            Color mainGlow = OvationUtils.Additive(OvationUtils.SpotlightGold, 0.65f);
            sb.Draw(texture, drawPos, null, mainGlow, Projectile.rotation, origin,
                Projectile.scale * 0.6f, SpriteEffects.None, 0f);

            // Bright inner core
            Color coreColor = OvationUtils.Additive(OvationUtils.JoyfulWhite, 0.5f);
            sb.Draw(texture, drawPos, null, coreColor, Projectile.rotation, origin,
                Projectile.scale * 0.3f, SpriteEffects.None, 0f);

            sb.End();
            OvationUtils.BeginDefault(sb);

            return false;
        }
    }
}
