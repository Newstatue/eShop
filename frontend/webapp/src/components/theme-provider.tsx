import {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react"

type Theme = "dark" | "light" | "system"
export type ResolvedTheme = Exclude<Theme, "system">

export const DEFAULT_THEME_STORAGE_KEY = "eshop-theme"
const DARK_MODE_QUERY = "(prefers-color-scheme: dark)"

type ThemeProviderProps = {
  children: React.ReactNode
  defaultTheme?: Theme
  storageKey?: string
}

type ThemeProviderState = {
  theme: Theme
  setTheme: (theme: Theme) => void
}

const initialState: ThemeProviderState = {
  theme: "system",
  setTheme: () => null,
}

const ThemeProviderContext = createContext<ThemeProviderState>(initialState)

export function ThemeProvider({
  children,
  defaultTheme = "system",
  storageKey = DEFAULT_THEME_STORAGE_KEY,
  ...props
}: ThemeProviderProps) {
  const [theme, setTheme] = useState<Theme>(
    () => (localStorage.getItem(storageKey) as Theme) || defaultTheme
  )

  useEffect(() => {
    const root = window.document.documentElement

    root.classList.remove("light", "dark")

    if (theme === "system") {
      const systemTheme = window.matchMedia(DARK_MODE_QUERY).matches
        ? "dark"
        : "light"

      root.classList.add(systemTheme)
      return
    }

    root.classList.add(theme)
  }, [theme])

  const value = {
    theme,
    setTheme: (theme: Theme) => {
      localStorage.setItem(storageKey, theme)
      setTheme(theme)
    },
  }

  return (
    <ThemeProviderContext.Provider {...props} value={value}>
      {children}
    </ThemeProviderContext.Provider>
  )
}

export const useTheme = () => {
  const context = useContext(ThemeProviderContext)

  if (context === undefined)
    throw new Error("useTheme must be used within a ThemeProvider")

  return context
}

export function useResolvedTheme(): ResolvedTheme {
  const { theme } = useTheme()

  return useMemo<ResolvedTheme>(() => {
    if (theme === "system") {
      if (typeof window === "undefined") {
        return "light"
      }

      return window.matchMedia(DARK_MODE_QUERY).matches ? "dark" : "light"
    }

    return theme
  }, [theme])
}

export function resolveThemePreference(
  storageKey = DEFAULT_THEME_STORAGE_KEY
): ResolvedTheme {
  if (typeof window === "undefined") {
    return "light"
  }

  const stored = localStorage.getItem(storageKey) as Theme | null

  if (stored === "dark" || stored === "light") {
    return stored
  }

  return window.matchMedia(DARK_MODE_QUERY).matches ? "dark" : "light"
}

export function buildThemeQueryParams(
  theme?: ResolvedTheme
): Record<string, string | undefined> {
  const resolved = theme ?? resolveThemePreference()
  const locale =
    typeof navigator !== "undefined"
      ? navigator.language?.split("-")[0]
      : undefined

  return {
    dark: resolved === "dark" ? "true" : "false",
    ui_locales: locale,
  }
}
