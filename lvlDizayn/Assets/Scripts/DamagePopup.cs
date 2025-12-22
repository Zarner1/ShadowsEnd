using UnityEngine;
using TMPro; 

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer = 1f; 
    private Color textColor;

    // Prefab oluştuğunda bu fonksiyonu çağıracağız
    public void Setup(int damageAmount)
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh != null)
        {
            textMesh.text = damageAmount.ToString();
            textColor = textMesh.color;
        }
    }

    void Update()
    {
        float moveSpeed = 2f;
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float fadeSpeed = 3f;
            if (textMesh != null)
            {
                textColor.a -= fadeSpeed * Time.deltaTime;
                textMesh.color = textColor;

                if (textColor.a < 0)
                {
                    Destroy(gameObject);
                }
            }
            else // Eğer textmesh yoksa direkt yok et (Hata önleyici)
            {
                Destroy(gameObject);
            }
        }
    }
}