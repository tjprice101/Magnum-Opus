using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract.Projectiles
{
    public class WrathDemonMinion : ModProjectile
    {
        private const float HomingRange = 350f;
        private const float HomingStrength = 0.08f;
        private const float MaxSpeed = 16f;
        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private VertexStrip _strip;

        // Blood-Bound Minion state
        private int _fireTimer;
        private int _aliveFrames;
        private int _killCount;
        private int _frenzyTimer;
        private int _breachFireTimer;

        public override string Texture => "MagnumOpus/Content/DiesIrae/Weapons/WrathfulContract/WrathfulContract";

        public override void SetStaticDefaults()
        {
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!CheckActive(player)) return;

            if (!_initialized)
            {
                _initialized = true;
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            // Find and attack enemies
            NPC target = WrathfulContractUtils.ClosestNPCAt(Projectile.Center, HomingRange);
            if (target != null)
            {
                Vector2 desiredDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredDir * MaxSpeed, HomingStrength);
            }
            else
            {
                // Hover near player when no target
                Vector2 targetPos = player.Center + new Vector2(player.direction * -60f, -70f);
                float bobOffset = MathF.Sin((float)Main.timeForVisualEffects * 0.03f) * 8f;
                targetPos.Y += bobOffset;

                Vector2 toTarget = targetPos - Projectile.Center;
                float dist = toTarget.Length();
                if (dist > 800f)
                    Projectile.Center = targetPos;
                else if (dist > 5f)
                {
                    float speed = MathHelper.Clamp(dist * 0.06f, 1f, 12f);
                    Projectile.velocity = toTarget.SafeNormalize(Vector2.Zero) * speed;
                }
                else
                    Projectile.velocity *= 0.9f;
            }

            if (Projectile.velocity.Length() > MaxSpeed)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // ─── Blood-Bound Minion: Fire rate ramping ───
            _aliveFrames = Math.Min(_aliveFrames + 1, 600);
            float rampProgress = _aliveFrames / 600f;
            int baseInterval = (int)MathHelper.Lerp(60f, 30f, rampProgress);

            // Frenzy countdown
            if (_frenzyTimer > 0)
                _frenzyTimer--;

            bool inFrenzy = _frenzyTimer > 0;
            int fireInterval = inFrenzy ? Math.Max(baseInterval / 2, 10) : baseInterval;

            // Update ModPlayer state
            var modPlayer = player.GetModPlayer<WrathfulContractPlayer>();
            modPlayer.HasActiveDemon = true;
            modPlayer.DemonInFrenzy = inFrenzy;

            // ─── Fire homing orbs at target ───
            _fireTimer++;
            if (_fireTimer >= fireInterval && target != null)
            {
                _fireTimer = 0;
                if (Main.myPlayer == Projectile.owner)
                {
                    float orbMode = inFrenzy ? 1f : 0f;
                    Vector2 fireDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    Vector2 vel = fireDir * 12f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel,
                        ModContent.ProjectileType<WrathFireballProjectile>(), Projectile.damage, Projectile.knockBack,
                        Projectile.owner, orbMode);
                }

                // Muzzle flash VFX
                for (int i = 0; i < 4; i++)
                {
                    Vector2 sparkVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * 3f
                        + Main.rand.NextVector2Circular(2f, 2f);
                    Color col = Main.rand.NextBool() ? new Color(255, 180, 50) : new Color(200, 40, 20);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, sparkVel, 0, col, 0.6f);
                    d.noGravity = true;
                }
            }

            // ─── Breach mode: below 10% player HP ───
            bool inBreach = modPlayer.IsBelowBreachThreshold();
            if (inBreach)
            {
                _breachFireTimer++;
                if (_breachFireTimer >= 120 && target != null)
                {
                    _breachFireTimer = 0;
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Vector2 fireDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                        Vector2 vel = fireDir * 10f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel,
                            ModContent.ProjectileType<WrathFireballProjectile>(), Projectile.damage, Projectile.knockBack,
                            Projectile.owner, 2f);
                    }

                    // Breach fire VFX burst
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 sparkVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                        Color col = i % 2 == 0 ? DiesIraePalette.InfernalRed : DiesIraePalette.HellfireGold;
                        Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, sparkVel, 0, col, 0.9f);
                        d.noGravity = true;
                    }
                    try { DiesIraeVFXLibrary.SpawnWrathBurst(Projectile.Center, 4, 0.5f); } catch { }
                }
            }
            else
            {
                _breachFireTimer = 0;
            }

            // ─── Blood Contract: drain 1 HP from owner every 60 frames ───
            if (_aliveFrames % 60 == 0 && player.statLife > 1)
            {
                player.statLife = Math.Max(player.statLife - 1, 1);
                if (Main.myPlayer == Projectile.owner)
                    player.HealEffect(-1, broadcast: false);
            }

            // Trail dust (intensified during frenzy)
            bool spawnTrailDust = inFrenzy ? Main.rand.NextBool(2) : Main.rand.NextBool(3);
            if (spawnTrailDust)
            {
                int dustType = Main.rand.NextBool() ? DustID.Torch : DustID.SolarFlare;
                Color dustColor = Main.rand.NextBool() ? new Color(255, 180, 50) : new Color(200, 40, 20);
                float dustScale = inFrenzy ? 1.1f : 0.8f;
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dustColor, dustScale);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            float pulse = 1f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.2f);
            float lightIntensity = inFrenzy ? 0.55f : 0.35f;
            Lighting.AddLight(Projectile.Center, new Vector3(0.6f, 0.2f, 0.1f) * lightIntensity * pulse);
        }

        private bool CheckActive(Player player)
        {
            if (player.dead || !player.active)
            {
                player.ClearBuff(ModContent.BuffType<Buffs.WrathfulContractBuff>());
                Projectile.Kill();
                return false;
            }
            if (player.HasBuff(ModContent.BuffType<Buffs.WrathfulContractBuff>()))
                Projectile.timeLeft = 2;
            return true;
        }

        /// <summary>
        /// Called by this minion and its fireballs when they kill an enemy.
        /// Every 3 kills triggers Frenzy mode for 5 seconds.
        /// </summary>
        public void RegisterKill()
        {
            _killCount++;
            if (_killCount >= 3)
            {
                _frenzyTimer = 300; // 5 seconds of Frenzy
                _killCount = 0;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);

            // Track kills from contact damage
            if (target.life <= 0)
                RegisterKill();

            Vector2 hitPos = target.Center;
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                Color col = i % 2 == 0 ? new Color(200, 40, 20) : new Color(255, 180, 50);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.Torch, sparkVel, 0, col, 0.5f);
                d.noGravity = true;
            }
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                Dust d = Dust.NewDustPerfect(hitPos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.SolarFlare, vel, 0, new Color(200, 40, 20), 0.5f);
                d.noGravity = true;
            }
            try { DiesIraeVFXLibrary.SpawnMusicNotes(hitPos, 1, 12f, 0.4f, 0.7f, 20); } catch { }
            try { DiesIraeVFXLibrary.SpawnMixedSparkleImpact(hitPos, 0.6f, 4, 4); } catch { }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.DiesIrae, ref _strip);
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

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                Color col = Main.rand.NextBool() ? new Color(200, 40, 20) : new Color(255, 180, 50);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, sparkVel, 0, col, 0.3f);
                d.noGravity = true;
            }
            try { DiesIraeVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 12f, 0.5f, 0.7f, 20); } catch { }
            try { DiesIraeVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 0.5f, 4, 4); } catch { }
            try { DiesIraeVFXLibrary.SpawnInfernalSparkles(Projectile.Center, 3, 15f); } catch { }
        }
    }

    /// <summary>
    /// Homing fireball with ember trail. Homes toward nearest enemy.
    /// </summary>
    public class WrathFireballProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/DiesIrae/Weapons/WrathfulContract/WrathfulContract";

        private VertexStrip _strip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // ─── Mode initialization (first frame) ───
            int mode = (int)Projectile.ai[0]; // 0=normal, 1=frenzy, 2=breach
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                if (mode == 1) // Frenzy: pierce 1 (can hit 2 targets)
                    Projectile.penetrate = 2;
                if (mode == 2) // Breach: 2x scale, expanded hitbox
                {
                    Projectile.scale = 2f;
                    int oldW = Projectile.width;
                    int oldH = Projectile.height;
                    Projectile.width = 32;
                    Projectile.height = 32;
                    Projectile.position -= new Vector2((32 - oldW) / 2f, (32 - oldH) / 2f);
                }
            }

            // ─── Homing (strength varies by mode) ───
            float homingStrength = mode == 2 ? 0.14f : 0.08f;
            NPC bestTarget = WrathfulContractUtils.ClosestNPCAt(Projectile.Center, 400f);
            if (bestTarget != null)
            {
                Vector2 dir = (bestTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                float speed = Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.UnitY), dir, homingStrength) * speed;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail dust (more intense for breach orbs)
            bool spawnDust = mode == 2 ? Main.rand.NextBool(2) : Main.rand.NextBool(3);
            if (spawnDust)
            {
                int dustType = Main.rand.NextBool() ? DustID.Torch : DustID.SolarFlare;
                Color dustColor = Main.rand.NextBool() ? new Color(255, 180, 50) : new Color(200, 40, 20);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dustColor, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            float pulse = 1f + 0.15f * (float)Math.Sin(Projectile.timeLeft * 0.2f);
            Lighting.AddLight(Projectile.Center, new Vector3(0.6f, 0.2f, 0.1f) * 0.35f * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 120);

            // Blood Contract: heal owner for 5% of damage dealt
            Player owner = Main.player[Projectile.owner];
            int healAmount = Math.Max((int)(damageDone * 0.05f), 1);
            owner.statLife = Math.Min(owner.statLife + healAmount, owner.statLifeMax2);
            if (Main.myPlayer == Projectile.owner)
                owner.HealEffect(healAmount, broadcast: false);

            // Report kill to parent demon minion for frenzy tracking
            if (target.life <= 0)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active && p.owner == Projectile.owner && p.type == ModContent.ProjectileType<WrathDemonMinion>())
                    {
                        (p.ModProjectile as WrathDemonMinion)?.RegisterKill();
                        break;
                    }
                }
            }

            Vector2 hitPos = target.Center;
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                Color col = i % 2 == 0 ? new Color(200, 40, 20) : new Color(255, 180, 50);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.Torch, sparkVel, 0, col, 0.5f);
                d.noGravity = true;
            }
            try { DiesIraeVFXLibrary.SpawnMusicNotes(hitPos, 1, 12f, 0.4f, 0.7f, 20); } catch { }
            try { DiesIraeVFXLibrary.SpawnMixedSparkleImpact(hitPos, 0.6f, 4, 4); } catch { }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.DiesIrae, ref _strip);
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

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                Color col = Main.rand.NextBool() ? new Color(200, 40, 20) : new Color(255, 180, 50);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, sparkVel, 0, col, 0.3f);
                d.noGravity = true;
            }
            try { DiesIraeVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 12f, 0.5f, 0.7f, 20); } catch { }
            try { DiesIraeVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 0.5f, 4, 4); } catch { }
            try { DiesIraeVFXLibrary.SpawnInfernalSparkles(Projectile.Center, 3, 15f); } catch { }
        }
    }
}
