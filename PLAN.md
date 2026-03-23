# PLAN DE DESARROLLO — QUESTLINE (JAME GAM)
> **Equipo:** 4 desarrolladores | **Motor:** Unity 2D (URP) | **Duración:** 7 días
> **Géneros:** Micromanagement + Roguelike
> **Leyenda de estado:** `[ ]` Pendiente · `[~]` En progreso · `[x]` Completado · `[!]` Bloqueado

---

## RESUMEN DEL JUEGO

**Questline** es un juego de micromanagement con elementos roguelike donde el jugador administra una guild de héroes. Los héroes llegan aleatoriamente con stats generados proceduralmente. El jugador debe asignarles misiones arrastrando cartas, infiriendo qué estadística encaja mejor con la descripción de la misión. Los fallos matan al héroe y penalizan al jugador; demasiados fallos significan game over. Con monedas de propina se compran buffs y mejoras. Las runs se vuelven más fáciles de interpretar a medida que el jugador progresa.

**Stats de héroe:** Fuerza · Destreza · Inteligencia · Carisma · XP
**Mecánica core:** Drag & Drop de carta-misión a héroe
**Roguelike loop:** Cada run desbloquea más pistas en las misiones + buffs acumulables

---

## FASES Y CRONOGRAMA

| Fase | Nombre | Días | Responsable sugerido |
|------|--------|------|----------------------|
| 0 | Setup y arquitectura | Día 1 (mañana) | Todos |
| 1 | Sistemas de datos core | Día 1-2 | Dev A + Dev B |
| 2 | Loop de gameplay | Día 2-3 | Dev C + Dev D |
| 3 | Economía y roguelike | Día 3-4 | Dev A + Dev C |
| 4 | UI y feedback visual | Día 4-5 | Dev B + Dev D |
| 5 | Audio, VFX y balance | Día 5-6 | Todos |
| 6 | Testing y build final | Día 6-7 | Todos |

---

## FASE 0 — SETUP Y ARQUITECTURA
**Objetivo:** Que todos los devs tengan el proyecto funcionando, con estructura de carpetas clara, convenciones definidas y escenas base creadas.
**Duración:** Medio día 1

---

### TAREA 0.1 — Estructura de carpetas del proyecto
**Estado:** `[ ]`
**Responsable:** Dev A
**Duración estimada:** 30 min
**Dependencias:** Ninguna

**Descripción:**
Crear la jerarquía de carpetas en `Assets/` para que el equipo sepa exactamente dónde va cada tipo de archivo. Una estructura clara evita conflictos de merge y confusiones.

**Pasos:**
1. Dentro de `Assets/`, crear las siguientes carpetas:
   ```
   Assets/
   ├── _Scenes/
   ├── Scripts/
   │   ├── Core/          ← GameManager, RunManager, EventBus
   │   ├── Data/          ← ScriptableObjects (Hero, Mission, Buff)
   │   ├── Heroes/        ← HeroController, HeroSpawner, HeroQueue
   │   ├── Missions/      ← MissionCard, MissionDeck, MissionEvaluator
   │   ├── Economy/       ← CoinJar, ShopManager
   │   ├── UI/            ← HUD, CardDrag, TooltipManager, Screens
   │   └── Utils/         ← Extensions, Constants, Enums
   ├── Art/
   │   ├── Sprites/
   │   ├── UI/
   │   └── Animations/
   ├── Audio/
   │   ├── SFX/
   │   └── Music/
   ├── Prefabs/
   │   ├── Heroes/
   │   ├── Missions/
   │   └── UI/
   └── Data/
       ├── Heroes/        ← HeroData ScriptableObjects
       ├── Missions/      ← MissionData ScriptableObjects
       └── Buffs/         ← BuffData ScriptableObjects
   ```
2. Agregar un `.gitkeep` vacío en cada carpeta vacía para que Git las trackee.
3. Comunicar al equipo la estructura por el canal de comunicación del equipo.

**Criterio de éxito:** Todas las carpetas existen y están commiteadas.

---

### TAREA 0.2 — Configuración de Git y convenciones del equipo
**Estado:** `[ ]`
**Responsable:** Dev B
**Duración estimada:** 30 min
**Dependencias:** Ninguna

**Descripción:**
Establecer el repositorio, `.gitignore` apropiado para Unity, y acordar convenciones de nomenclatura y ramas.

**Pasos:**
1. Asegurarse de que el proyecto tenga un `.gitignore` para Unity. Contenido mínimo necesario:
   ```
   /[Ll]ibrary/
   /[Tt]emp/
   /[Oo]bj/
   /[Bb]uild/
   /[Bb]uilds/
   /[Ll]ogs/
   /[Uu]ser[Ss]ettings/
   *.csproj
   *.unityproj
   *.sln
   *.suo
   *.tmp
   *.user
   *.userprefs
   *.pidb
   *.booproj
   *.svd
   *.pdb
   *.mdb
   *.opendb
   *.VC.db
   /[Aa]ssets/[Aa]sset[Ss]tore-5.x/
   ```
2. Definir estructura de ramas:
   - `main` → build estable
   - `dev` → integración continua
   - `feature/nombre-feature` → trabajo individual
3. Acordar convenciones de código:
   - Clases: `PascalCase` (ej: `HeroController`)
   - Variables privadas: `_camelCase` (ej: `_heroData`)
   - Variables públicas/propiedades: `PascalCase`
   - Métodos: `PascalCase` (ej: `AssignMission()`)
   - Constantes: `UPPER_SNAKE_CASE`
   - ScriptableObjects: sufijo `SO` o `Data` (ej: `HeroData`, `MissionSO`)
4. Subir estructura inicial al repositorio.

**Criterio de éxito:** Repo configurado, todos pueden hacer push/pull sin problemas.

---

### TAREA 0.3 — Crear escenas base
**Estado:** `[ ]`
**Responsable:** Dev C
**Duración estimada:** 45 min
**Dependencias:** 0.1

**Descripción:**
Crear las escenas del juego en Unity con los GameObjects placeholder necesarios para que los distintos sistemas puedan integrarse desde el primer día.

**Pasos:**
1. En `Assets/_Scenes/`, crear las siguientes escenas:
   - `MainMenu.unity` — Pantalla de inicio
   - `Gameplay.unity` — Escena principal del juego
   - `GameOver.unity` — Pantalla de game over
2. En `Gameplay.unity`, crear los siguientes GameObjects vacíos en la jerarquía:
   ```
   Gameplay
   ├── _MANAGERS/
   │   ├── GameManager
   │   ├── RunManager
   │   ├── HeroSpawner
   │   ├── MissionDeck
   │   ├── CoinJar
   │   └── AudioManager
   ├── _UI/
   │   ├── HUD (Canvas)
   │   ├── HeroQueue (Canvas)
   │   ├── MissionArea (Canvas)
   │   ├── ShopPanel (Canvas)
   │   └── TooltipLayer (Canvas)
   ├── _GAMEPLAY/
   │   ├── HeroSlots (4 slots vacíos)
   │   └── MissionCardArea
   └── _VFX/
       └── ParticleRoot
   ```
3. Configurar Build Settings con el orden de escenas: MainMenu(0) → Gameplay(1) → GameOver(2).
4. Configurar la cámara en la escena Gameplay para resolución 1920×1080.

**Criterio de éxito:** Las 3 escenas existen, se puede hacer SceneManager.LoadScene entre ellas sin errores.

---

### TAREA 0.4 — EventBus / sistema de eventos global
**Estado:** `[ ]`
**Responsable:** Dev D
**Duración estimada:** 1 hora
**Dependencias:** 0.1

**Descripción:**
Implementar un EventBus estático para comunicación desacoplada entre sistemas. Esto evita referencias directas entre componentes y facilita el trabajo paralelo del equipo.

**Pasos:**
1. Crear `Assets/Scripts/Core/EventBus.cs`:
   ```csharp
   // EventBus genérico con soporte para eventos con y sin payload
   // Implementar como clase estática con diccionario de Action<T>
   // Métodos: Subscribe<T>, Unsubscribe<T>, Publish<T>
   ```
