using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Utilities;
using MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Shaders;
using MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Particles;
using MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Primitives;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Common.Systems.VFX;
using ReLogic.Content;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Projectiles
{
    /// <summary>
    /// Iridescent Crystal — orbiting minion crystal for Feather of the Iridescent Flock.
    /// 
    /// BEHAVIOR:
    /// • Orbits the player at a set radius and angle speed
    /// • Uses 0.34 minion slots
    /// • Periodically locks onto closest enemy and dashes to strike
    /// • After striking, returns to orbit with oil-sheen trail
    /// • When 3+ crystals exist, participates in Flock Formation:
    ///   — iridescent lines connect crystals
    ///   — formation fires a beam every 60 ticks at targeted enemy
    /// </summary>
    public class IridescentCrystalProj : ModProjectile
    {
        private enum CrystalState { Orbiting, Attacking, Returning }

        private const int TrailLength = 16;
        private Vector2[] _trail = new Vector2[TrailLength];
        private FlockPrimitiveRenderer _renderer;
        private CrystalState _state = CrystalState.Orbiting;
        private int _attackTimer;
        private int _cooldown;
        private NPC _attackTarget;

        // ai[0] = orbit angle offset assigned at spawn
        private float OrbitOffset => Projectile.ai[0];
        private float _orbitAngle;
        private float _orbitRadius = 80f;

        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/FeatheroftheIridescentFlock/FeatheroftheIridescentFlock";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            Main.projPet[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.minion = true;
            Projectile.minionSlots = 0.34f;
            Projectile.timeLeft = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => _state == CrystalState.Attacking;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // Validate buff
            if (!owner.active || owner.dead) { Projectile.Kill(); return; }
            if (owner.HasBuff(ModContent.BuffType<IridescentFlockBuff>()))
                Projectile.timeLeft = 2;

            // Update trail
            for (int i = TrailLength - 1; i > 0; i--) _trail[i] = _trail[i - 1];
            _trail[0] = Projectile.Center;

            _cooldown = Math.Max(0, _cooldown - 1);

            switch (_state)
            {
                case CrystalState.Orbiting:
                    AIOrbit(owner);
                    break;
                case CrystalState.Attacking:
                    AIAttack(owner);
                    break;
                case CrystalState.Returning:
                    AIReturn(owner);
                    break;
            }

            // Formation visuals when 3+ crystals
            if (owner.ownedProjectileCounts[Type] >= 3 && _state == CrystalState.Orbiting)
            {
                if (Main.rand.NextBool(12))
                {
                    var shimmer = new OilShimmerParticle();
                    shimmer.Initialize(
                        Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                        Main.rand.NextVector2Circular(0.5f, 0.5f),
                        FlockUtils.GetIridescent(Main.rand.NextFloat()),
                        Main.rand.NextFloat(0.4f, 0.8f)
                    );
                    FlockParticleHandler.Spawn(shimmer);
                }
            }

            // Iridescent light
            Color lightCol = FlockUtils.GetOilSheen(_orbitAngle, Main.GameUpdateCount);
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * 0.3f);
        }

        private void AIOrbit(Player owner)
        {
            _orbitAngle += 0.03f;
            float angle = _orbitAngle + OrbitOffset;
            Vector2 target = owner.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * _orbitRadius;

            Projectile.velocity = (target - Projectile.Center) * 0.15f;
            Projectile.Center += Projectile.velocity;
            Projectile.rotation = angle + MathHelper.PiOver2;

            // Look for attack target
            if (_cooldown <= 0)
            {
                NPC closest = FindClosestNPC(400f);
                if (closest != null)
                {
                    _attackTarget = closest;
                    _state = CrystalState.Attacking;
                    _attackTimer = 0;
                }
            }
        }

        private void AIAttack(Player owner)
        {
            _attackTimer++;

            if (_attackTarget == null || !_attackTarget.active || _attackTarget.dontTakeDamage)
            {
                _state = CrystalState.Returning;
                _cooldown = 45;
                return;
            }

            // Dash toward target
            Vector2 toTarget = (_attackTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            float dashSpeed = 18f;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * dashSpeed, 0.2f);
            Projectile.Center += Projectile.velocity;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Attack particles
            if (Main.rand.NextBool(2))
            {
                var shard = new CrystalShardParticle();
                shard.Initialize(
                    Projectile.Center,
                    -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f),
                    FlockUtils.GetIridescent(Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.5f, 0.9f)
                );
                FlockParticleHandler.Spawn(shard);
            }

            // Timeout return
            if (_attackTimer > 60)
            {
                _state = CrystalState.Returning;
                _cooldown = 30;
            }
        }

        private void AIReturn(Player owner)
        {
            float angle = _orbitAngle + OrbitOffset;
            Vector2 orbitPos = owner.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * _orbitRadius;

            Projectile.velocity = (orbitPos - Projectile.Center) * 0.12f;
            Projectile.Center += Projectile.velocity;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Vector2.Distance(Projectile.Center, orbitPos) < 15f)
            {
                _state = CrystalState.Orbiting;
                _cooldown = 60;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 180); // 3 seconds
            _state = CrystalState.Returning;
            _cooldown = 45;

            // Impact particles
            for (int i = 0; i < 5; i++)
            {
                var shard = new CrystalShardParticle();
                shard.Initialize(
                    target.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2Circular(4f, 4f),
                    FlockUtils.GetIridescent(Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.5f, 1.0f)
                );
                FlockParticleHandler.Spawn(shard);
            }

            // Feather burst
            for (int i = 0; i < 3; i++)
            {
                var feather = new IridescentFeatherParticle();
                feather.Initialize(
                    target.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -0.5f),
                    FlockUtils.GetOilSheen(Main.rand.NextFloat(MathHelper.TwoPi), Main.GameUpdateCount),
                    Main.rand.NextFloat(0.6f, 1.0f)
                );
                FlockParticleHandler.Spawn(feather);
            }

            // Music notes on crystal strike — flock's iridescent chime
            SwanLakeVFXLibrary.SpawnMusicNotes(target.Center, 2, 12f, 0.5f, 0.8f, 22);

            // Prismatic sparkle accents
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(target.Center, 3, 10f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // === Shader trail ===
            DrawOilSheenTrail(sb);

            // === Formation lines ===
            DrawFormationLines(sb);

            // === Crystal bloom ===
            DrawCrystalBloom(sb);

            // === Sprite ===
            DrawCrystalSprite(sb, lightColor);

            // === Particles ===
            FlockParticleHandler.DrawAllParticles(sb);

            return false;
        }

        private void DrawOilSheenTrail(SpriteBatch sb)
        {
            if (_state == CrystalState.Orbiting) return;

            var valid = new List<Vector2>();
            foreach (var p in _trail) if (p != Vector2.Zero) valid.Add(p);
            if (valid.Count < 3) return;

            _renderer ??= new FlockPrimitiveRenderer();

            var settings = new FlockTrailSettings(
                t => MathHelper.Lerp(10f, 2f, t),
                t => {
                    Color sheen = FlockUtils.GetOilSheen(t * MathHelper.TwoPi, Main.GameUpdateCount);
                    return sheen * (1f - t * 0.6f);
                },
                FlockShaderLoader.HasCrystalOrbitTrailShader ? GameShaders.Misc["MagnumOpus:CrystalOrbitTrail"] : null
            );

            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            _renderer.RenderTrail(valid.ToArray(), settings, 14);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawFormationLines(SpriteBatch sb)
        {
            Player owner = Main.player[Projectile.owner];
            if (owner.ownedProjectileCounts[Type] < 3) return;

            // Find other crystals and draw iridescent lines between adjacent ones
            var crystals = new List<Vector2>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Projectile.owner && p.type == Type)
                    crystals.Add(p.Center);
            }

            if (crystals.Count < 3) return;

            Texture2D pixel = MagnumTextureRegistry.GetPointBloom();
            if (pixel == null) return;
            float alpha = 0.25f + (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < crystals.Count; i++)
            {
                int j = (i + 1) % crystals.Count;
                Vector2 start = crystals[i] - Main.screenPosition;
                Vector2 end = crystals[j] - Main.screenPosition;
                Vector2 dir = end - start;
                float dist = dir.Length();
                if (dist < 1f) continue;
                float rot = dir.ToRotation();
                Color lineCol = FlockUtils.GetIridescent((float)i / crystals.Count +
                    (float)Main.GameUpdateCount * 0.01f);

                sb.Draw(pixel, start, new Rectangle(0, 0, 1, 1), lineCol * alpha,
                    rot, Vector2.Zero, new Vector2(dist, 2f), SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawCrystalBloom(SpriteBatch sb)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f + OrbitOffset) * 0.1f + 0.9f;
            Color col = FlockUtils.GetOilSheen(_orbitAngle, Main.GameUpdateCount);

            // Load VFX Asset Library bloom textures
            Texture2D softRadial = null;
            Texture2D pointBloom = null;
            Texture2D starAccent = null;
            try
            {
                softRadial = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom",
                    AssetRequestMode.ImmediateLoad)?.Value;
                pointBloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom",
                    AssetRequestMode.ImmediateLoad)?.Value;
                starAccent = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft",
                    AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Outer iridescent halo (SoftRadialBloom)
            if (softRadial != null)
            {
                Vector2 srOrigin = new Vector2(softRadial.Width / 2f, softRadial.Height / 2f);
                sb.Draw(softRadial, drawPos, null, new Color(col.R, col.G, col.B, 0) * 0.25f * pulse,
                    0f, srOrigin, 0.35f, SpriteEffects.None, 0f);
            }

            // Layer 2: Core white spark (PointBloom)
            if (pointBloom != null)
            {
                Vector2 pbOrigin = new Vector2(pointBloom.Width / 2f, pointBloom.Height / 2f);
                sb.Draw(pointBloom, drawPos, null, new Color(255, 255, 255, 0) * 0.40f * pulse,
                    0f, pbOrigin, 0.10f, SpriteEffects.None, 0f);
            }

            // Layer 3: Rotating star accent — prismatic shimmer
            if (starAccent != null)
            {
                Vector2 starOrigin = new Vector2(starAccent.Width / 2f, starAccent.Height / 2f);
                Color prismatic = FlockUtils.GetIridescent(_orbitAngle + Main.GameUpdateCount * 0.02f);
                sb.Draw(starAccent, drawPos, null, new Color(prismatic.R, prismatic.G, prismatic.B, 0) * 0.30f * pulse,
                    _orbitAngle * 2f, starOrigin, 0.15f, SpriteEffects.None, 0f);
            }

            // Layer 4: Attacking state — intensified bloom
            if (_state == CrystalState.Attacking && softRadial != null)
            {
                Vector2 srOrigin = new Vector2(softRadial.Width / 2f, softRadial.Height / 2f);
                Color attackGlow = FlockUtils.GetIridescent(Main.GameUpdateCount * 0.03f);
                sb.Draw(softRadial, drawPos, null, new Color(attackGlow.R, attackGlow.G, attackGlow.B, 0) * 0.35f,
                    0f, srOrigin, 0.5f, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawCrystalSprite(SpriteBatch sb, Color lightColor)
        {
            // Draw as a small glowing diamond shape
            Texture2D tex = MagnumTextureRegistry.GetRadialBloom();
            if (tex == null) return;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            Color body = FlockUtils.GetOilSheen(_orbitAngle, Main.GameUpdateCount);

            sb.Draw(tex, drawPos, null, body, Projectile.rotation, origin,
                new Vector2(0.1f, 0.18f), SpriteEffects.None, 0f);

            // Additive bloom overlay
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
                sb.Draw(glow, drawPos, null, body * 0.4f, Projectile.rotation,
                    glow.Size() * 0.5f, new Vector2(0.15f, 0.27f), SpriteEffects.None, 0f);
        }

        private NPC FindClosestNPC(float maxRange)
        {
            NPC closest = null;
            float best = maxRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.active || n.friendly || n.dontTakeDamage) continue;
                float d = Vector2.Distance(Projectile.Center, n.Center);
                if (d < best) { best = d; closest = n; }
            }
            return closest;
        }
    }
}
