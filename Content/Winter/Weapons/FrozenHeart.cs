using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.Winter.Projectiles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Winter.Weapons
{
    /// <summary>
    /// Frozen Heart - Winter-themed summon weapon (Post-Golem tier)
    /// Summons sentinels of eternal ice to fight for you.
    /// - Frost Sentinel: Summons an orbiting ice elemental (115 damage)
    /// - Cryo Synchrony: 3+ sentinels create a freezing aura around the player
    /// - Shatter Strike: Critical hits cause enemies to shatter, damaging nearby foes
    /// - Permafrost Bond: Sentinels gain 15% damage for each frozen enemy nearby
    /// </summary>
    public class FrozenHeart : ModItem
    {
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color DeepBlue = new Color(60, 100, 180);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);
        private static readonly Color GlacialPurple = new Color(120, 130, 200);

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.damage = 115;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 15;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.knockBack = 4f;
            Item.value = Item.buyPrice(gold: 38);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<FrostSentinelMinion>();
            Item.buffType = ModContent.BuffType<FrostSentinelBuff>();
        }

        public override void HoldItem(Player player)
        {
            // ========== CALAMITY-STANDARD VFX PATTERN ==========
            // HEAVY DUST TRAILS - 2+ per frame (deep blue/ice dust)
            for (int d = 0; d < 2; d++)
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(26f, 26f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.IceTorch, Main.rand.NextVector2Circular(0.45f, 0.45f) + new Vector2(0, -0.35f), 0, DeepBlue, Main.rand.NextFloat(0.95f, 1.3f));
                dust.noGravity = true;
                dust.fadeIn = 1.4f;
            }
            
            // CONTRASTING SPARKLES - bright white/cyan crystalline
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(28f, 28f);
                Color sparkleColor = Main.rand.NextBool() ? FrostWhite : CrystalCyan;
                CustomParticles.PrismaticSparkle(sparklePos, sparkleColor, Main.rand.NextFloat(0.35f, 0.5f));
            }
            
            // SHIMMER TRAILS - icy motes rising with color cycling
            if (Main.rand.NextBool(3))
            {
                float hue = 0.58f + Main.rand.NextFloat(0.05f); // Deep blue range
                Color shimmerColor = Main.hslToRgb(hue, 0.6f, 0.7f);
                Vector2 shimmerPos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 shimmerVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.6f, -0.2f));
                var shimmer = new GenericGlowParticle(shimmerPos, shimmerVel, shimmerColor * 0.5f, 0.24f, 20, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }
            
            // MUSIC NOTES - visible scale with heart-beat sync
            if (Main.rand.NextBool(5))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(34f, 34f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.7f, -0.2f));
                Color noteColor = Color.Lerp(DeepBlue, IceBlue, Main.rand.NextFloat(0.5f));
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, Main.rand.NextFloat(0.85f, 1.0f), 26);
            }
            
            // ORBITING ICE MOTES - crystalline heart aura
            if (Main.rand.NextBool(4))
            {
                float orbitAngle = Main.GameUpdateCount * 0.04f + Main.rand.NextFloat(MathHelper.TwoPi);
                float orbitRadius = 36f + Main.rand.NextFloat(14f);
                Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                Color orbitColor = Color.Lerp(DeepBlue, IceBlue, Main.rand.NextFloat()) * 0.5f;
                var mote = new GenericGlowParticle(orbitPos, Vector2.Zero, orbitColor, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(mote);
            }

            // Enhanced dynamic heartbeat lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.12f + 0.5f;
            Lighting.AddLight(player.Center, DeepBlue.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Apply buff
            player.AddBuff(Item.buffType, 2);

            // Spawn sentinel around player
            float spawnAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
            Vector2 spawnPos = player.Center + spawnAngle.ToRotationVector2() * 60f;

            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);

            // ========== SENTINEL SUMMONING SPECTACULAR VFX ==========
            // MULTI-LAYER FLARES - white core → ice → deep blue
            CustomParticles.GenericFlare(spawnPos, FrostWhite, 1.0f, 24);
            CustomParticles.GenericFlare(spawnPos, IceBlue, 0.75f, 22);
            CustomParticles.GenericFlare(spawnPos, DeepBlue, 0.55f, 20);
            
            // GRADIENT HALO CASCADE - 6 layers IceBlue → DeepBlue
            for (int i = 0; i < 6; i++)
            {
                float progress = i / 5f;
                Color haloColor = Color.Lerp(IceBlue, DeepBlue, progress);
                float haloScale = 0.35f + i * 0.1f;
                int haloLife = 14 + i * 2;
                CustomParticles.HaloRing(spawnPos, haloColor * (0.7f - progress * 0.3f), haloScale, haloLife);
            }
            
            // RADIAL ICE DUST BURST - frozen materialization
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Dust iceDust = Dust.NewDustPerfect(spawnPos, DustID.IceTorch, burstVel, 0, DeepBlue, Main.rand.NextFloat(1.1f, 1.5f));
                iceDust.noGravity = true;
                iceDust.fadeIn = 1.3f;
            }
            
            // GLOW PARTICLE CORONA
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(7f, 7f);
                Color sparkColor = Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat()) * 0.55f;
                var spark = new GenericGlowParticle(spawnPos, sparkVel, sparkColor, 0.26f, 18, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // MUSIC NOTE CHORUS - summoning harmony
            for (int i = 0; i < 4; i++)
            {
                float noteAngle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(0.3f);
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(1.2f, 2.8f);
                Color noteColor = Color.Lerp(DeepBlue, IceBlue, Main.rand.NextFloat(0.5f));
                ThemedParticles.MusicNote(spawnPos, noteVel, noteColor, Main.rand.NextFloat(0.88f, 1.05f), 26);
            }
            
            // SPARKLE RING - crystalline emergence
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparklePos = spawnPos + Main.rand.NextVector2Circular(22f, 22f);
                Color sparkleColor = Main.rand.NextBool() ? FrostWhite : CrystalCyan;
                CustomParticles.PrismaticSparkle(sparklePos, sparkleColor, Main.rand.NextFloat(0.4f, 0.58f));
            }

            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, DeepBlue * 0.4f, rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, IceBlue * 0.3f, rotation, origin, scale * 1.2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, FrostWhite * 0.2f, rotation, origin, scale * 1.08f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, DeepBlue.ToVector3() * 0.5f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FrostSentinel", "Summons orbiting frost sentinels to fight for you") { OverrideColor = IceBlue });
            tooltips.Add(new TooltipLine(Mod, "CryoSynchrony", "3+ sentinels create a freezing aura around you") { OverrideColor = CrystalCyan });
            tooltips.Add(new TooltipLine(Mod, "ShatterStrike", "Critical hits shatter enemies, damaging nearby foes") { OverrideColor = FrostWhite });
            tooltips.Add(new TooltipLine(Mod, "PermafrostBond", "Sentinels gain 15% damage per frozen enemy nearby") { OverrideColor = DeepBlue });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A heart that beats with the cold of eternity'") { OverrideColor = Color.Lerp(DeepBlue, GlacialPurple, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<PermafrostBar>(), 16)
                .AddIngredient(ModContent.ItemType<WinterResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofFright, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Frost Sentinel Buff
    /// </summary>
    public class FrostSentinelBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Frostburn;
        
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<FrostSentinelMinion>()] > 0)
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
