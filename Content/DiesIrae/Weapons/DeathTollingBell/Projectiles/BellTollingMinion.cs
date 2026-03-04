using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Buffs;
using MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Projectiles
{
    /// <summary>
    /// Death Tolling Bell minion — spectral bell that hovers near the player.
    /// Tolls every 2 seconds, releasing expanding shockwave rings.
    /// Every 10th toll triggers Funeral March — enhanced 6-ring toll.
    /// ai[0] = state (0=idle, 1=charging, 2=toll, 3=funeral march flash)
    /// ai[1] = general timer
    /// localAI[0] = toll counter (for Funeral March tracking)
    /// localAI[1] = state sub-timer
    /// </summary>
    public class BellTollingMinion : ModProjectile
    {
        private const int TollCycleFrames = 120; // 2 seconds between tolls
        private const int ChargeFrames = 30;     // 0.5s charge-up
        private const int TollFrames = 15;       // Toll moment
        private const int FuneralMarchInterval = 10; // Every 10th toll

        private int State { get => (int)Projectile.ai[0]; set => Projectile.ai[0] = value; }
        private float Timer { get => Projectile.ai[1]; set => Projectile.ai[1] = value; }
        private float TollCount { get => Projectile.localAI[0]; set => Projectile.localAI[0] = value; }
        private float SubTimer { get => Projectile.localAI[1]; set => Projectile.localAI[1] = value; }

        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // Standard minion buff check
            if (!player.active || player.dead)
            {
                player.ClearBuff(ModContent.BuffType<DeathTollingBellBuff>());
                Projectile.Kill();
                return;
            }
            if (player.HasBuff(ModContent.BuffType<DeathTollingBellBuff>()))
                Projectile.timeLeft = 2;

            Timer++;

            // Hover near player — float above and slightly behind
            Vector2 targetPos = player.Center + new Vector2(player.direction * -40f, -80f);
            float bobOffset = (float)Math.Sin(Timer * 0.04f) * 6f;
            targetPos.Y += bobOffset;

            Vector2 toTarget = targetPos - Projectile.Center;
            float dist = toTarget.Length();
            if (dist > 800f)
            {
                // Teleport if too far
                Projectile.Center = targetPos;
            }
            else if (dist > 4f)
            {
                float speed = MathHelper.Clamp(dist * 0.08f, 1f, 16f);
                Projectile.velocity = toTarget.SafeNormalize(Vector2.Zero) * speed;
            }
            else
            {
                Projectile.velocity *= 0.9f;
            }

            Projectile.Center += Projectile.velocity;
            Projectile.velocity *= 0.85f;

            // State machine for toll cycle
            SubTimer++;

            switch (State)
            {
                case 0: // Idle — wait for next toll cycle
                    if (SubTimer >= TollCycleFrames - ChargeFrames)
                    {
                        State = 1; // Begin charging
                        SubTimer = 0;
                    }
                    // Ambient dust — subtle crimson motes
                    if (Main.rand.NextBool(8) && !Main.dedServ)
                    {
                        Dust d = Dust.NewDustPerfect(
                            Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                            DustID.Torch, Vector2.UnitY * -0.5f, 0, default, 0.6f);
                        d.noGravity = true;
                        d.fadeIn = 0.3f;
                    }
                    // Ambient ash flake drift
                    if (Main.rand.NextBool(20) && !Main.dedServ)
                    {
                        Vector2 ashVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1f, -0.3f));
                        BellParticleHandler.Spawn(new AshFlakeParticle(
                            Projectile.Center + Main.rand.NextVector2Circular(25f, 25f),
                            ashVel, 0.2f + Main.rand.NextFloat() * 0.1f, 60 + Main.rand.Next(40)));
                    }
                    break;

                case 1: // Charging — bell glows brighter
                    if (SubTimer >= ChargeFrames)
                    {
                        TollCount++;
                        bool isFuneralMarch = TollCount % FuneralMarchInterval == 0;

                        if (isFuneralMarch)
                        {
                            State = 3; // Funeral March flash
                        }
                        else
                        {
                            State = 2; // Normal toll
                        }
                        SubTimer = 0;
                        FireTollWaves(isFuneralMarch);
                    }
                    // Charge-up dust — intensifying embers
                    if (!Main.dedServ)
                    {
                        float chargeProgress = SubTimer / (float)ChargeFrames;
                        if (Main.rand.NextFloat() < chargeProgress * 0.5f)
                        {
                            Vector2 vel = Main.rand.NextVector2Circular(1f, 1f);
                            Dust d = Dust.NewDustPerfect(
                                Projectile.Center + Main.rand.NextVector2Circular(16f, 16f),
                                DustID.GoldFlame, vel, 0, default, 0.8f + chargeProgress * 0.5f);
                            d.noGravity = true;
                        }
                        // Converging ember particles during charge
                        if (Main.rand.NextFloat() < chargeProgress * 0.3f)
                        {
                            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                            float chargeDist = 30f + Main.rand.NextFloat() * 20f;
                            Vector2 spawnPos = Projectile.Center + angle.ToRotationVector2() * chargeDist;
                            Vector2 convergeVel = (Projectile.Center - spawnPos).SafeNormalize(Vector2.Zero) * (1.5f + chargeProgress * 2f);
                            Color col = Color.Lerp(DiesIraePalette.EmberOrange, DiesIraePalette.JudgmentGold, chargeProgress);
                            BellParticleHandler.Spawn(new TollEmberParticle(
                                spawnPos, convergeVel, col, 0.1f + chargeProgress * 0.08f, 15 + Main.rand.Next(10)));
                        }
                    }
                    break;

                case 2: // Toll moment — brief flash
                    if (SubTimer >= TollFrames)
                    {
                        State = 0;
                        SubTimer = 0;
                    }
                    break;

                case 3: // Funeral March flash — extended dramatic flash
                    if (SubTimer >= TollFrames * 2)
                    {
                        State = 0;
                        SubTimer = 0;
                    }
                    // Funeral March smoke burst on first frame
                    if (SubTimer == 1)
                    {
                        DeathTollingBellUtils.DoFuneralMarch(Projectile.Center);
                        SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact, Projectile.Center);
                    }
                    break;
            }
        }

        /// <summary>
        /// Fire toll wave projectiles — 3 concentric rings (or 6 for Funeral March).
        /// </summary>
        private void FireTollWaves(bool isFuneralMarch)
        {
            if (Main.myPlayer != Projectile.owner) return;

            int ringCount = isFuneralMarch ? 6 : 3;
            float damageMultiplier = isFuneralMarch ? 1.5f : 1f;

            for (int ring = 0; ring < ringCount; ring++)
            {
                // Stagger ring spawn with slight delay via velocity encoding
                float ringDelay = ring * 0.15f; // Encoded in ai[1] of the wave

                int projType = ModContent.ProjectileType<BellTollWaveProjectile>();
                int damage = (int)(Projectile.damage * damageMultiplier);

                var source = Projectile.GetSource_FromAI();
                int proj = Projectile.NewProjectile(source, Projectile.Center, Vector2.Zero,
                    projType, damage, Projectile.knockBack, Projectile.owner,
                    isFuneralMarch ? 1f : 0f, ringDelay);
            }

            // Toll sound
            SoundEngine.PlaySound(SoundID.Item29, Projectile.Center);
            DeathTollingBellUtils.DoTollWaveDust(Projectile.Center, 30f, isFuneralMarch);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // Load bell texture for sprite rendering
            Texture2D bellTex = null;
            try
            {
                bellTex = ModContent.Request<Texture2D>(Texture, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            }
            catch { }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            // Draw harmonic standing wave tether to player
            Player player = Main.player[Projectile.owner];
            DeathTollingBellUtils.DrawHarmonicTether(sb, Projectile.Center, player.Center, Timer);

            // Draw bell body with state-driven layered glow + bell sprite
            DeathTollingBellUtils.DrawBellBody(sb, Projectile.Center, State, Timer, bellTex);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => false;
    }
}