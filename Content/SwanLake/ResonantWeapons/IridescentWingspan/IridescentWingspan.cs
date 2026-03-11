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
using MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Projectiles;
using MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Utilities;
using MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Particles;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan
{
    /// <summary>
    /// Iridescent Wingspan — Magic Staff.
    /// 
    /// COMBAT SYSTEM:
    /// • Fires 3 wing-shaped spectral bolts in a spread pattern
    /// • While held, ethereal wings shimmer behind the player
    /// • Each bolt leaves an ethereal feather trail with prismatic edge glow
    /// • "Ethereal Flight" mechanic: Each hit builds wing charge (8 per hit)
    ///   — At 100 charge, next cast fires a single massive empowered wing blast
    ///   — Empowered blast passes through tiles, pierces 5 enemies, 3× damage
    ///   — Empowered blast leaves lingering ethereal feathers in its wake
    /// 
    /// STATS PRESERVED:
    /// Damage 420, Mana 16, UseTime 18, Knockback 5, Sell 60g, SwanRarity
    /// </summary>
    public class IridescentWingspan : ModItem
    {
        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/IridescentWingspan/IridescentWingspan";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 420;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 16;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 60);
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.UseSound = SoundID.Item43 with { Pitch = 0.6f, Volume = 0.8f };
            Item.autoReuse = true;

            Item.width = 50;
            Item.height = 50;
            Item.shoot = ModContent.ProjectileType<WingspanBoltProj>();
            Item.shootSpeed = 14f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var wsp = player.Wingspan();

            if (wsp.IsFullyCharged)
            {
                // Fire empowered wing blast
                wsp.ConsumeCharge();

                int empDmg = (int)(damage * 3f);
                Projectile.NewProjectile(source, position, velocity, type,
                    empDmg, knockback * 2f, player.whoAmI, ai0: 1f); // ai0=1 = empowered

                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.8f, Volume = 1f }, position);

                // Wing burst VFX
                var burst = new WingBurstParticle();
                burst.Initialize(player.Center, Vector2.Zero, Color.White, 0.5f);
                burst.Rotation = velocity.ToRotation();
                WingspanParticleHandler.Spawn(burst);

                return false;
            }

            // Normal: fire 5 bolts in wingspan fan pattern (2 outer arc, 2 inner arc, 1 center)
            // Each bolt gets a unique hue offset via ai[1] for spectrum cycling
            float[] spreadAngles = { -24f, -10f, 0f, 10f, 24f };
            for (int i = 0; i < 5; i++)
            {
                float angleRad = MathHelper.ToRadians(spreadAngles[i]);
                Vector2 spreadVel = velocity.RotatedBy(angleRad);
                float hueOffset = (float)i / 5f; // Each bolt at different spectrum position
                Projectile.NewProjectile(source, position, spreadVel, type,
                    damage, knockback, player.whoAmI, ai0: 0f, ai1: hueOffset);
            }

            // Track resonance — if cast within 1s of last convergence, grant bonus
            var wspData = player.Wingspan();
            if (wspData.ResonanceTimer > 0)
                wspData.ResonanceDamageBonus = 0.10f; // +10% on sequential casts

            // Muzzle sparks
            for (int i = 0; i < 5; i++)
            {
                var spark = new WingSparkParticle();
                spark.Initialize(
                    position,
                    velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 5f)
                        + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    WingspanUtils.EtherealWhite,
                    Main.rand.NextFloat(0.5f, 0.9f)
                );
                WingspanParticleHandler.Spawn(spark);
            }

            // Rainbow muzzle sparkle
            try { SwanLakeVFXLibrary.SpawnPrismaticSparkles(position, 4, 15f); } catch { }

            return false;
        }

        public override void HoldItem(Player player)
        {
            var wsp = player.Wingspan();
            wsp.WingDisplayTimer = 10;

            // Ethereal wing visual behind player
            float wingPhase = (float)Main.GameUpdateCount * 0.04f;
            float wingAlpha = 0.15f + (float)Math.Sin(wingPhase) * 0.05f;

            // Wing charge glow intensity
            float chargeRatio = wsp.WingCharge / 100f;
            if (chargeRatio > 0)
            {
                Color chargeCol = Color.Lerp(WingspanUtils.SpectralBlue, WingspanUtils.WingPrismatic, chargeRatio);
                Lighting.AddLight(player.Center, chargeCol.ToVector3() * 0.3f * chargeRatio);

                if (wsp.IsFullyCharged && Main.rand.NextBool(4))
                {
                    var mote = new PrismaticMoteParticle();
                    mote.Initialize(
                        player.Center + Main.rand.NextVector2Circular(30f, 30f),
                        Main.rand.NextVector2Circular(1f, 1f) + new Vector2(0, -0.5f),
                        WingspanUtils.WingPrismatic,
                        Main.rand.NextFloat(0.4f, 0.8f)
                    );
                    WingspanParticleHandler.Spawn(mote);
                }
            }

            // Ambient wing light
            Lighting.AddLight(player.Center, WingspanUtils.EtherealWhite.ToVector3() * wingAlpha);

            // Floating feather particles
            if (Main.rand.NextBool(10))
            {
                var feather = new EtherealFeatherParticle();
                float side = Main.rand.NextBool() ? -1f : 1f;
                feather.Initialize(
                    player.Center + new Vector2(side * Main.rand.NextFloat(20f, 40f), Main.rand.NextFloat(-15f, 15f)),
                    new Vector2(side * 0.3f, -0.5f) + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    WingspanUtils.EtherealWhite,
                    Main.rand.NextFloat(0.4f, 0.7f)
                );
                WingspanParticleHandler.Spawn(feather);
            }
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            var wsp = player.Wingspan();
            if (wsp.ResonanceDamageBonus > 0f)
                damage += wsp.ResonanceDamageBonus;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Fires five spectral wing bolts in a wingspan fan that converge at the cursor"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "All 5 bolts converging triggers a Prismatic Convergence burst with rainbow lasers"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Casting within 1 second of a convergence grants Wingspan Resonance: +10% damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "Each hit charges ethereal wings — at full charge, fires a devastating empowered wing blast"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'To witness the full wingspan is to know both the beauty and the death.'")
            {
                OverrideColor = new Color(240, 240, 255)
            });
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);

            spriteBatch.Draw(tex, drawPos, null, WingspanUtils.EtherealWhite * pulse, rotation,
                origin, scale, SpriteEffects.None, 0f);
        }
    }
}
