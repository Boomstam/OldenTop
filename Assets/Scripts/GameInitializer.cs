using UnityEngine;

namespace OldenTop
{
    [AddComponentMenu("Olden Top/Game Initializer")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HexMap), typeof(TurnSystem))]
    public sealed class GameInitializer : MonoBehaviour
    {
        [Header("Map Generation")]
        [Tooltip("The map is deterministic. Numeric text preserves the equivalent legacy integer seed; other text is converted with a stable hash.")]
        [SerializeField] private string seed = "1375840081";

        public string Seed => seed;

        private void Start()
        {
            HexMap map = GetComponent<HexMap>();
            TurnSystem turnSystem = GetComponent<TurnSystem>();

            map.Generate(seed);
            turnSystem.Initialize(map);
        }
    }
}
