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
            // Gentle spring aura
            if (Main.rand.NextBool(8))
            {
                Vector2 motePos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 moteVel = new Vector2(0, -Main.rand.NextFloat(0.4f, 1f));
                Color moteColor = Color.Lerp(SpringPink, SpringYellow, Main.rand.NextFloat()) * 0.5f;
                var mote = new GenericGlowParticle(motePos, moteVel, moteColor, 0.2f, 30, true);
                MagnumParticleHandler.SpawnParticle(mote);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 0.4f;
            Lighting.AddLight(player.Center, SpringYellow.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Apply buff
            player.AddBuff(Item.buffType, 2);

            // Spawn VFX
            CustomParticles.GenericFlare(Main.MouseWorld, SpringYellow, 0.7f, 20);
            CustomParticles.HaloRing(Main.MouseWorld, SpringGreen * 0.6f, 0.45f, 18);
            
            // Bloom burst on summon
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color burstColor = Color.Lerp(SpringPink, SpringYellow, (float)i / 8f);
                var burst = new GenericGlowParticle(Main.MouseWorld, burstVel, burstColor * 0.7f, 0.3f, 22, true);
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
