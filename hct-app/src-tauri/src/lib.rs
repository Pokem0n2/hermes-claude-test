use serde::{Deserialize, Serialize};
use std::fs;
use std::path::PathBuf;
use tauri::Manager;

#[derive(Debug, Serialize, Deserialize, Clone)]
pub struct ButtonConfig {
    pub name: String,
    pub link: String,
}

#[derive(Debug, Serialize, Deserialize, Clone, Default)]
pub struct AppConfig {
    pub buttons: Vec<Option<ButtonConfig>>,
}

impl AppConfig {
    pub fn new() -> Self {
        Self {
            buttons: vec![None; 25],
        }
    }
}

fn get_config_path() -> PathBuf {
    let exe_path = std::env::current_exe().unwrap_or_default();
    let exe_dir = exe_path.parent().unwrap_or(std::path::Path::new("."));
    exe_dir.join("hct-config.json")
}

#[tauri::command]
fn load_config() -> AppConfig {
    let path = get_config_path();
    if path.exists() {
        match fs::read_to_string(&path) {
            Ok(content) => {
                serde_json::from_str(&content).unwrap_or_else(|_| AppConfig::new())
            }
            Err(_) => AppConfig::new(),
        }
    } else {
        AppConfig::new()
    }
}

#[tauri::command]
fn save_config(config: AppConfig) -> Result<(), String> {
    let path = get_config_path();
    let json = serde_json::to_string_pretty(&config).map_err(|e| e.to_string())?;
    fs::write(&path, json).map_err(|e| e.to_string())
}

#[tauri::command]
fn open_url(url: String) {
    if let Err(e) = open::that(&url) {
        eprintln!("Failed to open URL: {}", e);
    }
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![load_config, save_config, open_url])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}