using MagnumOpus.Common;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.SandboxExoblade.Utilities;
using MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Projectiles;
using MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Utilities;
using MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Projectiles;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance
{
    /// <summary>
    /// Twilight Severance — Nachtmusik's dimensional katana. Exoblade-architecture weapon item.
    /// Severs the boundary between dusk and starlight with indigo-silver slash arcs.
    /// </summary>
    public class TwilightSeverance : ModItem, IOverdriveItem
    {
        public IResonantOverdrive GetOverdrivePlayer(Player player) => player.GetModPlayer<TwilightSeverancePlayer>();

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.scale = 0.09f;
            Item.damage = 280;
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
            Item.value = Item.sellPrice(gold: 40);
            Item.shoot = ModContent.ProjectileType<TwilightSeveranceSwing>();
            Item.shootSpeed = 8f;
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override bool CanShoot(Player player)
        {
            if (player.altFunctionUse == 2)
                return true;

            return player.ownedProjectileCounts[Item.shoot] <= 0;
        }

        public override void HoldItem(Player player)
        {
            player.ExoBlade().rightClickListener = true;
            player.ExoBlade().mouseWorldListener = true;
            player.GetModPlayer<TwilightSeverancePlayer>().IsHoldingTwilightSeverance = true;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                var tp = player.GetModPlayer<TwilightSeverancePlayer>();
                if (tp.IsChargeFull)
                {
                    tp.ConsumeCharge();
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.3f }, player.Center);

                    // Spawn VoidRift stationary zone at cursor position
                    Vector2 toCursor = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero,
                        ModContent.ProjectileType<VoidRiftProjectile>(),
                        damage * 2, knockback, player.whoAmI, toCursor.ToRotation());

                    // Also spawn marker projs on nearby NPCs
                    foreach (NPC npc in Main.ActiveNPCs)
                    {
                        if (!npc.CanBeChasedBy() || Vector2.Distance(npc.Center, player.Center) > 800f) continue;
                        Projectile.NewProjectile(source, npc.Center, Vector2.Zero,
                            ModContent.ProjectileType<TwilightSeveranceSpecialProj>(),
                            damage * 2, knockback, player.whoAmI);
                    }
                }
                else
                    SoundEngine.PlaySound(SoundID.Item16 with { Pitch = 0.5f, Volume = 0.5f }, player.Center);
                return false;
            }

            // Fire swing projectile
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, 0f, 0);

            // Also fire 1 NocturnalBladeProjectile homing orb alongside the swing
            Vector2 orbDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            Projectile.NewProjectile(source, player.MountedCenter, orbDir * 14f,
                ModContent.ProjectileType<NocturnalBladeProjectile>(),
                (int)(damage * 0.6f), knockback * 0.5f, player.whoAmI);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "Dimensional katana that severs the boundary between dusk and starlight"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Right-click performs a stellar dash strike through enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Swings leave trails of indigo starlight and silver sparks"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'Between dusk and starlight, every cut severs what was from what will be.'")
            { OverrideColor = new Color(100, 120, 200) });
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
            spriteBatch.Draw(tex, drawPos, null, NachtmusikPalette.DuskViolet with { A = 0 } * pulse,
                rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, NachtmusikPalette.MoonlitSilver with { A = 0 } * (pulse * 0.5f),
                rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);
        }
    }
}
