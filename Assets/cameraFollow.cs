using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Kameranın takip edeceği karakter objesi
    public Transform target; 

    // Kameranın sadece X ekseninde mi hareket etmesini istiyorsunuz?
    public bool followX = true; 
    
    // Kameranın sadece Y ekseninde mi hareket etmesini istiyorsunuz?
    public bool followY = true; 
    
    // Kameranın hedefe ne kadar yumuşak (gecikmeli) ulaşacağını belirler. 
    // Daha küçük değerler daha hızlı takip demektir. (Örn: 0.1)
    public float smoothSpeed = 0.125f; 

    // Unity bu metodu her kare sonunda çağırır. 
    // Kamerayı hareket ettirmek için idealdir, böylece karakterin hareketi tamamlanır.
    void LateUpdate()
    {
        // Eğer takip edilecek bir hedef atanmamışsa, hiçbir şey yapma.
        if (target == null)
            return;

        // Hedef pozisyonunu al. Kameranın Z pozisyonunu koru.
        float targetX = followX ? target.position.x : transform.position.x;
        float targetY = followY ? target.position.y : transform.position.y;
        
        Vector3 targetPosition = new Vector3(targetX, targetY, transform.position.z);

        // Kameranın mevcut pozisyonu ile hedef pozisyonu arasında yumuşak bir geçiş yap.
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
        
        // Kameranın yeni pozisyonunu ayarla.
        transform.position = smoothedPosition;
    }
}