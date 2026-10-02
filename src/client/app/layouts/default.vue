<script setup lang="ts">
const route = useRoute()
const navOpen = ref(false)
const { account, signedIn } = useAuth()
const { count: cartCount } = useCart()
watch(() => route.fullPath, () => { navOpen.value = false })
</script>

<template>
  <div class="page">
    <div class="announce">Free shipping on orders over $60 · Roasted fresh every Tuesday</div>

    <header class="site-header">
      <div class="container bar">
        <NuxtLink to="/" class="brand">
          <svg viewBox="0 0 64 40" aria-hidden="true"><path d="M16 38a14 14 0 0 1-1.6-27.9A18 18 0 0 1 49 12a13 13 0 0 1-1 26z" fill="currentColor" /></svg>
          <span>Nimbus <em>Coffee Co.</em></span>
        </NuxtLink>

        <SearchBox class="header-search" />

        <div class="actions">
          <NuxtLink v-if="signedIn" to="/account" class="account-link" title="Your account">
            <span class="avatar">{{ account?.firstName?.charAt(0) ?? '?' }}</span>
            <span class="who">{{ account?.firstName ?? 'Account' }}</span>
          </NuxtLink>
          <NuxtLink v-else to="/login" class="account-link">Sign in</NuxtLink>

          <NuxtLink to="/cart" class="cart-link" :title="`Cart (${cartCount})`">
            <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M6 7h12l-1 13H7L6 7zm3 0a3 3 0 0 1 6 0" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round" /></svg>
            <span class="sr-only">Cart</span>
            <span v-if="cartCount" class="count">{{ cartCount }}</span>
          </NuxtLink>

          <button class="menu" aria-label="Menu" :aria-expanded="navOpen" @click="navOpen = !navOpen">
            <span /><span /><span />
          </button>
        </div>
      </div>

      <nav :class="{ open: navOpen }">
        <div class="container links">
          <NuxtLink to="/shop">Shop all</NuxtLink>
          <NuxtLink to="/shop?category=single-origin">Single origin</NuxtLink>
          <NuxtLink to="/shop?category=blend">Blends</NuxtLink>
          <NuxtLink to="/shop?category=decaf">Decaf</NuxtLink>
          <NuxtLink to="/shop?category=equipment">Equipment</NuxtLink>
        </div>
      </nav>
    </header>

    <main>
      <slot />
    </main>

    <footer class="site-footer">
      <div class="container cols">
        <div>
          <strong class="foot-brand">Nimbus Coffee Co.</strong>
          <p>Small-batch specialty coffee, roasted in Melbourne and delivered Australia-wide.</p>
        </div>
        <div>
          <h4>Shop</h4>
          <NuxtLink to="/shop?category=single-origin">Single origin</NuxtLink>
          <NuxtLink to="/shop?category=blend">Blends</NuxtLink>
          <NuxtLink to="/shop?category=decaf">Decaf</NuxtLink>
          <NuxtLink to="/shop?category=equipment">Equipment</NuxtLink>
        </div>
        <div>
          <h4>Help</h4>
          <NuxtLink :to="signedIn ? '/account' : '/login'">Your account</NuxtLink>
          <span>Shipping &amp; returns</span>
          <span>Brew guides</span>
          <span>Contact us</span>
        </div>
      </div>
      <div class="container fine">A demo storefront for async microservices with Nimbus.</div>
    </footer>
  </div>
</template>

<style scoped>
.page { min-height: 100vh; display: flex; flex-direction: column; }
main { flex: 1; }

.announce {
  background: var(--ink); color: var(--kraft);
  text-align: center; font-size: .8rem; letter-spacing: .04em; padding: 8px 16px;
}

.site-header {
  position: sticky; top: 0; z-index: 10;
  background: rgba(245, 240, 232, .92);
  backdrop-filter: blur(8px);
  border-bottom: 1px solid var(--line);
}
.bar { display: flex; align-items: center; gap: 32px; height: 68px; }

.brand { display: flex; align-items: center; gap: 10px; text-decoration: none; font: 700 1.3rem var(--serif); white-space: nowrap; }
.brand svg { width: 38px; color: var(--accent); }
.brand em { font-style: normal; font-weight: 500; color: var(--muted); }

.header-search { flex: 1; max-width: 460px; margin: 0 auto; }

.actions { display: flex; align-items: center; gap: 20px; }

.account-link { display: flex; align-items: center; gap: 8px; text-decoration: none; font-weight: 600; font-size: .92rem; white-space: nowrap; }
.account-link:hover { color: var(--accent-dark); }
.avatar {
  width: 32px; height: 32px; border-radius: 50%;
  display: grid; place-items: center;
  background: var(--accent); color: #fff; font-size: .9rem; text-transform: uppercase;
}

.cart-link { position: relative; display: grid; place-items: center; width: 36px; height: 36px; color: var(--ink); }
.cart-link svg { width: 26px; height: 26px; }
.cart-link:hover { color: var(--accent-dark); }
.cart-link .count {
  position: absolute; top: -4px; right: -6px;
  min-width: 19px; height: 19px; padding: 0 5px; border-radius: 999px;
  background: var(--accent); color: #fff; font-size: .72rem; font-weight: 600;
  display: grid; place-items: center;
}
.sr-only { position: absolute; width: 1px; height: 1px; overflow: hidden; clip: rect(0 0 0 0); white-space: nowrap; }

nav { border-top: 1px solid var(--line); }
.links { display: flex; gap: 32px; height: 44px; align-items: stretch; }
.links a {
  display: flex; align-items: center;
  text-decoration: none; font-weight: 500; font-size: .92rem; color: var(--muted);
  border-bottom: 2px solid transparent; margin-bottom: -1px;
}
.links a:hover { color: var(--ink); }
.links a.router-link-exact-active { color: var(--ink); border-bottom-color: var(--accent); }

.menu { display: none; background: none; border: 0; padding: 8px; margin-right: -8px; cursor: pointer; }
.menu span { display: block; width: 22px; height: 2px; background: var(--ink); margin: 5px 0; }

@media (max-width: 800px) {
  .bar { flex-wrap: wrap; height: auto; padding-top: 12px; padding-bottom: 12px; gap: 12px 16px; }
  .actions { margin-left: auto; gap: 14px; }
  .who { display: none; }
  .menu { display: block; }
  .header-search { order: 3; flex-basis: 100%; max-width: none; }
  nav { display: none; }
  nav.open { display: block; }
  .links { flex-direction: column; height: auto; gap: 0; padding-top: 4px; padding-bottom: 8px; }
  .links a { padding: 10px 0; border-bottom: 0; margin: 0; }
  .links a.router-link-exact-active { color: var(--accent-dark); }
}

.site-footer { background: var(--kraft); margin-top: 64px; padding: 48px 0 24px; font-size: .92rem; }
.cols { display: grid; grid-template-columns: 2fr 1fr 1fr; gap: 32px; }
.cols > div { display: flex; flex-direction: column; gap: 6px; }
.cols a, .cols span { color: var(--bean); text-decoration: none; }
.cols h4 { margin: 0 0 6px; font-size: .8rem; letter-spacing: .14em; text-transform: uppercase; }
.foot-brand { font: 700 1.2rem var(--serif); }
.fine { margin-top: 32px; padding-top: 16px; border-top: 1px solid rgba(91, 58, 36, .15); color: var(--muted); font-size: .8rem; }
@media (max-width: 640px) { .cols { grid-template-columns: 1fr; } }
</style>
