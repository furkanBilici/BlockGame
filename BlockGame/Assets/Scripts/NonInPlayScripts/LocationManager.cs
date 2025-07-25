using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using LootLocker.Requests;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Dynamic;

public class LocationManager : MonoBehaviour
{
    public static LocationManager Instance;
    //private const string IP_API_URL = "https://ip-api.com/json";
    private const string IP_API_URL = "https://ipinfo.io/json";
    private const string LAST_CHECK_TIME_KEY = "LastLocationCheckTime";
    private const string PLAYER_LOCATION_FILENAME = "player_location.json";

    public string countryCode = "XX";
    public string city = "abcd";
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        countryCode=PlayerPrefs.GetString("CountryCode");
        city=PlayerPrefs.GetString("City");
    }
    public void CheckAndUpdateLocation()
    {
        if (PlayerPrefs.HasKey(LAST_CHECK_TIME_KEY))
        {
            long lastCheckTicks = long.Parse(PlayerPrefs.GetString(LAST_CHECK_TIME_KEY));
            System.DateTime lastCheckTime = new System.DateTime(lastCheckTicks);
            if ((System.DateTime.UtcNow - lastCheckTime).TotalHours < 72)
            {
                Debug.Log("Son konum kontrolünden bu yana 72 saat geçmedi. Ýstek atlanýyor.");
                return;
            }
        }
        StartCoroutine(GetLocationFromIp());

            
    }
    private IEnumerator GetLocationFromIp()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(IP_API_URL))
        {
            
            webRequest.SetRequestHeader("User-Agent", "Mozilla/5.0");
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("IP API'sinden Gelen Ham Yanýt: " + jsonResponse);
                IPinfo info = JsonUtility.FromJson<IPinfo>(jsonResponse);
                if (info != null && !string.IsNullOrEmpty(info.country))
                {
                    Debug.Log($"Konum baþarýyla tespit edildi: Þehir={info.city}, Ülke Kodu={info.country}");

                    PlayerPrefs.SetString(LAST_CHECK_TIME_KEY, System.DateTime.UtcNow.Ticks.ToString());
                    countryCode = info.country;
                    city = info.city;
                    PlayerPrefs.SetString("CountryCode",countryCode);
                    PlayerPrefs.SetString("City",city);
                }
                else
                {
                    Debug.LogError("IP API'sinden baþarýsýz veya geçersiz bir yanýt alýndý.");
                }
            }
            else
            {
                Debug.LogError("Konum bilgisi alýnamadý. Hata: " + webRequest.error);
            }
        }
    }
    private void UploadOrUpdatePlayerLocationFile(string countryCode, string city)
    {
        // 1. Veri objesini oluþtur ve JSON'a çevir.
        PlayerLocationData locationData = new PlayerLocationData
        {
            country_code = countryCode,
            city_name = city
        };
        string jsonContent = JsonUtility.ToJson(locationData);

        string tempFilePath = Path.Combine(Application.temporaryCachePath, PLAYER_LOCATION_FILENAME);
        File.WriteAllText(tempFilePath, jsonContent);

        // 3. Dosyanýn sunucuda zaten var olup olmadýðýný kontrol et.
        LootLockerSDKManager.GetAllPlayerFiles((response) =>
        {
            if (response.success)
            {
                // Mevcut dosyalar içinde bizim dosyamýzý ara.
                var existingFile = response.items.FirstOrDefault(file => file.name == PLAYER_LOCATION_FILENAME);

                if (existingFile != null)
                {
                    // DOSYA VAR: Update et.
                    Debug.Log($"'{PLAYER_LOCATION_FILENAME}' dosyasý bulundu (ID: {existingFile.id}). Güncelleniyor...");
                    UpdatePlayerFile(existingFile.id, tempFilePath);
                }
                else
                {
                    // DOSYA YOK: Ýlk defa oluþtur (Upload).
                    Debug.Log($"'{PLAYER_LOCATION_FILENAME}' dosyasý bulunamadý. Yeni dosya oluþturuluyor...");
                    UploadPlayerFile(tempFilePath);
                }
            }
            else
            {
                Debug.LogError("Oyuncu dosyalarý çekilemedi: " + response.text);
                // Hata durumunda geçici dosyayý silelim.
                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
            }
        });
    }

    private void UploadPlayerFile(string filePath)
    {
        LootLockerSDKManager.UploadPlayerFile(filePath, PLAYER_LOCATION_FILENAME, (response) =>
        {
            if (File.Exists(filePath)) File.Delete(filePath); // Her durumda geçici dosyayý sil.

            if (response.success)
            {
                Debug.Log($"Oyuncu konum dosyasý baþarýyla yüklendi.");
            }
            else
            {
                Debug.LogError($"Konum dosyasý yüklenemedi: " + response.text);
            }
        });
    }

    private void UpdatePlayerFile(int fileId, string filePath)
    {
        LootLockerSDKManager.UpdatePlayerFile(fileId, filePath, (response) =>
        {
            if (File.Exists(filePath)) File.Delete(filePath); // Her durumda geçici dosyayý sil.

            if (response.success)
            {
                Debug.Log($"Oyuncu konum dosyasý baþarýyla güncellendi.");
            }
            else
            {
                Debug.LogError($"Konum dosyasý güncellenemedi: " + response.text);
            }
        });
    }
}