2. Crear `Assets/Scripts/Core/GameEvents.cs` — archivo con todas las clases de eventos:
   ```csharp
   // Eventos a definir:
   public class OnHeroArrived { public HeroData Hero; }
   public class OnHeroLeft { public HeroData Hero; public bool Rejected; }
   public class OnHeroDied { public HeroData Hero; }
   public class OnMissionAssigned { public MissionData Mission; public HeroData Hero; }
   public class OnMissionCompleted { public MissionData Mission; public HeroData Hero; public int CoinsEarned; }
   public class OnMissionFailed { public MissionData Mission; public HeroData Hero; }
   public class OnCoinEarned { public int Amount; }
   public class OnCoinSpent { public int Amount; }
   public class OnFailPenalty { public int FailCount; public int MaxFails; }
   public class OnGameOver { }
   public class OnRunStart { public int RunNumber; }
   public class OnRunEnd { public int RunNumber; public int Score; }
   public class OnBuffPurchased { public BuffData Buff; }
   public class OnMissionsreshuffled { }
   public class OnMissionDiscarded { public MissionInstance Mission; }
   ```
3. Crear `Assets/Scripts/Core/GameManager.cs`:
   - Singleton persistente (DontDestroyOnLoad)
   - Mantiene el estado global: `RunNumber`, `FailCount`, `MaxFails`, `CurrentCoins`, `Score`
   - Expone métodos: `StartRun()`, `EndRun()`, `RegisterFail()`, `AddCoins()`, `SpendCoins()`
   - Escucha `OnHeroDied` para llamar `RegisterFail()`
   - Al llegar a `MaxFails`, publica `OnGameOver`
4. Probar con un test rápido en el editor: suscribirse a un evento, publicarlo, verificar que el handler se invoca.

**Criterio de éxito:** El EventBus compila sin errores. Se puede publicar y recibir eventos entre dos scripts sin referencia directa.

---

## FASE 1 — SISTEMAS DE DATOS CORE
**Objetivo:** Definir las estructuras de datos de héroe, misión y buff como ScriptableObjects. Implementar la generación procedural de héroes y el mazo de misiones.
**Duración:** Día 1 (tarde) – Día 2

---

### TAREA 1.1 — HeroData ScriptableObject
**Estado:** `[ ]`
**Responsable:** Dev A
**Duración estimada:** 1.5 horas
**Dependencias:** 0.1, 0.4

**Descripción:**
Definir la estructura de datos de un héroe como ScriptableObject base y la clase en runtime `HeroInstance` que representa a un héroe vivo con stats modificables.

**Pasos:**
1. Crear `Assets/Scripts/Data/HeroData.cs` (ScriptableObject — plantilla base):
   ```csharp
   [CreateAssetMenu(menuName = "Questline/Hero Data")]
   public class HeroData : ScriptableObject {
       public string HeroName;
       public Sprite Portrait;
       public HeroRarity Rarity; // Common, Rare, Epic, Boss

       // Rangos para generación aleatoria
       public Vector2Int StrengthRange;    // ej: (1, 10)
       public Vector2Int DexterityRange;
       public Vector2Int IntelligenceRange;
       public Vector2Int CharismaRange;
       public Vector2Int XPRange;          // XP mínima que exige el héroe

       // Personalidad — afecta el timer de paciencia
       public float PatienceMultiplier;    // 0.5=impaciente, 2.0=muy paciente
       public float BasePatience;          // segundos base de espera

       // Descripción de sabor
       [TextArea] public string FlavorText;
   }
   ```
2. Crear `Assets/Scripts/Heroes/HeroInstance.cs` (clase en runtime, NO ScriptableObject):
   ```csharp
   public class HeroInstance {
       // Stats base (generados al instanciar)
       public string Name;
       public int Strength;
       public int Dexterity;
       public int Intelligence;
       public int Charisma;
       public int ExperienceLevel;    // nivel del héroe
       public int MinMissionXP;       // XP mínima de misión que acepta
       public HeroRarity Rarity;
       public Sprite Portrait;
       public float Patience;         // segundos antes de irse

       // Estado en runtime
       public bool IsWaiting;
       public bool IsDead;

       // Método de construcción
       public static HeroInstance Generate(HeroData template) { ... }

       // Verifica si el héroe acepta una misión (stats y XP)
       public bool WillAcceptMission(MissionInstance mission) { ... }

       // Calcula probabilidad de éxito (0.0 a 1.0)
       public float CalculateSuccessChance(MissionInstance mission) { ... }
   }
   ```
3. Crear `Assets/Scripts/Utils/Enums.cs` con:
   ```csharp
   public enum HeroRarity { Common, Rare, Epic, Boss }
   public enum HeroStat { Strength, Dexterity, Intelligence, Charisma }
   public enum MissionDifficulty { Easy, Normal, Hard, Extreme }
   public enum BuffType { HeroStatBoost, MissionHintReveal, MissionReshuffle, PatienceBoost, FailProtection }
   ```
4. Crear 4-6 HeroData ScriptableObjects de ejemplo en `Assets/Data/Heroes/`:
   - `Hero_Knight.asset` (Fuerza alta)
   - `Hero_Rogue.asset` (Destreza alta)
   - `Hero_Mage.asset` (Inteligencia alta)
   - `Hero_Bard.asset` (Carisma alto)
   - `Hero_Boss_Dragon.asset` (Boss con stats extremos)
5. Implementar el método `CalculateSuccessChance`:
   - Obtener el stat primario de la misión
   - Fórmula: `chance = (heroStat / missionRequirement).Clamp(0, 1)`
   - Aplicar bonus por stats secundarios: `+5% por cada stat secundario que supera el umbral`

**Criterio de éxito:** Se puede generar un `HeroInstance` desde cualquier `HeroData`. Los valores de stats están dentro del rango definido. `CalculateSuccessChance` devuelve valores coherentes.

---

### TAREA 1.2 — MissionData ScriptableObject
**Estado:** `[ ]`
**Responsable:** Dev B
**Duración estimada:** 2 horas
**Dependencias:** 1.1 (necesita Enums)

**Descripción:**
Definir la estructura de datos de una misión. Las misiones tienen requisitos ocultos (stat principal + secundarios), descripción de sabor ambigua, y dificultad escalable. También incluye el sistema de pistas progresivas del roguelike.

**Pasos:**
1. Crear `Assets/Scripts/Data/MissionData.cs`:
   ```csharp
   [CreateAssetMenu(menuName = "Questline/Mission Data")]
   public class MissionData : ScriptableObject {
       [Header("Identidad")]
       public string MissionTitle;
       [TextArea] public string Description;       // Descripción ambigua para el jugador
       public Sprite MissionArt;
       public MissionDifficulty Difficulty;

       [Header("Requisitos (OCULTOS al jugador)")]
       public HeroStat PrimaryStat;               // El stat que más importa
       public int PrimaryStatRequirement;         // Umbral mínimo recomendado
       public HeroStat[] SecondaryStats;          // Stats de apoyo (opcionales)
       public int[] SecondaryStatRequirements;
       public int MinHeroLevel;                   // Nivel mínimo del héroe

       [Header("Recompensas")]
       public int CoinsReward;                    // Propina base al completar
       public int XPReward;                       // XP que gana el héroe
       public int ScoreReward;                    // Puntos para el marcador

       [Header("Pistas progresivas (Roguelike)")]
       // Pistas que se revelan según el nivel de progreso del jugador
       public MissionHint[] Hints;                // Array de pistas con nivel de desbloqueo

       [Header("Flags especiales")]
       public bool IsBossMission;                 // Requiere héroe Boss
       public float TimeLimitMultiplier;          // 1.0 = normal, 0.5 = urgente
   }

   [System.Serializable]
   public class MissionHint {
       public string HintText;        // ej: "Fuerza++" o "Inteligencia"
       public Color HintColor;        // Verde=bueno, Naranja=precaución
       public int UnlockAtRunLevel;   // A partir de qué run se muestra
       public HeroStat RelatedStat;   // Stat al que hace referencia
   }
   ```
2. Crear `Assets/Scripts/Missions/MissionInstance.cs`:
   ```csharp
   public class MissionInstance {
       public MissionData Template;
       public bool IsCompleted;
       public bool IsFailed;
       public int CurrentRunLevel;   // Para filtrar qué hints mostrar

       // Devuelve solo las pistas desbloqueadas para el run actual
       public List<MissionHint> GetRevealedHints(int runLevel) { ... }

       // Fábrica
       public static MissionInstance FromData(MissionData data, int runLevel) { ... }
   }
   ```
