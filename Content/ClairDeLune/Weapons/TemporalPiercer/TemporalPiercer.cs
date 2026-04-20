using MagnumOpus.Common;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.ClairDeLune;
using MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer.Projectiles;
using MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer.Utilities;
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

namespace MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer
{
    /// <summary>
    /// Temporal Piercer — Clair de Lune's precision rapier that punctures time itself.
    /// Exoblade-architecture weapon with frost shimmer VFX.
    /// </summary>
    public class TemporalPiercer : ModItem, IOverdriveItem
    {
        public IResonantOverdrive GetOverdrivePlayer(Player player) => player.GetModPlayer<TemporalPiercerPlayer>();

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 65;
            Item.height = 65;
            Item.scale = 0.09f;
            Item.damage = 260;
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
            Item.shoot = ModContent.ProjectileType<TemporalThrustProjectile>();
            Item.shootSpeed = 8f;
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
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
            player.GetModPlayer<TemporalPiercerPlayer>().IsHoldingTemporalPiercer = true;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                var cp = player.GetModPlayer<TemporalPiercerPlayer>();
                if (cp.IsChargeFull)
                {
                    cp.ConsumeCharge();
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.4f }, player.Center);

                    // Find nearest enemy
                    NPC target = null;
                    float closestDist = 800f;
                    foreach (NPC npc in Main.ActiveNPCs)
                    {
                        if (!npc.CanBeChasedBy()) continue;
                        float dist = Vector2.Distance(player.Center, npc.Center);
                        if (dist < closestDist) { closestDist = dist; target = npc; }
                    }
                    if (target != null)
                    {
                        player.Teleport(target.Center - new Vector2(0, 40), -1);
                        Projectile.NewProjectile(source, target.Center, Vector2.Zero,
                            ModContent.ProjectileType<TemporalPiercerSpecialProj>(),
                            (int)(damage * 1.5f), knockback, player.whoAmI);
                        player.statLife = System.Math.Min(player.statLife + player.statLifeMax2 * 15 / 100, player.statLifeMax2);
                        player.HealEffect(player.statLifeMax2 * 15 / 100);

                        // Temporal Rift orb — aggressive homing, spawns damage zone on impact
                        Vector2 orbVel = (target.Center - player.Center).SafeNormalize(Vector2.UnitX) * 12f;
                        GenericHomingOrbChild.SpawnChild(
                            source, target.Center, orbVel,
                            (int)(damage * 1.2f), knockback, player.whoAmI,
                            homingStrength: 0.10f,
                            behaviorFlags: GenericHomingOrbChild.FLAG_ZONE_ON_KILL,
                            themeIndex: GenericHomingOrbChild.THEME_CLAIRDELUNE,
                            scaleMult: 1.2f);
                    }
                }
                else
                    SoundEngine.PlaySound(SoundID.Item16 with { Pitch = 0.5f, Volume = 0.5f }, player.Center);
                return false;
            }

            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, 0f, 0);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "Temporal Puncture — ultra-precise rapier thrusts that pierce the veil of time"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Each hit spawns a temporal echo orb that reverses direction and homes to enemies at 60% damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Right-click to perform a Time-Pierce Lunge that fires a rift orb creating a slowing damage zone"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'Five marks upon the hours. And when the fifth chimes \u2014 the moment freezes.'")
            { OverrideColor = ClairDeLunePalette.LoreText });
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
            spriteBatch.Draw(tex, drawPos, null, ClairDeLunePalette.TemporalCrimson with { A = 0 } * pulse,
                rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, ClairDeLunePalette.PearlBlue with { A = 0 } * (pulse * 0.5f),
                rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);
        }
    }
}
