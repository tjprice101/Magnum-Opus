using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Spring.Materials;

namespace MagnumOpus.Content.Spring.Enemies
{
    public class Bloomweaver : ModNPC
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringGreen = new Color(150, 230, 130);

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
            NPC.height = 104;
            NPC.damage = 35;
            NPC.defense = 12;
            NPC.lifeMax = 300;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0.4f;
            NPC.value = Item.buyPrice(silver: 50);
            NPC.aiStyle = NPCAIStyleID.Fighter;
            AIType = NPCID.Pixie;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.scale = 0.65f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement(
                    "A living garden spirit born from the melody of spring. " +
                    "Vine-wrapped and blooming, it drifts through forests scattering petals and thorns alike.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneOverworldHeight &&
                !spawnInfo.Player.ZoneDesert && !spawnInfo.Player.ZoneSnow &&
                (spawnInfo.Player.ZoneForest || spawnInfo.Player.ZoneHallow) &&
                Main.hardMode)
                return 0.08f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BlossomEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            Lighting.AddLight(NPC.Center, SpringPink.ToVector3() * 0.3f);

            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.PinkFairy, NPC.velocity.X * 0.2f, -1f, 120, default, 0.8f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.JungleSpore, 0f, -0.5f, 100, default, 0.6f);
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
                        DustID.PinkFairy, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 1.2f);
                }
                for (int i = 0; i < 8; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.JungleSpore, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-4f, 0f), 80, default, 1f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Color.Lerp(drawColor, Color.White, 0.15f);
        }
    }
}
