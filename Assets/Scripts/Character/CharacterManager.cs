using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    private CharacterBase[] _characters;
    private CharacterBase _selectedCharacter;

    void Awake()
    {
        _characters = FindObjectsByType<CharacterBase>(FindObjectsSortMode.None);
    }

    void Start()
    {
        SelectCharacter(0);
    }

    void Update()
    {
        int numberKeyPressed = GetNumberKeyPressed();
        if (numberKeyPressed != -1)
        {
            SelectCharacter(numberKeyPressed - 1);
        }
    }

    private void SelectCharacter(int index){
        if(index < 0) throw new System.Exception($"Invalid character index {index}");
        if(index >= _characters.Length) return;

        foreach(var c in _characters) c.State = State.AI;
        _characters[index].State = State.Player;
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
