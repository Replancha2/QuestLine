# PLAN DE DESARROLLO — QUESTLINE (JAME GAM)
> **Equipo:** 4 desarrolladores | **Motor:** Unity 2D (URP) | **Duración:** 7 días
> **Géneros:** Micromanagement + Roguelike
> **Leyenda de estado:** `[ ]` Pendiente · `[~]` En progreso · `[x]` Completado · `[!]` Bloqueado

---

## RESUMEN DEL JUEGO

**Questline** es un juego de micromanagement con elementos roguelike donde el jugador administra una guild de héroes. Los héroes llegan aleatoriamente con stats generados proceduralmente. El jugador debe asignarles misiones arrastrando cartas, infiriendo qué estadística encaja mejor con la descripción de la misión. Los fallos matan al héroe y penalizan al jugador; demasiados fallos significan game over. Con monedas de propina se compran buffs, mejoras y pistas en la tienda inter-día.

**Stats de héroe:** Fuerza · Destreza · Inteligencia · Carisma · XP
**Mecánica core:** Drag & Drop de carta-misión a héroe
**Vidas:** 3 vidas por día. Cada héroe que se va sin recibir misión = -1 vida. Perder las 3 = Game Over.
**Fin de día:** El día termina al superar el umbral de Fama del día (Fama acumulada por misiones completadas).
**Dificultad de misiones:** Sistema de rangos tipo naipe inglés — 12 niveles × 4 palos (un palo por stat = 48 misiones únicas totales). A mayor rango: mayores requisitos (calculados por fórmula) y mayor Fama y coins.
**Progresión por días:** El pool diario escala el rango promedio con el número de día. Los stats de los héroes también escalan.
**Eventos diarios:** Cada día hay un evento aleatorio con efecto positivo o negativo.
**Pistas y consumibles:** Comprables en tienda. Consumibles de un solo uso: "Ver stats de la mano" (revela PrimaryStat de todas las cartas en mano) y "Revelar carta" (revela todos los stats de una misión específica).
**Tutorial:** Sin texto; el jugador aprende fallando. La narrativa de loop se establece desde el Game Over inicial.

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
**Estado:** `[x] COMPLETADA`
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
   public class OnHeroArrived { public HeroInstance Hero; }
   public class OnHeroLeft { public HeroInstance Hero; public bool WasAngry; }  // WasAngry=true → -1 vida
   public class OnHeroDied { public HeroInstance Hero; }
   public class OnLifeLost { public int LivesRemaining; }                        // Cada vez que un héroe se va sin misión
   public class OnMissionAssigned { public MissionData Mission; public HeroInstance Hero; }
   public class OnMissionCompleted { public MissionData Mission; public HeroInstance Hero; public int CoinsEarned; public int FameEarned; }
   public class OnMissionFailed { public MissionData Mission; public HeroInstance Hero; }
   public class OnCoinEarned { public int Amount; }
   public class OnCoinSpent { public int Amount; }
   public class OnFameEarned { public int Amount; public int TotalFame; public int DayThreshold; }
   public class OnDayThresholdReached { public int DayNumber; }
   public class OnGameOver { public int DayReached; public int TotalFameEarned; }
   public class OnDayStart { public int DayNumber; }
   public class OnDayEnd { public int DayNumber; }
   public class OnBuffPurchased { public BuffData Buff; }
   public class OnMissionsReshuffled { }
   public class OnMissionDiscarded { public MissionInstance Mission; }
   public class OnDailyEventActivated { public DailyEventData Event; }
   public class OnConsumableUsed { public ConsumableType Type; public MissionInstance TargetMission; }
   ```
3. Crear `Assets/Scripts/Core/GameManager.cs`:
   - Singleton persistente (DontDestroyOnLoad)
   - Mantiene el estado global: `DayNumber`, `DayLives` (= 3, reset cada día), `CurrentCoins`, `TotalFameEarned`
   - Expone métodos: `StartDay()`, `EndDay()`, `LoseLife()`, `AddCoins()`, `SpendCoins()`
   - Escucha `OnHeroLeft { WasAngry = true }` → llama `LoseLife()`
   - `LoseLife()`: decrementa `DayLives`, publica `OnLifeLost { LivesRemaining }`. Si `DayLives <= 0` → publica `OnGameOver`
   - Escucha `OnDayThresholdReached` → llama `EndDay()`, resetea `DayLives = 3`, incrementa `DayNumber`
4. Probar con un test rápido en el editor: suscribirse a un evento, publicarlo, verificar que el handler se invoca.

**Criterio de éxito:** El EventBus compila sin errores. Se puede publicar y recibir eventos entre dos scripts sin referencia directa.

**Notas de implementación:**
- `Assets/Scripts/Core/EventBus.cs` — clase estática genérica con diccionario `Dictionary<Type, List<Delegate>>`. Itera sobre snapshot de la lista para soportar (des)suscripción durante dispatch.
- `Assets/Scripts/Core/GameEvents.cs` — todos los eventos del catálogo definidos.
- `Assets/Scripts/Core/GameManager.cs` — singleton DontDestroyOnLoad, escucha `OnHeroLeft` y `OnDayThresholdReached`. También acumula `TotalFameEarned` escuchando `OnMissionCompleted`.
- **Stubs creados** para que todo compile desde el día 0 (se reemplazarán en sus respectivas tareas):
  - `Assets/Scripts/Data/HeroData.cs` → tarea 1.1
  - `Assets/Scripts/Data/MissionData.cs` → tarea 1.2
  - `Assets/Scripts/Data/BuffData.cs` → tarea 1.3
  - `Assets/Scripts/Data/DailyEventData.cs` → tarea 3.4
  - `Assets/Scripts/Economy/ConsumableType.cs` → tarea 3.2

---

## FASE 1 — SISTEMAS DE DATOS CORE
**Objetivo:** Definir las estructuras de datos de héroe, misión y buff como ScriptableObjects. Implementar la generación procedural de héroes y el mazo de misiones.
**Duración:** Día 1 (tarde) – Día 2

---

### TAREA 1.1 — HeroData ScriptableObject
**Estado:** `[x] COMPLETADA`
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
       public HeroRarity Rarity; // Common, Rare, Epic

       // Rangos para generación aleatoria
       public Vector2Int StrengthRange;    // ej: (1, 10)
       public Vector2Int DexterityRange;
       public Vector2Int IntelligenceRange;
       public Vector2Int CharismaRange;
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
       public HeroRarity Rarity;
       public Sprite Portrait;
       public float Patience;         // segundos antes de irse (Impulsivo: BasePatience / 2)

       // Rasgos procedurales — 2-3 por héroe, asignados en Generate()
       public List<HeroTrait> Traits;
       public List<HeroStat> HiddenStats;   // Solo relevante si tiene Distrustful; 2 stats elegidos al azar

       // Estado en runtime
       public bool IsWaiting;
       public bool IsDead;

       // Método de construcción
       // Generate() asigna Traits: elige 2-3 rasgos al azar del enum (sin repetición)
       // Si Impulsive está en Traits: Patience = BasePatience / 2
       public static HeroInstance Generate(HeroData template) { ... }

       // Verifica si el héroe acepta la misión según sus rasgos:
       //   Greedy    → false si mission.Template.CoinsReward < 6
       //   Demanding → false si mission.Template.Rank <= R3
       //   (resto)   → true
       public bool WillAcceptMission(MissionInstance mission) { ... }

       // Calcula probabilidad de éxito (0.0 a 1.0)
       // Specialist: si PrimaryStat de la misión == el stat más alto del héroe → +0.15
       public float CalculateSuccessChance(MissionInstance mission) { ... }

       // Devuelve el stat más alto del héroe (para Specialist)
       public HeroStat GetDominantStat() { ... }
   }
   ```
3. Crear `Assets/Scripts/Utils/Enums.cs` con:
   ```csharp
   public enum HeroStat { Strength, Dexterity, Intelligence, Charisma }
   public enum HeroRarity { Common, Rare, Epic }

   // Rasgos procedurales — se asignan 2-3 al spawnear cada héroe
   public enum HeroTrait {
       Greedy,       // Solo acepta misiones con CoinsReward >= 6
       Specialist,   // +15% chance si el PrimaryStat de la misión es el stat más alto del héroe
       Reckless,     // Acepta cualquier misión; si éxito en misión con chance < 30%: CoinsEarned × 2
       Distrustful,  // Sus stats se muestran parcialmente ocultos en UI (el jugador ve 2 de 4)
       Demanding,    // Rechaza misiones de Rank R1–R3 (dificultad baja)
       Impulsive,    // Patience drena 2× más rápido; CoinsEarned × 2 en cualquier misión
       // Loyal — STRETCH GOAL (requiere persistencia cross-run): paciencia ilimitada si ya completó
       //         una misión en una run anterior. No implementar en jam.
   }

   // Rango 1–12, tipo naipe inglés (A=1 hasta Q=12)
   // A mayor rango: mayores requisitos (calculados por fórmula) y mayor Fama/coins
   public enum MissionRank { R1=1, R2, R3, R4, R5, R6, R7, R8, R9, R10, R11, R12 }

   // Palo de la misión = stat primario. Los 4 palos × 12 rangos = 48 misiones únicas
   // MissionSuit es alias semántico de HeroStat para contexto de misiones
   // (En código se puede usar HeroStat directamente como palo)

   public enum BuffType { HeroStatBoost, MissionHintReveal, MissionReshuffle, PatienceBoost, FailProtection }
   public enum DailyEventType { StatBuff, StatDebuff, SpawnSpeedBuff, SpawnSpeedDebuff, FameBoost, FameDebuff, SlotReduction }

   // Tipos de consumibles de un solo uso (se usan desde el inventario del HUD)
   public enum ConsumableType {
       StatRevealHand,    // Revela el PrimaryStat de TODAS las cartas en la mano actual
       FullStatReveal,    // Revela todos los requisitos (primario + secundarios) de UNA carta elegida
   }
   ```
