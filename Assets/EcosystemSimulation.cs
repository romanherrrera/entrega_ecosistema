using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class EcosystemSimulation : MonoBehaviour
{
    [Header("Condiciones Iniciales")]
    public int initialRabbits = 20; // Empezamos con 20 conejos
    public int initialFoxes = 3;
    [Tooltip("Duración de un día en segundos reales")]
    public float secondsPerDay = 1.0f; 

    [Header("Estado Actual (Variables de Ejecución)")]
    [SerializeField] private int currentRabbits;
    [SerializeField] private int currentFoxes;
    [SerializeField] private int day;

    [Header("Referencias Visuales (Prefabs y UI)")]
    public TextMeshProUGUI uiText; 
    [Tooltip("Asigna un Prefab con Rigidbody (ej. Esfera blanca)")]
    public GameObject rabbitPrefab; 
    [Tooltip("Asigna un Prefab con Rigidbody (ej. Cubo naranja)")]
    public GameObject foxPrefab;    

    // Listas para llevar el control de los objetos físicos en la escena
    private List<GameObject> activeRabbits = new List<GameObject>();
    private List<GameObject> activeFoxes = new List<GameObject>();

    private float timer;
    private bool isSimulationRunning = true; // Interruptor de la simulación

    // 1. Definir condiciones iniciales y dibujar el ecosistema.
    void Start()
    {
        currentRabbits = initialRabbits;
        currentFoxes = initialFoxes;
        day = 0;
        timer = 0f;
        isSimulationRunning = true;

        // Instanciar los animales físicos iniciales
        SpawnAnimals(activeRabbits, rabbitPrefab, currentRabbits);
        SpawnAnimals(activeFoxes, foxPrefab, currentFoxes);

        DrawView(); // Mostrar el día 0
    }

    // 2. Controlar el paso del tiempo y llamar a la simulación.
    void Update()
    {
        // Si la simulación ya terminó, "return" ignora el resto del código y el tiempo se congela
        if (!isSimulationRunning) 
        {
            return; 
        }

        timer += Time.deltaTime; 
        
        if (timer >= secondsPerDay)
        {
            timer = 0f; 
            day++;      
            
            Simulate(); 
            DrawView(); 
        }
    }

    // 3. Aplicar las reglas de evolución del sistema.
    void Simulate()
    {
        // Regla A: Los conejos aumentan 5 por día
        currentRabbits += 5; 
        
        // Regla B: Los zorros cazan 2 conejos por día
        int huntedRabbits = currentFoxes * 2; 
        currentRabbits -= huntedRabbits;

        // Regla C: Limites y muerte por hambre
        if (currentRabbits <= 0) 
        {
            currentRabbits = 0; 
            currentFoxes = 0;   
            
            // Apagamos el interruptor para detener el programa
            isSimulationRunning = false; 
            Debug.LogWarning("¡Ecosistema colapsado! Fin de la simulación.");
        }
    }

    // 4. Representar visualmente el estado actual.
    void DrawView()
    {
        // --- SALIDA EN CONSOLA ---
        if (isSimulationRunning)
        {
            Debug.Log($"[Reporte Diario] DÍA {day} | Conejos vivos: {currentRabbits} | Zorros vivos: {currentFoxes}");
        }

        // --- SALIDA EN PANTALLA (UI) ---
        if (uiText != null)
        {
            if (isSimulationRunning)
            {
                uiText.text = $"DÍA {day}\n\nConejos: {currentRabbits}\nZorros: {currentFoxes}";
            }
            else
            {
                // Mensaje de Fin de Juego
                uiText.text = $"DÍA {day}\n\n<color=red>¡ECOSISTEMA COLAPSADO!</color>\nFIN DE LA SIMULACIÓN\n\nConejos: 0\nZorros: 0";
            }
        }

        // --- Sincronizar matemáticas con el mundo 3D (crear o destruir) ---
        AdjustPhysicalInstances(activeRabbits, rabbitPrefab, currentRabbits);
        AdjustPhysicalInstances(activeFoxes, foxPrefab, currentFoxes);

        // --- USO DE FOREACH Y RIGIDBODY ---
        // Recorremos cada conejo y zorro vivo para darles un pequeño salto de "vida" al pasar el día
        foreach (GameObject rabbit in activeRabbits)
        {
            Rigidbody rb = rabbit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(Vector3.up * Random.Range(2f, 4f), ForceMode.Impulse);
            }
        }

        foreach (GameObject fox in activeFoxes)
        {
            Rigidbody rb = fox.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(Vector3.up * Random.Range(2f, 4f), ForceMode.Impulse);
            }
        }
    }

    // --- Funciones auxiliares para mantener el código limpio ---

    private void SpawnAnimals(List<GameObject> list, GameObject prefab, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 randomPos = new Vector3(Random.Range(-4f, 4f), 1f, Random.Range(-4f, 4f));
            GameObject newAnimal = Instantiate(prefab, randomPos, Quaternion.identity);
            list.Add(newAnimal);
        }
    }

    private void AdjustPhysicalInstances(List<GameObject> list, GameObject prefab, int targetAmount)
    {
        // Nacimientos (Crear objetos)
        while (list.Count < targetAmount)
        {
            Vector3 randomPos = new Vector3(Random.Range(-4f, 4f), 1f, Random.Range(-4f, 4f));
            GameObject newAnimal = Instantiate(prefab, randomPos, Quaternion.identity);
            list.Add(newAnimal);
        }

        // Muertes (Destruir objetos)
        while (list.Count > targetAmount && list.Count > 0)
        {
            GameObject animalToDie = list[0]; 
            list.RemoveAt(0);                 
            Destroy(animalToDie);             
        }
    }
}