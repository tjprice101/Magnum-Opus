using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.ResonantWeapons.SymphonysEnd
{
    /// <summary>
    /// Spiraling spectral blade — the primary projectile of Symphony's End.
    /// 
    /// Behaviour:
    ///   • Corkscrews toward the cursor position stored in ai[0]/ai[1]
    ///   • Accelerates over its lifetime
    ///   • Deals bonus damage at high velocity
    ///   • Shatters into 4 <see cref="SymphonyBladeFragment"/>s on kill/expiry
    ///   • Shader-driven trail + self-contained particle VFX
    /// </summary>
    public class SymphonySpiralBlade : ModProjectile
    {
        // Use the item sprite — custom rendering handles visuals
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/SymphonysEnd";

        // ─── Constants ────────────────────────────────────────────
        private const int MaxLifetime  = 180;  // 3 seconds
        private const float BaseSpeed  = 10f;
        private const float MaxBoost   = 14f;
        private const float HelixSpeed = 0.25f;
        private const float BaseHelixRadius = 35f;

        // ─── State ────────────────────────────────────────────────
        private float helixAngle;

        private Vector2 TargetPos => new Vector2(Projectile.ai[0], Projectile.ai[1]);
        private float Age => 1f - (float)Projectile.timeLeft / MaxLifetime;

        // ─── Setup ────────────────────────────────────────────────

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width  = 24;
            Projectile.height = 24;
            Projectile.friendly    = true;
            Projectile.DamageType  = DamageClass.Magic;
            Projectile.penetrate   = 3;
            Projectile.timeLeft    = MaxLifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown  = 10;
            Projectile.extraUpdates = 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            helixAngle = Main.rand.NextFloat(MathHelper.TwoPi);

            // Track active blade count
            if (Main.player[Projectile.owner] is Player owner)
                owner.Symphony().ActiveBladeCount++;
        }

        // ─── AI: Helix Steering ───────────────────────────────────

        public override void AI()
        {
            float age   = Age;
            float speed = BaseSpeed + age * MaxBoost;

            // Helix phase advance
            helixAngle += HelixSpeed;
            float helixRadius = BaseHelixRadius * (1f - age * 0.6f);

            // Steering toward cursor
            Vector2 toTarget = TargetPos - Projectile.Center;
            float dist = toTarget.Length();
            Vector2 dir  = toTarget.SafeNormalize(Vector2.UnitX);
            Vector2 perp = new Vector2(-dir.Y, dir.X);

            // Helix component — perpendicular oscillation
            Vector2 helixVel = perp * MathF.Cos(helixAngle) * helixRadius * 0.15f;

            // Desired velocity
            Vector2 desired = dir * speed + helixVel;

            // Smooth steering (more responsive as blade ages)
            float steerFactor = 0.08f + age * 0.12f;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, steerFactor);

            // Close-range burst-through
            if (dist < 32f && age > 0.3f)
                Projectile.velocity *= 1.05f;

            // Rotation follows velocity
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // ─── VFX ──────────────────────────────────────────────
            if (!Main.dedServ)
            {
                SymphonyParticleFactory.SpawnSpiralTrailParticles(Projectile.Center, Projectile.velocity);

                Color lightCol = SymphonyUtils.GetSymphonyGradient(age);
                Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * 0.4f);
            }
        }

        // ─── Damage Scaling ───────────────────────────────────────

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float speedFactor = Projectile.velocity.Length() / BaseSpeed;
            if (speedFactor > 1.5f)
            {
                float bonus = MathHelper.Clamp((speedFactor - 1.5f) * 0.3f, 0f, 0.5f);
                modifiers.SourceDamage += bonus;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);

            // Impact VFX
            if (!Main.dedServ)
            {
                SymphonyParticleFactory.SpawnShatterBurst(Projectile.Center, 8);
                SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.3f, Volume = 0.6f }, Projectile.Center);
            }
        }

        // ─── Death → Shatter ──────────────────────────────────────

        public override void OnKill(int timeLeft)
        {
            SpawnFragments();

            // Decrement blade counter
            if (Main.player[Projectile.owner] is Player owner)
            {
                var sym = owner.Symphony();
                sym.ActiveBladeCount = System.Math.Max(0, sym.ActiveBladeCount - 1);
            }

            if (!Main.dedServ)
            {
                SymphonyParticleFactory.SpawnShatterBurst(Projectile.Center);
                SymphonyParticleHandler.Spawn(SymphonyParticleFactory.Ring(
                    Projectile.Center, SymphonyUtils.FinalWhite, 0.2f, 22));
                Lighting.AddLight(Projectile.Center, SymphonyUtils.SymphonyPink.ToVector3() * 0.8f);
            }
        }

        private void SpawnFragments()
        {
            if (Main.myPlayer != Projectile.owner) return;

            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 fragVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    fragVel,
                    ModContent.ProjectileType<SymphonyBladeFragment>(),
                    Projectile.damage / 3,
                    Projectile.knockBack * 0.5f,
                    Projectile.owner);
            }
        }

        // ─── Custom Drawing ───────────────────────────────────────

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // 1. End default SpriteBatch for custom rendering
            sb.End();

            // 2. Draw trail (shader-driven)
            DrawTrail();

            // 3. Chromatic afterimage echoes + blade sprite (additive)
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
            DrawAfterimages(sb);
            DrawBlade(sb);
            sb.End();

            // 4. Restart normal SpriteBatch
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        /// <summary>Chromatic-separated afterimage echoes along the spiral path.</summary>
        private void DrawAfterimages(SpriteBatch sb)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() * 0.5f;
            float age = Age;
            float time = (float)Main.timeForVisualEffects;

            // Draw RGB-separated echoes at old positions
            for (int i = 2; i < Projectile.oldPos.Length; i += 2)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                float t = (float)i / Projectile.oldPos.Length;
                float echoAlpha = (1f - t) * 0.25f;
                float echoScale = 0.45f * (1f - t * 0.3f);

                Vector2 basePos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float rot = Projectile.oldRot[i];

                // Chromatic separation: offset pink and violet slightly in opposite directions
                Vector2 chromOffset = new Vector2(MathF.Cos(rot), MathF.Sin(rot)) * (2f + t * 3f);

                // Violet echo (offset backward)
                sb.Draw(tex, basePos - chromOffset, null,
                    SymphonyUtils.Additive(SymphonyUtils.SymphonyViolet, echoAlpha * 0.6f),
                    rot, origin, echoScale * 1.1f, SpriteEffects.None, 0f);

                // Pink echo (offset forward)
                sb.Draw(tex, basePos + chromOffset, null,
                    SymphonyUtils.Additive(SymphonyUtils.SymphonyPink, echoAlpha * 0.5f),
                    rot, origin, echoScale, SpriteEffects.None, 0f);

                // Faint white core echo
                sb.Draw(tex, basePos, null,
                    SymphonyUtils.Additive(SymphonyUtils.FinalWhite, echoAlpha * 0.2f),
                    rot, origin, echoScale * 0.6f, SpriteEffects.None, 0f);
            }
        }

        private void DrawTrail()
        {
            Effect shader = SymphonyShaderLoader.SymphonySpiralTrail;
            if (shader != null)
            {
                try { shader.CurrentTechnique = shader.Techniques["SpiralMain"]; }
                catch { /* technique missing — fallback handles it */ }
            }

            SymphonyTrailRenderer.DrawTrail(
                Projectile.oldPos,
                Projectile.Size * 0.5f,
                SymphonyTrailSettings.SpiralBlade,
                shader);
        }

        private void DrawBlade(SpriteBatch sb)
        {
            Texture2D tex  = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin  = tex.Size() * 0.5f;

            float pulse = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.08f) * 0.1f;
            float scale = 0.5f * pulse;
            float age   = Age;
            float time  = (float)Main.timeForVisualEffects;

            // Harmonic overtone ring — small rotating satellites around the blade
            int harmonics = 3;
            float ringRadius = 12f + MathF.Sin(time * 0.06f) * 3f;
            for (int h = 0; h < harmonics; h++)
            {
                float hAngle = time * 0.1f * (1f + h * 0.5f) + h * MathHelper.TwoPi / harmonics;
                Vector2 hPos = drawPos + hAngle.ToRotationVector2() * ringRadius;
                float hAlpha = 0.2f + age * 0.15f;
                Color hCol = h switch
                {
                    0 => SymphonyUtils.SymphonyViolet,
                    1 => SymphonyUtils.SymphonyPink,
                    _ => SymphonyUtils.FinalWhite,
                };
                sb.Draw(tex, hPos, null,
                    SymphonyUtils.Additive(hCol, hAlpha * 0.4f),
                    Projectile.rotation + h * MathHelper.PiOver2, origin, scale * 0.35f, SpriteEffects.None, 0f);
            }

            // Outer violet glow
            sb.Draw(tex, drawPos, null,
                SymphonyUtils.Additive(SymphonyUtils.SymphonyViolet, 0.35f),
                Projectile.rotation, origin, scale * 1.3f, SpriteEffects.None, 0f);

            // Mid pink glow
            sb.Draw(tex, drawPos, null,
                SymphonyUtils.Additive(SymphonyUtils.SymphonyPink, 0.4f),
                Projectile.rotation, origin, scale * 1.1f, SpriteEffects.None, 0f);

            // Hot white core (brightens with age)
            sb.Draw(tex, drawPos, null,
                SymphonyUtils.Additive(SymphonyUtils.FinalWhite, 0.5f + age * 0.3f),
                Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