4. Crear 4-5 HeroData ScriptableObjects de ejemplo en `Assets/Data/Heroes/`:
   - `Hero_Knight.asset` (Fuerza alta)
   - `Hero_Rogue.asset` (Destreza alta)
   - `Hero_Mage.asset` (Inteligencia alta)
   - `Hero_Bard.asset` (Carisma alto)
5. Implementar el método `CalculateSuccessChance`:
   - Obtener el stat primario de la misión
   - Fórmula: `chance = (heroStat / missionRequirement).Clamp(0, 1)`
   - Aplicar bonus por stats secundarios: `+5% por cada stat secundario que supera el umbral`

**Criterio de éxito:** Se puede generar un `HeroInstance` desde cualquier `HeroData`. Los valores de stats están dentro del rango definido. `CalculateSuccessChance` devuelve valores coherentes.

**Notas de implementación:**
- `Assets/Scripts/Utils/Enums.cs` — Enums creados: `HeroStat`, `HeroRarity`, `HeroTrait`, `MissionRank`, `BuffType`, `DailyEventType`. `ConsumableType` ya existía en `Economy/ConsumableType.cs` (no duplicada).
- `Assets/Scripts/Data/HeroData.cs` — ScriptableObject completo con rangos de stats, personalidad (PatienceMultiplier, BasePatience) y FlavorText.
- `Assets/Scripts/Heroes/HeroInstance.cs` — Clase runtime con `Generate()`, `WillAcceptMission()`, `CalculateSuccessChance()`, `GetDominantStat()`. Lógica de Impulsive (Patience/2) y Distrustful (HiddenStats) implementada.
- `Assets/Scripts/Data/MissionData.cs` (stub actualizado) — Agregados `Rank`, `PrimaryStat`, `SecondaryStats`, `CoinsReward`, `FameReward` y métodos `GetEffectivePrimaryReq()`/`GetEffectiveSecondaryReq()` a `MissionInstance` para que `HeroInstance` compile. El stub anterior tenía `BaseData`; ahora usa `Template` (alineado con la spec de Tarea 1.2).
- `Assets/Data/Heroes/` — 4 ScriptableObjects creados: `Hero_Knight` (STR), `Hero_Rogue` (DEX), `Hero_Mage` (INT), `Hero_Bard` (CHA).

---

### TAREA 1.2 — MissionData ScriptableObject
**Estado:** `[x] COMPLETADA`
**Responsable:** Dev B
**Duración estimada:** 2 horas
**Dependencias:** 1.1 (necesita Enums)

**Descripción:**
Definir la estructura de datos de una misión. Las misiones tienen requisitos ocultos (stat principal + secundarios), descripción de sabor ambigua, y dificultad escalable. Las pistas son ítems comprables en la tienda o desbloqueables por eventos del juego (NO se desbloquean automáticamente por duración de run). Pueden ser permanentes o de un solo uso (por definir en balance).

**Pasos:**
1. Crear `Assets/Scripts/Data/MissionData.cs`:
   ```csharp
   [CreateAssetMenu(menuName = "Questline/Mission Data")]
   public class MissionData : ScriptableObject {
       [Header("Identidad")]
       public string MissionTitle;
       [TextArea] public string Description;       // Descripción ambigua para el jugador
       public Sprite MissionArt;

       [Header("Rango (sistema naipe inglés, 1–12)")]
       public MissionRank Rank;                   // R1 = más fácil, R12 = más difícil
       // El rango escala automáticamente los requisitos y las recompensas.
       // Un héroe con stat promedio puede superar misiones hasta ~Rango 5–6.
       // Rangos 7–12 requieren héroes con stats altos o buffs acumulados.

       [Header("Requisitos (OCULTOS al jugador)")]
       // PrimaryStatRequirement NO se almacena aquí — se calcula en runtime por fórmula:
       //   effectiveReq(rank, day) = (int)Rank * (1.5f + day * 0.2f)
       // Ejemplo: Rank 4, Día 1 → req = 4*(1.7) = 6.8 ≈ 7
       //          Rank 4, Día 5 → req = 4*(2.5) = 10
       public HeroStat PrimaryStat;               // El palo de esta misión (Fuerza/Destreza/etc.)
       public HeroStat[] SecondaryStats;          // Stats de apoyo (opcionales, 0-2)
       // Secondary requirements = 80% del primary requirement del mismo día

       [Header("Recompensas (escalan con Rank — base configurada en DayConfig)")]
       public int CoinsReward;                    // Propina base (Rank * 3 coins aprox.)
       public int FameReward;                     // Fama base (Rank * 5 aprox.)

       [Header("Pistas (desbloqueables por tienda o eventos)")]
       // Las pistas NO se revelan automáticamente; se compran en la tienda
       // o se otorgan como reward de eventos en el juego
       public MissionHint[] Hints;                // Array de pistas disponibles para esta misión

   }

   [System.Serializable]
   public class MissionHint {
       public string HintText;        // ej: "Fuerza++" o "Inteligencia"
       public Color HintColor;        // Verde=bueno, Naranja=precaución
       public HeroStat RelatedStat;   // Stat al que hace referencia
       // Sin UnlockAtRunLevel: las pistas se activan por compra en tienda o por eventos
   }
   ```
2. Crear `Assets/Scripts/Missions/MissionInstance.cs`:
   ```csharp
   public class MissionInstance {
       public MissionData Template;
       public int DayNumber;              // Día en que fue generada (afecta el requirement calculado)
       public bool IsCompleted;
       public bool IsFailed;
       public List<int> RevealedHintIndices;  // Índices de pistas actualmente visibles

       // Requisito calculado por fórmula (no hardcodeado en el SO)
       public int GetEffectivePrimaryReq()
           => Mathf.RoundToInt((int)Template.Rank * (1.5f + DayNumber * 0.2f));
       public int GetEffectiveSecondaryReq()
           => Mathf.RoundToInt(GetEffectivePrimaryReq() * 0.8f);

       // Revela una pista específica (llamado por consumible o evento)
       public void RevealHint(int hintIndex) { ... }

       // Devuelve las pistas actualmente reveladas
       public List<MissionHint> GetRevealedHints() { ... }

       // Fábrica
       public static MissionInstance FromData(MissionData data, int dayNumber) { ... }
   }
   ```
3. Crear **48 MissionData ScriptableObjects** en `Assets/Data/Missions/` — 4 por rango (uno por stat/palo):
   - Estructura: `Mission_R{rango}_{Stat}.asset` → ej: `Mission_R1_Str.asset`, `Mission_R1_Dex.asset`, etc.
   - Rangos 1–4: descripción bastante directa (para días 1–2)
   - Rangos 5–8: descripción ambigua (para días 3–5)
   - Rangos 9–12: descripción engañosa o con múltiples lecturas (para días 6+)
   - `FameReward` base: `rank * 5` coins (Rango 1 = 5, Rango 12 = 60). Ajustar en balance.
   - `CoinsReward` base: `rank * 3` (Rango 1 = 3, Rango 12 = 36). Ajustar en balance.
   - Nota: las 48 misiones constituyen el catálogo completo del juego, equivalente a un naipe inglés.

   **Ejemplos de descripción ambigua:**
   - *"El Bosque Sombrío necesita a alguien que pueda moverse sin ser visto entre las sombras."* → PrimaryStat: Destreza
   - *"Los comerciantes del puerto necesitan convencer a un noble corrupto."* → PrimaryStat: Carisma
   - *"Una antigua cripta ha sido profanada. Alguien debe descifrar los sellos arcanos."* → PrimaryStat: Inteligencia
   - *"Los muros de Ironhaven se están derrumbando. Se necesita mano de obra pesada."* → PrimaryStat: Fuerza
4. Para cada misión, diseñar al menos 2 pistas (que el jugador puede comprar en tienda):
   - Pista 1: vaga (ej: "Requiere habilidad física") — costo bajo en tienda
   - Pista 2: directa (ej: "Fuerza ++", color verde) — costo mayor en tienda
   - Las pistas también pueden otorgarse como recompensa de eventos aleatorios del juego

**Criterio de éxito:** Se puede crear una MissionInstance desde cualquier MissionData. `GetRevealedHints()` devuelve solo las pistas marcadas como reveladas. Las pistas inician ocultas y solo se revelan al activarlas explícitamente. El `FameReward` de misiones de Rango alto es notablemente mayor que el de Rango bajo.

**Notas de implementación:**
- `Assets/Scripts/Data/MissionData.cs` — ScriptableObject completo con identidad, rango, stats primario/secundarios, recompensas y array de `MissionHint[]`. El requisito numérico se calcula 100% en runtime (no se almacena).
- `Assets/Scripts/Missions/MissionInstance.cs` — Clase runtime con `RevealedHintIndices`, `RevealHint(int)`, `RevealAllHints()`, `GetRevealedHints()`, `HasAnyRevealedHint()` y la fábrica `FromData(MissionData, int)`. Las fórmulas `GetEffectivePrimaryReq()` / `GetEffectiveSecondaryReq()` migradas del stub anterior.
- `Assets/Editor/MissionDataGenerator.cs` — Script de editor (menú `Questline → Generate Mission Assets`) que crea los **48 MissionData assets** automáticamente en `Assets/Data/Missions/`. Naming: `Mission_R{rank}_{Stat}.asset`. Idempotente: no sobreescribe assets ya existentes. Recompensas base: `CoinsReward = rank × 3`, `FameReward = rank × 5`. Cada misión incluye 2 pistas (vaga/naranja + directa/verde). Descripciones por rango: R1-R4 directas, R5-R8 ambiguas, R9-R12 engañosas.
- **Para generar los assets:** abrir Unity → menú `Questline → Generate Mission Assets`. Los 48 `.asset` aparecen en `Assets/Data/Missions/`.

