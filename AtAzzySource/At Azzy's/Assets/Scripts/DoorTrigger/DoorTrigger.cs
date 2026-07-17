using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player Enter Trigger");
            FindObjectOfType<SceneController>().LoadTavern();
        }
    }

   
}