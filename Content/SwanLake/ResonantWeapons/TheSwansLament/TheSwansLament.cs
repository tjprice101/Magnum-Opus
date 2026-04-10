using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Projectiles;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Utilities;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament
{
    /// <summary>
    /// The Swan's Lament — a mourning shotgun (OVERHAUL).
    /// 
    /// COMBAT SYSTEM:
    /// • Lament Bullet: Fast white bullet with white streak trail, 3 feather shrapnel on hit
    /// • Destruction Halo: Every 6th shot fires a slow-moving expanding halo ring
    ///   - Enemies touching halo rim receive Mournful Gaze (-15% movement speed)
    /// • Lamentation Stacks: Consecutive hits on same target build stacks (max 5)
    ///   - At 5 stacks: target begins weeping (-20% attack speed, cosmetic)
    /// • Finale Lament: If a Destruction Halo kills an enemy, all enemies within
    ///   nova radius receive 5 Lamentation stacks instantly
    /// • Lament's Echo: Kills boost fire rate + spread temporarily
    /// </summary>
    public class TheSwansLament : ModItem
    {
        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/TheSwansLament/TheSwansLament";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 180;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 24;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 5.5f;
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.value = Item.sellPrice(gold: 60);
            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Bullet;
            Item.autoReuse = true;
            Item.crit = 10;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Fires a mourning volley of white bullets that leave streak trails"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "On hit, bullets scatter 3 feather shrapnel behind the target"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Every 6th shot fires a Destruction Halo — a slow, expanding ring that afflicts Mournful Gaze"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "Consecutive hits build Lamentation — at 5 stacks, enemies begin weeping"));
            tooltips.Add(new TooltipLine(Mod, "Effect5",
                "Killing enemies with a Halo triggers Finale Lament, spreading full Lamentation to nearby foes"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Each shot is a tear, and each tear is a farewell.'")
            {
                OverrideColor = new Color(240, 240, 255)
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var lamentPlayer = player.GetModPlayer<LamentPlayer>();
            float spreadMult = lamentPlayer.SpreadMult;

            // Check if this is a Destruction Halo shot (every 6th)
            bool isHaloShot = lamentPlayer.AdvanceShotCounter();

            if (isHaloShot)
            {
                // Fire a Destruction Halo instead of normal bullets
                Projectile.NewProjectile(source, position, velocity * 0.4f,
                    ModContent.ProjectileType<DestructionHaloProj>(),
                    (int)(damage * 1.5f), knockback * 2f, player.whoAmI);

                // Still fire a few bullets alongside the halo
                int sideCount = 4;
                float sideSpread = MathHelper.ToRadians(15f);
                for (int i = 0; i < sideCount; i++)
                {
                    float angle = Main.rand.NextFloat(-sideSpread, sideSpread);
                    Vector2 bulletVel = velocity.RotatedBy(angle) * Main.rand.NextFloat(0.9f, 1.1f);
                    Projectile.NewProjectile(source, position, bulletVel,
                        ModContent.ProjectileType<LamentBulletProj>(),
                        damage, knockback, player.whoAmI);
                }
            }
            else
            {
                // Normal bullet volley
                int bulletCount = Main.rand.Next(10, 16);
                float baseSpread = MathHelper.ToRadians(22f);
                float actualSpread = baseSpread * spreadMult;

                for (int i = 0; i < bulletCount; i++)
                {
                    float angle = Main.rand.NextFloat(-actualSpread, actualSpread);
                    float speedVariance = Main.rand.NextFloat(0.85f, 1.15f);
                    Vector2 bulletVel = velocity.RotatedBy(angle) * speedVariance;

                    Projectile.NewProjectile(source, position, bulletVel,
                        ModContent.ProjectileType<LamentBulletProj>(),
                        damage, knockback, player.whoAmI);
                }
            }

            // Rainbow muzzle sparkle on every shot
            try { SwanLakeVFXLibrary.SpawnPrismaticSparkles(position, 3, 12f); } catch { }

            return false;
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            // No damage modification from Echo — it's fire rate only
        }

        public override float UseSpeedMultiplier(Player player)
        {
            var echoPlayer = player.GetModPlayer<LamentPlayer>();
            return 1f / echoPlayer.FireRateMult; // lower FireRateMult = faster attacks
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10f, 0f);

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
            spriteBatch.Draw(tex, drawPos, null, SwanLakePalette.PureWhite with { A = 0 } * pulse,
                rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, SwanLakePalette.Pearlescent with { A = 0 } * (pulse * 0.6f),
                rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);
        }
    }
}
