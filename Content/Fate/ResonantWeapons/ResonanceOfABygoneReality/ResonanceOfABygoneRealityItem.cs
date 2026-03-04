using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Fate.ResonantWeapons.ResonanceOfABygoneReality
{
    /// <summary>
    /// Resonance of a Bygone Reality — Fate endgame ranged weapon.
    /// Rapid-fire cosmic gun (damage 400, useTime 6).
    /// Every 5th hit spawns a spectral slashing blade at 2× damage.
    /// Self-contained VFX — no external VFX system dependencies.
    /// </summary>
    public class ResonanceOfABygoneRealityItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/ResonanceOfABygoneReality";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 400;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 50;
            Item.height = 26;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(gold: 52);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<ResonanceRapidBullet>();
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Rapid-fire cosmic bullets with 40% ammo conservation"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 5th hit summons a spectral blade that slashes for 3 seconds at 2x damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Bygone Resonance: blade and bullet striking the same enemy within 0.5s triggers a delayed explosion"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Every 10th combined hit grants Reality Fade — 0.3 seconds of invulnerability"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The past does not stay buried. It echoes through every bullet.'")
            {
                OverrideColor = new Color(180, 40, 80) // Cosmic Crimson — Fate lore color
            });
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return Main.rand.NextFloat() > 0.4f; // 40% chance to NOT consume
        }

        public override void HoldItem(Player player)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;
            Vector2 weaponTip = player.MountedCenter + new Vector2(player.direction * 28f, -6f);

            // Energy streams flowing toward weapon
            if (Main.rand.NextBool(4))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(25f, 50f);
                Vector2 startPos = weaponTip + angle.ToRotationVector2() * dist;
                Vector2 vel = (weaponTip - startPos).SafeNormalize(Vector2.Zero) * 2.5f;
                Color col = ResonanceUtils.GradientLerp(Main.rand.NextFloat());
                ResonanceParticleHandler.Spawn(ResonanceParticleType.BulletGlow,
                    startPos, vel, col * 0.5f, 0.16f, 18);
            }

            // Ambient star sparkles
            if (Main.rand.NextBool(8))
            {
                Vector2 starPos = player.MountedCenter + Main.rand.NextVector2Circular(30f, 30f);
                Color starCol = Main.rand.NextBool(3) ? ResonanceUtils.StarGold : ResonanceUtils.ConstellationSilver;
                ResonanceParticleHandler.Spawn(ResonanceParticleType.MemoryWisp,
                    starPos, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    starCol * 0.4f, 0.14f, 16);
            }

            // Pulsing energy light at weapon grip
            float pulse = 0.25f + MathF.Sin(time * 0.07f) * 0.1f;
            Color lightCol = Color.Lerp(ResonanceUtils.NebulaPurple, ResonanceUtils.CosmicRose, pulse);
            Lighting.AddLight(weaponTip, lightCol.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spawn rapid bullet with ±0.05 radian spread
            Vector2 bulletVel = velocity.RotatedByRandom(0.05f);
            Projectile.NewProjectile(source, position, bulletVel,
                ModContent.ProjectileType<ResonanceRapidBullet>(), damage, knockback, player.whoAmI);

            // Muzzle flash VFX
            if (!Main.dedServ)
            {
                Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 30f;
                Vector2 dir = velocity.SafeNormalize(Vector2.UnitX);

                // Directional sparks
                for (int i = 0; i < 3; i++)
                {
                    Vector2 sparkVel = dir * Main.rand.NextFloat(3f, 6f)
                        + dir.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-1.5f, 1.5f);
                    Color sparkCol = ResonanceUtils.GradientLerp(Main.rand.NextFloat(0.3f, 0.8f));
                    ResonanceParticleHandler.Spawn(ResonanceParticleType.MuzzleSpark,
                        muzzlePos, sparkVel, sparkCol, 0.18f, 10);
                }

                // Central flash glow
                ResonanceParticleHandler.Spawn(ResonanceParticleType.BulletGlow,
                    muzzlePos, Vector2.Zero, ResonanceUtils.CosmicRose * 0.7f, 0.25f, 6);

                // Muzzle dust puff
                Dust d = Dust.NewDustPerfect(muzzlePos, DustID.PinkTorch,
                    dir * 2f + Main.rand.NextVector2Circular(1f, 1f), 0, default, 1.2f);
                d.noGravity = true;

                Lighting.AddLight(muzzlePos, ResonanceUtils.CosmicRose.ToVector3() * 0.5f);
            }

            return false;
        }
    }
}
