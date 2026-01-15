# MagnumOpus Midjourney Prompts Library

This folder contains organized Midjourney prompts for generating pixel art assets for the MagnumOpus Terraria mod.

## Folder Structure

### Core Particle Prompts
- `01_Soft_Glow_Particles.txt` - Most versatile soft circular glow particles
- `02_Energy_Spark_Flare.txt` - Sharp stars, lens flares, electric sparks
- `03_Smoke_Cloud_Vapor.txt` - Organic smoke and cloud shapes
- `04_Geometric_Magic_Symbols.txt` - Circles, pentagrams, runes
- `05_Impact_Explosion_Burst.txt` - Radial explosions and shockwaves
- `06_Trail_Streak_Motion.txt` - Projectile trails and motion effects
- `07_Dust_Debris_Clusters.txt` - Small particle clusters
- `08_Ring_Halo_Aura.txt` - Circular rings and aura effects
- `09_Musical_Notes.txt` - Musical notation particles
- `10_Feather_Petal_Organic.txt` - Natural organic shapes

### Advanced Effects
- `11_Crystal_Fractal.txt` - Crystalline and fractal formations
- `12_Crystal_Shards.txt` - Shattered crystal debris
- `13_Fractal_Energy.txt` - Mathematical fractal patterns
- `14_Explosive_Bursts.txt` - Various explosion effects
- `15_Explosion_Sequence.txt` - Animated explosion frames
- `16_Shockwave_Rings.txt` - Expanding ring effects
- `17_Light_Beams.txt` - Laser and spotlight effects
- `18_God_Rays.txt` - Divine light shafts
- `19_Beam_Cores.txt` - Concentrated energy beams
- `20_Sword_Smears.txt` - Melee weapon trails
- `21_Weapon_Trails.txt` - Various weapon motion trails
- `22_Anime_Slashes.txt` - Stylized anime slash effects
- `23_Prismatic_Sparkles.txt` - Diamond sparkle effects
- `24_Gem_Glitter.txt` - Jewelry shine effects
- `25_Sparkle_Animation.txt` - Animated sparkle sequence
- `26_Aura_Sparkle_Fields.txt` - Distributed sparkle patterns
- `27_Arcing_Projectiles.txt` - Traveling blade wave projectiles
- `28_Musical_Sound_Waves.txt` - Audio visualization effects
- `29_Piano_Impacts.txt` - Piano-themed effects
- `30_Swan_Feathers.txt` - Elegant feather trails
- `31_Conductor_Baton.txt` - Orchestral command trails
- `32_Harmonic_Waves.txt` - Resonance and standing waves
- `33_Heroic_Slashes.txt` - Eroica theme dramatic attacks
- `34_Moonlight_Effects.txt` - Moonlight Sonata ethereal glows
- `35_Vinyl_Disc.txt` - Spinning disc projectiles
- `36_Violin_Strings.txt` - String instrument effects
- `37_Chain_Links.txt` - Iron chain effects
- `38_Blazing_Chains.txt` - Burning chain effects
- `39_Chain_Attachments.txt` - Chain connection points

### Boss and Item Sprites
- `Boss_Swan_Lake.txt` - Swan Lake boss concept art prompts
- `Items_Treasure_Bags.txt` - Treasure bag designs
- `Items_Sheet_Music.txt` - Celestial sheet music items

### Theme-Specific Weapons
- `La_Campanella_Weapons_Base.txt` - Base La Campanella weapon designs
- `La_Campanella_Weapons_Celestial.txt` - Celestially enhanced versions
- `Celestial_Seeds.txt` - Seed of Universal Melodies designs

### NEW Theme Particle Effects
- `Theme_Enigma_Particles.txt` - Question marks, mysteries, arcane symbols
- `Theme_Fate_Particles.txt` - Planets, stars, constellations, cosmic effects
- `Theme_Clair_de_Lune_Particles.txt` - Clockwork, gears, mechanical effects
- `Theme_Ode_to_Joy_Particles.txt` - Rose petals, vines, botanical effects
- `Theme_Nachtmusik_Particles.txt` - Stars, constellations, night sky effects
- `Theme_Dies_Irae_Particles.txt` - Chains, flames, hellfire, infernal effects

## Usage Tips

1. All prompts generate WHITE/GRAYSCALE particles for tinting in code
2. Target resolutions: 8x8, 16x16, 32x32, 64x64 pixels
3. Always use transparent backgrounds (PNG with alpha)
4. Post-process to ensure pure grayscale with no color cast
5. Test tinting with `spriteBatch.Draw()` color parameter

## Color Palettes

Reference color palettes for each theme are included at the end of relevant prompt files.
