<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { login } from '../api'

const router = useRouter()
const username = ref('')
const password = ref('')
const busy = ref(false)
const err = ref('')

async function submit() {
    if (!username.value.trim() || !password.value) return
    err.value = ''
    busy.value = true
    try {
        await login(username.value, password.value)
        window.dispatchEvent(new CustomEvent('auth-changed', { detail: { authed: true } }))
        router.push('/')
    } catch {
        err.value = 'Invalid credentials'
    } finally {
        busy.value = false
    }
}
</script>

<template>
    <div class="auth-wrap">
        <div class="auth-card">
            <div class="auth-title">Admin Sign in</div>

            <div class="field">
                <input class="input" v-model="username" placeholder="Username" autocomplete="username" />
            </div>
            <div class="field">
                <input class="input" v-model="password" type="password" placeholder="Password"
                    autocomplete="current-password" />
            </div>

            <button class="btn btn-primary auth-btn" :disabled="busy" @click="submit">
                {{ busy ? 'Signing in…' : 'Sign in' }}
            </button>

            <div v-if="err" class="status error" style="text-align:center;margin-top:10px">{{ err }}</div>
        </div>
    </div>
</template>
