using System;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.Spring.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Spring.Weapons
{
    /// <summary>
    /// Blossom's Edge swing projectile — Spring theme melee. ExobladeStyleSwing architecture.
    /// 3-phase Petal Dance combo; every 5th hit triggers Renewal Strike (8 HP heal);
    /// crits trigger Spring Bloom (seeking crystals + AoE petal burst).
    /// </summary>
    public class BlossomsEdgeSwing : ExobladeStyleSwing
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringWhite = new Color(255, 250, 250);
        private static readonly Color SpringGreen = new Color(200, 255, 200);
        private static readonly Color CherryBlossom = new Color(255, 183, 197);

        private int comboPhase = 0;
        private int hitCounter = 0;

        protected override bool SupportsDash => false;
        protected override float BladeLength => 100f;
        protected override int BaseSwingFrames => 78;
        protected override float TextureDrawScale => 0.12f;
        protected override Color SlashPrimaryColor => SpringPink;
        protected override Color SlashSecondaryColor => new Color(120, 80, 100);
        protected override Color SlashAccentColor => SpringGreen;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/SpringGradientLUTandRAMP";

        public override string Texture => "MagnumOpus/Content/Spring/Weapons/BlossomsEdge";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(SpringPink, SpringGreen, (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(new Color(255, 180, 200), new Color(255, 220, 230), Main.rand.NextFloat())
                : Color.Lerp(new Color(255, 140, 160), SpringGreen, Main.rand.NextFloat());
        }

        protected override void OnSwingStart(bool isFirstSwing)
        {
            if (Main.myPlayer != Projectile.owner) return;

            int phase = comboPhase % 3;
            comboPhase++;

            Player player = Owner;
            int damage = Projectile.damage;
            float knockback = Projectile.knockBack;
            Vector2 aimDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            IEntitySource source = Projectile.GetSource_FromThis();
            int petalType = ModContent.ProjectileType<BlossomPetal>();
            int petalDmg = damage / 3;

            if (phase == 1)
            {
                // Phase 1: scatter 2-3 BlossomPetal projectiles
                int petalCount = 2 + Main.rand.Next(2);
                for (int i = 0; i < petalCount; i++)
                {
                    float spread = MathHelper.ToRadians(-25f + i * (50f / Math.Max(petalCount - 1, 1)));
                    Vector2 vel = aimDir.RotatedBy(spread) * Main.rand.NextFloat(6f, 9f);
                    Projectile.NewProjectile(source, player.MountedCenter, vel,
                        petalType, petalDmg, knockback * 0.5f, player.whoAmI);
                }
            }
            else if (phase == 2)
            {
                // Phase 2: 4 BlossomPetals + 3 seeking crystals
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.ToRadians(-40f + i * 27f);
                    Vector2 vel = aimDir.RotatedBy(angle) * Main.rand.NextFloat(7f, 11f);
                    Projectile.NewProjectile(source, player.MountedCenter, vel,
                        petalType, petalDmg, knockback * 0.5f, player.whoAmI);
                }

                SeekingCrystalHelper.SpawnSpringCrystals(
                    source, player.MountedCenter, aimDir * 8f,
                    (int)(damage * 0.35f), knockback * 0.3f, player.whoAmI, count: 3);
            }
        }

        protected override void OnSwingFrame()
        {
            if (Progression > 0.10f && Progression < 0.92f)
            {
                Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;

                // Pink petal dust
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dustPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.4f, 1f);
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.PinkFairy,
                        -SwordDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1f, 1f),
                        0, SpringPink, 1.4f);
                    d.noGravity = true;
                    d.fadeIn = 1.1f;
                }

                // Green sparkle every other frame
                if (Main.GameUpdateCount % 2 == 0)
                {
                    Dust g = Dust.NewDustPerfect(
                        Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.5f, 0.9f),
                        DustID.GreenFairy,
                        -SwordDirection * Main.rand.NextFloat(0.5f, 2f),
                        0, SpringGreen, 1.1f);
                    g.noGravity = true;
                }

                // Music note dust at tip
                if (Main.rand.NextBool(4))
                {
                    Color noteColor = Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat());
                    Dust note = Dust.NewDustPerfect(
                        tipPos + Main.rand.NextVector2Circular(6f, 6f),
                        DustID.Enchanted_Pink,
                        -SwordDirection * 1.5f + Main.rand.NextVector2Circular(1f, 1f),
                        0, noteColor, Main.rand.NextFloat(0.7f, 0.95f) * 1.6f);
                    note.noGravity = true;
                }
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            hitCounter++;

            // Halo ring impact VFX
            for (int i = 0; i < 4; i++)
            {
                float progress = i / 4f;
                Color ringColor = Color.Lerp(SpringPink, SpringGreen, progress);
                for (int j = 0; j < 2; j++)
                {
                    float angle = MathHelper.TwoPi * j / 2f + i * MathHelper.PiOver4;
                    Vector2 offset = angle.ToRotationVector2() * (15f + i * 8f);
                    Dust ring = Dust.NewDustPerfect(target.Center + offset, DustID.PinkFairy,
                        offset.SafeNormalize(Vector2.Zero) * 2f, 0, ringColor, 1.3f);
                    ring.noGravity = true;
                }
            }

            // Shimmer flares
            for (int i = 0; i < 3; i++)
            {
                Dust shimmer = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(12f, 12f),
                    DustID.Enchanted_Pink,
                    Main.rand.NextVector2Circular(3f, 3f), 0, SpringWhite, 1.5f);
                shimmer.noGravity = true;
            }

            // Dust burst
            for (int i = 0; i < 6; i++)
            {
                Dust burst = Dust.NewDustPerfect(target.Center, DustID.PinkFairy,
                    Main.rand.NextVector2Circular(5f, 5f), 0,
                    Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat()), 1.4f);
                burst.noGravity = true;
            }

            // Renewal Strike — every 5th hit heals 8 HP
            if (hitCounter >= 5)
            {
                hitCounter = 0;
                if (Main.myPlayer == Projectile.owner)
                {
                    owner.Heal(8);
                    CombatText.NewText(owner.getRect(), new Color(100, 255, 130), "Renewal!", true, false);
                }
                SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.4f, Volume = 0.6f }, target.Center);
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                    Dust heal = Dust.NewDustPerfect(owner.Center, DustID.GreenFairy, vel, 0, SpringGreen, 1.8f);
                    heal.noGravity = true;
                }
                for (int i = 0; i < 6; i++)
                {
                    Dust sparkle = Dust.NewDustPerfect(owner.Center + Main.rand.NextVector2Circular(20f, 20f),
                        DustID.Enchanted_Gold,
                        new Vector2(0, -Main.rand.NextFloat(1f, 3f)),
                        0, new Color(180, 255, 180), 1.3f);
                    sparkle.noGravity = true;
                }
            }

            // Spring Bloom — on crit: seeking crystals + AoE petal burst
            if (hit.Crit)
            {
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.3f, Volume = 0.7f }, target.Center);
                for (int i = 0; i < 8; i++)
                {
                    Dust critDust = Dust.NewDustPerfect(target.Center, DustID.PinkFairy,
                        Main.rand.NextVector2Circular(7f, 7f), 0, Color.White, 1.6f);
                    critDust.noGravity = true;
                }

                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 petalVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                    Dust petal = Dust.NewDustPerfect(target.Center, DustID.PinkFairy,
                        petalVel, 0, Color.Lerp(SpringPink, CherryBlossom, Main.rand.NextFloat()), 1.8f);
                    petal.noGravity = true;
                }

                if (Main.myPlayer == Projectile.owner)
                {
                    SeekingCrystalHelper.SpawnSpringCrystals(
                        Projectile.GetSource_FromThis(), target.Center,
                        (target.Center - owner.Center).SafeNormalize(Vector2.UnitY) * 6f,
                        (int)(Projectile.damage * 0.4f), Projectile.knockBack * 0.3f,
                        Projectile.owner, count: 4);

                    // AoE — 50% damage to nearby enemies
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI &&
                            Vector2.DistanceSquared(npc.Center, target.Center) < 10000f)
                        {
                            npc.SimpleStrikeNPC(Projectile.damage / 2, hit.HitDirection, hit.Crit, 0f, null, false, 0f, true);
                        }
                    }
                }
            }
        }
    }
}