3. Crear mínimo 15 MissionData ScriptableObjects en `Assets/Data/Missions/`:
   - 5 misiones Fáciles (stat evidente en la descripción si eres observador)
   - 5 misiones Normales (stat ambiguo)
   - 3 misiones Difíciles (descripción engañosa)
   - 2 misiones de Boss

   **Ejemplos de descripción ambigua:**
   - *"El Bosque Sombrío necesita a alguien que pueda moverse sin ser visto entre las sombras."* → PrimaryStat: Destreza
   - *"Los comerciantes del puerto necesitan convencer a un noble corrupto."* → PrimaryStat: Carisma
   - *"Una antigua cripta ha sido profanada. Alguien debe descifrar los sellos arcanos."* → PrimaryStat: Inteligencia
   - *"Los muros de Ironhaven se están derrumbando. Se necesita mano de obra pesada."* → PrimaryStat: Fuerza
4. Para cada misión, diseñar al menos 2 pistas:
   - Pista 1: se desbloquea en run 2 (texto vago, ej: "Requiere habilidad física")
   - Pista 2: se desbloquea en run 4 (texto directo, ej: "Fuerza ++", color verde)

**Criterio de éxito:** Se puede crear una MissionInstance desde cualquier MissionData. `GetRevealedHints(runLevel)` filtra correctamente según el nivel de run.

---

### TAREA 1.3 — BuffData ScriptableObject
**Estado:** `[ ]`
**Responsable:** Dev A
**Duración estimada:** 1 hora
**Dependencias:** 1.1, 1.2

**Descripción:**
Definir la estructura de los buffs comprables con monedas. Los buffs son el corazón de la progresión roguelike: hacen las misiones más fáciles de leer o potencian a los héroes.

**Pasos:**
1. Crear `Assets/Scripts/Data/BuffData.cs`:
   ```csharp
   [CreateAssetMenu(menuName = "Questline/Buff Data")]
   public class BuffData : ScriptableObject {
       public string BuffName;
       [TextArea] public string Description;
       public Sprite Icon;
       public int Cost;               // En monedas
       public BuffType Type;
       public bool IsPermanent;       // Persiste en la run o solo un turno

       // Según el tipo:
       public HeroStat AffectedStat;        // Para HeroStatBoost
       public int StatBoostAmount;          // +N al stat
       public int HintRunLevelBonus;        // Para MissionHintReveal (reduce el nivel de unlock)
       public float PatienceBoostPercent;   // Para PatienceBoost
       public int FailProtectionCount;      // Para FailProtection (absorbe N fallos)
   }
   ```
2. Crear los siguientes buffs en `Assets/Data/Buffs/`:
   - `Buff_StrBoost.asset` — +3 Fuerza a todos los héroes, costo: 5 coins
   - `Buff_DexBoost.asset` — +3 Destreza a todos los héroes, costo: 5 coins
   - `Buff_IntBoost.asset` — +3 Inteligencia a todos los héroes, costo: 5 coins
   - `Buff_ChaBoost.asset` — +3 Carisma a todos los héroes, costo: 5 coins
   - `Buff_Patience.asset` — +50% tiempo de paciencia, costo: 8 coins
   - `Buff_HintReveal.asset` — Desbloquea pistas 1 run antes, costo: 10 coins
   - `Buff_Reshuffle.asset` — Baraja de nuevo las misiones disponibles, costo: 3 coins
   - `Buff_FailSave.asset` — Absorbe 1 fallo (héroe sobrevive), costo: 15 coins
   - `Buff_MegaBoost.asset` — +10 a un stat aleatorio de todos los héroes, costo: 20 coins
3. Crear `Assets/Scripts/Core/BuffManager.cs`:
   - Mantiene lista de buffs activos para la run
   - `ApplyBuff(BuffData buff)` — aplica el efecto del buff
   - `GetActiveStatBonus(HeroStat stat)` — suma total de bonus de ese stat
   - `GetCurrentHintLevelBonus()` — bonus de revelación de pistas
   - `GetPatienceMultiplier()` — multiplicador de paciencia acumulado
   - `HasFailProtection()` y `ConsumeFailProtection()`

**Criterio de éxito:** Los buffs se pueden crear en el editor. `BuffManager.ApplyBuff` modifica los valores correctamente.

---

### TAREA 1.4 — Sistema de evaluación de misiones
**Estado:** `[ ]`
**Responsable:** Dev B
**Duración estimada:** 1.5 horas
**Dependencias:** 1.1, 1.2, 1.3

**Descripción:**
Implementar la lógica central de resolución de misiones: dada una misión y un héroe, calcular si la misión tiene éxito o falla, y qué recompensas/penalizaciones se aplican.

**Pasos:**
1. Crear `Assets/Scripts/Missions/MissionEvaluator.cs`:
   ```csharp
   public static class MissionEvaluator {
       // Calcula si la misión tiene éxito dado un héroe
       public static MissionResult Evaluate(HeroInstance hero, MissionInstance mission) {
           // 1. Obtener el stat del héroe correspondiente al PrimaryStat de la misión
           // 2. Sumar bonus de BuffManager
           // 3. Calcular chance base: heroStat / missionRequirement
           // 4. Clamp entre 0 y 1
           // 5. Bonus por stats secundarios: +5% cada uno que supere su umbral
           // 6. Random.value <= chance → éxito
           // 7. Si falla, verificar BuffManager.HasFailProtection()
           // 8. Retornar MissionResult con todos los detalles
       }
   }

   public class MissionResult {
       public bool Success;
       public bool ProtectionUsed;    // Si el buff absorbió el fallo
       public float SuccessChance;    // Para mostrar en UI de post-misión
       public int CoinsEarned;
       public int XPEarned;
       public int ScoreEarned;
       public HeroInstance Hero;
       public MissionInstance Mission;
   }
   ```
2. La fórmula de éxito detallada:
   ```
   effectiveStat = hero.PrimaryStat + BuffManager.GetActiveStatBonus(primaryStat)
   baseChance    = Mathf.Clamp01(effectiveStat / mission.PrimaryStatRequirement)
   bonusChance   = 0
   foreach secondaryStat:
       if hero.secondaryStat >= mission.secondaryRequirement:
           bonusChance += 0.05f
   finalChance = Mathf.Clamp01(baseChance + bonusChance)
   success = Random.value <= finalChance
   ```
3. Manejo de rechazo de misión por el héroe:
   ```
   hero.WillAcceptMission(mission):
       return mission.XPReward >= hero.MinMissionXP
              && mission.MinHeroLevel <= hero.ExperienceLevel
   ```
4. Escribir tests manuales en un MonoBehaviour de prueba:
   - Héroe con Fuerza 10 vs misión que requiere Fuerza 8 → alta probabilidad de éxito
   - Héroe con Fuerza 3 vs misión que requiere Fuerza 8 → baja probabilidad
   - Héroe nivel 1 vs misión que requiere nivel 5 → rechazo
5. Publicar eventos al terminar:
   - Éxito → `EventBus.Publish(new OnMissionCompleted {...})`
   - Fallo → `EventBus.Publish(new OnMissionFailed {...})` → si no hay protección, `EventBus.Publish(new OnHeroDied {...})`

**Criterio de éxito:** `MissionEvaluator.Evaluate` devuelve resultados estadísticamente coherentes. Los eventos se publican correctamente.

---

## FASE 2 — LOOP DE GAMEPLAY
**Objetivo:** Implementar la cola de héroes, el mazo de misiones, el drag & drop de cartas, y los timers de paciencia. Esto es el corazón jugable del juego.
**Duración:** Día 2 (tarde) – Día 3

---

### TAREA 2.1 — HeroQueue: cola de héroes y slots
**Estado:** `[ ]`
**Responsable:** Dev C
**Duración estimada:** 2 horas
**Dependencias:** 1.1, 0.3, 0.4

**Descripción:**
Implementar el sistema que gestiona la llegada de héroes, los 4 slots visibles simultáneos, y el timer de paciencia por héroe. Cuando el timer expira, el héroe se va (no muere, pero se pierde la oportunidad). Si el jugador asigna una misión, el slot se libera.

**Pasos:**
1. Crear `Assets/Scripts/Heroes/HeroSpawner.cs`:
   - Referencia al array de `HeroData` templates disponibles
   - `SpawnHero()` — elige un template aleatorio, genera un `HeroInstance`, lo agrega a la cola
   - Controla el intervalo entre llegadas (configurable, ej: cada 15 segundos en día 1, más rápido en días posteriores)
   - Publica `OnHeroArrived`
