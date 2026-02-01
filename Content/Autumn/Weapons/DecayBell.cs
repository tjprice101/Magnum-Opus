using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Autumn.Projectiles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Autumn.Weapons
{
    /// <summary>
    /// Decay Bell - Autumn-themed summon weapon (Post-Plantera tier)
    /// A cursed bell that summons spirits of decay.
    /// - Harvest Wraith: Summons spectral reapers that attack enemies (88 damage)
    /// - Death Toll: Wraiths periodically toll, dealing AoE damage
    /// - Soul Reap: Enemies killed by wraiths drop healing orbs
    /// - Decay Synchrony: 3+ wraiths create withering aura around player
    /// </summary>
    public class DecayBell : ModItem
    {
        private static readonly Color DecayPurple = new Color(100, 50, 120);
        private static readonly Color WraithGreen = new Color(120, 180, 100);
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color SoulWhite = new Color(240, 240, 255);

        public override void SetStaticDefaults()
        {
            ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 38;
            Item.damage = 88;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 14;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3.5f;
            Item.value = Item.buyPrice(gold: 30);
            Item.rare = ItemRarityID.Lime;
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<HarvestWraithMinion>();
            Item.buffType = ModContent.BuffType<HarvestWraithBuff>();
        }

        public override void HoldItem(Player player)
        {
            // Ambient decay aura
            if (Main.rand.NextBool(10))
            {
                Vector2 auraPos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 auraVel = new Vector2(0, -Main.rand.NextFloat(0.5f, 1.5f));
                Color auraColor = Color.Lerp(DecayPurple, WraithGreen, Main.rand.NextFloat()) * 0.35f;
                var aura = new GenericGlowParticle(auraPos, auraVel, auraColor, 0.2f, 28, true);
                MagnumParticleHandler.SpawnParticle(aura);
            }

            // Floating autumn melody notes
            if (Main.rand.NextBool(12))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(38f, 38f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.2f, 0.5f));
                Color noteColor = Color.Lerp(AutumnOrange, new Color(139, 90, 43), Main.rand.NextFloat()) * 0.6f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.75f, 40);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 0.4f;
            Lighting.AddLight(player.Center, DecayPurple.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Apply buff
            player.AddBuff(Item.buffType, 2);

            // Spawn VFX - layered bloom instead of halo
            CustomParticles.GenericFlare(Main.MouseWorld, WraithGreen, 0.7f, 20);
            CustomParticles.GenericFlare(Main.MouseWorld, DecayPurple, 0.55f, 16);
            CustomParticles.GenericFlare(Main.MouseWorld, DecayPurple * 0.5f, 0.4f, 13);
            
            // Music note on summon
            ThemedParticles.MusicNote(Main.MouseWorld, Vector2.Zero, AutumnOrange * 0.8f, 0.7f, 25);
            
            // Music note ring and burst for summoning
            ThemedParticles.MusicNoteRing(Main.MouseWorld, AutumnOrange, 40f, 6);
            ThemedParticles.MusicNoteBurst(Main.MouseWorld, new Color(139, 90, 43), 5, 4f);
            
            // Decay glyphs (Autumn harvest theme)
            CustomParticles.GlyphBurst(Main.MouseWorld, new Color(139, 90, 43), 4, 3f);
            
            // Sparkle accents
            for (int sparkIdx = 0; sparkIdx < 4; sparkIdx++)
            {
                var sparkle = new SparkleParticle(Main.MouseWorld + Main.rand.NextVector2Circular(12f, 12f),
                    Main.rand.NextVector2Circular(2f, 2f), new Color(218, 165, 32) * 0.5f, 0.2f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Soul wisp burst
            for (int wisp = 0; wisp < 6; wisp++)
            {
                float wispAngle = MathHelper.TwoPi * wisp / 6f;
                Vector2 wispPos = Main.MouseWorld + wispAngle.ToRotationVector2() * 18f;
                Color wispColor = Color.Lerp(WraithGreen, DecayPurple, (float)wisp / 6f);
                CustomParticles.GenericFlare(wispPos, wispColor * 0.7f, 0.25f, 14);
            }

            // Summoning burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color burstColor = Color.Lerp(DecayPurple, WraithGreen, (float)i / 10f) * 0.6f;
                var burst = new GenericGlowParticle(Main.MouseWorld, burstVel, burstColor, 0.3f, 22, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // Rising soul wisps
            for (int i = 0; i < 5; i++)
            {
                Vector2 wispVel = new Vector2(Main.rand.NextFloat(-2f, 2f), -Main.rand.NextFloat(3f, 6f));
                Color wispColor = Color.Lerp(WraithGreen, SoulWhite, Main.rand.NextFloat()) * 0.5f;
                var wisp = new GenericGlowParticle(Main.MouseWorld, wispVel, wispColor, 0.25f, 30, true);
                MagnumParticleHandler.SpawnParticle(wisp);
            }

            // Spawn minion at cursor
            Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI);

            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, DecayPurple * 0.32f, rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, WraithGreen * 0.25f, rotation, origin, scale * 1.15f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, AutumnOrange * 0.18f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, DecayPurple.ToVector3() * 0.45f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MinionSlots", "Uses 1 minion slot") { OverrideColor = Color.Gray });
            tooltips.Add(new TooltipLine(Mod, "HarvestWraith", "Summons spectral harvest wraiths that reap enemies") { OverrideColor = WraithGreen });
            tooltips.Add(new TooltipLine(Mod, "DeathToll", "Wraiths periodically toll, dealing AoE damage") { OverrideColor = DecayPurple });
            tooltips.Add(new TooltipLine(Mod, "SoulReap", "Enemies killed by wraiths drop healing orbs") { OverrideColor = SoulWhite });
            tooltips.Add(new TooltipLine(Mod, "DecaySynchrony", "3+ wraiths create a withering aura around you") { OverrideColor = AutumnOrange });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The bell tolls for all who stand in your way'") { OverrideColor = Color.Lerp(DecayPurple, WraithGreen, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<HarvestBar>(), 16)
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofFright, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Buff for Harvest Wraith minions
    /// </summary>
    public class HarvestWraithBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.CursedInferno;
        
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<HarvestWraithMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;

                // Decay Synchrony: 3+ wraiths create withering aura
                if (player.ownedProjectileCounts[ModContent.ProjectileType<HarvestWraithMinion>()] >= 3)
                {
                    // Aura damage to nearby enemies
                    foreach (NPC npc in Main.npc)
                    {
                        if (!npc.CanBeChasedBy()) continue;
                        if (Vector2.Distance(player.Center, npc.Center) < 200f)
                        {
                            npc.AddBuff(BuffID.CursedInferno, 30);
                        }
                    }

                    // Aura VFX
                    if (Main.rand.NextBool(6))
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float dist = Main.rand.NextFloat(80f, 180f);
                        Vector2 pos = player.Center + angle.ToRotationVector2() * dist;
                        Vector2 vel = (angle + MathHelper.PiOver2).ToRotationVector2() * 1.5f + new Vector2(0, -0.5f);
                        Color auraColor = Color.Lerp(new Color(100, 50, 120), new Color(120, 180, 100), Main.rand.NextFloat()) * 0.4f;
                        var aura = new GenericGlowParticle(pos, vel, auraColor, 0.22f, 25, true);
                        MagnumParticleHandler.SpawnParticle(aura);
                    }
                    
                    // Floating music notes in decay aura
                    if (Main.rand.NextBool(15))
                    {
                        float noteAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float noteDist = Main.rand.NextFloat(100f, 180f);
                        Vector2 notePos = player.Center + noteAngle.ToRotationVector2() * noteDist;
                        Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(0.1f, 0.4f));
                        Color noteColor = Color.Lerp(new Color(255, 140, 50), new Color(139, 90, 43), Main.rand.NextFloat()) * 0.5f;
                        ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.65f, 35);
                    }
                }
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
