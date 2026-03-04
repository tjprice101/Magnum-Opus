using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture.Buffs;
using MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture.Projectiles
{
    /// <summary>
    /// Celestial Muse minion — a musical spirit that hovers near the player
    /// and fires MuseNoteProjectile at enemies. Ranged-attack minion.
    /// </summary>
    public class CelestialMuseMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Nachtmusik/Weapons/GalacticOverture/CelestialMuseMinion";

        private float hoverAngle;
        private int attackCooldown;

        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override bool? CanCutTiles() => false;

        public override bool MinionContactDamage() => false;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!CheckActive(owner))
                return;

            hoverAngle += 0.02f;
            attackCooldown = Math.Max(0, attackCooldown - 1);

            // Hover near player with sinusoidal bob
            float hoverOffset = (float)Math.Sin(hoverAngle) * 30f;
            Vector2 idealPos = owner.Center + new Vector2(owner.direction * -60f, -50f + hoverOffset);
            Vector2 toIdeal = idealPos - Projectile.Center;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.1f, 0.08f);

            Projectile.spriteDirection = owner.direction;

            // Find and attack target
            NPC target = FindTarget(owner, 700f);
            if (target != null && attackCooldown == 0)
            {
                // Fire musical projectile
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, toTarget * 14f,
                    ModContent.ProjectileType<MuseNoteProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner);

                attackCooldown = 20;

                // Fire VFX
                GalacticOvertureVFX.MinionAttackVFX(Projectile.Center, toTarget);
            }

            // Ambient VFX
            GalacticOvertureVFX.MinionAmbientVFX(Projectile.Center, 1f);

            // Occasional ambient music notes
            if (Main.rand.NextBool(12))
            {
                NachtmusikVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 10f, 0.3f, 0.5f, 16);
            }

            Lighting.AddLight(Projectile.Center, NachtmusikPalette.RadianceGold.ToVector3() * 0.4f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float time = (float)Main.timeForVisualEffects * 0.03f;
            float pulse = MathF.Sin(Main.GameUpdateCount * 0.12f) * 0.15f + 0.85f;

            // ═══════════════════════════════════════════════════════════════
            //  SHADER LAYER: OvertureAura — musical spirit's radiant presence
            // ═══════════════════════════════════════════════════════════════
            if (NachtmusikShaderManager.HasSerenade)
            {
                Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
                if (glowTex != null)
                {
                    NachtmusikShaderManager.BeginShaderAdditive(sb);
                    NachtmusikShaderManager.ApplySerenade(time, NachtmusikPalette.RadianceGold,
                        NachtmusikPalette.Violet, phase: (float)(Main.timeForVisualEffects * 0.008f) % 1f);

                    sb.Draw(glowTex, drawPos, null,
                        NachtmusikPalette.RadianceGold with { A = 0 } * 0.25f,
                        0f, glowTex.Size() / 2f, 0.35f * pulse, SpriteEffects.None, 0f);

                    NachtmusikShaderManager.RestoreSpriteBatch(sb);
                }
            }

            // ═══════════════════════════════════════════════════════════════
            //  BLOOM LAYER: Multi-scale golden glow behind sprite
            // ═══════════════════════════════════════════════════════════════
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
            {
                Vector2 glowOrigin = glow.Size() / 2f;
                // Outer warm halo
                sb.Draw(glow, drawPos, null, NachtmusikPalette.RadianceGold with { A = 0 } * 0.3f,
                    0f, glowOrigin, 0.6f * pulse, SpriteEffects.None, 0f);
                // Inner violet accent
                sb.Draw(glow, drawPos, null, NachtmusikPalette.Violet with { A = 0 } * 0.2f,
                    0f, glowOrigin, 0.4f * pulse, SpriteEffects.None, 0f);
                // Core white
                sb.Draw(glow, drawPos, null, NachtmusikPalette.StarWhite with { A = 0 } * 0.1f,
                    0f, glowOrigin, 0.2f * pulse, SpriteEffects.None, 0f);
            }

            // Main sprite
            sb.Draw(tex, drawPos, null, Color.White, 0f, origin, Projectile.scale, effects, 0f);

            // Nachtmusik theme star flare accent
            NachtmusikShaderManager.BeginAdditive(sb);
            NachtmusikVFXLibrary.DrawThemeStarFlare(sb, Projectile.Center, 1f, 0.5f);
            NachtmusikShaderManager.RestoreSpriteBatch(sb);

            return false;
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<GalacticOvertureBuff>());
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<GalacticOvertureBuff>()))
                Projectile.timeLeft = 2;

            return true;
        }

        private NPC FindTarget(Player owner, float range)
        {
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC manual = Main.npc[owner.MinionAttackTargetNPC];
                if (manual.active && manual.CanBeChasedBy(Projectile) && Vector2.Distance(owner.Center, manual.Center) < range * 1.5f)
                    return manual;
            }

            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(owner.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
    }
}
