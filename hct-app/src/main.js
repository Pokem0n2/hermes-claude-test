const { invoke } = window.__TAURI__.core;

let config = { buttons: [] };
let currentIndex = -1;

const grid = document.getElementById('buttonGrid');
const modal = document.getElementById('editModal');
const modalTitle = document.getElementById('modalTitle');
const nameInput = document.getElementById('nameInput');
const linkInput = document.getElementById('linkInput');
const saveBtn = document.getElementById('saveBtn');
const cancelBtn = document.getElementById('cancelBtn');
const deleteBtn = document.getElementById('deleteBtn');

function createGrid() {
  grid.innerHTML = '';
  for (let i = 0; i < 25; i++) {
    const btn = document.createElement('button');
    btn.className = 'grid-btn';
    btn.dataset.index = i;
    updateButtonDisplay(btn, config.buttons[i]);
    btn.addEventListener('click', () => handleButtonClick(i));
    grid.appendChild(btn);
  }
}

function updateButtonDisplay(btn, buttonConfig) {
  if (buttonConfig && buttonConfig.name) {
    btn.textContent = buttonConfig.name;
    btn.classList.remove('empty');
  } else {
    btn.textContent = '+';
    btn.classList.add('empty');
  }
}

function openModal(index) {
  currentIndex = index;
  const btn = grid.children[index];
  const btnConfig = config.buttons[index];

  if (btnConfig && btnConfig.name) {
    modalTitle.textContent = `编辑按钮 ${index + 1}`;
    nameInput.value = btnConfig.name || '';
    linkInput.value = btnConfig.link || '';
    deleteBtn.style.display = 'block';
  } else {
    modalTitle.textContent = `设置按钮 ${index + 1}`;
    nameInput.value = '';
    linkInput.value = '';
    deleteBtn.style.display = 'none';
  }

  modal.style.display = 'flex';
  nameInput.focus();
}

function closeModal() {
  modal.style.display = 'none';
  currentIndex = -1;
}

async function saveConfig() {
  try {
    await invoke('save_config', { config });
  } catch (e) {
    console.error('Failed to save config:', e);
  }
}

async function handleSave() {
  const name = nameInput.value.trim();
  const link = linkInput.value.trim();

  if (!name) {
    nameInput.focus();
    return;
  }

  config.buttons[currentIndex] = { name, link };
  updateButtonDisplay(grid.children[currentIndex], config.buttons[currentIndex]);
  await saveConfig();
  closeModal();
}

async function handleDelete() {
  config.buttons[currentIndex] = null;
  updateButtonDisplay(grid.children[currentIndex], null);
  await saveConfig();
  closeModal();
}

async function handleButtonClick(index) {
  const btnConfig = config.buttons[index];
  if (btnConfig && btnConfig.link) {
    try {
      await invoke('open_url', { url: btnConfig.link });
    } catch (e) {
      console.error('Failed to open URL:', e);
    }
  } else {
    openModal(index);
  }
}

async function loadConfig() {
  try {
    config = await invoke('load_config');
    if (!config.buttons || config.buttons.length !== 25) {
      config.buttons = new Array(25).fill(null);
    }
  } catch (e) {
    console.error('Failed to load config:', e);
    config = { buttons: new Array(25).fill(null) };
  }
  createGrid();
}

// Event listeners
saveBtn.addEventListener('click', handleSave);
cancelBtn.addEventListener('click', closeModal);
deleteBtn.addEventListener('click', handleDelete);

nameInput.addEventListener('keydown', (e) => {
  if (e.key === 'Enter') {
    e.preventDefault();
    handleSave();
  }
});

linkInput.addEventListener('keydown', (e) => {
  if (e.key === 'Enter') {
    e.preventDefault();
    handleSave();
  }
});

modal.addEventListener('click', (e) => {
  if (e.target === modal) {
    closeModal();
  }
});

window.addEventListener('DOMContentLoaded', loadConfig);