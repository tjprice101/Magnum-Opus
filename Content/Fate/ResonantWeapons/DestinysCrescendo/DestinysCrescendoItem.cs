using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Fate.ResonantWeapons.DestinysCrescendo
{
    /// <summary>
    /// Destiny's Crescendo — The rising peak of fate's symphony.
    /// Endgame Fate summoner weapon that summons a cosmic deity minion.
    /// 
    /// Stats: 400 damage, Summon, useTime/Animation 36, knockBack 3f, mana 20
    /// Sells for 55g. FateRarity. Spawns CrescendoDeityMinion, gives CrescendoDeityBuff.
    /// 
    /// Texture: MagnumOpus/Content/Fate/ResonantWeapons/DestinysCrescendo (existing .png)
    /// ZERO shared VFX system references.
    /// </summary>
    public class DestinysCrescendoItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/DestinysCrescendo";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 400;
            Item.DamageType = DamageClass.Summon;
            Item.width = 44;
            Item.height = 44;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.mana = 20;
            Item.shoot = ModContent.ProjectileType<CrescendoDeityMinion>();
            Item.buffType = ModContent.BuffType<CrescendoDeityBuff>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons a cosmic deity minion with 4-phase Musical Escalation"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Pianissimo, Piano, Forte, Fortissimo — escalates every 15 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Beam volleys per phase: 1 / 2 / 3 / 5, with decreasing cooldowns"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Deity scales in size from 1.0x to 1.5x as escalation builds"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Deity Presence grants passive damage, regen, and defense based on phase"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Taking heavy damage (>200) resets escalation to Pianissimo"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The symphony of fate plays softly at first. By the finale, it shakes the heavens.'")
            {
                OverrideColor = new Color(180, 40, 80)
            });
        }

        public override void HoldItem(Player player)
        {
            if (Main.dedServ) return;

            player.Crescendo().IsHoldingStaff = true;

            // Ambient hold VFX — orbiting glyphs and star sparkles around the wielder
            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // 4-point divine constellation
            if (Main.rand.NextBool(8))
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = time * 0.025f + MathHelper.TwoPi * i / 4f;
                    float radius = 40f + System.MathF.Sin(time * 0.03f + i * 0.7f) * 6f;
                    Vector2 pos = center + CrescendoUtils.HelixOffset(angle, radius);
                    Color col = CrescendoUtils.PaletteLerp((float)i / 4f);
                    CrescendoParticleHandler.Spawn(CrescendoParticleFactory.OrbGlow(pos,
                        Main.rand.NextVector2Circular(0.3f, 0.3f), col * 0.4f, 0.14f, 16));
                }
            }

            // Star sparkles
            if (Main.rand.NextBool(9))
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(30f, 30f);
                Color sparkCol = Main.rand.NextBool(3) ? CrescendoUtils.StarGold : CrescendoUtils.CelestialWhite;
                CrescendoParticleHandler.Spawn(CrescendoParticleFactory.DivineSpark(sparkPos,
                    Main.rand.NextVector2Circular(0.4f, 0.4f), sparkCol * 0.35f, 0.1f, 14));
            }

            // Cosmic notes — destiny's melody builds
            if (Main.rand.NextBool(15))
                CrescendoParticleFactory.SpawnCosmicNotes(center, 1, 15f);

            // Divine light pulse
            float pulse = 0.25f + System.MathF.Sin(time * 0.06f) * 0.1f;
            Lighting.AddLight(center, CrescendoUtils.CrescendoPink.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            // Spawn deity at cursor
            Vector2 spawnPos = Main.MouseWorld;
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);

            // Track summon intensity
            player.Crescendo().OnSummon();

            // === SUMMON EXPLOSION VFX ===
            if (!Main.dedServ)
            {
                CrescendoParticleFactory.SpawnSummonExplosion(spawnPos);
                Lighting.AddLight(spawnPos, CrescendoUtils.StarGold.ToVector3() * 1.0f);
            }

            return false;
        }
    }
}
