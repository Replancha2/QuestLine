using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Herramienta de editor que genera los 48 MissionData ScriptableObjects del catálogo.
/// Menú: Questline → Generate Mission Assets
///
/// 12 rangos × 4 palos (Strength / Dexterity / Intelligence / Charisma) = 48 misiones.
/// Naming: Mission_R{rank}_{Stat}.asset
/// Recompensas base: CoinsReward = rank × 3 | FameReward = rank × 5
/// Pistas: 2 por misión (índice 0 = vaga/barata, índice 1 = directa/cara)
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

            // No sobreescribir si ya existe
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
    // Tabla de misiones
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
        // Colores reutilizables
        Color green  = new Color(0.18f, 0.80f, 0.44f);   // confirmación directa
        Color orange = new Color(1.00f, 0.60f, 0.10f);   // pista vaga / precaución

        // Helper local para crear el array de dos pistas estándar
        MissionHint[] Hints(HeroStat stat, string vague, string direct)
            => new MissionHint[]
            {
                new MissionHint { HintText = vague,  HintColor = orange, RelatedStat = stat },
                new MissionHint { HintText = direct, HintColor = green,  RelatedStat = stat },
            };

        return new Entry[]
        {
            // ==================================================================
            // FUERZA (Str) — R1-R4: directas | R5-R8: ambiguas | R9-R12: engañosas
            // ==================================================================
            new Entry
            {
                Rank = MissionRank.R1, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[0],
                Title       = "Escombros en la mina",
                Description = "Los trabajadores de la mina están atrapados tras un derrumbe. Hay que apartar toneladas de roca a mano.",
                Hints       = Hints(HeroStat.Strength, "Requiere músculo, no cerebro.", "Fuerza ++"),
            },
            new Entry
            {
                Rank = MissionRank.R2, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[0],
                Title       = "Cargamento al otro lado",
                Description = "Un mercader necesita trasladar su pesado cargamento al otro lado del río antes de que llegue la tormenta.",
                Hints       = Hints(HeroStat.Strength, "El peso lo es todo aquí.", "Fuerza ++"),
            },
            new Entry
            {
                Rank = MissionRank.R3, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[0],
                Title       = "El troll del paso",
                Description = "Un troll bloquea el paso montañoso. La milicia local lleva semanas sin poder desalojarlo.",
                Hints       = Hints(HeroStat.Strength, "La fuerza bruta es la única opción.", "Fuerza +++"),
            },
            new Entry
            {
                Rank = MissionRank.R4, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[] { HeroStat.Dexterity },
                Title       = "Los muros de Ironhaven",
                Description = "Los muros de la ciudad se están derrumbando. Se necesita mano de obra pesada para reforzar los cimientos antes del invierno.",
                Hints       = Hints(HeroStat.Strength, "Es trabajo duro de construcción.", "Fuerza +++ / Destreza +"),
            },
            new Entry
            {
                Rank = MissionRank.R5, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[0],
                Title       = "La bestia del norte",
                Description = "La bestia que aterrorizaba el bosque del norte ha dejado de aparecer. Algo la tiene asustada. Hay que averiguar qué hay allá adentro.",
                Hints       = Hints(HeroStat.Strength, "Lo que sea que asustó a la bestia, no huirá de palabras.", "Fuerza ++"),
            },
            new Entry
            {
                Rank = MissionRank.R6, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[0],
                Title       = "El campeón invicto",
                Description = "El torneo anual de la ciudad tiene un campeón que lleva tres años sin ser derrotado. El gremio apuesta fuerte este año.",
                Hints       = Hints(HeroStat.Strength, "Aquí gana quien resiste más.", "Fuerza +++"),
            },
            new Entry
            {
                Rank = MissionRank.R7, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[] { HeroStat.Intelligence },
                Title       = "El frío de las profundidades",
                Description = "La mazmorra bajo el castillo emana un frío antinatural. Alguien debe llegar al fondo y romper lo que sea que esté allí.",
                Hints       = Hints(HeroStat.Strength, "El foco no es la mente, es resistir lo físico.", "Fuerza +++ / Inteligencia +"),
            },
            new Entry
            {
                Rank = MissionRank.R8, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[0],
                Title       = "La ofrenda de acero",
                Description = "Los dioses de la guerra reclaman una ofrenda de acero y voluntad. El ritual exige que el enviado soporte pruebas de resistencia extrema.",
                Hints       = Hints(HeroStat.Strength, "Los dioses no premian la astucia aquí.", "Fuerza ++++"),
            },
            new Entry
            {
                Rank = MissionRank.R9, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[0],
                Title       = "Ecos desde las catacumbas",
                Description = "El eco de los muertos susurra desde las catacumbas bajo la ciudad. La verdad yace sepultada bajo toneladas de piedra y silencio.",
                Hints       = Hints(HeroStat.Strength, "Las palabras no mueven la roca.", "Fuerza ++++"),
            },
            new Entry
            {
                Rank = MissionRank.R10, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[] { HeroStat.Charisma },
                Title       = "El pacto olvidado",
                Description = "Un pacto olvidado exige ser honrado antes del próximo eclipse. El representante del gremio debe presentarse ante el concilio ancestral y demostrar su valía.",
                Hints       = Hints(HeroStat.Strength, "El concilio prueba el cuerpo antes que el espíritu.", "Fuerza ++++ / Carisma +"),
            },
            new Entry
            {
                Rank = MissionRank.R11, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[0],
                Title       = "El último guardián",
                Description = "El último guardián de la torre ha caído. Solo queda el umbral, y lo que se oculta más allá no tiene piedad con los débiles.",
                Hints       = Hints(HeroStat.Strength, "Quien entre debe poder con lo que los guardianes no pudieron.", "Fuerza +++++"),
            },
            new Entry
            {
                Rank = MissionRank.R12, StatTag = "Str", PrimaryStat = HeroStat.Strength,
                SecondaryStats = new HeroStat[] { HeroStat.Dexterity },
                Title       = "La cadena del primordial",
                Description = "La cadena que ata al primordial se rompe eslabón a eslabón. Alguien debe sostenerla con sus propias manos hasta que lleguen los refuerzos.",
                Hints       = Hints(HeroStat.Strength, "Solo la fuerza pura puede sostener lo insostenible.", "Fuerza +++++ / Destreza ++"),
            },

            // ==================================================================
            // DESTREZA (Dex) — R1-R4: directas | R5-R8: ambiguas | R9-R12: engañosas
            // ==================================================================
            new Entry
            {
                Rank = MissionRank.R1, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[0],
                Title       = "Sombras en el bosque",
                Description = "El bosque sombrío necesita a alguien que pueda moverse sin ser visto entre las sombras durante toda la noche.",
                Hints       = Hints(HeroStat.Dexterity, "No se trata de fuerza, sino de sigilo.", "Destreza ++"),
            },
            new Entry
            {
                Rank = MissionRank.R2, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[0],
                Title       = "El carterista del mercado",
                Description = "Un carterista opera en el mercado central. Los guardias llevan semanas sin poder atraparlo.",
                Hints       = Hints(HeroStat.Dexterity, "Hay que moverse más rápido que él.", "Destreza ++"),
            },
            new Entry
            {
                Rank = MissionRank.R3, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[0],
                Title       = "Robo en el archivo",
                Description = "Una figura encapuchada roba documentos del archivo del gremio por las noches. Hay que tenderle una trampa antes del amanecer.",
                Hints       = Hints(HeroStat.Dexterity, "El ladrón no puede verse venir.", "Destreza +++"),
            },
            new Entry
            {
                Rank = MissionRank.R4, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[0],
                Title       = "Mensajero urgente",
                Description = "Un mensajero urgente debe cruzar territorio enemigo sin ser detectado. El paquete no debe caer en manos equivocadas.",
                Hints       = Hints(HeroStat.Dexterity, "Visibilidad cero durante todo el trayecto.", "Destreza +++"),
            },
            new Entry
            {
                Rank = MissionRank.R5, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[0],
                Title       = "Los túneles bajo la ciudad",
                Description = "La red de túneles bajo la ciudad es un laberinto de trampas y pasajes ocultos. El paquete debe llegar al otro lado antes del alba.",
                Hints       = Hints(HeroStat.Dexterity, "No hay mapa que valga aquí; todo es instinto y reflejos.", "Destreza ++"),
            },
            new Entry
            {
                Rank = MissionRank.R6, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[] { HeroStat.Intelligence },
                Title       = "El relicario profanado",
                Description = "El relicario del templo ha sido profanado. Los ladrones se escabulleron sin dejar rastro visible. Alguien debe seguirlos por donde ellos pasaron.",
                Hints       = Hints(HeroStat.Dexterity, "El camino de los ladrones no es fácil de seguir.", "Destreza +++ / Inteligencia +"),
            },
            new Entry
            {
                Rank = MissionRank.R7, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[0],
                Title       = "Las trampas de Voss",
                Description = "Las trampas mecánicas del ingeniero Voss son legendarias. Su mansión está repleta de ellas, y hay algo dentro que el gremio necesita.",
                Hints       = Hints(HeroStat.Dexterity, "La mente no te sacará de una trampa de resortes.", "Destreza ++++"),
            },
            new Entry
            {
                Rank = MissionRank.R8, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[] { HeroStat.Charisma },
                Title       = "La danza del viento",
                Description = "La tribu del viento ejecuta un ritual que solo puede ser completado por manos bendecidas con precisión absoluta. El gremio ha sido invitado a participar.",
                Hints       = Hints(HeroStat.Dexterity, "El ritual premia la gracia, no la fuerza.", "Destreza ++++ / Carisma +"),
            },
            new Entry
            {
                Rank = MissionRank.R9, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[0],
                Title       = "El velo de la luna nueva",
                Description = "El velo entre lo visible y lo oculto se adelgaza con la luna nueva. Solo quien sabe moverse en los espacios entre las sombras puede cruzarlo sin consecuencias.",
                Hints       = Hints(HeroStat.Dexterity, "No es la mente lo que cruza ese velo.", "Destreza ++++"),
            },
            new Entry
            {
                Rank = MissionRank.R10, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[0],
                Title       = "Las cuerdas del destino",
                Description = "Las cuerdas del arpa del destino vibran con la frecuencia equivocada. Solo dedos entrenados durante años pueden reafinarlas antes de que la melodía se rompa.",
                Hints       = Hints(HeroStat.Dexterity, "La melodía se toca con las manos, no con la cabeza.", "Destreza +++++"),
            },
            new Entry
            {
                Rank = MissionRank.R11, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[] { HeroStat.Intelligence },
                Title       = "El asesino de sombras",
                Description = "El asesino de sombras ha marcado a un noble del concilio. La ventana para interceptarlo es mínima y el entorno está lleno de obstáculos.",
                Hints       = Hints(HeroStat.Dexterity, "El contacto debe ser limpio y silencioso.", "Destreza +++++ / Inteligencia +"),
            },
            new Entry
            {
                Rank = MissionRank.R12, StatTag = "Dex", PrimaryStat = HeroStat.Dexterity,
                SecondaryStats = new HeroStat[0],
                Title       = "El hilo suelto del tapiz",
                Description = "El último hilo del tapiz cósmico se ha soltado. Quien lo anude cambiará el patrón de la realidad para siempre. El tejedor necesita manos que no tiemblen.",
                Hints       = Hints(HeroStat.Dexterity, "La realidad no perdona la torpeza.", "Destreza +++++"),
            },

            // ==================================================================
            // INTELIGENCIA (Int) — R1-R4: directas | R5-R8: ambiguas | R9-R12: engañosas
            // ==================================================================
            new Entry
            {
                Rank = MissionRank.R1, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[0],
                Title       = "Los sellos arcanos",
                Description = "Una antigua cripta ha sido profanada. Alguien debe descifrar los sellos arcanos grabados en la entrada para sellarla de nuevo.",
                Hints       = Hints(HeroStat.Intelligence, "Los sellos son un problema de conocimiento.", "Inteligencia ++"),
            },
            new Entry
            {
                Rank = MissionRank.R2, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[0],
                Title       = "La fórmula del alquimista",
                Description = "El alquimista del pueblo necesita un ingrediente raro. La fórmula para sintetizarlo es extremadamente compleja.",
                Hints       = Hints(HeroStat.Intelligence, "Sin conocimiento de alquimia, no hay nada que hacer.", "Inteligencia ++"),
            },
            new Entry
            {
                Rank = MissionRank.R3, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[0],
                Title       = "Registros alterados",
                Description = "Los registros del archivo real han sido manipulados. Alguien debe encontrar las discrepancias antes de que el informe llegue al rey.",
                Hints       = Hints(HeroStat.Intelligence, "Hace falta ojo analítico y conocimiento de contabilidad.", "Inteligencia +++"),
            },
            new Entry
            {
                Rank = MissionRank.R4, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[0],
                Title       = "El lago de cristal",
                Description = "Un hechizo erróneo ha transformado el lago en cristal sólido. Debe revertirse antes del alba o los cultivos del valle quedarán sin riego.",
                Hints       = Hints(HeroStat.Intelligence, "La solución es arcana, no física.", "Inteligencia +++"),
            },
            new Entry
            {
                Rank = MissionRank.R5, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[0],
                Title       = "Los acertijos del oráculo",
                Description = "El oráculo de la montaña habla en acertijos. La respuesta correcta desbloqueará el conocimiento que el gremio necesita. La respuesta incorrecta tiene consecuencias.",
                Hints       = Hints(HeroStat.Intelligence, "Los acertijos no se responden con músculo.", "Inteligencia ++"),
            },
            new Entry
            {
                Rank = MissionRank.R6, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[0],
                Title       = "El volumen prohibido",
                Description = "La biblioteca prohibida contiene un volumen que nadie ha podido leer sin perder la cordura. El gremio necesita la información que contiene.",
                Hints       = Hints(HeroStat.Intelligence, "La mente debe ser excepcionalmente fuerte para resistirlo.", "Inteligencia +++"),
            },
            new Entry
            {
                Rank = MissionRank.R7, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[] { HeroStat.Dexterity },
                Title       = "El golem errático",
                Description = "El golem del maestro Theron actúa de forma errática. Sus runas de control parecen contradictorias entre sí. Debe reprogramarse antes de que cause daño.",
                Hints       = Hints(HeroStat.Intelligence, "El golem obedece a quien entienda sus runas.", "Inteligencia ++++ / Destreza +"),
            },
            new Entry
            {
                Rank = MissionRank.R8, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[0],
                Title       = "La constelación imposible",
                Description = "El mapa de las estrellas muestra una constelación que no debería existir según todos los textos conocidos. Alguien debe determinar qué significa antes de que amanezca.",
                Hints       = Hints(HeroStat.Intelligence, "Esto no se resuelve con espadas ni palabras.", "Inteligencia ++++"),
            },
            new Entry
            {
                Rank = MissionRank.R9, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[] { HeroStat.Charisma },
                Title       = "Los secretos del consejero",
                Description = "La mente del consejero real guarda secretos que podrían cambiar el reino. Alguien debe extraerlos sin que el consejero sepa que los ha revelado.",
                Hints       = Hints(HeroStat.Intelligence, "No basta con parecer amigable; hay que entender lo que no se dice.", "Inteligencia ++++ / Carisma +"),
            },
            new Entry
            {
                Rank = MissionRank.R10, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[0],
                Title       = "El sello del solsticio",
                Description = "El sello del pacto primordial requiere ser renovado antes del solsticio. La fórmula de renovación se perdió hace siglos. Hay que reconstruirla.",
                Hints       = Hints(HeroStat.Intelligence, "Quien lo reconstruya debe conocer el lenguaje de los antiguos.", "Inteligencia +++++"),
            },
            new Entry
            {
                Rank = MissionRank.R11, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[0],
                Title       = "El undécimo componente",
                Description = "La fórmula del veneno que infecta el río tiene doce componentes. Once son conocidos. El duodécimo ha sido borrado de todos los registros. El tiempo corre.",
                Hints       = Hints(HeroStat.Intelligence, "La solución está en textos que casi nadie puede leer.", "Inteligencia +++++"),
            },
            new Entry
            {
                Rank = MissionRank.R12, StatTag = "Int", PrimaryStat = HeroStat.Intelligence,
                SecondaryStats = new HeroStat[0],
                Title       = "El error en la ecuación",
                Description = "La ecuación que sostiene la realidad tiene un error. Solo alguien cuya mente trascienda los límites del conocimiento conocido puede encontrarlo y corregirlo.",
                Hints       = Hints(HeroStat.Intelligence, "Ninguna habilidad física o social puede resolver esto.", "Inteligencia +++++"),
            },

            // ==================================================================
            // CARISMA (Cha) — R1-R4: directas | R5-R8: ambiguas | R9-R12: engañosas
            // ==================================================================
            new Entry
            {
                Rank = MissionRank.R1, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[0],
                Title       = "El noble corrupto",
                Description = "Los comerciantes del puerto necesitan convencer a un noble corrupto de que retire el impuesto ilegal que está arruinando el negocio.",
                Hints       = Hints(HeroStat.Charisma, "No se resuelve a puñetazos.", "Carisma ++"),
            },
            new Entry
            {
                Rank = MissionRank.R2, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[0],
                Title       = "Mediación entre gremios",
                Description = "Dos gremios rivales están al borde de la guerra abierta. Alguien con la palabra correcta podría evitar el derramamiento de sangre.",
                Hints       = Hints(HeroStat.Charisma, "La diplomacia es la única arma válida aquí.", "Carisma ++"),
            },
            new Entry
            {
                Rank = MissionRank.R3, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[0],
                Title       = "La reina reticente",
                Description = "La reina de la ciudad vecina rechaza la alianza propuesta. Debe ser persuadida con diplomacia antes de que la ventana de negociación se cierre.",
                Hints       = Hints(HeroStat.Charisma, "La reina solo escucha a quien la impresiona.", "Carisma +++"),
            },
            new Entry
            {
                Rank = MissionRank.R4, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[0],
                Title       = "El jefe bandido",
                Description = "El líder de los bandidos del camino real podría convertirse en un aliado valioso. Las palabras correctas son la única llave que abre esa puerta.",
                Hints       = Hints(HeroStat.Charisma, "Los bandidos respetan la labia, no la fuerza... por ahora.", "Carisma +++"),
            },
            new Entry
            {
                Rank = MissionRank.R5, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[] { HeroStat.Intelligence },
                Title       = "El culto en el barrio bajo",
                Description = "Un culto misterioso gana adeptos a ritmo alarmante en el barrio bajo. Su líder es carismático y peligroso. Alguien debe infiltrarse y ganarse su confianza.",
                Hints       = Hints(HeroStat.Charisma, "Para infiltrarse, hay que encajar y convencer.", "Carisma +++ / Inteligencia +"),
            },
            new Entry
            {
                Rank = MissionRank.R6, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[0],
                Title       = "El orador del festival",
                Description = "El festival anual del gremio necesita un orador capaz de llevar a la multitud al éxtasis. La reputación del gremio depende de la actuación de esta noche.",
                Hints       = Hints(HeroStat.Charisma, "El público juzga la presencia, no el contenido.", "Carisma +++"),
            },
            new Entry
            {
                Rank = MissionRank.R7, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[] { HeroStat.Intelligence },
                Title       = "El secreto del embajador",
                Description = "El embajador de la corte extranjera guarda un secreto que afecta al reino entero. Para revelarlo, primero debe confiar en alguien del gremio.",
                Hints       = Hints(HeroStat.Charisma, "La confianza se gana, no se exige.", "Carisma ++++ / Inteligencia +"),
            },
            new Entry
            {
                Rank = MissionRank.R8, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[0],
                Title       = "Intermediario ancestral",
                Description = "Los espíritus del territorio ancestral piden un intermediario que entienda su lenguaje. Solo alguien con presencia y don de gentes puede comunicarse con ellos.",
                Hints       = Hints(HeroStat.Charisma, "Los espíritus no escuchan a quien no sabe escucharlos.", "Carisma ++++"),
            },
            new Entry
            {
                Rank = MissionRank.R9, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[0],
                Title       = "La leyenda encarnada",
                Description = "La leyenda del héroe caído resuena en los corazones del pueblo. Alguien debe encarnarla de manera convincente para que la gente no pierda la esperanza.",
                Hints       = Hints(HeroStat.Charisma, "Lo que el pueblo necesita ver no puede fingirse con espadas.", "Carisma ++++"),
            },
            new Entry
            {
                Rank = MissionRank.R10, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[] { HeroStat.Intelligence },
                Title       = "El defensor ante el tribunal",
                Description = "El tribunal divino delibera sobre el destino de la ciudad. Un defensor debe hablar por ella con palabras que muevan hasta a los dioses.",
                Hints       = Hints(HeroStat.Charisma, "Ni la fuerza ni la astucia solas bastan aquí.", "Carisma +++++ / Inteligencia +"),
            },
            new Entry
            {
                Rank = MissionRank.R11, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[0],
                Title       = "El punto muerto del concilio",
                Description = "El concilio de los cinco señores lleva tres lunas en punto muerto. La fractura entre ellos es irreversible sin alguien capaz de romper la impasse con la palabra exacta.",
                Hints       = Hints(HeroStat.Charisma, "Solo la palabra exacta, en el momento exacto, funciona aquí.", "Carisma +++++"),
            },
            new Entry
            {
                Rank = MissionRank.R12, StatTag = "Cha", PrimaryStat = HeroStat.Charisma,
                SecondaryStats = new HeroStat[0],
                Title       = "El nombre verdadero",
                Description = "El nombre verdadero del dios durmiente debe ser pronunciado por labios dignos. Solo aquel cuya presencia sea incontestable puede despertar lo que fue sellado hace eras.",
                Hints       = Hints(HeroStat.Charisma, "El nombre no puede ser pronunciado por quien no es digno.", "Carisma +++++"),
            },
        };
    }
}