---

### TAREA 1.3 — BuffData ScriptableObject
**Estado:** `[x] COMPLETADA`
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
       public int StatBoostAmount;          // +N al stat (permanente mientras dure la partida)
       public int HintsToReveal;            // Para MissionHintReveal: cuántas pistas revela al usarse
       public float PatienceBoostPercent;   // Para PatienceBoost
       public int FailProtectionCount;      // Para FailProtection (absorbe N muertes de héroe)
   }
   ```

   **Consumibles (inventario de un solo uso — no BuffData, sino ConsumableData):**
   ```csharp
   [CreateAssetMenu(menuName = "Questline/Consumable")]
   public class ConsumableData : ScriptableObject {
       public string Name;
       [TextArea] public string Description;
       public Sprite Icon;
       public int Cost;
       public ConsumableType Type;
       // StatRevealHand: no necesita parámetros extra
       // FullStatReveal: tampoco — el jugador elige la carta al usarlo
   }
   ```

2. Crear los siguientes items en `Assets/Data/Buffs/` y `Assets/Data/Consumables/`:

   **Buffs permanentes (tienda):**
   - `Buff_StrBoost.asset` — +3 Fuerza a todos los héroes, costo: 5 coins
   - `Buff_DexBoost.asset` — +3 Destreza a todos los héroes, costo: 5 coins
   - `Buff_IntBoost.asset` — +3 Inteligencia a todos los héroes, costo: 5 coins
   - `Buff_ChaBoost.asset` — +3 Carisma a todos los héroes, costo: 5 coins
   - `Buff_Patience.asset` — +50% tiempo de paciencia, costo: 8 coins
   - `Buff_HintReveal.asset` — Al usarse: revela el PrimaryStat de una misión aleatoria en mano, costo: 6 coins
   - `Buff_Reshuffle.asset` — Baraja de nuevo el pool diario + roba nueva mano, costo: 3 coins
   - `Buff_FailSave.asset` — Absorbe 1 muerte de héroe (el héroe falla pero no cuenta como vida perdida), costo: 12 coins
   - `Buff_MegaBoost.asset` — +10 a un stat aleatorio de todos los héroes, costo: 20 coins

   **Consumibles de un solo uso (tienda → inventario HUD):**
   - `Consumable_StatRevealHand.asset` — Revela el PrimaryStat de TODAS las cartas en mano en este momento, costo: 8 coins
   - `Consumable_FullStatReveal.asset` — Revela TODOS los requisitos (primario + secundarios) de UNA carta a elección, costo: 15 coins

3. Crear `Assets/Scripts/Core/BuffManager.cs`:
   ```csharp
   // DOS capas separadas:
   List<BuffData> _permanentBuffs;     // comprados en tienda, persisten toda la partida
   List<StatModifier> _dailyModifiers; // del evento del día, se limpian al inicio de cada día

   struct StatModifier { public HeroStat Stat; public int Amount; }

   // API pública:
   void ApplyBuff(BuffData buff);                     // para tienda
   void ApplyDailyModifier(HeroStat stat, int amount); // para DailyEventManager
   void ClearDailyModifiers();                         // llamado por RunManager en StartDay()
   int  GetActiveStatBonus(HeroStat stat);             // suma ambas capas
   float GetPatienceMultiplier();
   bool  HasFailProtection();
   void  ConsumeFailProtection();
   ```

4. Crear `Assets/Scripts/Core/ConsumableManager.cs`:
   - Mantiene el inventario de consumibles del jugador (`Dictionary<ConsumableType, int>` cantidad por tipo)
   - `AddConsumable(ConsumableData)` — llamado al comprarlo en la tienda
   - `UseConsumable(ConsumableType, MissionInstance targetMission = null)`:
     - `StatRevealHand`: itera todas las `MissionCard` en la mano, llama `RevealHint(0)` en cada una (la pista de PrimaryStat)
     - `FullStatReveal`: llama `RevealHint(0)` y `RevealHint(1)` en la `targetMission` elegida por el jugador
   - Publica `OnConsumableUsed`
   - El inventario del HUD (TAREA 4.1) se subscribe a los cambios del ConsumableManager

**Criterio de éxito:** Los buffs se pueden crear en el editor. `BuffManager.ApplyBuff` modifica los valores correctamente.

**Notas de implementación:**
- `Assets/Scripts/Data/BuffData.cs` — ScriptableObject completo: nombre, descripción, ícono, costo, `BuffType`, `IsPermanent`, y campos condicionados por tipo (`AffectedStat`/`StatBoostAmount`, `HintsToReveal`, `PatienceBoostPercent`, `FailProtectionCount`).
- `Assets/Scripts/Data/ConsumableData.cs` — Nuevo SO para consumibles de un solo uso: nombre, descripción, ícono, costo, `ConsumableType`.
- `Assets/Scripts/Core/BuffManager.cs` — Singleton `MonoBehaviour` con dos capas: `_permanentBuffs` (tienda) y `_dailyModifiers` (eventos diarios). API pública: `ApplyBuff()`, `ApplyDailyModifier()`, `ClearDailyModifiers()`, `GetActiveStatBonus()`, `GetPatienceMultiplier()`, `HasFailProtection()`, `ConsumeFailProtection()`. Publica `OnBuffPurchased` al aplicar cualquier buff.
- `Assets/Scripts/Core/ConsumableManager.cs` — Singleton `MonoBehaviour` con inventario `Dictionary<ConsumableType, int>`. API: `AddConsumable()`, `GetCount()`, `UseConsumable()`. `StatRevealHand` delega a `MissionDeck.Instance.CurrentHand`; `FullStatReveal` llama `RevealHint(0)` y `RevealHint(1)` sobre la misión objetivo. Publica `OnConsumableUsed`.
- `Assets/Scripts/Missions/MissionDeck.cs` — **Stub** creado para que `ConsumableManager` compile. Implementación completa en Tarea 2.1.
- `Assets/Editor/BuffDataGenerator.cs` — Script de editor (menú `Questline → Generate Buff & Consumable Assets`) que crea los **9 BuffData** en `Assets/Data/Buffs/` y los **2 ConsumableData** en `Assets/Data/Consumables/`. Idempotente: no sobreescribe assets ya existentes.
- **Para generar los assets:** abrir Unity → menú `Questline → Generate Buff & Consumable Assets`.

---

### TAREA 1.4 — Sistema de evaluación de misiones
**Estado:** `[x]`
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
       public bool ProtectionUsed;    // Si el buff absorbió la muerte del héroe
       public float SuccessChance;    // Para mostrar en UI de post-misión
       public int CoinsEarned;
       public int FameEarned;         // Ya incluye el multiplicador del evento del día
       public int XPEarned;
       public HeroInstance Hero;
       public MissionInstance Mission;
   }
   ```
2. La fórmula de éxito detallada:
   ```
   effectiveStat    = hero.PrimaryStat + BuffManager.GetActiveStatBonus(primaryStat)
   effectiveReq     = mission.GetEffectivePrimaryReq()   // calculado por fórmula en MissionInstance
   baseChance       = Mathf.Clamp01(effectiveStat / effectiveReq)
   bonusChance      = 0
   secondaryReq     = mission.GetEffectiveSecondaryReq()
   foreach secondaryStat:
       if hero.secondaryStat >= secondaryReq:
           bonusChance += 0.05f
   // Rasgos que modifican chance:
   if hero.Traits.Contains(Specialist) && mission.Template.PrimaryStat == hero.GetDominantStat():
       bonusChance += 0.15f
   finalChance = Mathf.Clamp01(baseChance + bonusChance)
   success = Random.value <= finalChance

   // Recompensas — FameModifier aplicado aquí (Opción A: antes de publicar el evento)
   float fameModifier = DailyEventManager.GetFameModifier()  // 1.0 por defecto
   result.FameEarned  = Mathf.RoundToInt(mission.Template.FameReward * fameModifier)
   result.CoinsEarned = mission.Template.CoinsReward
   // Rasgos que modifican recompensas:
   if hero.Traits.Contains(Impulsive):
       result.CoinsEarned *= 2
   if hero.Traits.Contains(Reckless) && success && finalChance < 0.30f:
       result.CoinsEarned *= 2
   ```
3. Manejo de rechazo de misión por el héroe:
   ```
   hero.WillAcceptMission(mission):
       if hero.Traits.Contains(Greedy)    && mission.Template.CoinsReward < 6:  return false
       if hero.Traits.Contains(Demanding) && mission.Template.Rank <= R3:        return false
       return true
   ```
4. Escribir tests manuales en un MonoBehaviour de prueba:
   - Héroe con Fuerza 10 vs misión que requiere Fuerza 8 → alta probabilidad de éxito
   - Héroe con Fuerza 3 vs misión que requiere Fuerza 8 → baja probabilidad
5. Publicar eventos al terminar:
   - Éxito → `EventBus.Publish(new OnMissionCompleted { CoinsEarned, FameEarned })` — FameEarned ya incluye el modificador del evento del día
   - Fallo → `EventBus.Publish(new OnMissionFailed {...})` → si no hay protección, `EventBus.Publish(new OnHeroDied {...})`
   - Todas las misiones tienen peso de fallo = 1. No hay penalizaciones dobles.

**Criterio de éxito:** `MissionEvaluator.Evaluate` devuelve resultados estadísticamente coherentes. Los rasgos Greedy, Demanding, Specialist, Reckless e Impulsive modifican el resultado correctamente. Los eventos se publican correctamente.

