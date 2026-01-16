using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
            
            // Cosmic fate particles when flying
            if (!hideVisual && player.velocity.Y != 0)
            {
                ThemedParticles.FateAura(player.Center, 40f);
                
                // Dark prismatic cosmic wisps
                if (Main.rand.NextBool(4))
                {
                    // Gradient from black through dark pink to bright red
                    float progress = Main.rand.NextFloat();
                    Color cosmicColor;
                    if (progress < 0.4f)
                        cosmicColor = Color.Lerp(ThemedParticles.FateBlack, ThemedParticles.FateDarkPink, progress / 0.4f);
                    else if (progress < 0.8f)
                        cosmicColor = Color.Lerp(ThemedParticles.FateDarkPink, ThemedParticles.FateBrightRed, (progress - 0.4f) / 0.4f);
                    else
                        cosmicColor = Color.Lerp(ThemedParticles.FateBrightRed, ThemedParticles.FateWhite, (progress - 0.8f) / 0.2f);
                    
                    var glow = new GenericGlowParticle(
                        player.Center + Main.rand.NextVector2Circular(25f, 20f),
                        new Vector2(player.velocity.X * -0.05f, -1.2f),
                        cosmicColor, 0.35f, 30, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                
                // Cosmic glyphs
                if (Main.rand.NextBool(12))
                {
                    CustomParticles.Glyph(player.Center + Main.rand.NextVector2Circular(35f, 30f), 
                        ThemedParticles.FateDarkPink, 0.3f, -1);
                }
                
                // === DARK COSMIC SMOKE - amorphous reality distortion ===
                if (Main.rand.NextBool(3))
                {
                    var smoke = new HeavySmokeParticle(
                        player.Center + Main.rand.NextVector2Circular(25f, 20f),
                        new Vector2(player.velocity.X * -0.1f, Main.rand.NextFloat(-0.5f, 0.5f)),
                        Color.Lerp(ThemedParticles.FateBlack, ThemedParticles.FatePurple, Main.rand.NextFloat(0.4f)),
                        Main.rand.Next(35, 55), 0.35f, 0.5f, 0.02f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
                
                // Temporal afterimage echoes
                if (Main.rand.NextBool(8))
                {
                    CustomParticles.GenericFlare(player.Center + Main.rand.NextVector2Circular(20f, 15f),
                        ThemedParticles.FateBrightRed * 0.6f, 0.3f, 15);
                }
            }
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
        private int dodgeCooldown = 0;
        private const int DodgeCooldownMax = 15; // Fastest cooldown
        private const float DodgeSpeed = 38f; // Fastest dodge
        private bool isDodging = false;
        private int dodgeTimer = 0;
        private const int DodgeDuration = 10;

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

            // Dodge cooldown tick
            if (dodgeCooldown > 0)
                dodgeCooldown--;
            
            // Handle dodge input - right click (only when no UI is open)
            bool canDodge = !Main.playerInventory && !Main.ingameOptionsWindow && !Main.inFancyUI && 
                           !Main.mapFullscreen && !Main.editChest && !Main.editSign;
            if (canDodge && Main.mouseRight && Main.mouseRightRelease && dodgeCooldown <= 0 && !isDodging)
            {
                PerformDodge();
            }
            
            // Handle ongoing dodge
            if (isDodging)
            {
                dodgeTimer++;
                Player.immune = true;
                Player.immuneTime = 2;
                Player.immuneNoBlink = true;
                
                // Cosmic reality-bending trail
                ThemedParticles.FateTrail(Player.Center, Player.velocity);
                
                // === DARK COSMIC SMOKE - dodge reality tear ===
                for (int s = 0; s < 2; s++)
                {
                    var smoke = new HeavySmokeParticle(
                        Player.Center + Main.rand.NextVector2Circular(15f, 15f),
                        -Player.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f),
                        Color.Lerp(ThemedParticles.FateBlack, ThemedParticles.FateDarkPink, Main.rand.NextFloat(0.35f)),
                        Main.rand.Next(30, 50), 0.4f, 0.55f, 0.02f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
                
                // Dark prismatic dust with chromatic separation
                for (int i = 0; i < 3; i++)
                {
                    Vector2 offset = new Vector2(i - 1, 0) * 3f; // RGB separation
                    Color[] rgbColors = { new Color(255, 80, 100), new Color(200, 80, 160), new Color(100, 60, 140) };
                    Dust trail = Dust.NewDustDirect(Player.position + offset, Player.width, Player.height, 
                        DustID.Enchanted_Pink, -Player.velocity.X * 0.2f, -Player.velocity.Y * 0.2f, 100, rgbColors[i], 1.5f);
                    trail.noGravity = true;
                }
                
                if (dodgeTimer >= DodgeDuration)
                {
                    isDodging = false;
                    dodgeTimer = 0;
                    
                    // Reality-shattering cosmic burst
                    ThemedParticles.FateImpact(Player.Center, 1.2f);
                    CustomParticles.GlyphCircle(Player.Center, ThemedParticles.FateDarkPink, 8, 50f, 0.04f);
                    
                    // Temporal echoes at arrival
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f;
                        Vector2 echoPos = Player.Center + angle.ToRotationVector2() * 40f;
                        float progress = (float)i / 6f;
                        Color echoColor = Color.Lerp(ThemedParticles.FateBrightRed, ThemedParticles.FateDarkPink, progress);
                        CustomParticles.GenericFlare(echoPos, echoColor * 0.7f, 0.4f, 20);
                    }
                }
            }

            // Wing animation logic
            bool isFlying = Player.controlJump && Player.velocity.Y != 0 && !Player.mount.Active;
            bool isOnGround = Player.velocity.Y == 0;
            
            if (isFlying || isDodging)
            {
                frameCounter++;
                if (frameCounter >= 2)
                {
                    frameCounter = 0;
                    wingFrame++;
                    if (wingFrame >= 36) // 6x6 = 36 frames
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

        private void PerformDodge()
        {
            isDodging = true;
            dodgeTimer = 0;
            dodgeCooldown = DodgeCooldownMax;
            
            Vector2 dodgeDirection = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.UnitX);
            Player.velocity = dodgeDirection * DodgeSpeed;
            
            SoundEngine.PlaySound(SoundID.Item163 with { Pitch = 0.1f, Volume = 0.8f }, Player.Center);
            
            // Reality-shattering cosmic burst at departure
            ThemedParticles.FateImpact(Player.Center, 2f);
            
            // Cosmic glyph circle with dark prismatic gradient
            CustomParticles.GlyphCircle(Player.Center, ThemedParticles.FateBrightRed, 10, 60f, 0.05f);
            
            // Fractal flare burst with Fate gradient
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                float progress = (float)i / 10f;
                Color flareColor;
                if (progress < 0.5f)
                    flareColor = Color.Lerp(ThemedParticles.FateDarkPink, ThemedParticles.FateBrightRed, progress * 2f);
                else
                    flareColor = Color.Lerp(ThemedParticles.FateBrightRed, ThemedParticles.FateWhite, (progress - 0.5f) * 2f);
                
                Vector2 flarePos = Player.Center + angle.ToRotationVector2() * 45f;
                CustomParticles.GenericFlare(flarePos, flareColor, 0.5f, 22);
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
