using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndHandler : MonoBehaviour
{
    [Header("UI Ayarları")]
    [SerializeField] private GameObject gameEndCanvas; // Hazırladığımız GameEndCanvas'ı buraya sürükleyeceğiz

    [Header("Sahne Ayarları")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Ana menü sahnenizin tam adı

    private void OnTriggerEnter(Collider other)
    {
        // Bölgeye giren nesnenin tag'i "Player" ise oyunu bitir
        if (other.CompareTag("Player"))
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        // 1. Siyah ekranı ve teşekkür yazısını aktifleştir
        if (gameEndCanvas != null)
        {
            gameEndCanvas.SetActive(true);
        }

        // 2. Karakterin arkada hareket etmesini önlemek için zamanı durdur
        Time.timeScale = 0f;

        // 3. Fare imlecini görünür yap ve kilidini aç (Butona basabilmek için)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Bu fonksiyonu "Ana Menüye Dön" butonuna bağlayacağız
    public void ReturnToMainMenu()
    {
        // Zaman akışını normale döndür (Menü ve diğer sahneler için çok önemli!)
        Time.timeScale = 1f;

        // Ana menü sahnesini yükle
        SceneManager.LoadScene(mainMenuSceneName);
    }
}