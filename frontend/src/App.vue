<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter, RouterLink, RouterView } from 'vue-router'
import { getAuthToken, isAuthenticated, setAuthToken } from './api'
import AddSymbolModal from './components/AddSymbolModal.vue'

const router = useRouter()
const theme = ref<'light' | 'dark'>('light')
const authed = ref(isAuthenticated())
const openAdd = ref(false)

function applyTheme(t: 'light' | 'dark') {
  theme.value = t
  document.documentElement.setAttribute('data-theme', t)
  localStorage.setItem('theme', t)
}
function toggleTheme() { applyTheme(theme.value === 'light' ? 'dark' : 'light') }

function onAuthChanged(e: Event) {
  authed.value = !!getAuthToken()
  // close any modal on auth boundary changes
  openAdd.value = false
}
function logout() {
  setAuthToken(null)
  authed.value = false
  window.dispatchEvent(new CustomEvent('auth-changed', { detail: { authed: false } }))
  router.push('/login')
}
function onAdded() {
  // let Home.vue know to refresh
  window.dispatchEvent(new CustomEvent('symbol-added'))
  openAdd.value = false
}

onMounted(() => {
  document.title = 'HubertasVin stock data aggregator'
  const saved = localStorage.getItem('theme') as 'light' | 'dark' | null
  applyTheme(saved === 'dark' ? 'dark' : 'light')
  authed.value = !!getAuthToken()
  openAdd.value = false
  window.addEventListener('auth-changed', onAuthChanged)
})
onUnmounted(() => {
  window.removeEventListener('auth-changed', onAuthChanged)
})
</script>

<template>
  <div class="container">
    <div class="header">
      <div class="title"><span class="brand">HubertasVin</span> stock data aggregator</div>
      <div class="header-actions">
        <button class="toggle" @click="toggleTheme" aria-label="Switch theme">
          <img v-if="theme === 'light'" src="/moon.svg" alt="" class="icon-20 icon-img" />
          <img v-else src="/sun.svg" alt="" class="icon-20 icon-img" />
        </button>

        <RouterLink v-if="!authed" class="btn" to="/login">Login</RouterLink>
        <button v-else class="btn" @click="logout">Logout</button>

        <button v-if="authed" class="btn btn-primary" @click="openAdd = true">+ Add symbol</button>
      </div>
    </div>

    <RouterView />
    <footer>Powered by StockDataAggregator</footer>

    <AddSymbolModal v-if="authed" v-model:open="openAdd" @added="onAdded" />
  </div>
</template>
