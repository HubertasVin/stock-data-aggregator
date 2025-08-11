<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted } from 'vue'
import { addSymbol } from '../api'

const props = defineProps<{ open: boolean }>()
const emit = defineEmits<{ (e: 'update:open', v: boolean): void; (e: 'added'): void }>()

const symbol = ref('')
const busy = ref(false)
const err = ref('')

function close() { emit('update:open', false) }

watch(() => props.open, (v) => {
    if (v) { symbol.value = ''; err.value = '' }
})

async function submit() {
    if (!symbol.value.trim()) return
    err.value = ''
    busy.value = true
    try {
        await addSymbol(symbol.value)
        emit('added')
    } catch (e) {
        err.value = String(e)
    } finally {
        busy.value = false
    }
}

function onKey(e: KeyboardEvent) {
    if (e.key === 'Escape') close()
}

onMounted(() => {
    window.addEventListener('keydown', onKey)
})
onUnmounted(() => {
    window.removeEventListener('keydown', onKey)
})
</script>

<template>
    <div v-if="open" class="modal-backdrop" @click.self="close">
        <div class="modal" role="dialog" aria-modal="true">
            <div class="modal-title">Add stock symbol</div>
            <input class="input" v-model="symbol" placeholder="AAPL" />
            <div class="modal-actions">
                <button class="btn" @click="close">Cancel</button>
                <button class="btn btn-primary" :disabled="busy || !symbol.trim()" @click="submit">
                    {{ busy ? 'Adding…' : 'Add' }}
                </button>
            </div>
            <div v-if="err" class="status error" style="text-align:left">{{ err }}</div>
        </div>
    </div>
</template>
