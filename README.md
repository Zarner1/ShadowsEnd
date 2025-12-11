# ShadowsEnd / lvlDizayn

Kısa açıklama
- Bu depo Unity proje dosyalarını içerir. Tüm proje içeriği `lvlDizayn/` klasörünün altında bulunur.

Unity sürümü
- Proje tarafından belirtilen Unity Editor sürümü: `6000.3.0f1` (ProjectSettings/ProjectVersion.txt).

Nasıl çalıştırılır (klonlayan için)
1. Repo'yu klonlayın:
```powershell
git clone https://github.com/Zarner1/ShadowsEnd.git
cd ShadowsEnd
```

2. Unity Hub veya Unity Editor ile projeyi açın
- Unity Hub → "Add" → klonladığınız klasörü (`ShadowsEnd`) seçin.
- Veya Unity Editor'den "Open Project" ile `ShadowsEnd` klasörünü açın.
- Editor, `lvlDizayn/ProjectSettings/ProjectVersion.txt` ile uyumlu bir Unity sürümünü kullanmanızı önerir (yukarıda belirtilen sürüm).

3. İlk açılışta
- Unity `Library/` klasörünü yeniden oluşturur ve assetleri import eder — bu işlem zaman alır.
- Package Manager gerekli paketleri indirip kuracaktır (internet gereklidir).

4. Oyunu çalıştırma
- Unity Editor içinde `File > Build Settings` menüsünden sahneleri (`Scenes`) kontrol edin.
- Sahneyi seçip Editor içinden `Play` ile test edin veya `Build` ile platforma özel executable oluşturun.

Önemli notlar
- `Library/`, `Logs/`, `UserSettings/` gibi cache klasörleri repoda yoktur (doğru yapı). Klonlayanların proje açılışında bu klasörler yerel olarak oluşturulur.
- Eğer büyük ikili dosyalar (100MB+) gerekiyorsa ileride `git-lfs` kullanılması gerekebilir; şu an push başarılı oldu.

Destek/Notlar
- Yeni karakter/dosyalar için `lvlDizayn/Characters/` gibi ayrı bir klasör tavsiye edilir.
- Sorun yaşarsanız Unity ve Git hata mesajlarını paylaşın, yardımcı olurum.

---
Bu README otomatik olarak eklendi ve pushlandı.
