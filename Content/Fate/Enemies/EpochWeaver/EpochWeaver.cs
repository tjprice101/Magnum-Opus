using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.Fate.Enemies
{
    public class EpochWeaver : ModNPC
    {
        private static readonly Color FatePink = new Color(200, 50, 100);
        private static readonly Color FateVoid = new Color(40, 10, 40);

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
            NPC.height = 77;
            NPC.damage = 155;
            NPC.defense = 68;
            NPC.lifeMax = 5000;
            NPC.HitSound = SoundID.NPCHit36;
            NPC.DeathSound = SoundID.NPCDeath39;
            NPC.knockBackResist = 0.04f;
            NPC.value = Item.buyPrice(gold: 13);
            NPC.aiStyle = NPCAIStyleID.Fighter;
            AIType = NPCID.GraniteGolem;
            NPC.scale = 0.65f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson,
                new FlavorTextBestiaryInfoElement(
                    "A weaver of epochs and timelines, its massive form draped in void-black armor " +
                    "threaded with pulsing crimson veins. Time itself warps around its deliberate steps.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (NPC.downedMoonlord &&
                (spawnInfo.Player.ZoneCorrupt || spawnInfo.Player.ZoneCrimson))
                return 0.04f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FateEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            float pulse = 0.5f + 0.4f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2.5f);
            Lighting.AddLight(NPC.Center, FatePink.ToVector3() * 0.4f * pulse);

            if (Main.rand.NextBool(7))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.PinkTorch, Main.rand.NextFloat(-0.8f, 0.8f), -0.6f, 100, default, 0.8f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(10))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Shadowflame, Main.rand.NextFloat(-0.5f, 0.5f), -0.4f, 120, default, 0.6f);
                dust.noGravity = true;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 22; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.PinkTorch, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 60, default, 1.3f);
                }
                for (int i = 0; i < 12; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.Shadowflame, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 80, default, 1f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Color.Lerp(drawColor, FatePink, 0.18f);
        }
    }
}
