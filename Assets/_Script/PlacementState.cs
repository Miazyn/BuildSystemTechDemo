using UnityEngine;

public class PlacementState : IBuildingState
{
    private int selectedObjectIndex = -1;
    int ID;
    Grid grid;
    PreviewSystem previewSystem;
    SO_Database database;
    GridData floorData;
    GridData furnitureData;
    ObjectPlacer objectPlacer;
    SoundFeedback soundFeedback;

    public PlacementState(int iD,
                          Grid grid,
                          PreviewSystem previewSystem,
                          SO_Database database,
                          GridData floorData,
                          GridData furnitureData,
                          ObjectPlacer objectPlacer,
                          SoundFeedback soundFeedback)
    {
        ID = iD;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.database = database;
        this.floorData = floorData;
        this.furnitureData = furnitureData;
        this.objectPlacer = objectPlacer;
        this.soundFeedback = soundFeedback;

        selectedObjectIndex = database.objectData.FindIndex(data => data.ID == ID);
        if(selectedObjectIndex > -1)
        {
            previewSystem.StartShowingPlacementPreview(
                database.objectData[selectedObjectIndex].Prefab,
                database.objectData[selectedObjectIndex].Size);
        }
        else
        {
            throw new System.Exception($"No Object with ID {iD}");
        }
    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
    }

    public void OnAction(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);

        if (placementValidity == false)
        {
            soundFeedback.PlaySound(SoundFeedback.SoundType.wrongPlacement);
            return;

        }
        soundFeedback.PlaySound(SoundFeedback.SoundType.Place);

        var _dbSelectedObject = database.objectData[selectedObjectIndex];

        int index = objectPlacer.PlaceObject(
            _dbSelectedObject.Prefab,
            grid.CellToWorld(gridPosition)
            );


        GridData selectedData =
           _dbSelectedObject.StructureType == ObjectData.ObjectType.Floor ?
           floorData :
           furnitureData;

        selectedData.AddObjectAt(gridPosition,
            _dbSelectedObject.Size,
            _dbSelectedObject.ID,
            index);

        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), false);
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        var _dbSelectedObject = database.objectData[selectedObjectIndex];

        GridData selectedData = _dbSelectedObject.StructureType == ObjectData.ObjectType.Floor ? 
            floorData : 
            furnitureData;

        return selectedData.CanPlaceObjectAt(gridPosition, _dbSelectedObject.Size);
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);

        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), placementValidity);

    }
}
