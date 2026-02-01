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
            // Ambient solar aura
            if (Main.rand.NextBool(8))
            {
                Vector2 auraPos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 auraVel = new Vector2(0, -Main.rand.NextFloat(0.5f, 1.5f));
                Color auraColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat()) * 0.4f;
                var aura = new GenericGlowParticle(auraPos, auraVel, auraColor, 0.22f, 28, true);
                MagnumParticleHandler.SpawnParticle(aura);
            }

            // Floating summer melody notes
            if (Main.rand.NextBool(12))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(38f, 38f);
                Vector2 noteVel = new Vector2(0, -Main.rand.NextFloat(0.3f, 0.7f));
                Color noteColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat()) * 0.6f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.75f, 40);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.045f) * 0.12f + 0.5f;
            Lighting.AddLight(player.Center, SunGold.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Apply buff
            player.AddBuff(Item.buffType, 2);

            // Spawn VFX - layered solar bloom instead of halo
            CustomParticles.GenericFlare(Main.MouseWorld, SunGold, 0.75f, 22);
            CustomParticles.GenericFlare(Main.MouseWorld, SunOrange * 0.6f, 0.55f, 18);
            CustomParticles.GenericFlare(Main.MouseWorld, SunOrange * 0.4f, 0.4f, 15);
            
            // Solar ray burst
            for (int ray = 0; ray < 6; ray++)
            {
                float rayAngle = MathHelper.TwoPi * ray / 6f;
                Vector2 rayPos = Main.MouseWorld + rayAngle.ToRotationVector2() * 20f;
                CustomParticles.GenericFlare(rayPos, SunOrange * 0.75f, 0.25f, 13);
            }

            // Music note on summon
            ThemedParticles.MusicNote(Main.MouseWorld, Vector2.Zero, SunGold * 0.8f, 0.7f, 25);

            // Music note ring and burst for summon effect
            ThemedParticles.MusicNoteRing(Main.MouseWorld, SunGold, 40f, 6);
            ThemedParticles.MusicNoteBurst(Main.MouseWorld, SunOrange, 5, 4f);

            // Sparkle accents
            for (int i = 0; i < 4; i++)
            {
                var sparkle = new SparkleParticle(Main.MouseWorld + Main.rand.NextVector2Circular(12f, 12f),
                    Main.rand.NextVector2Circular(2f, 2f), SunWhite * 0.5f, 0.2f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Solar burst on summon
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color burstColor = Color.Lerp(SunGold, SunRed, (float)i / 10f);
                var burst = new GenericGlowParticle(Main.MouseWorld, burstVel, burstColor * 0.7f, 0.35f, 24, true);
                MagnumParticleHandler.SpawnParticle(burst);
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
