using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake.Projectiles;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake.Utilities;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake
{
    /// <summary>
    /// Call of the Pearlescent Lake — Ranged Rapid-Fire Gun.
    /// 
    /// COMBAT SYSTEM:
    /// • Rapid-fire bullets that convert to pearlescent rockets on fire
    /// • Rockets leave opalescent shimmer trails (3-pass shader: bloom → core → halo)
    /// • On impact: concentric water-ripple explosion with pearl droplets
    /// • "Still Waters" mechanic: standing still for 1.5s creates a pearlescent zone
    ///   beneath the player that grants 15% damage boost and mild homing to rockets
    /// • Every 8th shot is an amplified "Tidal Rocket" with 2× blast radius and seeking
    /// 
    /// STATS PRESERVED FROM ORIGINAL:
    /// Damage 380, UseTime 8, Knockback 3, ShootSpeed 18, Sell 60g, SwanRarity
    /// </summary>
    public class CallofthePearlescentLake : ModItem
    {
        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/CallofthePearlescentLake/CallofthePearlescentLake";

        private int _shotCounter;
        private int _stillTimer;
        private Vector2 _lastPosition;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 380;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 60);
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.UseSound = SoundID.Item11 with { Pitch = 0.3f, Volume = 0.7f };
            Item.autoReuse = true;

            Item.width = 54;
            Item.height = 28;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet;
        }

        public override void HoldItem(Player player)
        {
            // Still Waters detection
            float distMoved = Vector2.Distance(player.Center, _lastPosition);
            _lastPosition = player.Center;

            if (distMoved < 0.5f)
            {
                _stillTimer++;
                if (_stillTimer >= 90) // 1.5 seconds
                {
                    // Pearlescent zone visual
                    float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f);
                    float intensity = MathHelper.Clamp((_stillTimer - 90) / 60f, 0f, 1f);

                    if (Main.rand.NextBool(3))
                    {
                        Vector2 offset = new Vector2(Main.rand.NextFloat(-40f, 40f), Main.rand.NextFloat(10f, 30f));
                        Dust d = Dust.NewDustPerfect(player.Bottom + offset, DustID.WhiteTorch,
                            new Vector2(0, -0.8f) + Main.rand.NextVector2Circular(0.3f, 0.3f),
                            0, PearlescentUtils.LakeSilver, 0.9f * intensity);
                        d.noGravity = true;
                        d.fadeIn = 0.8f;
                    }

                    Lighting.AddLight(player.Bottom, PearlescentUtils.MistBlue.ToVector3() * (0.4f + pulse * 0.1f) * intensity);
                }
            }
            else
            {
                _stillTimer = Math.Max(0, _stillTimer - 3);
            }

            // Gentle ambient glow
            Lighting.AddLight(player.Center, new Vector3(0.25f, 0.27f, 0.35f));
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            _shotCounter++;
            bool isTidal = _shotCounter % 8 == 0;
            bool isStillWaters = _stillTimer >= 90;

            // Slight spread
            float spread = MathHelper.ToRadians(isTidal ? 1f : 3.5f);
            velocity = velocity.RotatedByRandom(spread);

            // Muzzle flash particles
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 dustVel = velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 5f)
                        + Main.rand.NextVector2Circular(1.5f, 1.5f);
                    Dust d = Dust.NewDustPerfect(position, DustID.WhiteTorch, dustVel, 0,
                        PearlescentUtils.PearlWhite, 0.8f);
                    d.noGravity = true;
                }
            }

            // Fire our custom rocket instead of the bullet
            float ai0 = isTidal ? 1f : 0f;
            float ai1 = isStillWaters ? 1f : 0f;

            int dmg = isTidal ? (int)(damage * 1.4f) : damage;
            Projectile.NewProjectile(source, position, velocity,
                ModContent.ProjectileType<PearlescentRocketProj>(),
                dmg, knockback, player.whoAmI, ai0: ai0, ai1: ai1);

            return false; // We already spawned the projectile
        }

        public override Vector2? HoldoutOffset() => new Vector2(-6, 0);

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Converts ammunition into pearlescent rockets that explode in rippling waves"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Every 8th shot fires a tidal rocket with amplified blast and seeking"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Standing still creates a pearlescent zone that empowers your rockets"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'The lake remembers every ripple, and returns them thousandfold'")
            {
                OverrideColor = PearlescentUtils.LoreColor
            });
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);

            spriteBatch.Draw(tex, drawPos, null, PearlescentUtils.LakeSilver * pulse, rotation,
                origin, scale, SpriteEffects.None, 0f);
        }
    }
}
