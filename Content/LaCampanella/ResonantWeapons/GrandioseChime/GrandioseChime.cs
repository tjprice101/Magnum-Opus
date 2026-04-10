using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Projectiles;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime
{
    /// <summary>
    /// GrandioseChime — Ranged beam + mine weapon.
    /// Primary: wide golden beam that kills trigger Kill Echo Chains (3 chains max).
    /// Alt-fire: deploy floating bell-note mines (max 5).
    /// Grandiose Crescendo: after 5 full chain kills, next beam is triple width + deploys 3 auto mines.
    /// </summary>
    public class GrandioseChimeItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/GrandioseChime/GrandioseChime";
        public override string Name => "GrandioseChime";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 240;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 58;
            Item.height = 28;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.UseSound = SoundID.Item33;
            Item.shoot = ProjectileID.Bullet;
            Item.useAmmo = AmmoID.Bullet;
            Item.shootSpeed = 20f;
            Item.noMelee = true;
            Item.autoReuse = true;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Alt-fire: deploy note mine. Max 5 deployed at once.
                int mineCount = player.ownedProjectileCounts[ModContent.ProjectileType<NoteMineProj>()];
                return mineCount < 5;
            }
            return base.CanUseItem(player);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useTime = 30;
                Item.useAnimation = 30;
            }
            else
            {
                Item.useTime = 18;
                Item.useAnimation = 18;
                velocity = velocity.RotatedByRandom(MathHelper.ToRadians(1.5f));
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 muzzlePos = position + Vector2.Normalize(velocity) * 50f;

            if (player.altFunctionUse == 2)
            {
                // Alt-fire: deploy a note mine at cursor
                Vector2 mineVel = velocity * 0.2f;
                Projectile.NewProjectile(source, muzzlePos, mineVel,
                    ModContent.ProjectileType<NoteMineProj>(), (int)(damage * 0.8f), knockback, player.whoAmI);
                return false;
            }

            // Primary fire: golden beam
            var modPlayer = player.GetModPlayer<GrandioseChimePlayer>();

            // Check Grandiose Crescendo
            bool isCrescendo = modPlayer.GrandioseCrescendoReady;
            if (isCrescendo)
                modPlayer.ConsumeGrandioseCrescendo();

            // Muzzle flash
            float angle = velocity.ToRotation();
            GrandioseChimeParticleHandler.SpawnParticle(new GrandioseBeamFlashParticle(
                muzzlePos, angle, Main.rand.NextFloat(40f, 60f), Main.rand.Next(5, 10)));

            // Fire beam — ai[0]=1 if Grandiose Crescendo
            Projectile.NewProjectile(source, muzzlePos, velocity,
                ModContent.ProjectileType<GrandioseBeamProj>(), isCrescendo ? (int)(damage * 1.5f) : damage,
                knockback, player.whoAmI, ai0: isCrescendo ? 1f : 0f);

            // Grandiose Crescendo: auto-deploy 3 mines along beam path
            if (isCrescendo)
            {
                Vector2 beamDir = Vector2.Normalize(velocity);
                for (int i = 1; i <= 3; i++)
                {
                    Vector2 minePos = muzzlePos + beamDir * (100f * i) + Main.rand.NextVector2Circular(20f, 20f);
                    Projectile.NewProjectile(source, minePos, Vector2.Zero,
                        ModContent.ProjectileType<NoteMineProj>(), (int)(damage * 0.8f), knockback, player.whoAmI);
                }
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires wide golden beam shots that trigger Kill Echo Chains on kill"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Kill Echo chains to the nearest enemy within 15 tiles at 60% damage, up to 3 chains"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Right click to deploy floating bell-note mines (max 5)"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "After 5 complete kill chains, the next beam becomes Grandiose: triple width + 3 auto mines"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The bell speaks, and the world answers in fire.'")
            {
                OverrideColor = new Color(255, 140, 40)
            });
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
            spriteBatch.Draw(tex, drawPos, null, new Color(255, 140, 40, 0) * pulse,
                rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, new Color(255, 200, 60, 0) * (pulse * 0.7f),
                rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);
        }
    }
}