**Implementación (completada 2026-03-24):**
- `Assets/Scripts/Missions/MissionEvaluator.cs` — clase estática con `Evaluate(HeroInstance, MissionInstance) → MissionResult` y clase `MissionResult`.
- `Assets/Scripts/Core/DailyEventManager.cs` — stub creado para que compile; expone `GetFameModifier()` (devuelve 1.0f hasta Tarea 3.4).
- `Assets/Scripts/Missions/MissionEvaluatorTest.cs` — MonoBehaviour de tests manuales (8 casos de prueba, ejecutar en Play Mode; eliminar en build final).
- **Nota Tarea 3.4:** `DailyEventManager` es un stub. Al implementar 3.4, completar `_fameModifier` con la lógica del evento del día activo.

---

## GAME LOOP — ESTRUCTURA DE UN DÍA
> Esta sección describe el loop definitivo acordado por el equipo. Es la referencia de diseño para todas las tareas de Fase 2 y 3.

**Un "día" en Questline funciona así:**

1. **Inicio del día:** Se revela el **Evento del Día** (buff o debuf aleatorio que dura todo el día). El jugador empieza con **3 vidas**. Se genera el pool diario de misiones desde el catálogo de 48 (12 rangos × 4 palos), con rangos escalados por número de día.
2. **La mano del jugador:** El jugador tiene en mano un número limitado de cartas (ej: 5). Las cartas vienen del pool diario. A medida que el jugador asigna misiones, la mano se rellena automáticamente desde el pool.
3. **Presión:** Los héroes llegan continuamente con timers de paciencia. Cada héroe que se va sin recibir misión = **-1 vida**. Si el jugador deja acumular demasiados héroes esperando simultáneamente, no puede asignar misiones a todos a tiempo.
4. **Asignación:** El jugador asigna misiones de su mano a los héroes disponibles mediante drag & drop. Cada misión exitosa otorga **Fama** a la guild (además de coins).
5. **Fin del día — Victoria:** Cuando la **Fama acumulada supera el umbral del día**, el día termina exitosamente. → Tienda inter-día.
6. **Fin del día — Game Over:** Si el jugador pierde las **3 vidas** antes de alcanzar el umbral de Fama, es Game Over.
7. **Entre días — Tienda:** Al completar el día, el jugador accede a la tienda para gastar coins en buffs y consumibles.
8. **Nuevo día:** Las vidas se resetean a 3. Se genera un pool nuevo con mayor dificultad. Coins y buffs persisten.

```
[3 vidas al inicio]  [Evento del Día revelado]
        ↓
[Pool diario: subset escalado del catálogo de 48 misiones]
        ↓ (roba automáticamente a la mano de 5)
  [Héroe llega → timer de paciencia comienza]
        ↓ (drag & drop antes de que expire)
  [Héroe recibe misión → Evaluación → coins + Fama]
        ↓
  [¿Héroe se fue sin misión? → -1 vida]
        ↓
  Fama >= umbral → [Tienda] → Nuevo día (+dificultad)
  Vidas = 0 → [Game Over]
```

---

## FASE 2 — LOOP DE GAMEPLAY
**Objetivo:** Implementar la cola de héroes, el mazo de misiones (pool diario + mano), el drag & drop de cartas, y los timers de paciencia. Esto es el corazón jugable del juego.
**Duración:** Día 2 (tarde) – Día 3

---

### TAREA 2.1 — HeroQueue: cola de héroes y slots
**Estado:** `[x] COMPLETADA`
**Responsable:** Dev C
**Duración estimada:** 2 horas
**Dependencias:** 1.1, 0.3, 0.4

**Descripción:**
Implementar el sistema que gestiona la llegada de héroes, los 4 slots visibles simultáneos, y el timer de paciencia por héroe. Cuando el timer expira, el héroe se va (no muere, pero se pierde la oportunidad). Si el jugador asigna una misión, el slot se libera.

**Mecánica de vidas:** Cada héroe que pierde la paciencia y se va sin recibir misión cuesta **1 vida**. El jugador tiene **3 vidas por día** (se resetean al inicio de cada día). Perder las 3 vidas = Game Over. No hay contador intermedio ni acumulación entre días — cada día es independiente.

**Pasos:**
1. Crear `Assets/Scripts/Heroes/HeroSpawner.cs`:
   - Referencia al array de `HeroData` templates disponibles
   - `SpawnHero()` — elige un template aleatorio, genera un `HeroInstance` con stats escalados por `DayNumber`, asigna **2-3 rasgos aleatorios** (`HeroTrait`) sin repetición, lo agrega a la cola
   - `SpawnIntervalMultiplier = 1.0f` — modificado por `DailyEventManager` para eventos de velocidad de spawn
   - Intervalo efectivo: `SpawnIntervalBase * SpawnIntervalMultiplier` (clampeado a `SpawnIntervalMin`)
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
   - Al llegar a 0: héroe se va enojado, publica `OnHeroLeft { WasAngry = true }`
   - `GameManager` escucha `OnHeroLeft { WasAngry = true }` y llama `LoseLife()` directamente
   - Al impaciencia < 20%: reproducir animación de nerviosismo
4. Crear `Assets/Scripts/Heroes/HeroSlotUI.cs`:
   - Muestra el retrato del héroe
   - Muestra stats (con iconos o texto); si el héroe tiene rasgo **Distrustful**: ocultar 2 de los 4 stats (elegir cuáles ocultar aleatoriamente al spawnear, guardar en `HeroInstance.HiddenStatIndices`)
   - Muestra barra de paciencia (héroe con **Impulsive**: barra roja desde el inicio para señalizar urgencia)
   - Muestra iconos de rasgos debajo del retrato (2-3 iconos pequeños con tooltip al hover)
   - Efecto de entrada (tween desde el borde de la pantalla)
   - Efecto de salida al asignar misión o irse
5. Definir los datos de la cola en un `HeroQueueConfig` (SO):
   - `SpawnIntervalBase`: 15 segundos
   - `SpawnIntervalMin`: 5 segundos (en días avanzados)
   - `MaxSimultaneousHeroes`: 4
   - Lista de HeroData disponibles (todos los héroes son del mismo tipo base con stats variables)
   - Stats de héroes generados escalan con `DayNumber` usando esta fórmula:
     ```
     heroStat = Random.Range(2 + dayNumber, 5 + dayNumber * 2)
     // Día 1: [3, 7]   Día 5: [7, 15]   Día 10: [12, 25]
     ```
   - Misión Rank R en Día N tiene requisito: `R * (1.5f + N * 0.2f)`
     ```
     // Día 1 R4: 4*1.7=6.8≈7  →  héroe avg 5 → 71% ✓
     // Día 5 R7: 7*2.5=17.5   →  héroe avg 11 → 63% ✓
     // Día 5 R12: 12*2.5=30   →  héroe avg 11 → 37% (muy difícil, correcto)
     ```

**Criterio de éxito:** Los héroes aparecen en pantalla, tienen barras de paciencia que se agotan, y desaparecen solos si no reciben misión.

**Implementación (completada 2026-03-24):**
- `Assets/Scripts/Heroes/HeroQueueConfig.cs` — ScriptableObject de configuración: `SpawnIntervalBase` (15s), `SpawnIntervalMin` (5s), `MaxSimultaneousHeroes` (4), array `HeroTemplates`. Crear el asset en `Assets/Data/Heroes/` y asignarlo en Inspector.
- `Assets/Scripts/Heroes/HeroSpawner.cs` — Singleton MonoBehaviour. Escucha `OnDayStart`/`OnDayEnd`/`OnGameOver` para arrancar/parar la coroutine de spawn. `SpawnIntervalMultiplier` expuesto para que `DailyEventManager` lo modifique (Tarea 3.4). Stats escalados por día: `Random.Range(2+day, 5+day*2)`. Publica `OnHeroArrived`.
- `Assets/Scripts/Heroes/HeroPatience.cs` — MonoBehaviour de timer por slot. `Activate(hero)` inicia la cuenta atrás (aplica `BuffManager.GetPatienceMultiplier()`). Barra de colores verde→naranja→rojo; Impulsive arranca en rojo. Threshold <20% llama `TriggerNervousAnimation()` (placeholder para Fase 4/5). Al expirar publica `OnHeroLeft { WasAngry=true }`.
- `Assets/Scripts/Heroes/HeroQueue.cs` — Singleton MonoBehaviour. Gestiona array de `HeroSlotUI[]` (asignar en Inspector). Cola interna `Queue<HeroInstance>` para overflow. Escucha `OnHeroArrived`, `OnMissionAssigned`, `OnHeroDied`, `OnHeroLeft` para fill/remove de slots.
- `Assets/Scripts/Heroes/HeroSlotUI.cs` — MonoBehaviour de presentación de un slot. `SetHero()` → actualiza retrato, stats (Distrustful oculta 2), rasgos, activa `HeroPatience`. `ClearHero()` → pausa timer, dispara animación Exit (AnimationEvent llama `FinishClear()`). Iconos de rasgos (abreviatura 3 letras); hook de tooltip comentado para Fase 4.
- **Setup requerido:** en la escena, asignar los 4 `HeroSlotUI` al array `_slots` de `HeroQueue`. Asignar `HeroQueueConfig` asset a `HeroSpawner`. Cada slot necesita un `HeroPatience` asignado en `_patience`. El Animator es opcional; sin él `FinishClear()` se llama inmediatamente.
- **Nota Tarea 3.4:** `HeroSpawner.SpawnIntervalMultiplier` se modifica desde `DailyEventManager` al procesar eventos `SpawnSpeedBuff`/`SpawnSpeedDebuff`.

---

### TAREA 2.2 — MissionDeck: pool diario y mano de misiones
**Estado:** `[x]`
**Responsable:** Dev D
**Duración estimada:** 2 horas
**Dependencias:** 1.2, 0.4

