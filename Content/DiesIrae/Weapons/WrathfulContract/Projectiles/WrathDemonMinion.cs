using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract.Utilities;
using MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract.Particles;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract.Projectiles
{
    /// <summary>
    /// Wrath Demon Minion — Orbits the player, dashes at enemies to deal contact damage.
    /// Every 3rd dash combo spawns 6 wrath fireballs in a radial burst.
    /// 2 minion slots. Heavy demon energy VFX.
    /// </summary>
    public class WrathDemonMinion : ModProjectile
    {
        private static Asset<Texture2D> bloomTexture;
        private int comboCounter = 0;
        private int attackTimer = 0;
        private int dashCooldown = 0;
        private bool isDashing = false;
        private int dashTimer = 0;
        private NPC dashTarget = null;

        private enum MinionState { Orbiting, Dashing, Returning }
        private MinionState state = MinionState.Orbiting;
        private float orbitAngle = 0f;

        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minion = true;
            Projectile.minionSlots = 2f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // Buff check
            if (owner.dead || !owner.active) { owner.ClearBuff(ModContent.BuffType<Buffs.WrathfulContractBuff>()); Projectile.Kill(); return; }
            if (owner.HasBuff(ModContent.BuffType<Buffs.WrathfulContractBuff>())) Projectile.timeLeft = 2;

            attackTimer++;
            if (dashCooldown > 0) dashCooldown--;

            switch (state)
            {
                case MinionState.Orbiting:
                    OrbitBehavior(owner);
                    break;
                case MinionState.Dashing:
                    DashBehavior();
                    break;
                case MinionState.Returning:
                    ReturnBehavior(owner);
                    break;
            }

            // Ambient demon embers
            if (Main.rand.NextBool(5))
            {
                Vector2 vel = Main.rand.NextVector2Circular(1f, 1f);
                ContractParticleHandler.Spawn(new DashTrailParticle(Projectile.Center, vel,
                    ContractUtils.GetContractColor(Main.rand.NextFloat(0.2f, 0.6f)), 0.12f, 10));
            }

            Projectile.rotation += 0.05f;
            Lighting.AddLight(Projectile.Center, ContractUtils.WrathFlame.ToVector3() * 0.4f);
        }

        private void OrbitBehavior(Player owner)
        {
            orbitAngle += 0.04f;
            Vector2 targetPos = owner.Center + orbitAngle.ToRotationVector2() * 100f;
            Projectile.velocity = (targetPos - Projectile.Center) * 0.1f;

            // Look for targets
            if (dashCooldown <= 0 && attackTimer % 10 == 0)
            {
                NPC target = null;
                float closest = 500f * 500f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                    float d = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                    if (d < closest) { closest = d; target = npc; }
                }

                if (target != null)
                {
                    dashTarget = target;
                    state = MinionState.Dashing;
                    dashTimer = 0;
                    Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = dir * 18f;

                    // Dash initiation VFX
                    ContractParticleHandler.Spawn(new DemonAuraParticle(Projectile.Center, ContractUtils.WrathFlame, 0.6f, 10));
                    SoundEngine.PlaySound(SoundID.DD2_FlameburstTowerShot with { Volume = 0.5f }, Projectile.Center);
                }
            }
        }

        private void DashBehavior()
        {
            dashTimer++;

            // Home slightly during dash
            if (dashTarget != null && dashTarget.active)
            {
                Vector2 dir = (dashTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * 18f, 0.03f);
            }

            // Trail during dash
            ContractParticleHandler.Spawn(new DashTrailParticle(
                Projectile.Center, -Projectile.velocity * 0.1f,
                ContractUtils.GetContractColor(Main.rand.NextFloat(0.3f, 0.7f)), 0.2f, 10));

            if (dashTimer >= 25 || (dashTarget != null && Vector2.Distance(Projectile.Center, dashTarget.Center) < 30f))
            {
                comboCounter++;
                dashCooldown = 30;
                state = MinionState.Returning;

                // Every 3rd combo: fireball burst
                if (comboCounter % 3 == 0 && Main.myPlayer == Projectile.owner)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f;
                        Vector2 vel = angle.ToRotationVector2() * 8f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel,
                            ModContent.ProjectileType<WrathFireballProjectile>(), Projectile.damage / 2, 3f, Projectile.owner);
                    }

                    ContractParticleHandler.Spawn(new DemonAuraParticle(Projectile.Center, ContractUtils.ContractGold, 1.2f, 15));
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                        ContractParticleHandler.Spawn(new DashTrailParticle(Projectile.Center, vel,
                            ContractUtils.GetContractColor(Main.rand.NextFloat()), 0.2f, 12));
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                        ContractParticleHandler.Spawn(new ContractNoteParticle(Projectile.Center, vel,
                            ContractUtils.WrathFlame, 0.4f, 35));
                    }
                }
            }
        }

        private void ReturnBehavior(Player owner)
        {
            Vector2 returnPos = owner.Center + orbitAngle.ToRotationVector2() * 100f;
            Projectile.velocity = (returnPos - Projectile.Center) * 0.12f;

            if (Vector2.Distance(Projectile.Center, returnPos) < 30f)
            {
                state = MinionState.Orbiting;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
            ContractParticleHandler.Spawn(new DemonAuraParticle(target.Center, ContractUtils.DemonCrimson, 0.5f, 8));
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                ContractParticleHandler.Spawn(new DashTrailParticle(target.Center, vel,
                    ContractUtils.GetContractColor(Main.rand.NextFloat()), 0.15f, 10));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            bloomTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!bloomTexture.IsLoaded) return false;
            var tex = bloomTexture.Value;

            float pulse = 0.8f + 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.15f);
            float dashIntensity = state == MinionState.Dashing ? 1.3f : 1f;

            // Demon aura
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                ContractUtils.Additive(ContractUtils.DemonCrimson, 0.2f * pulse * dashIntensity), 0f,
                tex.Size() / 2f, 1f * dashIntensity, SpriteEffects.None, 0);
            // Core flame
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                ContractUtils.Additive(ContractUtils.WrathFlame, 0.5f * pulse * dashIntensity), 0f,
                tex.Size() / 2f, 0.5f, SpriteEffects.None, 0);
            // Hot center
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                ContractUtils.Additive(ContractUtils.HellcoreWhite, 0.3f * pulse), 0f,
                tex.Size() / 2f, 0.2f, SpriteEffects.None, 0);

            return false;
        }
    }

    /// <summary>
    /// Wrath Fireball — Homing fireball spawned by demon minion on every 3rd combo.
    /// Homes to enemies, explodes on contact.
    /// </summary>
    public class WrathFireballProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> bloomTexture;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation += 0.12f;

            // Homing
            NPC target = null;
            float closest = 400f * 400f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float d = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                if (d < closest) { closest = d; target = npc; }
            }

            if (target != null)
            {
                Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * 12f, 0.06f);
            }

            // Trail
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = -Projectile.velocity * 0.04f;
                ContractParticleHandler.Spawn(new FireballGlowParticle(Projectile.Center, vel,
                    ContractUtils.GetContractColor(Main.rand.NextFloat(0.3f, 0.7f)), 0.15f, 10));
            }

            Lighting.AddLight(Projectile.Center, ContractUtils.WrathFlame.ToVector3() * 0.25f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 120);
            ContractParticleHandler.Spawn(new DemonAuraParticle(target.Center, ContractUtils.WrathFlame, 0.5f, 8));
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                ContractParticleHandler.Spawn(new DashTrailParticle(target.Center, vel,
                    ContractUtils.ContractGold, 0.15f, 10));
            }
        }

        public override void OnKill(int timeLeft)
        {
            ContractParticleHandler.Spawn(new DemonAuraParticle(Projectile.Center, ContractUtils.DemonCrimson, 0.4f, 8));
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                ContractParticleHandler.Spawn(new ContractSmokeParticle(Projectile.Center, vel, 0.3f, 20));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            bloomTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!bloomTexture.IsLoaded) return false;
            var tex = bloomTexture.Value;

            float pulse = 0.8f + 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.25f);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                ContractUtils.Additive(ContractUtils.WrathFlame, 0.4f * pulse), 0f, tex.Size() / 2f, 0.4f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                ContractUtils.Additive(ContractUtils.HellcoreWhite, 0.2f * pulse), 0f, tex.Size() / 2f, 0.15f, SpriteEffects.None, 0);

            return false;
        }
    }
}
