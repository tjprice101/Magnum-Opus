using MagnumOpus.Common;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Utilities;
using MagnumOpus.Content.SandboxExoblade.Utilities;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury
{
    /// <summary>
    /// The Gardener's Fury — Ode to Joy's nature fury melee weapon.
    /// Exoblade-architecture melee with leaf green slash arcs and sun gold accents.
    /// </summary>
    public class TheGardenersFury : ModItem, IOverdriveItem
    {
        public IResonantOverdrive GetOverdrivePlayer(Player player) => player.GetModPlayer<TheGardenersFuryPlayer>();

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.scale = 0.09f;
            Item.damage = 270;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useTurn = true;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.knockBack = 7.5f;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;
            Item.value = Item.sellPrice(gold: 45);
            Item.shoot = ModContent.ProjectileType<GardenerFuryProjectile>();
            Item.shootSpeed = 8f;
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override bool CanShoot(Player player)
        {
            bool isDash = player.altFunctionUse == 2;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != player.whoAmI || p.type != Item.shoot)
                    continue;
                if (isDash) return false;
                if (!(p.ai[0] == 1 && p.ai[1] == 1)) return false;
            }
            return true;
        }

        public override void HoldItem(Player player)
        {
            player.ExoBlade().rightClickListener = true;
            player.ExoBlade().mouseWorldListener = true;
            player.GetModPlayer<TheGardenersFuryPlayer>().IsHoldingTheGardenersFury = true;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Right-click: charge-gated special — spawn 5 light pillars at nearby enemy positions
            if (player.altFunctionUse == 2)
            {
                var gfp = player.GetModPlayer<TheGardenersFuryPlayer>();

                if (gfp.IsChargeFull)
                {
                    gfp.ConsumeCharge();
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.9f }, player.MountedCenter);

                    // Gather up to 5 nearby enemy positions
                    int pillarCount = 5;
                    int spawned = 0;

                    foreach (NPC npc in Main.ActiveNPCs)
                    {
                        if (spawned >= pillarCount) break;
                        if (!npc.CanBeChasedBy()) continue;
                        if (Vector2.Distance(player.MountedCenter, npc.Center) > 800f) continue;

                        Projectile.NewProjectile(source, npc.Center, Vector2.Zero,
                            ModContent.ProjectileType<GardenerFurySpecialProj>(),
                            (int)(damage * 1.5f), knockback, player.whoAmI);
                        spawned++;
                    }

                    // If fewer than 5 enemies found, spawn remaining pillars at mouse cursor area
                    for (int i = spawned; i < pillarCount; i++)
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(100f, 100f);
                        Projectile.NewProjectile(source, Main.MouseWorld + offset, Vector2.Zero,
                            ModContent.ProjectileType<GardenerFurySpecialProj>(),
                            (int)(damage * 1.5f), knockback, player.whoAmI);
                    }
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.3f, Volume = 0.4f }, player.MountedCenter);
                }
                return false;
            }

            // Normal left-click: spawn swing projectile
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, 0f, 0);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "Nature's Fury — verdant slashes that scatter leaves and golden sparks"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Right-click to dash through enemies with a leaf-wreathed blade charge"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Successive combos build Gardener's Wrath for escalating damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'Plant in silence. Harvest in thunder.'")
            { OverrideColor = OdeToJoyPalette.LoreText });
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
            spriteBatch.Draw(tex, drawPos, null, OdeToJoyPalette.VerdantGreen with { A = 0 } * pulse,
                rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, OdeToJoyPalette.SunlightYellow with { A = 0 } * (pulse * 0.5f),
                rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);
        }
    }
}