2. Crear `Assets/Scripts/Heroes/HeroQueue.cs`:
   - Mantiene lista de hasta 4 `HeroInstance` activos
   - Referencia a 4 `HeroSlotUI` (los slots en pantalla)
   - `AddHero(HeroInstance)` — si hay slot libre, lo asigna; si no, el héroe espera en cola off-screen
   - `RemoveHero(HeroInstance)` — libera el slot
   - Escucha `OnHeroArrived`, `OnMissionAssigned`, `OnHeroDied`
3. Crear `Assets/Scripts/Heroes/HeroPatience.cs` (componente por héroe):
   - `MaxPatience` en segundos (del HeroData × BuffManager.GetPatienceMultiplier())
   - Timer que cuenta hacia abajo
   - Eventos visuales: barra de paciencia cambia de color (verde → amarillo → rojo)
   - Al llegar a 0: héroe se va, publica `OnHeroLeft { Rejected = false }`
   - Al impaciencia < 20%: reproducir animación de nerviosismo
4. Crear `Assets/Scripts/Heroes/HeroSlotUI.cs`:
   - Muestra el retrato del héroe
   - Muestra stats (con iconos o texto)
   - Muestra barra de paciencia
   - Efecto de entrada (tween desde el borde de la pantalla)
   - Efecto de salida al asignar misión o irse
5. Definir los datos de la cola en un `HeroQueueConfig` (SO):
   - `SpawnIntervalBase`: 15 segundos
   - `SpawnIntervalMin`: 5 segundos (en días avanzados)
   - `MaxSimultaneousHeroes`: 4
   - Lista de HeroData disponibles con pesos de probabilidad

**Criterio de éxito:** Los héroes aparecen en pantalla, tienen barras de paciencia que se agotan, y desaparecen solos si no reciben misión.

---

### TAREA 2.2 — MissionDeck: mazo y mano de misiones
**Estado:** `[ ]`
**Responsable:** Dev D
**Duración estimada:** 2 horas
**Dependencias:** 1.2, 0.4

**Descripción:**
Implementar el mazo de misiones y la mano visible del jugador. El jugador siempre tiene un número fijo de cartas en mano (ej: 5). Al asignar una carta, se roba una nueva del mazo.

**Pasos:**
1. Crear `Assets/Scripts/Missions/MissionDeck.cs`:
   - Lista de todos los `MissionData` disponibles (asignados desde el Inspector)
   - `BuildDeck()` — crea la lista shuffleada de `MissionInstance`
   - `DrawCard()` → devuelve una `MissionInstance` del tope del mazo
   - `Reshuffle()` — desordena el mazo de nuevo (buff de reshuffle)
   - `HandSize`: cuántas cartas tiene el jugador en mano (default: 5)
   - Mantiene la lista de cartas en mano actual
2. Crear `Assets/Scripts/Missions/MissionCard.cs` (MonoBehaviour UI):
   - Muestra: título, descripción, dificultad, recompensas
   - Muestra las pistas desbloqueadas según el run actual
   - Estado visual: normal, hovereada, seleccionada, desactivada (en tránsito)
   - Referencia a su `MissionInstance`
3. Crear `Assets/Scripts/Missions/MissionCardArea.cs`:
   - Gestiona el layout de la mano (posición de cada carta)
   - Al quitar una carta: roba automáticamente del mazo y la añade a la mano
   - Animación de "robar carta" desde el mazo
4. Crear 4 animaciones simples (pueden ser Tweens o DOTween si está disponible):
   - Carta aparece desde el mazo (slide + fade in)
   - Carta hovereada (scale up ligeramente)
   - Carta seleccionada para drag (se eleva, sombra)
   - Carta descartada al fallar (fade out + caída)

**Criterio de éxito:** El jugador ve 5 cartas de misión. Al retirar una del juego, se roba otra automáticamente. Las pistas se muestran correctamente según el run.

---

### TAREA 2.3 — Drag & Drop: asignación de misión a héroe
**Estado:** `[ ]`
**Responsable:** Dev C
**Duración estimada:** 3 horas
**Dependencias:** 2.1, 2.2

**Descripción:**
Implementar el sistema de drag and drop que permite arrastrar una carta de misión desde la mano y soltarla sobre un héroe para asignarla. Es la mecánica principal de interacción del juego.

**Pasos:**
1. Crear `Assets/Scripts/UI/CardDragHandler.cs` usando `IBeginDragHandler`, `IDragHandler`, `IEndDragHandler` de Unity UI:
   ```csharp
   // Implementar la interfaz IBeginDragHandler:
   // - Guardar posición original de la carta
   // - Deshabilitar el raycasting en la carta para que no bloquee el drop target
   // - Crear un "ghost" de la carta que sigue al mouse

   // Implementar IDragHandler:
   // - Mover el ghost a la posición del mouse (en coordenadas de canvas)
   // - Detectar si el mouse está sobre un HeroSlot (highlight del slot)

   // Implementar IEndDragHandler:
   // - Raycast para detectar si se soltó sobre un HeroSlot
   // - Si sí: intentar asignar misión
   // - Si no: regresar la carta a su posición original (tween)
   ```
2. Crear `Assets/Scripts/UI/HeroDropTarget.cs` (en cada HeroSlotUI):
   - Implementar `IDropHandler`
   - Al recibir drop: verificar que el héroe acepta la misión
   - Si acepta: llamar a `MissionEvaluator.Evaluate()` e iniciar la animación de resultado
   - Si rechaza: mostrar feedback de rechazo (shake + texto "¡Me niego!")
3. Flujo completo de asignación:
   ```
   1. Jugador arrastra carta sobre héroe
   2. HeroDropTarget.OnDrop detecta el evento
   3. HeroInstance.WillAcceptMission() → si false, rechazo visual y la carta vuelve
   4. Si acepta: carta desaparece del HeroSlot (animación)
   5. MissionEvaluator.Evaluate() → MissionResult
   6. Animación de "héroe parte de aventura" (2-3 segundos)
   7. Animación de "resultado": éxito (fanfare) o fallo (muerte)
   8. Publicar eventos correspondientes
   9. Héroe exitoso puede volver en una run posterior (o simplemente desaparece, a definir)
   10. MissionCardArea roba carta nueva del mazo
   ```
4. Highlight visual al arrastrar:
   - Héroe que puede aceptar la misión: borde verde brillante
   - Héroe que rechazaría la misión: borde rojo o gris
   - (Esto requiere calcular `WillAcceptMission` durante el drag sin ejecutarlo)

**Criterio de éxito:** Se puede arrastrar una carta a un héroe. El héroe acepta o rechaza. Se ejecuta la evaluación. Los eventos se publican correctamente.

---

### TAREA 2.4 — RunManager: gestión de días/runs
**Estado:** `[ ]`
**Responsable:** Dev D
**Duración estimada:** 1.5 horas
**Dependencias:** 0.4, 2.1, 2.2

**Descripción:**
Implementar la lógica de progresión por días/runs. Cada run tiene una duración (ej: 2 minutos o hasta que se cumpla una condición), al terminar se muestra un resumen y se ofrece el shop de buffs.

**Pasos:**
1. Crear `Assets/Scripts/Core/RunManager.cs`:
   - Mantiene `RunNumber` (incrementa cada run)
   - `RunDuration` en segundos (ej: 120s para la primera run, crece)
   - Timer de run
   - Al terminar el tiempo: pausa el juego, publica `OnRunEnd`
   - `StartRun()`: inicializa el mazo, spawner, timer; publica `OnRunStart`
2. Condiciones de fin de run:
   - Timer llega a 0 (fin normal)
   - `FailCount >= MaxFails` → publica `OnGameOver`
3. Pantalla de fin de run (inter-run):
   - Mostrar score de la run
   - Mostrar cuántos héroes sobrevivieron vs murieron
   - Mostrar coins ganadas
   - Transición a la pantalla de shop (ver Fase 3)
4. Escalado por run (configurable en Inspector):
   ```
   Run 1: MaxFails=3, SpawnInterval=15s, RunDuration=90s
   Run 2: MaxFails=3, SpawnInterval=13s, RunDuration=100s
   Run 3: MaxFails=4, SpawnInterval=11s, RunDuration=110s
   ...
   ```
