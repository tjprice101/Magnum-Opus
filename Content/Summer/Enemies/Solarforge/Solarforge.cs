using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Summer.Materials;

namespace MagnumOpus.Content.Summer.Enemies
{
    public class Solarforge : ModNPC
    {
        public override string Texture => "MagnumOpus/Content/Summer/Enemies/Solarforge/Solarforge";

        private static readonly Color SummerGold = new Color(255, 200, 80);
        private static readonly Color SummerEmber = new Color(230, 60, 20);

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new()
            {
                Velocity = 1f
            };
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = bestiaryData;
        }

        public override void SetDefaults()
        {
            NPC.width = 130;
            NPC.height = 105;
            NPC.damage = 50;
            NPC.defense = 18;
            NPC.lifeMax = 500;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.knockBackResist = 0.25f;
            NPC.value = Item.buyPrice(silver: 80);
            NPC.aiStyle = NPCAIStyleID.Fighter;
            AIType = NPCID.GraniteGolem;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.lavaImmune = true;
            NPC.scale = 0.65f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
                new FlavorTextBestiaryInfoElement(
                    "A golem of molten gold and obsidian glass, forged by the relentless summer sun. " +
                    "Heat mirages shimmer around its frame as molten droplets trail in its wake.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneDesert &&
                (spawnInfo.Player.ZoneOverworldHeight || spawnInfo.Player.ZoneRockLayerHeight) &&
                Main.hardMode)
                return 0.07f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SolarEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(NPC.Center, SummerGold.ToVector3() * 0.4f * pulse);

            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Torch, Main.rand.NextFloat(-1f, 1f), -1.5f, 80, default, 0.9f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(15))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.SolarFlare, 0f, -0.5f, 100, default, 0.6f);
                dust.noGravity = true;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 18; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.Torch, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 60, default, 1.3f);
                }
                for (int i = 0; i < 8; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.SolarFlare, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, 0f), 80, default, 1f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Color.Lerp(drawColor, SummerGold, 0.1f);
        }
    }
}