**Descripción:**
Implementar el pool de misiones diarias y la mano visible del jugador. Cada día se genera un pool finito de misiones. El jugador tiene una mano de N cartas que se rellena desde el pool a medida que asigna misiones. Al agotarse el pool, el día termina. Esta es la estructura del Game Loop acordada por el equipo.

**Pasos:**
1. Crear `Assets/Scripts/Missions/MissionDeck.cs`:
   - Lista de todos los `MissionData` disponibles (asignados desde el Inspector)
   - `BuildDailyPool(int dayNumber)` — genera el pool del día: subset shuffleado de misiones disponibles, escalado por día (más misiones difíciles en días avanzados)
   - `DrawCard()` → devuelve una `MissionInstance` del tope del pool; devuelve null si el pool está vacío
   - `Reshuffle()` — desordena el pool restante (buff de reshuffle)
   - `HandSize`: cuántas cartas tiene el jugador en mano (default: 5)
   - `DailyPoolSize`: total de misiones disponibles en el pool del día (configurable, ej: 15-20)
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

**Notas de implementación:**
- `MissionDeck.cs` — Implementación completa del stub. Incluye `BuildDailyPool`, `DrawCard`, `FillHand`, `RemoveFromHand`, `Reshuffle`, eventos `OnCardDrawn`/`OnPoolExhausted`. Rango por día con fórmula inline; se reemplaza con `DayConfig` en Tarea 2.4.
- `MissionCard.cs` — MonoBehaviour UI nuevo. Estados `Normal/Hovered/Selected/InTransit`, animaciones `PlayEnterAnimation` (slide+fade) y `PlayDiscardAnimation` (caída+fade) vía Coroutines. Se suscribe a `OnConsumableUsed` para refrescar pistas.
- `MissionCardArea.cs` — Layout manager nuevo. Escucha `MissionDeck.OnCardDrawn`, instancia prefabs, los distribuye horizontalmente centrados y expone `RemoveCard()` para Tarea 2.3.
- **Pendiente de setup en escena:** asignar `_allMissions` en `MissionDeck`, asignar `_cardPrefab` y `_deckAnchor` en `MissionCardArea`, crear prefab de carta con las referencias TMP/Image necesarias.

---

### TAREA 2.3 — Drag & Drop: asignación de misión a héroe
**Estado:** `[x]` ✅ Completada 2026-03-24
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

**Archivos implementados:**
- `Assets/Scripts/UI/CardDragHandler.cs` — `IBeginDragHandler/IDragHandler/IEndDragHandler`. Se reparenta al canvas raíz durante el drag, desactiva `blocksRaycasts` para que los drops pasen al target, y hace tween de regreso si el drop no fue aceptado. Expone `CardDragHandler.Current` (estático) para que `HeroDropTarget` calcule el highlight.
- `Assets/Scripts/UI/HeroDropTarget.cs` — `IDropHandler + IPointerEnterHandler/Exit`. Muestra borde verde/rojo según `WillAcceptMission`. Al aceptar: publica `OnMissionAssigned`, llama `MissionCardArea.RemoveCard`, `MissionDeck.RemoveFromHand` y `MissionEvaluator.Evaluate`. Al rechazar: shake + texto "¡Me niego!".

**Pendiente de setup en escena:**
- Añadir `CardDragHandler` al prefab de `MissionCard` (junto a `MissionCard` existente).
- Añadir `HeroDropTarget` a cada `HeroSlotUI` en la escena.
- En cada `HeroDropTarget`: asignar en el Inspector una `Image` de borde para el highlight y (opcional) un `TextMeshProUGUI` para el texto de rechazo.
- Asegurarse de que el `Canvas` raíz tenga `GraphicRaycaster` y que el EventSystem esté en la escena.
- Las animaciones de "héroe parte de aventura" (paso 6-7 del flujo) son responsabilidad de **Tarea 4.2**.

---

### TAREA 2.4 — RunManager: gestión de días/runs
**Estado:** `[x]` ✅ Completada 2026-03-24
**Responsable:** Dev D
**Duración estimada:** 1.5 horas
**Dependencias:** 0.4, 2.1, 2.2

**Descripción:**
Implementar la lógica de progresión por días. Cada día termina cuando la **Fama acumulada supera el umbral del día**. La Fama se gana completando misiones (más Fama por misiones de mayor rango). A mayor día, mayor es el umbral y mayor el rango promedio de misiones disponibles.

**Pasos:**
1. Crear `Assets/Scripts/Core/RunManager.cs`:
   - Mantiene `DayNumber` (incrementa cada día)
   - `StartDay()`: inicializa el pool diario (escalado por DayNumber), activa el evento del día, publica `OnRunStart`
   - Escucha `OnFameEarned` y verifica si `TotalFame >= DayFameThreshold`
   - Al superarse el umbral: pausa el juego, publica `OnDayThresholdReached` → transición a tienda
   - `FailCount >= MaxFails` → publica `OnGameOver`

2. Crear `Assets/Scripts/Core/FameManager.cs`:
   - Mantiene `CurrentFame` (se resetea al inicio de cada día)
   - `DayFameThreshold(int dayNumber)` — calcula el umbral del día:
     ```csharp
     // Ejemplo de curva (ajustar en balance):
     // Día 1: 50 Fama  |  Día 3: 120 Fama  |  Día 5: 200 Fama
     int DayFameThreshold(int day) => 30 + (day * 20);  // placeholder
     ```
   - Escucha `OnMissionCompleted` → suma `FameEarned` al contador
   - Publica `OnFameEarned { Amount, TotalFame, DayThreshold }`
   - El HUD escucha `OnFameEarned` para actualizar la barra de Fama

3. `DayConfig` — **un único ScriptableObject** con `AnimationCurve`s editables en el Inspector:
   ```csharp
   [CreateAssetMenu(menuName = "Questline/Day Config")]
   public class DayConfig : ScriptableObject {
       // X = DayNumber, Y = valor
       public AnimationCurve FameThresholdCurve;   // Día 1→50, Día 5→200, Día 10→500
       public AnimationCurve MinRankCurve;          // Día 1→1,  Día 5→4,   Día 10→8
       public AnimationCurve MaxRankCurve;          // Día 1→4,  Día 5→9,   Día 10→12
       public AnimationCurve SpawnIntervalCurve;    // Día 1→15s,Día 5→10s, Día 10→6s

       public int   GetFameThreshold(int day)   => Mathf.RoundToInt(FameThresholdCurve.Evaluate(day));
       public int   GetMinRank(int day)          => Mathf.RoundToInt(MinRankCurve.Evaluate(day));
       public int   GetMaxRank(int day)          => Mathf.RoundToInt(MaxRankCurve.Evaluate(day));
       public float GetSpawnInterval(int day)    => SpawnIntervalCurve.Evaluate(day);
   }
   ```
   Las curvas se editan visualmente en el Inspector de Unity. Ideal para iterar balance sin tocar código.
   > ⚠️ **BALANCE PENDIENTE:** Revisar curvas en TAREA 5.3 (playtest).

4. Persistencia entre días:
   - `DayNumber` persiste en GameManager (incrementa al completar día)
   - `DayLives` se **resetea a 3** al inicio de cada día
   - `CurrentCoins` persiste (no se resetea entre días)
   - Buffs permanentes persisten en `BuffManager`
   - Modificadores diarios de `BuffManager._dailyModifiers` se limpian via `ClearDailyModifiers()`
   - `CurrentFame` se resetea a 0 al inicio de cada día
   - Stats de héroes NO persisten (se generan de nuevo cada día con la fórmula escalada)

**Criterio de éxito:** El día termina al superar el umbral de Fama (victoria) o al llegar a 0 vidas (Game Over). El pool refleja el rango correcto del día. Las vidas se resetean correctamente entre días.

**Notas de implementación:**
- `DayConfig.cs` — ScriptableObject en `Assets/Scripts/Data/`. Crear asset en `Assets/Data/DayConfig.asset` desde Unity: Create → Questline → Day Config. Tiene curvas con valores por defecto sensatos.
- `FameManager.cs` — Singleton en `Assets/Scripts/Core/`. Escucha `OnMissionCompleted`, aplica modificador de `DailyEventManager`, publica `OnFameEarned` y `OnDayThresholdReached`. Expone `GetDayThreshold(int)` y `ResetDayFame()`. Tiene referencia `[SerializeField]` a `DayConfig`.
- `RunManager.cs` — Singleton orquestador en `Assets/Scripts/Core/`. No duplica lógica de `GameManager` (vidas, Game Over); solo coordina el startup del día. Tiene referencia `[SerializeField]` a `DayConfig` y la expone en propiedad `DayConfig` pública.
- `MissionDeck.cs` — Actualizado: `BuildDailyPool` usa `RunManager.Instance.DayConfig` o `FameManager.Instance.Config` si disponibles; si no, cae en las fórmulas inline originales.
- **Setup en escena:** Añadir GameObject con `RunManager`, `FameManager`. Asignar `DayConfig.asset` en ambos. Llamar `RunManager.Instance.StartDay()` al inicio de la partida.
- **Nota de diseño:** `DayNumber`, `DayLives` y `OnGameOver` permanecen en `GameManager` para evitar duplicación. `RunManager` es solo el orquestador de inicio de día.

---

### TAREA 2.5 — Descarte activo de misiones
**Estado:** `[x]` ✅ Completada 2026-03-24
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

