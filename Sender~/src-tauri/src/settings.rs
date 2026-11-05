use serde::{Deserialize, Serialize};
use std::fs;
use std::net::{SocketAddr, ToSocketAddrs};
use std::path::{Path, PathBuf};
use tauri::{AppHandle, Manager};

#[derive(Serialize, Deserialize, Clone)]
#[serde(default, rename_all = "camelCase")]
pub struct Settings {
    pub host: String,
    pub port: u16,
    pub hold_ms: u64,
    pub last_map_path: Option<String>,
    pub last_clip_dir: Option<String>,
    pub receive: bool,
    pub receive_port: u16,
}

impl Default for Settings {
    fn default() -> Self {
        Self {
            host: "127.0.0.1".to_string(),
            port: 9000,
            hold_ms: 250,
            last_map_path: None,
            last_clip_dir: None,
            receive: true,
            receive_port: 9001,
        }
    }
}

impl Settings {
    pub fn load(app: &AppHandle) -> Self {
        let Ok(path) = settings_path(app) else {
            return Self::default();
        };
        // アプリの識別子が変わっても、以前の版の送信先とポートを使えるようにする。
        let legacy = path.parent().and_then(Path::parent).map(|base| {
            base.join("net.maaaaa.kaotsuki.sender")
                .join("settings.json")
        });
        load_from_paths(&path, legacy.as_deref())
    }

    pub fn save(&self, app: &AppHandle) -> Result<(), String> {
        let path = settings_path(app)?;
        let parent = path
            .parent()
            .ok_or_else(|| "設定の保存先を取得できません".to_string())?;
        fs::create_dir_all(parent).map_err(|error| error.to_string())?;
        let content = serde_json::to_string_pretty(self).map_err(|error| error.to_string())?;
        fs::write(path, content).map_err(|error| error.to_string())
    }

    pub fn validate(&self) -> Result<SocketAddr, String> {
        if self.receive_port == 0 {
            return Err("受信ポートは 1〜65535 で指定してください".to_string());
        }
        if self.port == 0 {
            return Err("ポート番号は 1〜65535 で指定してください".to_string());
        }
        if !(100..=1000).contains(&self.hold_ms) {
            return Err("保持時間は 100〜1000 ミリ秒で指定してください".to_string());
        }

        (self.host.as_str(), self.port)
            .to_socket_addrs()
            .map_err(|_| format!("送信先のアドレスを取得できません: {}", self.host))?
            .find(SocketAddr::is_ipv4)
            .ok_or_else(|| format!("送信先のアドレスを取得できません: {}", self.host))
    }
}

fn load_from_paths(path: &Path, legacy: Option<&Path>) -> Settings {
    let selected = if !path.exists() {
        legacy.unwrap_or(path)
    } else {
        path
    };
    fs::read_to_string(selected)
        .ok()
        .and_then(|content| serde_json::from_str(&content).ok())
        .unwrap_or_default()
}

fn settings_path(app: &AppHandle) -> Result<PathBuf, String> {
    app.path()
        .app_config_dir()
        .map(|path| path.join("settings.json"))
        .map_err(|error| error.to_string())
}

#[cfg(test)]
mod tests {
    use super::Settings;

    #[test]
    fn old_settings_are_fallback_and_new_settings_take_priority() {
        let folder =
            std::env::temp_dir().join(format!("face-util-settings-{}", std::process::id()));
        std::fs::create_dir_all(&folder).expect("create fixture");
        let new = folder.join("new.json");
        let old = folder.join("old.json");
        let _ = std::fs::remove_file(&new);
        std::fs::write(&old, r#"{"port":9100}"#).expect("write legacy settings");
        assert_eq!(super::load_from_paths(&new, Some(&old)).port, 9100);
        std::fs::write(&new, r#"{"port":9200}"#).expect("write new settings");
        assert_eq!(super::load_from_paths(&new, Some(&old)).port, 9200);
        std::fs::remove_dir_all(folder).expect("remove fixture");
    }

    #[test]
    fn legacy_settings_keep_existing_values() {
        let settings: Settings = serde_json::from_str(
            r#"{"host":"127.0.0.2","port":9100,"holdMs":400,"lastMapPath":"map.json"}"#,
        )
        .expect("legacy settings");
        assert_eq!(settings.host, "127.0.0.2");
        assert_eq!(settings.port, 9100);
        assert_eq!(settings.hold_ms, 400);
        assert_eq!(settings.last_map_path.as_deref(), Some("map.json"));
        assert_eq!(settings.last_clip_dir, None);
        assert!(settings.receive);
        assert_eq!(settings.receive_port, 9001);
    }

    #[test]
    fn zero_receive_port_is_rejected() {
        let settings = Settings {
            receive_port: 0,
            ..Settings::default()
        };
        assert_eq!(
            settings.validate().err().as_deref(),
            Some("受信ポートは 1〜65535 で指定してください")
        );
    }
}
