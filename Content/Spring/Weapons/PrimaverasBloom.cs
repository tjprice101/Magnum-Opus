using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Spring.Projectiles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Spring.Weapons
{
    /// <summary>
    /// Primavera's Bloom - Spring-themed summon weapon (Post-WoF tier)
    /// Summons flower sprite minions that orbit and attack.
    /// - Spring Harmony: Multiple sprites boost each other's damage
    /// - Renewal Bond: Periodic healing to player
    /// - Pollen Cloud: Attacks leave lingering damage zones
    /// - Bloom Formation: 3+ sprites perform synchronized attacks
    /// </summary>
    public class PrimaverasBloom : ModItem
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringWhite = new Color(255, 250, 250);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color SpringYellow = new Color(255, 255, 180);

        public override void SetStaticDefaults()
        {
            ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f;
        }

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.damage = 42;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 10;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 2f;
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<FlowerSpriteMinion>();
            Item.buffType = ModContent.BuffType<FlowerSpriteBuff>();
        }

        public override void HoldItem(Player player)
        {
            // ========== IRIDESCENT WINGSPAN VFX PATTERN ==========
            // HEAVY DUST TRAILS - 2+ per frame with fadeIn (spring bloom dust)
            for (int d = 0; d < 2; d++)
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(26f, 26f);
                int dustType = Main.rand.NextBool() ? DustID.YellowTorch : DustID.PinkTorch;
                Color dustColor = Main.rand.NextBool() ? SpringYellow : SpringPink;
                Dust dust = Dust.NewDustPerfect(dustPos, dustType, new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(0.4f, 1.0f)), 0, dustColor, Main.rand.NextFloat(1.0f, 1.3f));
                dust.noGravity = true;
                dust.fadeIn = 1.4f;
            }
            
            // CONTRASTING SPARKLES - white/green contrast
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(28f, 28f);
                Color sparkleColor = Main.rand.NextBool() ? SpringWhite : SpringGreen;
                CustomParticles.PrismaticSparkle(sparklePos, sparkleColor, Main.rand.NextFloat(0.32f, 0.47f));
            }
            
            // SHIMMER TRAILS - floating blossom motes with color cycling
            if (Main.rand.NextBool(3))
            {
                float hue = 0.1f + Main.rand.NextFloat(0.08f); // Yellow-pink range
                Color shimmerColor = Main.hslToRgb(hue, 0.75f, 0.8f);
                Vector2 shimmerPos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 shimmerVel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -Main.rand.NextFloat(0.5f, 1.1f));
                var shimmer = new GenericGlowParticle(shimmerPos, shimmerVel, shimmerColor * 0.55f, 0.24f, 25, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }
            
            // MUSIC NOTES - visible scale with bloom theme
            if (Main.rand.NextBool(5))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(32f, 32f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.4f, 1.0f));
                Color noteColor = Color.Lerp(SpringYellow, SpringPink, Main.rand.NextFloat());
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, Main.rand.NextFloat(0.85f, 1.0f), 28);
            }
            
            // ORBITING FLOWER MOTES - bloom harmony
            if (Main.rand.NextBool(4))
            {
                float orbitAngle = Main.GameUpdateCount * 0.04f + Main.rand.NextFloat(MathHelper.TwoPi);
                float orbitRadius = 40f + Main.rand.NextFloat(13f);
                Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                Color orbitColor = Color.Lerp(SpringYellow, SpringGreen, Main.rand.NextFloat()) * 0.5f;
                var mote = new GenericGlowParticle(orbitPos, Vector2.Zero, orbitColor, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(mote);
            }

            // Enhanced dynamic lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.12f + 0.55f;
            Lighting.AddLight(player.Center, SpringYellow.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Apply buff
            player.AddBuff(Item.buffType, 2);

            // ========== SPECTACULAR FLOWER SPRITE SUMMONING VFX ==========
            // MULTI-LAYER CENTRAL FLARE - sprite manifestation
            CustomParticles.GenericFlare(Main.MouseWorld, Color.White, 0.85f, 18);
            CustomParticles.GenericFlare(Main.MouseWorld, SpringYellow, 0.68f, 20);
            CustomParticles.GenericFlare(Main.MouseWorld, SpringPink * 0.9f, 0.52f, 22);
            
            // 5-LAYER GRADIENT HALO CASCADE - spring summoning circle
            for (int ring = 0; ring < 5; ring++)
            {
                float progress = ring / 5f;
                Color ringColor = Color.Lerp(SpringYellow, SpringGreen, progress);
                float ringScale = 0.35f + ring * 0.11f;
                int ringLife = 14 + ring * 3;
                CustomParticles.HaloRing(Main.MouseWorld, ringColor * (0.68f - progress * 0.25f), ringScale, ringLife);
            }
            
            // RADIAL BLOOM DUST BURST - sprite emergence
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                int dustType = Main.rand.NextBool() ? DustID.YellowTorch : DustID.PinkTorch;
                Dust bloom = Dust.NewDustPerfect(Main.MouseWorld, dustType, dustVel, 0, SpringYellow, 1.3f);
                bloom.noGravity = true;
                bloom.fadeIn = 1.3f;
            }
            
            // SPARKLE RING - radiant bloom
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 sparklePos = Main.MouseWorld + angle.ToRotationVector2() * 30f;
                CustomParticles.PrismaticSparkle(sparklePos, Color.Lerp(SpringWhite, SpringGreen, Main.rand.NextFloat(0.3f)), 0.42f);
            }
            
            // MUSIC NOTE CHORUS - sprite song
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(0.2f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 3.5f);
                Color noteColor = Color.Lerp(SpringYellow, SpringPink, Main.rand.NextFloat());
                ThemedParticles.MusicNote(Main.MouseWorld, noteVel, noteColor, 0.88f, 26);
            }
            
            // RISING BLOOM WISPS - ascending to form
            for (int i = 0; i < 5; i++)
            {
                Vector2 wispVel = new Vector2(Main.rand.NextFloat(-1.2f, 1.2f), -Main.rand.NextFloat(2.5f, 4.5f));
                Color wispColor = Color.Lerp(SpringYellow, SpringGreen, Main.rand.NextFloat());
                var wisp = new GenericGlowParticle(Main.MouseWorld + Main.rand.NextVector2Circular(18f, 10f), wispVel, wispColor * 0.6f, 0.26f, 22, true);
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

            spriteBatch.Draw(texture, position, null, SpringYellow * 0.35f, rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SpringPink * 0.3f, rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SpringGreen * 0.25f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, SpringYellow.ToVector3() * 0.4f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MinionSlots", "Uses 1 minion slot") { OverrideColor = Color.Gray });
            tooltips.Add(new TooltipLine(Mod, "SpringHarmony", "Multiple sprites boost each other's attack speed") { OverrideColor = SpringPink });
            tooltips.Add(new TooltipLine(Mod, "RenewalBond", "Sprites periodically heal you for 3 HP") { OverrideColor = SpringGreen });
            tooltips.Add(new TooltipLine(Mod, "PollenCloud", "Attacks leave damaging pollen clouds") { OverrideColor = SpringYellow });
            tooltips.Add(new TooltipLine(Mod, "BloomFormation", "3+ sprites perform synchronized burst attacks") { OverrideColor = Color.Lerp(SpringPink, SpringYellow, 0.5f) });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Tiny guardians of the eternal spring'") { OverrideColor = SpringPink });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<VernalBar>(), 12)
                .AddIngredient(ModContent.ItemType<SpringResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofLight, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Buff for Flower Sprite minions
    /// </summary>
    public class FlowerSpriteBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Regeneration;
        
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<FlowerSpriteMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
