<script setup lang="ts">
definePageMeta({ middleware: 'auth' })

const { account, logout } = useAuth()

async function signOut() {
  logout()
  await navigateTo('/')
}
</script>

<template>
  <div class="container account">
    <aside>
      <div class="eyebrow">Your account</div>
      <h2 v-if="account">Hi, {{ account.firstName }}</h2>
      <nav>
        <NuxtLink to="/account" exact-active-class="active">Details</NuxtLink>
        <NuxtLink to="/account/orders" active-class="active">Orders</NuxtLink>
        <NuxtLink to="/account/addresses" active-class="active">Delivery addresses</NuxtLink>
        <NuxtLink to="/account/cards" active-class="active">Payment cards</NuxtLink>
      </nav>
      <button class="link-btn" @click="signOut">Sign out</button>
    </aside>
    <section>
      <NuxtPage />
    </section>
  </div>
</template>

<style scoped>
.account { display: grid; grid-template-columns: 240px 1fr; gap: 40px; padding-top: 40px; align-items: start; }
aside { position: sticky; top: 100px; display: grid; gap: 14px; }
aside h2 { margin: 0; font-size: 1.6rem; }
nav { display: grid; gap: 4px; margin: 6px 0 8px; }
nav a { text-decoration: none; padding: 8px 12px; border-radius: 10px; color: var(--muted); font-weight: 500; }
nav a:hover { color: var(--ink); background: var(--kraft); }
nav a.active { background: var(--ink); color: var(--paper); }
aside .link-btn { justify-self: start; padding-left: 12px; }
@media (max-width: 800px) {
  .account { grid-template-columns: 1fr; gap: 24px; }
  aside { position: static; }
  nav { grid-auto-flow: column; overflow-x: auto; }
  nav a { white-space: nowrap; }
}
</style>
