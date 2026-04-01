using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.EnemyDrops;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Nachtmusik.Enemies
{
    public class TwilightProwler : ModNPC
    {
        public override string Texture => "MagnumOpus/Content/Nachtmusik/Enemies/TwilightProwler/TwilightProwler";

        private static readonly Color TwilightBlue = new Color(60, 80, 160);
        private static readonly Color CosmicBlue = new Color(120, 150, 240);

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
            NPC.damage = 170;
            NPC.defense = 74;
            NPC.lifeMax = 6000;
            NPC.HitSound = SoundID.NPCHit36;
            NPC.DeathSound = SoundID.NPCDeath39;
            NPC.knockBackResist = 0.05f;
            NPC.value = Item.buyPrice(gold: 16);
            NPC.aiStyle = NPCAIStyleID.Fighter;
            AIType = NPCID.Herpling;
            NPC.scale = 0.65f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement(
                    "A twilight predator of deep indigo and cosmic blue, prowling the boundary between dusk and night. " +
                    "Its movements leave shimmering trails of stellar dust.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (MoonlightSonataSystem.FateBossKilledOnce &&
                !Main.dayTime &&
                spawnInfo.Player.ZoneOverworldHeight)
                return 0.045f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<NachtmusikEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            float pulse = 0.5f + 0.35f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2f);
            Lighting.AddLight(NPC.Center, TwilightBlue.ToVector3() * 0.35f * pulse);

            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.BlueTorch, Main.rand.NextFloat(-0.8f, 0.8f), -0.6f, 100, default, 0.65f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(14))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.DungeonSpirit, Main.rand.NextFloat(-0.3f, 0.3f), -0.3f, 120, CosmicBlue, 0.4f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 22; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.BlueTorch, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 60, default, 1.2f);
                }
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.DungeonSpirit, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 80, CosmicBlue, 0.8f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Color.Lerp(drawColor, CosmicBlue, 0.15f);
        }
    }
}