**Implementación (completada 2026-03-24):**
- `MissionCard.cs` — Nuevos campos `[Header("Descarte")]`: `_discardButton` (Button), `_discardCooldownFill` (Image Filled), `_discardTooltipText` (TMP), `_pulseScale`, `_pulseDuration`. Evento público `OnDiscardRequested`. Método `SetDiscardButtonState(bool, float, string)` que controla interactuabilidad, fill del indicador y tooltip, y dispara el pulse al quedar disponible. Hover del tooltip implementado con `EventTrigger` en el botón.
- `MissionCardArea.cs` — Campo `DiscardCooldownSeconds` (default 10s). Propiedad `CanDiscard`. Método `DiscardMission(MissionCard)`: valida cooldown, estado InTransit y mazo vacío; anima la carta, publica `OnMissionDiscarded`, llama `RemoveFromHand` (que repone la mano), activa el cooldown. `Update()` tick del timer + `UpdateDiscardButtonStates()`. Se suscribe a `card.OnDiscardRequested` en `SpawnCardRoutine`.
- `GameEvents.cs` — `OnMissionDiscarded` ya existía; no requirió cambios.
- **Setup en prefab:** En el prefab de MissionCard añadir un Button hijo en la esquina inferior con una Image Filled hija para el cooldown, y un TextMeshProUGUI para el tooltip. Asignar las referencias en el Inspector.

---

## FASE 3 — ECONOMÍA Y ROGUELIKE
**Objetivo:** Implementar el CoinJar, el Shop de buffs entre runs, y el sistema de progresión roguelike (pistas progresivas, buffs que persisten, escalado de dificultad).
**Duración:** Día 3 (tarde) – Día 4

---

### TAREA 3.1 — CoinJar: sistema de monedas
**Estado:** `[x]`
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

**Archivos creados/modificados:**
- `Assets/Scripts/Economy/CoinJar.cs` ← NUEVO — MonoBehaviour UI + animaciones
- `Assets/Scripts/Heroes/HeroInstance.cs` ← XP, Level, GainXP() añadidos
- `Assets/Scripts/Data/MissionData.cs` ← campo IsBoss añadido
- `Assets/Scripts/Missions/MissionEvaluator.cs` ← bonus de nivel, multiplicador boss, hero.GainXP()

**⚠️ Pendiente en Unity Editor (no automatizable):**
- Crear prefab `CoinJarUI` con GameObject que tenga: `CoinJar.cs`, TMP_Text para el contador, Image para el sprite del frasco.
- Crear prefab `CoinFlyParticle` (sprite de moneda simple) y asignarlo al campo `_coinFlyPrefab`.
- Asignar referencias en el Inspector: `_coinCountText`, `_jarRect`, `_coinSpawnPoint`.
- Arrastrar el prefab `CoinJarUI` a la escena principal (Canvas del HUD).

---

### TAREA 3.2 — ShopManager: tienda inter-run
**Estado:** `[x]`
**Responsable:** Dev C
**Duración estimada:** 2 horas
**Dependencias:** 1.3, 3.1, 2.4

**Descripción:**
Implementar la pantalla de tienda que aparece entre runs. El jugador puede gastar las monedas acumuladas para comprar buffs permanentes (de esa partida).

**Pasos:**
1. Crear `Assets/Scripts/Economy/ShopManager.cs`:
   - Dos pools separados: `List<BuffData> _buffPool` y `List<ConsumableData> _consumablePool`
   - Cada vez que se abre el shop: seleccionar **3 ítems aleatorios del pool combinado** para mostrar. ⚠️ **BALANCE PENDIENTE:** Puede subir a 5 en TAREA 5.3.
   - `PurchaseBuff(BuffData buff)`:
     - Verificar coins → `CoinJar.SpendCoins(buff.Cost)` → `BuffManager.ApplyBuff(buff)` → `OnBuffPurchased`
   - `PurchaseConsumable(ConsumableData consumable)`:
     - Verificar coins → `CoinJar.SpendCoins(consumable.Cost)` → `ConsumableManager.AddConsumable(consumable)` → `OnBuffPurchased`
     - El consumible va al inventario del HUD; se usa durante el gameplay, no en la tienda
   - `SkipShop()` — el jugador puede saltar la tienda sin comprar nada

   **Cadena completa de consumibles (definida aquí para que todos los devs la conozcan):**
   ```
   [Tienda] PurchaseConsumable()
        → ConsumableManager.AddConsumable()  → inventario HUD actualizado
        → [Jugador hace click en consumible del HUD durante gameplay]
        → ConsumableManager.UseConsumable(type, targetMission?)
             StatRevealHand: foreach card in MissionCardArea.Hand
                                → card.MissionInstance.RevealHint(0)  // pista del PrimaryStat
                                → card.RefreshUI()
             FullStatReveal: targetMission.RevealHint(0) + RevealHint(1)
                                → MissionCard.RefreshUI()
        → EventBus.Publish(OnConsumableUsed)
        → ConsumableManager descuenta 1 del inventario
   ```
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

**Implementación (completada 2026-03-24):**
- `Assets/Scripts/Economy/ShopOffer.cs` ← NUEVO — wrapper que envuelve un `BuffData` o `ConsumableData`; expone `Name`, `Description`, `Cost`, `Icon` e `IsBuff`.
- `Assets/Scripts/Economy/ShopManager.cs` ← NUEVO — Singleton que escucha `OnDayEnd` para abrir la tienda. `OpenShop()` mezcla pools y selecciona 3 ítems (⚠️ `_shopSlots` ajustable en Inspector). `PurchaseBuff()` y `PurchaseConsumable()` delegan en `CoinJar`, `BuffManager` y `ConsumableManager`. El buff `MissionReshuffle` se acumula como `ReshuffleCharges` en lugar de enviarse a `BuffManager`. `UseReshuffle()` llama `MissionDeck.Reshuffle()` → `MissionCardArea.ClearAllCards()` → `MissionDeck.ClearHand()` → `MissionDeck.FillHand()`. `CloseShop()`/`SkipShop()` cierra el panel y llama `RunManager.StartDay()`.
- `Assets/Scripts/UI/ShopSlotUI.cs` ← NUEVO — slot individual con `Setup(ShopOffer)`, `RefreshAffordable(int)`, `MarkPurchased()` (overlay gris + animación de brillo) y evento `OnBuyClicked`.
- `Assets/Scripts/UI/ShopPanel.cs` ← NUEVO — panel que escucha `ShopManager.OnShopOpened/OnShopClosed` para mostrarse/ocultarse. Popula los slots, muestra contador de coins, botón "Continuar". Se suscribe a `OnCoinEarned`/`OnCoinSpent` para refrescar asequibilidad dinámicamente. El shake por coins insuficientes lo gestiona `CoinJar` automáticamente.
- `Assets/Scripts/Economy/ConsumableType.cs` ← limpiado stub, tipos ya definitivos con documentación XML.
- `Assets/Scripts/Missions/MissionCardArea.cs` ← añadido `Instance` estático + `ClearAllCards()` (necesario para UseReshuffle).
- `Assets/Scripts/Missions/MissionDeck.cs` ← añadido `ClearHand()` (necesario para UseReshuffle sin disparar FillHand por cada carta).

**⚠️ Pendiente en Unity Editor (no automatizable):**
- Crear prefab `ShopPanel` en Canvas: título TMP, 3 GameObjects hijos con `ShopSlotUI` (cada uno con Image icono, TMP nombre, TMP descripción, TMP costo, Button comprar, Image overlay "vendido", Image brillo), Button "Continuar", TMP contador de coins.
- Asignar en `ShopPanel`: referencias a los 3 `ShopSlotUI`, al `Button` continuar, al TMP coins y al `ShopManager` de la escena.
- Asignar en `ShopManager` Inspector: arrastrar los `BuffData` assets a `_availableBuffs[]` y los `ConsumableData` a `_availableConsumables[]`.
- Añadir `ShopPanel` al Canvas de la escena (desactivado por defecto).
- Añadir `ShopManager` como GameObject persistente en la escena.
- En cada `ShopSlotUI`, conectar el `Button.onClick` al método `HandleBuyButton()`.

---

### TAREA 3.3 — FameManager: fama, umbrales y fin de día
**Estado:** `[x]` — Completada 2026-03-24
**Responsable:** Dev B
**Duración estimada:** 1.5 horas
**Dependencias:** 0.4, 2.4

**Descripción:**
Gestionar la Fama acumulada en la partida y los umbrales de victoria por día. FameManager es el árbitro del fin de día exitoso: cuando la Fama supera el umbral del día en curso, publica `OnDayThresholdReached` y el RunManager cierra el día y abre la tienda.

**Pasos:**
1. Crear `Assets/Scripts/Economy/FameManager.cs`:
   ```csharp
   public class FameManager : MonoBehaviour {
       public int TotalFame { get; private set; }
       public int DayFame { get; private set; }   // Fama acumulada solo en el día actual

       // Umbral de Fama para superar el día actual
       // fórmula: dayThreshold(day) = DayConfig.FameThresholdCurve.Evaluate(day)
       // (AnimationCurve en DayConfig — ajustable en Inspector sin recompilar)
       public int GetDayThreshold(int dayNumber) { ... }

       void OnEnable()  => EventBus.Subscribe<OnMissionCompleted>(OnMissionCompleted);
       void OnDisable() => EventBus.Unsubscribe<OnMissionCompleted>(OnMissionCompleted);

       void OnMissionCompleted(OnMissionCompleted e) {
           TotalFame += e.FameEarned;
           DayFame   += e.FameEarned;
           EventBus.Publish(new OnFameEarned { Amount = e.FameEarned, TotalFame = TotalFame,
                                               DayThreshold = GetDayThreshold(GameManager.Instance.DayNumber) });
           if (DayFame >= GetDayThreshold(GameManager.Instance.DayNumber))
               EventBus.Publish(new OnDayThresholdReached { DayNumber = GameManager.Instance.DayNumber });
       }

       public void ResetDayFame() => DayFame = 0;   // llamado por RunManager en StartDay()
   }
   ```
2. Agregar `FameThresholdCurve` a `DayConfig` (ScriptableObject de TAREA 2.4):
   - Curva sugerida de inicio: día 1 → 50 Fama, día 2 → 90, día 3 → 140, etc.
   - Ajustar en TAREA 5.3 (balance)
