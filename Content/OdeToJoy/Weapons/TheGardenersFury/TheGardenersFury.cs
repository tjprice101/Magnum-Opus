using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury
{
    /// <summary>
    /// The Gardener's Fury — wrath made manifest.
    /// 3-Phase Planting Combo: Sow → Cultivate → Harvest
    /// Plants seed pods that grow and detonate in cascade chain.
    /// Botanical Barrage after 3 full cycles: all pods detonate + bonus rain.
    /// </summary>
    public class TheGardenersFury : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 3200;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.crit = 25;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<GardenerSwingProj>();
            Item.shootSpeed = 1f;
            Item.scale = 1.3f;
        }

        public override bool CanUseItem(Player player)
        {
            // Prevent overlapping swings
            int projType = ModContent.ProjectileType<GardenerSwingProj>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == projType
                    && Main.projectile[i].owner == player.whoAmI)
                    return false;
            }

            var gp = player.GetModPlayer<GardenerPlayer>();

            // Check right-click for Botanical Barrage
            if (Main.mouseRight && Main.mouseRightRelease && gp.BarrageReady)
            {
                TriggerBarrage(player, gp);
                return false;
            }

            // Adjust timing per combo phase
            switch (gp.ComboPhase)
            {
                case 0: // Sow — standard sweep
                    Item.useTime = 22;
                    Item.useAnimation = 22;
                    break;
                case 1: // Cultivate — wider, slightly slower
                    Item.useTime = 24;
                    Item.useAnimation = 24;
                    break;
                case 2: // Harvest — heavy overhead slam
                    Item.useTime = 30;
                    Item.useAnimation = 30;
                    break;
            }

            return true;
        }

        private void TriggerBarrage(Player player, GardenerPlayer gp)
        {
            // Detonate ALL active seed pods
            int podType = ModContent.ProjectileType<SeedPodProjectile>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active
                    && Main.projectile[i].type == podType
                    && Main.projectile[i].owner == player.whoAmI)
                {
                    // Force detonate via ai[2] flag
                    Main.projectile[i].ai[2] = 1f;
                }
            }

            // Spawn 8 bonus rain pods from above
            var source = player.GetSource_ItemUse(Item);
            for (int i = 0; i < 8; i++)
            {
                Vector2 spawnPos = player.Center + new Vector2(
                    Main.rand.NextFloat(-300f, 300f),
                    -Main.rand.NextFloat(400f, 600f));
                Vector2 targetPos = player.Center + new Vector2(
                    Main.rand.NextFloat(-200f, 200f),
                    Main.rand.NextFloat(-50f, 50f));
                Vector2 rainVel = (targetPos - spawnPos).SafeNormalize(Vector2.UnitY) * 14f;

                int podVariant = Main.rand.Next(3); // random type
                Projectile.NewProjectile(source, spawnPos, rainVel,
                    ModContent.ProjectileType<SeedPodProjectile>(),
                    (int)(Item.damage * 0.7f), Item.knockBack * 0.6f,
                    player.whoAmI, ai0: podVariant, ai1: 1f); // ai[1]=1 = rain pod (detonate on landing)
            }

            gp.ConsumeBarrage();

            SoundEngine.PlaySound(SoundID.Item119 with { Volume = 0.9f, Pitch = -0.3f },
                player.Center);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var gp = player.GetModPlayer<GardenerPlayer>();
            int phase = gp.ComboPhase;
            Vector2 aimDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);

            // Spawn swing projectile: ai[0] = combo phase
            Projectile.NewProjectile(source, player.MountedCenter, aimDir, type,
                damage, knockback, player.whoAmI, ai0: phase);

            // Spawn seed pods based on phase
            int podType = gp.GetCurrentPodType();

            if (phase == 0)
            {
                // Phase 1 — Sow: 2 seed pods at swing endpoints
                for (int i = -1; i <= 1; i += 2)
                {
                    Vector2 podPos = player.MountedCenter + aimDir.RotatedBy(i * 0.4f) * 100f;
                    Projectile.NewProjectile(source, podPos, Vector2.Zero,
                        ModContent.ProjectileType<SeedPodProjectile>(),
                        (int)(damage * 0.5f), knockback * 0.3f,
                        player.whoAmI, ai0: podType);
                }
            }
            else if (phase == 1)
            {
                // Phase 2 — Cultivate: 3 pods in wider arc
                for (int i = -1; i <= 1; i++)
                {
                    Vector2 podPos = player.MountedCenter + aimDir.RotatedBy(i * 0.5f) * 130f;
                    Projectile.NewProjectile(source, podPos, Vector2.Zero,
                        ModContent.ProjectileType<SeedPodProjectile>(),
                        (int)(damage * 0.5f), knockback * 0.3f,
                        player.whoAmI, ai0: podType);
                }

                // Make existing pods pulse brighter — handled by pod AI checking phase
            }
            else if (phase == 2)
            {
                // Phase 3 — Harvest: detonate all pods in cascade
                int podProjType = ModContent.ProjectileType<SeedPodProjectile>();
                int cascadeDelay = 0;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active
                        && Main.projectile[i].type == podProjType
                        && Main.projectile[i].owner == player.whoAmI)
                    {
                        // Stagger detonation using localAI[0] as delay timer
                        Main.projectile[i].localAI[0] = cascadeDelay * 6; // 0.1s between each
                        Main.projectile[i].ai[2] = 1f; // trigger detonation
                        cascadeDelay++;
                    }
                }

                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.2f, Volume = 0.6f },
                    player.MountedCenter);
            }

            gp.AdvanceCombo();
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "3-phase planting combo: Sow seed pods → Cultivate → Harvest (cascade detonation)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Seed pod types cycle: Bloom (petal burst), Thorn (shrapnel), Pollen (slow cloud)"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Pods left undetonated for 2+ seconds grow larger and deal 20% more damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "Right-click after 3 full combos triggers Botanical Barrage — mass detonation + pod rain"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Plant in silence. Harvest in thunder.'")
            {
                OverrideColor = GardenerFuryTextures.LoreColor
            });
        }
    
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2.2f) * 0.05f
                + (float)Math.Sin(time * 3.8f) * 0.03f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            OdeToJoyPalette.DrawItemBloom(spriteBatch, tex, pos, origin, rotation, scale, pulse);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.35f);
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            float time = Main.GameUpdateCount * 0.04f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.06f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            float cycle = (float)Math.Sin(time * 0.7f) * 0.5f + 0.5f;
            Color glowColor = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.RosePink, cycle) * 0.24f;
            spriteBatch.Draw(tex, position, frame, glowColor with { A = 0 }, 0f, origin, scale * pulse * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}