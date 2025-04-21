using System.Linq;
using UnityEngine;

#nullable enable

[DisallowMultipleComponent]
public class CharacterManager : MonoBehaviour
{
    private CameraControl? _cameraControl = null;
    private Character[] _characters = {};
    private Character? _selectedCharacter = null;

    public Character? SelectedCharacter => _selectedCharacter;

    private void Awake()
    {
        _cameraControl = FindFirstObjectByType<CameraControl>();
        if(_cameraControl == null) throw new MonsterPartyNullReferenceException("_cameraControl");

        _characters = FindObjectsByType<Character>(FindObjectsSortMode.None);
        foreach(Character c in _characters){
            c.OnDeath += OnCharacterDeath;
        }
    }

    private void Start()
    {
        SelectCharacter(0, true);
    }

    private void Update()
    {
        int numberKeyPressed = GetNumberKeyPressed();
        if (numberKeyPressed != -1)
        {
            SelectCharacter(numberKeyPressed - 1, false);
        }
    }

    private void SelectCharacter(int index, bool immediate){
        if(index < 0) throw new MonsterPartyException($"Invalid character index {index}");
        if(index >= _characters.Length) return;
        SelectCharacter(_characters[index], immediate);
    }

    private void SelectCharacter(Character newSelection, bool immediate){
        if(_cameraControl == null) throw new MonsterPartyNullReferenceException("_cameraControl");

        if(_selectedCharacter == newSelection) return;
        if(newSelection.Alive == false) return;

        foreach(var c in _characters) c.SetState(Character.StateType.AI);
        _selectedCharacter = null;

        if(immediate){
            newSelection.SetState(Character.StateType.Player);
            _selectedCharacter = newSelection;
        }
        else {
            _cameraControl.SendCameraToNewCharacter(
                newSelection,
                ()=>{
                    newSelection.SetState(Character.StateType.Player);
                    _selectedCharacter = newSelection;
                }
            );
        }
    }

    private int GetNumberKeyPressed()
    {
        for (int i = 0; i <= 9; i++)
        {
            KeyCode key = KeyCode.Alpha0 + i; // Maps KeyCode.Alpha0 - KeyCode.Alpha9
            if (Input.GetKeyDown(key))
            {
                return i == 0 ? 10 : i; // Treat '0' as '10'
            }
        }
        return -1; // No number key pressed
    }

    private void OnCharacterDeath(Entity deadCharacter){
        if(this._selectedCharacter == deadCharacter){
            bool isAnyoneStillAlive = _characters.Any(c=>c.Alive);
            if(isAnyoneStillAlive == false) return;
            Character autoSwitchChar = _characters.First(c=>c.Alive);
            SelectCharacter( autoSwitchChar, true );
        }
    }
}
