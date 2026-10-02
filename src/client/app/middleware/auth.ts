// Sends signed-out visitors to the login page, then back here afterwards.
export default defineNuxtRouteMiddleware((to) => {
  const { signedIn } = useAuth()
  if (!signedIn.value)
    return navigateTo({ path: '/login', query: { redirect: to.fullPath } })
})
