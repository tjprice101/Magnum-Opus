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
    public class Fogwalker : ModNPC
    {
        public override string Texture => "MagnumOpus/Content/Autumn/Enemies/Fogwalker/Fogwalker";

        private static readonly Color AutumnGold = new Color(210, 160, 60);
        private static readonly Color AutumnEarth = new Color(80, 40, 20);

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
            NPC.height = 75;
            NPC.damage = 50;
            NPC.defense = 20;
            NPC.lifeMax = 550;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath2;
            NPC.knockBackResist = 0.3f;
            NPC.value = Item.buyPrice(silver: 90);
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
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson,
                new FlavorTextBestiaryInfoElement(
                    "A tarnished bronze scarecrow that walks through the autumn mist. " +
                    "Rolling fog and the scent of withered grain follow this harvest spirit, and crows circle above.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneOverworldHeight &&
                (spawnInfo.Player.ZoneCorrupt || spawnInfo.Player.ZoneCrimson) &&
                Main.hardMode)
                return 0.065f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DecayEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            Lighting.AddLight(NPC.Center, AutumnGold.ToVector3() * 0.15f);

            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Smoke, Main.rand.NextFloat(-1f, 1f), -0.5f, 150, default, 0.8f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(15))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.WoodFurniture, Main.rand.NextFloat(-0.8f, 0.8f), -1.2f, 100, default, 0.6f);
                dust.noGravity = false;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 15; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.Smoke, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 1f), 120, default, 1.2f);
                }
                for (int i = 0; i < 8; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.WoodFurniture, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, 0f), 80, default, 1f);
                }
            }
        }
    }
}
