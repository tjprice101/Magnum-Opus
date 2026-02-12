using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Summer.Projectiles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Summer.Weapons
{
    /// <summary>
    /// Solar Crest - Summer-themed summon weapon (Post-Mechs tier)
    /// Summons sun spirit minions that orbit and unleash solar fury.
    /// - Solar Spirit: Summons orbiting sun minions (65 damage)
    /// - Solar Flare: Spirits periodically unleash damaging flares
    /// - Radiant Bond: Spirits provide light and minor regen to player
    /// - Zenith Formation: 3+ spirits synchronize for powerful burst
    /// </summary>
    public class SolarCrest : ModItem
    {
        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunWhite = new Color(255, 250, 240);
        private static readonly Color SunRed = new Color(255, 100, 50);

        public override void SetStaticDefaults()
        {
            ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f;
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.damage = 65;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 12;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.value = Item.buyPrice(gold: 25);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<SunSpiritMinion>();
            Item.buffType = ModContent.BuffType<SunSpiritBuff>();
        }

        public override void HoldItem(Player player)
        {
            // ========== CALAMITY-STANDARD VFX PATTERN ==========
            // HEAVY DUST TRAILS - 2+ per frame with fadeIn
            for (int d = 0; d < 2; d++)
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(26f, 26f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.GoldFlame, new Vector2(0, -Main.rand.NextFloat(0.4f, 1.1f)), 0, SunGold, Main.rand.NextFloat(1.0f, 1.4f));
                dust.noGravity = true;
                dust.fadeIn = 1.4f;
            }
            
            // CONTRASTING SPARKLES - white solar brilliance
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(32f, 32f);
                CustomParticles.PrismaticSparkle(sparklePos, SunWhite, Main.rand.NextFloat(0.32f, 0.48f));
            }
            
            // SHIMMER TRAILS - rising solar motes with color cycling
            if (Main.rand.NextBool(3))
            {
                float hue = 0.09f + Main.rand.NextFloat(0.06f); // Gold to orange range
                Color shimmerColor = Main.hslToRgb(hue, 1f, 0.72f);
                Vector2 shimmerPos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 shimmerVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.6f, 1.3f));
                var shimmer = new GenericGlowParticle(shimmerPos, shimmerVel, shimmerColor * 0.6f, 0.25f, 22, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }
            
            // MUSIC NOTES - visible scale with solar spirit theme
            if (Main.rand.NextBool(5))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(34f, 34f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.7f, 0.7f), -Main.rand.NextFloat(0.5f, 1.1f));
                Color noteColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat());
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, Main.rand.NextFloat(0.85f, 1.0f), 30);
            }
            
            // ORBITING SPIRIT MOTES - solar bond visualization
            if (Main.rand.NextBool(4))
            {
                float orbitAngle = Main.GameUpdateCount * 0.045f + Main.rand.NextFloat(MathHelper.TwoPi);
                float orbitRadius = 42f + Main.rand.NextFloat(15f);
                Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                Color orbitColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat()) * 0.5f;
                var mote = new GenericGlowParticle(orbitPos, Vector2.Zero, orbitColor, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(mote);
            }

            // Enhanced dynamic lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.045f) * 0.12f + 0.6f;
            Lighting.AddLight(player.Center, SunGold.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Apply buff
            player.AddBuff(Item.buffType, 2);

            // ========== SPECTACULAR SOLAR SPIRIT SUMMONING VFX ==========
            // MULTI-LAYER CENTRAL FLARE - spirit manifestation
            CustomParticles.GenericFlare(Main.MouseWorld, Color.White, 0.9f, 18);
            CustomParticles.GenericFlare(Main.MouseWorld, SunGold, 0.7f, 20);
            CustomParticles.GenericFlare(Main.MouseWorld, SunOrange * 0.85f, 0.55f, 22);
            
            // 6-LAYER GRADIENT HALO CASCADE - solar summoning circle
            for (int ring = 0; ring < 6; ring++)
            {
                float progress = ring / 6f;
                Color ringColor = Color.Lerp(SunWhite, SunRed, progress);
                float ringScale = 0.38f + ring * 0.13f;
                int ringLife = 15 + ring * 3;
                CustomParticles.HaloRing(Main.MouseWorld, ringColor * (0.7f - progress * 0.28f), ringScale, ringLife);
            }
            
            // RADIAL SOLAR DUST BURST - spirit emergence
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                Dust sun = Dust.NewDustPerfect(Main.MouseWorld, DustID.GoldFlame, dustVel, 0, SunGold, 1.4f);
                sun.noGravity = true;
                sun.fadeIn = 1.4f;
            }
            
            // SPARKLE RING - radiant manifestation
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 sparklePos = Main.MouseWorld + angle.ToRotationVector2() * 35f;
                CustomParticles.PrismaticSparkle(sparklePos, SunWhite, 0.45f);
            }
            
            // MUSIC NOTE CHORUS - spirit song
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f + Main.rand.NextFloat(0.2f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color noteColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat());
                ThemedParticles.MusicNote(Main.MouseWorld, noteVel, noteColor, 0.9f, 28);
            }
            
            // RISING SPIRIT WISPS - ascending to form
            for (int i = 0; i < 6; i++)
            {
                Vector2 wispVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -Main.rand.NextFloat(3f, 5f));
                var wisp = new GenericGlowParticle(Main.MouseWorld + Main.rand.NextVector2Circular(20f, 10f), wispVel, SunGold * 0.65f, 0.28f, 22, true);
                MagnumParticleHandler.SpawnParticle(wisp);
            }

            // Spawn minion at cursor
            Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI);

            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.12f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, SunGold * 0.38f, rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SunOrange * 0.3f, rotation, origin, scale * pulse * 1.18f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SunRed * 0.22f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, SunGold.ToVector3() * 0.5f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MinionSlots", "Uses 1 minion slot") { OverrideColor = Color.Gray });
            tooltips.Add(new TooltipLine(Mod, "SolarSpirit", "Summons orbiting sun spirits that unleash solar fury") { OverrideColor = SunGold });
            tooltips.Add(new TooltipLine(Mod, "SolarFlare", "Spirits periodically release damaging solar flares") { OverrideColor = SunOrange });
            tooltips.Add(new TooltipLine(Mod, "RadiantBond", "Spirits provide light and +1 life regen") { OverrideColor = SunWhite });
            tooltips.Add(new TooltipLine(Mod, "ZenithFormation", "3+ spirits synchronize for devastating solar bursts") { OverrideColor = SunRed });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Fragments of the sun itself, bound to your will'") { OverrideColor = Color.Lerp(SunGold, SunOrange, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SolsticeBar>(), 16)
                .AddIngredient(ModContent.ItemType<SummerResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofMight, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Buff for Sun Spirit minions
    /// </summary>
    public class SunSpiritBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.OnFire;
        
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<SunSpiritMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
                // Radiant Bond: +1 life regen
                player.lifeRegen += 1;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
