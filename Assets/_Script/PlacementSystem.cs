using UnityEngine;
using static SoundFeedback;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;

    [SerializeField] private Grid grid;
    private GridData floorData, furnitureData;
    [SerializeField] private SO_Database database;
    [SerializeField] private GameObject gridVisualization;
    private Vector3Int lastDetectedPosition = Vector3Int.zero;

    IBuildingState buildingState;

    [SerializeField] private PreviewSystem preview;
    [SerializeField] private ObjectPlacer objectPlacer;

    [SerializeField] private SoundFeedback soundFeedback;

    private void Start()
    {
        gridVisualization.SetActive(false);
        floorData = new();
        furnitureData = new();
    }

    public void StartPlacement(int ID)
    {
        StopPlacement();
        gridVisualization.SetActive(true);

        buildingState = new PlacementState(ID,
                                           grid,
                                           preview,
                                           database,
                                           floorData,
                                           furnitureData,
                                           objectPlacer,
                                           soundFeedback);
        inputManager.OnClicked += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    public void StartRemoving()
    {
        StopPlacement();
        gridVisualization.SetActive(true);
        buildingState = new RemovingState(grid,
                                          preview,
                                          floorData,
                                          furnitureData,
                                          objectPlacer,
                                          soundFeedback);
        inputManager.OnClicked += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    private void PlaceStructure()
    {
        if (inputManager.IsPointerOverUI())
        {
            return;
        }
        
        Vector3 mousePos = inputManager.GetSelectedMapPos();
        Vector3Int gridPosition = grid.WorldToCell(mousePos);

        buildingState.OnAction(gridPosition);
    }

    private void StopPlacement()
    {
        soundFeedback.PlaySound(SoundType.Click);
        if(buildingState == null)
        {
            return;
        }
        gridVisualization.SetActive(false);

        buildingState.EndState();
        //RESETTING VALUES
        inputManager.OnClicked -= PlaceStructure;
        inputManager.OnExit -= StopPlacement;
        lastDetectedPosition = Vector3Int.zero;
        buildingState = null;
    }

    private void Update()
    {
        if (buildingState == null)
        {
            return;
        }

        Vector3 mousePos = inputManager.GetSelectedMapPos();
        Vector3Int gridPosition = grid.WorldToCell(mousePos);

        if (lastDetectedPosition != gridPosition)
        {
            buildingState.UpdateState(gridPosition);
            lastDetectedPosition = gridPosition;
        } 
    }
}