3. `RunManager.StartDay()` debe llamar `FameManager.ResetDayFame()` al inicio de cada día.
4. Verificar en el HUD (TAREA 4.1) que la barra de Fama usa `FameManager.DayFame` y `GetDayThreshold()`.

**Criterio de éxito:** Completar misiones acumula Fama. Al superar el umbral del día se publica `OnDayThresholdReached`. El umbral es distinto para cada día y editable en el Inspector sin recompilar.

---

### TAREA 3.4 — Sistema de Eventos Diarios
**Estado:** `[x]`
**Responsable:** Dev A
**Duración estimada:** 1.5 horas
**Dependencias:** 0.4, 1.3, 2.4

**Descripción:**
Implementar el sistema de eventos aleatorios que se activan al inicio de cada día. Cada evento tiene un efecto (buff o debuf) que dura todo el día. Los eventos añaden variabilidad al loop, forzando al jugador a adaptarse y añadiendo narrativa ligera al paso del tiempo.

**Pasos:**
1. Crear `Assets/Scripts/Data/DailyEventData.cs`:
   ```csharp
   [CreateAssetMenu(menuName = "Questline/Daily Event")]
   public class DailyEventData : ScriptableObject {
       public string EventTitle;          // ej: "Lluvia de meteoros"
       [TextArea] public string EventDescription;  // ej: "Los héroes de la zona están asustados. -2 Fuerza hoy."
       public Sprite EventIcon;
       public DailyEventType Type;        // StatBuff, StatDebuff, SpawnSpeedBuff, etc.
       public HeroStat AffectedStat;      // Para tipos de stat
       public int StatModifier;           // Valor positivo o negativo (+3, -2, etc.)
       public float SpawnSpeedModifier;   // Para tipos de spawn (0.8 = 20% más rápido)
       public float FameModifier;         // Multiplicador de Fama (1.2 = +20%, 0.8 = -20%)
   }
   ```
2. Crear `Assets/Scripts/Core/DailyEventManager.cs`:
   - Pool de todos los `DailyEventData` disponibles (asignados desde Inspector)
   - `PickDailyEvent(int dayNumber)` — elige un evento aleatorio del pool (sin repetir el del día anterior)
   - `ApplyEvent(DailyEventData event)` — aplica el modificador al sistema correspondiente:
     - `StatBuff/Debuff` → llama `BuffManager.ApplyTemporaryStatModifier(stat, amount)`
     - `SpawnSpeedBuff/Debuff` → ajusta `HeroSpawner.SpawnIntervalMultiplier`
     - `FameBoost/Debuff` → registra un multiplicador en `FameManager`
   - `ClearEvent()` — al inicio del siguiente día, remueve el modificador anterior
   - Escucha `OnRunStart` para activar el evento del nuevo día
   - Publica `OnDailyEventActivated { Event }`
3. Crear al menos 12 `DailyEventData` ScriptableObjects en `Assets/Data/Events/`:
   - **Buffs:**
     - `Event_HeroFeast.asset` — "Los héroes festejaron anoche. +3 Fuerza hoy."
     - `Event_GuildRumors.asset` — "Corren rumores de riqueza. +20% Fama hoy."
     - `Event_TrainingDay.asset` — "Un veterano visitó la guild. +3 Inteligencia hoy."
     - `Event_FairWeather.asset` — "Clima ideal. Los héroes llegan 20% más rápido."
     - `Event_MerchantBonus.asset` — "Un mercader pasó. +3 Carisma hoy."
     - `Event_SwiftHeroes.asset` — "Los héroes están ansiosos. +3 Destreza hoy."
   - **Debuffs:**
     - `Event_HeavyRain.asset` — "Lluvia torrencial. Los héroes llegan 30% más lento."
     - `Event_BadOmen.asset` — "Un presagio oscuro. -20% Fama hoy."
     - `Event_PlagueScare.asset` — "Miedo a la peste. -2 a todos los stats hoy." (requiere iterar stats)
     - `Event_RivalGuild.asset` — "La guild rival robó héroes. Solo 3 slots disponibles hoy."
     - `Event_Hangover.asset` — "Muchos héroes llegaron con resaca. -3 Fuerza hoy."
     - `Event_Gossip.asset` — "Chismes en la guild. -3 Carisma hoy."
4. UI del evento del día:
   - Al inicio del día: panel breve (2 segundos o click para cerrar) mostrando el evento activo
   - Icono del evento visible en el HUD durante todo el día (esquina del top bar)
   - Tooltip al hover con descripción completa del efecto

**Criterio de éxito:** Cada día activa un evento aleatorio. El efecto se aplica correctamente y se elimina al iniciar el siguiente día. El jugador puede ver el evento activo en todo momento desde el HUD.

**Implementación (completada 2026-03-24):**
- `Assets/Scripts/Data/DailyEventData.cs` — ScriptableObject completo con campos: `EventTitle`, `EventDescription`, `EventIcon`, `Type` (DailyEventType), `AffectedStat`, `StatModifier`, `AffectsAllStats` (bool para PlagueScare y similares), `SpawnSpeedModifier`, `FameModifier`, `SlotReductionAmount`.
- `Assets/Scripts/Core/DailyEventManager.cs` — implementación completa. Singleton. Escucha `OnDayStart`; ejecuta `ClearEvent()` → `PickDailyEvent(day)` (sin repetir el del día anterior) → `ApplyEvent(ev)` → publica `OnDailyEventActivated`. Soporta los 7 tipos de `DailyEventType`. Expone `GetFameModifier()` (usado por FameManager).
- `Assets/Scripts/Heroes/HeroQueue.cs` — añadidos `TotalSlots` (int, read-only) y `SlotOverride` (int, -1 = sin límite). `GetFreeSlot()` respeta el override para `SlotReduction`.
- `Assets/Scripts/UI/DailyEventUI.cs` — componente UI listo para conectar en Tarea 4.1. Escucha `OnDailyEventActivated` y `OnDayEnd`. Muestra panel de anuncio (2s o click) y actualiza icono del HUD. Los GameObjects/referencias se asignan en Inspector en Tarea 4.1.
- `Assets/Data/Events/` — 12 DailyEventData assets:
  - Buffs: `Event_HeroFeast` (+3 STR), `Event_GuildRumors` (+20% Fama), `Event_TrainingDay` (+3 INT), `Event_FairWeather` (spawn 20% más rápido), `Event_MerchantBonus` (+3 CHA), `Event_SwiftHeroes` (+3 DEX).
  - Debuffs: `Event_HeavyRain` (spawn 30% más lento), `Event_BadOmen` (-20% Fama), `Event_PlagueScare` (-2 a todos los stats), `Event_RivalGuild` (solo 3 slots), `Event_Hangover` (-3 STR), `Event_Gossip` (-3 CHA).