5. Persistencia entre runs:
   - `RunNumber` persiste en GameManager
   - `CurrentCoins` persiste (no se resetea entre runs)
   - Buffs comprados persisten en `BuffManager`
   - Stats de héroes NO persisten (se generan de nuevo cada run)

**Criterio de éxito:** El juego avanza por runs, el timer funciona, las condiciones de fin se cumplen correctamente.

---

### TAREA 2.5 — Descarte activo de misiones
**Estado:** `[ ]`
**Responsable:** Dev D
**Duración estimada:** 1 hora
**Dependencias:** 2.2

**Descripción:**
Permitir al jugador descartar cartas de misión que no le convengan, a cambio de tiempo. No cuesta coins, pero existe un cooldown global antes de poder descartar otra carta. Esto añade decisión activa sobre la mano sin romper el balance económico: el jugador puede deshacerse de una misión imposible para su héroe actual, pero no puede spamear descartes para buscar la misión perfecta.

**Pasos:**
1. Añadir botón "Rechazar" en `MissionCard.cs`:
   - Visible siempre, pero grisado cuando el cooldown está activo
   - Posición: esquina inferior de la carta, fuera del área de drag para no interferir
   - No disponible si la carta está en estado `desactivada` (ya asignada / en tránsito)
2. Añadir lógica de descarte en `MissionCardArea.cs`:
   ```csharp
   public float DiscardCooldownSeconds = 10f;  // Configurable en Inspector
   private float _discardCooldownTimer = 0f;
   public bool CanDiscard => _discardCooldownTimer <= 0f;

   public void DiscardMission(MissionCard card) {
       if (!CanDiscard) return;
       // 1. Animación de salida (fade out + caída hacia abajo)
       // 2. Publicar OnMissionDiscarded
       // 3. Llamar MissionDeck.DrawCard() y añadir nueva carta a la mano
       // 4. Activar cooldown: _discardCooldownTimer = DiscardCooldownSeconds
   }

   void Update() {
       if (_discardCooldownTimer > 0f)
           _discardCooldownTimer -= Time.deltaTime;
   }
   ```
3. Indicador visual del cooldown en el botón "Rechazar":
   - Fill circular (Image de tipo Filled) que se vacía durante el cooldown
   - Al completarse: pequeño destello/pulse para avisar al jugador que ya puede descartar
   - Tooltip al hover: "Rechaza esta misión y roba una nueva. Cooldown: Xs"
4. Publicar evento en `GameEvents.cs`:
   ```csharp
   public class OnMissionDiscarded { public MissionInstance Mission; }
   ```
   - El AudioManager puede escuchar este evento para reproducir un sonido de rechazo (papel arrugándose)
5. Casos límite a manejar:
   - Si el mazo está vacío al descartar: no se puede descartar (botón desactivado con tooltip "Mazo vacío")
   - Si solo queda 1 carta en mano: el descarte deja la mano momentáneamente vacía hasta que se roba la nueva

**Criterio de éxito:** El jugador puede descartar una carta. El cooldown se respeta correctamente. La mano se repone automáticamente. El botón comunica claramente cuándo está disponible y cuándo no.

---

## FASE 3 — ECONOMÍA Y ROGUELIKE
**Objetivo:** Implementar el CoinJar, el Shop de buffs entre runs, y el sistema de progresión roguelike (pistas progresivas, buffs que persisten, escalado de dificultad).
**Duración:** Día 3 (tarde) – Día 4

---

### TAREA 3.1 — CoinJar: sistema de monedas
**Estado:** `[ ]`
**Responsable:** Dev A
**Duración estimada:** 1.5 horas
**Dependencias:** 0.4, 1.4

**Descripción:**
Implementar el frasco de propinas (CoinJar) visible en pantalla. Los héroes dejan monedas al completar misiones exitosamente. Las monedas se usan para comprar buffs en el shop.

**Pasos:**
1. Crear `Assets/Scripts/Economy/CoinJar.cs`:
   - Variable `CurrentCoins` (sincronizado con GameManager)
   - `AddCoins(int amount)` — actualiza el contador, dispara animación de moneda cayendo
   - `SpendCoins(int amount)` → bool — verifica si hay suficientes, resta si es posible
   - Escucha `OnMissionCompleted` → llama `AddCoins(result.CoinsEarned)`
   - Escucha `OnBuffPurchased` → llama `SpendCoins(buff.Cost)`
2. Visual del CoinJar:
   - Sprite de un frasco o tarro (puede ser placeholder)
   - Número visible de monedas
   - Animación de moneda entrando al frasco cuando se gana una (partículas o tween)
   - Shake + número rojo cuando se gasta
3. Conectar el sistema de recompensas:
   - Al evaluar misión exitosa: `result.CoinsEarned = mission.CoinsReward + bonus`
   - Bonus por nivel de héroe: `+1 moneda por cada 5 niveles del héroe`
   - Boss misión exitosa: `2× las monedas`

**Criterio de éxito:** Las monedas se acumulan al completar misiones. El contador en pantalla se actualiza con animación.

---

### TAREA 3.2 — ShopManager: tienda inter-run
**Estado:** `[ ]`
**Responsable:** Dev C
**Duración estimada:** 2 horas
**Dependencias:** 1.3, 3.1, 2.4

**Descripción:**
Implementar la pantalla de tienda que aparece entre runs. El jugador puede gastar las monedas acumuladas para comprar buffs permanentes (de esa partida).

**Pasos:**
1. Crear `Assets/Scripts/Economy/ShopManager.cs`:
   - Pool de todos los `BuffData` disponibles
   - Cada vez que se abre el shop: seleccionar 3 buffs aleatorios para mostrar (sin repetir los ya comprados)
   - `PurchaseBuff(BuffData buff)`:
     - Verificar si el jugador tiene suficientes coins
     - Llamar `CoinJar.SpendCoins(buff.Cost)`
     - Llamar `BuffManager.ApplyBuff(buff)`
     - Publicar `OnBuffPurchased`
   - `SkipShop()` — el jugador puede saltar la tienda sin comprar nada
2. UI del shop (panel que aparece sobre el gameplay):
   - Título: "El Tablón de Buffs" o "Mejoras de Guild"
   - 3 cartas de buff mostrando: icono, nombre, descripción, costo en coins
   - Botón "Comprar" en cada carta (desactivado si no hay suficientes coins)
   - Botón "Continuar" para pasar al siguiente día sin comprar
   - Contador de coins actual visible
3. Feedback de compra:
   - Al comprar: animación de brillo en la carta, sonido de compra
   - La carta comprada queda en gris (no se puede comprar de nuevo)
   - Si hay coins insuficientes: shake en el contador de coins
4. El buff de Reshuffle (`Buff_Reshuffle`) funciona diferente:
   - No es permanente
   - Se puede usar EN CUALQUIER MOMENTO durante una run (botón en HUD)
   - Al usarlo: `MissionDeck.Reshuffle()` + descarta las cartas en mano y roba 5 nuevas

**Criterio de éxito:** El shop aparece entre runs, el jugador puede comprar buffs, los buffs se aplican y persisten en la siguiente run.

---

### TAREA 3.3 — Sistema de pistas progresivas
**Estado:** `[ ]`
**Responsable:** Dev B
**Duración estimada:** 1 hora
**Dependencias:** 1.2, 2.2, 2.4, 3.2

**Descripción:**
Conectar el nivel de run actual con el sistema de revelación de pistas en las cartas de misión. A mayor número de run, más pistas visibles. El buff `HintReveal` reduce el nivel requerido para desbloquear pistas.

**Pasos:**
1. En `MissionInstance.GetRevealedHints(runLevel)`:
   - Filtrar `hints` cuyo `UnlockAtRunLevel <= runLevel + BuffManager.GetCurrentHintLevelBonus()`
   - Devolver la lista filtrada
2. En `MissionCard.cs`, al renderizar la carta:
   - Llamar `GetRevealedHints(RunManager.CurrentRunNumber)`
   - Si hay pistas: mostrar área de pistas debajo de la descripción
   - Cada pista se muestra con su color (verde para stat bueno, naranja para precaución)
   - Pistas ocultas: mostrar ??? o espacio vacío para que el jugador sepa que HAY pistas sin revelarlas
