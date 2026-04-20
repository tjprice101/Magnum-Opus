using System;
using System.Collections.Generic;
using MagnumOpus.Content.ClairDeLune;
using MagnumOpus.Content.ClairDeLune.Weapons.AutomatonsTuningFork.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.AutomatonsTuningFork
{
    public class AutomatonsTuningForkItem : ModItem
    {
        // === Frequency System ===
        /// <summary>Current frequency mode (0=A, 1=C, 2=E, 3=G). Static for minion access.</summary>
        public static int CurrentFrequency = 0;

        /// <summary>Frame timestamps of last switch to each frequency.</summary>
        private static readonly int[] _freqSwitchFrames = new int[4] { -9999, -9999, -9999, -9999 };

        /// <summary>Remaining frames of Perfect Resonance buff.</summary>
        public static int ResonanceBuffTimer = 0;

        /// <summary>True when all 4 frequencies were cycled within 600 frames (10s).</summary>
        public static bool IsResonanceActive => ResonanceBuffTimer > 0;

        private static readonly string[] FrequencyNames = { "A", "C", "E", "G" };

        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/AutomatonsTuningFork/AutomatonsTuningFork";

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.damage = 3400;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<AutomatonMinionProjectile>();
            Item.buffType = ModContent.BuffType<AutomatonsTuningForkBuff>();
        }

        public override bool AltFunctionUse(Player player) => true;

        public override void ModifyManaCost(Player player, ref float reduce, ref float mult)
        {
            if (player.altFunctionUse == 2)
                mult = 0f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Cycle frequency
                CurrentFrequency = (CurrentFrequency + 1) % 4;
                _freqSwitchFrames[CurrentFrequency] = (int)Main.GameUpdateCount;

                // Check Perfect Resonance: all 4 cycled within 600 frames (10s)
                int now = (int)Main.GameUpdateCount;
                bool allRecent = true;
                for (int i = 0; i < 4; i++)
                {
                    if (now - _freqSwitchFrames[i] > 600) { allRecent = false; break; }
                }
                if (allRecent)
                {
                    ResonanceBuffTimer = 300; // 5 seconds
                    for (int i = 0; i < 4; i++) _freqSwitchFrames[i] = -9999;
                }

                // Audio feedback — pitch rises with frequency
                SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.2f + CurrentFrequency * 0.15f }, player.Center);

                // Visual feedback
                Color freqColor = CurrentFrequency switch
                {
                    0 => ClairDeLunePalette.SoftBlue,
                    1 => ClairDeLunePalette.PearlWhite,
                    2 => ClairDeLunePalette.MoonbeamGold,
                    3 => ClairDeLunePalette.PearlBlue,
                    _ => ClairDeLunePalette.SoftBlue,
                };
                for (int i = 0; i < 8; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                    Dust d = Dust.NewDustPerfect(player.Center, DustID.WhiteTorch, dustVel, 0, freqColor, 0.7f);
                    d.noGravity = true;
                }

                return false;
            }

            player.AddBuff(Item.buffType, 2);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void HoldItem(Player player)
        {
            // Tick down resonance timer
            if (ResonanceBuffTimer > 0)
                ResonanceBuffTimer--;

            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Color col = ClairDeLunePalette.GetMoonlitGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.WhiteTorch,
                    new Vector2(0, -0.8f) + Main.rand.NextVector2Circular(0.4f, 0.4f), 0, col, 0.5f);
                d.noGravity = true;
            }

            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(player.Center, ClairDeLunePalette.SoftBlue.ToVector3() * 0.35f * pulse);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);

            spriteBatch.Draw(tex, drawPos, null, ClairDeLunePalette.SoftBlue with { A = 0 } * pulse,
                rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, ClairDeLunePalette.PearlWhite with { A = 0 } * (pulse * 0.7f),
                rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons an Automaton that fires resonance orbs at enemies"));
            tooltips.Add(new TooltipLine(Mod, "AltUse", "Right-click to cycle frequency modes"));
            tooltips.Add(new TooltipLine(Mod, "FreqA", "Frequency A: Piercing orbs with reduced speed"));
            tooltips.Add(new TooltipLine(Mod, "FreqC", "Frequency C: Swift orbs with no homing"));
            tooltips.Add(new TooltipLine(Mod, "FreqE", "Frequency E: Double orbs at 60% damage each"));
            tooltips.Add(new TooltipLine(Mod, "FreqG", "Frequency G: Orbs decelerate and detonate into damage zones"));
            tooltips.Add(new TooltipLine(Mod, "Resonance", "Perfect Resonance: cycle all 4 frequencies within 10s for 5s of enhanced orbs"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Every machine has a frequency. Find it, and the world hums with you.'")
            {
                OverrideColor = ClairDeLunePalette.LoreText
            });
        }
    }

    public class AutomatonsTuningForkBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<AutomatonMinionProjectile>()] > 0)
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
