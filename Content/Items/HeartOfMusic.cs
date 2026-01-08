using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Items
{
    /// <summary>
    /// Heart of Music - Consumable item that unlocks the Harmonic Core accessory slots.
    /// Guaranteed drop from Moon Lord for each player.
    /// </summary>
    public class HeartOfMusic : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = true;
            Item.maxStack = 1;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(platinum: 1);
            Item.UseSound = SoundID.Item4;
        }

        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<HarmonicCoreModPlayer>();
            // Can only use if not already unlocked
            return !modPlayer.HasUnlockedHarmonicSlots;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                var modPlayer = player.GetModPlayer<HarmonicCoreModPlayer>();
                modPlayer.HasUnlockedHarmonicSlots = true;
                modPlayer.HasKilledMoonLord = true; // Also mark Moon Lord as killed

                // Dramatic unlock effect
                SoundEngine.PlaySound(SoundID.Item119, player.Center); // Celestial sound
                SoundEngine.PlaySound(SoundID.Item4, player.Center);

                // Particle explosion
                for (int i = 0; i < 60; i++)
                {
                    // Rainbow particles
                    Color particleColor = Main.hslToRgb(i / 60f, 1f, 0.6f);
                    Dust dust = Dust.NewDustDirect(player.Center - new Vector2(20, 20), 40, 40,
                        DustID.RainbowMk2, Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f), 0, particleColor, 1.5f);
                    dust.noGravity = true;
                }

                // White shimmer ring
                for (int i = 0; i < 30; i++)
                {
                    float angle = MathHelper.TwoPi * i / 30f;
                    Vector2 velocity = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * 6f;
                    Dust ring = Dust.NewDustDirect(player.Center, 1, 1, DustID.SparksMech, velocity.X, velocity.Y, 0, Color.White, 1.2f);
                    ring.noGravity = true;
                }

                Main.NewText("The Heart of Music resonates within you...", 255, 200, 255);
                Main.NewText("Harmonic Core slots have been unlocked!", 200, 255, 200);

                return true;
            }

            return false;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            var player = Main.LocalPlayer;
            if (player != null)
            {
                var modPlayer = player.GetModPlayer<HarmonicCoreModPlayer>();
                if (modPlayer.HasUnlockedHarmonicSlots)
                {
                    tooltips.Add(new TooltipLine(Mod, "AlreadyUnlocked", "You have already unlocked Harmonic Core slots")
                    {
                        OverrideColor = Color.Gray
                    });
                }
                else
                {
                    tooltips.Add(new TooltipLine(Mod, "UnlockInfo", "Consume to unlock 3 Harmonic Core accessory slots")
                    {
                        OverrideColor = new Color(255, 220, 255)
                    });
                    tooltips.Add(new TooltipLine(Mod, "UnlockInfo2", "Harmonic Cores provide powerful buffs and effects")
                    {
                        OverrideColor = new Color(200, 200, 255)
                    });
                }
            }

            // Flavor text
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The crystallized essence of cosmic harmony'")
            {
                OverrideColor = new Color(180, 180, 200)
            });
        }

        public override void PostUpdate()
        {
            // Glowing effect when dropped
            Lighting.AddLight(Item.Center, 0.8f, 0.6f, 0.9f);

            // Occasional sparkle
            if (Main.rand.NextBool(10))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height,
                    DustID.RainbowMk2, 0f, -1f, 0, Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.7f), 0.8f);
                sparkle.noGravity = true;
            }
        }
    }
}
