using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using ReLogic.Content;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Tools
{
    [AutoloadEquip(EquipType.Wings)]
    public class WingsOfTheMoon : ModItem
    {
        public override void SetStaticDefaults()
        {
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(
                flyTime: 220,
                flySpeedOverride: 11f,
                accelerationMultiplier: 2.8f,
                hasHoldDownHoverFeatures: true
            );
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 1);
            Item.rare = ModContent.RarityType<MoonlightSonataRainbowRarity>();
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
            ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            ascentWhenFalling = 0.95f;
            ascentWhenRising = 0.18f;
            maxCanAscendMultiplier = 1.1f;
            maxAscentMultiplier = 3.2f;
            constantAscend = 0.145f;
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<WingsOfTheMoonPlayer>().hasWingsEquipped = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonanceEnergies.MoonlightsResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<ResonanceEnergies.ResonantCoreOfMoonlightSonata>(), 5)
                .AddIngredient(ModContent.ItemType<Enemies.ShardsOfMoonlitTempo>(), 5)
                .AddIngredient(ItemID.SoulofFlight, 25)
                .AddIngredient(ItemID.SoulofNight, 15)
                .AddTile(ModContent.TileType<CraftingStations.MoonlightAnvilTile>())
                .Register();
        }
    }

    public class WingsOfTheMoonPlayer : ModPlayer
    {
        public int wingFrame = 0;
        private int frameCounter = 0;
        private bool wasFlying = false;  // Track previous flying state
        
        // Dodge mechanic - Double-tap left/right like Shield of Cthulhu
        // Direction constants for doubleTapCardinalTimer array
        private const int DashDown = 0;
        private const int DashUp = 1;
        private const int DashLeft = 2;
        private const int DashRight = 3;
        
        public bool hasWingsEquipped = false;
        private int dodgeCooldown = 0;
        private const int DodgeCooldownMax = 30; // 0.5 seconds at 60fps
        private const float DodgeSpeed = 25f;
        private bool isDodging = false;
        private int dodgeTimer = 0;
        private const int DodgeDuration = 8; // Duration of the dodge in ticks
        private int dashDir = -1; // -1 = none, DashLeft = left, DashRight = right

        public override void ResetEffects()
        {
            hasWingsEquipped = false;
            
            // ResetEffects is called right after doubleTapCardinalTimer values are set by vanilla
            // This is the optimal place to detect double-tap input (from tModLoader ExampleMod)
            // When a directional key is pressed and released, vanilla starts a 15 tick timer
            // If the timer is set to 15, this is the first press. Otherwise, it's a double-tap.
            
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "WingsOfTheMoon", EquipType.Wings);
            bool hasWings = Player.wings == wingSlot && wingSlot > 0;
            
            if (!hasWings && !hasWingsEquipped)
            {
                dashDir = -1;
                return;
            }
            
            // Check for double-tap (timer < 15 means this is the second tap)
            // Also check opposite direction timer == 0 to prevent conflicts
            if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[DashRight] < 15 && Player.doubleTapCardinalTimer[DashLeft] == 0)
            {
                dashDir = DashRight;
            }
            else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[DashLeft] < 15 && Player.doubleTapCardinalTimer[DashRight] == 0)
            {
                dashDir = DashLeft;
            }
            else
            {
                dashDir = -1;
            }
        }
        
        public override void PreUpdateMovement()
        {
            // PreUpdateMovement is the perfect place to apply dash movement
            // It's after vanilla movement code, before position is modified based on velocity
            
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "WingsOfTheMoon", EquipType.Wings);
            bool hasWings = Player.wings == wingSlot && wingSlot > 0;
            
            if (!hasWings && !hasWingsEquipped)
                return;
            
            // Check if we can start a new dodge
            if (CanDodge() && dashDir != -1 && dodgeCooldown <= 0)
            {
                int direction = dashDir == DashLeft ? -1 : 1;
                PerformDodge(direction);
            }
            
            // Handle active dodge
            if (isDodging)
            {
                dodgeTimer++;
                Player.immune = true;
                Player.immuneTime = 2;
                
                // Trail effects during dodge
                CreateDodgeTrailEffects();
                
                if (dodgeTimer >= DodgeDuration)
                {
                    EndDodge();
                }
            }
            
            // Dodge cooldown tick
            if (dodgeCooldown > 0)
                dodgeCooldown--;
        }
        
        private bool CanDodge()
        {
            return (hasWingsEquipped || Player.wings == EquipLoader.GetEquipSlot(Mod, "WingsOfTheMoon", EquipType.Wings))
                && Player.dashType == DashID.None // Don't override Tabi/EoCShield
                && !Player.setSolar // Not wearing solar armor
                && !Player.mount.Active; // Not mounted
        }

        public override void PostUpdate()
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "WingsOfTheMoon", EquipType.Wings);
            bool hasWings = Player.wings == wingSlot && wingSlot > 0;

            if (!hasWings && !hasWingsEquipped)
            {
                wingFrame = 0;
                frameCounter = 0;
                wasFlying = false;
                return;
            }

            // Wing animation logic
            bool isFlying = Player.controlJump && Player.velocity.Y != 0 && !Player.mount.Active;
            bool isOnGround = Player.velocity.Y == 0;

            if (isFlying || isDodging)
            {
                frameCounter++;
                if (frameCounter >= 2) // 33% faster animation (was 3)
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
                // On ground - reset to first frame (idle/folded)
                wingFrame = 0;
                frameCounter = 0;
                wasFlying = false;
            }
            else if (wasFlying && !isFlying)
            {
                // Stopped flying but still in air - hold on first frame until landing
                wingFrame = 0;
                frameCounter = 0;
            }
        }
        
        private void PerformDodge(int direction)
        {
            isDodging = true;
            dodgeTimer = 0;
            dodgeCooldown = DodgeCooldownMax;
            
            // Dodge in the tapped direction (left or right)
            Vector2 dodgeVelocity = new Vector2(direction, 0f);
            
            // Set velocity in dodge direction
            Player.velocity = dodgeVelocity * DodgeSpeed;
            
            // Play dodge sound
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.3f, Volume = 0.8f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Item24 with { Pitch = -0.2f, Volume = 0.6f }, Player.Center);
            
            // Create initial explosion at start position
            CreateDodgeExplosion(Player.Center);
        }
        
        private void EndDodge()
        {
            isDodging = false;
            dodgeTimer = 0;
            
            // Create explosion at end position
            CreateDodgeExplosion(Player.Center);
            
            // Play end sound
            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.5f, Volume = 0.7f }, Player.Center);
        }
        
        private void CreateDodgeExplosion(Vector2 position)
        {
            // Use new themed particle system for enhanced effects
            ThemedParticles.MoonlightImpact(position, 1.5f);
            ThemedParticles.TeleportBurst(position, true);
            
            // Deep purple color: RGB(75, 0, 130) - indigo
            Color deepPurple = new Color(75, 0, 130);
            // Light blue color: RGB(135, 206, 250) - light sky blue
            Color lightBlue = new Color(135, 206, 250);
            
            // Large outer ring - deep purple
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(8f, 14f);
                Dust dust = Dust.NewDustPerfect(position, DustID.PurpleTorch, vel, 0, default, 2.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.5f;
            }
            
            // Medium ring - light blue
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(6f, 10f);
                Dust dust = Dust.NewDustPerfect(position, DustID.IceTorch, vel, 0, default, 2.2f);
                dust.noGravity = true;
                dust.fadeIn = 1.3f;
            }
            
            // Inner burst - mixed
            for (int i = 0; i < 25; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(12f, 12f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Dust dust = Dust.NewDustPerfect(position, dustType, vel, 0, default, 2.8f);
                dust.noGravity = true;
                dust.fadeIn = 1.8f;
            }
            
            // Sparkle effects
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Dust sparkle = Dust.NewDustPerfect(position, DustID.SparksMech, vel, 100, Color.White, 1.5f);
                sparkle.noGravity = true;
            }
            
            // Electric effect
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust electric = Dust.NewDustPerfect(position, DustID.Electric, vel, 100, lightBlue, 1.2f);
                electric.noGravity = true;
            }
            
            // Shadowflame wisps
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(4f, 8f);
                Dust shadow = Dust.NewDustPerfect(position, DustID.Shadowflame, vel, 100, default, 2f);
                shadow.noGravity = true;
            }
            
            // Add light burst
            Lighting.AddLight(position, 0.8f, 0.4f, 1f);
        }
        
        private void CreateDodgeTrailEffects()
        {
            // Use new themed particle system for enhanced trail
            ThemedParticles.DodgeTrail(Player.Center, Player.velocity, true);
            
            // Additional dust trail for density
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(Player.width / 2f, Player.height / 2f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Dust dust = Dust.NewDustPerfect(Player.Center + offset, dustType, -Player.velocity * 0.2f, 0, default, 2f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
            
            // Sparkle trail
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(Player.width, Player.height);
                Dust sparkle = Dust.NewDustPerfect(Player.Center + offset, DustID.SparksMech, -Player.velocity * 0.1f, 100, Color.White, 1.2f);
                sparkle.noGravity = true;
            }
        }

        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "WingsOfTheMoon", EquipType.Wings);
            if (Player.wings == wingSlot && wingSlot > 0)
            {
                // ALWAYS hide vanilla - we do all drawing
                PlayerDrawLayers.Wings.Hide();
            }
        }
    }

    /// <summary>
    /// Custom draw layer for Wings of the Moon.
    /// Uses 6x6 sprite sheet for all animation - frame 0 when idle, animates when flying.
    /// </summary>
    public class WingsOfTheMoonLayer : PlayerDrawLayer
    {
        private Asset<Texture2D> _animatedTexture; // 6x6 sprite sheet for all states

        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.BackAcc);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "WingsOfTheMoon", EquipType.Wings);
            return drawInfo.drawPlayer.wings == wingSlot && wingSlot > 0 && !drawInfo.drawPlayer.dead && !drawInfo.drawPlayer.invis;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            var modPlayer = player.GetModPlayer<WingsOfTheMoonPlayer>();

            // Always use the 6x6 animated sprite sheet
            _animatedTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Content/MoonlightSonata/Tools/WingsOfTheMoon_Animated");
            if (_animatedTexture.State != AssetState.Loaded)
                return;

            Texture2D tex = _animatedTexture.Value;
            int cols = 6;
            int rows = 6;
            int frameW = tex.Width / cols;
            int frameH = tex.Height / rows;

            // Use wingFrame from modPlayer - it's 0 when idle, animates when flying
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

            DrawData data = new DrawData(tex, pos, source, color, player.bodyRotation, origin, 1f, fx, 0);
            drawInfo.DrawDataCache.Add(data);
        }
    }
}
