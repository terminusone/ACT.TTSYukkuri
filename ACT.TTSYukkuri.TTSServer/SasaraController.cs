﻿namespace ACT.TTSYukkuri.TTSServer
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using ACT.TTSYukkuri.TTSServer.Core.Models;
    using ACT.TTSYukkuri.TTSServer.Properties;
    using CeVIO.Talk.RemoteService;
    using NAudio.Wave;

    public class SasaraController
    {
        #region Singleton

        private static SasaraController instance = new SasaraController();

        public static SasaraController Default => instance;

        #endregion Singleton

        private Talker talker;

        public void CloseSasara()
        {
            if (ServiceControl.IsHostStarted)
            {
                ServiceControl.CloseHost();
            }

            if (talker != null)
            {
                talker = null;
            }
        }

        public SasaraSettings GetSasaraSettings()
        {
            this.StartSasara();

            var settings = new SasaraSettings();

            settings.Volume = talker.Volume;
            settings.Speed = talker.Speed;
            settings.Tone = talker.Tone;
            settings.Alpha = talker.Alpha;
            settings.Cast = talker.Cast;
            settings.ToneScale = talker.ToneScale;
            settings.AvailableCasts = Talker.AvailableCasts;

            var compornents = new List<SasaraTalkerComponent>();
            for (int i = 0; i < talker.Components.Count; i++)
            {
                compornents.Add(new SasaraTalkerComponent()
                {
                    Id = talker.Components[i].Id,
                    Name = talker.Components[i].Name,
                    Value = talker.Components[i].Value,
                });
            }

            settings.Components = compornents.ToArray();

            return settings;
        }

        public void OutputWaveToFile(
            string textToSpeak,
            string waveFile,
            SasaraSettings settings = null)
        {
            if (string.IsNullOrWhiteSpace(textToSpeak))
            {
                return;
            }

            this.StartSasara();

            if (settings != null)
            {
                this.SetSasaraSettings(settings);
            }

            var tempWave = Path.GetTempFileName();

            var stat = talker.OutputWaveToFile(
                textToSpeak,
                tempWave);

            if (stat)
            {
#if DEBUG
                File.Copy(tempWave, "Sasara.wave", true);
#endif

                // ささらは音量が小さめなので増幅する
                using (var reader = new WaveFileReader(tempWave))
                {
                    var prov = new VolumeWaveProvider16(reader);
                    prov.Volume = Settings.Default.SasaraGain;

                    WaveFileWriter.CreateWaveFile(
                        waveFile,
                        prov);
                }
            }

            if (File.Exists(tempWave))
            {
                File.Delete(tempWave);
            }
        }

        public void SetSasaraSettings(
            SasaraSettings settings)
        {
            this.StartSasara();

            if (string.IsNullOrWhiteSpace(talker.Cast) &&
                Talker.AvailableCasts.Length > 0)
            {
                talker.Cast = Talker.AvailableCasts[0];
            }

            if (talker.Cast != settings.Cast ||
                talker.Volume != settings.Volume ||
                talker.Speed != settings.Speed ||
                talker.Tone != settings.Tone ||
                talker.Alpha != settings.Alpha ||
                talker.ToneScale != settings.ToneScale)
            {
                talker.Cast = settings.Cast;
                talker.Volume = settings.Volume;
                talker.Speed = settings.Speed;
                talker.Tone = settings.Tone;
                talker.Alpha = settings.Alpha;
                talker.ToneScale = settings.ToneScale;
            }

            if (settings.Components != null)
            {
                foreach (var c in settings.Components)
                {
                    var t = talker.Components
                        .Where(x => x.Id == c.Id)
                        .FirstOrDefault();

                    if (t != null)
                    {
                        if (t.Value != c.Value)
                        {
                            t.Value = c.Value;
                        }
                    }
                }
            }
        }

        public void StartSasara()
        {
            if (!ServiceControl.IsHostStarted)
            {
                ServiceControl.StartHost(false);
            }

            if (talker == null)
            {
                talker = new Talker();
            }
        }
    }
}