3. Efecto visual al desbloquear una nueva pista (cuando el run avanza):
   - Al inicio de un run, si hay nuevas pistas desbloqueadas: breve animación de "revelación" en las cartas
4. Balance de diseño de pistas:
   - Run 1: sin pistas (solo título + descripción ambigua)
   - Run 2: pistas vagas ("Requiere habilidad física")
   - Run 3: pistas más claras ("Stat físico importante")
   - Run 4+: pistas directas ("Fuerza ++") con color

**Criterio de éxito:** En la run 1 no hay pistas. En runs posteriores, las pistas aparecen progresivamente. El buff de HintReveal funciona.

---

### TAREA 3.4 — Sistema de Boss Héroes y misiones boss
**Estado:** `[ ]`
**Responsable:** Dev A
**Duración estimada:** 1 hora
**Dependencias:** 1.1, 1.2, 2.1

**Descripción:**
Implementar los héroes Boss, que son raros, tienen stats extremos, exigen misiones boss, y ofrecen recompensas enormes. Estilo Balatro: los números se vuelven absurdos y emocionantes.

**Pasos:**
1. En `HeroSpawner`:
   - Agregar peso de probabilidad por rareza:
     - Common: 60%
     - Rare: 25%
     - Epic: 12%
     - Boss: 3%
   - La probabilidad de Boss aumenta con el RunNumber: `bossChance = 0.03 + (runNumber * 0.01)`
2. Los héroes Boss tienen:
   - Stats en el rango 50-100 (en lugar de 1-20)
   - `MinMissionXP` extremadamente alto (solo aceptan misiones boss)
   - `PatienceMultiplier = 0.5` (muy impaciente)
   - Retrato y animación distintos (borde dorado, efecto de partículas)
3. Las misiones Boss (`IsBossMission = true`):
   - Solo aparecen cuando hay un héroe Boss en la cola
   - `PrimaryStatRequirement` en el rango 40-80
   - Recompensa: 5× coins base + score especial
   - Si falla: penalidad doble (cuenta como 2 fallos en lugar de 1)
4. UI especial para héroes Boss:
   - El slot se hace más grande o tiene un marco especial
   - Texto de alerta: "¡HÉROE LEGENDARIO!" con animación

**Criterio de éxito:** Los héroes boss aparecen con baja frecuencia. Solo aceptan misiones boss. El fallo es más penalizante.

---

## FASE 4 — UI/UX Y FEEDBACK VISUAL
**Objetivo:** Crear una interfaz completa, clara e informativa. El jugador debe entender el estado del juego de un vistazo. Feedback visual claro para cada acción.
**Duración:** Día 4 – Día 5

---

### TAREA 4.1 — HUD principal (in-game)
**Estado:** `[ ]`
**Responsable:** Dev D
**Duración estimada:** 2.5 horas
**Dependencias:** 2.1, 2.2, 3.1, 2.4

**Descripción:**
Diseñar e implementar el HUD visible durante el gameplay. Debe mostrar toda la información crítica sin abrumar.

**Pasos:**
1. Layout del HUD (1920×1080):
   ```
   ┌────────────────────────────────────────────────────────┐
   │ [DÍA X]  [FALLOS: ●●○○]  [SCORE: 1234]  [TIMER: 1:30] │ ← Top bar
   ├────────────────────────────────────────────────────────┤
   │                                                        │
   │  [HÉROE 1] [HÉROE 2] [HÉROE 3] [HÉROE 4]              │ ← Fila de héroes (centro-arriba)
   │                                                        │
   │                                                        │
   │  [MISIÓN] [MISIÓN] [MISIÓN] [MISIÓN] [MISIÓN]         │ ← Mano de cartas (centro-abajo)
   │                                                        │
   ├────────────────────────────────────────────────────────┤
   │  [🪙 15 coins]  [MAZO: 18]  [RESHUFFLE]               │ ← Bottom bar
   └────────────────────────────────────────────────────────┘
   ```
2. Implementar cada elemento:
   - **Top bar**: `RunDayUI`, `FailCountUI` (corazones o puntos), `ScoreUI`, `TimerUI`
   - **Fallos**: Mostrar como iconos (calaveras). Los usados en rojo, los disponibles en gris
   - **Timer**: Cuenta regresiva. En los últimos 20 segundos: rojo y pulsante
   - **Mano de cartas**: Posición fija en la parte inferior, fan o hilera horizontal
   - **CoinJar**: Esquina inferior izquierda, con animación
   - **Mazo restante**: Número de cartas que quedan en el mazo
   - **Botón Reshuffle**: Solo activo si el jugador tiene el buff de reshuffle
3. Escuchar eventos para actualizar cada elemento:
   - `OnFailPenalty` → actualizar `FailCountUI`
   - `OnCoinEarned` / `OnCoinSpent` → actualizar coins
   - `OnRunStart` → resetear timer y día
   - `OnRunEnd` → detener timer

**Criterio de éxito:** El HUD muestra todos los valores correctos en tiempo real y reacciona a los eventos del juego.

---

### TAREA 4.2 — Pantalla de resultado de misión
**Estado:** `[ ]`
**Responsable:** Dev B
**Duración estimada:** 1.5 horas
**Dependencias:** 1.4, 2.3

**Descripción:**
Mostrar feedback claro al jugador cuando una misión se resuelve: éxito o fracaso, stats usados, probabilidad de éxito, recompensas obtenidas.

**Pasos:**
1. Crear panel de resultado (aparece 1-2 segundos después de soltar la carta):
   - **Éxito**: Fondo verde/dorado, texto "¡MISIÓN COMPLETADA!", monedas que saltan, XP ganada
   - **Fallo**: Fondo rojo oscuro, texto "HÉROE CAÍDO", skull animation, penalización visible
   - **Protección activada**: Fondo naranja, texto "¡BUFF ACTIVADO! El héroe sobrevivió..."
2. Mostrar detalles de la evaluación (para que el jugador aprenda):
   - Stat usado: "Tu héroe usó [Fuerza: 7] vs [Requerimiento: 8]"
   - Probabilidad final: barra de progreso mostrando el % de chance
   - Resultado: ícono de dado o ruleta
3. Duración: 2.5 segundos automático, o click para acelerar
4. En caso de fallo + muerte del héroe:
   - Animación dramática: héroe desaparece con efecto de partículas
   - Contador de fallos en el HUD hace shake y se actualiza
   - Si es el último fallo permitido: transición a Game Over

**Criterio de éxito:** El jugador siempre sabe qué pasó y por qué. La pantalla de resultado aparece y desaparece sin bloquear el juego más de lo necesario.

---

### TAREA 4.3 — Pantalla de Main Menu
**Estado:** `[ ]`
**Responsable:** Dev D
**Duración estimada:** 1 hora
**Dependencias:** 0.3

**Descripción:**
Implementar una pantalla de inicio funcional y atractiva.

**Pasos:**
1. Layout del Main Menu:
   - Título del juego: "QUESTLINE" (fuente medieval/fantasy)
   - Subtítulo: "Gestiona tu guild. Infiere lo que no se dice."
   - Botón "JUGAR" → carga `Gameplay.unity`
   - Botón "SALIR" → `Application.Quit()`
   - (Opcional) Botón "CÓMO JUGAR" → pequeño tutorial overlay
2. Tutorial overlay (mínimo viable):
   - 3-4 slides con texto e imágenes explicando:
     1. "Llegan héroes con stats: Fuerza, Destreza, Inteligencia, Carisma"
     2. "Arrastra una misión a un héroe. Lee la descripción e infiere qué stat necesita"
     3. "Si aciertas, ganas monedas. Si fallas, el héroe muere"
     4. "Con las monedas compras mejoras entre días. ¡Que las pistas se vayan revelando!"
3. Animación de fondo: parallax simple con elementos de una taberna/mesón

**Criterio de éxito:** El menú funciona, el botón de jugar carga el gameplay, hay al menos un tooltip de cómo jugar.

---

### TAREA 4.4 — Pantalla de Game Over y Resumen de Run
**Estado:** `[ ]`
**Responsable:** Dev B
**Duración estimada:** 1 hora
**Dependencias:** 0.3, 2.4

**Descripción:**
Mostrar una pantalla de game over que comunique el resultado de la partida y permita reiniciar.

