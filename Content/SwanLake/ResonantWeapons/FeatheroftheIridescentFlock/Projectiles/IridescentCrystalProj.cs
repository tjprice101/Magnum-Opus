using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Utilities;
using MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Primitives;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using Terraria.Graphics.Shaders;
using ReLogic.Content;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Projectiles
{
    /// <summary>
    /// Orbiting crystal minion for Feather of the Iridescent Flock (summoner).
    /// V-formation positioning, 4-state cycle: Formation → ShardVolley → DiveAttack → Return.
    /// Crystal resonance at 4+ active crystals. Fires CrystalShardProj volleys.
    /// Foundation-pattern rendering: SpriteBatch bloom layers, no primitives/custom particles.
    /// </summary>
    public class IridescentCrystalProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        // --- AI State Machine ---
        private enum CrystalState { FormationFlight, ShardVolley, DiveAttack, Return }

        public ref float Timer => ref Projectile.ai[0];
        public ref float StateTimer => ref Projectile.ai[1];
        private ref float StateFloat => ref Projectile.localAI[0];
        public ref float CrystalIndex => ref Projectile.localAI[1]; // Position in V-formation

        private CrystalState CurrentState
        {
            get => (CrystalState)(int)StateFloat;
            set => StateFloat = (float)value;
        }

        private const int TrailLength = 10;
        private Vector2[] oldPos = new Vector2[TrailLength];
        private float idleRotation = 0f;
        private int volleyShots = 0;
        private FlockPrimitiveRenderer _trailRenderer;

        // Hit flash system for noise zone rendering at impact points
        private Vector2 _lastHitPos;
        private int _hitFlashTimer;
        private const int HitFlashDuration = 14;

        private Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 1f;
            Projectile.minion = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => CurrentState == CrystalState.DiveAttack;

        public override void AI()
        {
            // --- Minion buff check ---
            if (!CheckActive()) return;

            Timer++;
            StateTimer++;

            // Hit flash decay
            if (_hitFlashTimer > 0) _hitFlashTimer--;

            // --- Trail recording ---
            for (int i = TrailLength - 1; i > 0; i--)
                oldPos[i] = oldPos[i - 1];
            oldPos[0] = Projectile.Center;

            // --- Idle rotation ---
            idleRotation += 0.02f;

            // --- State machine ---
            NPC target = FindTarget();

            switch (CurrentState)
            {
                case CrystalState.FormationFlight:
                    DoFormationFlight(target);
                    break;
                case CrystalState.ShardVolley:
                    DoShardVolley(target);
                    break;
                case CrystalState.DiveAttack:
                    DoDiveAttack(target);
                    break;
                case CrystalState.Return:
                    DoReturn();
                    break;
            }

            // --- Ambient iridescent dust ---
            if (Timer % 6 == 0)
            {
                Color c = FlockUtils.GetIridescent(Timer * 0.01f + CrystalIndex * 0.25f);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8),
                    DustID.WhiteTorch, Vector2.Zero, 0, c, 0.4f);
                d.noGravity = true;
            }

            // Lighting
            Color lightColor = FlockUtils.GetIridescent(Timer * 0.005f);
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * 0.4f);

            // Rotation visual
            Projectile.rotation += 0.03f;
        }

        private bool CheckActive()
        {
            Player owner = Owner;
            if (owner.dead || !owner.active)
            {
                Projectile.Kill();
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<IridescentFlockBuff>()))
                Projectile.timeLeft = 2;

            return true;
        }

        public override void OnKill(int timeLeft)
        {
            _trailRenderer?.Dispose();
            _trailRenderer = null;
        }

        // ================================================================
        // STATE: Formation Flight — idle V-formation around owner
        // ================================================================
        private void DoFormationFlight(NPC target)
        {
            // V-formation positioning
            Vector2 formationPos = GetFormationPosition();
            Vector2 toFormation = formationPos - Projectile.Center;
            float dist = toFormation.Length();

            if (dist > 8f)
            {
                float speed = MathHelper.Clamp(dist * 0.08f, 2f, 16f);
                Projectile.velocity = toFormation.SafeNormalize(Vector2.UnitY) * speed;
            }
            else
            {
                Projectile.velocity *= 0.85f;
            }

            // Transition to attack if target in range
            if (target != null && StateTimer > 60)
            {
                float targetDist = Vector2.Distance(Projectile.Center, target.Center);
                if (targetDist < 500f)
                {
                    // Alternate between volley and dive based on crystal index
                    bool doVolley = (int)CrystalIndex % 2 == 0 || CountActiveCrystals() < 3;
                    TransitionTo(doVolley ? CrystalState.ShardVolley : CrystalState.DiveAttack);
                }
            }
        }

        private Vector2 GetFormationPosition()
        {
            int index = (int)CrystalIndex;
            float spacing = 50f;
            float forwardOffset = -40f; // Behind owner

            // V-formation: even indices go left, odd go right
            int side = index % 2 == 0 ? -1 : 1;
            int row = (index + 1) / 2;

            Vector2 offset = new Vector2(forwardOffset - row * spacing * 0.5f, side * row * spacing);
            // Gentle bobbing
            offset.Y += MathF.Sin(Timer * 0.04f + index * 1.2f) * 8f;

            return Owner.Center + offset;
        }

        // ================================================================
        // STATE: Shard Volley — fires 3-burst CrystalShardProj homing shards
        // ================================================================
        private void DoShardVolley(NPC target)
        {
            if (target == null || !target.active)
            {
                TransitionTo(CrystalState.Return);
                return;
            }

            // Hover near owner during volley
            Vector2 hoverPos = Owner.Center + new Vector2(-30f, -60f - CrystalIndex * 20f);
            Vector2 toHover = hoverPos - Projectile.Center;
            Projectile.velocity = toHover.SafeNormalize(Vector2.UnitY) * MathHelper.Clamp(toHover.Length() * 0.06f, 0f, 8f);

            // Fire shards in 3-burst
            if (StateTimer % 15 == 0 && volleyShots < 3 && Projectile.owner == Main.myPlayer)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                float spread = MathHelper.ToRadians(10f) * (volleyShots - 1);
                Vector2 vel = toTarget.RotatedBy(spread) * 10f;

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                    vel, ModContent.ProjectileType<CrystalShardProj>(),
                    Projectile.damage / 2, Projectile.knockBack * 0.5f, Projectile.owner);

                volleyShots++;
                SoundEngine.PlaySound(SoundID.Item28, Projectile.Center);

                // Fire dust
                for (int i = 0; i < 3; i++)
                {
                    Color c = FlockUtils.GetIridescent(Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, vel * 0.3f, 0, c, 0.6f);
                    d.noGravity = true;
                }
            }

            // After 3 shots, return
            if (volleyShots >= 3 && StateTimer > 60)
                TransitionTo(CrystalState.Return);
        }

        // ================================================================
        // STATE: Dive Attack — synchronized dive toward target
        // ================================================================
        private void DoDiveAttack(NPC target)
        {
            if (target == null || !target.active)
            {
                TransitionTo(CrystalState.Return);
                return;
            }

            // Windup phase (first 20 ticks): pull back slightly
            if (StateTimer < 20)
            {
                Vector2 awayFromTarget = (Projectile.Center - target.Center).SafeNormalize(Vector2.UnitY);
                Projectile.velocity = awayFromTarget * 3f;
            }
            // Dive phase
            else if (StateTimer < 60)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                float diveSpeed = 14f + (StateTimer - 20) * 0.15f;
                Projectile.velocity = toTarget * diveSpeed;

                // Dive trail dust
                if (StateTimer % 2 == 0)
                {
                    Color c = FlockUtils.GetOilSheen(Projectile.rotation, Timer * 0.02f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                        -Projectile.velocity * 0.2f, 0, c, 0.7f);
                    d.noGravity = true;
                }
            }
            // Post-dive: transition back
            else
            {
                TransitionTo(CrystalState.Return);
            }
        }

        // ================================================================
        // STATE: Return — fly back to formation
        // ================================================================
        private void DoReturn()
        {
            Vector2 formationPos = GetFormationPosition();
            Vector2 toPos = formationPos - Projectile.Center;
            float dist = toPos.Length();

            float speed = MathHelper.Clamp(dist * 0.1f, 3f, 18f);
            Projectile.velocity = toPos.SafeNormalize(Vector2.UnitY) * speed;

            if (dist < 30f || StateTimer > 90)
                TransitionTo(CrystalState.FormationFlight);
        }

        private void TransitionTo(CrystalState newState)
        {
            CurrentState = newState;
            StateTimer = 0;
            volleyShots = 0;
        }

        private NPC FindTarget()
        {
            // Check for player-targeted NPC first
            if (Owner.HasMinionAttackTargetNPC)
            {
                NPC target = Main.npc[Owner.MinionAttackTargetNPC];
                if (target.active && !target.friendly && Vector2.Distance(Projectile.Center, target.Center) < 800f)
                    return target;
            }

            // Find closest hostile NPC
            NPC closest = null;
            float bestDist = 600f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        private int CountActiveCrystals()
        {
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == Projectile.type &&
                    Main.projectile[i].owner == Projectile.owner)
                    count++;
            }
            return count;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SwansMark>(), 240);

            // Record hit for noise zone flash
            _lastHitPos = target.Center;
            _hitFlashTimer = HitFlashDuration;

            // Iridescent impact sparkle
            for (int i = 0; i < 10; i++)
            {
                Color c = FlockUtils.GetIridescent(i / 10f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(5, 5), 0, c, 0.8f);
                d.noGravity = true;
            }

            // Crystal resonance: if 4+ crystals active, bonus burst
            if (CountActiveCrystals() >= 4)
            {
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi / 12f * i;
                    Vector2 vel = angle.ToRotationVector2() * 4f;
                    Color c = FlockUtils.GetOilSheen(angle, Timer * 0.01f);
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch, vel, 0, c, 1.0f);
                    d.noGravity = true;
                }
            }

            try
            {
                SwanLakeVFXLibrary.SpawnMixedSparkleImpact(target.Center, 0.7f, 5, 5);
                SwanLakeVFXLibrary.SpawnMusicNotes(target.Center, 2, 14f);
                if (CountActiveCrystals() >= 4)
                {
                    SwanLakeVFXLibrary.SpawnMixedSparkleImpact(target.Center, 0.5f, 4, 4);
                }
            } catch { }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Vector2 screenPos = Main.screenPosition;

            try
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Texture2D point = MagnumTextureRegistry.GetPointBloom();
                Texture2D radial = MagnumTextureRegistry.GetRadialBloom();
                Texture2D star = MagnumTextureRegistry.GetStar4Soft();
                Texture2D haloRing = MagnumTextureRegistry.GetHaloRing();

                Vector2 drawPos = Projectile.Center - screenPos;
                float pulse = 0.85f + 0.15f * MathF.Sin((float)Main.timeForVisualEffects * 0.06f + CrystalIndex * 1.5f);
                float crystalHue = CrystalIndex / 3f; // 0.0 / 0.33 / 0.66 for 3 crystals
                bool isDiving = CurrentState == CrystalState.DiveAttack;
                bool isMovingFast = Projectile.velocity.Length() > 3f;
                float stateIntensity = isDiving ? 1.4f : (CurrentState == CrystalState.ShardVolley ? 1.1f : 0.85f);

                // ========================================================
                // HIT FLASH NOISE ZONE — renders at last impact position
                // ========================================================
                if (_hitFlashTimer > 0 && _lastHitPos != Vector2.Zero)
                {
                    float flashProgress = 1f - (float)_hitFlashTimer / HitFlashDuration;
                    float flashAlpha = (1f - flashProgress) * 0.7f;
                    float flashRadius = MathHelper.Lerp(10f, 45f, flashProgress);
                    float flashTime = (float)Main.timeForVisualEffects * 0.02f;

                    SwanLakeVFXLibrary.DrawPrismaticSparkleImpact(sb, _lastHitPos, flashRadius, flashTime, flashAlpha, 6);
                }

                // --- Formation lines to nearby crystals ---
                DrawFormationLines(sb, screenPos, point);

                // ========================================================
                // GPU SHADER TRAIL — CrystalOrbitTrail (3-pass faceted prismatic)
                // Only render when moving or diving for visual clarity
                // ========================================================
                if ((isDiving || isMovingFast) && oldPos[1] != Vector2.Zero && GameShaders.Misc.ContainsKey("MagnumOpus:CrystalOrbitTrail"))
                {
                    // End SpriteBatch before GPU primitive rendering
                    sb.End();

                    _trailRenderer ??= new FlockPrimitiveRenderer();

                    var trailShader = GameShaders.Misc["MagnumOpus:CrystalOrbitTrail"];
                    var effect = trailShader.Shader;

                    float trailWidth = isDiving ? 24f : 14f;
                    float diveProgress = isDiving ? MathHelper.Clamp((StateTimer - 20f) / 40f, 0f, 1f) : 0f;
                    float trailOpacity = isDiving ? 0.7f + 0.3f * diveProgress : 0.5f;

                    // Set shared shader params
                    effect.Parameters["uPhase"]?.SetValue(crystalHue);
                    effect.Parameters["uOpacity"]?.SetValue(trailOpacity);
                    effect.Parameters["uIntensity"]?.SetValue(1.2f * stateIntensity);
                    effect.Parameters["uOverbrightMult"]?.SetValue(isDiving ? 0.5f : 0.2f);
                    trailShader.UseImage1(MagnumTextureRegistry.PerlinNoise);

                    // Pass 1: CrystalOrbitGlow @ 3x width (prismatic bloom underlay)
                    effect.CurrentTechnique = effect.Techniques["CrystalOrbitGlow"];
                    var glowSettings = new FlockTrailSettings(
                        t => trailWidth * 3f * (1f - t * 0.7f),
                        t => FlockUtils.GetIridescent(t + crystalHue + Timer * 0.008f) * (0.25f * (1f - t)));
                    glowSettings = new FlockTrailSettings(glowSettings.Width, glowSettings.TrailColor, trailShader);
                    _trailRenderer.RenderTrail(oldPos, glowSettings);

                    // Pass 2: CrystalOrbitMain @ 1x width (sharp crystal facets)
                    effect.CurrentTechnique = effect.Techniques["CrystalOrbitMain"];
                    var mainSettings = new FlockTrailSettings(
                        t => trailWidth * (1f - t * 0.6f),
                        t => Color.Lerp(Color.White, FlockUtils.GetIridescent(t * 0.5f + crystalHue), t * 0.7f) * (0.8f * (1f - t * 0.5f)));
                    mainSettings = new FlockTrailSettings(mainSettings.Width, mainSettings.TrailColor, trailShader);
                    _trailRenderer.RenderTrail(oldPos, mainSettings);

                    // Pass 3: CrystalOrbitGlow @ 1.5x width (overbright halo)
                    effect.CurrentTechnique = effect.Techniques["CrystalOrbitGlow"];
                    effect.Parameters["uOverbrightMult"]?.SetValue(isDiving ? 0.8f : 0.35f);
                    var haloSettings = new FlockTrailSettings(
                        t => trailWidth * 1.5f * (1f - t * 0.8f),
                        t => Color.Lerp(Color.White, FlockUtils.GetIridescent(crystalHue + 0.15f), t) * (0.3f * (1f - t)));
                    haloSettings = new FlockTrailSettings(haloSettings.Width, haloSettings.TrailColor, trailShader);
                    _trailRenderer.RenderTrail(oldPos, haloSettings);

                    // Restart sprite batch after GPU primitives
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }

                // ========================================================
                // LAYER 3: Halo ring (oil-sheen iridescent)
                // ========================================================
                if (haloRing != null)
                {
                    Vector2 hOrigin = haloRing.Size() * 0.5f;
                    Color haloC = FlockUtils.GetOilSheen(idleRotation * 0.7f, Timer * 0.005f);
                    float haloScale = 0.18f * pulse * stateIntensity;
                    sb.Draw(haloRing, drawPos, null, haloC * 0.15f * pulse, -idleRotation * 0.5f, hOrigin, haloScale, SpriteEffects.None, 0f);
                }

                // ========================================================
                // LAYER 6: Star sparkle (per-crystal rotation)
                // ========================================================
                if (star != null)
                {
                    Vector2 sOrigin = star.Size() * 0.5f;
                    float starRot = (float)Main.timeForVisualEffects * 0.035f + CrystalIndex * 2.094f;
                    Color starColor = FlockUtils.GetIridescent(Timer * 0.012f + crystalHue);
                    sb.Draw(star, drawPos, null, starColor * 0.3f * pulse, starRot, sOrigin, 0.18f * stateIntensity, SpriteEffects.None, 0f);
                    // Counter-rotating faint second star
                    sb.Draw(star, drawPos, null, starColor * 0.12f * pulse, -starRot * 0.7f, sOrigin, 0.25f, SpriteEffects.None, 0f);
                }

                // --- Draw crystal sprite ---
                DrawCrystalSpriteAdditive(sb, drawPos, lightColor);
            }
            catch { }
            finally
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            // Theme accents (additive)
            try
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                FlockUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch { }

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

        private void DrawFormationLines(SpriteBatch sb, Vector2 screenPos, Texture2D point)
        {
            if (point == null || CurrentState != CrystalState.FormationFlight) return;

            Vector2 pOrigin = point.Size() * 0.5f;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (!other.active || other.type != Projectile.type || other.whoAmI == Projectile.whoAmI ||
                    other.owner != Projectile.owner) continue;

                float dist = Vector2.Distance(Projectile.Center, other.Center);
                if (dist > 200f || dist < 10f) continue;

                // Draw faint line of dots between crystals
                int dotCount = (int)(dist / 20f);
                for (int d = 1; d < dotCount; d++)
                {
                    float t = d / (float)dotCount;
                    Vector2 dotPos = Vector2.Lerp(Projectile.Center, other.Center, t) - screenPos;
                    Color c = FlockUtils.GetIridescent(t + Timer * 0.003f);
                    sb.Draw(point, dotPos, null, c * 0.08f, 0f, pOrigin, 0.04f, SpriteEffects.None, 0f);
                }
            }
        }

        private void DrawCrystalSpriteAdditive(SpriteBatch sb, Vector2 drawPos, Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            if (tex == null) return;

            // Draw sprite in alpha blend (within the additive region, switch briefly)
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 origin = tex.Size() * 0.5f;
            Color drawColor = Projectile.GetAlpha(lightColor);
            // Iridescent tint on sprite
            Color tint = FlockUtils.GetIridescent(Timer * 0.008f);
            drawColor = Color.Lerp(drawColor, tint, 0.15f);

            sb.Draw(tex, drawPos, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            // Back to additive for caller's remaining draws
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
