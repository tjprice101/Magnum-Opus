using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Shaders;
using MagnumOpus.Content.LaCampanella.Debuffs;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;
using MagnumOpus.Content.FoundationWeapons.ExplosionParticlesFoundation;
using MagnumOpus.Content.FoundationWeapons.SmokeFoundation;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Projectiles
{
    /// <summary>
    /// Bell shockwave — expanding ring of fire + concussive blast.
    /// ai[0]: 0 = normal, 2 = Infernal Crescendo (larger, stronger), 
    /// ai[1]: 0 = normal, 1 = Bell Sacrifice (massive AoE)
    /// 
    /// Harmonic Convergence: If another active MinionShockwaveProj overlaps this one,
    /// enemies hit in the intersection zone take 2x damage.
    /// </summary>
    public class MinionShockwaveProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        private const int NormalDuration = 25;
        private const float NormalMaxRadius = 180f;

        private const int CrescendoDuration = 35;
        private const float CrescendoMaxRadius = 280f;

        private const int SacrificeDuration = 40;
        private const float SacrificeMaxRadius = 350f;

        private bool IsCrescendo => Projectile.ai[0] == 2f;
        private bool IsSacrifice => Projectile.ai[1] == 1f;

        private int Duration => IsSacrifice ? SacrificeDuration : (IsCrescendo ? CrescendoDuration : NormalDuration);
        private float MaxRadius => IsSacrifice ? SacrificeMaxRadius : (IsCrescendo ? CrescendoMaxRadius : NormalMaxRadius);

        private bool _damageDealt;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 40; // Will be overwritten in AI init
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // Hit each NPC once
        }

        public override void AI()
        {
            // Set timeLeft on first frame
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                Projectile.timeLeft = Duration;

                // Spawn VFX burst
                SpawnInitialVFX();
            }

            float progress = 1f - (float)Projectile.timeLeft / Duration;

            // Damage pulse at ~30% expansion
            if (!_damageDealt && progress >= 0.3f)
            {
                _damageDealt = true;
                DealWaveDamage(progress);
            }

            // Continuous ember particles along ring edge
            if (Projectile.timeLeft > Duration / 2 && Main.GameUpdateCount % 2 == 0)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = (float)Math.Sqrt(progress) * MaxRadius;
                Vector2 pos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                InfernalChimesParticleHandler.SpawnParticle(new ChoirEmberParticle(
                    pos, angle, 3f, 0.05f, Main.rand.Next(15, 30)));
            }

            // Light
            float lightIntensity = (1f - progress) * (IsSacrifice ? 2f : IsCrescendo ? 1.5f : 1f);
            Lighting.AddLight(Projectile.Center, InfernalChimesCallingUtils.ChoirPalette[2].ToVector3() * lightIntensity);
        }

        private void SpawnInitialVFX()
        {
            // Bell ring pulse
            InfernalChimesParticleHandler.SpawnParticle(new BellRingPulseParticle(
                Projectile.Center, MaxRadius, Duration - 5));

            // Musical notes burst outward
            int noteCount = IsSacrifice ? 16 : (IsCrescendo ? 12 : 8);
            for (int i = 0; i < noteCount; i++)
            {
                float angle = MathHelper.TwoPi / noteCount * i;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(2f, 5f);
                vel.Y -= 1f;
                InfernalChimesParticleHandler.SpawnParticle(new MusicalChoirNoteParticle(
                    Projectile.Center + vel * 5f, vel, Main.rand.Next(50, 80)));
            }

            // Ember ring
            int emberCount = IsSacrifice ? 30 : (IsCrescendo ? 20 : 12);
            for (int i = 0; i < emberCount; i++)
            {
                float angle = MathHelper.TwoPi / emberCount * i + Main.rand.NextFloat(-0.1f, 0.1f);
                InfernalChimesParticleHandler.SpawnParticle(new ChoirEmberParticle(
                    Projectile.Center, angle, 10f, 0.12f, Main.rand.Next(25, 45)));
            }

            if (IsSacrifice)
                SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            else
                SoundEngine.PlaySound(SoundID.Item28, Projectile.Center);

            // === FOUNDATION: RippleEffectProjectile — Shockwave ring per minion attack ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<RippleEffectProjectile>(),
                0, 0f, Projectile.owner, ai0: 1f);

            // === FOUNDATION: Sacrifice-specific effects ===
            if (IsSacrifice)
            {
                // SparkExplosionProjectile — Bell Sacrifice massive gold/flame burst
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<SparkExplosionProjectile>(),
                    0, 0f, Projectile.owner,
                    ai0: (float)SparkMode.SpiralShrapnel);

                // DamageZoneProjectile — Sacrifice persistent damage zone
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<DamageZoneProjectile>(),
                    (int)(Projectile.damage * 0.3f), 0f, Projectile.owner);
            }
        }

        private void DealWaveDamage(float progress)
        {
            float currentRadius = MaxRadius * (float)Math.Sqrt(progress);

            // Check for Harmonic Convergence: are there other active shockwaves overlapping?
            bool hasConvergence = false;
            int myType = Projectile.type;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (!other.active || other.whoAmI == Projectile.whoAmI || other.type != myType || other.owner != Projectile.owner)
                    continue;

                float dist = Vector2.Distance(Projectile.Center, other.Center);
                float otherProgress = 1f - (float)other.timeLeft / Duration;
                float otherRadius = MaxRadius * (float)Math.Sqrt(Math.Max(otherProgress, 0f));

                // Waves overlap if distance < sum of radii
                if (dist < currentRadius + otherRadius)
                {
                    hasConvergence = true;
                    break;
                }
            }

            float convergenceMultiplier = hasConvergence ? 2f : 1f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist <= currentRadius)
                {
                    int dmg = (int)(Projectile.damage * convergenceMultiplier);
                    Player owner = Main.player[Projectile.owner];
                    owner.ApplyDamageToNPC(npc, dmg, Projectile.knockBack,
                        Projectile.Center.X < npc.Center.X ? 1 : -1, false);
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, IsSacrifice ? 3 : (IsCrescendo ? 2 : 1));

                    // Harmonic Convergence VFX on affected enemies
                    if (hasConvergence)
                    {
                        InfernalChimesParticleHandler.SpawnParticle(new ChoirEmberParticle(
                            npc.Center, Main.rand.NextFloat(MathHelper.TwoPi), 8f, 0.1f, 15));
                    }
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Collision handled manually in DealWaveDamage
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            float progress = 1f - (float)Projectile.timeLeft / Duration;
            float fade = 1f - progress;

            var tex = ModContent.Request<Texture2D>(Texture, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            float ringRadius = MaxRadius * (float)Math.Sqrt(progress);
            float ringScale = ringRadius / (tex.Width * 0.5f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;

            try { sb.End(); } catch { }
            try
            {
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Core blast
            Color coreColor = InfernalChimesCallingUtils.Additive(
                InfernalChimesCallingUtils.ShockwavePalette[1], fade * 0.35f);
            sb.Draw(tex, drawPos, null, coreColor, 0f, origin, ringScale * 0.8f, SpriteEffects.None, 0f);

            // Outer ring
            Color ringColor = InfernalChimesCallingUtils.Additive(
                InfernalChimesCallingUtils.ShockwavePalette[2], fade * 0.25f);
            sb.Draw(tex, drawPos, null, ringColor, 0f, origin, ringScale, SpriteEffects.None, 0f);

            // Hot center flash (early frames only)
            if (progress < 0.3f)
            {
                float flashFade = 1f - progress / 0.3f;
                Color flashColor = InfernalChimesCallingUtils.Additive(
                    InfernalChimesCallingUtils.ShockwavePalette[4], flashFade * 0.5f);
                sb.Draw(tex, drawPos, null, flashColor, 0f, origin, 0.5f, SpriteEffects.None, 0f);
            }

            // Sacrifice has extra intense core
            if (IsSacrifice && progress < 0.5f)
            {
                float sacFlash = 1f - progress / 0.5f;
                Color sacColor = InfernalChimesCallingUtils.Additive(
                    InfernalChimesCallingUtils.ShockwavePalette[5], sacFlash * 0.6f);
                sb.Draw(tex, drawPos, null, sacColor, 0f, origin, ringScale * 0.6f, SpriteEffects.None, 0f);
            }

            // === SHADER: MusicalShockwave overlay ===
            var shockShader = InfernalChimesCallingShaderLoader.GetShockwaveShader();
            if (shockShader != null)
            {
                try
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                    shockShader.UseColor(InfernalChimesCallingUtils.ShockwavePalette[2]);
                    shockShader.UseSecondaryColor(InfernalChimesCallingUtils.ShockwavePalette[4]);
                    shockShader.UseOpacity(fade * 0.6f);
                    shockShader.UseSaturation(fade); // uIntensity
                    var shockFx = shockShader.Shader;
                    if (shockFx != null)
                    {
                        shockFx.Parameters["uTime"]?.SetValue((float)Main.GameUpdateCount * 0.02f);
                        shockFx.Parameters["uOverbrightMult"]?.SetValue(IsSacrifice ? 1.6f : IsCrescendo ? 1.4f : 1.2f);
                        shockFx.Parameters["uPhase"]?.SetValue(progress);
                        shockFx.Parameters["uScrollSpeed"]?.SetValue(1.5f);
                        shockFx.Parameters["uNoiseScale"]?.SetValue(3f);
                    }
                    shockShader.Apply();

                    // Draw the shockwave ring at full scale with shader-driven wavefront
                    Color shaderCol = Color.White * fade * 0.5f;
                    sb.Draw(tex, drawPos, null, shaderCol, 0f, origin, ringScale * 1.1f, SpriteEffects.None, 0f);

                    // Secondary inner ring for depth
                    Color innerShader = Color.White * fade * 0.3f;
                    sb.Draw(tex, drawPos, null, innerShader, 0f, origin, ringScale * 0.65f, SpriteEffects.None, 0f);

                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch
                {
                    try
                    {
                        sb.End();
                        sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                            DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    }
                    catch { }
                }
            }
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }

            } // end outer try
            catch
            {
                try
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }
            return false;
        }
    }
}