- **Setup requerido:** Asignar `DailyEventManager` como componente en la escena. Arrastrar los 12 assets al array `_eventPool` en el Inspector. En Tarea 4.1, cablear `DailyEventUI` con los GameObjects del HUD.
- **Nota Tarea 4.1:** `DailyEventUI` usa `UnityEngine.UI.Text`. Si el proyecto migra a TextMeshPro, cambiar los campos `_titleText`/`_descriptionText` a `TMP_Text`.

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
   ┌──────────────────────────────────────────────────────────────┐
   │ [DÍA X]  [EVENTO🎲]  [❤️❤️❤️ vidas]                         │ ← Top bar
   │ [FAMA: ████████░░░░  320/400 ⭐]                             │ ← Barra de Fama (sub-top)
   ├──────────────────────────────────────────────────────────────┤
   │                                                              │
   │  [HÉROE 1] [HÉROE 2] [HÉROE 3] [HÉROE 4]                    │ ← Fila de héroes
   │                                                              │
   │  [MISIÓN] [MISIÓN] [MISIÓN] [MISIÓN] [MISIÓN]               │ ← Mano de cartas
   │                                                              │
   ├──────────────────────────────────────────────────────────────┤
   │  [🪙 15 coins]  [MAZO: 18]  [RESHUFFLE]  [INVENTARIO]       │ ← Bottom bar
   └──────────────────────────────────────────────────────────────┘
   ```
2. Implementar cada elemento:
   - **Top bar**: `DayNumberUI`, `DailyEventIconUI`, `LivesUI`
   - **Vidas**: 3 iconos de corazón (o calavera invertida). Vida perdida = icono se rompe/oscurece con shake. El jugador siempre sabe cuánto margen le queda.
   - **Barra de Fama**: Barra de progreso horizontal. Muestra `CurrentFame / DayThreshold`. Al llenarse: flash dorado + transición a tienda.
   - **Icono de Evento del Día**: Icono clickeable. Al hover: descripción del evento activo (verde = buff, rojo = debuf).
   - **Mano de cartas**: Posición fija en la parte inferior, fan o hilera horizontal.
   - **CoinJar**: Esquina inferior izquierda, con animación.
   - **Mazo restante**: Número de cartas que quedan en el pool del día.
   - **Botón Reshuffle**: Solo activo si el jugador tiene el buff de reshuffle.
   - **Inventario de consumibles**: Barra de iconos en el bottom bar. Cada icono muestra el consumible y su cantidad. Click = usa el consumible. `StatRevealHand` aplica inmediatamente. `FullStatReveal` pone al jugador en modo "elige una carta" (cursor especial + highlight de cartas en mano).
3. Escuchar eventos para actualizar cada elemento:
   - `OnLifeLost` → shake + oscurecer icono de vida. Si `LivesRemaining == 1`: flash rojo pulsante en el HUD.
   - `OnCoinEarned` / `OnCoinSpent` → actualizar coins
   - `OnFameEarned` → actualizar barra de Fama con animación de llenado
   - `OnDayThresholdReached` → animación de barra llena + flash dorado
   - `OnDayStart` → resetear barra de Fama, número de día, vidas a 3
   - `OnDailyEventActivated` → actualizar icono de evento

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
   - **Éxito**: Fondo verde/dorado, texto "¡MISIÓN COMPLETADA!", monedas que saltan, `+X Fama ⭐`
   - **Fallo**: Fondo rojo oscuro, texto "HÉROE CAÍDO", skull animation, `-1 vida ❤️`
   - **Fallo con héroe sobreviviente** (falló pero no murió — FailProtection activa): el héroe regresa al slot moreteado/golpeado antes de irse
   - **Protección activada**: Fondo naranja, texto "¡BUFF ACTIVADO! El héroe sobrevivió..."
2. Mostrar detalles de la evaluación (para que el jugador aprenda):
   - Stat usado: "Tu héroe usó [Fuerza: 7] vs [Requerimiento: 8]"
   - Probabilidad final: barra de progreso mostrando el % de chance
   - Resultado: ícono de dado o ruleta
3. Duración: 2.5 segundos automático, o click para acelerar
4. **Bardo de la guild:** En el fondo del escenario de la guild, hay un bardo que reacciona a cada resolución de misión:
   - **Éxito**: el bardo toca una melodía festiva/alegre (animación de tocar instrumento con energía)
   - **Fallo**: el bardo toca una melodía de duelo/derrota (animación cabizbaja, notas tristes)
   - El bardo es un elemento de fondo pasivo — no bloquea la UI pero añade vida al escenario
   - La animación del bardo puede sincronizarse con los eventos `OnMissionCompleted` / `OnMissionFailed` del EventBus
5. En caso de fallo + muerte del héroe:
   - Animación dramática: héroe desaparece con efecto de partículas
   - Icono de vida en el HUD hace shake + se rompe/oscurece
   - Si `LivesRemaining == 0`: transición a Game Over

**Criterio de éxito:** El jugador siempre sabe qué pasó y por qué. La pantalla de resultado aparece y desaparece sin bloquear el juego más de lo necesario. El bardo reacciona visiblemente a cada resultado.

---

### TAREA 4.3 — Pantalla de Main Menu e Introducción
**Estado:** `[ ]`
**Responsable:** Dev D
**Duración estimada:** 1.5 horas
**Dependencias:** 0.3, 4.4

**Descripción:**
Implementar una pantalla de inicio funcional y la secuencia de introducción del juego. Sin tutorial de texto. El jugador aprende fallando. La narrativa del loop se establece desde el inicio.

**Pasos:**
1. **Secuencia de introducción (antes del Main Menu, al iniciar el juego por primera vez):**
   - Mostrar el slide de Game Over: imagen cinemática de un personaje siendo echado a patadas de la guild (sombreado/silhouette). Misma pantalla que verá el jugador si pierde.
   - La pantalla se desvanece y aparece el Main Menu normalmente.
   - Propósito: establecer el loop narrativo desde el arranque. Si pierdes, te expulsan y otro llega a reemplazarte. El jugador que juega ahora es el reemplazo del anterior.
   - Esta secuencia solo se muestra una vez (la primera vez que se lanza el juego). En runs subsecuentes, el Main Menu aparece directamente.

2. **Diálogo de inicio (al dar "Jugar", antes del gameplay):**
   - Breve cutscene/texto: el contratante de la guild se dirige al jugador:
     > *"Nuestros héroes necesitan sus misiones ASAP. ¡Muévete!"*
   - Sin más explicaciones. El juego comienza inmediatamente.
   - No hay tutorial de arrastrar ni indicadores. El jugador descubre las mecánicas explorando.

3. **Layout del Main Menu:**
   - Título del juego: "QUESTLINE" (fuente medieval/fantasy)
   - Subtítulo: "Gestiona tu guild. Infiere lo que no se dice."
   - Botón "JUGAR" → secuencia de inicio → carga `Gameplay.unity`
   - Botón "AJUSTES" → panel de ajustes (ver paso 4)
   - Botón "SALIR" → `Application.Quit()`

4. **Panel de Ajustes:**
   - Slider de volumen general (SFX + Música)
   - Slider de volumen de música independiente
   - Selector de resolución: 1920×1080 / 1280×720 / 1024×576
   - Botón "Aplicar" y "Cancelar"
   - Persistir ajustes con `PlayerPrefs`

5. Animación de fondo: parallax simple con elementos de una taberna/mesón

**Criterio de éxito:** El menú funciona. La secuencia de introducción establece la narrativa sin texto de tutorial. El jugador puede cambiar resolución y volumen. Al jugar, el contratante da la única instrucción antes de que empiece el caos.

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
   - Stats de la partida: días sobrevividos, misiones completadas, héroes perdidos (murieron o se fueron enojados), **Fama total acumulada** (reemplaza Score)
   - Botón "VOLVER AL MENÚ" → MainMenu
   - Botón "INTENTAR DE NUEVO" → reinicia desde Día 1, resetea todo (coins, buffs, fama)
2. Resumen de día (inter-día, antes del shop):
   - Tabla del día: misiones exitosas, fallidas, coins ganadas, Fama ganada
   - Mejor héroe del día (el que más Fama generó)
   - Streak de éxitos consecutivos si aplica
3. Pantalla de victoria (si existe condición de victoria):
   - Por ahora: si el jugador llega al Día 10 → pantalla de victoria
   - "¡Tu guild es legendaria!" + Fama total acumulada en toda la partida

**Criterio de éxito:** Game Over muestra stats correctos (Fama, no Score). El jugador puede reiniciar sin errores. Coins, buffs y consumibles se resetean correctamente al reiniciar desde el principio.

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
   - **Iconos de rasgo del héroe** (ej: hover sobre ícono de Greedy → "Codicioso: solo acepta misiones con propina ≥ 6 monedas")
   - Iconos de pista en misiones (ej: hover → "Esta pista se desbloqueó en el Día 2")
   - Contador de fallos (ej: hover → "Fallos restantes: 2. Al llegar a 0, game over.")
   - Buffs en el shop (ej: hover → descripción completa del buff)
3. Floating text (números que flotan y desaparecen):
   - Al ganar coins: "+5" dorado que sube y se desvanece desde el CoinJar
   - Al fallar: "FALLO" rojo que aparece sobre el héroe
   - Al rechazar misión por rasgo: texto de sabor sobre el héroe (ej: "¡Muy fácil para mí!" para Demanding, "¿Solo eso me pagas?" para Greedy)

**Criterio de éxito:** Todos los elementos importantes tienen tooltip. Los floating texts aparecen en el momento correcto.

---

### TAREA 4.6 — UI de la tienda inter-día
**Estado:** `[ ]`
**Responsable:** Dev D
**Duración estimada:** 2 horas
**Dependencias:** 3.2, 4.1

**Descripción:**
Construir el panel visual de la tienda que se muestra entre días. La lógica de compra ya está en `ShopManager` (TAREA 3.2); esta tarea conecta esa lógica con la UI.

**Pasos:**
1. En `ShopPanel` (Canvas ya creado en 0.3), construir el layout:
   - Título: "Mejoras de Guild" (o similar)
   - 3 slots de carta de ítem: icono + nombre + descripción + precio en coins
   - Botón "Comprar" por carta (se desactiva si coins insuficientes o ya comprado)
   - Botón "Continuar" alineado abajo para saltar la tienda sin comprar
   - Contador de coins actual visible en la esquina superior
2. Crear `Assets/Scripts/UI/ShopUI.cs`:
   - Al abrirse, llama `ShopManager.GetShopOffers()` y renderiza los 3 ítems
   - `OnBuyClicked(int slotIndex)` → llama `ShopManager.PurchaseBuff/PurchaseConsumable` → actualiza estado visual del slot (gris + "Comprado")
   - Si coins insuficientes al intentar comprar: shake animation en el contador de coins
   - Al cerrar (`OnContinueClicked`): `ShopManager.CloseShop()` → `RunManager.StartNextDay()`
3. Feedback visual:
   - Al comprar: brillo/glow breve en la carta comprada + sonido de compra (AudioManager)
   - Carta comprada queda desactivada visualmente (no se puede recomprar)
4. La tienda solo es accesible desde `RunManager` al recibir `OnDayThresholdReached` — no se puede abrir manualmente.

**Criterio de éxito:** El panel muestra 3 ítems con precio correcto. El botón Comprar funciona y descuenta coins. El botón Continuar avanza al siguiente día. El panel no es accesible durante gameplay activo.

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
   - **Evento diario — buff**: fanfare corto positivo (trompeta alegre)
   - **Evento diario — debuf**: acorde oscuro descendente
   - **Barra de Fama llena**: fanfare de victoria del día (diferente al de misión completada)
3. Música de fondo:
   - Música de taberna/mesón tranquila para el gameplay
   - Música más intensa cuando hay pocos fallos restantes o la barra de Fama está cerca del umbral
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
   - **Barra de Fama llena**: explosión de estrellas doradas desde la barra + flash pantalla dorado suave
   - **Evento diario buff**: partículas verdes/brillantes en el icono del evento
   - **Evento diario debuf**: partículas oscuras/púrpuras en el icono del evento
2. Animaciones de UI (con Animator o DOTween):
   - Carta de misión: float suave (sube y baja 2-3 pixels en loop)
   - HeroSlot al recibir un drop: pequeño bounce
   - FailCounter al incrementar: shake + flash rojo
   - CoinCounter al incrementar: bounce + flash dorado
   - **Barra de Fama**: fill animado al ganar Fama, pulso cuando está al 80% del umbral
3. Efectos de cámara simples (Camera Shake):
   - Al fallar una misión: pequeño shake de cámara (0.3 segundos)
4. Screen flash:
   - Al completarse el umbral de Fama: flash dorado suave
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
