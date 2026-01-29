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
            // Frozen heart pulse effect
            if (Main.rand.NextBool(10))
            {
                Vector2 heartPos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 heartVel = new Vector2(0, Main.rand.NextFloat(-1f, -0.3f));
                Color heartColor = Color.Lerp(DeepBlue, IceBlue, Main.rand.NextFloat()) * 0.35f;
                var heart = new GenericGlowParticle(heartPos, heartVel, heartColor, 0.2f, 28, true);
                MagnumParticleHandler.SpawnParticle(heart);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 0.35f;
            Lighting.AddLight(player.Center, DeepBlue.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Apply buff
            player.AddBuff(Item.buffType, 2);

            // Spawn sentinel around player
            float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
            Vector2 spawnPos = player.Center + angle.ToRotationVector2() * 60f;

            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);

            // Summon VFX
            CustomParticles.GenericFlare(spawnPos, FrostWhite, 0.85f, 25);
            CustomParticles.HaloRing(spawnPos, IceBlue * 0.6f, 0.55f, 20);
            CustomParticles.HaloRing(spawnPos, DeepBlue * 0.4f, 0.4f, 16);

            for (int i = 0; i < 10; i++)
            {
                float sparkAngle = MathHelper.TwoPi * i / 10f;
                Vector2 sparkVel = sparkAngle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color sparkColor = Color.Lerp(IceBlue, CrystalCyan, (float)i / 10f) * 0.55f;
                var spark = new GenericGlowParticle(spawnPos, sparkVel, sparkColor, 0.28f, 20, true);
                MagnumParticleHandler.SpawnParticle(spark);
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
