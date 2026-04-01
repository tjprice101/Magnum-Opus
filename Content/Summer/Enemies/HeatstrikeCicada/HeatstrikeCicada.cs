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
    public class HeatstrikeCicada : ModNPC
    {
        public override string Texture => "MagnumOpus/Content/Summer/Enemies/HeatstrikeCicada/HeatstrikeCicada";

        private static readonly Color SummerOrange = new Color(255, 150, 40);
        private static readonly Color SummerFlame = new Color(255, 100, 20);

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new()
            {
                Velocity = 2f
            };
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = bestiaryData;
        }

        public override void SetDefaults()
        {
            NPC.width = 130;
            NPC.height = 68;
            NPC.damage = 55;
            NPC.defense = 16;
            NPC.lifeMax = 450;
            NPC.HitSound = SoundID.NPCHit32;
            NPC.DeathSound = SoundID.NPCDeath36;
            NPC.knockBackResist = 0.3f;
            NPC.value = Item.buyPrice(silver: 75);
            NPC.aiStyle = NPCAIStyleID.Fighter;
            AIType = NPCID.MossHornet;
            NPC.noGravity = true;
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
                    "An ember-veined cicada of burnished copper, buzzing with the heat of an endless summer. " +
                    "Its wings trail wavering heat lines as it hunts through scorching sands.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneDesert &&
                (spawnInfo.Player.ZoneOverworldHeight || spawnInfo.Player.ZoneRockLayerHeight) &&
                Main.hardMode)
                return 0.06f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SolarEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            Lighting.AddLight(NPC.Center, SummerOrange.ToVector3() * 0.35f);

            if (Main.rand.NextBool(6))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Torch, NPC.velocity.X * 0.3f, -0.5f, 100, default, 0.7f);
                dust.noGravity = true;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 15; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.Torch, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 80, default, 1.1f);
                }
            }
        }
    }
}
