using UnityEngine;

public class AttackEventBridge : MonoBehaviour
{
    public PlayerController player;

    // Este método será llamado desde el evento de la animación
    public void AttackHit()
    {
        if (player != null)
            player.AttackHit();
    }
}
