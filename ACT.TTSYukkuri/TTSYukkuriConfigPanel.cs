﻿namespace ACT.TTSYukkuri
{
    using System;
    using System.Windows.Forms;

    using ACT.TTSYukkuri.Config;
    using ACT.TTSYukkuri.SoundPlayer;

    /// <summary>
    /// TTSゆっくり設定Panel
    /// </summary>
    public partial class TTSYukkuriConfigPanel : UserControl
    {
        /// <summary>
        /// TTS設定Panel
        /// </summary>
        private UserControl ttsSettingsPanel;

        /// <summary>
        /// ロード完了
        /// </summary>
        private bool Loaded;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TTSYukkuriConfigPanel()
        {
            this.InitializeComponent();

            this.ttsShuruiComboBox.DisplayMember = "Display";
            this.ttsShuruiComboBox.ValueMember = "Value";
            this.ttsShuruiComboBox.DataSource = TTSType.ToComboBox;

            this.ttsShuruiComboBox.TextChanged += (s1, e1) =>
            {
                if (this.Loaded)
                {
                    this.SaveSettings();

                    // 再生デバイスの選択の使用状況を切り替える
                    if (TTSYukkuriConfig.Default.TTS == TTSType.Boyomichan)
                    {
                        this.saiseiDeviceGroupBox.Enabled = false;
                    }
                    else
                    {
                        this.saiseiDeviceGroupBox.Enabled = true;
                    }

                    this.LoadTTS();
                }
            };
        }

        /// <summary>
        /// Load
        /// </summary>
        /// <param name="sender">イベント発生元</param>
        /// <param name="e">イベント引数</param>
        private void TTSYukkuriConfigPanel_Load(object sender, EventArgs e)
        {
            // 再生デバイスコンボボックスを設定する
            this.mainDeviceComboBox.DisplayMember = "Description";
            this.mainDeviceComboBox.ValueMember = "Guid";
            this.mainDeviceComboBox.DataSource = NAudioPlayer.EnumlateDevices();

            this.subDeviceComboBox.DisplayMember = "Description";
            this.subDeviceComboBox.ValueMember = "Guid";
            this.subDeviceComboBox.DataSource = NAudioPlayer.EnumlateDevices();

            if (TTSYukkuriConfig.Default.MainDeviceID != null)
            {
                this.mainDeviceComboBox.SelectedValue = TTSYukkuriConfig.Default.MainDeviceID;
            }
            else
            {
                this.mainDeviceComboBox.SelectedIndex = 0;
            }

            this.enabledSubDeviceCheckBox.Checked = TTSYukkuriConfig.Default.EnabledSubDevice;

            if (TTSYukkuriConfig.Default.SubDeviceID != null)
            {
                this.subDeviceComboBox.SelectedValue = TTSYukkuriConfig.Default.SubDeviceID;
            }
            else
            {
                this.subDeviceComboBox.SelectedIndex = 0;
            }

            this.WaveVolTrackBar.Value = TTSYukkuriConfig.Default.WaveVolume;

            this.WaveCacheClearCheckBox.Checked = TTSYukkuriConfig.Default.WaveCacheClearEnable;

            this.subDeviceComboBox.Enabled = this.enabledSubDeviceCheckBox.Checked;

            this.mainDeviceComboBox.TextChanged += (s1, e1) =>
            {
                this.SaveSettings();
            };

            this.enabledSubDeviceCheckBox.CheckedChanged += (s1, e1) =>
            {
                var c = s1 as CheckBox;
                this.subDeviceComboBox.Enabled = c.Checked;
                this.SaveSettings();
            };

            this.subDeviceComboBox.TextChanged += (s1, e1) =>
            {
                this.SaveSettings();
            };

            this.WaveVolTrackBar.ValueChanged += (s1, e1) =>
            {
                this.SaveSettings();
            };

            this.WaveCacheClearCheckBox.CheckStateChanged += (s1, e1) =>
            {
                this.SaveSettings();
            };

            this.ttsShuruiComboBox.SelectedValue = TTSYukkuriConfig.Default.TTS;
            if (TTSYukkuriConfig.Default.TTS == TTSType.Boyomichan)
            {
                this.saiseiDeviceGroupBox.Enabled = false;
            }
            else
            {
                this.saiseiDeviceGroupBox.Enabled = true;
            }

            this.LoadTTS();

            // オプションのロードを呼出す
            this.LoadOptions();

            this.Loaded = true;
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        private void SaveSettings()
        {
            TTSYukkuriConfig.Default.TTS = (this.ttsShuruiComboBox.SelectedItem as ComboBoxItem).Value;

            TTSYukkuriConfig.Default.MainDeviceID = (Guid)this.mainDeviceComboBox.SelectedValue;
            TTSYukkuriConfig.Default.EnabledSubDevice = this.enabledSubDeviceCheckBox.Checked;
            TTSYukkuriConfig.Default.SubDeviceID = (Guid)this.subDeviceComboBox.SelectedValue;
            TTSYukkuriConfig.Default.WaveVolume = (int)this.WaveVolTrackBar.Value;
            TTSYukkuriConfig.Default.WaveCacheClearEnable = this.WaveCacheClearCheckBox.Checked;

            this.SaveSettingsOptions();

            TTSYukkuriConfig.Default.Save();
        }

        /// <summary>
        /// TTSを読み込む
        /// </summary>
        private void LoadTTS()
        {
            try
            {
                // TTSを初期化する
                SpeechController.Default.Initialize();

                // 前のPanelを除去する
                if (this.ttsSettingsPanel != null)
                {
                    this.ttsSettingsGroupBox.Controls.Remove(this.ttsSettingsPanel);
                }

                // 新しいPanelをセットする
                var ttsSettingsPanelNew = SpeechController.Default.TTSSettingsPanel;
                if (ttsSettingsPanelNew != null)
                {
                    ttsSettingsPanelNew.Dock = DockStyle.Fill;
                    this.ttsSettingsGroupBox.Controls.Add(ttsSettingsPanelNew);

                    this.ttsSettingsPanel = ttsSettingsPanelNew;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "TTSの初期化中に例外が発生しました。環境を確認してください" + Environment.NewLine + Environment.NewLine +
                    ex.ToString(),
                    "TTSゆっくりプラグイン",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                // TTSをゆっくりに戻す
                TTSYukkuriConfig.Default.TTS = TTSType.Yukkuri;
                TTSYukkuriConfig.Default.Save();
                this.ttsShuruiComboBox.SelectedValue = TTSYukkuriConfig.Default.TTS;
            }
        }
    }
}
