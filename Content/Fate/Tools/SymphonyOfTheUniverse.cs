using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using ReLogic.Content;
using MagnumOpus.Content.Fate.ResonanceEnergies;
using MagnumOpus.Content.Fate.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Fate.Tools
{
    /// <summary>
    /// Symphony of the Universe - Fate wings, the ultimate endgame wings.
    /// Features cosmic dark prismatic effects, reality distortions, and chromatic aberration.
    /// Uses 6x6 sprite sheet for flying animation.
    /// </summary>
    [AutoloadEquip(EquipType.Wings)]
    public class SymphonyOfTheUniverse : ModItem
    {
        public override void SetStaticDefaults()
        {
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(
                flyTime: 400, // Ultimate tier (higher than Enigma 360)
                flySpeedOverride: 20f, // Ultimate speed (higher than Enigma 18)
                accelerationMultiplier: 4.2f, // Ultimate acceleration (higher than Enigma 3.8)
                hasHoldDownHoverFeatures: true
            );
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 8);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Press K to amplify your HP hearts with musical resonance, doubling your effective HP for 35 seconds (5 minute cooldown)") { OverrideColor = new Color(200, 80, 120) });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Wings woven from the fabric of cosmic destiny itself'") { OverrideColor = new Color(180, 40, 80) });
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
            ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            ascentWhenFalling = 1.4f;
            ascentWhenRising = 0.28f;
            maxCanAscendMultiplier = 1.6f;
            maxAscentMultiplier = 4.8f;
            constantAscend = 0.24f;
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<SymphonyOfTheUniversePlayer>().hasWingsEquipped = true;
            player.GetModPlayer<WingAmplificationPlayer>().hasFateWings = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<FateResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfFate>(), 8)
                .AddIngredient(ItemID.SoulofFlight, 50)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class SymphonyOfTheUniversePlayer : ModPlayer
    {
        public int wingFrame = 0;
        private int frameCounter = 0;
        private bool wasFlying = false;
        
        public bool hasWingsEquipped = false;

        public override void ResetEffects()
        {
            hasWingsEquipped = false;
        }

        public override void PostUpdate()
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "SymphonyOfTheUniverse", EquipType.Wings);
            bool hasWings = Player.wings == wingSlot && wingSlot > 0;

            if (!hasWings && !hasWingsEquipped)
            {
                wingFrame = 0;
                frameCounter = 0;
                wasFlying = false;
                return;
            }

            bool isFlying = Player.controlJump && Player.velocity.Y != 0 && !Player.mount.Active;
            bool isOnGround = Player.velocity.Y == 0;
            
            if (isFlying)
            {
                frameCounter++;
                if (frameCounter >= 2)
                {
                    frameCounter = 0;
                    wingFrame++;
                    if (wingFrame >= 36)
                        wingFrame = 0;
                }
                wasFlying = true;
            }
            else if (isOnGround)
            {
                wingFrame = 0;
                frameCounter = 0;
                wasFlying = false;
            }
            else if (wasFlying && !isFlying)
            {
                wingFrame = 0;
                frameCounter = 0;
            }
        }
        
        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "SymphonyOfTheUniverse", EquipType.Wings);
            if (Player.wings == wingSlot && wingSlot > 0)
            {
                PlayerDrawLayers.Wings.Hide();
            }
        }
    }
    
    /// <summary>
    /// Custom draw layer for Symphony of the Universe.
    /// Uses 6x6 sprite sheet for all animation.
    /// Includes subtle chromatic aberration effect for cosmic feel.
    /// </summary>
    public class SymphonyOfTheUniverseLayer : PlayerDrawLayer
    {
        private Asset<Texture2D> _animatedTexture;

        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.BackAcc);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "SymphonyOfTheUniverse", EquipType.Wings);
            return drawInfo.drawPlayer.wings == wingSlot && wingSlot > 0 && !drawInfo.drawPlayer.dead && !drawInfo.drawPlayer.invis;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            var modPlayer = player.GetModPlayer<SymphonyOfTheUniversePlayer>();

            _animatedTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Content/Fate/Tools/SymphonyOfTheUniverse_Wings");
            if (_animatedTexture.State != AssetState.Loaded)
                return;

            Texture2D tex = _animatedTexture.Value;
            int cols = 6;
            int rows = 6;
            int frameW = tex.Width / cols;
            int frameH = tex.Height / rows;

            int frame = modPlayer.wingFrame;
            int col = frame % cols;
            int row = frame / cols;

            Rectangle source = new Rectangle(col * frameW, row * frameH, frameW, frameH);

            Vector2 pos = drawInfo.Position - Main.screenPosition + new Vector2(player.width / 2f, player.height / 2f);
            pos.Y += player.gfxOffY;
            pos = pos.Floor();

            SpriteEffects fx = player.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (player.gravDir == -1)
                fx |= SpriteEffects.FlipVertically;

            Color color = drawInfo.colorArmorBody;

            Vector2 origin = new Vector2(source.Width / 2f, source.Height / 2f);

            // Subtle chromatic aberration effect for cosmic feel when moving
            if (Math.Abs(player.velocity.X) > 2f || Math.Abs(player.velocity.Y) > 2f)
            {
                float aberrationOffset = 1.5f;
                // Red channel offset
                DrawData redData = new DrawData(tex, pos + new Vector2(-aberrationOffset, 0), source, 
                    new Color(255, 80, 100) * 0.15f, player.bodyRotation, origin, 0.339f, fx, 0);
                drawInfo.DrawDataCache.Add(redData);
                
                // Blue channel offset
                DrawData blueData = new DrawData(tex, pos + new Vector2(aberrationOffset, 0), source, 
                    new Color(100, 80, 200) * 0.15f, player.bodyRotation, origin, 0.339f, fx, 0);
                drawInfo.DrawDataCache.Add(blueData);
            }

            // Main wing draw
            DrawData data = new DrawData(tex, pos, source, color, player.bodyRotation, origin, 0.339f, fx, 0);
            drawInfo.DrawDataCache.Add(data);
        }
    }
}
