<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import AddSymbolModal from './components/AddSymbolModal.vue'
import { isAuthenticated, setAuthToken } from './api'

const router = useRouter()

const authed = ref(isAuthenticated())
const addOpen = ref(false)
const theme = ref(localStorage.getItem('theme') || 'light')

function applyTheme() {
  if (theme.value === 'dark') document.documentElement.setAttribute('data-theme', 'dark')
  else document.documentElement.removeAttribute('data-theme')
}
function toggleTheme() {
  theme.value = theme.value === 'dark' ? 'light' : 'dark'
  localStorage.setItem('theme', theme.value)
  applyTheme()
}
function logout() {
  setAuthToken(null)
  authed.value = false
  router.push('/login')
}
function onAdded() {
  addOpen.value = false
  window.dispatchEvent(new CustomEvent('symbol-added'))
}
function onAuthChanged(e: Event) {
  const detail = (e as CustomEvent).detail as any
  authed.value = !!detail?.authed
}

onMounted(() => {
  applyTheme()
  window.addEventListener('auth-changed', onAuthChanged as EventListener)
})
onUnmounted(() => {
  window.removeEventListener('auth-changed', onAuthChanged as EventListener)
})
</script>

<template>
  <div class="container">
    <header class="header">
      <div class="title"><span class="brand">HubertasVin</span> stock data aggregator</div>
      <div class="header-actions">
        <button class="toggle" aria-label="Toggle theme" @click="toggleTheme">
          <img v-if="theme === 'dark'" class="icon-img" src="/sun.svg" alt="Toggle theme" />
          <img v-if="theme === 'light'" class="icon-img" src="/moon.svg" alt="Toggle theme" />
        </button>
        <button v-if="authed" class="btn" @click="addOpen = true">Add symbol</button>
        <router-link v-if="!authed" class="btn" to="/login">Login</router-link>
        <button v-else class="btn" @click="logout">Logout</button>
      </div>
    </header>

    <router-view />

    <footer>© {{ new Date().getFullYear() }}</footer>
  </div>

  <AddSymbolModal :open="addOpen" @update:open="v => (addOpen = v)" @added="onAdded" />
</template>
