using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Common;
using System.Collections.Generic;
using System.Linq;

namespace MagnumOpus.Content.Eroica.Weapons.FuneralPrayer
{
    public class FuneralPrayer : ModItem
    {
        public static Dictionary<int, HashSet<int>> BeamHitTracking = new Dictionary<int, HashSet<int>>();

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 340;
            Item.DamageType = DamageClass.Magic;
            Item.width = 50;
            Item.height = 50;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(gold: 35);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item20;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FuneralPrayerProjectile>();
            Item.shootSpeed = 16f;
            Item.mana = 14;
            Item.noMelee = true;
            Item.maxStack = 1;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int shotId = Main.rand.Next(int.MaxValue);
            BeamHitTracking[shotId] = new HashSet<int>();

            int beamCount = 5;
            int beamDamage = (int)(damage * 0.66f);
            float spreadAngle = MathHelper.ToRadians(90);

            Vector2 towardsCursor = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            float baseAngle = towardsCursor.ToRotation();

            for (int i = 0; i < beamCount; i++)
            {
                float offsetAngle = spreadAngle * ((float)i / (beamCount - 1) - 0.5f);
                float finalAngle = baseAngle + offsetAngle;
                Vector2 beamVelocity = new Vector2(1f, 0f).RotatedBy(finalAngle) * 15f;

                Projectile.NewProjectile(source, player.Center, beamVelocity,
                    ModContent.ProjectileType<FuneralPrayerBeam>(), beamDamage, knockback * 0.5f, player.whoAmI, shotId);
            }

            return false;
        }

        public static void RegisterBeamHit(int shotId, int beamIndex)
        {
            if (!BeamHitTracking.ContainsKey(shotId))
                BeamHitTracking[shotId] = new HashSet<int>();

            BeamHitTracking[shotId].Add(beamIndex);

            if (BeamHitTracking[shotId].Count >= 5)
            {
                Player player = Main.player.FirstOrDefault(p => p.active);
                if (player != null)
                {
                    NPC nearestEnemy = null;
                    float minDist = 1000f;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && npc.lifeMax > 5)
                        {
                            float dist = Vector2.Distance(player.Center, npc.Center);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                nearestEnemy = npc;
                            }
                        }
                    }

                    if (nearestEnemy != null)
                    {
                        Vector2 direction = (nearestEnemy.Center - player.Center).SafeNormalize(Vector2.UnitX);
                        Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, direction * 20f,
                            ModContent.ProjectileType<FuneralPrayerRicochetBeam>(), 400, 10f, player.whoAmI);

                    }
                }

                BeamHitTracking.Remove(shotId);
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires a spreading volley of five sacred flame beams"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "When all five beams find their mark, a devastating ricochet beam is unleashed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Ricochet beams chain between enemies, growing fiercer with each leap"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Ashes to ashes, hymn to silence — the last prayer is always fire'")
            {
                OverrideColor = new Color(200, 50, 50)
            });
        }
    }
}
