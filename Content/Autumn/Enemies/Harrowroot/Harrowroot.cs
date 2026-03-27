using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Autumn.Materials;

namespace MagnumOpus.Content.Autumn.Enemies
{
    public class Harrowroot : ModNPC
    {
        private static readonly Color AutumnAmber = new Color(200, 120, 40);
        private static readonly Color AutumnSienna = new Color(180, 60, 20);

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
            NPC.height = 141;
            NPC.damage = 55;
            NPC.defense = 22;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit7;
            NPC.DeathSound = SoundID.NPCDeath5;
            NPC.knockBackResist = 0.2f;
            NPC.value = Item.buyPrice(gold: 1);
            NPC.aiStyle = NPCAIStyleID.Fighter;
            AIType = NPCID.Herpling;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.scale = 0.65f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCorruption,
                new FlavorTextBestiaryInfoElement(
                    "A petrified harvest guardian of dark oak and rusted iron. " +
                    "Dead leaves swirl around its gnarled frame, and the smell of decay follows wherever it treads.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneOverworldHeight &&
                (spawnInfo.Player.ZoneCorrupt || spawnInfo.Player.ZoneCrimson) &&
                Main.hardMode)
                return 0.07f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DecayEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            Lighting.AddLight(NPC.Center, AutumnAmber.ToVector3() * 0.2f);

            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.WoodFurniture, Main.rand.NextFloat(-1f, 1f), -1f, 120, default, 0.8f);
                dust.noGravity = false;
            }
            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Dirt, Main.rand.NextFloat(-0.5f, 0.5f), -0.3f, 100, AutumnSienna, 0.5f);
                dust.noGravity = true;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 12; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.WoodFurniture, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 80, default, 1.2f);
                }
                for (int i = 0; i < 8; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.Dirt, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 1f), 100, AutumnSienna, 0.8f);
                }
            }
        }
    }
}
