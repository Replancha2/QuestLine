using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor tool that generates the 48 MissionData ScriptableObjects for the catalog.
/// Menu: Questline → Generate Mission Assets
///
/// 12 ranks × 4 stats (Strength / Dexterity / Intelligence / Charisma) = 48 missions.
/// Naming: Mission_R{rank}_{Stat}.asset
/// Base rewards: CoinsReward = rank × 3 | FameReward = rank × 5
/// Hints: 2 per mission (index 0 = vague/cheap, index 1 = direct/expensive)
/// </summary>
public static class MissionDataGenerator
{
    private const string OUTPUT_PATH = "Assets/Data/Missions";

    [MenuItem("Questline/Generate Mission Assets")]
    public static void GenerateAll()
    {
        if (!AssetDatabase.IsValidFolder(OUTPUT_PATH))
        {
            string parent = Path.GetDirectoryName(OUTPUT_PATH);
            string folder = Path.GetFileName(OUTPUT_PATH);
            AssetDatabase.CreateFolder(parent, folder);
        }

        var entries = BuildMissionTable();

        int created  = 0;
        int skipped  = 0;

        foreach (var e in entries)
        {
            string path = $"{OUTPUT_PATH}/Mission_R{(int)e.Rank}_{e.StatTag}.asset";

            // Don't overwrite if already exists
            if (AssetDatabase.LoadAssetAtPath<MissionData>(path) != null)
            {
                skipped++;
                continue;
            }

            var data            = ScriptableObject.CreateInstance<MissionData>();
            data.MissionTitle   = e.Title;
            data.Description    = e.Description;
            data.Rank           = e.Rank;
            data.PrimaryStat    = e.PrimaryStat;
            data.SecondaryStats = e.SecondaryStats;
            data.CoinsReward    = (int)e.Rank * 3;
            data.FameReward     = (int)e.Rank * 5;
            data.Hints          = e.Hints;

            AssetDatabase.CreateAsset(data, path);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[MissionDataGenerator] Done — {created} assets created, {skipped} already existed.");
        EditorUtility.DisplayDialog("Mission Generator", $"{created} assets created.\n{skipped} already existed.", "OK");
    }

    // -----------------------------------------------------------------------
    // Mission table
    // -----------------------------------------------------------------------

    private struct Entry
    {
        public MissionRank Rank;
        public string      StatTag;        // "Str" | "Dex" | "Int" | "Cha"
        public HeroStat    PrimaryStat;
        public HeroStat[]  SecondaryStats;
        public string      Title;
        public string      Description;
        public MissionHint[] Hints;
    }

    private static Entry[] BuildMissionTable()
    {
        // Reusable colors
        Color green  = new Color(0.18f, 0.80f, 0.44f);   // direct confirmation
        Color orange = new Color(1.00f, 0.60f, 0.10f);   // vague hint / caution

        // Local helper to create the standard two-hint array
        MissionHint[] Hints(HeroStat stat, string vague, string direct)
            => new MissionHint[]
            {
                new MissionHint { HintText = vague,  HintColor = orange, RelatedStat = stat },
                new MissionHint { HintText = direct, HintColor = green,  RelatedStat = stat },
            };

        return new Entry[]
        {
            // ==================================================================
            // STRENGTH (Str) — R1-R4: direct | R5-R8: ambiguous | R9-R12: deceptive
            // ==================================================================
            new Entry
            {
                Rank = MissionRank.R1, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[0],
                Title       = "Rubble in the Mine",
                Description = "Mine workers are trapped after a collapse. Tons of rock must be cleared by hand.",
                Hints       = Hints(HeroStat.Strength, "This requires muscle, not brains.", "Strength ++"),
            },
            new Entry
            {
                Rank = MissionRank.R2, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[0],
                Title       = "Cargo Across the River",
                Description = "A merchant needs to move their heavy cargo across the river before the storm arrives.",
                Hints       = Hints(HeroStat.Strength, "Weight is everything here.", "Strength ++"),
            },
            new Entry
            {
                Rank = MissionRank.R3, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[0],
                Title       = "The Troll of the Pass",
                Description = "A troll blocks the mountain pass. The local militia has been unable to dislodge it for weeks.",
                Hints       = Hints(HeroStat.Strength, "Brute force is the only option.", "Strength +++"),
            },
            new Entry
            {
                Rank = MissionRank.R4, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[] { HeroStat.Dexterity },
                Title       = "The Walls of Ironhaven",
                Description = "The city walls are crumbling. Heavy labor is needed to reinforce the foundations before winter.",
                Hints       = Hints(HeroStat.Strength, "It's hard construction work.", "Strength +++ / Dexterity +"),
            },
            new Entry
            {
                Rank = MissionRank.R5, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[0],
                Title       = "The Beast of the North",
                Description = "The beast that terrorized the northern forest has stopped appearing. Something has frightened it. Someone must find out what lurks within.",
                Hints       = Hints(HeroStat.Strength, "Whatever frightened the beast won't flee from words.", "Strength ++"),
            },
            new Entry
            {
                Rank = MissionRank.R6, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[0],
                Title       = "The Undefeated Champion",
                Description = "The city's annual tournament has a champion who has gone undefeated for three years. The guild is betting big this year.",
                Hints       = Hints(HeroStat.Strength, "The one who endures longest wins here.", "Strength +++"),
            },
            new Entry
            {
                Rank = MissionRank.R7, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[] { HeroStat.Intelligence },
                Title       = "The Cold of the Deep",
                Description = "The dungeon beneath the castle radiates unnatural cold. Someone must reach the bottom and break whatever lies there.",
                Hints       = Hints(HeroStat.Strength, "The focus is not the mind, but enduring the physical.", "Strength +++ / Intelligence +"),
            },
            new Entry
            {
                Rank = MissionRank.R8, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[0],
                Title       = "The Steel Offering",
                Description = "The gods of war demand an offering of steel and will. The ritual requires the envoy to endure extreme tests of endurance.",
                Hints       = Hints(HeroStat.Strength, "The gods do not reward cleverness here.", "Strength ++++"),
            },
            new Entry
            {
                Rank = MissionRank.R9, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[0],
                Title       = "Echoes from the Catacombs",
                Description = "The echo of the dead whispers from the catacombs beneath the city. The truth lies buried under tons of stone and silence.",
                Hints       = Hints(HeroStat.Strength, "Words do not move rock.", "Strength ++++"),
            },
            new Entry
            {
                Rank = MissionRank.R10, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[] { HeroStat.Charisma },
                Title       = "The Forgotten Pact",
                Description = "A forgotten pact must be honored before the next eclipse. The guild's representative must stand before the ancestral council and prove their worth.",
                Hints       = Hints(HeroStat.Strength, "The council tests the body before the spirit.", "Strength ++++ / Charisma +"),
            },
            new Entry
            {
                Rank = MissionRank.R11, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[0],
                Title       = "The Last Guardian",
                Description = "The last guardian of the tower has fallen. Only the threshold remains, and what lurks beyond shows no mercy to the weak.",
                Hints       = Hints(HeroStat.Strength, "Whoever enters must overcome what the guardians could not.", "Strength +++++"),
            },
            new Entry
            {
                Rank = MissionRank.R12, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[] { HeroStat.Dexterity },
                Title       = "The Primordial's Chain",
                Description = "The chain binding the primordial breaks link by link. Someone must hold it with their bare hands until reinforcements arrive.",
                Hints       = Hints(HeroStat.Strength, "Only pure strength can hold the unsustainable.", "Strength +++++ / Dexterity ++"),
            },

            // ==================================================================
            // DEXTERITY (Dex) — R1-R4: direct | R5-R8: ambiguous | R9-R12: deceptive
            // ==================================================================
            new Entry
            {
                Rank = MissionRank.R1, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[0],
                Title       = "Shadows in the Forest",
                Description = "The shadowy forest requires someone who can move unseen among the shadows throughout the night.",
                Hints       = Hints(HeroStat.Dexterity, "This is not about strength, but stealth.", "Dexterity ++"),
            },
            new Entry
            {
                Rank = MissionRank.R2, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[0],
                Title       = "The Market Pickpocket",
                Description = "A pickpocket operates in the central market. The guards have been unable to catch them for weeks.",
                Hints       = Hints(HeroStat.Dexterity, "You must move faster than they do.", "Dexterity ++"),
            },
            new Entry
            {
                Rank = MissionRank.R3, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[0],
                Title       = "Theft at the Archive",
                Description = "A hooded figure steals documents from the guild archive at night. A trap must be set before dawn.",
                Hints       = Hints(HeroStat.Dexterity, "The thief cannot see it coming.", "Dexterity +++"),
            },
            new Entry
            {
                Rank = MissionRank.R4, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[0],
                Title       = "Urgent Messenger",
                Description = "An urgent messenger must cross enemy territory undetected. The package must not fall into the wrong hands.",
                Hints       = Hints(HeroStat.Dexterity, "Zero visibility throughout the entire route.", "Dexterity +++"),
            },
            new Entry
            {
                Rank = MissionRank.R5, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[0],
                Title       = "The Tunnels Beneath the City",
                Description = "The tunnel network beneath the city is a maze of traps and hidden passages. The package must reach the other side before dawn.",
                Hints       = Hints(HeroStat.Dexterity, "No map is useful here; it's all instinct and reflexes.", "Dexterity ++"),
            },
            new Entry
            {
                Rank = MissionRank.R6, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[] { HeroStat.Intelligence },
                Title       = "The Desecrated Reliquary",
                Description = "The temple's reliquary has been desecrated. The thieves slipped away without leaving a visible trace. Someone must follow where they went.",
                Hints       = Hints(HeroStat.Dexterity, "The thieves' path is not easy to follow.", "Dexterity +++ / Intelligence +"),
            },
            new Entry
            {
                Rank = MissionRank.R7, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[0],
                Title       = "Voss's Traps",
                Description = "Engineer Voss's mechanical traps are legendary. His mansion is full of them, and there is something inside that the guild needs.",
                Hints       = Hints(HeroStat.Dexterity, "The mind won't save you from a spring trap.", "Dexterity ++++"),
            },
            new Entry
            {
                Rank = MissionRank.R8, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[] { HeroStat.Charisma },
                Title       = "The Wind Dance",
                Description = "The wind tribe performs a ritual that can only be completed by hands blessed with absolute precision. The guild has been invited to participate.",
                Hints       = Hints(HeroStat.Dexterity, "The ritual rewards grace, not strength.", "Dexterity ++++ / Charisma +"),
            },
            new Entry
            {
                Rank = MissionRank.R9, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[0],
                Title       = "The Veil of the New Moon",
                Description = "The veil between the visible and the hidden thins with the new moon. Only one who knows how to move in the spaces between shadows can cross it without consequence.",
                Hints       = Hints(HeroStat.Dexterity, "It is not the mind that crosses that veil.", "Dexterity ++++"),
            },
            new Entry
            {
                Rank = MissionRank.R10, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[0],
                Title       = "The Strings of Fate",
                Description = "The strings of fate's harp vibrate at the wrong frequency. Only fingers trained for years can retune them before the melody breaks.",
                Hints       = Hints(HeroStat.Dexterity, "The melody is played with the hands, not the mind.", "Dexterity +++++"),
            },
            new Entry
            {
                Rank = MissionRank.R11, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[] { HeroStat.Intelligence },
                Title       = "The Shadow Assassin",
                Description = "The shadow assassin has marked a council noble. The window to intercept them is minimal and the surroundings are full of obstacles.",
                Hints       = Hints(HeroStat.Dexterity, "The contact must be clean and silent.", "Dexterity +++++ / Intelligence +"),
            },
            new Entry
            {
                Rank = MissionRank.R12, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[0],
                Title       = "The Loose Thread of the Tapestry",
                Description = "The last thread of the cosmic tapestry has come loose. Whoever ties it will change the pattern of reality forever. The weaver needs hands that do not tremble.",
                Hints       = Hints(HeroStat.Dexterity, "Reality does not forgive clumsiness.", "Dexterity +++++"),
            },

            // ==================================================================
            // INTELLIGENCE (Int) — R1-R4: direct | R5-R8: ambiguous | R9-R12: deceptive
            // ==================================================================
            new Entry
            {
                Rank = MissionRank.R1, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[0],
                Title       = "The Arcane Seals",
                Description = "An ancient crypt has been desecrated. Someone must decipher the arcane seals carved at the entrance to seal it again.",
                Hints       = Hints(HeroStat.Intelligence, "The seals are a matter of knowledge.", "Intelligence ++"),
            },
            new Entry
            {
                Rank = MissionRank.R2, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[0],
                Title       = "The Alchemist's Formula",
                Description = "The town alchemist needs a rare ingredient. The formula to synthesize it is extremely complex.",
                Hints       = Hints(HeroStat.Intelligence, "Without knowledge of alchemy, there is nothing to be done.", "Intelligence ++"),
            },
            new Entry
            {
                Rank = MissionRank.R3, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[0],
                Title       = "Altered Records",
                Description = "The royal archive records have been tampered with. Someone must find the discrepancies before the report reaches the king.",
                Hints       = Hints(HeroStat.Intelligence, "An analytical eye and knowledge of accounting are needed.", "Intelligence +++"),
            },
            new Entry
            {
                Rank = MissionRank.R4, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[0],
                Title       = "The Crystal Lake",
                Description = "A miscast spell has transformed the lake into solid crystal. It must be reversed before dawn or the valley's crops will go unwatered.",
                Hints       = Hints(HeroStat.Intelligence, "The solution is arcane, not physical.", "Intelligence +++"),
            },
            new Entry
            {
                Rank = MissionRank.R5, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[0],
                Title       = "The Oracle's Riddles",
                Description = "The mountain oracle speaks in riddles. The correct answer will unlock the knowledge the guild needs. The wrong answer has consequences.",
                Hints       = Hints(HeroStat.Intelligence, "Riddles are not answered with muscle.", "Intelligence ++"),
            },
            new Entry
            {
                Rank = MissionRank.R6, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[0],
                Title       = "The Forbidden Volume",
                Description = "The forbidden library contains a volume that no one has been able to read without losing their sanity. The guild needs the information within.",
                Hints       = Hints(HeroStat.Intelligence, "The mind must be exceptionally strong to withstand it.", "Intelligence +++"),
            },
            new Entry
            {
                Rank = MissionRank.R7, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[] { HeroStat.Dexterity },
                Title       = "The Erratic Golem",
                Description = "Master Theron's golem is behaving erratically. Its control runes appear contradictory to one another. It must be reprogrammed before it causes harm.",
                Hints       = Hints(HeroStat.Intelligence, "The golem obeys whoever understands its runes.", "Intelligence ++++ / Dexterity +"),
            },
            new Entry
            {
                Rank = MissionRank.R8, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[0],
                Title       = "The Impossible Constellation",
                Description = "The star map shows a constellation that should not exist according to all known texts. Someone must determine what it means before dawn.",
                Hints       = Hints(HeroStat.Intelligence, "This is not resolved with swords or words.", "Intelligence ++++"),
            },
            new Entry
            {
                Rank = MissionRank.R9, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[] { HeroStat.Charisma },
                Title       = "The Counselor's Secrets",
                Description = "The royal counselor's mind holds secrets that could change the kingdom. Someone must extract them without the counselor knowing they've been revealed.",
                Hints       = Hints(HeroStat.Intelligence, "Seeming friendly is not enough; you must understand what is left unsaid.", "Intelligence ++++ / Charisma +"),
            },
            new Entry
            {
                Rank = MissionRank.R10, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[0],
                Title       = "The Solstice Seal",
                Description = "The seal of the primordial pact must be renewed before the solstice. The renewal formula was lost centuries ago. It must be reconstructed.",
                Hints       = Hints(HeroStat.Intelligence, "Whoever reconstructs it must know the language of the ancients.", "Intelligence +++++"),
            },
            new Entry
            {
                Rank = MissionRank.R11, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[0],
                Title       = "The Eleventh Component",
                Description = "The formula for the poison infecting the river has twelve components. Eleven are known. The twelfth has been erased from all records. Time is running out.",
                Hints       = Hints(HeroStat.Intelligence, "The solution lies in texts that almost no one can read.", "Intelligence +++++"),
            },
            new Entry
            {
                Rank = MissionRank.R12, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[0],
                Title       = "The Error in the Equation",
                Description = "The equation that sustains reality contains an error. Only someone whose mind transcends the boundaries of known knowledge can find and correct it.",
                Hints       = Hints(HeroStat.Intelligence, "No physical or social skill can resolve this.", "Intelligence +++++"),
            },

            // ==================================================================
            // CHARISMA (Cha) — R1-R4: direct | R5-R8: ambiguous | R9-R12: deceptive
            // ==================================================================
            new Entry
            {
                Rank = MissionRank.R1, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[0],
                Title       = "The Corrupt Noble",
                Description = "The harbor merchants need to convince a corrupt noble to withdraw the illegal tax that is ruining their business.",
                Hints       = Hints(HeroStat.Charisma, "This is not resolved with fists.", "Charisma ++"),
            },
            new Entry
            {
                Rank = MissionRank.R2, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[0],
                Title       = "Guild Mediation",
                Description = "Two rival guilds are on the brink of open war. Someone with the right words could prevent bloodshed.",
                Hints       = Hints(HeroStat.Charisma, "Diplomacy is the only valid weapon here.", "Charisma ++"),
            },
            new Entry
            {
                Rank = MissionRank.R3, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[0],
                Title       = "The Reluctant Queen",
                Description = "The queen of the neighboring city rejects the proposed alliance. She must be persuaded through diplomacy before the negotiation window closes.",
                Hints       = Hints(HeroStat.Charisma, "The queen only listens to those who impress her.", "Charisma +++"),
            },
            new Entry
            {
                Rank = MissionRank.R4, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[0],
                Title       = "The Bandit Leader",
                Description = "The leader of the highway bandits could become a valuable ally. The right words are the only key that opens that door.",
                Hints       = Hints(HeroStat.Charisma, "The bandits respect silver tongues, not strength... for now.", "Charisma +++"),
            },
            new Entry
            {
                Rank = MissionRank.R5, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[] { HeroStat.Intelligence },
                Title       = "The Cult in the Slums",
                Description = "A mysterious cult is gaining followers at an alarming rate in the slums. Its leader is charismatic and dangerous. Someone must infiltrate and earn their trust.",
                Hints       = Hints(HeroStat.Charisma, "To infiltrate, one must fit in and convince.", "Charisma +++ / Intelligence +"),
            },
            new Entry
            {
                Rank = MissionRank.R6, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[0],
                Title       = "The Festival Orator",
                Description = "The guild's annual festival needs an orator capable of bringing the crowd to ecstasy. The guild's reputation depends on tonight's performance.",
                Hints       = Hints(HeroStat.Charisma, "The audience judges presence, not content.", "Charisma +++"),
            },
            new Entry
            {
                Rank = MissionRank.R7, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[] { HeroStat.Intelligence },
                Title       = "The Ambassador's Secret",
                Description = "The ambassador from the foreign court holds a secret that affects the entire kingdom. To reveal it, they must first trust someone from the guild.",
                Hints       = Hints(HeroStat.Charisma, "Trust is earned, not demanded.", "Charisma ++++ / Intelligence +"),
            },
            new Entry
            {
                Rank = MissionRank.R8, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[0],
                Title       = "Ancestral Intermediary",
                Description = "The spirits of the ancestral territory request an intermediary who understands their language. Only someone with presence and a gift for people can communicate with them.",
                Hints       = Hints(HeroStat.Charisma, "The spirits do not listen to those who cannot listen to them.", "Charisma ++++"),
            },
            new Entry
            {
                Rank = MissionRank.R9, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[0],
                Title       = "The Living Legend",
                Description = "The legend of the fallen hero resonates in the hearts of the people. Someone must embody it convincingly so that people do not lose hope.",
                Hints       = Hints(HeroStat.Charisma, "What the people need to see cannot be faked with swords.", "Charisma ++++"),
            },
            new Entry
            {
                Rank = MissionRank.R10, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[] { HeroStat.Intelligence },
                Title       = "The Defender Before the Tribunal",
                Description = "The divine tribunal deliberates the fate of the city. A defender must speak on its behalf with words that move even the gods.",
                Hints       = Hints(HeroStat.Charisma, "Neither strength nor cleverness alone suffice here.", "Charisma +++++ / Intelligence +"),
            },
            new Entry
            {
                Rank = MissionRank.R11, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[0],
                Title       = "The Council Deadlock",
                Description = "The council of five lords has been deadlocked for three moons. The rift between them is irreversible without someone capable of breaking the impasse with the exact word.",
                Hints       = Hints(HeroStat.Charisma, "Only the exact word, at the exact moment, works here.", "Charisma +++++"),
            },
            new Entry
            {
                Rank = MissionRank.R12, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[0],
                Title       = "The True Name",
                Description = "The true name of the sleeping god must be spoken by worthy lips. Only one whose presence is unquestionable can awaken what was sealed ages ago.",
                Hints       = Hints(HeroStat.Charisma, "The name cannot be spoken by one who is unworthy.", "Charisma +++++"),
            },
        };
    }
}
