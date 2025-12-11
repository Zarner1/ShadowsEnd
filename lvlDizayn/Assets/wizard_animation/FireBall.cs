using UnityEngine;

public class MermiKodu : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 3f); // 3 saniye sonra yok ol
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Oyuncuya çarparsa
        {
            Debug.Log("Vurdum!"); 
            // Buraya hasar verme kodu gelecek
            Destroy(gameObject); // Mermiyi yok et
        }
        else if (other.CompareTag("ground")) // Bir engele çarparsa
        {
            Destroy(gameObject); // Mermiyi yok et
        }
    }
}