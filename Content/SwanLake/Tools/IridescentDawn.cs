using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using ReLogic.Content;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common;

namespace MagnumOpus.Content.SwanLake.Tools
{
    /// <summary>
    /// Iridescent Dawn - Swan Lake wings with graceful black and white aesthetics.
    /// Features rainbow shimmer, feather particles, and elegant movement.
    /// Uses 6x6 sprite sheet for flying animation.
    /// </summary>
    [AutoloadEquip(EquipType.Wings)]
    public class IridescentDawn : ModItem
    {
        public override void SetStaticDefaults()
        {
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(
                flyTime: 340, // Higher than La Campanella (320)
                flySpeedOverride: 17f, // Faster than La Campanella (16)
                accelerationMultiplier: 3.6f, // Higher than La Campanella (3.5)
                hasHoldDownHoverFeatures: true
            );
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 4);
            Item.rare = ModContent.RarityType<SwanRarity>();
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
            ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            ascentWhenFalling = 1.25f;
            ascentWhenRising = 0.25f;
            maxCanAscendMultiplier = 1.45f;
            maxAscentMultiplier = 4.2f;
            constantAscend = 0.21f;
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<IridescentDawnPlayer>().hasWingsEquipped = true;
            
            // Elegant swan particles when flying
            if (!hideVisual && player.velocity.Y != 0)
            {
                ThemedParticles.SwanLakeAura(player.Center, 35f);
                
                // Graceful feather drift
                if (Main.rand.NextBool(6))
                {
                    Color featherColor = Main.rand.NextBool() ? ThemedParticles.SwanWhite : ThemedParticles.SwanBlack;
                    ThemedParticles.SwanFeatherDrift(player.Center + Main.rand.NextVector2Circular(20f, 15f), featherColor, 0.3f);
                }
                
                // Rainbow shimmer accents
                if (Main.rand.NextBool(10))
                {
                    float hue = Main.rand.NextFloat();
                    Color rainbow = Main.hslToRgb(hue, 0.6f, 0.85f);
                    CustomParticles.GenericFlare(player.Center + Main.rand.NextVector2Circular(25f, 20f), rainbow * 0.5f, 0.25f, 20);
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SwansResonanceEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfSwanLake>(), 5)
                .AddIngredient(ModContent.ItemType<ShardOfTheFeatheredTempo>(), 5)
                .AddIngredient(ItemID.SoulofFlight, 35)
                .AddIngredient(ItemID.SoulofLight, 15)
                .AddIngredient(ItemID.SoulofNight, 15)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class IridescentDawnPlayer : ModPlayer
    {
        public int wingFrame = 0;
        private int frameCounter = 0;
        private bool wasFlying = false;
        
        public bool hasWingsEquipped = false;
        private int dodgeCooldown = 0;
        private const int DodgeCooldownMax = 20;
        private const float DodgeSpeed = 32f;
        private bool isDodging = false;
        private int dodgeTimer = 0;
        private const int DodgeDuration = 9;
        private int lastDodgeDirection = 0;

        public override void ResetEffects()
        {
            hasWingsEquipped = false;
        }

        public override void PostUpdate()
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "IridescentDawn", EquipType.Wings);
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
            
            // Handle dodge input - double-tap left/right like Shield of Cthulhu
            if (dodgeCooldown <= 0 && !isDodging)
            {
                if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[2] < 15)
                {
                    PerformDodge(-1);
                }
                else if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[3] < 15)
                {
                    PerformDodge(1);
                }
            }
            
            // Handle ongoing dodge
            if (isDodging)
            {
                dodgeTimer++;
                Player.immune = true;
                Player.immuneTime = 2;
                Player.immuneNoBlink = true;
                
                // Elegant feather trail
                ThemedParticles.SwanLakeTrail(Player.Center, Player.velocity);
                
                // Alternating black/white dust
                for (int i = 0; i < 2; i++)
                {
                    Color dustColor = i % 2 == 0 ? Color.White : new Color(30, 30, 40);
                    Dust trail = Dust.NewDustDirect(Player.position, Player.width, Player.height, 
                        DustID.WhiteTorch, -Player.velocity.X * 0.2f, -Player.velocity.Y * 0.2f, 100, dustColor, 1.3f);
                    trail.noGravity = true;
                }
                
                if (dodgeTimer >= DodgeDuration)
                {
                    isDodging = false;
                    dodgeTimer = 0;
                    
                    // Graceful burst on dodge end
                    ThemedParticles.SwanLakeImpact(Player.Center, 1f);
                    ThemedParticles.SwanFeatherBurst(Player.Center, 8, 40f);
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

        private void PerformDodge(int direction)
        {
            isDodging = true;
            dodgeTimer = 0;
            dodgeCooldown = DodgeCooldownMax;
            lastDodgeDirection = direction;
            
            Vector2 dodgeDir = new Vector2(direction, 0f);
            Player.velocity = dodgeDir * DodgeSpeed;
            
            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.4f, Volume = 0.7f }, Player.Center);
            
            // Graceful swan burst
            ThemedParticles.SwanLakeImpact(Player.Center, 1.5f);
            ThemedParticles.SwanFeatherBurst(Player.Center, 12, 50f);
        }
        
        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "IridescentDawn", EquipType.Wings);
            if (Player.wings == wingSlot && wingSlot > 0)
            {
                PlayerDrawLayers.Wings.Hide();
            }
        }
    }
    
    /// <summary>
    /// Custom draw layer for Iridescent Dawn.
    /// Uses 6x6 sprite sheet for all animation.
    /// </summary>
    public class IridescentDawnLayer : PlayerDrawLayer
    {
        private Asset<Texture2D> _animatedTexture;

        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.BackAcc);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "IridescentDawn", EquipType.Wings);
            return drawInfo.drawPlayer.wings == wingSlot && wingSlot > 0 && !drawInfo.drawPlayer.dead && !drawInfo.drawPlayer.invis;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            var modPlayer = player.GetModPlayer<IridescentDawnPlayer>();

            _animatedTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Content/SwanLake/Tools/IridescentDawn_Wings");
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

            DrawData data = new DrawData(tex, pos, source, color, player.bodyRotation, origin, 0.339f, fx, 0);
            drawInfo.DrawDataCache.Add(data);
        }
    }
}
