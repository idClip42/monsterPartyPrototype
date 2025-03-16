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
        if(_cameraControl == null) throw new System.Exception("Missing camera control");

        _characters = FindObjectsByType<Character>(FindObjectsSortMode.None);
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
        if(_cameraControl == null) throw new System.Exception("Null _cameraControl");
        if(index < 0) throw new System.Exception($"Invalid character index {index}");

        if(index >= _characters.Length) return;
        if(_selectedCharacter == _characters[index]) return;

        foreach(var c in _characters) c.State = State.AI;
        _selectedCharacter = null;

        if(immediate){
            _characters[index].State = State.Player;
            _selectedCharacter = _characters[index];
        }
        else {
            _cameraControl.SendCameraToNewCharacter(
                _characters[index],
                ()=>{
                    _characters[index].State = State.Player;
                    _selectedCharacter = _characters[index];
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
}
