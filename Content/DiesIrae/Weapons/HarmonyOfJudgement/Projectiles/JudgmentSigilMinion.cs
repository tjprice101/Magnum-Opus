using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgement.Utilities;
using MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgement.Particles;

namespace MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgement.Projectiles
{
    /// <summary>
    /// Judgment Sigil Minion — A floating arcane sigil that fires piercing judgment rays at enemies.
    /// Hovers above the player's head, rotates slowly, fires every 18 ticks.
    /// Multi-layered orbital glow rendering.
    /// </summary>
    public class JudgmentSigilMinion : ModProjectile
    {
        private static Asset<Texture2D> bloomTexture;
        private static Asset<Texture2D> circleTexture;
        private int attackTimer = 0;

        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
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
        public override bool MinionContactDamage() => false;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // Buff check
            if (owner.dead || !owner.active) { owner.ClearBuff(ModContent.BuffType<Buffs.HarmonyOfJudgementBuff>()); Projectile.Kill(); return; }
            if (owner.HasBuff(ModContent.BuffType<Buffs.HarmonyOfJudgementBuff>())) Projectile.timeLeft = 2;

            // Hover above player
            Vector2 targetPos = owner.Center + new Vector2(0f, -80f);
            float dist = Vector2.Distance(Projectile.Center, targetPos);
            if (dist > 500f)
                Projectile.Center = targetPos;
            else
                Projectile.velocity = (targetPos - Projectile.Center) * 0.08f;

            Projectile.rotation += 0.02f;
            attackTimer++;

            // Find target and fire
            if (attackTimer >= 18)
            {
                NPC target = null;
                float closest = 600f * 600f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                    float d = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                    if (d < closest) { closest = d; target = npc; }
                }

                if (target != null && Main.myPlayer == Projectile.owner)
                {
                    Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * 16f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, dir,
                        ModContent.ProjectileType<JudgmentRayProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    attackTimer = 0;

                    // Fire flash
                    HarmonyParticleHandler.Spawn(new SigilGlowParticle(Projectile.Center, HarmonyUtils.SigilGold, 0.6f, 8));
                }
            }

            // Ambient glow particles
            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(0.3f, 0.8f);
                HarmonyParticleHandler.Spawn(new HarmonyEmberParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), vel,
                    HarmonyUtils.GetHarmonyColor(Main.rand.NextFloat(0.3f, 0.7f)), 0.06f, 12));
            }

            Lighting.AddLight(Projectile.Center, HarmonyUtils.SigilGold.ToVector3() * 0.3f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            bloomTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            circleTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/HardCircleMask");
            if (!bloomTexture.IsLoaded) return false;
            var bloom = bloomTexture.Value;

            float pulse = 0.8f + 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.1f);

            // Outer aura
            Main.EntitySpriteDraw(bloom, Projectile.Center - Main.screenPosition, null,
                HarmonyUtils.Additive(HarmonyUtils.HarmonyRed, 0.15f * pulse), 0f, bloom.Size() / 2f, 1.2f, SpriteEffects.None, 0);
            // Inner glow
            Main.EntitySpriteDraw(bloom, Projectile.Center - Main.screenPosition, null,
                HarmonyUtils.Additive(HarmonyUtils.SigilGold, 0.4f * pulse), 0f, bloom.Size() / 2f, 0.5f, SpriteEffects.None, 0);
            // Core
            Main.EntitySpriteDraw(bloom, Projectile.Center - Main.screenPosition, null,
                HarmonyUtils.Additive(HarmonyUtils.RayWhite, 0.3f * pulse), 0f, bloom.Size() / 2f, 0.2f, SpriteEffects.None, 0);

            // Rotating ring (using circle mask if available)
            if (circleTexture.IsLoaded)
            {
                var circle = circleTexture.Value;
                Main.EntitySpriteDraw(circle, Projectile.Center - Main.screenPosition, null,
                    HarmonyUtils.Additive(HarmonyUtils.JudgmentEmber, 0.2f * pulse), Projectile.rotation,
                    circle.Size() / 2f, 0.15f, SpriteEffects.None, 0);
            }

            return false;
        }
    }

    /// <summary>
    /// Judgment Ray — A piercing beam of judgment fire.
    /// Penetrates 2 enemies, leaves trail particles.
    /// </summary>
    public class JudgmentRayProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> bloomTexture;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail particles
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = -Projectile.velocity * 0.03f;
                Color c = HarmonyUtils.GetHarmonyColor(Main.rand.NextFloat(0.4f, 0.8f));
                HarmonyParticleHandler.Spawn(new RayTrailParticle(Projectile.Center, vel, c, 0.1f, 8));
            }

            Lighting.AddLight(Projectile.Center, HarmonyUtils.JudgmentEmber.ToVector3() * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 120);
            HarmonyParticleHandler.Spawn(new HarmonyBloomParticle(target.Center, HarmonyUtils.JudgmentEmber, 0.6f, 8));
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                HarmonyParticleHandler.Spawn(new HarmonyEmberParticle(target.Center, vel,
                    HarmonyUtils.GetHarmonyColor(Main.rand.NextFloat()), 0.08f, 10));
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                HarmonyParticleHandler.Spawn(new HarmonyNoteParticle(Projectile.Center, vel,
                    HarmonyUtils.SigilGold, 0.3f, 25));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            bloomTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!bloomTexture.IsLoaded) return false;
            var tex = bloomTexture.Value;

            float speed = Projectile.velocity.Length();
            float stretch = Math.Max(1f, speed * 0.3f);

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                HarmonyUtils.Additive(HarmonyUtils.JudgmentEmber, 0.6f), Projectile.rotation,
                tex.Size() / 2f, new Vector2(0.3f * stretch, 0.15f), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                HarmonyUtils.Additive(HarmonyUtils.RayWhite, 0.3f), Projectile.rotation,
                tex.Size() / 2f, new Vector2(0.15f * stretch, 0.08f), SpriteEffects.None, 0);

            return false;
        }
    }
}
