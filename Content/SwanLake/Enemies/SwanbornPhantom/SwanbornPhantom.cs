using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.SwanLake.Enemies
{
    public class SwanbornPhantom : ModNPC
    {
        public override string Texture => "MagnumOpus/Content/SwanLake/Enemies/SwanbornPhantom/SwanbornPhantom";

        private static readonly Color SwanWhite = new Color(240, 240, 255);
        private static readonly Color SwanPrismatic = new Color(220, 220, 240);

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
            NPC.damage = 135;
            NPC.defense = 62;
            NPC.lifeMax = 3800;
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0.08f;
            NPC.value = Item.buyPrice(gold: 9);
            NPC.aiStyle = NPCAIStyleID.HoveringFighter;
            AIType = NPCID.Pixie;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.scale = 0.65f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheHallow,
                new FlavorTextBestiaryInfoElement(
                    "A phantom of porcelain grace, its white swan-feathered form gliding through hallowed light. " +
                    "Prismatic rainbow edges shimmer along its silhouette like dying beauty.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (NPC.downedMoonlord &&
                (spawnInfo.Player.ZoneHallow || spawnInfo.Player.ZoneSkyHeight))
                return 0.05f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GraceEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            NPC.rotation = NPC.velocity.X * 0.015f;

            float hue = (Main.GlobalTimeWrappedHourly * 0.3f) % 1f;
            Color shimmerLight = Main.hslToRgb(hue, 0.2f, 0.9f);
            float pulse = 0.7f + 0.2f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2f);
            Lighting.AddLight(NPC.Center, shimmerLight.ToVector3() * 0.4f * pulse);

            if (Main.rand.NextBool(6))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Enchanted_Gold, Main.rand.NextFloat(-0.8f, 0.8f), -0.5f, 50, Color.White, 0.6f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.WhiteTorch, Main.rand.NextFloat(-0.5f, 0.5f), -0.3f, 80, default, 0.5f);
                dust.noGravity = true;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 22; i++)
                {
                    float hue = Main.rand.NextFloat();
                    Color prismColor = Main.hslToRgb(hue, 0.4f, 0.9f);
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.Enchanted_Gold, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 40, prismColor, 1f);
                }
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.WhiteTorch, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 60, default, 1.1f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Color.Lerp(drawColor, SwanWhite, 0.3f);
        }
    }
}
