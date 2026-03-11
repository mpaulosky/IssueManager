
export function setTheme(theme) {
	// Set the theme by adding or removing the 'dark' class on the html element
	// This follows Tailwind CSS dark mode class strategy
	if (theme === 'dark') {
		document.documentElement.classList.add('dark');
		document.documentElement.classList.remove('light');
	} else {
		document.documentElement.classList.remove('dark');
		document.documentElement.classList.add('light');
	}

    // Sync with ThemeManager if available
    if (window.ThemeManager) {
        const color = window.ThemeManager.getCurrentColor();
        const themes = window.ThemeManager.COLOR_FAMILIES[color];
        if (themes) {
            const themeName = theme === 'dark' ? themes[1] : themes[0];
            if (window.ThemeManager.getCurrentTheme() !== themeName) {
                window.ThemeManager.setTheme(themeName);
            }
        }
    }
}