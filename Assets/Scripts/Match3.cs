using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Match3 {
    public class Match3 : MonoBehaviour {
        [SerializeField] int width = 8;
        [SerializeField] int height = 8;
        [SerializeField] float cellSize = 1f;
        [SerializeField] Vector3 originPosition = Vector3.zero;
        [SerializeField] bool debug = true;
        
        [SerializeField] Gem gemPrefab;
        [SerializeField] GemType[] gemTypes;
        [SerializeField] Ease ease = Ease.InQuad;
        [SerializeField] private GameObject explosion;
        
        [SerializeField] AudioManager audioManager;

        GridSystem2D<GridObject<Gem>> grid;

        InputReader inputReader;
        Vector2Int selectedGem = Vector2Int.one * -1;
        private Vector2Int specialGemPosition;
        private GridObject<Gem> specialGem = null;
        private bool isSpecialGemCreated = false;
        [SerializeField] private GemType specialGemType; // Asignar en el inspector

        void Awake() {
            inputReader = GetComponent<InputReader>();
            audioManager = GetComponent<AudioManager>();
        }
        
        void Start() {
            inputReader.Fire += OnSelectGem;
        }

        public void StartMatch3()
        {
            GameObject obj = this.gameObject;
            obj.SetActive(true);
            specialGemType = Match3Manager.Instance.currentLevel.gemType;
            RestartGrid();
        }

        public void StartMatch3WithCustomOrder()
        {
            GameObject obj = this.gameObject;
            obj.SetActive(true);
            GemOrderData gemOrderData = Match3Manager.Instance.currentLevel.gemOrder;
            RestartCustomGrid(gemOrderData);
        }
        void OnDestroy() {
            inputReader.Fire -= OnSelectGem;
        }

        void OnSelectGem() {
            var gridPos = grid.GetXY(Camera.main.ScreenToWorldPoint(inputReader.Selected));

            if (!IsValidPosition(gridPos) || IsEmptyPosition(gridPos)) return;

            if (selectedGem == gridPos) {
                DeselectGem();
                audioManager.PlayDeselect();
            } else if (selectedGem == Vector2Int.one * -1) {
                SelectGem(gridPos);
                audioManager.PlayClick();
            } else {
                // Solo permitir intercambio con gemas adyacentes
                if (IsAdjacentPosition(selectedGem, gridPos)) {
                    StartCoroutine(RunGameLoop(selectedGem, gridPos));
                } else {
                    // Si no es adyacente, seleccionar la nueva gema
                    DeselectGem();
                    SelectGem(gridPos);
                    audioManager.PlayClick();
                }
            }
        }

        private bool IsAdjacentPosition(Vector2Int selectedGem, Vector2Int gridPos)
        {
            if(selectedGem.x == gridPos.x && Mathf.Abs(selectedGem.y - gridPos.y) == 1)
                return true;
            if(selectedGem.y == gridPos.y && Mathf.Abs(selectedGem.x - gridPos.x) == 1)
                return true;
            return false;
        }

        IEnumerator RunGameLoop(Vector2Int gridPosA, Vector2Int gridPosB) {
            yield return StartCoroutine(SwapGems(gridPosA, gridPosB));
            
            
            // Matches?
            List<Vector2Int> matches = FindMatches();
            // Make Gems explode
            yield return StartCoroutine(ExplodeGems(matches));
            //Make gems fall
            yield return StartCoroutine(MakeGemsFall());
            // Fill empty spots
            yield return StartCoroutine(FillEmptySpots());

            DeselectGem();
            
            CheckSpecialGemPosition();
            Match3Manager.Instance.OnPlayerMove();
            
        }

        IEnumerator FillEmptySpots()
        {
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    // Quitar esquinas (solo saltar la celda, no romper el bucle)
                    if ((x == 0 && y == 0) ||
                        (x == 0 && y == height - 1) ||
                        (x == width - 1 && y == 0) ||
                        (x == width - 1 && y == height - 1))
                        continue;
                    
                    if (grid.GetValue(x, y) == null)
                    {
                        CreateGem(x, y);
                        //SFX play sound
                        audioManager.PlayPop();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }

        }

        IEnumerator MakeGemsFall()
        {
            for (var x = 0; x < width; x++)
            {
                for (var y  = 0; y < height; y++)
                {
                    if ((y == 0 && x == width - 1) ||
                        (y == 0 && x == 0)) continue;
                    
                    if (grid.GetValue(x, y) == null)
                    {
                        for (var i = y+1; i < height; i++)
                        {
                            if (grid.GetValue(x, i) == null) continue;
                            

                            var gem = grid.GetValue(x, i).GetValue();
                            grid.SetValue(x,y,grid.GetValue(x,i));
                            grid.SetValue(x,i,null);
                            gem.transform
                                .DOLocalMove(grid.GetWorldPositionCenter(x, y), 0.5f)
                                .SetEase(ease);
                            //SFX play sound
                            audioManager.PlayWoosh();
                            yield return new WaitForSeconds(0.1f);
                            break;
                        }
                    }
                }
                
            }
        }

        IEnumerator ExplodeGems(List<Vector2Int> matches)
        {   
          //SFX play sound
          audioManager.PlayPop();
          foreach (var match in matches)
          {
              // No destruir la gema especial
              if (match == specialGemPosition) continue;
              
              var gem = grid.GetValue(match.x, match.y).GetValue();
              grid.SetValue(match.x, match.y, null);

              ExplodeVFX(match);
              
              gem.transform.DOPunchScale(Vector3.one * 0.1f, 0.1f, 1, 0.5f );
              
              yield return new WaitForSeconds(0.1f);

              gem.DestroyGem();
          }
        }

        IEnumerator ExplodeAllGems()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid.GetValue(x, y) == null) continue;

                    var gem = grid.GetValue(x, y).GetValue();
                    grid.SetValue(x, y, null);

                    ExplodeVFX(new Vector2Int(x, y));
                    gem.transform.DOPunchScale(Vector3.one * 0.1f, 0.1f, 1, 0.5f);
                    yield return new WaitForSeconds(0.1f);
                    gem.DestroyGem();
                }

            }
            
            this.gameObject.SetActive(false);
            
        }

        public void GameOver()
        {
            StartCoroutine(FinishLevel());

        }

        IEnumerator FinishLevel()
        {
            yield return StartCoroutine(ExplodeAllGems());

            yield return new WaitForSeconds(0.5f);
            GameFlowManager.Instance.levelSelectorUI.SetActive(true);
        }

        private void ExplodeVFX(Vector2Int match)
        {
            // TODO: Pool
            var fx = Instantiate(explosion, transform);
            fx.transform.position = grid.GetWorldPositionCenter(match.x, match.y);
            Destroy(fx, 5f);
        }


        private List<Vector2Int> FindMatches()
        {
            HashSet<Vector2Int> matches = new();
            //Horizontal matches
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width - 2; x++)
                {
                    var gemA = grid.GetValue(x, y);
                    var gemB = grid.GetValue(x + 1, y);
                    var gemC = grid.GetValue(x + 2, y);

                    if (gemA == null || gemB == null || gemC == null) continue;

                    if (gemA.GetValue().GetType() == gemB.GetValue().GetType() &&
                        gemB.GetValue().GetType() == gemC.GetValue().GetType())
                    {
                        matches.Add(new Vector2Int(x, y));
                        matches.Add(new Vector2Int(x + 1, y));
                        matches.Add(new Vector2Int(x + 2, y));
                    }
                }
            }
            
            //Vertical matches
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height - 2; y++)
                {
                    var gemA = grid.GetValue(x, y);
                    var gemB = grid.GetValue(x, y + 1);
                    var gemC = grid.GetValue(x, y + 2);

                    if (gemA == null || gemB == null || gemC == null) continue;

                    if (gemA.GetValue().GetType() == gemB.GetValue().GetType() &&
                        gemB.GetValue().GetType() == gemC.GetValue().GetType())
                    {
                        matches.Add(new Vector2Int(x, y));
                        matches.Add(new Vector2Int(x, y + 1));
                        matches.Add(new Vector2Int(x, y + 2));
                    }
                }
            }

            if (matches.Count == 0)
            {
                audioManager.PlayNoMatch();
            }
            else
            {
                audioManager.PlayMatch();
            }
            return new List<Vector2Int>(matches);
        }

        IEnumerator SwapGems(Vector2Int gridPosA, Vector2Int gridPosB)
        {
            var gridObjectA = grid.GetValue(gridPosA.x, gridPosA.y);
            var gridObjectB = grid.GetValue(gridPosB.x, gridPosB.y);

            // See README for a link to the DOTween asset
            gridObjectA.GetValue().transform
                .DOLocalMove(grid.GetWorldPositionCenter(gridPosB.x, gridPosB.y), 0.5f)
                .SetEase(ease);
            gridObjectB.GetValue().transform
                .DOLocalMove(grid.GetWorldPositionCenter(gridPosA.x, gridPosA.y), 0.5f)
                .SetEase(ease);

            grid.SetValue(gridPosA.x, gridPosA.y, gridObjectB);
            grid.SetValue(gridPosB.x, gridPosB.y, gridObjectA);

            yield return new WaitForSeconds(0.5f);
        }
        public void RestartCustomGrid(GemOrderData customOrderData)
        {
            if (grid == null)
            {
                InitializeGrid();
                return;
            }

            // Destruir todas las gemas existentes
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var gridObject = grid.GetValue(x, y);
                    if (gridObject != null)
                    {
                        var gem = gridObject.GetValue();
                        if (gem != null)
                        {
                            Destroy(gem.gameObject);
                        }
                    }
                }
            }

            // Resetear variables
            isSpecialGemCreated = false;
            specialGem = null;
            selectedGem = Vector2Int.one * -1;

            // Inicializar nuevo grid
            InitializeGridWithCustomOrder(customOrderData);
        }
        public void RestartGrid()
        {
            if (grid == null)
            {
                InitializeGrid();
                return;
            }

            // Destruir todas las gemas existentes
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var gridObject = grid.GetValue(x, y);
                    if (gridObject != null)
                    {
                        var gem = gridObject.GetValue();
                        if (gem != null)
                        {
                            Destroy(gem.gameObject);
                        }
                    }
                }
            }

            // Resetear variables
            isSpecialGemCreated = false;
            specialGem = null;
            selectedGem = Vector2Int.one * -1;

            // Inicializar nuevo grid
            InitializeGrid();
        }
        void InitializeGrid()
        {
            grid = GridSystem2D<GridObject<Gem>>.VerticalGrid(width, height, cellSize, originPosition, debug);
            int randomX = Random.Range(1, width - 2);
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {

                    // Quitar esquinas (solo saltar la celda, no romper el bucle)
                    if ((x == 0 && y == 0) ||
                        (x == 0 && y == height - 1) ||
                        (x == width - 1 && y == 0) ||
                        (x == width - 1 && y == height - 1))
                        continue;

                    if (x == randomX && y == height - 1)
                    {
                        // Crear la gema especial en una posición aleatoria en la fila superior
                        if (!isSpecialGemCreated)
                        {
                            CreateSpecialGem(randomX, height - 1);
                            continue;
                        }
                    }
                    CreateGem(x, y);
                }
            }
        }
       public void InitializeGridWithCustomOrder(GemOrderData customOrderData)
        {
            List<GemType> customOrder = customOrderData.gemList;
            
           if (customOrder == null || customOrder.Count != width * height - 4)
           {
               Debug.LogError("Custom order list must match the grid size (width * height).");
               return;
           }
       
           grid = GridSystem2D<GridObject<Gem>>.VerticalGrid(width, height, cellSize, originPosition, debug);
       
           int index = -1;
           for (var x = 0; x < width; x++)
           {
               for (var y = 0; y < height; y++)
               {
                    // Skip corners
                    if ((x == 0 && y == 0) ||
                        (x == 0 && y == height - 1) ||
                        (x == width - 1 && y == 0) ||
                        (x == width - 1 && y == height - 1))
                        continue;

                    index++;

                    if (!isSpecialGemCreated && x == Match3Manager.Instance.currentLevel.specialGemPosition.x && y == Match3Manager.Instance.currentLevel.specialGemPosition.y)
                    {
                        CreateSpecialGem(x, y);
                        continue;
                    }
                   // Create gems based on the custom order

                   CreateCustomGem(x, y, customOrder[index]);
               }
           }
       }
        
        

        void CreateSpecialGem(int x, int y) {
            var gem = Instantiate(gemPrefab, grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
            specialGemPosition = new Vector2Int(x, y);
            specialGemType = Match3Manager.Instance.currentLevel.gemType;
            gem.SetType(specialGemType);
            var gridObject = new GridObject<Gem>(grid, x, y);
            gridObject.SetValue(gem);
            specialGem = gridObject;
            grid.SetValue(x, y, gridObject);
            isSpecialGemCreated = true;
        }

        private void CheckSpecialGemPosition()
        {
            specialGemPosition = grid.GetXY(specialGem.GetValue().transform.position);
            
            if (specialGemPosition.y == 0) {
                Match3Manager.Instance.OnObjectiveReachedBottom(ObjectiveType.Special);
                StartCoroutine(ExplodeAllGems());
            }


        }

        void CreateGem(int x, int y)
        {
            var gem = Instantiate(gemPrefab, grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
            gem.SetType(gemTypes[Random.Range(0, gemTypes.Length)]);
            var gridObject = new GridObject<Gem>(grid, x, y);
            gridObject.SetValue(gem);
            grid.SetValue(x, y, gridObject);
        }
        
        void CreateCustomGem(int x, int y, GemType gemType)
        {
            var gem = Instantiate(gemPrefab, grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
            gem.SetType(gemType);
            var gridObject = new GridObject<Gem>(grid, x, y);
            gridObject.SetValue(gem);
            grid.SetValue(x, y, gridObject);
        }

        void DeselectGem() => selectedGem = new Vector2Int(-1, -1);
        void SelectGem(Vector2Int gridPos) => selectedGem = gridPos;

        bool IsEmptyPosition(Vector2Int gridPosition) => grid.GetValue(gridPosition.x, gridPosition.y) == null;

        bool IsValidPosition(Vector2 gridPosition) {
            // Primero verificamos los límites básicos del grid
            if (gridPosition.x < 0 || gridPosition.x >= width || gridPosition.y < 0 || gridPosition.y >= height)
                return false;
                
            // Luego excluimos las esquinas
            if ((gridPosition.x == 0 && gridPosition.y == 0) ||
                (gridPosition.x == 0 && gridPosition.y == height - 1) ||
                (gridPosition.x == width - 1 && gridPosition.y == 0) ||
                (gridPosition.x == width - 1 && gridPosition.y == height - 1))
                return false;
                
            return true;
        }
    }
}