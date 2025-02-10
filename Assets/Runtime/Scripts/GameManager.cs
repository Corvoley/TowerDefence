using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] public Camera mainPlayerCamera;

    
    private void Awake()
    {
        instance = this;
    }
}
