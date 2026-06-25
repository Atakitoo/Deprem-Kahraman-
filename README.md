# Deprem Kahramanı
Çocuklar için pedagojik ve güvenli bir yaklaşımla tasarlanmış, deprem bilincini ve afet hazırlığını eğlenceli mekaniklerle aşılayan **eğitici bir 3D simülasyon oyunudur**. 

Bu proje, çocukları korkutmadan onlara acil durum çantası hazırlamanın önemini, afet anında yapılması gerekenleri (Çök-Kapan-Tutun) ve afet sonrasında güvenli bölgeye ulaşma adımlarını öğretmeyi amaçlar.

---

## 🎮 Oynanış Videosu & Özet

Oyunun temel mekaniklerini, eşya toplama sistemini ve sahne geçişlerini içeren kısa tanıtım videosuna aşağıdan göz atabilirsiniz:

https://youtu.be/OtyQtYc5uxM

## 🎮 Yükleme Linki 

https://drive.google.com/file/d/1Mll_09r7N8DV3IsHWZzaA9BH0fKhzP6s/view?usp=drive_link

## ✨ Öne Çıkan Özellikler

* 📱 **Uygulama Tabanlı Akıllı Telefon Sistemi:** Oyun içinde 'Tab' veya 'I' tuşlarıyla açılabilen; Acil Durum Çantası, 112 Arama, Robot Asistan, Oyun Ayarları ve Çıkış uygulamalarını barındıran dinamik telefon arayüzü.
* 🤖 **Eğitici Robot Asistanı:** Yerdeki her eşya (su, düdük, nakit para vb.) farenin sol tıkı ile toplandığında, telefondaki chat uygulamasına o eşyanın gerçek hayatta *neden* hayati olduğunu çocuk diliyle anlatan anlık mesajlar düşer.
* 🎯 **Dinamik Puanlama Sistemi:** Temel 8 acil durum eşyasının ilki 100 Ana Puan kazandırırken, fazla toplanan her kopya eşya ve ekstra eklenen nakit paralar "Ekstra Hazırlık Puanı" olarak yazılır.
* 🛡️ **Pedagojik Geçiş Ekranı:** Çocukların korkmasını önlemek amacıyla, deprem sahnesine geçmeden önce tüm sürecin güvenli bir simülasyon olduğunu vurgulayan, sakinleştirici ve bilgilendirici bir siyah ara ekran bulunur.

---

## 📸 Oyun İçi Görseller

| 🏠 Ev Keşfi ve Eşya Toplama | 📱 Akıllı Telefon Arayüzü |
| :---: | :---: |
| ![Ev Keşfi Screen]([<img width="1427" height="593" alt="Ev" src="https://github.com/user-attachments/assets/c0beb70f-a4d0-40ee-b797-2bc0bfda4533" />]) | ![Telefon Screen]([<img width="1428" height="595" alt="TelefonEsya" src="https://github.com/user-attachments/assets/e6885c24-8785-4626-9de9-d149070bfed9" />]) |

| 🚨 Afet Sonrası Güvenli Bölge | 
| :---: | :---: |
| ![Afet Sahnesi Screen]([<img width="1422" height="592" alt="AcilDurumToplanma" src="https://github.com/user-attachments/assets/d6a98ada-7e8d-41e9-a890-86514c813f73" />]) | 

---

## 🕹️ Sahneler ve Oyun Akışı

### 1. Tutorial (Eğitim Sahnesi)
Oyuncu simülasyon dünyasına giriş yapar. Temel yürüme, koşma, zıplama ve eğilme mekaniklerini temel geometrik engeller üzerinden öğrenir. Ekrandaki yönlendirici altyazılar eşliğinde ilk kez telefonunu açmayı deneyimler.

### 2. Before (Afet Öncesi - Ev Keşfi)
Oyuncu bir ev ortamındadır. Deprem öncesi hazırlık için evi köşe bucak gezerek farenin sol tıkı (Raycast) ile acil durum malzemelerini çantasına aktarır. Tüm zorunlu malzemeler (Su, Fener, İlk Yardım Seti, Düdük, Radyo, Konserve, Çakı, Evraklar) toplanmadan yatakta uyuma seçeneği aktif olmaz.

### 3. After (Afet Sonrası ve Tahliye)
Yatakta uyuma mekanizması tetiklendikten sonra ekrana çocukları rahatlatıcı bir skor ekranı ve "Çök, Kapan, Tutun" görsel rehberi gelir. Ardından deprem sonrasını temsil eden "After" sahnesi yüklenir. Oyuncu güvenli tahliye bölgesine (Safe Zone) ulaştığında ekran kararır ve simülasyon başarıyla tamamlanır.

---

## ⌨️ Kontroller

| Eylem | Tuş Kombinasyonu |
| :--- | :--- |
| **Hareket Etme** | `W` `A` `S` `D` / Yön Tuşları |
| **Koşma** | `Sol Shift` (Basılı Tutarak) |
| **Zıplama** | `Space` (Boşluk Tuşu) |
| **Eğilme** | `Sol Ctrl` |
| **Telefonu Aç / Kapat** | `Tab`  |
| **Eşya Toplama / Etkileşim** | `Farenin Sol Tıkı` |

---

## 🛠️ Teknolojiler ve Mimari

* **Motor:** Unity 3D
* **Dil:** C#
* **Girdi Sistemi:** Unity New Input System (Modern ve esnek girdi yönetimi)
* **Metin/Arayüz:** TextMeshPro (Dinamik ve yüksek çözünürlüklü UI elementleri)
* **Kod Mimarisi:** Temiz, modüler ve performansı optimize eden **Singleton** ve **Observer (Event-driven)** tasarım kalıpları.