**Pasos:**
1. Pantalla de Game Over:
   - Título: "EXPULSADO DE LA GUILD"
   - Texto dramático: "Tu gestión desastrosa ha costado demasiadas vidas..."
   - Stats de la partida: días sobrevividos, misiones completadas, monedas totales, héroes perdidos
   - Botón "VOLVER AL MENÚ" → MainMenu
   - Botón "INTENTAR DE NUEVO" → reinicia desde run 1, resetea todo
2. Resumen de run (inter-run, antes del shop):
   - Tabla de la run: misiones exitosas, fallidas, coins ganadas
   - Mejor héroe de la run (el que más misiones completó)
   - Streak de éxitos consecutivos si aplica
3. Pantalla de victoria (si existe condición de victoria):
   - Por ahora: si el jugador llega a la run 10 → pantalla de victoria
   - "¡Tu guild es legendaria!" + score final

**Criterio de éxito:** Game Over muestra stats correctos. El jugador puede reiniciar sin errores. Las coins y buffs se resetean correctamente.

---

### TAREA 4.5 — Tooltips y feedback de información
**Estado:** `[ ]`
**Responsable:** Dev C
**Duración estimada:** 1.5 horas
**Dependencias:** 4.1, 2.3

**Descripción:**
Implementar tooltips que aparecen al hover sobre elementos del juego, y mensajes de feedback flotante para acciones importantes.

**Pasos:**
1. Crear `Assets/Scripts/UI/TooltipManager.cs`:
   - Singleton
   - `ShowTooltip(string text, Vector2 position)` — muestra un panel con texto
   - `HideTooltip()` — oculta el panel
   - El panel sigue al cursor con offset
   - Delay de 0.5 segundos antes de aparecer (evitar spam)
2. Tooltips en elementos del juego:
   - Stats del héroe (ej: hover sobre el ícono de Fuerza → "Fuerza: qué tan bien combate cuerpo a cuerpo")
   - Iconos de pista en misiones (ej: hover → "Esta pista se desbloqueó en el Día 2")
   - Contador de fallos (ej: hover → "Fallos restantes: 2. Al llegar a 0, game over.")
   - Buffs en el shop (ej: hover → descripción completa del buff)
3. Floating text (números que flotan y desaparecen):
   - Al ganar coins: "+5" dorado que sube y se desvanece desde el CoinJar
   - Al fallar: "FALLO" rojo que aparece sobre el héroe
   - Al rechazar misión: "¡Me niego!" en globo de diálogo sobre el héroe

**Criterio de éxito:** Todos los elementos importantes tienen tooltip. Los floating texts aparecen en el momento correcto.

---

## FASE 5 — AUDIO, VFX Y BALANCE
**Objetivo:** Agregar capa sonora, efectos de partículas, y ajustar los números del juego para que sea divertido y justo.
**Duración:** Día 5 – Día 6

---

### TAREA 5.1 — AudioManager y sonidos del juego
**Estado:** `[ ]`
**Responsable:** Dev A
**Duración estimada:** 2 horas
**Dependencias:** Todos los sistemas core

**Descripción:**
Implementar el sistema de audio y agregar efectos de sonido para las acciones principales del juego. El audio es crucial para el feedback al jugador.

**Pasos:**
1. Crear `Assets/Scripts/Core/AudioManager.cs`:
   - Singleton con DontDestroyOnLoad
   - Pool de AudioSources para SFX (evitar instanciar/destruir)
   - `PlaySFX(AudioClip clip, float volume = 1f)`
   - `PlayMusic(AudioClip music, bool loop = true)`
   - `StopMusic()`
   - Volúmenes configurables (SFX master, Music master)
2. Sonidos requeridos (buscar en recursos libres o generar con herramientas free):
   - **Héroe llega**: sonido de campanilla o pasos de llegada
   - **Héroe impaciente**: sonido de tambores o golpes de mesa
   - **Héroe se va**: sonido de decepción (suspiro, boo)
   - **Drag de carta**: sonido de papel/pergamino
   - **Drop en héroe**: clic satisfactorio
   - **Misión exitosa**: fanfare corto, monedas cayendo
   - **Misión fallida**: sonido dramático de fallo, acorde triste
   - **Muerte de héroe**: sonido de campana grave o grito
   - **Comprar buff**: sonido de compra satisfactorio
   - **Reshuffle**: sonido de cartas barajándose
   - **Game Over**: melodía de derrota
   - **Pista revelada**: sonido de descubrimiento/eureka
3. Música de fondo:
   - Música de taberna/mesón tranquila para el gameplay
   - Música más intensa cuando hay pocos fallos restantes
   - Música de boss cuando hay un héroe boss activo
4. Escuchar eventos del EventBus para reproducir sonidos:
   - `OnHeroArrived` → sfx llegada
   - `OnMissionCompleted` → sfx éxito
   - `OnMissionFailed` → sfx fallo
   - `OnHeroDied` → sfx muerte
   - `OnBuffPurchased` → sfx compra

**Criterio de éxito:** Cada acción importante tiene sonido. La música ambiental se reproduce en loop. No hay errores de audio (null clips, etc).

---

### TAREA 5.2 — VFX y partículas
**Estado:** `[ ]`
**Responsable:** Dev D
**Duración estimada:** 2 horas
**Dependencias:** 4.1, 4.2

**Descripción:**
Agregar efectos de partículas y animaciones simples para hacer el juego más satisfactorio visualmente.

**Pasos:**
1. Efectos de partículas con el sistema de Particles de Unity:
   - **Monedas ganadas**: partículas doradas que van del héroe al CoinJar
   - **Héroe muere**: partículas rojas/oscuras + desvanecimiento del sprite
   - **Misión exitosa**: confetti o destellos dorados brevemente
   - **Buff comprado**: efecto de brillo en el ícono del buff
   - **Héroe Boss aparece**: explosion de luz + partículas doradas de alta intensidad
2. Animaciones de UI (con Animator o DOTween):
   - Carta de misión: float suave (sube y baja 2-3 pixels en loop)
   - HeroSlot al recibir un drop: pequeño bounce
   - FailCounter al incrementar: shake + flash rojo
   - CoinCounter al incrementar: bounce + flash dorado
3. Efectos de cámara simples (Camera Shake):
   - Al fallar una misión: pequeño shake de cámara (0.3 segundos)
   - Al morir un boss: shake más intenso (0.5 segundos)
4. Screen flash:
   - Al ganar una misión boss: flash blanco suave
   - Al perder el último fallo: flash rojo intenso antes del game over

**Criterio de éxito:** El juego se siente "vivo" con los efectos. Los VFX no causan drops de framerate.

---

### TAREA 5.3 — Balance de números y playtest interno
**Estado:** `[ ]`
**Responsable:** Todos
**Duración estimada:** 3 horas (sesión de playtest)
**Dependencias:** Todas las fases anteriores

**Descripción:**
Jugar el juego internamente, identificar problemas de balance, y ajustar los números hasta que el loop sea divertido.

**Pasos:**
1. Sesión de playtest de 30-45 minutos con los 4 devs jugando al mismo tiempo
2. Variables a ajustar según el playtest:
   - `MaxFails`: ¿Es 3 demasiado punitivo? ¿5 demasiado fácil?
   - `SpawnInterval`: ¿Los héroes llegan demasiado rápido o lento?
   - `BasePatience`: ¿El timer de paciencia es justo?
   - `CoinsReward` de misiones: ¿Se acumulan suficientes coins para comprar buffs?
   - `Prices` en el shop: ¿Son los buffs asequibles o inalcanzables?
   - Probabilidades de éxito de las misiones: ¿Es posible aprender a inferir los stats?
   - `StatRange` de héroes: ¿Los números son interesantes? ¿Escalado tipo Balatro?
3. Ajustar la curva de dificultad:
   ```
   Día 1: Fácil, pocas misiones difíciles, timer generoso (20s)
   Día 2: Ligeramente más difícil, 1-2 misiones normales
   Día 3-4: Misiones normales y difíciles mezcladas, 1 boss posible
   Día 5+: Mayoría difíciles, bosses más frecuentes, timer más apretado
   ```
4. Crear un Google Sheet o documento con los parámetros clave y sus valores actuales vs deseados
5. Aplicar cambios en los ScriptableObjects (sin tocar código)

**Criterio de éxito:** Una sesión completa (3-5 runs) se siente desafiante pero justa. El jugador aprende gradualmente a inferir los stats.

