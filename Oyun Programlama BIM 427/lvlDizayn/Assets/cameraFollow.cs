using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Kameranın takip edeceği karakter objesini Unity Inspector'dan atayın.
    public Transform target; 
    
    [Header("Ayarlar")]
    // Kameranın hedefe ne kadar yumuşak (gecikmeli) ulaşacağını belirler. (Örn: 0.125f)
    public float smoothSpeed = 0.125f; 

    // Y ekseninde takibi açıp kapatma (Kapalı tutarsanız kamera Y'de sabit kalır)
    public bool followY = true; 
    
    [Header("Harita Sınırları (Clamping)")]
    // Kameranın gidebileceği en sol (minimum) X pozisyonu
    public float minXPosition = 0f; 
    // Kameranın gidebileceği en sağ (maksimum) X pozisyonu
    public float maxXPosition = 50f; // Bu değeri haritanıza göre ayarlayın
    
    // Kameranın gidebileceği en alt (minimum) Y pozisyonu
    public float minYPosition = 0f; 
    // Kameranın gidebileceği en üst (maksimum) Y pozisyonu
    public float maxYPosition = 10f; // Bu değeri haritanıza göre ayarlayın


    // Bu metot, karakter hareket ettikten sonra (her kare sonunda) çağrılır.
    void LateUpdate()
    {
        // Hedef atanmadıysa veya yok edildiyse, hata vermemek için dur
        if (target == null)
            return;

        // 1. HEDEFLENEN POZİSYONU HESAPLA
        
        // Target X pozisyonunu al.
        float targetX = target.position.x;
        
        // Eğer followY true ise Target Y pozisyonunu al, değilse kameranın mevcut Y pozisyonunu (sabit kalır) al.
        float targetY = followY ? target.position.y : transform.position.y;
        
        // Z eksenini (derinlik) sabit tut.
        Vector3 targetPosition = new Vector3(targetX, targetY, transform.position.z);

        // 2. YUMUŞAK GEÇİŞİ HESAPLA
        // Mevcut pozisyon ile hedef pozisyon arasında yumuşak bir geçiş yapar.
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
        
        // 3. X VE Y POZİSYONLARINI SINIRLA (CLAMP)
        
        // X pozisyonunu minXPosition ve maxXPosition arasında kalacak şekilde sınırla.
        float clampedX = Mathf.Clamp(smoothedPosition.x, minXPosition, maxXPosition);

        // Y pozisyonunu minYPosition ve maxYPosition arasında kalacak şekilde sınırla.
        float clampedY = Mathf.Clamp(smoothedPosition.y, minYPosition, maxYPosition);

        // 4. KAMERANIN YENİ POZİSYONUNU AYARLA
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}