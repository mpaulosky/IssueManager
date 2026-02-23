// IssueManager — theme helpers
// Manages dark mode and color theme via localStorage + DOM attributes.

window.themeHelpers = {
	/**
	 * Returns true if dark mode is currently active.
	 * @returns {boolean}
	 */
	isDark: () => document.documentElement.classList.contains('dark'),

	/**
	 * Enables or disables dark mode.
	 * @param {boolean} enabled
	 */
	setDark: (enabled) => {
		if (enabled) {
			document.documentElement.classList.add('dark');
		} else {
			document.documentElement.classList.remove('dark');
		}
		localStorage.setItem('darkMode', String(enabled));
	},

	/**
	 * Returns the active color theme key.
	 * @returns {string}
	 */
	getColorTheme: () => localStorage.getItem('colorTheme') || 'blue',

	/**
	 * Sets the active color theme.
	 * @param {string} theme - 'blue' | 'green' | 'red' | 'yellow'
	 */
	setColorTheme: (theme) => {
		document.documentElement.setAttribute('data-theme', theme);
		localStorage.setItem('colorTheme', theme);
	},
};
