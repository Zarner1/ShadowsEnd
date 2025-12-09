using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Hedef")]
    public Transform target; // Karakterini buraya sürükleyeceksin

    [Header("Ayarlar")]
    [Range(0, 1)]
    public float smoothSpeed = 0.125f; // Kameranın takip yumuşaklığı (0 ile 1 arası)
    public Vector3 offset; // Karakterden ne kadar uzakta duracağı

    void LateUpdate()
    {
        // Hedef yoksa hata vermesin
        if (target == null) return;

        // HEDEF POZİSYON:
        // X: Karakterin X pozisyonu + offset (Takip etsin)
        // Y: Kameranın ŞU ANKİ Y pozisyonu (Sabit kalsın)
        // Z: Kameranın ŞU ANKİ Z pozisyonu (Sabit kalsın, genelde -10)
        
        Vector3 desiredPosition = new Vector3(target.position.x + offset.x, transform.position.y, transform.position.z);

        // LERP: Kamerayı aniden ışınlamak yerine yumuşakça kaydırır
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Pozisyonu uygula
        transform.position = smoothedPosition;
        
    }
}