using UnityEngine;

public class CustomGameManager : MonoBehaviour
{
    public int size;
    public int difficulty;
    private void Awake()
    {
        difficulty = PlayerPrefs.GetInt("difficulty", 0);
        size= PlayerPrefs.GetInt("boardScale", 0) + 7;
        if (size == 7)
        {
            transform.position = new Vector2(transform.position.x - 0.5f, transform.position.y - 0.5f);
        }
        else if (size == 9)
        {
            transform.position = new Vector2(transform.position.x + 0.5f, transform.position.y + 0.5f);
        }
    }
}
