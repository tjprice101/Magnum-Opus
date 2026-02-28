using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Projectiles
{
    // ═══════════════════════════════════════════════════════════
    // ThornBoltProjectile — small verdant glow bolt fired from the repeater
    // 14x14, pen 3, timeLeft 240, tileCollide true
    // On hit: embeds a StickyThornProjectile on the enemy
    // PreDraw: velocity-stretched green glow bolt with faint trail
    // ═══════════════════════════════════════════════════════════
    public class ThornBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 1;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void AI()
        {
            Projectile.alpha = Math.Max(Projectile.alpha - 30, 0);
            Projectile.ai[0]++;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Faint green spark trail
            if (!Main.dedServ && (int)Projectile.ai[0] % 3 == 0)
            {
                var spark = new ThornSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    Main.rand.NextFloat(0.08f, 0.15f),
                    Main.rand.Next(6, 12));
                ThornSprayParticleHandler.SpawnParticle(spark);
            }

            Lighting.AddLight(Projectile.Center, 0.2f, 0.4f, 0.1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner != Main.myPlayer)
                return;

            // Count existing sticky thorns on this NPC
            int stickyCount = 0;
            int oldestIndex = -1;
            int oldestTime = int.MinValue;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == ModContent.ProjectileType<StickyThornProjectile>()
                    && p.owner == Projectile.owner && (int)p.ai[1] == target.whoAmI)
                {
                    stickyCount++;
                    if ((int)p.ai[0] > oldestTime)
                    {
                        oldestTime = (int)p.ai[0];
                        oldestIndex = i;
                    }
                }
            }

            // Max 8 thorns per NPC — replace oldest if at cap
            if (stickyCount >= 8 && oldestIndex >= 0)
                Main.projectile[oldestIndex].Kill();

            // Spawn sticky thorn embedded on enemy
            Vector2 offset = Main.rand.NextVector2Circular(target.width * 0.3f, target.height * 0.3f);
            int idx = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                target.Center + offset,
                Vector2.Zero,
                ModContent.ProjectileType<StickyThornProjectile>(),
                Projectile.damage,
                0f,
                Projectile.owner,
                0f,         // ai[0] = timer
                target.whoAmI); // ai[1] = NPC index

            // Store the embed offset in localAI
            if (idx >= 0 && idx < Main.maxProjectiles)
            {
                Main.projectile[idx].localAI[0] = offset.X;
                Main.projectile[idx].localAI[1] = offset.Y;
            }

            // Spark burst on embed
            if (!Main.dedServ)
            {
                for (int i = 0; i < 4; i++)
                {
                    var spark = new ThornSparkParticle(
                        target.Center + offset,
                        Main.rand.NextVector2Circular(3f, 3f),
                        Main.rand.NextFloat(0.1f, 0.2f),
                        Main.rand.Next(8, 16));
                    ThornSprayParticleHandler.SpawnParticle(spark);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            _bloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _bloomTex.Value;
            Vector2 origin = tex.Size() / 2f;
            SpriteBatch sb = Main.spriteBatch;

            sb.End();
            ThornSprayUtils.BeginAdditive(sb);

            float alphaFade = 1f - (Projectile.alpha / 255f);
            float pulse = 1f + (float)Math.Sin(Projectile.ai[0] * 0.25f) * 0.12f;
            float rot = Projectile.velocity.ToRotation();

            // Trail from old positions
            for (int i = Projectile.oldPos.Length - 1; i >= 1; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailFade = (1f - (float)i / Projectile.oldPos.Length) * alphaFade;
                Color trailCol = ThornSprayUtils.Additive(ThornSprayUtils.ThornGreen, trailFade * 0.35f);
                sb.Draw(tex, trailPos, null, trailCol, rot, origin,
                    new Vector2(0.2f * pulse, 0.08f) * (1f - (float)i / Projectile.oldPos.Length),
                    SpriteEffects.None, 0f);
            }

            // Velocity-stretched outer glow
            float speed = Projectile.velocity.Length();
            float stretch = 1f + speed * 0.06f;
            Color outerCol = ThornSprayUtils.Additive(ThornSprayUtils.VerdantBolt, 0.55f * alphaFade);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, outerCol, rot, origin,
                new Vector2(0.25f * stretch * pulse, 0.12f * pulse), SpriteEffects.None, 0f);

            // Core bright green
            Color coreCol = ThornSprayUtils.Additive(ThornSprayUtils.FlashWhite, 0.4f * alphaFade);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, coreCol, rot, origin,
                new Vector2(0.1f * stretch * pulse, 0.05f * pulse), SpriteEffects.None, 0f);

            sb.End();
            ThornSprayUtils.BeginDefault(sb);

            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // StickyThornProjectile — embeds on enemy, follows NPC, explodes after 60 ticks
    // 8x8, pen -1, timeLeft 120, tileCollide false
    // ai[0] = frame timer, ai[1] = NPC index
    // localAI[0/1] = embed offset X/Y
    // After 60 ticks OR NPC death: chain explosion (1/2 damage, 80px radius)
    //   spawns 4 ThornSplinterProjectile + particle burst + music notes
    // Stacking bonus: each sticky on same NPC adds +10% explosion damage
    // PreDraw: pulsing glow that shifts green->gold as timer approaches 0
    // ═══════════════════════════════════════════════════════════
    public class StickyThornProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;

        private const int DetonationTime = 60;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            // Don't deal contact damage — the explosion does the damage
            Projectile.friendly = false;
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            int npcIndex = (int)Projectile.ai[1];
            Vector2 embedOffset = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);

            // Follow the NPC
            if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
            {
                NPC target = Main.npc[npcIndex];
                if (target.active)
                {
                    Projectile.Center = target.Center + embedOffset;

                    // Pulsing glow particle while embedded
                    if (!Main.dedServ && (int)Projectile.ai[0] % 8 == 0)
                    {
                        var glow = new StickyGlowParticle(
                            Projectile.Center,
                            npcIndex,
                            embedOffset,
                            Main.rand.NextFloat(0.15f, 0.25f),
                            12);
                        ThornSprayParticleHandler.SpawnParticle(glow);
                    }
                }
                else
                {
                    // NPC died — detonate immediately
                    Detonate();
                    return;
                }
            }
            else
            {
                Projectile.Kill();
                return;
            }

            // Timer reached — detonate
            if (Projectile.ai[0] >= DetonationTime)
            {
                Detonate();
            }

            Lighting.AddLight(Projectile.Center, 0.15f + Projectile.ai[0] / DetonationTime * 0.3f,
                0.3f, 0.05f);
        }

        private void Detonate()
        {
            if (Projectile.owner == Main.myPlayer)
            {
                int npcIndex = (int)Projectile.ai[1];

                // Count stacking bonus: each other sticky thorn on same NPC = +10% damage
                int stickyCount = 0;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active && p.whoAmI != Projectile.whoAmI
                        && p.type == Projectile.type
                        && p.owner == Projectile.owner
                        && (int)p.ai[1] == npcIndex)
                    {
                        stickyCount++;
                    }
                }

                // Explosion damage = 1/2 weapon damage + 10% per additional thorn (max 8)
                int baseDmg = Math.Max(Projectile.damage / 2, 1);
                float stackBonus = 1f + Math.Min(stickyCount, 8) * 0.1f;
                int explosionDmg = (int)(baseDmg * stackBonus);

                // Deal splash damage in 80-pixel radius
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                        continue;

                    if (Vector2.Distance(Projectile.Center, npc.Center) <= 80f)
                    {
                        Player owner = Main.player[Projectile.owner];
                        int dir = Projectile.Center.X < npc.Center.X ? 1 : -1;
                        var hitInfo = npc.CalculateHitInfo(explosionDmg, dir, false, 0f,
                            DamageClass.Ranged, true);
                        npc.StrikeNPC(hitInfo, false, false);
                        npc.AddBuff(BuffID.Poisoned, 180);

                        if (Main.netMode != NetmodeID.SinglePlayer)
                            NetMessage.SendStrikeNPC(npc, hitInfo);
                    }
                }

                // Spawn 4 splinter projectiles
                int splinterDmg = Math.Max(Projectile.damage / 4, 1);
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi / 4f * i + Main.rand.NextFloat(-0.3f, 0.3f);
                    Vector2 splinterVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        splinterVel,
                        ModContent.ProjectileType<ThornSplinterProjectile>(),
                        splinterDmg,
                        Projectile.knockBack * 0.2f,
                        Projectile.owner);
                }
            }

            // Explosion VFX
            if (!Main.dedServ)
            {
                // Golden chain explosion ring
                var ring = new ChainExplosionParticle(
                    Projectile.Center,
                    1.5f,
                    20);
                ThornSprayParticleHandler.SpawnParticle(ring);

                // Burst of thorn sparks
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi / 8f * i + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                    var spark = new ThornSparkParticle(
                        Projectile.Center,
                        sparkVel,
                        Main.rand.NextFloat(0.15f, 0.3f),
                        Main.rand.Next(10, 20));
                    ThornSprayParticleHandler.SpawnParticle(spark);
                }

                // Music note scatter — jubilant note burst
                for (int i = 0; i < 5; i++)
                {
                    Vector2 noteVel = Main.rand.NextVector2Circular(4f, 4f) + new Vector2(0f, -2f);
                    var note = new ThornNoteParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                        noteVel,
                        Main.rand.NextFloat(0.15f, 0.3f),
                        Main.rand.Next(30, 55));
                    ThornSprayParticleHandler.SpawnParticle(note);
                }
            }

            Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            _bloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _bloomTex.Value;
            Vector2 origin = tex.Size() / 2f;
            SpriteBatch sb = Main.spriteBatch;

            sb.End();
            ThornSprayUtils.BeginAdditive(sb);

            float progress = Math.Clamp(Projectile.ai[0] / DetonationTime, 0f, 1f);
            float pulse = 1f + (float)Math.Sin(Projectile.ai[0] * 0.2f) * 0.3f * progress;
            float growScale = (0.12f + progress * 0.2f) * pulse;

            // Color shifts green -> amber -> gold as timer approaches detonation
            Color baseCol = ThornSprayUtils.MulticolorLerp(progress,
                ThornSprayUtils.ThornGreen, ThornSprayUtils.AmberWarn, ThornSprayUtils.ExplosionGold);
            Color outerCol = ThornSprayUtils.Additive(baseCol, 0.5f + progress * 0.5f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, outerCol, 0f, origin,
                growScale, SpriteEffects.None, 0f);

            // Inner bright core — intensifies near detonation
            if (progress > 0.3f)
            {
                float coreAlpha = (progress - 0.3f) / 0.7f;
                Color coreCol = ThornSprayUtils.Additive(ThornSprayUtils.FlashWhite, coreAlpha * 0.5f * pulse);
                sb.Draw(tex, Projectile.Center - Main.screenPosition, null, coreCol, 0f, origin,
                    growScale * 0.4f, SpriteEffects.None, 0f);
            }

            sb.End();
            ThornSprayUtils.BeginDefault(sb);

            return false;
        }

        public override bool? CanDamage() => false; // damage is dealt manually via Detonate()
    }

    // ═══════════════════════════════════════════════════════════
    // ThornSplinterProjectile — small homing splinter from chain explosion
    // 8x8, pen 1, timeLeft 60, mild homing 0.05, 1/4 weapon damage
    // Applies Poisoned 60 on hit
    // PreDraw: tiny green glow
    // ═══════════════════════════════════════════════════════════
    public class ThornSplinterProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Mild homing toward closest NPC
            NPC target = ThornSprayUtils.ClosestNPC(Projectile.Center, 300f);
            if (target != null)
            {
                Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired * Projectile.velocity.Length(), 0.05f);
            }

            // Small trail particles
            if (!Main.dedServ && (int)Projectile.ai[0] % 2 == 0)
            {
                var trail = new SplinterTrailParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(2f, 2f),
                    -Projectile.velocity * 0.06f,
                    Main.rand.NextFloat(0.05f, 0.1f),
                    Main.rand.Next(6, 12));
                ThornSprayParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(Projectile.Center, 0.12f, 0.25f, 0.06f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 60);

            // Small green spark burst on impact
            if (!Main.dedServ)
            {
                for (int i = 0; i < 3; i++)
                {
                    var spark = new ThornSparkParticle(
                        target.Center + Main.rand.NextVector2Circular(8f, 8f),
                        Main.rand.NextVector2Circular(2f, 2f),
                        Main.rand.NextFloat(0.08f, 0.14f),
                        Main.rand.Next(5, 10));
                    ThornSprayParticleHandler.SpawnParticle(spark);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            _bloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _bloomTex.Value;
            Vector2 origin = tex.Size() / 2f;
            SpriteBatch sb = Main.spriteBatch;

            sb.End();
            ThornSprayUtils.BeginAdditive(sb);

            float pulse = 1f + (float)Math.Sin(Projectile.ai[0] * 0.35f) * 0.15f;
            float rot = Projectile.velocity.ToRotation();
            float speed = Projectile.velocity.Length();
            float stretch = 1f + speed * 0.05f;

            // Outer green glow
            Color outerCol = ThornSprayUtils.Additive(ThornSprayUtils.VerdantBolt, 0.5f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, outerCol, rot, origin,
                new Vector2(0.12f * stretch * pulse, 0.06f * pulse), SpriteEffects.None, 0f);

            // Core white-green
            Color coreCol = ThornSprayUtils.Additive(ThornSprayUtils.FlashWhite, 0.3f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, coreCol, rot, origin,
                new Vector2(0.05f * stretch * pulse, 0.025f * pulse), SpriteEffects.None, 0f);

            sb.End();
            ThornSprayUtils.BeginDefault(sb);

            return false;
        }
    }
}
