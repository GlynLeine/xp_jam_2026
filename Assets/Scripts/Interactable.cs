using UnityEngine;

public class Interactable : MonoBehaviour
{
    public void Interact(PlayerController player)
    {
        gameObject.SendMessage("OnInteract", player);
    }

    public void Hover(PlayerController player)
    {
        gameObject.SendMessage("OnHover", player);
    }
}
