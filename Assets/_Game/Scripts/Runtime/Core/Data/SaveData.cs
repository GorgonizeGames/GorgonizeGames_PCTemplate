using System;

namespace Game.Runtime.Core.Data
{
    /// <summary>
    /// Kaydedilecek tüm oyun verileri için temel (soyut) sınıf.
    /// Her yeni oyun, kendi SaveData'sını bu sınıftan türetmelidir.
    /// </summary>
    [Serializable]
    public abstract class SaveData
    {
        // Her oyunda olabilecek genel ayar verileri.
        public GameSettingsData Settings = new GameSettingsData();

        // Her save dosyasının sahip olacağı ortak veriler.
        public float TotalPlayTime;
        public DateTime LastSaveTime;

        protected SaveData()
        {
            LastSaveTime = DateTime.UtcNow;
            TotalPlayTime = 0;
        }
    }

    /// <summary>
    /// Tüm oyunlarda ortak olabilecek ayar verilerini tutan sınıf.
    /// (Ses, grafik, dil vb.)
    /// </summary>
    [Serializable]
    public class GameSettingsData
    {
        public float MasterVolume = 1.0f;
        public float MusicVolume = 0.8f;
        public float SfxVolume = 1.0f;
        public bool VSync = true;
        // Buraya gelecekte dil seçeneği gibi başka ayarlar da eklenebilir.
    }
}