---

## FASE 6 — TESTING, POLISH Y BUILD FINAL
**Objetivo:** Corregir bugs, pulir detalles, y generar el build final para la game jam.
**Duración:** Día 6 – Día 7

---

### TAREA 6.1 — Bug fixing y QA
**Estado:** `[ ]`
**Responsable:** Todos
**Duración estimada:** 4 horas
**Dependencias:** Fase 5

**Descripción:**
Sesión organizada de testing para identificar y corregir bugs antes del build final.

**Pasos:**
1. Crear una lista de casos de prueba a verificar manualmente:
   - [ ] Los héroes se generan con stats dentro del rango definido
   - [ ] La cola de héroes nunca supera 4 simultáneos
   - [ ] El timer de paciencia funciona correctamente y se muestra en UI
   - [ ] El drag & drop funciona en todo el área de la pantalla
   - [ ] El rechazo de misión funciona correctamente
   - [ ] Las misiones se evalúan con la fórmula correcta
   - [ ] Las monedas se otorgan correctamente al éxito
   - [ ] Los fallos se cuentan correctamente
   - [ ] El game over se dispara en el fallo correcto
   - [ ] Las pistas progresan correctamente entre runs
   - [ ] Los buffs se aplican y persisten entre runs
   - [ ] El shop muestra 3 buffs aleatorios
   - [ ] El reshuffle funciona correctamente
   - [ ] Los héroes Boss generan misiones boss
   - [ ] El score se acumula correctamente
   - [ ] El reinicio completo resetea todos los valores
   - [ ] No hay NullReferenceException en la consola durante una run normal
2. Reportar cada bug como comentario en el código con `// BUG:` y prioridad (CRÍTICO / ALTO / BAJO)
3. Dividir los bugs por prioridad y asignarlos al dev más cercano al sistema afectado
4. Verificar correcciones con el mismo caso de prueba

**Criterio de éxito:** Cero bugs críticos. Los bugs bajos son aceptables si no afectan la jugabilidad core.

---

### TAREA 6.2 — Polish de última hora
**Estado:** `[ ]`
**Responsable:** Dev B + Dev D
**Duración estimada:** 2 horas
**Dependencias:** 6.1

**Descripción:**
Pequeños detalles que hacen que el juego se sienta más pulido y profesional.

**Pasos:**
1. Verificar que todas las transiciones de escena tienen fade in/out (negro → escena)
2. Agregar una pantalla de carga simple si la escena tarda más de 0.5s en cargar
3. Verificar que el juego funciona en resoluciones 16:9 (1920×1080, 1280×720)
4. Verificar que el juego funciona con el cursor del mouse (no hay zonas muertas en el drag)
5. Asegurarse de que ningún texto está truncado en las cartas de misión
6. Verificar que los sonidos no se superponen de forma molesta
7. Agregar créditos en el Main Menu ("Hecho por [nombres] para [nombre jam]")
8. Verificar que el ícono del juego está configurado en Player Settings

**Criterio de éxito:** El juego se ve y se siente consistente. No hay textos cortados ni elementos fuera de pantalla.

---

### TAREA 6.3 — Build final y distribución
**Estado:** `[ ]`
**Responsable:** Dev A
**Duración estimada:** 1 hora
**Dependencias:** 6.1, 6.2

**Descripción:**
Generar el build final del juego y subirlo a la plataforma de la game jam (itch.io u otra).

**Pasos:**
1. En Unity: File → Build Settings:
   - Platform: WebGL (para itch.io) o Windows Standalone
   - Scenes: verificar orden correcto (MainMenu, Gameplay, GameOver)
   - Player Settings:
     - Company Name: [nombre del equipo]
     - Product Name: "Questline"
     - Version: "1.0.0"
     - Default Screen Width: 1920, Height: 1080
2. Para WebGL:
   - Compression Format: Gzip
   - Publishing Settings → Enable Exceptions: None (para mejor performance)
3. Hacer Build (no Build And Run) y verificar que compila sin errores
4. Probar el build ejecutable antes de subir
5. Comprimir en .zip si es necesario
6. Subir a itch.io:
   - Título: Questline
   - Short description: [texto del juego]
   - Tags: roguelike, card-game, management, jam
   - Capturas de pantalla (mínimo 3)
7. Compartir el link con el equipo para verificación final

**Criterio de éxito:** El build corre sin errores. El juego es accesible desde el link de itch.io.

---

## DEPENDENCIAS ENTRE TAREAS (Resumen)

```
0.1 ──┬──► 0.3 ──► 4.3, 4.4
      └──► 0.4 ──► 1.1, 1.2, 1.3

1.1 ──┬──► 1.2 ──► 1.4 ──► 2.3, 3.1
      ├──► 1.3 ──► 1.4
      └──► 2.1

1.2 ──► 2.2
1.3 ──► 3.2

2.1 + 2.2 ──► 2.3 ──► 4.2
2.1 + 2.2 + 2.3 ──► 2.4
2.2 ──► 2.5

2.4 + 3.1 ──► 3.2
2.4 + 1.2 ──► 3.3

3.1 + 3.2 + 3.3 + 3.4 ──► 4.x
4.x ──► 5.x ──► 6.x
```

---

## ASIGNACIÓN RECOMENDADA POR DEV

| Dev | Especialización sugerida | Tareas principales |
|-----|-------------------------|-------------------|
| Dev A | Sistemas core + datos | 0.1, 1.1, 1.3, 3.4, 5.1, 6.3 |
| Dev B | Misiones + evaluación + UI info | 0.4, 1.2, 1.4, 3.3, 4.2, 4.5 |
| Dev C | Héroes + interacción | 0.3, 2.1, 2.3, 3.2, 4.5 |
| Dev D | Misiones deck + UI + VFX | 0.2, 2.2, 2.4, 2.5, 4.1, 4.3, 5.2 |

> Nota: Las asignaciones son sugerencias. El equipo puede reorganizarlas según habilidades reales.

---

## VARIABLES DE DISEÑO A DEFINIR (Pendientes de decisión del equipo)

| Variable | Valor sugerido | Estado |
|----------|---------------|--------|
| MaxFails inicial | 3 | Por confirmar |
| HandSize (cartas en mano) | 5 | Por confirmar |
| MaxHeroesSimultáneos | 4 | Confirmado |
| BasePatience (segundos) | 25s | Por confirmar |
| RunDuration inicial | 90s | Por confirmar |
| RunsHastaVictoria | 10 | Por confirmar |
| CostoReshuffle | 3 coins | Por confirmar |
| CoinsBaseMisión | 3-8 | Por confirmar |
| StatRange héroes Common | 1-15 | Por confirmar |
| StatRange héroes Boss | 50-100 | Por confirmar |
| DiscardCooldownSeconds | 10s | Por confirmar |

---

## RECURSOS EXTERNOS NECESARIOS

- **Arte**: Sprites de héroes, cartas de misión, fondo de taberna, iconos de stats
  - Fuente gratuita sugerida: [OpenGameArt.org](https://opengameart.org), [itch.io assets](https://itch.io/game-assets/free)
- **Fuentes**: Fuente medieval para el título, fuente legible para el body
  - Sugeridas: Cinzel (Google Fonts), MedievalSharp
- **Audio**: SFX y música de taberna
  - Fuentes gratuitas: [freesound.org](https://freesound.org), [opengameart.org](https://opengameart.org/art-search-advanced?field_art_type_tid[]=12)
- **DOTween**: Librería gratuita de tweening para Unity (Asset Store, versión free)
  - Recomendada fuertemente para animaciones de UI

---

## NOTAS FINALES DEL EQUIPO

- **Prioridad absoluta**: El loop core (héroe llega → drag carta → resultado) debe estar funcionando al final del Día 3. Todo lo demás es secundario.
- **Feature freeze**: Al inicio del Día 6, no se agregan features nuevas. Solo bugfix y polish.
- **Scoping**: Si el tiempo apremia, cortar en este orden: Boss system → Resumen inter-run → VFX → Tutorial overlay → Pantalla de victoria.
- **Comunicación**: Daily standup de 10 minutos cada mañana: qué hice ayer, qué haré hoy, qué me está bloqueando.
- **Git commits**: Hacer commits pequeños y frecuentes. Al menos 1 commit por tarea completada. Push a `dev` al final de cada día.
